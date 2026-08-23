---
# Project J 개발일지

---
## 90일차 — PlayerLobbyPanel 기본 화면·페이지 UI 구성

### 개발 목표

89일차에 만든 Host 방 설정과 Room Preview 다음 단계로, 실제 플레이어 로비 형태의 기본 UI를 구성한다.

이번 일차에서는 화면 구조와 페이지 처리까지만 구현하고 실제 Photon Fusion 참가자 목록 및 Ready 상태 동기화는 다음 일차로 분리한다.

---
## 구현 내용

### Player Lobby 기본 화면 구성

기존 `PlayerLobbyPreviewRoot`를 실제 Player Lobby 형태로 확장했다.

한 화면에서 다음 정보를 확인할 수 있도록 구성했다.

- 최대 8개의 Player Slot
- 현재 Ready 인원 요약
- Match Info
- 페이지 이동 버튼
- CUSTOMIZE 버튼
- READY 버튼
- BACK 버튼

Player Slot은 현재 실제 네트워크 참가자 대신 임시 데이터를 사용한다.

첫 번째 슬롯은 Host를 표현한다.

```text
#01
PLAYER 01
HOST
```

나머지 슬롯은 빈 참가자 자리를 표현한다.

```text
#02 ~ #08
WAITING...
EMPTY
```

---
## 페이지 시스템

한 페이지에는 최대 8개의 Player Slot을 표시한다.

내부 구조는 최대 32명까지 계산할 수 있도록 구성했다.

| 참가 가능 인원 | 표시 페이지 |
| --- | --- |
| 1~8명 | Page 1 |
| 9~16명 | Page 2 |
| 17~24명 | Page 3 |
| 25~32명 | Page 4 |

현재 Project J의 비공개 방 최대 인원은 8명이므로 평소에는 페이지 이동 화살표와 Page Text가 자동으로 숨겨진다.

추후 최대 인원을 8명 이상으로 확장하면 페이지 버튼이 자동으로 활성화된다.

첫 페이지에서는 이전 페이지 버튼을 사용할 수 없고 마지막 페이지에서는 다음 페이지 버튼을 사용할 수 없도록 처리했다.

---
## Room Settings 데이터 연동

89일차 Host Room Settings에서 지정한 값을 Player Lobby의 Match Info로 전달하도록 연결했다.

전달되는 값은 다음과 같다.

- Room Name
- Max Players
- Round
- Difficulty
- Password

예를 들어 다음과 같이 설정한 상태에서 Room Preview 캐릭터를 클릭하면,

```text
Room Name   TEST ROOM
Players     6
Rounds      3
Difficulty  HARD
Password    ON
```

Player Lobby에서도 같은 설정값을 확인할 수 있다.

Ready 요약은 현재 실제 네트워크 데이터가 연결되지 않았으므로 Host 한 명을 기준으로 다음과 같이 표시한다.

```text
READY 1 / 6
```

---
## 화면 전환

Room Preview의 캐릭터 영역을 클릭하면 Player Lobby 화면으로 이동하도록 연결했다.

```text
PLAY
→ PRIVATE MATCH
→ SELECT
→ CREATE
→ Room Preview 캐릭터 클릭
→ PLAYER LOBBY
```

Player Lobby에서 `BACK`을 누르면 기존 Host Room Settings 화면으로 돌아간다.

이 과정에서 입력했던 방 설정값은 유지한다.

---
## 버튼 처리

이번 일차에서는 `CUSTOMIZE`와 `READY` 버튼의 위치와 기본 UI만 준비했다.

두 버튼은 아직 실제 기능이 연결되지 않았으므로 비활성화 상태로 설정했다.

- CUSTOMIZE : 비활성화
- READY : 비활성화
- BACK : Host Room Settings 복귀 기능 연결

실제 참가자 Ready 상태와 Ready 버튼의 네트워크 동기화는 91일차에서 구현한다.

---
## 추가된 스크립트

### `ProjectJPlayerLobbyPanel.cs`

Player Lobby 화면의 표시와 페이지 처리를 담당한다.

주요 기능은 다음과 같다.

- 페이지당 8명 표시
- 최대 32명 페이지 계산
- 현재 페이지 계산
- 이전·다음 페이지 이동
- 첫·마지막 페이지 버튼 상태 처리
- 최대 인원에 따른 Slot 활성화
- Host 및 Empty Slot 임시 표시
- Ready 요약 표시
- Room Settings 정보 표시

### `ProjectJDay90PlayerLobbyInstaller.cs`

Unity Editor에서 90일차 Player Lobby UI를 자동 구성하는 Installer다.

Unity 상단 메뉴에서 다음 항목으로 실행할 수 있다.

```text
Project J
→ Scene
→ 90일차 Player Lobby UI 구성
```

Installer 실행 후 `MainMenu.unity`에 Player Lobby UI 구성이 반영된다.

---
## 수정된 스크립트

### `ProjectJHostRoomCreatePanel.cs`

89일차 Host Room Settings 화면과 90일차 Player Lobby 화면 사이의 연결을 추가했다.

주요 변경 내용은 다음과 같다.

- Player Lobby Panel 참조 추가
- Room Preview 캐릭터 클릭 시 Player Lobby 열기
- Room Settings 값을 Player Lobby로 전달
- Player Lobby BACK 처리
- 기존 방 설정값 유지

---
## 변경 파일

```text
생성
Assets/ProjectJ/Network/Fusion/UI/
└─ ProjectJPlayerLobbyPanel.cs

Assets/ProjectJ/Editor/
└─ ProjectJDay90PlayerLobbyInstaller.cs

수정
Assets/ProjectJ/Network/Fusion/UI/
└─ ProjectJHostRoomCreatePanel.cs

Assets/ProjectJ/Scenes/
└─ MainMenu.unity

삭제
없음
```

---
## 테스트 항목

다음 항목을 기준으로 90일차 기능을 확인한다.

### Player Lobby 진입

```text
Room Preview 캐릭터 클릭
→ Player Lobby 표시
```

### 뒤로가기

```text
Player Lobby
→ BACK
→ Host Room Settings 복귀
```

### 최대 인원 표시

Host Room Settings에서 Max Players 값을 변경한 뒤 Player Lobby에 다시 진입했을 때 활성 Player Slot 수와 `READY 1 / N` 값이 동일하게 변경되는지 확인한다.

### Room Settings 정보

Room Name, Max Players, Round, Difficulty, Password 값이 Match Info에 동일하게 표시되는지 확인한다.

### 페이지 처리

현재 최대 인원이 8명이므로 페이지 이동 화살표와 Page Text가 숨겨지는지 확인한다.

### Console

```text
Console Error
→ 0
```

---
## 90일차 완료 범위

이번 일차에서 Player Lobby의 기본 화면 구조와 페이지 처리 기반을 완성했다.

실제 네트워크 참가자를 연결하기 전 단계에서 최대 8명 단위의 Player Slot, Match Info, Ready 요약, 페이지 이동 구조와 Room Settings 연동까지 준비했다.

---
## 다음 개발 방향

91일차에서는 현재 임시 데이터로 표시되는 Player Lobby를 실제 Photon Fusion 데이터에 연결한다.

주요 작업 대상은 다음과 같다.

- Fusion `ActivePlayers` 연결
- 실제 참가자 수 표시
- PlayerRef와 Player Slot 매핑
- 참가·이탈 시 Slot 갱신
- READY / NOT READY 상태 동기화
- 로컬 Ready 버튼 활성화
- Leave 흐름 연결
- Host와 Client 구분
- 모든 참가자 Ready 시 다음 경기 흐름 연결
