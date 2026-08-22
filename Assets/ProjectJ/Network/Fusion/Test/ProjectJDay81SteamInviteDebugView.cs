using ProjectJ.Steam; // Steam Invite Service 사용
using UnityEngine; // Runtime Debug GUI
using UnityEngine.InputSystem; // F8 입력

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJDay81SteamInviteDebugView :
        MonoBehaviour
    {
        private const int MaxVisibleFriends =
            10;

        private ProjectJFusionBootstrap bootstrap;

        private ProjectJSteamIdentityService steamIdentity;

        private ProjectJSteamInviteService inviteService;

        private bool visible =
            true;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void Install()
        {
            ProjectJDay81SteamInviteDebugView existing =
                FindFirstObjectByType<
                    ProjectJDay81SteamInviteDebugView
                >();

            if (existing != null)
            {
                return;
            }

            GameObject debugObject =
                new GameObject(
                    "=== Project J Day81 Steam Invite Debug ==="
                );

            DontDestroyOnLoad(
                debugObject
            );

            debugObject.AddComponent<
                ProjectJDay81SteamInviteDebugView
            >();
        }

        private void Update()
        {
            Keyboard keyboard =
                Keyboard.current;

            if (
                keyboard != null &&
                keyboard.f8Key.wasPressedThisFrame
            )
            {
                visible =
                    !visible;
            }

            FindReferences();
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
                    860f
                );

            int friendCount =
                inviteService == null
                    ? 0
                    : inviteService.Friends.Count;

            int visibleFriendCount =
                Mathf.Min(
                    friendCount,
                    MaxVisibleFriends
                );

            float height =
                255f +
                visibleFriendCount *
                32f;

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
                "DAY 81 - STEAM FRIEND INVITE / F8 Toggle"
            );

            if (
                steamIdentity == null ||
                inviteService == null
            )
            {
                DrawLine(
                    ref y,
                    width,
                    "Steam Service 준비 중"
                );

                return;
            }

            DrawLine(
                ref y,
                width,
                "Steam Auth : " +
                steamIdentity.State +
                " / Persona : " +
                ValueOrDash(
                    steamIdentity.PersonaName
                )
            );

            DrawLine(
                ref y,
                width,
                "Invite State : " +
                inviteService.State +
                " / " +
                inviteService.StatusMessage
            );

            DrawLine(
                ref y,
                width,
                "Fusion : " +
                GetFusionText()
            );

            DrawLine(
                ref y,
                width,
                "Published Room : " +
                ValueOrDash(
                    inviteService
                        .PublishedRoomCode
                ) +
                " / Pending Invite : " +
                ValueOrDash(
                    inviteService
                        .PendingInviteRoomCode
                )
            );

            DrawLine(
                ref y,
                width,
                "Last Accepted Room : " +
                ValueOrDash(
                    inviteService
                        .LastAcceptedRoomCode
                ) +
                " / Last Invite From : " +
                ValueOrDash(
                    inviteService
                        .LastInviteFromSteamId
                )
            );

            if (
                GUI.Button(
                    new Rect(
                        22f,
                        y,
                        170f,
                        28f
                    ),
                    "친구 목록 새로고침"
                )
            )
            {
                inviteService.RefreshFriends();
            }

            y +=
                36f;

            DrawLine(
                ref y,
                width,
                "STEAM FRIENDS : " +
                friendCount +
                " / Host의 OPEN Room에서 ONLINE 친구에게 INVITE"
            );

            for (
                int index = 0;
                index < visibleFriendCount;
                index++
            )
            {
                ProjectJSteamFriendInfo friend =
                    inviteService.Friends[index];

                string friendText =
                    friend.PersonaName +
                    " / " +
                    friend.PersonaState +
                    (
                        friend.IsInGame
                            ? " / IN GAME"
                            : string.Empty
                    ) +
                    " / " +
                    friend.SteamId64;

                GUI.Label(
                    new Rect(
                        22f,
                        y,
                        width - 150f,
                        26f
                    ),
                    friendText
                );

                bool previousEnabled =
                    GUI.enabled;

                GUI.enabled =
                    inviteService.CanInvite &&
                    friend.IsOnline;

                if (
                    GUI.Button(
                        new Rect(
                            width - 112f,
                            y,
                            90f,
                            26f
                        ),
                        "INVITE"
                    )
                )
                {
                    inviteService.TryInviteFriend(
                        friend.SteamId64
                    );
                }

                GUI.enabled =
                    previousEnabled;

                y +=
                    32f;
            }

            if (
                friendCount >
                MaxVisibleFriends
            )
            {
                DrawLine(
                    ref y,
                    width,
                    "친구가 많아 앞의 " +
                    MaxVisibleFriends +
                    "명만 표시합니다."
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

            if (inviteService == null)
            {
                inviteService =
                    ProjectJSteamInviteService
                        .Instance;
            }
        }

        private string GetFusionText()
        {
            if (bootstrap == null)
            {
                return "Bootstrap 없음";
            }

            if (
                bootstrap.Runner == null ||
                !bootstrap.Runner.IsRunning
            )
            {
                return
                    bootstrap.State +
                    " / Room -";
            }

            return
                bootstrap.State +
                " / " +
                (
                    bootstrap.ActiveMode
                        .HasValue
                        ? bootstrap.ActiveMode
                            .Value
                            .ToString()
                        : "-"
                ) +
                " / Room " +
                bootstrap.ConnectedRoomCode +
                " / " +
                (
                    bootstrap.IsSessionOpen
                        ? "OPEN"
                        : "CLOSED"
                );
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
