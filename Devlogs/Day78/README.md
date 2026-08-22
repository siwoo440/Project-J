# Project J - 78일차 개발일지

## 1. 개발 목표

77일차에 구축한 4인 전체 경기 Greybox 환경을 그대로 유지하면서,
Photon Fusion Host Mode의 최대 테스트 인원인 8명까지 참가자를 확장해
접속 상태와 네트워크 보정 상태를 한 화면에서 확인할 수 있는 검증 환경을 구축한다.

이번 일차에서는 새로운 맵이나 게임 규칙을 추가하지 않고
8인 멀티플레이에서 발생할 수 있는 다음 문제를 확인하는 데 집중한다.

- 참가 Player 수 불일치
- Network PlayerObject 중복 또는 누락
- Input Authority 오류
- 8인 Rank·Checkpoint·FINISH 상태 불일치
- Prediction Resimulation 증가
- 위치 Correction 증가
- Rollback 증가
- 다중 창 실행 시 FPS 저하

---

## 2. 기존 77일차 Game Scene 유지

78일차에서는 새로운 테스트 Scene이나 맵을 만들지 않는다.

계속 다음 실제 경기 Scene을 사용한다.

```text
Assets/ProjectJ/Scenes/Game.unity
```

77일차에 만들어 둔 Greybox 구조도 그대로 유지한다.

```text
START
↓
SECTION 01 / CP1
↓
SECTION 02 / CP2
↓
SECTION 03 / CP3
↓
SECTION 04 / CP4
↓
FINISH
```

Spawn Point 역시 기존 최대 8인 구조를 사용한다.

```text
Spawn_00
Spawn_01
Spawn_02
Spawn_03
Spawn_04
Spawn_05
Spawn_06
Spawn_07
```

---

## 3. 77일차 F4 Debug View 정리

77일차의 4인 검증 화면은 삭제하지 않고 그대로 보존했다.

대신 78일차부터 8인 Debug View가 기본 검증 화면이므로
`ProjectJDay77FourPlayerDebugView`의 기본 표시 상태를 다음처럼 변경했다.

```text
이전
F4 Debug View = 기본 표시

78일차
F4 Debug View = 기본 숨김
```

필요할 경우 `F4`를 눌러 기존 4인 Gate를 다시 확인할 수 있다.

이를 통해 77일차 검증 기능을 보존하면서
F4와 F5 화면이 동시에 겹쳐 표시되는 것을 방지한다.

---

## 4. Day78 8 Player Debug View 추가

새로운 파일:

```text
Assets/ProjectJ/Network/Fusion/Test/
└─ ProjectJDay78EightPlayerDebugView.cs
```

를 추가했다.

Editor 또는 Development Build에서
실제 `Game` Scene에 진입하면 자동으로 생성된다.

Runtime 오브젝트:

```text
=== Project J Day78 8P Debug ===
```

Debug View는 기본적으로 표시되며
`F5` 키를 눌러 표시/숨김을 전환한다.

---

## 5. 8인 Connection Gate

78일차의 가장 기본적인 검증 조건은 다음과 같다.

```text
Participants = 8
PlayerObjects = 8
Local InputAuthority = 1
```

세 조건이 모두 만족되면:

```text
8P CONNECTION GATE : PASS
```

로 표시된다.

아직 8명이 모두 준비되지 않았다면:

```text
8P CONNECTION GATE : WAIT
```

상태를 유지한다.

---

## 6. 참가자와 PlayerObject 검증

F5 화면에서 다음 값을 동시에 표시한다.

```text
Participants : N / 8
PlayerObjects : N / 8
```

정상 상태:

```text
Participants : 8 / 8
PlayerObjects : 8 / 8
```

비정상 예:

```text
Participants : 8 / 8
PlayerObjects : 7 / 8
```

PlayerObject 생성 누락 가능성이 있다.

```text
Participants : 8 / 8
PlayerObjects : 9 / 8
```

PlayerObject 중복 생성 여부를 확인해야 한다.

따라서 78일차에서는 단순히 8명이 Session에 들어온 것뿐 아니라
8개의 Network PlayerObject가 정확하게 대응되는지 함께 검사한다.

---

## 7. Local Input Authority 검증

각 실행 프로그램은 자신의 Player 하나에 대해서만
Input Authority를 가져야 한다.

F5 화면:

```text
Local InputAuthority : 1
```

정상 기준:

```text
Host
→ Local InputAuthority = 1

Client 1
→ Local InputAuthority = 1

...

Client 7
→ Local InputAuthority = 1
```

한 프로세스에서 값이 `0`이면
자신의 캐릭터 입력 권한을 받지 못한 상태일 수 있다.

값이 `2` 이상이면
여러 PlayerObject가 동일 프로세스의 입력 권한을 갖는 문제가 있는지 확인한다.

---

## 8. State Authority 정보

F5 화면에는 해당 프로세스에서
State Authority를 가진 Network PlayerObject 수 역시 표시한다.

```text
Local StateAuthority Objects : N
```

Host Mode에서는 Host가 다수의 Network Object에 대한
State Authority를 가질 수 있으므로
이 값은 Input Authority와 별도로 관찰한다.

Client에서는 일반적으로 자신의 Input Authority와
Host의 State Authority 구조가 분리되어 있는지 확인한다.

---

## 9. Player별 경기 정보

F5 화면에서는 P0부터 P7까지 각 Player의 상태를 표시한다.

표시 정보:

```text
Player Index
Authority 상태
Race Height
Race Rank
Current Checkpoint
FINISH 여부
Correction
Max Correction
Rollback
Resimulation
```

예:

```text
P0 [LOCAL]
H:3.20
R:4
CP:CP1
FIN:false
Corr:0.004
Max:0.021
Roll:0.000
ReSim:2
```

---

## 10. Authority 표시

각 Player 옆에는 현재 프로세스 관점의 Authority가 표시된다.

```text
LOCAL
STATE
REMOTE
```

의미:

```text
LOCAL
→ 현재 실행 프로그램이 Input Authority를 가진 Player

STATE
→ 현재 실행 프로그램이 State Authority를 가진 Player

REMOTE
→ 현재 프로그램에서는 원격 Player로 관찰되는 대상
```

이를 통해 여러 창을 실행했을 때
어느 Player가 현재 창의 조작 대상인지 빠르게 구분할 수 있다.

---

## 11. FPS 표시

한 PC에서 최대 8개의 게임 창을 동시에 실행하면
네트워크 문제가 아니라 렌더링 부하 때문에
전체 게임이 느려질 가능성이 있다.

이를 구분하기 위해 F5 화면에
현재 FPS를 표시한다.

```text
FPS : 60.0
```

FPS는 순간값 대신
간단히 보정된 값을 표시한다.

이를 통해 다음 상황을 구분한다.

```text
모든 캐릭터와 화면 자체가 느림
→ PC 성능/FPS 문제 가능성

화면 FPS는 정상인데 Remote Player만 순간이동
→ Network Prediction/Correction 문제 가능성
```

---

## 12. Session 상태 표시

F5 화면에 현재 Session 상태를 표시한다.

```text
SESSION : OPEN
```

또는

```text
SESSION : CLOSED
```

경기 시작 전에는 참가를 받을 수 있도록 OPEN 상태를 사용하고,
Host가 `GAME START`를 눌러 경기를 시작한 이후에는
기존 Day76 Flow에 따라 Session을 닫아
늦은 참가를 막는다.

따라서 경기 중:

```text
SESSION : CLOSED
```

가 표시되는 것은 정상이다.

---

## 13. Resimulation 표시

`ProjectJNetworkPlayer`에서 기존에 수집하고 있던
Prediction Resimulation 정보를 F5 화면에 표시한다.

전체 요약:

```text
Resimulation Batches : N
```

Player별:

```text
ReSim:N
```

Resimulation 자체는 네트워크 Prediction 과정에서
발생할 수 있기 때문에 값이 0이 아니라고 바로 오류는 아니다.

중요한 것은 8인으로 확장했을 때
4인 테스트보다 값이 비정상적으로 빠르게 증가하면서
실제 화면의 위치 떨림이나 입력 지연이 함께 발생하는지 확인하는 것이다.

---

## 14. Correction Distance 표시

Player별 최근 위치 보정 거리:

```text
Corr
```

Player별 누적 최대 보정 거리:

```text
Max
```

전체 Player 중 가장 큰 Max Correction도 상단에 표시한다.

예:

```text
Max Correction : 0.084
```

작은 위치 보정은 Prediction 과정에서 발생할 수 있다.

하지만 값이 반복적으로 크게 증가하고
캐릭터가 화면에서 순간이동하는 현상이 같이 발생한다면
78일차 이후 Network Tick·Prediction·Interpolation 검증 대상이 된다.

---

## 15. Rollback Distance 표시

Player별 최근 Rollback 거리를 표시한다.

```text
Roll:0.000
```

8인 동시 이동·점프·Push 상황에서
Rollback 값과 Correction 값이 갑자기 증가하는지 확인한다.

특히 다음 상황에서 관찰한다.

- 8명 동시 이동
- 여러 명 동시 Jump
- 여러 명 동시 Push
- Checkpoint 동시 통과
- 여러 Player 동시 Respawn
- FINISH 근처 다중 Player 이동

---

## 16. FINISH 인원 표시

상단에 현재 FINISH한 Player 수를 표시한다.

```text
Finished : N / 8
```

이를 통해 FINISH 순서를 시험하면서
정상적으로 완주 인원이 누적되는지 확인한다.

예:

```text
P5 FINISH
→ Finished : 1 / 8

P2 FINISH
→ Finished : 2 / 8

...

전체 완주
→ Finished : 8 / 8
```

---

## 17. 권장 8인 실행 구조

한 PC가 충분한 성능을 가진 경우:

```text
Unity Editor
└─ Host / P0

ProjectJ.exe #1
└─ Client / P1

ProjectJ.exe #2
└─ Client / P2

ProjectJ.exe #3
└─ Client / P3

ProjectJ.exe #4
└─ Client / P4

ProjectJ.exe #5
└─ Client / P5

ProjectJ.exe #6
└─ Client / P6

ProjectJ.exe #7
└─ Client / P7
```

을 사용한다.

한 PC에서 8개 창을 실행하기 어렵다면
두 대 이상의 PC로 나누어도 된다.

예:

```text
PC 1
→ Host + Client 3개

PC 2
→ Client 4개
```

---

## 18. 8인 경기 시작 절차

8명이 모두 같은 Session에 참가한다.

F5 화면에서:

```text
Participants : 8 / 8
PlayerObjects : 8 / 8
Local InputAuthority : 1

8P CONNECTION GATE : PASS
```

를 확인한다.

그 이후 Host가:

```text
GAME START
```

를 누른다.

경기 흐름:

```text
P0~P7 상태 초기화
↓
Spawn_00~Spawn_07 배치
↓
3초 Countdown
↓
Playing
```

---

## 19. 8인 이동 테스트

경기 시작 후 먼저
8명이 동시에 기본 입력을 사용한다.

```text
8명 동시 이동
8명 동시 Jump
8명 동시 Sprint
8명 동시 Crouch
```

확인 항목:

- 각 창에서 자신의 Player만 움직이는지
- Remote Player가 모두 보이는지
- 특정 Player만 멈추지 않는지
- 위치 순간이동이 과도하지 않은지
- FPS가 급격히 감소하지 않는지
- Correction과 ReSim 값이 비정상적으로 증가하지 않는지

---

## 20. 8인 Push 스트레스 테스트

77일차의 CP1 Push Arena를 그대로 사용한다.

8명을 Arena에 모은 뒤:

```text
여러 Player 동시 Push
한 Player 주변에 여러 Target 배치
서로 반대 방향 Push
Respawn Protection 상태 Player Push
Shield 상태 대상 Push
```

등을 확인한다.

기존 Push 규칙:

```text
거리 2.5m
전방 총 90도
가장 가까운 Player 1명
```

이 8인 상황에서도 유지되어야 한다.

---

## 21. 8인 Rank 검증

P0~P7을 서로 다른 높이에 배치하고
모든 실행 창에서 Rank가 같은지 확인한다.

공동순위도 함께 검사한다.

예:

```text
P0 = 15.24
P1 = 15.24
P2 = 10.32
P3 = 8.11
P4 = 8.11
P5 = 8.11
P6 = 5.20
P7 = 1.04
```

예상 Competition Ranking:

```text
P0 = 1위
P1 = 1위
P2 = 3위
P3 = 4위
P4 = 4위
P5 = 4위
P6 = 7위
P7 = 8위
```

---

## 22. Checkpoint 분산 테스트

8명을 서로 다른 진행 상태로 만든다.

예:

```text
P0 → Start
P1 → Start
P2 → CP1
P3 → CP1
P4 → CP2
P5 → CP3
P6 → CP4
P7 → CP4
```

F5 화면에서 각 Player의 CP 값이
정확히 개별 상태로 유지되는지 확인한다.

---

## 23. 동시 Respawn 테스트

여러 Player를 동시에 추락시킨다.

예:

```text
P2
P3
P4
P5

동시 추락
```

정상 결과:

- 각 Player 자신의 Checkpoint에서 Respawn
- 다른 Player의 Respawn Point를 사용하지 않음
- 개별 Respawn Protection 적용
- 다른 Player 상태를 덮어쓰지 않음
- 심각한 Correction 발생 여부 확인

---

## 24. 8인 FINISH 순서 테스트

FINISH 순서를 의도적으로 다르게 만든다.

예:

```text
1. P5
2. P2
3. P7
4. P0
5. P6
6. P1
7. P4
8. P3
```

모든 Client에서 동일한 결과를 확인한다.

FINISH한 Player는:

- 최종 결과 고정
- 입력 잠금
- 이후 높이 Rank 경쟁에서 제외
- FINISH 상태 유지

가 되어야 한다.

---

## 25. 미완주 + TimeExpired 테스트

전원이 FINISH하는 상황 외에도
일부 Player를 경기장에 남겨둔다.

예:

```text
P0~P5
→ FINISH

P6~P7
→ 미완주
```

10분 Match Timer 종료 시:

```text
MatchEndReason
→ TimeExpired
```

가 모든 Client에서 동일한지 확인한다.

---

## 26. F4 / F5 Debug View 사용

```text
F4
→ 77일차 4 Player Debug View

F5
→ 78일차 8 Player Debug View
```

78일차에서는 F5가 기본 표시되고
F4는 기본 숨김이다.

필요할 때만 F4를 켜서
4인 기준 정보와 비교한다.

---

## 27. 변경 파일

### 신규 파일

```text
Assets/ProjectJ/Network/Fusion/Test/
├─ ProjectJDay78EightPlayerDebugView.cs
└─ ProjectJDay78EightPlayerDebugView.cs.meta
```

### 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Test/
└─ ProjectJDay77FourPlayerDebugView.cs
```

### 삭제 파일

```text
없음
```

### Scene 수정

```text
없음
```

77일차의 실제 Game Scene과 Greybox 맵을 그대로 보존했다.

---

## 28. 최신 커밋 정적 검토

78일차 최신 GitHub 커밋을 확인했다.

78일차 변경 범위는 다음과 같다.

- 기존 F4 4인 Debug View 기본 숨김
- F5 8인 Debug View 신규 추가
- 8인 참가자와 PlayerObject Gate
- Local Input Authority 확인
- FPS 표시
- Session Open/Closed 상태 표시
- FINISH 인원 표시
- Resimulation Batch 표시
- 최근/최대 Correction Distance 표시
- Rollback Distance 표시
- P0~P7 개별 Race/Rank/Checkpoint/FINISH 상태 표시

현재 코드가 참조하는
`ProjectJNetworkPlayer`의 Prediction/Correction Debug 값과
`ProjectJFusionBootstrap`의 참가자/PlayerObject 상태는
기존 코드에 공개되어 있어 정적으로 연결 가능하다.

정적 코드 검토 기준으로
즉시 수정해야 할 명백한 오류는 발견하지 못했다.

다만 현재 GitHub 커밋에는
Unity Compile, PlayMode 또는 8인 Windows Build 실행을
자동 검증하는 CI 상태가 등록되어 있지 않다.

따라서 최종 완료 여부는
실제 Unity Editor와 Build를 사용해 확인한다.

---

## 29. 78일차 완료 기준

다음 조건을 실제 실행에서 만족하면 78일차를 완료한다.

- Unity Console Error 0
- Participants = 8
- PlayerObjects = 8
- 각 실행 프로그램 Local InputAuthority = 1
- `8P CONNECTION GATE : PASS`
- P0~P7 서로 다른 Spawn Point 사용
- Host GAME START 정상
- 3초 Countdown 전체 동기화
- 8인 이동 정상
- Jump·Sprint·Crouch 정상
- 8인 Push 상황 정상
- Rank와 공동순위 모든 Client 일치
- Checkpoint 상태 Player별 독립
- 여러 Player 동시 Respawn 정상
- Respawn Protection 정상
- FINISH 순서 모든 Client 일치
- TimeExpired 결과 일치
- 심각한 위치 순간이동 없음
- Correction/Resimulation의 비정상적 폭증 없음
- 다중 창 실행 시 테스트 가능한 FPS 유지

---

## 30. 다음 개발 방향

다음 79일차에서는 정상적인 8인 Host Mode 전체 경기 구조를 기준으로
의도적으로 Network 환경을 나쁘게 만들어 안정성을 검증한다.

주요 대상:

- RTT 증가
- Jitter
- Packet Loss
- Prediction 보정
- Remote Interpolation
- Push 반응
- Checkpoint 전달
- Respawn
- FINISH
- 경기 결과 일치

즉 78일차에서 최대 참가 인원 자체를 검증한 뒤
79일차에서는 불안정한 네트워크에서도 동일한 경기 규칙을 유지할 수 있는지 확인한다.
