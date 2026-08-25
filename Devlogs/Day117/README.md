# 117일차 : 낚시대 서버 권한 직선 조준 및 단일 대상 지속 끌어당기기

## 개발 목표

- 낚시대를 네트워크 아이템으로 등록한다.
- 서버 권한으로 최대 14m 직선 조준을 수행한다.
- 가장 먼저 맞은 유효 Player 1명만 연결한다.
- 대상과 연결된 동안 0.6초 동안 사용자 방향 8m/s 당김 속도를 적용한다.
- 사용자와 대상 사이에 벽이 생기거나 거리가 14m를 넘으면 연결을 종료한다.
- Jelly Shield, Respawn 보호, 완주 상태 등 기존 적대 아이템 보호 규칙을 재사용한다.
- 기존 외부 속도 누적 방식 대신 낚시대 전용 속도 설정 경로를 사용한다.

## 구현 내용

### 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 낚시대를 등록했다.

```text
Network ID: FishingRod = 16
Key: fishing_rod
Display Name: 낚시대
```

기존 `InkOctopus = 15` 다음 번호를 사용한다.

기존 ItemDefinition은 다음 값을 유지한다.

```text
itemId: fishing_rod
displayName: 낚시대
duration: 0.6
```

### 서버 권한 직선 조준

`ProjectJNetworkItemInventory.FishingRod.cs`를 추가했다.

낚시대 사용 시 서버에서 Player 전방으로 최대 14m Raycast를 수행한다.

처리 순서:

```text
낚시대 사용
→ 서버 Raycast
→ 가장 가까운 충돌 확인
→ 벽이 먼저 맞으면 연결 실패
→ Player가 먼저 맞으면 유효성 검사
→ 유효한 Player 1명 연결
```

사용자 자신의 Collider는 조준 대상에서 제외한다.

### 단일 대상 연결 상태

낚시대는 다음 Networked 상태를 사용한다.

```text
NetworkFishingRodTimer
NetworkFishingRodTargetIndex
```

연결 성공 시 Target의 Player Index를 저장하고 0.6초 TickTimer를 시작한다.

한 번의 사용으로 한 명의 Player만 연결한다.

### 0.6초 지속 당김

연결 중 매 Network Tick마다 다음을 갱신한다.

```text
Target 위치
→ 낚시대 사용자 위치 계산
→ 수평 사용자 방향 계산
→ 8m/s 당김 속도 설정
```

사용자가 이동하면 Target을 당기는 방향도 매 Tick 다시 계산된다.

당김 속도는 다음 값으로 유지한다.

```text
8m/s
```

### 외력 누적 방지

기존 `TryApplyExternalVelocityChange()`는 외부 속도를 기존 값에 더하는 구조이므로 낚시대에 매 Tick 사용할 경우 힘이 과도하게 누적될 수 있다.

이를 피하기 위해 `ProjectJNetworkExternalGameplay.FishingRod.cs`를 추가하고 낚시대 전용 경로를 만들었다.

```text
TrySetFishingRodPullVelocityAuthority()
```

이 경로는 연결 중 낚시대 외부 속도를 매 Tick 8m/s로 설정한다.

따라서 다음과 같은 누적을 방지한다.

```text
8
→ 16
→ 24
→ 32m/s
```

대신 항상 사용자 방향 8m/s를 기준으로 갱신한다.

### 보호 상태 재사용

낚시대 Target은 기존 아이템 보호 상태를 검사한다.

차단 조건:

- 자기 자신
- Gameplay 비활성
- 완주 Player
- Respawn 보호
- Jelly Shield

Jelly Shield는 기존 `BlocksExternalForce(ProjectJExternalForceSource.Item)` 판정을 그대로 재사용한다.

### 연결 중 거리 검사

처음 연결할 때만 사거리를 검사하지 않고 연결 중에도 사용자와 대상 사이 거리를 매 Tick 계산한다.

```text
거리 <= 14m
→ 연결 유지

거리 > 14m
→ 연결 즉시 해제
```

### 연결 중 벽 차폐 검사

사용자와 Target 사이에 벽이 생겼는지도 연결 중 계속 검사한다.

```text
사용자 ↔ Target
```

사이에 일반 지형 Collider가 들어오면 연결을 해제한다.

다른 Player Collider는 벽으로 취급하지 않는다.

### 연결 종료 조건

다음 조건 중 하나를 만족하면 낚시대 연결을 정리한다.

- 0.6초 Timer 종료
- Gameplay 비활성
- Target 제거
- Target 보호 상태 진입
- Target 완주
- Respawn 보호
- Jelly Shield
- 사용자와 Target 거리 14m 초과
- 사용자와 Target 사이 벽 차폐
- 인벤토리 Clear
- 사용자 Respawn

### 메인 인벤토리 연결

`ProjectJNetworkItemInventory.cs`에 다음 연결을 추가했다.

```text
Spawned()
→ InitializeFishingRodAuthority()

FixedUpdateNetwork()
→ UpdateFishingRodAuthority()

ClearAuthority()
→ ClearFishingRodAuthority()

HandleRespawnAuthority()
→ ClearFishingRodAuthority()

TryUseSelectedItemAuthority()
→ UseFishingRodAuthority()
```

### 개발용 연결 시각화

연결 중에는 `Debug.DrawLine()`으로 사용자와 Target 사이를 표시한다.

최종 낚싯줄 그래픽이나 Mesh는 이번 일차 구현 범위에 포함하지 않는다.

## 정책 분리

`ProjectJFishingRodPolicy.cs`를 추가했다.

주요 수치:

```text
PullDurationSeconds = 0.6초
MaximumRangeMeters = 14m
PullSpeedMetersPerSecond = 8m/s
```

정책 클래스에서 다음을 담당한다.

- 14m 사거리 포함 여부
- Target 적용 가능 여부
- 사용자 방향 수평 당김 속도 계산
- 연결 유지 조건

## 테스트 추가

`ProjectJFishingRodPolicyTests`를 추가했다.

총 24개 테스트 사례를 구성했다.

검증 항목:

- 지속 시간 0.6초
- 최대 사거리 14m
- 당김 속도 8m/s
- 14m 경계 포함
- 14m 초과 차단
- 음수 거리 차단
- 정상 Target 적용 허용
- Runner 미준비 차단
- Gameplay 비활성 차단
- 자기 자신 차단
- 완주 Player 차단
- Respawn 보호 차단
- Jelly Shield 차단
- 좌우 방향 당김 계산
- 수직 높이 차이 무시
- 동일 수평 위치에서 0 속도 반환
- Timer 활성 연결 유지
- Timer 종료 연결 해제
- Gameplay 종료 연결 해제
- 거리 초과 연결 해제
- 벽 차폐 연결 해제

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkExternalGameplay.FishingRod.cs
├─ ProjectJNetworkExternalGameplay.FishingRod.cs.meta
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.FishingRod.cs
├─ ProjectJNetworkItemInventory.FishingRod.cs.meta
└─ ProjectJNetworkItemInventory.cs

Assets/ProjectJ/Runtime/Items/
├─ ProjectJFishingRodPolicy.cs
└─ ProjectJFishingRodPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJFishingRodPolicyTests.cs
└─ ProjectJFishingRodPolicyTests.cs.meta
```

기존 `Item_FishingRod.asset`은 이미 `fishing_rod / 낚시대 / duration 0.6`으로 준비되어 있어 수정하지 않았다.

## 최신 커밋 검증

확인한 최신 `main` 커밋:

```text
807ed0400b43737f1a8407c3b9cc99bbe90e34ff
```

현재 커밋 메시지는 임시 제목 `a`다.

정적 확인 내용:

- `FishingRod = 16` 등록 확인
- `fishing_rod` Key 매핑 확인
- `낚시대` 표시 이름 확인
- `InitializeFishingRodAuthority()` 연결 확인
- `UpdateFishingRodAuthority()` 연결 확인
- `ClearAuthority()` 낚시대 연결 해제 확인
- `HandleRespawnAuthority()` 낚시대 연결 해제 확인
- 아이템 사용 switch 연결 확인
- 서버 권한 최대 14m Raycast 확인
- 가장 가까운 충돌 기준 단일 Target 선택 확인
- 벽 우선 충돌 시 연결 차단 확인
- 0.6초 Networked 연결 Timer 확인
- 사용자 방향 8m/s 수평 당김 계산 확인
- 낚시대 전용 외부 속도 설정 경로 확인
- Jelly Shield 차단 확인
- Respawn 보호 차단 확인
- 완주 Player 차단 확인
- 연결 중 14m 거리 검사 확인
- 연결 중 벽 차폐 검사 확인
- `ProjectJFishingRodPolicyTests` 24개 테스트 사례 작성 확인
- 기존 `Item_FishingRod.asset` duration 0.6 확인

GitHub에 등록된 CI Status가 없으므로 Unity Editor 실제 컴파일과 EditMode Test Runner 통과 여부는 GitHub만으로 확정하지 않았다.

## 테스트맵 Pickup 배치 보류

이번 일차에도 `Day49_AllSystemsTest`에 낚시대 Pickup을 개별 배치하지 않는다.

Fusion Scene NetworkObject SortKey/Bake 문제를 줄이기 위해 신규 아이템 Pickup 배치는 현재 아이템 구현 페이즈 종료 후 한 번에 통합한다.

따라서 Pickup 미배치는 미완료가 아니라 계획된 단계 보류다.

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJFishingRodPolicyTests` 전체 통과 확인
3. 낚시대 사용 시 아이템 정상 소비 확인
4. 최대 14m 전방 Player 조준 확인
5. 벽이 Player보다 먼저 있을 때 연결되지 않는지 확인
6. 가장 먼저 맞은 Player 한 명만 연결되는지 확인
7. 연결이 0.6초 유지되는지 확인
8. Target이 사용자 방향으로 8m/s 당겨지는지 확인
9. 사용자가 이동하면 당김 방향이 갱신되는지 확인
10. 당김 속도가 Tick마다 누적되지 않는지 확인
11. 14m 경계에서 연결 가능한지 확인
12. 연결 중 거리가 14m를 넘으면 즉시 해제되는지 확인
13. 연결 중 벽이 생기면 즉시 해제되는지 확인
14. Jelly Shield Player에게 연결되지 않는지 확인
15. Respawn 보호 Player에게 연결되지 않는지 확인
16. 완주 Player에게 연결되지 않는지 확인
17. 사용자 Respawn 시 연결이 제거되는지 확인
18. 경기 종료 시 연결 상태가 제거되는지 확인
19. Host와 Client에서 Target과 당김 결과가 일관되게 보이는지 확인
20. Pickup 배치는 아이템 구현 페이즈 종료 후 통합
