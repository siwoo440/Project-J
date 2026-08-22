using ProjectJ.Steam; // Steam 상태와 재시도
using UnityEngine; // Runtime Debug GUI
using UnityEngine.InputSystem; // F12 입력
using UnityEngine.SceneManagement; // 현재 Scene 표시

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(ProjectJNetworkConnectionRecovery)
    )]
    public sealed class ProjectJDay83ConnectionRecoveryDebugView :
        MonoBehaviour
    {
        private ProjectJNetworkConnectionRecovery recovery;

        private ProjectJSteamIdentityService steamIdentity;

        private bool visible;

        private void Awake()
        {
            recovery =
                GetComponent<
                    ProjectJNetworkConnectionRecovery
                >();

            steamIdentity =
                ProjectJSteamIdentityService
                    .Instance;
        }

        private void Update()
        {
            Keyboard keyboard =
                Keyboard.current;

            if (
                keyboard != null &&
                keyboard.f12Key
                    .wasPressedThisFrame
            )
            {
                visible =
                    !visible;
            }

            if (recovery == null)
            {
                recovery =
                    GetComponent<
                        ProjectJNetworkConnectionRecovery
                    >();
            }

            if (steamIdentity == null)
            {
                steamIdentity =
                    ProjectJSteamIdentityService
                        .Instance;
            }
        }

        private void OnGUI()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return;
#else
            if (
                recovery == null ||
                (
                    !visible &&
                    !recovery.HasError &&
                    recovery.State !=
                        ProjectJConnectionRecoveryState
                            .AutoRetryWaiting
                )
            )
            {
                return;
            }

            float panelWidth =
                Mathf.Min(
                    470f,
                    Screen.width - 24f
                );

            float panelHeight =
                330f;

            Rect panel =
                new Rect(
                    Mathf.Max(
                        12f,
                        Screen.width -
                            panelWidth -
                            12f
                    ),
                    Mathf.Max(
                        12f,
                        Screen.height -
                            panelHeight -
                            12f
                    ),
                    panelWidth,
                    panelHeight
                );

            GUI.Box(
                panel,
                string.Empty
            );

            float x =
                panel.x + 14f;

            float y =
                panel.y + 12f;

            float width =
                panel.width - 28f;

            DrawLine(
                x,
                ref y,
                width,
                "DAY 83 - CONNECTION RECOVERY / F12 Toggle"
            );

            DrawLine(
                x,
                ref y,
                width,
                "Scene : " +
                SceneManager.GetActiveScene().name
            );

            DrawLine(
                x,
                ref y,
                width,
                "State : " +
                recovery.State +
                " / Error : " +
                recovery.LastError
            );

            DrawLine(
                x,
                ref y,
                width,
                "Status : " +
                recovery.StatusMessage
            );

            DrawLine(
                x,
                ref y,
                width,
                "Last Room : " +
                ValueOrDash(
                    recovery.LastRoomCode
                ) +
                " / Mode : " +
                recovery.LastModeText
            );

            DrawLine(
                x,
                ref y,
                width,
                "Retry : AUTO " +
                recovery.AutoRetryCount +
                "/1 / MANUAL " +
                recovery.ManualRetryCount
            );

            if (
                !string.IsNullOrEmpty(
                    recovery.ErrorDetail
                )
            )
            {
                DrawLine(
                    x,
                    ref y,
                    width,
                    "Detail : " +
                    recovery.ErrorDetail
                );
            }

            float buttonWidth =
                (width - 10f) /
                2f;

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                recovery.CanReconnect;

            if (
                GUI.Button(
                    new Rect(
                        x,
                        y,
                        buttonWidth,
                        32f
                    ),
                    "RECONNECT"
                )
            )
            {
                recovery.RequestReconnect();
            }

            GUI.enabled =
                previousEnabled;

            if (
                GUI.Button(
                    new Rect(
                        x +
                            buttonWidth +
                            10f,
                        y,
                        buttonWidth,
                        32f
                    ),
                    "MAIN MENU"
                )
            )
            {
                recovery.RequestMainMenu();
            }

            y +=
                40f;

            bool steamRetryEnabled =
                steamIdentity != null &&
                !steamIdentity.IsAuthenticated;

            GUI.enabled =
                steamRetryEnabled;

            if (
                GUI.Button(
                    new Rect(
                        x,
                        y,
                        buttonWidth,
                        32f
                    ),
                    "RETRY STEAM"
                )
            )
            {
                recovery.RequestSteamRetry();
            }

            GUI.enabled =
                recovery.HasError;

            if (
                GUI.Button(
                    new Rect(
                        x +
                            buttonWidth +
                            10f,
                        y,
                        buttonWidth,
                        32f
                    ),
                    "CLEAR ERROR"
                )
            )
            {
                recovery.ClearError();
            }

            GUI.enabled =
                previousEnabled;
#endif
        }

        private static string ValueOrDash(
            string value
        )
        {
            return
                string.IsNullOrEmpty(
                    value
                )
                    ? "-"
                    : value;
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
                    23f
                ),
                text
            );

            y +=
                27f;
        }
    }
}
