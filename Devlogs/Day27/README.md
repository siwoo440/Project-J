# 27일차 개발일지 - 실시간 공동 순위 계산 시스템

## 1. 개발 목표

27일차의 목표는 26일차에서 구현한 플레이어 발 기준 높이 데이터를 이용해 **실시간 순위 계산 시스템**을 만드는 것이다.

순위는 플레이어의 현재 발 높이를 소수점 둘째 자리까지만 계산한 공식 값으로 비교한다.

같은 높이값을 가진 플레이어는 같은 등수를 받으며, 순위 방식은 Competition Ranking을 사용한다.

예:

```text
1위
2위
2위
4위
```

현재 기준 커밋:

```text
5479c6991f4dac60c6dd1bb31df53a8f99f98dff
```

현재 커밋 메시지:

```text
27
```

---

# 2. 공식 순위 기준값

26일차의 `PlayerHeightTracker`에서 이미 다음 값을 만든다.

```text
CurrentHeightCentimeters
```

이 값은 실제 발 World Y를 소수점 둘째 자리까지만 유지하도록 변환한 정수값이다.

예:

```text
123.4591m
→ 12345
→ 123.45m
```

```text
123.4501m
→ 12345
→ 123.45m
```

따라서 두 플레이어의 `CurrentHeightCentimeters`가 같으면 공식적으로 같은 높이로 판단한다.

순위 시스템은 float 원본값을 다시 비교하지 않는다.

---

# 3. 공동 순위 규칙

순위 계산 방식은 자신보다 높은 플레이어 수를 세어:

```text
Rank = 자신보다 높은 플레이어 수 + 1
```

로 계산한다.

예:

```text
Player A = 500.00m
Player B = 480.00m
Player C = 480.00m
Player D = 450.00m
```

결과:

```text
A = 1위
B = 2위
C = 2위
D = 4위
```

즉:

```text
1, 2, 2, 4
```

방식이다.

---

# 4. 소수점 둘째 자리 공동 순위

이번 순위 시스템은 다음처럼 실제 float값이 조금 달라도:

```text
123.4591m
123.4501m
```

26일차 높이 계산 결과가 둘 다:

```text
123.45m
```

라면 같은 등수를 부여한다.

예:

```text
A = 123.4591m → 123.45m
B = 123.4501m → 123.45m
C = 120.0000m → 120.00m
```

결과:

```text
A = 1위
B = 1위
C = 3위
```

---

# 5. PlayerRankingCalculator

새 스크립트:

```text
Assets/ProjectJ/Runtime/Ranking/
└─ PlayerRankingCalculator.cs
```

순수 순위 계산을 담당한다.

주요 기능:

```text
CalculateRank()
CalculateRanks()
```

`CalculateRank()`는 한 플레이어보다 높은 높이값이 몇 개인지 계산한다.

`CalculateRanks()`는 입력된 모든 플레이어의 순위를 한 번에 반환한다.

순위 계산 자체는 GameObject나 Unity Scene에 의존하지 않도록 분리했다.

---

# 6. PlayerRankingParticipant

새 스크립트:

```text
Assets/ProjectJ/Runtime/Ranking/
└─ PlayerRankingParticipant.cs
```

각 플레이어에 붙는 순위 참가자 컴포넌트다.

주요 데이터:

```text
PlayerId
PlayerHeightTracker
CurrentRank
CurrentHeightCentimeters
CurrentHeight
```

`CurrentHeightCentimeters`는 직접 새로 계산하지 않고 기존 `PlayerHeightTracker` 값을 그대로 사용한다.

---

# 7. PlayerRankingManager

새 스크립트:

```text
Assets/ProjectJ/Runtime/Ranking/
└─ PlayerRankingManager.cs
```

Scene의 순위 참가자를 관리한다.

주요 흐름:

```text
참가자 등록
↓
각 Player의 CurrentHeightCentimeters 수집
↓
PlayerRankingCalculator 실행
↓
각 Player의 CurrentRank 갱신
```

현재는 `LateUpdate()`에서 순위를 다시 계산한다.

따라서 플레이어가 이동, 점프, 낙하하면 현재 높이에 따라 순위도 실시간으로 변경된다.

---

# 8. 실시간 순위 변경

예:

```text
A = 500.00m
B = 480.00m
```

결과:

```text
A = 1위
B = 2위
```

A가 낙하하여:

```text
A = 450.00m
B = 480.00m
```

가 되면:

```text
B = 1위
A = 2위
```

로 변경된다.

순위는 `HighestHeight`가 아니라 현재 높이인:

```text
CurrentHeightCentimeters
```

를 사용한다.

---

# 9. 점프 중 순위 변화

Project J의 순위 기준은 체크포인트 번호나 발판 번호가 아니라 실제 발 World Y다.

따라서 점프 중 발 위치가 더 높아지면 순위도 잠시 변경될 수 있다.

예:

```text
Player A = 100.00m
Player B = 100.80m
```

이면 B가 더 높은 순위를 가진다.

---

# 10. Player ID

`PlayerRankingParticipant`에는 다음 값이 있다.

```text
PlayerId
```

Player Prefab에서는 기본값:

```text
-1
```

로 설정된다.

실행 중 Manager가 등록할 때 ID가 설정되어 있지 않은 참가자에게 임시 Runtime ID를 부여한다.

현재는 오프라인 구조이며 향후 Fusion 네트워크 시스템에서 PlayerRef 또는 네트워크 플레이어 ID와 연결할 수 있도록 분리했다.

---

# 11. Player Prefab 수정

기존:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

에 다음 컴포넌트를 추가했다.

```text
PlayerRankingParticipant
```

현재 Player 구조의 관련 부분은 다음과 같다.

```text
Player
├─ PlayerHeightTracker
├─ PlayerRankingParticipant
└─ HeightReference_Foot
```

`PlayerRankingParticipant`는 기존 `PlayerHeightTracker`를 참조한다.

---

# 12. Editor 설정 도구

새 Editor 스크립트:

```text
Assets/ProjectJ/Editor/
└─ Day27RankingSetup.cs
```

메뉴:

```text
ProjectJ
→ Day27
→ Setup Ranking System
```

실행 시:

```text
Player.prefab 열기
↓
PlayerHeightTracker 확인
↓
PlayerRankingParticipant 추가
↓
HeightTracker 연결
↓
Prefab 저장
↓
Day27 Ranking Test Scene 생성
```

순서로 처리한다.

---

# 13. Day27 테스트 Scene

생성 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day27/
└─ Day27_RankingTest.unity
```

기본 테스트 높이:

```text
Player_01 = 520.359m
Player_02 = 480.129m
Player_03 = 480.129m
Player_04 = 450.000m
```

공식 높이:

```text
Player_01 = 520.35m
Player_02 = 480.12m
Player_03 = 480.12m
Player_04 = 450.00m
```

예상 순위:

```text
Player_01 = 1위
Player_02 = 2위
Player_03 = 2위
Player_04 = 4위
```

---

# 14. EditMode 테스트

새 테스트:

```text
Assets/ProjectJ/Tests/EditMode/
└─ PlayerRankingCalculatorTests.cs
```

주요 검증 항목:

- 모두 다른 높이 → 1, 2, 3, 4
- 두 명 동점 → 1, 2, 2, 4
- 세 명 공동 1위 → 1, 1, 1, 4
- 모두 같은 높이 → 1, 1, 1, 1
- 입력 순서가 달라도 높이에 따라 순위 계산
- 소수점 둘째 자리까지 같은 값은 공동 순위
- 낙하 후 순위 변경
- 음수 높이에서도 공동 순위 계산
- 플레이어 1명일 때 1위
- 빈 입력 처리

---

# 15. 순위 계산 예시

## 모두 다른 경우

```text
1000.00
900.00
800.00
700.00
```

결과:

```text
1
2
3
4
```

## 두 명 공동 순위

```text
1000.00
900.00
900.00
700.00
```

결과:

```text
1
2
2
4
```

## 세 명 공동 1위

```text
1000.00
1000.00
1000.00
500.00
```

결과:

```text
1
1
1
4
```

## 모두 같은 경우

```text
500.00
500.00
500.00
500.00
```

결과:

```text
1
1
1
1
```

---

# 16. 현재 하지 않은 기능

27일차에는 다음 기능을 구현하지 않았다.

```text
순위 HUD
FINISH 도착 순위 고정
완주자와 미완주자 순위 통합
체크포인트 순위
네트워크 순위 동기화
서버 권한 순위 확정
경기 결과 UI
```

이번 일차는 오직 **현재 높이를 기준으로 한 실시간 공동 순위 계산**에 집중한다.

---

# 17. 생성 및 수정 파일

새 파일:

```text
Assets/ProjectJ/Runtime/Ranking/PlayerRankingCalculator.cs
Assets/ProjectJ/Runtime/Ranking/PlayerRankingManager.cs
Assets/ProjectJ/Runtime/Ranking/PlayerRankingParticipant.cs
Assets/ProjectJ/Editor/Day27RankingSetup.cs
Assets/ProjectJ/Tests/EditMode/PlayerRankingCalculatorTests.cs
```

수정 파일:

```text
Assets/ProjectJ/Prefabs/Player/Player.prefab
```

생성 Scene:

```text
Assets/ProjectJ/Tests/Manual/Day27/Day27_RankingTest.unity
```

삭제 파일:

```text
없음
```

---

# 18. 수동 검증 체크리스트

- [ ] Player Prefab에 `PlayerRankingParticipant` 존재
- [ ] `PlayerHeightTracker` 참조 정상
- [ ] 높은 Player가 더 높은 순위
- [ ] 동일한 0.00m 높이는 같은 순위
- [ ] 1, 2, 2, 4 규칙 정상
- [ ] 1, 1, 1, 4 규칙 정상
- [ ] 모두 같은 높이면 전원 1위
- [ ] 점프 중 높이 변화가 순위에 반영
- [ ] 낙하하면 순위 하락 가능
- [ ] `HighestHeight`가 순위 계산에 사용되지 않음
- [ ] EditMode 전체 Green
- [ ] PlayMode 전체 Green
- [ ] Console Error 0

---

# 19. 개발 결과

27일차에서는 26일차의 발 기준 공식 높이값을 이용해 실시간 공동 순위를 계산하는 시스템을 구축했다.

전체 흐름:

```text
HeightReference_Foot
↓
PlayerHeightTracker
↓
CurrentHeightCentimeters
↓
PlayerRankingParticipant
↓
PlayerRankingManager
↓
PlayerRankingCalculator
↓
CurrentRank
```

핵심 규칙:

```text
높이는 소수점 둘째 자리까지만 계산

같은 공식 높이값
→ 같은 등수

자신보다 높은 플레이어 수 + 1
→ 현재 순위
```

이제 Project J는 플레이어의 실제 현재 높이를 기준으로 `1, 2, 2, 4` 형태의 실시간 공동 순위를 계산할 수 있다.

GitHub 저장소에는 현재 이 커밋에 대한 별도 CI 상태가 등록되어 있지 않으므로 최종 완료 판정은 Unity 로컬의 EditMode / PlayMode 테스트와 Console Error 0을 기준으로 확인한다.
