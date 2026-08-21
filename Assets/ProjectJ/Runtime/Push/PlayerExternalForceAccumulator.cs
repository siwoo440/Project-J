using UnityEngine;

namespace ProjectJ.Push
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(Rigidbody)
    )]
    public sealed class PlayerExternalForceAccumulator :
        MonoBehaviour
    {
        [SerializeField]
        private Rigidbody body;

        [SerializeField]
        [Min(0f)]
        private float horizontalDecay =
            12f;

        [SerializeField]
        [Min(0f)]
        private float stopThreshold =
            0.05f;

        [SerializeField]
        private Vector3 currentExternalVelocity;

        public Rigidbody Body
        {
            get
            {
                return body;
            }
        }

        public float HorizontalDecay
        {
            get
            {
                return horizontalDecay;
            }
        }

        public float StopThreshold
        {
            get
            {
                return stopThreshold;
            }
        }

        public Vector3 CurrentExternalVelocity
        {
            get
            {
                return currentExternalVelocity;
            }
        }

        public bool HasExternalVelocity
        {
            get
            {
                return
                    currentExternalVelocity
                        .sqrMagnitude >
                    stopThreshold *
                    stopThreshold;
            }
        }

        private void Awake()
        {
            ResolveReferences();
            SanitizeValues();
        }

        private void FixedUpdate()
        {
            ApplyDecayStep(
                Time.fixedDeltaTime
            );
        }

        public void Configure(
            Rigidbody newBody,
            float newHorizontalDecay,
            float newStopThreshold
        )
        {
            body =
                newBody;

            horizontalDecay =
                Mathf.Max(
                    0f,
                    newHorizontalDecay
                );

            stopThreshold =
                Mathf.Max(
                    0f,
                    newStopThreshold
                );

            ResolveReferences();
            SanitizeValues();
        }

        public bool AddVelocityChange(
            Vector3 velocityChange
        )
        {
            ResolveReferences();

            if (
                body == null ||
                body.isKinematic
            )
            {
                return false;
            }

            Vector3 horizontalChange =
                new Vector3(
                    velocityChange.x,
                    0f,
                    velocityChange.z
                );

            if (
                horizontalChange.sqrMagnitude <=
                Mathf.Epsilon
            )
            {
                return true;
            }

            currentExternalVelocity +=
                horizontalChange;

            Vector3 currentVelocity =
                body.linearVelocity;

            body.linearVelocity =
                new Vector3(
                    currentVelocity.x +
                        horizontalChange.x,
                    currentVelocity.y,
                    currentVelocity.z +
                        horizontalChange.z
                );

            if (
                currentExternalVelocity
                    .sqrMagnitude <=
                stopThreshold *
                stopThreshold
            )
            {
                currentExternalVelocity =
                    Vector3.zero;
            }

            return true;
        }

        public Vector3 ApplyDecayStep(
            float deltaTime
        )
        {
            ResolveReferences();
            SanitizeValues();

            if (body == null)
            {
                currentExternalVelocity =
                    Vector3.zero;

                return Vector3.zero;
            }

            if (
                currentExternalVelocity
                    .sqrMagnitude <=
                stopThreshold *
                stopThreshold
            )
            {
                currentExternalVelocity =
                    Vector3.zero;

                return Vector3.zero;
            }

            Vector3 previousExternalVelocity =
                currentExternalVelocity;

            Vector3 nextExternalVelocity =
                Vector3.MoveTowards(
                    previousExternalVelocity,
                    Vector3.zero,
                    horizontalDecay *
                    Mathf.Max(
                        0f,
                        deltaTime
                    )
                );

            if (
                nextExternalVelocity
                    .sqrMagnitude <=
                stopThreshold *
                stopThreshold
            )
            {
                nextExternalVelocity =
                    Vector3.zero;
            }

            Vector3 decayDirection =
                previousExternalVelocity.normalized;

            float previousMagnitude =
                previousExternalVelocity.magnitude;

            float nextMagnitude =
                nextExternalVelocity.magnitude;

            float requestedReduction =
                Mathf.Max(
                    0f,
                    previousMagnitude -
                    nextMagnitude
                );

            Vector3 currentBodyVelocity =
                body.linearVelocity;

            Vector3 currentHorizontalVelocity =
                new Vector3(
                    currentBodyVelocity.x,
                    0f,
                    currentBodyVelocity.z
                );

            float alignedSpeed =
                Vector3.Dot(
                    currentHorizontalVelocity,
                    decayDirection
                );

            float actualReduction =
                Mathf.Min(
                    requestedReduction,
                    Mathf.Max(
                        0f,
                        alignedSpeed
                    )
                );

            Vector3 appliedCorrection =
                -decayDirection *
                actualReduction;

            body.linearVelocity =
                new Vector3(
                    currentBodyVelocity.x +
                        appliedCorrection.x,
                    currentBodyVelocity.y,
                    currentBodyVelocity.z +
                        appliedCorrection.z
                );

            currentExternalVelocity =
                nextExternalVelocity;

            return appliedCorrection;
        }

        public void ClearExternalVelocity(
            bool removeTrackedVelocityFromBody
        )
        {
            ResolveReferences();

            if (
                removeTrackedVelocityFromBody &&
                body != null &&
                currentExternalVelocity
                    .sqrMagnitude >
                    Mathf.Epsilon
            )
            {
                Vector3 bodyVelocity =
                    body.linearVelocity;

                Vector3 externalDirection =
                    currentExternalVelocity
                        .normalized;

                Vector3 horizontalBodyVelocity =
                    new Vector3(
                        bodyVelocity.x,
                        0f,
                        bodyVelocity.z
                    );

                float alignedSpeed =
                    Mathf.Max(
                        0f,
                        Vector3.Dot(
                            horizontalBodyVelocity,
                            externalDirection
                        )
                    );

                float removableSpeed =
                    Mathf.Min(
                        alignedSpeed,
                        currentExternalVelocity
                            .magnitude
                    );

                Vector3 correction =
                    -externalDirection *
                    removableSpeed;

                body.linearVelocity =
                    new Vector3(
                        bodyVelocity.x +
                            correction.x,
                        bodyVelocity.y,
                        bodyVelocity.z +
                            correction.z
                    );
            }

            currentExternalVelocity =
                Vector3.zero;
        }

        private void ResolveReferences()
        {
            if (body == null)
            {
                body =
                    GetComponent<Rigidbody>();
            }
        }

        private void SanitizeValues()
        {
            horizontalDecay =
                Mathf.Max(
                    0f,
                    horizontalDecay
                );

            stopThreshold =
                Mathf.Max(
                    0f,
                    stopThreshold
                );

            currentExternalVelocity.y =
                0f;
        }

        private void OnValidate()
        {
            SanitizeValues();
        }
    }
}
