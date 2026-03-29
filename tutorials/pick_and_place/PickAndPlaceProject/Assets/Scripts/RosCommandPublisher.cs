using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std; // std_msgs/String 메시지 타입 사용
using UnityEngine.UI; // 레거시 InputField 사용

public class RosCommandPublisher : MonoBehaviour
{
    [SerializeField]
    string topicName = "/smolvla/command_text"; // ROS 토픽 이름
    [SerializeField]
    public InputField commandInputField; // 사용자 명령을 입력받을 InputField (레거시 UI)

    [SerializeField]
    RosImagePublisher imagePublisher; // RosImagePublisher 컴포넌트 참조

    private ROSConnection rosConnectionInstance;

    void Start()
    {
        rosConnectionInstance = ROSConnection.GetOrCreateInstance();
        rosConnectionInstance.RegisterPublisher<StringMsg>(topicName);

        if (commandInputField == null)
        {
            Debug.LogError("RosCommandPublisher: CommandInputField is not assigned! Disabling script.");
            enabled = false;
            return;
        }
        if (imagePublisher == null)
        {
            imagePublisher = Camera.main?.GetComponent<RosImagePublisher>();
            if (imagePublisher == null)
            {
                Debug.LogError("RosCommandPublisher: RosImagePublisher is not assigned or found on Main Camera! Disabling script.");
                enabled = false;
                return;
            }
        }

        Debug.Log($"RosCommandPublisher: Ready to publish to {topicName}.");
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
            Debug.LogWarning("RosCommandPublisher: No command entered or InputField is empty.");
            return;
        }

        imagePublisher.PublishSingleImage();

        StringMsg commandMsg = new StringMsg(commandText);
        rosConnectionInstance.Publish(topicName, commandMsg);
        Debug.Log($"RosCommandPublisher: Published command: {commandText} to {topicName}.");

        if (commandInputField != null)
        {
            commandInputField.text = "";
        }
    }
}
