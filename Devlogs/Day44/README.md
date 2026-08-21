# 44일차 개발일지 - 플랫폼·표면 기믹 5종 통합 및 상호작용 안정화

## 1. 개발 목표

44일차에서는 기존 44~48일차로 분리되어 있던 플랫폼·표면 기믹 5종을 하나의 일차로 통합 구현했다.

구현 대상:

```text
이동 플랫폼
회전 플랫폼
스프링 플랫폼
빙판길
유령 플랫폼
```

추가로 실제 테스트 과정에서 확인된 다음 문제도 함께 수정했다.

```text
장애물이나 회전 플랫폼 근처에서
Player가 물리 반작용으로 혼자 빙글빙글 회전하는 현상

유령 플랫폼 Warning 상태에서
깜빡이는 연출이 부자연스러운 문제
```

현재 기준 커밋:

```text
4f8b91e94ed91ef5ede01c2a3f92ad2d12fd9a11
```

현재 커밋 메시지:

```text
44
```

---

## 2. Phase 4 테스트 구역 확장

Editor Setup:

```text
Assets/ProjectJ/Editor/
└─ Day44PlatformGimmickSetup.cs
```

Unity 메뉴:

```text
ProjectJ
→ Day44
→ Setup Platform Gimmicks
```

기존 Phase 4 테스트 Scene의 동쪽을 확장해 5종 기믹을 한 공간에서 확인할 수 있도록 구성했다.

대상 Scene:

```text
Assets/ProjectJ/Tests/Manual/Phase4/
└─ Phase4_InteractionTest.unity
```

개념 구조:

```text
기존 Phase4 테스트 구역
        │
      Bridge
        │
┌──────────────────────────────────┐
│ MOVING      ROTATING     SPRING  │
│ PLATFORM    PLATFORM     JUMP    │
│                                  │
│ ICE LANE          GHOST PLATFORM│
└──────────────────────────────────┘
```

---

## 3. 이동 플랫폼

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Platforms/
├─ MovingPlatform.cs
└─ PlatformPassengerCarrier.cs
```

이동 플랫폼은 지정한 Point A와 Point B 사이를 반복 왕복한다.

기본 흐름:

```text
Point A
↓
Moving Platform
↓
Point B
↓
다시 Point A
```

기본 이동 속도:

```text
2.5
```

플랫폼이 이동할 때 플랫폼 위에 서 있는 Player가 뒤에 남거나 미끄러지지 않도록 `PlatformPassengerCarrier`가 플랫폼의 이전 위치와 다음 위치 차이를 계산해 Player 위치도 함께 보정한다.

---

## 4. 공통 플랫폼 탑승 보정

`PlatformPassengerCarrier`는 이동 플랫폼과 회전 플랫폼이 공통으로 사용한다.

처리 흐름:

```text
플랫폼 이전 Position / Rotation
↓
플랫폼 위 Player 탐색
↓
플랫폼 다음 Position / Rotation
↓
Player 상대 위치 변환
↓
Player Rigidbody MovePosition
```

이를 통해 Player가 플랫폼 위에서:

```text
정지
걷기
점프 직전
가장자리 이동
```

상태일 때도 플랫폼 움직임을 따라갈 수 있는 기반을 마련했다.

---

## 5. 회전 플랫폼

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Platforms/
└─ RotatingPlatform.cs
```

기존 계획의 회전 장애물을 수정해 **플랫폼 자체가 중심축을 기준으로 회전하는 기믹**으로 구현했다.

기본 설정:

```text
World Axis = Up
Degrees Per Second = 35
```

구조:

```text
       Player
         ●
   ┌──────────┐
   │ Platform │
   └──────────┘
         ↻
```

플랫폼 위 Player의 위치는 회전을 따라가지만 Player의 바라보는 방향 자체를 플랫폼이 강제로 회전시키지는 않는다.

---

## 6. Player 물리 회전 버그 수정

테스트 과정에서 장애물 또는 회전 플랫폼과 접촉할 때 Player가 혼자 Y축으로 계속 회전하는 현상이 확인됐다.

원인:

```text
Player Rigidbody의 물리 회전이 일부 축에서 허용
↓
플랫폼/장애물 Collider와 접촉
↓
마찰·충돌 반작용
↓
angularVelocity 발생
↓
Player가 혼자 회전
```

수정 파일:

```text
Assets/ProjectJ/Runtime/Player/
└─ PlayerCameraRelativeMovement.cs
```

Player Rigidbody 초기화 시 물리 회전을 전부 잠그도록 변경했다.

```text
Freeze Rotation X
Freeze Rotation Y
Freeze Rotation Z
```

또 기존에 남아 있을 수 있는 각속도도 초기화한다.

현재 Player Prefab Rigidbody Constraints 역시:

```text
Freeze Rotation = X / Y / Z
```

전체가 활성화된 상태다.

Player의 정상적인 방향 전환은 기존처럼 이동 코드의:

```text
Rigidbody.MoveRotation()
```

이 담당한다.

따라서:

```text
물리 충돌에 의한 회전
→ 차단

WASD 이동 방향에 따른 회전
→ 정상 유지
```

구조가 되었다.

---

## 7. 스프링 플랫폼

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Platforms/
└─ SpringPlatform.cs
```

기존의 자동 발사 방식 대신 **스프링 플랫폼을 밟은 상태에서 Player가 직접 점프할 때 그 한 번의 점프력을 강화하는 방식**으로 구현했다.

기본 배율:

```text
Jump Multiplier = 1.5
```

예:

```text
일반 Jump Velocity = 8
Spring Jump = 12
```

흐름:

```text
Spring Platform 위에 서 있음
↓
Space 입력
↓
정상 점프 발생
↓
Y Jump Velocity × 1.5
↓
강화 1회 소비
```

자동으로 Player를 발사하지 않는다.

---

## 8. PlayerSurfaceInteraction 추가

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Player/
└─ PlayerSurfaceInteraction.cs
```

Player가 현재 어떤 특수 표면을 밟고 있는지 탐지한다.

현재 연결 대상:

```text
SpringPlatform
IceSurface
```

역할:

```text
현재 Ground Surface 검사
├─ Spring → 점프 강화
└─ Ice    → 이동 관성 보정
```

기존 `PlayerCameraRelativeMovement`의 핵심 이동 코드를 대규모 수정하지 않고 특수 표면 동작을 별도 계층으로 분리했다.

---

## 9. 빙판길

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Platforms/
└─ IceSurface.cs
```

빙판에서는 Rigidbody 전체 Damping을 변경하지 않고 Player의 수평 속도 변화량을 별도로 제한한다.

기본 설정:

```text
Acceleration = 6
Deceleration = 2.5
Turn Acceleration = 3
```

일반 바닥:

```text
입력 해제
→ 빠른 정지

반대 방향 입력
→ 빠른 방향 전환
```

빙판:

```text
입력 해제
→ 천천히 감속

반대 방향 입력
→ 기존 관성을 유지하며 느리게 전환
```

밀치기 외부 힘 감속과 빙판 이동 관성은 서로 다른 시스템으로 유지한다.

---

## 10. 유령 플랫폼

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Platforms/
└─ GhostPlatform.cs
```

상태 구조:

```text
Active
↓
Warning
↓
Hidden
↓
Active
```

기본 시간:

```text
Active = 3초
Warning = 1초
Hidden = 2초
```

Hidden 상태에서는:

```text
Renderer
→ 보이지 않음

Collider
→ 비활성
```

이 되어 위에 서 있던 Player가 정상적으로 추락한다.

다시 나타날 때 Player가 플랫폼 내부에 겹쳐 있다면 위쪽으로 위치를 보정해 끼임을 줄인다.

---

## 11. 유령 플랫폼 페이드 연출 수정

초기 구현에서는 Warning 상태에서 Renderer를 빠르게 켰다 껐다 하는 깜빡임 방식이었다.

수정 후에는 Warning 1초 동안 Alpha를 연속적으로 감소시키는 방식으로 변경했다.

```text
Active
Alpha = 1.0

Warning 시작
Alpha = 1.0

Warning 중간
Alpha = 0.5

Warning 종료
Alpha = 0.0

Hidden
Alpha = 0.0
Collider OFF
```

즉:

```text
완전 표시
→ 서서히 투명
→ 완전히 사라짐
→ 일정 시간 후 재등장
```

형태가 된다.

기존 Material이 Opaque여도 런타임에서 플랫폼 전용 Material 복사본을 만들고 Transparent 렌더링 설정을 적용하도록 구성했다.

원본 Shared Material 자체는 직접 변경하지 않는다.

---

## 12. 유령 플랫폼 Collider 처리

페이드 중에는 아직 플랫폼을 밟을 수 있다.

```text
Active
→ Collider ON

Warning / Fade
→ Collider ON

Hidden
→ Collider OFF
```

따라서 시각적으로 거의 사라질 때까지 Player가 서 있을 수 있고, 완전히 사라지는 순간 정상 추락한다.

---

## 13. 테스트용 기믹 배치

Day44 Setup Tool은 Phase 4 테스트맵에 다음 테스트 공간을 자동 구성한다.

```text
MOVING PLATFORM
ROTATING PLATFORM
SPRING JUMP x1.5
ICE LANE
GHOST PLATFORMS
```

각 구간에 별도의 임시 Material과 Label을 사용해 기능을 쉽게 구분하도록 했다.

---

## 14. 자동 테스트

테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlatformGimmickTests.cs
```

주요 검증 항목:

```text
Moving Platform 목표 지점 이동 계산
Player 플랫폼 이동량 보정
Player 플랫폼 회전 위치 보정
Spring Jump 1.5배
Ice Surface 느린 감속
Ghost Platform 상태 순환
Ghost Warning Alpha 페이드
Ghost Hidden Alpha = 0
```

유령 플랫폼의 새로운 페이드 연출은 수치 기반으로도 확인한다.

예:

```text
Warning 시작
→ 1.0

중간
→ 0.5

종료
→ 0.0
```

---

## 15. 생성·수정 요소

### 주요 생성

```text
Assets/ProjectJ/Runtime/Platforms/
├─ MovingPlatform.cs
├─ PlatformPassengerCarrier.cs
├─ RotatingPlatform.cs
├─ SpringPlatform.cs
├─ IceSurface.cs
└─ GhostPlatform.cs

Assets/ProjectJ/Runtime/Player/
└─ PlayerSurfaceInteraction.cs

Assets/ProjectJ/Editor/
└─ Day44PlatformGimmickSetup.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlatformGimmickTests.cs
```

### 주요 수정

```text
Assets/ProjectJ/Runtime/Player/
└─ PlayerCameraRelativeMovement.cs

Assets/ProjectJ/Prefabs/Player/
└─ Player.prefab

Assets/ProjectJ/Tests/Manual/Phase4/
└─ Phase4_InteractionTest.unity
```

Phase 4 테스트맵용 Material도 함께 추가됐다.

### 삭제

```text
없음
```

---

## 16. 수동 테스트 체크리스트

### 이동 플랫폼

- [ ] 플랫폼 위에서 가만히 서 있을 수 있음
- [ ] 이동 중 Player가 뒤에 남지 않음
- [ ] 플랫폼 위에서 걷기 가능
- [ ] 이동 중 점프 가능
- [ ] 왕복 방향 전환 시 크게 떨리지 않음

### 회전 플랫폼

- [ ] Player 위치가 회전 플랫폼을 따라감
- [ ] Player 자체 방향은 자동 회전하지 않음
- [ ] 중앙에서 안정적으로 서 있을 수 있음
- [ ] 가장자리에서 이동 가능
- [ ] 장애물과 접촉해도 Player가 혼자 빙글빙글 돌지 않음
- [ ] WASD 이동 시 Player의 정상 방향 회전은 유지

### 스프링 플랫폼

- [ ] 밟는 것만으로 자동 발사되지 않음
- [ ] Space를 눌렀을 때만 강화 점프
- [ ] 일반 점프보다 높게 올라감
- [ ] 기본 1.5배 강화 확인
- [ ] 공중에서 반복 강화되지 않음

### 빙판길

- [ ] 일반 바닥보다 정지 거리가 김
- [ ] 입력 해제 후 천천히 감속
- [ ] 180도 방향 전환이 일반 바닥보다 느림
- [ ] 밀치기 외부 힘 시스템과 충돌하지 않음

### 유령 플랫폼

- [ ] Active 상태 정상
- [ ] Warning에서 깜빡이지 않고 부드럽게 투명해짐
- [ ] Warning 중 Collider 유지
- [ ] 완전히 사라질 때 Collider OFF
- [ ] 위에 있던 Player 정상 추락
- [ ] Hidden 후 재등장
- [ ] 재등장 시 Player 끼임 없음

---

## 17. 자동 테스트 체크리스트

Unity:

```text
Window
→ General
→ Test Runner
→ EditMode
→ Run All
```

확인:

```text
PlatformGimmickTests 전체 Green
기존 Player 이동 테스트 Green
기존 Push 관련 테스트 Green
기존 Phase 3 회귀 테스트 Green
```

추가로 PlayMode에서 Phase4 테스트 Scene을 직접 실행해 실제 Rigidbody 접촉과 플랫폼 탑승 안정성을 확인한다.

---

## 18. 현재 구조

44일차 종료 기준 플랫폼 시스템:

```text
Player
│
├─ PlayerCameraRelativeMovement
│  └─ 일반 이동 / 점프 / 방향 회전
│
├─ PlayerSurfaceInteraction
│  ├─ Spring 감지
│  └─ Ice 감지
│
└─ Rigidbody
   └─ 물리 Rotation X/Y/Z 고정


Platform Systems
│
├─ MovingPlatform
│  └─ PlatformPassengerCarrier
│
├─ RotatingPlatform
│  └─ PlatformPassengerCarrier
│
├─ SpringPlatform
├─ IceSurface
└─ GhostPlatform
```

기능별 컴포넌트를 분리해 이후 다른 맵 Module에서도 같은 기믹을 재사용할 수 있도록 했다.

---

## 19. 개발 결과

44일차에서는 플랫폼·표면 기믹 5종을 한 번에 통합하고 Phase 4 전용 테스트 공간에 배치했다.

최종적으로:

```text
이동 플랫폼
→ 왕복 + 탑승 보정

회전 플랫폼
→ 플랫폼 자체 회전 + 탑승 위치 보정

스프링 플랫폼
→ 직접 점프 시 1회 1.5배 강화

빙판길
→ 강한 관성 + 느린 정지/방향 전환

유령 플랫폼
→ Active → 부드러운 Fade → Hidden → 재등장
```

이 동작하도록 구성했다.

또한 플랫폼과 장애물 접촉으로 Player가 비정상적으로 빙글빙글 회전하는 문제를 물리 Rotation 전체 고정으로 수정했다.

---

## 20. 저장소 검토 메모

GitHub 최신 `main` 커밋은:

```text
4f8b91e94ed91ef5ede01c2a3f92ad2d12fd9a11
```

이며 현재 메시지는:

```text
44
```

이다.

최신 저장소를 정적으로 확인한 결과:

```text
PlayerCameraRelativeMovement
→ Rigidbody FreezeRotation 적용
→ angularVelocity 초기화

Player.prefab
→ Rigidbody Constraints = 112
→ Rotation X/Y/Z 고정

GhostPlatform
→ Runtime Transparent Material
→ Warning 시간 기반 Alpha 감소
→ Hidden 시 Collider OFF
```

가 모두 최신 `main`에 반영되어 있다.

현재 코드 검토 기준으로 44일차 진행을 막는 문제는 확인되지 않았다.

다만 해당 커밋에는 별도의 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 로컬 Unity에서 다음을 확인한 결과를 기준으로 한다.

```text
Console Error 0
EditMode 전체 통과
PlayMode 회귀 테스트 통과
Phase4 5종 기믹 수동 테스트 통과
Player 비정상 회전 재현 안 됨
Ghost Platform Fade 정상
```
