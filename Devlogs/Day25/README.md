# 25일차 개발일지 - 정육면체 Module 규격 및 고정맵 수동 조립 기반 구축

## 1. 개발 목표

25일차부터 Phase 3에 진입한다.

이번 일차의 핵심은 하나의 큰 맵을 직접 제작하는 것이 아니라, 앞으로 **고정맵과 절차 생성 랜덤맵이 공통으로 사용할 최소 맵 단위인 정육면체 Module 규격을 확정하는 것**이다.

Project J의 맵은 동일한 크기의 Module Prefab을 레고 블록처럼 연결하는 방식으로 구성한다.

고정맵에서는 사람이 Module을 직접 연결하고, 이후 절차 생성 단계에서는 같은 Module을 생성기가 규칙에 따라 자동으로 연결한다.

현재 기준 커밋:

```text
e4180d41824ab56db33586056c1de34d6b4895a4
```

현재 커밋 메시지:

```text
25
```

---

# 2. 기본 Module 규격

플레이어 키의 기준값은 다음과 같다.

```text
Player Height Reference = 2
```

25일차 Greybox Module의 한 변은 다음 값으로 시작한다.

```text
Module Size = 20
```

따라서 모든 기본 Module은:

```text
20 × 20 × 20
```

크기의 `1 : 1 : 1` 정육면체 공간을 사용한다.

`20`은 현재 Greybox 검증용 기준값이며 이후 실제 플레이 테스트에 따라 조정할 수 있다.

중요한 원칙은 **모든 Module이 동일한 정육면체 규격을 공유한다는 것**이다.

---

# 3. Module 기본 구조

하나의 Module은 기본적으로 6개의 면을 기준으로 한다.

```text
North
South
East
West
Up
Down
```

실제 Geometry는 다음 구조로 분리한다.

```text
Module
├─ Geometry
│  ├─ Floor
│  ├─ Ceiling
│  ├─ Wall_North
│  ├─ Wall_South
│  ├─ Wall_East
│  └─ Wall_West
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

각 면을 하나의 Mesh에 합치지 않고 분리함으로써 Module Variant마다 필요한 면만 열거나 닫을 수 있게 했다.

---

# 4. Face 상태

각 Module의 6면에는 다음 상태 중 하나를 부여한다.

```text
Closed
Entrance
Exit
Drop
```

각 상태의 의미는 다음과 같다.

| 상태 | 역할 |
|---|---|
| Closed | 벽·바닥·천장으로 막힌 면 |
| Entrance | 이전 Module에서 현재 Module로 들어오는 정상 진행 입구 |
| Exit | 현재 Module에서 다음 Module로 나가는 정상 진행 출구 |
| Drop | Module 자체가 막지 않는 열린 위험 공간 |

`Drop`은 정상 진행용 Exit로 취급하지 않는다.

---

# 5. Entrance / Exit 필수 규칙

일반 Module은 반드시 다음 조건을 만족해야 한다.

```text
Entrance >= 1
Exit >= 1
```

즉 정상 진행 Module은 들어올 수 있는 입구와 다음 진행을 위한 출구를 각각 최소 하나 이상 가져야 한다.

현재 `MapModule.IsDefinitionValid()`에서:

- Socket 6개 존재
- 방향 중복 없음
- Entrance 1개 이상
- Exit 1개 이상

을 검사한다.

---

# 6. Socket 연결 규칙

정상적인 진행 연결은 반드시:

```text
현재 Module Exit
↓
다음 Module의 반대 방향 Entrance
```

형태를 사용한다.

예:

```text
North Exit
↕
South Entrance
```

```text
East Exit
↕
West Entrance
```

```text
Up Exit
↕
Down Entrance
```

반대로 다음 연결은 정상 진행 연결로 인정하지 않는다.

```text
Exit → Exit
Entrance → Entrance
Drop → Entrance
Closed → Entrance
```

이 규칙은 이후 절차 생성에서도 그대로 사용한다.

---

# 7. 3차원 Grid 방향

Module은 X / Y / Z의 3차원 Grid Cell 기준으로 연결할 수 있게 방향 Offset을 정의했다.

```text
North = ( 0,  0, +1)
South = ( 0,  0, -1)

East  = (+1,  0,  0)
West  = (-1,  0,  0)

Up    = ( 0, +1,  0)
Down  = ( 0, -1,  0)
```

따라서 Project J의 맵은 단순 평면 미로가 아니라 수평·수직 연결을 모두 사용할 수 있는 3차원 Module 구조를 전제로 한다.

---

# 8. 기본 Module Variant

25일차에서는 다음 기본 Module Prefab을 생성했다.

```text
PJ_Module_Straight_SouthNorth
PJ_Module_Corner_SouthEast
PJ_Module_Vertical_DownUp
PJ_Module_Branch_SouthNorthEast
PJ_Module_Merge_SouthWestNorth
PJ_Module_Drop_SouthNorth_EastDrop
PJ_Module_Start_SouthUp
```

## Straight

```text
South = Entrance
North = Exit
```

직선 진행을 담당한다.

## Corner

```text
South = Entrance
East = Exit
```

진행 방향을 90도 바꾸는 기본 Module이다.

## Vertical

```text
Down = Entrance
Up = Exit
```

한 Module 높이만큼 상승하는 수직 진행 Module이다.

내부에는 현재 Player 이동으로 위쪽까지 올라갈 수 있도록 Ramp와 Landing Greybox를 배치했다.

## Branch

```text
South = Entrance
North = Exit
East = Exit
```

하나의 진행선을 둘 이상의 경로로 분기하는 기본 구조다.

## Merge

```text
South = Entrance
West = Entrance
North = Exit
```

서로 다른 진행 경로를 하나로 합치는 기본 구조다.

## Drop

```text
South = Entrance
North = Exit
East = Drop
```

정상 진행 경로와 낙하 위험 공간이 함께 존재하는 테스트 Module이다.

## Start

```text
South = Entrance
Up = Exit
```

고정 테스트 코스의 시작 구간에서 사용한다.

---

# 9. 열린 면의 Geometry 처리

25일차 Editor 도구에서는 Face 상태가:

```text
Closed
```

인 면만 실제 Geometry를 생성한다.

따라서:

```text
Entrance
Exit
Drop
```

으로 지정된 방향은 해당 면 전체가 열린 상태가 된다.

현재는 기능 확인용 Greybox 단계이므로 문틀·부분 개구부·장식 구조를 만들지 않는다.

---

# 10. Gameplay 영역 기반

향후 Module 내부에서 장애물과 아이템을 안전하게 랜덤 배치할 수 있도록 각 Prefab에 다음 빈 부모를 미리 마련했다.

```text
Gameplay
├─ ObstacleSpawnAreas
├─ ItemSpawnAreas
└─ NoSpawnAreas
```

25일차에서는 실제 랜덤 Spawn 로직은 구현하지 않는다.

추후 Entrance·Exit·Socket, 필수 진행선, 착지 위치와 Player Capsule 공간을 보존하는 보수적인 범위 안에서 Spawn 영역을 설정할 예정이다.

---

# 11. 수직 이동용 Greybox

`Vertical`과 `Start` Module에는 20m 높이를 실제 Player가 올라갈 수 있도록 테스트용 구조를 추가했다.

기본 흐름:

```text
Bottom Landing
↓
Ramp A
↓
Middle Landing
↓
Ramp B
↓
Top Landing
```

이 구조는 최종 맵 아트가 아니라 Module 연결과 플레이 가능성을 확인하기 위한 Greybox다.

---

# 12. 0m → 1000m 고정맵 기준 Scene

25일차 Editor 메뉴를 실행하면 다음 Scene을 생성한다.

```text
Assets/ProjectJ/Tests/Manual/Day25/
└─ Day25_ModuleFixedMap.unity
```

Hierarchy의 큰 구조는 다음과 같다.

```text
=== Day25 Fixed Module Map ===
├─ Section_01
├─ Section_02
├─ Section_03
├─ Section_04
├─ Section_05
├─ START_0m
├─ Checkpoint_01_200m
├─ Checkpoint_02_400m
├─ Checkpoint_03_600m
├─ Checkpoint_04_800m
└─ FINISH_1000m
```

---

# 13. 1000m 높이 구조

현재 Module 한 변은 20m이므로:

```text
20m × 50 Module = 1000m
```

구조를 사용한다.

기준 높이:

```text
START = 0m

CP1 = 200m
CP2 = 400m
CP3 = 600m
CP4 = 800m

FINISH = 1000m
```

각 Section은 10개의 20m Module을 기준으로 한다.

```text
Section 1 = 0 ~ 200m
Section 2 = 200 ~ 400m
Section 3 = 400 ~ 600m
Section 4 = 600 ~ 800m
Section 5 = 800 ~ 1000m
```

이번 고정맵은 절차 생성기가 아닌 동일 Module Prefab을 규칙적으로 연결한 검증용 구조다.

---

# 14. 고정맵과 랜덤맵의 관계

앞으로 두 맵 방식은 별개의 시스템으로 만들지 않는다.

```text
고정맵
Module Prefab
→ 개발자가 직접 배치
→ 정해진 코스
```

```text
랜덤맵
같은 Module Prefab
→ 생성기가 자동 배치
→ Seed에 따른 코스
```

즉 25일차에서 만든 Module이 이후 절차 생성 시스템에서도 그대로 사용되는 것이 원칙이다.

---

# 15. 향후 Route Graph 기준

절차 생성 단계에서는 단순히 Module을 무작위로 붙이는 방식이 아니라:

```text
START
↓
Entrance → Exit
↓
다음 Module Entrance
↓
...
↓
FINISH 1000m
```

의 정상 진행 Graph를 구성해야 한다.

향후 필수 규칙:

```text
START → FINISH 경로 필수

정상 진행은 수평 또는 상승

하강 진행 경로 금지

FINISH 전 Dead End 금지

Branch는 계속 진행하거나 Merge

모든 정상 Branch는 최종적으로 FINISH에 도달 가능
```

25일차에서는 이 전체 Generator를 구현하지 않고, 이를 구현할 수 있는 Module·Socket 데이터 기반까지만 만든다.

---

# 16. EditMode 테스트

새 테스트:

```text
Assets/ProjectJ/Tests/EditMode/
└─ MapModuleTests.cs
```

주요 검증 항목:

- Entrance 1개 이상 + Exit 1개 이상이면 유효
- Drop이 Exit 역할을 대신할 수 없음
- Entrance가 없는 Module 거부
- North Exit ↔ South Entrance 연결
- Up Exit ↔ Down Entrance 연결
- Exit ↔ Exit 연결 거부
- Drop을 정상 진행 연결로 사용하지 않음
- 6방향 반대 방향 계산
- Up 방향 Grid Offset
- Module Size 20 / Player Height Reference 2 확인

---

# 17. 주요 Runtime 스크립트

## `MapModule.cs`

Module의 공통 데이터를 관리한다.

담당 범위:

```text
Module ID
Module Size
Socket 목록
Entrance 수
Exit 수
Module 정의 검증
Socket 검색
Exit→Entrance 연결 검증
반대 방향 계산
Grid Cell 방향 계산
```

## `MapModuleSocket.cs`

한 면의 연결 정보를 담당한다.

```text
Direction
State
IsOpen
```

을 제공한다.

## `MapModuleFaceDirection.cs`

```text
North
South
East
West
Up
Down
```

6방향을 정의한다.

## `MapModuleFaceState.cs`

```text
Closed
Entrance
Exit
Drop
```

4개의 Face 상태를 정의한다.

---

# 18. Editor 제작 도구

새 메뉴:

```text
ProjectJ
→ Day25
→ Create Module Prefabs And Fixed Map
```

실행 시:

```text
기본 Module Prefab 생성
↓
Module Definition 검증
↓
Prefab 저장
↓
5개 Section 생성
↓
20m Module 50개 배치
↓
START / CP1~CP4 / FINISH Anchor 생성
↓
Player 배치
↓
Main Camera 배치
↓
Day25 Scene 저장
```

순서로 처리한다.

---

# 19. 생성된 주요 Asset

Prefab 경로:

```text
Assets/ProjectJ/Prefabs/Map/Modules/Day25/
```

Runtime 코드:

```text
Assets/ProjectJ/Runtime/Map/
├─ MapModule.cs
├─ MapModuleSocket.cs
├─ MapModuleFaceDirection.cs
└─ MapModuleFaceState.cs
```

Editor 코드:

```text
Assets/ProjectJ/Editor/
└─ Day25ModuleSetup.cs
```

Test:

```text
Assets/ProjectJ/Tests/EditMode/
└─ MapModuleTests.cs
```

Manual Scene:

```text
Assets/ProjectJ/Tests/Manual/Day25/
└─ Day25_ModuleFixedMap.unity
```

---

# 20. 현재 단계에서 하지 않은 것

25일차에는 다음 기능을 구현하지 않았다.

```text
절차적 랜덤 생성
Seed
자동 Branch 생성
자동 Merge 생성
1000m Graph 자동 보장
Dead End 자동 검증
Module 회전 Resolver
Module Cell 중첩 검사
장애물 랜덤 Spawn
아이템 랜덤 Spawn
체크포인트 기능
부활 기능
순위 기능
FINISH 판정
네트워크 동기화
```

이번 일차는 이후 기능을 만들기 위한 **Module 제작 규격과 고정맵 기준선**에 집중한다.

---

# 21. 수동 테스트 체크리스트

## Module

- [ ] 모든 Module이 동일한 20×20×20 규격
- [ ] 6개의 Socket 존재
- [ ] Entrance 1개 이상
- [ ] Exit 1개 이상
- [ ] Closed 면만 Geometry가 존재
- [ ] Drop이 정상 진행 Exit로 처리되지 않음

## 연결

- [ ] North Exit ↔ South Entrance 연결 정상
- [ ] East Exit ↔ West Entrance 연결 정상
- [ ] Up Exit ↔ Down Entrance 연결 정상
- [ ] Socket 위치가 Module 경계와 일치
- [ ] 인접 Module 사이에 큰 틈이 없음

## 고정맵

- [ ] START 0m 존재
- [ ] CP1 200m 존재
- [ ] CP2 400m 존재
- [ ] CP3 600m 존재
- [ ] CP4 800m 존재
- [ ] FINISH 1000m 존재
- [ ] 5개 Section 존재
- [ ] Module Prefab 기반으로 구성

## 플레이어

- [ ] START에서 이동 가능
- [ ] Vertical Module 내부 Ramp 이동 가능
- [ ] 다음 Module로 연결 가능
- [ ] Player가 Geometry에 끼이지 않음
- [ ] 카메라가 벽·천장에서 치명적으로 깨지지 않음

---

# 22. 테스트 확인

GitHub 저장소에는 현재 이 커밋에 대한 CI 상태가 등록되어 있지 않다.

따라서 아래 항목은 Unity 로컬 환경에서 직접 확인한다.

```text
EditMode Run All
PlayMode Run All
Console Error 0
```

체크:

- [ ] `MapModuleTests` 전체 Green
- [ ] 기존 EditMode 테스트 전체 Green
- [ ] PlayMode 테스트 전체 Green
- [ ] Console Error 0

---

# 23. 개발 결과

25일차에서는 Project J의 맵 제작 방식을 **동일 규격 정육면체 Module 기반 구조**로 전환했다.

핵심 구조는 다음과 같다.

```text
정육면체 Module
↓
6면
↓
Closed / Entrance / Exit / Drop
↓
6방향 Socket
↓
Exit → 다음 Module Entrance
↓
수평 / 수직 Module 연결
```

또한 앞으로 고정맵과 랜덤맵이 서로 다른 맵 제작 방식을 사용하지 않고 같은 Module Prefab을 공유하도록 기반을 만들었다.

현재 0m에서 1000m까지의 5구간 고정 테스트 Scene도 같은 Module 규격을 사용해 구성되어 있다.

향후 절차 생성에서는 이 구조 위에:

```text
Grid Cell 점유
Module 회전
Main Route Backbone
Branch
Merge
Dead End 방지
하강 진행 방지
1000m FINISH 보장
Safe Spawn Volume
Traversal 검증
```

을 순차적으로 추가한다.

25일차의 가장 중요한 결과물은 특정 고정맵 하나가 아니라 **Project J 전체 맵 시스템이 공통으로 따라야 할 Module 제작 규격을 코드와 Prefab으로 처음 확정한 것**이다.
