# 36일차 개발일지 - 개인 결과 데이터

## 1. 개발 목표

36일차의 목표는 경기 종료 시점에 플레이어의 개인 경기 기록을 하나의 결과 데이터로 묶어 이후 결과 UI, 관전, 통계 시스템에서 재사용할 수 있도록 만드는 것이다.

정상 도달 또는 시간 종료 시 다음 데이터를 하나의 Snapshot으로 생성한다.

```text
PlayerId
FinalRank
IsFinished
FinishOrder
FinishTime
HighestHeight
HighestHeightCentimeters
HighestCheckpoint
```

현재 기준 커밋:

```text
bd25038a637822f726970b4ea4f12790894f1cb7
```

현재 커밋 메시지:

```text
36
```

---

## 2. 개인 결과 데이터 구조

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Results/PlayerMatchResult.cs
```

PlayerMatchResult는 경기 결과 생성 시점의 값을 저장하는 개인 결과 Snapshot이다.

주요 값:

```text
PlayerId
FinalRank
IsFinished
FinishOrder
FinishTime
HighestHeightCentimeters
HighestCheckpoint
```

HighestHeight는 HighestHeightCentimeters를 미터 단위로 변환해 제공한다.

미완주 Player는 FinishTime 대신:

```text
NoFinishTime = -1
```

을 사용한다.

---

## 3. Snapshot 방식

결과 데이터는 실시간 Player 컴포넌트를 계속 참조하지 않는다.

결과가 생성되는 순간 각 시스템의 값을 복사한다.

```text
결과 생성
↓
PlayerMatchResult 생성
↓
값 고정
```

따라서 결과 생성 이후 Player의 현재 순위, 높이, 체크포인트가 변해도 이미 생성된 결과는 변경되지 않는다.

---

## 4. PlayerMatchResultCollector

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Results/PlayerMatchResultCollector.cs
```

Player 오브젝트에 추가되는 개인 결과 수집 컴포넌트다.

필요한 기존 시스템:

```text
PlayerFinishState
PlayerRankingParticipant
PlayerHeightTracker
PlayerCheckpointTracker
MatchTimer
```

각 시스템에서 결과에 필요한 값을 읽어 PlayerMatchResult를 생성한다.

---

## 5. 정상 도달 결과 생성

35일차의 PlayerFinishState는 정상 도달 시:

```text
IsFinished
FinishOrder
FinishTime
```

을 확정한다.

PlayerMatchResultCollector는 PlayerFinishState.Finished Event를 구독한다.

처리 흐름:

```text
FINISH Trigger 접촉
↓
PlayerFinishState.TryConfirmFinish()
↓
Finished Event
↓
PlayerMatchResultCollector.TryCreateResult()
↓
PlayerMatchResult 생성
```

정상 도달 Player의 FinalRank는 실시간 CurrentRank가 아니라:

```text
FinishOrder
```

를 사용한다.

---

## 6. 시간 종료 결과 생성

PlayerMatchResultCollector는 MatchTimer.TimeExpired Event도 구독한다.

처리 흐름:

```text
MatchTimer 종료
↓
TimeExpired Event
↓
PlayerMatchResultCollector
↓
미완주 개인 결과 생성
```

정상에 도달하지 못한 Player는:

```text
IsFinished = false
FinishOrder = 0
FinishTime = -1
FinalRank = 시간 종료 시 CurrentRank
```

형태로 저장한다.

---

## 7. 최고 높이 기록

개인 결과의 최고 높이는 기존:

```text
PlayerHeightTracker.HighestHeightCentimeters
```

값을 사용한다.

결과 생성 직전에:

```text
PlayerHeightTracker.RefreshHeight()
```

를 호출해 현재 위치가 최고 높이를 갱신해야 하는 상황도 반영한다.

PlayerMatchResult에서는:

```text
HighestHeightCentimeters
HighestHeight
```

두 형태로 조회할 수 있다.

---

## 8. 최고 체크포인트 기록

개인 결과의 최고 체크포인트는 기존:

```text
PlayerCheckpointTracker.CurrentCheckpointId
```

를 사용한다.

31일차에서 낮은 체크포인트 재접촉이 최고 체크포인트를 덮어쓰지 않도록 구현했기 때문에 이 값은 결과 데이터의 HighestCheckpoint로 그대로 사용할 수 있다.

예:

```text
CP3 활성화
↓
CP1 재접촉
↓
CurrentCheckpointId = CP3 유지
↓
개인 결과 HighestCheckpoint = CP3
```

---

## 9. 최종 순위 결정

정상 도달 Player:

```text
FinalRank = FinishOrder
```

미완주 Player:

```text
FinalRank = PlayerRankingParticipant.CurrentRank
```

따라서 정상 도달 이후 실시간 높이 순위와 결과 순위가 다시 섞이지 않는다.

---

## 10. 결과 중복 생성 방지

PlayerMatchResultCollector에는:

```text
HasResult
```

상태를 둔다.

첫 생성:

```text
HasResult = false
↓
TryCreateResult()
↓
PlayerMatchResult 생성
↓
HasResult = true
```

이후 다시 호출하면:

```text
false 반환
```

하고 기존 CurrentResult를 유지한다.

따라서 다음과 같은 상황에서도 결과는 한 번만 만들어진다.

```text
FINISH로 결과 생성
↓
이후 MatchTimer 종료
↓
중복 결과 생성 안 함
```

---

## 11. ResultCreated Event

결과가 정상 생성되면:

```text
ResultCreated
```

Event를 발생시킨다.

이 Event는 이후 다음 기능과 연결할 수 있다.

```text
개인 결과 UI
관전 전환
결과 저장
통계 기록
전적 전송
```

36일차에서는 Event 생성까지만 구현한다.

---

## 12. PlayerMatchResultDebugView

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Results/PlayerMatchResultDebugView.cs
```

개발 테스트용으로 다음 값을 화면에 표시한다.

```text
Personal Result 상태
Player ID
Final Rank
Finished 여부
Finish Order
Finish Time
Highest Height
Highest Checkpoint
```

결과 생성 전:

```text
Personal Result : Waiting
```

결과 생성 후:

```text
Personal Result : CREATED
```

로 변경된다.

---

## 13. 시간 종료 결과 수동 테스트

Debug View에는 테스트용:

```text
Simulate Time End Result
```

버튼을 제공한다.

FINISH에 들어가지 않은 상태에서 버튼을 누르면 현재 기록 기준의 미완주 결과를 생성한다.

예:

```text
Finished : False
Finish Order : 0
Finish Time : --
Final Rank : 현재 순위
```

---

## 14. Player Prefab 수정

Player Prefab에:

```text
PlayerMatchResultCollector
```

를 추가했다.

현재 Player의 관련 구조:

```text
Player
├─ PlayerHeightTracker
├─ PlayerRankingParticipant
├─ PlayerCheckpointTracker
├─ PlayerFallTracker
├─ PlayerRespawnController
├─ PlayerRespawnProtection
├─ PlayerFinishState
└─ PlayerMatchResultCollector
```

---

## 15. Editor 자동 설정

새 Editor 파일:

```text
Assets/ProjectJ/Editor/Day36ResultSetup.cs
```

Unity 메뉴:

```text
ProjectJ
→ Day36
→ Setup Personal Result
```

실행 시 다음을 자동 처리한다.

```text
Player.prefab 확인
↓
선행 컴포넌트 확인
↓
PlayerMatchResultCollector 추가
↓
기존 컴포넌트 참조 연결
↓
Day35 테스트 Scene 복사
↓
Day36 테스트 Scene 생성
↓
PlayerMatchResultDebugView 생성
```

---

## 16. Day36 수동 테스트 Scene

생성 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day36/
└─ Day36_PersonalResultTest.unity
```

Day35 FINISH 테스트 Scene을 기반으로 구성한다.

대표 테스트 1:

```text
FINISH 접촉
↓
IsFinished = true
↓
FinalRank = FinishOrder
↓
FinishTime 기록
↓
개인 결과 생성
```

대표 테스트 2:

```text
FINISH 미접촉
↓
Simulate Time End Result
↓
IsFinished = false
↓
FinalRank = 현재 순위
↓
개인 결과 생성
```

---

## 17. EditMode 자동 테스트

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/PlayerMatchResultTests.cs
```

주요 검증 항목:

- 정상 도달 결과가 Finish 기록과 일치하는지 확인
- 정상 도달 Player의 FinalRank가 FinishOrder를 사용하는지 확인
- 미완주 결과가 현재 Rank를 사용하는지 확인
- 미완주 Player의 FinishOrder가 0인지 확인
- 미완주 Player의 FinishTime이 NoFinishTime인지 확인
- 최고 높이가 결과에 저장되는지 확인
- 최고 체크포인트가 결과에 저장되는지 확인
- Result가 한 번만 생성되는지 확인
- ResultCreated Event가 한 번만 발생하는지 확인
- 결과 생성 이후 높이가 변해도 기존 결과가 유지되는지 확인
- 결과 생성 이후 더 높은 체크포인트를 활성화해도 기존 HighestCheckpoint가 유지되는지 확인

---

## 18. 생성 및 수정 요소

새 Runtime 폴더:

```text
Assets/ProjectJ/Runtime/Results/
```

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Results/PlayerMatchResult.cs
Assets/ProjectJ/Runtime/Results/PlayerMatchResultCollector.cs
Assets/ProjectJ/Runtime/Results/PlayerMatchResultDebugView.cs
```

새 Editor 파일:

```text
Assets/ProjectJ/Editor/Day36ResultSetup.cs
```

새 Test 파일:

```text
Assets/ProjectJ/Tests/EditMode/PlayerMatchResultTests.cs
```

수정 요소:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

새 수동 테스트 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day36/Day36_PersonalResultTest.unity
```

삭제 파일:

```text
없음
```

---

## 19. 이번 일차에서 구현하지 않은 기능

36일차에서는 결과 데이터를 생성하는 기반까지만 구현한다.

아직 구현하지 않은 기능:

```text
개인 결과 UI
캐릭터 모델 숨김
관전 카메라 전환
다른 Player 관전
전체 결과 화면
보상 계산
로비 복귀
서버 전적 저장
```

이후 일차에서 PlayerMatchResult와 ResultCreated Event를 재사용해 연결한다.

---

## 20. 검증 체크리스트

- [ ] Unity Console Error 0
- [ ] Player Prefab에 PlayerMatchResultCollector 존재
- [ ] FINISH 시 개인 결과 자동 생성
- [ ] 정상 도달 Result의 FinalRank = FinishOrder
- [ ] 정상 도달 Result의 FinishTime 일치
- [ ] 시간 종료 Result의 FinalRank = 현재 Rank
- [ ] 미완주 Result의 FinishOrder = 0
- [ ] 미완주 Result의 FinishTime 없음
- [ ] HighestHeight 기록 일치
- [ ] HighestCheckpoint 기록 일치
- [ ] 결과 중복 생성 차단
- [ ] ResultCreated Event 1회
- [ ] 결과 생성 후 Rank 변화에도 결과 유지
- [ ] 결과 생성 후 높이 변화에도 결과 유지
- [ ] 결과 생성 후 체크포인트 변화에도 결과 유지
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

## 21. 개발 결과

36일차에서는 기존 경기 시스템에 흩어져 있던 개인 기록을 하나의 불변 결과 데이터로 묶는 구조를 구현했다.

최종 흐름:

```text
FINISH 또는 시간 종료
↓
PlayerMatchResultCollector
↓
PlayerFinishState
PlayerRankingParticipant
PlayerHeightTracker
PlayerCheckpointTracker
값 수집
↓
PlayerMatchResult Snapshot 생성
↓
HasResult = true
↓
ResultCreated Event
```

이제 이후 개인 결과 화면이나 관전 시스템은 여러 Player 컴포넌트를 직접 조회할 필요 없이 PlayerMatchResult 하나를 기준으로 처리할 수 있다.

GitHub 최신 커밋에는 PlayerMatchResult, PlayerMatchResultCollector, PlayerMatchResultDebugView, Day36 Editor Setup, EditMode 테스트, Player Prefab 연결 및 Day36 수동 테스트 Scene이 포함되어 있다.

GitHub에는 해당 커밋에 대한 별도 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 로컬 Unity에서 EditMode / PlayMode 테스트와 Console Error 0을 확인한 결과를 기준으로 한다.
