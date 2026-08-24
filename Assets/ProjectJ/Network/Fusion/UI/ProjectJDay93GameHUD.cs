using UnityEngine; // GameObject와 수치 처리
using UnityEngine.UI; // Image와 Button 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent] // HUD 중복 Component 방지
    public sealed class ProjectJDay93GameHUD :
        MonoBehaviour
    {
        private const float ReferenceResolveInterval =
            0.5f; // Network Player 재탐색 간격

        [SerializeField]
        private GameObject matchHudRoot;

        [SerializeField]
        private GameObject countdownRoot;

        [SerializeField]
        private GameObject resultRoot;

        [SerializeField]
        private GameObject respawnProtectionRoot;

        [SerializeField]
        private Text timerText;

        [SerializeField]
        private Text heightText;

        [SerializeField]
        private Text rankText;

        [SerializeField]
        private Text staminaText;

        [SerializeField]
        private Image staminaFill;

        [SerializeField]
        private Text leftItemText;

        [SerializeField]
        private Text rightItemText;

        [SerializeField]
        private Image leftItemBackground;

        [SerializeField]
        private Image rightItemBackground;

        [SerializeField]
        private Text respawnProtectionText;

        [SerializeField]
        private Text countdownText;

        [SerializeField]
        private Text resultTitleText;

        [SerializeField]
        private Text resultStatusText;

        [SerializeField]
        private Text resultRankText;

        [SerializeField]
        private Text resultTimeText;

        [SerializeField]
        private Text resultHeightText;

        [SerializeField]
        private Button returnLobbyButton;

        [SerializeField]
        private Text returnLobbyButtonText;

        private ProjectJNetworkPlayer localPlayer;

        private ProjectJNetworkExternalGameplay localGameplay;

        private ProjectJNetworkItemInventory localInventory;

        private ProjectJDay82SceneFlowCoordinator sceneFlow;

        private float nextReferenceResolveTime;

        private static readonly Color NormalSlotColor =
            new Color(
                0.12f,
                0.14f,
                0.18f,
                0.92f
            ); // 일반 슬롯 배경

        private static readonly Color SelectedSlotColor =
            new Color(
                0.18f,
                0.55f,
                0.95f,
                0.95f
            ); // 선택 슬롯 배경

        private void Awake()
        {
            if (returnLobbyButton != null)
            {
                returnLobbyButton.onClick.AddListener(
                    RequestReturnToLobby
                ); // 기존 Scene Flow Lobby 복귀 연결
            }

            ResolveReferences(
                true
            ); // 시작 시 Network 참조 탐색
        }

        private void OnDestroy()
        {
            if (returnLobbyButton != null)
            {
                returnLobbyButton.onClick.RemoveListener(
                    RequestReturnToLobby
                ); // Button Listener 정리
            }
        }

        private void Update()
        {
            ResolveReferences(
                false
            ); // 참가·Scene 전환 후 참조 재탐색

            if (
                localPlayer == null ||
                localGameplay == null
            )
            {
                RefreshWaitingHud();
                return;
            }

            RefreshMatchHud();
            RefreshCountdown();
            RefreshResult();
        }

        private void ResolveReferences(
            bool force
        )
        {
            if (
                !force &&
                Time.unscaledTime <
                    nextReferenceResolveTime &&
                localPlayer != null &&
                localGameplay != null
            )
            {
                return;
            }

            nextReferenceResolveTime =
                Time.unscaledTime +
                ReferenceResolveInterval;

            if (sceneFlow == null)
            {
                sceneFlow =
                    Object.FindFirstObjectByType<
                        ProjectJDay82SceneFlowCoordinator
                    >(); // 영구 Scene Flow 조회
            }

            if (
                localPlayer != null &&
                localPlayer.HasLocalInputAuthority
            )
            {
                return; // 기존 Local Player 참조 유지
            }

            localPlayer =
                null;

            localGameplay =
                null;

            localInventory =
                null;

            ProjectJNetworkPlayer[] players =
                Object.FindObjectsByType<
                    ProjectJNetworkPlayer
                >(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                ); // 현재 Peer의 Network Player 검색

            for (
                int index = 0;
                index < players.Length;
                index++
            )
            {
                ProjectJNetworkPlayer player =
                    players[index];

                if (
                    player == null ||
                    !player.HasLocalInputAuthority
                )
                {
                    continue;
                }

                localPlayer =
                    player; // Input Authority Player를 Local Player로 선택

                localGameplay =
                    player.GetComponent<
                        ProjectJNetworkExternalGameplay
                    >(); // 경기 Networked 상태 조회

                localInventory =
                    player.GetComponent<
                        ProjectJNetworkItemInventory
                    >(); // 2슬롯 Networked 인벤토리 조회

                break;
            }
        }

        private void RefreshWaitingHud()
        {
            SetActive(
                matchHudRoot,
                true
            );

            SetActive(
                countdownRoot,
                false
            );

            SetActive(
                resultRoot,
                false
            );

            SetActive(
                respawnProtectionRoot,
                false
            );

            SetText(
                timerText,
                "TIME --:--"
            );

            SetText(
                heightText,
                "HEIGHT --.-- m"
            );

            SetText(
                rankText,
                "RANK -- / --"
            );

            SetText(
                staminaText,
                "STAMINA -- / --"
            );

            if (staminaFill != null)
            {
                staminaFill.fillAmount =
                    0f;
            }

            RefreshItemSlots();
        }

        private void RefreshMatchHud()
        {
            bool showResult =
                localGameplay.IsResultLocked ||
                localGameplay.MatchState ==
                    ProjectJNetworkMatchState.Finished;

            SetActive(
                matchHudRoot,
                !showResult
            ); // 결과 화면에서는 기본 HUD 숨김

            SetText(
                timerText,
                "TIME " +
                FormatMatchTime(
                    localGameplay.MatchTimeRemaining
                )
            ); // Network Match Timer 표시

            SetText(
                heightText,
                "HEIGHT " +
                localGameplay.RaceHeight.ToString(
                    "F2"
                ) +
                " m"
            ); // Network 발 높이 표시

            int participantCount =
                CountParticipants();

            int rank =
                Mathf.Max(
                    1,
                    localGameplay.RaceRank
                );

            SetText(
                rankText,
                "RANK " +
                rank +
                " / " +
                Mathf.Max(
                    1,
                    participantCount
                )
            ); // Network 순위와 참가 인원 표시

            float stamina =
                Mathf.Max(
                    0f,
                    localPlayer.Stamina
                );

            float maximumStamina =
                Mathf.Max(
                    1f,
                    localPlayer.StaminaMaximum
                );

            SetText(
                staminaText,
                "STAMINA " +
                Mathf.CeilToInt(
                    stamina
                ) +
                " / " +
                Mathf.CeilToInt(
                    maximumStamina
                )
            ); // Network Stamina 수치 표시

            if (staminaFill != null)
            {
                staminaFill.fillAmount =
                    Mathf.Clamp01(
                        stamina /
                        maximumStamina
                    ); // Network Stamina 비율 표시
            }

            RefreshItemSlots();
            RefreshRespawnProtection();
        }

        private void RefreshItemSlots()
        {
            int leftItemId =
                localInventory != null
                    ? localInventory.SlotLeftItemId
                    : 0;

            int rightItemId =
                localInventory != null
                    ? localInventory.SlotRightItemId
                    : 0;

            int selectedSlotIndex =
                localInventory != null
                    ? localInventory.SelectedSlotIndex
                    : 0;

            string leftName =
                localInventory != null
                    ? localInventory.GetSlotDisplayName(0)
                    : ProjectJNetworkItemCatalog.GetDisplayName(leftItemId); // 첫 슬롯 Stack 표시

            string rightName =
                localInventory != null
                    ? localInventory.GetSlotDisplayName(1)
                    : ProjectJNetworkItemCatalog.GetDisplayName(rightItemId); // 두 번째 슬롯 Stack 표시

            SetText(
                leftItemText,
                "SLOT 1\n" +
                leftName
            );

            SetText(
                rightItemText,
                "SLOT 2\n" +
                rightName
            );

            if (leftItemBackground != null)
            {
                leftItemBackground.color =
                    selectedSlotIndex == 0
                        ? SelectedSlotColor
                        : NormalSlotColor;
            }

            if (rightItemBackground != null)
            {
                rightItemBackground.color =
                    selectedSlotIndex == 1
                        ? SelectedSlotColor
                        : NormalSlotColor;
            }
        }

        private void RefreshRespawnProtection()
        {
            bool protectedState =
                localGameplay != null &&
                localGameplay.IsRespawnProtected;

            SetActive(
                respawnProtectionRoot,
                protectedState
            );

            if (!protectedState)
            {
                return;
            }

            SetText(
                respawnProtectionText,
                "RESPAWN PROTECTION  " +
                localGameplay
                    .RespawnProtectionRemaining
                    .ToString(
                        "F1"
                    ) +
                "s"
            ); // Network 보호 Timer 표시
        }

        private void RefreshCountdown()
        {
            bool countdownActive =
                localGameplay != null &&
                localGameplay.MatchState ==
                    ProjectJNetworkMatchState.Countdown;

            SetActive(
                countdownRoot,
                countdownActive
            );

            if (!countdownActive)
            {
                return;
            }

            float remaining =
                localGameplay.CountdownRemaining;

            if (remaining <= 0.01f)
            {
                SetText(
                    countdownText,
                    "GO!"
                );
                return;
            }

            int number =
                Mathf.Clamp(
                    Mathf.CeilToInt(
                        remaining
                    ),
                    1,
                    3
                );

            SetText(
                countdownText,
                number.ToString()
            ); // Fusion Countdown Timer를 정수로 표시
        }

        private void RefreshResult()
        {
            bool showResult =
                localGameplay != null &&
                (
                    localGameplay.IsResultLocked ||
                    localGameplay.MatchState ==
                        ProjectJNetworkMatchState.Finished
                );

            SetActive(
                resultRoot,
                showResult
            );

            if (!showResult)
            {
                return;
            }

            bool matchFinished =
                localGameplay.MatchState ==
                    ProjectJNetworkMatchState.Finished;

            SetText(
                resultTitleText,
                localGameplay.IsFinished
                    ? "FINISH!"
                    : "MATCH OVER"
            );

            SetText(
                resultStatusText,
                matchFinished
                    ? GetFinalStatusText()
                    : "PERSONAL RESULT"
            );

            int finalRank =
                localGameplay.FinalRank > 0
                    ? localGameplay.FinalRank
                    : Mathf.Max(
                        1,
                        localGameplay.RaceRank
                    );

            int participantCount =
                Mathf.Max(
                    1,
                    CountParticipants()
                );

            SetText(
                resultRankText,
                "RANK  " +
                finalRank +
                " / " +
                participantCount
            ); // 서버 확정 순위 표시

            SetText(
                resultTimeText,
                "TIME  " +
                FormatFinishTime(
                    localGameplay
                        .FinishElapsedSeconds
                )
            ); // 서버 확정 Finish 시간 표시

            SetText(
                resultHeightText,
                "BEST HEIGHT  " +
                localGameplay
                    .BestRaceHeight
                    .ToString(
                        "F2"
                    ) +
                " m"
            ); // 서버 확정 최고 높이 표시

            bool canReturn =
                sceneFlow != null &&
                sceneFlow.LobbyFlow != null &&
                sceneFlow.LobbyFlow.CanReturnToLobby;

            if (returnLobbyButton != null)
            {
                returnLobbyButton.interactable =
                    canReturn; // Host Scene Authority의 종료 후 복귀만 허용
            }

            if (canReturn)
            {
                SetText(
                    returnLobbyButtonText,
                    "RETURN TO LOBBY"
                );
            }
            else if (matchFinished)
            {
                SetText(
                    returnLobbyButtonText,
                    "WAIT FOR HOST"
                );
            }
            else
            {
                SetText(
                    returnLobbyButtonText,
                    "WAIT FOR MATCH END"
                );
            }
        }

        private string GetFinalStatusText()
        {
            switch (
                localGameplay.MatchEndReason
            )
            {
                case ProjectJNetworkMatchEndReason.AllFinished:
                    return "FINAL RESULT / ALL FINISHED";

                case ProjectJNetworkMatchEndReason.TimeExpired:
                    return "FINAL RESULT / TIME EXPIRED";

                default:
                    return "FINAL RESULT";
            }
        }

        private int CountParticipants()
        {
            ProjectJNetworkPlayer[] players =
                Object.FindObjectsByType<
                    ProjectJNetworkPlayer
                >(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            int count =
                0;

            for (
                int index = 0;
                index < players.Length;
                index++
            )
            {
                if (
                    players[index] != null &&
                    players[index].isActiveAndEnabled
                )
                {
                    count++;
                }
            }

            return count;
        }

        private void RequestReturnToLobby()
        {
            ResolveReferences(
                true
            );

            if (sceneFlow == null)
            {
                Debug.LogWarning(
                    "[Project J/Day93] Scene Flow를 찾지 못해 Lobby 복귀를 요청할 수 없습니다."
                );

                return;
            }

            bool requested =
                sceneFlow.RequestReturnToLobby(); // 기존 Fusion Lobby 복귀 사용

            if (!requested)
            {
                Debug.LogWarning(
                    "[Project J/Day93] 현재 상태에서는 Lobby 복귀가 허용되지 않습니다. / " +
                    sceneFlow.StatusText
                );
            }
        }

        private static string FormatMatchTime(
            float seconds
        )
        {
            float clamped =
                Mathf.Max(
                    0f,
                    seconds
                );

            int totalSeconds =
                Mathf.CeilToInt(
                    clamped
                );

            int minutes =
                totalSeconds /
                60;

            int remainingSeconds =
                totalSeconds %
                60;

            return
                minutes.ToString(
                    "00"
                ) +
                ":" +
                remainingSeconds.ToString(
                    "00"
                );
        }

        private static string FormatFinishTime(
            float seconds
        )
        {
            if (seconds < 0f)
            {
                return "--:--.--";
            }

            float clamped =
                Mathf.Max(
                    0f,
                    seconds
                );

            int minutes =
                Mathf.FloorToInt(
                    clamped /
                    60f
                );

            float remainingSeconds =
                clamped -
                minutes *
                60f;

            return
                minutes.ToString(
                    "00"
                ) +
                ":" +
                remainingSeconds.ToString(
                    "00.00"
                );
        }

        private static void SetText(
            Text target,
            string value
        )
        {
            if (target != null)
            {
                target.text =
                    value;
            }
        }

        private static void SetActive(
            GameObject target,
            bool active
        )
        {
            if (
                target != null &&
                target.activeSelf !=
                    active
            )
            {
                target.SetActive(
                    active
                );
            }
        }
    }
}
