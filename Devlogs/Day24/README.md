# 24일차 개발일지 - Phase 2 플레이어 이동 시스템 종합 정리

## 1. 이번 일차 목적

24일차는 새로운 플레이어 기능을 추가하는 날이 아니라, **Phase 2에서 구현한 플레이어 이동·조작 시스템 전체를 하나의 기준 문서로 정리하는 날**로 진행한다.

현재 기준 커밋:

```text
d6b3dc06d0f53687b98db5ec47b5f9b40b711974
23일차 : 카메라 휠 줌 및 Sprint FOV 구현
```

Phase 2에서 정리된 핵심 범위는 다음과 같다.

```text
기본 이동
→ 가속 / 감속
→ 점프
→ Coyote Time / Jump Buffer
→ 공중 제어
→ Sprint / Stamina
→ Crouch
→ Standing Space Check
→ Slope / Ground Snap / Step Assist
→ Ledge Detect
→ Ledge Climb
→ 3인칭 Camera
→ Camera Collision
→ Mouse Wheel Zoom
→ Sprint FOV
→ 통합 회귀 확인
```

이 문서는 앞으로 플레이어 이동 감각을 수정할 때 참고하는 **Phase 2 이동 기준값 문서**로 사용한다.

---

# 2. Phase 2 구현 흐름

| 일차 | 구현 내용 | 현재 역할 |
|---|---|---|
| 11일차 | Camera Relative Movement | 카메라 방향 기준 WASD 이동 |
| 12일차 | Acceleration / Deceleration | 이동 시작·정지 감각 |
| 13일차 | Jump / Gravity / Ground Check | 기본 점프 및 착지 판정 |
| 14일차 | Coyote Time / Jump Buffer | 점프 입력 보정 |
| 15일차 | Air Control | 공중 이동 제어 |
| 16일차 | Sprint / Stamina | 달리기 및 스태미나 |
| 17일차 | Crouch | 앉기 및 충돌체 높이 변경 |
| 18일차 | Standing Space Check | 천장이 막혀 있을 때 강제 앉기 유지 |
| 19일차 | Slope / Ground Snap / Step Assist | 경사면·단차 이동 보정 |
| 20일차 | Ledge Detect | 오를 수 있는 턱 탐지 |
| 21일차 | Ledge Climb | 턱 위로 실제 이동 |
| 22일차 | Third Person Camera / Wall Collision | 카메라 회전 및 벽 가림 방지 |
| 23일차 | Mouse Wheel Zoom / Sprint FOV | 거리 조절 및 달리기 시야각 |
| 24일차 | Player Control Regression / 정리 | Phase 2 전체 이동 기준 정리 |

---

# 3. 플레이어 기본 물리 구조

현재 Player Prefab의 기본 물리 구조는 다음과 같다.

| 항목 | 현재 값 |
|---|---:|
| Layer | Player |
| Rigidbody Mass | 1 |
| Rigidbody Use Gravity | OFF |
| Rigidbody Interpolate | ON |
| Capsule Radius | 0.5 |
| Standing Capsule Height | 2.0 |
| Capsule Center | (0, 0, 0) |
| Ground Layer | World + Obstacle |

Unity 기본 Gravity를 사용하지 않고 `PlayerCameraRelativeMovement`에서 자체 Gravity를 계산한다.

Player는 이동할 때 Rigidbody의 `linearVelocity`를 직접 계산하며, 이동 방향이 존재할 경우 `MoveRotation`으로 해당 방향을 바라본다.

카메라만 회전할 때는 Player가 같이 회전하지 않는다.

---

# 4. 이동 속도 기준

## 4.1 기본 이동

| 항목 | 값 |
|---|---:|
| Normal Move Speed | 6.0 |
| Sprint Speed | 9.0 |
| Crouch Speed | 3.5 |

속도 우선순위는 현재 다음과 같다.

```text
지상 + Crouch
→ 3.5

지상 + Sprint
→ 9.0

일반 이동
→ 6.0
```

Crouch 상태에서는 Sprint가 허용되지 않는다.

공중에서는 새로운 Sprint가 시작되지 않는다.

---

# 5. 가속 / 감속

현재 이동은 입력 즉시 최대 속도로 바뀌지 않고 가속·감속 값을 사용한다.

| 항목 | 값 |
|---|---:|
| Ground Acceleration | 30 |
| Ground Deceleration | 40 |
| Air Acceleration | 12 |
| Air Deceleration | 6 |

지상에서는 빠르게 반응하고, 공중에서는 방향 변경과 감속을 더 느리게 만들어 공중 제어가 지상보다 제한적으로 느껴지도록 한다.

정리하면:

```text
지상 가속 30
지상 감속 40

공중 가속 12
공중 감속 6
```

공중 제어력이 지상보다 낮으므로 점프 후 방향을 완전히 자유롭게 바꾸는 방식은 아니다.

---

# 6. 카메라 기준 이동

WASD 이동 방향은 Player의 월드 방향이 아니라 **현재 Main Camera의 수평 Forward / Right 방향**을 기준으로 계산한다.

예:

```text
Camera가 북쪽을 바라봄
W → 북쪽

Camera를 동쪽으로 90° 회전
W → 동쪽
```

카메라의 위·아래 Pitch 성분은 이동 방향에서 제거하므로, 카메라가 위나 아래를 보고 있어도 W 입력이 공중 또는 지면 아래 방향으로 기울어지지 않는다.

---

# 7. 점프 시스템

## 7.1 기본 수치

| 항목 | 값 |
|---|---:|
| Jump Velocity | 8.0 |
| Gravity | -22.0 |
| Coyote Time | 0.12 sec |
| Jump Buffer Time | 0.12 sec |

Unity Rigidbody Gravity는 사용하지 않는다.

수직 속도는 이동 스크립트에서 직접 계산한다.

---

## 7.2 Ground Check

| 항목 | 값 |
|---|---:|
| Ground Check Radius | 0.22 |
| Ground Check Offset | 0.08 |
| Ground Probe Distance | 0.6 |

Ground Check는 Collider 바닥 근처에서 Sphere 방식으로 수행한다.

Ground Surface는 별도의 아래 방향 Raycast를 사용해 바닥 Normal과 바닥까지의 Gap을 확인한다.

---

## 7.3 Coyote Time

플레이어가 발판에서 막 떨어진 직후에도 짧은 시간 동안 점프 입력을 허용한다.

```text
Coyote Time = 0.12초
```

즉 발판 끝을 조금 지나친 직후 Space를 눌러도 점프할 수 있다.

---

## 7.4 Jump Buffer

착지 직전에 Space를 먼저 눌러도 입력을 잠시 기억한다.

```text
Jump Buffer = 0.12초
```

0.12초 안에 착지 조건이 만족되면 해당 입력을 사용해 점프한다.

---

## 7.5 Double Jump

현재 Phase 2에는 Double Jump가 없다.

공중에서 Space를 반복 입력해도 Coyote Time이나 착지 조건이 없으면 추가 점프하지 않는다.

---

# 8. Sprint / Stamina

## 8.1 Sprint 기본값

| 항목 | 값 |
|---|---:|
| Normal Speed | 6 |
| Sprint Speed | 9 |
| Max Stamina | 100 |
| Drain Rate | 25 / sec |
| Recovery Rate | 20 / sec |

100 Stamina를 전부 사용하는 데 걸리는 이론상 시간:

```text
100 ÷ 25 = 4초
```

0에서 100까지 완전히 회복하는 데 걸리는 이론상 시간:

```text
100 ÷ 20 = 5초
```

---

## 8.2 Sprint 가능 조건

현재 Sprint는 다음 조건을 모두 만족해야 한다.

```text
Sprint 입력 유지
이동 입력 존재
지상 상태
Stamina > 0
Sprint Exhausted 상태가 아님
Crouch 상태가 아님
```

따라서 다음 상황에서는 Sprint가 시작되지 않는다.

```text
공중
Crouch
Stamina 0
이동 입력 없음
```

---

## 8.3 Stamina Exhausted 처리

Sprint 중 Stamina가 0이 되면 `sprintExhausted` 상태가 된다.

이 상태에서는 Stamina가 조금 회복되었다고 바로 Sprint가 다시 시작되지 않는다.

현재 구현에서는 **Sprint 버튼을 한 번 놓아야 Exhausted 상태가 해제**된다.

따라서:

```text
Shift 계속 누름
→ Stamina 0
→ Sprint 종료
→ Stamina 회복 중
→ 여전히 Sprint 재시작 안 됨

Shift 해제
→ Exhausted 해제

다시 Shift
→ 조건 만족 시 Sprint 가능
```

---

# 9. Crouch

## 9.1 기본 수치

| 항목 | 값 |
|---|---:|
| Standing Height | 2.0 |
| Crouch Height | 1.2 |
| Capsule Radius | 0.5 |
| Crouch Speed | 3.5 |
| Standing Space Padding | 0.02 |

Crouch 시 CapsuleCollider 높이를 2.0에서 1.2로 낮춘다.

Collider 중심도 함께 아래로 이동시켜 발 위치가 크게 변하지 않도록 한다.

---

## 9.2 Standing Space Check

Ctrl을 해제했다고 무조건 일어서지 않는다.

Standing Capsule 크기로 `Physics.CheckCapsule` 검사를 수행해 머리 위 공간을 확인한다.

```text
위 공간 충분
→ Standing 가능

위 공간 부족
→ Crouch 강제 유지
```

이 시스템으로 낮은 천장 안에서 Collider가 천장을 뚫고 커지는 문제를 방지한다.

---

## 9.3 Crouch와 Sprint 관계

현재 규칙:

```text
Crouch 상태
→ Sprint 불가
```

Crouch 이동은 지상에서 3.5 속도를 사용한다.

---

# 10. 경사면 이동

## 10.1 기본 수치

| 항목 | 값 |
|---|---:|
| Max Slope Angle | 45° |
| Ground Probe Distance | 0.6 |
| Ground Snap Distance | 0.25 |
| Ground Snap Speed | 4 |

바닥 Raycast의 Surface Normal을 기준으로 경사각을 계산한다.

```text
45° 이하
→ Walkable Surface

45° 초과
→ Walkable Surface로 취급하지 않음
```

Walkable Surface에서는 이동 방향을 표면에 투영해 경사면을 따라 이동한다.

---

# 11. Ground Snap

플레이어가 걷거나 경사면을 내려갈 때 작은 간격 때문에 순간적으로 공중 상태가 되는 것을 줄이기 위해 Ground Snap을 사용한다.

| 항목 | 값 |
|---|---:|
| Ground Snap Distance | 0.25 |
| Ground Snap Speed | 4 |

주요 조건:

```text
Walkable Ground 존재
아래 방향 또는 거의 수평인 수직 속도
바닥 Gap이 0.25 이하
Jump 시작 상태가 아님
```

조건이 맞으면 아래 방향 속도를 적용해 바닥에 붙도록 보정한다.

---

# 12. Step Assist

작은 계단이나 단차를 매번 Jump하지 않고 통과할 수 있도록 Step Assist를 사용한다.

| 항목 | 값 |
|---|---:|
| Max Step Height | 0.4 |
| Step Check Distance | 0.6 |
| Step Up Speed | 3 |

현재 기준:

```text
약 0.4 이하 단차
→ Step Assist 대상

0.4보다 큰 단차
→ 기본 Step Assist 대상 아님
```

Step Assist가 감지되면 수직 속도를 최대 `3`까지 올려 단차 위로 올라가는 것을 보조한다.

Phase 2 기준에서 이 기능은 정확한 위치 Teleport 방식이 아니라 **수직 속도 보조 방식**이다.

---

# 13. Ledge Detect

Player 앞에 오를 수 있는 턱이 있는지 별도의 `PlayerLedgeDetector`가 검사한다.

## 13.1 Ledge 기본값

| 항목 | 값 |
|---|---:|
| Minimum Ledge Height | 0.45 |
| Maximum Ledge Height | 1.4 |
| Wall Check Distance | 0.8 |
| Top Probe Forward Offset | 0.2 |
| Top Probe Extra Height | 0.2 |
| Top Surface Max Angle | 45° |
| Landing Forward Offset | 0.35 |
| Landing Clearance Padding | 0.03 |

---

## 13.2 Ledge 유효 조건

Ledge 후보가 유효하려면 다음을 모두 만족해야 한다.

```text
앞쪽 Wall 존재
상단 앞쪽 공간이 막히지 않음
턱 윗면 Surface 존재
높이 0.45 ~ 1.4
윗면 경사 45° 이하
착지 위치에 Standing 공간 존재
```

하나라도 실패하면 `HasLedge = false`가 된다.

---

## 13.3 Ledge Layer

Ledge 검사 대상은 World / Obstacle을 사용하도록 설계되어 있다.

현재 저장소의 `Player.prefab`에서는 `PlayerLedgeDetector.ledgeLayers`의 Serialized 값이 0으로 남아 있다.

다만 런타임 `Awake()`의 Fallback에서 값이 0일 경우 자동으로:

```text
World
Obstacle
```

LayerMask를 설정한다.

따라서 현재 런타임 동작은 가능하지만, Prefab Inspector에서도 World + Obstacle을 명시적으로 저장해 두는 것이 이후 설정 확인에는 더 명확하다.

---

# 14. Ledge Climb

Ledge가 유효할 때 Space 입력은 일반 Jump보다 Climb을 먼저 시도한다.

## 14.1 시작 조건

```text
HasLedge = true
IsClimbing = false
IsCrouching = false
```

조건을 만족하지 않으면 기존 Jump Buffer 처리로 돌아간다.

---

## 14.2 Climb 수치

| 항목 | 값 |
|---|---:|
| Lift Duration | 0.2 sec |
| Forward Duration | 0.2 sec |
| Lift Clearance | 0.08 |
| 총 기본 Climb 시간 | 약 0.4 sec |

Climb은 두 단계로 나뉜다.

```text
1. Lift
현재 위치 → 턱 위 높이까지 상승

2. Forward
상승 위치 → 최종 착지 위치
```

---

## 14.3 Climb 중 물리 상태

Climb 시작 시:

```text
linearVelocity = 0
angularVelocity = 0
Rigidbody IsKinematic = true
CapsuleCollider = OFF
PlayerLedgeDetector = OFF
```

Climb 종료 후 이전 상태를 복구한다.

Climb 중에는 `PlayerCameraRelativeMovement.FixedUpdate()`가 조기 종료되므로 다음 시스템이 Climb 이동과 경쟁하지 않는다.

```text
일반 이동
Sprint
Gravity
Ground Snap
Step Assist
```

---

## 14.4 Climb 중 현재 주의점

Phase 2의 Ledge Climb은 기능 검증용 이동 방식이다.

Climb 약 0.4초 동안 CapsuleCollider가 꺼지므로 그 시간 동안 일반 물리 충돌·Trigger 상호작용이 제한될 수 있다.

Animation, Root Motion, IK, 네트워크 동기화는 현재 Phase 2 범위에 포함되지 않는다.

---

# 15. 입력 우선순위

Space 입력의 현재 우선순위는 다음과 같다.

```text
Space
↓
현재 Climb 중?
→ 입력 무시 / Jump Buffer 제거

아니면 유효 Ledge?
→ Ledge Climb 시작

Ledge Climb 불가?
→ 일반 Jump Buffer 등록
```

따라서 같은 Space 키를 Jump와 Ledge Climb에 함께 사용하지만 두 동작이 동시에 실행되지 않는다.

---

# 16. 3인칭 카메라와 이동 연동

Phase 2 후반에는 이동 기준이 되는 3인칭 카메라 시스템도 함께 정리했다.

## 16.1 Camera 기본값

| 항목 | 값 |
|---|---:|
| Mouse Sensitivity | 0.15 |
| Min Pitch | -45° |
| Max Pitch | 70° |
| Target Height | 1.5 |
| Default Distance | 7.5 |
| Minimum Distance | 3.5 |
| Maximum Distance | 10 |
| Zoom Step | 0.75 |

---

## 16.2 Camera Collision

| 항목 | 값 |
|---|---:|
| Collision Radius | 0.25 |
| Collision Padding | 0.15 |
| Camera Return Speed | 12 |
| Collision Layers | World + Obstacle |

Player와 Camera 사이에 벽이 끼면 SphereCast를 사용해 Camera를 벽 앞쪽으로 당긴다.

벽이 없어지면 사용자가 마지막으로 선택한 Camera Distance까지 부드럽게 복귀한다.

---

## 16.3 Mouse Wheel Zoom

```text
Wheel Up
→ Camera 가까이

Wheel Down
→ Camera 멀리
```

범위:

```text
3.5 ~ 10
```

1단계:

```text
0.75
```

Camera Collision으로 실제 거리가 줄어도 사용자가 선택한 Desired Distance는 유지된다.

---

## 16.4 Sprint FOV

| 항목 | 값 |
|---|---:|
| Normal FOV | 60 |
| Sprint FOV | 68 |
| FOV Change Speed | 8 |

FOV는 단순 Shift 입력이 아니라 실제 `PlayerCameraRelativeMovement.IsSprinting` 상태를 사용한다.

따라서 Stamina 부족, Crouch, 이동 입력 없음 등으로 Sprint가 실제 활성화되지 않으면 Sprint FOV도 적용되지 않는다.

---

# 17. Phase 2 전체 핵심 수치 요약

| 분류 | 항목 | 값 |
|---|---|---:|
| 이동 | Normal Speed | 6 |
| 이동 | Sprint Speed | 9 |
| 이동 | Crouch Speed | 3.5 |
| 이동 | Ground Acceleration | 30 |
| 이동 | Ground Deceleration | 40 |
| 이동 | Air Acceleration | 12 |
| 이동 | Air Deceleration | 6 |
| 점프 | Jump Velocity | 8 |
| 점프 | Gravity | -22 |
| 점프 | Coyote Time | 0.12 sec |
| 점프 | Jump Buffer | 0.12 sec |
| Ground | Ground Check Radius | 0.22 |
| Ground | Ground Check Offset | 0.08 |
| Ground | Ground Probe Distance | 0.6 |
| Sprint | Max Stamina | 100 |
| Sprint | Drain Rate | 25 / sec |
| Sprint | Recovery Rate | 20 / sec |
| Crouch | Standing Height | 2.0 |
| Crouch | Crouch Height | 1.2 |
| Crouch | Standing Padding | 0.02 |
| Slope | Max Slope Angle | 45° |
| Snap | Ground Snap Distance | 0.25 |
| Snap | Ground Snap Speed | 4 |
| Step | Max Step Height | 0.4 |
| Step | Step Check Distance | 0.6 |
| Step | Step Up Speed | 3 |
| Ledge | Min Height | 0.45 |
| Ledge | Max Height | 1.4 |
| Ledge | Wall Check Distance | 0.8 |
| Ledge | Top Forward Offset | 0.2 |
| Ledge | Top Extra Height | 0.2 |
| Ledge | Top Max Angle | 45° |
| Ledge | Landing Forward Offset | 0.35 |
| Ledge | Landing Padding | 0.03 |
| Climb | Lift Duration | 0.2 sec |
| Climb | Forward Duration | 0.2 sec |
| Climb | Lift Clearance | 0.08 |
| Camera | Sensitivity | 0.15 |
| Camera | Pitch | -45° ~ 70° |
| Camera | Default Distance | 7.5 |
| Camera | Zoom Range | 3.5 ~ 10 |
| Camera | Zoom Step | 0.75 |
| Camera | Collision Radius | 0.25 |
| Camera | Collision Padding | 0.15 |
| Camera | Return Speed | 12 |
| Camera | Normal FOV | 60 |
| Camera | Sprint FOV | 68 |
| Camera | FOV Change Speed | 8 |

---

# 18. 이동 상태별 규칙 요약

| 상태 | 이동 속도 | Jump | Sprint | 특징 |
|---|---:|---|---|---|
| 일반 지상 | 6 | 가능 | 가능 | Camera 기준 이동 |
| Sprint | 9 | 가능 | 활성 | Stamina 소모 |
| Crouch | 3.5 | 기존 Jump 입력 유지 | 불가 | Collider Height 1.2 |
| 공중 | 목표 기본 속도 기준 Air Control | 추가 Jump 불가 | 새 Sprint 불가 | Accel 12 / Decel 6 |
| 경사면 | 상태별 속도 | 가능 | 조건 충족 시 가능 | 45° 이하 Walkable |
| Step Assist | 상태별 속도 | 가능 | 조건 충족 시 가능 | 0.4 이하 단차 보조 |
| Ledge Detect | 일반 이동 유지 | Space가 Climb 우선 | 가능 여부는 이동 상태에 따름 | 0.45~1.4 턱 |
| Ledge Climb | 일반 이동 일시 중지 | Climb 중 추가 Jump 차단 | 불가 | 약 0.4초 |

---

# 19. Phase 2에서 확정된 이동 설계 원칙

## 카메라 기준 이동

플레이어 이동은 고정 월드축이 아니라 카메라 방향을 기준으로 한다.

## 캐릭터 방향

카메라 회전만으로 캐릭터가 돌아가지 않는다.

실제 이동 방향이 있을 때 캐릭터가 해당 방향을 바라본다.

## Sprint

Sprint는 별도 상태이며 이동 입력, 지상 여부, Stamina, Crouch 상태를 모두 확인한다.

## Jump

기본 점프는 단순 Grounded 여부만 확인하지 않고 Coyote Time과 Jump Buffer를 사용한다.

## Crouch

Crouch 해제 시 머리 위 공간을 먼저 확인한다.

## Slope

45° 이하만 정상 이동 가능한 Surface로 판단한다.

## Step

작은 단차는 Jump 없이 지나갈 수 있도록 보조하되, 큰 장애물을 자동으로 올라가지는 않는다.

## Ledge

일반 Step보다 높은 턱은 Ledge Detect / Climb 영역으로 분리한다.

## Camera

Camera Collision, Zoom, Sprint FOV는 플레이어 이동 상태와 연동되지만 Player 이동 물리 자체와는 분리한다.

---

# 20. 현재 구현 스크립트 역할

| 스크립트 | 역할 |
|---|---|
| `PlayerCameraRelativeMovement.cs` | 이동, 가속/감속, Jump, Gravity, Coyote, Buffer, Air Control, Sprint, Stamina, Crouch, Standing Check, Slope, Snap, Step |
| `PlayerLedgeDetector.cs` | 벽·턱 높이·윗면·착지 공간 검사 |
| `PlayerLedgeClimber.cs` | Lift → Forward Climb 이동 및 물리 상태 전환 |
| `PlayerThirdPersonCamera.cs` | Camera Look, Collision, Zoom, Sprint FOV |

Player Prefab은 이동 기능을 한 개의 거대한 스크립트로 모두 처리하는 구조가 아니라, **기본 이동 / Ledge 감지 / Ledge 이동 / Camera**로 역할을 나누고 있다.

---

# 21. Phase 2 현재 확인이 필요한 주의사항

### Ledge Layer Serialized 값

`Player.prefab`의 `PlayerLedgeDetector.ledgeLayers` 값은 현재 Serialized 상태에서 `0`이다.

런타임 Fallback으로 World + Obstacle이 설정되기 때문에 동작은 가능하지만, 이후 Prefab 설정을 정리할 때 Inspector에도 World + Obstacle을 저장하는 것이 좋다.

### Step Assist

현재 Step Assist는 단차 윗면으로 정확히 위치를 보정하는 방식이 아니라 `Step Up Speed = 3`의 수직 속도를 주는 방식이다.

향후 실제 맵 제작 후 단차에서 튀는 느낌이 있으면 조정 대상이다.

### Ledge Climb

현재 Climb 중 Collider를 잠시 비활성화한다.

향후 멀티플레이, 피격, Trigger, 움직이는 발판과 결합할 때 별도 검토가 필요하다.

### Camera Collision

현재 SphereCast는 World + Obstacle을 대상으로 한다.

실제 맵 Asset이 추가되면 모든 Camera Blocking 구조물이 올바른 Layer에 배치되어 있는지 확인해야 한다.

---

# 22. 24일차 회귀 테스트 기준

Phase 2 종료 전 다음 조합을 통합 확인한다.

```text
WASD
WASD + Camera Rotate
Sprint
Sprint + Jump
Sprint + Camera Rotate
Sprint + Wheel Zoom
Sprint + Wall Camera Collision
Crouch
Crouch + 낮은 천장
Jump
Coyote Jump
Buffered Jump
Slope 이동
Slope + Jump
Step Assist
Step + Sprint
Ledge Detect
Ledge Climb
Ledge Climb + Camera Collision
Ledge Climb + Zoom
```

최종 기준:

```text
Player 상태 고착 없음
Sprint 상태 고착 없음
Crouch 상태 고착 없음
Rigidbody 이상 속도 없음
Camera 심한 떨림 없음
Collider 복구 정상
EditMode 전체 Green
PlayMode 전체 Green
Console Error 0
```

---

# 23. Phase 2 완료 시점의 플레이어 이동 구조

Phase 2 종료 기준으로 플레이어는 다음 흐름으로 움직인다.

```text
Input
↓
Camera Relative Direction
↓
Ground / Air 판정
↓
Crouch / Sprint 상태 결정
↓
Move Speed 결정
↓
Ground 또는 Air Acceleration 적용
↓
Slope / Gravity / Jump 계산
↓
Ground Snap / Step Assist 보정
↓
Rigidbody Velocity 적용
↓
이동 방향으로 Player Rotation
```

Space 입력은 별도 우선순위를 가진다.

```text
Space
↓
Ledge Climb 가능?
├─ Yes → Climb
└─ No
   ↓
Jump Buffer
   ↓
Ground / Coyote 조건 충족
   ↓
Jump
```

Camera는 Player 이동과 분리되어 다음과 같이 동작한다.

```text
Mouse Look
→ Camera Yaw / Pitch

Mouse Wheel
→ Desired Camera Distance

World / Obstacle
→ Actual Camera Distance 보정

Player IsSprinting
→ FOV 60 ↔ 68
```

---

# 24. 개발 결과

24일차에서는 새로운 플레이어 기능을 추가하는 대신, Phase 2에서 구현한 이동 시스템을 하나의 기준으로 정리했다.

현재 Player 이동은 단순 WASD 수준을 넘어 다음 요소들이 서로 연결된 상태이다.

```text
Camera Relative Movement
Acceleration / Deceleration
Jump / Gravity
Coyote Time / Jump Buffer
Air Control
Sprint / Stamina
Crouch / Standing Space
Slope / Ground Snap
Step Assist
Ledge Detect / Climb
Third Person Camera
Camera Collision
Mouse Wheel Zoom
Sprint FOV
```

이 문서에 기록된 수치는 **Phase 2 종료 시점의 현재 구현 기준값**이며, 이후 고정 5구간 Greybox와 실제 장애물 테스트를 진행하면서 조작감 튜닝이 필요할 경우 변경 이력을 별도로 기록한다.

Phase 2가 안정적으로 검증되면 다음 개발 단계에서는 플레이어 조작 시스템 자체를 계속 확장하기보다, 이 이동 규칙을 실제 게임 공간에 적용하는 **고정 5구간 Greybox 제작 단계**로 넘어간다.
