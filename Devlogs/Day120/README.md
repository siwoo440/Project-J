# 120일차 : 연막탄 서버 권한 포물선 투척 및 범위 시야 방해 구역 구현

## 개발 목표

- 연막탄을 네트워크 아이템으로 등록한다.
- 서버 권한으로 연막탄을 포물선 투척한다.
- 최대 수평 투척 거리 14m를 적용한다.
- 지형 착탄 시 별도의 Network Smoke Zone을 생성한다.
- Smoke Zone을 반경 5m, 최대 6초 동안 유지한다.
- 연막 안에 있는 로컬 Player에게 약 60% 화면 시야 방해를 적용한다.
- 여러 연막이 겹쳐도 화면 농도를 중첩하지 않는다.
- 사용자당 활성 연막을 최대 2개까지 유지한다.
- 세 번째 연막 생성 시 가장 오래된 연막을 제거한다.
- 맵 낙하 한계 아래로 떨어진 투척체는 연막 생성 없이 제거한다.

## 구현 내용

### 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 연막탄을 등록했다.

```text
Network ID: SmokeGrenade = 19
Key: smoke_grenade
Display Name: 연막탄
```

기존 `SoapBubble = 18` 다음 번호를 사용한다.

기존 ItemDefinition은 다음 값을 유지한다.

```text
itemId: smoke_grenade
displayName: 연막탄
duration: 6
```

## 연막탄 정책

`ProjectJSmokeGrenadePolicy.cs`를 추가했다.

핵심 수치:

```text
SmokeDurationSeconds = 6초
MaximumThrowDistance = 14m
SmokeRadius = 5m
OverlayAlpha = 0.6
MaximumActiveZonesPerOwner = 2

CollisionRadius = 0.3m
PrototypeHorizontalThrowSpeed = 12m/s
PrototypeVerticalThrowSpeed = 6m/s
PrototypeGravity = -12m/s²
```

정책 클래스에서 다음을 담당한다.

- 사용 가능 여부
- 연막 반경 포함 여부
- 화면 Overlay 농도
- 여러 연막 중첩 시 농도 고정
- 낙하 한계 판정
- 포물선 초기 속도
- 수평 투척 거리 계산
- Smoke Zone 유지 여부

## 서버 권한 포물선 투척

`ProjectJNetworkSmokeGrenadeProjectile.cs`를 추가했다.

기존 폭탄의 포물선 투척 흐름을 기반으로 다음 구조를 사용한다.

```text
연막탄 사용
→ Runner.Spawn()
→ State Authority에서 포물선 이동
→ Gravity 적용
→ SphereCast 충돌 판정
→ 지형 착탄
→ Smoke Zone 생성
→ 투척체 Despawn
```

Player Collider는 착탄 지형 판정에서 제외한다.

따라서 다른 Player에 직접 맞았다고 즉시 연막이 생성되는 구조가 아니라 실제 지형에 착탄할 때 연막이 생성된다.

## 최대 투척 거리

투척체는 최초 투척 위치 기준 최대 수평 거리 14m를 사용한다.

```text
현재 수평 거리 < 14m
→ 정상 포물선 이동

현재 수평 거리 >= 14m
→ 수평 속도 제거
→ 중력에 의해 아래로 낙하
```

이를 통해 최대 투척 범위를 넘어서 계속 전진하지 않도록 제한한다.

## 맵 밖 투척 처리

기존 Checkpoint 낙하 한계 시스템을 재사용한다.

`ProjectJNetworkItemInventory.SmokeGrenade.cs`에서 현재 Player의:

```text
CurrentCheckpointId
```

를 기준으로 `CheckpointFallLimitSet.GetFallLimitY()` 값을 가져와 투척체에 전달한다.

투척체 Y가 해당 낙하 한계 아래로 내려가면:

```text
Projectile Despawn
Smoke Zone 생성 안 함
```

으로 처리한다.

## Smoke Zone 분리

투척체와 연막 구역을 별도의 NetworkObject로 분리했다.

```text
ProjectJNetworkSmokeGrenadeProjectile
        ↓ 지형 착탄
ProjectJNetworkSmokeZone
```

연막 구역은 착탄 지점에서 생성되고 독립적으로 수명을 관리한다.

## Smoke Zone Network 상태

`ProjectJNetworkSmokeZone`은 다음 Networked 상태를 사용한다.

```text
NetworkInitialized
NetworkOwner
NetworkLifetimeTimer
NetworkSpawnOrder
```

생성 시 6초 Timer를 시작한다.

## 연막 반경

Smoke Zone 중심 기준 반경 5m를 사용한다.

```text
Player와 연막 중심 거리 <= 5m
→ 연막 내부

Player와 연막 중심 거리 > 5m
→ 연막 외부
```

이번 구현은 프로토타입 단계이므로 벽이나 장애물에 따른 연막 차폐 계산은 사용하지 않는다.

## 로컬 시야 방해

연막 안에 들어간 로컬 Player에게 Screen Space Overlay를 표시한다.

Overlay 값:

```text
Alpha = 0.6
Sorting Order = 29000
```

연막 안에 있을 때만 활성화되고 구역을 벗어나면 즉시 숨긴다.

연막 효과는 특정 Target에게 6초 상태를 부여하는 방식이 아니라 현재 Smoke Zone 내부 여부를 실시간으로 검사하는 구역 기반 효과다.

## 연막 중첩 농도 고정

여러 Smoke Zone이 겹쳐도 Overlay Alpha는 증가하지 않는다.

```text
연막 0개
→ Alpha 0

연막 1개
→ Alpha 0.6

연막 2개 이상
→ Alpha 0.6
```

따라서 연막이 겹친다고 화면이 추가로 어두워지지 않는다.

## 자기 자신과 다른 Player 모두 영향

Smoke Zone은 월드 구역 효과이므로 투척 사용자와 상대 Player를 구분하지 않는다.

연막 내부에 있는 로컬 Player라면 누구든 화면 효과를 받는다.

Jelly Shield와 Respawn 보호는 연막의 시야 방해 판정에 사용하지 않는다.

## 활성 연막 최대 2개

사용자별 활성 Smoke Zone을 최대 2개로 제한한다.

각 연막 생성 시 `NetworkSpawnOrder`를 저장한다.

세 번째 Smoke Zone을 생성하면 같은 Owner의 연막 중 가장 오래된 하나를 제거한다.

```text
연막 1개
→ 유지

연막 2개
→ 모두 유지

연막 3번째 생성
→ 가장 오래된 연막 제거
→ 최신 2개 유지
```

## 경기 상태 연동

활성 Gameplay Player가 없거나 Smoke Zone의 6초 Timer가 종료되면 서버가 Smoke Zone을 제거한다.

경기 종료 후 연막이 계속 남아 있지 않도록 처리한다.

## 프로토타입 월드 시각화

Smoke Zone은 런타임에 반투명 Sphere를 생성한다.

```text
Diameter = 10m
Radius = 5m
```

프로토타입 연막 범위를 눈으로 확인하기 위한 표현이다.

최종 Smoke Particle, VFX Graph, Volumetric Fog, Shader는 이후 아트 단계에서 교체한다.

## 메인 인벤토리 연결

`ProjectJNetworkItemInventory.cs`의 아이템 사용 switch에 다음 연결을 추가했다.

```text
case ProjectJNetworkItemId.SmokeGrenade
→ UseSmokeGrenadeAuthority()
```

별도의 지속 Player 상태가 아니므로 Spawned 초기화, Respawn 상태 Clear 등은 추가하지 않는다.

## Network Prefab

다음 두 Resource Prefab을 추가했다.

```text
Assets/ProjectJ/Network/Fusion/Player/Resources/
├─ ProjectJNetworkSmokeGrenadeProjectile.prefab
└─ ProjectJNetworkSmokeZone.prefab
```

### 연막탄 투척체

```text
NetworkObject
NetworkTransform
ProjectJNetworkSmokeGrenadeProjectile
```

### Smoke Zone

```text
NetworkObject
NetworkTransform
ProjectJNetworkSmokeZone
```

## 테스트 추가

`ProjectJSmokeGrenadePolicyTests`를 추가했다.

총 30개 테스트 사례가 작성되어 있다.

검증 항목:

- 연막 지속 시간 6초
- 최대 투척 거리 14m
- 연막 반경 5m
- Overlay Alpha 0.6
- 최대 활성 연막 2개
- Runner 준비 여부
- Gameplay 활성 여부
- 5m 경계 포함
- 5m 초과 제외
- 연막 중첩 시 농도 고정
- 낙하 한계 판정
- 포물선 초기 속도
- Forward Y축 제거
- 수평 거리 계산에서 높이 제외
- Lifetime 종료 시 Smoke Zone 제거
- Gameplay 종료 시 Smoke Zone 제거

## 변경 파일

```text
Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.SmokeGrenade.cs
├─ ProjectJNetworkItemInventory.SmokeGrenade.cs.meta
├─ ProjectJNetworkItemInventory.cs
├─ ProjectJNetworkSmokeGrenadeProjectile.cs
├─ ProjectJNetworkSmokeGrenadeProjectile.cs.meta
├─ ProjectJNetworkSmokeZone.cs
└─ ProjectJNetworkSmokeZone.cs.meta

Assets/ProjectJ/Network/Fusion/Player/Resources/
├─ ProjectJNetworkSmokeGrenadeProjectile.prefab
├─ ProjectJNetworkSmokeGrenadeProjectile.prefab.meta
├─ ProjectJNetworkSmokeZone.prefab
└─ ProjectJNetworkSmokeZone.prefab.meta

Assets/ProjectJ/Runtime/Items/
├─ ProjectJSmokeGrenadePolicy.cs
└─ ProjectJSmokeGrenadePolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJSmokeGrenadePolicyTests.cs
└─ ProjectJSmokeGrenadePolicyTests.cs.meta
```

삭제한 파일은 없다.

## 최신 커밋 검증

확인한 최신 `main` 커밋:

```text
b3c710577714d69b722b4560f78046a05cf1afb3
```

현재 커밋 메시지는 임시 제목 `a`다.

정적 확인 내용:

- `SmokeGrenade = 19` 등록 확인
- `smoke_grenade` Key 매핑 확인
- `연막탄` 표시 이름 확인
- 아이템 사용 switch 연결 확인
- 서버 권한 연막탄 Spawn 확인
- Checkpoint 낙하 한계 재사용 확인
- 최대 수평 투척 거리 14m 확인
- 포물선 중력 적용 확인
- SphereCast 지형 착탄 판정 확인
- 착탄 시 `ProjectJNetworkSmokeZone` 생성 확인
- Smoke Zone 6초 Timer 확인
- 반경 5m 판정 확인
- 로컬 화면 Overlay Alpha 0.6 확인
- 여러 연막 중첩 시 농도 고정 확인
- 사용자당 활성 Smoke Zone 최대 2개 확인
- 세 번째 Smoke Zone 생성 시 가장 오래된 연막 제거 흐름 확인
- Gameplay 종료 시 Smoke Zone 제거 확인
- `ProjectJSmokeGrenadePolicyTests` 30개 테스트 사례 작성 확인
- Smoke Zone Prefab의 스크립트 GUID와 `.meta` GUID 일치 확인

GitHub에 등록된 CI Status가 없으므로 Unity Editor 실제 컴파일과 EditMode Test Runner 통과 여부는 GitHub만으로 확정하지 않았다.

## 테스트맵 Pickup 배치 보류

이번 일차에도 `Day49_AllSystemsTest`에 연막탄 Pickup을 개별 배치하지 않는다.

Fusion Scene NetworkObject SortKey/Bake 문제를 줄이기 위해 신규 아이템 Pickup 배치는 현재 아이템 구현 페이즈 종료 후 한 번에 통합한다.

따라서 Pickup 미배치는 미완료가 아니라 계획된 단계 보류다.

## Unity 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. `ProjectJSmokeGrenadePolicyTests` 전체 통과 확인
3. 연막탄 사용 시 아이템 정상 소비 확인
4. 서버에서 연막탄 투척체가 정상 Spawn되는지 확인
5. 투척체가 포물선으로 이동하는지 확인
6. 수평 최대 거리 약 14m가 적용되는지 확인
7. Player Collider에 맞아 즉시 착탄 처리되지 않는지 확인
8. 지형 착탄 시 투척체가 제거되는지 확인
9. 착탄 위치에 Smoke Zone이 생성되는지 확인
10. Smoke Zone이 약 6초 유지되는지 확인
11. Smoke Zone 반경이 약 5m인지 확인
12. 연막 내부에서 로컬 화면 Overlay가 표시되는지 확인
13. 연막 외부로 이동하면 Overlay가 즉시 해제되는지 확인
14. 투척 사용자 자신도 연막 내부에서 효과를 받는지 확인
15. 다른 Player도 자신의 Client에서 동일하게 효과를 받는지 확인
16. 연막 2개가 겹쳐도 화면 농도가 더 짙어지지 않는지 확인
17. 사용자당 연막 2개가 동시에 유지되는지 확인
18. 세 번째 연막 생성 시 가장 오래된 연막이 제거되는지 확인
19. 현재 Checkpoint 낙하 한계 아래로 떨어지면 Smoke Zone 없이 투척체가 제거되는지 확인
20. 경기 종료 시 활성 Smoke Zone이 제거되는지 확인
21. Host와 Client에서 Smoke Zone 위치와 수명이 일관되게 보이는지 확인
22. 최종 Smoke Particle/VFX는 추후 아트 단계에서 교체
23. Pickup 배치는 아이템 구현 페이즈 종료 후 통합
