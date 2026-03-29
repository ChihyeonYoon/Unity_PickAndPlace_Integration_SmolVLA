using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor; // sensor_msgs/Image 메시지 타입 사용
using RosMessageTypes.BuiltinInterfaces; // TimeMsg 타입 사용
using UnityEngine.Rendering; // CommandBuffer 사용
using System.Collections.Generic;
using System;

public class RosImagePublisher : MonoBehaviour
{
    [SerializeField]
    string topicName = "/smolvla/camera_image"; // ROS 토픽 이름
    [SerializeField]
    Camera targetCamera; // 이미지를 캡처할 카메라
    [SerializeField]
    int imageWidth = 640; // 이미지 폭
    [SerializeField]
    int imageHeight = 480; // 이미지 높이

    ROSConnection ros;
    Texture2D texture;
    RenderTexture renderTexture;
    Rect rect;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError("RosImagePublisher: No target camera found! Disabling script.");
            enabled = false;
            return;
        }

        renderTexture = new RenderTexture(imageWidth, imageHeight, 24, RenderTextureFormat.ARGB32);
        targetCamera.targetTexture = renderTexture; // 카메라의 렌더 타겟 설정

        texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false); // RGB24로 유지
        rect = new Rect(0, 0, imageWidth, imageHeight);

        Debug.Log($"RosImagePublisher: Ready to publish to {topicName}. Image size: {imageWidth}x{imageHeight}. ROS Master: {ros.RosIPAddress}:{ros.RosPort}");
    }

    void OnDestroy()
    {
        if (targetCamera != null)
        {
            targetCamera.targetTexture = null;
        }
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
        if (texture != null)
        {
            Destroy(texture);
        }
    }

    public void PublishSingleImage()
    {
        if (!enabled) return;

        RenderTexture.active = renderTexture;
        targetCamera.Render();

        texture.ReadPixels(rect, 0, 0);
        texture.Apply();
        RenderTexture.active = null;

        TimeMsg stamp = new TimeMsg();
        stamp.sec = (uint)Mathf.FloorToInt(Time.time);
        stamp.nanosec = (uint)((Time.time - Mathf.FloorToInt(Time.time)) * 1e9);

        ImageMsg imageMsg = new ImageMsg();
        imageMsg.header.stamp = stamp;
        imageMsg.header.frame_id = "camera_link";
        imageMsg.height = (uint)imageHeight;
        imageMsg.width = (uint)imageWidth;
        imageMsg.encoding = "rgb8";
        imageMsg.is_bigendian = 0;
        imageMsg.step = (uint)imageWidth * 3;
        imageMsg.data = texture.GetRawTextureData();

        ros.Publish(topicName, imageMsg);
        Debug.Log($"RosImagePublisher: Published single image to {topicName}.");
    }
}
