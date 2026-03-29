cmake_minimum_required(VERSION 3.0.2)
project(smolvla_ros)

## Compile as C++11, pulled from the ROS default for c++11
# add_compile_options(-std=c++11)

## Find catkin macros and libraries
find_package(catkin REQUIRED COMPONENTS
  rospy
  std_msgs
  sensor_msgs
  message_generation
)

## System dependencies of this project that are NOT packaged by ROS
# find_package(Boost REQUIRED COMPONENTS system)

## Uncomment this if the package has a setup.py. This should be done in the source space
## where you copy the files from, currently the repository is not setup this way
catkin_python_setup()


## Declare ROS messages, services and actions ##
add_message_files(
  FILES
  SmolVLACommandImage.msg
)

## Uncomment at least one of the following two lines
# add_service_files(
#   FILES
#   AddTwoInts.srv
# )

# add_action_files(
#   FILES
#   Test.action
# )

generate_messages(
  DEPENDENCIES
  std_msgs
  sensor_msgs
)

## Declare a catkin package
catkin_package(
#  INCLUDE_DIRS include
  LIBRARIES ${PROJECT_NAME}
  CATKIN_DEPENDS rospy std_msgs sensor_msgs message_runtime
#  DEPENDS system_lib
)

## Build
# add_executable(${PROJECT_NAME}_node src/${PROJECT_NAME}_node.cpp)
# add_library(${PROJECT_NAME} src/${PROJECT_NAME}.cpp)

# add_dependencies(${PROJECT_NAME}_node ${${PROJECT_NAME}_EXPORTED_TARGETS} ${catkin_EXPORTED_TARGETS})
# add_custom_command(
#   TARGET ${PROJECT_NAME}_node
#   PRE_BUILD
#   COMMAND ${CATKIN_ENV} python ${PROJECT_SOURCE_DIR}/scripts/myscript.py
# )

## Add Python scripts (optional)
catkin_install_python(
  PROGRAMS scripts/smolvla_node.py
  DESTINATION ${CATKIN_PACKAGE_BIN_DESTINATION}
)

## Add source files to a library or executable
# add_library(${PROJECT_NAME} ${${PROJECT_NAME}_SRCS})

## Add extra build targets: ROS topics, services, messages, etc.

# add_message_files(FILES
#   Message1.msg
#   Message2.msg
# )

# add_service_files(FILES
#   Service1.srv
#   Service2.srv
# )

# add_action_files(FILES
#   Action1.action
#   Action2.action
# )

# generate_messages(DEPENDENCIES
#   std_msgs
# )

# For more detailed documentation of the catkin build system see:
# http://ros.org/wiki/catkin/CMakeLists.txt
