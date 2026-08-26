---
# Project J - 144일차 개발일지

---
## 개발 주제

절차 생성 랜덤맵과 시연용 고정맵이 함께 사용할 **10×10×10 정육면체 Map Module 규격**으로 기존 맵 모듈을 재정비하고, 3차원 적층형 랜덤맵에 사용할 다양한 경로 바리에이션을 확장하였다.

---
## 개발 목표

Project J의 맵은 평면 위에 방을 이어 붙이는 구조가 아니라, 동일한 크기의 정육면체 Module을 X / Y / Z 3차원 Grid에 레고 블록처럼 쌓고 연결하는 구조를 사용한다.

기존 Day25에서 확정한 다음 원칙을 유지한다.

```text
Module
= 동일 크기 정육면체 Cell

연결 방향
= North / South / East / West / Up / Down

정상 연결
= 현재 Module Exit
  → 다음 Module 반대 방향 Entrance

고정맵
= 개발자가 같은 Module Prefab을 직접 배치

랜덤맵
= 생성기가 같은 Module Prefab을 자동 배치
```

기존 20×20×20 규격은 실제 플레이 공간에 비해 지나치게 커서 이번 일차부터 기본 Module 크기를 다음과 같이 변경하였다.

```text
20 × 20 × 20
↓
10 × 10 × 10
```

이번 일차의 목표는 다음과 같다.

- 기존 Day25 기본 Module을 10m 규격으로 재생성
- Runtime `MapModule`의 기본 크기를 10m로 변경
- 6방향 Socket 위치를 10m Cell 규격에 맞게 조정
- 기존 Day144 기본 Module을 10m 규격으로 재생성
- 랜덤맵 반복감을 줄이기 위한 Module 바리에이션 확장
- 3차원 수평·수직 Branch / Merge 구조 유지
- Day144 Module만 독립적으로 삭제·재생성할 수 있는 Editor 메뉴 구성
- EditMode 테스트의 기본 Module Size 기준을 10m로 변경

---
## 기준 커밋

이번 개발 정리 시점의 최신 `main` 기준:

```text
0bb788da3eaeec9bbf4f731485ae72bbd27e6bd0
```

현재 커밋 메시지:

```text
A
```

이전 정식 일차 커밋:

```text
6167e53 143일차 : 캐릭터 애니메이션 연동 및 Animator 전환 디버깅
```

---
## 10m 정육면체 Module 규격

Runtime 공통 기준값을 다음과 같이 변경하였다.

```text
MapModule.DefaultModuleSize = 10
PlayerHeightReference = 2
```

하나의 Module은 다음 공간을 점유한다.

```text
10m × 10m × 10m
```

Module 중심을 `(0, 0, 0)`으로 두었을 때 각 Socket은 정육면체 면 중심에 위치한다.

```text
North = ( 0,  0, +5)
South = ( 0,  0, -5)

East  = (+5,  0,  0)
West  = (-5,  0,  0)

Up    = ( 0, +5,  0)
Down  = ( 0, -5,  0)
```

따라서 생성기는 한 Cell에서 다음 Cell로 이동할 때 X / Y / Z축으로 10m 단위의 공간을 사용할 수 있다.

---
## 3차원 Grid 구조

Project J의 랜덤맵은 수평 미로가 아니라 위쪽 진행을 포함한 3차원 적층 구조를 전제로 한다.

예:

```text
                [Cube]
                   ↑
           [Cube]──[Cube]
              ↑
      [Cube]──[Cube]
         ↑
      [START]
```

Grid Cell 방향은 기존 규칙을 유지한다.

```text
North = ( 0,  0, +1)
South = ( 0,  0, -1)

East  = (+1,  0,  0)
West  = (-1,  0,  0)

Up    = ( 0, +1,  0)
Down  = ( 0, -1,  0)
```

실제 월드 배치 시에는 이 Cell Offset과 10m Module Size를 조합하여 다음 위치를 계산할 수 있다.

```text
현재 Cell
+
방향 Cell Offset
=
다음 Cell
```

---
## Socket 연결 규칙

각 Module은 6개의 방향 Socket을 사용한다.

```text
North
South
East
West
Up
Down
```

Face 상태는 기존 규칙을 유지한다.

```text
Closed
Entrance
Exit
Drop
```

정상 진행 연결은 반드시 다음 형태다.

```text
Exit
↓
반대 방향 Entrance
```

예:

```text
North Exit ↔ South Entrance
East Exit  ↔ West Entrance
Up Exit    ↔ Down Entrance
```

다음 연결은 정상 진행으로 인정하지 않는다.

```text
Exit → Exit
Entrance → Entrance
Drop → Entrance
Closed → Entrance
```

---
## 기존 Day25 기본 Module 10m 전환

기존 Day25 기본 Module 7종을 삭제하지 않고 동일 ID와 기존 GUID를 기준으로 10m 규격으로 재생성하도록 구성하였다.

대상:

```text
PJ_Module_Straight_SouthNorth
PJ_Module_Corner_SouthEast
PJ_Module_Vertical_DownUp
PJ_Module_Branch_SouthNorthEast
PJ_Module_Merge_SouthWestNorth
PJ_Module_Drop_SouthNorth_EastDrop
PJ_Module_Start_SouthUp
```

기존 Prefab 참조가 깨지는 것을 줄이기 위해 Day25 Module의 기존 GUID를 보존하는 방식으로 복구·재생성한다.

---
## Day144 Module 바리에이션 확장

기존 기본형만 반복될 경우 랜덤맵에서 코스의 체감 반복이 빠르게 발생할 수 있기 때문에 Day144에서는 내부 이동 방식과 연결 형태가 다른 바리에이션을 추가하였다.

Day144 생성 도구의 현재 목표 수량:

```text
40 Variations
```

주요 바리에이션 계열은 다음과 같다.

### 직선·이동 변형

```text
기본 직선
좁은 직선
넓은 진행 공간
슬라럼
중앙 기둥 우회
좁은 다리
분리 차선
```

같은 South → North 연결이라도 내부 Geometry를 달리하여 실제 이동 경로와 플레이 감각이 달라지도록 구성한다.

### Corner 변형

```text
South → East
South → West
좁은 Corner
위험 공간을 포함한 Corner
```

랜덤맵이 한 방향으로만 길게 늘어나는 것을 막고 수평 방향 변화를 만든다.

### Jump 변형

```text
단일 Jump
2연속 Jump
징검다리형 Platform
중앙 Gap
분리 Platform
```

모든 Jump 구조는 아이템이나 다른 플레이어의 도움 없이 기본 이동과 점프로 통과 가능한 범위 안에서 사용하는 것을 전제로 한다.

### Low Passage 변형

```text
중앙 낮은 통로
좌측 낮은 통로
우측 낮은 통로
```

앉기 이동을 사용하는 코스 변형을 제공한다.

### Branch 변형

```text
2방향 Branch
3방향 Branch
4방향 Branch
수평 + 수직 혼합 Branch
```

정상 Branch의 모든 경로는 이후 계속 진행하거나 Merge될 수 있어야 한다.

### Merge 변형

```text
2경로 Merge
3경로 Merge
4경로 Merge
수직 경로 포함 Merge
```

여러 진행 경로를 다시 하나의 정상 진행선으로 합치는 데 사용한다.

### Vertical 변형

```text
Down → Up Ramp
Down → Up Stairs
Vertical Platform
수평 → Up 전환
Down → 수평 전환
```

10m Cell을 위로 쌓아가는 Project J의 수직 등반 구조에 사용한다.

### Drop·위험 공간 변형

```text
한쪽 Drop
양쪽 Drop
Corner + Drop
Branch + Drop
```

`Drop`은 정상 Exit가 아니며 낙하 위험 공간으로만 사용한다.

---
## Module 내부 기본 구조

Day144 Module도 기존 Module 구조를 따른다.

```text
Module
├─ Geometry
│
├─ Sockets
│  ├─ Socket_North
│  ├─ Socket_South
│  ├─ Socket_East
│  ├─ Socket_West
│  ├─ Socket_Up
│  └─ Socket_Down
│
└─ Gameplay
   ├─ ObstacleSpawnAreas
   ├─ ItemSpawnAreas
   └─ NoSpawnAreas
```

향후 장애물과 아이템을 Module 내부에 배치할 때 Entrance / Exit, 필수 착지 공간, 통과 공간과 충돌하지 않도록 Gameplay 영역을 확장해서 사용할 수 있다.

---
## Editor 자동 생성 도구

이번 일차에는 다음 Editor Script를 사용한다.

```text
Assets/ProjectJ/Editor/Day144/
└─ Day144CubeModuleSetup.cs
```

주요 메뉴:

```text
ProjectJ
└─ Day144
   ├─ 1. Rebuild All Modules To 10m And Add Variations
   ├─ 2. Validate 10m Cube Modules
   ├─ 3. Delete Day144 Variations Only
   └─ 4. Rebuild Day25 Base Modules To 10m
```

### 전체 재생성

```text
1. Rebuild All Modules To 10m And Add Variations
```

실행 시:

```text
Day25 기존 7종 복구 / 재생성
↓
Day144 기존 생성물 정리
↓
Day144 40종 생성
↓
Asset 저장
↓
10m Module 검증
```

순서로 처리한다.

### 검증

```text
2. Validate 10m Cube Modules
```

다음 항목을 검사한다.

```text
Day25 예상 Prefab 수
Day144 예상 Prefab 수

MapModule 존재 여부
Module 정의 유효 여부
Module Size = 10
Entrance >= 1
Exit = 1~4
6방향 Socket 구성
Runtime DefaultModuleSize = 10
```

### Day144만 삭제

```text
3. Delete Day144 Variations Only
```

Day25 기본 Module은 유지하고 Day144에서 생성한 바리에이션만 제거한다.

### Day25만 재생성

```text
4. Rebuild Day25 Base Modules To 10m
```

기존 Day25 7종을 10m 규격으로 다시 구축한다.

---
## 기존 잘못된 평면형 프로토타입 정리

144일차 작업 초기에 별도의 평면형 Module 시스템을 추가하는 방향을 검토했으나, 기존 Project J에 이미 정육면체 Module 기반이 존재하는 것을 다시 확인하였다.

따라서 별도의 중복 맵 시스템을 유지하지 않고 기존:

```text
ProjectJ.Map.MapModule
ProjectJ.Map.MapModuleSocket
MapModuleFaceDirection
MapModuleFaceState
```

체계를 그대로 사용하는 방향으로 통일하였다.

잘못 추가된 평면형 프로토타입은 정확한 파일 경로만 삭제하도록 정리 도구의 범위를 제한하였다.

기존 정상 Module 폴더 전체를 삭제하지 않도록 수정하였다.

---
## EditMode 테스트 변경

기존 `MapModuleTests`의 Module Size 테스트를 10m 기준으로 변경하였다.

현재 핵심 테스트:

```text
Entrance + Exit 최소 규칙
Drop은 Exit 대체 불가
Entrance 누락 거부
North Exit ↔ South Entrance
Up Exit ↔ Down Entrance
Exit ↔ Exit 연결 거부
Drop 정상 진행 연결 거부
3축 반대 방향 계산
Up Grid Offset = (0, 1, 0)
DefaultModuleSize = 10
PlayerHeightReference = 2
```

---
## 수정 및 추가 파일

주요 코드:

```text
Assets/ProjectJ/Runtime/Map/
└─ MapModule.cs

Assets/ProjectJ/Editor/Day144/
├─ Day144CubeModuleSetup.cs
└─ Day144LegacyFlatModuleCleanupTool.cs

Assets/ProjectJ/Tests/EditMode/
└─ MapModuleTests.cs
```

기본 Module:

```text
Assets/ProjectJ/Prefabs/Map/Modules/Day25/
└─ 기존 기본 Module 7종
```

Day144 바리에이션:

```text
Assets/ProjectJ/Prefabs/Map/Modules/Day144/
└─ PJ144_Module_*.prefab
```

---
## 최신 커밋 확인

정리 시점 최신 커밋:

```text
0bb788da3eaeec9bbf4f731485ae72bbd27e6bd0
```

확인된 내용:

```text
MapModule.DefaultModuleSize = 10
Day144CubeModuleSetup의 ModuleSize = 10
Day144 예상 Prefab 수 = 40
Day25 예상 Prefab 수 = 7
EditMode Module Size 테스트 = 10
Day144 Prefab 폴더 존재
```

코드와 직렬화된 자산을 기준으로 확인했을 때 이번 144일차 맵 모듈 작업을 막는 직접적인 충돌은 확인되지 않았다.

다만 현재 GitHub Commit에는 별도의 CI Status가 등록되어 있지 않기 때문에 Unity Editor 실제 컴파일, EditMode Test Runner 실행, Player / Bot Play Mode 통과 여부는 로컬 Unity에서 최종 확인해야 한다.

---
## 플레이 검증 항목

다음 단계에서 실제 Unity Play Mode로 확인할 항목:

```text
Player가 10m Module 연결부에서 걸리지 않는지
Bot이 같은 연결부를 통과할 수 있는지

Straight 통과
Corner 통과
Low Passage 통과
Jump 통과
Ramp 통과
Stairs 통과

Up / Down Module 연결
Branch 모든 경로 통과
Merge 모든 진입 경로 통과

Module 경계 Collider 틈
Module 경계 Collider 중첩
낙하 공간 의도 확인
```

---
## 결과

144일차에는 Project J의 맵 Module 규격을 기존 20×20×20에서 10×10×10으로 축소하였다.

기존 Day25 기본 Module 7종을 새로운 10m 규격으로 재생성할 수 있게 하고, Runtime `MapModule.DefaultModuleSize`와 EditMode 테스트 기준도 동일하게 10m로 변경하였다.

모든 Module은 North / South / East / West / Up / Down 6방향 Socket 규칙을 유지하며, 수평뿐 아니라 위·아래 방향으로 Module을 쌓을 수 있는 3차원 Grid 구조를 그대로 사용한다.

또한 랜덤맵의 반복감을 줄이기 위해 Day144 Module 후보를 40종으로 확장하였다. 직선, Corner, Jump, Low Passage, Branch, Merge, Vertical, Drop 등 여러 이동 형태를 동일한 10m Cell 규격 안에서 조합할 수 있는 기반을 마련하였다.

이제 시연용 고정맵에서는 이 Module을 직접 조립하고, 이후 절차 생성 랜덤맵에서는 동일한 Module Prefab을 생성 후보로 사용하면 된다.

---
## 다음 일차

145일차에서는 이번에 구축한 10m 정육면체 Module을 기반으로 실제 경기에서 사용할 장애물 Module을 제작한다.

주요 대상:

```text
Moving Platform
회전 장애물
왕복 장애물
점프 방해 구조
밀침 / 낙하 위험 구조
```

장애물은 Module의 Entrance / Exit와 필수 진행선을 막지 않도록 배치하고, 이후 랜덤맵에서도 같은 Module 규칙 안에서 사용할 수 있도록 구성한다.

---
## 추천 커밋 제목

```text
144일차 : 10m 정육면체 맵 모듈 규격 전환 및 40종 바리에이션 구축
```
