# Unity Pick and Place Integration with SmolVLA

This repository contains the Unity Pick and Place tutorial project from the [Unity Robotics Hub](https://github.com/Unity-Technologies/Unity-Robotics-Hub/tree/main/tutorials/pick_and_place), enhanced with the integration of the **SmolVLA (Visual Language Action)** model within a ROS Docker environment.

The goal of this project is to explore how Unity's robotic simulation tools can be combined with advanced VLA models like SmolVLA for intuitive, language-driven robotic control.

<p align="center"><img src="tutorials/pick_and_place/img/0_pick_place.gif"/></p>

## Project Overview

This project builds upon the Unity Robotics Hub's Pick-and-Place tutorial, which demonstrates integrating Unity with ROS for robotic simulation. We have successfully established the communication pipeline between Unity and a ROS-based SmolVLA inference node.

### Summary of New and Modified Elements:

Based on the work in the `Unity-Robotics-Hub/tutorials/pick_and_place` directory and synchronized with the GitHub repository [Unity_PickAndPlace_Integration_SmolVLA](https://github.com/ChihyeonYoon/Unity_PickAndPlace_Integration_SmolVLA).

#### 1. New C# Scripts (Unity Project `Assets/Scripts`)
*   **`RosImagePublisher.cs`**:
    *   **Role**: Captures a single frame from the Unity Main Camera and publishes it to the `/smolvla/camera_image` ROS topic. Continuous publishing logic has been removed to optimize performance.
    *   **Location**: `Assets/Scripts/RosImagePublisher.cs`
*   **`RosCommandPublisher.cs`**:
    *   **Role**: Publishes text commands entered in the Unity UI `CommandInputField` to the `/smolvla/command_text` ROS topic. When the `SendCommandButton` is clicked, it first triggers `RosImagePublisher.PublishSingleImage()`.
    *   **Location**: `Assets/Scripts/RosCommandPublisher.cs`
*   **`SceneResetter.cs`**:
    *   **Role**: Provides the `ResetCurrentScene()` function, which is linked to the `ResetButton` to reload the scene.
    *   **Location**: `Assets/Scripts/SceneResetter.cs`

#### 2. Modified Existing C# Scripts
*   **`TrajectoryPlanner.cs`**: Resolved the `NullReferenceException` by utilizing the fully configured robot model copied from the `DemoScene`. Minimal code changes were applied to ensure compatibility with the manually assembled robot hierarchy.
*   **`PickAndPlaceRosPublisher.cs`**: A core tutorial script assigned to the `Publisher` GameObject to manage the communication of pick and place targets to ROS.

#### 3. New ROS Package and Messages (Host `ROS/src` & Docker Image)
*   **`smolvla_ros` Package**:
    *   **Role**: A custom ROS package created to house SmolVLA-specific nodes, messages, and launch files.
    *   **Location**: `ROS/src/smolvla_ros/`
    *   **Key Files**:
        *   `msg/SmolVLACommandImage.msg`: Defines a combined message type for images and text commands.
        *   `scripts/smolvla_node.py`: The skeleton for the Python ROS node that will host the SmolVLA model.
        *   `launch/smolvla_launch.launch`: Launch file for the `smolvla_node`.
        *   `CMakeLists.txt`, `package.xml`, `setup.py`: Build and dependency configuration files.

#### 4. Modified Docker Configuration
*   **`Dockerfile`**:
    *   **Key Changes**:
        *   Added installation of Python 3 and `pip3`.
        *   Cloned the `HuggingFace/LeRobot` repository and installed `smolvla` dependencies.
        *   Included instructions to copy the `smolvla_ros` package to the workspace.
        *   Restored original `ENTRYPOINT` and added custom start scripts.
    *   **Location**: `tutorials/pick_and_place/docker/Dockerfile`
*   **`start_roscore.sh`**:
    *   **Role**: Starts `roscore` and the `ROS-TCP-Endpoint` server within the `roscore_container`.
*   **`start_subscriber.sh`**:
    *   **Role**: Sets up the ROS environment and subscribes to image/command topics for verification in the `subscriber_container`.

#### 5. New Docker Image
*   **`unity-robotics:smolvla`**:
    *   **Role**: The updated Docker image built from the modified `Dockerfile`. Contains Python 3, SmolVLA dependencies, the `smolvla_ros` package, and custom startup scripts.

#### 6. New Unity UI Elements
*   **`CommandInputField`**: A legacy `InputField` for user text commands.
*   **`SendCommandButton`**: A legacy `Button` to publish commands, linked to `RosCommandPublisher`.
*   **`ResetButton`**: A legacy `Button` to reset the simulation, linked to `SceneResetter`.

#### 7. Miscellaneous
*   **`manual_ros_setup_guide.md`**: Provides detailed, step-by-step manual instructions for configuring the ROS Docker environment to ensure reliable networking.
*   **Git Setup**: Established an independent Git repository for the project to track all custom integration work.

---

## Getting Started

To run the integration, follow the steps outlined in the [Manual ROS Setup Guide](manual_ros_setup_guide.md) to initialize the Docker environment and connect Unity.

## Next Steps

1.  **SmolVLA Model Integration**: Fully implement the `smolvla_node.py` to load the model and process incoming Unity data.
2.  **Action interpretation**: Develop the Unity-side logic to execute the actions generated by the SmolVLA model.
