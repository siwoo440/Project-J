# Project J 개발 일지

## 47일차 : Runtime·Data 스크립트 기능별 폴더 통합

### 개발 목표

46일차에서 Unity Editor 메뉴를 기능별 구조로 재분류한 데 이어, 47일차에서는 실제 Runtime 및 Data 스크립트의 폴더 구조를 기능 기준으로 정리했습니다.

기존 클래스명, namespace, 메서드 내용과 게임 로직은 변경하지 않고 `.cs`와 `.meta`를 함께 이동하여 기존 Scene·Prefab·ScriptableObject 참조를 유지하는 것을 핵심 목표로 진행했습니다.

---

### 주요 작업 내용

#### 1. Runtime·Data 폴더 구조 재정리

기존에 한 폴더에 집중되어 있던 Data, MapGeneration, Items 스크립트를 역할별 하위 폴더로 분리했습니다.

총 이동 대상은 다음과 같습니다.

```text
Data : 9개
Map  : 21개
Item : 23개

총 53개
```

기존 `ProjectJ.Runtime.asmdef`는 `Assets/_ProjectJ/Scripts/Runtime` 루트에 유지하여 하위 폴더 이동 후에도 모든 Runtime 스크립트가 기존 `ProjectJ.Runtime` 어셈블리에 포함되도록 했습니다.

---

#### 2. Data 스크립트 기능별 통합

Data 관련 스크립트 9개를 다음 구조로 정리했습니다.

```text
Runtime/Data
├─ Catalog
├─ Definitions
│  ├─ Audio
│  ├─ Common
│  ├─ Cosmetic
│  ├─ Item
│  ├─ Map
│  ├─ Obstacle
│  └─ Player
├─ Identity
├─ Player
└─ Validation
```

주요 이동 예시는 다음과 같습니다.

```text
Data/DataValidationService.cs
→ Data/Validation/DataValidationService.cs
```

```text
Data/Definitions/ProjectDataCatalog.cs
→ Data/Catalog/ProjectDataCatalog.cs
```

```text
Data/Definitions/ItemDataDefinition.cs
→ Data/Definitions/Item/ItemDataDefinition.cs
```

```text
Data/Definitions/ProjectDataAsset.cs
→ Data/Definitions/Common/ProjectDataAsset.cs
```

데이터 기반 클래스, 각 데이터 종류, 검증 기능, 런타임 카탈로그를 역할별로 구분하여 이후 데이터가 늘어나더라도 파일 위치를 쉽게 파악할 수 있도록 정리했습니다.

---

#### 3. MapGeneration 스크립트 기능별 통합

기존 `Runtime/MapGeneration`에 모여 있던 21개 스크립트를 새로운 `Runtime/Map` 구조로 이동했습니다.

최종 구조는 다음과 같습니다.

```text
Runtime/Map
├─ Debug
├─ Generation
├─ Modules
├─ Obstacles
├─ Traversal
└─ Validation
```

각 폴더의 역할은 다음과 같습니다.

- `Generation` : 절차적 생성, 분기 생성, 수직 생성과 생성 설정
- `Modules` : 맵 모듈 정의, 연결 지점, 모듈 타입과 수직 모듈 데이터
- `Validation` : 생성 결과와 플레이 가능 경로 검증
- `Obstacles` : 맵 장애물 배치 계획과 생성 지점 관리
- `Debug` : 맵 및 장애물 생성 디버그 시각화
- `Traversal` : 플레이어 이동 가능 기준 프로필

주요 이동 예시는 다음과 같습니다.

```text
MapGeneration/ProceduralMapGenerator.cs
→ Map/Generation/ProceduralMapGenerator.cs
```

```text
MapGeneration/MapModuleDefinition.cs
→ Map/Modules/MapModuleDefinition.cs
```

```text
MapGeneration/MapPlayableRouteValidation.cs
→ Map/Validation/MapPlayableRouteValidation.cs
```

```text
MapGeneration/MapObstacleSpawnPoint.cs
→ Map/Obstacles/MapObstacleSpawnPoint.cs
```

기존 `MapGeneration` 폴더를 기능별 구조로 분해하면서도 기존 클래스 및 namespace는 그대로 유지했습니다.

---

#### 4. Items 스크립트 기능별 통합

기존 `Runtime/Items` 루트에 집중되어 있던 23개 스크립트도 역할별로 분리했습니다.

최종 구조는 다음과 같습니다.

```text
Runtime/Items
├─ Chests
├─ Effects
│  ├─ Cart
│  ├─ Common
│  ├─ Player
│  └─ Rewind
├─ Inventory
├─ Placement
├─ Rules
└─ Use
```

각 폴더의 역할은 다음과 같습니다.

- `Inventory` : 2슬롯 아이템 인벤토리와 수량 관리
- `Chests` : 아이템 상자 획득, 생성 지점, 생성 규칙과 생성기
- `Placement` : 지뢰·트램폴린 등 설치형 아이템 위치 검증
- `Rules` : 아이템 선택 및 P1·P2 공통 규칙
- `Use` : 플레이어 아이템 사용 요청 처리
- `Effects/Common` : 발사체·투척·설치·유도·연막 공통 효과
- `Effects/Player` : 플레이어에게 직접 적용되는 아이템 효과
- `Effects/Rewind` : 되감기 전용 이동 기록
- `Effects/Cart` : 카트 전용 이동 경로

주요 이동 예시는 다음과 같습니다.

```text
Items/PlayerItemInventory.cs
→ Items/Inventory/PlayerItemInventory.cs
```

```text
Items/ItemChestPickup.cs
→ Items/Chests/ItemChestPickup.cs
```

```text
Items/ItemPlacementValidator.cs
→ Items/Placement/ItemPlacementValidator.cs
```

```text
Items/PlayerP2ItemEffectController.cs
→ Items/Effects/Player/PlayerP2ItemEffectController.cs
```

```text
Items/PlayerRewindRecorder.cs
→ Items/Effects/Rewind/PlayerRewindRecorder.cs
```

---

### 폴더 이동 도구 구현

다음 Editor 도구를 추가했습니다.

```text
Assets/_ProjectJ/Scripts/Editor/Day47RuntimeDataFolderStructureTool.cs
```

해당 도구에서는 Runtime 스크립트를 Windows 탐색기에서 직접 이동하지 않고 Unity의 `AssetDatabase.MoveAsset()`을 사용하도록 구성했습니다.

주요 기능은 다음과 같습니다.

- Data 9개 이동
- Map 21개 이동
- Item 23개 이동
- 기존 위치와 새 위치 사전 검증
- 이미 이동된 파일 자동 인식
- `ValidateMoveAsset`을 이용한 이동 가능 여부 사전 확인
- `.meta` GUID 보존 확인
- 이동 후 전체 구조 검증
- 기존 Runtime asmdef 위치 검증
- 오류 발생 시 현재 실행에서 이동한 파일 롤백 지원

---

### 폴더 이동 도구 오류 수정

초기 47일차 적용 과정에서 Data 단계가 일부 파일만 이동된 뒤 정상적으로 완료되지 않는 문제가 발생했습니다.

원인은 `AssetDatabase.StartAssetEditing()`으로 자동 Import가 중지된 상태에서 `MoveAsset()` 직후 새 경로의 GUID를 다시 조회했던 구조였습니다.

초기 흐름은 다음과 같았습니다.

```text
StartAssetEditing
→ MoveAsset
→ 새 경로 GUID 즉시 조회
→ GUID 비교
```

이 과정에서 AssetDatabase가 아직 새 경로 상태를 완전히 반영하기 전에 GUID 검증이 실행될 수 있었습니다.

이를 다음 순서로 수정했습니다.

```text
기존/대상 경로 검사
→ ValidateMoveAsset 사전 검사
→ StartAssetEditing
→ MoveAsset 일괄 실행
→ StopAssetEditing
→ ForceSynchronousImport
→ 이동 후 GUID 검사
→ 최종 경로 검사
```

수정 후 기존 부분 이동 상태를 인식하여 이미 완료된 파일은 유지하고 남은 파일만 이어서 이동할 수 있도록 했습니다.

---

### 47일차 구조 회귀 테스트 추가

다음 EditMode 테스트를 추가했습니다.

```text
Assets/_ProjectJ/Tests/EditMode/RuntimeDataFolderStructureTests.cs
```

주요 검증 항목은 다음과 같습니다.

#### AllMovedScriptsExistOnlyAtFunctionalDestinations

총 53개의 기존 스크립트가 이전 경로에 남아 있지 않고 새 기능별 경로에 존재하는지 검사합니다.

#### RuntimeAssemblyDefinitionRemainsSingleAtRuntimeRoot

다음 asmdef가 Runtime 루트에 그대로 존재하는지 확인합니다.

```text
Assets/_ProjectJ/Scripts/Runtime/ProjectJ.Runtime.asmdef
```

또한 Runtime 내부에 예상하지 않은 추가 asmdef가 생성되지 않았는지도 검사합니다.

---

### Runtime 폴더 구조 문서 작성

다음 문서를 추가했습니다.

```text
Assets/_ProjectJ/Documentation/ProjectJ_RuntimeData_폴더구조.md
```

문서에는 다음 내용을 기록했습니다.

- 최종 Runtime 폴더 구조
- Data·Map·Item 이동 수
- 각 기능별 하위 폴더 구조
- `.meta` GUID 유지 규칙
- Runtime asmdef 유지 규칙
- 47일차 검증 기준

---

### 비의도 Prefab 변경 복구

폴더 이동 과정에서 47일차 작업 범위와 관계없는 다음 맵 모듈 Prefab 변경이 발견됐습니다.

```text
MAP-001_FixedStraight.prefab
MAP-002_LowPassage.prefab
MAP-003_JumpGap.prefab
```

해당 변경은 Runtime 폴더 통합과 관계없는 내용이었기 때문에 46일차 상태로 복구했습니다.

최종 47일차 커밋에서는 위 Prefab 변경이 제외되어 Runtime·Data 구조 정리에 필요한 변경만 유지했습니다.

---

### 최종 Runtime 구조

47일차 완료 후 Runtime 스크립트의 주요 구조는 다음과 같습니다.

```text
Assets/_ProjectJ/Scripts/Runtime
├─ Audio
├─ Common
│  ├─ Build
│  ├─ Diagnostics
│  └─ Testing
├─ Core
│  ├─ Physics
│  ├─ SceneFlow
│  └─ Services
├─ Data
│  ├─ Catalog
│  ├─ Definitions
│  │  ├─ Audio
│  │  ├─ Common
│  │  ├─ Cosmetic
│  │  ├─ Item
│  │  ├─ Map
│  │  ├─ Obstacle
│  │  └─ Player
│  ├─ Identity
│  ├─ Player
│  └─ Validation
├─ Gameplay
│  └─ Match
├─ Input
├─ Items
│  ├─ Chests
│  ├─ Effects
│  │  ├─ Cart
│  │  ├─ Common
│  │  ├─ Player
│  │  └─ Rewind
│  ├─ Inventory
│  ├─ Placement
│  ├─ Rules
│  └─ Use
├─ Map
│  ├─ Debug
│  ├─ Generation
│  ├─ Modules
│  ├─ Obstacles
│  ├─ Traversal
│  └─ Validation
├─ Player
│  ├─ Camera
│  ├─ Forces
│  ├─ Input
│  ├─ Interaction
│  ├─ Movement
│  ├─ Progression
│  ├─ Respawn
│  └─ State
├─ UI
│  ├─ HUD
│  ├─ Menu
│  └─ System
└─ ProjectJ.Runtime.asmdef
```

---

### 기존 기능 보존

47일차에서는 다음 항목을 변경하지 않았습니다.

- 기존 Runtime 클래스 이름
- 기존 namespace 구조
- 기존 Runtime 메서드 동작
- 플레이어 이동 및 상태 로직
- 맵 생성 알고리즘
- 아이템 28종 효과 로직
- Scene 구성
- Prefab 기능
- `ProjectJ.Runtime.asmdef` 내용과 위치
- 기존 `.meta` GUID

즉 이번 일차는 게임 기능 변경이 아니라 프로젝트 구조와 유지보수성 개선에 집중한 작업입니다.

---

### 테스트 및 검증

47일차 적용 후 다음 항목을 기준으로 최종 검증을 진행했습니다.

- Data 9개 새 기능별 경로 확인
- Map 21개 새 기능별 경로 확인
- Item 23개 새 기능별 경로 확인
- 총 53개 기존 경로 제거 확인
- `.meta` GUID 유지 확인
- `ProjectJ.Runtime.asmdef` Runtime 루트 유지 확인
- Runtime 내부 asmdef 단일 구조 확인
- Game Scene 주요 스크립트 참조 확인
- 주요 Prefab Missing Script 확인
- EditMode 전체 테스트 확인
- PlayMode 전체 테스트 확인
- Unity Console 오류 확인

---

### 47일차 결과

Runtime과 Data 구조가 기존 개발 일차 중심의 누적 구조에서 실제 기능 중심 구조로 정리되었습니다.

특히 규모가 커진 맵 생성과 아이템 시스템을 세부 역할별 폴더로 분리하여, 이후 시스템 확장 시 필요한 코드를 빠르게 찾고 관리할 수 있는 기반을 마련했습니다.

또한 `.meta` GUID 보존과 폴더 구조 회귀 테스트를 추가하여 앞으로의 리팩터링 과정에서 Scene·Prefab 참조 손상이나 잘못된 파일 이동을 자동으로 발견할 수 있도록 했습니다.

---

### 다음 개발 방향

48일차에서는 Runtime에 이어 `Editor·Tests` 스크립트를 실제 기능 기준 폴더로 통합합니다.

46일차에서 정리한 Unity Editor 메뉴 분류와 47일차 Runtime 구조를 기준으로 Editor 도구와 EditMode·PlayMode 테스트 파일도 기능별로 재배치합니다.

파일 이동 시 기존 `.meta` GUID를 유지하고, asmdef 경계와 테스트 참조가 바뀌지 않는지 단계별로 검증합니다.

---

## 47일차 커밋

```text
47일차 : Runtime·Data 스크립트 기능별 폴더 통합
```

최종 확인 커밋:

```text
29800b73029a5a29b3411344fa99065f9f06825d
```
