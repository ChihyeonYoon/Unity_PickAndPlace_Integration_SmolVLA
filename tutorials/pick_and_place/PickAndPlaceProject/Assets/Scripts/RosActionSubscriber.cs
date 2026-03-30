using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std; // std_msgs/Float32MultiArray 타입 사용
using System.Collections; // Coroutine 사용
using System.Collections.Generic;

public class RosActionSubscriber : MonoBehaviour
{
    [SerializeField]
    string topicName = "/smolvla/joint_action_cmd"; // ROS에서 발행하는 새로운 토픽

    [SerializeField]
    GameObject m_NiryoOne; // 제어할 로봇 (Niryo One) 루트 오브젝트
    
    [SerializeField]
    float stepDelay = 0.05f; // 각 스텝 사이의 대기 시간 (초) - 부드러운 움직임을 위해 조절

    // 로봇의 6개 관절
    ArticulationBody[] m_JointArticulationBodies;
    const int k_NumRobotJoints = 6;
    const int k_ActionDim = 6; // 한 스텝당 데이터 개수 [J1, J2, J3, J4, J5, Gripper]
    
    // 그리퍼 조인트
    ArticulationBody m_LeftGripper;
    ArticulationBody m_RightGripper;

    ROSConnection m_Ros;
    private Coroutine currentTrajectoryCoroutine;

    void Start()
    {
        if (m_NiryoOne == null)
        {
            Debug.LogError("RosActionSubscriber: NiryoOne GameObject is not assigned!");
            enabled = false;
            return;
        }

        InitializeJoints();

        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.Subscribe<Float32MultiArrayMsg>(topicName, ReceiveActionCommand);
        
        Debug.Log($"RosActionSubscriber: Subscribed to {topicName}. Trajectory execution ready.");
    }

    void InitializeJoints()
    {
        m_JointArticulationBodies = new ArticulationBody[k_NumRobotJoints];
        var linkName = string.Empty;

        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            linkName += SourceDestinationPublisher.LinkNames[i];
            m_JointArticulationBodies[i] = m_NiryoOne.transform.Find(linkName).GetComponent<ArticulationBody>();
        }

        var rightGripper = linkName + "/tool_link/gripper_base/servo_head/control_rod_right/right_gripper";
        var leftGripper = linkName + "/tool_link/gripper_base/servo_head/control_rod_left/left_gripper";

        m_RightGripper = m_NiryoOne.transform.Find(rightGripper).GetComponent<ArticulationBody>();
        m_LeftGripper = m_NiryoOne.transform.Find(leftGripper).GetComponent<ArticulationBody>();
    }

    void ReceiveActionCommand(Float32MultiArrayMsg actionMsg)
    {
        if (actionMsg.data.Length % k_ActionDim != 0)
        {
            Debug.LogError($"RosActionSubscriber: Received array length ({actionMsg.data.Length}) is not a multiple of {k_ActionDim}.");
            return;
        }

        // 기존에 실행 중인 동작이 있다면 중단
        if (currentTrajectoryCoroutine != null)
        {
            StopCoroutine(currentTrajectoryCoroutine);
        }

        // 새로운 궤적 실행 시작
        currentTrajectoryCoroutine = StartCoroutine(ExecuteTrajectory(actionMsg.data));
    }

    IEnumerator ExecuteTrajectory(float[] trajectoryData)
    {
        int totalSteps = trajectoryData.Length / k_ActionDim;
        Debug.Log($"RosActionSubscriber: Starting execution of {totalSteps} steps trajectory...");

        for (int step = 0; step < totalSteps; step++)
        {
            int baseIdx = step * k_ActionDim;

            // 1. 관절 1~5 적용
            for (int i = 0; i < k_NumRobotJoints - 1; i++)
            {
                float targetAngleRadians = trajectoryData[baseIdx + i];

                // 수동 스케일링: 모델 출력이 -1.0 ~ 1.0으로 정규화되어 있을 가능성이 높으므로, 
                // 실제 로봇 관절의 움직임 폭(약 180도 = 3.14 라디안)에 맞춰 증폭해 줍니다.
                // 실험적 값: 2.0 (상황에 따라 1.0 ~ 3.14 사이로 조절)
                targetAngleRadians *= 2.0f; 

                float targetAngleDegrees = targetAngleRadians * Mathf.Rad2Deg;

                var jointDrive = m_JointArticulationBodies[i].xDrive;                jointDrive.target = targetAngleDegrees;
                m_JointArticulationBodies[i].xDrive = jointDrive;
            }

            // 2. 6번째 값(그리퍼) 적용
            float gripperState = trajectoryData[baseIdx + 5];
            if (gripperState < 0.0f)
            {
                CloseGripper();
            }
            else
            {
                OpenGripper();
            }

            // 지정된 시간만큼 대기 후 다음 스텝 진행
            yield return new WaitForSeconds(stepDelay);
        }

        Debug.Log("RosActionSubscriber: Trajectory execution finished.");
        currentTrajectoryCoroutine = null;
    }

    void CloseGripper()
    {
        var leftDrive = m_LeftGripper.xDrive;
        var rightDrive = m_RightGripper.xDrive;
        leftDrive.target = -0.01f;
        rightDrive.target = 0.01f;
        m_LeftGripper.xDrive = leftDrive;
        m_RightGripper.xDrive = rightDrive;
    }

    void OpenGripper()
    {
        var leftDrive = m_LeftGripper.xDrive;
        var rightDrive = m_RightGripper.xDrive;
        leftDrive.target = 0.01f;
        rightDrive.target = -0.01f;
        m_LeftGripper.xDrive = leftDrive;
        m_RightGripper.xDrive = rightDrive;
    }
}