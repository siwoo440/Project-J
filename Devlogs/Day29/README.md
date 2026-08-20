# 29일차 개발일지 - 15분 경기 타이머 및 종료 경고 시스템

## 1. 개발 목표

29일차의 목표는 28일차에서 구현한 경기 상태 시스템에 실제 경기 제한 시간을 연결하고, 경기 종료가 가까워질수록 단계별 경고를 제공하는 것이다.

현재 기준 커밋:

```text
654fbead6c32f0adfa78ee8921acf726ea7e392d
```

현재 커밋 메시지:

```text
29
```

이번 일차에서 구현한 핵심 흐름은 다음과 같다.

```text
Preparing
↓
Countdown
↓
Playing
↓
15:00부터 경기 타이머 감소
↓
01:00 경고
↓
00:30 경고
↓
00:10 경고
↓
00:00
↓
Finished
```

---

## 2. 경기 제한 시간

기본 경기 제한 시간을 다음과 같이 설정했다.

```text
15분
= 900초
```

실제 Game Scene의 MatchTimer는 다음 값으로 저장된다.

```text
Match Duration Seconds = 900
Remaining Seconds = 900
```

게임 시작 전에는 시간이 감소하지 않으며, `Playing` 상태로 진입한 이후에만 타이머가 감소한다.

---

## 3. 경기 상태와 타이머 연동

타이머는 기존 `MatchStateController`의 상태를 확인한다.

동작 기준:

```text
Preparing
→ 시간 감소 X

Countdown
→ 시간 감소 X

Playing
→ 시간 감소 O

Finished
→ 시간 감소 X
```

따라서 28일차의:

```text
READY
↓
3
↓
2
↓
1
↓
시작!
```

카운트다운이 진행되는 동안에는 15분 경기 시간이 줄어들지 않는다.

`Playing` 상태가 된 뒤부터 실제 경기 시간이 감소한다.

---

## 4. MatchTimer

새 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Match/MatchTimer.cs
```

주요 역할:

- 기본 경기 시간 15분 관리
- 현재 남은 시간 관리
- `Playing` 상태에서만 시간 감소
- 1분 경고
- 30초 경고
- 10초 경고
- 0초 도달 감지
- 경기 종료 요청
- 화면 표시용 시간 문자열 변환

기본 시간 상수:

```text
DefaultMatchDurationSeconds = 15 × 60
```

결과:

```text
900초
```

---

## 5. Time.unscaledDeltaTime 사용

경기 시간은 다음 값을 기준으로 감소한다.

```text
Time.unscaledDeltaTime
```

따라서 향후 ESC 메뉴나 로컬 UI 처리 과정에서 `Time.timeScale`이 변경되더라도 경기 제한 시간 자체는 계속 흐를 수 있는 구조다.

멀티플레이 단계에서는 최종적으로 Host 또는 Server의 공식 경기 시간을 기준으로 동기화할 예정이다.

현재는 서버 기능이 아직 없으므로 로컬 서버 대체용 타이머 역할을 한다.

---

## 6. 시간 표시

남은 시간은 `MM:SS` 형태로 표시한다.

예:

```text
900초 → 15:00
600초 → 10:00
60초  → 01:00
30초  → 00:30
10초  → 00:10
0초   → 00:00
```

표시에는 올림 방식을 사용한다.

예:

```text
59.9초
→ 01:00
```

이후 실제 남은 시간이 59초 이하로 내려가면:

```text
00:59
```

형태로 표시된다.

---

## 7. 1분 경고

경기 시간이 처음으로:

```text
60초 이하
```

가 되는 순간 다음 경고를 한 번 발생시킨다.

```text
1분 남음!
```

동일한 경고가 이후 매 프레임 반복되지 않도록 별도의 Trigger 상태를 저장한다.

---

## 8. 30초 경고

남은 시간이:

```text
30초 이하
```

로 진입하는 순간:

```text
30초 남음!
```

경고를 한 번 발생시킨다.

---

## 9. 10초 경고

남은 시간이:

```text
10초 이하
```

로 진입하는 순간:

```text
10초 남음!
```

경고를 한 번 발생시킨다.

최종적으로 경기 종료가 임박했음을 알리는 마지막 단계 경고다.

---

## 10. 경고 Event

MatchTimer는 다음 Event를 제공한다.

```text
WarningReached
TimeExpired
```

`WarningReached`는 다음 값 중 하나를 전달한다.

```text
60
30
10
```

현재는 Debug View에서 글자로 표시하지만, 향후 다음 기능을 연결할 수 있다.

```text
경고음
HUD 색상 변경
애니메이션
화면 효과
마지막 10초 연출
```

---

## 11. 0초 경기 종료

남은 시간이:

```text
0초
```

가 되면 더 이상 음수로 내려가지 않는다.

타이머는:

```text
TimeExpired
```

Event를 발생시키고 기존 28일차의:

```text
MatchStateController.FinishMatch()
```

를 호출한다.

결과:

```text
Playing
↓
00:00
↓
Finished
```

로 전환된다.

28일차의 입력 제어 구조를 그대로 사용하므로 `Finished` 상태가 되면 Player의 경기 조작도 잠긴다.

---

## 12. MatchTimerDebugView

새 Runtime 스크립트:

```text
Assets/ProjectJ/Runtime/Match/MatchTimerDebugView.cs
```

현재 테스트 단계에서 화면에 다음 정보를 표시한다.

```text
15:00
01:00
00:30
00:10
00:00
```

경고 발생 시:

```text
1분 남음!
30초 남음!
10초 남음!
```

을 표시한다.

이 View는 최종 HUD가 아니라 기능 검증용 Debug View다.

---

## 13. 글자 색상 수정

29일차 최종 수정에서 Debug UI의 가독성을 위해 표시 글자를 검은색으로 통일했다.

적용 대상:

```text
READY
WAITING
3
2
1
시작!
FINISHED
Match State
경기 타이머
1분 남음!
30초 남음!
10초 남음!
```

`MatchStateDebugView`와 `MatchTimerDebugView`의 GUIStyle에:

```text
Color.black
```

을 적용했다.

---

## 14. Editor 자동 설정

새 Editor 스크립트:

```text
Assets/ProjectJ/Editor/Day29MatchTimerSetup.cs
```

Unity 메뉴:

```text
ProjectJ
→ Day29
→ Setup 15 Minute Match Timer
```

실행하면 기존 Game Scene의:

```text
=== Match State ===
```

오브젝트에 다음 컴포넌트를 추가한다.

```text
MatchStateController
MatchTimer
MatchTimerDebugView
```

실제 Game Scene에는:

```text
900초
= 15:00
```

이 설정된다.

---

## 15. Game Scene 적용 결과

Game Scene의 Match 관련 구조:

```text
=== Match State ===
├─ MatchStateController
├─ MatchTimer
└─ MatchTimerDebugView
```

MatchTimer 설정:

```text
Match Duration Seconds = 900
Remaining Seconds = 900
```

기존 Ready 및 Countdown 시스템을 그대로 유지하면서 타이머만 추가 연결했다.

---

## 16. Day29 수동 테스트 Scene

생성된 테스트 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day29/
└─ Day29_MatchTimerTest.unity
```

실제 15분을 기다리지 않고 세 종류의 경고를 빠르게 확인할 수 있도록 테스트 Scene은:

```text
65초
```

로 구성했다.

테스트 흐름:

```text
READY
↓
3
↓
2
↓
1
↓
시작!
↓
01:05
↓
01:00
1분 남음!
↓
00:30
30초 남음!
↓
00:10
10초 남음!
↓
00:00
↓
Finished
```

---

## 17. EditMode 테스트

새 테스트:

```text
Assets/ProjectJ/Tests/EditMode/MatchTimerTests.cs
```

주요 검증 항목:

- 기본 경기 시간이 900초인지 확인
- 900초가 `15:00`으로 표시되는지 확인
- Preparing에서 시간이 감소하지 않는지 확인
- Countdown에서 시간이 감소하지 않는지 확인
- Playing에서 시간이 감소하는지 확인
- 60초 경고가 발생하는지 확인
- 30초 경고가 발생하는지 확인
- 10초 경고가 발생하는지 확인
- 동일 경고가 중복 발생하지 않는지 확인
- 0초에서 Finished 상태로 전환되는지 확인
- Finished 이후 시간이 0초 아래로 감소하지 않는지 확인
- 시간 문자열 변환이 정상인지 확인

---

## 18. 생성 및 수정 파일

새 파일:

```text
Assets/ProjectJ/Runtime/Match/MatchTimer.cs
Assets/ProjectJ/Runtime/Match/MatchTimerDebugView.cs
Assets/ProjectJ/Editor/Day29MatchTimerSetup.cs
Assets/ProjectJ/Tests/EditMode/MatchTimerTests.cs
```

수정 파일:

```text
Assets/ProjectJ/Runtime/Match/MatchStateDebugView.cs
Assets/ProjectJ/Scenes/Game.unity
```

생성 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day29/Day29_MatchTimerTest.unity
```

삭제 파일:

```text
없음
```

---

## 19. 수동 검증 체크리스트

- [ ] Game Scene의 MatchTimer가 900초로 설정됨
- [ ] 경기 시작 전에는 시간이 감소하지 않음
- [ ] Countdown 중에는 시간이 감소하지 않음
- [ ] `시작!` 이후부터 시간이 감소함
- [ ] 실제 Game Scene은 `15:00`부터 시작
- [ ] Day29 테스트 Scene은 `01:05`부터 시작
- [ ] `01:00`에서 1분 경고 발생
- [ ] `00:30`에서 30초 경고 발생
- [ ] `00:10`에서 10초 경고 발생
- [ ] 각 경고가 한 번씩만 발생
- [ ] `00:00`에서 Finished 전환
- [ ] Finished 이후 Player 경기 입력 잠금
- [ ] 시간이 0보다 작아지지 않음
- [ ] Debug 글자가 검은색으로 표시됨
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

## 20. 개발 결과

29일차에서는 기존 경기 상태 시스템 위에 **15분 제한 시간 기반 경기 종료 시스템**을 추가했다.

최종 흐름:

```text
Preparing
↓
Ready
↓
3
2
1
시작!
↓
Playing
↓
15:00
↓
01:00 - 1분 경고
↓
00:30 - 30초 경고
↓
00:10 - 10초 경고
↓
00:00
↓
Finished
```

현재 단계에서는 로컬 타이머가 서버 시간을 대신하며, 이후 Fusion Host/Server 구현 시 서버가 공식 경기 시작 시각과 종료 시각을 결정하도록 확장할 수 있다.

GitHub 저장소에는 현재 이 커밋에 대한 별도 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 Unity 로컬 환경에서 EditMode / PlayMode 테스트와 Console Error 0을 확인한 뒤 확정한다.
