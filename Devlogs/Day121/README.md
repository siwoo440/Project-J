# 121일차 : 트램폴린 서버 권한 설치 및 3단계 소유자 전용 도약 구현

## 개발 목표

- 트램폴린을 네트워크 아이템으로 등록한다.
- 서버가 Player 발밑 지면을 검사해 설치 위치를 확정한다.
- 사용자당 활성 트램폴린을 1개만 유지한다.
- 새로운 트램폴린 설치 시 기존 자기 트램폴린을 제거한다.
- 트램폴린은 Owner에게만 발동한다.
- 첫 번째 7m/s, 두 번째 9m/s, 세 번째 11m/s 도약을 적용한다.
- 도약 속도는 누적하지 않고 Y 속도를 단계별 값으로 설정한다.
- 한 번 올라섰을 때 여러 Tick에서 중복 발동하지 않도록 한다.
- 최대 3회 사용 또는 12초 경과 시 제거한다.
- 경기 종료 또는 Owner 소실 시 제거한다.

## 구현 내용

### 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 트램폴린을 등록했다.

```text
Network ID: Trampoline = 20
Key: trampoline
Display Name: 트램폴린
```

기존 `SmokeGrenade = 19` 다음 번호를 사용한다.

기존 ItemDefinition은 다음 값을 사용한다.

```text
itemId: trampoline
displayName: 트램폴린
duration: 12
isPlaceable: 1
```

## 트램폴린 정책

`ProjectJTrampolinePolicy.cs`를 추가했다.

핵심 수치:

```text
LifetimeSeconds = 12초
MaximumUseCount = 3회

FirstLaunchSpeed = 7m/s
SecondLaunchSpeed = 9m/s
ThirdLaunchSpeed = 11m/s

InstallRayStartHeight = 0.5m
InstallRayDistance = 2.5m
MinimumGroundNormalY = 0.65

ActivationRadius = 0.9m
ActivationMinVerticalOffset = -0.25m
ActivationMaxVerticalOffset = 0.75m
```

정책 클래스에서 다음을 담당한다.

- 설치 가능 여부
- 유효 지면 판정
- 사용 횟수별 도약 속도
- 다음 사용 횟수
- 3회 사용 완료 판정
- Owner 발동 영역 판정
- 착지/하강 상태 발동 판정
- 제거 조건
- 설정형 수직 도약 속도 계산

## 발밑 설치

`ProjectJNetworkItemInventory.Trampoline.cs`를 추가했다.

트램폴린 사용 시 Player 위치 기준 아래 방향으로 Raycast를 실행한다.

```text
Player 위치 + 0.5m
↓
아래 방향 Raycast
↓
최대 2.5m
↓
유효 지면 확인
↓
트램폴린 Spawn
```

다음 조건을 만족하는 지면만 설치 대상으로 사용한다.

```text
Normal Y >= 0.65
Player Collider가 아님
설치 탐색 거리 안
```

유효한 지면이 없으면 아이템 사용은 실패한다.

## 설치 회전

지면 Normal을 기준으로 트램폴린의 위 방향을 맞춘다.

```text
Quaternion.FromToRotation(
    Vector3.up,
    groundHit.normal
)
```

이를 통해 완전히 수평이 아닌 지면에서도 지면 방향에 맞춰 설치한다.

## 사용자당 1개 제한

새 트램폴린을 설치하기 전에 현재 Runner에서 같은 Owner의 기존 트램폴린을 검색한다.

```text
기존 자기 트램폴린 없음
→ 그대로 설치

기존 자기 트램폴린 있음
→ 기존 트램폴린 Despawn
→ 새 트램폴린 Spawn
```

다른 Player가 설치한 트램폴린은 제거하지 않는다.

## Network Trampoline

`ProjectJNetworkTrampoline.cs`를 추가했다.

Networked 상태:

```text
NetworkInitialized
NetworkOwner
NetworkLifetimeTimer
NetworkUseCount
NetworkOwnerInsideActivationArea
NetworkLastLaunchSpeed
```

Spawn 후 서버가 Owner와 수명을 관리한다.

## Owner 전용 발동

트램폴린은 일반 Trigger 이벤트에 의존하지 않는다.

서버가 현재 Runner의 Player 목록에서 `NetworkOwner`와 일치하는 Player만 찾고 위치를 직접 검사한다.

따라서 다른 Player가 같은 위치를 지나가도 도약 효과를 받지 않는다.

## 발동 영역

Owner와 트램폴린 중심의 수평 거리 및 Y 차이를 검사한다.

```text
수평 거리 <= 0.9m
Y Offset >= -0.25m
Y Offset <= 0.75m
```

영역 안에 있고 다음 조건을 만족하면 발동한다.

```text
Owner 유효
Gameplay 허용
남은 사용 횟수 존재
Grounded 상태
또는
현재 수직 속도 <= 0
```

## 중복 발동 방지

`NetworkOwnerInsideActivationArea`를 사용한다.

```text
Owner가 발동 영역 밖
→ false

Owner가 처음 영역 안으로 들어옴
→ 발동 가능

발동 후 영역 안에 계속 머무름
→ 추가 발동 차단

공중으로 벗어남
→ false

다시 내려와 영역 진입
→ 다음 단계 발동 가능
```

이를 통해 한 번의 착지에서 여러 Tick이 연속으로 사용 횟수를 소비하는 문제를 막는다.

## 3단계 도약

사용 횟수별 도약 속도:

```text
UseCount 0
→ 7m/s
→ UseCount 1

UseCount 1
→ 9m/s
→ UseCount 2

UseCount 2
→ 11m/s
→ UseCount 3
→ 트램폴린 제거
```

## 설정형 수직 도약

`ProjectJNetworkExternalGameplay.Trampoline.cs`를 추가했다.

기존 외부 속도 시스템의 수평 성분은 유지하면서 Y 성분만 해당 도약 속도로 설정한다.

예:

```text
현재 External Velocity
(3, -5, 4)

두 번째 트램폴린 발동
→ (3, 9, 4)
```

따라서 다음처럼 도약 속도가 누적되지 않는다.

```text
7 + 9 + 11
```

각 발동 시점의 수직 속도를 정확히:

```text
7
9
11
```

중 하나로 설정한다.

외력 원인은 기존 `ProjectJExternalForceSource.Item`으로 기록한다.

## 제거 조건

다음 중 하나가 발생하면 트램폴린을 제거한다.

- 12초 Lifetime 종료
- 3회 사용 완료
- Owner NetworkObject 소실
- Owner Gameplay 비활성
- 새 자기 트램폴린 설치에 의한 교체

## 프로토타입 시각화

최종 트램폴린 에셋 대신 런타임 Cylinder를 사용한다.

사용 횟수에 따라 높이를 조금씩 변경해 현재 단계를 확인할 수 있게 한다.

```text
0회 사용
→ 기본 높이

1회 사용
→ 약간 증가

2회 사용
→ 추가 증가
```

최종 Mesh, 눌림/복원 애니메이션, Material, VFX는 이후 아트 단계에서 교체한다.

## Network Prefab

다음 Resource Prefab을 추가했다.

```text
Assets/ProjectJ/Network/Fusion/Player/Resources/
└─ ProjectJNetworkTrampoline.prefab
```

구성:

```text
NetworkObject
NetworkTransform
ProjectJNetworkTrampoline
```

Prefab에는 `FusionPrefab` 라벨이 적용되어 있다.

## 메인 인벤토리 연결

`ProjectJNetworkItemInventory.cs`의 아이템 사용 switch에 다음 연결을 추가했다.

```text
case ProjectJNetworkItemId.Trampoline
→ UseTrampolineAuthority()
```

## 테스트 추가

`ProjectJTrampolinePolicyTests`를 추가했다.

총 47개 테스트 사례가 작성되어 있다.

검증 항목:

- 수명 12초
- 최대 사용 횟수 3회
- 1회차 도약 7m/s
- 2회차 도약 9m/s
- 3회차 도약 11m/s
- 사용 완료 후 속도 0
- 사용 횟수 증가 및 상한
- 3회 사용 완료 판정
- Runner 준비 여부
- Gameplay 허용 여부
- 지면 Normal 경계값
- 설치 거리 경계값
- 발동 반경 0.9m 경계
- 발동 Y Offset 경계
- Owner 유효성
- Grounded/하강 상태 발동
- 상승 중 재발동 차단
- 수명 종료 제거
- Owner 소실 제거
- 3회 사용 후 제거
- 설정형 수직 속도
- 기존 수평 외부 속도 유지

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkExternalGameplay.Trampoline.cs
├─ ProjectJNetworkExternalGameplay.Trampoline.cs.meta
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.Trampoline.cs
├─ ProjectJNetworkItemInventory.Trampoline.cs.meta
├─ ProjectJNetworkItemInventory.cs
├─ ProjectJNetworkTrampoline.cs
└─ ProjectJNetworkTrampoline.cs.meta

Assets/ProjectJ/Network/Fusion/Player/Resources/
├─ ProjectJNetworkTrampoline.prefab
└─ ProjectJNetworkTrampoline.prefab.meta

Assets/ProjectJ/Runtime/Items/
├─ ProjectJTrampolinePolicy.cs
└─ ProjectJTrampolinePolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJTrampolinePolicyTests.cs
└─ ProjectJTrampolinePolicyTests.cs.meta
```

삭제한 파일은 없다.

## 최신 커밋 검증

확인한 최신 `main` 커밋:

```text
96d4f9fb61c96273f3ab1d419f10597e7d3a158d
```

현재 커밋 메시지는 임시 제목 `a`다.

정적 확인 내용:

- `Trampoline = 20` 등록 확인
- `trampoline` Key 매핑 확인
- `트램폴린` 표시 이름 확인
- 아이템 사용 switch 연결 확인
- 서버 권한 발밑 Raycast 설치 확인
- 유효 지면 Normal/거리 검사 확인
- 기존 자기 트램폴린 교체 흐름 확인
- Network Owner 저장 확인
- 12초 Networked Lifetime 확인
- 최대 3회 사용 확인
- 7→9→11m/s 단계별 속도 확인
- 설정형 Y 속도 적용 확인
- 기존 수평 External Velocity 유지 확인
- Owner 전용 검색 및 발동 확인
- 영역 이탈 전 재발동 차단 확인
- Gameplay 비활성 시 제거 확인
- Owner 소실 시 제거 확인
- 런타임 Cylinder 프로토타입 시각화 확인
- `ProjectJTrampolinePolicyTests` 47개 테스트 사례 작성 확인
- Network Prefab에 `FusionPrefab` 라벨 확인
- Network Prefab의 `ProjectJNetworkTrampoline` 스크립트 GUID와 `.meta` GUID 일치 확인

GitHub에 등록된 CI Status가 없으므로 Unity Editor 실제 컴파일과 EditMode Test Runner 통과 여부는 GitHub만으로 확정하지 않았다.

## 테스트맵 Pickup 배치 보류

이번 일차에도 `Day49_AllSystemsTest`에 트램폴린 Pickup을 개별 배치하지 않는다.

Fusion Scene NetworkObject SortKey/Bake 문제를 줄이기 위해 신규 아이템 Pickup 배치는 현재 아이템 구현 페이즈 종료 후 한 번에 통합한다.

따라서 Pickup 미배치는 미완료가 아니라 계획된 단계 보류다.

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJTrampolinePolicyTests` 전체 통과 확인
3. 트램폴린 사용 시 아이템 정상 소비 확인
4. Player 발밑 유효 지면에서 정상 설치되는지 확인
5. 허공 또는 유효하지 않은 지면에서 사용 실패하는지 확인
6. 새 트램폴린 설치 시 기존 자기 트램폴린이 제거되는지 확인
7. 다른 Player의 트램폴린은 유지되는지 확인
8. Network Prefab이 Host와 Client 양쪽에 표시되는지 확인
9. 설치 직후 Owner가 첫 번째 7m/s 도약을 하는지 확인
10. 다시 착지하면 두 번째 9m/s 도약을 하는지 확인
11. 세 번째 착지에서 11m/s 도약하는지 확인
12. 세 번째 사용 직후 트램폴린이 제거되는지 확인
13. 한 번 올라섰을 때 여러 Tick에서 사용 횟수가 연속 증가하지 않는지 확인
14. 다른 Player는 트램폴린 위에서 도약하지 않는지 확인
15. 도약 시 기존 수평 External Velocity가 유지되는지 확인
16. 도약 Y 속도가 이전 값에 누적되지 않는지 확인
17. 사용하지 않으면 약 12초 후 제거되는지 확인
18. Owner Gameplay 종료 시 제거되는지 확인
19. Owner NetworkObject가 사라지면 제거되는지 확인
20. Host와 Client에서 트램폴린 위치와 사용 횟수 변화가 일관되게 보이는지 확인
21. Pickup 배치는 아이템 구현 페이즈 종료 후 통합
