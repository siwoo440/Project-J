using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(ProjectJFusionBootstrap)
    )]
    public sealed class
        ProjectJFusionBootstrapDebugView :
            MonoBehaviour
    {
        private ProjectJFusionBootstrap
            bootstrap;

        private bool isVisible;

        private GUIStyle titleStyle;
        private GUIStyle sectionStyle;
        private GUIStyle codeStyle;
        private GUIStyle labelStyle;
        private GUIStyle smallStyle;
        private GUIStyle buttonStyle;

        private void Awake()
        {
            bootstrap =
                GetComponent<
                    ProjectJFusionBootstrap
                >();

            isVisible = false;
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Keyboard keyboard =
                Keyboard.current;

            if (
                keyboard != null &&
                keyboard.f2Key
                    .wasPressedThisFrame
            )
            {
                isVisible =
                    !isVisible;
            }
#endif
        }

        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (
                !isVisible ||
                bootstrap == null
            )
            {
                return;
            }

            EnsureStyles();

            const float x = 20f;
            const float y = 20f;
            const float width = 1180f;
            const float height = 1020f;
            const float left = x + 24f;
            const float contentWidth =
                width - 48f;

            GUI.Box(
                new Rect(
                    x,
                    y,
                    width,
                    height
                ),
                string.Empty
            );

            GUI.Label(
                new Rect(
                    left,
                    y + 14f,
                    560f,
                    34f
                ),
                "Project J - Fusion 64일차",
                titleStyle
            );

            GUI.Label(
                new Rect(
                    x + width - 330f,
                    y + 18f,
                    305f,
                    28f
                ),
                "F2 : 표시/숨김   ALT : 커서",
                labelStyle
            );

            GUI.Label(
                new Rect(
                    left,
                    y + 62f,
                    110f,
                    32f
                ),
                "방 코드",
                labelStyle
            );

            GUI.enabled =
                bootstrap.CanStart;

            bootstrap.RoomCode =
                GUI.TextField(
                    new Rect(
                        left + 120f,
                        y + 58f,
                        250f,
                        40f
                    ),
                    bootstrap.RoomCode,
                    codeStyle
                );

            GUI.enabled = true;

            GUI.Label(
                new Rect(
                    left + 390f,
                    y + 64f,
                    300f,
                    28f
                ),
                "Host 생성 시 6자리 자동 발급",
                labelStyle
            );

            GUI.Label(
                new Rect(
                    left,
                    y + 112f,
                    contentWidth,
                    28f
                ),
                "Session",
                sectionStyle
            );

            DrawInfoRow(
                left,
                y + 146f,
                contentWidth,
                "상태",
                GetStateText(
                    bootstrap.State
                )
            );

            DrawInfoRow(
                left,
                y + 176f,
                contentWidth,
                "역할",
                GetModeText(
                    bootstrap.ActiveMode
                )
            );

            DrawInfoRow(
                left,
                y + 206f,
                contentWidth,
                "현재 방 코드",
                bootstrap.ConnectedRoomCode
            );

            DrawInfoRow(
                left,
                y + 236f,
                contentWidth,
                "참가 / Spawn",
                bootstrap.ParticipantCount +
                " / 8   |   " +
                bootstrap.SpawnedPlayerCount
            );

            DrawInfoRow(
                left,
                y + 266f,
                contentWidth,
                "Region",
                bootstrap.ConnectedRegion
            );

            GUI.Label(
                new Rect(
                    left,
                    y + 306f,
                    contentWidth,
                    28f
                ),
                "Fusion Tick Input · Prediction / Resimulation",
                sectionStyle
            );

            DrawInfoRow(
                left,
                y + 340f,
                contentWidth,
                "ProvideInput",
                GetProvideInputText()
            );

            DrawInfoRow(
                left,
                y + 370f,
                contentWidth,
                "Local PlayerRef",
                GetLocalPlayerText()
            );

            DrawInfoRow(
                left,
                y + 400f,
                contentWidth,
                "Provider Move",
                GetProviderMoveText()
            );

            DrawInfoRow(
                left,
                y + 430f,
                contentWidth,
                "Provider Buttons",
                GetProviderButtonsText()
            );

            DrawInfoRow(
                left,
                y + 460f,
                contentWidth,
                "Input Tick / Count",
                GetProviderTickText()
            );

            ProjectJNetworkPlayer
                localPlayer =
                    GetLocalNetworkPlayer();

            GUI.Label(
                new Rect(
                    left,
                    y + 500f,
                    contentWidth,
                    28f
                ),
                "Local Prediction Diagnostics",
                sectionStyle
            );

            DrawInfoRow(
                left,
                y + 534f,
                contentWidth,
                "Resim Batch / Ticks",
                GetResimulationCountText(
                    localPlayer
                )
            );

            DrawInfoRow(
                left,
                y + 564f,
                contentWidth,
                "Last Resim / Forward",
                GetSimulationTickCountText(
                    localPlayer
                )
            );

            DrawInfoRow(
                left,
                y + 594f,
                contentWidth,
                "Rollback Distance",
                GetRollbackText(
                    localPlayer
                )
            );

            DrawInfoRow(
                left,
                y + 624f,
                contentWidth,
                "Correction / Max",
                GetCorrectionText(
                    localPlayer
                )
            );

            DrawInfoRow(
                left,
                y + 654f,
                contentWidth,
                "Before → Corrected",
                GetPredictionPositionText(
                    localPlayer
                )
            );

            GUI.Label(
                new Rect(
                    left,
                    y + 696f,
                    contentWidth,
                    28f
                ),
                "Remote NetworkTransform Interpolation",
                sectionStyle
            );

            DrawInterpolationHeader(
                left,
                y + 730f
            );

            DrawPlayerRows(
                left,
                y + 759f
            );

            GUI.enabled =
                bootstrap.CanStart;

            if (
                GUI.Button(
                    new Rect(
                        left,
                        y + 958f,
                        260f,
                        42f
                    ),
                    "비공개 방 생성",
                    buttonStyle
                )
            )
            {
                bootstrap
                    .RequestCreatePrivateRoom();
            }

            if (
                GUI.Button(
                    new Rect(
                        left + 280f,
                        y + 958f,
                        260f,
                        42f
                    ),
                    "방 코드로 참가",
                    buttonStyle
                )
            )
            {
                bootstrap
                    .RequestJoinPrivateRoom();
            }

            GUI.enabled =
                bootstrap.CanShutdown;

            if (
                GUI.Button(
                    new Rect(
                        left + 560f,
                        y + 958f,
                        260f,
                        42f
                    ),
                    "방 나가기",
                    buttonStyle
                )
            )
            {
                bootstrap
                    .RequestLeaveRoom();
            }

            GUI.enabled = true;
#endif
        }

        private void DrawInterpolationHeader(
            float x,
            float y
        )
        {
            DrawSmall(
                x,
                y,
                60f,
                "Player"
            );

            DrawSmall(
                x + 60f,
                y,
                150f,
                "Role"
            );

            DrawSmall(
                x + 210f,
                y,
                240f,
                "Simulation Position"
            );

            DrawSmall(
                x + 450f,
                y,
                240f,
                "Render Position"
            );

            DrawSmall(
                x + 690f,
                y,
                120f,
                "Offset"
            );

            DrawSmall(
                x + 810f,
                y,
                120f,
                "Render Δ"
            );

            DrawSmall(
                x + 930f,
                y,
                160f,
                "Interpolation"
            );
        }

        private ProjectJNetworkPlayer
            GetLocalNetworkPlayer()
        {
            NetworkRunner runner =
                bootstrap.Runner;

            if (
                runner == null ||
                !runner.IsRunning
            )
            {
                return null;
            }

            if (
                !runner.TryGetPlayerObject(
                    runner.LocalPlayer,
                    out NetworkObject playerObject
                ) ||
                playerObject == null
            )
            {
                return null;
            }

            playerObject.TryGetComponent(
                out ProjectJNetworkPlayer
                    networkPlayer
            );

            return networkPlayer;
        }

        private string GetResimulationCountText(
            ProjectJNetworkPlayer player
        )
        {
            if (player == null)
            {
                return "-";
            }

            return
                player.ResimulationBatchCount +
                " / " +
                player.ResimulationTickCount;
        }

        private string GetSimulationTickCountText(
            ProjectJNetworkPlayer player
        )
        {
            if (player == null)
            {
                return "-";
            }

            return
                player.LastResimulationTickCount +
                " / " +
                player.LastForwardTickCount;
        }

        private string GetRollbackText(
            ProjectJNetworkPlayer player
        )
        {
            if (player == null)
            {
                return "-";
            }

            return
                player.LastRollbackDistance
                    .ToString("0.000") +
                " m";
        }

        private string GetCorrectionText(
            ProjectJNetworkPlayer player
        )
        {
            if (player == null)
            {
                return "-";
            }

            return
                player.LastCorrectionDistance
                    .ToString("0.000") +
                " m / " +
                player.MaxCorrectionDistance
                    .ToString("0.000") +
                " m";
        }

        private string GetPredictionPositionText(
            ProjectJNetworkPlayer player
        )
        {
            if (player == null)
            {
                return "-";
            }

            return
                FormatPosition(
                    player
                        .PredictionPositionBeforeResimulation
                ) +
                " → " +
                FormatPosition(
                    player
                        .CorrectedPositionAfterResimulation
                );
        }

        private void DrawPlayerRows(
            float x,
            float startY
        )
        {
            NetworkRunner runner =
                bootstrap.Runner;

            if (
                runner == null ||
                !runner.IsRunning
            )
            {
                GUI.Label(
                    new Rect(
                        x,
                        startY,
                        700f,
                        26f
                    ),
                    "연결된 Network Player 없음",
                    labelStyle
                );

                return;
            }

            int row = 0;

            foreach (
                PlayerRef player
                in runner.ActivePlayers
            )
            {
                if (row >= 6)
                {
                    break;
                }

                float y =
                    startY +
                    row * 32f;

                NetworkObject playerObject =
                    null;

                ProjectJNetworkPlayer
                    networkPlayer =
                        null;

                bool hasObject =
                    runner.TryGetPlayerObject(
                        player,
                        out playerObject
                    ) &&
                    playerObject != null;

                if (hasObject)
                {
                    playerObject.TryGetComponent(
                        out networkPlayer
                    );
                }

                DrawSmall(
                    x,
                    y,
                    60f,
                    "P" + player.AsIndex
                );

                DrawSmall(
                    x + 60f,
                    y,
                    150f,
                    GetInterpolationRoleText(
                        networkPlayer
                    )
                );

                DrawSmall(
                    x + 210f,
                    y,
                    240f,
                    networkPlayer != null
                        ? FormatPosition(
                            networkPlayer
                                .LastSimulationPosition
                        )
                        : "-"
                );

                DrawSmall(
                    x + 450f,
                    y,
                    240f,
                    networkPlayer != null
                        ? FormatPosition(
                            networkPlayer
                                .LastRenderPosition
                        )
                        : "-"
                );

                DrawSmall(
                    x + 690f,
                    y,
                    120f,
                    networkPlayer != null
                        ? networkPlayer
                            .RenderSimulationOffset
                            .ToString("0.000")
                        : "-"
                );

                DrawSmall(
                    x + 810f,
                    y,
                    120f,
                    networkPlayer != null
                        ? networkPlayer
                            .LastRenderStepDistance
                            .ToString("0.000")
                        : "-"
                );

                DrawSmall(
                    x + 930f,
                    y,
                    160f,
                    GetInterpolationStateText(
                        networkPlayer
                    )
                );

                row++;
            }
        }

        private static string
            GetInterpolationRoleText(
                ProjectJNetworkPlayer player
            )
        {
            if (player == null)
            {
                return "-";
            }

            if (player.HasLocalInputAuthority)
            {
                return "LOCAL";
            }

            if (player.IsRemoteProxy)
            {
                return "REMOTE PROXY";
            }

            if (player.HasLocalStateAuthority)
            {
                return "REMOTE STATE";
            }

            return "REMOTE";
        }

        private static string
            GetInterpolationStateText(
                ProjectJNetworkPlayer player
            )
        {
            if (player == null)
            {
                return "-";
            }

            if (!player.HasNetworkTransform)
            {
                return "NO NT";
            }

            if (player.RemoteInterpolationExpected)
            {
                return "AUTO";
            }

            return "LOCAL";
        }

        private void DrawSmall(
            float x,
            float y,
            float width,
            string value
        )
        {
            GUI.Label(
                new Rect(
                    x,
                    y,
                    width,
                    26f
                ),
                value,
                smallStyle
            );
        }

        private void DrawInfoRow(
            float x,
            float y,
            float width,
            string label,
            string value
        )
        {
            GUI.Label(
                new Rect(
                    x,
                    y,
                    190f,
                    28f
                ),
                label,
                labelStyle
            );

            GUI.Label(
                new Rect(
                    x + 195f,
                    y,
                    width - 195f,
                    28f
                ),
                ": " + value,
                labelStyle
            );
        }

        private string GetProvideInputText()
        {
            NetworkRunner runner =
                bootstrap.Runner;

            if (
                runner == null ||
                !runner.IsRunning
            )
            {
                return "-";
            }

            return runner.ProvideInput
                ? "TRUE"
                : "FALSE";
        }

        private string GetLocalPlayerText()
        {
            NetworkRunner runner =
                bootstrap.Runner;

            if (
                runner == null ||
                !runner.IsRunning
            )
            {
                return "-";
            }

            return
                "P" +
                runner.LocalPlayer.AsIndex;
        }

        private string GetProviderMoveText()
        {
            ProjectJFusionInputProvider provider =
                bootstrap.InputProvider;

            if (provider == null)
            {
                return "-";
            }

            return FormatMove(
                provider.LastSubmittedMove
            );
        }

        private string GetProviderButtonsText()
        {
            ProjectJFusionInputProvider provider =
                bootstrap.InputProvider;

            if (provider == null)
            {
                return "-";
            }

            return
                "Jump " +
                IsTrueText(
                    provider.LastSubmittedJump
                ) +
                "   Sprint " +
                IsTrueText(
                    provider.LastSubmittedSprint
                ) +
                "   Crouch " +
                IsTrueText(
                    provider.LastSubmittedCrouch
                );
        }

        private string GetProviderTickText()
        {
            ProjectJFusionInputProvider provider =
                bootstrap.InputProvider;

            if (provider == null)
            {
                return "-";
            }

            return
                provider.LastSubmittedTick +
                " / " +
                provider.SubmitCount;
        }

        private static string FormatMove(
            Vector2 move
        )
        {
            return
                "(" +
                move.x.ToString("0.00") +
                ", " +
                move.y.ToString("0.00") +
                ")";
        }

        private static string FormatPosition(
            Vector3 position
        )
        {
            return
                "(" +
                position.x.ToString("0.00") +
                ", " +
                position.y.ToString("0.00") +
                ", " +
                position.z.ToString("0.00") +
                ")";
        }

        private static string IsTrueText(
            bool value
        )
        {
            return value
                ? "TRUE"
                : "-";
        }

        private static string GetStateText(
            ProjectJFusionBootstrapState state
        )
        {
            switch (state)
            {
                case
                    ProjectJFusionBootstrapState
                        .Starting:
                    return "연결 중";

                case
                    ProjectJFusionBootstrapState
                        .Running:
                    return "연결됨";

                case
                    ProjectJFusionBootstrapState
                        .Stopping:
                    return "종료 중";

                case
                    ProjectJFusionBootstrapState
                        .Failed:
                    return "실패";

                default:
                    return "대기";
            }
        }

        private static string GetModeText(
            GameMode? gameMode
        )
        {
            if (!gameMode.HasValue)
            {
                return "없음";
            }

            switch (gameMode.Value)
            {
                case GameMode.Host:
                    return "호스트";

                case GameMode.Client:
                    return "클라이언트";

                default:
                    return
                        gameMode.Value.ToString();
            }
        }

        private void EnsureStyles()
        {
            if (titleStyle == null)
            {
                titleStyle =
                    new GUIStyle(
                        GUI.skin.label
                    );

                titleStyle.fontSize = 25;
                titleStyle.fontStyle =
                    FontStyle.Bold;
            }

            if (sectionStyle == null)
            {
                sectionStyle =
                    new GUIStyle(
                        GUI.skin.label
                    );

                sectionStyle.fontSize = 19;
                sectionStyle.fontStyle =
                    FontStyle.Bold;
            }

            if (codeStyle == null)
            {
                codeStyle =
                    new GUIStyle(
                        GUI.skin.textField
                    );

                codeStyle.fontSize = 22;
                codeStyle.fontStyle =
                    FontStyle.Bold;
                codeStyle.alignment =
                    TextAnchor.MiddleCenter;
            }

            if (labelStyle == null)
            {
                labelStyle =
                    new GUIStyle(
                        GUI.skin.label
                    );

                labelStyle.fontSize = 16;
            }

            if (smallStyle == null)
            {
                smallStyle =
                    new GUIStyle(
                        GUI.skin.label
                    );

                smallStyle.fontSize = 14;
            }

            if (buttonStyle == null)
            {
                buttonStyle =
                    new GUIStyle(
                        GUI.skin.button
                    );

                buttonStyle.fontSize = 16;
                buttonStyle.fontStyle =
                    FontStyle.Bold;
            }
        }
    }
}
