namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        public bool TryBotSelectSlotAuthority(
            int slotIndex
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                (
                    slotIndex != 0 &&
                    slotIndex != 1
                )
            )
            {
                return false; // Bot 슬롯 선택 권한·범위 차단
            }

            SelectSlotAuthority(
                slotIndex
            ); // 기존 Player 슬롯 선택 서버 처리 재사용

            return
                SelectedSlotIndex ==
                slotIndex; // 슬롯 선택 결과 반환
        }

        public bool TryBotUseSelectedItemAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed ||
                SelectedItemId <= 0
            )
            {
                return false; // Bot Item 사용 권한·경기·보유 조건 차단
            }

            if (TryHandleSniperWaterGunUseInputAuthority())
            {
                return true; // 저격 물총 조준 시작 처리
            }

            int successCountBefore =
                NetworkUseSuccessCount; // 사용 전 성공 횟수 저장

            int failCountBefore =
                NetworkUseFailCount; // 사용 전 실패 횟수 저장

            TryUseSelectedItemWithStackAuthority(); // 기존 Stack 포함 Item 사용 처리 재사용

            return
                NetworkUseSuccessCount != successCountBefore ||
                NetworkUseFailCount != failCountBefore; // Item 사용 시도 결과 반환
        }

        public void UpdateBotHeldItemAuthority(
            bool useHeld
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed
            )
            {
                return; // Bot Hold Item 권한·경기 조건 차단
            }

            ProjectJNetworkInput input =
                default; // Bot Hold 가상 입력 초기화

            input.Buttons.Set(
                ProjectJNetworkButton.ItemUseHeld,
                useHeld
            ); // 저격 Hold 입력 설정

            UpdateSniperWaterGunInputAuthority(
                input
            ); // 기존 저격 물총 Hold·발사 처리 재사용

            UpdateWaterGunAuthority(
                useHeld
            ); // 기존 물총 Hold·Release 처리 재사용
        }
    }
}
