using Fusion; // NetworkBehaviour와 Networked 상태 사용
using ProjectJ.Items; // 기존 ItemDefinition 사용
using UnityEngine; // Unity 기본 타입 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent] // 동일 네트워크 인벤토리 중복 방지
    [RequireComponent(typeof(ProjectJNetworkExternalGameplay))] // 경기 상태 확인 보장
    public sealed class ProjectJNetworkItemInventory :
        NetworkBehaviour
    {
        private const int EmptyItemId = 0; // 비어 있는 슬롯 ID
        private const int SlotCount = 2; // Project J 아이템 슬롯 수

        private ProjectJNetworkExternalGameplay externalGameplay; // 경기 조작 가능 상태 확인

        [Networked] // 첫 번째 슬롯 Item ID 동기화
        private int NetworkSlotLeftItemId
        {
            get;
            set;
        }

        [Networked] // 두 번째 슬롯 Item ID 동기화
        private int NetworkSlotRightItemId
        {
            get;
            set;
        }

        [Networked] // 현재 선택 슬롯 동기화
        private int NetworkSelectedSlotIndex
        {
            get;
            set;
        }

        [Networked] // 인벤토리 변경 횟수 동기화
        private int NetworkInventoryRevision
        {
            get;
            set;
        }

        public int SlotLeftItemId =>
            NetworkSlotLeftItemId; // 첫 번째 슬롯 조회

        public int SlotRightItemId =>
            NetworkSlotRightItemId; // 두 번째 슬롯 조회

        public int SelectedSlotIndex =>
            NetworkSelectedSlotIndex; // 선택 슬롯 조회

        public int SelectedItemId =>
            NetworkSelectedSlotIndex == 0
                ? NetworkSlotLeftItemId
                : NetworkSlotRightItemId; // 선택 아이템 조회

        public int InventoryRevision =>
            NetworkInventoryRevision; // 변경 횟수 조회

        public int OwnerIndex =>
            Object != null && Object.IsValid
                ? Object.InputAuthority.AsIndex
                : -1; // Player Index 조회

        public bool CanReceiveWorldItem =>
            Object != null &&
            Object.IsValid &&
            Object.HasStateAuthority &&
            externalGameplay != null &&
            externalGameplay.GameplayInputAllowed; // Host 승인 가능 여부

        public override void Spawned()
        {
            ResolveReferences(); // 경기 상태 참조 준비

            if (!Object.HasStateAuthority)
            {
                return; // Client 초기 상태 쓰기 차단
            }

            NetworkSlotLeftItemId = EmptyItemId; // 첫 슬롯 초기화
            NetworkSlotRightItemId = EmptyItemId; // 두 번째 슬롯 초기화
            NetworkSelectedSlotIndex = 0; // 첫 슬롯 기본 선택
            NetworkInventoryRevision = 0; // 변경 횟수 초기화
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
            {
                return; // State Authority만 인벤토리 판정
            }

            ResolveReferences(); // 참조 유실 보정

            if (
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed
            )
            {
                return; // 경기 전·완주 후·종료 후 선택 차단
            }

            if (!GetInput<ProjectJNetworkInput>(out ProjectJNetworkInput input))
            {
                return; // 입력 없음 처리
            }

            if (input.Buttons.IsSet(ProjectJNetworkButton.ItemSlotLeft))
            {
                SelectSlotAuthority(0); // Q 첫 슬롯 선택
            }

            if (input.Buttons.IsSet(ProjectJNetworkButton.ItemSlotRight))
            {
                SelectSlotAuthority(1); // E 두 번째 슬롯 선택
            }
        }

        public int GetItemId(int slotIndex)
        {
            if (slotIndex == 0)
            {
                return NetworkSlotLeftItemId; // 첫 슬롯 반환
            }

            if (slotIndex == 1)
            {
                return NetworkSlotRightItemId; // 두 번째 슬롯 반환
            }

            return EmptyItemId; // 잘못된 슬롯 처리
        }

        public bool TryStoreItemAuthority(
            ItemDefinition definition,
            out int storedSlotIndex
        )
        {
            storedSlotIndex = -1; // 실패 기본값

            if (
                !CanReceiveWorldItem ||
                !TryConvertItemId(definition, out int networkItemId)
            )
            {
                return false; // 권한 또는 Item ID 문제 처리
            }

            if (NetworkSlotLeftItemId == EmptyItemId)
            {
                NetworkSlotLeftItemId = networkItemId; // 첫 빈 슬롯 저장
                storedSlotIndex = 0; // 저장 위치 기록
                NetworkInventoryRevision++; // 변경 횟수 증가
                return true;
            }

            if (NetworkSlotRightItemId == EmptyItemId)
            {
                NetworkSlotRightItemId = networkItemId; // 두 번째 빈 슬롯 저장
                storedSlotIndex = 1; // 저장 위치 기록
                NetworkInventoryRevision++; // 변경 횟수 증가
                return true;
            }

            storedSlotIndex = Mathf.Clamp(
                NetworkSelectedSlotIndex,
                0,
                SlotCount - 1
            ); // 가득 찬 경우 현재 선택 슬롯 교체

            SetSlotItemIdAuthority(
                storedSlotIndex,
                networkItemId
            ); // 선택 슬롯 교체

            NetworkInventoryRevision++; // 변경 횟수 증가
            return true;
        }

        public void ClearAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return; // Client 직접 초기화 차단
            }

            NetworkSlotLeftItemId = EmptyItemId; // 첫 슬롯 비우기
            NetworkSlotRightItemId = EmptyItemId; // 두 번째 슬롯 비우기
            NetworkSelectedSlotIndex = 0; // 첫 슬롯 선택 복원
            NetworkInventoryRevision++; // 변경 횟수 증가
        }

        private void SelectSlotAuthority(int slotIndex)
        {
            if (
                slotIndex < 0 ||
                slotIndex >= SlotCount ||
                NetworkSelectedSlotIndex == slotIndex
            )
            {
                return; // 잘못된 값 또는 동일 슬롯 처리
            }

            NetworkSelectedSlotIndex = slotIndex; // 선택 슬롯 확정
            NetworkInventoryRevision++; // 변경 횟수 증가
        }

        private void SetSlotItemIdAuthority(
            int slotIndex,
            int itemId
        )
        {
            if (slotIndex == 0)
            {
                NetworkSlotLeftItemId = itemId; // 첫 슬롯 변경
                return;
            }

            NetworkSlotRightItemId = itemId; // 두 번째 슬롯 변경
        }

        private static bool TryConvertItemId(
            ItemDefinition definition,
            out int networkItemId
        )
        {
            networkItemId = EmptyItemId; // 실패 기본값

            if (
                definition == null ||
                string.IsNullOrWhiteSpace(definition.ItemId)
            )
            {
                return false; // ItemDefinition 누락 처리
            }

            string itemId = definition.ItemId; // 기존 ITM-001 형태 ID 조회
            int parsedValue = 0; // 숫자 누적값
            bool foundDigit = false; // 숫자 존재 여부

            for (int index = 0; index < itemId.Length; index++)
            {
                char value = itemId[index]; // 현재 문자 확인

                if (value < '0' || value > '9')
                {
                    continue; // 숫자가 아니면 건너뜀
                }

                foundDigit = true; // 숫자 확인
                parsedValue =
                    parsedValue * 10 +
                    (value - '0'); // 숫자 ID 누적
            }

            if (!foundDigit || parsedValue <= EmptyItemId)
            {
                return false; // 네트워크 ID 변환 실패
            }

            networkItemId = parsedValue; // 변환 결과 저장
            return true;
        }

        private void ResolveReferences()
        {
            if (externalGameplay == null)
            {
                externalGameplay =
                    GetComponent<ProjectJNetworkExternalGameplay>(); // 경기 상태 컴포넌트 탐색
            }
        }

        private void OnGUI()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasInputAuthority
            )
            {
                return; // 로컬 소유 Player만 표시
            }

            GUILayout.BeginArea(
                new Rect(12f, 660f, 360f, 135f),
                GUI.skin.box
            ); // 72일차 개발 확인 영역

            GUILayout.Label("DAY 72 NETWORK INVENTORY");
            GUILayout.Label(
                "Slot 1 [Q] : " +
                FormatItemId(NetworkSlotLeftItemId)
            );
            GUILayout.Label(
                "Slot 2 [E] : " +
                FormatItemId(NetworkSlotRightItemId)
            );
            GUILayout.Label(
                "Selected : " +
                (NetworkSelectedSlotIndex + 1)
            );
            GUILayout.Label(
                "Revision : " +
                NetworkInventoryRevision
            );

            GUILayout.EndArea();
        }

        private static string FormatItemId(int networkItemId)
        {
            return networkItemId <= EmptyItemId
                ? "Empty"
                : "ITM-" + networkItemId.ToString("000"); // 디버그용 ID 표시
        }
    }
}
