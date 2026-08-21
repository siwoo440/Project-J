using UnityEngine;

namespace ProjectJ.Platforms
{
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(
        typeof(PlatformPassengerCarrier)
    )]
    public sealed class RotatingPlatform :
        MonoBehaviour
    {
        [SerializeField]
        private Vector3 worldAxis =
            Vector3.up;

        [SerializeField]
        private float degreesPerSecond =
            35f;

        private Rigidbody body;
        private PlatformPassengerCarrier
            passengerCarrier;

        private void Awake()
        {
            ResolveReferences();

            if (body != null)
            {
                body.isKinematic =
                    true;

                body.useGravity =
                    false;
            }
        }

        private void FixedUpdate()
        {
            if (body == null)
            {
                return;
            }

            Vector3 oldPosition =
                body.position;

            Quaternion oldRotation =
                body.rotation;

            Quaternion nextRotation =
                CalculateNextRotation(
                    oldRotation,
                    worldAxis,
                    degreesPerSecond,
                    Time.fixedDeltaTime
                );

            passengerCarrier
                .MovePassengers(
                    oldPosition,
                    oldRotation,
                    oldPosition,
                    nextRotation
                );

            body.MoveRotation(
                nextRotation
            );
        }

        public void Configure(
            Vector3 newWorldAxis,
            float newDegreesPerSecond
        )
        {
            worldAxis =
                newWorldAxis.sqrMagnitude >
                    Mathf.Epsilon
                    ? newWorldAxis
                    : Vector3.up;

            degreesPerSecond =
                newDegreesPerSecond;

            ResolveReferences();
        }

        public static Quaternion
            CalculateNextRotation(
                Quaternion currentRotation,
                Vector3 axis,
                float speedDegreesPerSecond,
                float deltaTime
            )
        {
            Vector3 safeAxis =
                axis.sqrMagnitude >
                    Mathf.Epsilon
                    ? axis.normalized
                    : Vector3.up;

            Quaternion deltaRotation =
                Quaternion.AngleAxis(
                    speedDegreesPerSecond *
                    Mathf.Max(
                        0f,
                        deltaTime
                    ),
                    safeAxis
                );

            return
                deltaRotation *
                currentRotation;
        }

        private void ResolveReferences()
        {
            if (body == null)
            {
                body =
                    GetComponent<Rigidbody>();
            }

            if (passengerCarrier == null)
            {
                passengerCarrier =
                    GetComponent<
                        PlatformPassengerCarrier
                    >();
            }
        }
    }
}
