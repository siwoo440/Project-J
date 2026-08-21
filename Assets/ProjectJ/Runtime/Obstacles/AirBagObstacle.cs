using System;
using ProjectJ.Push;
using UnityEngine;

namespace ProjectJ.Obstacles
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(BoxCollider)
    )]
    public sealed class AirBagObstacle :
        MonoBehaviour
    {
        [SerializeField]
        [Min(0f)]
        private float horizontalVelocityChange =
            12f;

        [SerializeField]
        private Vector3 localPushDirection =
            Vector3.forward;

        [SerializeField]
        [Range(0f, 1f)]
        private float contactSpread =
            0.35f;

        public event Action<
            PlayerExternalForceReceiver,
            Vector3
        > AirBagTriggered;

        public float HorizontalVelocityChange
        {
            get
            {
                return horizontalVelocityChange;
            }
        }

        public Vector3 LocalPushDirection
        {
            get
            {
                return localPushDirection;
            }
        }

        public float ContactSpread
        {
            get
            {
                return contactSpread;
            }
        }

        public void Configure(
            float newHorizontalVelocityChange,
            Vector3 newLocalPushDirection,
            float newContactSpread
        )
        {
            horizontalVelocityChange =
                Mathf.Max(
                    0f,
                    newHorizontalVelocityChange
                );

            localPushDirection =
                newLocalPushDirection
                    .sqrMagnitude >
                    Mathf.Epsilon
                    ? newLocalPushDirection
                    : Vector3.forward;

            contactSpread =
                Mathf.Clamp01(
                    newContactSpread
                );
        }

        private void OnCollisionEnter(
            Collision collision
        )
        {
            Rigidbody otherBody =
                collision.rigidbody;

            if (otherBody == null)
            {
                return;
            }

            PlayerExternalForceReceiver receiver =
                otherBody.GetComponent<
                    PlayerExternalForceReceiver
                >();

            if (receiver == null)
            {
                return;
            }

            Vector3 contactPoint =
                otherBody.worldCenterOfMass;

            if (collision.contactCount > 0)
            {
                contactPoint =
                    collision
                        .GetContact(0)
                        .point;
            }

            Vector3 pushDirection =
                CalculatePushDirection(
                    transform,
                    contactPoint,
                    localPushDirection,
                    contactSpread
                );

            Vector3 velocityChange =
                pushDirection *
                horizontalVelocityChange;

            bool applied =
                receiver.TryApplyVelocityChange(
                    ExternalForceSource.AirBag,
                    velocityChange
                );

            if (applied)
            {
                AirBagTriggered?.Invoke(
                    receiver,
                    velocityChange
                );
            }
        }

        public static Vector3
            CalculatePushDirection(
                Transform airBagTransform,
                Vector3 contactPoint,
                Vector3 localDirection,
                float spread
            )
        {
            if (airBagTransform == null)
            {
                return Vector3.forward;
            }

            Vector3 safeLocalDirection =
                localDirection.sqrMagnitude >
                    Mathf.Epsilon
                    ? localDirection.normalized
                    : Vector3.forward;

            Vector3 baseDirection =
                airBagTransform
                    .TransformDirection(
                        safeLocalDirection
                    );

            baseDirection.y =
                0f;

            if (
                baseDirection.sqrMagnitude <=
                Mathf.Epsilon
            )
            {
                baseDirection =
                    airBagTransform.forward;

                baseDirection.y =
                    0f;
            }

            baseDirection.Normalize();

            Vector3 centerToContact =
                contactPoint -
                airBagTransform.position;

            centerToContact.y =
                0f;

            Vector3 lateralOffset =
                centerToContact -
                Vector3.Project(
                    centerToContact,
                    baseDirection
                );

            Vector3 lateralDirection =
                lateralOffset.sqrMagnitude >
                    Mathf.Epsilon
                    ? lateralOffset.normalized
                    : Vector3.zero;

            Vector3 combinedDirection =
                baseDirection +
                lateralDirection *
                Mathf.Clamp01(
                    spread
                );

            combinedDirection.y =
                0f;

            if (
                combinedDirection
                    .sqrMagnitude <=
                Mathf.Epsilon
            )
            {
                return baseDirection;
            }

            return
                combinedDirection.normalized;
        }

        private void OnValidate()
        {
            horizontalVelocityChange =
                Mathf.Max(
                    0f,
                    horizontalVelocityChange
                );

            if (
                localPushDirection
                    .sqrMagnitude <=
                Mathf.Epsilon
            )
            {
                localPushDirection =
                    Vector3.forward;
            }

            contactSpread =
                Mathf.Clamp01(
                    contactSpread
                );
        }
    }
}
