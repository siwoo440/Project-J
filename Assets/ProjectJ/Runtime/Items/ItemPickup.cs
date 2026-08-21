using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Items // 아이템 시스템 네임스페이스
{
    [DisallowMultipleComponent] // 중복 Pickup 방지
    public sealed class ItemPickup : MonoBehaviour // 접촉 즉시 획득되는 월드 아이템
    {
        [SerializeField] // 인스펙터 직렬화
        private ItemDefinition itemDefinition; // 획득할 아이템 데이터

        [SerializeField] // 인스펙터 직렬화
        private Collider pickupTrigger; // 접촉 판정 Trigger

        [SerializeField] // 인스펙터 직렬화
        private GameObject visualRoot; // 획득 전 표시할 외형

        [SerializeField] // 인스펙터 직렬화
        private bool destroyOnCollect = true; // 획득 후 오브젝트 제거 여부

        private bool collected; // 중복 획득 방지 상태

        public ItemDefinition Definition // 현재 아이템 데이터 조회
        {
            get
            {
                return itemDefinition; // 아이템 데이터 반환
            }
        }

        public bool IsCollected // 현재 획득 완료 여부
        {
            get
            {
                return collected; // 획득 상태 반환
            }
        }

        public void Configure( // Editor Setup과 테스트용 구성
            ItemDefinition definition,
            Collider trigger,
            GameObject visual,
            bool shouldDestroyOnCollect = true
        )
        {
            itemDefinition = definition; // 아이템 데이터 저장
            pickupTrigger = trigger; // Trigger 저장
            visualRoot = visual; // 외형 Root 저장
            destroyOnCollect = shouldDestroyOnCollect; // 제거 규칙 저장
            collected = false; // 초기 획득 상태로 복원
        }

        public bool TryCollect(GameObject collector) // 지정 플레이어가 아이템 획득 시도
        {
            if (collected || collector == null) // 중복 또는 잘못된 Collector 검사
            {
                return false; // 획득 실패 반환
            }

            if (itemDefinition == null) // ItemDefinition 누락 검사
            {
                return false; // 잘못된 Pickup 차단
            }

            if (!itemDefinition.IsDefinitionValid(out string errorMessage)) // 아이템 데이터 유효성 검사
            {
                Debug.LogWarning(
                    $"ItemPickup '{name}' 데이터 오류: {errorMessage}",
                    this
                ); // 잘못된 데이터 경고 출력

                return false; // 획득 차단
            }

            PlayerItemInventory inventory =
                collector.GetComponentInParent<PlayerItemInventory>(); // Player 또는 자식 Collider에서 Inventory 탐색

            if (inventory == null) // Inventory 존재 검사
            {
                return false; // 플레이어가 아니면 획득하지 않음
            }

            bool stored =
                inventory.TryAdd(itemDefinition, out int storedSlotIndex); // 기존 2슬롯 규칙으로 저장

            if (!stored) // Inventory 저장 실패 검사
            {
                return false; // 획득 상태 유지
            }

            collected = true; // 첫 성공 즉시 중복 획득 차단

            if (pickupTrigger != null) // Trigger 존재 검사
            {
                pickupTrigger.enabled = false; // 추가 Trigger 진입 차단
            }

            if (visualRoot != null) // 외형 존재 검사
            {
                visualRoot.SetActive(false); // 획득 즉시 화면에서 숨김
            }

            Debug.Log(
                $"Item 획득: {itemDefinition.DisplayName} → Slot {storedSlotIndex + 1}",
                collector
            ); // 획득 결과 확인 로그

            if (destroyOnCollect && Application.isPlaying) // 실제 Play Mode 제거 여부 검사
            {
                Destroy(gameObject); // Frame 종료 시 Pickup 제거
            }

            return true; // 획득 성공 반환
        }

        private void Reset() // 컴포넌트 최초 추가 시 기본 참조 탐색
        {
            pickupTrigger = GetComponent<Collider>(); // Root Collider 자동 연결
            visualRoot = transform.childCount > 0 // 첫 자식이 있으면 외형으로 사용
                ? transform.GetChild(0).gameObject
                : null;
        }

        private void OnTriggerEnter(Collider other) // Unity Trigger 접촉 처리
        {
            TryCollect(other.gameObject); // 실제 획득 시도
        }
    }
}
