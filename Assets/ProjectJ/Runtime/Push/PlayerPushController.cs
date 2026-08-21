using System;
using ProjectJ.Finish;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Push
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(PlayerInput)
    )]
    [RequireComponent(
        typeof(PlayerPushTargetSelector)
    )]
    public sealed class PlayerPushController :
        MonoBehaviour
    {
        private const string PushActionName =
            "Push";

        [SerializeField]
        [Min(0f)]
        private float horizontalVelocityChange =
            12f;

        [SerializeField]
        [Min(0f)]
        private float upwardVelocityChange =
            0f;

        [SerializeField]
        [Min(0f)]
        private float cooldownDuration =
            1.5f;

        [SerializeField]
        private PlayerPushTargetSelector
            targetSelector;

        [SerializeField]
        private PlayerInput playerInput;

        [SerializeField]
        private PlayerFinishState selfFinishState;

        [SerializeField]
        private PushAttemptResult lastResult =
            PushAttemptResult.InvalidState;

        [SerializeField]
        private PlayerFinishState lastTarget;

        [SerializeField]
        [Min(0f)]
        private float remainingCooldown;

        private InputAction pushAction;
        private double nextAllowedPushTime;
        private bool isSubscribed;

        public event Action<
            PushAttemptResult,
            PlayerFinishState,
            Vector3
        > PushAttempted;

        public float HorizontalVelocityChange
        {
            get
            {
                return horizontalVelocityChange;
            }
        }

        public float UpwardVelocityChange
        {
            get
            {
                return upwardVelocityChange;
            }
        }

        public float CooldownDuration
        {
            get
            {
                return cooldownDuration;
            }
        }

        public float RemainingCooldown
        {
            get
            {
                return remainingCooldown;
            }
        }

        public bool IsOnCooldown
        {
            get
            {
                return remainingCooldown > 0f;
            }
        }

        public PushAttemptResult LastResult
        {
            get
            {
                return lastResult;
            }
        }

        public PlayerFinishState LastTarget
        {
            get
            {
                return lastTarget;
            }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribePushInput();
        }

        private void OnDisable()
        {
            UnsubscribePushInput();
        }

        private void Update()
        {
            EvaluateCooldownAt(
                Time.unscaledTimeAsDouble
            );
        }

        public void Configure(
            PlayerPushTargetSelector
                newTargetSelector,
            PlayerInput newPlayerInput,
            PlayerFinishState
                newSelfFinishState,
            float newHorizontalVelocityChange,
            float newUpwardVelocityChange,
            float newCooldownDuration
        )
        {
            UnsubscribePushInput();

            targetSelector =
                newTargetSelector;

            playerInput =
                newPlayerInput;

            selfFinishState =
                newSelfFinishState;

            horizontalVelocityChange =
                Mathf.Max(
                    0f,
                    newHorizontalVelocityChange
                );

            upwardVelocityChange =
                0f;

            cooldownDuration =
                Mathf.Max(
                    0f,
                    newCooldownDuration
                );

            ResetCooldown();
            ResolveReferences();

            if (isActiveAndEnabled)
            {
                SubscribePushInput();
            }
        }

        public PushAttemptResult TryPush()
        {
            return TryPushAt(
                Time.unscaledTimeAsDouble
            );
        }

        public PushAttemptResult TryPushAt(
            double currentTime
        )
        {
            ResolveReferences();
            EvaluateCooldownAt(
                currentTime
            );

            if (IsOnCooldown)
            {
                return CompleteAttempt(
                    PushAttemptResult.Cooldown,
                    null,
                    Vector3.zero
                );
            }

            if (
                targetSelector == null ||
                (
                    selfFinishState != null &&
                    selfFinishState.IsFinished
                )
            )
            {
                return CompleteAttempt(
                    PushAttemptResult.InvalidState,
                    null,
                    Vector3.zero
                );
            }

            StartCooldownAt(
                currentTime
            );

            bool foundTarget =
                targetSelector.TryFindTarget(
                    out PlayerFinishState target
                );

            if (!foundTarget)
            {
                return CompleteAttempt(
                    PushAttemptResult.Miss,
                    null,
                    Vector3.zero
                );
            }

            PlayerPushReceiver receiver =
                target.GetComponent<
                    PlayerPushReceiver
                >();

            if (receiver == null)
            {
                return CompleteAttempt(
                    PushAttemptResult.MissingReceiver,
                    target,
                    Vector3.zero
                );
            }

            if (receiver.IsRespawnProtected)
            {
                return CompleteAttempt(
                    PushAttemptResult.Protected,
                    target,
                    Vector3.zero
                );
            }

            Vector3 velocityChange =
                CalculatePushVelocityChange(
                    transform.position,
                    transform.forward,
                    target.transform.position,
                    horizontalVelocityChange,
                    upwardVelocityChange
                );

            bool applied =
                receiver.TryApplyPush(
                    velocityChange
                );

            if (!applied)
            {
                return CompleteAttempt(
                    PushAttemptResult.InvalidState,
                    target,
                    Vector3.zero
                );
            }

            return CompleteAttempt(
                PushAttemptResult.Success,
                target,
                velocityChange
            );
        }

        public bool EvaluateCooldownAt(
            double currentTime
        )
        {
            double remaining =
                nextAllowedPushTime -
                currentTime;

            remainingCooldown =
                Mathf.Max(
                    0f,
                    (float)remaining
                );

            return remainingCooldown > 0f;
        }

        public void ResetCooldown()
        {
            nextAllowedPushTime =
                0d;

            remainingCooldown =
                0f;
        }

        public static Vector3
            CalculatePushVelocityChange(
                Vector3 sourcePosition,
                Vector3 sourceForward,
                Vector3 targetPosition,
                float horizontalAmount,
                float upwardAmount
            )
        {
            Vector3 horizontalDirection =
                targetPosition -
                sourcePosition;

            horizontalDirection.y =
                0f;

            if (
                horizontalDirection.sqrMagnitude <=
                Mathf.Epsilon
            )
            {
                horizontalDirection =
                    sourceForward;

                horizontalDirection.y =
                    0f;
            }

            if (
                horizontalDirection.sqrMagnitude >
                Mathf.Epsilon
            )
            {
                horizontalDirection.Normalize();
            }

            return
                horizontalDirection *
                Mathf.Max(
                    0f,
                    horizontalAmount
                );
        }

        private PushAttemptResult CompleteAttempt(
            PushAttemptResult result,
            PlayerFinishState target,
            Vector3 velocityChange
        )
        {
            lastResult =
                result;

            lastTarget =
                target;

            PushAttempted?.Invoke(
                result,
                target,
                velocityChange
            );

            return result;
        }

        private void StartCooldownAt(
            double currentTime
        )
        {
            cooldownDuration =
                Mathf.Max(
                    0f,
                    cooldownDuration
                );

            nextAllowedPushTime =
                currentTime +
                cooldownDuration;

            remainingCooldown =
                cooldownDuration;
        }

        private void ResolveReferences()
        {
            if (targetSelector == null)
            {
                targetSelector =
                    GetComponent<
                        PlayerPushTargetSelector
                    >();
            }

            if (playerInput == null)
            {
                playerInput =
                    GetComponent<PlayerInput>();
            }

            if (selfFinishState == null)
            {
                selfFinishState =
                    GetComponent<
                        PlayerFinishState
                    >();
            }
        }

        private void SubscribePushInput()
        {
            if (
                isSubscribed ||
                playerInput == null ||
                playerInput.actions == null
            )
            {
                return;
            }

            pushAction =
                playerInput.actions.FindAction(
                    PushActionName,
                    false
                );

            if (pushAction == null)
            {
                return;
            }

            pushAction.performed +=
                HandlePushPerformed;

            isSubscribed = true;
        }

        private void UnsubscribePushInput()
        {
            if (
                !isSubscribed ||
                pushAction == null
            )
            {
                isSubscribed = false;
                pushAction = null;
                return;
            }

            pushAction.performed -=
                HandlePushPerformed;

            isSubscribed = false;
            pushAction = null;
        }

        private void HandlePushPerformed(
            InputAction.CallbackContext context
        )
        {
            TryPush();
        }

        private void OnValidate()
        {
            horizontalVelocityChange =
                Mathf.Max(
                    0f,
                    horizontalVelocityChange
                );

            upwardVelocityChange =
                0f;

            cooldownDuration =
                Mathf.Max(
                    0f,
                    cooldownDuration
                );
        }
    }
}
