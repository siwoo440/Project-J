using ProjectJ.Debugging;
using ProjectJ.Finish;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjectJ.Push
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(PlayerInput)
    )]
    [RequireComponent(
        typeof(PlayerPushController)
    )]
    [RequireComponent(
        typeof(PlayerPushReceiver)
    )]
    public sealed class PlayerPushFeedbackUI :
        MonoBehaviour
    {
        [SerializeField]
        private PlayerInput playerInput;

        [SerializeField]
        private PlayerPushController pushController;

        [SerializeField]
        private PlayerPushReceiver pushReceiver;

        [SerializeField]
        private PlayerPushTargetSelector targetSelector;

        [SerializeField]
        private Canvas feedbackCanvas;

        [SerializeField]
        private Text judgmentText;

        [SerializeField]
        private LineRenderer swingLine;

        [SerializeField]
        [Min(0.05f)]
        private float judgmentDuration =
            0.65f;

        [SerializeField]
        [Min(0.02f)]
        private float swingDuration =
            0.12f;

        private float judgmentVisibleUntil;
        private float swingVisibleUntil;
        private bool hasTransientJudgment;

        private void Awake()
        {
            ResolveReferences();
            RefreshSwingGeometry();

            if (feedbackCanvas != null)
            {
                feedbackCanvas.enabled =
                    false;
            }

            if (swingLine != null)
            {
                swingLine.enabled =
                    false;
            }
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();

            if (feedbackCanvas != null)
            {
                feedbackCanvas.enabled =
                    false;
            }

            if (swingLine != null)
            {
                swingLine.enabled =
                    false;
            }
        }

        private void Update()
        {
            ResolveReferences();

            bool debugVisible =
                ProjectJDebugOverlayController
                    .IsVisible;

            bool shouldShowLocalUi =
                debugVisible &&
                playerInput != null &&
                playerInput.enabled &&
                gameObject.activeInHierarchy;

            if (feedbackCanvas != null)
            {
                feedbackCanvas.enabled =
                    shouldShowLocalUi;
            }

            if (
                !debugVisible &&
                swingLine != null
            )
            {
                swingLine.enabled =
                    false;
            }

            float now =
                Time.unscaledTime;

            if (
                swingLine != null &&
                swingLine.enabled &&
                now >= swingVisibleUntil
            )
            {
                swingLine.enabled =
                    false;
            }

            if (!shouldShowLocalUi)
            {
                return;
            }

            if (
                hasTransientJudgment &&
                now < judgmentVisibleUntil
            )
            {
                return;
            }

            hasTransientJudgment =
                false;

            UpdateReadyText();
        }

        public void Configure(
            PlayerInput newPlayerInput,
            PlayerPushController
                newPushController,
            PlayerPushReceiver
                newPushReceiver,
            PlayerPushTargetSelector
                newTargetSelector,
            Canvas newFeedbackCanvas,
            Text newJudgmentText,
            LineRenderer newSwingLine
        )
        {
            UnsubscribeEvents();

            playerInput =
                newPlayerInput;

            pushController =
                newPushController;

            pushReceiver =
                newPushReceiver;

            targetSelector =
                newTargetSelector;

            feedbackCanvas =
                newFeedbackCanvas;

            judgmentText =
                newJudgmentText;

            swingLine =
                newSwingLine;

            ResolveReferences();
            RefreshSwingGeometry();

            if (feedbackCanvas != null)
            {
                feedbackCanvas.enabled =
                    false;
            }

            if (swingLine != null)
            {
                swingLine.enabled =
                    false;
            }

            if (isActiveAndEnabled)
            {
                SubscribeEvents();
            }
        }

        public static string
            GetJudgmentText(
                PushAttemptResult result
            )
        {
            switch (result)
            {
                case PushAttemptResult.Success:
                    return "판정 : 명중";

                case PushAttemptResult.Miss:
                    return "판정 : 빗나감";

                case PushAttemptResult.Cooldown:
                    return "판정 : 재사용 대기";

                case PushAttemptResult.Protected:
                    return "판정 : 보호됨";

                case PushAttemptResult.MissingReceiver:
                    return "판정 : 대상 없음";

                default:
                    return "판정 : 사용 불가";
            }
        }

        public static bool ShouldPlaySwing(
            PushAttemptResult result
        )
        {
            return
                result ==
                    PushAttemptResult.Success ||
                result ==
                    PushAttemptResult.Miss ||
                result ==
                    PushAttemptResult.Protected ||
                result ==
                    PushAttemptResult.MissingReceiver;
        }

        public static string
            GetHitDirectionLabel(
                Transform targetTransform,
                Vector3 pushVelocity
            )
        {
            if (
                targetTransform == null ||
                pushVelocity.sqrMagnitude <=
                    Mathf.Epsilon
            )
            {
                return "알 수 없음";
            }

            Vector3 sourceDirection =
                -pushVelocity.normalized;

            Vector3 localDirection =
                targetTransform
                    .InverseTransformDirection(
                        sourceDirection
                    );

            if (
                Mathf.Abs(
                    localDirection.x
                ) >
                Mathf.Abs(
                    localDirection.z
                )
            )
            {
                return
                    localDirection.x >= 0f
                        ? "오른쪽"
                        : "왼쪽";
            }

            return
                localDirection.z >= 0f
                    ? "앞"
                    : "뒤";
        }

        private void HandlePushAttempted(
            PushAttemptResult result,
            PlayerFinishState target,
            Vector3 velocityChange
        )
        {
            ShowJudgment(
                GetJudgmentText(
                    result
                )
            );

            if (ShouldPlaySwing(result))
            {
                ShowSwing();
            }
        }

        private void HandlePushReceived(
            Vector3 velocityChange
        )
        {
            string direction =
                GetHitDirectionLabel(
                    transform,
                    velocityChange
                );

            ShowJudgment(
                "피격 방향 : " +
                direction
            );
        }

        private void ShowJudgment(
            string message
        )
        {
            if (judgmentText == null)
            {
                return;
            }

            judgmentText.text =
                message;

            hasTransientJudgment =
                true;

            judgmentVisibleUntil =
                Time.unscaledTime +
                Mathf.Max(
                    0.05f,
                    judgmentDuration
                );
        }

        private void UpdateReadyText()
        {
            if (
                judgmentText == null ||
                pushController == null
            )
            {
                return;
            }

            if (pushController.IsOnCooldown)
            {
                judgmentText.text =
                    "밀치기 대기 " +
                    pushController
                        .RemainingCooldown
                        .ToString("0.0") +
                    "초";

                return;
            }

            judgmentText.text =
                "밀치기 준비";
        }

        private void ShowSwing()
        {
            if (
                !ProjectJDebugOverlayController
                    .IsVisible ||
                swingLine == null
            )
            {
                return;
            }

            RefreshSwingGeometry();

            swingLine.enabled =
                true;

            swingVisibleUntil =
                Time.unscaledTime +
                Mathf.Max(
                    0.02f,
                    swingDuration
                );
        }

        private void RefreshSwingGeometry()
        {
            if (
                swingLine == null ||
                targetSelector == null
            )
            {
                return;
            }

            const int segmentCount =
                16;

            float range =
                Mathf.Max(
                    0.1f,
                    targetSelector.SearchRange
                );

            float halfAngle =
                Mathf.Clamp(
                    targetSelector.SearchAngle,
                    1f,
                    180f
                ) *
                0.5f;

            swingLine.useWorldSpace =
                false;

            swingLine.positionCount =
                segmentCount + 1;

            for (
                int i = 0;
                i <= segmentCount;
                i++
            )
            {
                float t =
                    i /
                    (float)segmentCount;

                float angle =
                    Mathf.Lerp(
                        -halfAngle,
                        halfAngle,
                        t
                    ) *
                    Mathf.Deg2Rad;

                Vector3 point =
                    new Vector3(
                        Mathf.Sin(angle) *
                            range,
                        1f,
                        Mathf.Cos(angle) *
                            range
                    );

                swingLine.SetPosition(
                    i,
                    point
                );
            }
        }

        private void ResolveReferences()
        {
            if (playerInput == null)
            {
                playerInput =
                    GetComponent<PlayerInput>();
            }

            if (pushController == null)
            {
                pushController =
                    GetComponent<
                        PlayerPushController
                    >();
            }

            if (pushReceiver == null)
            {
                pushReceiver =
                    GetComponent<
                        PlayerPushReceiver
                    >();
            }

            if (targetSelector == null)
            {
                targetSelector =
                    GetComponent<
                        PlayerPushTargetSelector
                    >();
            }
        }

        private void SubscribeEvents()
        {
            if (pushController != null)
            {
                pushController.PushAttempted -=
                    HandlePushAttempted;

                pushController.PushAttempted +=
                    HandlePushAttempted;
            }

            if (pushReceiver != null)
            {
                pushReceiver.PushReceived -=
                    HandlePushReceived;

                pushReceiver.PushReceived +=
                    HandlePushReceived;
            }
        }

        private void UnsubscribeEvents()
        {
            if (pushController != null)
            {
                pushController.PushAttempted -=
                    HandlePushAttempted;
            }

            if (pushReceiver != null)
            {
                pushReceiver.PushReceived -=
                    HandlePushReceived;
            }
        }

        private void OnValidate()
        {
            judgmentDuration =
                Mathf.Max(
                    0.05f,
                    judgmentDuration
                );

            swingDuration =
                Mathf.Max(
                    0.02f,
                    swingDuration
                );

            ResolveReferences();
            RefreshSwingGeometry();
        }
    }
}
