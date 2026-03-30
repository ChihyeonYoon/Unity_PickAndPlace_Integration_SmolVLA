#!/bin/bash

# Source ROS and catkin workspaces
source /opt/ros/noetic/setup.bash
source /catkin_ws/devel/setup.bash

# Export ROS_MASTER_URI (important for multi-container setup)
export ROS_MASTER_URI=http://${ROSCORE_CONTAINER_NAME}:${ROS_MASTER_PORT_IN_CONTAINER}/

# Start roscore in background
roscore &
ROSCORE_PID=$!

echo "Waiting for roscore to fully start..."
# Use rostopic list to check roscore status, with a timeout
TIMEOUT_SECONDS=60
START_TIME=$(date +%s)
while ! rostopic list > /dev/null 2>&1; do
  CURRENT_TIME=$(date +%s)
  ELAPSED_TIME=$((CURRENT_TIME - START_TIME))
  if [ $ELAPSED_TIME -ge $TIMEOUT_SECONDS ]; then
    echo "ERROR: roscore did not start within ${TIMEOUT_SECONDS} seconds. Exiting."
    kill $ROSCORE_PID # Terminate roscore if it didn't start
    exit 1
  fi
  echo "Still waiting for roscore..."
  sleep 1
done
echo "roscore is ready."

# Start ROS-TCP-Endpoint server
echo "Starting ROS-TCP-Endpoint server..."
roslaunch ros_tcp_endpoint endpoint.launch || 
  { echo "ERROR: roslaunch ros_tcp_endpoint endpoint.launch failed!"; exit 1; }

# Keep the container running by waiting for roscore to exit
wait $ROSCORE_PID
