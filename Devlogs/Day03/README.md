# Project J

3D 3인칭 온라인 수직 점프 경쟁 파티 게임 **Project J**의 개발 저장소입니다.

---

# 개발 환경

| 항목 | 내용 |
|---|---|
| 게임 엔진 | Unity 6 |
| Unity 버전 | 6000.3.21f1 |
| 프로젝트 템플릿 | Universal 3D |
| 렌더 파이프라인 | URP |
| 대상 플랫폼 | Steam PC |
| 개발 인원 | 1인 개발 |
| 저장소 | siwoo440/Project-J |

---

# 3일차 : 씬 흐름 뼈대 구성

## 개발 목표

게임 실행의 시작점이 되는 `Bootstrap` 씬과 주요 게임 씬을 생성하고, 모든 씬 전환을 하나의 관리자를 통해 처리할 수 있는 기본 흐름을 구성했습니다.

게임 기능을 본격적으로 구현하기 전에 다음 기반을 확보하는 것을 목표로 진행했습니다.

- 게임 시작 씬 통일
- 프로젝트 전체 씬 이름과 경로 중앙 관리
- 비동기 씬 전환
- 중복 씬 전환 방지
- 잘못된 씬 요청 방어
- 개발용 씬 이동 기능
- 씬 목록 자동 생성 및 Build Settings 등록
- 씬 구성 자동 테스트

---

## 최신 커밋

| 항목 | 내용 |
|---|---|
| 커밋 제목 | `3일차 : 씬 흐름 뼈대 구성` |
| 커밋 SHA | `031626c7f15bb138aa07ad74e23e756d3eed1638` |
| 브랜치 | `main` |
| 이전 커밋 | `fd7c216053552af73b5ca5e2242476b00c2f8efa` |
| 커밋 링크 | https://github.com/siwoo440/Project-J/commit/031626c7f15bb138aa07ad74e23e756d3eed1638 |

---

# 최신 커밋 검토 결과

최신 커밋을 기준으로 다음 항목을 확인했습니다.

- `Bootstrap`, `MainMenu`, `Lobby`, `MatchLoading`, `Game`, `Tests` 씬 생성
- 여섯 개 씬의 Build Settings 등록
- `Bootstrap` 씬이 빌드 순서 0번으로 등록
- `GameSceneId`를 통한 씬 식별자 통일
- `GameSceneCatalog`를 통한 씬 이름과 경로 중앙 관리
- `SceneFlowManager`를 통한 비동기 씬 전환
- 동일 씬 요청과 중복 로드 요청 방어
- Build Settings에 등록되지 않은 씬 요청 방어
- `BootstrapEntryPoint`에서 `MainMenu`로 자동 전환
- 개발용 `SceneFlowDebugPanel` 추가
- 씬 생성과 Build Settings 등록용 에디터 도구 추가
- 씬 순서, 이름 중복과 Bootstrap 경로를 검사하는 EditMode 테스트 추가

저장소에서 확인 가능한 범위에서는 수정이 필요한 치명적인 구조 오류를 발견하지 못했습니다.

GitHub Actions가 아직 구성되지 않았으므로 Unity Console 오류 여부, Play Mode 전환 결과와 Test Runner의 실제 통과 여부는 로컬 Unity 에디터에서 최종 확인해야 합니다.

---

# 구현 내용

## 1. 게임용 기본 씬 생성

다음 여섯 개 씬을 생성했습니다.

```text
Assets/_ProjectJ/Scenes/Game/Bootstrap.unity
Assets/_ProjectJ/Scenes/Game/MainMenu.unity
Assets/_ProjectJ/Scenes/Game/Lobby.unity
Assets/_ProjectJ/Scenes/Game/MatchLoading.unity
Assets/_ProjectJ/Scenes/Game/Game.unity
Assets/_ProjectJ/Scenes/Game/Tests.unity
```

각 씬의 역할은 다음과 같습니다.

| 씬 | 역할 |
|---|---|
| Bootstrap | 게임 실행 시작과 공통 관리자 준비 |
| MainMenu | 메인 메뉴 화면 |
| Lobby | 공개·비공개 경기 준비 |
| MatchLoading | 경기 데이터와 맵 로딩 |
| Game | 실제 경기 진행 |
| Tests | 기능별 런타임 검증 |

---

## 2. Build Settings 씬 순서 구성

Build Settings에 다음 순서로 씬을 등록했습니다.

| Build Index | 씬 |
|---:|---|
| 0 | Bootstrap |
| 1 | MainMenu |
| 2 | Lobby |
| 3 | MatchLoading |
| 4 | Game |
| 5 | Tests |

게임 실행은 항상 `Bootstrap`에서 시작합니다.

```text
Bootstrap
→ MainMenu
→ Lobby
→ MatchLoading
→ Game
```

`Tests` 씬은 개발 중 기능 검증에 사용합니다.

---

## 3. GameSceneId 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/SceneFlow/GameSceneId.cs
```

프로젝트에서 사용하는 씬을 enum으로 관리하도록 구성했습니다.

```text
Bootstrap
MainMenu
Lobby
MatchLoading
Game
Tests
```

씬 이름을 문자열로 여러 코드에 직접 작성하지 않고 다음과 같이 요청할 수 있습니다.

```csharp
GameSceneId.MainMenu
GameSceneId.Lobby
GameSceneId.Game
```

이를 통해 씬 이름 오타와 잘못된 문자열 사용 가능성을 줄였습니다.

---

## 4. GameSceneCatalog 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/SceneFlow/GameSceneCatalog.cs
```

다음 정보를 한 곳에서 관리합니다.

- 씬 빌드 순서
- 씬 이름
- Unity 씬 에셋 경로

게임 씬 공통 폴더:

```text
Assets/_ProjectJ/Scenes/Game
```

주요 기능:

```text
GetBuildOrder()
GetSceneName()
GetScenePath()
```

정의되지 않은 씬 식별자가 전달되면 `ArgumentOutOfRangeException`을 발생시켜 잘못된 요청을 조기에 발견할 수 있도록 구성했습니다.

---

## 5. SceneFlowManager 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/SceneFlow/SceneFlowManager.cs
```

프로젝트 전체 씬 전환을 담당하는 관리자를 구현했습니다.

주요 역할:

- 씬 전환 관리자 단일 인스턴스 유지
- 씬이 바뀌어도 관리자 유지
- 씬 비동기 로드
- 현재 씬 재요청 방지
- 로딩 중 중복 요청 방지
- 빌드 목록 미등록 씬 요청 방지
- 씬 전환 시작·완료 로그 출력

관리자는 `DontDestroyOnLoad`를 통해 씬 전환 후에도 유지됩니다.

### 중복 관리자 방지

이미 인스턴스가 존재하는 상태에서 새로운 관리자가 생성되면 중복 게임 오브젝트를 제거합니다.

### 중복 로드 방지

다른 씬을 불러오는 동안 추가 요청이 들어오면 요청을 거절합니다.

```text
[SceneFlow] 이미 다른 씬을 불러오는 중입니다.
```

### 동일 씬 요청 방지

현재 활성화된 씬을 다시 요청하면 씬을 재로딩하지 않습니다.

```text
[SceneFlow] 이미 MainMenu 씬에 있습니다.
```

### 미등록 씬 방어

Build Settings 또는 Build Profile에 등록되지 않은 씬은 불러오지 않습니다.

```text
[SceneFlow] Game 씬이 Build Settings 또는 Build Profile에 등록되지 않았습니다.
```

---

## 6. BootstrapEntryPoint 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/SceneFlow/BootstrapEntryPoint.cs
```

`Bootstrap` 씬이 실행되면 다음 순서로 동작합니다.

```text
Awake
→ SceneFlowManager 조회 또는 생성

Start
→ MainMenu 씬 전환 요청
```

기본 첫 씬은 다음 값으로 설정했습니다.

```text
MainMenu
```

게임 초기화 시스템이 추가되면 `Bootstrap`에서 공통 초기화를 완료한 뒤 첫 씬으로 이동하도록 확장할 예정입니다.

---

## 7. Bootstrap 씬 구성

`Bootstrap` 씬에 다음 루트 오브젝트를 구성했습니다.

```text
ProjectJ_Bootstrap
```

추가된 컴포넌트:

```text
SceneFlowManager
BootstrapEntryPoint
```

`BootstrapEntryPoint`의 첫 이동 대상은 `MainMenu`로 설정했습니다.

---

## 8. SceneFlowDebugPanel 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Runtime/Core/SceneFlow/SceneFlowDebugPanel.cs
```

개발 중 각 씬을 빠르게 오가며 확인할 수 있는 디버그 패널을 구현했습니다.

버튼 목록:

```text
MainMenu
Lobby
MatchLoading
Game
Tests
```

패널 기능:

- 현재 활성 씬 표시
- 현재 씬 버튼 비활성화
- 씬 로딩 중 모든 버튼 비활성화
- 버튼을 통한 SceneFlowManager 전환 요청
- 패널 위치 드래그

패널은 다음 환경에서만 표시됩니다.

```text
UNITY_EDITOR
DEVELOPMENT_BUILD
```

일반 출시 빌드에서는 표시되지 않습니다.

---

## 9. 일반 씬 구성

다음 씬에 개발용 씬 전환 패널을 추가했습니다.

```text
MainMenu
Lobby
MatchLoading
Game
Tests
```

각 씬에는 다음 오브젝트가 존재합니다.

```text
Main Camera
Directional Light
ProjectJ_SceneFlowDebug
```

`ProjectJ_SceneFlowDebug`에는 `SceneFlowDebugPanel`이 연결되어 있습니다.

---

## 10. Day03SceneSetupTool 생성

파일 위치:

```text
Assets/_ProjectJ/Scripts/Editor/Day03SceneSetupTool.cs
```

Unity 상단 메뉴에 다음 항목을 추가했습니다.

```text
Project J
→ Day 03
→ Create Scene Flow Skeleton
```

에디터 도구의 역할:

- 게임 씬 폴더 확인 및 생성
- 여섯 개 기본 씬 생성
- 기존 씬이 있는 경우 필수 오브젝트 보완
- Bootstrap 필수 컴포넌트 추가
- 일반 씬 디버그 패널 추가
- Build Settings 씬 순서 등록
- Play Mode 시작 씬을 Bootstrap으로 지정
- 작업 완료 후 Bootstrap 씬 열기

씬을 직접 하나씩 만들고 등록할 때 발생할 수 있는 경로 오타와 순서 오류를 줄이기 위해 자동화했습니다.

---

# 자동 테스트

## GameSceneCatalogTests 생성

파일 위치:

```text
Assets/_ProjectJ/Tests/EditMode/GameSceneCatalogTests.cs
```

다음 세 가지 테스트를 추가했습니다.

### BuildOrderContainsExpectedScenes

빌드 순서에 다음 여섯 개 씬이 정확한 순서로 들어 있는지 검사합니다.

```text
Bootstrap
MainMenu
Lobby
MatchLoading
Game
Tests
```

### EverySceneIdHasUniqueSceneName

각 씬 식별자가 서로 중복되지 않는 씬 이름을 반환하는지 검사합니다.

### BootstrapScenePathMatchesExpectedValue

Bootstrap 씬 경로가 다음 값과 일치하는지 검사합니다.

```text
Assets/_ProjectJ/Scenes/Game/Bootstrap.unity
```

기존 2일차 테스트 2개를 포함한 예상 결과:

```text
Passed: 5
Failed: 0
Ignored: 0
```

---

# 생성된 주요 파일

```text
Assets/_ProjectJ/Scenes/Game/Bootstrap.unity
Assets/_ProjectJ/Scenes/Game/MainMenu.unity
Assets/_ProjectJ/Scenes/Game/Lobby.unity
Assets/_ProjectJ/Scenes/Game/MatchLoading.unity
Assets/_ProjectJ/Scenes/Game/Game.unity
Assets/_ProjectJ/Scenes/Game/Tests.unity

Assets/_ProjectJ/Scripts/Runtime/Core/SceneFlow/GameSceneId.cs
Assets/_ProjectJ/Scripts/Runtime/Core/SceneFlow/GameSceneCatalog.cs
Assets/_ProjectJ/Scripts/Runtime/Core/SceneFlow/SceneFlowManager.cs
Assets/_ProjectJ/Scripts/Runtime/Core/SceneFlow/BootstrapEntryPoint.cs
Assets/_ProjectJ/Scripts/Runtime/Core/SceneFlow/SceneFlowDebugPanel.cs

Assets/_ProjectJ/Scripts/Editor/Day03SceneSetupTool.cs
Assets/_ProjectJ/Tests/EditMode/GameSceneCatalogTests.cs
```

수정된 주요 설정 파일:

```text
ProjectSettings/EditorBuildSettings.asset
ProjectSettings/SceneTemplateSettings.json
```

---

# 주요 프로젝트 구조

```text
Assets/_ProjectJ
├─ Scenes
│  └─ Game
│     ├─ Bootstrap.unity
│     ├─ MainMenu.unity
│     ├─ Lobby.unity
│     ├─ MatchLoading.unity
│     ├─ Game.unity
│     └─ Tests.unity
├─ Scripts
│  ├─ Runtime
│  │  └─ Core
│  │     └─ SceneFlow
│  │        ├─ GameSceneId.cs
│  │        ├─ GameSceneCatalog.cs
│  │        ├─ SceneFlowManager.cs
│  │        ├─ BootstrapEntryPoint.cs
│  │        └─ SceneFlowDebugPanel.cs
│  └─ Editor
│     └─ Day03SceneSetupTool.cs
└─ Tests
   └─ EditMode
      └─ GameSceneCatalogTests.cs
```

---

# 검증 결과

| 검증 항목 | 저장소 확인 |
|---|:---:|
| 최신 커밋 제목 정상 | 완료 |
| 게임 씬 6개 생성 | 완료 |
| Bootstrap 빌드 인덱스 0 등록 | 완료 |
| 나머지 씬 빌드 순서 등록 | 완료 |
| 씬 식별자 enum 생성 | 완료 |
| 씬 이름과 경로 중앙 관리 | 완료 |
| 비동기 씬 전환 관리자 생성 | 완료 |
| 동일 씬 요청 방어 | 완료 |
| 중복 로딩 요청 방어 | 완료 |
| 미등록 씬 요청 방어 | 완료 |
| Bootstrap 진입점 생성 | 완료 |
| 개발용 씬 이동 패널 생성 | 완료 |
| 씬 자동 생성 에디터 도구 생성 | 완료 |
| 씬 카탈로그 테스트 3개 작성 | 완료 |
| GitHub Actions 자동 검사 | 미구성 |

로컬 Unity 에디터 최종 확인 항목:

```text
Console Error: 0개
Bootstrap → MainMenu 전환 정상
디버그 패널 씬 이동 정상
EditMode Passed: 5개
EditMode Failed: 0개
```

---

# 다음 개발 방향

## 4일차 : 공통 서비스 초기화

다음 일차에는 Bootstrap에서 프로젝트 공통 서비스를 생성하고 초기화 순서를 관리하는 구조를 구성합니다.

예정 작업:

- 서비스 인터페이스 정의
- 서비스 등록과 조회 구조
- 초기화 상태 관리
- 중복 서비스 등록 방지
- 초기화 실패 처리
- Bootstrap 초기화 순서 연결
- 씬 전환 관리자 서비스 등록
- 공통 서비스 EditMode 테스트

---

# 커밋 정보

```text
3일차 : 씬 흐름 뼈대 구성
```

```text
https://github.com/siwoo440/Project-J/commit/031626c7f15bb138aa07ad74e23e756d3eed1638
```
