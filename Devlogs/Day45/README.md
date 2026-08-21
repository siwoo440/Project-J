# 45일차 개발일지 - 에어백 및 External Force 통합

## 개발 목표

45일차에서는 기존 에어백 구현과 다음 일차로 예정되어 있던 External Force 통합 작업을 하나로 묶어 진행했다.

핵심 목표는 다음과 같다.

- 측면 에어백 구현
- 에어백 설치 방향에 따른 밀림 방향 처리
- 접촉 위치에 따른 약한 측면 분산
- Player Push와 AirBag의 외부 힘 처리 계층 통합
- 여러 외부 힘의 합산
- 외부 힘 적용 시 기존 Y 속도 보존
- Phase 4 전용 테스트 구역 구성
- 테스트 Dummy의 중력 누락 문제 수정

현재 기준 커밋:

```text
84edffcdf5d44bc76fca2a78c2cbb949a5286323
```

현재 커밋 메시지:

```text
45
```

---

## 1. 공통 External Force 구조

새 파일:

```text
Assets/ProjectJ/Runtime/Push/
├─ ExternalForceSource.cs
└─ PlayerExternalForceReceiver.cs
```

기존에는 Player Push가 `PlayerExternalForceAccumulator`에 직접 힘을 전달했다.

45일차부터는 다음 구조를 사용한다.

```text
Player Push
        │
        ▼
PlayerPushReceiver
        │
        ▼
PlayerExternalForceReceiver
        │
        ▼
PlayerExternalForceAccumulator

Air Bag
        │
        ▼
PlayerExternalForceReceiver
        │
        ▼
PlayerExternalForceAccumulator
```

현재 `ExternalForceSource`는 다음 두 종류를 사용한다.

```text
Push
AirBag
```

---

## 2. PlayerExternalForceReceiver

공통 External Force Receiver는 Player가 외부 힘을 받을 수 있는지 검사한 뒤 기존 누적기에 전달한다.

검사 항목:

```text
Rigidbody 존재
Rigidbody가 Kinematic이 아님
Finish 상태가 아님
PlayerExternalForceAccumulator 존재
```

외부 힘은 수평 성분만 사용한다.

```text
X 유지
Y = 0
Z 유지
```

따라서 점프나 낙하 중 외부 힘을 받아도 기존 Y 속도를 직접 덮어쓰지 않는다.

---

## 3. 기존 Push 연결 변경

수정 파일:

```text
Assets/ProjectJ/Runtime/Push/
└─ PlayerPushReceiver.cs
```

변경 전:

```text
PlayerPushReceiver
→ PlayerExternalForceAccumulator
```

변경 후:

```text
PlayerPushReceiver
→ PlayerExternalForceReceiver
→ PlayerExternalForceAccumulator
```

Respawn Protection 같은 Push 전용 규칙은 기존 `PlayerPushReceiver`에 그대로 유지한다.

---

## 4. 에어백 구현

새 파일:

```text
Assets/ProjectJ/Runtime/Obstacles/
└─ AirBagObstacle.cs
```

에어백은 `OnCollisionEnter`에서 한 번 작동한다.

```text
Player 접촉 시작
↓
밀어낼 방향 계산
↓
External Force 적용
↓
Player가 밀려남
```

`OnCollisionStay`를 사용하지 않으므로 Collider에 계속 붙어 있어도 매 Physics Frame마다 힘이 반복 누적되지 않는다.

기본 힘:

```text
Horizontal Velocity Change = 12
```

기본 방향:

```text
localPushDirection = Vector3.forward
```

따라서 에어백 GameObject를 회전시키면 밀어내는 방향도 함께 회전한다.

---

## 5. 접촉 위치 기반 방향 보정

기본 설치 방향을 중심으로 Player가 에어백 가장자리에 닿은 위치를 약하게 반영한다.

기본값:

```text
Contact Spread = 0.35
```

정중앙 접촉:

```text
거의 transform.forward 방향
```

가장자리 접촉:

```text
기본 방향 + 약한 좌우 방향
```

Y 방향은 항상 제거한다.

---

## 6. 외부 힘 합산

기존 `PlayerExternalForceAccumulator`를 그대로 공통 누적기로 사용한다.

예:

```text
Push   = (4, 0, 0)
AirBag = (0, 0, 6)
```

결과:

```text
Combined = (4, 0, 6)
```

한 힘이 다른 힘을 덮어쓰지 않는다.

또한 다음 두 순서가 같은 결과를 만드는지 자동 테스트한다.

```text
Push → AirBag
AirBag → Push
```

---

## 7. Y 속도 보존

예를 들어 Player의 기존 Y 속도가 다음과 같을 때:

```text
Y Velocity = 5
```

외부 힘 입력에 임의의 Y 값이 포함되어도 실제 Receiver에서 Y를 제거한다.

결과:

```text
기존 Y Velocity 유지
외부 힘은 X/Z만 적용
```

따라서 에어백은 수평 밀침 장치로만 동작한다.

---

## 8. Day45 Setup Tool

새 파일:

```text
Assets/ProjectJ/Editor/
└─ Day45AirBagExternalForceSetup.cs
```

Unity 메뉴:

```text
ProjectJ
→ Day45
→ Setup AirBag External Force
```

실행 시:

```text
Player.prefab
→ PlayerExternalForceReceiver 추가 및 연결

Phase4_InteractionTest.unity
→ Day45 테스트 구역 생성
```

을 자동 수행한다.

---

## 9. Phase 4 테스트 구역

테스트 구역에는 다음 항목이 포함된다.

```text
BASIC : PUSH +X
ROTATED : PUSH +Z
EDGE : PUSH TO DROP
PUSH DUMMY INTO AIR BAG
```

### BASIC

설치 방향과 실제 밀리는 방향이 일치하는지 확인한다.

### ROTATED

에어백을 회전했을 때 힘 방향도 함께 회전하는지 확인한다.

### EDGE

가장자리 접촉에서 방향이 비정상적으로 뒤집히지 않는지 확인한다.

### PUSH DUMMY INTO AIR BAG

```text
Player가 Dummy 밀치기
↓
Dummy가 AirBag 접촉
↓
Push + AirBag
↓
공통 External Force 처리
```

를 확인한다.

---

## 10. Dummy 중력 문제 수정

초기 테스트에서는 Dummy가 에어백에 맞은 뒤 공중에서 일직선으로 계속 날아가는 현상이 있었다.

원인은 에어백 힘이 아니라 Dummy 설정이었다.

Project J Player는:

```text
Rigidbody.useGravity = false
```

상태이고, `PlayerCameraRelativeMovement`에서 자체 중력을 계산한다.

초기 Dummy 설정에서는 이 이동 스크립트까지 비활성화해 중력 계산 자체가 사라졌다.

수정 후에는 Dummy의 `PlayerCameraRelativeMovement`를 활성 상태로 유지한다.

비활성:

```text
PlayerInput
PlayerSurfaceInteraction
PlayerPushController
PlayerPushFeedbackUI
Dummy Camera
Dummy AudioListener
```

유지:

```text
PlayerCameraRelativeMovement
Rigidbody
PlayerExternalForceAccumulator
PlayerExternalForceReceiver
```

따라서 Dummy는 직접 조작되지는 않지만 정상적으로 중력을 받아 낙하한다.

---

## 11. Direction Marker 충돌 제거

에어백 방향 표시용 Marker는 시각 확인용 오브젝트다.

```text
Renderer 유지
Collider 제거
```

로 구성해 Player 이동이나 충돌에 영향을 주지 않도록 했다.

---

## 12. 자동 테스트

새 테스트:

```text
Assets/ProjectJ/Tests/EditMode/
└─ AirBagExternalForceTests.cs
```

검증 항목:

- Push + AirBag 힘 합산
- 힘 적용 순서 독립성
- Y Velocity 보존
- 중앙 접촉 방향
- 가장자리 Contact Spread
- 에어백 회전에 따른 힘 방향 변경

---

## 13. 생성·수정 요소

### 생성

```text
Assets/ProjectJ/Editor/
└─ Day45AirBagExternalForceSetup.cs

Assets/ProjectJ/Runtime/Obstacles/
└─ AirBagObstacle.cs

Assets/ProjectJ/Runtime/Push/
├─ ExternalForceSource.cs
└─ PlayerExternalForceReceiver.cs

Assets/ProjectJ/Tests/EditMode/
└─ AirBagExternalForceTests.cs
```

테스트용 Material:

```text
Assets/ProjectJ/Tests/Manual/Phase4/Materials/
├─ Day45_AirBag.mat
├─ Day45_DirectionMarker.mat
└─ Day45_TestFloor.mat
```

### 수정

```text
Assets/ProjectJ/Prefabs/Player/
└─ Player.prefab

Assets/ProjectJ/Runtime/Push/
└─ PlayerPushReceiver.cs

Assets/ProjectJ/Tests/Manual/Phase4/
└─ Phase4_InteractionTest.unity
```

### 삭제

```text
없음
```

---

## 14. 현재 구조

```text
Push ─────┐
          ├─→ PlayerExternalForceReceiver
AirBag ───┘
                  ↓
        PlayerExternalForceAccumulator
                  ↓
          수평 힘 합산 및 감속
```

현재 실제 Source는 Push와 AirBag 두 종류다.

---

## 15. 수동 테스트 체크리스트

- [ ] Player가 AirBag 접촉 시 한 번만 밀림
- [ ] 계속 붙어 있어도 힘이 매 프레임 반복되지 않음
- [ ] 재접촉 시 다시 발동
- [ ] 에어백 설치 방향과 실제 밀림 방향 일치
- [ ] 회전한 에어백도 정상
- [ ] 중앙 접촉 시 거의 정면으로 밀림
- [ ] 가장자리에서 약한 측면 분산
- [ ] Push와 AirBag 힘이 모두 유지
- [ ] 마지막 힘만 남지 않음
- [ ] 점프·낙하 중 Y 속도 보존
- [ ] Dummy가 AirBag 접촉 후 정상적으로 중력을 받음
- [ ] Dummy가 공중에서 일직선으로 무한 이동하지 않음
- [ ] Player가 AirBag 접촉 후 비정상 회전하지 않음
- [ ] 기존 Push 기능 정상
- [ ] 기존 플랫폼 기믹 회귀 문제 없음

---

## 16. 자동 테스트 체크리스트

```text
Window
→ General
→ Test Runner
→ EditMode
→ Run All
```

확인:

- [ ] AirBagExternalForceTests Green
- [ ] PlayerExternalForceAccumulatorTests Green
- [ ] PlayerPushControllerTests Green
- [ ] PlayerPushFeedbackEventTests Green
- [ ] PlatformGimmickTests Green
- [ ] 기존 EditMode 전체 Green
- [ ] 기존 PlayMode 테스트 Green

---

## 17. 이번 일차에서 구현하지 않은 기능

아직 미구현:

```text
Fusion State Authority 기반 External Force 확정
External Force 네트워크 동기화
아이템 External Force
추가 장애물 External Force
최종 AirBag 모델
최종 애니메이션
최종 VFX
최종 효과음
```

현재 단계에서는 Push와 AirBag의 공통 외부 힘 처리 기반까지만 구현한다.

---

## 18. 개발 결과

45일차에서는 에어백을 구현하고 기존 Player Push와 동일한 External Force 처리 계층으로 통합했다.

또한 테스트 Dummy에서 이동 스크립트를 꺼 자체 중력까지 사라졌던 문제를 수정해, 에어백에 맞은 Dummy도 정상적인 낙하 궤적을 유지하도록 테스트 환경을 보정했다.

---

## 19. 저장소 검토 메모

최신 `main` 커밋:

```text
84edffcdf5d44bc76fca2a78c2cbb949a5286323
```

현재 메시지:

```text
45
```

44일차 대비 45일차 커밋에는 AirBagObstacle, ExternalForceSource, PlayerExternalForceReceiver, Push Receiver 변경, Player Prefab 연결, EditMode 테스트, Phase 4 Day45 테스트 구역 및 Material이 포함되어 있다.

최신 Day45 Setup Tool에서는 Dummy의 `PlayerCameraRelativeMovement`를 비활성화하지 않아 자체 중력 계산이 유지된다.

정적 코드 검토 기준으로 현재 45일차 진행을 막는 문제는 확인되지 않았다.

다만 GitHub에는 별도 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 로컬 Unity에서 다음을 확인한 결과를 기준으로 한다.

```text
Console Error 0
EditMode 전체 통과
PlayMode 회귀 테스트 통과
AirBag 방향 테스트 통과
Push + AirBag 합산 테스트 통과
Dummy 중력·낙하 정상
기존 Push·Platform 회귀 문제 없음
```
