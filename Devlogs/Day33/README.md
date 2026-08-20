# 33일차 개발일지 - 체크포인트 부활 시스템

## 1. 개발 목표

33일차의 목표는 30~32일차에서 구현한 체크포인트 저장과 추락 감지 시스템을 실제 부활 동작으로 연결하는 것이다.

플레이어가 Fall Limit 아래로 추락하거나 직접 부활을 요청하면 현재까지 도달한 가장 높은 체크포인트의 RespawnPoint로 이동한다. 아직 체크포인트를 활성화하지 않았다면 게임 시작 시 저장한 START 위치로 돌아간다.

현재 기준 커밋:

```text
d5063f6640930c385d73eeabe77714a636739541
```

현재 커밋 메시지:

```text
33
```

---

## 2. 부활 처리 흐름

33일차의 기본 흐름은 다음과 같다.

```text
추락 감지 또는 직접 부활 요청
↓
PlayerRespawnController
↓
저장된 RespawnPosition / RespawnRotation 확인
↓
Rigidbody 속도 초기화
↓
저장 위치와 회전으로 이동
↓
Fall 상태 초기화
↓
Respawn 완료
```

추락과 직접 부활은 서로 다른 시스템을 만들지 않고 동일한 Respawn 로직을 사용한다.

---

## 3. PlayerRespawnController

새 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Checkpoint/PlayerRespawnController.cs
```

Player 오브젝트에 추가되는 부활 전용 컴포넌트다.

필수 컴포넌트:

```text
Rigidbody
PlayerCheckpointTracker
PlayerFallTracker
```

주요 역할:

- PlayerFallTracker의 Fell Event 구독
- 추락 발생 시 자동 Respawn
- 직접 Respawn 요청 처리
- 최고 체크포인트의 위치와 회전으로 이동
- Rigidbody 이동 속도 초기화
- Rigidbody 회전 속도 초기화
- 추락 상태 초기화
- Respawn 횟수 기록
- Respawn 완료 Event 발생

---

## 4. 최고 체크포인트를 부활 위치로 사용

부활 위치는 기존 PlayerCheckpointTracker가 저장하고 있는 데이터를 사용한다.

```text
RespawnPosition
RespawnRotation
CurrentCheckpointId
```

따라서 별도로 체크포인트를 다시 검색하지 않는다.

예:

```text
CP3 활성화
↓
CP1 재접촉
↓
최고 Checkpoint = CP3 유지
↓
추락
↓
CP3 RespawnPoint로 복귀
```

31일차에서 구현한 최고 체크포인트 유지 규칙을 그대로 활용한다.

---

## 5. START 부활

아직 CP1~CP4를 하나도 활성화하지 않은 경우에도 별도의 예외용 RespawnPoint를 만들지 않는다.

PlayerCheckpointTracker의 CaptureStartPoint()가 게임 시작 위치와 방향을 이미 저장하고 있으므로 다음과 같이 처리한다.

```text
CurrentCheckpointId = Start
↓
추락 또는 직접 Respawn
↓
게임 시작 위치로 복귀
```

---

## 6. 위치와 회전 복구

Respawn 시 다음 두 값을 함께 복구한다.

```text
Position
Rotation
```

사용하는 데이터:

```text
PlayerCheckpointTracker.RespawnPosition
PlayerCheckpointTracker.RespawnRotation
```

따라서 추락 중 Player가 다른 방향을 보고 있거나 회전한 상태라도 저장된 RespawnPoint가 정한 방향으로 돌아간다.

---

## 7. Rigidbody 물리 상태 초기화

위치만 이동시키면 추락 직전의 속도가 남아 바로 다시 떨어질 수 있으므로 Respawn 전에 Rigidbody의 물리 속도를 제거한다.

초기화 대상:

```text
linearVelocity = Vector3.zero
angularVelocity = Vector3.zero
```

현재 프로젝트가 Unity 6의 Rigidbody.linearVelocity를 사용하고 있으므로 동일한 방식으로 처리한다.

현재 별도의 외부 힘 누적 시스템은 없기 때문에 33일차에서는 Rigidbody에 남은 선형 속도와 회전 속도를 초기화하는 범위까지 구현한다.

---

## 8. Fall Event 자동 부활 연결

32일차 PlayerFallTracker는 Fall Limit 아래로 내려간 순간:

```text
Fell
```

Event를 발생시킨다.

PlayerRespawnController는 활성화될 때 이 Event를 구독한다.

```text
PlayerFallTracker.Fell
↓
PlayerRespawnController.HandleFell()
↓
RespawnToSavedPoint()
```

비활성화될 때는 Event 구독을 해제하여 중복 호출을 방지한다.

---

## 9. 직접 부활 요청

추락하지 않은 상태에서도 동일한 부활 로직을 실행할 수 있도록:

```text
RequestRespawn()
```

을 제공한다.

직접 Respawn과 Fall Respawn의 최종 처리 함수는 동일하다.

```text
RequestRespawn()
→ RespawnToSavedPoint()

Fell Event
→ RespawnToSavedPoint()
```

향후 ESC 메뉴의 직접 부활 기능도 이 메서드에 연결할 수 있다.

---

## 10. Fall 상태 Reset

Respawn 이동이 끝난 뒤:

```text
PlayerFallTracker.ResetFallenState()
```

를 호출한다.

최종 상태:

```text
IsFallen = false
```

이후 다시 Fall Limit 아래로 떨어지면 새로운 추락으로 정상 감지할 수 있다.

Reset은 Player를 Respawn 위치로 이동시킨 뒤 실행한다.

---

## 11. Respawn Event

부활이 정상 완료되면:

```text
Respawned
```

Event를 발생시킨다.

전달 값:

```text
CheckpointId
```

예:

```text
CP4에서 Respawn
→ Respawned(CP4)
```

향후 부활 연출, 사운드, HUD, 3초 보호 시스템 등의 연결 지점으로 사용할 수 있다.

---

## 12. Respawn 횟수

개발 테스트를 위해:

```text
RespawnCount
```

를 기록한다.

부활이 성공할 때마다 1 증가한다.

예:

```text
첫 번째 Respawn → 1
두 번째 Respawn → 2
세 번째 Respawn → 3
```

게임의 최종 통계 데이터가 아니라 현재 부활 동작 검증용 값이다.

---

## 13. RespawnDebugView

새 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Checkpoint/RespawnDebugView.cs
```

화면에 다음 정보를 표시한다.

```text
Respawn Target : 현재 최고 Checkpoint
Respawn Count : 현재 부활 횟수
```

테스트 버튼:

```text
Direct Respawn
Test Fall
```

### Direct Respawn

현재 최고 Checkpoint로 즉시 부활한다.

### Test Fall

현재 Fall Limit보다 아래로 Player를 이동시켜 추락 → 자동 Respawn 과정을 빠르게 검증한다.

---

## 14. Player Prefab 구조

현재 Player의 체크포인트/추락/부활 관련 구조는 다음과 같다.

```text
Player
├─ PlayerCheckpointTracker
├─ PlayerFallTracker
└─ PlayerRespawnController
```

각 책임:

```text
PlayerCheckpointTracker
→ 어디로 부활할지 저장

PlayerFallTracker
→ 언제 추락했는지 감지

PlayerRespawnController
→ 실제 부활 수행
```

기능별 책임을 분리해 이후 네트워크 권한이나 부활 보호 시스템을 추가하기 쉽게 구성했다.

---

## 15. Editor 자동 설정

새 Editor 스크립트:

```text
Assets/ProjectJ/Editor/Day33RespawnSetup.cs
```

Unity 메뉴:

```text
ProjectJ
→ Day33
→ Setup Checkpoint Respawn
```

실행 시 다음을 자동 처리한다.

```text
Player.prefab 확인
↓
Rigidbody 확인
↓
PlayerCheckpointTracker 확인
↓
PlayerFallTracker 확인
↓
PlayerRespawnController 추가 및 연결
↓
Day25 고정맵 Respawn Debug 설정
↓
Day33 테스트 Scene 생성
```

---

## 16. Day33 수동 테스트 Scene

생성 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day33/
└─ Day33_CheckpointRespawnTest.unity
```

Day32 테스트 Scene을 기반으로 생성한다.

대표 테스트:

```text
START
→ Test Fall
→ START 복귀

CP2 접촉
→ Test Fall
→ CP2 복귀

CP3 접촉
→ CP1 재접촉
→ Test Fall
→ CP3 복귀

Direct Respawn
→ 현재 최고 CP로 즉시 복귀
```

---

## 17. EditMode 자동 테스트

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/PlayerRespawnControllerTests.cs
```

주요 검증 항목:

- 체크포인트가 없을 때 START로 복귀
- START 위치와 회전 복구
- CP3 활성화 후 CP3로 복귀
- 낮은 Checkpoint가 Respawn Target을 덮어쓰지 않음
- Fall Event 발생 시 자동 Respawn
- Respawn 후 IsFallen이 false로 초기화
- linearVelocity 초기화
- angularVelocity 초기화
- 같은 Checkpoint에서 반복 Respawn 시 항상 같은 위치로 복귀
- Respawned Event에 현재 CheckpointId 전달
- Respawn 횟수 증가

---

## 18. EditMode Fall Event 테스트 수정

최초 테스트에서는 Rigidbody.position을 Fall Limit 아래로 변경한 직후:

```text
EvaluateCurrentPosition()
```

을 호출했다.

EditMode에서는 Rigidbody 위치 변경 직후 Transform 위치의 동기화 시점 차이로 이전 Transform Y를 읽을 수 있어 다음 테스트가 실패했다.

```text
FallEvent_AutomaticallyRespawnsAndResetsFallState
```

실패 결과:

```text
Expected: True
But was: False
```

이는 Runtime 부활 로직의 오류가 아니라 EditMode 테스트의 물리 동기화 방식에 의존한 문제였다.

테스트를 다음 방식으로 수정했다.

```text
EvaluateHeight(-21f)
```

Fall 높이를 직접 입력하여 추락 판정과 Fell Event → Respawn 흐름 자체를 독립적으로 검증하도록 변경했다.

Runtime 코드는 이 수정 과정에서 변경하지 않았다.

---

## 19. 반복 부활 검증

같은 Checkpoint에서 반복 Respawn을 수행해도 매번 동일한 위치와 회전으로 돌아가야 한다.

예:

```text
CP2 저장
↓
Respawn
→ CP2

다시 이동
↓
Respawn
→ CP2

다시 이동
↓
Respawn
→ CP2
```

Respawn 과정에서 저장된 RespawnPosition이나 RespawnRotation을 Player의 현재 위치로 덮어쓰지 않는다.

---

## 20. 생성 및 수정 요소

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Checkpoint/PlayerRespawnController.cs
Assets/ProjectJ/Runtime/Checkpoint/RespawnDebugView.cs
```

새 Editor 파일:

```text
Assets/ProjectJ/Editor/Day33RespawnSetup.cs
```

새 Test 파일:

```text
Assets/ProjectJ/Tests/EditMode/PlayerRespawnControllerTests.cs
```

수정 요소:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
Assets/ProjectJ/Tests/Manual/Day25/Day25_ModuleFixedMap.unity
```

새 수동 테스트 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day33/Day33_CheckpointRespawnTest.unity
```

삭제 파일:

```text
없음
```

---

## 21. 이번 일차에서 구현하지 않은 기능

다음 단계인 34일차에서 별도로 구현할 예정인 기능은 이번 일차에 포함하지 않았다.

```text
3초 부활 보호
밀치기 무효화
적대 아이템 효과 무효화
보호 상태 HUD
보호 VFX
```

33일차의 책임은 다음 범위까지다.

```text
추락/직접 부활 요청
→ 저장된 최고 Checkpoint로 복귀
→ 위치·회전·Rigidbody 물리 상태 초기화
→ Fall 상태 Reset
```

---

## 22. 검증 체크리스트

- [ ] Unity Console Error 0
- [ ] Player Prefab에 PlayerRespawnController 존재
- [ ] START 상태에서 Respawn 시 START 복귀
- [ ] CP1 활성화 후 Respawn 시 CP1 복귀
- [ ] CP3 직접 활성화 후 Respawn 시 CP3 복귀
- [ ] CP3 이후 CP1 접촉 후에도 CP3로 복귀
- [ ] Respawn 후 linearVelocity = 0
- [ ] Respawn 후 angularVelocity = 0
- [ ] Respawn 후 IsFallen = false
- [ ] Direct Respawn 정상 동작
- [ ] Test Fall → 자동 Respawn 정상 동작
- [ ] 반복 Respawn 시 동일 위치·회전 유지
- [ ] Respawn Count 정상 증가
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

## 23. 개발 결과

33일차에서는 체크포인트와 추락 시스템을 실제 부활 동작으로 연결했다.

최종 구조:

```text
PlayerCheckpointTracker
→ 최고 Checkpoint와 Respawn 위치 저장

PlayerFallTracker
→ Fall Limit 아래 추락 감지

PlayerRespawnController
→ 위치·회전 복귀
→ Rigidbody 속도 초기화
→ Fall 상태 Reset
```

최종 흐름:

```text
추락 또는 직접 Respawn
↓
최고 Checkpoint 확인
↓
없으면 START 사용
↓
Rigidbody 속도 제거
↓
저장된 위치·회전 적용
↓
추락 상태 초기화
↓
Respawn 완료
```

GitHub 최신 커밋에는 EditMode에서 발생했던 Fall Event 테스트의 위치 동기화 문제를 피하도록 `EvaluateHeight(-21f)` 수정도 반영되어 있다.

현재 GitHub에는 이 커밋에 대한 별도 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 로컬 Unity에서 수정 후 EditMode 전체 Green, PlayMode 전체 Green, Console Error 0을 확인한 결과를 기준으로 한다.
