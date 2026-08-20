# 16일차 개발일지 - Sprint 및 Stamina 구현

## 오늘의 목표

15일차까지 구현한 카메라 기준 이동, 지상·공중 이동 제어,
점프, 중력, Ground Check, 코요테 타임, 점프 버퍼를 유지하면서
지상 이동에 Sprint와 Stamina 시스템을 추가한다.

이번 일차에서는 Left Shift를 누른 상태에서 이동하면
기본 이동보다 빠르게 달릴 수 있도록 만들고,
Sprint 중에는 Stamina가 감소하며
Sprint를 사용하지 않을 때는 다시 회복하도록 구현한다.

또한 Stamina가 완전히 소진되었을 때
Shift를 계속 누르고 있어도 Sprint가 반복 재시작되지 않도록
소진 잠금 처리도 추가한다.

---

# 구현 내용

## 1. Sprint 입력 연결

기존 Input System의 다음 Action을 사용한다.

```text
Player / Sprint
```

기본 입력:

```text
Keyboard : Left Shift
```

`PlayerCameraRelativeMovement`에서
Sprint Action의 `performed`와 `canceled` 이벤트를 구독한다.

동작:

```text
Shift 누름
→ sprintHeld = true

Shift 해제
→ sprintHeld = false
```

---

# 2. Sprint Speed 추가

기본 이동 속도는 그대로 유지한다.

```text
Move Speed = 6
```

Sprint 중 사용할 별도 속도를 추가했다.

```text
Sprint Speed = 9
```

동작:

```text
일반 이동
→ 목표 속도 6

Sprint
→ 목표 속도 9
```

기존 수평 이동 계산 함수인:

```text
CalculateHorizontalVelocity
```

를 그대로 사용하고,
상태에 따라 목표 이동 속도만 선택한다.

---

# 3. Sprint 가능 조건

새로운 Sprint 판정 함수:

```text
CanSprint
```

를 추가했다.

Sprint가 가능하려면 다음 조건을 모두 만족해야 한다.

```text
Shift 입력 중

+

이동 입력 존재

+

지상 상태

+

Current Stamina > 0

+

Sprint Exhausted 상태가 아님
```

따라서 다음 상황에서는 Sprint가 시작되지 않는다.

```text
Shift만 누르고 정지

공중 상태

Stamina = 0

Stamina 완전 소진 후 Shift를 계속 누르고 있는 상태
```

---

# 4. Stamina 기본 설정

다음 설정값을 추가했다.

| 설정 | 기본값 | 역할 |
| --- | ---: | --- |
| Max Stamina | 100 | 최대 스태미나 |
| Stamina Drain Rate | 25 | Sprint 중 초당 소모량 |
| Stamina Recovery Rate | 20 | Sprint 미사용 시 초당 회복량 |

게임 시작 시:

```text
Current Stamina = Max Stamina
```

로 초기화된다.

즉 시작 스태미나는:

```text
100
```

이다.

---

# 5. Sprint 중 Stamina 감소

실제로 Sprint가 허용된 상태에서는:

```text
Current Stamina
-
Drain Rate × Delta Time
```

형태로 감소한다.

기본 설정:

```text
100 Stamina
25 / sec Drain
```

이므로 약 4초 동안 지속 Sprint가 가능하다.

예:

```text
100
→ 75
→ 50
→ 25
→ 0
```

---

# 6. Sprint 미사용 시 Stamina 회복

Sprint 중이 아니라면:

```text
Current Stamina
+
Recovery Rate × Delta Time
```

으로 회복한다.

기본값:

```text
Recovery Rate = 20 / sec
```

예:

```text
0
→ 20
→ 40
→ 60
→ 80
→ 100
```

이번 일차에서는 별도의 회복 지연 시간은 적용하지 않는다.

Sprint가 종료되면 즉시 회복을 시작한다.

---

# 7. Stamina 범위 제한

새로운:

```text
CalculateStamina
```

함수에서 Stamina를 항상 다음 범위로 제한한다.

```text
0 <= Current Stamina <= Max Stamina
```

따라서 Sprint를 오래 사용해도:

```text
-1
-10
```

같은 음수 값으로 내려가지 않는다.

회복 중에도:

```text
101
120
```

같이 최대값을 초과하지 않는다.

---

# 8. Stamina 완전 소진 처리

Sprint 중 Stamina가 0에 도달하면:

```text
sprintExhausted = true
```

로 설정한다.

그 결과:

```text
Stamina = 0
↓
Sprint 종료
↓
기본 이동 속도 6으로 복귀
```

한다.

---

# 9. Sprint 자동 재시작 방지

Stamina가 0이 된 뒤에도
Shift를 계속 누르고 있으면 Stamina는 회복한다.

하지만 회복된 즉시 Sprint가 자동으로 켜지면:

```text
0
→ 조금 회복
→ Sprint
→ 다시 0
→ 회복
→ Sprint
```

가 반복될 수 있다.

이를 방지하기 위해
완전 소진 후에는 Shift를 한 번 해제해야 한다.

동작:

```text
Stamina 완전 소진
↓
Sprint Exhausted = true
↓
Stamina 회복 가능
↓
Shift 계속 누름
→ Sprint 재시작 불가

Shift 해제
↓
Sprint Exhausted = false
↓
Shift 다시 입력
→ Sprint 재사용 가능
```

---

# 10. Sprint 상태 공개

향후 UI나 애니메이션 시스템에서 사용할 수 있도록
다음 읽기 전용 상태를 제공한다.

```text
IsSprinting
CurrentStamina
MaxStamina
```

이번 일차에서는 UI를 구현하지 않지만
이후 Stamina UI에서 직접 사용할 수 있는 기반을 만들었다.

---

# 11. 공중 Sprint 제한

이번 Sprint는 지상 이동 기능으로 제한한다.

```text
isAirborne = true
→ Sprint 불가
```

따라서 공중에서 Shift를 새로 눌러도:

```text
Sprint Speed 9 적용 X
Stamina 소비 X
```

이다.

---

# 12. Sprint Jump 관성 유지

지상에서 Sprint 중 점프하면:

```text
W + Shift
↓
Speed 9
↓
Space
```

공중에 들어가는 순간 Sprint는 종료된다.

하지만 수평 속도를 즉시 6으로 잘라버리지 않고
기존 Air Control을 통해 점진적으로 변화시킨다.

예:

```text
공중 진입 속도 9
↓
Air Control 적용
↓
8.x
↓
7.x
↓
6
```

즉 Sprint로 얻은 관성은 점프 직후 유지된다.

---

# 13. 대각선 Sprint 제한

기존 이동 방향 정규화 구조를 그대로 사용한다.

따라서:

```text
W + D + Shift
```

를 입력해도 Sprint 최고 속도는:

```text
9
```

를 넘지 않는다.

---

# 기존 시스템 유지

이번 일차에서도 다음 시스템은 그대로 유지한다.

```text
Camera Relative Movement

Ground Acceleration / Deceleration

Air Control

Jump

Gravity

Ground Check

Coyote Time

Jump Buffer

공중 연속 점프 방지
```

Sprint와 Stamina는 기존 이동 시스템 위에
추가 상태로 연결했다.

---

# 자동 테스트 확장

다음 테스트 파일을 수정했다.

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlayerCameraRelativeMovementTests.cs
```

기존 이동, 점프, Air Control 테스트를 유지하면서
Sprint 및 Stamina 관련 테스트를 추가했다.

---

## Sprint_RequiresHeldInputGroundAndStamina

다음 조건이 모두 존재하면 Sprint가 가능한지 확인한다.

```text
Shift 입력
이동 입력
지상
Stamina 존재
미소진 상태
```

---

## Sprint_DoesNotStartWithoutMoveInput

Shift만 누르고 이동 입력이 없으면
Sprint가 시작되지 않는지 확인한다.

---

## Sprint_DoesNotStartInAir

공중 상태에서는
Sprint가 시작되지 않는지 확인한다.

---

## Sprint_DoesNotStartAtZeroStamina

Current Stamina가 0이면
Sprint가 시작되지 않는지 확인한다.

---

## Sprint_DoesNotRestartWhileExhausted

Stamina가 일부 회복되어도
Sprint Exhausted 상태이고 Shift를 계속 누르고 있다면
Sprint가 자동 재시작되지 않는지 확인한다.

---

## Sprint_SelectsSprintMoveSpeed

Sprint 상태에서는:

```text
Sprint Speed = 9
```

가 선택되는지 확인한다.

---

## Walk_SelectsNormalMoveSpeed

일반 이동 상태에서는:

```text
Move Speed = 6
```

이 선택되는지 확인한다.

---

## Sprint_DrainsStamina

1초 동안 Sprint했을 때:

```text
100
→ 75
```

로 감소하는지 확인한다.

---

## NonSprint_RecoversStamina

1초 동안 Sprint하지 않았을 때:

```text
50
→ 70
```

으로 회복하는지 확인한다.

---

## Stamina_DoesNotGoBelowZero

Stamina가 남은 양보다 더 많이 소모되어도:

```text
Current Stamina = 0
```

에서 멈추는지 확인한다.

---

## Stamina_DoesNotExceedMaximum

회복량이 최대값을 넘어가도:

```text
Current Stamina = Max Stamina
```

로 제한되는지 확인한다.

---

## Sprint_DiagonalTargetDoesNotExceedSprintSpeed

대각선 Sprint에서도
수평 최고 속도 9를 초과하지 않는지 확인한다.

---

## SprintJump_AirControlPreservesMomentumGradually

Sprint Speed 9 상태에서 점프 후
Air Control이 적용될 때
수평 속도가 즉시 6으로 잘리지 않고
점진적으로 감소하는지 확인한다.

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

---

# 수동 테스트

## 기본 이동

```text
WASD
```

확인:

```text
최고 속도 = 6
```

---

## Sprint

```text
W + Left Shift
```

확인:

```text
최고 속도 = 9
```

기본 이동보다 빠르게 이동해야 한다.

---

## 정지 상태 Shift

```text
Shift
```

만 누른다.

확인:

```text
Sprint 시작 X
Stamina 소비 X
```

---

## Stamina 소모

Sprint를 계속 유지한다.

확인:

```text
100
→ 감소
→ 0
```

Stamina가 0이 되면
Sprint가 자동 종료되어야 한다.

---

## Stamina 회복

Sprint를 중단한다.

확인:

```text
0
→ 회복
→ 100
```

---

## 소진 잠금

Stamina를 0까지 사용한 뒤
Shift를 계속 누르고 있는다.

확인:

```text
Stamina 회복 O
Sprint 자동 재시작 X
```

이후:

```text
Shift 해제
↓
Shift 다시 입력
```

하면 Sprint가 다시 가능해야 한다.

---

## 공중 Sprint

```text
Jump
↓
공중에서 Shift
```

확인:

```text
Sprint X
Stamina 소비 X
```

---

## Sprint Jump

```text
W + Shift
↓
Space
```

확인:

```text
공중에서 Sprint 자체는 종료
하지만 수평 관성은 유지
```

---

## 대각선 Sprint

```text
W + D + Shift
```

확인:

```text
최고 속도 <= 9
```

---

# 완료 확인 항목

## Sprint

- [ ] Left Shift 입력 정상
- [ ] 이동 입력이 있을 때만 Sprint
- [ ] Sprint Speed 9 적용
- [ ] 정지 상태 Shift는 Sprint 불가
- [ ] 공중 Sprint 불가
- [ ] 대각선 Sprint 최고 속도 9 이하

## Stamina

- [ ] 시작 Stamina 100
- [ ] Sprint 중 초당 25 감소
- [ ] Sprint 미사용 시 초당 20 회복
- [ ] Stamina 0 이하로 내려가지 않음
- [ ] Stamina 100 초과하지 않음
- [ ] Stamina 0에서 Sprint 자동 종료
- [ ] 완전 소진 후 Shift 재입력 전까지 Sprint 재시작 불가

## 이동 회귀

- [ ] 기본 Move Speed 6 유지
- [ ] 지상 가속·감속 정상
- [ ] Air Control 정상
- [ ] Sprint Jump 관성 유지
- [ ] 카메라 기준 이동 정상

## Jump 시스템

- [ ] Jump 정상
- [ ] Gravity 정상
- [ ] Ground Check 정상
- [ ] Coyote Time 정상
- [ ] Jump Buffer 정상
- [ ] 공중 연속 점프 불가

## 테스트

- [ ] 기존 EditMode 테스트 Green
- [ ] Sprint 테스트 Green
- [ ] Stamina 테스트 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

# 결과

기존 기본 이동 시스템에 Sprint와 Stamina를 추가했다.

Left Shift와 이동 입력을 함께 사용하면
기본 속도 6에서 Sprint 속도 9까지 가속되며,
Sprint 중에는 Stamina가 감소한다.

Stamina가 0이 되면 Sprint가 자동 종료되고,
Sprint를 사용하지 않을 때는 Stamina가 다시 회복된다.

완전 소진 후 Shift를 계속 누르고 있을 때
Sprint가 반복적으로 켜졌다 꺼지는 문제를 방지하기 위해
Shift를 한 번 해제해야 Sprint를 다시 사용할 수 있는
소진 잠금 구조도 추가했다.

공중에서는 Sprint를 새로 사용할 수 없지만,
지상 Sprint 상태에서 점프했을 때 얻은 수평 관성은
기존 Air Control을 통해 자연스럽게 유지되도록 구성했다.

다음 17일차에서는
현재 기본 이동 시스템에 Crouch를 추가하고
Capsule Collider의 높이를 변경하는 구조를 구현한다.
