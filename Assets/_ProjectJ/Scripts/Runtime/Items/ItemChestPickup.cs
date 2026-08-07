using System; // 상자 획득 이벤트 기능 참조
using ProjectJ.Data; // 아이템 공통 데이터 형식 참조
using UnityEngine; // Unity 충돌과 오브젝트 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 아이템 상자 지급 기능 정의
    [DisallowMultipleComponent] // 상자당 획득 컴포넌트 한 개 제한
    [RequireComponent(typeof(BoxCollider))] // 접촉 감지 BoxCollider 필수 지정
    public sealed class ItemChestPickup : MonoBehaviour // 접촉 시 아이템을 지급하는 상자 선언
    { // 접촉·지급·표시 비활성화 기능 정의
        [SerializeField] private ItemDataDefinition itemData; // 상자에서 지급할 아이템 데이터 저장
        [SerializeField] private Collider pickupTrigger; // 플레이어 접촉 감지 Trigger 저장
        [SerializeField] private GameObject visualRoot; // 획득 후 숨길 상자 표시 오브젝트 저장
        [SerializeField] private bool deactivateAfterPickup = true; // 획득 뒤 상자 비활성화 여부 저장
        [SerializeField] private bool logPickupResult = true; // 획득 결과 Console 출력 여부 저장

        private bool isCollected; // 이미 획득된 상자 여부 저장

        public event Action<ItemChestPickup, ItemDataDefinition, int> Collected; // 상자 획득 완료 정보 전달 이벤트

        public ItemDataDefinition ItemData => itemData; // 상자 아이템 데이터 반환
        public bool IsCollected => isCollected; // 상자 획득 완료 여부 반환

        private void Awake() // 실행 시작 시 Trigger 참조 준비
        { // 현재 Collider 자동 연결 준비
            CachePickupTrigger(); // 현재 오브젝트 Collider 자동 연결
        } // 런타임 Trigger 준비 완료

        private void Reset() // 컴포넌트 추가 시 기본 Trigger 구성
        { // 새 상자의 접촉 전용 Collider 준비
            CachePickupTrigger(); // 현재 오브젝트 Collider 자동 연결

            if (pickupTrigger != null) // Collider 존재 여부 확인
            { // 상호작용 전용 Trigger 속성 적용
                pickupTrigger.isTrigger = true; // 접촉 전용 Trigger 설정
            } // Trigger 속성 적용 완료
        } // 기본 상자 구성 완료

        private void OnTriggerEnter(Collider other) // 다른 Collider 접촉 시 아이템 획득 시도
        { // 플레이어 인벤토리 검색과 지급 처리
            if (isCollected || other == null) // 이미 획득했거나 접촉 대상 누락 여부 확인
            { // 중복 또는 무효 접촉 차단
                return; // 접촉 처리 중단
            } // 무효 접촉 처리 완료

            PlayerItemInventory inventory = other.GetComponentInParent<PlayerItemInventory>(); // 접촉 대상 부모에서 플레이어 인벤토리 조회

            if (inventory == null) // 플레이어 인벤토리 누락 여부 확인
            { // 비플레이어 접촉 무시
                return; // 아이템 획득 처리 중단
            } // 비플레이어 접촉 처리 완료

            TryCollect(inventory); // 확인된 플레이어 인벤토리에 아이템 지급 시도
        } // Trigger 획득 처리 완료

        public bool TryCollect(PlayerItemInventory inventory) // 지정 인벤토리에 상자 아이템 지급 시도
        { // 빈 슬롯·중첩·선택 슬롯 교체 규칙 적용
            if (isCollected || inventory == null || itemData == null) // 획득 상태와 필수 참조 확인
            { // 지급 불가 상태 차단
                return false; // 아이템 지급 실패 반환
            } // 필수 조건 검사 완료

            if (!inventory.TryAddOrReplaceSelectedItem(itemData, out int placedSlotIndex, out ItemDataDefinition replacedItem)) // 기획서 기준 추가 또는 선택 슬롯 교체 시도
            { // 예상하지 못한 인벤토리 지급 실패 처리
                LogPickupFailure(); // 실패 원인 확인용 로그 출력
                return false; // 상자를 남긴 채 지급 실패 반환
            } // 인벤토리 지급 실패 처리 완료

            isCollected = true; // 상자 획득 완료 상태 저장
            Collected?.Invoke(this, itemData, placedSlotIndex); // 상자 생성 지점에 획득 완료 정보 전달
            DisablePickupObjects(); // 상자 Trigger와 표시 비활성화
            LogPickupSuccess(placedSlotIndex, replacedItem); // 일반 추가 또는 교체 결과 로그 출력
            return true; // 아이템 지급 성공 반환
        } // 상자 아이템 지급 처리 완료

        public void ConfigureRuntime(ItemDataDefinition newItemData, Collider newPickupTrigger, GameObject newVisualRoot, bool newDeactivateAfterPickup, bool newLogPickupResult) // 런타임 생성 상자 데이터 연결
        { // 런타임 상자 재사용을 위한 상태 초기화
            itemData = newItemData; // 지급 아이템 데이터 저장
            pickupTrigger = newPickupTrigger; // 접촉 Trigger 저장
            visualRoot = newVisualRoot; // 상자 표시 오브젝트 저장
            deactivateAfterPickup = newDeactivateAfterPickup; // 획득 뒤 비활성화 설정 저장
            logPickupResult = newLogPickupResult; // 결과 로그 설정 저장
            isCollected = false; // 획득 전 상태로 초기화

            if (pickupTrigger != null) // 연결된 Trigger 존재 여부 확인
            { // 런타임 Trigger 활성 상태 복원
                pickupTrigger.enabled = true; // 접촉 감지 활성화
                pickupTrigger.isTrigger = true; // 접촉 전용 Trigger 적용
            } // Trigger 재사용 준비 완료

            if (visualRoot != null) // 연결된 표시 오브젝트 존재 여부 확인
            { // 런타임 표시 상태 복원
                visualRoot.SetActive(true); // 상자 표시 활성화
            } // 표시 재사용 준비 완료
        } // 런타임 상자 설정 완료

        private void DisablePickupObjects() // 획득 완료 상자 접촉과 표시 중지
        { // 추가 획득 방지와 시각 제거 처리
            CachePickupTrigger(); // 현재 Collider 참조 보장

            if (pickupTrigger != null) // Trigger 존재 여부 확인
            { // 추가 접촉 감지 중지
                pickupTrigger.enabled = false; // Trigger 비활성화
            } // Trigger 비활성화 완료

            if (visualRoot != null) // 표시 오브젝트 존재 여부 확인
            { // 획득 완료 상자 외형 숨김
                visualRoot.SetActive(false); // 상자 표시 비활성화
            } // 상자 표시 숨김 완료

            if (deactivateAfterPickup) // 상자 전체 비활성화 설정 확인
            { // 런타임 오브젝트 전체 중지
                gameObject.SetActive(false); // 획득 완료 상자 비활성화
            } // 상자 전체 비활성화 완료
        } // 획득 완료 오브젝트 정리 완료

        private void CachePickupTrigger() // 현재 오브젝트 Collider 자동 조회
        { // 직렬화 참조 누락 시 같은 오브젝트에서 복구
            if (pickupTrigger == null) // 저장된 Trigger 누락 여부 확인
            { // Collider 자동 조회 필요 상태
                pickupTrigger = GetComponent<Collider>(); // 현재 오브젝트 Collider 저장
            } // Collider 자동 연결 완료
        } // Trigger 참조 준비 완료

        private void LogPickupSuccess(int placedSlotIndex, ItemDataDefinition replacedItem) // 아이템 획득 성공 또는 교체 로그 출력
        { // Console 결과를 한 줄 형식으로 통일
            if (!logPickupResult) // 로그 출력 비활성화 여부 확인
            { // 불필요한 Console 출력 생략
                return; // 로그 처리 중단
            } // 로그 비활성 처리 완료

            if (replacedItem != null) // 선택 슬롯 교체 발생 여부 확인
            { // 기존 아이템과 새 아이템 교체 결과 출력
                Debug.Log($"[ProjectJ][Day45] 아이템 교체 획득 | 슬롯 {placedSlotIndex + 1}/{PlayerItemInventory.Capacity} | {replacedItem.DisplayName} -> {itemData.DisplayName}", this); // 교체 전후 아이템 이름 출력
                return; // 일반 획득 로그 중복 출력 방지
            } // 교체 획득 로그 완료

            Debug.Log($"[ProjectJ][Day45] 아이템 획득 | {itemData.DisplayName} | 슬롯 {placedSlotIndex + 1}/{PlayerItemInventory.Capacity}", this); // 아이템 이름과 배치 슬롯 출력
        } // 획득 성공 로그 처리 완료

        private void LogPickupFailure() // 예상하지 못한 아이템 지급 실패 로그 출력
        { // 잘못된 데이터 또는 인벤토리 상태 추적 지원
            if (!logPickupResult) // 로그 출력 비활성화 여부 확인
            { // 불필요한 Console 출력 생략
                return; // 로그 처리 중단
            } // 로그 비활성 처리 완료

            string itemName = itemData == null ? "<null>" : itemData.DisplayName; // 누락 데이터까지 안전하게 표시할 이름 계산
            Debug.LogWarning($"[ProjectJ][Day45] 아이템 획득 실패 | 지급 처리 확인 필요: {itemName}", this); // 통합 검증용 실패 경고 출력
        } // 획득 실패 로그 처리 완료

        private void OnDrawGizmosSelected() // Scene 선택 시 상자 접촉 범위 표시
        { // 현재 Collider 범위를 아이템 색상으로 표시
            Collider currentCollider = pickupTrigger != null ? pickupTrigger : GetComponent<Collider>(); // 표시할 Collider 조회

            if (currentCollider == null) // 표시할 Collider 누락 여부 확인
            { // 잘못된 상자 설정에서 기즈모 생략
                return; // 기즈모 처리 중단
            } // 기즈모 생략 처리 완료

            Gizmos.color = itemData != null ? itemData.PickupColor : Color.red; // 아이템 대표 색상 또는 오류 색상 적용
            Gizmos.DrawWireCube(currentCollider.bounds.center, currentCollider.bounds.size); // 상자 접촉 범위 선 표시
        } // 상자 접촉 범위 표시 완료

#if UNITY_EDITOR // Editor 전용 설정 코드 포함 조건
        public void ConfigureForEditor(ItemDataDefinition newItemData, Collider newPickupTrigger, GameObject newVisualRoot, bool newDeactivateAfterPickup, bool newLogPickupResult) // Editor 자동 설정용 상자 데이터 연결
        { // 런타임과 동일한 설정 경로 재사용
            ConfigureRuntime(newItemData, newPickupTrigger, newVisualRoot, newDeactivateAfterPickup, newLogPickupResult); // 런타임 공통 설정 방식으로 참조 연결
        } // Editor 상자 설정 완료
#endif // Editor 전용 설정 코드 제외 경계
    } // 아이템 상자 지급 기능 정의
} // 프로젝트 아이템 기능 정의
