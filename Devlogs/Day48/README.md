# Project J 개발 일지

## 48일차 : Editor·Tests 스크립트 기능별 폴더 통합

### 개발 목표

47일차에서 Runtime·Data 스크립트를 기능별 폴더로 정리한 데 이어, 48일차에서는 Unity Editor 도구와 EditMode·PlayMode 테스트 스크립트도 기능 중심의 폴더 구조로 통합했습니다.

기존 클래스명과 namespace, 테스트 로직을 최대한 유지하면서 `.cs`와 `.meta`를 함께 이동하여 기존 GUID와 어셈블리 참조를 보존하는 것을 핵심 목표로 진행했습니다.

또한 46일차에서 구성한 기능별 Unity Editor 메뉴 구조와 실제 Editor 스크립트 위치를 최대한 같은 기준으로 맞춰 이후 유지보수성을 높였습니다.

---

### 주요 작업 내용

#### 1. Editor 스크립트 기능별 폴더 통합

기존 `Assets/_ProjectJ/Scripts/Editor` 루트와 일부 하위 폴더에 분산되어 있던 Editor 스크립트를 역할별로 재배치했습니다.

주요 구조는 다음과 같습니다.

```text
Assets/_ProjectJ/Scripts/Editor
├─ ProjectManagement
│  ├─ Menu
│  └─ Structure
├─ ProjectSettings
│  ├─ Scenes
│  ├─ Services
│  └─ Physics
├─ Player
├─ Data
│  ├─ Setup
│  ├─ CSV
│  └─ Catalog
├─ Testing
├─ Build
├─ Map
│  ├─ Modules
│  ├─ Generation
│  ├─ Validation
│  └─ Obstacles
├─ Items
│  ├─ Inventory
│  ├─ Chests
│  ├─ Effects
│  └─ Validation
├─ UI
├─ Common
└─ ProjectJ.Editor.asmdef
```

46일차에서 작성한 `ProjectJEditorMenuPaths.cs`는 프로젝트 관리 메뉴 정의 파일로 분류하여 다음 위치로 이동했습니다.

```text
Assets/_ProjectJ/Scripts/Editor/
ProjectManagement/Menu/
ProjectJEditorMenuPaths.cs
```

47일차와 48일차의 구조 관리 도구는 다음 위치에 통합했습니다.

```text
Assets/_ProjectJ/Scripts/Editor/
ProjectManagement/Structure/
```

---

#### 2. 기존 Editor 메뉴 구조 유지

파일 위치는 변경했지만 46일차에서 정리한 Unity Editor 상단 메뉴 구조는 그대로 유지했습니다.

```text
Project J
├─ 01. 프로젝트 설정
├─ 02. 플레이어와 입력
├─ 03. 데이터
├─ 04. 테스트
├─ 05. 빌드
├─ 06. 맵
├─ 07. 장애물
├─ 08. 아이템
└─ 09. UI
```

즉 48일차는 메뉴 기능을 다시 만드는 작업이 아니라 실제 Editor `.cs` 위치를 메뉴의 기능 분류와 맞추는 구조 정리 작업으로 진행했습니다.

---

#### 3. Data Editor 도구 세부 분류

Data 관련 Editor 도구를 다음 세 역할로 구분했습니다.

```text
Editor/Data
├─ Setup
├─ CSV
└─ Catalog
```

대표적인 이동은 다음과 같습니다.

```text
ProjectDataAssetDatabase.cs
→ Data/Setup/

ProjectDataAssetPostprocessor.cs
→ Data/Setup/

ProjectDataBuildValidator.cs
→ Data/Setup/

ProjectDataCsvImporter.cs
→ Data/CSV/

ProjectDataCatalogBuilder.cs
→ Data/Catalog/
```

데이터 생성·검증, CSV 가져오기, 런타임 카탈로그 생성을 서로 분리하여 목적에 맞는 Editor 도구를 쉽게 찾을 수 있도록 했습니다.

---

#### 4. Map Editor 도구 세부 분류

맵 관련 Editor 도구도 기능별로 분리했습니다.

```text
Editor/Map
├─ Modules
├─ Generation
├─ Validation
└─ Obstacles
```

대표적인 이동은 다음과 같습니다.

```text
Day30MapModuleSetupTool.cs
→ Map/Modules/

Day31ProceduralMapSetupTool.cs
→ Map/Generation/

Day37MapPlayabilitySetupTool.cs
→ Map/Validation/

Day38BranchObstacleSetupTool.cs
→ Map/Obstacles/
```

맵 모듈 제작, 절차적 생성, 플레이 가능성 검증, 장애물 구성을 각각 독립적으로 관리할 수 있도록 정리했습니다.

---

#### 5. Item Editor 도구 세부 분류

아이템 Editor 도구는 다음 기준으로 분리했습니다.

```text
Editor/Items
├─ Inventory
├─ Chests
├─ Effects
└─ Validation
```

주요 이동은 다음과 같습니다.

```text
Day39ItemInventorySetupTool.cs
→ Items/Inventory/

Day41ItemChestPlacementSetupTool.cs
→ Items/Chests/

Day42ItemSystemSetupTool.cs
Day43P1ItemSetupTool.cs
Day44P2ItemSetupTool.cs
→ Items/Effects/

Day45ItemIntegrationValidationTool.cs
→ Items/Validation/
```

28종 아이템의 개발·검증 도구를 실제 기능 단위로 구분하여 이후 아이템 시스템 유지보수 시 필요한 Editor 도구를 쉽게 찾을 수 있게 했습니다.

---

### Tests 폴더 기능별 통합

EditMode와 PlayMode 테스트도 기존의 루트 집중 구조에서 기능별 하위 폴더 구조로 정리했습니다.

EditMode 테스트는 다음과 같은 기능 범주를 기준으로 구분했습니다.

```text
Tests/EditMode
├─ Common
├─ Data
├─ Gameplay
├─ Items
├─ Map
├─ Player
├─ ProjectSettings
├─ Structure
├─ UI
└─ ProjectJ.Tests.EditMode.asmdef
```

PlayMode 테스트 역시 기능에 따라 하위 폴더로 이동했습니다.

```text
Tests/PlayMode
├─ Items
├─ Testing
└─ ProjectJ.Tests.PlayMode.asmdef
```

기존 asmdef는 각 테스트 루트에 그대로 유지하여 테스트 파일 이동으로 어셈블리 경계가 변경되지 않도록 했습니다.

---

### asmdef 구조 유지

48일차에서는 다음 Assembly Definition 파일을 이동하지 않았습니다.

```text
Assets/_ProjectJ/Scripts/Editor/
ProjectJ.Editor.asmdef

Assets/_ProjectJ/Tests/EditMode/
ProjectJ.Tests.EditMode.asmdef

Assets/_ProjectJ/Tests/PlayMode/
ProjectJ.Tests.PlayMode.asmdef
```

하위 기능 폴더에는 별도의 asmdef를 추가하지 않았습니다.

따라서 Editor·EditMode·PlayMode의 기존 Assembly 구성을 유지하면서 물리적인 파일 위치만 기능별로 재배치했습니다.

---

### Day48EditorTestsFolderStructureTool 구현

48일차 파일 이동을 안전하게 처리하기 위해 다음 Editor 도구를 추가했습니다.

```text
Assets/_ProjectJ/Scripts/Editor/
ProjectManagement/Structure/
Day48EditorTestsFolderStructureTool.cs
```

주요 기능은 다음과 같습니다.

- Editor 스크립트 자동 검색
- EditMode 테스트 자동 검색
- PlayMode 테스트 자동 검색
- 파일명과 메뉴 경로를 이용한 기능 분류
- 이동 계획 미리보기
- 대상 경로 충돌 검사
- Unity `AssetDatabase.MoveAsset()` 기반 이동
- 기존 `.meta` GUID 보존 검사
- 실패 시 현재 실행 이동 파일 롤백
- Editor·Tests 루트 구조 최종 검증
- asmdef 위치와 개수 검증

---

### 이동 도구 오류 수정

초기 적용 과정에서 `01. Editor 스크립트 기능별 폴더 통합`을 실행했을 때 다수의 `MoveAsset 사전 검사 실패` 오류가 발생했습니다.

대표적인 오류는 다음 형태였습니다.

```text
ProjectDataAssetDatabase.cs
→ Data/Setup/ProjectDataAssetDatabase.cs

Trying to move asset as a sub directory of a directory that does not exist
```

원인은 실제 파일 이동 문제가 아니라 이동 대상 폴더가 아직 생성되지 않은 상태에서 `AssetDatabase.ValidateMoveAsset()`을 먼저 실행했던 순서 문제였습니다.

기존 흐름은 다음과 같았습니다.

```text
이동 계획 수집
→ ValidateMoveAsset
→ 대상 폴더 생성
→ MoveAsset
```

이를 다음 순서로 수정했습니다.

```text
이동 계획 수집
→ 대상 기능별 폴더 생성
→ AssetDatabase Refresh
→ ValidateMoveAsset
→ MoveAsset
→ ForceSynchronousImport
→ GUID 및 최종 경로 검증
```

수정 이후 대상 폴더가 정상적으로 생성된 뒤 사전 검사가 수행되면서 Editor 스크립트 기능별 이동을 오류 없이 진행할 수 있게 됐습니다.

---

### 46일차 메뉴 회귀 테스트 수정

48일차에서 `ProjectJEditorMenuPaths.cs`의 위치가 변경되었기 때문에 기존 46일차 테스트의 고정 경로도 함께 수정했습니다.

대상 테스트:

```text
EditorMenuClassificationTests.cs
```

기존 검사 경로:

```text
Assets/_ProjectJ/Scripts/Editor/
ProjectJEditorMenuPaths.cs
```

변경 검사 경로:

```text
Assets/_ProjectJ/Scripts/Editor/
ProjectManagement/Menu/
ProjectJEditorMenuPaths.cs
```

이를 통해 Editor 파일 구조를 변경한 뒤에도 46일차에서 확정한 9개 상단 메뉴 구조에 대한 회귀 테스트를 계속 사용할 수 있도록 했습니다.

---

### 48일차 구조 회귀 테스트 추가

다음 테스트를 새로 추가했습니다.

```text
Assets/_ProjectJ/Tests/EditMode/Structure/
EditorTestsFolderStructureTests.cs
```

주요 테스트는 다음과 같습니다.

#### RootFoldersDoNotContainLooseCSharpScripts

다음 루트 바로 아래에 기능별로 분류되지 않은 `.cs`가 남아 있는지 검사합니다.

```text
Scripts/Editor
Tests/EditMode
Tests/PlayMode
```

#### AssemblyDefinitionsRemainSingleAtTheirRoots

Editor, EditMode, PlayMode의 asmdef가 기존 루트에 유지되어 있는지 확인하고 예상하지 않은 중첩 asmdef가 생성되지 않았는지 검사합니다.

#### CommonEditorMenuPathFileExistsAtFunctionalLocation

`ProjectJEditorMenuPaths.cs`가 48일차에서 정한 프로젝트 관리 메뉴 위치에 정상적으로 존재하는지 검사합니다.

---

### 구조 문서 작성

Editor·Tests의 새로운 폴더 구조와 향후 유지 규칙을 기록하기 위해 다음 문서를 추가했습니다.

```text
Assets/_ProjectJ/Documentation/
ProjectJ_EditorTests_폴더구조.md
```

문서에는 다음 내용을 정리했습니다.

- Editor 최종 기능별 구조
- EditMode·PlayMode 테스트 분류 기준
- `ProjectJEditorMenuPaths.cs` 최종 위치
- asmdef 유지 규칙
- `.meta` GUID 유지 규칙
- 48일차 최종 검증 기준

---

### 기존 기능 보존 원칙

48일차에서는 프로젝트 구조 정리를 중심으로 다음 요소를 유지했습니다.

- Runtime 게임 로직
- 기존 Editor 클래스 이름
- 기존 namespace
- 기존 Editor 메뉴 기능
- 기존 테스트 클래스와 테스트 로직
- Editor asmdef
- EditMode asmdef
- PlayMode asmdef
- 기존 `.meta` GUID

Editor·Tests 파일의 위치를 변경하면서 실제 게임 Runtime 기능에는 영향을 주지 않는 방향으로 작업했습니다.

---

### 테스트 및 검증

48일차 적용 과정에서는 다음 순서로 검증했습니다.

```text
Editor 스크립트 폴더 통합
→ Unity 컴파일 확인

EditMode 테스트 폴더 통합
→ Unity 컴파일 확인

PlayMode 테스트 폴더 통합
→ Unity 컴파일 확인

48일차 전체 구조 검증
→ 기존 테스트 프레임워크 검증

EditMode Run All
→ PlayMode Run All

Game Scene 기본 실행
→ Console 및 Missing Script 확인
```

주요 완료 기준은 다음과 같습니다.

- Unity Console 컴파일 Error 없음
- Editor 기능별 폴더 이동 완료
- EditMode 기능별 폴더 이동 완료
- PlayMode 기능별 폴더 이동 완료
- 기존 Editor 9개 대분류 메뉴 유지
- `ProjectJEditorMenuPaths.cs` 새 위치 확인
- asmdef 루트 위치 유지
- 새 중첩 asmdef 없음
- 기존 `.meta` GUID 유지
- 구조 회귀 테스트 추가
- 기존 테스트 프레임워크 유지
- Runtime 기능 변경 없음

---

### 48일차 결과

46일차에서 Unity Editor 메뉴 구조를 기능 중심으로 정리하고 47일차에서 Runtime·Data 스크립트를 기능별 폴더로 통합한 데 이어, 48일차에서는 Editor와 Tests 영역까지 같은 기준으로 정리했습니다.

이를 통해 프로젝트의 주요 코드 영역이 다음과 같이 역할 중심 구조를 갖추게 됐습니다.

```text
Runtime
→ 실제 게임 기능

Editor
→ 제작·설정·검증 도구

Tests/EditMode
→ 로직·구조 단위 테스트

Tests/PlayMode
→ 실제 실행 환경 테스트
```

또한 폴더 이동 자체를 검증하는 회귀 테스트와 안전 이동 도구를 추가하여 이후 프로젝트 규모가 커져도 구조가 다시 무질서해지는 문제를 감지할 수 있는 기반을 마련했습니다.

---

### 다음 개발 방향

49일차에서는 새로운 시스템을 추가하지 않고 46~48일차 구조 정리 이후 프로젝트 전체가 정상적으로 유지되는지 최종 검증합니다.

주요 확인 대상은 다음과 같습니다.

```text
컴파일
스크립트 참조
Unity Editor 메뉴
asmdef
EditMode 테스트
PlayMode 테스트
Scene·Prefab Missing Script
개발 빌드
실제 실행
```

49일차 검증까지 완료하면 프로젝트 구조 정리 작업을 종료하고 이후 설정 시스템 구현 단계로 넘어갈 수 있는 기준점을 확보하게 됩니다.

---

## 48일차 커밋

```text
48일차 : Editor·Tests 스크립트 기능별 폴더 통합
```

확인 커밋:

```text
4b4871e70b053b23fa9bc906c7cbb06794a46cff
```
