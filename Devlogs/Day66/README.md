# Project J - 66일차 개발 일지

## 개발 목표

65일차에서 구현한 Networked 점프·중력·Ground 판정 구조를 유지하면서, 기존 NetworkInput으로 전달되고 있던 Sprint 입력을 실제 이동 속도와 Stamina 상태에 연결한다.

이번 일차의 핵심 흐름은 다음과 같다.

```text
Shift 입력
↓
Sprint 가능 조건 확인
↓
Networked Sprint 상태 적용
↓
이동 속도 증가
↓
Stamina 소비
↓
Stamina 소진 시 Sprint 종료
↓
회복 후 재사용
```

Local Prediction, Resimulation, 점프·중력, Remote NetworkTransform Interpolation 구조는 그대로 유지한다.

---

## 주요 개발 내용

### 1. Sprint 이동 속도 추가

기존 기본 이동 속도와 별도로 Sprint 속도를 추가했다.

```text
Walk Speed
5

Sprint Speed
8
```

현재 이동 속도는 Sprint 상태에 따라 다음처럼 결정된다.

```text
IsSprinting = FALSE
→ 5

IsSprinting = TRUE
→ 8
```

외부 확인용 속성:

```text
WalkSpeed
SprintSpeed
CurrentMoveSpeed
MovementSpeed
```

---

## Networked Sprint 상태

### 2. Stamina 상태 추가

Sprint 사용량을 Fusion Tick 상태로 관리하기 위해 다음 Networked 값을 추가했다.

```text
NetworkStamina
```

기본 최대값:

```text
Max Stamina
100
```

State Authority가 Player를 Spawn할 때 Stamina를 최대치로 초기화한다.

외부 확인용 속성:

```text
Stamina
StaminaMaximum
```

---

### 3. Sprint 활성 상태 추가

현재 Player가 실제 Sprint 중인지 다음 Networked 값으로 기록한다.

```text
NetworkIsSprinting
```

외부 확인용 속성:

```text
IsSprinting
```

Sprint 조건은 다음과 같다.

```text
Sprint Input = TRUE
+
Move Input 존재
+
Stamina > 0
+
Exhausted 상태 아님
```

Shift만 누르고 이동하지 않을 때는 Sprint 상태가 되지 않는다.

---

### 4. Sprint Exhausted 상태 추가

Stamina를 완전히 소진했을 때 바로 Sprint가 반복 재시작되는 것을 막기 위해 다음 상태를 추가했다.

```text
NetworkSprintExhausted
```

외부 확인용 속성:

```text
IsSprintExhausted
```

Stamina가 0이 되면:

```text
Sprint = FALSE
Speed = Walk Speed
Exhausted = TRUE
```

상태로 변경된다.

---

## Stamina 소비와 회복

### 5. Sprint 중 Stamina 소비

Sprint 중에는 Fusion Tick마다 다음 계산을 수행한다.

```text
Stamina
-=
25 × Runner.DeltaTime
```

설정값:

```text
SprintStaminaDrainPerSecond
25
```

최대 Stamina 100 기준으로 약 4초간 연속 Sprint가 가능하다.

---

### 6. Sprint 종료 시 Stamina 회복

Sprint하지 않을 때는 다음 계산을 수행한다.

```text
Stamina
+=
20 × Runner.DeltaTime
```

설정값:

```text
StaminaRecoveryPerSecond
20
```

Stamina는 항상 다음 범위로 제한한다.

```text
0 ~ 100
```

---

### 7. 소진 후 재사용 조건

Stamina를 모두 소진하면 Sprint가 Exhausted 상태에 들어간다.

재사용 기준:

```text
SprintRestartStamina
20
```

처리 흐름:

```text
Stamina = 0
↓
Exhausted = TRUE
↓
Sprint 강제 종료
↓
Stamina 회복
↓
Shift 해제 상태에서 Stamina >= 20
↓
Exhausted 해제
↓
다시 Shift 입력
↓
Sprint 사용 가능
```

Shift를 계속 누르고 있는 것만으로 Sprint가 자동으로 반복 시작되지 않도록 구성했다.

---

## FixedUpdateNetwork 이동 확장

### 8. Sprint 상태 계산 연결

기존 `FixedUpdateNetwork()`에서 이동 입력을 읽은 뒤 다음 처리를 추가했다.

```text
Move Input 확인
↓
UpdateSprintState()
↓
CurrentMoveSpeed 계산
↓
XZ 이동
```

사용 메서드:

```text
UpdateSprintState()
```

---

### 9. 수평 이동 속도 변경

기존에는 모든 수평 이동이 다음 값만 사용했다.

```text
BaseMoveSpeed
```

66일차부터는 다음 값으로 이동한다.

```text
CurrentMoveSpeed
```

따라서:

```text
Walk
→ 5

Sprint
→ 8
```

로 X / Z 이동 속도가 전환된다.

---

## 점프·중력과의 연결

### 10. Sprint Jump 유지

65일차 점프와 중력 계산은 그대로 유지한다.

```text
W + Shift
→ Sprint

W + Shift + Space
→ Sprint 속도를 유지하면서 점프
```

공중에서도 Sprint 입력, 이동 입력, Stamina 조건을 만족하면 수평 Sprint 속도를 유지한다.

이번 단계에서는 별도의 Air Control 규칙이나 점프 전용 Sprint 처리는 추가하지 않았다.

---

## Prediction / Resimulation 유지

### 11. Client Sprint Prediction

Sprint와 Stamina 역시 `FixedUpdateNetwork()` 안에서 계산한다.

```text
Client Shift 입력
↓
NetworkInput
↓
FixedUpdateNetwork()
↓
Local Prediction
↓
즉시 Sprint
↓
Host 상태 수신
↓
필요 시 Resimulation
```

따라서 Client가 Host 응답을 기다린 뒤 늦게 빨라지는 구조가 아니라 기존 Prediction 흐름 안에서 Sprint를 처리한다.

---

## Remote Interpolation 유지

### 12. Remote Player 이동 표시

Remote Player는 기존 `NetworkTransform` 기반 Interpolation 구조를 그대로 사용한다.

```text
State Authority 이동 계산
↓
NetworkTransform Snapshot
↓
Remote Player Render
↓
Interpolation
```

Sprint 전용 RPC나 별도의 Remote Lerp는 추가하지 않았다.

---

## F2 네트워크 디버그 UI 확장

### 13. 66일차 화면 표시

F2 디버그 창 제목을 다음과 같이 변경했다.

```text
Project J - Fusion 66일차
```

진단 섹션 이름:

```text
Sprint · Stamina · Jump / Interpolation
```

---

### 14. Sprint / Stamina 진단 항목 추가

Player별로 다음 값을 확인할 수 있도록 수정했다.

```text
Player
Role
Ground
Sprint
Stamina
Speed
Exhausted
Vertical V
Sim Y
Interpolation
```

기본 이동 상태 예시:

```text
Sprint
-

Stamina
100.0 / 100

Speed
5.0

Exhausted
-
```

Sprint 중:

```text
Sprint
TRUE

Stamina
감소

Speed
8.0
```

Stamina 소진 후:

```text
Sprint
-

Stamina
0.0 / 100

Speed
5.0

Exhausted
TRUE
```

---

## 테스트 흐름

### Host 단독 테스트

```text
W
→ Speed 5

W + Shift
→ Speed 8
→ Stamina 감소

Shift 해제
→ Speed 5
→ Stamina 회복

W + Shift 계속 유지
→ Stamina 0
→ Sprint 강제 종료

Shift 해제 상태로 Stamina 20 이상 회복
→ Exhausted 해제

W + Shift
→ Sprint 재사용
```

---

### 점프 결합 테스트

```text
W + Shift
↓
Sprint
↓
Space
↓
Sprint 수평 속도를 유지하며 점프
↓
낙하
↓
착지
```

확인 항목:

```text
점프·중력 정상
Ground 판정 정상
Sprint 속도 유지
착지 후 이동 정상
```

---

### Host / Client 테스트

```text
Unity Editor
→ Host

Development Build
→ Client
```

Client에서 Sprint 입력:

```text
Client Local Player
→ 즉시 Sprint

Host 화면
→ Client Player의 빠른 이동 확인
```

반대로 Host가 Sprint할 때 Client 화면에서 Remote Host Player의 이동이 부드럽게 표시되는지 확인한다.

---

## 지연 환경 테스트

기존 Network Conditions 기준을 사용할 수 있다.

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
Client Sprint가 즉시 반응
Stamina 값이 지속적으로 크게 어긋나지 않음
Sprint 종료 / 회복 정상
점프 Prediction 유지
Remote Interpolation 유지
큰 위치 순간이동 없음
```

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

## 66일차 완료 기준

```text
Shift 입력 실제 Sprint 연결
↓
Walk / Sprint 속도 전환
↓
Networked Stamina 적용
↓
Networked Sprint 상태 적용
↓
Networked Exhausted 상태 적용
↓
Sprint 중 Stamina 소비
↓
비 Sprint 상태에서 Stamina 회복
↓
Stamina 0에서 Sprint 강제 종료
↓
회복 후 Sprint 재사용
↓
Sprint Jump 정상
↓
Local Prediction 유지
↓
Resimulation 구조 유지
↓
Remote Interpolation 유지
↓
F2 Sprint / Stamina 진단 추가
```

---

## 최신 커밋 확인

README 작성 시점 최신 `main` 커밋:

```text
53594fa9b85dbc805589d9c11296b618f47c0c46
66
```

이 커밋은 65일차 커밋 `15707fbc3e7144e66fa57145223a8faca738fb81` 바로 다음에 이어지는 1개 커밋이다.

65일차 대비 변경 파일은 다음 두 개뿐이다.

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/ProjectJFusionBootstrapDebugView.cs
Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkPlayer.cs
```

GitHub에는 해당 커밋에 대한 CI 상태가 등록되어 있지 않으므로 Unity Compile, Test Runner, Host / Client Runtime 결과는 로컬 실행을 최종 기준으로 확인한다.

---

## 다음 개발 방향

다음 67일차에서는 이미 NetworkInput으로 전달되고 있는 Crouch 입력을 실제 Player 상태에 연결한다.

예상 흐름:

```text
Crouch Input
↓
Networked Crouch 상태
↓
Player 높이 / 시각 크기 변경
↓
낮은 공간 진입
↓
천장 검사 후 일어서기 허용
↓
Host / Client 동기화
```

기존 이동, 점프, Sprint, Stamina, Prediction, Resimulation, Remote Interpolation 구조를 유지한 채 Crouch 상태를 확장한다.
