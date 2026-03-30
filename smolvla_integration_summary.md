# SmolVLA + Unity Robotics Hub Integration: Session Handover Summary

**최종 목표:** Unity 시뮬레이션(Mac 호스트)의 시각 및 상태 정보를 바탕으로 ROS 환경의 SmolVLA 모델이 50단계의 연속 로봇 동작을 추론하고 실행함.

---

## 1. 오늘 완료된 핵심 마일스톤

### 1.1 Unity (Host Mac)
*   **멀티 카메라 전송:** `RosImagePublisher.cs`를 수정하여 3개의 카메라(Top, Side, Wrist) 시점 이미지를 동시에 전송함. 게임 화면은 1번 카메라로 고정하고 보조 카메라는 스텔스 렌더링하도록 처리함.
*   **로봇 상태 동기화:** `RosCommandPublisher.cs`에서 Niryo 로봇의 실제 관절 각도(6축)를 추출하여 AI 모델의 입력값(`observation.state`)으로 전달함.
*   **궤적 실행 엔진:** `RosActionSubscriber.cs`를 구현하여 ROS에서 받은 50프레임의 데이터를 0.05초 간격 코루틴으로 실행, 부드러운 연속 동작을 완성함.

### 1.2 ROS & AI 모델 (Docker)
*   **3개 카메라 지원:** `smolvla_node.py`가 3개의 이미지 토픽을 동시에 수신하여 모델에 입력함.
*   **Autoregressive 추론:** 모델이 1프레임씩만 출력하는 한계를 극복하기 위해, 파이썬 코드 내에서 50번의 자기회귀 루프를 돌려 전체 궤적(Trajectory)을 생성함.
*   **수동 스케일링 적용:** 모델의 정규화된 출력값을 Unity 로봇의 물리적 가동 범위에 맞춰 2배 증폭(Scaling) 처리함.

---

## 2. 프로젝트 아키텍처 및 통신 토픽

*   **입력 (Unity -> ROS):**
    *   `/smolvla/camera_image1, 2, 3`: 3개 카메라 이미지 (256x256)
    *   `/smolvla/command_text`: 사용자 텍스트 명령
    *   `/smolvla/current_state`: 로봇 현재 관절 각도 (6축)
*   **출력 (ROS -> Unity):**
    *   `/smolvla/joint_action_cmd`: 50스텝의 목표 관절 각도 배열 (300개 float)

---

## 3. 향후 개선 과제 (한계점)

1.  **Open-loop 제어:** 현재는 첫 프레임 이미지만 보고 50단계를 상상함. 더 정밀한 작업을 위해서는 매 프레임 이미지를 갱신하는 실시간 제어(Closed-loop) 구조로의 고도화가 필요함.
2.  **모델 최적화:** 현재 사용 중인 `smolvla_base` 모델과 Niryo 로봇 간의 신체 규격 차이를 줄이기 위해 소량의 Niryo 전용 데이터를 이용한 **파인튜닝(Fine-tuning)** 권장.
3.  **카메라 각도:** AI 성능 극대화를 위해 Unity 내 카메라 위치를 실제 LeRobot 데이터셋과 더욱 정밀하게 일치시키는 작업 필요.

---

## 4. 실행 방법 요약
`manual_ros_setup_guide.md` 파일을 참조하여 터미널 1(MoveIt)과 터미널 2(SmolVLA)를 순서대로 실행하십시오.
