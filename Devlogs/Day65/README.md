# Project J - 65일차 개발 일지

## 개발 목표

64일차에서 구축한 Local Prediction / Remote Interpolation 구조를 유지한 채, Network Player의 기본 XZ 이동에 점프와 중력을 추가한다.

이번 일차의 핵심 흐름은 다음과 같다.

```text
Space 입력
↓
ProjectJNetworkInput.Jump
↓
FixedUpdateNetwork()
↓
Ground 판정
↓
Vertical Velocity 적용
↓
Gravity 누적
↓
Y Position 변경
↓
NetworkTransform
↓
Host / Client 동기화
```

이번 단계에서는 점프와 중력만 네트워크 이동에 연결하며 Sprint, Stamina, Crouch의 실제 동작은 아직 적용하지 않는다.

---

## 주요 개발 내용

### 1. 점프 / 중력 기본 수치 추가

`ProjectJNetworkPlayer`에 수직 이동 테스트용 상수를 추가했다.

```text
BaseMoveSpeed
= 5

JumpVelocity
= 7

Gravity
= -20

GroundProbeStartHeight
= 0.15

GroundProbeDistance
= 0.25
```

현재 값은 네트워크 이동 구조 검증을 위한 테스트 수치다.

---

## Networked 수직 이동 상태

### 2. Vertical Velocity 네트워크 상태 추가

점프와 낙하를 계산하기 위한 수직 속도를 Networked 상태로 추가했다.

```text
NetworkVerticalVelocity
```

외부 진단용 속성:

```text
VerticalVelocity
```

이를 통해 Fusion Prediction / Resimulation 과정에서도 수직 이동 상태를 Tick 단위로 다룰 수 있는 구조를 마련했다.

---

### 3. Grounded 네트워크 상태 추가

현재 Player가 지면 위에 있는지 기록하는 상태를 추가했다.

```text
NetworkGrounded
```

외부 확인용 속성:

```text
IsGrounded
```

점프는 다음 조건에서만 시작한다.

```text
Jump Input
+
Grounded = TRUE
```

따라서 공중에서 Space를 다시 눌러도 추가 점프가 발생하지 않는다.

---

## Ground 판정

### 4. Ground Height 검사 추가

Player Root 위치 아래쪽으로 Raycast를 실행해 지면 높이를 확인한다.

```text
Player Position
↓
GroundProbeStartHeight만큼 위에서 시작
↓
Vector3.down Raycast
↓
Collider 확인
↓
Ground Height 반환
```

Trigger Collider는 Ground 판정에서 제외한다.

사용 메서드:

```text
TryGetGroundHeight()
```

---

### 5. 지면 위 위치 보정

Player가 지면 가까이에 있고 수직 속도가 0 이하라면 다음 처리를 한다.

```text
Grounded = TRUE
Vertical Velocity = 0
Player Y = Ground Height
```

이를 통해 지면 위에서 Gravity가 계속 누적되지 않도록 했다.

---

## 점프 처리

### 6. Jump Input 실제 이동 연결

61일차부터 이미 전달하고 있던 `ProjectJNetworkButton.Jump` 입력을 실제 수직 이동에 연결했다.

```text
LastReceivedJump = TRUE
+
NetworkGrounded = TRUE
↓
NetworkVerticalVelocity = 7
↓
NetworkGrounded = FALSE
```

새로운 입력 시스템이나 Jump RPC는 추가하지 않았다.

---

## Gravity 처리

### 7. Fusion Tick 기반 중력 적용

공중에서는 매 Simulation Tick마다 다음 계산을 수행한다.

```text
NetworkVerticalVelocity
+= Gravity × Runner.DeltaTime
```

즉 Unity 일반 Frame 시간인 `Time.deltaTime`이 아니라 Fusion Simulation 기준 시간인 `Runner.DeltaTime`을 사용한다.

수직 위치는 다음과 같이 계산한다.

```text
Next Y
=
Current Y
+
Vertical Velocity × Runner.DeltaTime
```

---

## X / Y / Z 이동 통합

### 8. 기본 이동과 수직 이동 통합

기존 XZ 이동과 점프 / 중력을 하나의 `FixedUpdateNetwork()`에서 계산한다.

```text
X
→ A / D

Y
→ Jump / Gravity

Z
→ W / S
```

현재 이동 구조:

```text
ProjectJNetworkInput
↓
FixedUpdateNetwork()
↓
Horizontal Move 계산
↓
Ground 판정
↓
Jump 처리
↓
Gravity 처리
↓
Landing 처리
↓
Transform Position 확정
↓
NetworkTransform 동기화
```

---

## 착지 처리

### 9. 낙하 구간 Ground 검사 추가

낙하 중 현재 위치와 다음 위치 사이에 지면이 존재하는지 검사한다.

사용 메서드:

```text
TryGetLandingGroundHeight()
```

처리 구조:

```text
Vertical Velocity <= 0
↓
현재 Y → 다음 Y 낙하 거리 계산
↓
아래쪽 Raycast
↓
지면 발견
↓
Next Y = Ground Height
↓
Vertical Velocity = 0
↓
Grounded = TRUE
```

이를 통해 한 Tick 안에서 지면 아래로 통과하는 상황을 줄이도록 했다.

---

## Prediction / Resimulation 유지

### 10. Local Player Prediction 유지

63일차에서 구축한 Prediction / Resimulation 구조는 그대로 유지한다.

```text
Client Jump Input
↓
Local Prediction
↓
즉시 점프
↓
Host 권위 상태 수신
↓
Rollback / Resimulation
↓
Correction 확인
```

점프와 중력 역시 `FixedUpdateNetwork()`와 Networked 상태를 기준으로 계산한다.

기존 진단값:

```text
ResimulationBatchCount
ResimulationTickCount
Rollback Distance
Correction Distance
Max Correction Distance
```

도 그대로 유지한다.

---

## Remote Interpolation 유지

### 11. Remote Player Y 이동 보간

64일차에서 사용한 `NetworkTransform` 기반 Remote Interpolation 구조를 그대로 유지한다.

```text
Host / State Authority 점프 계산
↓
NetworkTransform Snapshot
↓
Remote Player Render
↓
Y축 이동 Interpolation
```

점프 전용 RPC 또는 별도의 Render Lerp는 추가하지 않았다.

---

## F2 네트워크 디버그 UI 확장

### 12. 65일차 화면 표시

F2 디버그 창 제목을 다음과 같이 변경했다.

```text
Project J - Fusion 65일차
```

Remote 진단 섹션은 다음 이름으로 확장했다.

```text
Jump · Gravity · Remote Interpolation
```

---

### 13. 수직 이동 진단 정보 추가

Player별 표시 항목:

```text
Player
Role
Ground
Vertical V
Sim Y
Render Y
Offset
Render Δ
Interpolation
```

예상 지상 상태:

```text
Ground
TRUE

Vertical V
0.00
```

점프 상승 중:

```text
Ground
-

Vertical V
양수

Sim Y
증가
```

낙하 중:

```text
Vertical V
음수

Sim Y
감소
```

착지 후:

```text
Ground
TRUE

Vertical V
0.00
```

---

## 테스트 흐름

### Host 단독 테스트

```text
Host Session 생성
↓
Network Player Spawn
↓
Space 입력
↓
상승
↓
정점
↓
낙하
↓
착지
```

확인 항목:

```text
Grounded 전환 정상
Vertical Velocity 증가 / 감소 정상
Y Position 변화 정상
착지 후 Vertical Velocity = 0
```

---

### Host / Client 테스트

```text
Unity Editor
→ Host

Development Build
→ Client
```

Client에서 Space 입력:

```text
Client Local Player
→ 즉시 점프

Host 화면
→ Client Player 점프 확인
```

Host에서 Space 입력:

```text
Host Player
→ 점프

Client 화면
→ Remote Host Player Y 이동 확인
```

---

## 지연 환경 테스트

기존 Network Conditions 테스트값을 사용할 수 있다.

```text
Delay
150 ms

Jitter
0

Packet Loss
0
```

확인 기준:

```text
Client Local 점프가 즉시 반응
큰 Y 위치 순간이동 없음
Correction 값 지속 증가 없음
Remote Player 점프가 부드럽게 표시
착지 후 Y 떨림 없음
```

테스트가 끝난 뒤 Network Conditions는 다시 비활성화한다.

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
└─ ProjectJFusionBootstrapDebugView.cs

Assets/ProjectJ/Network/Fusion/Player/
└─ ProjectJNetworkPlayer.cs
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

## 65일차 완료 기준

```text
Space 입력 실제 점프 연결
↓
Ground 판정 추가
↓
지상에서만 점프 허용
↓
Networked Vertical Velocity 적용
↓
Networked Grounded 상태 적용
↓
Runner.DeltaTime 기반 Gravity 적용
↓
XZ + Y 이동 통합
↓
낙하 구간 착지 검사
↓
Host 점프 정상
↓
Client 점프 정상
↓
Local Prediction 유지
↓
Remote Y Interpolation 유지
↓
공중 연속 점프 없음
↓
착지 후 Vertical Velocity = 0
↓
지속적인 Y 떨림 없음
```

---

## 최신 커밋 확인

README 작성 시점 최신 `main` 커밋:

```text
a8ca779716f59559aa39e50e65867a21d237f910
65
```

이 커밋은 64일차 커밋 `b1b489161e524c986ca555dfd13208086a33877a` 바로 다음에 이어지는 1개 커밋이다.

64일차 대비 변경 파일은 다음 두 개뿐이다.

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/ProjectJFusionBootstrapDebugView.cs
Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkPlayer.cs
```

GitHub에는 해당 커밋에 대한 CI 상태가 등록되어 있지 않으므로 Unity Compile, Test Runner, Host / Client Runtime 결과는 로컬 실행을 최종 기준으로 확인한다.

---

## 다음 개발 방향

다음 66일차에서는 이미 NetworkInput으로 전달되고 있는 Sprint 입력을 실제 이동 속도와 Stamina 상태에 연결한다.

예상 흐름:

```text
Shift 입력
↓
Sprint 조건 확인
↓
Stamina 소비
↓
Sprint Speed 적용
↓
Shift 해제 / Stamina 부족
↓
기본 속도로 복귀
↓
Stamina 회복
```

현재 구축된 Tick Input, Prediction, Resimulation, Remote Interpolation 구조 위에서 Sprint / Stamina를 네트워크 상태로 확장한다.
