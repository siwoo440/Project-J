using System.Collections.Generic; // RTT 샘플과 Player 정렬 사용
using Fusion; // NetworkRunner와 NetworkProjectConfig 사용
using UnityEngine; // FPS와 Debug GUI 사용
using UnityEngine.InputSystem; // F6 입력 사용
using UnityEngine.SceneManagement; // Game Scene 확인

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJDay79NetworkConditionDebugView :
        MonoBehaviour
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private const int RequiredPlayerCount =
            8;

        private const float JitterSmoothing =
            0.15f;

        private readonly Dictionary<int, double>
            previousRttByPlayer =
                new Dictionary<int, double>();

        private readonly Dictionary<int, double>
            jitterByPlayer =
                new Dictionary<int, double>();

        private ProjectJFusionBootstrap bootstrap;

        private bool visible =
            true;

        private float smoothedFps;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void Install()
        {
            ProjectJDay79NetworkConditionDebugView existing =
                Object.FindFirstObjectByType<
                    ProjectJDay79NetworkConditionDebugView
                >();

            if (existing != null)
            {
                return;
            }

            GameObject debugObject =
                new GameObject(
                    "=== Project J Day79 Network Condition Debug ==="
                );

            Object.DontDestroyOnLoad(
                debugObject
            );

            debugObject.AddComponent<
                ProjectJDay79NetworkConditionDebugView
            >();
        }

        private void Update()
        {
            UpdateFps();

            Keyboard keyboard =
                Keyboard.current;

            if (
                keyboard != null &&
                keyboard.f6Key.wasPressedThisFrame
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

            UpdateRttSamples();
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

        private void UpdateRttSamples()
        {
            if (
                bootstrap == null ||
                bootstrap.Runner == null ||
                !bootstrap.Runner.IsRunning
            )
            {
                previousRttByPlayer.Clear();
                jitterByPlayer.Clear();
                return;
            }

            NetworkRunner runner =
                bootstrap.Runner;

            foreach (
                PlayerRef player
                in runner.ActivePlayers
            )
            {
                double rtt =
                    runner.GetPlayerRtt(
                        player
                    );

                if (rtt < 0d)
                {
                    continue;
                }

                int key =
                    player.AsIndex;

                if (
                    previousRttByPlayer.TryGetValue(
                        key,
                        out double previousRtt
                    )
                )
                {
                    double delta =
                        System.Math.Abs(
                            rtt -
                            previousRtt
                        );

                    if (
                        jitterByPlayer.TryGetValue(
                            key,
                            out double previousJitter
                        )
                    )
                    {
                        jitterByPlayer[key] =
                            previousJitter +
                            (
                                delta -
                                previousJitter
                            ) *
                            JitterSmoothing;
                    }
                    else
                    {
                        jitterByPlayer[key] =
                            delta;
                    }
                }

                previousRttByPlayer[key] =
                    rtt;
            }
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

            GetNetworkConditionConfig(
                out bool simulationEnabled,
                out double configuredDelay,
                out double configuredJitter,
                out double configuredLoss
            );

            float maxCorrection =
                0f;

            int totalResimulationBatches =
                0;

            double averageRtt =
                0d;

            double maxRtt =
                0d;

            double averageJitter =
                0d;

            int rttSampleCount =
                0;

            for (
                int index = 0;
                index < players.Count;
                index++
            )
            {
                PlayerRef player =
                    players[index];

                double rtt =
                    runner.GetPlayerRtt(
                        player
                    );

                if (rtt >= 0d)
                {
                    averageRtt +=
                        rtt;

                    maxRtt =
                        System.Math.Max(
                            maxRtt,
                            rtt
                        );

                    if (
                        jitterByPlayer.TryGetValue(
                            player.AsIndex,
                            out double jitter
                        )
                    )
                    {
                        averageJitter +=
                            jitter;
                    }

                    rttSampleCount++;
                }

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

                if (networkPlayer == null)
                {
                    continue;
                }

                maxCorrection =
                    Mathf.Max(
                        maxCorrection,
                        networkPlayer
                            .MaxCorrectionDistance
                    );

                totalResimulationBatches +=
                    networkPlayer
                        .ResimulationBatchCount;
            }

            if (rttSampleCount > 0)
            {
                averageRtt /=
                    rttSampleCount;

                averageJitter /=
                    rttSampleCount;
            }

            bool eightPlayerGate =
                bootstrap.ParticipantCount ==
                    RequiredPlayerCount &&
                bootstrap.SpawnedPlayerCount ==
                    RequiredPlayerCount;

            float width =
                Mathf.Min(
                    Screen.width - 24f,
                    980f
                );

            float height =
                Mathf.Min(
                    Screen.height - 24f,
                    220f +
                    players.Count *
                    30f
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
                "DAY 79 - NETWORK CONDITION GATE / F6 Toggle"
            );

            DrawLine(
                ref y,
                width,
                "FPS : " +
                smoothedFps.ToString("F1") +
                "    Players : " +
                bootstrap.ParticipantCount +
                " / 8    Objects : " +
                bootstrap.SpawnedPlayerCount +
                " / 8"
            );

            DrawLine(
                ref y,
                width,
                "SIM CONFIG : " +
                (
                    simulationEnabled
                        ? "ENABLED"
                        : "DISABLED"
                ) +
                "    Delay:" +
                (
                    configuredDelay *
                    1000d
                ).ToString("F0") +
                "ms    Add.Jitter:" +
                (
                    configuredJitter *
                    1000d
                ).ToString("F0") +
                "ms    Loss:" +
                (
                    configuredLoss *
                    100d
                ).ToString("F1") +
                "%"
            );

            DrawLine(
                ref y,
                width,
                "Measured RTT Avg:" +
                (
                    averageRtt *
                    1000d
                ).ToString("F1") +
                "ms    Max:" +
                (
                    maxRtt *
                    1000d
                ).ToString("F1") +
                "ms    RTT Jitter Avg:" +
                (
                    averageJitter *
                    1000d
                ).ToString("F1") +
                "ms"
            );

            DrawLine(
                ref y,
                width,
                "Resimulation Batches:" +
                totalResimulationBatches +
                "    Max Correction:" +
                maxCorrection.ToString("F3")
            );

            DrawLine(
                ref y,
                width,
                eightPlayerGate
                    ? "8P BASE GATE : PASS"
                    : "8P BASE GATE : WAIT"
            );

            DrawLine(
                ref y,
                width,
                "주의: Fusion Debug DLL에서만 내장 Network Conditions가 실제 적용됩니다."
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
            double rtt =
                runner.GetPlayerRtt(
                    player
                );

            double jitter =
                0d;

            jitterByPlayer.TryGetValue(
                player.AsIndex,
                out jitter
            );

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
                    " RTT:" +
                    (
                        rtt *
                        1000d
                    ).ToString("F1") +
                    "ms / PlayerObject 없음"
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
                    " / Network Component 누락"
                );

                return;
            }

            string text =
                "P" +
                player.AsIndex +
                " RTT:" +
                (
                    rtt *
                    1000d
                ).ToString("F1") +
                " Jit:" +
                (
                    jitter *
                    1000d
                ).ToString("F1") +
                " H:" +
                gameplay.RaceHeight.ToString("F2") +
                " R:" +
                gameplay.RaceRank +
                " CP:" +
                gameplay.CurrentCheckpointId +
                " FIN:" +
                gameplay.IsFinished +
                " Corr:" +
                networkPlayer
                    .LastCorrectionDistance
                    .ToString("F3") +
                " Roll:" +
                networkPlayer
                    .LastRollbackDistance
                    .ToString("F3") +
                " ReSim:" +
                networkPlayer
                    .ResimulationBatchCount;

            DrawLine(
                ref y,
                width,
                text
            );
        }

        private static void GetNetworkConditionConfig(
            out bool enabled,
            out double delay,
            out double jitter,
            out double loss
        )
        {
            enabled = false;
            delay = 0d;
            jitter = 0d;
            loss = 0d;

            NetworkProjectConfig config =
                NetworkProjectConfig.Global;

            if (
                config == null ||
                config.NetworkConditions == null
            )
            {
                return;
            }

            NetworkSimulationConfiguration conditions =
                config.NetworkConditions;

            enabled =
                conditions.Enabled;

            delay =
                (
                    conditions.DelayMin +
                    conditions.DelayMax
                ) *
                0.5d;

            jitter =
                conditions.AdditionalJitter;

            loss =
                (
                    conditions.LossChanceMin +
                    conditions.LossChanceMax
                ) *
                0.5d;
        }

        private static List<PlayerRef> CollectPlayers(
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
