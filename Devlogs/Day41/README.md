# 41일차 개발일지 - 밀치기 힘·쿨타임 및 Phase 4 테스트맵

## 1. 개발 목표

41일차의 핵심 목표는 40일차에서 구현한 밀치기 Target 선택 결과를 실제 물리 반응으로 연결하고, 밀치기 재사용 대기시간과 부활 보호 판정을 추가하는 것이다.

현재 기준 커밋:

```text
a89bdc47435bc76f1a48b352d60a97d472898659
```

현재 커밋 메시지:

```text
41
```

41일차의 기본 흐름:

```text
Push 입력
↓
쿨타임 확인
↓
전방 Target 탐색
↓
가장 가까운 유효 Player 선택
↓
부활 보호 확인
↓
수평 밀치기 적용
↓
쿨타임 시작
```

이번 작업과 함께 Phase 4 동안 반복적으로 사용할 상호작용 전용 테스트 구역도 추가했다.

---

## 2. PlayerPushController 추가

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Push/PlayerPushController.cs
```

주요 책임:

```text
Push Input 연결
Target Selector 호출
밀치기 시도 상태 판정
수평 밀치기 방향 계산
쿨타임 시작 및 갱신
최근 Push 결과 저장
```

기본 설정값:

```text
Horizontal Velocity Change = 6
Cooldown Duration = 1.5초
```

입력은 기존 PlayerInput의 `Push` Action을 사용한다.

---

## 3. Push 시도 결과 분리

새 파일:

```text
Assets/ProjectJ/Runtime/Push/PushAttemptResult.cs
```

밀치기 요청 결과를 다음 상태로 구분한다.

```text
Success
Miss
Cooldown
Protected
InvalidState
MissingReceiver
```

이를 통해 이후 UI, 효과음, 네트워크 판정, 통계 기록에서 밀치기 결과를 명확하게 구분할 수 있는 기반을 마련했다.

---

## 4. PlayerPushReceiver 추가

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Push/PlayerPushReceiver.cs
```

Target Player가 실제 밀치기를 받을 수 있는지 확인하고 Rigidbody 속도를 변경한다.

검사 항목:

```text
Rigidbody 존재
Rigidbody가 Kinematic이 아님
FINISH Player가 아님
부활 보호 상태가 아님
```

유효한 경우에만 밀치기 속도를 적용한다.

---

## 5. 부활 보호와 밀치기 연결

기존 `PlayerRespawnProtection`을 밀치기 적대 효과 판정과 연결했다.

구조:

```text
Target 부활 직후
↓
PlayerRespawnProtection 활성
↓
Push 시도
↓
Protected
↓
속도 변화 없음
```

부활 보호 중에도 이동과 점프는 가능하지만 다른 Player의 밀치기 효과는 받지 않는다.

---

## 6. 밀치기 쿨타임

기본 쿨타임:

```text
1.5초
```

밀치기 시도 시:

```text
쿨타임 없음
→ Push 시도 가능

쿨타임 중
→ Cooldown 반환
→ 추가 밀치기 적용 안 됨
```

이번 구현에서는 Target을 맞히지 못한 `Miss`도 밀치기 시도로 간주해 쿨타임을 소비한다.

따라서 허공에서 Push 입력을 반복해 Target Query를 무제한 실행하는 것을 방지한다.

---

## 7. 밀치기 방향

밀치기 방향은 공격자에서 Target으로 향하는 수평 방향을 기준으로 계산한다.

```text
공격자
A → Target B
```

Target이 공격자의 정면에서 약간 좌우로 벗어나 있더라도 실제 Target 위치 방향으로 밀린다.

수직 방향은 밀치기에 새로 추가하지 않는다.

---

## 8. 수직 튕김 문제 수정

초기 밀치기 테스트에서 Target 캐릭터가 정상적으로 뒤로 밀리는 대신 하늘 방향으로 크게 날아가는 문제가 확인됐다.

원인:

```text
수평 속도 변화
+
추가 Y 속도
```

가 동시에 적용되던 구조였다.

수정 후:

```text
Push가 변경하는 값
→ X / Z 속도만

기존 Y 속도
→ 그대로 유지
```

하도록 변경했다.

즉 점프 중인 Player를 밀쳐도 기존 점프의 Y 속도를 강제로 제거하지 않으면서, 밀치기 자체가 추가적인 상승 속도를 만들지 않는다.

최종 구조:

```text
현재 Velocity
(x, y, z)

Push
↓

(x + pushX,
 y,
 z + pushZ)
```

---

## 9. Player Prefab 연결

Player Prefab에 다음 컴포넌트를 추가했다.

```text
PlayerPushReceiver
PlayerPushController
```

기존의:

```text
PlayerPushTargetSelector
PlayerInput
PlayerFinishState
PlayerRespawnProtection
Rigidbody
```

와 자동으로 연결될 수 있도록 구성했다.

---

## 10. Phase 4 상호작용 테스트맵 구축

Phase 4 동안 Player 간 경쟁 시스템과 장애물 상호작용을 반복 검증할 수 있도록 별도의 테스트맵을 추가했다.

생성 Scene:

```text
Assets/ProjectJ/Tests/Manual/Phase4/
└─ Phase4_InteractionTest.unity
```

Editor Setup:

```text
Assets/ProjectJ/Editor/
└─ Phase4InteractionTestMapSetup.cs
```

Unity 메뉴:

```text
ProjectJ
→ Phase4
→ Build Interaction Test Map
```

기존 Day37 테스트 Scene을 기반으로 복사하고, 기존 구역 한쪽 벽을 제거한 뒤 Phase 4 전용 구역을 연결한다.

---

## 11. Phase 4 테스트 구역 구성

전체 개념:

```text
기존 테스트 구역
↓
연결 벽 개방
↓
Bridge
↓
Phase 4 Interaction Test Area
```

주요 테스트 구역:

```text
PASS THROUGH
NEAREST TARGET
PUSH / COOLDOWN
RESPAWN PROTECTED
PUSH EDGE / DROP
```

### PASS THROUGH

39일차의 Player ↔ Player 일반 충돌 무시를 확인한다.

```text
Player
↓
Dummy Player
↓
서로 밀려나지 않고 통과
```

### NEAREST TARGET

40일차의 밀치기 Target 선택을 검증한다.

배치:

```text
Near
Far
Angle Outside
Range Outside
```

전방 범위 안에서 가장 가까운 Player 한 명만 선택되는지 확인한다.

### PUSH / COOLDOWN

41일차의 실제 밀치기와 쿨타임을 반복 테스트한다.

```text
LMB
↓
Target 밀림
↓
1.5초 내 재입력
↓
추가 Push 차단
```

### RESPAWN PROTECTED

부활 보호 상태의 Target에게 밀치기가 적용되지 않는지 확인한다.

테스트 전용:

```text
Phase4ProtectedTargetLoop
```

를 이용해 해당 Dummy를 계속 보호 상태로 유지한다.

### PUSH EDGE / DROP

낭떠러지 근처에서 실제 밀치기 방향과 낙하 결과를 확인할 수 있도록 개방된 Edge 구역을 구성했다.

---

## 12. Phase4ProtectedTargetLoop 추가

새 테스트 전용 파일:

```text
Assets/ProjectJ/Tests/Manual/Phase4/
└─ Phase4ProtectedTargetLoop.cs
```

역할:

```text
보호 Target의 PlayerRespawnProtection 확인
↓
보호 종료
↓
즉시 다시 보호 시작
```

이를 통해 수동 테스트 중 매번 Respawn을 반복하지 않아도 보호 상태의 밀치기 차단을 확인할 수 있다.

게임 Runtime 기능이 아니라 Manual Test 전용 보조 컴포넌트다.

---

## 13. 자동 테스트

새 테스트:

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlayerPushControllerTests.cs
```

검증 내용:

```text
밀치기 성공
Target Rigidbody 속도 변경
밀치기 후 쿨타임 시작
Miss 시 쿨타임 시작
쿨타임 중 두 번째 Push 차단
쿨타임 종료 후 재사용
부활 보호 Target 거부
FINISH한 실행자의 Push 차단
Target 방향 기반 수평 벡터 계산
밀치기 Y 속도 추가 없음
```

특히 수직 튕김 수정 이후에는 밀치기 방향 계산 결과의 Y가 `0`인지 검증하도록 테스트를 보강했다.

---

## 14. 생성·수정 요소

### 주요 생성 파일

```text
Assets/ProjectJ/Runtime/Push/
├─ PlayerPushController.cs
├─ PlayerPushReceiver.cs
└─ PushAttemptResult.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerPushControllerTests.cs

Assets/ProjectJ/Editor/
└─ Phase4InteractionTestMapSetup.cs

Assets/ProjectJ/Tests/Manual/Phase4/
├─ Phase4ProtectedTargetLoop.cs
└─ Phase4_InteractionTest.unity
```

Phase 4 테스트 Scene용 Material 및 `.meta` 파일도 함께 추가했다.

### 수정

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

Player에 Push Receiver와 Push Controller를 연결했다.

### 삭제

```text
없음
```

---

## 15. 현재 밀치기 구조

41일차 종료 기준:

```text
LMB / Push Action
↓
PlayerPushController
↓
Cooldown 검사
↓
PlayerPushTargetSelector
↓
전방 최근접 Player
↓
PlayerPushReceiver
↓
Finish / Respawn Protection 검사
↓
수평 Velocity Change 적용
```

평상시에는 Player끼리 서로 통과하지만 Push를 입력한 순간에만 별도 게임플레이 효과로 Target을 밀어낸다.

---

## 16. 이번 일차에서 구현하지 않은 기능

41일차에서는 단일 Push와 쿨타임까지만 구현한다.

아직 구현하지 않은 기능:

```text
동시 다중 Push 힘 합산
외부 힘 Queue
여러 공격자의 동시 밀치기
다른 장애물 Force와의 통합
서버 Authority 판정
네트워크 동기화
밀치기 애니메이션
밀치기 효과음
밀치기 기록 통계
```

다음 개발 단계에서 외부 힘 합산 구조를 별도로 정리한다.

---

## 17. 수동 테스트 체크리스트

- [ ] Console Error 0
- [ ] LMB Push 입력 정상
- [ ] 정면 Target 한 명 밀림
- [ ] Target이 위로 솟아오르지 않음
- [ ] 점프 중 Target의 기존 Y 속도는 유지
- [ ] 1.5초 내 재입력 차단
- [ ] 1.5초 후 다시 Push 가능
- [ ] Target 없음 → Miss
- [ ] Respawn 보호 Target → Protected
- [ ] FINISH Player가 밀치기 대상에서 제외
- [ ] PASS THROUGH 구역에서 Player 길막 없음
- [ ] NEAREST TARGET 구역에서 최근접 대상 선택 정상
- [ ] PUSH EDGE 구역에서 수평 밀치기 방향 정상
- [ ] 기존 Phase 3 기능 회귀 오류 없음

---

## 18. 자동 테스트 체크리스트

Unity:

```text
Window
→ General
→ Test Runner
→ EditMode
→ Run All
```

확인:

- [ ] `PlayerPushControllerTests` 전체 Green
- [ ] `PlayerPushTargetSelectorTests` 전체 Green
- [ ] `PlayerCollisionRulesTests` 전체 Green
- [ ] 기존 EditMode 테스트 전체 Green
- [ ] PlayMode 기존 테스트 Green

---

## 19. 저장소 검토 메모

GitHub 최신 커밋 기준으로 41일차 Runtime Push 구조, Player Prefab 연결, EditMode 테스트, Phase 4 Manual Test Scene과 Setup Tool이 반영되어 있다.

정적 코드 검토 기준으로 41일차 목표를 막는 기능상 문제는 확인되지 않았다.

수직 튕김 문제도 현재 Runtime 경로에서는 수정되어 있다.

```text
PlayerPushController
→ 수직 밀치기 성분을 계산 결과에 넣지 않음

PlayerPushReceiver
→ 기존 Rigidbody Y 속도를 그대로 보존
```

Player Prefab의 직렬화 데이터에는 이전 `upwardVelocityChange = 1.5` 값이 남아 있을 수 있지만, 현재 Runtime 계산과 Receiver가 수직 성분을 사용하지 않기 때문에 실제 Push 동작에는 반영되지 않는다. 이후 Prefab을 Unity에서 저장할 때 0으로 정리하면 설정값과 코드 상태도 일치한다.

해당 최신 커밋에는 별도 CI 상태가 등록되어 있지 않다.

따라서 최종 완료 판정은 로컬 Unity에서:

```text
Console Error 0
EditMode 전체 통과
PlayMode 회귀 테스트 통과
Phase4 수동 Push 테스트 통과
```

를 확인한 결과를 기준으로 한다.
