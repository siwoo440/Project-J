using UnityEngine; // GameObject와 Mathf 사용
using UnityEngine.UI; // Button과 Text 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJPlayerLobbyPanel :
        MonoBehaviour
    {
        private const int PlayersPerPage =
            8;

        private const int MaximumSupportedPlayers =
            32;

        [SerializeField]
        private Button previousPageButton;

        [SerializeField]
        private Button nextPageButton;

        [SerializeField]
        private Text pageText;

        [SerializeField]
        private Text readySummaryText;

        [SerializeField]
        private Text roomNameText;

        [SerializeField]
        private Text playerCountText;

        [SerializeField]
        private Text roundText;

        [SerializeField]
        private Text difficultyText;

        [SerializeField]
        private Text passwordText;

        [SerializeField]
        private GameObject[] slotRoots;

        [SerializeField]
        private Text[] slotIndexTexts;

        [SerializeField]
        private Text[] slotNameTexts;

        [SerializeField]
        private Text[] slotStateTexts;

        private int currentPage;

        private int totalSlots =
            8;

        public int CurrentPage =>
            currentPage;

        public int PageCount =>
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    totalSlots /
                    (float)PlayersPerPage
                )
            );

        public void Configure(
            Button previousButton,
            Button nextButton,
            Text pageLabel,
            Text readySummary,
            Text roomName,
            Text playerCount,
            Text rounds,
            Text difficulty,
            Text password,
            GameObject[] roots,
            Text[] indexLabels,
            Text[] playerNames,
            Text[] states
        )
        {
            previousPageButton =
                previousButton;

            nextPageButton =
                nextButton;

            pageText =
                pageLabel;

            readySummaryText =
                readySummary;

            roomNameText =
                roomName;

            playerCountText =
                playerCount;

            roundText =
                rounds;

            difficultyText =
                difficulty;

            passwordText =
                password;

            slotRoots =
                roots;

            slotIndexTexts =
                indexLabels;

            slotNameTexts =
                playerNames;

            slotStateTexts =
                states;
        }

        private void Awake()
        {
            BindUi();
            RefreshPage();
        }

        private void OnEnable()
        {
            RefreshPage();
        }

        private void OnDestroy()
        {
            UnbindUi();
        }

        public void SetPreviewData(
            string roomName,
            int maxPlayers,
            int roundCount,
            string difficulty,
            bool usePassword
        )
        {
            totalSlots =
                Mathf.Clamp(
                    maxPlayers,
                    1,
                    MaximumSupportedPlayers
                );

            currentPage =
                Mathf.Clamp(
                    currentPage,
                    0,
                    PageCount - 1
                );

            if (roomNameText != null)
            {
                roomNameText.text =
                    string.IsNullOrWhiteSpace(
                        roomName
                    )
                        ? "MY ROOM"
                        : roomName;
            }

            if (playerCountText != null)
            {
                playerCountText.text =
                    "1 / " +
                    totalSlots;
            }

            if (roundText != null)
            {
                roundText.text =
                    roundCount +
                    " ROUND";
            }

            if (difficultyText != null)
            {
                difficultyText.text =
                    string.IsNullOrWhiteSpace(
                        difficulty
                    )
                        ? "NORMAL"
                        : difficulty;
            }

            if (passwordText != null)
            {
                passwordText.text =
                    usePassword
                        ? "ON"
                        : "OFF";
            }

            if (readySummaryText != null)
            {
                readySummaryText.text =
                    "READY  1 / " +
                    totalSlots;
            }

            RefreshPage();
        }

        public void PreviousPage()
        {
            if (currentPage <= 0)
            {
                return;
            }

            currentPage--;
            RefreshPage();
        }

        public void NextPage()
        {
            if (
                currentPage >=
                PageCount - 1
            )
            {
                return;
            }

            currentPage++;
            RefreshPage();
        }

        private void BindUi()
        {
            if (previousPageButton != null)
            {
                previousPageButton.onClick
                    .AddListener(
                        PreviousPage
                    );
            }

            if (nextPageButton != null)
            {
                nextPageButton.onClick
                    .AddListener(
                        NextPage
                    );
            }
        }

        private void UnbindUi()
        {
            if (previousPageButton != null)
            {
                previousPageButton.onClick
                    .RemoveListener(
                        PreviousPage
                    );
            }

            if (nextPageButton != null)
            {
                nextPageButton.onClick
                    .RemoveListener(
                        NextPage
                    );
            }
        }

        private void RefreshPage()
        {
            int pageCount =
                PageCount;

            currentPage =
                Mathf.Clamp(
                    currentPage,
                    0,
                    pageCount - 1
                );

            bool multiplePages =
                pageCount > 1;

            if (previousPageButton != null)
            {
                previousPageButton.gameObject
                    .SetActive(
                        multiplePages
                    );

                previousPageButton.interactable =
                    currentPage > 0;
            }

            if (nextPageButton != null)
            {
                nextPageButton.gameObject
                    .SetActive(
                        multiplePages
                    );

                nextPageButton.interactable =
                    currentPage <
                    pageCount - 1;
            }

            if (pageText != null)
            {
                pageText.text =
                    "PAGE " +
                    (currentPage + 1) +
                    " / " +
                    pageCount;

                pageText.gameObject.SetActive(
                    multiplePages
                );
            }

            for (
                int localIndex = 0;
                localIndex < PlayersPerPage;
                localIndex++
            )
            {
                int globalIndex =
                    currentPage *
                    PlayersPerPage +
                    localIndex;

                bool validSlot =
                    globalIndex <
                    totalSlots;

                SetSlotActive(
                    localIndex,
                    validSlot
                );

                if (!validSlot)
                {
                    continue;
                }

                bool isHost =
                    globalIndex == 0;

                SetSlotText(
                    localIndex,
                    globalIndex,
                    isHost
                );
            }
        }

        private void SetSlotActive(
            int localIndex,
            bool active
        )
        {
            if (
                slotRoots == null ||
                localIndex < 0 ||
                localIndex >=
                    slotRoots.Length
            )
            {
                return;
            }

            if (
                slotRoots[localIndex] != null
            )
            {
                slotRoots[localIndex]
                    .SetActive(
                        active
                    );
            }
        }

        private void SetSlotText(
            int localIndex,
            int globalIndex,
            bool isHost
        )
        {
            if (
                slotIndexTexts != null &&
                localIndex <
                    slotIndexTexts.Length &&
                slotIndexTexts[localIndex] != null
            )
            {
                slotIndexTexts[localIndex].text =
                    "#" +
                    (globalIndex + 1)
                        .ToString("00");
            }

            if (
                slotNameTexts != null &&
                localIndex <
                    slotNameTexts.Length &&
                slotNameTexts[localIndex] != null
            )
            {
                slotNameTexts[localIndex].text =
                    isHost
                        ? "PLAYER 01"
                        : "WAITING...";
            }

            if (
                slotStateTexts != null &&
                localIndex <
                    slotStateTexts.Length &&
                slotStateTexts[localIndex] != null
            )
            {
                slotStateTexts[localIndex].text =
                    isHost
                        ? "HOST"
                        : "EMPTY";
            }
        }
    }
}
