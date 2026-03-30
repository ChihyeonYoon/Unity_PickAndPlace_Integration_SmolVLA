using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std; // std_msgs/String 및 Float32MultiArray 사용
using UnityEngine.UI; // 레거시 InputField 사용
using System.Collections; // Coroutine 사용
using System.Linq;

public class RosCommandPublisher : MonoBehaviour
{
    [SerializeField]
    string textTopicName = "/smolvla/command_text"; // ROS 텍스트 토픽
    [SerializeField]
    string stateTopicName = "/smolvla/current_state"; // ROS 현재 상태(관절 각도) 토픽

    [SerializeField]
    public InputField commandInputField; // 사용자 명령 UI

    [SerializeField]
    RosImagePublisher imagePublisher; // 이미지 퍼블리셔 참조

    [SerializeField]
    GameObject m_NiryoOne; // 관절 각도를 읽어올 로봇 (Niryo One) 루트
    
    // 로봇의 6개 관절
    ArticulationBody[] m_JointArticulationBodies;
    const int k_NumRobotJoints = 6;
    ArticulationBody m_LeftGripper;

    private ROSConnection rosConnectionInstance;

    void Start()
    {
        rosConnectionInstance = ROSConnection.GetOrCreateInstance();
        rosConnectionInstance.RegisterPublisher<StringMsg>(textTopicName);
        rosConnectionInstance.RegisterPublisher<Float32MultiArrayMsg>(stateTopicName); // 상태 배열 퍼블리셔 등록

        if (commandInputField == null)
        {
            Debug.LogError("RosCommandPublisher: CommandInputField is not assigned! Disabling script.");
            enabled = false;
            return;
        }
        if (imagePublisher == null)
        {
            imagePublisher = Camera.main?.GetComponent<RosImagePublisher>();
        }
        
        if (m_NiryoOne != null)
        {
            InitializeJoints();
        }
        else
        {
            Debug.LogWarning("RosCommandPublisher: NiryoOne GameObject not assigned. Current state will not be sent.");
        }

        Debug.Log($"RosCommandPublisher: Ready to publish to {textTopicName} and {stateTopicName}.");
    }

    void InitializeJoints()
    {
        m_JointArticulationBodies = new ArticulationBody[k_NumRobotJoints];
        var linkName = string.Empty;

        // joint_1 부터 joint_6 까지 찾기
        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            linkName += SourceDestinationPublisher.LinkNames[i];
            m_JointArticulationBodies[i] = m_NiryoOne.transform.Find(linkName).GetComponent<ArticulationBody>();
        }

        // 그리퍼 관절 찾기 (한 쪽만 상태를 읽어옴)
        var leftGripper = linkName + "/tool_link/gripper_base/servo_head/control_rod_left/left_gripper";
        m_LeftGripper = m_NiryoOne.transform.Find(leftGripper).GetComponent<ArticulationBody>();
    }

    public void PublishCommand()
    {
        string commandText = "";
        if (commandInputField != null)
        {
            commandText = commandInputField.text;
        }

        if (string.IsNullOrEmpty(commandText))
        {
            Debug.LogWarning("RosCommandPublisher: No command entered.");
            return;
        }

        // 이미지 퍼블리시 시작
        if (imagePublisher != null) imagePublisher.PublishSingleImage();

        // 현재 상태(관절 각도) 읽어서 퍼블리시
        PublishCurrentState();

        // 텍스트는 지연 후 퍼블리시 (이미지와 상태가 먼저 도착하도록 보장)
        StartCoroutine(PublishCommandWithDelay(commandText));

        if (commandInputField != null)
        {
            commandInputField.text = "";
        }
    }

    private void PublishCurrentState()
    {
        if (m_JointArticulationBodies == null || m_JointArticulationBodies[0] == null) return;

        // 보통 14축 입력을 받는 모델의 경우, [6개의 로봇 관절 + 1개의 그리퍼 상태 + 나머지 0 패딩] 형태로 보냅니다.
        // SmolVLA 기본 설정(ALOHA 데이터셋 등)에 따라 14차원 배열을 만듭니다.
        float[] currentState = new float[14]; 
        
        for (int i = 0; i < k_NumRobotJoints - 1; i++) // Joint 1~5
        {
            // Unity ArticulationBody의 jointPosition은 라디안 단위입니다.
            // 모델이 요구하는 범위(-1~1 등으로 정규화되어 있을 수 있음)에 맞게 향후 스케일링이 필요할 수 있습니다.
            // 일단은 실제 라디안 값을 그대로 보냅니다.
            currentState[i] = m_JointArticulationBodies[i].jointPosition[0]; 
        }

        // 6번째 인덱스 (그리퍼 상태)
        if (m_LeftGripper != null && m_LeftGripper.jointPosition.dofCount > 0)
        {
            currentState[5] = m_LeftGripper.jointPosition[0];
        }

        Float32MultiArrayMsg stateMsg = new Float32MultiArrayMsg();
        stateMsg.data = currentState;
        rosConnectionInstance.Publish(stateTopicName, stateMsg);
        Debug.Log("RosCommandPublisher: Published current robot state (14-dim array).");
    }

    private IEnumerator PublishCommandWithDelay(string commandText)
    {
        yield return new WaitForSeconds(0.2f);

        StringMsg commandMsg = new StringMsg(commandText);
        rosConnectionInstance.Publish(textTopicName, commandMsg);
        Debug.Log($"RosCommandPublisher: Published command: {commandText} to {textTopicName}.");
    }
}
