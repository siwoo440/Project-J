# 123일차 : 카트 서버 권한 경로 추적 및 탑승 자동 이동 구현

## 개발 목표

- 카트를 네트워크 아이템으로 등록한다.
- 카트 사용 시 주변 Route Node를 서버에서 탐색한다.
- 유효한 시작 Route Node가 있을 때만 아이템 사용을 성공시킨다.
- 카트를 NetworkObject로 생성하고 사용자를 즉시 탑승시킨다.
- 카트가 서버 권한으로 Route Node를 따라 자동 이동하도록 한다.
- 이동 속도를 10m/s로 유지한다.
- 한 번의 사용에서 최대 3개의 Route Node까지만 추적한다.
- 탑승 중 Player의 일반 이동, Sprint, Crouch를 잠근다.
- Jump 입력으로 즉시 카트에서 하차할 수 있게 한다.
- 점프 하차 시 기존 Player 점프 속도를 적용한다.
- 카트가 다른 Player와 접촉하면 좌우 방향으로 6m/s 외력을 적용한다.
- 같은 대상에게 연속 충돌이 반복되지 않도록 0.5초 재적중 제한을 적용한다.
- 최대 8초, Route 종료, Jump 하차, Respawn, Gameplay 종료 시 카트를 정리한다.

## 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 카트를 등록했다.

```text
Network ID: Cart = 22
Key: cart
Display Name: 카트
```

기존 `GiantBalloon = 21` 다음 번호를 사용한다.

## 카트 정책

`ProjectJCartPolicy.cs`를 추가했다.

핵심 값:

```text
LifetimeSeconds = 8초
MovementSpeed = 10m/s
MaximumRouteNodes = 3
StartNodeSearchRadius = 4m
NodeArrivalDistance = 0.4m
RiderVerticalOffset = 0.65m
ContactRadius = 1.15m
SidePushSpeed = 6m/s
RehitCooldownSeconds = 0.5초
```

정책 클래스에서 다음을 담당한다.

- 카트 사용 가능 여부
- Tick별 이동 거리 계산
- Route Node 도착 판정
- 다음 노드 진행 가능 여부
- 탑승 종료 조건
- 같은 대상 재적중 가능 여부
- 카트 기준 좌우 밀치기 방향
- 시작 Route Node 검색 반경 판정

## Route Node 시스템

`ProjectJCartRouteNode.cs`를 추가했다.

각 Route Node는 다음 Node 하나를 참조한다.

```text
CartRoute_A
↓
CartRoute_B
↓
CartRoute_C
↓
None
```

카트 사용 시 현재 Player 위치에서 가장 가까운 `ProjectJCartRouteNode`를 검색한다.

시작 노드는 Player 기준 4m 이내에 존재해야 한다.

유효한 시작 Node가 없으면 카트 사용은 실패하고 아이템을 소비하지 않는다.

## Game Scene Route 배치

`Assets/ProjectJ/Scenes/Game.unity`에 실제 카트 Route Node를 배치했다.

배치된 Node:

```text
CartRoute_A
CartRoute_B
CartRoute_C
```

연결:

```text
CartRoute_A.NextNode = CartRoute_B
CartRoute_B.NextNode = CartRoute_C
CartRoute_C.NextNode = None
```

따라서 카트 기능이 실제 Game Scene에서 시작 Route를 탐색할 수 있는 최소 경로가 준비되어 있다.

## Network Cart

`ProjectJNetworkCart.cs`를 추가했다.

필수 컴포넌트:

```text
NetworkObject
NetworkTransform
ProjectJNetworkCart
```

Networked 상태:

```text
NetworkInitialized
NetworkOwner
NetworkLifetimeTimer
NetworkVisitedNodeCount
NetworkLastPushTargetIndex
NetworkPushSuccessCount
```

카트의 경로 이동, Owner 판정, 종료 조건, 접촉 밀치기는 State Authority에서 처리한다.

## 카트 사용

`ProjectJNetworkItemInventory.Cart.cs`를 추가했다.

카트 사용 시 다음을 확인한다.

```text
Runner 준비
Server / State Authority
Gameplay 허용
현재 카트에 탑승 중이 아님
4m 이내 시작 Route Node 존재
현재 Owner의 기존 카트 없음
```

조건을 만족하면:

```text
ProjectJNetworkCart Spawn
→ Owner 지정
→ 시작 Route Node 지정
→ NetworkCartRiding = true
```

순서로 처리한다.

## 사용자당 카트 1대 제한

같은 Owner가 이미 활성 카트를 가지고 있으면 새 카트 사용에 실패한다.

```text
기존 Owner 카트 없음
→ 사용 가능

기존 Owner 카트 있음
→ 사용 실패
→ 아이템 유지
```

## 서버 권한 자동 이동

카트는 현재 위치에서 Target Route Node 방향으로 이동한다.

```text
이동 속도 = 10m/s
```

매 Network Tick에서:

```text
이동 거리 = 10m/s × Runner.DeltaTime
```

을 계산해 `Vector3.MoveTowards`로 이동한다.

Node까지 0.4m 이하로 접근하면 해당 Node에 도착한 것으로 판정한다.

## 최대 3개 Route Node

한 번의 카트 사용에서 방문 가능한 Route Node는 최대 3개다.

예:

```text
A
→ B
→ C
→ 종료
```

C에 다음 Node가 연결되어 있더라도 `MaximumRouteNodes = 3`에 도달하면 종료한다.

## Rider 자동 운반

카트가 이동하면 Owner Player 위치를 카트 위치 기준으로 직접 운반한다.

```text
Rider Position
=
Cart Position
+
Vector3.up × 0.65m
```

Player의 시점 회전은 카트 회전에 강제로 고정하지 않는다.

## 탑승 중 일반 이동 잠금

`ProjectJNetworkPlayer.cs`에 카트 탑승 상태 검사를 추가했다.

탑승 중에는:

```text
Move Input = 0
Sprint = false
Crouch = false
VerticalVelocity = 0
```

으로 처리하고 Player 자체 이동 시뮬레이션을 종료한다.

카트가 서버에서 Rider 위치를 직접 운반하므로 Player 이동 계산과 카트 이동이 서로 충돌하지 않는다.

## Jump 중도 하차

탑승 중에도 `LastReceivedJump` 입력 상태는 카트가 확인한다.

Jump 입력을 감지하면:

```text
NetworkCartRiding = false
→ 카트에서 하차
→ Player 위치를 카트 위쪽으로 보정
→ 기존 Player JumpSpeed 적용
→ 카트 Despawn
```

처리를 한다.

별도의 카트 전용 점프 수치를 만들지 않고 Player의 기존 `JumpSpeed`를 재사용한다.

## 다른 Player 접촉 밀치기

카트는 이동 중 주변 Player를 `OverlapSphereNonAlloc`로 검사한다.

Owner 자신은 대상에서 제외한다.

다른 Player가 접촉하면 카트의 `transform.right`와 Target 위치를 사용해 좌우 방향을 결정한다.

```text
오른쪽 Target
→ +Cart Right × 6m/s

왼쪽 Target
→ -Cart Right × 6m/s
```

외력은 기존:

```text
TryApplyExternalVelocityChange(
    ProjectJExternalForceSource.Item,
    ...
)
```

흐름을 재사용한다.

따라서 기존 외력 시스템의 Jelly Shield와 Respawn Protection 판정도 그대로 적용된다.

## 재적중 제한

같은 Player가 카트 접촉 범위에 여러 Tick 머무르더라도 매 Tick 외력이 추가되지 않도록 대상별 마지막 적중 시간을 저장한다.

```text
첫 적중
→ 6m/s 외력

0.5초 미만 재접촉
→ 무시

0.5초 이후 재접촉
→ 다시 적용 가능
```

## 종료 조건

다음 중 하나가 발생하면 카트를 종료한다.

```text
8초 Lifetime 종료
Gameplay 비활성
Owner 소실
Route 종료
최대 3개 Node 방문
Jump 중도 하차
Inventory Clear
Respawn
```

종료 시 `NetworkCartRiding`을 false로 복원하고 Network Cart를 Despawn한다.

## 프로토타입 외형

최종 카트 에셋 대신 런타임 Primitive를 생성한다.

구성:

```text
Cart Body
→ Cube

Cart Wheel × 4
→ Cylinder
```

프로토타입 Collider는 Gameplay 충돌에 사용하지 않고 제거한다.

실제 카트 외형과 애니메이션은 이후 아트 단계에서 교체할 수 있다.

## Network Prefab

추가된 Resource Prefab:

```text
Assets/ProjectJ/Network/Fusion/Player/Resources/
└─ ProjectJNetworkCart.prefab
```

Prefab 구성:

```text
NetworkObject
NetworkTransform
ProjectJNetworkCart
```

Prefab `.meta`에는 `FusionPrefab` 라벨이 적용되어 있다.

## Inventory 연결

`ProjectJNetworkItemInventory.cs`에 카트 상태 초기화와 사용 분기를 연결했다.

Spawn:

```text
InitializeCartAuthority()
```

Clear:

```text
ClearCartAuthority()
```

Respawn:

```text
ClearCartAuthority()
```

아이템 사용:

```text
case ProjectJNetworkItemId.Cart
→ UseCartAuthority()
```

## 테스트 추가

`ProjectJCartPolicyTests.cs`를 추가했다.

총 47개 테스트 사례가 작성되어 있다.

검증 항목:

- 최대 수명 8초
- 이동 속도 10m/s
- 최대 Route Node 3개
- 옆 밀치기 속도 6m/s
- 재적중 제한 0.5초
- 카트 사용 조건
- Gameplay Lock 사용 차단
- 이미 탑승 중 사용 차단
- 시작 Route Node 누락 사용 차단
- 기존 Owner 카트 존재 시 사용 차단
- DeltaTime별 이동 거리
- Node 도착 거리 0.4m 경계
- 최대 Node 진행 제한
- Route 종료 조건
- Lifetime 종료 조건
- Owner 소실 종료
- Gameplay 종료 조건
- 재적중 시간 경계
- 좌우 Push 방향
- Push 방향 수직 성분 제거
- 비정상 Right 방향 보정
- 시작 Node 검색 반경 4m 경계

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkCart.cs
├─ ProjectJNetworkCart.cs.meta
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.Cart.cs
├─ ProjectJNetworkItemInventory.Cart.cs.meta
├─ ProjectJNetworkItemInventory.cs
└─ ProjectJNetworkPlayer.cs

Assets/ProjectJ/Network/Fusion/Player/Resources/
├─ ProjectJNetworkCart.prefab
└─ ProjectJNetworkCart.prefab.meta

Assets/ProjectJ/Runtime/Items/
├─ ProjectJCartPolicy.cs
├─ ProjectJCartPolicy.cs.meta
├─ ProjectJCartRouteNode.cs
└─ ProjectJCartRouteNode.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJCartPolicyTests.cs
└─ ProjectJCartPolicyTests.cs.meta

Assets/ProjectJ/Scenes/
└─ Game.unity
```

삭제된 게임 런타임 파일은 없다.

123일차 적용에 사용했던 1회용 Player 자동 패처는 최종 최신 커밋에 남아 있지 않고, 적용 결과인 `ProjectJNetworkPlayer.cs` 변경만 반영되어 있다.

## 최신 커밋 검증

확인한 최신 `main` 커밋:

```text
cd8451cee50226ce20d6d1f56f2873a978ac6795
```

현재 커밋 메시지:

```text
a
```

정적 확인 내용:

- `Cart = 22` 등록 확인
- `cart` Key 매핑 확인
- `카트` 표시 이름 확인
- 카트 Inventory 사용 분기 확인
- `NetworkCartRiding` 상태 확인
- 사용자당 기존 카트 검사 확인
- 시작 Route Node 4m 검색 확인
- Network Cart Spawn 확인
- 카트 8초 Lifetime 확인
- 이동 속도 10m/s 정책 확인
- 최대 Route Node 3개 정책 확인
- Node 도착 거리 0.4m 확인
- Jump 중도 하차 흐름 확인
- 기존 Player JumpSpeed 재사용 확인
- Player 탑승 중 자체 이동 잠금 확인
- Rider 카트 위치 운반 확인
- 접촉 반경 1.15m 확인
- 좌우 외력 6m/s 확인
- 같은 대상 재적중 제한 0.5초 확인
- 기존 Jelly Shield / Respawn Protection 외력 경로 재사용 확인
- Inventory Clear 시 카트 정리 확인
- Respawn 시 카트 정리 확인
- `ProjectJNetworkCart.prefab` 존재 확인
- Prefab `FusionPrefab` 라벨 확인
- `ProjectJCartRouteNode` 스크립트 존재 확인
- `Game.unity`에 `CartRoute_A`, `CartRoute_B`, `CartRoute_C` 배치 확인
- `CartRoute_A → CartRoute_B` 연결 확인
- `CartRoute_B → CartRoute_C` 연결 확인
- `CartRoute_C → None` 종료 확인
- `ProjectJCartPolicyTests` 47개 테스트 사례 작성 확인

GitHub Commit Status와 GitHub Actions workflow run은 등록되어 있지 않았다.

따라서 Unity Editor 실제 컴파일 성공과 EditMode Test Runner 전체 통과 여부는 GitHub 저장소만으로 확정하지 않았다.

## 테스트맵 Pickup 배치 보류

이번 일차에도 `Day49_AllSystemsTest`에 카트 Pickup을 개별 배치하지 않는다.

Fusion Scene NetworkObject SortKey/Bake 문제를 줄이기 위해 신규 아이템 Pickup 배치는 현재 아이템 구현 페이즈 종료 후 한 번에 통합한다.

카트의 Route Node는 기능 동작에 필수이므로 `Game.unity`에 별도로 배치했으며, Pickup 배치 보류와는 구분한다.

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJCartPolicyTests` 전체 통과 확인
3. `Game` Scene의 `CartRoute_A`, `B`, `C` 존재 확인
4. A의 Next Node가 B인지 확인
5. B의 Next Node가 C인지 확인
6. C의 Next Node가 None인지 확인
7. 시작 위치가 A에서 4m 이내인지 확인
8. 카트 사용 시 아이템이 정상 소비되는지 확인
9. 시작 Route Node가 없으면 아이템이 소비되지 않는지 확인
10. 사용 즉시 Network Cart가 Spawn되는지 확인
11. Owner가 카트에 즉시 탑승하는지 확인
12. 카트가 약 10m/s로 이동하는지 확인
13. A → B → C 순서로 이동하는지 확인
14. 최대 3개 Node에서 종료되는지 확인
15. 탑승 중 WASD 이동이 Player 자체 이동에 영향을 주지 않는지 확인
16. 탑승 중 Sprint가 적용되지 않는지 확인
17. 탑승 중 Crouch가 적용되지 않는지 확인
18. Space 입력 시 즉시 하차하는지 확인
19. Jump 하차 시 기존 점프 속도로 위쪽 이동하는지 확인
20. 카트가 다른 Player와 접촉할 때 좌우 6m/s 외력이 적용되는지 확인
21. Owner 자신에게 접촉 외력이 적용되지 않는지 확인
22. 같은 Target에게 0.5초 이내 연속 외력이 반복되지 않는지 확인
23. Jelly Shield가 카트 접촉 외력을 차단하는지 확인
24. Respawn Protection이 카트 접촉 외력을 차단하는지 확인
25. 8초 경과 시 카트가 제거되는지 확인
26. Route 끝에서 카트가 제거되는지 확인
27. Respawn 시 카트와 탑승 상태가 정리되는지 확인
28. Gameplay 종료 시 카트와 탑승 상태가 정리되는지 확인
29. Host와 Client에서 카트 위치가 일관되게 보이는지 확인
30. Pickup 배치는 아이템 구현 페이즈 종료 후 통합
