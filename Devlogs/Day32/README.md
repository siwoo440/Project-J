# 32일차 개발일지 - 구간별 추락 한계 시스템

## 1. 개발 목표

32일차의 목표는 플레이어가 현재 도달한 최고 체크포인트에 따라 서로 다른 Fall Limit를 사용하고, 해당 한계보다 아래로 떨어졌을 때 추락 상태를 감지하는 시스템을 구현하는 것이다.

현재 기준 커밋:

```text
6e4efbdc209ea2063f9f5425150c41debd607856
```

현재 커밋 메시지:

```text
32
```

이번 일차의 핵심 흐름은 다음과 같다.

```text
현재 최고 Checkpoint 확인
↓
Checkpoint에 대응하는 Fall Limit 선택
↓
Player의 World Y와 비교
↓
Fall Limit 아래로 이동
↓
FALLEN 상태
```

이번 일차에서는 추락 상태의 감지만 구현하며 실제 체크포인트 부활은 다음 단계에서 연결한다.

---

## 2. 체크포인트별 Fall Limit

현재 개발용 Fall Limit는 다음과 같이 설정했다.

```text
START = -20m
CP1   = 180m
CP2   = 380m
CP3   = 580m
CP4   = 780m
```

기존 체크포인트가 약 200m 간격으로 배치되어 있으므로 각 체크포인트보다 약 20m 아래를 임시 추락 기준으로 사용한다.

이 값은 최종 밸런스 값이 아니라 현재 고정맵 기능 검증용 값이다.

---

## 3. CheckpointFallLimitSet

새 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Checkpoint/CheckpointFallLimitSet.cs
```

주요 역할:

- START Fall Limit 저장
- CP1 Fall Limit 저장
- CP2 Fall Limit 저장
- CP3 Fall Limit 저장
- CP4 Fall Limit 저장
- 현재 CheckpointId에 대응하는 Fall Limit 반환
- Fall Limit가 순서대로 상승하는지 검사

현재 Checkpoint에 따른 기준:

```text
Start → StartFallLimitY
CP1   → Cp1FallLimitY
CP2   → Cp2FallLimitY
CP3   → Cp3FallLimitY
CP4   → Cp4FallLimitY
```

---

## 4. PlayerFallTracker

새 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Checkpoint/PlayerFallTracker.cs
```

Player Prefab에 추가되는 컴포넌트다.

주요 역할:

- PlayerCheckpointTracker 참조
- CheckpointFallLimitSet 참조
- 현재 최고 Checkpoint 확인
- 활성 Fall Limit 갱신
- Player World Y와 Fall Limit 비교
- 추락 상태 저장
- 최초 추락 순간 Event 발생
- Respawn 이후 사용할 추락 상태 Reset 기능 제공

기본 판정 구조:

```text
Player Y >= Fall Limit
→ SAFE

Player Y < Fall Limit
→ FALLEN
```

Fall Limit와 정확히 같은 높이는 아직 안전한 상태로 처리한다.

---

## 5. 현재 체크포인트와 Fall Limit 연동

PlayerFallTracker는 기존 PlayerCheckpointTracker의:

```text
CurrentCheckpointId
```

를 사용한다.

예:

```text
CurrentCheckpoint = Start
→ Fall Limit = -20

CurrentCheckpoint = CP1
→ Fall Limit = 180

CurrentCheckpoint = CP3
→ Fall Limit = 580
```

31일차에서 최고 체크포인트가 낮아지지 않도록 구현했기 때문에 Fall Limit 역시 낮은 체크포인트를 다시 밟는다고 이전 값으로 돌아가지 않는다.

예:

```text
CP3 활성화
↓
Fall Limit = 580

CP1 재접촉
↓
CurrentCheckpoint = CP3 유지
↓
Fall Limit = 580 유지
```

---

## 6. 체크포인트 건너뛰기와 Fall Limit

하위 체크포인트를 건너뛰고 높은 체크포인트를 바로 활성화하는 경우에도 즉시 해당 구간의 Fall Limit를 사용한다.

예:

```text
START
↓
CP3 직접 활성화
↓
CurrentCheckpoint = CP3
↓
Fall Limit = 580
```

따라서 체크포인트 건너뛰기 규칙과 구간별 추락 기준이 서로 일관되게 동작한다.

---

## 7. 추락 상태

PlayerFallTracker는 다음 상태를 보관한다.

```text
IsFallen
ActiveFallLimitY
```

정상 상태:

```text
IsFallen = false
```

Fall Limit 아래로 처음 떨어진 순간:

```text
IsFallen = true
```

로 변경한다.

---

## 8. Fell Event

추락이 처음 감지되면:

```text
Fell
```

Event를 한 번 발생시킨다.

예:

```text
SAFE
↓
Fall Limit 아래 진입
↓
Fell Event 1회
↓
FALLEN
```

이미 FALLEN 상태인 동안 계속 아래로 떨어져도 Event를 매 프레임 반복하지 않는다.

이 Event는 이후 실제 Respawn 시스템에서 사용할 수 있다.

---

## 9. ResetFallenState

향후 체크포인트 부활 이후 새로운 추락을 다시 감지할 수 있도록:

```text
ResetFallenState()
```

를 준비했다.

호출하면:

```text
IsFallen = false
```

로 돌아가고 현재 체크포인트에 맞는 Fall Limit를 다시 계산한다.

32일차에서는 Reset 기능을 테스트만 하며 실제 Respawn 과정에는 아직 연결하지 않는다.

---

## 10. FallLimitDebugView

새 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Checkpoint/FallLimitDebugView.cs
```

현재 상태를 화면에서 확인할 수 있도록 다음 정보를 표시한다.

```text
Fall Check : SAFE / FALLEN
Checkpoint : 현재 Checkpoint
Player Y : 현재 높이
Fall Limit Y : 현재 추락 한계
```

예:

```text
Fall Check : SAFE
Checkpoint : CP2
Player Y : 401.20
Fall Limit Y : 380.00
```

추락 후:

```text
Fall Check : FALLEN
```

으로 변경된다.

Debug 글자는 기존 개발 UI와 동일하게 검은색으로 표시한다.

---

## 11. Fall Limit Marker

고정맵에는 다음 기준 Transform을 생성한다.

```text
=== Fall Limits ===
├─ FallLimit_START
├─ FallLimit_CP1
├─ FallLimit_CP2
├─ FallLimit_CP3
└─ FallLimit_CP4
```

각 Transform은 해당 Fall Limit의 실제 World Y 위치에 배치된다.

따라서 Scene View에서도 각 구간의 추락 기준 높이를 확인할 수 있다.

---

## 12. Player Prefab 수정

대상:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

기존:

```text
PlayerCheckpointTracker
```

에 이어 다음 컴포넌트를 추가했다.

```text
PlayerFallTracker
```

따라서 현재 체크포인트 관련 Player 구조는 다음과 같다.

```text
Player
├─ PlayerCheckpointTracker
└─ PlayerFallTracker
```

---

## 13. Editor 자동 설정

새 Editor 스크립트:

```text
Assets/ProjectJ/Editor/Day32FallLimitSetup.cs
```

Unity 메뉴:

```text
ProjectJ
→ Day32
→ Setup Section Fall Limits
```

실행 시 다음 작업을 수행한다.

```text
Player.prefab에 PlayerFallTracker 추가
↓
Day25 고정맵 열기
↓
CheckpointFallLimitSet 생성
↓
FallLimit_START ~ CP4 Marker 생성
↓
PlayerFallTracker와 Fall Limit 연결
↓
FallLimitDebugView 생성
↓
Day32 테스트 Scene 생성
```

---

## 14. Day32 수동 테스트 Scene

생성 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day32/
└─ Day32_FallLimitTest.unity
```

기존 Day30 체크포인트 테스트 Scene을 기반으로 생성된다.

빠른 수동 테스트를 위해 Day32 테스트 Scene에서는 다음 작은 Fall Limit 값을 사용한다.

```text
START = -5
CP1   = -4
CP2   = -3
CP3   = -2
CP4   = -1
```

이를 통해 같은 테스트 Scene 안에서 체크포인트 전후의 Fall Limit 변화를 쉽게 확인할 수 있다.

---

## 15. EditMode 테스트

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/PlayerFallTrackerTests.cs
```

주요 검증 항목:

- START / CP1 / CP2 / CP3 / CP4 Fall Limit 반환
- Fall Limit가 순차적으로 상승하는지 확인
- 정확히 Fall Limit 높이에서는 추락하지 않는지 확인
- Fall Limit보다 아래에서 추락하는지 확인
- 하위 CP를 건너뛰고 CP3 활성화 시 CP3 Fall Limit 사용
- CP3 이후 CP1 접촉 시 CP3 Fall Limit 유지
- 같은 Player Y도 Checkpoint 전후 기준에 따라 SAFE/FALLEN이 달라지는지 확인
- Fell Event가 추락당 한 번만 발생하는지 확인
- ResetFallenState 후 새로운 추락을 다시 감지할 수 있는지 확인

---

## 16. 대표 동작 예시

### START 상태

```text
CurrentCheckpoint = Start
Fall Limit = -20

Player Y = -10
→ SAFE

Player Y = -20
→ SAFE

Player Y = -20.01
→ FALLEN
```

### CP2 상태

```text
CurrentCheckpoint = CP2
Fall Limit = 380

Player Y = 400
→ SAFE

Player Y = 379
→ FALLEN
```

### CP3 이후 낮은 CP 재접촉

```text
CP3 활성화
→ Fall Limit 580

CP1 접촉
→ CP3 유지
→ Fall Limit 580 유지
```

---

## 17. 생성 및 수정 요소

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Checkpoint/CheckpointFallLimitSet.cs
Assets/ProjectJ/Runtime/Checkpoint/PlayerFallTracker.cs
Assets/ProjectJ/Runtime/Checkpoint/FallLimitDebugView.cs
```

새 Editor 파일:

```text
Assets/ProjectJ/Editor/Day32FallLimitSetup.cs
```

새 Test 파일:

```text
Assets/ProjectJ/Tests/EditMode/PlayerFallTrackerTests.cs
```

수정 요소:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
Assets/ProjectJ/Tests/Manual/Day25/Day25_ModuleFixedMap.unity
```

새 테스트 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day32/Day32_FallLimitTest.unity
```

삭제 파일:

```text
없음
```

---

## 18. 이번 일차에서 구현하지 않은 기능

32일차에서는 다음 기능을 아직 구현하지 않았다.

```text
FALLEN 후 자동 이동
Checkpoint Respawn
Rigidbody 속도 초기화
외부 힘 초기화
직접 부활 입력
3초 보호 상태
```

이번 일차의 책임은 오직:

```text
현재 최고 Checkpoint에 맞는 Fall Limit 선택
+
추락 상태 정확히 감지
```

까지다.

---

## 19. 수동 검증 체크리스트

- [ ] Unity Console Error 0
- [ ] Player Prefab에 PlayerFallTracker 존재
- [ ] Day25 고정맵에 CheckpointFallLimitSet 존재
- [ ] FallLimit_START ~ CP4 Marker 존재
- [ ] START 상태에서 START Fall Limit 사용
- [ ] CP1 활성화 후 CP1 Fall Limit 사용
- [ ] CP3 직접 활성화 후 CP3 Fall Limit 사용
- [ ] CP3 이후 CP1 접촉 시 CP3 Fall Limit 유지
- [ ] Fall Limit와 같은 높이는 SAFE
- [ ] Fall Limit 아래에서는 FALLEN
- [ ] Fell Event 중복 발생 없음
- [ ] Reset 후 다시 추락 감지 가능
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

## 20. 개발 결과

32일차에서는 현재 플레이어의 최고 체크포인트를 기준으로 서로 다른 추락 한계를 적용하는 구간별 Fall Limit 시스템을 구현했다.

최종 흐름:

```text
최고 Checkpoint
↓
구간별 Fall Limit 결정
↓
Player Y 비교
↓
한계 아래 진입
↓
FALLEN
↓
Fell Event 1회
```

이제 다음 Respawn 단계에서는 `Fell` 신호를 받아 기존 PlayerCheckpointTracker가 저장하고 있는 RespawnPosition과 RespawnRotation으로 플레이어를 복귀시키는 구조로 확장할 수 있다.

GitHub 저장소에는 현재 이 커밋에 대한 별도 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 Unity 로컬 환경에서 EditMode / PlayMode 테스트와 Console Error 0을 확인한 뒤 확정한다.
