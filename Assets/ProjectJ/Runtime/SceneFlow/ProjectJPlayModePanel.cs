using UnityEngine; // MonoBehaviour와 Debug 사용
using UnityEngine.UI; // 상세 정보와 SELECT 버튼 사용

namespace ProjectJ
{
    [DisallowMultipleComponent]
    public sealed class ProjectJPlayModePanel :
        MonoBehaviour
    {
        [SerializeField]
        private ProjectJGameModeCard[] cards;

        [SerializeField]
        private Text detailTitleText;

        [SerializeField]
        private Text detailDescriptionText;

        [SerializeField]
        private Text detailStatusText;

        [SerializeField]
        private Button selectButton;

        [SerializeField]
        private Text selectButtonText;

        private ProjectJGameModeCard
            selectedCard;

        public ProjectJGameModeCard
            SelectedCard =>
                selectedCard;

        public void Configure(
            ProjectJGameModeCard[] modeCards,
            Text detailTitle,
            Text detailDescription,
            Text detailStatus,
            Button confirmButton,
            Text confirmButtonText
        )
        {
            cards =
                modeCards;

            detailTitleText =
                detailTitle;

            detailDescriptionText =
                detailDescription;

            detailStatusText =
                detailStatus;

            selectButton =
                confirmButton;

            selectButtonText =
                confirmButtonText;
        }

        private void Awake()
        {
            if (selectButton != null)
            {
                selectButton.onClick
                    .AddListener(
                        ConfirmSelection
                    );
            }

            ClearSelection();
        }

        private void OnDestroy()
        {
            if (selectButton != null)
            {
                selectButton.onClick
                    .RemoveListener(
                        ConfirmSelection
                    );
            }
        }

        public void SelectCard(
            ProjectJGameModeCard card
        )
        {
            if (card == null)
            {
                return;
            }

            selectedCard =
                card;

            if (cards != null)
            {
                foreach (
                    ProjectJGameModeCard
                        modeCard in cards
                )
                {
                    if (modeCard != null)
                    {
                        modeCard.SetSelected(
                            modeCard ==
                            selectedCard
                        );
                    }
                }
            }

            RefreshDetail();
        }

        public void ClearSelection()
        {
            selectedCard =
                null;

            if (cards != null)
            {
                foreach (
                    ProjectJGameModeCard
                        modeCard in cards
                )
                {
                    if (modeCard != null)
                    {
                        modeCard.SetSelected(
                            false
                        );
                    }
                }
            }

            if (detailTitleText != null)
            {
                detailTitleText.text =
                    "게임 모드를 선택하세요";
            }

            if (
                detailDescriptionText != null
            )
            {
                detailDescriptionText.text =
                    "카드에 마우스를 올리면 강조되고, 클릭하면 선택 상태가 유지됩니다.";
            }

            if (detailStatusText != null)
            {
                detailStatusText.text =
                    string.Empty;
            }

            if (selectButton != null)
            {
                selectButton.interactable =
                    false;
            }

            if (selectButtonText != null)
            {
                selectButtonText.text =
                    "SELECT";
            }
        }

        private void RefreshDetail()
        {
            if (selectedCard == null)
            {
                ClearSelection();
                return;
            }

            if (detailTitleText != null)
            {
                detailTitleText.text =
                    selectedCard.DisplayName;
            }

            if (
                detailDescriptionText != null
            )
            {
                detailDescriptionText.text =
                    selectedCard.Description;
            }

            bool comingSoon =
                selectedCard.ComingSoon;

            if (detailStatusText != null)
            {
                detailStatusText.text =
                    comingSoon
                        ? "COMING SOON"
                        : "선택 가능";
            }

            if (selectButton != null)
            {
                selectButton.interactable =
                    !comingSoon;
            }

            if (selectButtonText != null)
            {
                selectButtonText.text =
                    comingSoon
                        ? "COMING SOON"
                        : "SELECT";
            }
        }

        private void ConfirmSelection()
        {
            if (
                selectedCard == null ||
                selectedCard.ComingSoon
            )
            {
                return;
            }

            if (
                selectedCard.ModeId ==
                ProjectJGameModeId.PrivateMatch
            )
            {
                Debug.Log(
                    "[Project J/Day87] PRIVATE MATCH 선택 완료 - Host/Join UI는 88일차에 연결합니다."
                );

                if (detailStatusText != null)
                {
                    detailStatusText.text =
                        "PRIVATE MATCH 선택 완료 · 88일차 Host/Join UI 연결 예정";
                }

                return;
            }

            Debug.Log(
                "[Project J/Day87] 선택 모드: " +
                selectedCard.DisplayName
            );
        }
    }
}
