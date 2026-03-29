#!/usr/bin/env python3

import rospy
from std_msgs.msg import String
# from smolvla_ros.msg import SmolVLACommandImage # 나중에 빌드 후 추가
# from sensor_msgs.msg import Image # 이미지 메시지 처리용

def smolvla_node():
    rospy.init_node('smolvla_node', anonymous=True)

    # 예시: 'smolvla_output' 토픽에 String 메시지를 발행하는 publisher
    # 나중에 SmolVLA의 실제 행동 출력을 발행하도록 수정됩니다.
    pub = rospy.Publisher('smolvla_output', String, queue_size=10)

    # 이미지 및 텍스트 명령 구독자 (나중에 활성화)
    # image_sub = rospy.Subscriber('/smolvla/camera_image', Image, image_callback)
    # command_sub = rospy.Subscriber('/smolvla/command_text', String, command_callback)

    rate = rospy.Rate(10) # 10hz

    rospy.loginfo("SmolVLA ROS Node Started.")

    while not rospy.is_shutdown():
        hello_str = "Hello from SmolVLA node! Time: %s" % rospy.get_time()
        rospy.loginfo(hello_str)
        pub.publish(hello_str)
        rate.sleep()

# def image_callback(msg):
#     rospy.loginfo("Received image: %s", msg.header.stamp)
#     # SmolVLA 모델에 이미지 전달 로직 추가

# def command_callback(msg):
#     rospy.loginfo("Received command: %s", msg.data)
#     # SmolVLA 모델에 텍스트 명령 전달 로직 추가

if __name__ == '__main__':
    try:
        smolvla_node()
    except rospy.ROSInterruptException:
        pass
