using UnityEngine;

namespace ProjectJ.Platforms
{
    [DisallowMultipleComponent]
    public sealed class IceSurface :
        MonoBehaviour
    {
        [SerializeField]
        [Min(0f)]
        private float acceleration =
            6f;

        [SerializeField]
        [Min(0f)]
        private float deceleration =
            2.5f;

        [SerializeField]
        [Min(0f)]
        private float turnAcceleration =
            3f;

        public float Acceleration
        {
            get
            {
                return acceleration;
            }
        }

        public float Deceleration
        {
            get
            {
                return deceleration;
            }
        }

        public float TurnAcceleration
        {
            get
            {
                return turnAcceleration;
            }
        }

        public void Configure(
            float newAcceleration,
            float newDeceleration,
            float newTurnAcceleration
        )
        {
            acceleration =
                Mathf.Max(
                    0f,
                    newAcceleration
                );

            deceleration =
                Mathf.Max(
                    0f,
                    newDeceleration
                );

            turnAcceleration =
                Mathf.Max(
                    0f,
                    newTurnAcceleration
                );
        }

        public float SelectChangeRate(
            Vector3 previousVelocity,
            Vector3 desiredVelocity
        )
        {
            Vector3 previous =
                new Vector3(
                    previousVelocity.x,
                    0f,
                    previousVelocity.z
                );

            Vector3 desired =
                new Vector3(
                    desiredVelocity.x,
                    0f,
                    desiredVelocity.z
                );

            if (
                desired.sqrMagnitude <=
                0.0001f
            )
            {
                return deceleration;
            }

            if (
                previous.sqrMagnitude <=
                0.0001f
            )
            {
                return acceleration;
            }

            float directionDot =
                Vector3.Dot(
                    previous.normalized,
                    desired.normalized
                );

            if (directionDot < 0.5f)
            {
                return
                    turnAcceleration;
            }

            if (
                desired.sqrMagnitude >
                previous.sqrMagnitude
            )
            {
                return acceleration;
            }

            return deceleration;
        }

        private void OnValidate()
        {
            acceleration =
                Mathf.Max(
                    0f,
                    acceleration
                );

            deceleration =
                Mathf.Max(
                    0f,
                    deceleration
                );

            turnAcceleration =
                Mathf.Max(
                    0f,
                    turnAcceleration
                );
        }
    }
}
