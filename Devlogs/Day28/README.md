# 28일차 개발일지 - 경기 상태 및 Ready 기반 3·2·1 카운트다운

## 1. 개발 목표

28일차의 목표는 경기 시작 전후 상태를 명확하게 관리하고, 모든 플레이어가 준비된 뒤 동일한 타이밍에 출발할 수 있도록 **Ready 기반 경기 상태 및 카운트다운 시스템**을 만드는 것이다.

현재 기준 커밋:

```text
d0ae6095b1286dcacdf7c98f02664703070574e8
```

현재 커밋 메시지:

```text
28
```

이번 일차의 핵심 흐름은 다음과 같다.

```text
Preparing
↓
모든 플레이어 Ready
↓
안정화 대기
↓
3
↓
2
↓
1
↓
시작!
↓
Playing
↓
Finished
```

---

# 2. 경기 상태 정의

새 경기 상태는 다음 네 단계로 구성한다.

```text
Preparing
Countdown
Playing
Finished
```

각 상태의 역할은 다음과 같다.

| 상태 | 역할 |
|---|---|
| Preparing | 플레이어와 Scene 준비를 기다리는 상태 |
| Countdown | 3 → 2 → 1 카운트다운 진행 상태 |
| Playing | 실제 경기 진행 상태 |
| Finished | 경기 종료 상태 |

상태 전환은 다음 순서만 허용한다.

```text
Preparing
→ Countdown
→ Playing
→ Finished
```

잘못된 역방향 또는 단계 건너뛰기 전환은 허용하지 않는다.

---

# 3. Ready 기반 경기 시작

처음 구현에서는 Scene 시작과 동시에 카운트다운을 시작했지만, 향후 멀티플레이를 고려하면 각 플레이어의 로딩 및 Spawn 완료 시점이 다를 수 있다.

따라서 최종적으로 다음 구조로 수정했다.

```text
Preparing
↓
모든 플레이어 준비 확인
↓
NotifyAllPlayersReady()
↓
Ready 안정화 시간
↓
Countdown
```

현재 오프라인 테스트에서는:

```text
autoReadyInOfflineMode = true
```

를 사용해 자동으로 Ready 신호를 보낸다.

향후 Fusion 네트워크 단계에서는 Host 또는 State Authority가 모든 플레이어 상태를 확인한 뒤 같은 Ready 진입점을 호출할 수 있도록 설계했다.

---

# 4. Ready 안정화 시간

모든 플레이어가 Ready가 되자마자 즉시 카운트다운으로 넘어가지 않고 짧은 안정화 시간을 둔다.

현재 기본값:

```text
Ready Settle Duration = 0.5초
```

흐름:

```text
모든 Player Ready
↓
0.5초 안정화
↓
3 표시 시작
```

이 시간은 향후 다음 상태가 모두 안정적으로 완료된 뒤 경기 시작을 보장하기 위한 여유 구간으로 활용할 수 있다.

```text
Scene Load
Player Spawn
시작 위치 배치
입력 권한 설정
NetworkObject 준비
```

---

# 5. 카운트다운 템포

카운트다운은 각 숫자를 독립된 단계로 처리한다.

기본값:

```text
Countdown Step Duration = 1.25초
```

따라서 실제 흐름은 다음과 같다.

```text
3
1.25초 유지

2
1.25초 유지

1
1.25초 유지

시작!
```

전체 숫자 카운트다운 시간:

```text
1.25 × 3 = 3.75초
```

기존처럼 전체 남은 시간에서 숫자를 역산하는 방식보다 각 단계가 명확하게 분리된다.

---

# 6. 숫자 건너뛰기 방지

초기 구현에서는 첫 프레임 또는 순간적인 프레임 지연으로 인해:

```text
2 → 1 → 시작!
```

또는:

```text
1 → 시작!
```

처럼 첫 숫자가 보이지 않는 문제가 있었다.

이를 해결하기 위해 카운트다운 숫자를:

```text
3
2
1
```

각각 별도의 단계값으로 관리하도록 변경했다.

또한 카운트다운 진입 직후 첫 Update에서는 시간 감소를 하지 않는 보호 로직을 추가했다.

```text
Countdown 진입
↓
3 상태 설정
↓
첫 화면 렌더 기회 확보
↓
그 다음 Update부터 시간 감소
```

따라서 첫 카운트다운 표시는 반드시 `3`부터 시작하도록 구성했다.

---

# 7. 큰 프레임 지연 대응

한 프레임의 Delta Time이 매우 커지더라도 여러 숫자를 한 번에 건너뛰지 않도록 했다.

예:

```text
현재 3
↓
큰 프레임 지연 발생
↓
2까지만 진행

다음 진행
↓
1

다음 진행
↓
Playing
```

즉 카운트다운의 순서 자체를 보장한다.

```text
3 → 2 → 1 → 시작!
```

---

# 8. 입력 잠금

경기 시작 전에는 Player가 먼저 출발할 수 없어야 한다.

따라서:

```text
Preparing
→ Player Input OFF

Countdown
→ Player Input OFF

Playing
→ Player Input ON

Finished
→ Player Input OFF
```

구조로 동작한다.

Player GameObject 자체를 비활성화하는 것이 아니라 `PlayerInput`의 경기 입력만 활성/비활성화한다.

덕분에 Player의 다음 시스템은 계속 유지될 수 있다.

```text
Rigidbody
Collider
HeightTracker
RankingParticipant
Camera 관련 상태
```

---

# 9. 시작 순간

카운트다운이 종료되면:

```text
1
↓
Playing 전환
↓
Player Input ON
↓
시작!
```

순서로 진행한다.

`시작!`이 표시되는 순간 실제 조작도 허용되므로 플레이어가 화면 연출보다 먼저 출발하거나 늦게 출발하는 느낌을 줄인다.

---

# 10. 경기 종료

`Playing` 상태에서는:

```text
FinishMatch()
```

를 통해 경기 종료 상태로 진입할 수 있다.

```text
Playing
↓
FinishMatch()
↓
Finished
↓
Player Input OFF
```

현재 Day28 테스트 화면에는 수동 확인용 `Finish Match` 버튼이 포함되어 있다.

실제 FINISH 지점 판정과 경기 결과 처리는 이후 별도 일차에서 연결한다.

---

# 11. MatchStateController

주요 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Match/
└─ MatchStateController.cs
```

주요 역할:

```text
현재 MatchState 관리
Ready 신호 관리
Ready 안정화 시간 관리
카운트다운 단계 관리
3 → 2 → 1 순서 보장
PlayerInput 활성/비활성화
Playing 진입
Finished 진입
상태 변경 Event 제공
```

주요 진입 함수:

```text
NotifyAllPlayersReady()
CancelReadySignal()
StartCountdown()
AdvanceReadySettle()
AdvanceCountdown()
FinishMatch()
```

---

# 12. MatchStateDebugView

테스트용 화면 표시:

```text
Assets/ProjectJ/Runtime/Match/
└─ MatchStateDebugView.cs
```

현재 테스트 단계에서는 다음 상태를 화면에서 확인할 수 있다.

```text
WAITING
READY
3
2
1
시작!
FINISHED
```

숫자에는 간단한 크기 변화 연출도 포함되어 있다.

이 UI는 정식 경기 HUD가 아니라 기능 확인용 Debug View다.

---

# 13. Editor Setup

Editor 자동 설정 스크립트:

```text
Assets/ProjectJ/Editor/
└─ Day28MatchCountdownSetup.cs
```

메뉴:

```text
ProjectJ
→ Day28
→ Setup Match Countdown
```

현재 설정값:

```text
Countdown Step Duration = 1.25
Ready Settle Duration = 0.5
Auto Ready In Offline Mode = true
```

메뉴를 실행하면 Game Scene에 경기 상태 Controller를 적용하고 Day28 수동 테스트 Scene을 생성한다.

---

# 14. Day28 테스트 Scene

테스트 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day28/
└─ Day28_MatchCountdownTest.unity
```

테스트 흐름:

```text
Play
↓
Preparing / READY
↓
0.5초 안정화
↓
3
↓
2
↓
1
↓
시작!
↓
Player 이동 가능
```

`Finish Match`를 누르면:

```text
Finished
```

상태로 변경되고 Player 입력이 다시 잠긴다.

---

# 15. EditMode 테스트

테스트 파일:

```text
Assets/ProjectJ/Tests/EditMode/
└─ MatchStateControllerTests.cs
```

주요 검증 항목:

```text
새 Controller는 Preparing 상태
Ready 신호만으로 즉시 Countdown하지 않음
Ready 안정화 후 Countdown 시작
Countdown은 반드시 3부터 시작
첫 Countdown Update에서도 3 유지
3 → 2 → 1 → Playing 순서
큰 프레임 지연에도 숫자 건너뛰기 방지
Ready 취소 시 Preparing 유지
Playing 전에는 FinishMatch 불가
Playing 이후 Finished 전환 가능
```

---

# 16. 향후 네트워크 연결 구조

현재 오프라인에서는:

```text
autoReadyInOfflineMode
↓
NotifyAllPlayersReady()
```

방식으로 동작한다.

향후 Fusion Host Mode에서는 서버 권한 측에서 다음 조건을 확인한다.

```text
모든 참가자 Session 입장 완료
↓
모든 Client Game Scene 로드 완료
↓
모든 Player NetworkObject Spawn 완료
↓
시작 위치 배치 완료
↓
Input Authority 설정 완료
↓
각 Player Ready 상태 확인
↓
전원 Ready
↓
NotifyAllPlayersReady()
↓
0.5초 안정화
↓
3 → 2 → 1 → 시작!
```

이 구조를 사용하면 참가자가 늘어나도 가장 늦게 준비된 플레이어를 기준으로 공정하게 경기를 시작할 수 있다.

---

# 17. 현재 하지 않은 기능

28일차에는 아직 다음 기능을 구현하지 않았다.

```text
실제 Fusion Ready 동기화
Host / State Authority 카운트다운 동기화
경기 제한 시간
30초 / 10초 경고
체크포인트
추락 및 Respawn
FINISH Trigger
완주 순위 고정
경기 결과 UI
```

이번 일차는 **경기 상태와 공정한 시작 시점 관리**에 집중한다.

---

# 18. 생성 및 수정 요소

새 Runtime 영역:

```text
Assets/ProjectJ/Runtime/Match/
```

주요 파일:

```text
MatchState.cs
MatchStateController.cs
MatchStateDebugView.cs
```

Editor:

```text
Assets/ProjectJ/Editor/Day28MatchCountdownSetup.cs
```

Test:

```text
Assets/ProjectJ/Tests/EditMode/MatchStateControllerTests.cs
Assets/ProjectJ/Tests/Manual/Day28/Day28_MatchCountdownTest.unity
```

Game Scene에도 `MatchStateController`가 적용된다.

---

# 19. 수동 검증 체크리스트

- [ ] Scene 시작 후 즉시 Playing이 되지 않음
- [ ] Preparing 상태 존재
- [ ] Ready 신호 후 안정화 시간이 존재
- [ ] 첫 숫자는 반드시 3
- [ ] `3 → 2 → 1 → 시작!` 순서 유지
- [ ] 각 숫자가 약 1.25초 유지
- [ ] Countdown 중 Player 이동 불가
- [ ] Countdown 중 Jump 불가
- [ ] `시작!`과 함께 Player 조작 활성화
- [ ] Finished 이후 Player 조작 비활성화
- [ ] 큰 프레임 지연에도 숫자를 건너뛰지 않음
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

# 20. 개발 결과

28일차에서는 단순한 3초 타이머가 아니라 **향후 온라인 멀티플레이에서도 사용할 수 있는 Ready 기반 경기 시작 흐름**을 구축했다.

최종 구조:

```text
Preparing
↓
All Players Ready
↓
0.5초 안정화
↓
3
↓
2
↓
1
↓
시작!
↓
Playing
↓
Finished
```

카운트다운은 각 숫자를 독립적인 단계로 관리하며 첫 숫자 `3`을 실제 화면에 표시할 수 있도록 초기 프레임 보호를 추가했다.

향후 Fusion에서는 서버 권한이 모든 플레이어의 준비 완료를 판단한 뒤 동일한 Ready 진입점을 호출하는 방식으로 확장할 수 있다.

GitHub 저장소에는 현재 이 커밋에 대한 별도 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 Unity 로컬 환경의 EditMode / PlayMode 테스트와 Console Error 0을 기준으로 확인한다.
