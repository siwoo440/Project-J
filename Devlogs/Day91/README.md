# Project J - 91일차 개발일지

## 개발 주제

**Lobby 실제 네트워크 데이터·Ready 연결**

90일차에 구성한 `PlayerLobbyPanel`의 화면 구조를 실제 Photon Fusion Lobby 데이터와 연결하고, Host/Client 참가자 표시, Ready 상태, Room Code, Leave 흐름을 실제 UI에서 사용할 수 있도록 구성했다.

추가로 Unity 종료 시 Steamworks가 먼저 종료된 뒤 Rich Presence 정리 코드가 호출되면서 발생하던 `Steamworks is not initialized` 예외를 수정했다.

---

## 개발 목표

- `Lobby.unity`에 실제 Player Lobby UI 구성
- Fusion `ActivePlayers`를 Player Slot에 연결
- 참가자 입장·퇴장 시 Slot과 빈 자리 갱신
- Host / Client 역할 표시
- 각 Player의 `READY / NOT READY` 상태 표시
- 로컬 `READY` 버튼을 기존 Fusion Ready RPC에 연결
- Room Code와 현재 참가 인원 표시
- `LEAVE` 버튼을 기존 MainMenu 복귀 흐름에 연결
- 최소 2명 전원 Ready 시 기존 Lobby → Game 전환 흐름 재사용
- Unity 종료 시 Steam Rich Presence 정리 예외 방지

---

## 구현 내용

### 1. 실제 Fusion Player Lobby UI 구성

`Lobby.unity`에 `Day91PlayerLobbyCanvas`를 추가하고 실제 네트워크 Lobby 전용 UI를 구성했다.

주요 UI 구성은 다음과 같다.

- Player Lobby 제목
- Ready 인원 요약
- Player Slot 8개
- 이전 / 다음 페이지 버튼
- Page Text
- Match Info
- Room Code
- 참가 인원
- 로컬 Host / Client 역할
- Ready 인원
- Lobby Flow 상태
- Customize 버튼
- Ready 버튼
- Leave 버튼

현재 Private Match의 최대 인원은 8명이지만, `PlayerLobbyPanel`의 페이지 구조는 이후 확장을 고려해 최대 32명까지 처리할 수 있도록 유지했다.

---

### 2. Fusion ActivePlayers 연결

`NetworkRunner.ActivePlayers`를 읽어 현재 Session에 참가 중인 Player 목록을 구성하도록 변경했다.

Player는 `PlayerRef.AsIndex`를 기준으로 정렬하고 가능한 경우 해당 Index를 Slot Index로 사용한다.

이를 통해 참가자가 추가되거나 이탈할 때 Player Slot이 실제 Fusion 참가자 상태를 반영하도록 했다.

빈 Slot은 다음과 같이 표시한다.

```text
WAITING...
EMPTY
```

참가자가 있는 Slot은 다음과 같은 형태로 표시한다.

```text
PLAYER 01
HOST / NOT READY
```

로컬 Player에는 `(YOU)`를 추가해 자신의 Slot을 구분한다.

```text
PLAYER 02 (YOU)
CLIENT / READY
```

---

### 3. Host / Client 표시

현재 Runner의 Scene Authority 여부를 이용해 로컬 사용자의 역할을 Match Info에 표시한다.

```text
HOST
```

또는

```text
CLIENT
```

Player Slot에서는 정렬된 첫 Player를 Host Slot 기준으로 사용하고 나머지 참가자를 Client로 표시한다.

---

### 4. Ready 상태 동기화

새 Ready 시스템을 별도로 만들지 않고 기존 `ProjectJNetworkExternalGameplay`의 Lobby Ready 네트워크 기능을 재사용했다.

로컬 Ready 버튼을 누르면 다음 기존 메서드를 호출한다.

```text
RequestToggleLobbyReady()
```

Ready 상태 자체는 기존 Networked 변수인 `LobbyReady`에서 읽는다.

따라서 UI가 직접 Networked 값을 변경하지 않고 기존 Input Authority → State Authority RPC 구조를 그대로 사용한다.

Ready 전에는 다음과 같이 표시한다.

```text
HOST / NOT READY
CLIENT / NOT READY

READY 0 / 2
```

한 명이 Ready 상태가 되면 다음처럼 변경된다.

```text
READY 1 / 2
```

두 명 모두 Ready 상태가 되면 다음과 같이 표시된다.

```text
READY 2 / 2
```

Ready 버튼의 문구도 현재 로컬 Ready 상태에 따라 자동 변경한다.

```text
READY
```

Ready 상태에서는:

```text
CANCEL READY
```

---

### 5. 기존 Lobby → Game Flow 재사용

전원 Ready 이후 Game Scene을 직접 UI에서 로드하지 않는다.

기존 `ProjectJNetworkLobbyFlow`가 다음 조건을 확인한다.

- 참가자 최소 2명
- 모든 Player Object 준비 완료
- 모든 참가자가 Ready

조건이 만족되면 기존 흐름을 그대로 사용한다.

```text
Lobby
↓
MatchLoading
↓
Game
```

따라서 91일차 UI는 Ready 입력과 상태 표시만 담당하고 실제 Match 시작 권한과 Scene 전환 로직은 기존 네트워크 Flow에 유지했다.

---

### 6. Room Code 표시

현재 실행 중인 `ProjectJFusionBootstrap`의 연결 Session 정보에서 실제 Room Code를 읽어 Match Info에 표시하도록 연결했다.

Host와 Client가 동일한 비공개 Session에 접속했는지 Lobby 화면에서 확인할 수 있다.

---

### 7. 참가·이탈 갱신

Lobby UI는 실행 중 실제 `ActivePlayers` 상태를 계속 갱신한다.

참가자가 들어오면:

```text
EMPTY
→
PLAYER XX / CLIENT / NOT READY
```

이탈하면:

```text
PLAYER XX
→
WAITING... / EMPTY
```

형태로 변경된다.

참가자 수와 Ready Summary도 함께 갱신된다.

---

### 8. Leave 흐름 연결

Lobby의 `LEAVE` 버튼은 Scene을 직접 전환하지 않는다.

기존 `ProjectJDay82SceneFlowCoordinator`의 다음 흐름을 호출한다.

```text
RequestLeaveToMainMenu()
```

이를 통해:

```text
Fusion Session 종료
↓
NetworkRunner 종료
↓
MainMenu 복귀
```

순서가 기존 Scene Flow를 통해 처리된다.

---

### 9. Lobby Scene 자동 구성 Installer

다음 Editor Installer를 추가했다.

```text
Assets/ProjectJ/Editor/ProjectJDay91NetworkPlayerLobbyInstaller.cs
```

Unity 상단 메뉴에서 다음 메뉴를 실행하면 Lobby UI를 자동 생성한다.

```text
Project J
└─ Scene
   └─ 91일차 Network Player Lobby 구성
```

Installer는 다음 요소를 자동 처리한다.

- `Lobby.unity` 열기
- 기존 Day91 Canvas 제거
- 새 Player Lobby Canvas 생성
- EventSystem 확인 및 생성
- 8개 Player Slot 생성
- Match Info 생성
- Ready / Leave 버튼 생성
- `ProjectJPlayerLobbyPanel` 참조 자동 연결
- Scene 저장

---

## Steam 종료 예외 수정

### 발생 오류

Play Mode 또는 Application 종료 과정에서 다음 예외가 발생했다.

```text
InvalidOperationException: Steamworks is not initialized.
Steamworks.SteamFriends.SetRichPresence
ProjectJSteamInviteService.ClearPublishedRichPresence
ProjectJSteamInviteService.OnApplicationQuit
```

### 원인

`ProjectJSteamIdentityService`가 먼저 `SteamAPI.Shutdown()`을 실행한 뒤에도 기존 인증 상태가 남아 있었다.

그 상태에서 `ProjectJSteamInviteService`가 종료되면서 `IsAuthenticated`를 확인하면 여전히 인증 상태로 판단했고, 이미 종료된 Steamworks API의 `SteamFriends.SetRichPresence()`를 호출할 수 있었다.

### 수정

`ProjectJSteamIdentityService.IsAuthenticated`가 실제 SteamAPI 초기화 상태까지 검사하도록 변경했다.

검사 기준:

```text
SteamAPI 초기화 상태
+
Authenticated State
+
ProjectAccountId 존재
+
Web API Ticket 존재
```

또한 `ShutdownSteam()`에서 Steam 종료 후 다음 상태를 정리하도록 변경했다.

```text
State → Uninitialized
StatusMessage → Steam 종료됨
Steam ID 초기화
Persona 초기화
Project Account ID 초기화
Web API Ticket 초기화
```

이제 Steamworks가 먼저 종료된 경우 `IsAuthenticated`가 즉시 `false`가 되어 Rich Presence API를 다시 호출하지 않는다.

---

## 변경 파일

### 생성

```text
Assets/ProjectJ/Editor/ProjectJDay91NetworkPlayerLobbyInstaller.cs
Assets/ProjectJ/Editor/ProjectJDay91NetworkPlayerLobbyInstaller.cs.meta
```

### 수정

```text
Assets/ProjectJ/Network/Fusion/UI/ProjectJPlayerLobbyPanel.cs
Assets/ProjectJ/Scenes/Lobby.unity
Assets/ProjectJ/Steam/Runtime/ProjectJSteamIdentityService.cs
```

### 삭제

```text
없음
```

---

## 최종 동작 구조

```text
MainMenu
↓
PLAY
↓
PRIVATE MATCH
↓
Host 방 생성 / Client Room Code 참가
↓
Fusion Session 연결
↓
Lobby.unity
↓
PlayerLobbyPanel
├─ ActivePlayers
├─ Room Code
├─ Host / Client
├─ READY / NOT READY
├─ 참가 인원
├─ READY 버튼
└─ LEAVE 버튼
↓
2명 이상 전원 READY
↓
기존 ProjectJNetworkLobbyFlow
↓
MatchLoading
↓
Game.unity
```

---

## 테스트 항목

### Lobby 진입

- Host가 비공개 방 생성 가능
- Client가 Room Code로 참가 가능
- Host와 Client가 `Lobby.unity`로 진입
- Player Slot에 실제 참가자 표시
- Room Code 동일 표시
- 참가자 수 동일 표시

### Ready

- Host Ready 버튼 입력
- Client에서 Host Ready 상태 확인
- Client Ready 버튼 입력
- Host에서 Client Ready 상태 확인
- Ready Summary `2 / 2` 표시
- Ready 취소 가능
- 전원 Ready 시 기존 Game Scene 로딩 시작

### Player Slot

- Host / Client 구분
- 로컬 `(YOU)` 표시
- 참가 시 빈 Slot 갱신
- 이탈 시 Slot이 EMPTY로 복귀

### Leave

- Lobby에서 Leave 가능
- Fusion Session 정상 종료
- MainMenu 복귀
- NetworkRunner 중복 생성 없음

### Steam 종료

- Steam 인증 후 Play Mode 종료
- `Steamworks is not initialized` 예외 미발생
- Rich Presence 종료 처리 중 예외 미발생

---

## 91일차 완료 범위

91일차에서는 Lobby 화면과 실제 Fusion 참가자 데이터를 연결하는 데 집중했다.

이번 일차에서 구현하지 않은 내용은 다음 일차 이후로 유지한다.

- Game Scene 테스트 경기장 정리
- Game HUD 최종 연결
- Countdown UI 정리
- Result UI 정리
- 전체 Scene 전환 회귀 검증
- Customize 실제 기능

다음 개발 대상은 **92일차 - Game Scene 테스트 경기장 정리**이다.

---

## GitHub 기준

91일차 확인 기준 커밋:

```text
d2edbdf24fed53bd096371bbe5e2f3e239ca994e
```

91일차는 90일차의 PlayerLobbyPanel 화면 구조를 실제 Fusion 네트워크 Lobby로 확장하고, 종료 과정에서 발견된 Steamworks Rich Presence 예외까지 함께 수정한 단계이다.
