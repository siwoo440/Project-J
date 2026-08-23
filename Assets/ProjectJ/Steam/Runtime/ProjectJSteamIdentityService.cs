using System; // 예외와 문자열 처리
using UnityEngine; // Runtime 서비스 생성

#if STEAMWORKS_NET
using Steamworks; // Steamworks.NET API
#endif

namespace ProjectJ.Steam
{
    public enum ProjectJSteamAuthState
    {
        Uninitialized = 0,
        Initializing = 1,
        WaitingForWebApiTicket = 2,
        Authenticated = 3,
        SteamUnavailable = 4,
        LoginRequired = 5,
        TicketFailed = 6,
        PackageMissing = 7
    }

    [DisallowMultipleComponent]
    public sealed class ProjectJSteamIdentityService :
        MonoBehaviour
    {
        private const string WebApiTicketIdentity =
            "projectj-fusion-auth-v1";

        private static ProjectJSteamIdentityService instance;

        public static ProjectJSteamIdentityService Instance =>
            instance;

        public ProjectJSteamAuthState State
        {
            get;
            private set;
        } =
            ProjectJSteamAuthState.Uninitialized;

        public string SteamId64
        {
            get;
            private set;
        } =
            string.Empty;

        public string PersonaName
        {
            get;
            private set;
        } =
            string.Empty;

        public string ProjectAccountId
        {
            get;
            private set;
        } =
            string.Empty;

        public bool WebApiTicketReady =>
            !string.IsNullOrEmpty(
                webApiTicketHex
            );

        public int WebApiTicketByteLength
        {
            get;
            private set;
        }

        public string StatusMessage
        {
            get;
            private set;
        } =
            "Steam 초기화 전";

        public bool IsSteamInitialized
        {
            get
            {
#if STEAMWORKS_NET
                return steamInitialized; // 실제 SteamAPI 초기화 상태
#else
                return false; // Steamworks 미포함 상태
#endif
            }
        }

        public bool IsAuthenticated =>
            IsSteamInitialized &&
            State ==
                ProjectJSteamAuthState.Authenticated &&
            !string.IsNullOrEmpty(
                ProjectAccountId
            ) &&
            WebApiTicketReady;

        private string webApiTicketHex =
            string.Empty;

#if STEAMWORKS_NET
        private bool steamInitialized;

        private HAuthTicket webApiTicketHandle =
            HAuthTicket.Invalid;

        private Callback<GetTicketForWebApiResponse_t>
            webApiTicketCallback;
#endif

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad
        )]
        private static void Install()
        {
            if (instance != null)
            {
                return;
            }

            ProjectJSteamIdentityService existing =
                FindFirstObjectByType<
                    ProjectJSteamIdentityService
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
                    "=== Project J Steam Identity ==="
                );

            DontDestroyOnLoad(
                serviceObject
            );

            instance =
                serviceObject.AddComponent<
                    ProjectJSteamIdentityService
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

            TryInitialize();
        }

        private void Update()
        {
#if STEAMWORKS_NET
            if (steamInitialized)
            {
                SteamAPI.RunCallbacks();
            }
#endif
        }

        private void OnApplicationQuit()
        {
            ShutdownSteam();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                ShutdownSteam();

                instance =
                    null;
            }
        }

        public void TryInitialize()
        {
            ResetIdentity();

#if STEAMWORKS_NET
            State =
                ProjectJSteamAuthState.Initializing;

            StatusMessage =
                "Steam 초기화 중";

            try
            {
                if (!SteamAPI.IsSteamRunning())
                {
                    State =
                        ProjectJSteamAuthState.SteamUnavailable;

                    StatusMessage =
                        "Steam Client가 실행 중이 아닙니다.";

                    return;
                }

                steamInitialized =
                    SteamAPI.Init();

                if (!steamInitialized)
                {
                    State =
                        ProjectJSteamAuthState.SteamUnavailable;

                    StatusMessage =
                        "SteamAPI.Init 실패 / steam_appid.txt와 Steam 실행 상태를 확인하세요.";

                    return;
                }

                if (!SteamUser.BLoggedOn())
                {
                    State =
                        ProjectJSteamAuthState.LoginRequired;

                    StatusMessage =
                        "Steam 로그인이 필요합니다.";

                    return;
                }

                CSteamID steamId =
                    SteamUser.GetSteamID();

                ulong steamIdValue =
                    steamId.m_SteamID;

                if (steamIdValue == 0UL)
                {
                    State =
                        ProjectJSteamAuthState.LoginRequired;

                    StatusMessage =
                        "유효한 SteamID를 가져오지 못했습니다.";

                    return;
                }

                SteamId64 =
                    steamIdValue.ToString();

                PersonaName =
                    SteamFriends.GetPersonaName();

                ProjectAccountId =
                    "pj-steam-" +
                    SteamId64;

                webApiTicketCallback =
                    Callback<
                        GetTicketForWebApiResponse_t
                    >.Create(
                        OnWebApiTicketResponse
                    );

                webApiTicketHandle =
                    SteamUser.GetAuthTicketForWebApi(
                        WebApiTicketIdentity
                    );

                if (
                    webApiTicketHandle ==
                        HAuthTicket.Invalid
                )
                {
                    State =
                        ProjectJSteamAuthState.TicketFailed;

                    StatusMessage =
                        "Steam Web API 인증 티켓 요청 실패";

                    return;
                }

                State =
                    ProjectJSteamAuthState
                        .WaitingForWebApiTicket;

                StatusMessage =
                    "Steam 인증 티켓 응답 대기 중";
            }
            catch (Exception exception)
            {
                State =
                    ProjectJSteamAuthState.SteamUnavailable;

                StatusMessage =
                    "Steam 초기화 예외: " +
                    exception.GetType().Name;

                Debug.LogError(
                    "[Project J/Steam] " +
                    StatusMessage +
                    "\n" +
                    exception
                );
            }
#else
            State =
                ProjectJSteamAuthState.PackageMissing;

            StatusMessage =
                "Steamworks.NET 패키지가 아직 컴파일되지 않았습니다.";
#endif
        }

        public bool TryGetWebApiTicketHex(
            out string ticketHex
        )
        {
            ticketHex =
                webApiTicketHex;

            return WebApiTicketReady;
        }

        public static bool TryGetAuthenticated(
            out ProjectJSteamIdentityService service
        )
        {
            service =
                instance;

            return
                service != null &&
                service.IsAuthenticated;
        }

#if STEAMWORKS_NET
        private void OnWebApiTicketResponse(
            GetTicketForWebApiResponse_t response
        )
        {
            if (
                response.m_hAuthTicket !=
                    webApiTicketHandle
            )
            {
                return;
            }

            if (
                response.m_eResult !=
                    EResult.k_EResultOK
            )
            {
                State =
                    ProjectJSteamAuthState.TicketFailed;

                StatusMessage =
                    "Steam 인증 티켓 실패: " +
                    response.m_eResult;

                return;
            }

            int ticketLength =
                Mathf.Clamp(
                    response.m_cubTicket,
                    0,
                    response.m_rgubTicket.Length
                );

            if (ticketLength <= 0)
            {
                State =
                    ProjectJSteamAuthState.TicketFailed;

                StatusMessage =
                    "Steam 인증 티켓 데이터가 비어 있습니다.";

                return;
            }

            webApiTicketHex =
                BitConverter
                    .ToString(
                        response.m_rgubTicket,
                        0,
                        ticketLength
                    )
                    .Replace(
                        "-",
                        string.Empty
                    )
                    .ToLowerInvariant();

            WebApiTicketByteLength =
                ticketLength;

            State =
                ProjectJSteamAuthState.Authenticated;

            StatusMessage =
                "Steam 인증 준비 완료";

            Debug.Log(
                "[Project J/Steam] 인증 준비 완료 / " +
                "ProjectAccountId: " +
                ProjectAccountId +
                " / Persona: " +
                PersonaName +
                " / TicketBytes: " +
                WebApiTicketByteLength
            );
        }
#endif

        private void ResetIdentity()
        {
            SteamId64 =
                string.Empty;

            PersonaName =
                string.Empty;

            ProjectAccountId =
                string.Empty;

            webApiTicketHex =
                string.Empty;

            WebApiTicketByteLength =
                0;
        }

        private void ShutdownSteam()
        {
#if STEAMWORKS_NET
            if (
                webApiTicketHandle !=
                    HAuthTicket.Invalid
            )
            {
                SteamUser.CancelAuthTicket(
                    webApiTicketHandle
                );

                webApiTicketHandle =
                    HAuthTicket.Invalid;
            }

            if (webApiTicketCallback != null)
            {
                webApiTicketCallback.Dispose();

                webApiTicketCallback =
                    null;
            }

            if (steamInitialized)
            {
                SteamAPI.Shutdown();

                steamInitialized =
                    false;
            }
#endif

            State =
                ProjectJSteamAuthState.Uninitialized; // 종료 후 인증 상태 해제

            StatusMessage =
                "Steam 종료됨"; // 종료 상태 표시

            ResetIdentity(); // 종료 후 인증 데이터 초기화
        }
    }
}
