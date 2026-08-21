using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public sealed class ProjectJFusionBootstrap : MonoBehaviour
    {
        private const string DefaultSessionName = "ProjectJ-Day58";

        private NetworkRunner runner;
        private GameObject runnerObject;

        public ProjectJFusionBootstrapState State { get; private set; } =
            ProjectJFusionBootstrapState.Idle;

        public NetworkRunner Runner => runner;

        public GameMode? ActiveMode { get; private set; }

        public string SessionName { get; set; } = DefaultSessionName;

        public string StatusMessage { get; private set; } = "대기 중";

        public bool CanStart =>
            State == ProjectJFusionBootstrapState.Idle ||
            State == ProjectJFusionBootstrapState.Failed;

        public bool CanShutdown =>
            State == ProjectJFusionBootstrapState.Running;

        private void Update()
        {
            if (
                State == ProjectJFusionBootstrapState.Running &&
                (runner == null || !runner.IsRunning)
            )
            {
                runner = null;
                runnerObject = null;
                ActiveMode = null;
                State = ProjectJFusionBootstrapState.Idle;
                StatusMessage = "연결이 종료되었습니다.";
            }
        }

        private void OnApplicationQuit()
        {
            if (runner != null && runner.IsRunning)
            {
                _ = runner.Shutdown();
            }
        }

        public void RequestStartHost()
        {
            if (!CanStart)
            {
                return;
            }

            _ = StartRunnerAsync(GameMode.Host);
        }

        public void RequestStartClient()
        {
            if (!CanStart)
            {
                return;
            }

            _ = StartRunnerAsync(GameMode.Client);
        }

        public void RequestShutdown()
        {
            if (!CanShutdown)
            {
                return;
            }

            _ = ShutdownRunnerAsync();
        }

        private async Task StartRunnerAsync(GameMode gameMode)
        {
            string resolvedSessionName =
                string.IsNullOrWhiteSpace(SessionName)
                    ? DefaultSessionName
                    : SessionName.Trim();

            SessionName = resolvedSessionName;

            await DestroyPreviousRunnerAsync();

            State = ProjectJFusionBootstrapState.Starting;
            ActiveMode = gameMode;

            StatusMessage =
                gameMode == GameMode.Host
                    ? "호스트 시작 중..."
                    : "클라이언트 접속 중...";

            runnerObject = new GameObject("=== Fusion NetworkRunner ===");
            runnerObject.transform.SetParent(transform, false);

            runner = runnerObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = false;

            NetworkSceneManagerDefault sceneManager =
                runnerObject.AddComponent<NetworkSceneManagerDefault>();

            StartGameArgs startArgs = new StartGameArgs
            {
                GameMode = gameMode,
                SessionName = resolvedSessionName,
                SceneManager = sceneManager
            };

            Scene activeScene = SceneManager.GetActiveScene();

            if (activeScene.buildIndex >= 0)
            {
                NetworkSceneInfo sceneInfo = default;

                sceneInfo.AddSceneRef(
                    SceneRef.FromIndex(activeScene.buildIndex),
                    LoadSceneMode.Single
                );

                startArgs.Scene = sceneInfo;
            }

            StartGameResult result = await runner.StartGame(startArgs);

            if (!result.Ok)
            {
                State = ProjectJFusionBootstrapState.Failed;
                StatusMessage = "시작 실패: " + result.ShutdownReason;

                Debug.LogError(
                    "[Project J/Fusion] " +
                    StatusMessage
                );

                await DestroyPreviousRunnerAsync();
                ActiveMode = null;
                return;
            }

            State = ProjectJFusionBootstrapState.Running;

            StatusMessage =
                gameMode == GameMode.Host
                    ? "호스트 실행 중"
                    : "클라이언트 접속 완료";

            Debug.Log(
                "[Project J/Fusion] " +
                StatusMessage +
                " / 세션: " +
                resolvedSessionName
            );
        }

        private async Task ShutdownRunnerAsync()
        {
            if (runner == null)
            {
                State = ProjectJFusionBootstrapState.Idle;
                ActiveMode = null;
                StatusMessage = "대기 중";
                return;
            }

            State = ProjectJFusionBootstrapState.Stopping;
            StatusMessage = "NetworkRunner 종료 중...";

            NetworkRunner targetRunner = runner;
            runner = null;

            if (targetRunner.IsRunning)
            {
                await targetRunner.Shutdown();
            }

            if (runnerObject != null)
            {
                Destroy(runnerObject);
            }

            runnerObject = null;
            ActiveMode = null;
            State = ProjectJFusionBootstrapState.Idle;
            StatusMessage = "NetworkRunner 종료 완료";

            Debug.Log(
                "[Project J/Fusion] " +
                StatusMessage
            );
        }

        private async Task DestroyPreviousRunnerAsync()
        {
            if (runner != null)
            {
                NetworkRunner previousRunner = runner;
                runner = null;

                if (previousRunner.IsRunning)
                {
                    await previousRunner.Shutdown();
                }
            }

            if (runnerObject != null)
            {
                Destroy(runnerObject);
                runnerObject = null;
                await Task.Yield();
            }

            ActiveMode = null;
        }
    }
}
