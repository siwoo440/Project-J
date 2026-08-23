using UnityEngine; // Runtime UI Controller
using UnityEngine.UI; // Button, InputField, Text 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJPrivateMatchPanel :
        MonoBehaviour
    {
        [SerializeField]
        private ProjectJPlayModePanel playModePanel;

        [SerializeField]
        private GameObject modeSelectRoot;

        [SerializeField]
        private GameObject privateMatchRoot;

        [SerializeField]
        private Button createRoomButton;

        [SerializeField]
        private Button joinRoomButton;

        [SerializeField]
        private Button backButton;

        [SerializeField]
        private InputField roomCodeInput;

        [SerializeField]
        private Text statusText;

        [SerializeField]
        private ProjectJHostRoomCreatePanel
            hostRoomCreatePanel;

        private ProjectJDay82SceneFlowCoordinator
            sceneFlow;

        private bool requestPending;

        public void Configure(
            ProjectJPlayModePanel modePanel,
            GameObject modeSelection,
            GameObject privateMatch,
            Button createButton,
            Button joinButton,
            Button returnButton,
            InputField codeInput,
            Text connectionStatus
        )
        {
            playModePanel =
                modePanel;

            modeSelectRoot =
                modeSelection;

            privateMatchRoot =
                privateMatch;

            createRoomButton =
                createButton;

            joinRoomButton =
                joinButton;

            backButton =
                returnButton;

            roomCodeInput =
                codeInput;

            statusText =
                connectionStatus;
        }

        public void ConfigureHostRoomCreatePanel(
            ProjectJHostRoomCreatePanel panel
        )
        {
            hostRoomCreatePanel =
                panel;
        }

        private void Awake()
        {
            ResolvePlayModePanel();
            ResolveHostRoomCreatePanel();
            ResolveSceneFlow();
            BindUi();

            if (privateMatchRoot != null)
            {
                privateMatchRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            UnbindUi();
        }

        private void Update()
        {
            ResolveSceneFlow();

            if (
                privateMatchRoot == null ||
                !privateMatchRoot.activeSelf
            )
            {
                return;
            }

            RefreshConnectionState();
        }

        private void OnModeSelectionConfirmed(
            ProjectJGameModeCard card
        )
        {
            if (
                card == null ||
                card.ComingSoon ||
                card.ModeId !=
                    ProjectJGameModeId.PrivateMatch
            )
            {
                return;
            }

            Open();
        }

        public void Open()
        {
            ResolveSceneFlow();
            requestPending =
                false;

            if (modeSelectRoot != null)
            {
                modeSelectRoot.SetActive(false);
            }

            if (privateMatchRoot != null)
            {
                privateMatchRoot.SetActive(true);
            }

            SetStatus(
                sceneFlow == null
                    ? "온라인 시스템을 찾는 중..."
                    : "비공개 방을 만들거나 Room Code로 참가하세요."
            );

            RefreshConnectionState();
        }

        public void Close()
        {
            if (IsConnecting())
            {
                SetStatus(
                    "연결 중에는 뒤로 갈 수 없습니다."
                );

                return;
            }

            requestPending =
                false;

            if (privateMatchRoot != null)
            {
                privateMatchRoot.SetActive(false);
            }

            if (modeSelectRoot != null)
            {
                modeSelectRoot.SetActive(true);
            }
        }

        public void CreateRoom()
        {
            ResolveHostRoomCreatePanel();

            if (hostRoomCreatePanel == null)
            {
                SetStatus(
                    "Host Room Create 화면을 찾을 수 없습니다."
                );

                Debug.LogWarning(
                    "[Project J/Day89] ProjectJHostRoomCreatePanel 연결 없음"
                );

                return;
            }

            requestPending =
                false;

            hostRoomCreatePanel.OpenFromPrivateMatch();
        }

        public void JoinRoom()
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

            string inputValue =
                roomCodeInput != null
                    ? roomCodeInput.text
                    : string.Empty;

            if (
                !ProjectJFusionRoomCode
                    .TryNormalize(
                        inputValue,
                        out string normalizedCode,
                        out string errorMessage
                    )
            )
            {
                SetStatus(
                    errorMessage
                );

                return;
            }

            if (roomCodeInput != null)
            {
                roomCodeInput.SetTextWithoutNotify(
                    normalizedCode
                );
            }

            requestPending =
                true;

            SetButtonsInteractable(
                false
            );

            SetStatus(
                normalizedCode +
                " 방 참가 요청 중..."
            );

            bool requested =
                sceneFlow.RequestJoinPrivateRoom(
                    normalizedCode
                );

            if (!requested)
            {
                requestPending =
                    false;

                SetButtonsInteractable(
                    true
                );

                SetStatus(
                    sceneFlow.StatusText
                );
            }
        }

        private void BindUi()
        {
            if (playModePanel != null)
            {
                playModePanel.SelectionConfirmed +=
                    OnModeSelectionConfirmed;
            }

            if (createRoomButton != null)
            {
                createRoomButton.onClick
                    .AddListener(
                        CreateRoom
                    );
            }

            if (joinRoomButton != null)
            {
                joinRoomButton.onClick
                    .AddListener(
                        JoinRoom
                    );
            }

            if (backButton != null)
            {
                backButton.onClick
                    .AddListener(
                        Close
                    );
            }

            if (roomCodeInput != null)
            {
                roomCodeInput.onValueChanged
                    .AddListener(
                        NormalizeInputDisplay
                    );
            }
        }

        private void UnbindUi()
        {
            if (playModePanel != null)
            {
                playModePanel.SelectionConfirmed -=
                    OnModeSelectionConfirmed;
            }

            if (createRoomButton != null)
            {
                createRoomButton.onClick
                    .RemoveListener(
                        CreateRoom
                    );
            }

            if (joinRoomButton != null)
            {
                joinRoomButton.onClick
                    .RemoveListener(
                        JoinRoom
                    );
            }

            if (backButton != null)
            {
                backButton.onClick
                    .RemoveListener(
                        Close
                    );
            }

            if (roomCodeInput != null)
            {
                roomCodeInput.onValueChanged
                    .RemoveListener(
                        NormalizeInputDisplay
                    );
            }
        }

        private void NormalizeInputDisplay(
            string value
        )
        {
            if (roomCodeInput == null)
            {
                return;
            }

            string upper =
                string.IsNullOrEmpty(
                    value
                )
                    ? string.Empty
                    : value.ToUpperInvariant();

            if (upper == value)
            {
                return;
            }

            roomCodeInput.SetTextWithoutNotify(
                upper
            );
        }

        private void ResolvePlayModePanel()
        {
            if (playModePanel != null)
            {
                return;
            }

            playModePanel =
                GetComponent<
                    ProjectJPlayModePanel
                >();

            if (playModePanel == null)
            {
                Debug.LogError(
                    "[Project J/Day88] 같은 PlayPanel에서 ProjectJPlayModePanel을 찾지 못했습니다."
                );
            }
        }

        private void ResolveHostRoomCreatePanel()
        {
            if (hostRoomCreatePanel != null)
            {
                return;
            }

            hostRoomCreatePanel =
                GetComponent<
                    ProjectJHostRoomCreatePanel
                >();
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
            bool connecting =
                IsConnecting();

            if (connecting)
            {
                SetButtonsInteractable(false);

                if (
                    sceneFlow != null &&
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

            ProjectJFusionBootstrap bootstrap =
                sceneFlow != null
                    ? sceneFlow.Bootstrap
                    : null;

            if (
                bootstrap != null &&
                bootstrap.State ==
                    ProjectJFusionBootstrapState.Failed
            )
            {
                requestPending =
                    false;

                SetButtonsInteractable(true);
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
                SetButtonsInteractable(false);

                SetStatus(
                    "연결 완료 · Lobby로 이동합니다..."
                );

                return;
            }

            if (requestPending)
            {
                requestPending =
                    false;
            }

            SetButtonsInteractable(
                sceneFlow != null
            );
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

        private void SetButtonsInteractable(
            bool interactable
        )
        {
            if (createRoomButton != null)
            {
                createRoomButton.interactable =
                    interactable;
            }

            if (joinRoomButton != null)
            {
                joinRoomButton.interactable =
                    interactable;
            }

            if (roomCodeInput != null)
            {
                roomCodeInput.interactable =
                    interactable;
            }

            if (backButton != null)
            {
                backButton.interactable =
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
    }
}
