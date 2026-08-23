using System.Collections.Generic; // 잠금한 Canvas 상태 보존
using Fusion; // NetworkRunner 중복 점검
using ProjectJ; // MainMenu Controller 상태 확인
using UnityEngine; // Runtime Scene Guard
using UnityEngine.SceneManagement; // Scene 전환 감지

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProjectJFusionBootstrap))]
    [RequireComponent(typeof(ProjectJNetworkLobbyFlow))]
    [RequireComponent(typeof(ProjectJDay82SceneFlowCoordinator))]
    public sealed class ProjectJDay94SceneFlowGuard :
        MonoBehaviour
    {
        private const string MainMenuSceneName =
            "MainMenu";

        private const string LobbySceneName =
            "Lobby";

        private const string GameSceneName =
            "Game";

        private const float AuditDelaySeconds =
            0.5f; // Scene 로드 직후 생성 완료 대기

        private sealed class CanvasLockState
        {
            public CanvasGroup Group;
            public bool AddedByGuard;
            public bool PreviousInteractable;
            public bool PreviousBlocksRaycasts;
        }

        private readonly List<CanvasLockState>
            canvasLockStates =
                new List<CanvasLockState>(); // 현재 Scene Canvas 잠금 원상복구 정보

        private ProjectJFusionBootstrap bootstrap;

        private ProjectJNetworkLobbyFlow lobbyFlow;

        private ProjectJDay82SceneFlowCoordinator sceneFlow;

        private bool sceneUiLocked;

        private bool auditPending;

        private float auditAt;

        private string lastAuditSignature =
            string.Empty;

        private void Awake()
        {
            ResolveReferences();
            ScheduleAudit();
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged +=
                OnActiveSceneChanged; // 실제 활성 Scene 변경 감지
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -=
                OnActiveSceneChanged;

            RestoreSceneUi();
        }

        private void Update()
        {
            ResolveReferences();
            RefreshSceneUiLock();

            if (
                auditPending &&
                Time.unscaledTime >=
                    auditAt
            )
            {
                auditPending =
                    false;

                AuditCurrentScene(); // Scene 전환 뒤 중복 Runtime 요소 점검
            }
        }

        private void OnActiveSceneChanged(
            Scene previousScene,
            Scene nextScene
        )
        {
            RestoreSceneUi(); // 이전 Scene 잠금 기록 제거
            ScheduleAudit();
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

            if (sceneFlow == null)
            {
                sceneFlow =
                    GetComponent<
                        ProjectJDay82SceneFlowCoordinator
                    >();
            }
        }

        private void RefreshSceneUiLock()
        {
            Scene activeScene =
                SceneManager.GetActiveScene();

            bool shouldLock =
                ShouldLockSceneUi(
                    activeScene
                );

            if (
                shouldLock &&
                !sceneUiLocked
            )
            {
                LockSceneUi(
                    activeScene
                );

                return;
            }

            if (
                !shouldLock &&
                sceneUiLocked
            )
            {
                RestoreSceneUi();
            }
        }

        private bool ShouldLockSceneUi(
            Scene activeScene
        )
        {
            if (
                !activeScene.IsValid() ||
                sceneFlow == null
            )
            {
                return false;
            }

            ProjectJFusionBootstrapState
                bootstrapState =
                    bootstrap != null
                        ? bootstrap.State
                        : ProjectJFusionBootstrapState.Idle;

            ProjectJNetworkLobbyFlowPhase
                lobbyPhase =
                    lobbyFlow != null
                        ? lobbyFlow.Phase
                        : ProjectJNetworkLobbyFlowPhase.Disconnected;

            if (
                activeScene.name ==
                    MainMenuSceneName
            )
            {
                return
                    bootstrapState ==
                        ProjectJFusionBootstrapState.Starting ||
                    bootstrapState ==
                        ProjectJFusionBootstrapState.Running ||
                    bootstrapState ==
                        ProjectJFusionBootstrapState.Stopping ||
                    sceneFlow.State ==
                        ProjectJDay82SceneFlowState.Connecting;
            }

            if (
                activeScene.name ==
                    LobbySceneName
            )
            {
                return
                    bootstrapState ==
                        ProjectJFusionBootstrapState.Stopping ||
                    sceneFlow.State ==
                        ProjectJDay82SceneFlowState.ReturningToMainMenu ||
                    lobbyPhase ==
                        ProjectJNetworkLobbyFlowPhase.EnteringLobby ||
                    lobbyPhase ==
                        ProjectJNetworkLobbyFlowPhase.MatchLoading ||
                    lobbyPhase ==
                        ProjectJNetworkLobbyFlowPhase.ReturningToLobby;
            }

            if (
                activeScene.name ==
                    GameSceneName
            )
            {
                return
                    lobbyPhase ==
                        ProjectJNetworkLobbyFlowPhase.GamePreparing ||
                    lobbyPhase ==
                        ProjectJNetworkLobbyFlowPhase.Countdown ||
                    lobbyPhase ==
                        ProjectJNetworkLobbyFlowPhase.ReturningToLobby;
            }

            return false;
        }

        private void LockSceneUi(
            Scene activeScene
        )
        {
            RestoreSceneUi();

            Canvas[] canvases =
                Object.FindObjectsByType<
                    Canvas
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                ); // 현재 Scene의 Root Canvas 검색

            for (
                int index = 0;
                index < canvases.Length;
                index++
            )
            {
                Canvas canvas =
                    canvases[index];

                if (
                    canvas == null ||
                    !canvas.isRootCanvas ||
                    canvas.gameObject.scene !=
                        activeScene
                )
                {
                    continue;
                }

                CanvasGroup group =
                    canvas.GetComponent<
                        CanvasGroup
                    >();

                bool addedByGuard =
                    false;

                if (group == null)
                {
                    group =
                        canvas.gameObject.AddComponent<
                            CanvasGroup
                        >(); // Runtime 전환 잠금용 CanvasGroup 추가

                    addedByGuard =
                        true;
                }

                CanvasLockState state =
                    new CanvasLockState
                    {
                        Group =
                            group,
                        AddedByGuard =
                            addedByGuard,
                        PreviousInteractable =
                            group.interactable,
                        PreviousBlocksRaycasts =
                            group.blocksRaycasts
                    };

                canvasLockStates.Add(
                    state
                );

                group.interactable =
                    false; // 전환 중 UI 재입력 차단

                group.blocksRaycasts =
                    true; // 비활성 UI가 입력을 받아 아래 UI로 전달하지 않게 유지
            }

            sceneUiLocked =
                canvasLockStates.Count > 0;
        }

        private void RestoreSceneUi()
        {
            for (
                int index = 0;
                index <
                    canvasLockStates.Count;
                index++
            )
            {
                CanvasLockState state =
                    canvasLockStates[index];

                if (
                    state == null ||
                    state.Group == null
                )
                {
                    continue;
                }

                if (state.AddedByGuard)
                {
                    Destroy(
                        state.Group
                    ); // Guard가 만든 CanvasGroup만 제거

                    continue;
                }

                state.Group.interactable =
                    state.PreviousInteractable;

                state.Group.blocksRaycasts =
                    state.PreviousBlocksRaycasts;
            }

            canvasLockStates.Clear();
            sceneUiLocked =
                false;
        }

        private void ScheduleAudit()
        {
            auditPending =
                true;

            auditAt =
                Time.unscaledTime +
                AuditDelaySeconds;
        }

        public void AuditCurrentScene()
        {
            ResolveReferences();

            Scene activeScene =
                SceneManager.GetActiveScene();

            ProjectJFusionBootstrap[] bootstraps =
                Object.FindObjectsByType<
                    ProjectJFusionBootstrap
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            NetworkRunner[] runners =
                Object.FindObjectsByType<
                    NetworkRunner
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            ProjectJNetworkPlayer[] players =
                Object.FindObjectsByType<
                    ProjectJNetworkPlayer
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            Camera[] cameras =
                Object.FindObjectsByType<
                    Camera
                >(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            AudioListener[] listeners =
                Object.FindObjectsByType<
                    AudioListener
                >(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            int runningRunnerCount =
                CountRunningRunners(
                    runners
                );

            int mainCameraCount =
                CountEnabledMainCameras(
                    cameras
                );

            int audioListenerCount =
                CountEnabledAudioListeners(
                    listeners
                );

            int participantCount =
                bootstrap != null
                    ? bootstrap.ParticipantCount
                    : 0;

            string signature =
                activeScene.name +
                "|B" +
                bootstraps.Length +
                "|R" +
                runningRunnerCount +
                "|P" +
                players.Length +
                "/" +
                participantCount +
                "|C" +
                mainCameraCount +
                "|A" +
                audioListenerCount;

            if (
                signature ==
                lastAuditSignature
            )
            {
                return; // 같은 Scene 상태 중복 로그 방지
            }

            lastAuditSignature =
                signature;

            bool hasError =
                false;

            if (bootstraps.Length > 1)
            {
                hasError =
                    true;

                Debug.LogError(
                    "[Project J/Day94] ProjectJFusionBootstrap 중복: " +
                    bootstraps.Length
                );
            }

            if (runningRunnerCount > 1)
            {
                hasError =
                    true;

                Debug.LogError(
                    "[Project J/Day94] 실행 중 NetworkRunner 중복: " +
                    runningRunnerCount
                );
            }

            if (mainCameraCount > 1)
            {
                hasError =
                    true;

                Debug.LogError(
                    "[Project J/Day94] 활성 MainCamera 중복: " +
                    mainCameraCount
                );
            }

            if (audioListenerCount > 1)
            {
                hasError =
                    true;

                Debug.LogError(
                    "[Project J/Day94] 활성 AudioListener 중복: " +
                    audioListenerCount
                );
            }

            bool sessionScene =
                activeScene.name ==
                    LobbySceneName ||
                activeScene.name ==
                    GameSceneName;

            if (
                sessionScene &&
                runningRunnerCount == 1 &&
                participantCount > 0 &&
                players.Length !=
                    participantCount
            )
            {
                Debug.LogWarning(
                    "[Project J/Day94] Network Player 수 확인 필요 / ActivePlayers: " +
                    participantCount +
                    " / Player Objects: " +
                    players.Length
                );
            }

            bool mainMenuWithoutSession =
                activeScene.name ==
                    MainMenuSceneName &&
                runningRunnerCount == 0;

            if (
                mainMenuWithoutSession &&
                players.Length > 0
            )
            {
                hasError =
                    true;

                Debug.LogError(
                    "[Project J/Day94] MainMenu 복귀 후 Network Player 잔존: " +
                    players.Length
                );
            }

            ProjectJMainMenuController menuController =
                activeScene.name ==
                    MainMenuSceneName
                    ? Object.FindFirstObjectByType<
                        ProjectJMainMenuController
                    >()
                    : null;

            string menuTabText =
                menuController != null
                    ? menuController.CurrentTabIndex
                        .ToString()
                    : "-";

            if (!hasError)
            {
                Debug.Log(
                    "[Project J/Day94] Scene Flow Audit OK / Scene: " +
                    activeScene.name +
                    " / Runner: " +
                    runningRunnerCount +
                    " / Players: " +
                    players.Length +
                    " / MainCamera: " +
                    mainCameraCount +
                    " / AudioListener: " +
                    audioListenerCount +
                    " / MainMenuTab: " +
                    menuTabText
                );
            }
        }

        private static int CountRunningRunners(
            NetworkRunner[] runners
        )
        {
            int count =
                0;

            for (
                int index = 0;
                index < runners.Length;
                index++
            )
            {
                if (
                    runners[index] != null &&
                    runners[index].IsRunning
                )
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountEnabledMainCameras(
            Camera[] cameras
        )
        {
            int count =
                0;

            for (
                int index = 0;
                index < cameras.Length;
                index++
            )
            {
                Camera camera =
                    cameras[index];

                if (
                    camera == null ||
                    !camera.enabled ||
                    !camera.gameObject
                        .activeInHierarchy
                )
                {
                    continue;
                }

                if (
                    camera.gameObject
                        .CompareTag(
                            "MainCamera"
                        )
                )
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountEnabledAudioListeners(
            AudioListener[] listeners
        )
        {
            int count =
                0;

            for (
                int index = 0;
                index < listeners.Length;
                index++
            )
            {
                AudioListener listener =
                    listeners[index];

                if (
                    listener != null &&
                    listener.enabled &&
                    listener.gameObject
                        .activeInHierarchy
                )
                {
                    count++;
                }
            }

            return count;
        }
    }
}
