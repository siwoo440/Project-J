using ProjectJ.Steam; // Steam 초기화 상태 확인
using UnityEngine; // Runtime Coordinator
using UnityEngine.SceneManagement; // 로컬 Scene 이동

namespace ProjectJ.Networking.Fusion
{
    public enum ProjectJDay82SceneFlowState
    {
        Bootstrap = 0,
        MainMenu = 1,
        Connecting = 2,
        Lobby = 3,
        MatchLoading = 4,
        Game = 5,
        Finished = 6,
        ReturningToMainMenu = 7
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProjectJFusionBootstrap))]
    [RequireComponent(typeof(ProjectJNetworkLobbyFlow))]
    public sealed class ProjectJDay82SceneFlowCoordinator :
        MonoBehaviour
    {
        private const string BootstrapSceneName =
            "Bootstrap";

        private const string MainMenuSceneName =
            "MainMenu";

        private const string LobbySceneName =
            "Lobby";

        private const string GameSceneName =
            "Game";

        private ProjectJFusionBootstrap bootstrap;

        private ProjectJNetworkLobbyFlow lobbyFlow;

        private ProjectJSteamIdentityService steamIdentity;

        private bool startedFromBootstrap;

        private bool bootstrapTransitionFinished;

        private bool returnToMainMenuRequested;

        private bool shutdownRequested;

        public ProjectJDay82SceneFlowState State
        {
            get;
            private set;
        } =
            ProjectJDay82SceneFlowState.Bootstrap;

        public string StatusText
        {
            get;
            private set;
        } =
            "초기화 준비";

        public ProjectJFusionBootstrap Bootstrap =>
            bootstrap;

        public ProjectJNetworkLobbyFlow LobbyFlow =>
            lobbyFlow;

        private void Awake()
        {
            bootstrap =
                GetComponent<ProjectJFusionBootstrap>();

            lobbyFlow =
                GetComponent<ProjectJNetworkLobbyFlow>();

            steamIdentity =
                ProjectJSteamIdentityService.Instance;

            Scene activeScene =
                SceneManager.GetActiveScene();

            startedFromBootstrap =
                activeScene.IsValid() &&
                activeScene.name ==
                    BootstrapSceneName;

            if (!startedFromBootstrap)
            {
                bootstrapTransitionFinished =
                    true;
            }
        }

        private void Update()
        {
            ResolveReferences();

            if (!bootstrapTransitionFinished)
            {
                UpdateBootstrapTransition();

                if (!bootstrapTransitionFinished)
                {
                    return;
                }
            }

            UpdateReturnToMainMenu();

            if (returnToMainMenuRequested)
            {
                return;
            }

            RefreshState();
        }

        public void RequestCreatePrivateRoom()
        {
            if (
                bootstrap == null ||
                !bootstrap.CanStart
            )
            {
                StatusText =
                    "현재 Host 방을 만들 수 없습니다.";

                return;
            }

            StatusText =
                "비공개 Host Room 생성 요청";

            State =
                ProjectJDay82SceneFlowState
                    .Connecting;

            bootstrap.RequestCreatePrivateRoom();
        }

        public bool RequestJoinPrivateRoom(
            string roomCode
        )
        {
            if (
                bootstrap == null ||
                !bootstrap.CanStart
            )
            {
                StatusText =
                    "현재 Room에 참가할 수 없습니다.";

                return false;
            }

            if (
                !ProjectJFusionRoomCode
                    .TryNormalize(
                        roomCode,
                        out string normalizedCode,
                        out string errorMessage
                    )
            )
            {
                StatusText =
                    errorMessage;

                return false;
            }

            bootstrap.RoomCode =
                normalizedCode;

            StatusText =
                "Room " +
                normalizedCode +
                " 참가 요청";

            State =
                ProjectJDay82SceneFlowState
                    .Connecting;

            bootstrap.RequestJoinPrivateRoom();

            return true;
        }

        public bool RequestReturnToLobby()
        {
            if (lobbyFlow == null)
            {
                StatusText =
                    "Lobby Flow가 없습니다.";

                return false;
            }

            bool requested =
                lobbyFlow.RequestReturnToLobby();

            if (requested)
            {
                StatusText =
                    "Game → Lobby 복귀 요청";
            }

            return requested;
        }

        public void RequestLeaveToMainMenu()
        {
            returnToMainMenuRequested =
                true;

            shutdownRequested =
                false;

            State =
                ProjectJDay82SceneFlowState
                    .ReturningToMainMenu;

            StatusText =
                "Session 종료 후 MainMenu 복귀";
        }

        public void RequestQuit()
        {
            Application.Quit();

#if UNITY_EDITOR
            Debug.Log(
                "[Project J/Day82] Quit 요청"
            );
#endif
        }

        private void UpdateBootstrapTransition()
        {
            State =
                ProjectJDay82SceneFlowState
                    .Bootstrap;

            if (steamIdentity == null)
            {
                StatusText =
                    "Steam Identity Service 생성 대기";

                return;
            }

            if (!IsSteamStartupResolved())
            {
                StatusText =
                    "Steam 초기화 대기: " +
                    steamIdentity.State;

                return;
            }

            bootstrapTransitionFinished =
                true;

            LoadMainMenuLocal();
        }

        private bool IsSteamStartupResolved()
        {
            if (steamIdentity == null)
            {
                return false;
            }

            switch (steamIdentity.State)
            {
                case ProjectJSteamAuthState.Uninitialized:
                case ProjectJSteamAuthState.Initializing:
                case ProjectJSteamAuthState.WaitingForWebApiTicket:
                    return false;

                default:
                    return true;
            }
        }

        private void UpdateReturnToMainMenu()
        {
            if (!returnToMainMenuRequested)
            {
                return;
            }

            State =
                ProjectJDay82SceneFlowState
                    .ReturningToMainMenu;

            if (bootstrap == null)
            {
                LoadMainMenuLocal();
                return;
            }

            if (
                bootstrap.CanShutdown &&
                !shutdownRequested
            )
            {
                shutdownRequested =
                    true;

                bootstrap.RequestLeaveRoom();

                StatusText =
                    "Fusion Runner 종료 중";

                return;
            }

            bool runnerStopped =
                bootstrap.Runner == null ||
                !bootstrap.Runner.IsRunning;

            if (
                runnerStopped &&
                bootstrap.CanStart
            )
            {
                returnToMainMenuRequested =
                    false;

                shutdownRequested =
                    false;

                LoadMainMenuLocal();
            }
        }

        private void RefreshState()
        {
            Scene activeScene =
                SceneManager.GetActiveScene();

            if (
                activeScene.IsValid() &&
                activeScene.name ==
                    MainMenuSceneName
            )
            {
                if (
                    bootstrap != null &&
                    bootstrap.State ==
                        ProjectJFusionBootstrapState
                            .Starting
                )
                {
                    State =
                        ProjectJDay82SceneFlowState
                            .Connecting;

                    StatusText =
                        bootstrap.StatusMessage;

                    return;
                }

                State =
                    ProjectJDay82SceneFlowState
                        .MainMenu;

                StatusText =
                    bootstrap != null
                        ? bootstrap.StatusMessage
                        : "MainMenu";

                return;
            }

            if (
                activeScene.IsValid() &&
                activeScene.name ==
                    LobbySceneName
            )
            {
                State =
                    ProjectJDay82SceneFlowState
                        .Lobby;

                StatusText =
                    lobbyFlow != null
                        ? lobbyFlow.StatusText
                        : "Lobby";

                return;
            }

            if (
                activeScene.IsValid() &&
                activeScene.name ==
                    GameSceneName
            )
            {
                if (
                    lobbyFlow != null &&
                    lobbyFlow.Phase ==
                        ProjectJNetworkLobbyFlowPhase
                            .Finished
                )
                {
                    State =
                        ProjectJDay82SceneFlowState
                            .Finished;
                }
                else
                {
                    State =
                        ProjectJDay82SceneFlowState
                            .Game;
                }

                StatusText =
                    lobbyFlow != null
                        ? lobbyFlow.StatusText
                        : "Game";

                return;
            }

            if (
                lobbyFlow != null &&
                lobbyFlow.Phase ==
                    ProjectJNetworkLobbyFlowPhase
                        .MatchLoading
            )
            {
                State =
                    ProjectJDay82SceneFlowState
                        .MatchLoading;

                StatusText =
                    lobbyFlow.StatusText;
            }
        }

        private void ResolveReferences()
        {
            if (bootstrap == null)
            {
                bootstrap =
                    GetComponent<
                        ProjectJFusionBootstrap
                    >();
            }

            if (lobbyFlow == null)
            {
                lobbyFlow =
                    GetComponent<
                        ProjectJNetworkLobbyFlow
                    >();
            }

            if (steamIdentity == null)
            {
                steamIdentity =
                    ProjectJSteamIdentityService
                        .Instance;
            }
        }

        private void LoadMainMenuLocal()
        {
            Scene activeScene =
                SceneManager.GetActiveScene();

            if (
                activeScene.IsValid() &&
                activeScene.name ==
                    MainMenuSceneName
            )
            {
                State =
                    ProjectJDay82SceneFlowState
                        .MainMenu;

                StatusText =
                    "MainMenu";

                return;
            }

            int mainMenuBuildIndex =
                SceneUtility.GetBuildIndexByScenePath(
                    "Assets/ProjectJ/Scenes/MainMenu.unity"
                );

            if (mainMenuBuildIndex < 0)
            {
                StatusText =
                    "MainMenu Scene이 Build Settings에 없습니다.";

                Debug.LogError(
                    "[Project J/Day82] MainMenu Build Index 없음"
                );

                return;
            }

            SceneManager.LoadScene(
                mainMenuBuildIndex,
                LoadSceneMode.Single
            );

            State =
                ProjectJDay82SceneFlowState
                    .MainMenu;

            StatusText =
                "MainMenu 진입";
        }
    }
}
