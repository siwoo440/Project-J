# 128일차 개발일지 - 드론 서버 권한 1위 추적 및 1회 공격·재탐색

## 작업 개요

128일차에는 `드론(Drone)` 아이템을 네트워크 아이템 시스템에 연결하고, 사용 시 서버가 현재 경쟁 순위 1위 플레이어를 선택하여 추적하는 Network Drone을 구현했다.

드론의 목표 선정, 이동, 최대 수명, 목표 재탐색, 공격, Despawn을 State Authority에서 처리하도록 구성했으며, 기존 Project J의 `RaceRank`, 공통 외력 시스템, 유도탄 Route Node 구조를 재사용했다.

---

## 구현 내용

### 1. 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 드론을 추가했다.

```text
Drone = 27
```

등록 정보:

- Network Item ID: `27`
- Key: `drone`
- 표시 이름: `드론`

기존 `SpikedArmor = 26` 다음 번호를 사용한다.

### 2. 기존 Drone ItemDefinition 재사용

`Item_Drone.asset`은 128일차 이전부터 이미 프로젝트에 존재하므로 새 ItemDefinition을 생성하지 않았다.

따라서 127일차 가시 갑옷처럼 ItemDefinition 전체 개수가 증가하지 않는다.

기존 데이터의 주요 값:

```text
itemId = drone
displayName = 드론
duration = 12초
```

### 3. 서버 현재 1위 목표 선정

드론 사용 시 State Authority가 현재 Runner의 활성 플레이어 목록을 확인한다.

초기 목표 조건:

- NetworkObject 유효
- Gameplay 활성
- 사용자 자신 제외
- 추적 가능한 상태
- `RaceRank == 1`

현재 순위는 별도의 높이 계산을 새로 만들지 않고 기존 `ProjectJNetworkExternalGameplay.RaceRank` 값을 그대로 사용한다.

동일한 1위 후보가 여러 명인 예외 상황에서는 PlayerRef Index가 낮은 대상을 우선 선택한다.

### 4. 1위 사용자는 드론 사용 불가

드론 사용자의 현재 `RaceRank`가 1이면 사용을 허용하지 않는다.

```text
사용자 RaceRank == 1
→ 사용 실패
→ 아이템 유지
```

드론을 현재 1위를 견제하는 전용 아이템으로 유지하기 위한 처리다.

### 5. Network Drone 생성

새 NetworkObject:

`Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkDrone.cs`

Network Prefab:

`Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkDrone.prefab`

Prefab 구성:

```text
ProjectJNetworkDrone
├─ NetworkObject
├─ NetworkTransform
└─ ProjectJNetworkDrone
```

Prefab에는 `FusionPrefab` 라벨을 적용했다.

### 6. 드론 Network 상태

주요 Networked 상태:

```text
NetworkInitialized
NetworkOwner
NetworkTarget
NetworkLifetimeTimer
NetworkReacquireCount
NetworkTargetRevision
```

드론 생성 시 서버가 Owner와 Target을 저장하고 12초 수명 Timer를 시작한다.

### 7. 9m/s 자동 추적

드론 이동 속도:

```text
9m/s
```

State Authority가 매 Fusion Tick Target 위치를 확인하고 이동한다.

목표와 드론 사이에 장애물이 없는 경우:

```text
Drone
→ Target 직접 추적
```

방식으로 이동한다.

### 8. 기존 유도탄 Route Node 재사용

장애물 우회를 위해 새로운 경로 시스템을 만들지 않고 기존:

```text
ProjectJHomingMissileRouteNode
```

를 재사용한다.

드론과 목표 사이가 막혀 있을 경우:

```text
현재 위치 주변 Route Node 탐색
→ Target 주변 Route Node 탐색
→ BFS 연결 경로 계산
→ 노드를 따라 이동
```

하도록 구현했다.

현재 Game Scene에는 해당 Route Node를 별도로 배치하지 않은 상태이므로 기본 열린 공간 직접 추적은 사용할 수 있지만, 실제 장애물 우회 경로는 Route Node 배치 후 확인한다.

### 9. 목표를 매 순위 변화마다 교체하지 않음

드론 생성 시 선택된 목표는 순위가 변하더라도 계속 추적한다.

예:

```text
사용 순간
A = 1위
→ Target A

이후 B가 새로운 1위가 됨
→ 기존 Target A 계속 추적
```

짧은 순위 변화로 드론이 여러 플레이어 사이를 반복 이동하는 상황을 방지한다.

### 10. 목표 재탐색 1회

현재 Target이 유효하지 않게 되면 재탐색을 한 번 시도한다.

최대 재탐색 횟수:

```text
1회
```

재탐색 시:

- 사용자 자신 제외
- 직전 Target 제외
- Gameplay 유효 대상만 사용
- 현재 RaceRank가 가장 높은 대상 우선
- 같은 Rank면 PlayerRef Index가 낮은 대상 우선

새 목표를 찾으면 추적을 계속하고, 찾지 못하면 Drone을 Despawn한다.

### 11. 향후 투명 망토 연동 지점

현재 Target 유효성 검사에는:

```text
IsTrackableByDrone()
```

구조를 두었다.

현재는 유효 플레이어를 추적 가능한 것으로 처리하며, 이후 투명 망토 구현 시 해당 함수에서 은신 상태를 제외할 수 있도록 준비했다.

### 12. 접촉형 1회 공격

드론 공격 거리:

```text
1m
```

드론이 Target과 직접 추적 가능한 상태에서 1m 안으로 접근하면 공격한다.

공격 후에는 성공 여부와 관계없이 드론을 Despawn한다.

따라서 Jelly Shield 같은 기존 보호 효과가 외력을 차단해도 드론의 공격 기회는 1회 소비된다.

### 13. 7m/s 외부 속도

공격 외력:

```text
7m/s
```

드론에서 Target으로 향하는 진행 방향의 Y축을 제거하여 수평 방향으로 적용한다.

공격에는 기존:

```text
TryApplyExternalVelocityChange(
    ProjectJExternalForceSource.Item,
    velocity
)
```

를 사용한다.

따라서 기존 공통 외력 보호 규칙을 그대로 거친다.

예:

- Jelly Shield
- Respawn Protection
- 되감기 외력 보호

### 14. 최대 12초 수명

드론은 생성 후 최대:

```text
12초
```

동안 유지된다.

목표를 재탐색하더라도 수명 Timer는 초기화하지 않는다.

```text
12초 경과
→ Drone Despawn
```

### 15. Owner 상태 종료 처리

드론 Owner가 Gameplay 상태에서 벗어나거나 NetworkObject가 유효하지 않게 되면 드론도 제거된다.

따라서 완주·경기 종료 등의 상태에서 드론이 계속 남는 상황을 방지한다.

---

## 정책 값

`ProjectJDronePolicy`에 주요 값을 분리했다.

| 항목 | 값 |
| --- | ---: |
| Network Item ID | 27 |
| 최대 수명 | 12초 |
| 이동 속도 | 9m/s |
| 공격 외부 속도 | 7m/s |
| 공격 거리 | 1m |
| 재탐색 | 1회 |
| 충돌 스윕 반경 | 0.4m |
| Route Node 도착 거리 | 0.4m |
| Route Node 탐색 거리 | 12m |
| Target 추적 높이 Offset | 0.9m |

---

## 기본 동작 흐름

```text
드론 사용
→ 사용자의 RaceRank 확인

사용자가 1위
→ 사용 실패
→ 아이템 유지

사용자가 1위가 아님
→ RaceRank == 1인 Target 탐색
→ Network Drone Spawn
→ 12초 Timer 시작

열린 공간
→ 9m/s 직접 추적

장애물이 있고 Route Node가 존재
→ BFS 우회 추적

Target 상실
→ 다른 유효 상위 Target 재탐색 1회

재탐색 성공
→ 새 Target 계속 추적

재탐색 실패
→ Drone Despawn

Target과 1m 이내
→ 수평 방향 7m/s 외력 적용 시도
→ Drone Despawn

12초 경과
→ Drone Despawn
```

---

## 테스트

`ProjectJDronePolicyTests.cs`를 추가했다.

작성된 정책 테스트 사례는 총 47개다.

주요 검증 범위:

- 최대 수명 12초
- 이동 속도 9m/s
- 공격 외부 속도 7m/s
- 공격 거리 1m
- 재탐색 1회
- 1위 사용자 사용 실패
- 유효 Target 조건
- 초기 Target은 RaceRank 1
- 재탐색 후보 Rank 우선순위
- 동일 Rank의 PlayerRef Index 우선순위
- Tick별 이동 거리
- 공격 거리 경계
- 공격 수평 방향 계산
- 12초 수명 경계
- Route Node 탐색 거리
- Route Node 도착 거리

---

## 변경 파일

127일차 커밋 대비 128일차 커밋은 정확히 1개 커밋 앞서 있으며 다음 12개 파일이 변경되었다.

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkDrone.cs
├─ ProjectJNetworkDrone.cs.meta
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.Drone.cs
├─ ProjectJNetworkItemInventory.Drone.cs.meta
└─ ProjectJNetworkItemInventory.cs

Assets/ProjectJ/Network/Fusion/Player/Resources/
├─ ProjectJNetworkDrone.prefab
└─ ProjectJNetworkDrone.prefab.meta

Assets/ProjectJ/Runtime/Items/
├─ ProjectJDronePolicy.cs
└─ ProjectJDronePolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJDronePolicyTests.cs
└─ ProjectJDronePolicyTests.cs.meta
```

128일차 적용용 1회성 Editor Installer는 최종 커밋에 남지 않고, Installer가 적용한 Catalog와 Inventory 변경 결과만 저장소에 반영되어 있다.

---

## Pickup 배치

이번에도 `Day49_AllSystemsTest` Scene에 드론 Pickup을 개별 추가하지 않는다.

남은 신규 아이템 구현이 완료된 뒤 Fusion Scene NetworkObject Bake/SortKey 문제를 피하기 위해 Pickup을 일괄 배치한다.

---

## Route Node 현재 상태

`ProjectJNetworkDrone`은 기존 `ProjectJHomingMissileRouteNode`를 사용할 수 있도록 구현되어 있다.

하지만 현재 작업 기준으로 Game Scene에는 Route Node를 별도로 추가하지 않았다.

따라서:

```text
열린 공간 직접 추적
→ 현재 확인 대상

장애물 우회 추적
→ Route Node Scene 배치 이후 확인 대상
```

으로 구분한다.

---

## 최신 커밋 확인

브랜치:

```text
main
```

최신 SHA:

```text
2f1b41425e48f815f39b6d35cc7278aae5112e8b
```

현재 커밋 메시지:

```text
a
```

부모 커밋:

```text
00dfdcab21c61f26436791989683a46ef5711bc4
127일차 : 가시 갑옷 서버 권한 근접 접촉 반격 및 대상별 재발동 제한 구현
```

GitHub 비교 결과:

```text
ahead_by = 1
behind_by = 0
total_commits = 1
```

최신 커밋에서 정적으로 확인한 항목:

- `Drone = 27`
- `drone` Key
- `드론` 표시 이름
- 기존 `Item_Drone.asset` 재사용
- 서버 `RaceRank` 기반 초기 1위 Target 선정
- 1위 사용자 사용 실패
- Network Drone Spawn
- NetworkTransform 동기화 Prefab
- FusionPrefab 라벨
- 9m/s 이동 정책
- 12초 수명
- 1m 공격 거리
- 7m/s Item 외력
- 공격 후 Despawn
- 목표 재탐색 1회
- 유도탄 Route Node 구조 재사용
- 향후 투명 망토 추적 제외 연결 지점
- EditMode 정책 테스트 파일 포함
- 정책 테스트 사례 47개 작성

---

## 검증 상태

GitHub Combined Status:

```text
등록된 Status 없음
```

해당 커밋의 GitHub Actions Workflow Run:

```text
없음
```

따라서 최신 저장소의 코드 및 파일 연결은 정적으로 확인했지만, Unity Editor 실제 컴파일 성공 및 Unity Test Runner 전체 통과 여부를 GitHub에서 증명할 수 없다.

정책 테스트 47개는 작성되어 있으나 이 개발일지에서는 실행 통과를 주장하지 않는다.

---

## Unity 최종 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. EditMode `ProjectJDronePolicyTests` 실행
3. 가능하면 EditMode 전체 테스트 실행
4. 2인 이상 경기에서 현재 1위가 아닌 Player가 드론 사용
5. 서버 `RaceRank == 1` Player를 Target으로 선택하는지 확인
6. 사용자가 1위이면 사용 실패 및 아이템 유지 확인
7. Drone이 약 9m/s로 이동하는지 확인
8. Target 순위가 바뀌어도 기존 Target을 계속 추적하는지 확인
9. Target 상실 시 재탐색이 1회만 발생하는지 확인
10. 1m 접근 시 7m/s 외력이 적용되는지 확인
11. Jelly Shield가 공격 외력을 차단하는지 확인
12. 보호막으로 차단되어도 Drone이 공격 후 사라지는지 확인
13. 12초가 지나면 Drone이 제거되는지 확인
14. Host와 Client에서 Drone 위치가 동기화되는지 확인
15. Route Node 배치 전 열린 공간 직접 추적 확인
16. Route Node 배치 이후 장애물 우회 기능 별도 확인
17. Pickup 배치는 신규 아이템 구현 페이즈 종료 후 통합
