using System.Threading.Tasks;
using Fusion;
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
        private const string DefaultSessionName =
            "ProjectJ-Day59";

        private const int PrivateRoomPlayerCount =
            8;

        private NetworkRunner runner;
        private GameObject runnerObject;

        public ProjectJFusionBootstrapState State
        {
            get;
            private set;
        } =
            ProjectJFusionBootstrapState.Idle;

        public NetworkRunner Runner =>
            runner;

        public GameMode? ActiveMode
        {
            get;
            private set;
        }

        public string SessionName
        {
            get;
            set;
        } =
            DefaultSessionName;

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

            if (
                !TryResolveSessionName(
                    out string sessionName
                )
            )
            {
                return;
            }

            _ = StartRunnerAsync(
                GameMode.Host,
                sessionName
            );
        }

        public void RequestJoinPrivateRoom()
        {
            if (!CanStart)
            {
                return;
            }

            if (
                !TryResolveSessionName(
                    out string sessionName
                )
            )
            {
                return;
            }

            _ = StartRunnerAsync(
                GameMode.Client,
                sessionName
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

        private bool TryResolveSessionName(
            out string sessionName
        )
        {
            bool isValid =
                ProjectJFusionSessionNameValidator
                    .TryNormalize(
                        SessionName,
                        out sessionName,
                        out string errorMessage
                    );

            if (isValid)
            {
                SessionName =
                    sessionName;

                return true;
            }

            State =
                ProjectJFusionBootstrapState.Failed;

            StatusMessage =
                errorMessage;

            LastConnectionResult =
                "입력 오류";

            return false;
        }

        private async Task StartRunnerAsync(
            GameMode gameMode,
            string sessionName
        )
        {
            await DestroyPreviousRunnerAsync();

            State =
                ProjectJFusionBootstrapState.Starting;

            ActiveMode =
                gameMode;

            StatusMessage =
                gameMode == GameMode.Host
                    ? "비공개 방 생성 중..."
                    : "비공개 방 참가 중...";

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

            runner.ProvideInput = false;

            NetworkSceneManagerDefault
                sceneManager =
                    runnerObject.AddComponent<
                        NetworkSceneManagerDefault
                    >();

            StartGameArgs startArgs =
                new StartGameArgs
                {
                    GameMode =
                        gameMode,
                    SessionName =
                        sessionName,
                    SceneManager =
                        sceneManager
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
                    "비공개 방 참가 완료";

                LastConnectionResult =
                    "Client 연결 성공";
            }

            Debug.Log(
                "[Project J/Fusion] " +
                StatusMessage +
                " / 세션: " +
                sessionName
            );
        }

        private async Task ShutdownRunnerAsync()
        {
            if (runner == null)
            {
                State =
                    ProjectJFusionBootstrapState.Idle;

                ActiveMode = null;

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

                runnerObject = null;

                await Task.Yield();
            }

            ActiveMode = null;
        }
    }
}
