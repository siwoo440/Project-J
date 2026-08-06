using ProjectJ.Data; // 아이템 공통 데이터 형식 참조
using UnityEngine; // Unity 충돌과 오브젝트 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 상자당 획득 컴포넌트 한 개만 허용
    [RequireComponent(typeof(BoxCollider))] // 접촉 감지 BoxCollider 필수 지정
    public sealed class ItemChestPickup : MonoBehaviour // 접촉 시 아이템을 지급하는 상자 선언
    { // 아이템 상자 획득 기능 묶음
        [SerializeField] private ItemDataDefinition itemData; // 상자에서 지급할 아이템 데이터 저장
        [SerializeField] private Collider pickupTrigger; // 플레이어 접촉 감지 Trigger 저장
        [SerializeField] private GameObject visualRoot; // 획득 후 숨길 상자 표시 오브젝트 저장
        [SerializeField] private bool deactivateAfterPickup = true; // 획득 뒤 상자 비활성화 여부 저장
        [SerializeField] private bool logPickupResult = true; // 획득 결과 Console 출력 여부 저장

        private bool isCollected; // 이미 획득된 상자 여부 저장

        public ItemDataDefinition ItemData => itemData; // 상자 아이템 데이터 반환
        public bool IsCollected => isCollected; // 상자 획득 완료 여부 반환

        private void Awake() // 실행 시작 시 Trigger 참조 준비
        { // Trigger 참조 준비 처리
            CachePickupTrigger(); // 현재 오브젝트 Collider 자동 연결
        } // Trigger 참조 준비 처리 종료

        private void Reset() // 컴포넌트 추가 시 기본 Trigger 구성
        { // 기본 Trigger 구성 처리
            CachePickupTrigger(); // 현재 오브젝트 Collider 자동 연결

            if (pickupTrigger != null) // Collider 존재 여부 확인
            { // Trigger 속성 적용 처리
                pickupTrigger.isTrigger = true; // 접촉 전용 Trigger로 설정
            } // Trigger 속성 적용 처리 종료
        } // 기본 Trigger 구성 처리 종료

        private void OnTriggerEnter(Collider other) // 다른 Collider 접촉 시 아이템 획득 시도
        { // 접촉 아이템 획득 처리
            if (isCollected || other == null) // 이미 획득했거나 접촉 대상 누락 여부 확인
            { // 무시할 접촉 처리
                return; // 접촉 처리 중단
            } // 무시할 접촉 처리 종료

            PlayerItemInventory inventory = other.GetComponentInParent<PlayerItemInventory>(); // 접촉 대상 부모에서 플레이어 인벤토리 조회

            if (inventory == null) // 플레이어 인벤토리 누락 여부 확인
            { // 비플레이어 접촉 처리
                return; // 아이템 획득 처리 중단
            } // 비플레이어 접촉 처리 종료

            TryCollect(inventory); // 확인된 플레이어 인벤토리에 아이템 지급 시도
        } // 접촉 아이템 획득 처리 종료

        public bool TryCollect(PlayerItemInventory inventory) // 지정 인벤토리에 상자 아이템 지급 시도
        { // 상자 아이템 지급 처리
            if (isCollected || inventory == null || itemData == null) // 획득 상태와 필수 참조 확인
            { // 지급 불가 상태 처리
                return false; // 아이템 지급 실패 반환
            } // 지급 불가 상태 처리 종료

            if (!inventory.TryAddItem(itemData, out int placedSlotIndex)) // 첫 빈 슬롯 아이템 추가 시도
            { // 인벤토리 가득 참 처리
                LogInventoryFull(); // 가득 찬 인벤토리 로그 출력
                return false; // 상자를 남긴 채 지급 실패 반환
            } // 인벤토리 가득 참 처리 종료

            isCollected = true; // 상자 획득 완료 상태 저장
            DisablePickupObjects(); // 상자 Trigger와 표시 비활성화
            LogPickupSuccess(placedSlotIndex); // 획득 성공 로그 출력
            return true; // 아이템 지급 성공 반환
        } // 상자 아이템 지급 처리 종료

        private void DisablePickupObjects() // 획득 완료 상자 접촉과 표시 중지
        { // 상자 비활성화 처리
            CachePickupTrigger(); // 현재 Collider 참조 보장

            if (pickupTrigger != null) // Trigger 존재 여부 확인
            { // Trigger 비활성화 처리
                pickupTrigger.enabled = false; // 추가 접촉 감지 중지
            } // Trigger 비활성화 처리 종료

            if (visualRoot != null) // 표시 오브젝트 존재 여부 확인
            { // 표시 오브젝트 숨김 처리
                visualRoot.SetActive(false); // 상자 표시 비활성화
            } // 표시 오브젝트 숨김 처리 종료

            if (deactivateAfterPickup) // 상자 전체 비활성화 설정 확인
            { // 상자 전체 비활성화 처리
                gameObject.SetActive(false); // 획득 완료 상자 비활성화
            } // 상자 전체 비활성화 처리 종료
        } // 상자 비활성화 처리 종료

        private void CachePickupTrigger() // 현재 오브젝트 Collider 자동 조회
        { // Collider 참조 준비 처리
            if (pickupTrigger == null) // 저장된 Trigger 누락 여부 확인
            { // Trigger 자동 조회 처리
                pickupTrigger = GetComponent<Collider>(); // 현재 오브젝트 Collider 저장
            } // Trigger 자동 조회 처리 종료
        } // Collider 참조 준비 처리 종료

        private void LogPickupSuccess(int placedSlotIndex) // 아이템 획득 성공 로그 출력
        { // 획득 성공 로그 처리
            if (!logPickupResult) // 로그 출력 비활성화 여부 확인
            { // 로그 생략 처리
                return; // 로그 처리 중단
            } // 로그 생략 처리 종료

            Debug.Log($"[ProjectJ][Day39] 아이템 획득 | {itemData.DisplayName} | 슬롯 {placedSlotIndex + 1}/{PlayerItemInventory.Capacity}", this); // 아이템 이름과 배치 슬롯 출력
        } // 획득 성공 로그 처리 종료

        private void LogInventoryFull() // 인벤토리 가득 참 로그 출력
        { // 인벤토리 가득 참 로그 처리
            if (!logPickupResult) // 로그 출력 비활성화 여부 확인
            { // 로그 생략 처리
                return; // 로그 처리 중단
            } // 로그 생략 처리 종료

            Debug.Log($"[ProjectJ][Day39] 아이템 획득 실패 | 2개 슬롯 사용 중 | 상자 유지: {itemData.DisplayName}", this); // 가득 참 상태와 상자 유지 안내 출력
        } // 인벤토리 가득 참 로그 처리 종료

        private void OnDrawGizmosSelected() // Scene 선택 시 상자 접촉 범위 표시
        { // 상자 기즈모 표시 처리
            Collider currentCollider = pickupTrigger != null ? pickupTrigger : GetComponent<Collider>(); // 표시할 Collider 조회

            if (currentCollider == null) // 표시할 Collider 누락 여부 확인
            { // 기즈모 생략 처리
                return; // 기즈모 처리 중단
            } // 기즈모 생략 처리 종료

            Gizmos.color = itemData != null ? itemData.PickupColor : Color.red; // 아이템 대표 색상 또는 오류 색상 적용
            Gizmos.DrawWireCube(currentCollider.bounds.center, currentCollider.bounds.size); // 상자 접촉 범위 선 표시
        } // 상자 기즈모 표시 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(ItemDataDefinition newItemData, Collider newPickupTrigger, GameObject newVisualRoot, bool newDeactivateAfterPickup, bool newLogPickupResult) // Editor 자동 설정용 상자 데이터 연결
        { // Editor 상자 설정 처리
            itemData = newItemData; // 지급 아이템 데이터 저장
            pickupTrigger = newPickupTrigger; // 접촉 Trigger 저장
            visualRoot = newVisualRoot; // 상자 표시 오브젝트 저장
            deactivateAfterPickup = newDeactivateAfterPickup; // 획득 뒤 비활성화 설정 저장
            logPickupResult = newLogPickupResult; // 결과 로그 설정 저장
            isCollected = false; // 획득 전 상태로 초기화
        } // Editor 상자 설정 처리 종료
#endif // Editor 전용 설정 종료
    } // 아이템 상자 획득 기능 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
