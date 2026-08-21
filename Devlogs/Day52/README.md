# Project J - 52일차 개발 일지

## 개발 목표

51일차까지 완성한 아이템 획득과 2슬롯 인벤토리 구조에 실제 `UseItem` 입력을 연결하고, 모든 아이템이 공통으로 사용할 수 있는 사용 요청·성공·실패·소비 파이프라인을 구축한다.

이번 일차의 핵심 흐름은 다음과 같다.

```text
아이템 획득
    ↓
Q / E 슬롯 선택
    ↓
우클릭 UseItem
    ↓
선택 ItemDefinition 확인
    ↓
등록된 Item Effect 탐색
    ↓
Effect 실행
  ↙       ↘
실패       성공
 ↓          ↓
유지       슬롯 소비
             ↓
        Canvas 자동 갱신
```

---

## 주요 개발 내용

### 1. 공통 아이템 사용 인터페이스 구현

모든 실제 아이템 효과가 같은 방식으로 호출될 수 있도록 `IItemUseEffect` 인터페이스를 추가했다.

```text
IItemUseEffect
└─ TryUse(ItemUseContext)
```

각 아이템 효과는 공통 Context를 받아 실행하고 `ItemUseResult`를 반환한다.

이 구조를 사용하면 `PlayerItemUseController`가 Spring Shoes, Jelly Shield, Water Gun 등의 개별 구현을 직접 알 필요가 없다.

---

### 2. ItemUseContext 구현

아이템 효과 실행에 필요한 공통 정보를 하나의 Context로 묶었다.

포함 정보:

```text
User
Inventory
Definition
SlotIndex
```

이를 통해 이후 개별 Effect에서 플레이어, 인벤토리, 사용 ItemDefinition, 사용 슬롯 정보를 동일한 방식으로 참조할 수 있다.

---

### 3. ItemUseResult 및 실패 상태 구현

아이템 사용 결과를 단순 `bool`이 아닌 명확한 상태 값으로 구분하도록 구현했다.

```text
Success
EmptySlot
InvalidItem
NoEffectHandler
InvalidTarget
InvalidPosition
Blocked
Cooldown
EffectFailed
InventoryChanged
```

이를 통해 사용 실패 이유와 성공 여부를 분리할 수 있게 되었다.

예:

```text
빈 슬롯
→ EmptySlot

실제 Effect 없음
→ NoEffectHandler

Target 없음
→ InvalidTarget

정상 사용
→ Success
```

---

### 4. ItemUseEffectRegistry 구현

Item ID와 실제 Effect 구현을 연결하기 위한 공통 Registry를 추가했다.

기본 구조:

```text
Item ID
   ↓
ItemUseEffectRegistry
   ↓
IItemUseEffect
```

예를 들어 이후 실제 효과가 구현되면 다음처럼 연결할 수 있다.

```text
spring_shoes
→ SpringShoesEffect

jelly_shield
→ JellyShieldEffect
```

Registry는 Item ID의 대소문자를 구분하지 않는다.

Play Mode가 새로 시작될 때 Static 상태를 초기화해 이전 실행의 Effect가 남지 않도록 구성했다.

---

### 5. PlayerItemUseController 구현

현재 선택 슬롯의 아이템 사용 전체 흐름을 담당하는 `PlayerItemUseController`를 추가했다.

처리 순서는 다음과 같다.

```text
현재 선택 슬롯 확인
      ↓
ItemDefinition 확인
      ↓
데이터 유효성 검사
      ↓
Effect Registry 조회
      ↓
Effect 실행
      ↓
결과 확인
      ↓
성공한 경우만 RemoveItem()
```

Inventory는 계속 아이템 보관과 슬롯 관리만 담당하고, 아이템 사용 과정은 `PlayerItemUseController`가 담당하도록 역할을 분리했다.

---

### 6. 실패 시 아이템 미소비 규칙 구현

52일차의 가장 중요한 공통 규칙이다.

잘못된 구조:

```text
우클릭
↓
아이템 먼저 제거
↓
Effect 실행
↓
실패
```

현재 구현:

```text
우클릭
↓
Effect 실행
↓
Success 확인
↓
성공했을 때만 RemoveItem()
```

따라서 다음 상황에서는 아이템이 사라지지 않는다.

```text
빈 슬롯
잘못된 데이터
Effect 없음
Target 없음
잘못된 위치
사용 차단
Cooldown
Effect 실패
```

---

### 7. Inventory 변경 보호

Effect 실행 중 다른 로직에 의해 선택 슬롯 내용이 바뀌는 상황을 방어하도록 구현했다.

Effect 성공 후에도 다음 조건을 다시 확인한다.

```text
현재 슬롯 Item
==
처음 사용 요청한 Item
```

다른 아이템으로 변경되어 있다면 자동 소비를 중단하고 `InventoryChanged`를 반환한다.

이로 인해 Effect 실행 과정에서 잘못된 다른 아이템이 제거되는 것을 방지한다.

---

### 8. UseItem 입력 연결

기존 `PlayerItemInventoryInput`을 수정해 슬롯 선택뿐 아니라 실제 아이템 사용 입력도 처리하도록 확장했다.

기존:

```text
Q
→ ItemSlotLeft

E
→ ItemSlotRight
```

추가:

```text
UseItem
→ PlayerItemUseController.TryUseSelectedItem()
```

Project J의 기존 Input Action을 그대로 사용하므로 별도의 새로운 Input Action은 추가하지 않았다.

현재 키보드·마우스 기준 아이템 사용은 우클릭이다.

---

### 9. Runtime Installer 확장

기존 `ItemInventoryRuntimeInstaller`가 Local Player에 다음 컴포넌트를 자동 준비하도록 확장했다.

```text
Player
├─ PlayerItemInventory
├─ PlayerItemUseController
└─ PlayerItemInventoryInput
```

따라서 Player Prefab에 52일차 컴포넌트를 직접 추가하지 않아도 Play Mode에서 공통 아이템 사용 시스템이 연결된다.

---

## 초기 사용 문제 확인

공통 아이템 사용 파이프라인을 처음 연결한 상태에서는 우클릭 입력 자체는 정상적으로 들어왔지만 실제 대표 아이템 Effect가 아직 등록되어 있지 않았다.

따라서 다음 상태가 발생했다.

```text
우클릭
↓
UseItem 입력
↓
PlayerItemUseController
↓
ItemUseEffectRegistry
↓
Effect 없음
↓
NoEffectHandler
↓
아이템 유지
```

코드 구조상 정상적인 실패 처리였지만, 실제 Play Mode에서는 아이템이 전혀 사용되지 않는 것처럼 보였다.

---

## 10. Day49 전용 테스트 Effect 추가

52일차 공통 사용 흐름을 실제 입력으로 검증하기 위해 `Day52ItemUseDebugEffectInstaller`를 추가했다.

이 테스트 Effect는 다음 Scene에서만 실행된다.

```text
Day49_AllSystemsTest
```

다음 대표 아이템 5종을 Registry에 등록한다.

```text
spring_shoes
jelly_shield
banana_cushion
balloon_horn
water_gun
```

각 아이템을 사용하면 테스트 Effect가 `Success`를 반환한다.

따라서 실제 흐름을 다음 단계까지 검증할 수 있다.

```text
Pickup
↓
Inventory 저장
↓
Q / E 슬롯 선택
↓
우클릭
↓
Effect 실행 성공
↓
선택 슬롯 소비
↓
Inventory Changed
↓
Canvas EMPTY
```

---

## 테스트 Effect의 역할

현재 추가된 Day52 테스트 Effect는 각 아이템의 실제 게임 능력을 구현한 코드가 아니다.

현재 동작:

```text
Spring Shoes
→ 사용 성공
→ 슬롯 소비
→ 실제 추가 점프 효과는 아직 없음

Jelly Shield
→ 사용 성공
→ 슬롯 소비
→ 실제 보호막 효과는 아직 없음

Banana Cushion
→ 사용 성공
→ 슬롯 소비
→ 실제 설치 효과는 아직 없음

Balloon Horn
→ 사용 성공
→ 슬롯 소비
→ 실제 범위 밀치기는 아직 없음

Water Gun
→ 사용 성공
→ 슬롯 소비
→ 실제 지속 사격은 아직 없음
```

즉 52일차에서는 공통 사용 시스템의 입력과 소비 규칙을 검증하고, 실제 개별 능력은 이후 일차에 순서대로 연결한다.

---

## EditMode 테스트 추가

`PlayerItemUseControllerTests`를 통해 공통 사용 시스템의 핵심 규칙을 검증하도록 구성했다.

검증 대상:

```text
빈 슬롯 사용
→ EmptySlot

Effect 미등록
→ NoEffectHandler
→ 아이템 유지

Effect 성공
→ 아이템 소비

Effect 실패
→ 아이템 유지

E 슬롯 선택 후 성공
→ E 슬롯만 소비
→ Q 슬롯 유지
```

---

## 추가된 주요 파일

```text
Assets/ProjectJ/Runtime/Items/
├─ IItemUseEffect.cs
├─ ItemUseContext.cs
├─ ItemUseEffectRegistry.cs
├─ ItemUseResult.cs
└─ PlayerItemUseController.cs

Assets/ProjectJ/Tests/EditMode/
└─ PlayerItemUseControllerTests.cs

Assets/ProjectJ/Tests/Manual/Day49/Scripts/
└─ Day52ItemUseDebugEffectInstaller.cs
```

---

## 수정된 주요 파일

```text
Assets/ProjectJ/Runtime/Items/
├─ PlayerItemInventoryInput.cs
└─ ItemInventoryRuntimeInstaller.cs
```

`PlayerItemInventoryInput`에는 `UseItem` 입력 처리를 추가했고, Runtime Installer에는 `PlayerItemUseController` 자동 설치 기능을 추가했다.

---

## 구현 결과

52일차 기준 아이템 수직 슬라이스의 공통 흐름은 다음 단계까지 연결되었다.

```text
월드 Pickup
↓
2슬롯 Inventory
↓
Canvas 표시
↓
Q / E 선택
↓
우클릭 UseItem
↓
Effect Registry 탐색
↓
공통 Effect 실행
↓
성공 / 실패 판정
↓
성공한 경우만 소비
↓
Canvas 자동 갱신
```

대표 아이템 5종은 Day49 테스트맵에서 임시 성공 Effect를 통해 우클릭 사용과 소비 과정을 직접 확인할 수 있다.

---

## 현재 확인 상태

최신 GitHub `main` 기준으로 다음 요소가 반영되어 있다.

```text
IItemUseEffect
ItemUseContext
ItemUseEffectRegistry
ItemUseResult
PlayerItemUseController
UseItem 입력 연결
Runtime Installer 확장
PlayerItemUseControllerTests
Day52ItemUseDebugEffectInstaller
```

최신 커밋에서 Day49 테스트용 Effect 등록기가 실제 저장소에 포함되어 있으며 대표 아이템 5종 ID를 Registry에 등록하도록 구성되어 있다.

저장소 정적 검토 기준으로 다음 개발 진행을 막는 명확한 문제는 확인되지 않았다.

GitHub에는 현재 CI 상태 검사가 등록되어 있지 않으므로 최종 완료 여부는 로컬 Unity 환경에서 다음 조건으로 확인한다.

```text
Unity Compile Error 0
Day49_AllSystemsTest Play Mode 실행

아이템 Pickup 정상
Q / E 슬롯 선택 정상
우클릭 입력 정상

대표 아이템 사용 시
[Day52 Test] ... 사용 성공 로그 출력

사용 성공 후 선택 슬롯 EMPTY
다른 슬롯은 유지
Canvas 즉시 갱신

PlayerItemUseControllerTests 통과
Console 반복 Error / Exception 없음
```

---

## 다음 개발 방향

다음 일차에서는 공통 테스트 Effect를 실제 첫 번째 아이템 Effect로 교체한다.

첫 구현 대상은 `Spring Shoes`다.

기본 흐름:

```text
Spring Shoes 획득
↓
Q 또는 E 선택
↓
우클릭 사용
↓
SpringShoesEffect 실행
↓
8초 Buff 시작
↓
공중 추가 점프 1회 허용
↓
슬롯 소비
↓
8초 종료
↓
기본 점프 상태 복귀
```

53일차부터는 52일차에서 완성한 공통 사용 파이프라인을 수정하지 않고 각 아이템 Effect를 독립적으로 추가하는 방향으로 진행한다.
