using System.Collections.Generic; // 보존 Player Instance ID 저장
using Fusion; // NetworkRunner와 SceneRef 사용
using UnityEngine; // MonoBehaviour와 GUI 사용
using UnityEngine.SceneManagement; // Scene 로드와 Build Index 확인

namespace ProjectJ.Networking.Fusion
{
    public enum ProjectJNetworkLobbyFlowPhase
    {
        Disconnected = 0,
        EnteringLobby = 1,
        Lobby = 2,
        MatchLoading = 3,
        GamePreparing = 4,
        Countdown = 5,
        Playing = 6,
        Finished = 7,
        ReturningToLobby = 8
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProjectJFusionBootstrap))]
    public sealed class ProjectJNetworkLobbyFlow :
        MonoBehaviour
    {
        private const string LobbyScenePath =
            "Assets/ProjectJ/Scenes/Lobby.unity";

        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private const string LobbySceneName =
            "Lobby";

        private const string GameSceneName =
            "Game";

        private const int MinimumReadyPlayers =
            2;

        private const float SpawnSpacing =
            3f;

        private const float SpawnY =
            2f;

        private const float SpawnZ =
            4f;

        private readonly HashSet<int>
            persistedPlayerInstanceIds =
                new HashSet<int>();

        private ProjectJFusionBootstrap bootstrap;

        private NetworkRunner trackedRunner;

        private bool lobbyLoadRequested;

        private bool enteredLobby;

        private bool gameLoadRequested;

        private bool gamePlayersPrepared;

        private bool countdownRequested;

        public ProjectJNetworkLobbyFlowPhase Phase
        {
            get;
            private set;
        } =
            ProjectJNetworkLobbyFlowPhase.Disconnected;

        public int ReadyPlayerCount
        {
            get;
            private set;
        }

        public int ParticipantCount
        {
            get;
            private set;
        }

        public string StatusText
        {
            get;
            private set;
        } =
            "Session 연결 대기";

        public bool CanReturnToLobby
        {
            get
            {
                return
                    trackedRunner != null &&
                    trackedRunner.IsRunning &&
                    trackedRunner.IsSceneAuthority &&
                    Phase ==
                        ProjectJNetworkLobbyFlowPhase.Finished;
            }
        }

        private void Awake()
        {
            bootstrap =
                GetComponent<ProjectJFusionBootstrap>();
        }

        private void Update()
        {
            ResolveBootstrap();

            NetworkRunner runner =
                bootstrap != null
                    ? bootstrap.Runner
                    : null;

            if (
                runner == null ||
                !runner.IsRunning ||
                bootstrap.State !=
                    ProjectJFusionBootstrapState.Running
            )
            {
                ResetForDisconnectedRunner();
                return;
            }

            if (trackedRunner != runner)
            {
                ResetForNewRunner(
                    runner
                );
            }

            PersistRunnerAndPlayers(
                runner
            );

            Scene activeScene =
                SceneManager.GetActiveScene();

            if (
                activeScene.IsValid() &&
                activeScene.name ==
                    LobbySceneName
            )
            {
                enteredLobby =
                    true;

                lobbyLoadRequested =
                    false;

                UpdateLobby(
                    runner
                );

                return;
            }

            if (
                activeScene.IsValid() &&
                activeScene.name ==
                    GameSceneName &&
                enteredLobby
            )
            {
                UpdateGame(
                    runner
                );

                return;
            }

            UpdateEnteringLobby(
                runner
            );
        }

        public bool RequestReturnToLobby()
        {
            if (!CanReturnToLobby)
            {
                StatusText =
                    "Host가 경기 종료 후에만 Lobby로 돌아갈 수 있습니다.";

                return false;
            }

            int lobbyBuildIndex =
                SceneUtility.GetBuildIndexByScenePath(
                    LobbyScenePath
                );

            if (lobbyBuildIndex < 0)
            {
                StatusText =
                    "Lobby Scene이 Build Settings에 없습니다.";

                Debug.LogError(
                    "[Project J/Fusion] 82일차 / Lobby 복귀 Build Index 없음"
                );

                return false;
            }

            if (
                trackedRunner.SessionInfo.IsValid
            )
            {
                trackedRunner.SessionInfo.IsOpen =
                    true;
            }


            lobbyLoadRequested =
                true;

            enteredLobby =
                false;

            gameLoadRequested =
                false;

            gamePlayersPrepared =
                false;

            countdownRequested =
                false;

            ReadyPlayerCount =
                0;

            Phase =
                ProjectJNetworkLobbyFlowPhase
                    .ReturningToLobby;

            StatusText =
                "경기 종료 / Lobby 복귀 중";

            trackedRunner.LoadScene(
                SceneRef.FromIndex(
                    lobbyBuildIndex
                ),
                LoadSceneMode.Single
            );

            Debug.Log(
                "[Project J/Fusion] 82일차 / Game → Lobby 복귀 요청"
            );

            return true;
        }

        private void UpdateEnteringLobby(
            NetworkRunner runner
        )
        {
            Phase =
                ProjectJNetworkLobbyFlowPhase
                    .EnteringLobby;

            StatusText =
                "Lobby Scene 진입 대기";

            if (
                !runner.IsSceneAuthority ||
                lobbyLoadRequested
            )
            {
                return;
            }

            int lobbyBuildIndex =
                SceneUtility.GetBuildIndexByScenePath(
                    LobbyScenePath
                );

            if (lobbyBuildIndex < 0)
            {
                StatusText =
                    "Lobby Scene이 Build Settings에 없습니다.";

                Debug.LogError(
                    "[Project J/Fusion] 82일차 / Lobby Build Index 없음"
                );

                return;
            }

            lobbyLoadRequested =
                true;

            runner.LoadScene(
                SceneRef.FromIndex(
                    lobbyBuildIndex
                ),
                LoadSceneMode.Single
            );

            Debug.Log(
                "[Project J/Fusion] 82일차 / Session 연결 후 Lobby Scene 로드 요청"
            );
        }

        private void UpdateLobby(
            NetworkRunner runner
        )
        {
            CountReadyPlayers(
                runner,
                out int participantCount,
                out int readyCount,
                out bool allPlayerObjectsReady
            );

            ParticipantCount =
                participantCount;

            ReadyPlayerCount =
                readyCount;

            bool allReady =
                participantCount >=
                    MinimumReadyPlayers &&
                allPlayerObjectsReady &&
                readyCount ==
                    participantCount;

            if (allReady)
            {
                Phase =
                    ProjectJNetworkLobbyFlowPhase
                        .MatchLoading;

                StatusText =
                    "전원 Ready / Game Scene 로딩 준비";
            }
            else
            {
                Phase =
                    ProjectJNetworkLobbyFlowPhase
                        .Lobby;

                StatusText =
                    "R 키로 Ready 전환";
            }

            if (
                !runner.IsSceneAuthority ||
                gameLoadRequested ||
                !allReady
            )
            {
                return;
            }

            int gameBuildIndex =
                SceneUtility.GetBuildIndexByScenePath(
                    GameScenePath
                );

            if (gameBuildIndex < 0)
            {
                StatusText =
                    "Game Scene이 Build Settings에 없습니다.";

                Debug.LogError(
                    "[Project J/Fusion] 82일차 / Game Build Index 없음"
                );

                return;
            }

            if (
                runner.SessionInfo.IsValid &&
                runner.SessionInfo.IsOpen
            )
            {
                runner.SessionInfo.IsOpen =
                    false;
            }

            gameLoadRequested =
                true;

            gamePlayersPrepared =
                false;

            countdownRequested =
                false;

            runner.LoadScene(
                SceneRef.FromIndex(
                    gameBuildIndex
                ),
                LoadSceneMode.Single
            );

            Debug.Log(
                "[Project J/Fusion] 82일차 / Lobby → Game / " +
                participantCount +
                "명 전원 Ready"
            );
        }

        private void UpdateGame(
            NetworkRunner runner
        )
        {
            ParticipantCount =
                CountParticipants(
                    runner
                );

            ReadyPlayerCount =
                0;

            if (!gamePlayersPrepared)
            {
                Phase =
                    ProjectJNetworkLobbyFlowPhase
                        .GamePreparing;

                StatusText =
                    "Game Player 시작 위치 준비";

                if (!runner.IsSceneAuthority)
                {
                    ProjectJNetworkExternalGameplay
                        observedMatch =
                            GetAnyPlayerGameplay(
                                runner
                            );

                    if (
                        observedMatch == null ||
                        observedMatch.MatchState ==
                            ProjectJNetworkMatchState
                                .Preparing
                    )
                    {
                        return;
                    }

                    gamePlayersPrepared =
                        true;
                }
                else
                {
                    if (
                        !TryPrepareAllPlayers(
                            runner
                        )
                    )
                    {
                        return;
                    }

                    gamePlayersPrepared =
                        true;
                }
            }

            ProjectJNetworkExternalGameplay
                localMatch =
                    GetAnyPlayerGameplay(
                        runner
                    );

            if (
                runner.IsSceneAuthority &&
                !countdownRequested &&
                localMatch != null
            )
            {
                countdownRequested =
                    localMatch
                        .TryBeginCountdownFromLobbyFlowAuthority();

                if (countdownRequested)
                {
                    Debug.Log(
                        "[Project J/Fusion] 82일차 / Game 준비 완료 / Countdown 요청"
                    );
                }
            }

            ProjectJNetworkMatchState matchState =
                localMatch != null
                    ? localMatch.MatchState
                    : ProjectJNetworkMatchState
                        .Preparing;

            switch (matchState)
            {
                case ProjectJNetworkMatchState.Countdown:
                    Phase =
                        ProjectJNetworkLobbyFlowPhase
                            .Countdown;

                    StatusText =
                        "3초 Countdown";
                    break;

                case ProjectJNetworkMatchState.Playing:
                    Phase =
                        ProjectJNetworkLobbyFlowPhase
                            .Playing;

                    StatusText =
                        "경기 진행 중";
                    break;

                case ProjectJNetworkMatchState.Finished:
                    Phase =
                        ProjectJNetworkLobbyFlowPhase
                            .Finished;

                    StatusText =
                        runner.IsSceneAuthority
                            ? "경기 종료 / Host가 Lobby 복귀 가능"
                            : "경기 종료 / Host의 Lobby 복귀 대기";
                    break;

                default:
                    Phase =
                        ProjectJNetworkLobbyFlowPhase
                            .GamePreparing;

                    StatusText =
                        "Countdown 시작 대기";
                    break;
            }
        }

        private bool TryPrepareAllPlayers(
            NetworkRunner runner
        )
        {
            int participantCount =
                0;

            int preparedCount =
                0;

            int slot =
                0;

            foreach (
                PlayerRef player
                in runner.ActivePlayers
            )
            {
                participantCount++;

                if (
                    !runner.TryGetPlayerObject(
                        player,
                        out NetworkObject playerObject
                    ) ||
                    playerObject == null
                )
                {
                    slot++;
                    continue;
                }

                ProjectJNetworkExternalGameplay gameplay =
                    playerObject.GetComponent<
                        ProjectJNetworkExternalGameplay
                    >();

                if (gameplay == null)
                {
                    slot++;
                    continue;
                }

                Vector3 spawnPosition =
                    new Vector3(
                        slot *
                            SpawnSpacing,
                        SpawnY,
                        SpawnZ
                    );

                if (
                    gameplay
                        .PrepareForGameSceneAuthority(
                            spawnPosition,
                            Quaternion.identity
                        )
                )
                {
                    preparedCount++;
                }

                slot++;
            }

            return
                participantCount >=
                    MinimumReadyPlayers &&
                preparedCount ==
                    participantCount;
        }

        private void PersistRunnerAndPlayers(
            NetworkRunner runner
        )
        {
            if (
                runner.gameObject.scene.IsValid()
            )
            {
                DontDestroyOnLoad(
                    runner.gameObject
                );
            }

            foreach (
                PlayerRef player
                in runner.ActivePlayers
            )
            {
                if (
                    !runner.TryGetPlayerObject(
                        player,
                        out NetworkObject playerObject
                    ) ||
                    playerObject == null
                )
                {
                    continue;
                }

                int instanceId =
                    playerObject.gameObject
                        .GetInstanceID();

                if (
                    !persistedPlayerInstanceIds
                        .Add(
                            instanceId
                        )
                )
                {
                    continue;
                }

                runner.MakeDontDestroyOnLoad(
                    playerObject.gameObject
                );
            }
        }

        private static void CountReadyPlayers(
            NetworkRunner runner,
            out int participantCount,
            out int readyCount,
            out bool allPlayerObjectsReady
        )
        {
            participantCount =
                0;

            readyCount =
                0;

            allPlayerObjectsReady =
                true;

            foreach (
                PlayerRef player
                in runner.ActivePlayers
            )
            {
                participantCount++;

                if (
                    !runner.TryGetPlayerObject(
                        player,
                        out NetworkObject playerObject
                    ) ||
                    playerObject == null
                )
                {
                    allPlayerObjectsReady =
                        false;

                    continue;
                }

                ProjectJNetworkExternalGameplay gameplay =
                    playerObject.GetComponent<
                        ProjectJNetworkExternalGameplay
                    >();

                if (gameplay == null)
                {
                    allPlayerObjectsReady =
                        false;

                    continue;
                }

                if (gameplay.LobbyReady)
                {
                    readyCount++;
                }
            }
        }

        private static int CountParticipants(
            NetworkRunner runner
        )
        {
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

        private static ProjectJNetworkExternalGameplay
            GetAnyPlayerGameplay(
                NetworkRunner runner
            )
        {
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
                    ProjectJNetworkExternalGameplay
                        gameplay =
                            playerObject.GetComponent<
                                ProjectJNetworkExternalGameplay
                            >();

                    if (gameplay != null)
                    {
                        return gameplay;
                    }
                }
            }

            return null;
        }

        private void ResolveBootstrap()
        {
            if (bootstrap == null)
            {
                bootstrap =
                    GetComponent<
                        ProjectJFusionBootstrap
                    >();
            }
        }

        private void ResetForNewRunner(
            NetworkRunner runner
        )
        {
            trackedRunner =
                runner;

            persistedPlayerInstanceIds
                .Clear();

            lobbyLoadRequested =
                false;

            enteredLobby =
                false;

            gameLoadRequested =
                false;

            gamePlayersPrepared =
                false;

            countdownRequested =
                false;


            ReadyPlayerCount =
                0;

            ParticipantCount =
                0;

            Phase =
                ProjectJNetworkLobbyFlowPhase
                    .EnteringLobby;

            StatusText =
                "새 Session / Lobby 진입 준비";
        }

        private void ResetForDisconnectedRunner()
        {
            if (trackedRunner == null)
            {
                return;
            }

            trackedRunner =
                null;

            persistedPlayerInstanceIds
                .Clear();

            lobbyLoadRequested =
                false;

            enteredLobby =
                false;

            gameLoadRequested =
                false;

            gamePlayersPrepared =
                false;

            countdownRequested =
                false;


            ReadyPlayerCount =
                0;

            ParticipantCount =
                0;

            Phase =
                ProjectJNetworkLobbyFlowPhase
                    .Disconnected;

            StatusText =
                "Session 연결 대기";
        }

        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (
                trackedRunner == null ||
                !trackedRunner.IsRunning
            )
            {
                return;
            }

            Rect box =
                new Rect(
                    12f,
                    12f,
                    390f,
                    170f
                );

            GUI.Box(
                box,
                string.Empty
            );

            GUI.Label(
                new Rect(
                    24f,
                    22f,
                    350f,
                    24f
                ),
                "DAY 82 LOBBY / MATCH FLOW"
            );

            GUI.Label(
                new Rect(
                    24f,
                    50f,
                    350f,
                    22f
                ),
                "Phase : " +
                Phase
            );

            GUI.Label(
                new Rect(
                    24f,
                    74f,
                    350f,
                    22f
                ),
                "Players : " +
                ParticipantCount +
                " / Ready : " +
                ReadyPlayerCount
            );

            GUI.Label(
                new Rect(
                    24f,
                    98f,
                    350f,
                    22f
                ),
                StatusText
            );

            if (
                Phase ==
                    ProjectJNetworkLobbyFlowPhase
                        .Lobby
            )
            {
                GUI.Label(
                    new Rect(
                        24f,
                        124f,
                        350f,
                        28f
                    ),
                    "R : READY / NOT READY"
                );
            }
            else if (
                Phase ==
                    ProjectJNetworkLobbyFlowPhase
                        .Finished
            )
            {
                GUI.Label(
                    new Rect(
                        24f,
                        124f,
                        350f,
                        28f
                    ),
                    trackedRunner.IsSceneAuthority
                        ? "F9 : RETURN TO LOBBY"
                        : "Host Lobby 복귀 대기"
                );
            }
#endif
        }
    }
}
