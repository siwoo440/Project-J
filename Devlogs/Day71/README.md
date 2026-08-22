# Project J - 71일차 개발 일지

## 개발 기준

70일차 완료 커밋:

```text
49463bda605baf449f635ae2a3b9e523a5d973bd
70일차 : State Authority 부활·3초 보호 및 높이·순위 동기화 구현
```

71일차 작업 커밋:

```text
acc499b06c148eaa0a6797a437298646ff9937e1
71
```

이번 일차에서는 70일차까지 구현한 실시간 높이·순위 시스템 위에 경기 시작부터 종료까지의 네트워크 경기 흐름을 연결했다.

```text
Preparing
↓
3초 Countdown
↓
Playing
↓
10분 Match Timer
↓
FINISH 또는 시간 종료
↓
Final Result
```

---

## 71일차 목표

경기의 시작과 종료를 Client별 로컬 시간에 맡기지 않고 Fusion State Authority 기준으로 동기화한다.

핵심 목표:

```text
3초 시작 카운트다운
10분 경기 타이머
경기 전 입력 잠금
FINISH 접촉 처리
도착 순서 고정
완주자 실시간 높이 경쟁 제외
전원 완주 경기 종료
시간 종료 처리
미완주자 최종 순위 확정
Host / Client 결과 동기화
```

---

## 1. 네트워크 경기 상태 추가

신규 `ProjectJNetworkMatchState.cs`를 생성하여 경기 전체 상태를 구분했다.

```text
Preparing
Countdown
Playing
Finished
```

경기 종료 원인은 다음과 같이 구분한다.

```text
None
AllFinished
TimeExpired
```

경기 상태와 종료 원인은 State Authority가 확정하며 Client는 동기화된 값을 조회한다.

---

## 2. 경기 Coordinator 구조 구현

별도의 Scene NetworkObject를 새로 만들지 않고 현재 활성 Network Player 중 기준 Player를 경기 Coordinator로 사용한다.

Coordinator가 담당하는 항목:

```text
경기 상태
Countdown Timer
Match Timer
경기 종료 원인
전원 완주 확인
시간 종료 확인
최종 결과 확정
```

이를 통해 71일차에서는 추가 Prefab 연결 없이 기존 Network Player 구조를 유지했다.

---

## 3. 2인 접속 후 자동 Countdown

71일차 Host + Client 테스트 기준으로 활성 Player가 2명 이상이면 State Authority가 자동으로 경기를 시작한다.

```text
Player 1 접속
↓
Preparing

Player 2 접속
↓
3초 Countdown 시작
```

개발 테스트를 위해 Coordinator Host에서는 `F5`로 수동 Countdown을 시작할 수 있다.

---

## 4. 3초 Network Countdown

Fusion `TickTimer`를 사용하여 3초 시작 카운트다운을 구현했다.

```text
CountdownSeconds = 3
```

Countdown 상태에서는 모든 Player의 게임 조작을 잠근다.

잠금 대상:

```text
이동
점프
Sprint
Crouch
Push
직접 Respawn
External Force 이동
```

Countdown이 끝나는 Network Tick부터 모든 Player의 입력을 동시에 허용한다.

---

## 5. Network Player 입력 잠금 연결

`ProjectJNetworkPlayer`에서 `ProjectJNetworkExternalGameplay.GameplayInputAllowed`를 확인하도록 연결했다.

게임 진행이 허용되지 않은 상태에서는:

```text
NetworkVerticalVelocity = 0
NetworkIsSprinting = false
현재 Simulation 위치 유지
```

로 처리하여 Countdown이나 결과 확정 이후 Player가 계속 이동하지 않도록 했다.

기존 이동·점프·중력·Sprint·Stamina·Crouch·Prediction·Resimulation 구조는 유지한다.

---

## 6. 10분 경기 타이머 구현

Countdown 종료 후 State Authority가 경기 상태를 `Playing`으로 변경하고 10분 Timer를 시작한다.

```text
MatchDurationSeconds = 600
```

전체 흐름:

```text
Countdown 종료
↓
Playing
↓
TickTimer 600초 시작
↓
0초
↓
TimeExpired
↓
Finished
```

Client별 `Time.time`을 사용하지 않고 Fusion TickTimer를 사용하여 경기 종료 기준을 통일했다.

---

## 7. FINISH 공통 수신 인터페이스 추가

신규 파일:

```text
IFinishReceiver.cs
```

기존 `FinishTrigger`가 Network Player와 기존 오프라인 Player를 모두 처리할 수 있도록 공통 FINISH 접촉 계약을 추가했다.

FINISH Trigger 흐름:

```text
Collider 접촉
↓
상위 MonoBehaviour 검색
↓
IFinishReceiver 존재
↓
ReceiveFinish()
```

Network Player가 아닌 기존 구조에서는 기존 `PlayerFinishState` 방식으로 fallback 처리한다.

---

## 8. State Authority FINISH 확정

Network Player의 FINISH는 State Authority만 확정한다.

FINISH가 유효하려면:

```text
State Authority
Playing 상태
결과 미확정
```

조건을 만족해야 한다.

이미 완주했거나 경기 전·경기 종료 후 FINISH Trigger에 접촉한 경우 중복 처리하지 않는다.

---

## 9. 도착 순서 기반 Final Rank

정상적으로 FINISH에 도달한 Player의 최종 순위는 도착 순서로 즉시 확정한다.

예:

```text
첫 번째 FINISH
→ Final Rank 1

두 번째 FINISH
→ Final Rank 2

세 번째 FINISH
→ Final Rank 3
```

FINISH 처리 시 저장되는 주요 값:

```text
NetworkIsFinished
NetworkResultLocked
NetworkFinalRank
NetworkRaceRank
NetworkFinishElapsedSeconds
```

완주 순간 `NetworkResultLocked`가 활성화되어 이후 높이가 변하더라도 최종 순위가 변경되지 않는다.

---

## 10. FINISH 경과 시간 저장

정상 도달 Player는 경기 시작 후 FINISH까지 걸린 시간을 저장한다.

```text
FinishElapsedSeconds
```

기준:

```text
전체 경기 시간 600초
-
현재 남은 경기 시간
=
FINISH 경과 시간
```

이를 통해 이후 Result UI에서 완주 시간 표시가 가능하도록 준비했다.

---

## 11. 완주자 실시간 높이 경쟁 제외

70일차까지는 모든 활성 Player를 대상으로 현재 높이 순위를 계산했다.

71일차에서는 `NetworkResultLocked`된 Player를 실시간 높이 경쟁에서 제외한다.

```text
미완주 Player
→ 현재 높이 경쟁

완주 Player
→ Final Rank 고정
→ 높이 경쟁 제외
```

따라서 먼저 FINISH한 Player의 순위가 이후 현재 높이에 의해 다시 변경되지 않는다.

---

## 12. 전원 완주 종료

미완주 Player 수가 0명이 되면 Coordinator가 즉시 경기를 종료한다.

```text
모든 Player FINISH
↓
AllFinished
↓
Match State = Finished
```

경기 Timer가 남아 있어도 전원이 완주하면 즉시 종료한다.

---

## 13. 시간 종료 처리

10분 Timer가 0이 되면 State Authority가 `TimeExpired`로 경기를 종료한다.

종료 직전 모든 미완주 Player의 현재 발 높이를 갱신한 뒤 최종 결과를 확정한다.

---

## 14. 미완주자 최종 순위 확정

시간 종료 시 이미 정상 FINISH한 Player의 순위는 유지한다.

미완주 Player는 완주자 뒤에서 현재 높이 순위로 결과를 확정한다.

예:

```text
P1 FINISH → 1위
P2 FINISH → 2위

시간 종료 시

P3 높이 500.00
P4 높이 400.00
P5 높이 400.00

최종 결과

P1 → 1위
P2 → 2위
P3 → 3위
P4 → 4위
P5 → 4위
```

동일 높이는 기존 경쟁 순위 방식대로 공동 순위를 유지한다.

---

## 15. 경기 종료 후 입력 잠금

개인 결과가 확정된 Player는 더 이상 게임 조작을 수행하지 않는다.

차단:

```text
이동
점프
Sprint
Crouch
Push
External Force
Respawn
Checkpoint 갱신
```

또한 남아 있던 External Velocity를 제거하여 결과 확정 후 캐릭터가 계속 밀려나지 않도록 했다.

---

## 16. 개발 테스트 키

71일차 네트워크 테스트를 위해 다음 키를 유지·추가했다.

```text
R
직접 Respawn 테스트

F5
Host Coordinator 수동 Countdown 시작

F6
Host Coordinator 강제 시간 종료
```

실제 게임 UI가 연결되기 전 네트워크 상태를 빠르게 검증하기 위한 개발용 입력이다.

---

## 17. 71일차 디버그 상태

기존 Network Debug 표시에서 다음 경기 정보를 확인할 수 있도록 확장했다.

```text
Match State
Countdown Remaining
Match Time Remaining
Match End Reason

Current Height
Best Height
Race Rank

Finished
Result Locked
Final Rank
Finish Elapsed Time
```

이를 통해 Host와 Client에서 경기 상태와 개인 결과를 비교할 수 있다.

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkExternalGameplay.cs
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Runtime/Finish/
└─ FinishTrigger.cs
```

---

## 생성 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkMatchState.cs
└─ ProjectJNetworkMatchState.cs.meta

Assets/ProjectJ/Runtime/Finish/
├─ IFinishReceiver.cs
└─ IFinishReceiver.cs.meta
```

---

## 삭제 파일

```text
없음
```

---

## 테스트 항목

```text
Host + Client 접속
↓
2인 접속 후 자동 Countdown

Countdown
3 → 2 → 1
↓
두 Player 모두 조작 잠금

Countdown 종료
↓
두 Player 동시에 조작 허용

Playing
↓
10분 Timer 양쪽 동일

Client 먼저 FINISH
↓
Final Rank 1 고정
Result Locked 활성화

Host 다음 FINISH
↓
Final Rank 2 고정

완주 Player
↓
실시간 높이 경쟁 제외
조작 잠금

동일 Player FINISH 재접촉
↓
Final Rank 변경 없음

전원 FINISH
↓
AllFinished
↓
경기 즉시 종료

F6 강제 시간 종료
↓
TimeExpired
↓
미완주자 현재 높이 기준 결과 확정

동점 높이
↓
공동 순위 유지

Host / Client
↓
Match State
Timer
Final Rank
Finish Time
동일

기존 기능
↓
이동
점프
Sprint
Stamina
Crouch
Push
Respawn
Checkpoint
Prediction / Resimulation
정상 유지

Console Error 0
```

---

## 코드 검토 결과

GitHub 최신 커밋 기준으로 다음 연결을 확인했다.

```text
3초 Countdown
→ 구현됨

10분 Match Timer
→ 구현됨

Countdown 입력 잠금
→ Network Player 연결됨

FINISH 공통 인터페이스
→ FinishTrigger 연결됨

State Authority FINISH
→ 구현됨

도착 순서 Final Rank
→ 구현됨

중복 FINISH 차단
→ 구현됨

완주자 높이 경쟁 제외
→ 구현됨

전원 완주 종료
→ 구현됨

시간 종료
→ 구현됨

미완주자 현재 높이 최종 순위
→ 구현됨
```

GitHub 저장소에는 자동 Unity 빌드 CI가 등록되어 있지 않아 컴파일·런타임 성공 여부는 GitHub만으로 확정할 수 없다.

최종 완료 기준:

```text
Unity Console Error 0
+
Host / Client 2인 테스트 통과
```

---

## 71일차 완료 구조

```text
Network Player Spawn
↓
Preparing
↓
3초 Countdown
↓
동시 입력 허용
↓
Playing
↓
10분 Timer
↓
실시간 Height / Rank
↓
FINISH
├─ 도착 순서 Final Rank
└─ 완주자 경쟁 제외
↓
전원 완주 또는 시간 종료
↓
미완주자 높이 결과 확정
↓
Finished
```

---

## 다음 개발 방향

72일차에서는 경기 진행 중 상호작용하는 오브젝트를 State Authority 기준으로 네트워크화한다.

```text
동적 장애물
↓
아이템 상자
↓
아이템 획득 권한
↓
2슬롯 인벤토리
↓
Q / E 슬롯 선택
↓
Host / Client 상태 동기화
```

71일차까지 경기의 시작·진행·종료 기반이 완성되고, 72일차부터 실제 경기 중 아이템과 동적 오브젝트를 네트워크 경기 흐름에 연결한다.
