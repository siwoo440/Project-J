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
    public sealed class MovingPlatform :
        MonoBehaviour
    {
        [SerializeField]
        private Transform pointA;

        [SerializeField]
        private Transform pointB;

        [SerializeField]
        [Min(0.01f)]
        private float moveSpeed =
            2.5f;

        [SerializeField]
        private bool moveTowardB =
            true;

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
            if (
                body == null ||
                pointA == null ||
                pointB == null
            )
            {
                return;
            }

            Vector3 target =
                moveTowardB
                    ? pointB.position
                    : pointA.position;

            Vector3 oldPosition =
                body.position;

            Quaternion oldRotation =
                body.rotation;

            Vector3 nextPosition =
                CalculateNextPosition(
                    oldPosition,
                    target,
                    moveSpeed,
                    Time.fixedDeltaTime
                );

            passengerCarrier
                .MovePassengers(
                    oldPosition,
                    oldRotation,
                    nextPosition,
                    oldRotation
                );

            body.MovePosition(
                nextPosition
            );

            if (
                Vector3.SqrMagnitude(
                    nextPosition -
                    target
                ) <= 0.0001f
            )
            {
                moveTowardB =
                    !moveTowardB;
            }
        }

        public void Configure(
            Transform newPointA,
            Transform newPointB,
            float newMoveSpeed
        )
        {
            pointA =
                newPointA;

            pointB =
                newPointB;

            moveSpeed =
                Mathf.Max(
                    0.01f,
                    newMoveSpeed
                );

            ResolveReferences();
        }

        public static Vector3
            CalculateNextPosition(
                Vector3 currentPosition,
                Vector3 targetPosition,
                float speed,
                float deltaTime
            )
        {
            return
                Vector3.MoveTowards(
                    currentPosition,
                    targetPosition,
                    Mathf.Max(
                        0f,
                        speed
                    ) *
                    Mathf.Max(
                        0f,
                        deltaTime
                    )
                );
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

        private void OnValidate()
        {
            moveSpeed =
                Mathf.Max(
                    0.01f,
                    moveSpeed
                );
        }
    }
}
