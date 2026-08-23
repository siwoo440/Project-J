using ProjectJ.Steam; // Steam 초기화 상태 표시
using UnityEngine; // MonoBehaviour와 Application 사용
using UnityEngine.UI; // Bootstrap UI Text 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJDay85BootstrapStatusView :
        MonoBehaviour
    {
        [SerializeField]
        private Text loadingDotsText;

        [SerializeField]
        private Text statusText;

        [SerializeField]
        private Text detailText;

        [SerializeField]
        private Text versionText;

        private ProjectJSteamIdentityService steamIdentity;

        private ProjectJFusionBootstrap fusionBootstrap;

        private ProjectJDay82SceneFlowCoordinator sceneFlow;

        private float nextDotUpdateTime;

        private int dotStep;

        private void Awake()
        {
            ResolveReferences();
            RefreshVersion();
            RefreshView();
        }

        private void Update()
        {
            ResolveReferences();
            UpdateLoadingDots();
            RefreshView();
        }

        private void ResolveReferences()
        {
            if (steamIdentity == null)
            {
                steamIdentity =
                    ProjectJSteamIdentityService.Instance;
            }

            if (fusionBootstrap == null)
            {
                fusionBootstrap =
                    FindFirstObjectByType<
                        ProjectJFusionBootstrap
                    >();
            }

            if (sceneFlow == null)
            {
                sceneFlow =
                    FindFirstObjectByType<
                        ProjectJDay82SceneFlowCoordinator
                    >();
            }
        }

        private void RefreshVersion()
        {
            if (versionText == null)
            {
                return;
            }

            string version =
                string.IsNullOrWhiteSpace(
                    Application.version
                )
                    ? "DEV"
                    : Application.version;

            versionText.text =
                "v" +
                version +
                "  •  DAY 85";
        }

        private void UpdateLoadingDots()
        {
            if (
                loadingDotsText == null ||
                Time.unscaledTime <
                    nextDotUpdateTime
            )
            {
                return;
            }

            nextDotUpdateTime =
                Time.unscaledTime +
                0.35f;

            dotStep =
                (dotStep + 1) %
                4;

            loadingDotsText.text =
                dotStep switch
                {
                    0 => "○  ○  ○",
                    1 => "●  ○  ○",
                    2 => "●  ●  ○",
                    _ => "●  ●  ●"
                };
        }

        private void RefreshView()
        {
            if (
                statusText == null ||
                detailText == null
            )
            {
                return;
            }

            if (steamIdentity == null)
            {
                statusText.text =
                    "Steam 서비스 준비 중...";

                detailText.text =
                    BuildSystemDetail(
                        "Steam Identity Service 생성 대기"
                    );

                return;
            }

            switch (steamIdentity.State)
            {
                case ProjectJSteamAuthState.Uninitialized:
                    statusText.text =
                        "Steam 초기화 준비 중...";

                    detailText.text =
                        BuildSystemDetail(
                            steamIdentity.StatusMessage
                        );
                    return;

                case ProjectJSteamAuthState.Initializing:
                    statusText.text =
                        "Steam 초기화 중...";

                    detailText.text =
                        BuildSystemDetail(
                            steamIdentity.StatusMessage
                        );
                    return;

                case ProjectJSteamAuthState.WaitingForWebApiTicket:
                    statusText.text =
                        "Steam 사용자 인증 확인 중...";

                    detailText.text =
                        BuildSystemDetail(
                            steamIdentity.StatusMessage
                        );
                    return;

                case ProjectJSteamAuthState.Authenticated:
                    statusText.text =
                        "온라인 시스템 준비 완료";

                    detailText.text =
                        BuildSystemDetail(
                            "MainMenu 전환 준비"
                        );
                    return;

                case ProjectJSteamAuthState.LoginRequired:
                    statusText.text =
                        "Steam 로그인이 필요합니다.";

                    detailText.text =
                        BuildSystemDetail(
                            steamIdentity.StatusMessage
                        );
                    return;

                case ProjectJSteamAuthState.TicketFailed:
                    statusText.text =
                        "Steam 인증 티켓 확인 실패";

                    detailText.text =
                        BuildSystemDetail(
                            steamIdentity.StatusMessage
                        );
                    return;

                case ProjectJSteamAuthState.PackageMissing:
                    statusText.text =
                        "Steamworks.NET 패키지를 확인해주세요.";

                    detailText.text =
                        BuildSystemDetail(
                            steamIdentity.StatusMessage
                        );
                    return;

                default:
                    statusText.text =
                        "Steam 연결 상태를 확인해주세요.";

                    detailText.text =
                        BuildSystemDetail(
                            steamIdentity.StatusMessage
                        );
                    return;
            }
        }

        private string BuildSystemDetail(
            string message
        )
        {
            string fusionState =
                fusionBootstrap == null
                    ? "Fusion 생성 대기"
                    : "Fusion " +
                      fusionBootstrap.State;

            string flowState =
                sceneFlow == null
                    ? "Flow 생성 대기"
                    : "Flow " +
                      sceneFlow.State;

            return
                message +
                "   |   " +
                fusionState +
                "   |   " +
                flowState;
        }
    }
}
