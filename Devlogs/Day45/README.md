# Project J 개발 일지

## 45일차 : 아이템 28종 통합 테스트 및 밸런스 검증

### 개발 목표

42~44일차에 구현한 아이템 28종을 하나의 완성된 아이템 시스템으로 통합하고, 데이터 구조·획득·보관·소비·교체·효과 초기화·테스트 환경을 점검한다.

이번 일차에서는 신규 아이템 효과를 추가하기보다 기존 28종이 서로 충돌하지 않고 정상적으로 사용할 수 있는지 검증하고, 이후 밸런스 조정을 진행할 수 있는 기준 데이터를 마련하는 데 집중했다.

---

### 주요 작업 내용

#### 1. 아이템 28종 통합 검증 도구 추가

Unity Editor에서 현재 구현된 아이템 데이터를 한 번에 검사할 수 있는 통합 검증 도구를 추가했다.

검증 항목은 다음과 같다.

- 아이템 데이터가 정확히 28개 존재하는지 확인
- `ITM-001`부터 `ITM-028`까지 ID 누락 여부 확인
- 중복된 `DataId` 존재 여부 확인
- 중복된 `ItemEffectType` 존재 여부 확인
- Runtime `ItemEffectType`이 정확히 28종인지 확인
- P0 아이템 10종 확인
- P1 아이템 11종 확인
- P2 아이템 7종 확인
- 모든 아이템의 `SpawnWeight`가 0보다 큰지 확인
- 모든 아이템의 `MaximumStackCount`가 1 이상인지 확인
- 전체 등장 가중치 합계가 정상인지 확인
- 아이콘이 없는 아이템을 경고로 표시

Unity 상단 메뉴에 다음 기능을 추가했다.

```text
Project J
└─ Day 45
   ├─ Validate 28 Item Integration
   └─ Export Item Balance Baseline CSV
```

---

#### 2. 아이템 밸런스 기준 CSV 출력 기능 추가

현재 아이템 28종의 주요 수치를 CSV 파일로 출력할 수 있도록 구현했다.

출력 경로:

```text
Assets/_ProjectJ/Documentation/Day45_ItemBalanceBaseline.csv
```

CSV에는 다음 정보를 기록한다.

- DataId
- DisplayName
- Priority
- UseType
- EffectType
- SpawnWeight
- SpawnProbability
- MaximumStackCount
- EffectDuration
- PrimaryValue
- SecondaryValue
- EffectRange
- EffectRadius
- Cooldown
- ProjectileSpeed
- ManualResult
- Notes

기존 CSV가 존재하는 경우 실수로 수동 테스트 기록을 덮어쓰지 않도록 확인 창을 표시하도록 구성했다.

---

#### 3. 가득 찬 인벤토리의 아이템 획득 규칙 수정

기존 인벤토리는 두 슬롯이 모두 차 있는 상태에서 새로운 아이템을 획득하면 획득에 실패하도록 되어 있었다.

기획 규칙에 맞춰 상자 획득 전용 동작을 다음과 같이 수정했다.

```text
같은 아이템 중첩 가능
→ 기존 슬롯에 중첩

빈 슬롯 존재
→ 첫 빈 슬롯에 추가

두 슬롯 모두 사용 중
→ 현재 선택된 슬롯의 아이템을 새 아이템으로 교체
```

이를 위해 `PlayerItemInventory`에 상자 획득용 추가·교체 기능을 구현했다.

추가 기능:

```text
TryAddOrReplaceSelectedItem
```

기존 `TryAddItem`은 다른 시스템과의 호환성을 위해 기존 동작을 유지했다.

---

#### 4. 아이템 상자 획득 처리 갱신

`ItemChestPickup`이 새로운 인벤토리 교체 규칙을 사용하도록 수정했다.

이제 인벤토리가 가득 차 있어도 현재 선택 슬롯을 교체하여 아이템을 정상 획득한다.

교체가 발생한 경우 Console에서 다음 정보를 확인할 수 있도록 로그를 구분했다.

```text
아이템 교체 획득
슬롯 번호
기존 아이템
새 아이템
```

정상 획득 이후에는 기존과 동일하게 상자의 Trigger와 표시 오브젝트를 비활성화한다.

---

#### 5. EditMode 통합 테스트 추가

아이템 시스템의 핵심 규칙이 이후 작업에서 다시 깨지는 것을 방지하기 위해 45일차 전용 EditMode 테스트를 추가했다.

검증 내용:

- 아이템 데이터 28종 확인
- DataId 28종 고유성 확인
- ItemEffectType 28종 고유성 확인
- P0 / P1 / P2 아이템 개수 확인
- 등장 가중치 정상 여부 확인
- 가중치 선택 범위 정상 여부 확인
- 인벤토리 가득 참 상태의 선택 슬롯 교체 확인
- 중첩 아이템 수량 감소 확인
- 마지막 수량 소비 후 슬롯 초기화 확인

---

#### 6. PlayMode 통합 테스트 추가

실제 Runtime GameObject를 사용하는 PlayMode 테스트도 추가했다.

주요 검증 내용:

- 두 슬롯이 가득 찬 상태에서 아이템 상자 획득
- 현재 선택 슬롯이 새로운 아이템으로 교체되는지 확인
- 상자 획득 완료 상태 확인
- 아이템 소비 후 슬롯 데이터 정상 여부 확인
- `ClearInventory()` 실행 후 잔여 슬롯 상태가 남지 않는지 확인

---

#### 7. 기존 ItemChestPickup 회귀 테스트 갱신

45일차 인벤토리 규칙을 적용한 이후 기존 `ItemChestPickupTests`에서 테스트 1개가 실패했다.

원인을 확인한 결과 Runtime 코드 문제가 아니라 41일차에 작성했던 기존 테스트가 이전 규칙을 계속 검사하고 있었다.

기존 테스트 규칙:

```text
인벤토리가 가득 참
→ 아이템 획득 실패
→ 상자 유지
```

현재 규칙:

```text
인벤토리가 가득 참
→ 현재 선택 슬롯 교체
→ 아이템 획득 성공
→ 상자 소비
```

따라서 기존 테스트를 현재 기획 규칙에 맞게 갱신했다.

수정된 테스트는 다음 내용을 검증한다.

- 첫 슬롯 기존 아이템 유지
- 선택한 둘째 슬롯만 새 아이템으로 교체
- 인벤토리 점유 슬롯 수 2개 유지
- 교체된 아이템 수량 1개
- 상자 획득 성공
- 상자 획득 완료 상태 적용
- 획득한 상자 비활성화

---

#### 8. TextMeshPro 구형 API 경고 제거

Unity 최신 TextMeshPro API에서 다음 속성이 폐기 예정으로 변경되어 CS0618 경고가 발생했다.

```text
TMP_Text.enableWordWrapping
```

발생 파일:

```text
Assets/_ProjectJ/Scripts/Runtime/UI/System/FatalErrorScreen.cs
Assets/_ProjectJ/Scripts/Editor/Day42ItemSystemSetupTool.cs
Assets/_ProjectJ/Scripts/Editor/Day40CanvasUISetupTool.cs
```

기존 API를 최신 `textWrappingMode` 방식으로 교체했다.

자동 줄바꿈 사용:

```text
TextWrappingModes.Normal
```

자동 줄바꿈 미사용:

```text
TextWrappingModes.NoWrap
```

기존 UI 표시 동작은 유지하면서 CS0618 경고를 제거했다.

---

### 이번 일차 주요 수정 파일

```text
Assets/_ProjectJ/Scripts/Runtime/Items/PlayerItemInventory.cs
Assets/_ProjectJ/Scripts/Runtime/Items/ItemChestPickup.cs

Assets/_ProjectJ/Scripts/Editor/Day45ItemIntegrationValidationTool.cs
Assets/_ProjectJ/Scripts/Editor/Day42ItemSystemSetupTool.cs
Assets/_ProjectJ/Scripts/Editor/Day40CanvasUISetupTool.cs

Assets/_ProjectJ/Scripts/Runtime/UI/System/FatalErrorScreen.cs

Assets/_ProjectJ/Tests/EditMode/Day45ItemIntegrationTests.cs
Assets/_ProjectJ/Tests/EditMode/ItemChestPickupTests.cs
Assets/_ProjectJ/Tests/PlayMode/Day45ItemIntegrationPlayModeTests.cs

Assets/_ProjectJ/Documentation/Day45_ItemBalanceBaseline.csv
```

---

### 테스트 결과

EditMode Test Runner에서 45일차 통합 테스트와 기존 아이템 테스트를 함께 실행했다.

초기 테스트에서는 기존 `ItemChestPickupTests`의 구형 규칙 때문에 295개 테스트 중 1개가 실패했다.

```text
전체 테스트 : 295
실패 : 1
```

실패 원인을 확인한 뒤 기존 회귀 테스트를 현재 선택 슬롯 교체 규칙으로 수정했다.

최종적으로 다음 항목을 확인했다.

- 아이템 28종 데이터 구조 검증
- P0 10종 / P1 11종 / P2 7종 구성 확인
- 아이템 등장 가중치 검증
- 두 슬롯 인벤토리 추가와 교체 규칙 검증
- 중첩 아이템 소비 검증
- 상자 획득 규칙 검증
- Runtime 인벤토리 초기화 검증
- TextMeshPro obsolete API 경고 제거

---

### 45일차 완료 결과

이번 일차를 통해 42~44일차에 개별적으로 구현한 아이템 28종을 하나의 통합 시스템 기준으로 검증할 수 있는 기반을 마련했다.

특히 아이템 획득 규칙과 기존 테스트 규칙의 충돌을 발견해 최신 기획 기준으로 통일했고, 이후 기능 수정 시 회귀 오류를 빠르게 발견할 수 있도록 EditMode와 PlayMode 테스트를 보강했다.

또한 밸런스 기준 CSV를 통해 이후 실제 플레이 테스트 결과를 기록하고 수치를 비교할 수 있는 기반을 준비했다.

현재 아이템 시스템은 다음 개발 단계에서 기능별 Editor 메뉴와 스크립트 구조를 정리할 수 있는 상태가 되었다.

---

### 다음 개발 방향

46일차에는 Unity 상단의 `Project J/Day XX/...` 형태로 흩어져 있는 Editor 메뉴를 실제 기능 기준으로 재분류한다.

주요 방향:

- 프로젝트 설정
- 플레이어와 입력
- 데이터
- 테스트
- 빌드
- 맵
- 장애물
- 아이템
- UI

기존 스크립트와 클래스 이름은 유지하고 `[MenuItem]` 경로만 기능 중심의 한글 메뉴 구조로 정리한다.

추가로 프로젝트 전체 기능을 한눈에 확인할 수 있는 한국어 기능 분류 문서를 작성한다.

---

### GitHub 커밋

```text
45일차 : 아이템 28종 통합 테스트 및 밸런스 검증
```
