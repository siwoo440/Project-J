# Project J Runtime·Data 폴더 구조

## 기준

- 기준 커밋: `00b554af0a5a2c8fc0fec349f611a00a90102d0d`
- 기준 일차: 46일차 완료
- 47일차 목표: Runtime·Data 스크립트 기능별 폴더 통합
- 기존 클래스명·namespace·메서드 내용 변경 없음
- `ProjectJ.Runtime.asmdef`는 Runtime 루트에 유지
- 모든 기존 스크립트 이동은 `AssetDatabase.MoveAsset` 사용
- 기존 `.meta` GUID 유지

## 최종 구조

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

## 이동 수

- Data: 9개
- Map: 21개
- Item: 23개
- 총 이동: 53개

## 검증 기준

1. 기존 경로의 53개 `.cs`가 모두 없어야 함
2. 새 기능별 경로의 53개 `.cs`가 모두 존재해야 함
3. 각 이동 전후 `.meta` GUID가 같아야 함
4. `ProjectJ.Runtime.asmdef`가 Runtime 루트에 그대로 존재해야 함
5. Runtime 내부 `.asmdef`는 1개만 존재해야 함
6. Unity Console Error 0
7. EditMode 전체 Failed 0
8. PlayMode 전체 Failed 0
9. Game Scene과 주요 Prefab의 Missing Script 0
