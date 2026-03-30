FROM ros:noetic-ros-base

RUN sudo apt-key adv --keyserver keyserver.ubuntu.com --recv-keys F42ED6FBAB17C654

# Install basic dependencies and build tools for compiling Python
RUN sudo apt-get update && sudo apt-get install -y vim iputils-ping net-tools ros-noetic-robot-state-publisher ros-noetic-moveit ros-noetic-rosbridge-suite ros-noetic-joy ros-noetic-ros-control ros-noetic-ros-controllers ros-noetic-tf2-web-republisher dos2unix git curl wget \
    build-essential zlib1g-dev libncurses5-dev libgdbm-dev libnss3-dev libssl-dev libreadline-dev libffi-dev libsqlite3-dev python3-pip python-is-python3

# Install rospkg for system python (ROS Melodic default)
RUN sudo -H pip3 install rospkg jsonpickle

# Compile Python 3.12 from source (Resolves GLIBC 2.28+ dependency issue)
RUN curl -O https://www.python.org/ftp/python/3.12.6/Python-3.12.6.tar.xz && \
    tar -xf Python-3.12.6.tar.xz && \
    cd Python-3.12.6 && \
    ./configure --enable-optimizations && \
    make -j$(nproc) && \
    sudo make altinstall && \
    cd .. && rm -rf Python-3.12.6*

# Upgrade pip for python3.12 and install ROS dependencies for it
RUN python3.12 -m pip install --upgrade pip && \
    python3.12 -m pip install rospkg jsonpickle defusedxml netifaces

ENV ROS_WORKSPACE=/catkin_ws

# SmolVLA installation steps
RUN git clone https://github.com/huggingface/lerobot.git /lerobot_ws
WORKDIR /lerobot_ws

# Install PyTorch CPU version explicitly (Safe for Mac M2 Docker ARM64 architecture)
RUN python3.12 -m pip install torch torchvision torchaudio

# Relax torchvision version constraint to allow newer torchvision versions
RUN sed -i 's/<0.26.0//g' pyproject.toml

# Install lerobot with smolvla dependencies
RUN python3.12 -m pip install -e ".[smolvla]"

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

# Uninstall cmake installed by pip to force catkin_make to use system cmake (3.16) which is compatible with older ROS packages
RUN python3.12 -m pip uninstall -y cmake

# Set up workspace and install Python dependencies
RUN dos2unix /tutorial && dos2unix /setup.sh && chmod +x /setup.sh && /setup.sh && rm /setup.sh

WORKDIR $ROS_WORKSPACE

# making sure the file modes are executable
RUN chmod +x src/niryo_moveit/scripts/*.py
RUN chmod +x src/smolvla_ros/scripts/*.py

COPY ./docker/start_roscore.sh /start_roscore.sh
COPY ./docker/start_subscriber.sh /start_subscriber.sh

RUN chmod +x /start_roscore.sh /start_subscriber.sh
