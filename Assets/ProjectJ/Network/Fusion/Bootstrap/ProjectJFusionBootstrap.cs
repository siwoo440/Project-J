using System;
using System.Threading.Tasks;
using Fusion;
using Photon.Realtime;
using ProjectJ.Steam;
using UnityEngine;

namespace ProjectJ.Networking.Fusion
{
    public enum ProjectJFusionBootstrapState
    {
        Idle = 0,
        Starting = 1,
        Running = 2,
        Stopping = 3,
        Failed = 4
    }

    [DisallowMultipleComponent]
    public sealed class ProjectJFusionBootstrap :
        MonoBehaviour
    {
        private const int PrivateRoomPlayerCount =
            8;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static readonly string
            DevelopmentFusionInstanceId =
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 8);
#endif

        private NetworkRunner runner;
        private GameObject runnerObject;

        private ProjectJNetworkPlayerSpawner
            playerSpawner;

        private ProjectJFusionInputProvider
            inputProvider;

        public ProjectJFusionBootstrapState State
        {
            get;
            private set;
        } =
            ProjectJFusionBootstrapState.Idle;

        public NetworkRunner Runner =>
            runner;

        public ProjectJNetworkPlayerSpawner
            PlayerSpawner =>
                playerSpawner;

        public ProjectJFusionInputProvider
            InputProvider =>
                inputProvider;

        public GameMode? ActiveMode
        {
            get;
            private set;
        }

        public string RoomCode
        {
            get;
            set;
        } =
            string.Empty;

        public string SessionName
        {
            get;
            set;
        } =
            string.Empty;

        public string StatusMessage
        {
            get;
            private set;
        } =
            "대기 중";

        public string LastConnectionResult
        {
            get;
            private set;
        } =
            "아직 연결 시도 없음";

        public bool CanStart =>
            State ==
                ProjectJFusionBootstrapState.Idle ||
            State ==
                ProjectJFusionBootstrapState.Failed;

        public bool CanShutdown =>
            State ==
                ProjectJFusionBootstrapState.Running;

        public int ParticipantCount
        {
            get
            {
                if (
                    runner == null ||
                    !runner.IsRunning
                )
                {
                    return 0;
                }

                int count = 0;

                foreach (
                    PlayerRef player
                    in runner.ActivePlayers
                )
                {
                    count++;
                }

                return count;
            }
        }

        public int SpawnedPlayerCount
        {
            get
            {
                if (
                    runner == null ||
                    !runner.IsRunning
                )
                {
                    return 0;
                }

                int count = 0;

                foreach (
                    PlayerRef player
                    in runner.ActivePlayers
                )
                {
                    if (
                        runner.TryGetPlayerObject(
                            player,
                            out NetworkObject playerObject
                        ) &&
                        playerObject != null
                    )
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public bool HasValidSessionInfo =>
            runner != null &&
            runner.IsRunning &&
            runner.SessionInfo.IsValid;

        public string ConnectedSessionName =>
            HasValidSessionInfo
                ? runner.SessionInfo.Name
                : "-";

        public string ConnectedRegion =>
            HasValidSessionInfo
                ? runner.SessionInfo.Region
                : "-";

        public bool IsSessionVisible =>
            HasValidSessionInfo &&
            runner.SessionInfo.IsVisible;

        public bool IsSessionOpen =>
            HasValidSessionInfo &&
            runner.SessionInfo.IsOpen;

        public string ConnectedRoomCode
        {
            get
            {
                if (
                    HasValidSessionInfo &&
                    ProjectJFusionRoomCode
                        .TryExtractFromSessionName(
                            runner.SessionInfo.Name,
                            out string connectedCode
                        )
                )
                {
                    return connectedCode;
                }

                return string.IsNullOrEmpty(
                    RoomCode
                )
                    ? "-"
                    : RoomCode;
            }
        }

        private void Update()
        {
            if (
                State ==
                    ProjectJFusionBootstrapState.Running &&
                (
                    runner == null ||
                    !runner.IsRunning
                )
            )
            {
                runner = null;
                runnerObject = null;
                playerSpawner = null;
                inputProvider = null;
                ActiveMode = null;

                State =
                    ProjectJFusionBootstrapState.Idle;

                StatusMessage =
                    "연결이 종료되었습니다.";

                LastConnectionResult =
                    "연결 종료";
            }
        }

        private void OnApplicationQuit()
        {
            if (
                runner != null &&
                runner.IsRunning
            )
            {
                _ = runner.Shutdown();
            }
        }

        public void RequestCreatePrivateRoom()
        {
            if (!CanStart)
            {
                return;
            }

            string generatedCode =
                ProjectJFusionRoomCode
                    .Generate();

            RoomCode =
                generatedCode;

            SessionName =
                ProjectJFusionRoomCode
                    .ToSessionName(
                        generatedCode
                    );

            _ = StartRunnerAsync(
                GameMode.Host,
                SessionName
            );
        }

        public void RequestJoinPrivateRoom()
        {
            if (!CanStart)
            {
                return;
            }

            bool isValid =
                ProjectJFusionRoomCode
                    .TryNormalize(
                        RoomCode,
                        out string normalizedCode,
                        out string errorMessage
                    );

            if (!isValid)
            {
                State =
                    ProjectJFusionBootstrapState.Failed;

                StatusMessage =
                    errorMessage;

                LastConnectionResult =
                    "방 코드 입력 오류";

                return;
            }

            RoomCode =
                normalizedCode;

            SessionName =
                ProjectJFusionRoomCode
                    .ToSessionName(
                        normalizedCode
                    );

            _ = StartRunnerAsync(
                GameMode.Client,
                SessionName
            );
        }

        public void RequestLeaveRoom()
        {
            if (!CanShutdown)
            {
                return;
            }

            _ = ShutdownRunnerAsync();
        }

        public void RequestStartHost()
        {
            RequestCreatePrivateRoom();
        }

        public void RequestStartClient()
        {
            RequestJoinPrivateRoom();
        }

        public void RequestShutdown()
        {
            RequestLeaveRoom();
        }

        private async Task StartRunnerAsync(
            GameMode gameMode,
            string sessionName
        )
        {
            await DestroyPreviousRunnerAsync();

            if (
                !ProjectJSteamIdentityService
                    .TryGetAuthenticated(
                        out ProjectJSteamIdentityService
                            steamIdentity
                    )
            )
            {
                State =
                    ProjectJFusionBootstrapState.Failed;

                ActiveMode =
                    null;

                ProjectJSteamIdentityService
                    currentIdentity =
                        ProjectJSteamIdentityService
                            .Instance;

                StatusMessage =
                    currentIdentity == null
                        ? "Steam 인증 서비스가 없습니다."
                        : "Steam 인증 필요: " +
                            currentIdentity
                                .StatusMessage;

                LastConnectionResult =
                    "Steam 인증 실패";

                Debug.LogWarning(
                    "[Project J/Fusion] " +
                    StatusMessage
                );

                return;
            }

            State =
                ProjectJFusionBootstrapState.Starting;

            ActiveMode =
                gameMode;

            StatusMessage =
                gameMode == GameMode.Host
                    ? "비공개 방 생성 중..."
                    : "방 코드로 참가 중...";

            LastConnectionResult =
                "연결 시도 중";

            runnerObject =
                new GameObject(
                    "=== Fusion NetworkRunner ==="
                );

            runner =
                runnerObject.AddComponent<
                    NetworkRunner
                >();

            runner.ProvideInput =
                true;

            inputProvider =
                runnerObject.AddComponent<
                    ProjectJFusionInputProvider
                >();

            runner.AddCallbacks(
                inputProvider
            );

            GameObject playerPrefabObject =
                Resources.Load<GameObject>(
                    "ProjectJNetworkPlayer"
                );

            NetworkObject playerPrefab =
                playerPrefabObject != null
                    ? playerPrefabObject
                        .GetComponent<
                            NetworkObject
                        >()
                    : null;

            if (playerPrefab == null)
            {
                State =
                    ProjectJFusionBootstrapState.Failed;

                StatusMessage =
                    "Network Player Prefab을 찾을 수 없습니다.";

                LastConnectionResult =
                    "Player Prefab 없음";

                Destroy(
                    runnerObject
                );

                runner = null;
                runnerObject = null;
                inputProvider = null;
                ActiveMode = null;

                Debug.LogError(
                    "[Project J/Fusion] " +
                    StatusMessage
                );

                return;
            }

            playerSpawner =
                runnerObject.AddComponent<
                    ProjectJNetworkPlayerSpawner
                >();

            playerSpawner.Configure(
                playerPrefab
            );

            NetworkSceneManagerDefault
                sceneManager =
                    runnerObject.AddComponent<
                        NetworkSceneManagerDefault
                    >();

            string fusionUserId =
                BuildFusionUserId(
                    steamIdentity
                );

            AuthenticationValues
                authenticationValues =
                    new AuthenticationValues(
                        fusionUserId
                    );

            StartGameArgs startArgs =
                new StartGameArgs
                {
                    GameMode =
                        gameMode,
                    SessionName =
                        sessionName,
                    SceneManager =
                        sceneManager,
                    AuthValues =
                        authenticationValues
                };

            if (gameMode == GameMode.Host)
            {
                startArgs.IsVisible =
                    false;

                startArgs.IsOpen =
                    true;

                startArgs.PlayerCount =
                    PrivateRoomPlayerCount;
            }
            else if (
                gameMode == GameMode.Client
            )
            {
                startArgs
                    .EnableClientSessionCreation =
                        false;
            }

            StartGameResult result =
                await runner.StartGame(
                    startArgs
                );

            if (!result.Ok)
            {
                string failureMessage =
                    result.ShutdownReason
                        .ToString();

                State =
                    ProjectJFusionBootstrapState.Failed;

                StatusMessage =
                    gameMode == GameMode.Host
                        ? "방 생성 실패: " +
                            failureMessage
                        : "방 참가 실패: " +
                            failureMessage;

                LastConnectionResult =
                    StatusMessage;

                Debug.LogError(
                    "[Project J/Fusion] " +
                    StatusMessage
                );

                await DestroyPreviousRunnerAsync();

                ActiveMode = null;

                return;
            }

            State =
                ProjectJFusionBootstrapState.Running;

            if (gameMode == GameMode.Host)
            {
                StatusMessage =
                    "비공개 방 생성 완료";

                LastConnectionResult =
                    "Host 연결 성공";
            }
            else
            {
                StatusMessage =
                    "방 코드 참가 완료";

                LastConnectionResult =
                    "Client 연결 성공";
            }

            Debug.Log(
                "[Project J/Fusion] " +
                StatusMessage +
                " / 방 코드: " +
                ConnectedRoomCode +
                " / 세션: " +
                sessionName +
                " / SteamProjectAccountId: " +
                steamIdentity.ProjectAccountId +
                " / FusionUserId: " +
                fusionUserId +
                " / ProvideInput: " +
                runner.ProvideInput
            );
        }

        private static string BuildFusionUserId(
            ProjectJSteamIdentityService steamIdentity
        )
        {
            string baseUserId =
                steamIdentity.ProjectAccountId;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return
                baseUserId +
                "-dev-" +
                DevelopmentFusionInstanceId;
#else
            return baseUserId;
#endif
        }

        private async Task ShutdownRunnerAsync()
        {
            if (runner == null)
            {
                State =
                    ProjectJFusionBootstrapState.Idle;

                ActiveMode = null;
                playerSpawner = null;
                inputProvider = null;

                StatusMessage =
                    "대기 중";

                return;
            }

            State =
                ProjectJFusionBootstrapState.Stopping;

            StatusMessage =
                "방 나가는 중...";

            NetworkRunner targetRunner =
                runner;

            runner = null;
            playerSpawner = null;
            inputProvider = null;

            if (targetRunner.IsRunning)
            {
                await targetRunner.Shutdown();
            }

            if (runnerObject != null)
            {
                Destroy(
                    runnerObject
                );
            }

            runnerObject = null;
            ActiveMode = null;

            State =
                ProjectJFusionBootstrapState.Idle;

            StatusMessage =
                "방 나가기 완료";

            LastConnectionResult =
                "정상 종료";

            Debug.Log(
                "[Project J/Fusion] " +
                StatusMessage
            );
        }

        private async Task
            DestroyPreviousRunnerAsync()
        {
            if (runner != null)
            {
                NetworkRunner previousRunner =
                    runner;

                runner = null;

                if (previousRunner.IsRunning)
                {
                    await previousRunner
                        .Shutdown();
                }
            }

            if (runnerObject != null)
            {
                Destroy(
                    runnerObject
                );

                runnerObject =
                    null;

                await Task.Yield();
            }

            playerSpawner = null;
            inputProvider = null;
            ActiveMode = null;
        }
    }
}
