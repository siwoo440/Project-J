# 프로젝트 J - 89일차 개발일지

## 개발 목표

89일차는 88일차의 `PRIVATE MATCH → CREATE` 흐름을 확장하여, 방을 즉시 생성하지 않고 먼저 **호스트 전용 방 설정 화면**을 거치도록 수정하는 작업이다.

핵심 목표는 다음과 같다.

- Host Room Create 화면 구성
- 게임 모드 / 방 설정 / Room Preview 영역 분리
- 방 이름 설정
- 최대 인원 설정
- 비밀번호 사용 여부 설정
- 라운드 수 설정
- 난이도 설정
- Room Preview 실시간 갱신
- Preview 캐릭터 클릭 시 Player Lobby 미리보기 화면 전환
- 최종 `CREATE ROOM` 버튼에서 기존 Fusion Host 생성 흐름 호출

---

## 주요 구현 내용

### 1. CREATE ROOM 흐름 변경

기존 88일차에서는 PRIVATE MATCH의 CREATE 버튼을 누르면 바로 Fusion Host 생성을 요청했다.

기존 흐름:

```text
PRIVATE MATCH
↓
CREATE
↓
RequestCreatePrivateRoom()
↓
Lobby
```

89일차부터는 다음과 같이 변경했다.

```text
PRIVATE MATCH
↓
CREATE
↓
Host Room Create
↓
방 설정
↓
CREATE ROOM
↓
RequestCreatePrivateRoom()
↓
Lobby
```

이제 PRIVATE MATCH 화면의 CREATE 버튼은 방 생성 버튼이 아니라 **Host Room Create 화면 진입 버튼** 역할을 한다.

---

### 2. Host Room Create 화면 구성

새로운 Host Room Create 화면을 다음 세 영역으로 구성했다.

```text
GAME MODE
ROOM SETTINGS
ROOM PREVIEW
```

#### GAME MODE

현재는 `PRIVATE MATCH` 고정으로 표시한다.

추후 사용자 지정 게임 모드 확장 시 같은 영역을 재사용할 수 있도록 구조만 준비했다.

#### ROOM SETTINGS

다음 항목을 설정할 수 있도록 구성했다.

- Room Name
- Max Players
- Password ON / OFF
- Rounds
- Difficulty

현재 설정 범위는 다음과 같다.

```text
Max Players : 2 ~ 8
Rounds      : 1 / 3 / 5
Difficulty  : EASY / NORMAL / HARD
```

---

### 3. Room Preview 구현

방 설정값을 변경하면 오른쪽 `ROOM PREVIEW`에 즉시 반영되도록 했다.

표시 내용:

```text
Room Name
1 / Max Players
Round
Difficulty
Password
```

예:

```text
MY ROOM
1 / 6 PLAYERS
3 ROUND
HARD
PASSWORD : ON
```

89일차에서는 이 값들이 로컬 UI와 Preview에만 반영된다.

실제 Fusion Session Property 및 참가자 동기화는 이후 일정에서 연결한다.

---

### 4. 임시 캐릭터 Preview

Room Preview 영역에 호스트 캐릭터 위치를 확인하기 위한 임시 캐릭터를 배치했다.

현재는 실제 플레이어 모델이 아닌 단순 UI 형태의 Placeholder를 사용한다.

이 캐릭터 영역 전체를 클릭 가능한 버튼으로 구성했다.

---

### 5. Player Lobby Preview 연결

Room Preview의 캐릭터 영역을 클릭하면 다음 화면으로 전환되도록 연결했다.

```text
Host Room Create
↓
Character Preview 클릭
↓
Player Lobby Preview
```

현재 Player Lobby Preview는 다음 일차의 실제 Lobby UI를 연결하기 위한 임시 화면이다.

기본 구성:

```text
PLAYER 01
HOST

나머지 슬롯
EMPTY / WAITING
```

`BACK TO ROOM SETTINGS` 버튼을 누르면 다시 Host Room Create 화면으로 돌아간다.

---

### 6. BACK 동작

Host Room Create 화면의 BACK 버튼은 이전 PRIVATE MATCH 화면으로 돌아간다.

```text
Host Room Create
↓
BACK
↓
PRIVATE MATCH
```

따라서 MainMenu의 PLAY 흐름을 유지하면서 단계별 화면 전환이 가능하다.

---

### 7. 실제 Fusion 방 생성 위치 이동

실제 Fusion Host 생성 요청은 Host Room Create 화면 하단의 `CREATE ROOM` 버튼에서만 실행하도록 변경했다.

```text
ConfirmCreateRoom()
↓
ProjectJDay82SceneFlowCoordinator
↓
RequestCreatePrivateRoom()
```

기존 Fusion Scene Flow 구조는 그대로 재사용한다.

---

## 변경 파일

### 생성

```text
Assets/ProjectJ/Network/Fusion/UI/
└─ ProjectJHostRoomCreatePanel.cs

Assets/ProjectJ/Editor/
└─ ProjectJDay89HostRoomCreateInstaller.cs
```

### 수정

```text
Assets/ProjectJ/Network/Fusion/UI/
└─ ProjectJPrivateMatchPanel.cs
```

### 삭제

```text
없음
```

---

## Unity Scene 구성

89일차 Editor 메뉴:

```text
Project J
→ Scene
→ 89일차 Host Room Create UI 구성
```

메뉴 실행 시 `MainMenu.unity`의 `PlayPanel` 아래에 다음 구조를 생성한다.

```text
PlayPanel
├─ ModeSelectRoot
├─ PrivateMatchRoot
├─ HostRoomCreateRoot
└─ PlayerLobbyPreviewRoot
```

Host Room Create 내부에는 다음 주요 오브젝트가 포함된다.

```text
HostRoomCreateRoot
├─ GameModePanel
├─ RoomSettingsPanel
├─ RoomPreviewPanel
├─ HostRoomStatusText
└─ ConfirmCreateRoomButton
```

---

## 테스트 항목

다음 흐름을 확인한다.

```text
PLAY
→ PRIVATE MATCH
→ SELECT
→ CREATE
→ Host Room Create 표시
```

추가 확인:

- Room Name 변경 시 Preview 갱신
- Max Players 변경 시 Preview 갱신
- Password ON / OFF 갱신
- Round 변경
- Difficulty 변경
- Character Preview 클릭 시 Player Lobby Preview 전환
- BACK TO ROOM SETTINGS 정상 동작
- Host Room Create의 BACK으로 PRIVATE MATCH 복귀
- 최종 CREATE ROOM에서 기존 Fusion Host 생성 요청 실행
- Console Error 없음

---

## 89일차 결과

PRIVATE MATCH의 CREATE 기능을 단순 즉시 방 생성 방식에서 **호스트 방 설정 → Room Preview → 최종 생성** 구조로 확장했다.

방 생성 전에 호스트가 주요 설정을 확인할 수 있게 되었고, 설정값을 Room Preview에서 실시간으로 확인할 수 있도록 구성했다.

또한 Preview 캐릭터 영역을 Player Lobby 화면으로 이어지는 진입점으로 만들어 다음 Lobby UI 개발을 위한 기반을 준비했다.

---

## 다음 개발 방향

90일차에는 `PlayerLobbyPanel`을 실제 Lobby 화면 형태로 확장한다.

예정 범위:

- 8개 Player Slot 구성
- 한 페이지당 8명 표시
- 좌우 페이지 화살표
- 빈 슬롯 표시
- Ready Summary
- Match Info
- Customize / Ready / Back UI
- 실제 Fusion 참가자 데이터 연결 전 UI 기반 완성
