# Project J Editor·Tests 폴더 구조

## 기준

- 기준 커밋: `f0e4679c1a7e519d534334be35c5f3b6b962fddb`
- 기준 일차: 47일차 완료
- 48일차 목표: Editor·Tests 스크립트 기능별 폴더 통합
- 기존 클래스명·namespace·메서드 로직 변경 최소화
- 기존 `.meta` GUID 유지
- Editor, EditMode, PlayMode의 기존 asmdef는 각 루트에 유지
- 하위 폴더에 새 asmdef를 만들지 않음

## Editor 최종 분류

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

`ProjectJEditorMenuPaths.cs`는 `ProjectManagement/Menu`으로 이동하고,
47·48일차 구조 정리 도구는 `ProjectManagement/Structure`에 배치합니다.

## Tests 최종 분류

EditMode와 PlayMode는 각각 기존 asmdef를 루트에 유지하고,
C# 테스트 파일만 다음 기능 폴더로 재분류합니다.

```text
ProjectSettings
Player
Data
Testing
Build
Map
Items
UI
Structure
Gameplay
Audio
Common
```

테스트 파일명과 테스트 대상 소스 키워드에 따라 기능 폴더를 자동 결정합니다.

## 46일차 회귀 테스트 수정

기존 `EditorMenuClassificationTests.cs`는
`ProjectJEditorMenuPaths.cs`의 이전 루트 경로를 직접 검사하고 있었으므로
48일차 최종 위치인 다음 경로를 사용하도록 수정합니다.

```text
Assets/_ProjectJ/Scripts/Editor/ProjectManagement/Menu/ProjectJEditorMenuPaths.cs
```

## asmdef 유지 규칙

다음 파일은 이동하지 않습니다.

```text
Assets/_ProjectJ/Scripts/Editor/ProjectJ.Editor.asmdef
Assets/_ProjectJ/Tests/EditMode/ProjectJ.Tests.EditMode.asmdef
Assets/_ProjectJ/Tests/PlayMode/ProjectJ.Tests.PlayMode.asmdef
```

하위 폴더는 기존 어셈블리 범위에 계속 포함됩니다.

## 검증 기준

1. Editor 루트 바로 아래 `.cs` 0개
2. EditMode 루트 바로 아래 `.cs` 0개
3. PlayMode 루트 바로 아래 `.cs` 0개
4. Editor asmdef 1개
5. EditMode asmdef 1개
6. PlayMode asmdef 1개
7. `ProjectJEditorMenuPaths.cs` 새 위치 존재
8. 이동 전후 `.meta` GUID 동일
9. 기존 Unity Editor 메뉴 구조 유지
10. EditMode 전체 Failed 0
11. PlayMode 전체 Failed 0
12. Console Error 0
