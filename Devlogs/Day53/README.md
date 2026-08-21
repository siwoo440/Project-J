# Project J - 53일차 개발 일지

## 개발 목표

아이템 시스템의 시각 요소를 실제 게임 UI에 연결하기 위해 아이템 PNG 리소스를 Unity Sprite로 정리하고 Sprite Atlas로 묶은 뒤, 대표 아이템 5종의 ItemDefinition에 실제 아이콘을 연결한다.

동시에 기존 가로형 2슬롯 인벤토리 UI를 아이템 이미지를 중심으로 확인하기 쉬운 정사각형 슬롯 UI로 개선한다.

이번 일차의 핵심 흐름은 다음과 같다.

```text
아이템 PNG
    ↓
Sprite Import
    ↓
ItemIcons Sprite Atlas
    ↓
ItemDefinition.Icon
    ↓
PlayerItemInventory
    ↓
Canvas Inventory UI
```

---

## 주요 개발 내용

### 1. 아이템 아이콘 리소스 구조 추가

아이템 UI 리소스를 다음 구조로 정리했다.

```text
Assets/ProjectJ/Art/UI/Items/

├─ Icons/
│  ├─ BalloonHorn.png
│  ├─ BananaCushion.png
│  ├─ JellyShield.png
│  ├─ SpringShoes.png
│  ├─ WaterGun.png
│  └─ 기타 아이템 아이콘
│
└─ Atlases/
   └─ ItemIcons.spriteatlas
```

각 PNG는 Unity에서 `Sprite (2D and UI)` 방식으로 사용할 수 있도록 Import되었다.

---

### 2. 아이템 Sprite Atlas 구성

아이템 아이콘을 하나의 `ItemIcons.spriteatlas`로 묶었다.

Atlas는 개별 아이템 Sprite를 하나의 Texture 묶음으로 관리하면서 UI에서는 계속 각각의 Sprite를 독립적으로 사용할 수 있도록 구성했다.

현재 Atlas의 주요 설정은 다음과 같다.

```text
Atlas Name
ItemIcons

Packing Target
Icons 폴더

Padding
4

Allow Rotation
Off

Tight Packing
Off

Generate Mip Maps
Off

Compression
None

Default Platform Max Texture Size
4096
```

아이콘 폴더 자체를 Packing 대상으로 등록했기 때문에 이후 같은 폴더에 추가되는 Sprite도 동일한 Atlas 관리 구조를 사용할 수 있다.

---

### 3. 대표 아이템 5종 Icon 연결

기존 대표 아이템 5종의 `ItemDefinition.asset`에 실제 PNG Sprite를 연결했다.

```text
Item_SpringShoes
→ 스프링 신발
→ SpringShoes Sprite

Item_JellyShield
→ 젤리 보호막
→ JellyShield Sprite

Item_BananaCushion
→ 바나나 쿠션
→ BananaCushion Sprite

Item_BalloonHorn
→ 풍선 나팔
→ BalloonHorn Sprite

Item_WaterGun
→ 물총
→ WaterGun Sprite
```

내부 Item ID는 기존 값을 그대로 유지했다.

```text
spring_shoes
jelly_shield
banana_cushion
balloon_horn
water_gun
```

따라서 아이템 효과 Registry와 Pickup 시스템의 ID 연결에는 영향을 주지 않는다.

---

### 4. Display Name 한국어화

대표 아이템 5종의 게임 화면 표시 이름을 기획서 기준 한국어 이름으로 변경했다.

```text
Spring Shoes
→ 스프링 신발

Jelly Shield
→ 젤리 보호막

Banana Cushion
→ 바나나 쿠션

Balloon Horn
→ 풍선 나팔

Water Gun
→ 물총
```

내부 코드용 ID와 실제 플레이어에게 보여주는 이름을 분리해 관리한다.

---

### 5. 인벤토리 UI 크기 확대

기존 인벤토리 슬롯은 가로형 구조였다.

```text
기존 슬롯
171 × 82
```

이번 작업에서 아이콘을 크게 확인할 수 있도록 다음과 같이 변경했다.

```text
변경 슬롯
170 × 170
```

각 Q/E 슬롯이 동일한 크기의 정사각형으로 표시된다.

---

### 6. 슬롯 내부 정보 단순화

기존 슬롯 내부에는 다음 정보가 함께 표시되었다.

```text
아이템 이름
Category / UseMode
작은 아이콘
```

예:

```text
Spring Shoes
Mobility / Instant
```

이번 수정에서는 슬롯 안의 이름과 Category / UseMode 텍스트를 제거했다.

현재 슬롯 내부에는 다음 요소만 표시한다.

```text
Q 또는 E 키 표시
실제 아이템 아이콘
```

이를 통해 작은 UI 안에서 텍스트보다 아이템의 시각적 식별을 우선하도록 변경했다.

---

### 7. 아이템 아이콘 표시 영역 확대

기존 아이콘 영역은 약 36×36 크기였다.

이번 수정에서는 170×170 정사각형 슬롯 내부에 10px 여백만 두고 아이콘을 배치한다.

```text
Slot
170 × 170

Icon Padding
10

실제 아이콘 사용 영역
약 150 × 150
```

`preserveAspect`를 유지해 원본 이미지 비율이 찌그러지지 않도록 했다.

---

### 8. 아이템 이름을 슬롯 위로 이동

현재 들고 있는 아이템의 이름은 슬롯 내부가 아니라 각 슬롯 바로 위에 표시하도록 변경했다.

```text
      스프링 신발            물총

┌────────────────┐  ┌────────────────┐
│ Q              │  │ E              │
│                │  │                │
│    아이콘      │  │    아이콘      │
│                │  │                │
└────────────────┘  └────────────────┘
```

긴 아이템 이름은 슬롯 너비 안에서 자동으로 글자 크기가 줄어들도록 구성했다.

---

### 9. 빈 슬롯 표시 변경

아이템이 없는 경우 슬롯 위 이름 영역에 다음 문구를 표시한다.

```text
빈 슬롯
```

아이템을 획득하면 Inventory의 `Changed` 이벤트를 통해 자동으로 실제 Display Name으로 갱신된다.

예:

```text
빈 슬롯
↓
스프링 신발
```

동시에 해당 ItemDefinition에 연결된 실제 PNG Sprite도 슬롯에 표시된다.

---

### 10. Q / E 키 표시 유지 및 가독성 개선

슬롯 선택 입력을 확인할 수 있도록 Q와 E 표시는 유지했다.

아이콘과 겹쳐도 키 문자가 잘 보이도록 작은 반투명 검정 배경을 추가했다.

```text
┌────────────────┐
│ Q              │
│                │
│    아이콘      │
│                │
└────────────────┘
```

Q/E 선택에 따른 기존 슬롯 강조 색상도 그대로 유지한다.

---

## 수정된 주요 파일

```text
Assets/ProjectJ/Runtime/UI/
└─ ItemInventoryCanvasView.cs

Assets/ProjectJ/Data/Items/
├─ Item_SpringShoes.asset
├─ Item_JellyShield.asset
├─ Item_BananaCushion.asset
├─ Item_BalloonHorn.asset
└─ Item_WaterGun.asset
```

---

## 추가된 주요 리소스

```text
Assets/ProjectJ/Art/UI/Items/
├─ Icons/
└─ Atlases/
   └─ ItemIcons.spriteatlas
```

아이템 PNG와 Unity `.meta` 파일이 함께 관리되므로 Sprite GUID가 유지된다.

---

## 구현 결과

이번 작업으로 아이템 데이터와 실제 UI 이미지가 다음 구조로 연결되었다.

```text
Item PNG
↓
Sprite
↓
ItemIcons Sprite Atlas
↓
ItemDefinition.Icon
↓
Inventory Slot
↓
Canvas Image
```

대표 아이템 5종은 실제 아이콘과 한국어 이름을 사용하며, 인벤토리에서는 텍스트 정보보다 큰 아이콘을 중심으로 식별할 수 있게 되었다.

현재 인벤토리 UI 구조는 다음과 같다.

```text
아이템 이름            아이템 이름

┌──────────────┐     ┌──────────────┐
│ Q            │     │ E            │
│              │     │              │
│   큰 아이콘  │     │   큰 아이콘  │
│              │     │              │
└──────────────┘     └──────────────┘
```

---

## 최신 저장소 확인 상태

최신 `main` 커밋 기준으로 다음 항목을 확인했다.

```text
Sprite Atlas 존재
Icons 폴더 Packing 연결
Mip Map Off
Atlas Rotation Off
Atlas Tight Packing Off

대표 5종 Display Name 한국어화
대표 5종 Icon Sprite GUID 연결

인벤토리 슬롯 170 × 170
아이콘 영역 확대
Category / UseMode 텍스트 제거
아이템 이름 슬롯 위 표시
Q / E 표시 유지
빈 슬롯 표시 처리
Inventory Changed 기반 UI 갱신 유지
```

저장소 정적 검토 기준으로 다음 개발을 막는 명확한 문제는 확인되지 않았다.

GitHub에는 CI 상태 검사가 등록되어 있지 않으므로 실제 완료 여부는 Unity에서 다음 조건으로 최종 확인한다.

```text
Unity Compile Error 0
Day49_AllSystemsTest Play Mode 실행

아이템 획득 시 실제 PNG 표시
스프링 신발 이름 표시
젤리 보호막 이름 표시
바나나 쿠션 이름 표시
풍선 나팔 이름 표시
물총 이름 표시

Q / E 선택 강조 정상
슬롯이 정사각형으로 표시
아이콘 비율이 찌그러지지 않음
사용 후 슬롯 EMPTY 상태 갱신
Console 반복 Error / Exception 없음
```

---

## 다음 개발 방향

다음 단계부터는 현재 연결된 대표 아이템 5종의 실제 Effect 구현을 진행한다.

예정 흐름:

```text
스프링 신발
→ 추가 공중 점프

젤리 보호막
→ 플레이어·아이템 방해 Force 방어

바나나 쿠션
→ 설치형 방해 오브젝트

풍선 나팔
→ 전방 다중 밀치기

물총
→ 조준 방향 지속 약한 Force
```

공통 획득·Inventory·UseItem·Sprite UI 구조는 유지하고 각 아이템의 실제 Effect만 독립적으로 추가한다.
