using System.Collections.Generic; // RTT 샘플과 Player 정렬 사용
using Fusion; // NetworkRunner와 NetworkProjectConfig 사용
using ProjectJ.Debugging; // 이동 품질 역할·최대값 정책 사용
using UnityEngine; // FPS와 Debug GUI 사용
using UnityEngine.InputSystem; // F6 표시와 F10 초기화 입력 사용
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

        private float measurementStartedAt; // 현재 측정 구간 시작 시각

        private float peakRenderStepDistance; // 구간 최대 Render 이동 거리

        private float peakSimulationOffset; // 구간 최대 Simulation Offset

        private float peakCameraStepDistance; // 구간 최대 로컬 카메라 이동 거리

        private float peakCameraFollowOffset; // 구간 최대 카메라 추적 오차

        private bool measurementRunning; // 측정 구간 활성화 여부

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

            if ( // 측정 초기화 입력 확인
                keyboard != null && // 키보드 연결 확인
                keyboard.f10Key.wasPressedThisFrame // F10 현재 프레임 입력 확인
            )
            {
                ResetMeasurementDiagnostics(); // 현재 측정 구간 초기화
            }

            UpdateRttSamples();
            UpdateMeasurementPeaks(); // 역할별 이동·카메라 최대값 누적
        }

        private void UpdateMeasurementPeaks() // 현재 측정 구간 최대값 갱신
        {
            if ( // 실행 중인 Runner 확인
                bootstrap == null || // Bootstrap 누락 확인
                bootstrap.Runner == null || // Runner 누락 확인
                !bootstrap.Runner.IsRunning // Runner 실행 상태 확인
            )
            {
                measurementRunning = // 측정 활성 상태 해제
                    false; // 다음 경기에서 새 구간 시작

                return; // 최대값 갱신 중단
            }

            if (!measurementRunning) // 새 경기의 측정 시작 여부 확인
            {
                ClearPeakMeasurements(); // 이전 경기 최대값 제거

                measurementStartedAt = // 측정 시작 시각 저장
                    Time.unscaledTime; // 현재 비스케일 시간 사용

                measurementRunning = // 측정 활성 상태 설정
                    true; // 최대값 누적 시작
            }

            NetworkRunner runner = // 현재 Fusion Runner 조회
                bootstrap.Runner; // Bootstrap Runner 사용

            foreach (PlayerRef player in runner.ActivePlayers) // 참가 Player 전체 순회
            {
                if ( // Player Object 조회 확인
                    !runner.TryGetPlayerObject( // 참가자 Player Object 검색
                        player, // 현재 PlayerRef 전달
                        out NetworkObject playerObject // 검색 결과 저장
                    ) ||
                    playerObject == null // Player Object 누락 확인
                )
                {
                    continue; // 다음 Player 확인
                }

                ProjectJNetworkPlayer networkPlayer = // 이동 진단 Component 조회
                    playerObject.GetComponent< // Player Object Component 검색
                        ProjectJNetworkPlayer // 대상 이동 Component
                    >();

                if (networkPlayer == null) // 이동 Component 누락 확인
                {
                    continue; // 다음 Player 확인
                }

                peakRenderStepDistance = // Render 이동 최대값 갱신
                    ProjectJMovementQualityPolicy.AccumulatePeak( // 최대값 누적 정책 호출
                        peakRenderStepDistance, // 기존 구간 최대값
                        networkPlayer.LastRenderStepDistance // 현재 Player 표본
                    );

                peakSimulationOffset = // Simulation Offset 최대값 갱신
                    ProjectJMovementQualityPolicy.AccumulatePeak( // 최대값 누적 정책 호출
                        peakSimulationOffset, // 기존 구간 최대값
                        networkPlayer.RenderSimulationOffset // 현재 Player 표본
                    );
            }

            ProjectJLocalPlayerPresentationController localPresentation = // 로컬 카메라 관리자 조회
                ProjectJLocalPlayerPresentationController.Instance; // Runtime 자동 설치 Instance 사용

            if (localPresentation == null) // 로컬 카메라 관리자 누락 확인
            {
                return; // 카메라 최대값 갱신 생략
            }

            peakCameraStepDistance = // 로컬 카메라 이동 최대값 갱신
                ProjectJMovementQualityPolicy.AccumulatePeak( // 최대값 누적 정책 호출
                    peakCameraStepDistance, // 기존 구간 최대값
                    localPresentation.CameraStepDistance // 현재 카메라 표본
                );

            peakCameraFollowOffset = // 카메라 추적 오차 최대값 갱신
                ProjectJMovementQualityPolicy.AccumulatePeak( // 최대값 누적 정책 호출
                    peakCameraFollowOffset, // 기존 구간 최대값
                    localPresentation.CameraFollowOffset // 현재 추적 오차 표본
                );
        }

        private void ResetMeasurementDiagnostics() // F10 측정 구간 초기화
        {
            previousRttByPlayer.Clear(); // 이전 RTT 표본 제거
            jitterByPlayer.Clear(); // 이전 Jitter 누적값 제거
            ClearPeakMeasurements(); // 구간 최대값 제거

            measurementStartedAt = // 새 측정 시작 시각 저장
                Time.unscaledTime; // 현재 비스케일 시간 사용

            measurementRunning = // 측정 활성 상태 갱신
                bootstrap != null && // Bootstrap 존재 확인
                bootstrap.Runner != null && // Runner 존재 확인
                bootstrap.Runner.IsRunning; // Runner 실행 여부 사용

            if (!measurementRunning) // 실행 중인 경기가 없는지 확인
            {
                return; // Player 진단 초기화 생략
            }

            NetworkRunner runner = // 현재 Fusion Runner 조회
                bootstrap.Runner; // Bootstrap Runner 사용

            foreach (PlayerRef player in runner.ActivePlayers) // 참가 Player 전체 순회
            {
                if ( // Player Object 조회 확인
                    !runner.TryGetPlayerObject( // 참가자 Player Object 검색
                        player, // 현재 PlayerRef 전달
                        out NetworkObject playerObject // 검색 결과 저장
                    ) ||
                    playerObject == null // Player Object 누락 확인
                )
                {
                    continue; // 다음 Player 확인
                }

                ProjectJNetworkPlayer networkPlayer = // 이동 진단 Component 조회
                    playerObject.GetComponent< // Player Object Component 검색
                        ProjectJNetworkPlayer // 대상 이동 Component
                    >();

                if (networkPlayer == null) // 이동 Component 누락 확인
                {
                    continue; // 다음 Player 확인
                }

                networkPlayer.ResetMovementDiagnostics(); // Player별 진단 누적값 초기화
            }
        }

        private void ClearPeakMeasurements() // 화면 구간 최대값 초기화
        {
            peakRenderStepDistance = // Render 이동 최대값 초기화
                0f; // 측정 시작값 사용

            peakSimulationOffset = // Simulation Offset 최대값 초기화
                0f; // 측정 시작값 사용

            peakCameraStepDistance = // 카메라 이동 최대값 초기화
                0f; // 측정 시작값 사용

            peakCameraFollowOffset = // 카메라 추적 오차 최대값 초기화
                0f; // 측정 시작값 사용
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

            bool twoPlayerMeasurementGate = // Day100 2인 개선 확인 준비 여부
                bootstrap.ParticipantCount ==
                    RequiredPlayerCount &&
                bootstrap.SpawnedPlayerCount ==
                    RequiredPlayerCount;

            ProjectJLocalPlayerPresentationController localPresentation = // 현재 Client 로컬 표시 관리자
                ProjectJLocalPlayerPresentationController.Instance; // Runtime 자동 설치 Instance 조회

            float localCameraStepDistance = // 최근 로컬 카메라 이동 거리
                localPresentation != null // 표시 관리자 존재 여부 확인
                    ? localPresentation.CameraStepDistance // 실제 최근 이동 거리 사용
                    : 0f; // 관리자 없음 기본값

            float localCameraFollowOffset = // 로컬 카메라 목표 추적 오차
                localPresentation != null // 표시 관리자 존재 여부 확인
                    ? localPresentation.CameraFollowOffset // 실제 목표 추적 오차 사용
                    : 0f; // 관리자 없음 기본값

            float measurementElapsed = // 현재 측정 구간 경과 시간
                measurementRunning // 측정 활성 상태 확인
                    ? ProjectJMovementQualityPolicy.CalculateElapsed( // 경과 시간 정책 호출
                        measurementStartedAt, // 구간 시작 시각 전달
                        Time.unscaledTime // 현재 비스케일 시간 전달
                    )
                    : 0f; // 측정 전 기본값

            float width =
                Mathf.Min(
                    Screen.width - 24f,
                    980f
                );

            float height =
                Mathf.Min(
                    Screen.height - 24f,
                    331f + // Day101 구간 측정 두 줄 높이 확보
                    players.Count *
                    81f // 플레이어당 세 줄 표시 높이 확보
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
                "DAY 101 - REMOTE PLAYER MOVEMENT QUALITY / F6 Toggle" // Day101 원격 이동 품질 화면 제목
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

            DrawLine( // 로컬 카메라 보간 결과 표시
                ref y, // 다음 출력 위치 갱신
                width, // 현재 진단 창 너비 전달
                "Local Camera Step:" + // 최근 카메라 이동 거리 레이블
                localCameraStepDistance.ToString("F3") + // 최근 카메라 이동 거리 표시
                "    Follow Offset:" + // 카메라 목표 추적 오차 레이블
                localCameraFollowOffset.ToString("F3") // 카메라 목표 추적 오차 표시
            );

            DrawLine( // 현재 측정 구간 상태 표시
                ref y, // 다음 출력 위치 갱신
                width, // 현재 진단 창 너비 전달
                "MEASURE : " + // 측정 구간 레이블
                measurementElapsed.ToString("F1") + // 경과 시간 표시
                "s    F10 : RESET MEASUREMENT" // 충돌 없는 초기화 단축키 안내
            );

            DrawLine( // 현재 측정 구간 최대값 표시
                ref y, // 다음 출력 위치 갱신
                width, // 현재 진단 창 너비 전달
                "PEAK Step:" + // 최대 Render 이동 거리 레이블
                peakRenderStepDistance.ToString("F3") + // 최대 Render 이동 거리 표시
                " Offset:" + // 최대 Simulation Offset 레이블
                peakSimulationOffset.ToString("F3") + // 최대 Simulation Offset 표시
                " CameraStep:" + // 최대 카메라 이동 거리 레이블
                peakCameraStepDistance.ToString("F3") + // 최대 카메라 이동 거리 표시
                " Follow:" + // 최대 카메라 추적 오차 레이블
                peakCameraFollowOffset.ToString("F3") // 최대 카메라 추적 오차 표시
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
                " [" + // 역할 표시 시작 구분자
                ProjectJMovementQualityPolicy.GetRoleLabel( // 현재 PC 기준 역할 판정
                    networkPlayer.HasLocalInputAuthority, // Input Authority 보유 여부 전달
                    networkPlayer.HasLocalStateAuthority // State Authority 보유 여부 전달
                ) +
                "]" + // 역할 표시 종료 구분자
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

            string movementStateText = // 플레이어 현재 동작 상태 세 번째 줄
                "  State:" + // 동작 상태 레이블
                (
                    networkPlayer.LastReceivedMove.sqrMagnitude > 0.0001f // 이동 입력 존재 여부 확인
                        ? "MOVE" // 이동 중 표시
                        : "IDLE" // 정지 상태 표시
                ) +
                " Sprint:" + // 달리기 상태 레이블
                networkPlayer.IsSprinting + // 현재 달리기 상태 표시
                " Jump:" + // 점프 입력 상태 레이블
                networkPlayer.LastReceivedJump + // 최근 점프 입력 표시
                " Ground:" + // 지면 상태 레이블
                networkPlayer.IsGrounded + // 현재 지면 판정 표시
                " Crouch:" + // 앉기 상태 레이블
                networkPlayer.IsCrouching + // 현재 앉기 상태 표시
                " Speed:" + // 현재 이동 속도 레이블
                networkPlayer.MovementSpeed.ToString("F1"); // 현재 이동 속도 표시

            DrawLine( // 플레이어 현재 동작 상태 표시
                ref y, // 다음 출력 위치 갱신
                width, // 현재 진단 창 너비 전달
                movementStateText // 동작 상태 문자열 표시
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
