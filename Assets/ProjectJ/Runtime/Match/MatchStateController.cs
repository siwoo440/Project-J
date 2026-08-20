using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Match
{
    [DisallowMultipleComponent]
    public sealed class MatchStateController : MonoBehaviour
    {
        public const int CountdownStartNumber = 3;

        [SerializeField]
        [Min(0.1f)]
        private float countdownStepDuration = 1.25f;

        [SerializeField]
        [Min(0f)]
        private float readySettleDuration = 0.5f;

        [SerializeField]
        private bool autoReadyInOfflineMode = true;

        [SerializeField]
        private MatchState currentState =
            MatchState.Preparing;

        [SerializeField]
        private bool readySignalReceived;

        [SerializeField]
        [Min(0f)]
        private float readySettleRemaining;

        [SerializeField]
        [Min(0f)]
        private float countdownRemaining;

        [SerializeField]
        private int countdownDisplayNumber;

        [SerializeField]
        [Min(0f)]
        private float countdownStepRemaining;

        private bool countdownInitialFramePending;

        public event Action<MatchState> StateChanged;

        public MatchState CurrentState
        {
            get
            {
                return currentState;
            }
        }

        public float CountdownStepDuration
        {
            get
            {
                return countdownStepDuration;
            }
        }

        public float CountdownTotalDuration
        {
            get
            {
                return
                    CountdownStartNumber *
                    countdownStepDuration;
            }
        }

        public float CountdownRemaining
        {
            get
            {
                return countdownRemaining;
            }
        }

        public int CountdownDisplayNumber
        {
            get
            {
                if (
                    currentState !=
                    MatchState.Countdown
                )
                {
                    return 0;
                }

                return countdownDisplayNumber;
            }
        }

        public bool IsReadySignalReceived
        {
            get
            {
                return readySignalReceived;
            }
        }

        public float ReadySettleRemaining
        {
            get
            {
                return readySettleRemaining;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return currentState ==
                    MatchState.Playing;
            }
        }

        private void Awake()
        {
            currentState =
                MatchState.Preparing;

            ResetReadyValues();
            ResetCountdownValues();
        }

        private void Start()
        {
            ApplyPlayerInputState(
                false
            );

            if (autoReadyInOfflineMode)
            {
                NotifyAllPlayersReady();
            }
        }

        private void Update()
        {
            if (
                currentState ==
                MatchState.Preparing
            )
            {
                AdvanceReadySettle(
                    Time.unscaledDeltaTime
                );

                return;
            }

            if (
                currentState ==
                MatchState.Countdown
            )
            {
                ApplyPlayerInputState(
                    false
                );

                AdvanceCountdown(
                    Time.unscaledDeltaTime
                );
            }
        }

        public bool NotifyAllPlayersReady()
        {
            if (
                currentState !=
                MatchState.Preparing
            )
            {
                return false;
            }

            readySignalReceived = true;

            readySettleRemaining =
                Mathf.Max(
                    0f,
                    readySettleDuration
                );

            return true;
        }

        public bool CancelReadySignal()
        {
            if (
                currentState !=
                MatchState.Preparing
            )
            {
                return false;
            }

            ResetReadyValues();

            return true;
        }

        public void AdvanceReadySettle(
            float deltaTime
        )
        {
            if (
                currentState !=
                MatchState.Preparing ||
                !readySignalReceived
            )
            {
                return;
            }

            readySettleRemaining =
                Mathf.Max(
                    0f,
                    readySettleRemaining -
                    Mathf.Max(
                        0f,
                        deltaTime
                    )
                );

            if (readySettleRemaining > 0f)
            {
                return;
            }

            StartCountdown();
        }

        public bool StartCountdown()
        {
            if (
                !CanTransition(
                    currentState,
                    MatchState.Countdown
                )
            )
            {
                return false;
            }

            countdownDisplayNumber =
                CountdownStartNumber;

            countdownStepRemaining =
                countdownStepDuration;

            countdownRemaining =
                CountdownTotalDuration;

            countdownInitialFramePending =
                true;

            ChangeState(
                MatchState.Countdown
            );

            return true;
        }

        public void AdvanceCountdown(
            float deltaTime
        )
        {
            if (
                currentState !=
                MatchState.Countdown
            )
            {
                return;
            }

            if (countdownInitialFramePending)
            {
                countdownInitialFramePending =
                    false;

                return;
            }

            float safeDeltaTime =
                Mathf.Max(
                    0f,
                    deltaTime
                );

            countdownStepRemaining =
                Mathf.Max(
                    0f,
                    countdownStepRemaining -
                    safeDeltaTime
                );

            countdownRemaining =
                CalculateCountdownRemaining(
                    countdownDisplayNumber,
                    countdownStepRemaining,
                    countdownStepDuration
                );

            if (countdownStepRemaining > 0f)
            {
                return;
            }

            if (countdownDisplayNumber > 1)
            {
                countdownDisplayNumber--;

                countdownStepRemaining =
                    countdownStepDuration;

                countdownRemaining =
                    CalculateCountdownRemaining(
                        countdownDisplayNumber,
                        countdownStepRemaining,
                        countdownStepDuration
                    );

                return;
            }

            ResetCountdownValues();

            ChangeState(
                MatchState.Playing
            );
        }

        public bool FinishMatch()
        {
            if (
                !CanTransition(
                    currentState,
                    MatchState.Finished
                )
            )
            {
                return false;
            }

            ResetReadyValues();
            ResetCountdownValues();

            ChangeState(
                MatchState.Finished
            );

            return true;
        }

        public static bool CanTransition(
            MatchState from,
            MatchState to
        )
        {
            switch (from)
            {
                case MatchState.Preparing:
                    return to ==
                        MatchState.Countdown;

                case MatchState.Countdown:
                    return to ==
                        MatchState.Playing;

                case MatchState.Playing:
                    return to ==
                        MatchState.Finished;

                default:
                    return false;
            }
        }

        public static float CalculateCountdownRemaining(
            int displayNumber,
            float stepRemaining,
            float stepDuration
        )
        {
            if (
                displayNumber <= 0 ||
                stepDuration <= 0f
            )
            {
                return 0f;
            }

            int futureSteps =
                Mathf.Max(
                    0,
                    displayNumber - 1
                );

            return
                Mathf.Max(
                    0f,
                    stepRemaining
                ) +
                futureSteps *
                stepDuration;
        }

        private void ChangeState(
            MatchState nextState
        )
        {
            if (
                currentState ==
                nextState
            )
            {
                return;
            }

            if (
                !CanTransition(
                    currentState,
                    nextState
                )
            )
            {
                return;
            }

            currentState =
                nextState;

            ApplyPlayerInputState(
                currentState ==
                MatchState.Playing
            );

            StateChanged?.Invoke(
                currentState
            );
        }

        private void ResetReadyValues()
        {
            readySignalReceived = false;
            readySettleRemaining = 0f;
        }

        private void ResetCountdownValues()
        {
            countdownRemaining = 0f;
            countdownDisplayNumber = 0;
            countdownStepRemaining = 0f;
            countdownInitialFramePending =
                false;
        }

        private void ApplyPlayerInputState(
            bool allowGameplayInput
        )
        {
            PlayerInput[] playerInputs =
                FindObjectsByType<PlayerInput>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < playerInputs.Length;
                i++
            )
            {
                PlayerInput playerInput =
                    playerInputs[i];

                if (playerInput == null)
                {
                    continue;
                }

                if (allowGameplayInput)
                {
                    bool inputAlreadyActive =
                        playerInput.currentActionMap !=
                        null &&
                        playerInput
                            .currentActionMap
                            .enabled;

                    if (!inputAlreadyActive)
                    {
                        playerInput.ActivateInput();
                    }

                    continue;
                }

                bool inputAlreadyInactive =
                    playerInput.currentActionMap ==
                    null ||
                    !playerInput
                        .currentActionMap
                        .enabled;

                if (!inputAlreadyInactive)
                {
                    playerInput.DeactivateInput();
                }
            }
        }

        private void OnValidate()
        {
            countdownStepDuration =
                Mathf.Max(
                    0.1f,
                    countdownStepDuration
                );

            readySettleDuration =
                Mathf.Max(
                    0f,
                    readySettleDuration
                );
        }
    }
}
