using ProjectJ.Debugging; // 통합 디버그 패널 표시 상태 사용
using UnityEngine;

namespace ProjectJ.Match
{
    [DisallowMultipleComponent]
    public sealed class MatchStateDebugView : MonoBehaviour
    {
        [SerializeField]
        private MatchStateController controller;

        [SerializeField]
        [Min(0f)]
        private float startDisplayDuration = 0.9f;

        private MatchState lastState;
        private float startVisibleUntil;
        private GUIStyle centerStyle;
        private GUIStyle stateStyle;

        private void Awake()
        {
            TryFindController();

            if (controller != null)
            {
                lastState =
                    controller.CurrentState;
            }
        }

        private void Update()
        {
            TryFindController();

            if (controller == null)
            {
                return;
            }

            MatchState state =
                controller.CurrentState;

            if (state == lastState)
            {
                return;
            }

            lastState = state;

            if (
                state ==
                MatchState.Playing
            )
            {
                startVisibleUntil =
                    Time.unscaledTime +
                    startDisplayDuration;
            }
        }

        private void OnGUI()
        {
            if (!ProjectJDebugOverlayController.IsVisible) // 통합 패널 선택 상태 확인
            {
                return; // 독립 진단창 출력 차단
            }

            TryFindController();

            if (controller == null)
            {
                return;
            }

            EnsureStyles();

            string centerText =
                GetCenterText();

            int originalFontSize =
                centerStyle.fontSize;

            if (
                controller.CurrentState ==
                MatchState.Countdown
            )
            {
                float pulse =
                    1f +
                    Mathf.Sin(
                        Time.unscaledTime *
                        6f
                    ) *
                    0.05f;

                centerStyle.fontSize =
                    Mathf.RoundToInt(
                        76f * pulse
                    );
            }
            else if (
                controller.CurrentState ==
                MatchState.Playing &&
                Time.unscaledTime <
                startVisibleUntil
            )
            {
                centerStyle.fontSize = 82;
            }

            GUI.Label(
                new Rect(
                    0f,
                    35f,
                    Screen.width,
                    120f
                ),
                centerText,
                centerStyle
            );

            centerStyle.fontSize =
                originalFontSize;

            GUI.Label(
                new Rect(
                    20f,
                    20f,
                    420f,
                    40f
                ),
                BuildStateText(),
                stateStyle
            );

            bool previousEnabled =
                GUI.enabled;

            GUI.enabled =
                controller.CurrentState ==
                MatchState.Playing;

            if (
                GUI.Button(
                    new Rect(
                        20f,
                        Screen.height - 70f,
                        180f,
                        42f
                    ),
                    "Finish Match"
                )
            )
            {
                controller.FinishMatch();
            }

            GUI.enabled =
                previousEnabled;
        }

        public void Configure(
            MatchStateController targetController
        )
        {
            controller =
                targetController;
        }

        private string GetCenterText()
        {
            if (
                controller.CurrentState ==
                MatchState.Preparing
            )
            {
                if (
                    controller
                        .IsReadySignalReceived
                )
                {
                    return "READY";
                }

                return "WAITING";
            }

            if (
                controller.CurrentState ==
                MatchState.Countdown
            )
            {
                return controller
                    .CountdownDisplayNumber
                    .ToString();
            }

            if (
                controller.CurrentState ==
                MatchState.Playing &&
                Time.unscaledTime <
                startVisibleUntil
            )
            {
                return "시작!";
            }

            if (
                controller.CurrentState ==
                MatchState.Finished
            )
            {
                return "FINISHED";
            }

            return string.Empty;
        }

        private string BuildStateText()
        {
            string text =
                "Match State : " +
                controller.CurrentState;

            if (
                controller.CurrentState ==
                MatchState.Preparing &&
                controller.IsReadySignalReceived
            )
            {
                text +=
                    " | Ready settle : " +
                    controller
                        .ReadySettleRemaining
                        .ToString("0.00");
            }

            return text;
        }

        private void TryFindController()
        {
            if (controller != null)
            {
                return;
            }

            controller =
                FindFirstObjectByType<
                    MatchStateController
                >();
        }

        private void EnsureStyles()
        {
            if (centerStyle == null)
            {
                centerStyle =
                    new GUIStyle(
                        GUI.skin.label
                    );

                centerStyle.alignment =
                    TextAnchor.MiddleCenter;

                centerStyle.fontSize = 76;
                centerStyle.fontStyle =
                    FontStyle.Bold;

                centerStyle.normal.textColor =
                    Color.black;
            }

            if (stateStyle == null)
            {
                stateStyle =
                    new GUIStyle(
                        GUI.skin.label
                    );

                stateStyle.fontSize = 20;
                stateStyle.fontStyle =
                    FontStyle.Bold;

                stateStyle.normal.textColor =
                    Color.black;
            }
        }
    }
}
