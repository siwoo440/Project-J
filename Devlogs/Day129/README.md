# 129일차 개발일지 - 투명 망토 서버 권한 은신 및 자동 추적 제외·행동 해제

## 작업 개요

129일차에는 `투명 망토(Invisibility Cloak)` 아이템을 네트워크 아이템 시스템에 연결하고, 서버 권한으로 5초 동안 은신 상태를 유지하는 기능을 구현했다.

은신 중에는 다른 플레이어 화면에서 캐릭터 Visual을 숨기고, 2m 이내에서는 짧은 실루엣 깜빡임과 좌우 흔들림을 통해 근거리 탐지가 가능하도록 구성했다.

또한 기존 유도탄과 드론의 자동 Target에서 은신 플레이어를 제외하고, Push 사용이나 다른 아이템의 성공적인 사용 시 은신이 즉시 해제되도록 연결했다.

개발 과정에서 두 차례 검증 문제가 발견되었으며, 최신 커밋에는 컴파일 부트스트랩 문제와 Shimmer 0.35초 경계 문제에 대한 수정이 모두 반영되어 있다.

---

## 구현 내용

### 1. 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 투명 망토를 추가했다.

```text
InvisibilityCloak = 28
```

등록 정보:

- Network Item ID: `28`
- Key: `invisibility_cloak`
- 표시 이름: `투명 망토`

기존 `Drone = 27` 다음 번호를 사용한다.

### 2. 기존 ItemDefinition 재사용

기존 프로젝트에 이미 존재하는:

```text
Assets/ProjectJ/Data/Items/Item_InvisibilityCloak.asset
```

을 그대로 사용했다.

새 ItemDefinition은 추가하지 않았으므로 전체 ItemDefinition 카탈로그 개수에는 변화가 없다.

기존 주요 값:

```text
itemId = invisibility_cloak
displayName = 투명 망토
duration = 5초
```

### 3. 서버 권한 은신 상태

`ProjectJNetworkItemInventory.InvisibilityCloak.cs`를 추가했다.

주요 Network 상태:

```text
NetworkInvisibilityCloakActive
NetworkInvisibilityCloakTimer
NetworkInvisibilityCloakRevision
```

사용 성공 시 서버에서 5초 Timer를 시작하고:

```text
IsInvisibilityCloakActive = true
```

상태를 동기화한다.

이미 은신 중에는 다시 사용할 수 없으며, 사용 실패 시 아이템은 소비하지 않는다.

### 4. 5초 지속

정책값:

```text
DurationSeconds = 5
```

다음 상황에서 은신이 종료된다.

- 5초 Timer 종료
- Push 사용
- 다른 아이템 성공 사용
- Respawn
- Gameplay 비활성
- Inventory Clear

### 5. 자기 화면에서는 정상 표시

투명 망토를 사용한 플레이어 자신의 화면에서는 Player Visual을 숨기지 않는다.

```text
Local Input Authority
→ Visible
```

로 처리한다.

NetworkObject, Collider, NetworkTransform, 순위 데이터는 그대로 유지한다.

### 6. 다른 플레이어 화면에서 은신

Remote Player가 투명 망토를 사용 중이면 관찰자와의 거리를 기준으로 표시 상태를 결정한다.

```text
거리 > 2m
→ Hidden

거리 <= 2m
→ ProximityShimmer
```

따라서 멀리서는 캐릭터가 보이지 않고, 가까이 접근하면 미세한 시각 신호를 통해 위치를 추측할 수 있다.

### 7. 2m 근거리 Shimmer

근거리 탐지 기준:

```text
ProximityRevealDistance = 2m
```

Shimmer 정책:

```text
반복 주기 = 0.3초
잠깐 표시되는 시간 = 0.05초
좌우 흔들림 폭 = 0.035
```

2m 안에서는 Visual이 짧게 깜빡이며 X축으로 소폭 흔들린다.

별도 Shader나 VFX 없이 현재 프로토타입 Visual만으로 은신 상태를 테스트할 수 있게 구성했다.

### 8. Fusion Multi-Peer 표시 처리

`ProjectJNetworkInvisibilityPresentation`에서 Runner별 로컬 관찰자를 별도로 관리한다.

```text
Dictionary<NetworkRunner, ProjectJNetworkInvisibilityPresentation>
```

구조를 사용해 같은 Unity Editor에서 여러 Fusion Peer를 실행하는 상황에서도 각 Runner의 로컬 Player를 관찰자 기준으로 사용할 수 있도록 했다.

Input Authority 판정은:

```text
networkPlayer.Object.HasInputAuthority
```

를 사용한다.

### 9. 유도탄 자동 추적 제외

기존 `ProjectJNetworkHomingMissile`의 Target 유효성 검사에 투명 망토 상태를 연결했다.

```text
IsAutoTargetTrackable == false
→ Homing Missile Target 제외
```

이미 유도탄이 플레이어를 추적 중일 때 해당 Target이 투명 망토를 사용하면 기존 재탐색 흐름으로 넘어간다.

```text
은신
→ 기존 Target 무효
→ 재탐색 1회
→ 새 Target 없음
→ 유도탄 제거
```

### 10. 드론 자동 추적 제외

128일차 드론에도 동일한 추적 가능 속성을 연결했다.

```text
IsAutoTargetTrackable == false
→ Drone Target 제외
```

현재 드론 Target이 투명화되면 기존 드론 재탐색 규칙을 사용한다.

따라서 투명 망토는 유도탄과 드론에 공통으로 대응할 수 있다.

### 11. 공통 자동 추적 가능 상태

`ProjectJNetworkExternalGameplay.InvisibilityCloak.cs`를 추가해:

```text
IsAutoTargetTrackable
IsInvisibleByCloak
```

속성을 제공한다.

향후 새로운 자동 추적 아이템이 추가되더라도 Inventory 내부 구현을 직접 참조하지 않고 이 공통 속성을 사용할 수 있다.

### 12. Push 사용 시 즉시 해제

Push 입력을 처리할 때:

```text
BreakInvisibilityCloakForPushAuthority()
```

를 호출한다.

상대를 실제로 밀쳤는지와 관계없이 Push 행동을 시작한 순간 은신을 해제한다.

따라서 은신 상태를 유지하면서 반복적으로 Push를 시도하는 것을 막는다.

### 13. 다른 아이템 사용 성공 시 해제

일반 Inventory 아이템 사용이 성공하면:

```text
BreakInvisibilityCloakForSuccessfulItemUseAuthority(itemId)
```

를 호출한다.

규칙:

```text
다른 아이템 성공
→ 은신 해제

다른 아이템 실패
→ 은신 유지

투명 망토 자체 사용 성공
→ 자기 자신 때문에 즉시 해제되지 않음
```

### 14. Stack 아이템 사용 연결

일반 단일 아이템 사용 흐름과 별개인 Stack 아이템 `PoolBall`에도 성공 사용 시 은신 해제 처리를 연결했다.

따라서 풀 공 투척 성공도 다른 공격 아이템과 동일하게 은신을 해제한다.

### 15. Respawn 및 경기 종료

다음 상황에서 상태를 초기화한다.

```text
Respawn
Gameplay 종료
Inventory Clear
```

이전 생명의 은신 상태와 Timer가 부활 뒤까지 남지 않도록 처리했다.

---

## 정책 값

`ProjectJInvisibilityCloakPolicy`의 주요 값:

| 항목 | 값 |
| --- | ---: |
| Network Item ID | 28 |
| 지속시간 | 5초 |
| 근거리 탐지 거리 | 2m |
| Shimmer 반복 주기 | 0.3초 |
| Shimmer 표시 시간 | 0.05초 |
| Shimmer 좌우 진폭 | 0.035 |

---

## 기본 동작 흐름

```text
투명 망토 사용
→ InvisibilityCloak = 28
→ 서버 5초 은신

자기 화면
→ Visual 정상 표시

다른 Player 화면
→ 2m 초과: Visual 숨김
→ 2m 이하: 짧은 Shimmer 표시

은신 중
→ Collider 유지
→ Network 위치 유지
→ RaceRank 유지
→ 유도탄 자동 Target 제외
→ 드론 자동 Target 제외

Push 사용
→ 즉시 은신 해제

다른 아이템 성공 사용
→ 즉시 은신 해제

다른 아이템 사용 실패
→ 은신 유지

5초 종료 / Respawn / 경기 종료
→ 은신 해제
```

---

## 정책 테스트

`ProjectJInvisibilityCloakPolicyTests.cs`를 추가했다.

작성된 테스트 사례는 총 35개다.

주요 검증 범위:

- Network Item ID 28
- 지속시간 5초
- 근거리 탐지 거리 2m
- 0.3초 Shimmer 주기
- 0.05초 표시 시간
- 활성 상태 중 재사용 차단
- 자기 화면 표시
- Remote 2m 초과 숨김
- Remote 2m 이하 Shimmer
- 자동 추적 제외
- Push 사용 해제
- 다른 아이템 성공 사용 해제
- 아이템 사용 실패 시 은신 유지
- 투명 망토 자체 사용 예외
- Shimmer 주기 경계
- 좌우 흔들림 계산
- 5초 지속시간 경계

---

## 컴파일 오류 수정

129일차 최초 구현 적용 후 Unity Test Runner 실행 시:

```text
Fix compilation issues before running tests
```

가 발생했다.

확인한 원인은 두 가지였다.

### 원인 1. Catalog 패치 전 enum 직접 참조

새 Runtime 코드가 Editor Installer 실행 전에:

```text
ProjectJNetworkItemId.InvisibilityCloak
```

를 직접 참조하고 있었다.

Unity에서는 Runtime 코드가 먼저 컴파일되어야 Editor Installer를 실행할 수 있으므로 Catalog에 enum 값이 추가되기 전에 컴파일이 막힐 수 있었다.

수정:

```text
ProjectJInvisibilityCloakPolicy.NetworkItemId = 28
```

상수를 두어 Installer 실행 전에는 고정 ID 값을 사용하게 변경했다.

### 원인 2. 존재하지 않는 Local Authority 속성

은신 Presentation 코드가:

```text
ProjectJNetworkPlayer.HasLocalInputAuthority
```

를 사용했지만 현재 Player 구현에는 해당 속성이 없었다.

수정:

```text
networkPlayer.Object.HasInputAuthority
```

를 사용하도록 변경했다.

이 수정 이후 사용자가 Unity Test Runner를 실행할 수 있는 상태까지 진행되었다.

---

## Shimmer 0.35초 경계 오류 수정

컴파일 문제 수정 후 `ProjectJInvisibilityCloakPolicyTests`에서 한 개의 테스트 실패가 확인되었다.

실패:

```text
IsShimmerVisible_UsesPeriodicBriefReveal(0.35f, False)

Expected: False
But was: True
```

원인은 `Mathf.Repeat()`를 사용한 float 반복 계산이었다.

논리적으로:

```text
0.35 - 0.30 = 0.05
```

이지만 float 계산에서는 Phase가 `0.05`보다 아주 작은 값으로 표현될 수 있어:

```text
phase < 0.05
```

판정이 `true`가 될 수 있었다.

최신 정책 코드는 다음 경계 보정을 추가했다.

```text
phase < ShimmerVisibleSeconds
&&
!Mathf.Approximately(
    phase,
    ShimmerVisibleSeconds
)
```

따라서 정확한 표시 종료 경계와 float 근사 경계를 표시 구간에서 제외한다.

테스트가 요구하는 주요 경계:

```text
0.049초 → 표시
0.05초  → 숨김
0.3초   → 새 주기 시작, 표시
0.349초 → 표시
0.35초  → 숨김
```

---

## 128일차 대비 변경 파일

최신 GitHub 비교 기준으로 129일차는 128일차 커밋보다 정확히 1개 커밋 앞서 있다.

변경 파일은 총 16개다.

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkDrone.cs
├─ ProjectJNetworkExternalGameplay.InvisibilityCloak.cs
├─ ProjectJNetworkExternalGameplay.InvisibilityCloak.cs.meta
├─ ProjectJNetworkExternalGameplay.cs
├─ ProjectJNetworkHomingMissile.cs
├─ ProjectJNetworkInvisibilityPresentation.cs
├─ ProjectJNetworkInvisibilityPresentation.cs.meta
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.InvisibilityCloak.cs
├─ ProjectJNetworkItemInventory.InvisibilityCloak.cs.meta
├─ ProjectJNetworkItemInventory.PoolBall.cs
└─ ProjectJNetworkItemInventory.cs

Assets/ProjectJ/Runtime/Items/
├─ ProjectJInvisibilityCloakPolicy.cs
└─ ProjectJInvisibilityCloakPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJInvisibilityCloakPolicyTests.cs
└─ ProjectJInvisibilityCloakPolicyTests.cs.meta
```

---

## Pickup 배치

이번에도 `Day49_AllSystemsTest` Scene에는 투명 망토 Pickup을 개별 추가하지 않는다.

남은 신규 아이템 구현이 끝난 뒤 Fusion Scene NetworkObject Bake/SortKey 문제를 피하기 위해 Pickup을 일괄 배치한다.

---

## 최신 커밋 확인

브랜치:

```text
main
```

최신 SHA:

```text
5c77508c6ef94f74586a0b80acdfc350a710cf00
```

현재 커밋 메시지:

```text
a
```

부모 커밋:

```text
b9715cb5d060c46f60e5b94dcfd0823a07ab78ee
128일차 : 드론 서버 권한 1위 추적 및 1회 공격·재탐색 구현
```

GitHub 비교 결과:

```text
ahead_by = 1
behind_by = 0
total_commits = 1
```

최신 커밋에서 정적으로 확인한 항목:

- `InvisibilityCloak = 28`
- `invisibility_cloak` Key
- `투명 망토` 표시 이름
- 5초 서버 은신 상태
- 2m Remote 근거리 표시
- 0.3초 / 0.05초 Shimmer 정책
- Runner별 Local Viewer 처리
- Input Authority 판정 수정
- 유도탄 Target 제외
- 드론 Target 제외
- Push 사용 시 해제
- 일반 아이템 성공 사용 시 해제
- PoolBall 성공 사용 시 해제
- Respawn / Clear 해제
- Catalog 부트스트랩 컴파일 문제 수정
- 0.35초 float 경계 보정
- EditMode 정책 테스트 35개 작성

---

## 검증 상태

129일차 진행 중 사용자가 확인한 순서는 다음과 같다.

```text
초기 적용
→ 컴파일 오류로 Test Runner 실행 불가

컴파일 수정 적용
→ Test Runner 실행 가능

ProjectJInvisibilityCloakPolicyTests
→ 0.35초 Shimmer 경계 1건 실패

Shimmer 경계 수정
→ 최신 main에 수정 코드 반영
```

최신 GitHub 코드에서는 `Mathf.Approximately()`를 이용한 0.35초 경계 수정이 확인된다.

다만 개발일지 작성 시점에는 **마지막 Shimmer 수정 이후 Unity Test Runner 전체를 다시 실행하여 0 Failure가 나온 실행 결과는 별도로 확인되지 않았다.**

GitHub Combined Status:

```text
등록된 Status 없음
```

GitHub Actions Workflow Run:

```text
없음
```

따라서 최신 코드 반영과 정적 연결은 확인했지만, 최종 Unity 전체 테스트 통과를 주장하지 않는다.

---

## Unity 최종 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJInvisibilityCloakPolicyTests` 다시 실행
3. `IsShimmerVisible_UsesPeriodicBriefReveal(0.35f, False)` 통과 확인
4. 가능하면 EditMode 전체 테스트 실행
5. 투명 망토 사용 시 5초 은신 확인
6. 자기 화면에서는 Visual이 계속 보이는지 확인
7. 다른 Player 화면에서 2m 초과 시 Visual이 숨겨지는지 확인
8. 2m 이내에서 Shimmer가 보이는지 확인
9. Host/Client 또는 Multi-Peer에서 각 관찰자 기준 표시가 정상인지 확인
10. 유도탄이 은신 Player를 Target에서 제외하는지 확인
11. 이미 추적 중 은신하면 재탐색으로 넘어가는지 확인
12. 드론이 은신 Player를 Target에서 제외하는지 확인
13. Push 입력 시 은신이 즉시 해제되는지 확인
14. 다른 아이템 성공 사용 시 은신이 해제되는지 확인
15. 실패한 아이템 사용에서는 은신이 유지되는지 확인
16. PoolBall 투척 성공 시 은신이 해제되는지 확인
17. Respawn 시 은신이 제거되는지 확인
18. 5초 종료 후 모든 Remote 화면에서 다시 정상 표시되는지 확인
19. Pickup 배치는 신규 아이템 구현 페이즈 종료 후 통합
