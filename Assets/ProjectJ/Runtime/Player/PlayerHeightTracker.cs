using System;
using UnityEngine;

namespace ProjectJ.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerHeightTracker : MonoBehaviour
    {
        public const int HeightUnitsPerMeter = 100;

        [SerializeField]
        private Transform heightReferenceFoot;

        [SerializeField]
        private float rawHeight;

        [SerializeField]
        private int currentHeightCentimeters;

        [SerializeField]
        private int highestHeightCentimeters;

        private CapsuleCollider capsuleCollider;
        private bool highestHeightInitialized;

        public Transform HeightReferenceFoot
        {
            get
            {
                return heightReferenceFoot;
            }
        }

        public float RawHeight
        {
            get
            {
                return rawHeight;
            }
        }

        public int CurrentHeightCentimeters
        {
            get
            {
                return currentHeightCentimeters;
            }
        }

        public float CurrentHeight
        {
            get
            {
                return CentimetersToMeters(
                    currentHeightCentimeters
                );
            }
        }

        public int HighestHeightCentimeters
        {
            get
            {
                return highestHeightCentimeters;
            }
        }

        public float HighestHeight
        {
            get
            {
                return CentimetersToMeters(
                    highestHeightCentimeters
                );
            }
        }

        private void Awake()
        {
            capsuleCollider =
                GetComponent<CapsuleCollider>();

            TryFindHeightReference();

            ResetTracking();
        }

        private void Update()
        {
            RefreshHeight();
        }

        public void Configure(
            Transform newHeightReferenceFoot
        )
        {
            heightReferenceFoot =
                newHeightReferenceFoot;

            if (capsuleCollider == null)
            {
                capsuleCollider =
                    GetComponent<CapsuleCollider>();
            }
        }

        public void ResetTracking()
        {
            highestHeightInitialized = false;

            RefreshHeight();
        }

        public void RefreshHeight()
        {
            rawHeight =
                GetFootWorldHeight();

            currentHeightCentimeters =
                TruncateToCentimeters(
                    rawHeight
                );

            if (!highestHeightInitialized)
            {
                highestHeightCentimeters =
                    currentHeightCentimeters;

                highestHeightInitialized = true;
                return;
            }

            if (
                currentHeightCentimeters >
                highestHeightCentimeters
            )
            {
                highestHeightCentimeters =
                    currentHeightCentimeters;
            }
        }

        private float GetFootWorldHeight()
        {
            if (heightReferenceFoot != null)
            {
                return
                    heightReferenceFoot.position.y;
            }

            if (capsuleCollider != null)
            {
                Vector3 localFootPosition =
                    CalculateCapsuleFootLocalPosition(
                        capsuleCollider.center,
                        capsuleCollider.height,
                        capsuleCollider.direction
                    );

                return
                    transform.TransformPoint(
                        localFootPosition
                    ).y;
            }

            return transform.position.y;
        }

        private void TryFindHeightReference()
        {
            if (heightReferenceFoot != null)
            {
                return;
            }

            Transform found =
                transform.Find(
                    "HeightReference_Foot"
                );

            if (found != null)
            {
                heightReferenceFoot = found;
            }
        }

        public static int TruncateToCentimeters(
            float heightMeters
        )
        {
            double scaled =
                (double)heightMeters *
                HeightUnitsPerMeter;

            double truncated =
                Math.Truncate(
                    scaled
                );

            if (truncated >= int.MaxValue)
            {
                return int.MaxValue;
            }

            if (truncated <= int.MinValue)
            {
                return int.MinValue;
            }

            return (int)truncated;
        }

        public static float TruncateToTwoDecimals(
            float heightMeters
        )
        {
            return CentimetersToMeters(
                TruncateToCentimeters(
                    heightMeters
                )
            );
        }

        public static float CentimetersToMeters(
            int centimeters
        )
        {
            return
                centimeters /
                (float)HeightUnitsPerMeter;
        }

        public static Vector3 CalculateCapsuleFootLocalPosition(
            Vector3 capsuleCenter,
            float capsuleHeight,
            int capsuleDirection
        )
        {
            float halfHeight =
                Mathf.Max(
                    0f,
                    capsuleHeight
                ) *
                0.5f;

            Vector3 axis;

            switch (capsuleDirection)
            {
                case 0:
                    axis = Vector3.right;
                    break;

                case 2:
                    axis = Vector3.forward;
                    break;

                default:
                    axis = Vector3.up;
                    break;
            }

            return
                capsuleCenter -
                axis *
                halfHeight;
        }
    }
}
