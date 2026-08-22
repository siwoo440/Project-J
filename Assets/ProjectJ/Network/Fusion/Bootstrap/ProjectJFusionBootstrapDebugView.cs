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
            const float width = 820f;
            const float height = 730f;
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
                    420f,
                    34f
                ),
                "Project J - Fusion 60일차",
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
                "연결 Session",
                bootstrap.ConnectedSessionName
            );

            DrawInfoRow(
                left,
                y + 266f,
                contentWidth,
                "참가 인원",
                bootstrap.ParticipantCount +
                " / 8"
            );

            DrawInfoRow(
                left,
                y + 296f,
                contentWidth,
                "공개 여부",
                GetVisibilityText()
            );

            DrawInfoRow(
                left,
                y + 326f,
                contentWidth,
                "Region",
                bootstrap.ConnectedRegion
            );

            DrawInfoRow(
                left,
                y + 356f,
                contentWidth,
                "상태 메시지",
                bootstrap.StatusMessage
            );

            DrawInfoRow(
                left,
                y + 386f,
                contentWidth,
                "마지막 결과",
                bootstrap.LastConnectionResult
            );

            GUI.Label(
                new Rect(
                    left,
                    y + 426f,
                    contentWidth,
                    28f
                ),
                "Network Player · Authority",
                sectionStyle
            );

            DrawInfoRow(
                left,
                y + 460f,
                contentWidth,
                "Spawn 수",
                bootstrap.SpawnedPlayerCount
                    .ToString()
            );

            DrawInfoRow(
                left,
                y + 490f,
                contentWidth,
                "Local PlayerRef",
                GetLocalPlayerText()
            );

            GUI.Label(
                new Rect(
                    left,
                    y + 525f,
                    110f,
                    26f
                ),
                "Player",
                labelStyle
            );

            GUI.Label(
                new Rect(
                    left + 120f,
                    y + 525f,
                    150f,
                    26f
                ),
                "State Authority",
                labelStyle
            );

            GUI.Label(
                new Rect(
                    left + 285f,
                    y + 525f,
                    150f,
                    26f
                ),
                "Input Authority",
                labelStyle
            );

            GUI.Label(
                new Rect(
                    left + 450f,
                    y + 525f,
                    120f,
                    26f
                ),
                "Camera",
                labelStyle
            );

            GUI.Label(
                new Rect(
                    left + 585f,
                    y + 525f,
                    150f,
                    26f
                ),
                "Local Input",
                labelStyle
            );

            DrawPlayerRows(
                left,
                y + 554f
            );

            GUI.enabled =
                bootstrap.CanStart;

            if (
                GUI.Button(
                    new Rect(
                        left,
                        y + 674f,
                        220f,
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
                        left + 240f,
                        y + 674f,
                        220f,
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
                        left + 480f,
                        y + 674f,
                        220f,
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
                if (row >= 4)
                {
                    break;
                }

                float y =
                    startY +
                    row * 28f;

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

                GUI.Label(
                    new Rect(
                        x,
                        y,
                        110f,
                        26f
                    ),
                    "P" +
                    player.AsIndex,
                    labelStyle
                );

                GUI.Label(
                    new Rect(
                        x + 120f,
                        y,
                        150f,
                        26f
                    ),
                    hasObject &&
                    playerObject.HasStateAuthority
                        ? "TRUE"
                        : "FALSE",
                    labelStyle
                );

                GUI.Label(
                    new Rect(
                        x + 285f,
                        y,
                        150f,
                        26f
                    ),
                    hasObject &&
                    playerObject.HasInputAuthority
                        ? "TRUE"
                        : "FALSE",
                    labelStyle
                );

                GUI.Label(
                    new Rect(
                        x + 450f,
                        y,
                        120f,
                        26f
                    ),
                    networkPlayer != null &&
                    networkPlayer
                        .AuthorityCameraEnabled
                        ? "ON"
                        : "OFF",
                    labelStyle
                );

                GUI.Label(
                    new Rect(
                        x + 585f,
                        y,
                        150f,
                        26f
                    ),
                    networkPlayer != null &&
                    networkPlayer
                        .LocalInputSeenRecently
                        ? "DETECTED"
                        : "-",
                    labelStyle
                );

                row++;
            }
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
                    160f,
                    28f
                ),
                label,
                labelStyle
            );

            GUI.Label(
                new Rect(
                    x + 165f,
                    y,
                    width - 165f,
                    28f
                ),
                ": " + value,
                labelStyle
            );
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

        private string GetVisibilityText()
        {
            if (
                !bootstrap.HasValidSessionInfo
            )
            {
                return "-";
            }

            if (!bootstrap.IsSessionOpen)
            {
                return "비공개 / 참가 닫힘";
            }

            return bootstrap.IsSessionVisible
                ? "공개"
                : "비공개";
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
