# Project J 개발 일지 - 132일차

## 개발 주제

**유도탄·드론 Route Node Scene 배치 및 장애물 우회 기반 검증**

132일차에는 이미 구현되어 있던 유도탄과 드론의 Route Node/BFS 경로 시스템을 실제 `Game.unity` 고정맵에 연결했다.

기존에는 유도탄과 드론이 목표까지 직선 경로가 열려 있을 때는 정상 추적할 수 있었지만, 장애물이 있는 경우 Scene에 실제 Route Node Graph가 없어 우회 경로를 사용할 수 없었다.

이번 작업에서는 `Game.unity`에 유도탄·드론이 공용으로 사용할 Route Node Graph를 생성하고, Scene 자체를 대상으로 구조를 검증하는 EditMode 테스트를 추가했다.

---

## 1. Route Node 전용 Scene 그룹 구성

`Game.unity`에 다음 전용 Root를 추가했다.

`=== ROUTE NODES ===`

해당 Root 아래에는 다음 형식의 Route Node들이 배치된다.

- `RouteNode_001_L`
- `RouteNode_001_C`
- `RouteNode_001_R`
- `RouteNode_002_L`
- `RouteNode_002_C`
- `RouteNode_002_R`
- 이후 고정맵 진행 경로에 따라 반복

각 Route Node에는 기존 `ProjectJHomingMissileRouteNode` 컴포넌트를 사용한다.

별도의 새로운 Route Node 타입은 만들지 않았다.

---

## 2. 유도탄·드론 공용 Route Graph

유도탄과 드론을 위해 서로 다른 경로 시스템을 만들지 않고 기존 `ProjectJHomingMissileRouteNode` Graph를 공용으로 사용한다.

동작 방향은 다음과 같다.

1. 목표까지 직선 경로가 열려 있으면 직접 추적
2. 장애물이 가로막고 있으면 주변 Route Node 검색
3. Route Node Graph를 BFS로 탐색
4. 선택된 Node를 순서대로 따라 장애물 우회
5. 목표까지 직선 경로가 다시 확보되면 직접 추적으로 복귀

드론 역시 유도탄과 같은 Route Node Graph를 사용한다.

---

## 3. 고정맵 기준 Route Node 자동 배치

Route Node 생성 시 고정맵의 START부터 FINISH까지 주요 코스 오브젝트를 기준으로 경로 행을 구성했다.

주요 기준 구간:

- Start Plaza
- Stage Step 구간
- CP1 Push Arena
- CP2 Deck
- CP3 Deck
- CP4 Deck
- Final Step
- Finish Deck

각 Route 행에는 세 개의 Lane을 사용한다.

- L: 왼쪽
- C: 중앙
- R: 오른쪽

이를 통해 단일 중앙선만 존재하는 Graph보다 장애물 주변을 여러 방향으로 우회할 수 있는 기본 구조를 확보했다.

---

## 4. Route Node 연결 규칙

같은 행의 Route Node는 다음처럼 연결한다.

`L ↔ C ↔ R`

그리고 다음 행의 같은 Lane도 연결한다.

- L ↔ 다음 행 L
- C ↔ 다음 행 C
- R ↔ 다음 행 R

Neighbour 관계는 양방향으로 저장한다.

따라서 어느 방향에서 경로 탐색을 시작하더라도 동일 Graph를 역방향으로 사용할 수 있다.

---

## 5. Route Node 간격

인접 Route 행은 최대 약 `7.5m`를 기준으로 세분화했다.

기존 유도탄과 드론의 Route Node 검색 반경보다 여유 있게 배치하여 현재 위치에서 주변 Node를 찾지 못하는 상황을 줄이는 것을 목표로 했다.

Route Node는 플레이어용 Waypoint가 아니라 비행형 유도탄·드론의 우회 경로 데이터이므로 지면에 직접 붙이지 않고 각 코스 Collider 상단에서 일정 높이를 띄운 위치를 사용한다.

---

## 6. Fusion NetworkObject 미사용

Route Node 자체는 네트워크 오브젝트가 아니다.

각 Route Node에는 다음만 필요하다.

- Transform
- `ProjectJHomingMissileRouteNode`

`NetworkObject`는 추가하지 않았다.

따라서 이번 Route Graph는 Host/State Authority가 경로 계산에 참고하는 고정 Scene 데이터이며 Route Node마다 Fusion Spawn/Despawn을 수행하지 않는다.

---

## 7. Scene 검증 테스트 추가

다음 EditMode 테스트를 추가했다.

`Assets/ProjectJ/Tests/EditMode/ProjectJDay132RouteNodeSceneTests.cs`

테스트는 `Game.unity` 자체를 열어 Route Node 구성을 검사한다.

검증 항목은 다음과 같다.

### GameScene_HasDedicatedRouteNodeRootAndEnoughNodes

- `=== ROUTE NODES ===` Root 존재 확인
- Root 아래에 충분한 Route Node가 존재하는지 확인

### RouteGraph_IsFullyConnectedAndSymmetric

- Route Node가 0개가 아닌지 확인
- 각 Node에 Neighbour가 존재하는지 확인
- Neighbour가 null이 아닌지 확인
- A → B 연결이 있으면 B → A도 존재하는지 확인
- BFS로 전체 Graph를 방문할 수 있는지 확인
- 인접 Node 간 거리가 허용 범위를 넘지 않는지 확인

### RouteNodes_AreClearOfSolidColliders

- Route Node 위치가 Solid Collider 내부에 생성되지 않았는지 확인

### RouteNodes_DoNotContainFusionNetworkObjects

- Route Node 오브젝트에 `Fusion.NetworkObject`가 붙어 있지 않은지 확인

총 4개의 Scene Route Node 검증 테스트를 추가했다.

---

## 8. 초기 검증 실패와 원인 확인

첫 번째 검증에서는 다음 두 테스트가 실패했다.

- `GameScene_HasDedicatedRouteNodeRootAndEnoughNodes`
- `RouteGraph_IsFullyConnectedAndSymmetric`

당시 테스트 결과에서 Route Node 개수가 `0`으로 확인됐다.

원인은 테스트 코드 자체가 아니라 실제 저장된 `Game.unity`에 Route Node Graph가 아직 정상 반영되지 않은 상태였기 때문이다.

이후 Route Node Scene 배치를 다시 적용하고 `Game.unity`를 저장하여 실제 Route Node 오브젝트와 `ProjectJHomingMissileRouteNode` 컴포넌트가 Scene에 포함되도록 수정했다.

---

## 9. 최종 Scene 상태

최신 `Game.unity`에는 다음 항목이 실제로 저장되어 있다.

- `=== ROUTE NODES ===` Root
- `RouteNode_001_L` 등의 Route Node GameObject
- `ProjectJHomingMissileRouteNode` 컴포넌트
- 각 Route Node의 `neighbours` 참조
- Route Node Transform 위치

예를 들어 `RouteNode_001_L`에는 기존 Route Node Script가 연결되어 있고 Neighbour 참조도 Scene YAML에 저장되어 있다.

따라서 132일차 Route Graph는 테스트 코드만 추가된 상태가 아니라 실제 Scene 데이터에도 반영되어 있다.

---

## 10. 변경 파일

131일차 커밋과 비교하여 132일차 최신 커밋에는 다음 3개 파일 변경이 포함된다.

### 수정

- `Assets/ProjectJ/Scenes/Game.unity`

### 생성

- `Assets/ProjectJ/Tests/EditMode/ProjectJDay132RouteNodeSceneTests.cs`
- `Assets/ProjectJ/Tests/EditMode/ProjectJDay132RouteNodeSceneTests.cs.meta`

Route Node 배치용 일회성 Editor Installer는 Scene 적용 후 제거되므로 최종 커밋에는 남기지 않는다.

---

## 11. Pickup 및 Network Prefab

이번 일차에서는 아이템 Pickup을 추가하지 않았다.

또한 Route Node 전용 Network Prefab도 생성하지 않았다.

아이템 Pickup 일괄 배치는 이후 계획된 통합 일차에서 처리한다.

---

## 12. 최신 커밋 확인

개발일지 작성 시점의 `main` 최신 커밋:

- SHA: `6398d236985d97126ab05715cfcdf1ab2c116872`
- 현재 커밋 메시지: `A`
- 이전 커밋: `cfa75bc74a89a329299767eb8c2bcc9d8b16cce5`
- 이전 커밋 제목: `131일차 : 손거울 서버 권한 4초 투사체 반사 및 반복 소유권 이전 구현`
- 이전 커밋 대비: 1 commit ahead / 0 behind

최신 커밋에는 `Game.unity` 수정과 Day132 Scene Route Node 테스트가 함께 포함되어 있다.

---

## 13. 검증 상태

사용자 Unity 환경에서 첫 실행 시 Route Node 0개로 인해 4개 테스트 중 2개가 실패했고, Route Node Scene 배치를 다시 적용한 뒤 문제 해결을 확인했다.

GitHub 최신 `Game.unity`에서도 Route Node Root와 실제 Route Node 컴포넌트, Neighbour 참조가 저장된 상태를 확인했다.

다만 GitHub에는 해당 커밋을 대상으로 한 별도의 CI / GitHub Actions 결과가 등록되어 있지 않다.

따라서 이 개발일지에서는 GitHub 자동화 기준의 전체 Unity Test Suite 통과를 별도로 주장하지 않는다.

---

## 132일차 결과

고정맵 `Game.unity`에 유도탄과 드론이 공용으로 사용할 Route Node Graph를 실제 배치했다.

기존에 코드로만 존재하던 BFS 우회 시스템이 Scene 데이터와 연결되어, 이후 유도탄과 드론이 장애물을 만났을 때 직접 추적과 Route Node 우회를 선택할 수 있는 기반을 갖췄다.

또한 Route Node 수, Graph 연결성, 양방향 Neighbour, Collider 겹침, Fusion NetworkObject 미사용 여부를 검사하는 Scene 전용 EditMode 테스트를 추가했다.
