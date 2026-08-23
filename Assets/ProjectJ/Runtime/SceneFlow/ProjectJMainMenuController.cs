using UnityEngine; // MainMenu 상태와 종료 처리
using UnityEngine.UI; // Button, Text, Image 사용

namespace ProjectJ
{
    [DisallowMultipleComponent]
    public sealed class ProjectJMainMenuController :
        MonoBehaviour
    {
        private const int HomeIndex = 0;
        private const int PlayIndex = 1;
        private const int CustomizeIndex = 2;
        private const int ProfileIndex = 3;
        private const int SettingsIndex = 4;

        [SerializeField]
        private Button[] tabButtons;

        [SerializeField]
        private Text[] tabLabels;

        [SerializeField]
        private Image[] selectedBars;

        [SerializeField]
        private GameObject[] tabPanels;

        [SerializeField]
        private Button exitButton;

        [SerializeField]
        private GameObject characterPreviewRoot;

        [SerializeField]
        private Color normalTextColor =
            new Color(
                0.82f,
                0.86f,
                0.92f,
                1f
            );

        [SerializeField]
        private Color selectedTextColor =
            new Color(
                0.20f,
                0.82f,
                1f,
                1f
            );

        [SerializeField]
        private Color normalButtonColor =
            new Color(
                0.04f,
                0.07f,
                0.12f,
                0.86f
            );

        [SerializeField]
        private Color selectedButtonColor =
            new Color(
                0.04f,
                0.28f,
                0.42f,
                0.96f
            );

        private int currentTabIndex =
            -1;

        public int CurrentTabIndex =>
            currentTabIndex;

        public void Configure(
            Button[] buttons,
            Text[] labels,
            Image[] bars,
            GameObject[] panels,
            Button quitButton,
            GameObject previewRoot
        )
        {
            tabButtons =
                buttons;

            tabLabels =
                labels;

            selectedBars =
                bars;

            tabPanels =
                panels;

            exitButton =
                quitButton;

            characterPreviewRoot =
                previewRoot;
        }

        private void Awake()
        {
            BindButtons();
            OpenHome();
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        public void OpenHome()
        {
            SetTab(
                HomeIndex
            );
        }

        public void OpenPlay()
        {
            SetTab(
                PlayIndex
            );
        }

        public void OpenCustomize()
        {
            SetTab(
                CustomizeIndex
            );
        }

        public void OpenProfile()
        {
            SetTab(
                ProfileIndex
            );
        }

        public void OpenSettings()
        {
            SetTab(
                SettingsIndex
            );
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            Debug.Log(
                "[Project J/Day86] EXIT 버튼 입력 - Build에서는 게임을 종료합니다."
            );
#else
            Application.Quit();
#endif
        }

        private void BindButtons()
        {
            if (
                tabButtons == null ||
                tabButtons.Length < 5
            )
            {
                return;
            }

            tabButtons[HomeIndex]
                .onClick.AddListener(
                    OpenHome
                );

            tabButtons[PlayIndex]
                .onClick.AddListener(
                    OpenPlay
                );

            tabButtons[CustomizeIndex]
                .onClick.AddListener(
                    OpenCustomize
                );

            tabButtons[ProfileIndex]
                .onClick.AddListener(
                    OpenProfile
                );

            tabButtons[SettingsIndex]
                .onClick.AddListener(
                    OpenSettings
                );

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(
                    QuitGame
                );
            }
        }

        private void UnbindButtons()
        {
            if (
                tabButtons != null &&
                tabButtons.Length >= 5
            )
            {
                tabButtons[HomeIndex]
                    .onClick.RemoveListener(
                        OpenHome
                    );

                tabButtons[PlayIndex]
                    .onClick.RemoveListener(
                        OpenPlay
                    );

                tabButtons[CustomizeIndex]
                    .onClick.RemoveListener(
                        OpenCustomize
                    );

                tabButtons[ProfileIndex]
                    .onClick.RemoveListener(
                        OpenProfile
                    );

                tabButtons[SettingsIndex]
                    .onClick.RemoveListener(
                        OpenSettings
                    );
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveListener(
                    QuitGame
                );
            }
        }

        private void SetTab(
            int targetIndex
        )
        {
            if (
                tabPanels == null ||
                targetIndex < 0 ||
                targetIndex >=
                    tabPanels.Length
            )
            {
                return;
            }

            currentTabIndex =
                targetIndex;

            for (
                int index = 0;
                index < tabPanels.Length;
                index++
            )
            {
                if (
                    tabPanels[index] != null
                )
                {
                    tabPanels[index]
                        .SetActive(
                            index ==
                            currentTabIndex
                        );
                }
            }

            if (
                characterPreviewRoot != null
            )
            {
                bool showCharacter =
                    currentTabIndex ==
                        HomeIndex ||
                    currentTabIndex ==
                        CustomizeIndex;

                characterPreviewRoot.SetActive(
                    showCharacter
                );
            }

            RefreshNavigationVisuals();
        }

        private void RefreshNavigationVisuals()
        {
            if (tabButtons == null)
            {
                return;
            }

            for (
                int index = 0;
                index < tabButtons.Length;
                index++
            )
            {
                bool selected =
                    index ==
                    currentTabIndex;

                Button button =
                    tabButtons[index];

                if (button != null)
                {
                    Image background =
                        button.targetGraphic
                            as Image;

                    if (background != null)
                    {
                        background.color =
                            selected
                                ? selectedButtonColor
                                : normalButtonColor;
                    }
                }

                if (
                    tabLabels != null &&
                    index <
                        tabLabels.Length &&
                    tabLabels[index] != null
                )
                {
                    tabLabels[index].color =
                        selected
                            ? selectedTextColor
                            : normalTextColor;
                }

                if (
                    selectedBars != null &&
                    index <
                        selectedBars.Length &&
                    selectedBars[index] != null
                )
                {
                    selectedBars[index]
                        .gameObject
                        .SetActive(
                            selected
                        );
                }
            }
        }
    }
}
