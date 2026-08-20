# 21일차 개발일지 - Ledge Climb 구현

## 개발 목표

20일차에 구현한 `PlayerLedgeDetector`의 감지 결과를 실제 플레이어 이동으로 연결한다.

이번 일차에서는 애니메이션이나 IK 없이 기능 중심의 Ledge Climb을 구현하며, 유효한 턱 앞에서 Space 입력 시 일반 Jump보다 Climb을 우선 처리하도록 구성한다.

핵심 목표는 다음과 같다.

- 유효한 Ledge에서 Space 입력으로 Climb 시작
- Climb 중 기존 이동 시스템 일시 정지
- Rigidbody 속도 초기화
- 턱 방향으로 플레이어 정렬
- Lift → Forward 두 단계 이동
- Climb 종료 후 Collider / Rigidbody 상태 복구
- Climb 불가 상황에서는 기존 Jump 유지
- 기존 Walk / Sprint / Crouch / Jump 시스템 회귀 방지

---

## 주요 구현 내용

### 1. PlayerLedgeClimber 추가

새로운 런타임 컴포넌트를 추가했다.

```text
PlayerLedgeDetector
→ Ledge 감지 담당

PlayerLedgeClimber
→ 실제 Climb 이동 담당

PlayerCameraRelativeMovement
→ 일반 이동 및 Jump 입력 담당
```

Ledge 감지와 실제 이동을 분리해 각 스크립트의 역할을 명확하게 유지했다.

---

## 2. Climb 시작 조건

다음 조건을 모두 만족해야 Climb을 시작한다.

```text
HasLedge = true
IsClimbing = false
IsCrouching = false
```

따라서 다음 상황에서는 Climb을 시작하지 않는다.

```text
Ledge 없음
이미 Climb 진행 중
Crouch 상태
```

---

## 3. Space 입력 우선순위 변경

기존 Space 입력은 Jump Buffer를 활성화했다.

21일차부터는 Space 입력 시 먼저 Ledge Climb 가능 여부를 확인한다.

```text
Space 입력
↓
Climb 가능?
├─ Yes → Ledge Climb
└─ No  → 기존 Jump Buffer
```

유효한 Ledge가 있을 경우 일반 Jump와 Climb이 동시에 발생하지 않도록 처리했다.

Climb 시작 시:

```text
Coyote Timer = 0
Jump Buffer = 0
Sprint = false
```

로 정리한다.

---

## 4. Climb 중 일반 이동 중지

`PlayerCameraRelativeMovement`는 `PlayerLedgeClimber.IsClimbing`을 확인한다.

Climb 중에는 기존 FixedUpdate 이동 계산을 조기에 종료한다.

따라서 다음 기능이 Climb 이동과 동시에 개입하지 않는다.

```text
Walk
Sprint
Gravity
Ground Snap
Step Assist
Slope Movement
```

---

## 5. Rigidbody 상태 전환

Climb 시작 시 기존 물리 속도를 제거한다.

```text
linearVelocity = 0
angularVelocity = 0
```

그리고 Climb 이동 동안:

```text
Rigidbody.isKinematic = true
```

로 전환한다.

이를 통해 Gravity나 외부 물리 힘 때문에 Climb 경로가 틀어지는 것을 방지한다.

---

## 6. Collider 일시 비활성화

Climb 과정에서는 CapsuleCollider를 잠시 비활성화한다.

```text
Climb 시작
→ CapsuleCollider OFF

Climb 종료
→ 이전 상태로 복구
```

턱 앞 벽 Collider가 플레이어 이동을 막아 Climb이 중단되는 문제를 피하기 위한 초기 기능 구현 방식이다.

실제 애니메이션과 정교한 Collision 처리는 이후 단계에서 보완할 수 있다.

---

## 7. Ledge Detector 일시 정지

Climb 시작 시 `PlayerLedgeDetector`도 잠시 비활성화한다.

```text
Climb 시작
→ Detector OFF

Climb 종료
→ 기존 상태 복구
```

Climb 중 플레이어 위치가 변경되면서 새로운 Ledge를 연속 검출하는 상황을 방지한다.

---

## 8. 착지 위치 계산

20일차에서 계산한:

```text
LedgeTopPoint
```

는 플레이어 발이 놓일 위치의 기준으로 사용한다.

현재 CapsuleCollider의 발 위치와 Player Transform 간 Offset을 계산하고, 이를 적용해 최종 Rigidbody 중심 위치를 계산한다.

```text
LedgeTopPoint
+
Foot To Body Offset
=
Target Body Position
```

이를 통해 Climb 완료 후 플레이어 발이 턱 윗면에 놓이도록 한다.

---

## 9. 플레이어 방향 정렬

20일차의:

```text
LedgeWallNormal
```

을 사용해 플레이어가 벽을 향하도록 회전한다.

```text
Target Forward = -LedgeWallNormal
```

Climb 중 위치 이동과 함께 회전도 보간한다.

---

## 10. 2단계 Climb 이동

Climb은 한 번에 대각선으로 이동하지 않고 두 단계로 나눈다.

### Lift 단계

```text
현재 위치
↓
턱 위 높이까지 상승
```

기본 설정:

```text
Lift Duration = 0.2
Lift Clearance = 0.08
```

턱 높이보다 약간 위쪽까지 올라가도록 Clearance를 추가한다.

### Forward 단계

```text
Lift 위치
↓
LedgeTopPoint 기반 Target Position
```

기본 설정:

```text
Forward Duration = 0.2
```

전체 Climb 시간은 약 0.4초이다.

---

## 11. Climb 종료 및 상태 복구

Target Position에 도착하면 다음 상태를 복구한다.

```text
CapsuleCollider
PlayerLedgeDetector
Rigidbody Kinematic 상태
```

그리고 일반 물리 상태로 복귀했을 경우:

```text
linearVelocity = 0
angularVelocity = 0
```

로 초기화한다.

마지막으로:

```text
IsClimbing = false
```

로 변경해 일반 이동을 다시 사용할 수 있게 한다.

---

## 주요 설정값

`PlayerLedgeClimber` 기본값:

```text
Lift Duration     = 0.2
Forward Duration  = 0.2
Lift Clearance    = 0.08
```

20일차 `PlayerLedgeDetector` 기준값은 유지한다.

```text
Min Ledge Height          = 0.45
Max Ledge Height          = 1.4
Wall Check Distance       = 0.8
Top Surface Max Angle     = 45
Landing Forward Offset    = 0.35
```

---

## Player Prefab 적용

Player Prefab에 다음 컴포넌트를 추가했다.

```text
PlayerLedgeClimber
```

Player는 현재 다음 이동 관련 컴포넌트를 함께 사용한다.

```text
PlayerCameraRelativeMovement
PlayerLedgeDetector
PlayerLedgeClimber
```

---

## 테스트 구역

새 맵 구조는 추가하지 않고 20일차에 만든 Zone 08 테스트 구역을 그대로 사용한다.

```text
Zone_08_JumpBlocks
└─ Day20_LedgeDetectTests
```

Climb 성공 대상:

```text
Ledge_0.7
Ledge_1.0
Ledge_1.4
```

Climb 불가 대상:

```text
Ledge_0.3
Ledge_1.7
BlockedHeadroom
SteepTop_60deg
```

---

## 자동 테스트 추가

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlayerLedgeClimberTests.cs
```

주요 테스트 항목:

- 유효한 Ledge에서 Climb 시작 가능
- Ledge가 없으면 Climb 차단
- 이미 Climb 중이면 재시작 차단
- Crouch 중이면 Climb 차단
- 발 위치와 Rigidbody 중심 Offset 계산
- Ledge Top에서 최종 Player 위치 계산
- Lift 위치가 Target보다 Clearance만큼 높은지 확인
- Climb Phase 진행률 계산
- 진행률 0~1 Clamp
- Wall Normal 기준 플레이어 회전 계산

---

## 변경 파일

### 생성

```text
Assets/ProjectJ/Runtime/Player/
└─ PlayerLedgeClimber.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerLedgeClimberTests.cs
```

### 수정

```text
Assets/ProjectJ/Runtime/Player/
└─ PlayerCameraRelativeMovement.cs

Assets/ProjectJ/Prefabs/Player/
└─ Player.prefab
```

### 삭제

```text
없음
```

---

## 수동 테스트 항목

### 정상 Climb

`Ledge_0.7`, `Ledge_1.0`, `Ledge_1.4`에서:

- [ ] Ledge가 정상 감지됨
- [ ] Space 입력 시 일반 Jump 대신 Climb 시작
- [ ] 플레이어가 먼저 위로 이동
- [ ] 이후 턱 안쪽으로 이동
- [ ] 턱 위에 정상 착지
- [ ] 플레이어가 벽 방향으로 정렬됨
- [ ] Climb 종료 후 일반 이동 복귀

### Climb 불가 구조

다음 구조에서 Space 입력 시 Climb이 시작되지 않아야 한다.

- [ ] Ledge_0.3
- [ ] Ledge_1.7
- [ ] BlockedHeadroom
- [ ] SteepTop_60deg

### Crouch 상태

- [ ] Ctrl 유지 중 Climb 시작 안 됨
- [ ] 기존 Jump 동작 유지

### Climb 중 입력

- [ ] 이동 입력이 Climb 경로를 방해하지 않음
- [ ] Sprint가 개입하지 않음
- [ ] Gravity가 개입하지 않음
- [ ] Ground Snap이 개입하지 않음
- [ ] Step Assist가 개입하지 않음
- [ ] Space 연타로 Jump Buffer가 누적되지 않음

### Climb 종료 후 회귀

- [ ] Walk 정상
- [ ] Sprint 정상
- [ ] Stamina 정상
- [ ] Crouch 정상
- [ ] Standing Space Check 정상
- [ ] Jump 정상
- [ ] Coyote Time 정상
- [ ] Jump Buffer 정상
- [ ] Slope 정상
- [ ] Step Assist 정상
- [ ] Ledge Detect 정상

---

## 테스트 체크리스트

- [ ] 기존 EditMode 테스트 전체 Green
- [ ] PlayerLedgeClimberTests 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

GitHub 저장소에는 자동 CI 결과가 등록되어 있지 않으므로 위 항목은 Unity 로컬 Test Runner에서 직접 확인한다.

---

## 개발 결과

21일차에서는 20일차의 Ledge Detect 결과를 실제 플레이어 이동으로 연결했다.

유효한 Ledge에서 Space를 누르면 일반 Jump보다 Climb이 우선 실행되며, Rigidbody와 Collider를 Climb 전용 상태로 전환한 뒤 Lift와 Forward 두 단계로 이동한다.

Climb이 끝나면 기존 물리 상태와 이동 시스템을 복구해 Walk, Sprint, Crouch, Jump 등 기존 기능을 다시 사용할 수 있도록 구성했다.

이로써 기본 이동 계열은 다음 단계까지 연결되었다.

```text
Walk
→ Sprint
→ Crouch
→ Slope / Step
→ Ledge Detect
→ Ledge Climb
```

다음 22일차에서는 실제 3인칭 플레이에 필요한 **카메라 회전 시스템**을 구현한다.
