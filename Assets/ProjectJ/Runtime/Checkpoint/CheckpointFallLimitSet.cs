using UnityEngine;

namespace ProjectJ.Checkpoint
{
    [DisallowMultipleComponent]
    public sealed class CheckpointFallLimitSet :
        MonoBehaviour
    {
        [SerializeField]
        private float startFallLimitY = -20f;

        [SerializeField]
        private float cp1FallLimitY = 180f;

        [SerializeField]
        private float cp2FallLimitY = 380f;

        [SerializeField]
        private float cp3FallLimitY = 580f;

        [SerializeField]
        private float cp4FallLimitY = 780f;

        public float StartFallLimitY
        {
            get
            {
                return startFallLimitY;
            }
        }

        public float Cp1FallLimitY
        {
            get
            {
                return cp1FallLimitY;
            }
        }

        public float Cp2FallLimitY
        {
            get
            {
                return cp2FallLimitY;
            }
        }

        public float Cp3FallLimitY
        {
            get
            {
                return cp3FallLimitY;
            }
        }

        public float Cp4FallLimitY
        {
            get
            {
                return cp4FallLimitY;
            }
        }

        public float GetFallLimitY(
            CheckpointId checkpointId
        )
        {
            switch (checkpointId)
            {
                case CheckpointId.CP1:
                    return cp1FallLimitY;

                case CheckpointId.CP2:
                    return cp2FallLimitY;

                case CheckpointId.CP3:
                    return cp3FallLimitY;

                case CheckpointId.CP4:
                    return cp4FallLimitY;

                default:
                    return startFallLimitY;
            }
        }

        public void Configure(
            float start,
            float cp1,
            float cp2,
            float cp3,
            float cp4
        )
        {
            startFallLimitY = start;
            cp1FallLimitY = cp1;
            cp2FallLimitY = cp2;
            cp3FallLimitY = cp3;
            cp4FallLimitY = cp4;
        }

        public bool HasAscendingLimits()
        {
            return
                startFallLimitY <
                cp1FallLimitY &&
                cp1FallLimitY <
                cp2FallLimitY &&
                cp2FallLimitY <
                cp3FallLimitY &&
                cp3FallLimitY <
                cp4FallLimitY;
        }
    }
}
