# 프로젝트 J - 87일차 개발일지

## 개발 목표

87일차는 86일차에 구성한 MainMenu의 `PLAY` 화면을 실제 게임 모드 선택 UI로 확장하는 작업이다.

게임 모드 카드를 위아래로 쌓는 방식이 아니라, 오버워치의 게임 모드 선택 화면처럼 **왼쪽에서 오른쪽으로 나란히 배치한 세로로 긴 직사각형 카드** 형태로 구성했다.

핵심 목표는 다음과 같다.

- PLAY Panel 실제 게임 모드 선택 화면 구성
- 세로형 카드 4개를 좌→우 배치
- Hover 시 확대 및 위치 강조
- 클릭 시 Selected 상태 유지
- 선택 카드 설명 영역 갱신
- 선택 가능 / COMING SOON 상태 분리
- PRIVATE MATCH 선택 가능 처리
- 카드 상단 이미지용 RawImage 슬롯 준비
- 실제 Host / Join 연결은 88일차로 분리

---

## 최신 커밋 기준

- Commit: `32063ee2f6ef00e4f1de5e08e050d94988930c3b`
- 현재 Commit Title: `a`

이번 개발일지는 해당 최신 `main` 커밋을 기준으로 작성했다.

---

## 주요 구현 내용

### 1. PLAY Panel 재구성

기존 86일차의 `PlayPanel` Placeholder를 실제 게임 모드 선택 UI로 변경했다.

구조는 다음과 같다.

```text
PlayPanel
├─ PlayTitle
├─ PlaySubtitle
├─ ModeCardContainer
│  ├─ QuickPlayCard
│  ├─ PrivateMatchCard
│  ├─ TrainingCard
│  └─ CustomGameCard
└─ ModeDetailPanel
   ├─ DetailTitle
   ├─ DetailDescription
   ├─ DetailStatus
   └─ SelectButton
```

---

### 2. 게임 모드 카드 좌→우 배치

게임 모드 카드는 세로로 긴 직사각형 형태로 제작했다.

기본 카드 크기:

```text
Width  : 300
Height : 540
```

네 장의 카드를 한 줄에 다음 순서로 배치했다.

```text
QUICK PLAY
PRIVATE MATCH
TRAINING
CUSTOM GAME
```

카드의 X 위치를 개별적으로 고정하여 왼쪽에서 오른쪽으로 일정한 간격을 유지하도록 했다.

---

### 3. 게임 모드 정의

현재 모드 상태는 다음과 같다.

| 게임 모드 | 상태 |
| --- | --- |
| QUICK PLAY | COMING SOON |
| PRIVATE MATCH | AVAILABLE |
| TRAINING | COMING SOON |
| CUSTOM GAME | COMING SOON |

87일차에서는 현재 실제 온라인 구조와 연결할 예정인 `PRIVATE MATCH`만 선택 가능 상태로 설정했다.

나머지 모드는 향후 기능 구현 전까지 `COMING SOON`으로 유지한다.

---

### 4. 카드 Hover 효과

`ProjectJGameModeCard`를 추가해 마우스 포인터 상태를 처리하도록 했다.

카드에 마우스를 올리면 다음 효과가 적용된다.

```text
기본 Scale : 1.00
Hover Scale : 1.10
Hover 이동 : Y +20
```

Hover 상태에서는 카드 색상도 조금 더 밝아지고, 다른 카드보다 앞으로 표시되도록 구성했다.

변화는 즉시 튀는 방식이 아니라 `Lerp`를 사용해 부드럽게 전환된다.

---

### 5. Selected 상태

Hover와 실제 선택 상태를 분리했다.

카드를 클릭하면 해당 카드가 Selected 상태가 되고, 다른 카드에 마우스를 올려도 선택 상태 자체는 유지된다.

Selected 상태에서는 다음 효과를 적용한다.

- 약간 확대된 Scale 유지
- 카드 색상 강조
- 청록색 계열 Outline 표시
- 선택 카드 정보를 ModeDetailPanel에 유지

다른 카드를 선택하면 이전 카드의 Selected 상태는 해제된다.

---

### 6. ModeDetailPanel

선택한 카드 정보를 화면 하단의 Detail Panel에 표시하도록 했다.

표시 내용:

- 게임 모드 이름
- 모드 설명
- AVAILABLE / COMING SOON 상태
- SELECT 버튼

초기 상태에서는 다음 안내를 표시한다.

```text
게임 모드를 선택하세요

카드에 마우스를 올리면 강조되고,
클릭하면 선택 상태가 유지됩니다.
```

---

### 7. PRIVATE MATCH 선택

현재 실제 선택 가능한 모드는 `PRIVATE MATCH`이다.

선택 흐름:

```text
PLAY
↓
PRIVATE MATCH 클릭
↓
Selected 상태 유지
↓
설명 표시
↓
SELECT 활성화
```

SELECT를 누르면 아직 Host / Join 화면으로 이동하지 않고 개발 확인용 로그만 출력한다.

```text
[Project J/Day87]
PRIVATE MATCH 선택 완료
Host/Join UI는 88일차에 연결
```

따라서 네트워크 Room 생성과 참가 로직은 아직 변경하지 않는다.

---

### 8. COMING SOON 카드

QUICK PLAY, TRAINING, CUSTOM GAME은 카드를 선택할 수는 있지만 실제 게임 시작은 할 수 없다.

해당 카드를 클릭하면:

```text
COMING SOON
```

상태가 Detail Panel에 표시되며 SELECT 버튼은 비활성화된다.

---

### 9. 카드 이미지 슬롯

각 카드 상단에는 향후 게임 모드 대표 이미지를 넣을 수 있도록 `ModeVisual` 영역을 만들었다.

현재 구조:

```text
GameModeCard
├─ ModeVisual
│  └─ RawImage
├─ ModeTitle
└─ StatusText
```

현재 `RawImage`의 Texture는 의도적으로 `None` 상태로 비워 두었다.

또한 Texture가 없는 동안 화면에 불필요한 사각형이 표시되지 않도록 RawImage Color Alpha를 `0`으로 설정했다.

향후 이미지를 추가할 때는 각 카드의:

```text
ModeVisual
→ Raw Image
→ Texture
```

에 모드 이미지를 연결하고 `Color A` 값을 `1`로 변경하면 된다.

---

## 추가된 스크립트

### ProjectJGameModeCard.cs

게임 모드 카드 개별 상호작용을 담당한다.

주요 역할:

- Pointer Enter
- Pointer Exit
- Pointer Click
- Hover 확대
- Hover 위치 이동
- Selected 상태
- 카드 색상 강조
- Outline 표시

---

### ProjectJPlayModePanel.cs

PLAY 화면 전체 카드 선택 상태를 관리한다.

주요 역할:

- 현재 선택 카드 저장
- 이전 카드 선택 해제
- Detail Panel 갱신
- COMING SOON 판정
- SELECT 활성 / 비활성
- PRIVATE MATCH 선택 확인

---

### ProjectJDay87PlayModeCardInstaller.cs

87일차 PLAY 화면을 Unity Editor에서 자동 구성하기 위한 Editor 도구이다.

메뉴:

```text
Project J
→ Scene
→ 87일차 PLAY 게임 모드 카드 구성
```

을 실행하면 기존 PlayPanel의 내용을 지우고 현재 87일차 카드 구조를 다시 생성하고 저장한다.

---

## 변경 파일

### 생성

- `Assets/ProjectJ/Editor/ProjectJDay87PlayModeCardInstaller.cs`
- `Assets/ProjectJ/Editor/ProjectJDay87PlayModeCardInstaller.cs.meta`
- `Assets/ProjectJ/Runtime/SceneFlow/ProjectJGameModeCard.cs`
- `Assets/ProjectJ/Runtime/SceneFlow/ProjectJGameModeCard.cs.meta`
- `Assets/ProjectJ/Runtime/SceneFlow/ProjectJPlayModePanel.cs`
- `Assets/ProjectJ/Runtime/SceneFlow/ProjectJPlayModePanel.cs.meta`

### 수정

- `Assets/ProjectJ/Scenes/MainMenu.unity`

### 삭제

별도 파일 삭제 없음.

---

## 확인 결과

최신 GitHub 커밋 기준으로 다음 사항을 확인했다.

- 87일차 Editor Installer 존재
- `ProjectJGameModeCard` 존재
- `ProjectJPlayModePanel` 존재
- MainMenu Scene 수정 반영
- 카드 크기 300×540 구성
- 좌→우 4카드 배치
- PRIVATE MATCH AVAILABLE
- 나머지 3개 모드 COMING SOON
- Hover 확대 및 Y 이동 코드 존재
- Selected 상태 및 Outline 코드 존재
- ModeDetailPanel 구성
- SELECT 버튼 상태 제어
- 카드 상단 `ModeVisual` 4개 존재
- ModeVisual RawImage Texture는 `None`
- 임시 Background 이미지 연결 제거

GitHub에는 별도의 CI 상태 체크가 등록되어 있지 않으므로 Unity 실제 PlayMode, Build 및 마우스 입력 결과가 자동 검증되었다고 기록하지 않는다.

---

## 87일차 결과

PLAY 메뉴를 실제 게임 모드 선택 화면 형태로 확장했다.

각 게임 모드를 세로로 긴 카드로 구성해 왼쪽에서 오른쪽으로 나란히 배치했으며, 마우스를 올렸을 때 확대되고 위로 올라오는 Hover 연출과 클릭 후 유지되는 Selected 상태를 구현했다.

현재 PRIVATE MATCH만 실제 선택 가능 상태이며, 나머지 모드는 COMING SOON으로 분리했다.

각 카드의 상단에는 추후 대표 이미지를 연결할 수 있는 빈 RawImage 슬롯도 준비했다.

---

## 다음 개발 방향

88일차에는 `PRIVATE MATCH` 선택 후 실제 온라인 메뉴를 연결한다.

예정 범위:

- PRIVATE MATCH Panel
- 방 만들기
- Host
- Room Code 표시
- Room Code 입력
- Join
- 뒤로가기
- 기존 Fusion Host / Join 요청 연결
- 온라인 연결 상태 표시
