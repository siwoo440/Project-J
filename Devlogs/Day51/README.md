# Project J - 51일차 개발 일지

## 개발 목표

50일차에 구현한 아이템 공통 데이터, 2슬롯 인벤토리, Canvas UI를 실제 월드 아이템 획득 시스템과 연결한다.

이번 일차의 핵심 목표는 다음 흐름을 완성하는 것이다.

```text
빈 인벤토리
    ↓
월드 아이템 상자 접촉
    ↓
ItemDefinition 확인
    ↓
PlayerItemInventory.TryAdd()
    ↓
Q / E 슬롯 저장
    ↓
Canvas 자동 갱신
    ↓
획득 상자 제거
```

---

## 주요 개발 내용

### 1. ItemPickup 구현

접촉 즉시 획득되는 월드 아이템용 `ItemPickup` 컴포넌트를 추가했다.

주요 역할은 다음과 같다.

- Trigger 접촉 감지
- ItemDefinition 유효성 검사
- PlayerItemInventory 탐색
- 기존 `TryAdd()` 규칙을 이용한 아이템 저장
- 획득 성공 후 Trigger 비활성
- 획득 성공 후 Visual 숨김
- 획득 성공 후 Pickup 제거
- 중복 획득 방지

아이템 상자가 직접 슬롯 위치를 결정하지 않고 기존 인벤토리 규칙을 재사용하도록 구성했다.

```text
ItemPickup
    ↓
PlayerItemInventory.TryAdd()
```

따라서 첫 아이템은 Q 슬롯, 두 번째 아이템은 E 슬롯, 두 슬롯이 모두 차 있으면 현재 선택 슬롯을 교체하는 50일차 규칙이 그대로 유지된다.

---

### 2. F 상호작용과 분리된 접촉 획득

아이템 상자는 공통 F 상호작용 시스템과 분리했다.

```text
Player
  ↓ 접촉
ItemPickup Trigger
  ↓
즉시 획득
```

아이템 획득에 `IInteractable`이나 F 입력을 사용하지 않는다.

---

### 3. 실제 ItemDefinition 에셋 생성

대표 아이템 5종의 실제 ScriptableObject 데이터를 생성했다.

```text
Assets/ProjectJ/Data/Items/

├─ Item_SpringShoes.asset
├─ Item_JellyShield.asset
├─ Item_BananaCushion.asset
├─ Item_BalloonHorn.asset
└─ Item_WaterGun.asset
```

현재 각 아이템은 실제 효과가 아닌 공통 데이터 정의 단계다.

#### Spring Shoes

```text
ID          spring_shoes
Category    Mobility
Use Mode    Instant
Target      Self
Duration    8
```

#### Jelly Shield

```text
ID          jelly_shield
Category    Defense
Use Mode    Instant
Target      Self
Duration    4
```

#### Banana Cushion

```text
ID          banana_cushion
Category    Trap
Use Mode    Place
Target      WorldPosition
Placeable   true
```

#### Balloon Horn

```text
ID          balloon_horn
Category    Offensive
Use Mode    Instant
Target      Area
```

#### Water Gun

```text
ID          water_gun
Category    Offensive
Use Mode    Hold
Target      OtherPlayer
```

---

### 4. 공통 아이템 상자 Prefab 생성

다음 공통 Prefab을 추가했다.

```text
Assets/ProjectJ/Prefabs/Items/
└─ ItemPickupBox.prefab
```

기본 구조는 다음과 같다.

```text
ItemPickupBox
├─ BoxCollider
├─ ItemPickup
└─ Visual
   └─ QuestionMark
```

Root Collider는 Trigger로 사용하고, Visual의 Collider는 제거해 접촉 판정이 중복되지 않도록 구성했다.

---

### 5. Item Pickup 테스트 Lane 추가

기존 대형 통합 테스트 Scene에 아이템 획득 전용 Lane을 추가했다.

```text
Assets/ProjectJ/Tests/Manual/Day49/
└─ Day49_AllSystemsTest.unity
```

테스트 구역에는 다음 5개의 Pickup이 배치된다.

```text
Spring Shoes
Jelly Shield
Banana Cushion
Balloon Horn
Water Gun
```

각 Pickup에는 실제 ItemDefinition 에셋이 연결되어 있다.

---

### 6. 빈 인벤토리에서 실제 획득 흐름으로 변경

50일차에서는 Canvas 확인을 위해 Spring Shoes와 Jelly Shield를 자동으로 지급하는 Debug Seeder를 사용했다.

51일차에서는 실제 획득 테스트를 위해 해당 자동 지급 방식을 제거했다.

삭제된 테스트 요소:

```text
Day50InventoryDebugSeeder.cs
```

이제 게임 시작 시 인벤토리는 빈 상태에서 시작한다.

```text
[Q] EMPTY
[E] EMPTY
```

첫 번째 아이템 획득:

```text
[Q] Spring Shoes
[E] EMPTY
```

두 번째 아이템 획득:

```text
[Q] Spring Shoes
[E] Jelly Shield
```

두 슬롯이 가득 찬 상태에서 E 슬롯을 선택하고 세 번째 아이템을 획득하면 다음처럼 교체된다.

```text
[Q] Spring Shoes
[E] Banana Cushion
```

---

### 7. 획득 성공 후 Pickup 소비 처리

획득 성공 시 다음 순서로 처리한다.

```text
Inventory 저장 성공
      ↓
collected = true
      ↓
Trigger 비활성
      ↓
Visual 비활성
      ↓
Pickup Destroy
```

`collected` 상태를 먼저 적용해 같은 Pickup에서 Trigger가 여러 번 발생해도 아이템이 중복 지급되지 않도록 했다.

---

### 8. 잘못된 Pickup 방어 처리

다음 경우에는 아이템을 지급하지 않는다.

- ItemDefinition이 없음
- ItemDefinition 데이터가 유효하지 않음
- 접촉 대상에 PlayerItemInventory가 없음
- 이미 획득된 Pickup임
- Inventory 저장에 실패함

잘못된 ItemDefinition이 연결된 경우 Warning 로그를 출력하도록 구성했다.

---

### 9. ItemPickup EditMode 테스트 추가

`ItemPickupTests`를 추가했다.

다음 규칙을 검증한다.

- Pickup을 통해 아이템이 Inventory에 정상 저장되는지 확인
- 같은 Pickup의 중복 획득 차단
- ItemDefinition이 없는 Pickup 거부
- 두 슬롯이 가득 찬 경우 기존 선택 슬롯 교체 규칙 재사용

---

## 추가된 주요 파일

```text
Assets/ProjectJ/Runtime/Items/
└─ ItemPickup.cs

Assets/ProjectJ/Data/Items/
├─ Item_SpringShoes.asset
├─ Item_JellyShield.asset
├─ Item_BananaCushion.asset
├─ Item_BalloonHorn.asset
└─ Item_WaterGun.asset

Assets/ProjectJ/Prefabs/Items/
├─ ItemPickupBox.prefab
└─ Materials/

Assets/ProjectJ/Tests/EditMode/
└─ ItemPickupTests.cs
```

---

## 수정된 주요 요소

```text
Assets/ProjectJ/Tests/Manual/Day49/
└─ Day49_AllSystemsTest.unity
```

Day49 통합 테스트맵에 실제 아이템 획득 Lane과 대표 아이템 5종 Pickup을 추가했다.

---

## 제거된 요소

```text
Assets/ProjectJ/Tests/Manual/Day49/Scripts/
└─ Day50InventoryDebugSeeder.cs
```

51일차부터 실제 월드 획득을 기준으로 테스트하기 때문에 자동 아이템 지급은 제거했다.

---

## 구현 결과

- 아이템 데이터를 실제 월드 Pickup과 연결할 수 있게 되었다.
- 아이템 상자에 접촉하면 F 입력 없이 즉시 획득된다.
- 기존 2슬롯 인벤토리 저장 규칙을 그대로 재사용한다.
- 획득 직후 Inventory Changed 이벤트를 통해 Canvas가 자동 갱신된다.
- 첫 번째와 두 번째 아이템은 빈 슬롯에 순서대로 저장된다.
- 두 슬롯이 가득 찬 경우 선택 슬롯이 교체된다.
- 획득된 Pickup은 다시 사용할 수 없다.
- 대표 아이템 5종이 실제 ScriptableObject 에셋으로 저장되었다.
- 이후 아이템 효과 구현에서 동일한 ItemDefinition 에셋을 재사용할 수 있다.

---

## 현재 확인 상태

최신 GitHub 커밋 기준으로 다음 요소가 저장소에 반영되어 있다.

- ItemPickup Runtime 코드
- 대표 아이템 5종 ItemDefinition 에셋
- ItemPickupBox Prefab
- Pickup Material
- Day49 통합 Scene의 Item Pickup 테스트 구역
- ItemPickup EditMode 테스트
- Day50 Debug Seeder 제거

저장소 정적 검토 기준으로 개발 진행을 막는 명확한 구조적 문제는 확인되지 않았다.

현재 GitHub 커밋에는 CI 상태 검사가 등록되어 있지 않으므로 다음 항목은 로컬 Unity 환경에서 최종 확인한다.

- Unity Compile Error 0
- Day49_AllSystemsTest Play Mode 실행
- 시작 시 Q/E 슬롯 EMPTY
- 첫 Pickup → Q 슬롯 저장
- 두 번째 Pickup → E 슬롯 저장
- 선택 슬롯 교체 규칙 정상 동작
- Pickup 획득 즉시 Canvas 갱신
- 동일 Pickup 중복 획득 불가
- ItemPickupTests 통과
- Console 반복 Error 및 Exception 없음

---

## 다음 개발 방향

다음 단계에서는 기존 Input System에 이미 존재하는 `UseItem` 입력을 현재 선택된 슬롯과 연결한다.

기본 흐름은 다음과 같다.

```text
Q / E 슬롯 선택
      ↓
우클릭 UseItem
      ↓
SelectedItem 확인
      ↓
사용 가능 조건 검사
      ↓
성공하면 효과 실행
      ↓
성공한 경우만 아이템 소비
```

52일차에서는 아직 개별 아이템 효과를 구현하기보다 공통 사용 요청, 실패 처리, 소비 규칙을 먼저 완성하는 것을 목표로 한다.
