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
            const float width = 430f;
            const float height = 250f;

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
                "Project J - Fusion 58일차",
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

            bool canEditSession =
                bootstrap.CanStart;

            GUI.enabled =
                canEditSession;

            bootstrap.SessionName =
                GUI.TextField(
                    new Rect(
                        x + 115f,
                        y + 48f,
                        295f,
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
                    26f
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
                    y + 110f,
                    width - 32f,
                    26f
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
                    y + 138f,
                    width - 32f,
                    26f
                ),
                bootstrap.StatusMessage,
                labelStyle
            );

            GUI.enabled =
                bootstrap.CanStart;

            if (
                GUI.Button(
                    new Rect(
                        x + 16f,
                        y + 180f,
                        120f,
                        42f
                    ),
                    "Host 시작",
                    buttonStyle
                )
            )
            {
                bootstrap
                    .RequestStartHost();
            }

            if (
                GUI.Button(
                    new Rect(
                        x + 146f,
                        y + 180f,
                        120f,
                        42f
                    ),
                    "Client 접속",
                    buttonStyle
                )
            )
            {
                bootstrap
                    .RequestStartClient();
            }

            GUI.enabled =
                bootstrap.CanShutdown;

            if (
                GUI.Button(
                    new Rect(
                        x + 276f,
                        y + 180f,
                        134f,
                        42f
                    ),
                    "Runner 종료",
                    buttonStyle
                )
            )
            {
                bootstrap
                    .RequestShutdown();
            }

            GUI.enabled = true;

            GUI.Label(
                new Rect(
                    x + 16f,
                    y + 226f,
                    width - 32f,
                    20f
                ),
                "F2 : 네트워크 테스트 창 표시/숨김",
                labelStyle
            );
#endif
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
                    return "시작 중";

                case
                    ProjectJFusionBootstrapState
                        .Running:
                    return "실행 중";

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

                buttonStyle.fontSize = 15;
                buttonStyle.fontStyle =
                    FontStyle.Bold;
            }
        }
    }
}
