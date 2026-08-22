using Fusion; // NetworkRunner와 PlayerRef
using ProjectJ.Steam; // Steam Identity
using UnityEngine; // Runtime Debug GUI
using UnityEngine.InputSystem; // F7 입력

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJDay80SteamIdentityDebugView :
        MonoBehaviour
    {
        private ProjectJFusionBootstrap bootstrap;

        private ProjectJSteamIdentityService steamIdentity;

        private ProjectJDay79NetworkConditionDebugView
            day79DebugView;

        private bool visible =
            false; // 81일차부터 기본 화면은 F8 Steam Invite 사용

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void Install()
        {
            ProjectJDay80SteamIdentityDebugView existing =
                FindFirstObjectByType<
                    ProjectJDay80SteamIdentityDebugView
                >();

            if (existing != null)
            {
                return;
            }

            GameObject debugObject =
                new GameObject(
                    "=== Project J Day80 Steam Identity Debug ==="
                );

            DontDestroyOnLoad(
                debugObject
            );

            debugObject.AddComponent<
                ProjectJDay80SteamIdentityDebugView
            >();
        }

        private void Start()
        {
            FindReferences();
            SetDay79Suppressed(
                visible
            );
        }

        private void Update()
        {
            Keyboard keyboard =
                Keyboard.current;

            if (
                keyboard != null &&
                keyboard.f7Key.wasPressedThisFrame
            )
            {
                visible =
                    !visible;

                SetDay79Suppressed(
                    visible
                );
            }

            FindReferences();
        }

        private void OnDisable()
        {
            SetDay79Suppressed(
                false
            );
        }

        private void OnGUI()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return;
#else
            if (!visible)
            {
                return;
            }

            float width =
                Mathf.Min(
                    Screen.width - 24f,
                    760f
                );

            float height =
                245f;

            if (
                bootstrap != null &&
                bootstrap.Runner != null &&
                bootstrap.Runner.IsRunning &&
                bootstrap.Runner.IsServer
            )
            {
                height +=
                    bootstrap.ParticipantCount *
                    27f;
            }

            GUI.Box(
                new Rect(
                    12f,
                    12f,
                    width,
                    Mathf.Min(
                        height,
                        Screen.height - 24f
                    )
                ),
                string.Empty
            );

            float y =
                20f;

            DrawLine(
                ref y,
                width,
                "DAY 80 - STEAM IDENTITY GATE / F7 Toggle"
            );

            if (steamIdentity == null)
            {
                DrawLine(
                    ref y,
                    width,
                    "Steam Identity Service : 없음"
                );

                return;
            }

            DrawLine(
                ref y,
                width,
                "Steam State : " +
                steamIdentity.State +
                " / " +
                steamIdentity.StatusMessage
            );

            DrawLine(
                ref y,
                width,
                "SteamID64 : " +
                ValueOrDash(
                    steamIdentity.SteamId64
                )
            );

            DrawLine(
                ref y,
                width,
                "Persona : " +
                ValueOrDash(
                    steamIdentity.PersonaName
                )
            );

            DrawLine(
                ref y,
                width,
                "Project Account ID : " +
                ValueOrDash(
                    steamIdentity.ProjectAccountId
                )
            );

            DrawLine(
                ref y,
                width,
                "Web API Ticket : " +
                (
                    steamIdentity.WebApiTicketReady
                        ? "READY / " +
                            steamIdentity
                                .WebApiTicketByteLength +
                            " bytes"
                        : "WAIT"
                )
            );

            if (
                GUI.Button(
                    new Rect(
                        22f,
                        y,
                        190f,
                        28f
                    ),
                    "Steam 인증 다시 시도"
                )
            )
            {
                steamIdentity.TryInitialize();
            }

            y +=
                36f;

            if (
                bootstrap == null ||
                bootstrap.Runner == null ||
                !bootstrap.Runner.IsRunning
            )
            {
                DrawLine(
                    ref y,
                    width,
                    "Fusion : 연결 전 / Steam Authenticated 이후 Host 또는 Client 시작"
                );

                return;
            }

            NetworkRunner runner =
                bootstrap.Runner;

            DrawLine(
                ref y,
                width,
                "Fusion : " +
                bootstrap.State +
                " / Local PlayerRef: " +
                runner.LocalPlayer.AsIndex
            );

            if (!runner.IsServer)
            {
                DrawLine(
                    ref y,
                    width,
                    "Client Project Account ID : " +
                    ValueOrDash(
                        steamIdentity.ProjectAccountId
                    )
                );

                return;
            }

            DrawLine(
                ref y,
                width,
                "HOST ACCOUNT MAP / 중복 Project Account ID는 Spawn 전에 연결 종료"
            );

            foreach (
                PlayerRef player
                in runner.ActivePlayers
            )
            {
                string userId =
                    runner.GetPlayerUserId(
                        player
                    );

                DrawLine(
                    ref y,
                    width,
                    "P" +
                    player.AsIndex +
                    " -> " +
                    ValueOrDash(
                        userId
                    )
                );
            }
#endif
        }

        private void FindReferences()
        {
            if (bootstrap == null)
            {
                bootstrap =
                    FindFirstObjectByType<
                        ProjectJFusionBootstrap
                    >();
            }

            if (steamIdentity == null)
            {
                steamIdentity =
                    ProjectJSteamIdentityService
                        .Instance;
            }

            if (day79DebugView == null)
            {
                day79DebugView =
                    FindFirstObjectByType<
                        ProjectJDay79NetworkConditionDebugView
                    >();
            }
        }

        private void SetDay79Suppressed(
            bool suppress
        )
        {
            if (day79DebugView == null)
            {
                day79DebugView =
                    FindFirstObjectByType<
                        ProjectJDay79NetworkConditionDebugView
                    >();
            }

            if (day79DebugView != null)
            {
                day79DebugView.enabled =
                    !suppress;
            }
        }

        private static string ValueOrDash(
            string value
        )
        {
            return string.IsNullOrEmpty(
                value
            )
                ? "-"
                : value;
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
                    width - 30f,
                    23f
                ),
                text
            );

            y +=
                27f;
        }
    }
}
