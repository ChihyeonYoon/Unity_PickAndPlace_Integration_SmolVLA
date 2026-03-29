FROM ros:melodic-ros-base

RUN sudo apt-key adv --keyserver keyserver.ubuntu.com --recv-keys F42ED6FBAB17C654

RUN sudo apt-get update && sudo apt-get install -y vim iputils-ping net-tools python3 python3-pip ros-melodic-robot-state-publisher ros-melodic-moveit ros-melodic-rosbridge-suite ros-melodic-joy ros-melodic-ros-control ros-melodic-ros-controllers ros-melodic-tf2-web-republisher dos2unix git

RUN sudo -H pip3 install rospkg jsonpickle

ENV ROS_WORKSPACE=/catkin_ws

# Add SmolVLA installation steps
RUN git clone https://github.com/huggingface/lerobot.git /lerobot_ws
WORKDIR /lerobot_ws
RUN pip3 install -e ".[smolvla]"
WORKDIR $ROS_WORKSPACE

# Copy each directory explicitly to avoid workspace cruft
COPY ./ROS/src/moveit_msgs $ROS_WORKSPACE/src/moveit_msgs
COPY ./ROS/src/niryo_moveit $ROS_WORKSPACE/src/niryo_moveit
COPY ./ROS/src/niryo_one_ros $ROS_WORKSPACE/src/niryo_one_ros
COPY ./ROS/src/niryo_one_urdf $ROS_WORKSPACE/src/niryo_one_urdf
COPY ./ROS/src/ros_tcp_endpoint $ROS_WORKSPACE/src/ros_tcp_endpoint
COPY ./ROS/src/smolvla_ros $ROS_WORKSPACE/src/smolvla_ros

COPY ./docker/set-up-workspace /setup.sh
COPY docker/tutorial /tutorial

RUN /bin/bash -c "find /catkin_ws -type f -print0 | xargs -0 dos2unix"

# Set up workspace and install Python dependencies
RUN dos2unix /tutorial && dos2unix /setup.sh && chmod +x /setup.sh && /setup.sh && rm /setup.sh

WORKDIR $ROS_WORKSPACE

# making sure the file modes are executable
RUN chmod +x src/niryo_moveit/scripts/*.py
RUN chmod +x src/smolvla_ros/scripts/*.py

COPY ./docker/start_roscore.sh /start_roscore.sh
COPY ./docker/start_subscriber.sh /start_subscriber.sh

RUN chmod +x /start_roscore.sh /start_subscriber.sh
