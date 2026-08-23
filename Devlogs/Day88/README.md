# 프로젝트 J - 88일차 개발일지

## 개발 목표

88일차는 87일차에 구현한 `PRIVATE MATCH` 게임 모드 선택 이후에 실제 비공개 온라인 방 생성·참가 UI를 연결하는 작업이다.

핵심 목표는 다음과 같다.

- PRIVATE MATCH 전용 화면 구성
- CREATE ROOM / JOIN ROOM 카드 2개 구성
- 두 카드를 가로로 나란히 배치
- 각 카드를 정사각형 형태로 제작
- 기존 Fusion Host / Join 흐름 연결
- 6자리 Room Code 입력 및 검증
- 연결 상태 표시
- BACK으로 게임 모드 선택 화면 복귀
- Runtime asmdef와 Fusion 코드 간 의존성 오류 수정
- PRIVATE MATCH SELECT 전환 연결 문제 수정

---

## 최신 커밋 기준

- Commit: `14f898fd4dfb2073789da65f0004477342a95dcc`
- 현재 Commit Title: `a`

이번 개발일지는 해당 최신 `main` 커밋을 기준으로 작성했다.

---

## 주요 구현 내용

### 1. PRIVATE MATCH 화면 추가

87일차 PLAY 화면에서 다음 흐름으로 진입하도록 구성했다.

```text
PLAY
↓
PRIVATE MATCH
↓
SELECT
↓
PRIVATE MATCH 전용 화면
```

PRIVATE MATCH 화면은 기존 MainMenu 내부에서 Panel을 전환하는 방식으로 구성했다.

별도의 Scene을 추가하지 않는다.

---

### 2. ModeSelectRoot / PrivateMatchRoot 분리

기존 게임 모드 선택 UI를 `ModeSelectRoot`로 묶고, PRIVATE MATCH용 화면을 `PrivateMatchRoot`로 분리했다.

```text
PlayPanel
├─ ModeSelectRoot
│  ├─ PlayTitle
│  ├─ PlaySubtitle
│  ├─ ModeCardContainer
│  └─ ModeDetailPanel
│
└─ PrivateMatchRoot
   ├─ PrivateMatchTitle
   ├─ PrivateMatchSubtitle
   ├─ PrivateMatchCardContainer
   ├─ ConnectionStatusText
   └─ BackButton
```

초기 상태에서는 `ModeSelectRoot`가 활성화되고 `PrivateMatchRoot`는 비활성화된다.

---

### 3. CREATE ROOM / JOIN ROOM 정사각형 카드 구성

PRIVATE MATCH 화면 중앙에 두 개의 정사각형 카드를 가로로 나란히 배치했다.

```text
┌────────────────────┐   ┌────────────────────┐
│    CREATE ROOM     │   │     JOIN ROOM      │
│                    │   │                    │
│ 새로운 비공개 방을  │   │ 6자리 Room Code   │
│ 생성합니다.         │   │ 입력               │
│                    │   │                    │
│     [ CREATE ]     │   │ [ CODE ] [ JOIN ]  │
└────────────────────┘   └────────────────────┘
```

각 카드 크기는 다음과 같다.

```text
420 × 420
```

왼쪽은 방 생성, 오른쪽은 방 참가 기능을 담당한다.

---

### 4. CREATE ROOM 연결

CREATE 버튼은 새 NetworkRunner를 MainMenu에서 직접 만드는 방식이 아니라 기존 Scene Flow를 사용한다.

```text
CREATE
↓
ProjectJDay82SceneFlowCoordinator
↓
RequestCreatePrivateRoom()
↓
ProjectJFusionBootstrap
↓
Fusion Host 시작
↓
Lobby
```

이를 통해 기존 82일차 이후의 네트워크 흐름을 그대로 재사용한다.

---

### 5. JOIN ROOM 연결

JOIN 카드에는 6자리 Room Code 입력창을 추가했다.

입력 규칙:

- 최대 6자리
- 영문 / 숫자
- 소문자는 화면에서 대문자로 정규화
- 최종 검증은 기존 `ProjectJFusionRoomCode.TryNormalize()` 사용

흐름은 다음과 같다.

```text
Room Code 입력
↓
JOIN
↓
ProjectJFusionRoomCode.TryNormalize()
↓
RequestJoinPrivateRoom()
↓
Fusion Client 연결
↓
Lobby
```

잘못된 Room Code는 네트워크 연결을 시작하지 않고 Status Text에 오류 내용을 표시한다.

---

### 6. 연결 상태 UI

PRIVATE MATCH 화면 하단에 `ConnectionStatusText`를 배치했다.

다음과 같은 상태를 표시할 수 있도록 구성했다.

```text
비공개 방을 만들거나 Room Code로 참가하세요.

비공개 방 생성 요청 중...

XXXXXX 방 참가 요청 중...

연결 완료 · Lobby로 이동합니다...

온라인 Scene Flow를 찾을 수 없습니다.
```

연결 중에는 CREATE / JOIN / Room Code / BACK 입력을 비활성화해 중복 요청을 방지하도록 했다.

---

### 7. BACK 처리

BACK 버튼은 MainMenu HOME으로 나가는 것이 아니라 이전 게임 모드 선택 화면으로 돌아간다.

```text
PrivateMatchRoot
↓
BACK
↓
ModeSelectRoot
```

PLAY 탭 자체는 그대로 유지된다.

---

## 컴파일 오류 수정

88일차 최초 구현에서는 다음 오류가 발생했다.

```text
CS0234
The type or namespace name 'Networking'
does not exist in the namespace 'ProjectJ'

CS0246
ProjectJDay82SceneFlowCoordinator could not be found
```

원인은 `Assets/ProjectJ/Runtime`이 별도의 `ProjectJ.Runtime.asmdef`를 사용하고 있는데, Runtime 어셈블리에서 Fusion / Network 영역을 직접 참조하려 했기 때문이다.

구조를 다음과 같이 수정했다.

```text
ProjectJ.Runtime
└─ ProjectJPlayModePanel
   └─ 네트워크 타입 직접 참조 없음
   └─ SelectionConfirmed 이벤트 발생

            ↓

Network / Fusion
└─ ProjectJPrivateMatchPanel
   ├─ 선택 이벤트 수신
   ├─ CREATE
   ├─ JOIN
   ├─ Room Code 검증
   └─ 기존 Scene Flow 호출
```

`ProjectJPrivateMatchPanel.cs`를 다음 위치로 이동했다.

```text
Assets/ProjectJ/Network/Fusion/UI/
```

이를 통해 Runtime asmdef와 Fusion 코드 사이의 잘못된 의존성을 제거했다.

---

## PRIVATE MATCH SELECT 연결 수정

컴파일 오류 해결 후 PRIVATE MATCH를 선택하고 SELECT를 눌러도 화면이 전환되지 않는 문제가 확인되었다.

Scene의 `ProjectJPrivateMatchPanel`에서 `playModePanel` 직렬화 참조가 비어 있는 상태에서도 동작할 수 있도록 다음 처리를 추가했다.

```text
Awake()
↓
ResolvePlayModePanel()
↓
같은 PlayPanel GameObject에서
ProjectJPlayModePanel 자동 탐색
↓
SelectionConfirmed 이벤트 연결
```

따라서 Inspector의 `playModePanel` 참조가 비어 있어도 같은 `PlayPanel`에 두 Component가 존재하면 Runtime에서 자동으로 연결된다.

현재 Scene의 나머지 주요 참조는 다음과 같이 저장되어 있다.

- ModeSelectRoot
- PrivateMatchRoot
- CreateRoomButton
- JoinRoomButton
- BackButton
- RoomCodeInput
- ConnectionStatusText

---

## 변경 파일

### 생성

- `Assets/ProjectJ/Network/Fusion/UI/ProjectJPrivateMatchPanel.cs`
- `Assets/ProjectJ/Network/Fusion/UI/ProjectJPrivateMatchPanel.cs.meta`
- `Assets/ProjectJ/Editor/ProjectJDay88PrivateMatchPanelInstaller.cs`
- `Assets/ProjectJ/Editor/ProjectJDay88PrivateMatchPanelInstaller.cs.meta`

### 수정

- `Assets/ProjectJ/Runtime/SceneFlow/ProjectJPlayModePanel.cs`
- `Assets/ProjectJ/Scenes/MainMenu.unity`

### 기존 잘못된 위치 정리

- `Assets/ProjectJ/Runtime/SceneFlow/ProjectJPrivateMatchPanel.cs`

Fusion 의존 코드가 Runtime asmdef에 남지 않도록 실제 네트워크 UI 구현을 `Network/Fusion/UI` 영역으로 이동했다.

---

## 확인 결과

최신 GitHub 커밋 기준으로 다음 내용을 확인했다.

- PRIVATE MATCH 전용 UI 존재
- CREATE ROOM 카드 존재
- JOIN ROOM 카드 존재
- 두 카드 420×420 정사각형 구성
- ModeSelectRoot / PrivateMatchRoot 분리
- Room Code Input 존재
- 6자리 입력 제한
- CREATE → 기존 Scene Flow 연결
- JOIN → 기존 Scene Flow 연결
- Room Code 기존 검증 시스템 재사용
- Connection Status Text 연결
- BACK 버튼 연결
- Runtime asmdef → Fusion 직접 참조 오류 제거
- `ProjectJPrivateMatchPanel` Network/Fusion/UI 영역 이동
- `ProjectJPlayModePanel` 선택 확정 이벤트 구조 적용
- PRIVATE MATCH Panel이 같은 GameObject의 PlayModePanel을 Runtime에서 자동 탐색

현재 Scene의 `playModePanel` 직렬화 값은 비어 있지만 최신 Runtime 코드가 자동 탐색하도록 되어 있으므로 현재 구조에서는 차단 문제가 아니다.

GitHub에는 별도의 CI 상태 체크가 등록되어 있지 않으므로 Unity Build, 실제 Steam 인증, 두 계정 간 Host / Join 성공까지 자동 검증됐다고 기록하지 않는다.

---

## 88일차 결과

PRIVATE MATCH 선택 이후 실제 비공개 온라인 메뉴로 진입할 수 있는 기반을 구성했다.

CREATE ROOM과 JOIN ROOM을 각각 정사각형 카드로 구성해 가로로 나란히 배치하고, 기존 Fusion Host / Join Scene Flow에 연결했다.

Room Code 입력·정규화·오류 표시·중복 입력 방지·BACK 복귀 구조를 추가했으며, 구현 중 발생한 asmdef 의존성 오류와 SELECT 화면 전환 연결 문제도 수정했다.

---

## 다음 개발 방향

89일차에는 `Lobby` Scene을 실제 화면 형태로 구성한다.

예정 범위:

- Lobby Scene Hierarchy 정리
- Lobby 전용 Camera / UI 구성
- 방 코드 표시 위치
- 플레이어 대기 공간
- Host / Client가 동일 Lobby에 존재하는지 시각적으로 확인
- 이후 90일차 Player Slot / Ready UI 연결 기반 준비
