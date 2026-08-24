using Fusion; // NetworkBehaviour와 NetworkObject 사용
using ProjectJ.Items; // 기존 ItemPickup 사용
using UnityEngine; // Unity 기본 타입 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent] // 동일 네트워크 상자 중복 방지
    [RequireComponent(typeof(NetworkObject))] // Fusion Scene Object 보장
    [RequireComponent(typeof(ItemPickup))] // 기존 ItemDefinition 데이터 재사용
    [RequireComponent(typeof(BoxCollider))] // 접촉 Trigger 보장
    [RequireComponent(typeof(Rigidbody))] // Trigger 이벤트 수신 보장
    public sealed class ProjectJNetworkItemBox :
        NetworkBehaviour
    {
        private ItemPickup legacyPickup; // 기존 ItemDefinition 보관용 Pickup
        private BoxCollider pickupTrigger; // 네트워크 획득 Trigger
        private Rigidbody body; // Trigger 이벤트용 Rigidbody
        private Renderer[] visualRenderers; // 획득 전 표시 Renderer
        private ProjectJNetworkItemInventory pendingCollector; // 다음 Fusion Tick 처리 대상

        [Networked] // 상자 획득 여부 동기화
        private NetworkBool NetworkCollected
        {
            get;
            set;
        }

        [Networked] // 획득 Player Index 동기화
        private int NetworkCollectorIndex
        {
            get;
            set;
        }

        [Networked] // 지급 Item ID 동기화
        private int NetworkAwardedItemId
        {
            get;
            set;
        }

        [Networked] // 저장된 슬롯 번호 동기화
        private int NetworkStoredSlotIndex
        {
            get;
            set;
        }

        public bool IsCollected =>
            NetworkCollected; // 획득 여부 조회

        public int CollectorIndex =>
            NetworkCollectorIndex; // 획득 Player 조회

        public int AwardedItemId =>
            NetworkAwardedItemId; // 지급 Item 조회

        public int StoredSlotIndex =>
            NetworkStoredSlotIndex; // 저장 슬롯 조회

        private void Awake()
        {
            ResolveReferences(); // 기존 Pickup과 물리 참조 준비

            if (legacyPickup != null)
            {
                legacyPickup.enabled = false; // 로컬 즉시 획득 로직 차단
            }

            if (pickupTrigger != null)
            {
                pickupTrigger.isTrigger = true; // 네트워크 획득 Trigger로 사용
            }

            if (body != null)
            {
                body.isKinematic = true; // 월드 상자 위치 고정
                body.useGravity = false; // 중력 비활성화
            }
        }

        public override void Spawned()
        {
            ResolveReferences(); // Spawn 이후 참조 보정

            if (Object.HasStateAuthority)
            {
                NetworkCollected = false; // 최초 미획득 상태
                NetworkCollectorIndex = -1; // 획득자 없음
                NetworkAwardedItemId = 0; // 지급 Item 없음
                NetworkStoredSlotIndex = -1; // 저장 슬롯 없음
            }

            ApplyCollectedPresentation(); // 현재 네트워크 상태 반영
        }

        public override void FixedUpdateNetwork()
        {
            if (
                !Object.HasStateAuthority ||
                NetworkCollected ||
                pendingCollector == null
            )
            {
                return; // Host의 대기 중 획득 요청만 처리
            }

            ProjectJNetworkItemInventory collector =
                pendingCollector; // 이번 Tick 처리 대상 복사

            pendingCollector = null; // 다음 요청을 받을 수 있도록 초기화

            if (!collector.CanReceiveWorldItem)
            {
                return; // 경기 상태 또는 권한 검증 실패
            }

            ResolveReferences(); // ItemDefinition 참조 보정

            if (
                legacyPickup == null ||
                legacyPickup.Definition == null
            )
            {
                return; // 지급 데이터 누락 처리
            }

            bool stored = collector.TryStoreWorldItemAuthority(
                legacyPickup.Definition,
                out int storedSlotIndex
            ); // State Authority 인벤토리 저장

            if (!stored)
            {
                return; // 저장 실패 시 상자 유지
            }

            NetworkCollected = true; // 첫 성공으로 상자 잠금
            NetworkCollectorIndex = collector.OwnerIndex; // 획득자 저장
            NetworkAwardedItemId = collector.GetItemId(storedSlotIndex); // 지급 ID 저장
            NetworkStoredSlotIndex = storedSlotIndex; // 저장 슬롯 저장

            ApplyCollectedPresentation(); // Host 즉시 외형 제거

            Debug.Log(
                "[Project J/Fusion] 73일차 Item Box 획득 / P" +
                NetworkCollectorIndex +
                " / " +
                ProjectJNetworkItemCatalog.GetKey(NetworkAwardedItemId) +
                " / Slot " +
                (NetworkStoredSlotIndex + 1),
                this
            );
        }

        private void Update()
        {
            ApplyCollectedPresentation(); // Proxy도 Networked 획득 상태 표현
        }

        private void OnTriggerEnter(Collider other)
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                NetworkCollected ||
                other == null
            )
            {
                return; // Host만 획득 후보 수집
            }

            ProjectJNetworkItemInventory collector =
                other.GetComponentInParent<ProjectJNetworkItemInventory>(); // Network Player Inventory 탐색

            if (collector == null)
            {
                return; // Player가 아니면 무시
            }

            if (
                pendingCollector == null ||
                collector.OwnerIndex < pendingCollector.OwnerIndex
            )
            {
                pendingCollector = collector; // 동일 시점 경쟁은 낮은 Player Index 우선
            }
        }

        private void ApplyCollectedPresentation()
        {
            bool collected = NetworkCollected; // 현재 상태 복사

            if (pickupTrigger != null)
            {
                pickupTrigger.enabled = !collected; // 획득 후 재접촉 차단
            }

            if (visualRenderers == null)
            {
                return; // Renderer 없음 처리
            }

            for (int index = 0; index < visualRenderers.Length; index++)
            {
                Renderer visual = visualRenderers[index]; // 현재 Renderer 조회

                if (visual != null)
                {
                    visual.enabled = !collected; // 모든 Peer에서 동일 표시
                }
            }
        }

        private void ResolveReferences()
        {
            if (legacyPickup == null)
            {
                legacyPickup = GetComponent<ItemPickup>(); // 기존 ItemDefinition 사용
            }

            if (pickupTrigger == null)
            {
                pickupTrigger = GetComponent<BoxCollider>(); // Trigger 조회
            }

            if (body == null)
            {
                body = GetComponent<Rigidbody>(); // Rigidbody 조회
            }

            if (visualRenderers == null || visualRenderers.Length == 0)
            {
                visualRenderers = GetComponentsInChildren<Renderer>(true); // 상자 외형 전체 조회
            }
        }
    }
}
