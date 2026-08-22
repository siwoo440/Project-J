using System.Collections.Generic; // Player 목록 정렬 사용
using Fusion; // NetworkRunner와 PlayerRef 사용
using UnityEngine; // FPS와 Debug GUI 사용
using UnityEngine.InputSystem; // F5 입력 사용
using UnityEngine.SceneManagement; // Game Scene 확인

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJDay78EightPlayerDebugView :
        MonoBehaviour
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private const int RequiredPlayerCount =
            8;

        private ProjectJFusionBootstrap bootstrap;

        private bool visible =
            true;

        private float smoothedFps;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void Install()
        {
            ProjectJDay78EightPlayerDebugView existing =
                Object.FindFirstObjectByType<
                    ProjectJDay78EightPlayerDebugView
                >();

            if (existing != null)
            {
                return;
            }

            GameObject debugObject =
                new GameObject(
                    "=== Project J Day78 8P Debug ==="
                );

            Object.DontDestroyOnLoad(
                debugObject
            );

            debugObject.AddComponent<
                ProjectJDay78EightPlayerDebugView
            >();
        }

        private void Update()
        {
            UpdateFps();

            Keyboard keyboard =
                Keyboard.current;

            if (
                keyboard != null &&
                keyboard.f5Key.wasPressedThisFrame
            )
            {
                visible =
                    !visible;
            }

            if (bootstrap == null)
            {
                bootstrap =
                    Object.FindFirstObjectByType<
                        ProjectJFusionBootstrap
                    >();
            }
        }

        private void UpdateFps()
        {
            float deltaTime =
                Time.unscaledDeltaTime;

            if (deltaTime <= 0.0001f)
            {
                return;
            }

            float currentFps =
                1f /
                deltaTime;

            if (smoothedFps <= 0f)
            {
                smoothedFps =
                    currentFps;

                return;
            }

            smoothedFps =
                Mathf.Lerp(
                    smoothedFps,
                    currentFps,
                    0.1f
                );
        }

        private void OnGUI()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return;
#else
            if (
                !visible ||
                SceneManager.GetActiveScene().path !=
                    GameScenePath ||
                bootstrap == null ||
                bootstrap.Runner == null ||
                !bootstrap.Runner.IsRunning
            )
            {
                return;
            }

            NetworkRunner runner =
                bootstrap.Runner;

            List<PlayerRef> players =
                CollectPlayers(
                    runner
                );

            int localInputAuthorityCount = 0;
            int localStateAuthorityCount = 0;
            int finishedCount = 0;
            int totalResimulationBatches = 0;

            float highestCorrection =
                0f;

            for (
                int index = 0;
                index < players.Count;
                index++
            )
            {
                PlayerRef player =
                    players[index];

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

                ProjectJNetworkPlayer networkPlayer =
                    playerObject.GetComponent<
                        ProjectJNetworkPlayer
                    >();

                ProjectJNetworkExternalGameplay gameplay =
                    playerObject.GetComponent<
                        ProjectJNetworkExternalGameplay
                    >();

                if (networkPlayer != null)
                {
                    if (
                        networkPlayer.HasLocalInputAuthority
                    )
                    {
                        localInputAuthorityCount++;
                    }

                    if (
                        networkPlayer.HasLocalStateAuthority
                    )
                    {
                        localStateAuthorityCount++;
                    }

                    totalResimulationBatches +=
                        networkPlayer
                            .ResimulationBatchCount;

                    highestCorrection =
                        Mathf.Max(
                            highestCorrection,
                            networkPlayer
                                .MaxCorrectionDistance
                        );
                }

                if (
                    gameplay != null &&
                    gameplay.IsFinished
                )
                {
                    finishedCount++;
                }
            }

            bool connectionGatePassed =
                bootstrap.ParticipantCount ==
                    RequiredPlayerCount &&
                bootstrap.SpawnedPlayerCount ==
                    RequiredPlayerCount &&
                localInputAuthorityCount == 1;

            float width =
                Mathf.Min(
                    Screen.width - 24f,
                    930f
                );

            float height =
                Mathf.Min(
                    Screen.height - 24f,
                    175f +
                    players.Count *
                    31f
                );

            GUI.Box(
                new Rect(
                    12f,
                    12f,
                    width,
                    height
                ),
                string.Empty
            );

            float y =
                20f;

            DrawLine(
                ref y,
                width,
                "DAY 78 - 8 PLAYER GATE / F5 Toggle"
            );

            DrawLine(
                ref y,
                width,
                "FPS : " +
                smoothedFps.ToString("F1") +
                "    ROLE : " +
                GetRoleText() +
                "    SESSION : " +
                (
                    bootstrap.IsSessionOpen
                        ? "OPEN"
                        : "CLOSED"
                )
            );

            DrawLine(
                ref y,
                width,
                "Participants : " +
                bootstrap.ParticipantCount +
                " / 8    PlayerObjects : " +
                bootstrap.SpawnedPlayerCount +
                " / 8    Local InputAuthority : " +
                localInputAuthorityCount
            );

            DrawLine(
                ref y,
                width,
                "Local StateAuthority Objects : " +
                localStateAuthorityCount +
                "    Finished : " +
                finishedCount +
                " / 8"
            );

            DrawLine(
                ref y,
                width,
                "Resimulation Batches : " +
                totalResimulationBatches +
                "    Max Correction : " +
                highestCorrection.ToString("F3")
            );

            DrawLine(
                ref y,
                width,
                connectionGatePassed
                    ? "8P CONNECTION GATE : PASS"
                    : "8P CONNECTION GATE : WAIT - GAME START는 8/8 이후 사용"
            );

            y +=
                4f;

            for (
                int index = 0;
                index < players.Count;
                index++
            )
            {
                DrawPlayerLine(
                    runner,
                    players[index],
                    ref y,
                    width
                );
            }
#endif
        }

        private void DrawPlayerLine(
            NetworkRunner runner,
            PlayerRef player,
            ref float y,
            float width
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
                DrawLine(
                    ref y,
                    width,
                    "P" +
                    player.AsIndex +
                    " : PlayerObject 없음"
                );

                return;
            }

            ProjectJNetworkPlayer networkPlayer =
                playerObject.GetComponent<
                    ProjectJNetworkPlayer
                >();

            ProjectJNetworkExternalGameplay gameplay =
                playerObject.GetComponent<
                    ProjectJNetworkExternalGameplay
                >();

            if (
                networkPlayer == null ||
                gameplay == null
            )
            {
                DrawLine(
                    ref y,
                    width,
                    "P" +
                    player.AsIndex +
                    " : Network Component 누락"
                );

                return;
            }

            string authorityText =
                networkPlayer.HasLocalInputAuthority
                    ? "LOCAL"
                    : (
                        networkPlayer.HasLocalStateAuthority
                            ? "STATE"
                            : "REMOTE"
                    );

            string text =
                "P" +
                player.AsIndex +
                " [" +
                authorityText +
                "]" +
                "  H:" +
                gameplay.RaceHeight.ToString("F2") +
                "  R:" +
                gameplay.RaceRank +
                "  CP:" +
                gameplay.CurrentCheckpointId +
                "  FIN:" +
                gameplay.IsFinished +
                "  Corr:" +
                networkPlayer
                    .LastCorrectionDistance
                    .ToString("F3") +
                "  Max:" +
                networkPlayer
                    .MaxCorrectionDistance
                    .ToString("F3") +
                "  Roll:" +
                networkPlayer
                    .LastRollbackDistance
                    .ToString("F3") +
                "  ReSim:" +
                networkPlayer
                    .ResimulationBatchCount;

            DrawLine(
                ref y,
                width,
                text
            );
        }

        private List<PlayerRef> CollectPlayers(
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
                players.Add(
                    player
                );
            }

            players.Sort(
                (
                    left,
                    right
                ) =>
                    left.AsIndex.CompareTo(
                        right.AsIndex
                    )
            );

            return players;
        }

        private string GetRoleText()
        {
            if (
                bootstrap == null ||
                !bootstrap.ActiveMode.HasValue
            )
            {
                return "-";
            }

            return
                bootstrap.ActiveMode
                    .Value
                    .ToString();
        }

        private static void DrawLine(
            ref float y,
            float width,
            string text
        )
        {
            GUI.Label(
                new Rect(
                    22f,
                    y,
                    width - 20f,
                    23f
                ),
                text
            );

            y +=
                27f;
        }
    }
}
