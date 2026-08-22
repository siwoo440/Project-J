using System; // 문자열 비교와 숫자 변환
using System.Collections.Generic; // Steam 친구 목록
using Fusion; // Host Mode 확인
using ProjectJ.Networking.Fusion; // 기존 Fusion Room Code와 Bootstrap 사용
using UnityEngine; // Runtime Service 생성

#if STEAMWORKS_NET
using Steamworks; // Steamworks.NET 친구 초대 API
#endif

namespace ProjectJ.Steam
{
    public enum ProjectJSteamInviteState
    {
        WaitingForSteam = 0,
        Ready = 1,
        HostRoomReady = 2,
        InviteSent = 3,
        InviteReceived = 4,
        LeavingCurrentRoom = 5,
        Joining = 6,
        Joined = 7,
        InvalidInvite = 8,
        Failed = 9
    }

    public sealed class ProjectJSteamFriendInfo
    {
        public string SteamId64
        {
            get;
            set;
        } =
            string.Empty;

        public string PersonaName
        {
            get;
            set;
        } =
            string.Empty;

        public string PersonaState
        {
            get;
            set;
        } =
            string.Empty;

        public bool IsOnline
        {
            get;
            set;
        }

        public bool IsInGame
        {
            get;
            set;
        }
    }

    [DisallowMultipleComponent]
    public sealed class ProjectJSteamInviteService :
        MonoBehaviour
    {
        private const string ConnectCommand =
            "+projectj_room";

        private const string RichPresenceConnectKey =
            "connect";

        private const string RichPresenceStatusKey =
            "status";

        private const float FriendRefreshInterval =
            2f;

        private const int LaunchCommandBufferSize =
            1024;

        private static ProjectJSteamInviteService instance;

        private readonly List<ProjectJSteamFriendInfo> friends =
            new List<ProjectJSteamFriendInfo>();

        private ProjectJSteamIdentityService steamIdentity;

        private ProjectJFusionBootstrap bootstrap;

        private float nextFriendRefreshTime;

        private string publishedRoomCode =
            string.Empty;

        private string pendingInviteRoomCode =
            string.Empty;

        private string lastAcceptedRoomCode =
            string.Empty;

        private bool leaveRequestedForInvite;

        private bool startupCommandChecked;

#if STEAMWORKS_NET
        private bool callbacksRegistered;

        private Callback<GameRichPresenceJoinRequested_t>
            richPresenceJoinCallback;

        private Callback<NewUrlLaunchParameters_t>
            newUrlLaunchCallback;
#endif

        public static ProjectJSteamInviteService Instance =>
            instance;

        public IReadOnlyList<ProjectJSteamFriendInfo> Friends =>
            friends;

        public ProjectJSteamInviteState State
        {
            get;
            private set;
        } =
            ProjectJSteamInviteState.WaitingForSteam;

        public string StatusMessage
        {
            get;
            private set;
        } =
            "Steam 인증 대기 중";

        public string PendingInviteRoomCode =>
            pendingInviteRoomCode;

        public string LastAcceptedRoomCode =>
            lastAcceptedRoomCode;

        public string PublishedRoomCode =>
            publishedRoomCode;

        public string LastInviteFromSteamId
        {
            get;
            private set;
        } =
            string.Empty;

        public string LastInviteTarget
        {
            get;
            private set;
        } =
            string.Empty;

        public bool CanInvite
        {
            get
            {
                return
                    steamIdentity != null &&
                    steamIdentity.IsAuthenticated &&
                    bootstrap != null &&
                    bootstrap.State ==
                        ProjectJFusionBootstrapState.Running &&
                    bootstrap.ActiveMode ==
                        GameMode.Host &&
                    bootstrap.IsSessionOpen &&
                    TryGetHostRoomCode(
                        out _
                    );
            }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad
        )]
        private static void Install()
        {
            if (instance != null)
            {
                return;
            }

            ProjectJSteamInviteService existing =
                FindFirstObjectByType<
                    ProjectJSteamInviteService
                >();

            if (existing != null)
            {
                instance =
                    existing;

                DontDestroyOnLoad(
                    existing.gameObject
                );

                return;
            }

            GameObject serviceObject =
                new GameObject(
                    "=== Project J Steam Invite ==="
                );

            DontDestroyOnLoad(
                serviceObject
            );

            instance =
                serviceObject.AddComponent<
                    ProjectJSteamInviteService
                >();
        }

        private void Awake()
        {
            if (
                instance != null &&
                instance != this
            )
            {
                Destroy(
                    gameObject
                );

                return;
            }

            instance =
                this;

            DontDestroyOnLoad(
                gameObject
            );
        }

        private void Update()
        {
            FindReferences();

            if (
                steamIdentity == null ||
                !steamIdentity.IsAuthenticated
            )
            {
                State =
                    ProjectJSteamInviteState.WaitingForSteam;

                StatusMessage =
                    steamIdentity == null
                        ? "Steam Identity Service 없음"
                        : "Steam 인증 대기: " +
                            steamIdentity.StatusMessage;

                ClearPublishedRichPresence();
                return;
            }

#if STEAMWORKS_NET
            EnsureCallbacksRegistered();
            CheckStartupLaunchCommand();
            UpdateHostRichPresence();
            RefreshFriendsIfNeeded();
            ProcessPendingInvite();
            UpdateJoinedState();
#else
            State =
                ProjectJSteamInviteState.Failed;

            StatusMessage =
                "STEAMWORKS_NET Define이 없습니다.";
#endif
        }

        private void OnApplicationQuit()
        {
            ClearPublishedRichPresence();
        }

        private void OnDestroy()
        {
            ClearPublishedRichPresence();

#if STEAMWORKS_NET
            if (richPresenceJoinCallback != null)
            {
                richPresenceJoinCallback.Dispose();
                richPresenceJoinCallback =
                    null;
            }

            if (newUrlLaunchCallback != null)
            {
                newUrlLaunchCallback.Dispose();
                newUrlLaunchCallback =
                    null;
            }

            callbacksRegistered =
                false;
#endif

            if (instance == this)
            {
                instance =
                    null;
            }
        }

        public bool TryInviteFriend(
            string steamId64
        )
        {
#if STEAMWORKS_NET
            if (!CanInvite)
            {
                State =
                    ProjectJSteamInviteState.Failed;

                StatusMessage =
                    "Host의 열려 있는 비공개 방에서만 초대할 수 있습니다.";

                return false;
            }

            if (
                !ulong.TryParse(
                    steamId64,
                    out ulong steamIdValue
                ) ||
                steamIdValue == 0UL
            )
            {
                State =
                    ProjectJSteamInviteState.Failed;

                StatusMessage =
                    "유효하지 않은 SteamID입니다.";

                return false;
            }

            if (
                !TryGetHostRoomCode(
                    out string roomCode
                )
            )
            {
                State =
                    ProjectJSteamInviteState.Failed;

                StatusMessage =
                    "Host Room Code를 확인할 수 없습니다.";

                return false;
            }

            string connectString =
                BuildConnectString(
                    roomCode
                );

            CSteamID friendSteamId =
                new CSteamID(
                    steamIdValue
                );

            bool invited =
                SteamFriends.InviteUserToGame(
                    friendSteamId,
                    connectString
                );

            if (!invited)
            {
                State =
                    ProjectJSteamInviteState.Failed;

                StatusMessage =
                    "Steam 친구 초대 전송 실패";

                return false;
            }

            LastInviteTarget =
                SteamFriends.GetFriendPersonaName(
                    friendSteamId
                );

            State =
                ProjectJSteamInviteState.InviteSent;

            StatusMessage =
                "Steam 초대 전송 완료: " +
                LastInviteTarget +
                " / Room " +
                roomCode;

            Debug.Log(
                "[Project J/Steam Invite] " +
                StatusMessage
            );

            return true;
#else
            State =
                ProjectJSteamInviteState.Failed;

            StatusMessage =
                "STEAMWORKS_NET Define이 없습니다.";

            return false;
#endif
        }

        public void RefreshFriends()
        {
#if STEAMWORKS_NET
            friends.Clear();

            if (
                steamIdentity == null ||
                !steamIdentity.IsAuthenticated
            )
            {
                return;
            }

            EFriendFlags friendFlags =
                EFriendFlags.k_EFriendFlagImmediate;

            int friendCount =
                SteamFriends.GetFriendCount(
                    friendFlags
                );

            for (
                int index = 0;
                index < friendCount;
                index++
            )
            {
                CSteamID friendSteamId =
                    SteamFriends.GetFriendByIndex(
                        index,
                        friendFlags
                    );

                EPersonaState personaState =
                    SteamFriends.GetFriendPersonaState(
                        friendSteamId
                    );

                bool isInGame =
                    SteamFriends.GetFriendGamePlayed(
                        friendSteamId,
                        out FriendGameInfo_t _
                    );

                friends.Add(
                    new ProjectJSteamFriendInfo
                    {
                        SteamId64 =
                            friendSteamId
                                .m_SteamID
                                .ToString(),
                        PersonaName =
                            SteamFriends
                                .GetFriendPersonaName(
                                    friendSteamId
                                ),
                        PersonaState =
                            personaState.ToString(),
                        IsOnline =
                            personaState !=
                                EPersonaState
                                    .k_EPersonaStateOffline,
                        IsInGame =
                            isInGame
                    }
                );
            }

            friends.Sort(
                (
                    left,
                    right
                ) =>
                    string.Compare(
                        left.PersonaName,
                        right.PersonaName,
                        StringComparison
                            .OrdinalIgnoreCase
                    )
            );
#endif
        }

        public static string BuildConnectString(
            string roomCode
        )
        {
            if (
                !ProjectJFusionRoomCode.TryNormalize(
                    roomCode,
                    out string normalizedCode,
                    out _
                )
            )
            {
                return string.Empty;
            }

            return
                ConnectCommand +
                " " +
                normalizedCode;
        }

        public static bool TryParseConnectString(
            string connectString,
            out string roomCode
        )
        {
            roomCode =
                string.Empty;

            if (
                string.IsNullOrWhiteSpace(
                    connectString
                )
            )
            {
                return false;
            }

            string cleaned =
                connectString
                    .Replace(
                        "\"",
                        string.Empty
                    )
                    .Trim();

            string[] tokens =
                cleaned.Split(
                    new[]
                    {
                        ' ',
                        '\t',
                        '\r',
                        '\n'
                    },
                    StringSplitOptions
                        .RemoveEmptyEntries
                );

            for (
                int index = 0;
                index < tokens.Length;
                index++
            )
            {
                string token =
                    tokens[index];

                if (
                    token.StartsWith(
                        ConnectCommand + "=",
                        StringComparison
                            .OrdinalIgnoreCase
                    )
                )
                {
                    string candidate =
                        token.Substring(
                            ConnectCommand.Length +
                            1
                        );

                    return
                        ProjectJFusionRoomCode
                            .TryNormalize(
                                candidate,
                                out roomCode,
                                out _
                            );
                }

                if (
                    !string.Equals(
                        token,
                        ConnectCommand,
                        StringComparison
                            .OrdinalIgnoreCase
                    ) ||
                    index + 1 >=
                        tokens.Length
                )
                {
                    continue;
                }

                return
                    ProjectJFusionRoomCode
                        .TryNormalize(
                            tokens[index + 1],
                            out roomCode,
                            out _
                        );
            }

            return false;
        }

        private void FindReferences()
        {
            if (steamIdentity == null)
            {
                steamIdentity =
                    ProjectJSteamIdentityService
                        .Instance;
            }

            if (bootstrap == null)
            {
                bootstrap =
                    FindFirstObjectByType<
                        ProjectJFusionBootstrap
                    >();
            }
        }

#if STEAMWORKS_NET
        private void EnsureCallbacksRegistered()
        {
            if (callbacksRegistered)
            {
                return;
            }

            richPresenceJoinCallback =
                Callback<
                    GameRichPresenceJoinRequested_t
                >.Create(
                    OnRichPresenceJoinRequested
                );

            newUrlLaunchCallback =
                Callback<
                    NewUrlLaunchParameters_t
                >.Create(
                    OnNewUrlLaunchParameters
                );

            callbacksRegistered =
                true;

            State =
                ProjectJSteamInviteState.Ready;

            StatusMessage =
                "Steam 친구 초대 준비 완료";
        }

        private void CheckStartupLaunchCommand()
        {
            if (startupCommandChecked)
            {
                return;
            }

            startupCommandChecked =
                true;

            int length =
                SteamApps.GetLaunchCommandLine(
                    out string launchCommandLine,
                    LaunchCommandBufferSize
                );

            if (
                length <= 0 ||
                string.IsNullOrWhiteSpace(
                    launchCommandLine
                )
            )
            {
                return;
            }

            ReceiveConnectString(
                launchCommandLine,
                0UL
            );
        }

        private void OnNewUrlLaunchParameters(
            NewUrlLaunchParameters_t _
        )
        {
            int length =
                SteamApps.GetLaunchCommandLine(
                    out string launchCommandLine,
                    LaunchCommandBufferSize
                );

            if (
                length <= 0 ||
                string.IsNullOrWhiteSpace(
                    launchCommandLine
                )
            )
            {
                return;
            }

            ReceiveConnectString(
                launchCommandLine,
                0UL
            );
        }

        private void OnRichPresenceJoinRequested(
            GameRichPresenceJoinRequested_t response
        )
        {
            ReceiveConnectString(
                response.m_rgchConnect,
                response.m_steamIDFriend
                    .m_SteamID
            );
        }

        private void ReceiveConnectString(
            string connectString,
            ulong inviterSteamId
        )
        {
            if (
                !TryParseConnectString(
                    connectString,
                    out string roomCode
                )
            )
            {
                State =
                    ProjectJSteamInviteState.InvalidInvite;

                StatusMessage =
                    "잘못된 Steam 초대 문자열: " +
                    connectString;

                Debug.LogWarning(
                    "[Project J/Steam Invite] " +
                    StatusMessage
                );

                return;
            }

            LastInviteFromSteamId =
                inviterSteamId == 0UL
                    ? string.Empty
                    : inviterSteamId.ToString();

            pendingInviteRoomCode =
                roomCode;

            leaveRequestedForInvite =
                false;

            State =
                ProjectJSteamInviteState.InviteReceived;

            StatusMessage =
                "Steam 초대 수신 / Room " +
                roomCode;

            Debug.Log(
                "[Project J/Steam Invite] " +
                StatusMessage
            );
        }

        private void ProcessPendingInvite()
        {
            if (
                string.IsNullOrEmpty(
                    pendingInviteRoomCode
                ) ||
                bootstrap == null
            )
            {
                return;
            }

            if (
                bootstrap.HasValidSessionInfo &&
                string.Equals(
                    bootstrap.ConnectedRoomCode,
                    pendingInviteRoomCode,
                    StringComparison.Ordinal
                )
            )
            {
                lastAcceptedRoomCode =
                    pendingInviteRoomCode;

                pendingInviteRoomCode =
                    string.Empty;

                leaveRequestedForInvite =
                    false;

                State =
                    ProjectJSteamInviteState.Joined;

                StatusMessage =
                    "이미 초대 받은 Room에 참가 중";

                return;
            }

            if (bootstrap.CanStart)
            {
                string roomCode =
                    pendingInviteRoomCode;

                bootstrap.RoomCode =
                    roomCode;

                lastAcceptedRoomCode =
                    roomCode;

                pendingInviteRoomCode =
                    string.Empty;

                leaveRequestedForInvite =
                    false;

                State =
                    ProjectJSteamInviteState.Joining;

                StatusMessage =
                    "Steam 초대 Room 참가 중: " +
                    roomCode;

                bootstrap
                    .RequestJoinPrivateRoom();

                return;
            }

            if (
                bootstrap.CanShutdown &&
                !leaveRequestedForInvite
            )
            {
                leaveRequestedForInvite =
                    true;

                State =
                    ProjectJSteamInviteState
                        .LeavingCurrentRoom;

                StatusMessage =
                    "기존 Fusion Room 종료 후 초대 Room으로 이동";

                bootstrap.RequestLeaveRoom();
            }
        }

        private void UpdateJoinedState()
        {
            if (
                bootstrap == null ||
                string.IsNullOrEmpty(
                    lastAcceptedRoomCode
                ) ||
                bootstrap.State !=
                    ProjectJFusionBootstrapState.Running ||
                !bootstrap.HasValidSessionInfo
            )
            {
                return;
            }

            if (
                !string.Equals(
                    bootstrap.ConnectedRoomCode,
                    lastAcceptedRoomCode,
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }

            if (
                State ==
                    ProjectJSteamInviteState.Joined
            )
            {
                return;
            }

            State =
                ProjectJSteamInviteState.Joined;

            StatusMessage =
                "Steam 초대 Room 참가 완료: " +
                lastAcceptedRoomCode;
        }

        private void UpdateHostRichPresence()
        {
            if (
                TryGetHostRoomCode(
                    out string roomCode
                ) &&
                bootstrap.IsSessionOpen
            )
            {
                if (
                    string.Equals(
                        publishedRoomCode,
                        roomCode,
                        StringComparison.Ordinal
                    )
                )
                {
                    return;
                }

                string connectString =
                    BuildConnectString(
                        roomCode
                    );

                bool connectSet =
                    SteamFriends.SetRichPresence(
                        RichPresenceConnectKey,
                        connectString
                    );

                bool statusSet =
                    SteamFriends.SetRichPresence(
                        RichPresenceStatusKey,
                        "Project J Private Match"
                    );

                if (
                    connectSet &&
                    statusSet
                )
                {
                    publishedRoomCode =
                        roomCode;

                    State =
                        ProjectJSteamInviteState
                            .HostRoomReady;

                    StatusMessage =
                        "Steam Join Game 준비 완료 / Room " +
                        roomCode;
                }

                return;
            }

            ClearPublishedRichPresence();
        }

        private bool TryGetHostRoomCode(
            out string roomCode
        )
        {
            roomCode =
                string.Empty;

            if (
                bootstrap == null ||
                bootstrap.State !=
                    ProjectJFusionBootstrapState.Running ||
                bootstrap.ActiveMode !=
                    GameMode.Host ||
                !bootstrap.HasValidSessionInfo
            )
            {
                return false;
            }

            return
                ProjectJFusionRoomCode
                    .TryNormalize(
                        bootstrap
                            .ConnectedRoomCode,
                        out roomCode,
                        out _
                    );
        }

        private void RefreshFriendsIfNeeded()
        {
            if (
                Time.unscaledTime <
                    nextFriendRefreshTime
            )
            {
                return;
            }

            nextFriendRefreshTime =
                Time.unscaledTime +
                FriendRefreshInterval;

            RefreshFriends();
        }

        private void ClearPublishedRichPresence()
        {
            if (
                string.IsNullOrEmpty(
                    publishedRoomCode
                )
            )
            {
                return;
            }

            if (
                steamIdentity != null &&
                steamIdentity.IsAuthenticated
            )
            {
                SteamFriends.SetRichPresence(
                    RichPresenceConnectKey,
                    string.Empty
                );

                SteamFriends.SetRichPresence(
                    RichPresenceStatusKey,
                    string.Empty
                );
            }

            publishedRoomCode =
                string.Empty;
        }
#else
        private void ClearPublishedRichPresence()
        {
            publishedRoomCode =
                string.Empty;
        }
#endif
    }
}
