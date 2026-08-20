# 12일차 개발일지 - 지상 가속·감속 구현

## 오늘의 목표

11일차에 구현한 카메라 기준 플레이어 이동을 유지하면서,
입력 즉시 최고 속도로 이동하고 입력 해제 즉시 멈추던 방식을 개선한다.

플레이어의 수평 속도가 목표 속도까지 점진적으로 증가하고,
입력을 놓으면 자연스럽게 감소하도록
지상 가속·감속 시스템을 구현한다.

이번 일차에서는 점프, 공중 제어, 경사 처리 등은 추가하지 않고
현재 지상 이동의 속도 변화만 다룬다.

---

## 현재 기준

11일차에서 구현된 이동 구조:

```text
WASD / Left Stick
↓
카메라 기준 이동 방향 계산
↓
Rigidbody 수평 이동
```

기존에는 이동 입력이 들어오면 즉시 `Move Speed`까지 속도가 변경됐다.

```text
정지
↓ W
즉시 최고 속도

W 해제
↓
즉시 정지
```

12일차에서는 이 부분을 점진적인 속도 변화 방식으로 수정했다.

---

# 구현 내용

## 1. 이동 설정값 추가

다음 스크립트를 수정했다.

```text
Assets/ProjectJ/Runtime/Player/PlayerCameraRelativeMovement.cs
```

기존:

```text
Move Speed = 6
```

에 다음 값을 추가했다.

```text
Acceleration = 30
Deceleration = 40
```

각 값의 역할:

| 설정 | 기본값 | 역할 |
| --- | ---: | --- |
| Move Speed | 6 | 플레이어 최고 수평 이동 속도 |
| Acceleration | 30 | 이동 입력이 있을 때 목표 속도까지 증가하는 속도 |
| Deceleration | 40 | 이동 입력이 없을 때 정지까지 감소하는 속도 |

감속 값을 가속보다 높게 설정해
출발은 부드럽게 하면서 정지는 지나치게 미끄럽지 않도록 했다.

---

## 2. 수평 속도 계산 분리

다음 계산 함수를 추가했다.

```text
CalculateHorizontalVelocity
```

처리 흐름:

```text
현재 Rigidbody Velocity
↓
X / Z 수평 속도 추출
↓
Move Direction × Move Speed
↓
목표 수평 속도 계산
↓
현재 속도 → 목표 속도
↓
Acceleration 또는 Deceleration만큼 이동
↓
새 수평 속도 반환
```

Y축 속도는 이 계산에서 제외한다.

이를 통해 향후 점프와 중력이 추가되더라도
수평 이동 로직이 수직 속도를 직접 덮어쓰지 않도록 했다.

---

## 3. Vector3.MoveTowards 기반 속도 보간

현재 속도에서 목표 속도로 한 번에 변경하지 않고
`Vector3.MoveTowards`를 사용해 일정한 변화량만 적용하도록 수정했다.

개념:

```text
현재 속도
→
현재 속도 + 이번 FixedUpdate에서 허용되는 변화량
→
목표 속도
```

변화량:

```text
Acceleration × Fixed Delta Time
```

또는:

```text
Deceleration × Fixed Delta Time
```

을 사용한다.

---

# 가속 처리

이동 입력이 존재하면 `Acceleration`을 사용한다.

예:

```text
Move Speed = 6
Acceleration = 30
Fixed Delta Time = 0.02
```

한 번의 물리 업데이트에서 허용되는 속도 변화량:

```text
30 × 0.02
= 0.6
```

따라서 정지 상태에서 W를 누르면 대략:

```text
0
→ 0.6
→ 1.2
→ 1.8
→ ...
→ 6
```

형태로 증가한다.

한 프레임 만에 최고 속도에 도달하지 않는다.

---

# 감속 처리

이동 입력이 없으면 `Deceleration`을 사용한다.

예:

```text
Deceleration = 40
Fixed Delta Time = 0.02
```

한 번의 물리 업데이트에서:

```text
40 × 0.02
= 0.8
```

만큼 수평 속도가 0에 가까워진다.

예:

```text
6
→ 5.2
→ 4.4
→ 3.6
→ ...
→ 0
```

입력을 놓자마자 즉시 정지하지 않으면서도
지나치게 오래 미끄러지지 않도록 했다.

---

# 방향 반전 처리

앞으로 이동 중 반대 방향 입력을 넣어도
속도를 즉시 반전하지 않는다.

예:

```text
W 이동 중
↓
S 입력
```

정상 처리:

```text
앞 방향 속도
↓
점진적으로 감소
↓
0 통과
↓
뒤 방향으로 가속
```

따라서 다음과 같은 즉각적인 반전은 발생하지 않는다.

```text
앞 6
→
뒤 6
```

이 구조를 통해 플레이어 이동에 기본적인 무게감이 생긴다.

---

# 최대 속도 제한

목표 속도는 다음 구조로 계산한다.

```text
Move Direction × Move Speed
```

이동 방향은 최대 길이 1로 제한되므로
수평 이동 속도가 `Move Speed`를 초과하지 않는다.

현재 기본 최고 속도:

```text
6
```

---

# 대각선 이동 유지

11일차에서 구현한 대각선 정규화도 유지한다.

입력:

```text
W + D
W + A
S + D
S + A
```

에서도:

```text
대각선 최고 속도
=
직선 최고 속도
```

가 유지된다.

즉 대각선 입력 때문에 이동 속도가 증가하지 않는다.

---

# 카메라 기준 이동 유지

11일차의 Camera Relative Movement 계산은 수정하지 않았다.

따라서:

```text
CameraRig Y = 0°
90°
180°
270°
```

어느 방향에서도:

```text
W = 현재 화면 기준 앞으로
```

동작한다.

12일차에서는 이동 방향 계산이 아니라
계산된 방향으로 얼마나 빠르게 가속하고 감속할지를 확장했다.

---

# 수직 속도 보존

수평 이동 계산에서는:

```text
X
Z
```

만 사용한다.

실제 Rigidbody에 다시 적용할 때는 기존:

```text
currentVelocity.y
```

값을 그대로 유지한다.

구조:

```text
새 X 속도
기존 Y 속도
새 Z 속도
```

따라서 향후 13일차에서 점프와 중력을 구현할 때
수평 이동 코드가 점프 속도를 직접 제거하지 않는 구조가 마련됐다.

---

# 자동 테스트 확장

다음 테스트 파일을 수정했다.

```text
Assets/ProjectJ/Tests/EditMode/PlayerCameraRelativeMovementTests.cs
```

11일차 테스트는 그대로 유지하면서
가속·감속 관련 테스트를 추가했다.

---

## Acceleration_DoesNotReachMaxSpeedImmediately

정지 상태에서 이동 입력을 넣었을 때
한 번의 업데이트만으로 최고 속도에 도달하지 않는지 확인한다.

검증:

```text
0 < 첫 가속 속도 < Move Speed
```

---

## Acceleration_DoesNotExceedMoveSpeed

이미 최고 속도에 가까운 상태에서 계속 가속해도
Move Speed를 초과하지 않는지 확인한다.

기준:

```text
Move Speed = 6
최종 Speed = 6 이하
```

---

## Deceleration_ReducesSpeedWithoutInput

이동 중 입력을 놓았을 때
현재 속도가 점진적으로 감소하는지 확인한다.

검증:

```text
0 < 감속 후 속도 < 감속 전 속도
```

---

## Deceleration_EventuallyStopsAtZero

현재 속도가 매우 낮을 때
감속 결과가 0을 넘어 반대 방향으로 이동하지 않고
정확히 정지하는지 확인한다.

---

## ReverseInput_ChangesDirectionGradually

앞으로 최고 속도로 이동하는 상태에서
반대 방향 입력을 넣었을 때
한 번에 뒤 방향 최고 속도로 변경되지 않는지 확인한다.

---

## DiagonalTarget_DoesNotExceedMoveSpeed

대각선 이동 목표 속도도
Move Speed를 초과하지 않는지 확인한다.

---

## HorizontalCalculation_DoesNotUseVerticalVelocity

현재 Rigidbody에 큰 Y 속도가 존재해도
수평 속도 계산 결과 자체에는 Y값이 포함되지 않는지 확인한다.

이 테스트는 향후 점프 시스템과
현재 수평 이동 시스템을 분리하기 위한 기반이다.

---

# 수정된 파일

이번 일차에서 수정된 파일:

```text
Assets/ProjectJ/Runtime/Player/PlayerCameraRelativeMovement.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerCameraRelativeMovementTests.cs
```

---

# 생성된 파일

```text
없음
```

11일차 테스트 맵과 Player Prefab을 그대로 재사용했다.

---

# 삭제된 파일

```text
없음
```

---

# 테스트 방법

11일차에서 만든 다음 Scene을 사용한다.

```text
Assets/ProjectJ/Tests/Manual/Day11/
└─ Day11_MovementTest.unity
```

---

## Zone 01 - 가속·감속

확인:

```text
정지
↓ W
점진적으로 가속

W 해제
↓
점진적으로 감속
↓
정지
```

---

## Zone 02 - 직선 이동

거리 표시를 기준으로:

```text
가속 거리
감속 거리
최고 속도 도달
```

을 확인한다.

---

## Zone 03 - 대각선 이동

다음 입력을 확인한다.

```text
W + D
W + A
S + D
S + A
```

확인:

```text
직선보다 빨라지지 않음
가속 정상
감속 정상
```

---

## Zone 04 - 장애물 충돌

가속한 상태에서 벽과 박스에 접근한다.

확인:

```text
Player가 Obstacle을 통과하지 않음
충돌 이후 입력 정상
충돌 이후 속도 계산 이상 없음
```

---

# 완료 확인 항목

## 이동

- [ ] W 입력 시 점진적으로 가속
- [ ] A/S/D 입력에서도 동일하게 가속
- [ ] 입력 해제 시 점진적으로 감속
- [ ] 완전히 정지 가능
- [ ] 최고 속도 6 유지
- [ ] 대각선 이동 속도 증가 없음
- [ ] W → S 반전이 점진적으로 처리됨

## 기존 기능 회귀

- [ ] 카메라 기준 이동 유지
- [ ] 이동 방향 회전 유지
- [ ] Player ↔ World 충돌 정상
- [ ] Player ↔ Obstacle 충돌 정상

## 자동 테스트

- [ ] 기존 이동 방향 테스트 Green
- [ ] 가속 테스트 Green
- [ ] 감속 테스트 Green
- [ ] 최대 속도 테스트 Green
- [ ] 방향 반전 테스트 Green
- [ ] 대각선 속도 테스트 Green
- [ ] 수직 속도 분리 테스트 Green
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green

## 프로젝트 상태

- [ ] Console Error 0
- [ ] 기존 Scene 흐름 정상

---

# 결과

카메라 기준 플레이어 이동에
지상 가속·감속 시스템을 추가했다.

이제 플레이어는 입력과 동시에 최고 속도로 이동하지 않고
일정한 가속도를 통해 목표 속도까지 도달한다.

입력을 놓으면 감속도를 통해 자연스럽게 정지하며,
반대 방향 입력에서도 속도가 즉시 뒤집히지 않고
현재 속도에서 새로운 목표 속도로 점진적으로 전환된다.

수평 속도 계산과 Y축 속도를 분리해
향후 점프 및 중력 시스템을 추가할 수 있는 구조도 마련했다.

다음 13일차에서는
현재 수평 이동 시스템을 유지한 상태에서
점프와 중력을 구현한다.
