# 프로젝트 J — 39일차 개발 일지

---

## 개발 주제

**아이템 공통 데이터 및 2슬롯 인벤토리 구현**

---

## 개발 목표

아이템별 효과를 구현하기 전에 모든 아이템이 공통으로 사용하는 데이터 구조와 플레이어의 아이템 보관 시스템을 구축했다.

플레이어는 아이템을 최대 2개까지 보유할 수 있으며, 아이템 상자와 접촉하면 앞쪽의 빈 슬롯부터 자동으로 아이템을 획득하도록 구성했다.

---

## 주요 개발 내용

### 1. 아이템 공통 데이터 확장

기존 `ItemDataDefinition`을 수정하여 모든 아이템이 공통으로 사용하는 정보를 관리하도록 확장했다.

- 아이템 고유 ID
- 아이템 표시 이름
- 아이템 설명
- 아이템 아이콘
- 아이템 대표 색상
- 아이템 표시 크기

아이템의 실제 효과와 보관 데이터를 분리하여 새로운 아이템을 추가하기 쉬운 구조를 마련했다.

### 2. 2슬롯 인벤토리 구현

`PlayerItemInventory`를 추가하여 플레이어가 아이템을 최대 2개까지 보유하도록 구현했다.

1. 0번 슬롯 확인
2. 0번 슬롯이 차 있으면 1번 슬롯 확인
3. 첫 번째 빈 슬롯에 아이템 배치
4. 두 슬롯이 모두 차 있으면 획득 거부

슬롯이 비워진 경우에는 다시 앞쪽의 빈 슬롯부터 사용한다.

### 3. 아이템 상자 접촉 획득

`ItemChestPickup`을 추가하여 플레이어가 아이템 상자에 접촉하면 자동으로 아이템을 획득하도록 구현했다.

- 접촉 대상의 부모에서 인벤토리 검색
- 인벤토리가 있는 플레이어만 획득 가능
- 획득 성공 시 상자 비활성화
- 인벤토리가 가득 차면 상자 유지
- 중복 획득 처리 방지
- `Player` Tag에 의존하지 않는 구조 적용

### 4. 테스트용 아이템 데이터 추가

| ID | 아이템 | 39일차 구현 범위 |
|---|---|---|
| `ITM-001` | Spring Shoes | 공통 데이터와 인벤토리 보관 |
| `ITM-002` | Jelly Shield | 공통 데이터와 인벤토리 보관 |
| `ITM-003` | Banana Cushion | 공통 데이터와 인벤토리 보관 |

각 아이템의 실제 사용 효과는 이후 개발 일정에서 구현한다.

### 5. 자동 설정 도구 제작

`Day39ItemInventorySetupTool`을 추가하여 다음 작업을 자동으로 처리하도록 구성했다.

- 아이템 데이터 생성 및 갱신
- 데이터 카탈로그 갱신
- 플레이어 인벤토리 컴포넌트 연결
- 테스트용 아이템 상자 3개 생성
- 상자 Trigger와 Rigidbody 설정
- 테스트용 임시 시각 오브젝트 구성

### 6. EditMode 테스트 추가

| 구분 | 검사 내용 |
|---|---|
| 인벤토리 | 첫 슬롯과 두 번째 슬롯 배치 |
| 인벤토리 | 슬롯이 가득 찬 경우 획득 거부 |
| 인벤토리 | 비워진 앞 슬롯 재사용 |
| 인벤토리 | `null` 데이터 획득 방지 |
| 인벤토리 | 아이템 공통 데이터 확인 |
| 아이템 상자 | 아이템 획득 성공 |
| 아이템 상자 | 인벤토리가 가득 찬 경우 상자 유지 |
| 아이템 상자 | 인벤토리가 없는 대상 처리 |

---

## 변경된 주요 파일

### 수정 파일

```text
Assets/_ProjectJ/Scripts/Runtime/Data/Definitions/ItemDataDefinition.cs
Assets/_ProjectJ/Data/Definitions/Item/ITM-001_SpringShoes.asset
Assets/_ProjectJ/Resources/ProjectDataCatalog.asset
Assets/_ProjectJ/Scenes/Game/Game.unity
```

### 신규 파일

```text
Assets/_ProjectJ/Scripts/Runtime/Items/PlayerItemInventory.cs
Assets/_ProjectJ/Scripts/Runtime/Items/ItemChestPickup.cs
Assets/_ProjectJ/Scripts/Editor/Day39ItemInventorySetupTool.cs
Assets/_ProjectJ/Tests/EditMode/PlayerItemInventoryTests.cs
Assets/_ProjectJ/Tests/EditMode/ItemChestPickupTests.cs
Assets/_ProjectJ/Data/Definitions/Item/ITM-002_JellyShield.asset
Assets/_ProjectJ/Data/Definitions/Item/ITM-003_BananaCushion.asset
```

---

## 구현 결과

- 아이템 공통 데이터 구조 확장
- 플레이어 최대 보유 아이템 2개 제한
- 앞쪽 빈 슬롯 우선 배치
- 아이템 상자 접촉 획득 구현
- 획득 성공 시 상자 비활성화
- 인벤토리가 가득 찬 경우 상자 유지
- 아이템 데이터 3종 등록
- 데이터 카탈로그 갱신
- 자동 설정 도구 추가
- EditMode 테스트 9개 추가

---

## 다음 개발 방향

40일차에는 아이템 상자 생성 위치·확률·재생성 규칙과 인벤토리 표시 기반을 구축한다.

- 맵 내부 상자 생성 위치 설정
- 생성 확률 적용
- 상자 중복 생성 방지
- 재생성 조건 구성
- 플레이어 보유 아이템 표시
- 설치형 아이템의 공통 위치 검사 구조 준비

---

## 커밋 정보

```text
39일차 : 아이템 공통 데이터 및 2슬롯 인벤토리 구현
```
