using UnityEngine;

namespace ProjectJ.Platforms
{
    [DisallowMultipleComponent]
    public sealed class SpringPlatform :
        MonoBehaviour
    {
        [SerializeField]
        [Min(1f)]
        private float jumpMultiplier =
            1.5f;

        public float JumpMultiplier
        {
            get
            {
                return jumpMultiplier;
            }
        }

        public void Configure(
            float newJumpMultiplier
        )
        {
            jumpMultiplier =
                Mathf.Max(
                    1f,
                    newJumpMultiplier
                );
        }

        public float GetBoostedJumpVelocity(
            float baseJumpVelocity
        )
        {
            return
                Mathf.Max(
                    0f,
                    baseJumpVelocity
                ) *
                jumpMultiplier;
        }

        private void OnValidate()
        {
            jumpMultiplier =
                Mathf.Max(
                    1f,
                    jumpMultiplier
                );
        }
    }
}
