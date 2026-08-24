using System.Collections.Generic; // Player 정렬과 보존 ID 사용
using Fusion; // NetworkRunner와 SceneRef 사용
using ProjectJ.Debugging; // 통합 디버그 패널 표시 상태 사용
using UnityEngine; // Runtime UI와 Scene 오브젝트 사용
using UnityEngine.SceneManagement; // Game Scene 로드

namespace ProjectJ.Networking.Fusion
{
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public sealed class ProjectJDay76TestFlow :
        MonoBehaviour
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private const int MinimumPlayers = 2;
        private const int MaximumPlayers = 8;

        private readonly HashSet<int>
            persistedPlayerInstanceIds =
                new HashSet<int>(); // Scene 전환 Player 보존 기록

        private ProjectJFusionBootstrap bootstrap;
        private ProjectJNetworkLobbyFlow legacyLobbyFlow;
        private NetworkRunner trackedRunner;

        private bool sceneLoadRequested;
        private bool runnerPersisted;
        private bool startRequested;

        private string statusText =
            "Session 연결 대기"; // Day76 상태 표시

        private void Awake()
        {
            Application.runInBackground = true; // 비활성 창도 계속 실행
            Application.targetFrameRate = 60; // 다중 창 테스트 부하 제한

            ResolveBootstrap();
            DisableLegacyLobbyFlow();
        }

        private void Update()
        {
            ResolveBootstrap();
            DisableLegacyLobbyFlow();

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
                ResetDisconnectedState();
                return;
            }

            if (trackedRunner != runner)
            {
                ResetForRunner(runner);
            }

            PersistRunnerAndPlayers(runner);

            Scene activeScene =
                SceneManager.GetActiveScene();

            if (
                activeScene.IsValid() &&
                activeScene.path == GameScenePath
            )
            {
                UpdateGameScene(runner);
                return;
            }

            UpdateSceneLoading(runner);
        }

        private void OnGUI()
        {
            if (!ProjectJDebugOverlayController.IsVisible) // 통합 패널 선택 상태 확인
            {
                return; // 독립 진단창 출력 차단
            }

            NetworkRunner runner =
                bootstrap != null
                    ? bootstrap.Runner
                    : null;

            if (
                runner == null ||
                !runner.IsRunning ||
                SceneManager.GetActiveScene().path !=
                    GameScenePath
            )
            {
                return;
            }

            float panelWidth = 350f;
            float panelHeight = 235f;

            Rect panel =
                new Rect(
                    Mathf.Max(
                        12f,
                        Screen.width -
                        panelWidth -
                        12f
                    ),
                    12f,
                    panelWidth,
                    panelHeight
                );

            GUI.Box(
                panel,
                string.Empty
            );

            float x = panel.x + 14f;
            float y = panel.y + 12f;
            float width = panel.width - 28f;

            DrawLine(
                x,
                ref y,
                width,
                "DAY 76 - GAME SCENE MULTIPLAYER TEST"
            );

            DrawLine(
                x,
                ref y,
                width,
                "ROOM : " +
                (
                    bootstrap != null
                        ? bootstrap.ConnectedRoomCode
                        : "-"
                )
            );

            DrawLine(
                x,
                ref y,
                width,
                "PLAYERS : " +
                CountParticipants(runner) +
                " / " +
                MaximumPlayers
            );

            ProjectJNetworkExternalGameplay match =
                GetCoordinatorGameplay(runner);

            DrawLine(
                x,
                ref y,
                width,
                "MATCH : " +
                (
                    match != null
                        ? match.MatchState.ToString()
                        : "Preparing"
                )
            );

            DrawLine(
                x,
                ref y,
                width,
                statusText
            );

            if (
                runner.IsSceneAuthority &&
                CanHostStart(runner, match)
            )
            {
                if (
                    GUI.Button(
                        new Rect(
                            x,
                            y + 4f,
                            width,
                            38f
                        ),
                        "GAME START"
                    )
                )
                {
                    TryStartMatch(runner);
                }

                return;
            }

            if (!runner.IsSceneAuthority)
            {
                DrawLine(
                    x,
                    ref y,
                    width,
                    "Waiting for Host..."
                );
            }
            else if (
                match == null ||
                match.MatchState ==
                    ProjectJNetworkMatchState.Preparing
            )
            {
                DrawLine(
                    x,
                    ref y,
                    width,
                    "2명 이상 접속 후 시작 가능"
                );
            }
        }

        private void UpdateSceneLoading(
            NetworkRunner runner
        )
        {
            statusText =
                "Game Scene 진입 중";

            if (
                !runner.IsSceneAuthority ||
                sceneLoadRequested
            )
            {
                return; // Host Scene Authority만 Game 로드
            }

            int buildIndex =
                SceneUtility.GetBuildIndexByScenePath(
                    GameScenePath
                );

            if (buildIndex < 0)
            {
                statusText =
                    "Game Scene이 Build Settings에 없습니다.";

                Debug.LogError(
                    "[Project J/Fusion] 76일차 / 실제 Game Scene Build Index 없음"
                );

                return;
            }

            sceneLoadRequested = true;

            runner.LoadScene(
                SceneRef.FromIndex(buildIndex),
                LoadSceneMode.Single
            ); // 실제 Game Scene을 로드하고 Client는 Fusion 동기화로 추종

            Debug.Log(
                "[Project J/Fusion] 76일차 / 실제 Game Scene 로드 요청"
            );
        }

        private void UpdateGameScene(
            NetworkRunner runner
        )
        {
            sceneLoadRequested = false;

            ProjectJNetworkExternalGameplay match =
                GetCoordinatorGameplay(runner);

            if (match == null)
            {
                statusText =
                    "Network Player 생성 대기";
                return;
            }

            switch (match.MatchState)
            {
                case ProjectJNetworkMatchState.Countdown:
                    statusText =
                        "Countdown : " +
                        match.CountdownRemaining
                            .ToString("F1") +
                        "s";
                    break;

                case ProjectJNetworkMatchState.Playing:
                    statusText =
                        "Playing / " +
                        match.MatchTimeRemaining
                            .ToString("F1") +
                        "s";
                    break;

                case ProjectJNetworkMatchState.Finished:
                    statusText =
                        "경기 종료";
                    break;

                default:
                    statusText =
                        runner.IsSceneAuthority
                            ? "Client 접속 후 Host GAME START 대기"
                            : "Host GAME START 대기";
                    break;
            }
        }

        private bool CanHostStart(
            NetworkRunner runner,
            ProjectJNetworkExternalGameplay match
        )
        {
            if (
                startRequested ||
                !runner.IsSceneAuthority ||
                match == null ||
                match.MatchState !=
                    ProjectJNetworkMatchState.Preparing
            )
            {
                return false;
            }

            int participantCount =
                CountParticipants(runner);

            return
                participantCount >= MinimumPlayers &&
                participantCount <= MaximumPlayers &&
                CountSpawnedPlayers(runner) ==
                    participantCount;
        }

        private void TryStartMatch(
            NetworkRunner runner
        )
        {
            startRequested = true;

            if (!TryPrepareAllPlayers(runner))
            {
                startRequested = false;
                statusText =
                    "Player 또는 Spawn Point 준비 실패";
                return;
            }

            ProjectJNetworkExternalGameplay coordinator =
                GetCoordinatorGameplay(runner);

            if (
                coordinator == null ||
                !coordinator
                    .TryBeginCountdownFromLobbyFlowAuthority()
            )
            {
                startRequested = false;
                statusText =
                    "Countdown 시작 실패";
                return;
            }

            if (
                runner.SessionInfo.IsValid &&
                runner.SessionInfo.IsOpen
            )
            {
                runner.SessionInfo.IsOpen = false; // 경기 시작 후 늦은 참가 차단
            }

            statusText =
                "3초 Countdown 시작";
        }

        private bool TryPrepareAllPlayers(
            NetworkRunner runner
        )
        {
            List<PlayerRef> players =
                new List<PlayerRef>();

            foreach (
                PlayerRef player
                in runner.ActivePlayers
            )
            {
                players.Add(player);
            }

            players.Sort(
                (
                    left,
                    right
                ) =>
                    left.AsIndex.CompareTo(
                        right.AsIndex
                    )
            ); // Player 번호 순서로 Spawn Slot 고정

            if (
                players.Count < MinimumPlayers ||
                players.Count > MaximumPlayers
            )
            {
                return false;
            }

            for (
                int slot = 0;
                slot < players.Count;
                slot++
            )
            {
                PlayerRef player =
                    players[slot];

                if (
                    !runner.TryGetPlayerObject(
                        player,
                        out NetworkObject playerObject
                    ) ||
                    playerObject == null
                )
                {
                    return false;
                }

                ProjectJNetworkExternalGameplay gameplay =
                    playerObject.GetComponent<
                        ProjectJNetworkExternalGameplay
                    >();

                if (gameplay == null)
                {
                    return false;
                }

                if (
                    !ProjectJNetworkSpawnPoint.TryGetPose(
                        slot,
                        out Vector3 spawnPosition,
                        out Quaternion spawnRotation
                    )
                )
                {
                    return false;
                }

                if (
                    !gameplay.PrepareForGameSceneAuthority(
                        spawnPosition,
                        spawnRotation
                    )
                )
                {
                    return false;
                }
            }

            return true;
        }

        private void PersistRunnerAndPlayers(
            NetworkRunner runner
        )
        {
            if (!runnerPersisted)
            {
                DontDestroyOnLoad(
                    runner.gameObject
                ); // Runner를 Game Scene 전환에서 유지

                runnerPersisted = true;
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
                    !persistedPlayerInstanceIds.Add(
                        instanceId
                    )
                )
                {
                    continue;
                }

                runner.MakeDontDestroyOnLoad(
                    playerObject.gameObject
                ); // 참가 Player를 Game Scene 전환에서 유지
            }
        }

        private static ProjectJNetworkExternalGameplay
            GetCoordinatorGameplay(
                NetworkRunner runner
            )
        {
            ProjectJNetworkExternalGameplay selected =
                null;

            int selectedIndex =
                int.MaxValue;

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

                ProjectJNetworkExternalGameplay gameplay =
                    playerObject.GetComponent<
                        ProjectJNetworkExternalGameplay
                    >();

                if (
                    gameplay == null ||
                    player.AsIndex >= selectedIndex
                )
                {
                    continue;
                }

                selected =
                    gameplay;

                selectedIndex =
                    player.AsIndex;
            }

            return selected;
        }

        private static int CountParticipants(
            NetworkRunner runner
        )
        {
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

        private static int CountSpawnedPlayers(
            NetworkRunner runner
        )
        {
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

        private void ResolveBootstrap()
        {
            if (bootstrap != null)
            {
                return;
            }

            bootstrap =
                Object.FindFirstObjectByType<
                    ProjectJFusionBootstrap
                >();
        }

        private void DisableLegacyLobbyFlow()
        {
            if (bootstrap == null)
            {
                return;
            }

            if (legacyLobbyFlow == null)
            {
                legacyLobbyFlow =
                    bootstrap.GetComponent<
                        ProjectJNetworkLobbyFlow
                    >();
            }

            if (
                legacyLobbyFlow != null &&
                legacyLobbyFlow.enabled
            )
            {
                legacyLobbyFlow.enabled = false; // Day76 동안 Day49 자동 진입·자동 시작 차단
            }
        }

        private void ResetForRunner(
            NetworkRunner runner
        )
        {
            trackedRunner = runner;
            sceneLoadRequested = false;
            runnerPersisted = false;
            startRequested = false;

            persistedPlayerInstanceIds.Clear();

            statusText =
                "Day76 Game Scene Session 준비";
        }

        private void ResetDisconnectedState()
        {
            trackedRunner = null;
            sceneLoadRequested = false;
            runnerPersisted = false;
            startRequested = false;

            persistedPlayerInstanceIds.Clear();

            statusText =
                "Session 연결 대기";
        }

        private static void DrawLine(
            float x,
            ref float y,
            float width,
            string text
        )
        {
            GUI.Label(
                new Rect(
                    x,
                    y,
                    width,
                    22f
                ),
                text
            );

            y += 25f;
        }
    }
}
