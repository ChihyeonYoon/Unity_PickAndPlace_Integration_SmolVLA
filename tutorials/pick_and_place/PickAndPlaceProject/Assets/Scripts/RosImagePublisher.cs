using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.BuiltinInterfaces;
using System;

public class RosImagePublisher : MonoBehaviour
{
    [SerializeField]
    Camera[] targetCameras; // 3개의 카메라를 넣을 배열 (Camera 1, 2, 3)
    
    [SerializeField]
    string[] topicNames = { "/smolvla/camera_image1", "/smolvla/camera_image2", "/smolvla/camera_image3" };

    [SerializeField]
    int imageWidth = 256; // 모델 최적화 크기
    [SerializeField]
    int imageHeight = 256;

    ROSConnection ros;
    RenderTexture renderTexture;
    Texture2D texture;
    Rect rect;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        foreach (var topic in topicNames)
        {
            ros.RegisterPublisher<ImageMsg>(topic);
        }

        renderTexture = new RenderTexture(imageWidth, imageHeight, 24, RenderTextureFormat.ARGB32);
        texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        rect = new Rect(0, 0, imageWidth, imageHeight);

        // 메인 카메라(첫 번째)를 제외한 나머지 보조 카메라들은 게임 화면에 그리지 않도록 비활성화합니다.
        // (비활성화되어 있어도 스크립트에서 cam.Render()를 호출하면 사진을 찍을 수 있습니다.)
        if (targetCameras != null)
        {
            for (int i = 1; i < targetCameras.Length; i++)
            {
                if (targetCameras[i] != null)
                {
                    targetCameras[i].enabled = false;
                }
            }
        }
    }

    public void PublishSingleImage()
    {
        if (targetCameras == null || targetCameras.Length == 0) return;

        for (int i = 0; i < targetCameras.Length; i++)
        {
            if (i >= topicNames.Length || targetCameras[i] == null) continue;

            CaptureAndPublish(targetCameras[i], topicNames[i]);
        }
    }

    private void CaptureAndPublish(Camera cam, string topic)
    {
        var oldTarget = cam.targetTexture;
        cam.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        cam.Render();

        texture.ReadPixels(rect, 0, 0);
        texture.Apply();

        cam.targetTexture = oldTarget;
        RenderTexture.active = null;

        ImageMsg imageMsg = new ImageMsg
        {
            header = new RosMessageTypes.Std.HeaderMsg { frame_id = "camera_link", stamp = GetTimeMsg() },
            height = (uint)imageHeight,
            width = (uint)imageWidth,
            encoding = "rgb8",
            step = (uint)imageWidth * 3,
            data = texture.GetRawTextureData()
        };

        ros.Publish(topic, imageMsg);
        Debug.Log($"Published image from {cam.name} to {topic}");
    }

    private TimeMsg GetTimeMsg()
    {
        return new TimeMsg { sec = (uint)Time.time, nanosec = (uint)((Time.time - Math.Floor(Time.time)) * 1e9) };
    }
}