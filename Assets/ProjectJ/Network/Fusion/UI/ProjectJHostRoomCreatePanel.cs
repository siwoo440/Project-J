using UnityEngine; // Runtime UI와 GameObject 사용
using UnityEngine.UI; // Button, InputField, Text 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJHostRoomCreatePanel :
        MonoBehaviour
    {
        private static readonly string[]
            Difficulties =
            {
                "EASY",
                "NORMAL",
                "HARD"
            };

        private static readonly int[]
            RoundOptions =
            {
                1,
                3,
                5
            };

        [SerializeField]
        private GameObject privateMatchRoot;

        [SerializeField]
        private GameObject hostRoomCreateRoot;

        [SerializeField]
        private GameObject playerLobbyPreviewRoot;

        [SerializeField]
        private InputField roomNameInput;

        [SerializeField]
        private Text maxPlayersValueText;

        [SerializeField]
        private Text passwordValueText;

        [SerializeField]
        private Text roundValueText;

        [SerializeField]
        private Text difficultyValueText;

        [SerializeField]
        private Text previewRoomNameText;

        [SerializeField]
        private Text previewPlayerCountText;

        [SerializeField]
        private Text previewRoundText;

        [SerializeField]
        private Text previewDifficultyText;

        [SerializeField]
        private Text previewPasswordText;

        [SerializeField]
        private Text statusText;

        [SerializeField]
        private Button maxPlayersPreviousButton;

        [SerializeField]
        private Button maxPlayersNextButton;

        [SerializeField]
        private Button passwordToggleButton;

        [SerializeField]
        private Button roundPreviousButton;

        [SerializeField]
        private Button roundNextButton;

        [SerializeField]
        private Button difficultyPreviousButton;

        [SerializeField]
        private Button difficultyNextButton;

        [SerializeField]
        private Button characterPreviewButton;

        [SerializeField]
        private Button backButton;

        [SerializeField]
        private Button createRoomButton;

        [SerializeField]
        private Button playerLobbyBackButton;

        private ProjectJDay82SceneFlowCoordinator
            sceneFlow;

        private int maxPlayers =
            8;

        private bool usePassword;

        private int roundIndex;

        private int difficultyIndex =
            1;

        private bool requestPending;

        public int MaxPlayers =>
            maxPlayers;

        public bool UsePassword =>
            usePassword;

        public int RoundCount =>
            RoundOptions[roundIndex];

        public string Difficulty =>
            Difficulties[difficultyIndex];

        public string RoomName =>
            roomNameInput != null &&
            !string.IsNullOrWhiteSpace(
                roomNameInput.text
            )
                ? roomNameInput.text.Trim()
                : "MY ROOM";

        public void Configure(
            GameObject privateRoot,
            GameObject hostCreateRoot,
            GameObject lobbyPreviewRoot,
            InputField roomName,
            Text maxPlayersText,
            Text passwordText,
            Text roundsText,
            Text difficultyText,
            Text previewRoomName,
            Text previewPlayerCount,
            Text previewRounds,
            Text previewDifficulty,
            Text previewPassword,
            Text connectionStatus,
            Button maxPlayersPrevious,
            Button maxPlayersNext,
            Button passwordToggle,
            Button roundsPrevious,
            Button roundsNext,
            Button difficultyPrevious,
            Button difficultyNext,
            Button previewCharacter,
            Button returnButton,
            Button confirmCreateButton,
            Button lobbyPreviewBackButton
        )
        {
            privateMatchRoot =
                privateRoot;

            hostRoomCreateRoot =
                hostCreateRoot;

            playerLobbyPreviewRoot =
                lobbyPreviewRoot;

            roomNameInput =
                roomName;

            maxPlayersValueText =
                maxPlayersText;

            passwordValueText =
                passwordText;

            roundValueText =
                roundsText;

            difficultyValueText =
                difficultyText;

            previewRoomNameText =
                previewRoomName;

            previewPlayerCountText =
                previewPlayerCount;

            previewRoundText =
                previewRounds;

            previewDifficultyText =
                previewDifficulty;

            previewPasswordText =
                previewPassword;

            statusText =
                connectionStatus;

            maxPlayersPreviousButton =
                maxPlayersPrevious;

            maxPlayersNextButton =
                maxPlayersNext;

            passwordToggleButton =
                passwordToggle;

            roundPreviousButton =
                roundsPrevious;

            roundNextButton =
                roundsNext;

            difficultyPreviousButton =
                difficultyPrevious;

            difficultyNextButton =
                difficultyNext;

            characterPreviewButton =
                previewCharacter;

            backButton =
                returnButton;

            createRoomButton =
                confirmCreateButton;

            playerLobbyBackButton =
                lobbyPreviewBackButton;
        }

        private void Awake()
        {
            ResolveSceneFlow();
            BindUi();

            if (hostRoomCreateRoot != null)
            {
                hostRoomCreateRoot.SetActive(false);
            }

            if (playerLobbyPreviewRoot != null)
            {
                playerLobbyPreviewRoot.SetActive(false);
            }

            RefreshAll();
        }

        private void OnDestroy()
        {
            UnbindUi();
        }

        private void Update()
        {
            ResolveSceneFlow();

            if (
                hostRoomCreateRoot == null ||
                !hostRoomCreateRoot.activeSelf
            )
            {
                return;
            }

            RefreshConnectionState();
        }

        public void OpenFromPrivateMatch()
        {
            requestPending =
                false;

            if (privateMatchRoot != null)
            {
                privateMatchRoot.SetActive(false);
            }

            if (playerLobbyPreviewRoot != null)
            {
                playerLobbyPreviewRoot.SetActive(false);
            }

            if (hostRoomCreateRoot != null)
            {
                hostRoomCreateRoot.SetActive(true);
            }

            SetControlsInteractable(true);
            SetStatus(
                "방 설정을 확인한 뒤 CREATE ROOM을 눌러주세요."
            );

            RefreshAll();
        }

        public void BackToPrivateMatch()
        {
            if (IsConnecting())
            {
                SetStatus(
                    "방을 생성하는 동안에는 뒤로 갈 수 없습니다."
                );

                return;
            }

            requestPending =
                false;

            if (hostRoomCreateRoot != null)
            {
                hostRoomCreateRoot.SetActive(false);
            }

            if (playerLobbyPreviewRoot != null)
            {
                playerLobbyPreviewRoot.SetActive(false);
            }

            if (privateMatchRoot != null)
            {
                privateMatchRoot.SetActive(true);
            }
        }

        public void OpenPlayerLobbyPreview()
        {
            if (IsConnecting())
            {
                return;
            }

            if (hostRoomCreateRoot != null)
            {
                hostRoomCreateRoot.SetActive(false);
            }

            if (playerLobbyPreviewRoot != null)
            {
                playerLobbyPreviewRoot.SetActive(true);
            }
        }

        public void BackToHostRoomCreate()
        {
            if (playerLobbyPreviewRoot != null)
            {
                playerLobbyPreviewRoot.SetActive(false);
            }

            if (hostRoomCreateRoot != null)
            {
                hostRoomCreateRoot.SetActive(true);
            }

            RefreshAll();
        }

        public void ConfirmCreateRoom()
        {
            ResolveSceneFlow();

            if (sceneFlow == null)
            {
                SetStatus(
                    "온라인 Scene Flow를 찾을 수 없습니다."
                );

                return;
            }

            if (IsConnecting())
            {
                return;
            }

            if (
                roomNameInput != null &&
                string.IsNullOrWhiteSpace(
                    roomNameInput.text
                )
            )
            {
                roomNameInput.SetTextWithoutNotify(
                    "MY ROOM"
                );
            }

            requestPending =
                true;

            SetControlsInteractable(false);

            SetStatus(
                "비공개 방 생성 요청 중..."
            );

            sceneFlow.RequestCreatePrivateRoom();
        }

        private void BindUi()
        {
            if (roomNameInput != null)
            {
                roomNameInput.onValueChanged
                    .AddListener(
                        OnRoomNameChanged
                    );
            }

            AddButtonListener(
                maxPlayersPreviousButton,
                DecreaseMaxPlayers
            );

            AddButtonListener(
                maxPlayersNextButton,
                IncreaseMaxPlayers
            );

            AddButtonListener(
                passwordToggleButton,
                TogglePassword
            );

            AddButtonListener(
                roundPreviousButton,
                PreviousRound
            );

            AddButtonListener(
                roundNextButton,
                NextRound
            );

            AddButtonListener(
                difficultyPreviousButton,
                PreviousDifficulty
            );

            AddButtonListener(
                difficultyNextButton,
                NextDifficulty
            );

            AddButtonListener(
                characterPreviewButton,
                OpenPlayerLobbyPreview
            );

            AddButtonListener(
                backButton,
                BackToPrivateMatch
            );

            AddButtonListener(
                createRoomButton,
                ConfirmCreateRoom
            );

            AddButtonListener(
                playerLobbyBackButton,
                BackToHostRoomCreate
            );
        }

        private void UnbindUi()
        {
            if (roomNameInput != null)
            {
                roomNameInput.onValueChanged
                    .RemoveListener(
                        OnRoomNameChanged
                    );
            }

            RemoveButtonListener(
                maxPlayersPreviousButton,
                DecreaseMaxPlayers
            );

            RemoveButtonListener(
                maxPlayersNextButton,
                IncreaseMaxPlayers
            );

            RemoveButtonListener(
                passwordToggleButton,
                TogglePassword
            );

            RemoveButtonListener(
                roundPreviousButton,
                PreviousRound
            );

            RemoveButtonListener(
                roundNextButton,
                NextRound
            );

            RemoveButtonListener(
                difficultyPreviousButton,
                PreviousDifficulty
            );

            RemoveButtonListener(
                difficultyNextButton,
                NextDifficulty
            );

            RemoveButtonListener(
                characterPreviewButton,
                OpenPlayerLobbyPreview
            );

            RemoveButtonListener(
                backButton,
                BackToPrivateMatch
            );

            RemoveButtonListener(
                createRoomButton,
                ConfirmCreateRoom
            );

            RemoveButtonListener(
                playerLobbyBackButton,
                BackToHostRoomCreate
            );
        }

        private void OnRoomNameChanged(
            string value
        )
        {
            RefreshPreview();
        }

        private void DecreaseMaxPlayers()
        {
            maxPlayers =
                Mathf.Max(
                    2,
                    maxPlayers - 1
                );

            RefreshAll();
        }

        private void IncreaseMaxPlayers()
        {
            maxPlayers =
                Mathf.Min(
                    8,
                    maxPlayers + 1
                );

            RefreshAll();
        }

        private void TogglePassword()
        {
            usePassword =
                !usePassword;

            RefreshAll();
        }

        private void PreviousRound()
        {
            roundIndex =
                Mathf.Max(
                    0,
                    roundIndex - 1
                );

            RefreshAll();
        }

        private void NextRound()
        {
            roundIndex =
                Mathf.Min(
                    RoundOptions.Length - 1,
                    roundIndex + 1
                );

            RefreshAll();
        }

        private void PreviousDifficulty()
        {
            difficultyIndex =
                Mathf.Max(
                    0,
                    difficultyIndex - 1
                );

            RefreshAll();
        }

        private void NextDifficulty()
        {
            difficultyIndex =
                Mathf.Min(
                    Difficulties.Length - 1,
                    difficultyIndex + 1
                );

            RefreshAll();
        }

        private void RefreshAll()
        {
            if (maxPlayersValueText != null)
            {
                maxPlayersValueText.text =
                    maxPlayers.ToString();
            }

            if (passwordValueText != null)
            {
                passwordValueText.text =
                    usePassword
                        ? "ON"
                        : "OFF";
            }

            if (roundValueText != null)
            {
                roundValueText.text =
                    RoundCount +
                    " ROUND";
            }

            if (difficultyValueText != null)
            {
                difficultyValueText.text =
                    Difficulty;
            }

            RefreshPreview();
            RefreshButtons();
        }

        private void RefreshPreview()
        {
            if (previewRoomNameText != null)
            {
                previewRoomNameText.text =
                    RoomName;
            }

            if (previewPlayerCountText != null)
            {
                previewPlayerCountText.text =
                    "1 / " +
                    maxPlayers +
                    " PLAYERS";
            }

            if (previewRoundText != null)
            {
                previewRoundText.text =
                    RoundCount +
                    " ROUND";
            }

            if (previewDifficultyText != null)
            {
                previewDifficultyText.text =
                    Difficulty;
            }

            if (previewPasswordText != null)
            {
                previewPasswordText.text =
                    usePassword
                        ? "PASSWORD : ON"
                        : "PASSWORD : OFF";
            }
        }

        private void RefreshButtons()
        {
            if (maxPlayersPreviousButton != null)
            {
                maxPlayersPreviousButton.interactable =
                    maxPlayers > 2;
            }

            if (maxPlayersNextButton != null)
            {
                maxPlayersNextButton.interactable =
                    maxPlayers < 8;
            }

            if (roundPreviousButton != null)
            {
                roundPreviousButton.interactable =
                    roundIndex > 0;
            }

            if (roundNextButton != null)
            {
                roundNextButton.interactable =
                    roundIndex <
                    RoundOptions.Length - 1;
            }

            if (difficultyPreviousButton != null)
            {
                difficultyPreviousButton.interactable =
                    difficultyIndex > 0;
            }

            if (difficultyNextButton != null)
            {
                difficultyNextButton.interactable =
                    difficultyIndex <
                    Difficulties.Length - 1;
            }
        }

        private void ResolveSceneFlow()
        {
            if (sceneFlow != null)
            {
                return;
            }

            sceneFlow =
                FindFirstObjectByType<
                    ProjectJDay82SceneFlowCoordinator
                >();
        }

        private void RefreshConnectionState()
        {
            if (!requestPending)
            {
                return;
            }

            if (sceneFlow == null)
            {
                SetControlsInteractable(true);
                requestPending =
                    false;

                return;
            }

            ProjectJFusionBootstrap bootstrap =
                sceneFlow.Bootstrap;

            if (
                sceneFlow.State ==
                    ProjectJDay82SceneFlowState.Connecting ||
                (
                    bootstrap != null &&
                    (
                        bootstrap.State ==
                            ProjectJFusionBootstrapState.Starting ||
                        bootstrap.State ==
                            ProjectJFusionBootstrapState.Stopping
                    )
                )
            )
            {
                SetControlsInteractable(false);

                if (
                    !string.IsNullOrWhiteSpace(
                        sceneFlow.StatusText
                    )
                )
                {
                    SetStatus(
                        sceneFlow.StatusText
                    );
                }

                return;
            }

            if (
                bootstrap != null &&
                bootstrap.State ==
                    ProjectJFusionBootstrapState.Failed
            )
            {
                requestPending =
                    false;

                SetControlsInteractable(true);
                SetStatus(
                    bootstrap.StatusMessage
                );

                return;
            }

            if (
                bootstrap != null &&
                bootstrap.State ==
                    ProjectJFusionBootstrapState.Running
            )
            {
                SetControlsInteractable(false);
                SetStatus(
                    "방 생성 완료 · Lobby로 이동합니다..."
                );
            }
        }

        private bool IsConnecting()
        {
            if (sceneFlow == null)
            {
                return false;
            }

            if (
                sceneFlow.State ==
                    ProjectJDay82SceneFlowState.Connecting
            )
            {
                return true;
            }

            ProjectJFusionBootstrap bootstrap =
                sceneFlow.Bootstrap;

            return
                bootstrap != null &&
                (
                    bootstrap.State ==
                        ProjectJFusionBootstrapState.Starting ||
                    bootstrap.State ==
                        ProjectJFusionBootstrapState.Stopping
                );
        }

        private void SetControlsInteractable(
            bool interactable
        )
        {
            if (roomNameInput != null)
            {
                roomNameInput.interactable =
                    interactable;
            }

            if (passwordToggleButton != null)
            {
                passwordToggleButton.interactable =
                    interactable;
            }

            if (characterPreviewButton != null)
            {
                characterPreviewButton.interactable =
                    interactable;
            }

            if (backButton != null)
            {
                backButton.interactable =
                    interactable;
            }

            if (createRoomButton != null)
            {
                createRoomButton.interactable =
                    interactable;
            }

            if (interactable)
            {
                RefreshButtons();
            }
            else
            {
                SetArrowButtonsInteractable(false);
            }
        }

        private void SetArrowButtonsInteractable(
            bool interactable
        )
        {
            if (maxPlayersPreviousButton != null)
            {
                maxPlayersPreviousButton.interactable =
                    interactable;
            }

            if (maxPlayersNextButton != null)
            {
                maxPlayersNextButton.interactable =
                    interactable;
            }

            if (roundPreviousButton != null)
            {
                roundPreviousButton.interactable =
                    interactable;
            }

            if (roundNextButton != null)
            {
                roundNextButton.interactable =
                    interactable;
            }

            if (difficultyPreviousButton != null)
            {
                difficultyPreviousButton.interactable =
                    interactable;
            }

            if (difficultyNextButton != null)
            {
                difficultyNextButton.interactable =
                    interactable;
            }
        }

        private void SetStatus(
            string message
        )
        {
            if (statusText != null)
            {
                statusText.text =
                    message;
            }
        }

        private static void AddButtonListener(
            Button button,
            UnityEngine.Events.UnityAction action
        )
        {
            if (button != null)
            {
                button.onClick.AddListener(
                    action
                );
            }
        }

        private static void RemoveButtonListener(
            Button button,
            UnityEngine.Events.UnityAction action
        )
        {
            if (button != null)
            {
                button.onClick.RemoveListener(
                    action
                );
            }
        }
    }
}
