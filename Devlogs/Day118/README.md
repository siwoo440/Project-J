# 118일차 : 갈고리 서버 권한 구조물 부착 및 3D 자기 이동 구현

## 개발 목표

- 갈고리를 네트워크 아이템으로 등록한다.
- 서버 권한으로 최대 20m 전방 구조물을 조준한다.
- `GrappleSurface` 태그가 지정된 구조물에만 부착한다.
- 부착 지점을 Networked Anchor로 저장한다.
- 최대 1.5초 동안 Anchor 방향으로 12m/s 3D 자기 이동을 수행한다.
- Anchor 도착, 이동 경로 충돌, 경기 종료, Respawn 시 연결을 해제한다.
- 유효한 갈고리 표면을 잡지 못한 경우 아이템을 소비하지 않는다.

## 구현 내용

### 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 갈고리를 추가했다.

```text
Network ID: GrapplingHook = 17
Key: grappling_hook
Display Name: 갈고리
```

기존 `FishingRod = 16` 다음 번호를 사용한다.

기존 ItemDefinition은 다음 값을 유지한다.

```text
itemId: grappling_hook
displayName: 갈고리
duration: 1.5
```

## 갈고리 정책

`ProjectJGrapplingHookPolicy.cs`를 추가했다.

핵심 수치:

```text
GrappleSurfaceTag = GrappleSurface
DurationSeconds = 1.5초
MaximumRangeMeters = 20m
PullSpeedMetersPerSecond = 12m/s
ArrivalDistanceMeters = 0.75m
SweepRadiusMeters = 0.35m
```

## 구조물 조준

갈고리 사용 시 서버가 Player 전방 최대 20m까지 Raycast를 수행한다.

```text
갈고리 사용
→ 서버 Raycast
→ 가장 가까운 Collider 확인
→ GrappleSurface 태그 확인
→ 유효하면 충돌 지점을 Anchor로 저장
```

사용자 자신의 Collider는 제외한다.

Raycast에서 가장 먼저 맞은 Collider가 `GrappleSurface`가 아니면 부착하지 않는다.

## GrappleSurface 태그

갈고리는 모든 벽에 자동 부착하지 않고 `GrappleSurface` 태그가 지정된 구조물에만 연결한다.

Unity에서 다음 태그가 필요하다.

```text
GrappleSurface
```

갈고리를 사용할 수 있는 벽이나 구조물의 Collider GameObject 또는 부모 GameObject에 해당 태그를 지정한다.

## Networked 연결 상태

갈고리 연결 상태는 다음 Networked 값을 사용한다.

```text
NetworkGrapplingHookActive
NetworkGrapplingHookTimer
NetworkGrapplingHookAnchor
```

부착 성공 시:

```text
Active = true
Timer = 1.5초
Anchor = Raycast 충돌 지점
```

으로 설정한다.

## 3D 자기 이동

갈고리는 상대 Player를 끌어오는 낚시대와 달리 사용자 자신을 Anchor 방향으로 이동시킨다.

매 Network Tick:

```text
Anchor - Player 위치
→ 3차원 방향 정규화
→ 12m/s 이동 속도 설정
```

을 수행한다.

Y축을 제거하지 않으므로 위쪽 벽이나 높은 구조물로 이동할 수 있다.

예시:

```text
Anchor
   ●
  /
 / 12m/s
/
● Player
```

## 속도 누적 방지

갈고리는 매 Tick 12m/s를 외부 속도에 계속 더하지 않는다.

`ProjectJNetworkExternalGameplay.GrapplingHook.cs`의 갈고리 전용 이동 경로에서:

```text
NetworkExternalVelocity = desiredVelocity
```

방식으로 현재 갈고리 이동 속도를 설정한다.

따라서 Tick마다 12 → 24 → 36m/s처럼 속도가 누적되는 것을 방지한다.

## 이동 중 충돌 스윕

Anchor 방향으로 이동하기 전에 다음 Tick 예상 이동 거리만큼 SphereCast를 수행한다.

```text
SphereCast Radius: 0.35m
Sweep Distance: 12m/s × Runner.DeltaTime
```

이동 경로에서 다른 Collider가 감지되면 갈고리를 즉시 해제한다.

이를 통해 벽이나 구조물을 관통하며 Anchor까지 이동하는 상황을 방지한다.

## Anchor 도착 판정

Player와 Anchor 사이 거리가 약 0.75m 이하가 되면 갈고리 연결을 종료한다.

```text
거리 <= 0.75m
→ 도착
→ 연결 종료
```

Anchor를 지나쳐 반대 방향으로 계속 이동하는 현상을 줄이기 위한 프로토타입 도착 거리다.

## 아이템 소비 규칙

이번 구현에서는 실제 유효한 `GrappleSurface`를 잡았을 때만 사용 성공으로 처리한다.

```text
유효 GrappleSurface 부착 성공
→ 아이템 소비

허공
→ 사용 실패
→ 아이템 유지

일반 벽
→ 사용 실패
→ 아이템 유지
```

## 보호 상태

갈고리는 다른 Player에게 적대 효과를 주는 아이템이 아니라 자기 이동 보조 아이템이다.

따라서 Jelly Shield나 Respawn 보호 상태를 Target 보호 판정에 사용하지 않는다.

다만 다음 상황에서는 연결이 유지되지 않는다.

- Gameplay 비활성
- 완주 후
- 경기 종료
- 사용자 Respawn
- 인벤토리 Clear
- Timer 1.5초 종료
- Anchor 도착
- 이동 중 충돌 발생

## 메인 인벤토리 연결

`ProjectJNetworkItemInventory.cs`에 다음 연결을 추가했다.

```text
Spawned()
→ InitializeGrapplingHookAuthority()

FixedUpdateNetwork()
→ UpdateGrapplingHookAuthority()

ClearAuthority()
→ ClearGrapplingHookAuthority()

HandleRespawnAuthority()
→ ClearGrapplingHookAuthority()

TryUseSelectedItemAuthority()
→ UseGrapplingHookAuthority()
```

## 개발용 시각화

연결 중에는 `Debug.DrawLine()`으로 Player와 Anchor 사이를 표시한다.

최종 갈고리 줄 Mesh, Rope, 애니메이션은 이번 일차 구현 범위에 포함하지 않는다.

## 테스트 추가

`ProjectJGrapplingHookPolicyTests`를 추가했다.

총 27개 테스트 사례가 작성되어 있다.

검증 항목:

- 지속 시간 1.5초
- 최대 사거리 20m
- 자기 이동 속도 12m/s
- Anchor 도착 거리 0.75m
- 20m 경계 포함
- 20m 초과 차단
- 음수 거리 차단
- GrappleSurface 여부
- Runner 준비 여부
- Gameplay 활성 여부
- 3D 방향 이동 계산
- Y축 이동 포함
- 동일 위치에서 0 속도 반환
- Timer 종료 시 연결 해제
- Gameplay 종료 시 연결 해제
- Anchor 도착 시 연결 해제

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkExternalGameplay.GrapplingHook.cs
├─ ProjectJNetworkExternalGameplay.GrapplingHook.cs.meta
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.GrapplingHook.cs
├─ ProjectJNetworkItemInventory.GrapplingHook.cs.meta
└─ ProjectJNetworkItemInventory.cs

Assets/ProjectJ/Runtime/Items/
├─ ProjectJGrapplingHookPolicy.cs
└─ ProjectJGrapplingHookPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJGrapplingHookPolicyTests.cs
└─ ProjectJGrapplingHookPolicyTests.cs.meta
```

삭제한 파일은 없다.

## 최신 커밋 검증

확인한 최신 `main` 커밋:

```text
91921061ef53694ad108093277925400618f5386
```

현재 커밋 메시지는 임시 제목 `a`다.

정적 확인 내용:

- `GrapplingHook = 17` 등록 확인
- `grappling_hook` Key 매핑 확인
- `갈고리` 표시 이름 확인
- `InitializeGrapplingHookAuthority()` 연결 확인
- `UpdateGrapplingHookAuthority()` 연결 확인
- `ClearAuthority()` 갈고리 상태 제거 확인
- `HandleRespawnAuthority()` 갈고리 상태 제거 확인
- 아이템 사용 switch 연결 확인
- 서버 권한 최대 20m Raycast 확인
- `GrappleSurface` 태그 검증 확인
- 충돌 지점 Networked Anchor 저장 확인
- 최대 1.5초 Networked Timer 확인
- 12m/s 3D 자기 이동 계산 확인
- 속도 누적 대신 현재 속도 설정 확인
- 이동 중 SphereCast 충돌 검사 확인
- Anchor 0.75m 도착 종료 확인
- 유효 표면 부착 실패 시 아이템 유지 확인
- `ProjectJGrapplingHookPolicyTests` 27개 테스트 사례 작성 확인

GitHub에 등록된 CI Status가 없으므로 Unity Editor 실제 컴파일과 EditMode Test Runner 통과 여부는 GitHub만으로 확정하지 않았다.

## 테스트맵 Pickup 배치 보류

이번 일차에도 `Day49_AllSystemsTest`에 갈고리 Pickup을 개별 배치하지 않는다.

Fusion Scene NetworkObject SortKey/Bake 문제를 줄이기 위해 신규 아이템 Pickup 배치는 현재 아이템 구현 페이즈 종료 후 한 번에 통합한다.

따라서 Pickup 미배치는 미완료가 아니라 계획된 단계 보류다.

## Unity 확인 항목

1. `GrappleSurface` 태그 생성 확인
2. 테스트 구조물 Collider 또는 부모에 `GrappleSurface` 지정
3. Unity Console 컴파일 Error 0건 확인
4. `ProjectJGrapplingHookPolicyTests` 전체 통과 확인
5. 20m 안의 GrappleSurface에 정상 부착되는지 확인
6. 20m 밖에서는 부착되지 않는지 확인
7. 일반 벽에는 부착되지 않는지 확인
8. 부착 실패 시 아이템이 유지되는지 확인
9. Anchor 위치가 실제 충돌 지점과 일치하는지 확인
10. 연결이 최대 1.5초 유지되는지 확인
11. Player가 Anchor 방향으로 12m/s 이동하는지 확인
12. 위쪽 Anchor로 Y축 이동이 발생하는지 확인
13. 이동 속도가 Tick마다 누적되지 않는지 확인
14. Anchor 약 0.75m 도착 시 연결이 종료되는지 확인
15. 이동 경로에 벽이 있으면 SphereCast로 연결이 종료되는지 확인
16. Respawn 시 갈고리 연결이 제거되는지 확인
17. 경기 종료 시 갈고리 연결이 제거되는지 확인
18. Host와 Client에서 Anchor와 이동 결과가 일관되게 보이는지 확인
19. 최종 Rope/Mesh는 추후 아트 단계에서 적용
20. Pickup 배치는 아이템 구현 페이즈 종료 후 통합
