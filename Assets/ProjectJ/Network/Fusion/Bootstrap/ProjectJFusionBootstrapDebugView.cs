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
            const float width = 1000f;
            const float height = 900f;
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
                    460f,
                    34f
                ),
                "Project J - Fusion 61일차",
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
                "Fusion Tick Input",
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

            GUI.Label(
                new Rect(
                    left,
                    y + 505f,
                    90f,
                    26f
                ),
                "Player",
                smallStyle
            );

            GUI.Label(
                new Rect(
                    left + 90f,
                    y + 505f,
                    100f,
                    26f
                ),
                "State",
                smallStyle
            );

            GUI.Label(
                new Rect(
                    left + 190f,
                    y + 505f,
                    100f,
                    26f
                ),
                "Input",
                smallStyle
            );

            GUI.Label(
                new Rect(
                    left + 290f,
                    y + 505f,
                    165f,
                    26f
                ),
                "Move",
                smallStyle
            );

            GUI.Label(
                new Rect(
                    left + 455f,
                    y + 505f,
                    85f,
                    26f
                ),
                "Jump",
                smallStyle
            );

            GUI.Label(
                new Rect(
                    left + 540f,
                    y + 505f,
                    85f,
                    26f
                ),
                "Sprint",
                smallStyle
            );

            GUI.Label(
                new Rect(
                    left + 625f,
                    y + 505f,
                    85f,
                    26f
                ),
                "Crouch",
                smallStyle
            );

            GUI.Label(
                new Rect(
                    left + 710f,
                    y + 505f,
                    90f,
                    26f
                ),
                "Camera",
                smallStyle
            );

            GUI.Label(
                new Rect(
                    left + 800f,
                    y + 505f,
                    130f,
                    26f
                ),
                "Received Tick",
                smallStyle
            );

            DrawPlayerRows(
                left,
                y + 534f
            );

            GUI.enabled =
                bootstrap.CanStart;

            if (
                GUI.Button(
                    new Rect(
                        left,
                        y + 838f,
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
                        y + 838f,
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
                        y + 838f,
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
                if (row >= 8)
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
                    90f,
                    "P" + player.AsIndex
                );

                DrawSmall(
                    x + 90f,
                    y,
                    100f,
                    hasObject &&
                    playerObject.HasStateAuthority
                        ? "TRUE"
                        : "FALSE"
                );

                DrawSmall(
                    x + 190f,
                    y,
                    100f,
                    hasObject &&
                    playerObject.HasInputAuthority
                        ? "TRUE"
                        : "FALSE"
                );

                DrawSmall(
                    x + 290f,
                    y,
                    165f,
                    networkPlayer != null &&
                    networkPlayer.HasReceivedInput
                        ? FormatMove(
                            networkPlayer
                                .LastReceivedMove
                        )
                        : "-"
                );

                DrawSmall(
                    x + 455f,
                    y,
                    85f,
                    IsTrueText(
                        networkPlayer != null &&
                        networkPlayer
                            .LastReceivedJump
                    )
                );

                DrawSmall(
                    x + 540f,
                    y,
                    85f,
                    IsTrueText(
                        networkPlayer != null &&
                        networkPlayer
                            .LastReceivedSprint
                    )
                );

                DrawSmall(
                    x + 625f,
                    y,
                    85f,
                    IsTrueText(
                        networkPlayer != null &&
                        networkPlayer
                            .LastReceivedCrouch
                    )
                );

                DrawSmall(
                    x + 710f,
                    y,
                    90f,
                    networkPlayer != null &&
                    networkPlayer
                        .AuthorityCameraEnabled
                        ? "ON"
                        : "OFF"
                );

                DrawSmall(
                    x + 800f,
                    y,
                    130f,
                    networkPlayer != null
                        ? networkPlayer
                            .LastReceivedTick
                        : "-"
                );

                row++;
            }
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
                    170f,
                    28f
                ),
                label,
                labelStyle
            );

            GUI.Label(
                new Rect(
                    x + 175f,
                    y,
                    width - 175f,
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
