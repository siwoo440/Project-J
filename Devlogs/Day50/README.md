# Project J - 50일차 개발 일지

## 개발 목표

PHASE 5 아이템 수직 슬라이스의 첫 단계로 모든 아이템이 공통으로 사용할 데이터 구조를 만들고, 플레이어가 최대 2개의 아이템을 보관할 수 있는 기본 인벤토리 시스템과 Canvas 기반 인벤토리 UI를 구현한다.

이번 단계에서는 실제 아이템 효과와 아이템 상자 획득 기능까지 확장하지 않고 다음 기반을 완성하는 것을 목표로 한다.

```text
아이템 공통 데이터
      ↓
데이터 검증
      ↓
2슬롯 인벤토리
      ↓
Q / E 슬롯 선택
      ↓
Canvas UI 표시
```

---

## 주요 개발 내용

### 1. 아이템 공통 데이터 구조 구현

`ItemDefinition` ScriptableObject를 추가했다.

모든 아이템이 공통으로 사용할 수 있도록 다음 데이터를 정의했다.

```text
Item ID
Display Name
Category
Use Mode
Target Type
Duration
Cooldown
Is Placeable
Icon
```

아이템 효과 로직과 아이템 데이터를 분리해 이후 새로운 아이템을 추가할 때 공통 데이터 구조를 재사용할 수 있도록 구성했다.

---

### 2. 아이템 분류 Enum 구현

아이템의 역할과 사용 방식을 명확하게 구분하기 위해 공통 Enum을 추가했다.

#### ItemCategory

```text
Mobility
Defense
Offensive
Trap
Utility
```

#### ItemUseMode

```text
Instant
Hold
Toggle
Place
```

#### ItemTargetType

```text
Self
OtherPlayer
Area
WorldPosition
```

이 구조를 기준으로 이후 대표 아이템과 전체 아이템 데이터를 동일한 규칙으로 관리할 수 있다.

---

### 3. 아이템 데이터 Validator 구현

`ItemDefinitionValidator`를 추가했다.

개별 아이템 데이터에서 다음 문제를 검사한다.

- Item ID 누락
- Display Name 누락
- 잘못된 Duration
- 잘못된 Cooldown
- Place 아이템의 설치 가능 설정 불일치

여러 아이템을 하나의 목록으로 검사할 때는 Item ID 중복 여부도 검사한다.

ID 중복 검사는 대소문자를 구분하지 않는다.

```text
water_gun
WATER_GUN
```

위 두 ID는 동일한 ID로 판단한다.

---

### 4. 플레이어 2슬롯 인벤토리 구현

`PlayerItemInventory`를 추가했다.

Project J의 기본 아이템 보관 수를 2개로 고정했다.

```text
Slot 0 → Q
Slot 1 → E
```

아이템 저장 규칙은 다음과 같다.

```text
첫 번째 슬롯이 비어 있음
→ 첫 번째 슬롯에 저장

첫 번째 슬롯 사용 중
두 번째 슬롯 비어 있음
→ 두 번째 슬롯에 저장

두 슬롯 모두 사용 중
→ 현재 선택 슬롯을 새 아이템으로 교체
```

추가로 다음 기본 기능을 구현했다.

- 슬롯 아이템 조회
- 아이템 추가
- 아이템 제거
- 슬롯 선택
- 전체 초기화
- 선택 아이템 조회
- Inventory 변경 이벤트

---

### 5. Q / E 슬롯 선택 입력 연결

`PlayerItemInventoryInput`을 추가했다.

기존 Input System의 다음 Action을 그대로 재사용한다.

```text
ItemSlotLeft
ItemSlotRight
```

키보드 기준:

```text
Q → Slot 1
E → Slot 2
```

게임패드 기준:

```text
D-Pad Left  → Slot 1
D-Pad Right → Slot 2
```

새로운 Input Action을 만들지 않고 기존 Project J 입력 정의를 그대로 활용하도록 구성했다.

---

### 6. Runtime Inventory 자동 설치

`ItemInventoryRuntimeInstaller`를 추가했다.

Play Mode가 시작되면 Local Player의 `PlayerInput`을 탐색하고 다음 컴포넌트를 자동으로 준비한다.

```text
Player
├─ PlayerItemInventory
└─ PlayerItemInventoryInput
```

Local Player가 Scene에 늦게 생성되는 상황도 대응할 수 있도록 Player가 준비될 때까지 Coroutine으로 탐색한다.

Scene이 변경되면 새 Local Player에 다시 Inventory를 연결한다.

---

### 7. Canvas 기반 인벤토리 UI 구현

`ItemInventoryCanvasView`를 추가했다.

인벤토리는 별도의 Scene 수동 설정 없이 런타임에 Canvas로 자동 생성된다.

Canvas 설정은 다음과 같다.

```text
Render Mode
Screen Space - Overlay

UI Scale Mode
Scale With Screen Size

Reference Resolution
1920 × 1080

Match
0.5
```

화면 오른쪽 아래에 두 개의 아이템 슬롯이 표시된다.

기본 형태:

```text
┌────────────────────────────────────┐
│ ITEM INVENTORY                     │
│                                    │
│ ┌──────────────┐ ┌──────────────┐  │
│ │ [Q]          │ │ [E]          │  │
│ │ Item         │ │ Item         │  │
│ │ Type / Mode  │ │ Type / Mode  │  │
│ └──────────────┘ └──────────────┘  │
└────────────────────────────────────┘
```

선택된 슬롯은 다른 배경색으로 강조된다.

아이템에 Icon Sprite가 등록되어 있으면 실제 아이콘을 표시하고, 아이콘이 없는 경우에는 Item Category에 따라 임시 색상을 표시한다.

---

### 8. Inventory와 Canvas 실시간 연동

`PlayerItemInventory.Changed` 이벤트를 이용해 인벤토리 상태가 변경될 때 Canvas가 즉시 갱신되도록 구현했다.

다음 변경 사항이 바로 UI에 반영된다.

- 아이템 획득
- 아이템 제거
- 슬롯 교체
- Q / E 슬롯 선택
- Inventory 초기화

UI가 Inventory 데이터를 직접 수정하지 않고 표시만 담당하도록 구조를 분리했다.

---

### 9. Day49 통합 테스트맵 Debug Seeder 추가

`Day50InventoryDebugSeeder`를 추가했다.

`Day49_AllSystemsTest` Scene에서 Canvas 인벤토리 상태를 바로 확인할 수 있도록 테스트 전용 아이템 두 개를 임시로 지급한다.

```text
Slot 1
Spring Shoes

Slot 2
Jelly Shield
```

이 아이템 데이터는 런타임 테스트용 ScriptableObject로만 생성되며 실제 프로젝트 에셋으로 저장하지 않는다.

Debug Seeder는 다음 Scene에서만 동작한다.

```text
Day49_AllSystemsTest
```

다른 Scene에는 테스트 아이템을 자동 지급하지 않는다.

---

### 10. EditMode 테스트 추가

아이템 데이터와 2슬롯 인벤토리의 기본 규칙을 검증하기 위한 EditMode 테스트를 추가했다.

#### ItemDefinitionValidatorTests

- 정상 ItemDefinition 허용
- Item ID 누락 거부
- 중복 Item ID 거부

#### PlayerItemInventoryTests

- 빈 슬롯 순서대로 저장
- 두 슬롯이 가득 찬 경우 선택 슬롯 교체
- 슬롯 선택 상태 변경

---

## 추가된 주요 파일

```text
Assets/ProjectJ/Runtime/Items/
├─ ItemEnums.cs
├─ ItemDefinition.cs
├─ ItemDefinitionValidator.cs
├─ PlayerItemInventory.cs
├─ PlayerItemInventoryInput.cs
└─ ItemInventoryRuntimeInstaller.cs

Assets/ProjectJ/Runtime/UI/
└─ ItemInventoryCanvasView.cs

Assets/ProjectJ/Tests/EditMode/
├─ ItemDefinitionValidatorTests.cs
└─ PlayerItemInventoryTests.cs

Assets/ProjectJ/Tests/Manual/Day49/Scripts/
└─ Day50InventoryDebugSeeder.cs
```

---

## 기존 시스템 재사용

이번 개발에서는 기존 Input Action 파일을 수정하지 않았다.

이미 Project J에 다음 Action이 정의되어 있기 때문이다.

```text
UseItem
ItemSlotLeft
ItemSlotRight
```

현재 연결은 다음과 같다.

```text
UseItem
Keyboard & Mouse → Right Mouse Button
Gamepad          → Left Trigger

ItemSlotLeft
Keyboard         → Q
Gamepad          → D-Pad Left

ItemSlotRight
Keyboard         → E
Gamepad          → D-Pad Right
```

50일차에서는 슬롯 선택만 실제 Inventory와 연결했다.

`UseItem`은 이후 실제 아이템 사용 시스템에서 연결한다.

---

## 구현 결과

- PHASE 5에서 사용할 공통 아이템 데이터 구조가 마련되었다.
- 아이템 효과 코드와 데이터 정의가 분리되었다.
- 잘못된 Item ID와 중복 ID를 검사할 수 있게 되었다.
- 플레이어가 최대 2개의 아이템을 보관할 수 있게 되었다.
- 두 슬롯이 가득 찬 경우 현재 선택 슬롯을 교체할 수 있다.
- Q와 E 입력으로 원하는 아이템 슬롯을 선택할 수 있다.
- Canvas를 통해 현재 보유 아이템을 화면에서 확인할 수 있다.
- 선택된 슬롯이 UI에서 즉시 강조된다.
- 인벤토리 변경과 UI 갱신이 이벤트 기반으로 연결되었다.
- Day49 통합 테스트맵에서 Spring Shoes와 Jelly Shield를 이용해 UI를 바로 확인할 수 있다.

---

## 현재 확인 상태

최신 50일차 GitHub 커밋 기준으로 다음 요소가 저장소에 포함되어 있다.

- ItemDefinition
- 아이템 Enum
- ItemDefinitionValidator
- PlayerItemInventory
- PlayerItemInventoryInput
- ItemInventoryRuntimeInstaller
- Canvas 기반 ItemInventoryCanvasView
- Day49 Inventory Debug Seeder
- ItemDefinition Validator EditMode 테스트
- Player Inventory EditMode 테스트

`ProjectJ.Runtime.asmdef`에는 현재 구현에 필요한 다음 참조가 이미 포함되어 있다.

```text
Unity.InputSystem
Unity.UGUI
```

EditMode 테스트 Assembly도 `ProjectJ.Runtime`을 참조하고 있어 이번 테스트 코드와 Runtime 코드의 Assembly 구조가 일치한다.

저장소 정적 검토 기준으로 개발 진행을 막는 명확한 문제는 확인되지 않았다.

GitHub에는 현재 커밋의 CI 상태 검사가 등록되어 있지 않으므로 다음 항목은 로컬 Unity 환경에서 최종 확인한다.

- Unity Editor Compile Error 0
- Day49_AllSystemsTest Play Mode 실행
- 오른쪽 아래 Inventory Canvas 표시
- Spring Shoes / Jelly Shield 표시
- Q 입력 시 첫 슬롯 선택
- E 입력 시 두 번째 슬롯 선택
- 해상도 변경 후 UI 위치 유지
- ItemDefinitionValidatorTests 통과
- PlayerItemInventoryTests 통과
- Console 반복 Error 및 Exception 없음

---

## 다음 개발 방향

다음 단계에서는 실제 경기에서 아이템을 획득할 수 있도록 아이템 상자와 2슬롯 인벤토리를 연결한다.

핵심 흐름은 다음과 같다.

```text
플레이어가 아이템 상자와 접촉
        ↓
아이템 획득 판정
        ↓
PlayerItemInventory.TryAdd
        ↓
빈 슬롯 우선 저장
        ↓
Inventory Changed
        ↓
Canvas 자동 갱신
```

아이템 상자는 공통 F 상호작용과 분리하고, 접촉 즉시 획득되는 구조를 기준으로 구현한다.
