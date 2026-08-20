# 15일차 개발일지 - 공중 조작(Air Control) 구현

## 오늘의 목표

14일차까지 구현한 카메라 기준 이동, 지상 가속·감속,
점프, 중력, Ground Check, 코요테 타임, 점프 버퍼를 유지하면서
지상과 공중의 수평 이동 제어감을 분리한다.

기존에는 지상과 공중에서 동일한 가속·감속 값을 사용했기 때문에
점프 중에도 지상처럼 빠르게 방향을 바꾸거나 멈출 수 있었다.

이번 일차에서는 공중 전용 가속·감속 값을 추가해
점프 중에도 방향 조작은 가능하지만
지상보다 느리고 관성이 남도록 수정한다.

---

# 구현 내용

## 1. 공중 전용 이동 설정값 추가

`PlayerCameraRelativeMovement`에 다음 값을 추가했다.

| 설정 | 기본값 | 역할 |
| --- | ---: | --- |
| Air Acceleration | 12 | 공중에서 입력 방향으로 속도를 변경하는 속도 |
| Air Deceleration | 6 | 공중에서 입력이 없을 때 수평 속도가 감소하는 속도 |

기존 지상 값:

```text
Acceleration = 30
Deceleration = 40
```

공중 값:

```text
Air Acceleration = 12
Air Deceleration = 6
```

공중에서는 지상보다 가속이 느리고,
입력을 놓았을 때도 수평 속도가 더 오래 유지되도록 설정했다.

---

# 2. 지상·공중 이동 상태 분리

새로운 수평 이동 상태 판정 함수를 추가했다.

```text
IsAirborneForHorizontalControl
```

공중으로 판정하는 조건:

```text
Jump 실행 프레임

또는

IsGrounded = false

또는

Current Y Velocity > 0.1
```

즉 다음과 같은 경우 공중 이동 값을 사용한다.

```text
점프를 시작한 순간

실제로 바닥에서 떨어진 상태

Ground Check가 잠깐 바닥과 겹치더라도
이미 위로 상승 중인 상태
```

---

# 3. 점프 시작 프레임 Air Control 적용

점프 버튼을 누른 순간에는 Ground Check가 아직 true일 수 있다.

이 상태에서 단순히:

```text
IsGrounded
```

만으로 이동 상태를 구분하면
점프 직후 한 프레임 동안 지상 가속값이 적용될 수 있다.

이를 방지하기 위해:

```text
shouldJump = true
```

이면 즉시 공중 이동 상태로 판정한다.

따라서 점프가 시작되는 순간부터:

```text
Air Acceleration
Air Deceleration
```

을 사용한다.

---

# 4. 상승 중 Ground Overlap 대응

13일차부터 Ground Check Sphere는
Player Collider 발 위치를 기준으로 검사한다.

점프 직후 Sphere가 잠깐 바닥과 겹칠 수 있기 때문에
다음 조건도 공중으로 처리한다.

```text
Current Y Velocity > 0.1
```

따라서:

```text
IsGrounded = true
Y Velocity = 7
```

같은 순간에도 실제 이동 제어는 Air Control을 사용한다.

---

# 5. 이동 변화율 선택 함수 추가

다음 함수를 추가했다.

```text
SelectHorizontalChangeRates
```

지상:

```text
Acceleration = 30
Deceleration = 40
```

공중:

```text
Air Acceleration = 12
Air Deceleration = 6
```

중 현재 상태에 맞는 값을 선택한다.

이후 기존:

```text
CalculateHorizontalVelocity
```

함수에 선택된 값을 전달한다.

---

# 6. 기존 수평 이동 계산 재사용

12일차에서 만든:

```text
CalculateHorizontalVelocity
```

는 그대로 유지한다.

따라서 지상과 공중이 서로 다른 이동 코드를 사용하는 것이 아니라:

```text
같은 수평 속도 계산 함수
+
상태별 Acceleration / Deceleration
```

구조로 동작한다.

이를 통해 코드 중복을 늘리지 않고
이동 감각만 분리했다.

---

# 7. 공중 방향 전환

점프 중에도 WASD 입력은 계속 사용한다.

예:

```text
W로 이동
↓
Space
↓
공중에서 A
```

동작:

```text
기존 전진 속도 유지
↓
Air Acceleration = 12
↓
점진적으로 왼쪽 방향으로 전환
```

지상처럼 즉시 강하게 꺾이지 않는다.

---

# 8. 공중 반대 방향 입력

예:

```text
W 이동
↓
점프
↓
공중에서 S
```

기존에는 지상과 같은 가속값이 적용되어
방향 전환이 지나치게 빨랐다.

이제:

```text
앞 방향 속도
↓
공중 가속값으로 점진적 감소
↓
0
↓
뒤 방향으로 점진적 가속
```

형태가 된다.

---

# 9. 공중 관성 유지

공중에서 이동 입력을 놓으면:

```text
Air Deceleration = 6
```

을 사용한다.

지상:

```text
Deceleration = 40
```

과 비교하면 매우 낮은 값이므로
점프 중 입력을 놓아도 수평 속도가 급격히 사라지지 않는다.

예:

```text
W + Space
↓
공중에서 W 해제
↓
기존 전진 관성 유지
↓
천천히 수평 속도 감소
```

---

# 10. 최대 수평 속도 유지

지상과 공중 모두 기존:

```text
Move Speed = 6
```

을 사용한다.

따라서 Air Control을 계속 사용해도:

```text
Horizontal Speed <= 6
```

이 유지된다.

대각선 이동도 기존 정규화 구조가 유지되므로
W+D 입력으로 속도가 증가하지 않는다.

---

# 기존 시스템 유지

이번 일차에서는 다음 시스템을 변경하지 않았다.

```text
Camera Relative Movement

Ground Acceleration / Deceleration

Jump

Gravity

Ground Check

Coyote Time

Jump Buffer
```

Air Control은 기존 수평 이동 시스템 위에
상태별 변화율만 추가한 구조다.

---

# 자동 테스트 확장

다음 테스트 파일을 수정했다.

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlayerCameraRelativeMovementTests.cs
```

기존 이동, 점프, 중력, Coyote Time,
Jump Buffer 테스트는 유지하면서
Air Control 관련 테스트를 추가했다.

---

## AirControl_JumpFrameIsAirborne

점프 실행 프레임은
Ground Check가 true여도 공중 이동 상태인지 확인한다.

---

## AirControl_NotGroundedIsAirborne

Ground Check가 false이면
수직 속도와 관계없이 공중 상태인지 확인한다.

---

## AirControl_RisingGroundOverlapIsAirborne

Ground Check가 true더라도
플레이어가 위로 상승 중이면 공중 상태인지 확인한다.

---

## AirControl_StandingGroundStateIsNotAirborne

정상적으로 바닥에 서 있는 상태:

```text
IsGrounded = true
Y Velocity = 0
Jump 없음
```

에서는 지상 이동 상태인지 확인한다.

---

## AirControl_SelectsAirChangeRates

Air 상태에서는:

```text
Acceleration = 12
Deceleration = 6
```

을 선택하는지 확인한다.

---

## AirControl_SelectsGroundChangeRates

Ground 상태에서는:

```text
Acceleration = 30
Deceleration = 40
```

을 선택하는지 확인한다.

---

## AirAcceleration_IsSlowerThanGroundAcceleration

같은 입력과 같은 시간에서
공중 가속 결과가 지상 가속 결과보다 작은지 확인한다.

---

## AirDeceleration_PreservesMoreMomentumThanGround

같은 속도에서 입력을 놓았을 때
공중 속도가 지상보다 더 많이 남아 있는지 확인한다.

즉 공중에서 관성이 더 오래 유지되는지 검증한다.

---

## AirControl_DoesNotExceedMoveSpeed

Air Acceleration을 계속 적용해도
최대 수평 속도 6을 초과하지 않는지 확인한다.

---

# 수정된 파일

이번 일차에서 수정된 파일:

```text
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

---

# 삭제된 파일

```text
없음
```

---

# 수동 테스트 환경

기존 테스트 Scene을 계속 사용한다.

```text
Assets/ProjectJ/Tests/Manual/Day11/
└─ Day11_MovementTest.unity
```

14일차에서 추가한:

```text
CoyoteTestPlatform
```

을 Air Control 테스트에도 사용한다.

---

# 수동 테스트

## 지상 이동

지상에서 WASD를 사용한다.

확인:

```text
Acceleration = 30
Deceleration = 40
```

기존 이동감이 유지되어야 한다.

---

## 공중 좌우 이동

```text
Space
↓
공중에서 A / D
```

확인:

- [ ] 공중에서도 방향 변경 가능
- [ ] 지상보다 느리게 방향 변경

---

## 공중 반대 방향

```text
W 이동
↓
Space
↓
공중에서 S
```

확인:

- [ ] 즉시 반대 방향으로 전환되지 않음
- [ ] 기존 전진 속도가 점진적으로 줄어듦
- [ ] 이후 뒤 방향으로 점진적 가속

---

## 공중 입력 해제

```text
W + Space
↓
공중에서 W 해제
```

확인:

- [ ] 수평 속도가 즉시 사라지지 않음
- [ ] 지상보다 관성이 오래 유지됨

---

## 기존 점프 회귀

확인:

```text
일반 Jump

Coyote Jump

Buffered Landing Jump

Gravity

Ground Check

공중 연속 점프 방지
```

모두 정상 동작해야 한다.

---

# 완료 확인 항목

## Air Control

- [ ] 점프 시작 프레임 Air 상태
- [ ] 비접지 상태 Air 상태
- [ ] 상승 중 Ground Overlap도 Air 상태
- [ ] 정상 착지 상태 Ground 상태
- [ ] Air Acceleration 12 적용
- [ ] Air Deceleration 6 적용
- [ ] 공중 방향 변경 가능
- [ ] 공중 방향 전환이 지상보다 느림
- [ ] 공중 관성 유지
- [ ] 공중 최대 수평 속도 6 이하

## 기존 지상 이동

- [ ] Acceleration 30 유지
- [ ] Deceleration 40 유지
- [ ] 카메라 기준 이동 정상
- [ ] 대각선 속도 증가 없음

## Jump 시스템

- [ ] Jump 정상
- [ ] Gravity 정상
- [ ] Ground Check 정상
- [ ] Coyote Time 정상
- [ ] Jump Buffer 정상
- [ ] 공중 연속 점프 불가

## 테스트

- [ ] 기존 EditMode 테스트 Green
- [ ] Air Control 테스트 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

# 결과

지상과 공중의 수평 이동 제어를 분리했다.

지상에서는 기존의 높은 가속·감속 값을 사용해
즉각적인 조작감을 유지하고,
공중에서는 낮은 가속·감속 값을 사용해
점프 중 방향 전환이 제한되고 관성이 남도록 개선했다.

또한 점프 시작 순간과 상승 중 Ground Check가 잠깐 겹치는 상황까지
공중 상태로 처리해 Air Control 적용 시점을 안정적으로 만들었다.

기존 수평 속도 계산 함수를 그대로 재사용해
지상과 공중 이동 로직을 중복하지 않고
상태별 이동 변화율만 선택하는 구조로 확장했다.

다음 16일차에서는
현재 기본 이동 시스템에 Sprint와 Stamina를 추가한다.
