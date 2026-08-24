using Fusion; // Host / Client 상태 표시
using UnityEngine; // Runtime Debug GUI
using UnityEngine.InputSystem; // F9 입력
using UnityEngine.SceneManagement; // 현재 Scene 표시

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJDay82SceneFlowDebugView :
        MonoBehaviour
    {
        private ProjectJDay82SceneFlowCoordinator
            coordinator;

        private string roomCodeInput =
            string.Empty;

        private bool visible =
            false; // 통합 패널 기본 숨김

        private void Awake()
        {
            coordinator =
                GetComponent<
                    ProjectJDay82SceneFlowCoordinator
                >();
        }

        private void Update()
        {
            Keyboard keyboard =
                Keyboard.current;

            if (
                keyboard != null &&
                keyboard.f9Key
                    .wasPressedThisFrame
            )
            {
                visible =
                    !visible;
            }

            if (coordinator == null)
            {
                coordinator =
                    GetComponent<
                        ProjectJDay82SceneFlowCoordinator
                    >();
            }
        }

        private void OnGUI()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return;
#else
            if (
                !visible ||
                coordinator == null
            )
            {
                return;
            }

            float panelWidth =
                Mathf.Min(
                    430f,
                    Screen.width - 24f
                );

            float panelHeight =
                350f;

            Rect panel =
                new Rect(
                    Mathf.Max(
                        12f,
                        Screen.width -
                            panelWidth -
                            12f
                    ),
                    12f,
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
                "DAY 82 - FULL SCENE FLOW / F9 Toggle"
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
                "Flow : " +
                coordinator.State
            );

            DrawLine(
                x,
                ref y,
                width,
                coordinator.StatusText
            );

            ProjectJFusionBootstrap bootstrap =
                coordinator.Bootstrap;

            ProjectJNetworkLobbyFlow lobbyFlow =
                coordinator.LobbyFlow;

            if (bootstrap != null)
            {
                DrawLine(
                    x,
                    ref y,
                    width,
                    "Fusion : " +
                    bootstrap.State +
                    " / " +
                    (
                        bootstrap.ActiveMode.HasValue
                            ? bootstrap
                                .ActiveMode
                                .Value
                                .ToString()
                            : "-"
                    )
                );

                DrawLine(
                    x,
                    ref y,
                    width,
                    "Room : " +
                    bootstrap.ConnectedRoomCode +
                    " / Players : " +
                    bootstrap.ParticipantCount
                );
            }

            if (lobbyFlow != null)
            {
                DrawLine(
                    x,
                    ref y,
                    width,
                    "Match Flow : " +
                    lobbyFlow.Phase +
                    " / Ready " +
                    lobbyFlow.ReadyPlayerCount +
                    "/" +
                    lobbyFlow.ParticipantCount
                );
            }

            string sceneName =
                SceneManager.GetActiveScene().name;

            if (sceneName == "MainMenu")
            {
                DrawMainMenuControls(
                    x,
                    ref y,
                    width
                );

                return;
            }

            if (sceneName == "Lobby")
            {
                DrawLobbyControls(
                    x,
                    ref y,
                    width
                );

                return;
            }

            if (sceneName == "Game")
            {
                DrawGameControls(
                    x,
                    ref y,
                    width
                );
            }
#endif
        }

        private void DrawMainMenuControls(
            float x,
            ref float y,
            float width
        )
        {
            if (
                GUI.Button(
                    new Rect(
                        x,
                        y,
                        width,
                        32f
                    ),
                    "HOST - PRIVATE ROOM CREATE"
                )
            )
            {
                coordinator
                    .RequestCreatePrivateRoom();
            }

            y +=
                40f;

            GUI.Label(
                new Rect(
                    x,
                    y,
                    85f,
                    28f
                ),
                "ROOM CODE"
            );

            roomCodeInput =
                GUI.TextField(
                    new Rect(
                        x + 90f,
                        y,
                        150f,
                        28f
                    ),
                    roomCodeInput,
                    ProjectJFusionRoomCode.Length
                );

            if (
                GUI.Button(
                    new Rect(
                        x + 250f,
                        y,
                        width - 250f,
                        28f
                    ),
                    "JOIN"
                )
            )
            {
                coordinator
                    .RequestJoinPrivateRoom(
                        roomCodeInput
                    );
            }

            y +=
                38f;

            if (
                GUI.Button(
                    new Rect(
                        x,
                        y,
                        width,
                        28f
                    ),
                    "QUIT"
                )
            )
            {
                coordinator.RequestQuit();
            }
        }

        private void DrawLobbyControls(
            float x,
            ref float y,
            float width
        )
        {
            DrawLine(
                x,
                ref y,
                width,
                "R : READY / NOT READY"
            );

            if (
                GUI.Button(
                    new Rect(
                        x,
                        y,
                        width,
                        32f
                    ),
                    "LEAVE TO MAIN MENU"
                )
            )
            {
                coordinator
                    .RequestLeaveToMainMenu();
            }
        }

        private void DrawGameControls(
            float x,
            ref float y,
            float width
        )
        {
            ProjectJNetworkLobbyFlow lobbyFlow =
                coordinator.LobbyFlow;

            ProjectJFusionBootstrap bootstrap =
                coordinator.Bootstrap;

            if (
                lobbyFlow != null &&
                bootstrap != null &&
                bootstrap.Runner != null &&
                bootstrap.Runner.IsRunning &&
                bootstrap.Runner.IsSceneAuthority &&
                lobbyFlow.CanReturnToLobby
            )
            {
                if (
                    GUI.Button(
                        new Rect(
                            x,
                            y,
                            width,
                            32f
                        ),
                        "RETURN TO LOBBY"
                    )
                )
                {
                    coordinator
                        .RequestReturnToLobby();
                }

                y +=
                    40f;
            }

            if (
                GUI.Button(
                    new Rect(
                        x,
                        y,
                        width,
                        32f
                    ),
                    "LEAVE TO MAIN MENU"
                )
            )
            {
                coordinator
                    .RequestLeaveToMainMenu();
            }
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
