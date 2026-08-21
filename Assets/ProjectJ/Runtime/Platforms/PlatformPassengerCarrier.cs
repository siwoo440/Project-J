using UnityEngine;

namespace ProjectJ.Platforms
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PlatformPassengerCarrier :
        MonoBehaviour
    {
        private const int MaxPassengerColliders =
            32;

        [SerializeField]
        [Min(0.05f)]
        private float probeHeight =
            0.35f;

        [SerializeField]
        [Range(0.5f, 1f)]
        private float horizontalProbeScale =
            0.92f;

        [SerializeField]
        private LayerMask playerLayers =
            1 << 8;

        private BoxCollider platformCollider;

        private readonly Collider[] overlapResults =
            new Collider[
                MaxPassengerColliders
            ];

        private readonly Rigidbody[] passengerBodies =
            new Rigidbody[
                MaxPassengerColliders
            ];

        private void Awake()
        {
            ResolveReferences();
        }

        public void Configure(
            float newProbeHeight,
            float newHorizontalProbeScale,
            LayerMask newPlayerLayers
        )
        {
            probeHeight =
                Mathf.Max(
                    0.05f,
                    newProbeHeight
                );

            horizontalProbeScale =
                Mathf.Clamp(
                    newHorizontalProbeScale,
                    0.5f,
                    1f
                );

            playerLayers =
                newPlayerLayers;

            ResolveReferences();
        }

        public void MovePassengers(
            Vector3 oldPlatformPosition,
            Quaternion oldPlatformRotation,
            Vector3 newPlatformPosition,
            Quaternion newPlatformRotation
        )
        {
            ResolveReferences();

            if (platformCollider == null)
            {
                return;
            }

            int passengerCount =
                CollectPassengers();

            for (
                int i = 0;
                i < passengerCount;
                i++
            )
            {
                Rigidbody passengerBody =
                    passengerBodies[i];

                if (
                    passengerBody == null ||
                    passengerBody.isKinematic
                )
                {
                    continue;
                }

                Vector3 newPassengerPosition =
                    CalculatePassengerPosition(
                        passengerBody.position,
                        oldPlatformPosition,
                        oldPlatformRotation,
                        newPlatformPosition,
                        newPlatformRotation
                    );

                passengerBody.MovePosition(
                    newPassengerPosition
                );
            }

            ClearPassengerCache(
                passengerCount
            );
        }

        public static Vector3
            CalculatePassengerPosition(
                Vector3 passengerPosition,
                Vector3 oldPlatformPosition,
                Quaternion oldPlatformRotation,
                Vector3 newPlatformPosition,
                Quaternion newPlatformRotation
            )
        {
            Quaternion deltaRotation =
                newPlatformRotation *
                Quaternion.Inverse(
                    oldPlatformRotation
                );

            Vector3 relativePosition =
                passengerPosition -
                oldPlatformPosition;

            return
                newPlatformPosition +
                deltaRotation *
                relativePosition;
        }

        private int CollectPassengers()
        {
            Vector3 worldCenter =
                transform.TransformPoint(
                    platformCollider.center
                );

            Vector3 lossyScale =
                transform.lossyScale;

            Vector3 scaledSize =
                new Vector3(
                    Mathf.Abs(
                        platformCollider.size.x *
                        lossyScale.x
                    ),
                    Mathf.Abs(
                        platformCollider.size.y *
                        lossyScale.y
                    ),
                    Mathf.Abs(
                        platformCollider.size.z *
                        lossyScale.z
                    )
                );

            Vector3 halfExtents =
                scaledSize *
                0.5f;

            Vector3 probeCenter =
                worldCenter +
                transform.up *
                (
                    halfExtents.y +
                    probeHeight *
                    0.5f
                );

            Vector3 probeHalfExtents =
                new Vector3(
                    halfExtents.x *
                        horizontalProbeScale,
                    probeHeight *
                        0.5f,
                    halfExtents.z *
                        horizontalProbeScale
                );

            int hitCount =
                Physics.OverlapBoxNonAlloc(
                    probeCenter,
                    probeHalfExtents,
                    overlapResults,
                    transform.rotation,
                    playerLayers,
                    QueryTriggerInteraction.Ignore
                );

            int passengerCount =
                0;

            for (
                int i = 0;
                i < hitCount;
                i++
            )
            {
                Collider hit =
                    overlapResults[i];

                overlapResults[i] =
                    null;

                if (hit == null)
                {
                    continue;
                }

                Rigidbody body =
                    hit.attachedRigidbody;

                if (
                    body == null ||
                    ContainsBody(
                        passengerCount,
                        body
                    )
                )
                {
                    continue;
                }

                passengerBodies[
                    passengerCount
                ] =
                    body;

                passengerCount++;

                if (
                    passengerCount >=
                    passengerBodies.Length
                )
                {
                    break;
                }
            }

            return passengerCount;
        }

        private bool ContainsBody(
            int passengerCount,
            Rigidbody body
        )
        {
            for (
                int i = 0;
                i < passengerCount;
                i++
            )
            {
                if (
                    passengerBodies[i] ==
                    body
                )
                {
                    return true;
                }
            }

            return false;
        }

        private void ClearPassengerCache(
            int passengerCount
        )
        {
            for (
                int i = 0;
                i < passengerCount;
                i++
            )
            {
                passengerBodies[i] =
                    null;
            }
        }

        private void ResolveReferences()
        {
            if (platformCollider == null)
            {
                platformCollider =
                    GetComponent<
                        BoxCollider
                    >();
            }
        }

        private void OnValidate()
        {
            probeHeight =
                Mathf.Max(
                    0.05f,
                    probeHeight
                );

            horizontalProbeScale =
                Mathf.Clamp(
                    horizontalProbeScale,
                    0.5f,
                    1f
                );
        }
    }
}
