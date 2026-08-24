using System.Threading.Tasks;
using Fusion;
using ProjectJ.Networking; // 공통 네트워크 실행 정책 사용
using UnityEngine;

namespace ProjectJ.Networking.Fusion
{
    public enum ProjectJDay96ServerState
    {
        Idle = 0,
        Starting = 1,
        Running = 2,
        Stopping = 3,
        Failed = 4
    }

    [DisallowMultipleComponent]
    public sealed class ProjectJDay96ServerModeBootstrap :
        MonoBehaviour
    {
        private const int MaximumPlayers =
            8;

        [SerializeField]
        private string roomCode =
            "960001"; // Day96 로컬 접속용 고정 Room Code

        [SerializeField]
        private bool startOnPlay =
            true; // Play 시작 시 Server Mode 자동 실행

        private NetworkRunner runner;

        private GameObject runnerObject;

        private ProjectJNetworkPlayerSpawner
            playerSpawner;

        public ProjectJDay96ServerState State
        {
            get;
            private set;
        } =
            ProjectJDay96ServerState.Idle;

        public NetworkRunner Runner =>
            runner;

        public ProjectJNetworkPlayerSpawner
            PlayerSpawner =>
                playerSpawner;

        public string RoomCode =>
            roomCode;

        public string SessionName
        {
            get;
            private set;
        } =
            string.Empty;

        public string StatusMessage
        {
            get;
            private set;
        } =
            "대기 중";

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

                int count =
                    0;

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

                int count =
                    0;

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

        private void Start()
        {
            bool shouldAutoStart = // Dedicated 자동 시작 여부
                ProjectJNetworkExecutionPolicy.ShouldAutoStartDedicatedServer( // 공통 실행 정책 호출
                    ProjectJNetworkExecutionPolicy.IsDedicatedServerBuild, // 현재 Server 빌드 여부 전달
                    startOnPlay // 기존 자동 시작 설정 전달
                );

            if (!shouldAutoStart) // 자동 시작 차단 상태 확인
            {
                if (startOnPlay) // 일반 실행에서 기존 자동 시작 설정 확인
                {
                    Debug.Log(
                        "[Project J/Day98] " +
                        "일반 Host·Client 실행에서는 " +
                        "Dedicated Server 자동 시작을 생략합니다."
                    );
                }

                return;
            }

            RequestStartServer(); // Server 빌드에서만 자동 시작
        }

        private void Update()
        {
            if (
                State ==
                    ProjectJDay96ServerState.Running &&
                (
                    runner == null ||
                    !runner.IsRunning
                )
            )
            {
                runner =
                    null;

                runnerObject =
                    null;

                playerSpawner =
                    null;

                State =
                    ProjectJDay96ServerState.Idle;

                StatusMessage =
                    "Server Runner 연결 종료";
            }
        }

        private void OnApplicationQuit()
        {
            if (
                runner != null &&
                runner.IsRunning
            )
            {
                _ =
                    runner.Shutdown();
            }
        }

        public void RequestStartServer()
        {
            if (
                State !=
                    ProjectJDay96ServerState.Idle &&
                State !=
                    ProjectJDay96ServerState.Failed
            )
            {
                return;
            }

            bool valid =
                ProjectJFusionRoomCode
                    .TryNormalize(
                        roomCode,
                        out string normalizedCode,
                        out string errorMessage
                    );

            if (!valid)
            {
                State =
                    ProjectJDay96ServerState.Failed;

                StatusMessage =
                    errorMessage;

                Debug.LogError(
                    "[Project J/Day96] " +
                    StatusMessage
                );

                return;
            }

            roomCode =
                normalizedCode;

            SessionName =
                ProjectJFusionRoomCode
                    .ToSessionName(
                        normalizedCode
                    );

            _ =
                StartServerAsync();
        }

        public void RequestShutdown()
        {
            if (
                State !=
                    ProjectJDay96ServerState.Running
            )
            {
                return;
            }

            _ =
                ShutdownServerAsync();
        }

        private async Task StartServerAsync()
        {
            await DestroyPreviousRunnerAsync();

            State =
                ProjectJDay96ServerState.Starting;

            StatusMessage =
                "Fusion Server Mode 시작 중...";

            runnerObject =
                new GameObject(
                    "=== Fusion Dedicated Server Runner ==="
                );

            Object.DontDestroyOnLoad(
                runnerObject
            ); // 이후 Server Scene 전환에도 Runner 유지

            runner =
                runnerObject.AddComponent<
                    NetworkRunner
                >();

            runner.ProvideInput =
                false; // Dedicated Server는 로컬 입력을 제공하지 않음

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
                    ProjectJDay96ServerState.Failed;

                StatusMessage =
                    "Network Player Prefab을 찾을 수 없습니다.";

                Debug.LogError(
                    "[Project J/Day96] " +
                    StatusMessage
                );

                await DestroyPreviousRunnerAsync();

                return;
            }

            playerSpawner =
                runnerObject.AddComponent<
                    ProjectJNetworkPlayerSpawner
                >();

            playerSpawner.Configure(
                playerPrefab
            ); // 접속한 Client Player만 Server가 Spawn

            NetworkSceneManagerDefault
                sceneManager =
                    runnerObject.AddComponent<
                        NetworkSceneManagerDefault
                    >();

            StartGameArgs startArgs =
                new StartGameArgs
                {
                    GameMode =
                        GameMode.Server,
                    SessionName =
                        SessionName,
                    SceneManager =
                        sceneManager,
                    IsVisible =
                        false,
                    IsOpen =
                        true,
                    PlayerCount =
                        MaximumPlayers
                };

            StartGameResult result =
                await runner.StartGame(
                    startArgs
                );

            if (!result.Ok)
            {
                State =
                    ProjectJDay96ServerState.Failed;

                StatusMessage =
                    "Server Mode 시작 실패: " +
                    result.ShutdownReason;

                Debug.LogError(
                    "[Project J/Day96] " +
                    StatusMessage
                );

                await DestroyPreviousRunnerAsync();

                return;
            }

            State =
                ProjectJDay96ServerState.Running;

            StatusMessage =
                "Fusion Server Mode 실행 중";

            bool hasInputProvider =
                runnerObject.GetComponent<
                    ProjectJFusionInputProvider
                >() != null;

            Debug.Log(
                "[Project J/Day96] Server Mode 시작 성공" +
                " / RoomCode: " +
                roomCode +
                " / Session: " +
                SessionName +
                " / ProvideInput: " +
                runner.ProvideInput +
                " / InputProvider: " +
                hasInputProvider +
                " / Participants: " +
                ParticipantCount +
                " / SpawnedPlayers: " +
                SpawnedPlayerCount
            );
        }

        private async Task ShutdownServerAsync()
        {
            State =
                ProjectJDay96ServerState.Stopping;

            StatusMessage =
                "Server Mode 종료 중...";

            await DestroyPreviousRunnerAsync();

            State =
                ProjectJDay96ServerState.Idle;

            StatusMessage =
                "Server Mode 종료 완료";

            Debug.Log(
                "[Project J/Day96] " +
                StatusMessage
            );
        }

        private async Task DestroyPreviousRunnerAsync()
        {
            if (runner != null)
            {
                NetworkRunner previousRunner =
                    runner;

                runner =
                    null;

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

            playerSpawner =
                null;
        }
    }
}
