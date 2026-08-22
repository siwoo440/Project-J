# Project J - 62일차 개발 일지

## 개발 목표

61일차에서 구축한 Photon Fusion Tick Input 구조를 실제 Network Player 이동에 연결한다.

이번 일차의 핵심 목표는 다음과 같다.

```text
WASD 입력
↓
ProjectJFusionInputProvider
↓
ProjectJNetworkInput.Move
↓
FixedUpdateNetwork()
↓
Network Player 위치 계산
↓
NetworkTransform
↓
Host / Client 위치 동기화
```

이번 단계에서는 XZ 평면의 기본 이동만 구현하며, 점프·중력·Sprint·Crouch의 실제 동작은 아직 적용하지 않는다.

---

## 주요 개발 내용

### 1. Fusion Tick 기반 기본 이동 구현

`ProjectJNetworkPlayer.FixedUpdateNetwork()`에서 61일차에 수신하던 `ProjectJNetworkInput.Move` 값을 실제 위치 이동에 사용하도록 확장했다.

입력 방향은 다음과 같다.

```text
W
→ World +Z

S
→ World -Z

A
→ World -X

D
→ World +X
```

현재는 카메라 방향과 무관한 World 좌표 기준 이동으로 구현했다.

---

### 2. 이동 입력 정규화

대각선 입력 시 이동 속도가 더 빨라지지 않도록 `Vector2` 입력의 크기가 1을 넘으면 정규화한다.

```text
W
→ (0, 1)

D
→ (1, 0)

W + D
→ 정규화된 대각선 입력
```

이를 통해 직선과 대각선 이동의 최대 속도를 동일하게 유지한다.

---

### 3. 기본 이동 속도 추가

Network Player에 테스트용 기본 이동 속도를 추가했다.

```text
BaseMoveSpeed = 5
```

이번 일차에서는 모든 Player가 동일한 속도를 사용한다.

아직 다음 요소는 이동 속도에 영향을 주지 않는다.

```text
Sprint
Crouch
Stamina
가속
감속
공중 이동
```

---

### 4. Runner.DeltaTime 기반 이동

네트워크 Simulation 이동 계산에는 Unity 일반 Frame 시간 대신 Fusion Simulation 시간을 사용한다.

```text
Runner.DeltaTime
```

기본 이동 계산 구조:

```text
현재 위치
+
이동 방향
×
BaseMoveSpeed
×
Runner.DeltaTime
```

이를 `FixedUpdateNetwork()` 안에서 실행하도록 했다.

---

### 5. NetworkTransform 기반 위치 동기화

60일차 Network Player Prefab에 적용한 `NetworkTransform`을 그대로 활용한다.

따라서 별도의 이동 RPC를 추가하지 않았다.

현재 구조:

```text
NetworkInput
↓
FixedUpdateNetwork()
↓
Transform 위치 변경
↓
NetworkTransform
↓
다른 Peer에 위치 반영
```

Player 이동을 매 Frame RPC로 전송하는 구조는 사용하지 않는다.

---

## Authority와 이동 처리

### Host Player

```text
Host 입력
↓
Host Player GetInput()
↓
FixedUpdateNetwork()
↓
Host Player 이동
```

### Client Player

```text
Client 입력
↓
Fusion Input 전달
↓
Client Player GetInput()
↓
Simulation 이동
↓
Host 권위 상태와 NetworkTransform 동기화
```

각 Player는 자신의 Input Authority에 연결된 입력만 사용한다.

따라서 Host 입력으로 Client Player가 같이 움직이거나 Client 입력으로 Host Player가 움직이지 않아야 한다.

---

## 아직 적용하지 않은 입력

61일차에서 이미 전달하고 있는 다음 버튼 입력은 계속 수신한다.

```text
Jump
Sprint
Crouch
```

하지만 62일차에서는 실제 동작에 사용하지 않는다.

```text
Space
→ Jump 입력 수신만
→ Y 이동 없음

Shift
→ Sprint 입력 수신만
→ 속도는 5 유지

Ctrl
→ Crouch 입력 수신만
→ 자세 변화 없음
```

이동 네트워크화와 다른 기능을 분리해 검증하기 위한 상태다.

---

## F2 네트워크 디버그 UI 확장

### 6. 62일차 UI 표시

디버그 창 제목을 다음과 같이 변경했다.

```text
Project J - Fusion 62일차
```

그리고 섹션명을 다음과 같이 확장했다.

```text
Fusion Tick Input · Basic Movement
```

---

### 7. 디버그 창 너비 확대

Player별 Position 정보를 함께 표시하기 위해 F2 창 너비를 확장했다.

```text
기존
1000 × 900

변경
1180 × 900
```

---

### 8. Player Position 표시 추가

각 Network Player 행에 현재 Transform Position을 표시한다.

예:

```text
P0
Position
(0.00, 2.00, 8.35)

P1
Position
(7.42, 2.00, 4.00)
```

이를 통해 Host와 Client에서 같은 Player의 위치가 갱신되는지 쉽게 비교할 수 있다.

---

### 9. 이동 속도 표시 추가

각 Player 행에 현재 기본 이동 속도를 표시한다.

```text
Speed
5.0
```

현재는 모든 Network Player가 동일한 `BaseMoveSpeed`를 사용한다.

---

## 62일차 F2 Player 표시 항목

```text
Player
State
Input
Move
Position
Jump
Sprint
Crouch
Camera
Received Tick
Speed
```

최대 8명의 Player를 한 화면에서 확인할 수 있다.

---

## 전체 네트워크 이동 흐름

```text
Keyboard
↓
ProjectJFusionInputProvider.Update()
↓
INetworkRunnerCallbacks.OnInput()
↓
ProjectJNetworkInput
↓
Fusion Tick
↓
ProjectJNetworkPlayer.FixedUpdateNetwork()
↓
GetInput<ProjectJNetworkInput>()
↓
Move Vector2
↓
World XZ 이동 방향 변환
↓
BaseMoveSpeed × Runner.DeltaTime
↓
Transform Position 변경
↓
NetworkTransform
↓
Host / Client 위치 동기화
```

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Network/Fusion/Bootstrap/
└─ ProjectJFusionBootstrapDebugView.cs
```

---

## 생성 파일

```text
없음
```

---

## 삭제 파일

```text
없음
```

---

## 테스트 항목

### Host 기본 이동

```text
Editor에서 Host 생성
↓
WASD 입력
↓
Host Player 이동
↓
F2 Position 변화 확인
```

### Client 기본 이동

```text
Development Build 실행
↓
Host 방 코드 참가
↓
WASD 입력
↓
Client Player 이동
```

### 상호 위치 확인

```text
Host가 W 입력
→ Host Player만 +Z 이동
→ Client 화면에서도 Host Player 위치 변화

Client가 D 입력
→ Client Player만 +X 이동
→ Host 화면에서도 Client Player 위치 변화
```

---

## 62일차 완료 기준

```text
Host WASD 이동 가능
↓
Client WASD 이동 가능
↓
각자의 입력으로 각자의 Player만 이동
↓
Runner.DeltaTime 기반 이동
↓
대각선 입력 정규화
↓
NetworkTransform 위치 동기화
↓
Host에서 Client 이동 확인
↓
Client에서 Host 이동 확인
↓
F2 Position 갱신
↓
Speed = 5.0 확인
↓
Jump 실제 동작 없음
↓
Gravity 없음
↓
Sprint 속도 변화 없음
↓
Crouch 실제 동작 없음
```

---

## 현재 남아 있는 Fusion Scene Warning

현재 다음 Warning은 계속 발생할 수 있다.

```text
[Fusion] NetworkRunner started with no scene in StartGameArgs.Scene.
No network scene will be loaded and no scene NetworkObjects will be spawned.
```

현재 Network Player는 Scene에 미리 배치된 NetworkObject가 아니라 `Runner.Spawn()`으로 Runtime 생성하고 있으므로, 62일차 기본 이동 검증 자체를 막는 오류로 취급하지 않는다.

Network Scene과 Scene NetworkObject 동기화는 이후 별도 단계에서 연결한다.

---

## 최신 커밋 확인

README 작성 시점 최신 `main` 커밋:

```text
6b4e85d41edf79572c17e0335374598646f4743e
62
```

이 커밋은 61일차 커밋 바로 다음에 이어지는 1개 커밋이며, 변경 파일은 다음 두 개뿐이다.

```text
ProjectJFusionBootstrapDebugView.cs
ProjectJNetworkPlayer.cs
```

GitHub에는 해당 커밋에 대한 CI 상태가 등록되어 있지 않으므로 실제 Unity Compile 및 Host/Client Runtime 성공 여부는 로컬 테스트에서 최종 확인한다.

---

## 다음 개발 방향

다음 일차에서는 기본 이동을 대상으로 Prediction과 Resimulation 동작을 검증한다.

예상 흐름:

```text
Client 입력
↓
Client 예측 이동
↓
Host 권위 Simulation
↓
상태 차이 발생
↓
Resimulation
↓
위치 보정
```

지연 환경에서도 Local Player가 즉각 반응하고, Host 권위 상태와 차이가 발생했을 때 자연스럽게 보정되는지 확인하는 단계로 진행한다.
