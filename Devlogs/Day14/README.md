# 14일차 개발일지 - 코요테 타임 및 점프 버퍼 구현

## 오늘의 목표

13일차에 구현한 점프·중력·Ground Check 구조를 유지하면서
플레이어가 점프 입력을 조금 늦거나 조금 빠르게 입력해도
자연스럽게 점프할 수 있도록 조작 보정 기능을 추가한다.

이번 일차에서는 다음 두 기능을 구현한다.

```text
Coyote Time
Jump Buffer
```

기존 카메라 기준 이동, 지상 가속·감속, 점프, 중력,
Ground Check는 그대로 유지한다.

---

# 구현 내용

## 1. Coyote Time 추가

플레이어가 발판 끝을 벗어난 직후
아주 짧은 시간 동안 점프를 허용하도록 구현했다.

기본값:

```text
Coyote Time = 0.12초
```

동작:

```text
지상
↓
Coyote Timer = 0.12

발판에서 떨어짐
↓
0.12
→ 0.10
→ 0.08
→ ...
→ 0
```

타이머가 남아 있는 동안에는
이미 Ground Check가 false가 되었더라도 점프할 수 있다.

---

## 2. Jump Buffer 추가

착지 직전에 Jump 입력을 했을 경우
입력을 즉시 버리지 않고 잠시 저장하도록 구현했다.

기본값:

```text
Jump Buffer Time = 0.12초
```

동작:

```text
공중
↓
Space
↓
Jump Buffer Timer = 0.12
↓
타이머 감소
↓
착지 전에 시간이 남아 있음
↓
즉시 점프
```

너무 일찍 입력해 타이머가 0이 되면
착지 후 점프는 발생하지 않는다.

---

# 기존 Jump 요청 방식 변경

13일차에서는:

```text
jumpRequested = true
```

형태의 단순 Bool 값을 사용했다.

14일차에서는 이를 제거하고 다음 두 Timer를 사용한다.

```text
coyoteTimer
jumpBufferTimer
```

Jump 입력이 들어오면:

```text
jumpBufferTimer = jumpBufferTime
```

으로 입력 시간을 저장한다.

---

# Coyote Timer 갱신

플레이어가 실제 점프 가능한 Ground 상태일 때:

```text
IsGrounded = true
AND
Current Y Velocity <= 0.1
```

이면:

```text
Coyote Timer = Coyote Time
```

으로 계속 갱신한다.

공중에서는 FixedUpdate마다 감소한다.

---

# Jump Buffer Timer 갱신

Space 입력이 들어오면:

```text
Jump Buffer Timer = 0.12
```

로 설정한다.

이후 FixedUpdate마다 0을 향해 감소한다.

```text
0.12
→ 0.10
→ 0.08
→ ...
→ 0
```

---

# 점프 실행 조건

점프 실행 조건을 다음처럼 변경했다.

```text
Coyote Timer > 0
AND
Jump Buffer Timer > 0
```

두 조건을 모두 만족하면:

```text
shouldJump = true
```

가 된다.

따라서 다음 두 상황을 같은 구조로 처리할 수 있다.

```text
일반 지상 점프

발판 이탈 직후 Coyote Jump

착지 직전 입력한 Buffered Jump
```

---

# 점프 성공 후 Timer 소비

점프가 실제 실행되면:

```text
Coyote Timer = 0
Jump Buffer Timer = 0
```

으로 즉시 초기화한다.

이를 통해 하나의 입력으로
추가 점프가 다시 발생하지 않도록 했다.

공중 연속 점프는 여전히 허용하지 않는다.

---

# 수직 속도 처리 유지

기존:

```text
CalculateVerticalVelocity
```

구조는 유지하면서
`jumpRequested` 대신 최종 판정 결과인:

```text
shouldJump
```

를 전달한다.

`shouldJump = true`인 경우:

```text
Y Velocity = Jump Velocity
```

를 적용한다.

기본값:

```text
Jump Velocity = 8
Gravity = -22
```

은 유지한다.

---

# 기존 Ground Check 유지

13일차에 구현한 Ground Check는 변경하지 않았다.

```text
Player Collider Bounds
↓
발 위치 계산
↓
Physics.CheckSphere
```

검사 Layer:

```text
World
Obstacle
```

기본값:

```text
Ground Check Radius = 0.22
Ground Check Offset = 0.08
```

---

# 테스트용 Coyote Platform 추가

기존 이동 테스트 Scene을 확장했다.

```text
Assets/ProjectJ/Tests/Manual/Day11/
└─ Day11_MovementTest.unity
```

다음 테스트용 발판을 추가했다.

```text
CoyoteTestPlatform
```

설정:

```text
Position
X = 0
Y = 4
Z = 0

Scale
X = 6
Y = 0.5
Z = 6

Layer
World
```

Player 시작 위치도 테스트 발판 위로 이동했다.

```text
Player Y = 5.3
```

이를 통해 발판 끝에서 떨어지는 상황과
착지 직전 Jump 입력을 반복해서 테스트할 수 있다.

---

# Coyote Time 테스트

테스트 발판 끝으로 이동한 뒤 그대로 떨어진다.

발판에서 벗어난 직후 Space를 입력한다.

정상:

```text
발판 이탈
↓
0.12초 이내 Space
↓
점프 성공
```

조금 늦게 입력한다.

정상:

```text
Coyote Timer 만료
↓
Space
↓
점프 실패
```

---

# Jump Buffer 테스트

높은 위치에서 아래로 떨어진다.

착지 직전에 Space를 누른다.

정상:

```text
공중 Space
↓
Jump Buffer 저장
↓
착지
↓
즉시 점프
```

착지보다 너무 일찍 입력하면:

```text
Jump Buffer 만료
↓
착지
↓
점프 없음
```

이어야 한다.

---

# 자동 테스트 확장

다음 테스트 파일을 수정했다.

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlayerCameraRelativeMovementTests.cs
```

기존 이동, 가속·감속, 점프·중력 테스트를 유지하면서
Coyote Time과 Jump Buffer 테스트를 추가했다.

---

## CoyoteTimer_RefreshesWhileGrounded

지상에서는 Coyote Timer가
설정값인 0.12초로 갱신되는지 확인한다.

---

## CoyoteTimer_CountsDownAfterLeavingGround

지면을 떠난 뒤 Coyote Timer가
시간에 따라 감소하는지 확인한다.

---

## CoyoteTimer_StopsAtZero

Timer가 0보다 작은 값으로 내려가지 않는지 확인한다.

---

## JumpBufferTimer_CountsDown

Jump Buffer가 시간에 따라 감소하는지 확인한다.

---

## JumpBufferTimer_StopsAtZero

Jump Buffer가 0 이하로 내려가지 않는지 확인한다.

---

## BufferedJump_RequiresCoyoteAndBufferTime

다음 두 Timer가 모두 남아 있을 때만
점프가 허용되는지 확인한다.

```text
Coyote Timer > 0
Jump Buffer Timer > 0
```

---

## BufferedJump_FailsAfterCoyoteExpires

Coyote Time이 만료된 이후에는
저장된 Jump 입력이 있어도 점프할 수 없는지 확인한다.

---

## BufferedJump_FailsAfterBufferExpires

Jump Buffer가 만료되었다면
Ground 조건이 존재해도 오래된 입력으로 점프하지 않는지 확인한다.

---

## CoyoteJump_CanApplyJumpVelocityOffGround

이미 Ground를 벗어난 상황에서도
유효한 Coyote Time이 남아 있다면
Jump Velocity를 적용할 수 있는지 확인한다.

---

## BufferedLandingJump_CanApplyJumpVelocity

낙하 중 저장된 Jump 입력이 있고
착지 조건을 만족하면
즉시 Jump Velocity를 적용하는지 확인한다.

---

# 수정된 파일

이번 일차에서 수정된 파일:

```text
Assets/ProjectJ/Runtime/Player/
└─ PlayerCameraRelativeMovement.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerCameraRelativeMovementTests.cs

Assets/ProjectJ/Tests/Manual/Day11/
└─ Day11_MovementTest.unity
```

---

# 생성된 파일

```text
없음
```

테스트 발판은 기존 Manual Test Scene 내부에 추가했다.

---

# 삭제된 파일

```text
없음
```

---

# 기존 기능 회귀 확인

다음 기존 기능도 그대로 동작해야 한다.

```text
카메라 기준 WASD 이동

지상 가속·감속

대각선 속도 제한

점프

중력

Ground Check

공중 연속 점프 방지

Player ↔ World 충돌

Player ↔ Obstacle 충돌
```

---

# 완료 확인 항목

## Coyote Time

- [ ] 지상에서 Coyote Timer 갱신
- [ ] 발판 이탈 후 Timer 감소
- [ ] 약 0.12초 이내 점프 가능
- [ ] Timer 만료 이후 점프 불가
- [ ] Timer가 0보다 작아지지 않음

## Jump Buffer

- [ ] 공중 Jump 입력 저장
- [ ] 약 0.12초 동안 입력 유지
- [ ] 착지 직전 입력 시 즉시 재점프
- [ ] 너무 일찍 입력한 Jump는 만료
- [ ] Buffer가 0보다 작아지지 않음

## 점프 안정성

- [ ] 일반 지상 점프 정상
- [ ] Coyote Jump 정상
- [ ] Buffered Landing Jump 정상
- [ ] 점프 성공 후 두 Timer 초기화
- [ ] 공중 연속 점프 불가
- [ ] Jump Velocity 8 유지
- [ ] Gravity -22 유지

## 기존 이동

- [ ] 카메라 기준 이동 정상
- [ ] 가속 정상
- [ ] 감속 정상
- [ ] 대각선 속도 증가 없음
- [ ] 이동 중 점프 정상

## 테스트

- [ ] 기존 EditMode 테스트 Green
- [ ] Coyote Timer 테스트 Green
- [ ] Jump Buffer 테스트 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

# 결과

기존 점프 시스템에 Coyote Time과 Jump Buffer를 추가해
플레이어가 점프 버튼을 정확한 한 프레임에 입력하지 않아도
의도한 점프가 자연스럽게 실행되도록 개선했다.

발판에서 아주 조금 늦게 Jump를 눌러도 점프할 수 있으며,
착지 직전에 미리 Jump를 눌러도 입력이 잠시 저장되어
착지 순간 바로 점프할 수 있다.

두 기능은 각각 0.12초의 짧은 허용 시간을 사용하며,
점프가 실제로 실행된 순간 두 Timer를 즉시 소비해
공중 연속 점프가 발생하지 않도록 했다.

기존 Manual Test Scene에는 CoyoteTestPlatform을 추가해
앞으로도 점프와 공중 이동 관련 기능을 반복 검증할 수 있도록 했다.

다음 15일차에서는 현재 수평 이동과 점프 시스템을 기반으로
공중에서의 방향 조작을 별도로 제어하는 Air Control을 구현한다.
