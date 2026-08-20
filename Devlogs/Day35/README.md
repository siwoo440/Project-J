# 35일차 개발일지 - 정상 도달 및 도착 순서 확정

## 1. 개발 목표

35일차의 목표는 플레이어가 FINISH Trigger에 처음 접촉한 순간 정상 도달을 확정하고, 도착 시각과 도착 순서를 저장하며, 이후 해당 플레이어를 실시간 높이 순위 경쟁에서 제외하는 것이다.

현재 기준 커밋:

```text
78e7b7b32c0ad4594f57b716576ef9388fbe78cf
```

현재 커밋 메시지:

```text
35
```

이번 일차의 핵심 흐름은 다음과 같다.

```text
Player가 FINISH Trigger 접촉
↓
중복 Finish 여부 검사
↓
FinishOrderManager에 등록
↓
도착 순서 확정
↓
도착 시각 저장
↓
높이 순위 경쟁 대상에서 제외
↓
완주 순위 고정
```

---

## 2. PlayerFinishState

새 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Finish/PlayerFinishState.cs
```

Player 오브젝트에 추가되는 완주 상태 전용 컴포넌트다.

주요 저장 값:

```text
IsFinished
FinishOrder
FinishTime
RankingParticipant
```

초기 상태:

```text
IsFinished = false
FinishOrder = 0
```

정상 도달 후:

```text
IsFinished = true
FinishOrder = 확정된 도착 순서
FinishTime = FINISH 판정 시각
```

---

## 3. FinishOrderManager

새 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Finish/FinishOrderManager.cs
```

각 플레이어가 스스로 자신의 도착 순서를 결정하지 않고 중앙에서 순서를 확정하도록 구성했다.

기본 처리:

```text
첫 번째 Player
→ FinishOrder = 1

두 번째 Player
→ FinishOrder = 2

세 번째 Player
→ FinishOrder = 3
```

완주한 플레이어는 내부 Finishers 목록에 기록한다.

현재 단계에서는 실제 서버 권한 대신 로컬 Manager가 순서를 확정한다.

향후 네트워크 구현 시 이 판정 책임을 State Authority로 이전할 수 있다.

---

## 4. FinishTrigger

새 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Finish/FinishTrigger.cs
```

FINISH 영역의 Trigger Collider와 함께 사용한다.

Player Collider가 FINISH Trigger에 들어오면 부모에서:

```text
PlayerFinishState
```

를 찾는다.

그 뒤 현재 로컬 시간:

```text
Time.unscaledTimeAsDouble
```

을 정상 도달 시각으로 전달한다.

기본 흐름:

```text
OnTriggerEnter
↓
PlayerFinishState 검색
↓
FinishOrderManager.TryRegisterFinish()
↓
완주 확정
```

---

## 5. 중복 Finish 차단

같은 플레이어가 FINISH Trigger에 다시 접촉해도 새로운 도착 순서를 받지 않도록 차단했다.

검사 조건:

```text
Player가 null
IsFinished == true
이미 Finishers에 등록됨
```

중 하나라도 해당되면 Finish 요청을 거부한다.

예:

```text
Player A 첫 접촉
→ FinishOrder 1

Player A 재접촉
→ 거부

Player B 첫 접촉
→ FinishOrder 2
```

따라서 같은 플레이어가 여러 도착 순서를 점유하지 않는다.

---

## 6. 정상 도달 시각 저장

PlayerFinishState에는:

```text
FinishTime
```

을 저장한다.

현재 온라인 서버 시간이 아직 연결되지 않았으므로:

```text
Time.unscaledTimeAsDouble
```

을 임시 기준으로 사용한다.

현재 FinishTime의 목적은 순위 계산이 아니라 이후 개인 결과 데이터와 통계에 사용할 정상 도달 기록값을 확보하는 것이다.

실제 최종 순위는 FinishTime이 아니라:

```text
FinishOrder
```

로 고정한다.

---

## 7. 높이 순위 경쟁에서 제외

기존 PlayerRankingParticipant에 다음 상태를 추가했다.

```text
HeightRankingEligible
```

기본값:

```text
true
```

정상 도달 후:

```text
false
```

로 변경한다.

PlayerRankingManager는 HeightRankingEligible이 false인 참가자를 더 이상 높이 비교 입력에 포함하지 않는다.

---

## 8. PlayerRankingParticipant 수정

수정 파일:

```text
Assets/ProjectJ/Runtime/Ranking/PlayerRankingParticipant.cs
```

새 항목:

```text
HeightRankingEligible
SetHeightRankingEligible(bool)
```

정상 도달 전:

```text
HeightRankingEligible = true
```

정상 도달 후:

```text
HeightRankingEligible = false
```

Finish 확정과 동시에 해당 플레이어의 CurrentRank는 FinishOrder로 고정한다.

---

## 9. PlayerRankingManager 수정

수정 파일:

```text
Assets/ProjectJ/Runtime/Ranking/PlayerRankingManager.cs
```

기존에는 등록된 모든 PlayerRankingParticipant의 높이를 그대로 비교했다.

35일차부터는:

```text
HeightRankingEligible == true
```

인 플레이어만 실시간 높이 계산에 포함한다.

완주자가 존재하면 완주 인원 수만큼 현재 경기 중 플레이어의 전체 순위를 뒤로 이동시킨다.

예:

```text
A 완주
→ A = 최종 1위

B, C 경기 중
→ B/C끼리 높이 비교

B가 경기 중 최고 높이
→ 전체 순위 2위

C
→ 전체 순위 3위
```

완주한 A와 경기 중 B가 동시에 1위로 표시되는 상황을 방지한다.

---

## 10. 완주 후 높이가 변해도 순위 고정

정상 도달 Player의 위치가 이후 변하더라도 FinishOrder는 다시 계산되지 않는다.

예:

```text
Player A
→ FinishOrder 1 확정

A의 World Y가 낮아짐
→ 높이 순위 계산 대상 아님
→ FinishOrder 1 유지
```

따라서 완주 결과와 실시간 높이 경쟁을 서로 분리했다.

---

## 11. FinishDebugView

새 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Finish/FinishDebugView.cs
```

수동 테스트를 위해 화면에 다음 정보를 표시한다.

```text
Finish Count
Local Player 상태
Finish Order
Finish Time
```

완주 전:

```text
Local Player : Not Finished
```

완주 후 예:

```text
Finish Count : 1
Local Player : Finished #1 / Time 12.345
```

---

## 12. Player Prefab 수정

Player Prefab에:

```text
PlayerFinishState
```

를 추가했다.

현재 주요 경기 상태 관련 구조는 다음과 같다.

```text
Player
├─ PlayerHeightTracker
├─ PlayerRankingParticipant
├─ PlayerCheckpointTracker
├─ PlayerFallTracker
├─ PlayerRespawnController
├─ PlayerRespawnProtection
└─ PlayerFinishState
```

---

## 13. FINISH Trigger 구성

Day25 고정맵의:

```text
FINISH_1000m
```

을 기준으로 자식 Trigger를 구성한다.

구조:

```text
FINISH_1000m
└─ FinishTrigger
```

FinishTrigger에는:

```text
BoxCollider
FinishTrigger
```

컴포넌트를 사용한다.

BoxCollider는 Trigger 상태로 설정한다.

---

## 14. Editor 자동 설정

새 Editor 스크립트:

```text
Assets/ProjectJ/Editor/Day35FinishSetup.cs
```

Unity 메뉴:

```text
ProjectJ
→ Day35
→ Setup Finish Order
```

실행 시 다음을 자동 처리한다.

```text
Player.prefab 확인
↓
PlayerRankingParticipant 확인
↓
PlayerFinishState 추가 및 연결
↓
Day25 고정맵 FINISH_1000m 검색
↓
FinishTrigger 생성
↓
FinishOrderManager 생성
↓
FinishDebugView 생성
↓
Day35 테스트 Scene 생성
```

---

## 15. Day35 수동 테스트 Scene

생성 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day35/
└─ Day35_FinishOrderTest.unity
```

Day34 테스트 Scene을 기반으로 생성하며 별도의 테스트용:

```text
FINISH_Test
```

영역을 추가한다.

Player가 FINISH_Test에 들어가면 정상 도달이 확정되는지 확인할 수 있다.

---

## 16. EditMode 자동 테스트

새 테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/FinishSystemTests.cs
```

주요 검증 항목:

- 첫 완주 Player가 FinishOrder 1을 받는지 확인
- FinishTime이 전달된 값으로 저장되는지 확인
- 여러 Player가 실제 접촉 순서대로 1, 2, 3을 받는지 확인
- 같은 Player의 중복 Finish가 거부되는지 확인
- 중복 Finish로 FinishCount가 증가하지 않는지 확인
- 정상 도달 Player가 높이 Ranking에서 제외되는지 확인
- 완주자 수만큼 경기 중 Player의 전체 순위가 보정되는지 확인
- 완주 후 World Y가 변해도 FinishOrder가 유지되는지 확인

---

## 17. Editor 폴더 정리

35일차 개발과 함께 과거 테스트 환경을 생성하기 위해 사용했던 오래된 일회성 Editor Setup 도구 일부를 정리했다.

삭제한 Editor 파일:

```text
Assets/ProjectJ/Editor/Day18TestMapEnhancer.cs
Assets/ProjectJ/Editor/Day19SlopeStepMapSetup.cs
Assets/ProjectJ/Editor/Day20LedgeMapSetup.cs
Assets/ProjectJ/Editor/Day22CameraSetup.cs
Assets/ProjectJ/Editor/Day23CameraPolishSetup.cs
```

각 파일의 `.meta`도 함께 삭제했다.

Day25 이후의 Module, Ranking, Match, Checkpoint, Respawn 관련 Editor Setup 파일은 현재 테스트 및 재구성에 사용할 수 있으므로 유지했다.

---

## 18. 생성 및 수정 요소

새 Runtime 폴더:

```text
Assets/ProjectJ/Runtime/Finish/
```

새 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Finish/PlayerFinishState.cs
Assets/ProjectJ/Runtime/Finish/FinishOrderManager.cs
Assets/ProjectJ/Runtime/Finish/FinishTrigger.cs
Assets/ProjectJ/Runtime/Finish/FinishDebugView.cs
```

수정 Runtime 파일:

```text
Assets/ProjectJ/Runtime/Ranking/PlayerRankingParticipant.cs
Assets/ProjectJ/Runtime/Ranking/PlayerRankingManager.cs
```

새 Editor 파일:

```text
Assets/ProjectJ/Editor/Day35FinishSetup.cs
```

새 Test 파일:

```text
Assets/ProjectJ/Tests/EditMode/FinishSystemTests.cs
```

수정 요소:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
Assets/ProjectJ/Tests/Manual/Day25/Day25_ModuleFixedMap.unity
```

새 수동 테스트 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day35/Day35_FinishOrderTest.unity
```

삭제 Editor 파일:

```text
Day18TestMapEnhancer.cs
Day19SlopeStepMapSetup.cs
Day20LedgeMapSetup.cs
Day22CameraSetup.cs
Day23CameraPolishSetup.cs
```

---

## 19. 이번 일차에서 구현하지 않은 기능

35일차에서는 다음 기능을 아직 구현하지 않았다.

```text
개인 결과 데이터
개인 결과 UI
캐릭터 모델 숨김
관전 전환
전체 경기 종료
전체 결과 화면
보상 계산
```

이번 일차의 책임은:

```text
FINISH 접촉
→ 정상 도달 확정
→ 도착 시각 저장
→ 도착 순서 확정
→ 중복 판정 차단
→ 높이 순위 경쟁 제외
```

까지다.

---

## 20. 검증 체크리스트

- [ ] Unity Console Error 0
- [ ] Player Prefab에 PlayerFinishState 존재
- [ ] FINISH_1000m에 FinishTrigger 존재
- [ ] 첫 Player의 FinishOrder = 1
- [ ] 두 번째 Player의 FinishOrder = 2
- [ ] FinishTime 정상 저장
- [ ] 동일 Player 재접촉 시 중복 Finish 거부
- [ ] 중복 접촉 후 FinishCount 변화 없음
- [ ] 완주 Player HeightRankingEligible = false
- [ ] 완주 Player의 최종 순위 유지
- [ ] 경기 중 Player끼리 높이 순위 계속 계산
- [ ] 완주자 수를 반영해 경기 중 Player의 전체 순위 계산
- [ ] 완주 후 Player 높이가 변해도 FinishOrder 유지
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

## 21. 개발 결과

35일차에서는 FINISH Trigger 기반의 정상 도달 판정과 도착 순서 확정 시스템을 구현했다.

최종 흐름:

```text
FINISH Trigger 접촉
↓
중복 여부 검사
↓
FinishOrderManager 등록
↓
FinishOrder 확정
↓
FinishTime 저장
↓
HeightRankingEligible = false
↓
완주 순위 고정
↓
남은 Player만 높이 경쟁 계속
```

이제 정상에 도달한 플레이어는 기존 높이 순위 경쟁과 분리된 확정 결과를 가지며, 아직 완주하지 않은 플레이어만 실시간 높이 순위를 계속 계산한다.

또한 과거 테스트 맵·카메라 생성에 사용했던 오래된 Editor Setup 파일 일부를 정리해 ProjectJ Editor 폴더의 불필요한 개발용 파일을 줄였다.

GitHub에는 해당 커밋에 대한 별도 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 로컬 Unity에서 EditMode / PlayMode 테스트와 Console Error 0을 확인한 결과를 기준으로 한다.
