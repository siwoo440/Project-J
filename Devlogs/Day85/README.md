# 프로젝트 J - 85일차 개발일지

## 개발 목표

85일차는 새 PHASE 8의 첫 작업으로, 프로젝트 실행 시 가장 먼저 진입하는 `Bootstrap` Scene을 실제 화면 형태로 구성하고 기존 Steam/Fusion 초기화 상태를 UI에서 확인할 수 있도록 정리하는 작업이다.

핵심 목표는 다음과 같다.

- Bootstrap Scene Hierarchy 정리
- 기존 Day3 Scene Flow 제거
- 82일차 Scene Flow Coordinator를 기준 전환 구조로 유지
- Bootstrap 전용 Canvas 구성
- 배경 이미지 적용
- Steam / Fusion / Scene Flow 상태 표시
- 로딩 표시 및 버전 표시
- Bootstrap → MainMenu 전환 구조와 충돌하지 않는 Scene 구성

---

## 최신 커밋 기준

- Commit: `9db24acbe86fb7f98106c74191291d001817659a`
- 현재 Commit Title: `a`

이번 개발일지는 해당 최신 커밋의 변경 사항을 기준으로 작성했다.

---

## 주요 구현 내용

### 1. Bootstrap Scene 구조 정리

기존 Bootstrap Scene을 기능별 루트로 재구성했다.

```text
Bootstrap
├─ === CAMERA ===
│  └─ Main Camera
├─ === ENVIRONMENT ===
│  └─ Global Volume
├─ === UI ===
│  └─ Canvas_Bootstrap
└─ === SCENE FLOW ===
   └─ BootstrapStatusView
```

기존 `Day3_SceneFlow`는 제거하고, 구형 Bootstrap Scene Controller가 MainMenu를 즉시 로드하지 않도록 정리했다.

MainMenu 전환 책임은 기존 82일차 `ProjectJDay82SceneFlowCoordinator` 구조에 맡긴다.

---

### 2. Bootstrap 전용 UI 구성

`Canvas_Bootstrap`을 추가하고 1920×1080 기준의 `Scale With Screen Size` 구조로 설정했다.

Canvas 내부에는 다음 요소를 구성했다.

- BackgroundViewport
- Background
- DimOverlay
- ContentRoot
- TitleText
- LoadingDots
- StatusText
- DetailText
- VersionText

이를 통해 Bootstrap Scene에서 단순 카메라 화면이 아니라 실제 초기화 화면을 확인할 수 있게 했다.

---

### 3. 배경 이미지 적용

프로젝트 배경 이미지:

`Assets/ProjectJ/Art/Background.png`

를 Bootstrap Scene의 `RawImage`에 직접 연결했다.

현재 Scene 파일에는 해당 이미지가 실제 Texture 참조로 저장되어 있으므로 Bootstrap 화면 배경으로 사용된다.

---

### 4. Steam / Fusion / Scene Flow 상태 표시

`ProjectJDay85BootstrapStatusView`를 추가했다.

이 View는 기존 시스템을 새로 생성하지 않고 다음 Runtime 상태를 읽어 화면에 표시한다.

- `ProjectJSteamIdentityService`
- `ProjectJFusionBootstrap`
- `ProjectJDay82SceneFlowCoordinator`

Steam 상태에 따라 초기화, 인증 대기, 로그인 필요, 인증 실패 등의 상태를 표시하고 Fusion 상태와 Scene Flow 상태를 Detail 영역에서 함께 확인할 수 있다.

---

### 5. 로딩 표시

`LoadingDots`를 추가해 Bootstrap Scene이 활성화되어 있는 동안 간단한 로딩 애니메이션을 표시하도록 했다.

```text
○  ○  ○
●  ○  ○
●  ●  ○
●  ●  ●
```

---

### 6. 버전 표시

화면 하단에 `Application.version`과 개발 일차를 표시하도록 구성했다.

예:

```text
v0.1 • DAY 85
```

---

### 7. Bootstrap 자동 구성 도구

`ProjectJDay85BootstrapSceneInstaller`를 추가했다.

Unity Editor 메뉴:

```text
Project J
→ Scene
→ 85일차 Bootstrap Scene 구성
```

을 통해 Bootstrap Scene 기본 구조, Canvas, 상태 View, 구형 Day3 Scene Flow 제거 등을 자동 구성할 수 있다.

현재 Scene은 사용자가 직접 배경 위치와 일부 Scene 구성을 수정한 상태이므로 현 상태를 유지하려면 이 자동 구성 메뉴를 다시 실행하지 않는 것이 안전하다.

자동 구성 도구가 검색하는 기본 배경 경로는 `Assets/ProjectJ/Art/UI/Bootstrap/BootstrapBackground.*`이고, 현재 Scene은 `Assets/ProjectJ/Art/Background.png`를 직접 사용하기 때문이다.

---

## 변경 파일

### 추가

- `Assets/ProjectJ/Art/Background.png`
- `Assets/ProjectJ/Art/Background.png.meta`
- `Assets/ProjectJ/Art/UI/Bootstrap.meta`
- `Assets/ProjectJ/Editor/ProjectJDay85BootstrapSceneInstaller.cs`
- `Assets/ProjectJ/Editor/ProjectJDay85BootstrapSceneInstaller.cs.meta`
- `Assets/ProjectJ/Network/Fusion/UI.meta`
- `Assets/ProjectJ/Network/Fusion/UI/ProjectJDay85BootstrapStatusView.cs`
- `Assets/ProjectJ/Network/Fusion/UI/ProjectJDay85BootstrapStatusView.cs.meta`

### 수정

- `Assets/ProjectJ/Runtime/SceneFlow/BootstrapSceneController.cs`
- `Assets/ProjectJ/Scenes/Bootstrap.unity`

### 삭제

- 별도 파일 삭제 없음
- Bootstrap Scene 내부의 기존 `Day3_SceneFlow` 오브젝트 제거

---

## 확인 결과

최신 커밋의 Scene 및 소스 구조를 기준으로 다음을 확인했다.

- `Day3_SceneFlow` 제거
- Bootstrap 즉시 MainMenu 직행 코드 제거
- `Canvas_Bootstrap` 존재
- `=== CAMERA ===` 루트 존재
- `=== ENVIRONMENT ===` 루트 존재
- `=== UI ===` 루트 존재
- `=== SCENE FLOW ===` 루트 존재
- `BootstrapStatusView`가 Scene에 연결됨
- Loading / Status / Detail / Version Text 참조가 직렬화됨
- `Background.png`가 Bootstrap `RawImage`에 연결됨

GitHub에는 별도의 CI 상태 체크가 등록되어 있지 않으므로 Unity 실제 PlayMode, 빌드, Steam Runtime 동작이 자동 검증되었다고 기록하지 않는다.

---

## 85일차 결과

Bootstrap Scene을 실제 게임 초기화 화면 형태로 구성했다.

기존 Day3 Scene Flow와의 중복 전환을 제거하고, 82일차 Scene Flow 구조를 유지한 상태에서 Steam/Fusion 초기화 정보를 화면으로 확인할 수 있게 했다.

또한 사용자가 직접 선택한 배경 이미지를 Bootstrap 화면에 적용해 이후 MainMenu, Lobby, Game Scene을 실제 화면 단위로 연결·검증할 수 있는 PHASE 8의 기반을 만들었다.

---

## 다음 개발 방향

86일차에는 `MainMenu` Scene을 실제 메뉴 화면으로 구성한다.

예정 범위:

- PLAY
- SETTINGS
- QUIT
- MainMenu 기본 레이아웃
- Bootstrap → MainMenu 실제 화면 연결 확인
