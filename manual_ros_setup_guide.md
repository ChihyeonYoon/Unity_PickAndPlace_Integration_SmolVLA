# Manual ROS Environment Setup Guide for Unity-SmolVLA Integration

이 가이드는 Unity와 SmolVLA 모델을 연동하기 위한 ROS Docker 환경의 최종 수동 설정 방법을 안내합니다.

## 1단계: 기존 Docker 환경 정리 (Host Mac Terminal)

```bash
docker stop roscore_container smolvla_inference 2>/dev/null
docker rm roscore_container smolvla_inference 2>/dev/null
docker network rm ros_network 2>/dev/null
docker network create ros_network
```

## 2단계: Terminal 1 - ROS Core & MoveIt Bridge 실행

기존 튜토리얼의 MoveIt 기능과 Unity-ROS 통신 브릿지를 담당합니다.

```bash
docker run -it --rm --network ros_network --name roscore_container -p 10000:10000 unity-robotics:smolvla /bin/bash

# 컨테이너 내부 쉘에서 실행:
source /opt/ros/noetic/setup.bash
source /catkin_ws/devel/setup.bash
roscore &
sleep 5
# TCP Endpoint와 MoveIt 노드를 동시에 실행
roslaunch niryo_moveit part_3.launch
```

## 3단계: Terminal 2 - SmolVLA AI 추론 노드 실행

실제 AI 모델이 작동하며 호스트의 최신 파이썬 코드를 마운트하여 실행합니다.

```bash
docker run -it --rm --network ros_network --name smolvla_inference \
  -v /Users/home/Desktop/Unity-Robotics-Hub/tutorials/pick_and_place/ROS/src/smolvla_ros/scripts/smolvla_node.py:/catkin_ws/src/smolvla_ros/scripts/smolvla_node.py \
  unity-robotics:smolvla /bin/bash

# 컨테이너 내부 쉘에서 실행:
source /opt/ros/noetic/setup.bash
source /catkin_ws/devel/setup.bash
export ROS_MASTER_URI=http://roscore_container:11311/
export PYTHONPATH=$PYTHONPATH:/lerobot_ws/src
export HF_TOKEN=YOUR_HF_TOKEN

/usr/local/bin/python3.12 /catkin_ws/src/smolvla_ros/scripts/smolvla_node.py
```

## 4단계: Unity 에디터 설정 및 실행

1. **RosImagePublisher:** `Target Cameras` 리스트에 Camera1, 2, 3를 순서대로 할당합니다.
2. **RosCommandPublisher:** `M_NiryoOne` 칸에 `niryo_one` 로봇 오브젝트를 할당합니다.
3. **RosActionSubscriber:** `M_NiryoOne` 칸에 `niryo_one` 로봇 오브젝트를 할당합니다.
4. **Play & Test:** 텍스트 명령 입력 후 **Send** 버튼을 클릭합니다.

