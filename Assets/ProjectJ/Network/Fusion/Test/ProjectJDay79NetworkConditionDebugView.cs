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
            2; // Day99 Host 1명·Client 1명 측정 기준

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

            float maxRenderStepDistance = // 최대 Render 이동 거리
                0f; // 측정 시작값 초기화

            float maxSimulationOffset = // 최대 Simulation·Render 위치 차이
                0f; // 측정 시작값 초기화

            int totalResimulationBatches =
                0;

            int totalResimulationTicks = // 전체 Resimulation Tick 누적값
                0; // 측정 시작값 초기화

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

                totalResimulationTicks += // 전체 Resimulation Tick 누적
                    networkPlayer // 현재 Network Player 사용
                        .ResimulationTickCount; // 누적 Tick 값 추가

                maxRenderStepDistance = // 최대 Render 이동 거리 갱신
                    Mathf.Max( // 기존 최대값과 현재값 비교
                        maxRenderStepDistance, // 기존 최대 Render 이동 거리
                        networkPlayer // 현재 Network Player 사용
                            .LastRenderStepDistance // 최근 Render 이동 거리
                    );

                maxSimulationOffset = // 최대 Simulation·Render 위치 차이 갱신
                    Mathf.Max( // 기존 최대값과 현재값 비교
                        maxSimulationOffset, // 기존 최대 위치 차이
                        networkPlayer // 현재 Network Player 사용
                            .RenderSimulationOffset // 현재 Simulation·Render 위치 차이
                    );
            }

            if (rttSampleCount > 0)
            {
                averageRtt /=
                    rttSampleCount;

                averageJitter /=
                    rttSampleCount;
            }

            bool twoPlayerMeasurementGate = // Day99 2인 측정 준비 여부
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
                    250f + // Day99 전체 측정 줄 높이 확보
                    players.Count *
                    54f // 플레이어당 두 줄 표시 높이 확보
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
                "DAY 99 - HOST·CLIENT MOVEMENT DIAGNOSTICS / F6 Toggle" // Day99 측정 화면 제목
            );

            DrawLine(
                ref y,
                width,
                "FPS : " +
                smoothedFps.ToString("F1") +
                "    Players : " +
                bootstrap.ParticipantCount +
                " / " + // 목표 참가 인원 구분자
                RequiredPlayerCount + // Day99 목표 참가 인원 표시
                "    Objects : " +
                bootstrap.SpawnedPlayerCount +
                " / " + // 목표 Spawn 수 구분자
                RequiredPlayerCount // Day99 목표 Spawn 수 표시
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
                "    Ticks:" + // 전체 Resimulation Tick 표시
                totalResimulationTicks + // 누적 Tick 값 표시
                "    Max Correction:" +
                maxCorrection.ToString("F3")
            );

            DrawLine( // Render 경로 위치 오차 요약 표시
                ref y, // 다음 출력 위치 갱신
                width, // 현재 진단 창 너비 전달
                "Max Render Step:" + // 최대 Render 이동 거리 레이블
                maxRenderStepDistance.ToString("F3") + // 최대 Render 이동 거리 표시
                "    Max Simulation Offset:" + // 최대 위치 차이 레이블
                maxSimulationOffset.ToString("F3") // 최대 Simulation·Render 위치 차이 표시
            );

            DrawLine(
                ref y,
                width,
                twoPlayerMeasurementGate // 2인 측정 준비 상태 확인
                    ? "2P MEASURE GATE : PASS" // Host·Client 및 PlayerObject 준비 완료
                    : "2P MEASURE GATE : WAIT" // 2인 측정 준비 대기
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

            string playerStateText = // 플레이어 네트워크·경기 상태 첫 줄
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
                gameplay.IsFinished; // 완주 여부로 첫 줄 종료

            DrawLine( // 플레이어 기본 상태 표시
                ref y, // 다음 출력 위치 갱신
                width, // 현재 진단 창 너비 전달
                playerStateText // 네트워크·경기 상태 문자열 표시
            );

            string movementDiagnosticText = // 플레이어 이동 진단 두 번째 줄
                "  Move Corr:" + // 최근·최대 보정 거리 레이블
                networkPlayer.LastCorrectionDistance.ToString("F3") + // 최근 보정 거리 표시
                "/" + // 최근값과 최대값 구분
                networkPlayer.MaxCorrectionDistance.ToString("F3") + // 최대 보정 거리 표시
                " Roll:" + // Rollback 거리 레이블
                networkPlayer.LastRollbackDistance.ToString("F3") + // 최근 Rollback 거리 표시
                " ReSim B/T:" + // Resimulation Batch·Tick 레이블
                networkPlayer.ResimulationBatchCount + // 누적 Batch 수 표시
                "/" + // Batch와 Tick 구분
                networkPlayer.ResimulationTickCount + // 누적 Tick 수 표시
                " Last R/F:" + // 최근 Resimulation·Forward Tick 레이블
                networkPlayer.LastResimulationTickCount + // 최근 Resimulation Tick 수 표시
                "/" + // Resimulation과 Forward 구분
                networkPlayer.LastForwardTickCount + // 최근 Forward Tick 수 표시
                " Step:" + // 최근 Render 이동 거리 레이블
                networkPlayer.LastRenderStepDistance.ToString("F3") + // 최근 Render 이동 거리 표시
                " Offset:" + // Simulation·Render 위치 차이 레이블
                networkPlayer.RenderSimulationOffset.ToString("F3"); // 현재 위치 차이 표시

            DrawLine( // 플레이어 이동 진단 표시
                ref y, // 다음 출력 위치 갱신
                width, // 현재 진단 창 너비 전달
                movementDiagnosticText // 이동 진단 문자열 표시
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
