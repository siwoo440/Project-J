using Fusion; // NetworkRunner 상태 확인
using UnityEngine; // Debug GUI와 Object 검색
using UnityEngine.InputSystem; // F3 표시 전환

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProjectJFusionBootstrap))]
    public sealed class ProjectJPhase6GateDebugView :
        MonoBehaviour
    {
        private ProjectJFusionBootstrap bootstrap; // 현재 Fusion Bootstrap
        private ProjectJNetworkLobbyFlow lobbyFlow; // Lobby → Match Flow
        private bool isVisible; // F3 표시 상태

        private void Awake()
        {
            ResolveReferences();
            isVisible = false; // 필요할 때 F3으로 표시
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ResolveReferences();

            Keyboard keyboard =
                Keyboard.current;

            if (
                keyboard != null &&
                keyboard.f3Key.wasPressedThisFrame
            )
            {
                isVisible = !isVisible; // Phase 6 Gate 화면 전환
            }
#endif
        }

        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!isVisible)
            {
                return;
            }

            ResolveReferences();

            NetworkRunner[] runners =
                Object.FindObjectsByType<
                    NetworkRunner
                >(
                    FindObjectsSortMode.None
                );

            ProjectJNetworkPlayer[] players =
                Object.FindObjectsByType<
                    ProjectJNetworkPlayer
                >(
                    FindObjectsSortMode.None
                );

            ProjectJNetworkDynamicPlatform[] platforms =
                Object.FindObjectsByType<
                    ProjectJNetworkDynamicPlatform
                >(
                    FindObjectsSortMode.None
                );

            int localPlayerCount = 0;
            int stateAuthorityPlayerCount = 0;
            ProjectJNetworkExternalGameplay match = null;

            for (
                int index = 0;
                index < players.Length;
                index++
            )
            {
                ProjectJNetworkPlayer player =
                    players[index];

                if (player == null)
                {
                    continue;
                }

                if (player.HasLocalInputAuthority)
                {
                    localPlayerCount++;
                }

                if (player.HasLocalStateAuthority)
                {
                    stateAuthorityPlayerCount++;
                }

                if (match == null)
                {
                    match =
                        player.GetComponent<
                            ProjectJNetworkExternalGameplay
                        >();
                }
            }

            int passengerCarryCount = 0;
            int lastPassengerCount = 0;

            for (
                int index = 0;
                index < platforms.Length;
                index++
            )
            {
                ProjectJNetworkDynamicPlatform platform =
                    platforms[index];

                if (platform == null)
                {
                    continue;
                }

                passengerCarryCount +=
                    platform.PassengerCarryCount;

                lastPassengerCount +=
                    platform.LastPassengerCount;
            }

            int participantCount =
                bootstrap != null
                    ? bootstrap.ParticipantCount
                    : 0;

            int spawnedPlayerCount =
                bootstrap != null
                    ? bootstrap.SpawnedPlayerCount
                    : 0;

            bool runnerCountOk =
                runners.Length == 1;

            bool playerCountOk =
                participantCount == 0 ||
                players.Length == participantCount;

            bool localPlayerOk =
                participantCount == 0 ||
                localPlayerCount == 1;

            Rect panel =
                new Rect(
                    420f,
                    12f,
                    470f,
                    330f
                );

            GUI.Box(
                panel,
                string.Empty
            );

            float x = panel.x + 14f;
            float y = panel.y + 12f;
            float width = panel.width - 28f;

            DrawLine(
                x,
                ref y,
                width,
                "DAY 75 - PHASE 6 GATE"
            );

            DrawLine(
                x,
                ref y,
                width,
                "F3 : 표시 / 숨김"
            );

            DrawCheck(
                x,
                ref y,
                width,
                "NetworkRunner 1개",
                runnerCountOk,
                runners.Length.ToString()
            );

            DrawCheck(
                x,
                ref y,
                width,
                "PlayerObject = 참가 인원",
                playerCountOk,
                players.Length +
                " / " +
                participantCount
            );

            DrawCheck(
                x,
                ref y,
                width,
                "Local InputAuthority 1명",
                localPlayerOk,
                localPlayerCount.ToString()
            );

            DrawLine(
                x,
                ref y,
                width,
                "Spawned : " +
                spawnedPlayerCount +
                " / StateAuthority Player : " +
                stateAuthorityPlayerCount
            );

            DrawLine(
                x,
                ref y,
                width,
                "Lobby Flow : " +
                (
                    lobbyFlow != null
                        ? lobbyFlow.Phase.ToString()
                        : "-"
                )
            );

            DrawLine(
                x,
                ref y,
                width,
                "Match : " +
                (
                    match != null
                        ? match.MatchState.ToString()
                        : "-"
                )
            );

            DrawLine(
                x,
                ref y,
                width,
                "Dynamic Platform : " +
                platforms.Length
            );

            DrawLine(
                x,
                ref y,
                width,
                "Platform Carry 누적 / 현재 : " +
                passengerCarryCount +
                " / " +
                lastPassengerCount
            );

            DrawLine(
                x,
                ref y,
                width,
                "최종 Gate : Console Error 0 + 2PC 전체 경기"
            );
#endif
        }

        private void ResolveReferences()
        {
            if (bootstrap == null)
            {
                bootstrap =
                    GetComponent<
                        ProjectJFusionBootstrap
                    >();
            }

            if (lobbyFlow == null)
            {
                lobbyFlow =
                    GetComponent<
                        ProjectJNetworkLobbyFlow
                    >();
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
                    22f
                ),
                text
            );

            y += 25f;
        }

        private static void DrawCheck(
            float x,
            ref float y,
            float width,
            string label,
            bool passed,
            string value
        )
        {
            DrawLine(
                x,
                ref y,
                width,
                (
                    passed
                        ? "[OK] "
                        : "[CHECK] "
                ) +
                label +
                " : " +
                value
            );
        }
    }
}
