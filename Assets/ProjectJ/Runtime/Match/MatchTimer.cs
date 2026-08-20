using System;
using UnityEngine;

namespace ProjectJ.Match
{
    [DisallowMultipleComponent]
    public sealed class MatchTimer : MonoBehaviour
    {
        public const float DefaultMatchDurationSeconds =
            15f * 60f;

        public const int OneMinuteWarningSeconds = 60;
        public const int ThirtySecondsWarningSeconds = 30;
        public const int TenSecondsWarningSeconds = 10;

        [SerializeField]
        private MatchStateController matchStateController;

        [SerializeField]
        [Min(1f)]
        private float matchDurationSeconds =
            DefaultMatchDurationSeconds;

        [SerializeField]
        [Min(0f)]
        private float remainingSeconds =
            DefaultMatchDurationSeconds;

        [SerializeField]
        private bool oneMinuteWarningTriggered;

        [SerializeField]
        private bool thirtySecondsWarningTriggered;

        [SerializeField]
        private bool tenSecondsWarningTriggered;

        [SerializeField]
        private bool expirationTriggered;

        public event Action<int> WarningReached;
        public event Action TimeExpired;

        public MatchStateController MatchStateController
        {
            get
            {
                return matchStateController;
            }
        }

        public float MatchDurationSeconds
        {
            get
            {
                return matchDurationSeconds;
            }
        }

        public float RemainingSeconds
        {
            get
            {
                return remainingSeconds;
            }
        }

        public int RemainingWholeSeconds
        {
            get
            {
                return Mathf.Max(
                    0,
                    Mathf.CeilToInt(
                        remainingSeconds
                    )
                );
            }
        }

        public string FormattedRemainingTime
        {
            get
            {
                return FormatTime(
                    remainingSeconds
                );
            }
        }

        public bool IsExpired
        {
            get
            {
                return expirationTriggered;
            }
        }

        private void Awake()
        {
            ResolveController();
            ResetTimer();
        }

        private void Update()
        {
            AdvanceTimer(
                Time.unscaledDeltaTime
            );
        }

        public void Configure(
            MatchStateController controller,
            float durationSeconds
        )
        {
            matchStateController =
                controller;

            matchDurationSeconds =
                Mathf.Max(
                    1f,
                    durationSeconds
                );

            ResetTimer();
        }

        public void ResetTimer()
        {
            remainingSeconds =
                Mathf.Max(
                    1f,
                    matchDurationSeconds
                );

            oneMinuteWarningTriggered = false;
            thirtySecondsWarningTriggered = false;
            tenSecondsWarningTriggered = false;
            expirationTriggered = false;
        }

        public void AdvanceTimer(
            float deltaTime
        )
        {
            ResolveController();

            if (
                matchStateController == null ||
                matchStateController.CurrentState !=
                MatchState.Playing ||
                expirationTriggered
            )
            {
                return;
            }

            float safeDeltaTime =
                Mathf.Max(
                    0f,
                    deltaTime
                );

            float previousSeconds =
                remainingSeconds;

            remainingSeconds =
                Mathf.Max(
                    0f,
                    remainingSeconds -
                    safeDeltaTime
                );

            TriggerWarnings(
                previousSeconds,
                remainingSeconds
            );

            if (remainingSeconds > 0f)
            {
                return;
            }

            expirationTriggered = true;

            TimeExpired?.Invoke();

            matchStateController
                .FinishMatch();
        }

        public static string FormatTime(
            float seconds
        )
        {
            int totalSeconds =
                Mathf.Max(
                    0,
                    Mathf.CeilToInt(
                        seconds
                    )
                );

            int minutes =
                totalSeconds / 60;

            int remaining =
                totalSeconds % 60;

            return
                minutes.ToString("00") +
                ":" +
                remaining.ToString("00");
        }

        private void TriggerWarnings(
            float previousSeconds,
            float currentSeconds
        )
        {
            TryTriggerWarning(
                OneMinuteWarningSeconds,
                previousSeconds,
                currentSeconds,
                ref oneMinuteWarningTriggered
            );

            TryTriggerWarning(
                ThirtySecondsWarningSeconds,
                previousSeconds,
                currentSeconds,
                ref thirtySecondsWarningTriggered
            );

            TryTriggerWarning(
                TenSecondsWarningSeconds,
                previousSeconds,
                currentSeconds,
                ref tenSecondsWarningTriggered
            );
        }

        private void TryTriggerWarning(
            int warningSeconds,
            float previousSeconds,
            float currentSeconds,
            ref bool alreadyTriggered
        )
        {
            if (alreadyTriggered)
            {
                return;
            }

            if (
                previousSeconds >
                warningSeconds &&
                currentSeconds <=
                warningSeconds
            )
            {
                alreadyTriggered = true;

                WarningReached?.Invoke(
                    warningSeconds
                );
            }
        }

        private void ResolveController()
        {
            if (matchStateController != null)
            {
                return;
            }

            matchStateController =
                GetComponent<
                    MatchStateController
                >();

            if (matchStateController != null)
            {
                return;
            }

            matchStateController =
                FindFirstObjectByType<
                    MatchStateController
                >();
        }

        private void OnValidate()
        {
            matchDurationSeconds =
                Mathf.Max(
                    1f,
                    matchDurationSeconds
                );

            remainingSeconds =
                Mathf.Clamp(
                    remainingSeconds,
                    0f,
                    matchDurationSeconds
                );
        }
    }
}
