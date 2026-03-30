#!/usr/local/bin/python3.12

import rospy
import torch
import torch.nn.functional as F
import numpy as np
import os
from std_msgs.msg import String, Float32MultiArray
from sensor_msgs.msg import Image
from transformers import AutoTokenizer

try:
    from lerobot.policies.smolvla.modeling_smolvla import SmolVLAPolicy
    from lerobot.policies.factory import make_pre_post_processors
    LEROBOT_AVAILABLE = True
except ImportError as e:
    rospy.logerr(f"Failed to import lerobot: {e}")
    LEROBOT_AVAILABLE = False

class SmolVLANode:
    def __init__(self):
        rospy.init_node('smolvla_node', anonymous=True)
        
        self.camera_images = { "camera1": None, "camera2": None, "camera3": None }
        self.latest_state_tensor = None
        self.pending_command = None
        self.device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        
        os.environ["HF_TOKEN"] = "YOUR_HF_TOKEN"
        
        if LEROBOT_AVAILABLE:
            model_id = "lerobot/smolvla_base"
            rospy.loginfo(f"Loading SmolVLA model '{model_id}' on {self.device}...")
            try:
                self.policy = SmolVLAPolicy.from_pretrained(model_id)
                self.policy.to(self.device)
                self.policy.eval()
                # SmolVLA에서 사용하는 기본 언어 토크나이저 로드
                self.tokenizer = AutoTokenizer.from_pretrained("HuggingFaceTB/SmolVLM2-500M-Video-Instruct")
                
                # LeRobot 자동 프로세서는 의존성 문제(smolvla_new_line_processor 누락 등)로 인해
                # 현재 컨테이너 버전에서 작동하지 않으므로 수동 전/후처리(Manual Scaling)를 적용합니다.
                self.preprocess = None
                self.postprocess = None
                rospy.loginfo("SmolVLA setup complete with 3-camera support!")
            except Exception as e:
                rospy.logerr(f"Error loading model: {e}")
                self.policy = None
        else:
            self.policy = None

        # 구독자 (Subscribers)
        rospy.Subscriber('/smolvla/camera_image1', Image, lambda msg: self.image_callback(msg, "camera1"))
        rospy.Subscriber('/smolvla/camera_image2', Image, lambda msg: self.image_callback(msg, "camera2"))
        rospy.Subscriber('/smolvla/camera_image3', Image, lambda msg: self.image_callback(msg, "camera3"))
        rospy.Subscriber('/smolvla/command_text', String, self.command_callback)
        rospy.Subscriber('/smolvla/current_state', Float32MultiArray, self.state_callback)
        
        self.action_pub = rospy.Publisher('/smolvla/robot_action_debug', String, queue_size=10)
        self.joint_action_pub = rospy.Publisher('/smolvla/joint_action_cmd', Float32MultiArray, queue_size=10)
        
        rospy.loginfo("SmolVLA Node is ready (3 cameras).")

    def state_callback(self, msg):
        try:
            state_array = np.array(msg.data, dtype=np.float32)
            self.latest_state_tensor = torch.from_numpy(state_array).unsqueeze(0).to(self.device)
        except Exception as e:
            rospy.logerr(f"Error processing robot state: {e}")

    def image_callback(self, msg, cam_key):
        try:
            img_np = np.frombuffer(msg.data, dtype=np.uint8).reshape(msg.height, msg.width, 3).copy()
            img_tensor = torch.from_numpy(img_np).permute(2, 0, 1).float() / 255.0
            self.camera_images[cam_key] = img_tensor.unsqueeze(0).to(self.device)

            # 모든 카메라와 명령이 준비되었는지 확인
            if all(v is not None for v in self.camera_images.values()) and self.pending_command is not None:
                command_to_run = self.pending_command
                self.pending_command = None
                self.run_inference(command_to_run)
        except Exception as e:
            rospy.logerr(f"Error processing image {cam_key}: {e}")

    def command_callback(self, msg):
        self.pending_command = msg.data
        if all(v is not None for v in self.camera_images.values()):
            command_to_run = self.pending_command
            self.pending_command = None
            self.run_inference(command_to_run)

    def run_inference(self, instruction_text):
        if self.policy is None: return

        rospy.loginfo(f"Running inference (3 cameras) for: '{instruction_text}'")
        
        inputs = self.tokenizer(instruction_text, return_tensors="pt")
        language_tokens = inputs["input_ids"].to(self.device)
        language_attention_mask = inputs["attention_mask"].to(self.device).bool()
        
        if self.latest_state_tensor is not None:
            current_state = self.latest_state_tensor[:, :6]
        else:
            current_state = torch.zeros(1, 6).to(self.device)
            
        raw_observation = {
            "observation.images.camera1": self.camera_images["camera1"],
            "observation.images.camera2": self.camera_images["camera2"],
            "observation.images.camera3": self.camera_images["camera3"],
            "observation.state": current_state,
            "observation.language.tokens": language_tokens,
            "observation.language.attention_mask": language_attention_mask
        }
        
        if self.preprocess is not None:
            observation = self.preprocess(raw_observation)
        else:
            observation = raw_observation

        try:
            with torch.inference_mode():
                # 한 번에 1스텝만 내뱉는 모델을 위해, 강제로 50번 반복하여 궤적을 만듭니다.
                num_steps = 50
                trajectory_sequence = []
                
                # 현재 상태 복사 (루프 안에서 갱신할 예정)
                simulated_state = current_state.clone()
                
                for step in range(num_steps):
                    # 매 스텝마다 업데이트된 상태를 관측값에 넣습니다.
                    observation["observation.state"] = simulated_state
                    
                    # 1스텝 추론
                    action_chunk = self.policy.select_action(observation)
                    
                    # (선택) 포스트 프로세서가 있다면 적용
                    if self.postprocess is not None:
                        action_chunk = self.postprocess(action_chunk)
                    
                    # (1, 6) 형태의 텐서를 리스트로 변환하여 궤적에 추가
                    single_action = action_chunk.squeeze().tolist()
                    trajectory_sequence.extend(single_action)
                    
                    # "다음 스텝은 방금 모델이 내뱉은 동작을 수행한 상태일 것이다"라고 가정한 뒤 뇌를 속임
                    # (주의: 이상적인 방식은 매 프레임 실제 카메라를 다시 찍는 것이지만, 
                    # 한 번에 50프레임을 보내야 하는 현재 구조상 상태만 강제로 업데이트합니다.)
                    simulated_state = action_chunk.clone().detach()

            rospy.loginfo(f"Model generated autoregressive trajectory of {num_steps} steps.")
            rospy.loginfo(f"Total values sent to Unity: {len(trajectory_sequence)}")
            
            msg = Float32MultiArray()
            msg.data = trajectory_sequence
            self.joint_action_pub.publish(msg)
            
        except Exception as e:
            rospy.logerr(f"Inference failed: {e}")

if __name__ == '__main__':
    try:
        node = SmolVLANode()
        rospy.spin()
    except rospy.ROSInterruptException:
        pass