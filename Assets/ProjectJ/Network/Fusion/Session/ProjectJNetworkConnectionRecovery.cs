using System; // ArraySegment와 문자열 비교
using System.Collections.Generic; // Fusion Callback List
using Fusion; // NetworkRunner Callback API
using Fusion.Sockets; // NetAddress와 연결 실패 사유
using ProjectJ.Steam; // Steam 인증 재시도
using UnityEngine; // Runtime Service
using UnityEngine.SceneManagement; // MainMenu 안전 복귀

namespace ProjectJ.Networking.Fusion
{
    public enum ProjectJConnectionRecoveryState
    {
        WaitingForBootstrap = 0,
        Idle = 1,
        Monitoring = 2,
        AutoRetryWaiting = 3,
        Reconnecting = 4,
        Recovered = 5,
        Error = 6,
        ReturningToMainMenu = 7
    }

    public enum ProjectJConnectionError
    {
        None = 0,
        ConnectionFailed = 1,
        Disconnected = 2,
        Timeout = 3,
        SessionNotFound = 4,
        SessionClosed = 5,
        AuthenticationFailed = 6,
        SteamUnavailable = 7,
        HostDisconnected = 8,
        InvalidRoomCode = 9,
        ServerFull = 10,
        Unknown = 11
    }

    [DisallowMultipleComponent]
    public sealed class ProjectJNetworkConnectionRecovery :
        MonoBehaviour,
        INetworkRunnerCallbacks
    {
        private const float AutoRetryDelaySeconds =
            1.5f;

        private const int MaximumAutoRetryCount =
            1;

        private const string MainMenuScenePath =
            "Assets/ProjectJ/Scenes/MainMenu.unity";

        private static ProjectJNetworkConnectionRecovery instance;

        private ProjectJFusionBootstrap bootstrap;

        private ProjectJDay82SceneFlowCoordinator sceneFlow;

        private ProjectJSteamIdentityService steamIdentity;

        private NetworkRunner trackedRunner;

        private int trackedRunnerInstanceId =
            -1;

        private ProjectJFusionBootstrapState
            previousBootstrapState =
                ProjectJFusionBootstrapState.Idle;

        private GameMode? lastObservedMode;

        private bool trackedRunnerWasConnected;

        private bool autoRetryScheduled;

        private float autoRetryAt;

        private int autoRetryCount;

        private int manualRetryCount;

        private bool reconnectRequested;

        private bool reconnectLeaveRequested;

        private bool returnToMainMenuRequested;

        private bool mainMenuShutdownRequested;

        private int lastFailureRunnerInstanceId =
            -1;

        public static ProjectJNetworkConnectionRecovery Instance =>
            instance;

        public ProjectJConnectionRecoveryState State
        {
            get;
            private set;
        } =
            ProjectJConnectionRecoveryState.WaitingForBootstrap;

        public ProjectJConnectionError LastError
        {
            get;
            private set;
        } =
            ProjectJConnectionError.None;

        public string StatusMessage
        {
            get;
            private set;
        } =
            "Fusion Bootstrap 대기";

        public string ErrorDetail
        {
            get;
            private set;
        } =
            string.Empty;

        public string LastRoomCode
        {
            get;
            private set;
        } =
            string.Empty;

        public string LastSessionName
        {
            get;
            private set;
        } =
            string.Empty;

        public string LastModeText =>
            lastObservedMode.HasValue
                ? lastObservedMode.Value.ToString()
                : "-";

        public int AutoRetryCount =>
            autoRetryCount;

        public int ManualRetryCount =>
            manualRetryCount;

        public bool HasError =>
            LastError !=
                ProjectJConnectionError.None;

        public bool IsReconnectInProgress =>
            reconnectRequested ||
            State ==
                ProjectJConnectionRecoveryState
                    .AutoRetryWaiting ||
            State ==
                ProjectJConnectionRecoveryState
                    .Reconnecting;

        public bool HasReconnectTarget =>
            lastObservedMode ==
                GameMode.Client &&
            ProjectJFusionRoomCode.TryNormalize(
                LastRoomCode,
                out _,
                out _
            );

        public bool CanReconnect
        {
            get
            {
                return
                    HasReconnectTarget &&
                    !returnToMainMenuRequested &&
                    bootstrap != null &&
                    (
                        bootstrap.CanStart ||
                        bootstrap.CanShutdown
                    );
            }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad
        )]
        private static void Install()
        {
            if (instance != null)
            {
                return;
            }

            ProjectJNetworkConnectionRecovery existing =
                FindFirstObjectByType<
                    ProjectJNetworkConnectionRecovery
                >();

            if (existing != null)
            {
                instance =
                    existing;

                DontDestroyOnLoad(
                    existing.gameObject
                );

                EnsureDebugView(
                    existing.gameObject
                );

                return;
            }

            GameObject recoveryObject =
                new GameObject(
                    "=== Project J Connection Recovery ==="
                );

            DontDestroyOnLoad(
                recoveryObject
            );

            instance =
                recoveryObject.AddComponent<
                    ProjectJNetworkConnectionRecovery
                >();

            EnsureDebugView(
                recoveryObject
            );
        }

        private static void EnsureDebugView(
            GameObject target
        )
        {
            if (
                target.GetComponent<
                    ProjectJDay83ConnectionRecoveryDebugView
                >() == null
            )
            {
                target.AddComponent<
                    ProjectJDay83ConnectionRecoveryDebugView
                >();
            }
        }

        private void Awake()
        {
            if (
                instance != null &&
                instance != this
            )
            {
                Destroy(
                    gameObject
                );

                return;
            }

            instance =
                this;

            DontDestroyOnLoad(
                gameObject
            );
        }

        private void Update()
        {
            ResolveReferences();
            ObserveBootstrapAttempt();
            ObserveRunner();
            ObserveBootstrapFailure();
            ProcessReconnectRequest();
            ProcessMainMenuRequest();
            RefreshConnectedState();

            if (bootstrap != null)
            {
                previousBootstrapState =
                    bootstrap.State;
            }
        }

        private void OnDestroy()
        {
            if (trackedRunner != null)
            {
                trackedRunner.RemoveCallbacks(
                    this
                );
            }

            if (instance == this)
            {
                instance =
                    null;
            }
        }

        public bool RequestReconnect()
        {
            if (!HasReconnectTarget)
            {
                LastError =
                    ProjectJConnectionError
                        .SessionNotFound;

                State =
                    ProjectJConnectionRecoveryState
                        .Error;

                StatusMessage =
                    "재접속할 이전 Room Code가 없습니다.";

                ErrorDetail =
                    "LastRoomCode 없음";

                return false;
            }

            autoRetryScheduled =
                false;

            reconnectRequested =
                true;

            reconnectLeaveRequested =
                false;

            manualRetryCount++;

            State =
                ProjectJConnectionRecoveryState
                    .Reconnecting;

            StatusMessage =
                "사용자 재접속 요청 / Room " +
                LastRoomCode;

            return true;
        }

        public void RequestMainMenu()
        {
            autoRetryScheduled =
                false;

            reconnectRequested =
                false;

            reconnectLeaveRequested =
                false;

            returnToMainMenuRequested =
                true;

            mainMenuShutdownRequested =
                false;

            State =
                ProjectJConnectionRecoveryState
                    .ReturningToMainMenu;

            StatusMessage =
                "MainMenu 복귀 준비";
        }

        public void RequestSteamRetry()
        {
            ResolveReferences();

            if (steamIdentity == null)
            {
                LastError =
                    ProjectJConnectionError
                        .SteamUnavailable;

                State =
                    ProjectJConnectionRecoveryState
                        .Error;

                StatusMessage =
                    "Steam Identity Service를 찾을 수 없습니다.";

                return;
            }

            steamIdentity.TryInitialize();

            StatusMessage =
                "Steam 인증 다시 시도";
        }

        public void ClearError()
        {
            LastError =
                ProjectJConnectionError.None;

            ErrorDetail =
                string.Empty;

            autoRetryScheduled =
                false;

            reconnectRequested =
                false;

            reconnectLeaveRequested =
                false;

            if (
                bootstrap != null &&
                bootstrap.State ==
                    ProjectJFusionBootstrapState.Running
            )
            {
                State =
                    ProjectJConnectionRecoveryState
                        .Monitoring;

                StatusMessage =
                    "연결 상태 감시 중";
            }
            else
            {
                State =
                    ProjectJConnectionRecoveryState.Idle;

                StatusMessage =
                    "재접속 대기";
            }
        }

        private void ResolveReferences()
        {
            if (bootstrap == null)
            {
                bootstrap =
                    FindFirstObjectByType<
                        ProjectJFusionBootstrap
                    >();
            }

            if (sceneFlow == null)
            {
                sceneFlow =
                    FindFirstObjectByType<
                        ProjectJDay82SceneFlowCoordinator
                    >();
            }

            if (steamIdentity == null)
            {
                steamIdentity =
                    ProjectJSteamIdentityService
                        .Instance;
            }

            if (
                bootstrap == null &&
                !returnToMainMenuRequested
            )
            {
                State =
                    ProjectJConnectionRecoveryState
                        .WaitingForBootstrap;

                StatusMessage =
                    "Fusion Bootstrap 대기";
            }
        }

        private void ObserveBootstrapAttempt()
        {
            if (bootstrap == null)
            {
                return;
            }

            if (
                bootstrap.State !=
                    ProjectJFusionBootstrapState.Starting
            )
            {
                return;
            }

            if (bootstrap.ActiveMode.HasValue)
            {
                lastObservedMode =
                    bootstrap.ActiveMode.Value;
            }

            if (
                lastObservedMode ==
                    GameMode.Client &&
                ProjectJFusionRoomCode.TryNormalize(
                    bootstrap.RoomCode,
                    out string normalizedCode,
                    out _
                )
            )
            {
                LastRoomCode =
                    normalizedCode;

                LastSessionName =
                    ProjectJFusionRoomCode
                        .ToSessionName(
                            normalizedCode
                        );
            }
        }

        private void ObserveRunner()
        {
            if (bootstrap == null)
            {
                return;
            }

            NetworkRunner currentRunner =
                bootstrap.Runner;

            if (
                currentRunner != null &&
                currentRunner != trackedRunner
            )
            {
                AttachRunner(
                    currentRunner
                );
            }

            if (
                trackedRunner == null ||
                trackedRunner == currentRunner
            )
            {
                return;
            }

            int previousRunnerId =
                trackedRunnerInstanceId;

            bool wasConnected =
                trackedRunnerWasConnected;

            trackedRunner =
                currentRunner;

            trackedRunnerInstanceId =
                currentRunner != null
                    ? currentRunner.GetInstanceID()
                    : -1;

            trackedRunnerWasConnected =
                false;

            if (
                currentRunner != null
            )
            {
                currentRunner.AddCallbacks(
                    this
                );
            }

            if (
                previousBootstrapState ==
                    ProjectJFusionBootstrapState
                        .Stopping ||
                bootstrap.LastConnectionResult ==
                    "정상 종료"
            )
            {
                return;
            }

            if (
                wasConnected &&
                previousRunnerId !=
                    lastFailureRunnerInstanceId
            )
            {
                ProjectJConnectionError error =
                    lastObservedMode ==
                        GameMode.Client
                        ? ProjectJConnectionError
                            .HostDisconnected
                        : ProjectJConnectionError
                            .Disconnected;

                HandleFailure(
                    previousRunnerId,
                    error,
                    "NetworkRunner가 예기치 않게 종료되었습니다.",
                    ShouldAutoRetry(
                        error
                    )
                );
            }
        }

        private void AttachRunner(
            NetworkRunner runner
        )
        {
            if (runner == null)
            {
                return;
            }

            trackedRunner =
                runner;

            trackedRunnerInstanceId =
                runner.GetInstanceID();

            trackedRunnerWasConnected =
                runner.IsRunning;

            if (
                bootstrap != null &&
                bootstrap.ActiveMode.HasValue
            )
            {
                lastObservedMode =
                    bootstrap.ActiveMode.Value;
            }

            runner.AddCallbacks(
                this
            );

            if (
                bootstrap != null &&
                ProjectJFusionRoomCode.TryNormalize(
                    bootstrap.ConnectedRoomCode,
                    out string roomCode,
                    out _
                )
            )
            {
                LastRoomCode =
                    roomCode;

                LastSessionName =
                    bootstrap.ConnectedSessionName;
            }

            if (!HasError)
            {
                State =
                    ProjectJConnectionRecoveryState
                        .Monitoring;

                StatusMessage =
                    "NetworkRunner 연결 상태 감시 중";
            }
        }

        private void ObserveBootstrapFailure()
        {
            if (bootstrap == null)
            {
                return;
            }

            if (
                bootstrap.State !=
                    ProjectJFusionBootstrapState.Failed ||
                previousBootstrapState ==
                    ProjectJFusionBootstrapState.Failed
            )
            {
                return;
            }

            if (
                trackedRunnerInstanceId ==
                    lastFailureRunnerInstanceId
            )
            {
                return;
            }

            string detail =
                bootstrap.StatusMessage +
                " / " +
                bootstrap.LastConnectionResult;

            ProjectJConnectionError error =
                ClassifyBootstrapFailure(
                    detail
                );

            HandleFailure(
                trackedRunnerInstanceId,
                error,
                detail,
                ShouldAutoRetry(
                    error
                )
            );
        }

        private void RefreshConnectedState()
        {
            if (
                bootstrap == null ||
                bootstrap.State !=
                    ProjectJFusionBootstrapState.Running ||
                bootstrap.Runner == null ||
                !bootstrap.Runner.IsRunning
            )
            {
                return;
            }

            trackedRunnerWasConnected =
                true;

            if (
                bootstrap.ActiveMode.HasValue
            )
            {
                lastObservedMode =
                    bootstrap.ActiveMode.Value;
            }

            if (
                ProjectJFusionRoomCode.TryNormalize(
                    bootstrap.ConnectedRoomCode,
                    out string roomCode,
                    out _
                )
            )
            {
                LastRoomCode =
                    roomCode;

                LastSessionName =
                    bootstrap.ConnectedSessionName;
            }

            if (
                State ==
                    ProjectJConnectionRecoveryState
                        .Reconnecting ||
                State ==
                    ProjectJConnectionRecoveryState
                        .AutoRetryWaiting ||
                HasError
            )
            {
                LastError =
                    ProjectJConnectionError.None;

                ErrorDetail =
                    string.Empty;

                reconnectRequested =
                    false;

                reconnectLeaveRequested =
                    false;

                autoRetryScheduled =
                    false;

                autoRetryCount =
                    0;

                State =
                    ProjectJConnectionRecoveryState
                        .Recovered;

                StatusMessage =
                    "연결 복구 완료 / Room " +
                    LastRoomCode;

                return;
            }

            if (
                State !=
                    ProjectJConnectionRecoveryState
                        .Monitoring
            )
            {
                State =
                    ProjectJConnectionRecoveryState
                        .Monitoring;

                StatusMessage =
                    "연결 상태 정상";
            }
        }

        private void ProcessReconnectRequest()
        {
            if (
                autoRetryScheduled &&
                Time.unscaledTime >=
                    autoRetryAt
            )
            {
                autoRetryScheduled =
                    false;

                reconnectRequested =
                    true;

                reconnectLeaveRequested =
                    false;

                State =
                    ProjectJConnectionRecoveryState
                        .Reconnecting;

                StatusMessage =
                    "자동 재접속 시도 / Room " +
                    LastRoomCode;
            }

            if (
                !reconnectRequested ||
                bootstrap == null
            )
            {
                return;
            }

            if (
                !ProjectJFusionRoomCode.TryNormalize(
                    LastRoomCode,
                    out string normalizedCode,
                    out _
                )
            )
            {
                reconnectRequested =
                    false;

                HandleFailure(
                    trackedRunnerInstanceId,
                    ProjectJConnectionError
                        .SessionNotFound,
                    "재접속 Room Code가 유효하지 않습니다.",
                    false
                );

                return;
            }

            if (
                steamIdentity == null ||
                !steamIdentity.IsAuthenticated
            )
            {
                reconnectRequested =
                    false;

                ProjectJConnectionError steamError =
                    ClassifySteamError();

                HandleFailure(
                    trackedRunnerInstanceId,
                    steamError,
                    steamIdentity == null
                        ? "Steam Identity Service 없음"
                        : steamIdentity.StatusMessage,
                    false
                );

                return;
            }

            if (
                bootstrap.CanShutdown &&
                !reconnectLeaveRequested
            )
            {
                reconnectLeaveRequested =
                    true;

                bootstrap.RequestLeaveRoom();

                StatusMessage =
                    "기존 Runner 종료 후 재접속 준비";

                return;
            }

            if (!bootstrap.CanStart)
            {
                return;
            }

            reconnectRequested =
                false;

            reconnectLeaveRequested =
                false;

            bootstrap.RoomCode =
                normalizedCode;

            bootstrap.RequestJoinPrivateRoom();

            State =
                ProjectJConnectionRecoveryState
                    .Reconnecting;

            StatusMessage =
                "새 NetworkRunner로 Room 재참가 중: " +
                normalizedCode;
        }

        private void ProcessMainMenuRequest()
        {
            if (!returnToMainMenuRequested)
            {
                return;
            }

            State =
                ProjectJConnectionRecoveryState
                    .ReturningToMainMenu;

            if (sceneFlow != null)
            {
                returnToMainMenuRequested =
                    false;

                sceneFlow.RequestLeaveToMainMenu();

                StatusMessage =
                    "82일차 Scene Flow를 통해 MainMenu 복귀";

                return;
            }

            if (
                bootstrap != null &&
                bootstrap.CanShutdown &&
                !mainMenuShutdownRequested
            )
            {
                mainMenuShutdownRequested =
                    true;

                bootstrap.RequestLeaveRoom();

                StatusMessage =
                    "Fusion Runner 종료 후 MainMenu 복귀";

                return;
            }

            if (
                bootstrap != null &&
                !bootstrap.CanStart
            )
            {
                return;
            }

            int buildIndex =
                SceneUtility.GetBuildIndexByScenePath(
                    MainMenuScenePath
                );

            if (buildIndex < 0)
            {
                returnToMainMenuRequested =
                    false;

                LastError =
                    ProjectJConnectionError.Unknown;

                State =
                    ProjectJConnectionRecoveryState
                        .Error;

                StatusMessage =
                    "MainMenu Scene Build Index 없음";

                ErrorDetail =
                    MainMenuScenePath;

                return;
            }

            returnToMainMenuRequested =
                false;

            mainMenuShutdownRequested =
                false;

            SceneManager.LoadScene(
                buildIndex,
                LoadSceneMode.Single
            );

            StatusMessage =
                "MainMenu 복귀 완료";
        }

        private void ScheduleAutoRetry()
        {
            if (
                autoRetryCount >=
                    MaximumAutoRetryCount ||
                !HasReconnectTarget
            )
            {
                return;
            }

            autoRetryCount++;

            autoRetryScheduled =
                true;

            autoRetryAt =
                Time.unscaledTime +
                AutoRetryDelaySeconds;

            State =
                ProjectJConnectionRecoveryState
                    .AutoRetryWaiting;

            StatusMessage =
                "연결 손실 / " +
                AutoRetryDelaySeconds.ToString("F1") +
                "초 후 자동 재접속 1회";
        }

        private void HandleFailure(
            int runnerInstanceId,
            ProjectJConnectionError error,
            string detail,
            bool allowAutoRetry
        )
        {
            if (
                runnerInstanceId >= 0 &&
                runnerInstanceId ==
                    lastFailureRunnerInstanceId
            )
            {
                return;
            }

            if (runnerInstanceId >= 0)
            {
                lastFailureRunnerInstanceId =
                    runnerInstanceId;
            }

            LastError =
                error;

            ErrorDetail =
                detail ?? string.Empty;

            reconnectRequested =
                false;

            reconnectLeaveRequested =
                false;

            if (
                allowAutoRetry &&
                autoRetryCount <
                    MaximumAutoRetryCount
            )
            {
                ScheduleAutoRetry();
                return;
            }

            State =
                ProjectJConnectionRecoveryState.Error;

            StatusMessage =
                GetUserMessage(
                    error
                );

            Debug.LogWarning(
                "[Project J/Day83] " +
                StatusMessage +
                " / Detail: " +
                ErrorDetail
            );
        }

        private ProjectJConnectionError
            ClassifyBootstrapFailure(
                string detail
            )
        {
            string value =
                detail ?? string.Empty;

            if (
                ContainsIgnoreCase(
                    value,
                    "Steam 인증"
                )
            )
            {
                return
                    ClassifySteamError();
            }

            if (
                ContainsIgnoreCase(
                    value,
                    "방 코드 입력 오류"
                )
            )
            {
                return
                    ProjectJConnectionError
                        .InvalidRoomCode;
            }

            if (
                ContainsIgnoreCase(
                    value,
                    "GameNotFound"
                ) ||
                ContainsIgnoreCase(
                    value,
                    "GameDoesNotExist"
                )
            )
            {
                return
                    ProjectJConnectionError
                        .SessionNotFound;
            }

            if (
                ContainsIgnoreCase(
                    value,
                    "GameClosed"
                ) ||
                ContainsIgnoreCase(
                    value,
                    "Closed"
                )
            )
            {
                return
                    ProjectJConnectionError
                        .SessionClosed;
            }

            if (
                ContainsIgnoreCase(
                    value,
                    "GameFull"
                ) ||
                ContainsIgnoreCase(
                    value,
                    "ServerFull"
                )
            )
            {
                return
                    ProjectJConnectionError
                        .ServerFull;
            }

            if (
                ContainsIgnoreCase(
                    value,
                    "Timeout"
                )
            )
            {
                return
                    ProjectJConnectionError
                        .Timeout;
            }

            if (
                ContainsIgnoreCase(
                    value,
                    "Auth"
                )
            )
            {
                return
                    ProjectJConnectionError
                        .AuthenticationFailed;
            }

            return
                ProjectJConnectionError
                    .ConnectionFailed;
        }

        private ProjectJConnectionError
            ClassifySteamError()
        {
            if (steamIdentity == null)
            {
                return
                    ProjectJConnectionError
                        .SteamUnavailable;
            }

            switch (steamIdentity.State)
            {
                case ProjectJSteamAuthState
                    .SteamUnavailable:
                case ProjectJSteamAuthState
                    .PackageMissing:
                    return
                        ProjectJConnectionError
                            .SteamUnavailable;

                default:
                    return
                        ProjectJConnectionError
                            .AuthenticationFailed;
            }
        }

        private static ProjectJConnectionError
            ClassifyConnectFailure(
                NetConnectFailedReason reason
            )
        {
            switch (reason)
            {
                case NetConnectFailedReason.Timeout:
                    return
                        ProjectJConnectionError
                            .Timeout;

                case NetConnectFailedReason.ServerFull:
                    return
                        ProjectJConnectionError
                            .ServerFull;

                default:
                    return
                        ProjectJConnectionError
                            .ConnectionFailed;
            }
        }

        private ProjectJConnectionError
            ClassifyDisconnect(
                NetDisconnectReason reason
            )
        {
            switch (reason)
            {
                case NetDisconnectReason.Timeout:
                    return
                        ProjectJConnectionError
                            .Timeout;

                case NetDisconnectReason.ByRemote:
                    return
                        lastObservedMode ==
                            GameMode.Client
                            ? ProjectJConnectionError
                                .HostDisconnected
                            : ProjectJConnectionError
                                .Disconnected;

                default:
                    return
                        ProjectJConnectionError
                            .Disconnected;
            }
        }

        private ProjectJConnectionError
            ClassifyShutdown(
                ShutdownReason reason
            )
        {
            string value =
                reason.ToString();

            if (
                ContainsIgnoreCase(
                    value,
                    "GameNotFound"
                ) ||
                ContainsIgnoreCase(
                    value,
                    "GameDoesNotExist"
                )
            )
            {
                return
                    ProjectJConnectionError
                        .SessionNotFound;
            }

            if (
                ContainsIgnoreCase(
                    value,
                    "GameClosed"
                ) ||
                ContainsIgnoreCase(
                    value,
                    "Closed"
                )
            )
            {
                return
                    ProjectJConnectionError
                        .SessionClosed;
            }

            if (
                ContainsIgnoreCase(
                    value,
                    "Timeout"
                )
            )
            {
                return
                    ProjectJConnectionError
                        .Timeout;
            }

            if (
                ContainsIgnoreCase(
                    value,
                    "Auth"
                )
            )
            {
                return
                    ProjectJConnectionError
                        .AuthenticationFailed;
            }

            if (
                lastObservedMode ==
                    GameMode.Client &&
                trackedRunnerWasConnected
            )
            {
                return
                    ProjectJConnectionError
                        .HostDisconnected;
            }

            return
                ProjectJConnectionError
                    .Disconnected;
        }

        private static bool ShouldAutoRetry(
            ProjectJConnectionError error
        )
        {
            return
                error ==
                    ProjectJConnectionError.Timeout ||
                error ==
                    ProjectJConnectionError
                        .Disconnected;
        }

        private static bool ContainsIgnoreCase(
            string source,
            string value
        )
        {
            return
                !string.IsNullOrEmpty(
                    source
                ) &&
                source.IndexOf(
                    value,
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;
        }

        public static string GetUserMessage(
            ProjectJConnectionError error
        )
        {
            switch (error)
            {
                case ProjectJConnectionError
                    .ConnectionFailed:
                    return
                        "온라인 연결에 실패했습니다.";

                case ProjectJConnectionError
                    .Disconnected:
                    return
                        "서버 연결이 끊어졌습니다.";

                case ProjectJConnectionError
                    .Timeout:
                    return
                        "네트워크 응답 시간이 초과되었습니다.";

                case ProjectJConnectionError
                    .SessionNotFound:
                    return
                        "방을 찾을 수 없습니다.";

                case ProjectJConnectionError
                    .SessionClosed:
                    return
                        "경기가 이미 시작되었거나 방이 닫혔습니다.";

                case ProjectJConnectionError
                    .AuthenticationFailed:
                    return
                        "Steam 인증에 실패했습니다.";

                case ProjectJConnectionError
                    .SteamUnavailable:
                    return
                        "Steam을 사용할 수 없습니다.";

                case ProjectJConnectionError
                    .HostDisconnected:
                    return
                        "Host가 방을 종료했거나 연결이 끊어졌습니다.";

                case ProjectJConnectionError
                    .InvalidRoomCode:
                    return
                        "올바른 Room Code를 입력해주세요.";

                case ProjectJConnectionError
                    .ServerFull:
                    return
                        "방의 최대 인원에 도달했습니다.";

                case ProjectJConnectionError.Unknown:
                    return
                        "알 수 없는 온라인 오류가 발생했습니다.";

                default:
                    return
                        "온라인 연결 상태가 정상입니다.";
            }
        }

        public void OnConnectedToServer(
            NetworkRunner runner
        )
        {
            if (
                runner == null ||
                runner != trackedRunner
            )
            {
                return;
            }

            trackedRunnerWasConnected =
                true;

            autoRetryScheduled =
                false;

            reconnectRequested =
                false;

            reconnectLeaveRequested =
                false;

            LastError =
                ProjectJConnectionError.None;

            ErrorDetail =
                string.Empty;

            State =
                ProjectJConnectionRecoveryState
                    .Monitoring;

            StatusMessage =
                "Fusion 서버 연결 성공";
        }

        public void OnConnectFailed(
            NetworkRunner runner,
            NetAddress remoteAddress,
            NetConnectFailedReason reason
        )
        {
            ProjectJConnectionError error =
                ClassifyConnectFailure(
                    reason
                );

            HandleFailure(
                runner != null
                    ? runner.GetInstanceID()
                    : -1,
                error,
                "OnConnectFailed / " +
                reason +
                " / " +
                remoteAddress,
                ShouldAutoRetry(
                    error
                )
            );
        }

        public void OnDisconnectedFromServer(
            NetworkRunner runner,
            NetDisconnectReason reason
        )
        {
            if (
                bootstrap != null &&
                bootstrap.State ==
                    ProjectJFusionBootstrapState.Stopping
            )
            {
                return;
            }

            ProjectJConnectionError error =
                ClassifyDisconnect(
                    reason
                );

            HandleFailure(
                runner != null
                    ? runner.GetInstanceID()
                    : -1,
                error,
                "OnDisconnectedFromServer / " +
                reason,
                ShouldAutoRetry(
                    error
                )
            );
        }

        public void OnShutdown(
            NetworkRunner runner,
            ShutdownReason shutdownReason
        )
        {
            if (
                bootstrap != null &&
                bootstrap.State ==
                    ProjectJFusionBootstrapState.Stopping
            )
            {
                return;
            }

            if (
                shutdownReason ==
                    ShutdownReason.Ok &&
                !trackedRunnerWasConnected
            )
            {
                return;
            }

            ProjectJConnectionError error =
                ClassifyShutdown(
                    shutdownReason
                );

            HandleFailure(
                runner != null
                    ? runner.GetInstanceID()
                    : -1,
                error,
                "OnShutdown / " +
                shutdownReason,
                ShouldAutoRetry(
                    error
                )
            );
        }

        public void OnPlayerJoined(
            NetworkRunner runner,
            PlayerRef player
        )
        {
        }

        public void OnPlayerLeft(
            NetworkRunner runner,
            PlayerRef player
        )
        {
        }

        public void OnInput(
            NetworkRunner runner,
            NetworkInput input
        )
        {
        }

        public void OnInputMissing(
            NetworkRunner runner,
            PlayerRef player,
            NetworkInput input
        )
        {
        }

        public void OnConnectRequest(
            NetworkRunner runner,
            NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token
        )
        {
        }

#pragma warning disable CS0618

        public void OnUserSimulationMessage(
            NetworkRunner runner,
            SimulationMessagePtr message
        )
        {
        }

#pragma warning restore CS0618

        public void OnSessionListUpdated(
            NetworkRunner runner,
            List<SessionInfo> sessionList
        )
        {
        }

        public void OnCustomAuthenticationResponse(
            NetworkRunner runner,
            Dictionary<string, object> data
        )
        {
        }

        public void OnHostMigration(
            NetworkRunner runner,
            HostMigrationToken hostMigrationToken
        )
        {
        }

        public void OnSceneLoadDone(
            NetworkRunner runner
        )
        {
        }

        public void OnSceneLoadStart(
            NetworkRunner runner
        )
        {
        }

        public void OnObjectExitAOI(
            NetworkRunner runner,
            NetworkObject obj,
            PlayerRef player
        )
        {
        }

        public void OnObjectEnterAOI(
            NetworkRunner runner,
            NetworkObject obj,
            PlayerRef player
        )
        {
        }

        public void OnReliableDataReceived(
            NetworkRunner runner,
            PlayerRef player,
            ReliableKey key,
            ReadOnlySpan<byte> data
        )
        {
        }

        public void OnReliableDataProgress(
            NetworkRunner runner,
            PlayerRef player,
            ReliableKey key,
            float progress
        )
        {
        }
    }
}
