# 125일차 개발일지 - 유도탄 서버 권한 자동 목표 추적

## 작업 개요

125일차에는 자동으로 목표 플레이어를 선정해 추적하는 **유도탄(Homing Missile)** 아이템의 네트워크 동작을 구현했다.

서버가 목표 선정, 투사체 생성, 이동, 충돌, 재탐색, 수명 종료를 판정하도록 구성했으며, 기존 Project J의 아이템 인벤토리와 외부 속도 시스템을 재사용했다.

이번 작업에서는 유도탄의 기본 직선 추적 기능까지 적용했으며, 장애물을 우회하기 위한 `ProjectJHomingMissileRouteNode` 스크립트는 준비했지만 실제 Scene에는 아직 Route Node를 배치하지 않았다.

---

## 구현 내용

### 1. 유도탄 아이템 등록

네트워크 아이템 목록에 유도탄을 추가했다.

- Network Item ID: `24`
- Key: `homing_missile`
- 표시 이름: `유도탄`
- 일반 아이템 사용 흐름에서 `UseHomingMissileAuthority()` 호출
- 발사 성공 시에만 아이템 소비
- 목표가 없거나 생성에 실패하면 아이템 유지

### 2. 서버 권한 자동 목표 선정

유도탄 사용 시 서버가 현재 경기의 플레이어를 검색한다.

- 사용자 자신 제외
- 유효하지 않은 NetworkObject 제외
- Gameplay 입력이 허용되지 않은 플레이어 제외
- 탐색 반경: **35m**
- 조건을 만족하는 대상 중 가장 가까운 플레이어 선택

목표가 존재하지 않으면 유도탄을 생성하지 않고 사용 실패로 처리한다.

### 3. NetworkObject 유도탄 생성

`ProjectJNetworkHomingMissile`을 새로 구현했다.

구성:

- `NetworkObject`
- `NetworkTransform`
- `ProjectJNetworkHomingMissile`

Resources Prefab:

`Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkHomingMissile.prefab`

Prefab에는 `FusionPrefab` 라벨을 적용했다.

### 4. 유도탄 이동

유도탄의 이동 속도는 **11m/s**로 설정했다.

State Authority가 매 Fusion Tick마다 목표 위치를 확인하고 이동 방향을 계산한다.

목표까지 장애물이 없는 경우에는 플레이어의 현재 위치를 직접 추적한다.

### 5. 최대 수명

유도탄은 생성 후 최대 **10초** 동안 존재한다.

목표를 계속 추적 중이더라도 10초가 지나면 서버에서 Despawn된다.

목표가 변경되더라도 전체 수명은 초기화하지 않는다.

### 6. 목표 재탐색

현재 목표가 유효하지 않거나 추적할 수 없게 된 경우 한 번만 새로운 목표를 검색한다.

- 최대 재탐색 횟수: **1회**
- 기존 목표는 재탐색 후보에서 제외
- 새 목표를 찾으면 계속 추적
- 재탐색에도 실패하면 유도탄 제거

### 7. 플레이어 적중 처리

유도탄이 플레이어에게 적중하면 기존 외부 속도 시스템을 사용한다.

- 외력 종류: `ProjectJExternalForceSource.Item`
- 외부 속도: **8m/s**
- 유도탄의 진행 수평 방향으로 적용
- 적중 후 유도탄 Despawn

기존 `TryApplyExternalVelocityChange()` 흐름을 재사용하므로 젤리 보호막, 부활 보호, 되감기 중 외력 보호와 같은 기존 방어 규칙을 유지할 수 있는 구조다.

### 8. 장애물 및 Route Node 구조

장애물이 목표와 유도탄 사이를 막고 있을 때 사용할 수 있도록 `ProjectJHomingMissileRouteNode`를 구현했다.

Route Node는 여러 이웃 Node를 연결할 수 있으며 BFS 방식으로 우회 경로를 찾도록 구성했다.

현재 상태:

- Route Node 스크립트 구현: 완료
- Route Node 연결 구조 구현: 완료
- BFS 경로 탐색 코드 구현: 완료
- 실제 Game Scene Route Node 배치: **미적용**

따라서 현재 Scene에서는 **장애물이 없는 직선 경로에서의 추적을 우선 사용할 수 있다.**

벽이나 구조물 뒤의 플레이어까지 안정적으로 추적시키는 기능은 이후 Scene에 Route Node를 배치하고 연결한 뒤 확인한다.

### 9. 투명 망토 연동 준비

향후 투명 망토 같은 은신 아이템을 유도탄 Target에서 제외할 수 있도록 목표 유효성 검사를 별도 함수로 분리했다.

현재는 모든 유효 플레이어를 보이는 대상으로 처리하며, 은신 아이템 구현 시 해당 조건만 확장할 수 있다.

### 10. 정책 분리 및 EditMode 테스트 작성

`ProjectJHomingMissilePolicy`를 추가해 주요 밸런스 값을 코드에서 분리했다.

주요 값:

| 항목 | 값 |
| --- | ---: |
| 목표 탐색 반경 | 35m |
| 이동 속도 | 11m/s |
| 최대 수명 | 10초 |
| 적중 외부 속도 | 8m/s |
| 재탐색 | 1회 |
| 충돌 반경 | 0.3m |
| Route Node 도착 거리 | 0.4m |
| Route Node 탐색 반경 | 12m |

`ProjectJHomingMissilePolicyTests.cs`에는 탐색 거리, 이동 거리, 재탐색 횟수, 목표 조건, Route Node 거리, 적중 외력, 수명 등의 정책 테스트를 작성했다.

---

## 주요 생성 파일

- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkHomingMissile.cs`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkItemInventory.HomingMissile.cs`
- `Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkHomingMissile.prefab`
- `Assets/ProjectJ/Runtime/Items/ProjectJHomingMissilePolicy.cs`
- `Assets/ProjectJ/Runtime/Items/ProjectJHomingMissileRouteNode.cs`
- `Assets/ProjectJ/Tests/EditMode/ProjectJHomingMissilePolicyTests.cs`

## 주요 수정 파일

- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkItemCatalog.cs`
- `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkItemInventory.cs`

---

## 현재 동작 흐름

```text
유도탄 사용
→ 서버가 35m 안의 가장 가까운 플레이어 탐색
→ 목표가 없으면 사용 실패 및 아이템 유지
→ 목표가 있으면 Network 유도탄 생성
→ 11m/s로 목표 추적
→ 플레이어 적중 시 수평 방향 8m/s 외력 적용
→ 유도탄 제거

목표 상실
→ 다른 목표 재탐색 1회
→ 성공하면 새 목표 추적
→ 실패하면 제거

10초 경과
→ 유도탄 제거
```

현재 Route Node를 Scene에 배치하지 않았기 때문에 장애물 우회 추적은 실제 Scene 테스트 대상에서 제외한다.

---

## 확인 상태

검토 기준 최신 커밋:

`2d8ac2358f27045d40cf8c4265faa8631c33ac22`

검토 당시 커밋 메시지:

`a`

124일차 커밋 `3e85325f502a68f9af600e6b95a488ee5137b82b` 대비 125일차 변경은 단일 커밋으로 적용되어 있다.

GitHub diff 기준으로 유도탄 NetworkObject, Item Catalog, Inventory 사용 분기, Prefab, 정책, Route Node, EditMode 테스트 파일이 반영된 것을 확인했다.

GitHub Actions CI Status 및 Workflow Run은 등록된 실행 결과가 없으므로 **Unity 실제 컴파일 및 Test Runner 통과 여부는 확인되지 않았다.**

Route Node는 코드만 준비된 상태이며 Game Scene 배치는 추후 진행한다.

---

## 다음 확인 항목

- Unity Console 컴파일 오류 여부 확인
- EditMode `ProjectJHomingMissilePolicyTests` 실행
- 2인 이상 멀티플레이에서 35m 자동 목표 선정 확인
- 11m/s 추적 및 10초 Despawn 확인
- 적중 시 8m/s 외부 속도 확인
- 목표 상실 후 재탐색 1회 확인
- 추후 Route Node를 Scene에 배치한 뒤 장애물 우회 추적 확인
