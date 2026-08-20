using UnityEngine;

namespace ProjectJ.Match
{
    [DisallowMultipleComponent]
    public sealed class MatchTimerDebugView : MonoBehaviour
    {
        [SerializeField]
        private MatchTimer matchTimer;

        [SerializeField]
        [Min(0f)]
        private float warningDisplayDuration = 1.5f;

        private string warningText =
            string.Empty;

        private float warningVisibleUntil;
        private GUIStyle timerStyle;
        private GUIStyle warningStyle;

        private void OnEnable()
        {
            ResolveTimer();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnGUI()
        {
            ResolveTimer();

            if (matchTimer == null)
            {
                return;
            }

            EnsureStyles();

            GUI.Label(
                new Rect(
                    Screen.width - 240f,
                    20f,
                    210f,
                    50f
                ),
                matchTimer.FormattedRemainingTime,
                timerStyle
            );

            if (
                Time.unscaledTime <
                warningVisibleUntil &&
                !string.IsNullOrEmpty(
                    warningText
                )
            )
            {
                GUI.Label(
                    new Rect(
                        0f,
                        165f,
                        Screen.width,
                        70f
                    ),
                    warningText,
                    warningStyle
                );
            }
        }

        public void Configure(
            MatchTimer timer
        )
        {
            Unsubscribe();

            matchTimer =
                timer;

            Subscribe();
        }

        private void HandleWarningReached(
            int warningSeconds
        )
        {
            switch (warningSeconds)
            {
                case MatchTimer.OneMinuteWarningSeconds:
                    warningText =
                        "1분 남음!";
                    break;

                case MatchTimer.ThirtySecondsWarningSeconds:
                    warningText =
                        "30초 남음!";
                    break;

                case MatchTimer.TenSecondsWarningSeconds:
                    warningText =
                        "10초 남음!";
                    break;

                default:
                    warningText =
                        warningSeconds +
                        "초 남음!";
                    break;
            }

            warningVisibleUntil =
                Time.unscaledTime +
                warningDisplayDuration;
        }

        private void ResolveTimer()
        {
            if (matchTimer != null)
            {
                return;
            }

            matchTimer =
                FindFirstObjectByType<
                    MatchTimer
                >();
        }

        private void Subscribe()
        {
            if (matchTimer == null)
            {
                return;
            }

            matchTimer.WarningReached -=
                HandleWarningReached;

            matchTimer.WarningReached +=
                HandleWarningReached;
        }

        private void Unsubscribe()
        {
            if (matchTimer == null)
            {
                return;
            }

            matchTimer.WarningReached -=
                HandleWarningReached;
        }

        private void EnsureStyles()
        {
            if (timerStyle == null)
            {
                timerStyle =
                    new GUIStyle(
                        GUI.skin.label
                    );

                timerStyle.alignment =
                    TextAnchor.MiddleRight;

                timerStyle.fontSize = 32;
                timerStyle.fontStyle =
                    FontStyle.Bold;

                timerStyle.normal.textColor =
                    Color.black;
            }

            if (warningStyle == null)
            {
                warningStyle =
                    new GUIStyle(
                        GUI.skin.label
                    );

                warningStyle.alignment =
                    TextAnchor.MiddleCenter;

                warningStyle.fontSize = 36;
                warningStyle.fontStyle =
                    FontStyle.Bold;

                warningStyle.normal.textColor =
                    Color.black;
            }
        }
    }
}
