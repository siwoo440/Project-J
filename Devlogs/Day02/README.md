# Project J

3D 3인칭 온라인 수직 점프 경쟁 파티 게임 **Project J**의 개발 저장소입니다.

## 개발 환경

| 항목 | 내용 |
|---|---|
| 게임 엔진 | Unity 6 |
| Unity 버전 | 6000.3.21f1 |
| 템플릿 | Universal 3D |
| 렌더 파이프라인 | URP |
| 대상 플랫폼 | Steam PC |
| 개발 인원 | 1인 개발 |
| 저장소 | siwoo440/Project-J |

---

# 2일차 : 폴더·어셈블리·네임스페이스 구성

## 개발 목표

프로젝트 전용 파일을 `Assets/_ProjectJ` 아래에 정리하고 Runtime, Editor, Tests 코드를 서로 다른 Assembly Definition으로 분리했습니다.

기능이 늘어나더라도 코드의 책임과 컴파일 범위가 섞이지 않도록 프로젝트 기반 구조를 확립하는 것이 이번 일차의 목표입니다.

## 최신 커밋

| 항목 | 내용 |
|---|---|
| 커밋 제목 | `2일차 : 폴더·어셈블리·네임스페이스 구성` |
| 커밋 SHA | `bbad8d6d70f949bda6a3a0e3c3ba303236aed9ee` |
| 브랜치 | `main` |
| 커밋 링크 | https://github.com/siwoo440/Project-J/commit/bbad8d6d70f949bda6a3a0e3c3ba303236aed9ee |

## 커밋 검토 결과

최신 커밋을 기준으로 다음 항목을 확인했습니다.

- `Assets/_ProjectJ` 프로젝트 전용 루트 생성
- Art, Audio, Data, Materials, Prefabs, Scenes, Scripts, Settings, Tests 폴더 분리
- `ProjectJ.Runtime`, `ProjectJ.Editor`, `ProjectJ.Tests.EditMode` 어셈블리 생성
- Editor와 Tests에서 Runtime을 참조하는 단방향 구조
- 프로젝트 Root Namespace를 `ProjectJ`로 설정
- SampleScene과 Input System Actions의 GUID를 유지한 상태로 경로 이동
- Build Settings의 SampleScene 경로 갱신
- URP 설정 에셋 이동
- Runtime 어셈블리와 네임스페이스 검증용 테스트 작성

저장소에서 확인 가능한 범위에서는 수정이 필요한 치명적인 구조 오류를 발견하지 못했습니다.

GitHub Actions가 아직 구성되지 않았으므로 Unity Console 오류와 Test Runner 실행 결과는 로컬 Unity 에디터에서 최종 확인해야 합니다.

---

# 구현 내용

## 1. 프로젝트 전용 폴더 생성

다음 루트 폴더를 생성했습니다.

```text
Assets/_ProjectJ
```

앞으로 Project J에서 직접 제작하거나 관리하는 파일은 `_ProjectJ` 아래에 배치합니다.

## 2. 리소스 폴더 구성

```text
Assets/_ProjectJ
├─ Art
│  ├─ Animations
│  ├─ Models
│  ├─ Textures
│  └─ VFX
├─ Audio
│  ├─ BGM
│  └─ SFX
├─ Data
│  ├─ Definitions
│  └─ Generated
├─ Materials
├─ Prefabs
│  ├─ Characters
│  ├─ Gameplay
│  └─ UI
├─ Scenes
│  ├─ Development
│  └─ Game
├─ Scripts
│  ├─ Runtime
│  └─ Editor
├─ Settings
│  ├─ Input
│  └─ Rendering
└─ Tests
   ├─ EditMode
   └─ PlayMode
```

## 3. Runtime 스크립트 폴더 구성

```text
Assets/_ProjectJ/Scripts/Runtime
├─ Audio
├─ Common
├─ Competition
├─ Core
├─ Data
├─ Items
├─ Map
├─ Network
├─ Player
└─ UI
```

| 폴더 | 예정 책임 |
|---|---|
| Audio | 음악, 효과음과 오디오 서비스 |
| Common | 여러 시스템에서 사용하는 공통 코드 |
| Competition | 경기 흐름, 순위와 체크포인트 |
| Core | 초기화, 씬 흐름과 핵심 기반 |
| Data | 데이터 모델과 검증 |
| Items | 아이템 데이터와 효과 |
| Map | 절차 생성, 맵 모듈과 장애물 |
| Network | 서버·클라이언트 통신 |
| Player | 이동, 점프, 앉기와 밀치기 |
| UI | 메뉴, HUD, 결과와 설정 화면 |

---

# Assembly Definition 구성

## ProjectJ.Runtime

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/ProjectJ.Runtime.asmdef
```

설정:

```text
Assembly Name: ProjectJ.Runtime
Root Namespace: ProjectJ
Platform: 전체
```

Runtime 어셈블리는 실제 게임 실행에 필요한 코드를 포함합니다.

## ProjectJ.Editor

파일 위치:

```text
Assets/_ProjectJ/Scripts/Editor/ProjectJ.Editor.asmdef
```

설정:

```text
Assembly Name: ProjectJ.Editor
Root Namespace: ProjectJ.Editor
Reference: ProjectJ.Runtime
Platform: Editor
```

Editor 어셈블리는 Unity 에디터에서만 컴파일되고 실제 게임 빌드에는 포함되지 않습니다.

## ProjectJ.Tests.EditMode

파일 위치:

```text
Assets/_ProjectJ/Tests/EditMode/ProjectJ.Tests.EditMode.asmdef
```

설정:

```text
Assembly Name: ProjectJ.Tests.EditMode
Root Namespace: ProjectJ.Tests.EditMode
Reference: ProjectJ.Runtime
Platform: Editor
Optional Unity Reference: TestAssemblies
Auto Referenced: false
```

## 최종 참조 방향

```text
ProjectJ.Editor ───────────┐
                          ├─→ ProjectJ.Runtime
ProjectJ.Tests.EditMode ──┘
```

Runtime 어셈블리에서는 Editor 또는 Tests 어셈블리를 참조하지 않습니다.

---

# 생성된 스크립트

## RuntimeAssemblyMarker.cs

```text
Assets/_ProjectJ/Scripts/Runtime/Core/RuntimeAssemblyMarker.cs
```

역할:

- Runtime 어셈블리 컴파일 확인
- `ProjectJ.Core` 네임스페이스 확인
- Editor와 Tests에서 Runtime 참조 확인

## EditorAssemblyMarker.cs

```text
Assets/_ProjectJ/Scripts/Editor/EditorAssemblyMarker.cs
```

역할:

- Editor 어셈블리에서 Runtime 어셈블리 참조 확인
- 에디터 전용 코드가 별도 어셈블리에 포함되는지 확인

## ProjectStructureTests.cs

```text
Assets/_ProjectJ/Tests/EditMode/ProjectStructureTests.cs
```

작성된 테스트:

1. Runtime 어셈블리 이름이 `ProjectJ.Runtime`인지 검사
2. 최상위 네임스페이스가 `ProjectJ`인지 검사

예상 결과:

```text
Passed: 2
Failed: 0
Ignored: 0
```

---

# 프로젝트 설정 변경

## Root Namespace

다음 값으로 설정했습니다.

```text
ProjectJ
```

반영 파일:

```text
ProjectSettings/EditorSettings.asset
```

## SampleScene 이동

```text
Assets/Scenes/SampleScene.unity
→ Assets/_ProjectJ/Scenes/Development/SampleScene.unity
```

씬 파일과 `.meta` 파일은 내용 변경 없이 이동됐으며 Build Settings의 경로도 새 위치로 갱신됐습니다.

## Input System Actions 이동

```text
Assets/InputSystem_Actions.inputactions
→ Assets/_ProjectJ/Settings/Input/InputSystem_Actions.inputactions
```

Input Actions 파일과 `.meta` 파일은 내용 변경 없이 이동되어 기존 GUID를 유지합니다.

## URP 설정 파일 이동

다음 파일을 `Assets/_ProjectJ/Settings/Rendering`으로 이동했습니다.

```text
DefaultVolumeProfile.asset
Mobile_RPAsset.asset
Mobile_Renderer.asset
PC_RPAsset.asset
PC_Renderer.asset
SampleSceneProfile.asset
UniversalRenderPipelineGlobalSettings.asset
```

일부 파일에는 Unity의 자동 재직렬화 결과가 함께 반영됐습니다.

---

# 검증 결과

| 검증 항목 | 결과 |
|---|:---:|
| `_ProjectJ` 루트 폴더 생성 | 완료 |
| 리소스 종류별 폴더 구성 | 완료 |
| Runtime 어셈블리 생성 | 완료 |
| Editor 어셈블리 생성 | 완료 |
| EditMode Tests 어셈블리 생성 | 완료 |
| Editor → Runtime 참조 | 완료 |
| Tests → Runtime 참조 | 완료 |
| Root Namespace `ProjectJ` 설정 | 완료 |
| SampleScene 이동 | 완료 |
| Build Settings 경로 갱신 | 완료 |
| Input Actions GUID 유지 이동 | 완료 |
| URP 설정 에셋 이동 | 완료 |
| 구조 검증 테스트 작성 | 완료 |
| Git 커밋 반영 | 완료 |
| GitHub Actions 자동 검사 | 미구성 |

로컬 Unity 에디터 최종 확인 항목:

```text
Console Error: 0개
EditMode Passed: 2개
EditMode Failed: 0개
SampleScene Play Mode 실행 정상
```

---

# 다음 개발 방향

## 3일차 : 씬 흐름 뼈대 구성

다음 일차에는 실제 게임 흐름에서 사용할 기본 씬과 씬 전환 구조를 구성합니다.

예정 씬:

```text
Bootstrap
MainMenu
Lobby
MatchLoading
Game
Tests
```

예정 작업:

- 게임용 씬 생성
- Build Settings 또는 Build Profile에 씬 등록
- Bootstrap 시작 씬 구성
- 씬 이름과 경로 상수 정의
- 기본 SceneFlow 구조 작성
- Bootstrap에서 MainMenu로 전환
- 잘못된 씬 요청 방어
- 씬 전환 테스트 작성

---

# 커밋 정보

```text
2일차 : 폴더·어셈블리·네임스페이스 구성
```

```text
https://github.com/siwoo440/Project-J/commit/bbad8d6d70f949bda6a3a0e3c3ba303236aed9ee
```
