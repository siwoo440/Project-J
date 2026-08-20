# 13일차 개발일지 - 점프·중력 및 Ground Check 구현

## 오늘의 목표

12일차까지 구현한 카메라 기준 수평 이동과 지상 가속·감속을 유지하면서
플레이어의 Y축 이동을 처음 추가한다.

지면에 있을 때 Jump 입력을 받아 위로 도약하고,
직접 제어하는 중력에 의해 다시 낙하하도록 구성한다.

또한 공중 연속 점프를 막기 위해
Player Collider의 발 위치를 기준으로 Ground Check를 구현한다.

이번 일차에서는 코요테 타임, 점프 버퍼, 공중 제어는 구현하지 않는다.

---

## 구현 내용

### 1. Jump 입력 연결

기존 Input System의 다음 Action을 사용한다.

```text
Player / Jump
```

입력:

```text
Keyboard : Space
Gamepad  : Button South
```

`PlayerCameraRelativeMovement`에서 Jump Action을 직접 찾아
`performed` 이벤트가 발생하면 점프 요청을 기록하도록 구현했다.

점프 요청은 한 번의 물리 업데이트에서 소비한 뒤 초기화한다.

---

## 2. Ground Check 구현

별도의 GroundCheck 자식 오브젝트를 만들지 않고
현재 Player Collider의 Bounds를 기준으로 발 위치를 자동 계산한다.

구조:

```text
Player Collider Bounds
↓
bounds.min.y
↓
Ground Check Offset 적용
↓
Physics.CheckSphere
```

검사 대상 Layer:

```text
World
Obstacle
```

Trigger는 Ground 판정에서 제외한다.

Ground Check 기본값:

```text
Ground Check Radius = 0.22
Ground Check Offset = 0.08
```

Player를 Scene에서 선택하면
Ground Check 범위를 Wire Sphere Gizmo로 확인할 수 있다.

---

## 3. IsGrounded 상태 추가

현재 플레이어가 지면에 있는지를 외부에서도 확인할 수 있도록
다음 읽기 전용 상태를 추가했다.

```text
IsGrounded
```

동작:

```text
Ground Check가 World 또는 Obstacle 감지
→ IsGrounded = true

공중
→ IsGrounded = false
```

이 값은 이후 코요테 타임, 공중 제어, 착지 처리 등의 기반으로 사용할 수 있다.

---

## 4. Unity 기본 Gravity 비활성화

이번부터 Rigidbody의 기본 중력을 사용하지 않고
플레이어 스크립트에서 Gravity 값을 직접 관리한다.

Player Prefab:

```text
Rigidbody
Use Gravity = OFF
```

스크립트 실행 시에도:

```text
body.useGravity = false
```

로 강제해 중력이 중복 적용되지 않도록 했다.

---

## 5. 점프 설정값 추가

PlayerCameraRelativeMovement에 다음 값을 추가했다.

| 설정 | 기본값 | 역할 |
| --- | ---: | --- |
| Jump Velocity | 8 | 점프 순간 적용할 상승 속도 |
| Gravity | -22 | 매 FixedUpdate마다 Y 속도에 적용할 중력 |
| Ground Check Radius | 0.22 | 지면 감지 Sphere 반경 |
| Ground Check Offset | 0.08 | Collider 발 위치 기준 Ground Check 높이 보정 |
| Ground Layers | World, Obstacle | 지면으로 판정할 Layer |

---

# 점프 처리

지면에 있는 상태에서 Jump 입력이 들어오면:

```text
current Y velocity
↓
Jump Velocity = 8 적용
```

한다.

즉:

```text
지상
↓ Space
Y Velocity = 8
↓
상승
```

으로 시작한다.

---

# 공중 연속 점프 방지

점프는 지면에 있을 때만 가능하다.

정상 동작:

```text
지상
→ Space
→ 점프 가능

공중
→ Space
→ 추가 점프 불가

착지
→ Space
→ 다시 점프 가능
```

이번 일차에서는 공중에서 누른 Jump 입력을 저장하지 않는다.

따라서 아직 점프 버퍼는 존재하지 않는다.

---

# 중력 처리

수직 속도 계산을 별도 함수로 분리했다.

```text
CalculateVerticalVelocity
```

공중에서는:

```text
현재 Y 속도
+
Gravity × Fixed Delta Time
```

형태로 속도를 계속 감소시킨다.

예:

```text
8
→ 7.56
→ 7.12
→ ...
→ 0
→ 음수
→ 낙하
```

기본값:

```text
Gravity = -22
```

---

# 착지 처리

Ground 상태이고 현재 Y 속도가 아래 방향일 때는
수직 속도를 0으로 정리한다.

예:

```text
낙하 중
Y = -5
↓
Ground 감지
↓
Y = 0
```

이를 통해 바닥에 착지한 뒤
계속 아래 방향 속도가 누적되지 않도록 했다.

---

# 점프 직후 Ground Overlap 대응

점프 직후 한두 프레임 동안
Ground Check Sphere가 아직 바닥과 겹칠 수 있다.

이때 상승 속도를 바로 0으로 만들면
점프가 취소될 수 있기 때문에
다음 조건을 사용한다.

```text
IsGrounded = true
AND
Current Y Velocity <= 0.1
```

일 때만 Ground 상태를 실제 착지 상태로 사용한다.

따라서:

```text
Y = 7
Ground Check가 잠깐 true
```

여도 상승 속도를 강제로 0으로 만들지 않는다.

---

# 수평 이동과 수직 이동 분리

12일차 구조를 유지한다.

```text
X / Z
→ Camera Relative Movement
→ Acceleration / Deceleration

Y
→ Jump / Gravity
```

최종 Rigidbody Velocity:

```text
new X
new Y
new Z
```

로 구성한다.

따라서 점프 중에도 기존 수평 이동 속도가 사라지지 않는다.

---

# 이동 중 점프

다음 입력을 사용할 수 있다.

```text
W + Space
W + D + Space
A + Space
```

점프하는 순간에도 X/Z 이동 속도는 유지된다.

이 구조는 이후 15일차 공중 제어를 추가할 수 있는 기반이 된다.

---

# Player Prefab 수정

다음 프리팹을 수정했다.

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

Rigidbody:

```text
Use Gravity = OFF
```

PlayerCameraRelativeMovement:

```text
Move Speed = 6
Acceleration = 30
Deceleration = 40

Jump Velocity = 8
Gravity = -22

Ground Check Radius = 0.22
Ground Check Offset = 0.08

Ground Layers
- World
- Obstacle
```

---

# 자동 테스트 확장

다음 테스트 파일을 수정했다.

```text
Assets/ProjectJ/Tests/EditMode/PlayerCameraRelativeMovementTests.cs
```

기존 카메라 기준 이동과 가속·감속 테스트는 그대로 유지하고
Jump / Gravity 관련 테스트를 추가했다.

---

## GroundedJump_ReturnsJumpVelocity

지상 상태에서 Jump 입력을 넣으면
Jump Velocity가 반환되는지 확인한다.

```text
Expected = 8
```

---

## GroundedWithoutJump_StopsDownwardVelocity

낙하 중 지면을 감지했을 때
Y 속도가 0으로 정리되는지 확인한다.

---

## AirborneJump_DoesNotApplySecondJump

공중에서 Jump 입력을 넣어도
두 번째 Jump Velocity가 적용되지 않는지 확인한다.

---

## Gravity_ReducesUpwardVelocity

상승 중 Gravity가 적용되어
Y 속도가 점차 감소하는지 확인한다.

---

## Gravity_IncreasesDownwardSpeed

낙하 중에는 Gravity 때문에
음수 Y 속도의 절댓값이 더 커지는지 확인한다.

---

## UpwardVelocity_IsNotCanceledByGroundOverlap

점프 직후 Ground Check가 잠깐 true여도
현재 Y 속도가 상승 중이라면
점프 속도가 0으로 취소되지 않는지 확인한다.

---

# 수정된 파일

이번 일차에서 수정된 파일:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab

Assets/ProjectJ/Runtime/Player/
└─ PlayerCameraRelativeMovement.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerCameraRelativeMovementTests.cs
```

---

# 생성된 파일

```text
없음
```

기존 Player Prefab과 기존 테스트 맵을 그대로 사용했다.

---

# 삭제된 파일

```text
없음
```

---

# 테스트 환경

다음 Scene을 계속 사용한다.

```text
Assets/ProjectJ/Tests/Manual/Day11/
└─ Day11_MovementTest.unity
```

---

# 수동 테스트

## 기본 점프

```text
정지
↓ Space
상승
↓
낙하
↓
착지
```

확인:

- [ ] Space로 점프 가능
- [ ] 자연스럽게 낙하
- [ ] 바닥에 착지
- [ ] 착지 후 다시 점프 가능

---

## 공중 재점프

점프 중 Space를 여러 번 입력한다.

확인:

- [ ] 첫 점프만 적용
- [ ] 공중 추가 점프 없음

---

## 이동 중 점프

다음 입력을 확인한다.

```text
W + Space
W + D + Space
A + Space
```

확인:

- [ ] 점프 중 X/Z 속도 유지
- [ ] 카메라 기준 이동 유지
- [ ] 대각선 속도 제한 유지

---

## 기존 이동 회귀

CameraRig Y Rotation:

```text
0°
90°
180°
270°
```

에서 확인:

```text
W = 화면 기준 앞으로
```

기존 기능이 깨지지 않아야 한다.

---

# 완료 확인 항목

## Jump / Gravity

- [ ] Space 점프 정상
- [ ] Jump Velocity 8 적용
- [ ] Gravity -22 적용
- [ ] 상승 후 자연스럽게 낙하
- [ ] 착지 정상
- [ ] 착지 후 재점프 가능
- [ ] 공중 연속 점프 불가

## Ground Check

- [ ] World 감지
- [ ] Obstacle 감지
- [ ] Trigger 무시
- [ ] Ground Check Gizmo 확인 가능
- [ ] 점프 직후 Ground Overlap이 점프를 취소하지 않음

## 기존 기능 회귀

- [ ] 카메라 기준 이동 정상
- [ ] 지상 가속 정상
- [ ] 지상 감속 정상
- [ ] 대각선 속도 증가 없음
- [ ] Player 회전 정상
- [ ] Player ↔ World 충돌 정상
- [ ] Player ↔ Obstacle 충돌 정상

## 자동 테스트

- [ ] 기존 이동 테스트 Green
- [ ] 기존 가속·감속 테스트 Green
- [ ] Grounded Jump 테스트 Green
- [ ] Airborne Jump 테스트 Green
- [ ] Gravity 테스트 Green
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green

## 프로젝트 상태

- [ ] Console Error 0
- [ ] 기존 Scene 흐름 정상

---

# 결과

카메라 기준 이동과 지상 가속·감속 위에
점프와 중력 시스템을 추가했다.

Player Collider의 발 위치를 기반으로 Ground Check를 수행하며,
지상에서만 Jump 입력을 사용할 수 있도록 구성했다.

Rigidbody 기본 중력을 끄고
Gravity 값을 직접 제어하도록 변경해
향후 상승·하강 중력 조정과 점프 조작감 개선이 가능한 기반을 만들었다.

또한 수평 이동과 수직 이동을 분리해
점프 중에도 기존 X/Z 이동이 유지되도록 했다.

다음 14일차에서는
현재 Jump / Ground Check 구조를 기반으로
코요테 타임과 점프 버퍼를 추가한다.
