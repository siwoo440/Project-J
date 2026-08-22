# Project J - 74일차 개발 일지

## 개발 기준

73일차 완료 커밋:

```text
f3e06cfabae553c2c22e25e9b6a5f94a9d9a2804
73일차 : 대표 아이템 5종 네트워크 사용·효과 및 소비 구현
```

74일차 최신 커밋:

```text
e16a33f93bd3baa4de4f3745525b4d267fecc50b
74일차 : Lobby Ready·MatchLoading 및 2인 경기 진입 흐름 구현
```

이번 일차에서는 기존 비공개 방 생성·방 코드 참가·NetworkRunner·Network Player Spawn 구조 위에 Lobby Ready, MatchLoading, Fusion Scene 전환, Game 진입 후 Countdown 시작 흐름을 연결했다.

---

## 74일차 목표

두 명의 Player가 같은 비공개 Session에 접속한 뒤 Lobby에서 준비 상태를 맞추고 실제 Game Scene으로 이동하여 한 경기를 시작할 수 있는 흐름을 만든다.

```text
Host 방 생성
↓
Client 방 코드 참가
↓
Lobby
↓
각 Player Ready
↓
전원 Ready
↓
MatchLoading
↓
Game Scene
↓
Player 시작 위치 준비
↓
3초 Countdown
↓
Playing
```

---

## 1. Lobby Ready 상태 추가

각 Network Player에 Lobby Ready 상태를 추가했다.

```text
NetworkLobbyReady
```

Lobby에서는 `R` 키를 눌러 다음 상태를 전환한다.

```text
NOT READY
↔
READY
```

Ready 상태는 Player별 Networked 값으로 관리하며 State Authority가 최종 상태를 확정한다.

Client는 RPC를 통해 자신의 Ready 변경을 요청한다.

---

## 2. Lobby와 Game의 R 키 역할 분리

기존 `R` 키는 경기 중 수동 Respawn 테스트에 사용하고 있었다.

74일차부터 Scene에 따라 기능을 분리했다.

```text
Lobby
R → Ready / Not Ready

Game
R → Manual Respawn
```

따라서 Lobby에서 Ready를 누르는 과정이 기존 Respawn 기능과 충돌하지 않는다.

---

## 3. 기존 자동 2인 Countdown 제거

71일차에서는 Active Player가 2명 이상이면 자동으로 Countdown이 시작됐다.

기존 구조:

```text
Player Count >= 2
↓
Countdown
```

74일차부터는 이 자동 시작을 제거했다.

새 구조:

```text
2명 이상 접속
+
Lobby 전원 Ready
+
Game Scene 진입
+
Player 준비 완료
↓
Countdown
```

즉 단순히 두 명이 같은 방에 들어왔다는 이유만으로 경기가 시작되지 않는다.

---

## 4. ProjectJNetworkLobbyFlow 추가

Lobby부터 실제 경기까지 전체 흐름을 담당하는:

```text
ProjectJNetworkLobbyFlow
```

를 추가했다.

Flow 상태:

```text
Disconnected
EnteringLobby
Lobby
MatchLoading
GamePreparing
Countdown
Playing
Finished
```

각 단계의 역할:

```text
Disconnected
→ Session 연결 전

EnteringLobby
→ Host가 Lobby Scene 로드 요청

Lobby
→ Player Ready 대기

MatchLoading
→ 전원 Ready 후 Game Scene 로드

GamePreparing
→ Network Player 시작 위치와 경기 상태 준비

Countdown
→ 기존 3초 경기 시작 카운트다운

Playing
→ 실제 경기 진행

Finished
→ 경기 종료
```

---

## 5. Fusion Bootstrap에 Lobby Flow 자동 설치

`ProjectJFusionBootstrapRuntimeInstaller`를 수정하여 Bootstrap GameObject에 다음 컴포넌트가 항상 존재하도록 했다.

```text
ProjectJFusionBootstrap
ProjectJFusionBootstrapDebugView
ProjectJNetworkLobbyFlow
```

기존 Bootstrap이 이미 Scene에 존재하는 경우에도 `ProjectJNetworkLobbyFlow`가 없다면 자동 추가한다.

따라서 Inspector에서 별도 컴포넌트를 연결할 필요가 없다.

---

## 6. Session 연결 후 Lobby Scene 자동 진입

비공개 방 생성 또는 참가가 완료되어 Fusion Runner가 Running 상태가 되면 Lobby Flow가 시작된다.

Scene Authority인 Host가:

```text
NetworkRunner.LoadScene()
```

을 사용하여 `Lobby.unity`를 로드한다.

Client는 Fusion Scene 상태를 따라 같은 Lobby Scene으로 이동한다.

---

## 7. Lobby Scene Build Settings 등록

기존 저장소에는:

```text
Assets/ProjectJ/Scenes/Lobby.unity
```

Scene이 존재했지만 Build Settings에는 등록되지 않은 상태였다.

74일차에서 Build Settings를 다음 구조로 수정했다.

```text
Bootstrap
MainMenu
Lobby
Game
```

따라서 Fusion SceneRef에서 Lobby와 Game을 Build Index로 정상 조회할 수 있다.

---

## 8. 전원 Ready 판정

Lobby에서는 현재 Session의 Active Player를 순회한다.

확인 항목:

```text
참가 인원
PlayerObject 존재 여부
각 Player LobbyReady 상태
```

경기 시작 조건:

```text
Participant Count >= 2
AND
모든 PlayerObject 준비
AND
Ready Count == Participant Count
```

예:

```text
P0 READY
P1 NOT READY
→ 대기

P0 READY
P1 READY
→ MatchLoading
```

3명이 참가했다면 3명 모두 Ready여야 시작한다.

---

## 9. 경기 시작 시 Session 신규 참가 차단

전원 Ready가 확정되면 Host는 Game Scene을 로드하기 전에 Session을 닫는다.

```text
SessionInfo.IsOpen = false
```

따라서:

```text
Lobby
→ 신규 참가 가능

MatchLoading 이후
→ 신규 참가 차단
```

구조가 된다.

---

## 10. MatchLoading → Game Scene 전환

Host가 전원 Ready를 확인하면 Fusion을 통해 Game Scene을 로드한다.

```text
Lobby
↓
MatchLoading
↓
NetworkRunner.LoadScene(Game)
↓
Host / Client Game Scene 진입
```

Host와 Client가 각자 로컬 Scene 전환을 실행하지 않고 Scene Authority의 Fusion Scene 상태를 기준으로 동기화한다.

---

## 11. NetworkRunner와 Player Scene 전환 유지

런타임에 생성된 NetworkRunner와 Network Player가 Lobby → Game Scene 전환 중 제거되지 않도록 유지 처리했다.

```text
NetworkRunner
→ DontDestroyOnLoad

Network Player
→ Runner.MakeDontDestroyOnLoad
```

이를 통해 같은 Fusion Session과 같은 PlayerObject를 유지한 상태로 Game Scene에 진입한다.

---

## 12. Game Scene Player 준비

Game Scene에 들어오면 Host가 Active Player를 확인하고 각 Player의 경기 시작 상태를 초기화한다.

초기화 대상:

```text
External Force
Push Cooldown
Respawn Protection
Checkpoint
Respawn Position
Lobby Ready
Inventory
Player Motion
Race Height
Best Height
```

현재 시작 위치는 임시 슬롯 좌표를 사용한다.

```text
P0 → (0, 2, 4)
P1 → (3, 2, 4)
P2 → (6, 2, 4)
...
```

Player 간 X 간격은 3으로 설정했다.

---

## 13. Game Scene 진입 후 Network Player 위치 동기화

Player 준비 과정에서 `NetworkTransform.Teleport()`를 사용해 Player를 시작 위치로 이동한다.

NetworkTransform을 찾을 수 없는 경우에는 일반 Transform 이동을 예비 처리로 사용한다.

이후 시작 위치를 Respawn Position과 Race Height 기준으로 함께 저장한다.

---

## 14. Ready Flow 승인 후 Countdown 시작

Game Scene 준비가 완료되면 Host만 기존 경기 Coordinator에 Countdown 시작을 요청한다.

```text
TryBeginCountdownFromLobbyFlowAuthority()
```

검증 조건:

```text
State Authority
Game Scene
2명 이상
Match Coordinator 존재
```

조건을 통과하면 기존 71일차의:

```text
Preparing
↓
Countdown
↓
Playing
```

경기 상태 구조를 그대로 사용한다.

---

## 15. Client Game Flow 상태 보정

초기 74일차 구현에서는 Client가 Scene Authority가 아니기 때문에 Game Scene 진입 후 `GamePreparing`에서 계속 빠져나오는 문제가 있었다.

최종 커밋에서는 Client가 Host의 Network Match State를 확인하도록 수정했다.

```text
Client GamePreparing
↓
Host MatchState 확인

Preparing
→ 계속 대기

Countdown 이상
→ Host 준비 완료로 판단
↓
Client Flow도 계속 진행
```

따라서 Host뿐 아니라 Client도:

```text
GamePreparing
→ Countdown
→ Playing
→ Finished
```

상태를 정상적으로 따라간다.

---

## 16. 74일차 Debug 표시

Editor 또는 Development Build에서는 화면에 다음 Flow 정보를 표시한다.

```text
DAY 74 LOBBY / MATCH FLOW

Phase
Players
Ready Count
Status
```

Lobby에서는:

```text
R : READY / NOT READY
```

안내도 표시한다.

이를 통해 Host와 Client가 현재 같은 Lobby/Match 단계에 있는지 확인할 수 있다.

---

## 수정 파일

```text
Assets/ProjectJ/Network/Fusion/Bootstrap/
└─ ProjectJFusionBootstrapRuntimeInstaller.cs

Assets/ProjectJ/Network/Fusion/Player/
└─ ProjectJNetworkExternalGameplay.cs

ProjectSettings/
└─ EditorBuildSettings.asset
```

---

## 생성 파일

```text
Assets/ProjectJ/Network/Fusion/Session/
├─ ProjectJNetworkLobbyFlow.cs
└─ ProjectJNetworkLobbyFlow.cs.meta
```

---

## 삭제 파일

```text
없음
```

---

## 74일차 테스트 항목

### Session 연결

```text
Host
→ 비공개 방 생성
→ 6자리 Room Code 확인

Client
→ 같은 Room Code 입력
→ Session 참가 성공
```

### Lobby 진입

```text
Host / Client
→ Lobby Scene 자동 진입

DAY 74 LOBBY / MATCH FLOW
→ 양쪽 표시 확인
```

### Ready

```text
Host R
→ P0 READY

Client R
→ P1 READY

Ready Count
→ 2 / 2
```

### MatchLoading

```text
전원 Ready
→ MatchLoading

Session
→ 신규 참가 차단

Host / Client
→ Game Scene 이동
```

### GamePreparing

```text
P0 시작 위치 배치
P1 시작 위치 배치

Checkpoint
Inventory
External Force
Respawn
Race State
→ 경기 시작 상태로 초기화
```

### Countdown

```text
Host
GamePreparing → Countdown

Client
GamePreparing → Countdown

3초 Countdown
→ 양쪽 동일
```

### Playing

```text
Countdown 종료
→ Host Playing
→ Client Playing

이동 입력 허용
```

### 기존 기능 회귀 테스트

```text
Movement
Jump
Sprint
Stamina
Crouch
Push
Checkpoint
Respawn
3초 Respawn Protection
Height / Rank
Item Box
2 Slot Inventory
5 Item Effects
FINISH
10분 Match Timer
Final Rank
정상 유지
```

---

## 코드 검토 결과

최신 GitHub 커밋 기준으로 다음 항목을 확인했다.

```text
Lobby Ready Networked 상태
→ 구현됨

Client Ready RPC
→ 구현됨

기존 2인 자동 Countdown 제거
→ 반영됨

ProjectJNetworkLobbyFlow
→ 추가됨

Lobby Build Settings
→ 반영됨

Host Fusion Lobby Scene 로드
→ 구현됨

전원 Ready 판정
→ 구현됨

MatchLoading
→ 구현됨

Session 신규 참가 차단
→ 구현됨

Lobby → Game Fusion Scene 전환
→ 구현됨

Runner / Player Scene 전환 유지
→ 구현됨

Game Player 초기화·시작 위치 이동
→ 구현됨

Lobby Flow 승인 후 Countdown
→ 구현됨

Client GamePreparing 고정 문제
→ 수정됨
```

GitHub 저장소에는 자동 Unity 빌드·Host/Client 통합 테스트 CI가 등록되어 있지 않으므로 실제 Unity 컴파일과 런타임 성공 여부는 원격 저장소만으로 확정할 수 없다.

최종 완료 기준:

```text
Unity Console Error 0
+
Host / Client 2인 Lobby → Game → Countdown → Playing 테스트 통과
```

---

## 74일차 완료 구조

```text
Private Session
↓
Lobby
├─ P0 Ready
└─ P1 Ready
        ↓
Host All Ready 판정
        ↓
Session Close
        ↓
MatchLoading
        ↓
Fusion Game Scene Load
        ↓
Network Player 준비
        ↓
3초 Countdown
        ↓
Playing
        ↓
기존 경기 시스템 전체
```

---

## 다음 개발 방향

75일차에서는 Photon Fusion Host Mode Phase 6의 통합 마감 단계로 진행한다.

핵심 방향:

```text
58~74일차 전체 네트워크 기능 통합 점검
↓
Lobby → Game → Finish 전체 경기 반복 테스트
↓
Host / Client 상태 불일치 확인
↓
Disconnect / Scene Transition 예외 점검
↓
Network Player 중복 Spawn 점검
↓
Prediction / External Force / Item 회귀 점검
↓
Phase 6 완료 Gate 정리
```

74일차까지 개별 네트워크 기능과 실제 2인 경기 진입 흐름을 연결했고, 75일차에서는 이를 하나의 안정적인 멀티플레이 프로토타입으로 묶는 것을 목표로 한다.
