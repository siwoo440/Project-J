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
            const float width = 470f;
            const float height = 360f;

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
                    x + 16f,
                    y + 12f,
                    width - 32f,
                    28f
                ),
                "Project J - Fusion 59일차",
                titleStyle
            );

            GUI.Label(
                new Rect(
                    x + 16f,
                    y + 48f,
                    95f,
                    26f
                ),
                "세션 이름",
                labelStyle
            );

            GUI.enabled =
                bootstrap.CanStart;

            bootstrap.SessionName =
                GUI.TextField(
                    new Rect(
                        x + 115f,
                        y + 48f,
                        335f,
                        26f
                    ),
                    bootstrap.SessionName
                );

            GUI.enabled = true;

            GUI.Label(
                new Rect(
                    x + 16f,
                    y + 82f,
                    width - 32f,
                    24f
                ),
                "상태 : " +
                GetStateText(
                    bootstrap.State
                ),
                labelStyle
            );

            GUI.Label(
                new Rect(
                    x + 16f,
                    y + 108f,
                    width - 32f,
                    24f
                ),
                "역할 : " +
                GetModeText(
                    bootstrap.ActiveMode
                ),
                labelStyle
            );

            GUI.Label(
                new Rect(
                    x + 16f,
                    y + 134f,
                    width - 32f,
                    24f
                ),
                "연결 세션 : " +
                bootstrap.ConnectedSessionName,
                labelStyle
            );

            GUI.Label(
                new Rect(
                    x + 16f,
                    y + 160f,
                    width - 32f,
                    24f
                ),
                "참가 인원 : " +
                bootstrap.ParticipantCount +
                " / 8",
                labelStyle
            );

            GUI.Label(
                new Rect(
                    x + 16f,
                    y + 186f,
                    width - 32f,
                    24f
                ),
                "공개 여부 : " +
                GetVisibilityText(),
                labelStyle
            );

            GUI.Label(
                new Rect(
                    x + 16f,
                    y + 212f,
                    width - 32f,
                    24f
                ),
                "Region : " +
                bootstrap.ConnectedRegion,
                labelStyle
            );

            GUI.Label(
                new Rect(
                    x + 16f,
                    y + 238f,
                    width - 32f,
                    24f
                ),
                bootstrap.StatusMessage,
                labelStyle
            );

            GUI.Label(
                new Rect(
                    x + 16f,
                    y + 264f,
                    width - 32f,
                    24f
                ),
                "마지막 결과 : " +
                bootstrap.LastConnectionResult,
                labelStyle
            );

            GUI.enabled =
                bootstrap.CanStart;

            if (
                GUI.Button(
                    new Rect(
                        x + 16f,
                        y + 296f,
                        135f,
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
                        x + 161f,
                        y + 296f,
                        135f,
                        42f
                    ),
                    "방 참가",
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
                        x + 306f,
                        y + 296f,
                        144f,
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

            GUI.Label(
                new Rect(
                    x + 16f,
                    y + 340f,
                    width - 32f,
                    20f
                ),
                "F2 : 창 표시/숨김 | ALT : 커서 활성화",
                labelStyle
            );
#endif
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

                titleStyle.fontSize = 20;
                titleStyle.fontStyle =
                    FontStyle.Bold;
            }

            if (labelStyle == null)
            {
                labelStyle =
                    new GUIStyle(
                        GUI.skin.label
                    );

                labelStyle.fontSize = 15;
            }

            if (buttonStyle == null)
            {
                buttonStyle =
                    new GUIStyle(
                        GUI.skin.button
                    );

                buttonStyle.fontSize = 14;
                buttonStyle.fontStyle =
                    FontStyle.Bold;
            }
        }
    }
}
