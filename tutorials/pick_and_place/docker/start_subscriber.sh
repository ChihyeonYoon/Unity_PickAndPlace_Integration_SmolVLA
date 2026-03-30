#!/bin/bash

# Source ROS and catkin workspaces
source /opt/ros/noetic/setup.bash
source /catkin_ws/devel/setup.bash

# Export ROS_MASTER_URI to connect to the roscore_container
export ROS_MASTER_URI=http://${ROSCORE_CONTAINER_NAME}:${ROS_MASTER_PORT_IN_CONTAINER}/

# Echo the image topic
exec rostopic echo /smolvla/camera_image
