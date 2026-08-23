# 프로젝트 J - 86일차 개발일지

## 개발 목표

86일차는 기존의 단순 `START → Game` 형태 MainMenu를 실제 게임용 메뉴 구조로 교체하고, 이후 PLAY·꾸미기·프로필·설정 기능을 연결할 수 있는 MainMenu 기반을 구성하는 작업이다.

핵심 목표는 다음과 같다.

- MainMenu 상단 Navigation 구성
- HOME 기본 선택 상태 구현
- 현재 선택 탭 색상 강조
- HOME 중앙 캐릭터 Preview 구성
- PLAY / CUSTOMIZE / PROFILE / SETTINGS Panel 구조 생성
- EXIT 기능 연결
- 기존 `START → Game` 직접 이동 구조 제거
- MainMenu 전용 배경 분리
- 이후 87일차 게임 모드 카드 UI를 붙일 기반 마련

---

## 최신 커밋 기준

- Commit: `248f4e40dbf0f8e123c0c31cfadcf7259afcaf57`
- 현재 Commit Title: `a`

이번 개발일지는 해당 최신 `main` 커밋의 변경 사항을 기준으로 작성했다.

---

## 주요 구현 내용

### 1. MainMenu Scene 전체 구조 재구성

기존 `UI_MainMenu`, `StartButton`, `SceneNavigation`, `Directional Light` 중심 구조를 정리하고 기능별 루트 구조로 변경했다.

```text
MainMenu
├─ === CAMERA ===
│  └─ Main Camera
│
├─ === CHARACTER PREVIEW ===
│  ├─ CharacterPreviewRoot
│  │  └─ PreviewVisual
│  │     ├─ Body
│  │     ├─ Head
│  │     ├─ LeftArm
│  │     └─ RightArm
│  └─ CharacterPreviewLight
│
├─ === UI ===
│  └─ Canvas_MainMenu
│     ├─ Background
│     ├─ BackgroundShade
│     ├─ TopNavigation
│     ├─ ContentRoot
│     └─ VersionText
│
├─ === EVENT SYSTEM ===
│  └─ EventSystem
│
└─ === MENU SYSTEM ===
   └─ MainMenuController
```

---

### 2. 상단 Navigation 구성

MainMenu 상단에 다음 메뉴를 구성했다.

```text
HOME
PLAY
CUSTOMIZE
PROFILE
SETTINGS
EXIT
```

MainMenu 진입 시 기본 상태는 `HOME`이다.

현재 선택된 탭은 일반 탭과 다른 색상 및 하단 선택 Bar로 표시하여 현재 메뉴 상태를 바로 확인할 수 있도록 했다.

---

### 3. 탭 전환 시스템 구현

`ProjectJMainMenuController`를 추가해 MainMenu의 Panel 활성 상태와 Navigation 강조 상태를 관리하도록 했다.

지원하는 기본 기능은 다음과 같다.

- `OpenHome()`
- `OpenPlay()`
- `OpenCustomize()`
- `OpenProfile()`
- `OpenSettings()`
- `QuitGame()`

한 번에 하나의 MainMenu Panel만 활성화되며 선택된 Navigation 버튼의 색상과 Selected Bar도 함께 갱신된다.

---

### 4. HOME 화면과 캐릭터 Preview

HOME 화면에는 플레이어 캐릭터가 중앙에 표시될 수 있도록 로컬 Preview 전용 구조를 추가했다.

현재 Preview는 실제 캐릭터 모델이 준비되기 전까지 사용할 임시 형태로 구성되어 있다.

중요하게도 MainMenu의 Preview 캐릭터에는 Fusion `NetworkObject`나 실제 Network Player 기능을 사용하지 않는다.

따라서 MainMenu 진입만으로 Network Player가 Spawn되거나 Authority가 생성되지 않는다.

향후 실제 캐릭터 Visual이 준비되면 `PreviewVisual` 영역만 교체할 수 있다.

---

### 5. HOME / CUSTOMIZE 캐릭터 표시

캐릭터 Preview는 다음 메뉴에서 표시되도록 구성했다.

```text
HOME
CUSTOMIZE
```

PLAY, PROFILE, SETTINGS으로 이동하면 Preview는 숨겨진다.

이를 통해 추후 HOME의 캐릭터 표시와 CUSTOMIZE의 캐릭터 꾸미기 화면이 동일한 Preview 기반을 공유할 수 있도록 했다.

---

### 6. PLAY Panel 기반 구성

86일차에서는 PLAY 버튼을 눌렀을 때 실제 게임으로 바로 진입하지 않고 `PlayPanel`을 표시하도록 변경했다.

현재 PLAY Panel은 87일차 게임 모드 카드 UI를 추가하기 위한 Placeholder 상태이다.

향후 흐름은 다음과 같이 확장할 예정이다.

```text
PLAY
↓
게임 모드 카드
↓
PRIVATE MATCH
↓
Host / Join / Room Code
↓
Lobby
```

---

### 7. CUSTOMIZE / PROFILE / SETTINGS Placeholder

아직 실제 기능이 구현되지 않은 메뉴는 먼저 Panel 구조만 구성했다.

- CUSTOMIZE: 향후 캐릭터 꾸미기 기능 연결
- PROFILE: 플레이어 이름, 레벨, 승리 수, 경기 수, 최고 높이 등을 표시할 위치
- SETTINGS: 그래픽, 사운드, 조작 설정을 연결할 위치

이를 통해 후속 일차에서 MainMenu 구조를 다시 크게 수정하지 않고 각 기능만 추가할 수 있다.

---

### 8. EXIT 기능

EXIT 버튼을 MainMenu 상단에 추가했다.

Editor에서는 실제 Unity Editor가 종료되지 않고 Console에 종료 요청 로그를 출력한다.

Build에서는 `Application.Quit()`을 사용해 게임을 종료하도록 구성했다.

---

### 9. MainMenu 전용 배경 분리

Bootstrap에서 사용하는 배경 이미지를 MainMenu에서 재사용하지 않도록 분리했다.

현재 MainMenu는 연한 보라색 계열의 단색 배경을 사용한다.

기본 색상은 대략 다음 값이다.

```text
R 0.82
G 0.77
B 0.94
A 0.90
```

따라서 Bootstrap과 MainMenu가 서로 다른 화면 분위기를 가지며, 나중에 MainMenu 전용 배경 이미지가 생기면 독립적으로 교체할 수 있다.

---

### 10. MainMenu Scene 자동 구성 도구

`ProjectJDay86MainMenuSceneInstaller`를 추가했다.

Unity Editor 메뉴:

```text
Project J
→ Scene
→ 86일차 MainMenu Scene 구성
```

을 실행하면 MainMenu Scene의 기본 Hierarchy, Navigation, Panel, 캐릭터 Preview, 배경, Controller 구성을 자동으로 생성하고 저장한다.

---

## 변경 파일

### 추가

- `Assets/ProjectJ/Art/UI/MainMenu.meta`
- `Assets/ProjectJ/Art/UI/MainMenu/Day86PreviewCharacter.mat`
- `Assets/ProjectJ/Art/UI/MainMenu/Day86PreviewCharacter.mat.meta`
- `Assets/ProjectJ/Editor/ProjectJDay86MainMenuSceneInstaller.cs`
- `Assets/ProjectJ/Editor/ProjectJDay86MainMenuSceneInstaller.cs.meta`
- `Assets/ProjectJ/Runtime/SceneFlow/ProjectJMainMenuController.cs`
- `Assets/ProjectJ/Runtime/SceneFlow/ProjectJMainMenuController.cs.meta`

### 수정

- `Assets/ProjectJ/Scenes/MainMenu.unity`

### 삭제

별도 파일 삭제 없음.

Scene 내부에서는 기존 MainMenu용 `UI_MainMenu`, `SceneNavigation`, `Directional Light` 구조를 새 구조로 교체했다.

---

## 확인 결과

최신 GitHub 커밋 기준으로 다음 내용을 확인했다.

- `ProjectJMainMenuController` 추가
- `ProjectJDay86MainMenuSceneInstaller` 추가
- `MainMenu.unity` 수정
- `Canvas_MainMenu` 존재
- 캐릭터 Preview 루트 존재
- HOME 기본 Panel 존재
- MainMenu 전용 연한 보라색 배경 설정
- 캐릭터 Preview용 Material 생성
- GitHub CI 상태 체크는 별도로 등록되어 있지 않음

Unity 실제 PlayMode 및 Build 동작은 GitHub만으로 자동 검증할 수 없으므로, 사용자 로컬 테스트 결과를 기준으로 최종 확인한다.

---

## 현재 알려진 잔여 사항

Steam Web API Ticket이 `WaitingForWebApiTicket` 상태에서 응답하지 않을 경우 Bootstrap Scene이 계속 대기할 수 있는 기존 문제가 확인되었다.

이 문제는 86일차 MainMenu UI 자체와는 별개지만 `Bootstrap → MainMenu` 전체 진입 흐름에는 영향을 줄 수 있다.

최신 원격 커밋에서는 해당 Steam Ticket Timeout 수정이 아직 확인되지 않았다.

따라서 86일차 MainMenu 구현은 완료 상태로 기록하되, Steam 초기화 Timeout 처리는 별도 후속 수정 또는 검증 대상으로 남긴다.

---

## 86일차 결과

기존 단순 START 메뉴를 오버워치식 상단 Navigation 기반 MainMenu 구조로 변경했다.

HOME을 기본 진입 상태로 설정하고 현재 선택된 메뉴를 색상으로 강조하도록 했으며, 중앙에는 로컬 캐릭터 Preview를 표시할 수 있는 기반을 구성했다.

PLAY, CUSTOMIZE, PROFILE, SETTINGS는 각각 독립 Panel 구조로 나누어 이후 기능을 확장할 수 있게 했으며, Bootstrap과 MainMenu의 배경도 분리했다.

---

## 다음 개발 방향

87일차에는 PLAY Panel에 게임 모드 카드 UI를 추가한다.

예정 범위:

- 게임 모드 카드 배치
- 마우스 Hover 시 카드 확대
- 밝기 및 외곽선 강조
- Selected 상태 유지
- PRIVATE MATCH 카드
- 미구현 모드 `COMING SOON` 처리
