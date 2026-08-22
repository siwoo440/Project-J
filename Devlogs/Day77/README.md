# Project J - 77일차 개발일지

## 1. 개발 목표

76일차에 구축한 다중 창 멀티플레이 환경을 기반으로,
실제 `Game` Scene에서 4명의 Player가 START부터 FINISH까지
전체 경기를 진행할 수 있는 고정 Greybox 맵과 4인 검증용 디버그 환경을 구축한다.

이번 일차의 핵심 목표는 다음과 같다.

- 실제 `Game` Scene에 Player가 설 수 있는 경기용 플랫폼 추가
- 4인 Spawn 위치를 Start Plaza 위로 재배치
- START → CP1 → CP2 → CP3 → CP4 → FINISH 고정 코스 구축
- Push 테스트용 넓은 Arena 구성
- Checkpoint와 Respawn Point 연결
- Checkpoint별 Fall Limit 구성
- Network FINISH Trigger 연결
- F4 4인 연결·높이·순위·Checkpoint·FINISH Debug View 추가
- 다음 8인 테스트 전에 4인 전체 경기 Gate를 검증할 기반 마련

---

## 2. 실제 Game Scene에 4인 테스트 맵 추가

기존 `Game` Scene에는 76일차 Spawn Point는 존재했지만
Player가 서서 이동하거나 경기를 진행할 수 있는 충분한 플랫폼이 없었다.

77일차에서는 실제 다음 Scene에 고정 Greybox 맵을 추가했다.

```text
Assets/ProjectJ/Scenes/Game.unity
```

생성되는 최상위 구조는 다음과 같다.

```text
=== DAY77 4 PLAYER TEST MAP ===
├─ === START ===
├─ === SECTION 01 / CP1 ===
├─ === SECTION 02 / CP2 ===
├─ === SECTION 03 / CP3 ===
├─ === SECTION 04 / CP4 ===
├─ === FINISH ===
└─ === SYSTEM ===
```

이 맵은 최종 아트용 맵이 아니라
4인 네트워크 전체 경기 검증용 고정 Greybox 맵이다.

---

## 3. Start Plaza 구성

시작 구역에는 4~8명의 Player가 동시에 대기할 수 있는
넓은 Start Plaza를 배치했다.

주요 구성은 다음과 같다.

```text
Start_Plaza
Start_Rail_Left
Start_Rail_Right
Start_Rail_Back
```

Start Plaza 크기는 4명의 Player가 서로 겹치지 않고
대기·이동·Push 테스트를 할 수 있도록 넓게 구성했다.

---

## 4. Spawn Point 재배치

76일차에 생성한 다음 Spawn Point를
새 Start Plaza 위에 맞춰 재배치한다.

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

배치 형태는 최대 8인을 고려해 두 줄 구조로 사용한다.

```text
P0   P1   P2   P3

P4   P5   P6   P7
```

77일차 4인 테스트에서는 다음 슬롯을 사용한다.

```text
P0 → Spawn_00
P1 → Spawn_01
P2 → Spawn_02
P3 → Spawn_03
```

---

## 5. 상승형 Greybox 코스

경기는 START에서 FINISH 방향으로 진행되며,
각 구간마다 약간씩 높이가 상승한다.

기본 Platform 높이 차이는 약 `0.8m`를 사용한다.

```text
START
↓
SECTION 01
↓
CP1
↓
SECTION 02
↓
CP2
↓
SECTION 03
↓
CP3
↓
SECTION 04
↓
CP4
↓
FINAL
↓
FINISH
```

Network Player의 현재 이동·점프 값을 기준으로
일반 점프로 올라갈 수 있는 높이 차이를 사용했다.

---

## 6. Section 01 - CP1 Push Arena

첫 번째 구간은 기본 이동과 점프를 확인한 뒤
넓은 Push 테스트 공간으로 연결된다.

주요 오브젝트:

```text
S1_Step_01
S1_Step_02
S1_Step_03
CP1_Push_Arena
CP1_Trigger
CP1_Respawn
```

`CP1_Push_Arena`는 4명의 Player를 한 공간에 모아
다음 상황을 시험하기 위한 구역이다.

- 가장 가까운 Player Push Target 선정
- 여러 Player가 동시에 Push
- 양방향 동시 Push
- Respawn Protection 중 Push 차단
- 높이 Rank 변화
- 외부 힘 동기화

---

## 7. Checkpoint 4개 구성

현재 게임의 Checkpoint 체계에 맞춰
CP1~CP4를 모두 실제 코스에 배치했다.

```text
CP1
CP2
CP3
CP4
```

각 Checkpoint에는 다음 두 오브젝트가 존재한다.

```text
CPX_Trigger
CPX_Respawn
```

Player가 Trigger를 통과하면
기존 `Checkpoint`와 `ICheckpointReceiver` 구조를 통해
Network Player의 Checkpoint 상태가 갱신된다.

각 Player는 자신의 최고 Checkpoint를 독립적으로 보유한다.

---

## 8. Respawn Point

각 Checkpoint에는 별도의 Respawn Transform을 배치했다.

예:

```text
CP1_Respawn
CP2_Respawn
CP3_Respawn
CP4_Respawn
```

Player가 추락하면
현재 자신이 마지막으로 활성화한 Checkpoint의 Respawn 위치로 이동한다.

예:

```text
P0 → CP1
P1 → CP2
P2 → CP3
P3 → CP4
```

상태에서 각각 떨어질 경우
서로 다른 자신의 Checkpoint로 돌아가야 한다.

---

## 9. Checkpoint Fall Limit

현재 `CheckpointFallLimitSet`을 사용해
Checkpoint별 낙하 판정 높이를 구성했다.

설정 값은 다음과 같다.

```text
Start = -6.0

CP1 = 0.0
CP2 = 3.5
CP3 = 7.0
CP4 = 10.5
```

높은 Checkpoint에 도달한 Player는
그 구간의 플랫폼 아래로 떨어졌을 때
해당 Checkpoint Respawn으로 복귀한다.

---

## 10. Finish 구역

마지막 상승 구간 이후 넓은 Finish Deck을 배치했다.

주요 구성:

```text
Final_Step_01
Final_Step_02
Finish_Deck
Finish_Trigger

Finish_Gate_Left
Finish_Gate_Right
Finish_Gate_Top
```

`Finish_Trigger`에는 기존 `FinishTrigger`가 연결된다.

Network Player가 FINISH를 통과하면
`IFinishReceiver`를 통해 기존 네트워크 FINISH 처리로 전달된다.

이를 통해 4명의 도착 순서를 검증할 수 있다.

예:

```text
P2 → 1번째
P0 → 2번째
P3 → 3번째
P1 → 4번째
```

최종 결과는 모든 Client에서 동일해야 한다.

---

## 11. Day77 4 Player Debug View

4인 전체 상태를 한 화면에서 빠르게 확인할 수 있도록
`ProjectJDay77FourPlayerDebugView`를 추가했다.

Editor 또는 Development Build에서 `F4`로 표시 상태를 전환한다.

표시 내용:

```text
DAY 77 - 4 PLAYER GATE

Participants : N / 4
PlayerObjects : N / 4

4P CONNECTION GATE : PASS / WAIT
```

4명이 모두 접속하고
Network PlayerObject가 모두 생성되면:

```text
4P CONNECTION GATE : PASS
```

로 표시된다.

---

## 12. Player별 Debug 정보

각 Player별로 다음 상태를 표시한다.

```text
Player Index
Race Height
Race Rank
Current Checkpoint
FINISH 여부
```

예:

```text
P0  H:3.20  Rank:4  CP:CP1  FIN:false
P1  H:6.40  Rank:2  CP:CP2  FIN:false
P2  H:9.60  Rank:1  CP:CP3  FIN:false
P3  H:4.80  Rank:3  CP:CP1  FIN:false
```

이를 통해 Host 화면에서
4명의 경기 상태를 동시에 확인할 수 있다.

---

## 13. 4인 테스트 실행 구조

권장 테스트 구성:

```text
Unity Editor
└─ Host / P0

ProjectJ.exe #1
└─ Client / P1

ProjectJ.exe #2
└─ Client / P2

ProjectJ.exe #3
└─ Client / P3
```

모든 Player는 동일한 방 코드로 접속한다.

Host 화면에서:

```text
PLAYERS : 4 / 8
```

을 확인한 뒤 `GAME START`를 누른다.

---

## 14. 경기 시작 흐름

77일차에서도 76일차 Host 시작 Flow를 그대로 사용한다.

```text
4명 접속
↓
Host GAME START
↓
P0~P3 Spawn 배치
↓
Player 상태 초기화
↓
3초 Network Countdown
↓
Playing
```

경기 시작 전에는 Client가 직접 시작할 수 없다.

---

## 15. 4인 전체 경기 검증 항목

77일차에서 다음 순서로 한 경기를 진행한다.

```text
4 Player 접속
↓
4 Player Spawn
↓
GAME START
↓
3초 Countdown
↓
기본 이동
↓
Jump
↓
Sprint
↓
Crouch
↓
CP1 Push Arena
↓
Push
↓
CP1
↓
추락
↓
Respawn
↓
Respawn Protection
↓
CP2
↓
CP3
↓
CP4
↓
FINISH
↓
도착 순위 확인
```

---

## 16. Rank 검증

플랫폼 코스는 위로 상승하기 때문에
현재 발 높이를 사용하는 Rank 시스템을 확인할 수 있다.

예:

```text
P0 = 3.20
P1 = 6.40
P2 = 9.60
P3 = 4.80
```

예상 순위:

```text
P2 = 1위
P1 = 2위
P3 = 3위
P0 = 4위
```

모든 Client에서 동일한 순위가 표시되는지 확인한다.

---

## 17. 변경 파일

### 신규 파일

```text
Assets/ProjectJ/Editor/
├─ ProjectJDay77FourPlayerMapInstaller.cs
└─ ProjectJDay77FourPlayerMapInstaller.cs.meta

Assets/ProjectJ/Network/Fusion/Test/
├─ ProjectJDay77FourPlayerDebugView.cs
└─ ProjectJDay77FourPlayerDebugView.cs.meta
```

### 수정 파일

```text
Assets/ProjectJ/Scenes/
└─ Game.unity
```

### 삭제 파일

```text
없음
```

---

## 18. 최신 커밋 정적 검토

최신 GitHub 커밋에서
76일차 완료 커밋 대비 77일차 변경을 확인했다.

77일차 변경은 다음 범위에 집중되어 있다.

- 4인 Greybox 맵 생성 Editor Tool
- F4 4인 상태 Debug View
- 실제 `Game.unity`의 Greybox 맵과 Trigger 데이터

`Game.unity` 내부에도 다음 요소가 실제 저장되어 있는 것을 확인했다.

- `=== DAY77 4 PLAYER TEST MAP ===`
- Spawn Point
- Checkpoint Trigger
- Finish Trigger
- 경기용 Cube Platform

Debug View가 사용하는 다음 값들도 현재 네트워크 코드에서 제공되고 있다.

- `ParticipantCount`
- `SpawnedPlayerCount`
- `RaceHeight`
- `RaceRank`
- `CurrentCheckpointId`
- `IsFinished`

정적 코드 연결 기준으로
즉시 수정해야 할 명백한 오류는 발견하지 못했다.

단, GitHub 저장소에는 해당 커밋의
Unity Compile 또는 멀티플레이 PlayMode를 자동 검증하는 CI 상태가 존재하지 않는다.

따라서 최종 완료 판정은 실제 Unity 실행으로 확인한다.

---

## 19. 77일차 완료 기준

다음 조건을 실제 실행에서 만족하면 77일차를 완료한다.

- Unity Console Error 0
- 실제 `Game` Scene에 Greybox 맵 표시
- Start Plaza에 Player가 정상적으로 서 있음
- 4명의 Player가 서로 다른 Spawn Point 사용
- Participants = 4
- PlayerObjects = 4
- `4P CONNECTION GATE : PASS`
- Host의 `GAME START` 정상 작동
- 3초 Countdown 정상 동기화
- 4인 이동·점프·Sprint·Crouch 정상
- Push Target과 외력 정상
- CP1~CP4 Player별 독립 동기화
- 추락 후 올바른 Checkpoint Respawn
- Respawn Protection 정상
- Race Height·Rank 모든 Client 일치
- FINISH Trigger 정상
- 4인의 Finish 순서 모든 Client 일치
- 심각한 위치 튐이나 상태 불일치 없음

---

## 20. 다음 개발 방향

다음 78일차에서는
77일차에서 검증한 동일한 Host Mode 구조를 최대 8인까지 확장한다.

중점 확인 대상은 다음과 같다.

- 8 Player 동시 접속
- Spawn_00 ~ Spawn_07
- 8인 Rank 계산
- 다중 Push 충돌 상황
- Checkpoint·Respawn 동시 처리
- FINISH 순서 8인 확정
- Host CPU 부하
- Client FPS
- Network Tick 안정성
- Network Object 수
- 4인 대비 8인 위치 보정 증가 여부

77일차에서 발견된 기능 오류가 있다면
78일차 확장 전에 먼저 수정한다.
