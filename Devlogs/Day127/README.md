# 127일차 개발일지 - 가시 갑옷 서버 권한 근접 접촉 반격

## 작업 개요

127일차에는 `가시 갑옷(Spiked Armor)` 아이템을 네트워크 아이템 시스템에 추가하고, 활성 시간 동안 가까이 접근한 다른 플레이어를 자동으로 밀어내는 접촉 방어 기능을 구현했다.

서버가 효과 지속시간, 주변 플레이어 탐색, 대상별 재발동 제한, 외부 속도 적용을 판정하도록 구성했으며, 기존 Project J의 공통 외력 시스템을 재사용했다.

구현 후 초기 `ItemDefinition` 카탈로그 테스트가 29종을 고정값으로 사용하고 있어 가시 갑옷 추가 시 30종이 되면서 실패하는 문제가 발생했다. 이에 가시 갑옷을 정식 30번째 데이터로 등록하고 카탈로그 테스트 및 임시 아이콘을 함께 수정했다.

---

## 구현 내용

### 1. 네트워크 아이템 등록

`ProjectJNetworkItemCatalog`에 가시 갑옷을 추가했다.

- Network Item ID: `26`
- Key: `spiked_armor`
- 표시 이름: `가시 갑옷`

기존 `ShrinkPotion = 25` 다음 번호를 사용한다.

### 2. ItemDefinition 추가

새 데이터 파일:

`Assets/ProjectJ/Data/Items/Item_SpikedArmor.asset`

주요 값:

```text
itemId = spiked_armor
displayName = 가시 갑옷
category = Defense
useMode = Instant
targetType = Self
duration = 5초
```

가시 갑옷 전용 최종 아이콘은 아직 없으므로 현재는 기존 복어 풍선옷 아이콘을 임시로 재사용한다.

### 3. 서버 권한 지속 효과

`ProjectJNetworkItemInventory.SpikedArmor.cs`를 추가했다.

주요 Network 상태:

```text
NetworkSpikedArmorTimer
```

사용 성공 시 서버 기준으로 5초 Timer를 시작한다.

이미 활성 상태에서는 다시 사용할 수 없으며, 사용 실패 시 두 번째 아이템은 소비하지 않는다.

### 4. 주변 플레이어 자동 감지

가시 갑옷이 활성 상태이면 State Authority가 매 Fusion Tick마다 현재 Runner의 플레이어를 검사한다.

검사 조건:

- 자기 자신 제외
- NetworkObject 유효 여부
- Gameplay 활성 여부
- 사용자와 대상 거리
- 대상별 재발동 제한

접촉 감지 반경:

```text
1.2m
```

### 5. 바깥 방향 반격

대상이 접촉 반경 안으로 들어오면:

```text
대상 위치 - 가시 갑옷 사용자 위치
```

를 기준으로 바깥 방향을 계산한다.

Y축은 제거하여 수평 방향만 사용한다.

외부 속도:

```text
6m/s
```

두 플레이어가 거의 같은 위치에 있어 방향을 계산하기 어려운 경우 사용자의 Forward 방향을 사용하고, 그마저 유효하지 않으면 World Forward를 사용한다.

### 6. 기존 외력 시스템 재사용

가시 갑옷 반격에는 새로운 이동 처리를 만들지 않고 기존 공통 API를 사용한다.

```text
TryApplyExternalVelocityChange(
    ProjectJExternalForceSource.Item,
    velocityChange
)
```

따라서 기존 외부 힘 보호 판정과 동일한 흐름을 사용한다.

예:

- 젤리 보호막
- Respawn Protection
- 되감기 중 외력 보호

외력이 보호 효과로 차단된 경우에는 해당 대상의 가시 갑옷 재발동 쿨다운을 시작하지 않는다.

### 7. 대상별 1초 재발동 제한

플레이어가 1.2m 안에 계속 머무르더라도 매 Tick 외력을 받지 않도록 PlayerRef Index별 `TickTimer`를 관리한다.

```text
대상 A 접촉
→ 6m/s 반격
→ A에게 1초 재발동 제한

1초 이내 다시 접촉
→ 발동하지 않음

1초 이후 다시 접촉
→ 다시 반격 가능
```

여러 플레이어가 동시에 접근하면 각 대상의 재발동 제한은 독립적으로 관리된다.

### 8. 종료 및 초기화

다음 상황에서 가시 갑옷 효과를 정리한다.

- 5초 Timer 종료
- Respawn
- Gameplay 비활성
- Inventory 전체 초기화

정리 시 대상별 재발동 Timer 기록도 함께 제거한다.

### 9. Inventory 연결

기존 Inventory 흐름에 다음 처리를 연결했다.

```text
Spawned()
→ InitializeSpikedArmorAuthority()

FixedUpdateNetwork()
→ UpdateSpikedArmorAuthority()

아이템 사용
→ ProjectJNetworkItemId.SpikedArmor
→ UseSpikedArmorAuthority()

ClearAuthority()
→ ClearSpikedArmorAuthority()

HandleRespawnAuthority()
→ ClearSpikedArmorAuthority()
```

---

## 정책 값

`ProjectJSpikedArmorPolicy`에 주요 수치를 분리했다.

| 항목 | 값 |
| --- | ---: |
| 지속시간 | 5초 |
| 접촉 감지 반경 | 1.2m |
| 반격 외부 속도 | 6m/s |
| 대상별 재발동 제한 | 1초 |
| Network Item ID | 26 |
| Item Key | `spiked_armor` |

---

## 정책 테스트

`ProjectJSpikedArmorPolicyTests.cs`를 추가했다.

작성된 테스트 범위:

- 정책 상수
- 활성 가능 조건
- 이미 활성 상태에서 재사용 차단
- 자기 자신 제외
- 대상 쿨다운 중 발동 차단
- Gameplay 비활성 대상 제외
- 1.2m 접촉 경계
- 1.2m 초과 대상 제외
- 수평 바깥 방향 계산
- 대각선 방향 정규화
- 동일 좌표 fallback 방향
- 1초 재발동 경계
- 5초 지속시간 경계

---

## ItemDefinition 카탈로그 오류 및 수정

127일차 최초 적용 후 Unity EditMode 전체 테스트에서 다음 실패가 발생했다.

```text
InitialReleaseCatalog_HasTwentyNineUniqueValidItems

Expected: 29
But was: 30
```

원인은 `Assets/ProjectJ/Data/Items`의 기존 초기 출시 카탈로그가 29종으로 고정되어 있는 상태에서 새 `Item_SpikedArmor.asset`이 추가되며 실제 ItemDefinition 수가 30개가 되었기 때문이다.

가시 갑옷을 삭제하지 않고 정식 카탈로그 항목으로 유지하기 위해 테스트를 다음과 같이 갱신했다.

```text
InitialReleaseCatalog_HasThirtyUniqueValidItems
Expected = 30
```

또한 개별 계획값 검증에 다음 TestCase를 추가했다.

```text
SpikedArmor
spiked_armor
가시 갑옷
Defense
Instant
Self
5초
```

개별 ItemDefinition 검증에서 Icon이 필수이므로 가시 갑옷에는 복어 풍선옷 이미지를 임시 아이콘으로 연결했다.

---

## 126일차 대비 변경 파일

최신 GitHub 비교 기준으로 127일차는 126일차 커밋보다 1개 커밋 앞서 있으며 다음 11개 파일이 변경되었다.

```text
Assets/ProjectJ/Data/Items/
├─ Item_SpikedArmor.asset
└─ Item_SpikedArmor.asset.meta

Assets/ProjectJ/Network/Fusion/Player/
├─ ProjectJNetworkItemCatalog.cs
├─ ProjectJNetworkItemInventory.SpikedArmor.cs
├─ ProjectJNetworkItemInventory.SpikedArmor.cs.meta
└─ ProjectJNetworkItemInventory.cs

Assets/ProjectJ/Runtime/Items/
├─ ProjectJSpikedArmorPolicy.cs
└─ ProjectJSpikedArmorPolicy.cs.meta

Assets/ProjectJ/Tests/EditMode/
├─ ProjectJItemDefinitionCatalogTests.cs
├─ ProjectJSpikedArmorPolicyTests.cs
└─ ProjectJSpikedArmorPolicyTests.cs.meta
```

---

## Pickup 배치

이번에도 `Day49_AllSystemsTest` Scene에 가시 갑옷 Pickup을 개별 추가하지 않는다.

Fusion Scene NetworkObject Bake/SortKey 문제를 피하기 위해 남은 신규 아이템 구현이 완료된 뒤 Pickup을 일괄 배치한다.

---

## 최신 커밋 확인

브랜치:

```text
main
```

최신 SHA:

```text
f4e1b8b63db0dfc3c2e703b2c0c94ec3582fec65
```

현재 커밋 메시지:

```text
a
```

부모 커밋:

```text
5c15c0a1f32eda1eef0443ac9b6e2697b6bf5c32
126일차 : 소형화 물약 서버 권한 80% 충돌체 축소 및 안전 원상복귀 구현
```

최신 커밋에서 정적으로 확인한 항목:

- `SpikedArmor = 26`
- `spiked_armor` Key
- `가시 갑옷` 표시 이름
- ItemDefinition 5초 설정
- 임시 Icon 연결
- Inventory 초기화 연결
- 매 Tick 접촉 판정 연결
- 아이템 사용 분기 연결
- Respawn 및 Clear 처리
- 접촉 반경 1.2m
- 외부 속도 6m/s
- 대상별 1초 재발동 제한
- 기존 공통 외력 시스템 사용
- 30종 ItemDefinition 카탈로그 테스트 반영
- 가시 갑옷 계획값 TestCase 반영
- 가시 갑옷 정책 테스트 포함

---

## 검증 상태

사용자가 수정 전 Unity EditMode 전체 테스트를 실행했을 때:

```text
986 tests
1 test failed
```

실패 항목은 29종 카탈로그 기대값과 새 30번째 가시 갑옷 데이터의 개수 불일치였으며, 최신 커밋에는 해당 수정이 반영되어 있다.

다만 수정 반영 이후 **전체 Unity Test Runner를 다시 실행한 결과는 현재 개발일지 작성 시점에 별도로 확인되지 않았다.**

또한 최신 GitHub 커밋에는 등록된 Combined Status와 GitHub Actions Workflow Run이 없다.

따라서 최신 저장소의 코드 및 데이터 연결은 정적으로 확인했지만, 수정 후 Unity 전체 테스트가 0 Failure라는 실행 증거는 아직 기록하지 않는다.

---

## Unity 최종 확인 항목

1. Unity Console 컴파일 Error 0건 확인
2. EditMode 전체 테스트 재실행
3. `InitialReleaseCatalog_HasThirtyUniqueValidItems` 통과 확인
4. `InitialReleaseItem_WithPlannedValues_IsValid(SpikedArmor...)` 통과 확인
5. `ProjectJSpikedArmorPolicyTests` 실행
6. 가시 갑옷 사용 시 5초 활성 확인
7. 1.2m 밖에서는 반격하지 않는지 확인
8. 1.2m 안 플레이어가 바깥 방향 6m/s로 밀리는지 확인
9. 동일 대상이 1초 이내 재발동하지 않는지 확인
10. 여러 대상이 각각 독립적으로 반격되는지 확인
11. 젤리 보호막 등 기존 외력 방어가 적용되는지 확인
12. Respawn 시 즉시 효과가 제거되는지 확인
13. 활성 중 두 번째 가시 갑옷 사용이 실패하고 아이템이 유지되는지 확인
14. Pickup 배치는 신규 아이템 구현 페이즈 종료 후 통합
