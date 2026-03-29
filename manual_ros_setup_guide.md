# ,Manual ROS Environment Setup Guide for Unity-SmolVLA Integration

This guide outlines the manual steps to set up the ROS Docker environment for the Unity-SmolVLA integration project. This is necessary if the automated `start_ros_smolvla_env.sh` script encounters issues.

**Note:** Ensure you have already built the `unity-robotics:smolvla` Docker image by navigating to `Unity-Robotics-Hub/tutorials/pick_and_place` and running `docker build -t unity-robotics:smolvla -f docker/Dockerfile .`

## Prerequisites:

- Unity Editor is closed.
- All previous Docker containers are stopped and removed.
- `ros_network` Docker network is deleted (if it exists).

## Step-by-Step Manual Setup:

### 1. Clean Up Existing Docker Environment (Host Mac Terminal)

Open your Mac OS Terminal and run the following commands to ensure a clean start:

```bash
docker stop $(docker ps -q) # Stop all running containers
docker rm $(docker ps -aq)  # Remove all stopped containers
docker network rm ros_network # Remove the custom network (if it exists)
```

### 2. Create Custom Docker Network (Host Mac Terminal)

Create a new Docker network for containers to communicate:

```bash
docker network create ros_network
```

### 3. Start `roscore_container` and ROS-TCP-Endpoint Server (First Terminal)

Open a **NEW Terminal window** on your Mac and run the following. This terminal will run `roscore` and the `ros_tcp_endpoint` server, and **MUST remain active**.

```bash
docker run -it --rm --network ros_network --name roscore_container -p 10000:10000 unity-robotics:smolvla /bin/bash
# Once inside the container shell (`root@<container_id>:/catkin_ws#`):
source /opt/ros/melodic/setup.bash
source /catkin_ws/devel/setup.bash
roscore &
ROS_MASTER_URI=http://roscore_container:11311/
roslaunch ros_tcp_endpoint endpoint.launch
wait $! # Keep container alive until roscore exits (Ctrl+C)
```

- **Verify:** Ensure `roscore` starts and `ros_tcp_endpoint` server outputs `Listening on port 10000`.

### 4. Start `subscriber_container` and Test Image/Command Topics (Second Terminal)

Open **ANOTHER NEW Terminal window** on your Mac. This terminal will subscribe to image and command topics.

```bash
docker run -it --rm --network ros_network --name subscriber_container unity-robotics:smolvla /bin/bash
# Once inside the container shell (`root@<container_id>:/catkin_ws#`):
source /opt/ros/melodic/setup.bash
source /catkin_ws/devel/setup.bash
export ROS_MASTER_URI=http://roscore_container:11311/
rostopic echo /smolvla/camera_image & rostopic echo /smolvla/command_text
# Verify image, text publishing from Unity
# In a new tab/window of this same container, you can also test:
```

### 5. Configure Unity Editor and Enter Play Mode (Unity Editor)

Open your Unity project and scene (`New Scene.unity`):

- `**ROSConnect` GameObject (`Hierarchy`):**
  - Select `ROSConnect` GameObject.
  - `ROSConnection` Component (`Inspector`):
    - `Ros IP Address`: `**YOUR_HOST_IP_ADDRESS`** (Your Mac's Host IP Address)
    - `Ros Port`: `**10000`**
- `**Robotics -> ROS Settings` (`Top Menu Bar`):**
  - `ROS IP Address`: `**YOUR_HOST_IP_ADDRESS`**
  - `ROS Port`: `**10000`**
- **Enter Play Mode:**
  - Click the **Play** button in Unity. The Console should show `Connection to YOUR_HOST_IP_ADDRESS:10000 successfully started`.
- **Final Verification:** The `subscriber_container` terminal(s) should now be continuously displaying image pixel data and/or text commands from Unity.

