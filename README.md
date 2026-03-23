# Unity Pick and Place Integration with SmolVLA

This repository contains the Unity Pick and Place tutorial project from the [Unity Robotics Hub](https://github.com/Unity-Technologies/Unity-Robotics-Hub/tree/main/tutorials/pick_and_place), enhanced with initial steps for integrating the **SmolVLA (Visual Language Action)** model within the ROS Docker environment.

The goal of this project is to explore how Unity's robotic simulation tools can be combined with advanced VLA models like SmolVLA for intuitive, language-driven robotic control.

<p align="center"><img src="tutorials/pick_and_place/img/0_pick_place.gif"/></p>

## Project Overview

This project builds upon the Unity Robotics Hub's Pick-and-Place tutorial, which demonstrates integrating Unity with ROS for robotic simulation. We have completed the core tutorial steps and initiated the integration of SmolVLA.

### Key Enhancements & Progress:

*   **ROS Docker Environment with Python 3 & SmolVLA:**
    *   The ROS Docker environment has been successfully upgraded to support Python 3.
    *   **SmolVLA and its `LeRobot` dependencies are now installed** within a new Docker image (`unity-robotics:smolvla`). This involved modifying the `Dockerfile` (`tutorials/pick_and_place/docker/Dockerfile`) to include `LeRobot` cloning and `pip3 install -e ".[smolvla]"` commands, resolving Python 2/3 compatibility issues.
*   **Resolved Unity Import and Scripting Issues:**
    *   Successfully imported the `niryo_one` robot URDF into Unity, addressing `DllNotFoundException` (by setting Mesh Decomposer to Unity) and `DirectoryNotFoundException` (by manually copying mesh files into the Unity project's `Assets/New Folder/niryo_one_urdf/meshes` path).
    *   Resolved `CS0246` errors by ensuring all necessary tutorial scripts (`TargetPlacement.cs`, `PickAndPlaceRosPublisher.cs` etc.) were correctly placed in the `Assets/Scripts` folder of the Unity project.
*   **ROS-Unity Communication Established:**
    *   Configured `ROS TCP Endpoint` and `ROS Settings` in Unity for seamless communication with the ROS Docker server, using the host machine's IP address.
    *   Confirmed successful ROS message publishing and reception (e.g., `pick_pose`, `place_pose`).
*   **Pick-and-Place Demo Setup in Custom Scene:**
    *   **Successfully transferred all runtime-generated objects (robot, gripper, table, target, etc.) from `DemoScene` to a custom scene (`New Scene.unity`). This was achieved by running `DemoScene` in Play Mode, copying the instantiated objects, and pasting them into `New Scene.unity` in Edit Mode, thereby resolving dynamic instantiation issues and ensuring a fully configured scene in the editor.**
    *   **Implemented a "Reset Scene" UI button for convenient demo iteration, allowing users to easily restart the pick-and-place simulation.**
    *   **Main Camera perspective, UI button size, and position were adjusted for optimal viewing and interaction.**
*   **Gripper Integration Status:**
    *   Initially, a `NullReferenceException` in `TrajectoryPlanner.cs` occurred due to the `niryo_one` URDF lacking a direct gripper definition. **This was successfully resolved by copying the fully configured robot model, including its gripper, from the runtime-generated `DemoScene` to `New Scene.unity`. This ensured the gripper's `ArticulationBody` components were correctly loaded and referenced by `TrajectoryPlanner.cs`, allowing gripper control to function.**

## Tutorial Phases Covered (and where we are):

*   **[Part 0: ROS Setup](tutorials/pick_and_place/0_ros_setup.md):** Completed, with Docker environment upgraded to Python 3 for SmolVLA.
*   **[Part 1: Create Unity scene with imported URDF](tutorials/pick_and_place/1_urdf.md):** Completed, with fixes for import issues.
*   **[Part 2: ROS–Unity Integration](tutorials/pick_and_place/2_ros_tcp.md):** Completed, with communication confirmed.
*   **[Part 3: Pick-and-Place In Unity](tutorials/pick_and_place/3_pick_and_place.md):** Fully completed, with gripper integration resolved.
*   **[Part 4: Pick-and-Place on the Real Robot](tutorials/pick_and_place/4_pick_and_place.md):** Reviewed, focused on future real-robot integration or simulation mirroring.

---

## Next Steps for SmolVLA Integration:

1.  **`SmolVLA` ROS Node Development:** Develop a Python-based ROS node within the Docker container that loads the `SmolVLA` model, processes Unity camera images and text commands, and generates robot action outputs.
2.  **Unity Vision & Language Interface:** Implement C# scripts in Unity to capture camera feeds, send them to the `SmolVLA` ROS node, provide a text command input UI, and receive/interpret `SmolVLA`'s robot action outputs to control the simulated Niryo One robot.

---

## Original Tutorial References:

(The original content of the Unity Robotics Hub Pick-and-Place tutorial's markdown files are linked above for detailed steps and concepts.)
