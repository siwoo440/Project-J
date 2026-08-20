using System;
using UnityEngine;

namespace ProjectJ.Checkpoint
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(PlayerCheckpointTracker)
    )]
    public sealed class PlayerFallTracker :
        MonoBehaviour
    {
        [SerializeField]
        private PlayerCheckpointTracker
            checkpointTracker;

        [SerializeField]
        private CheckpointFallLimitSet
            fallLimitSet;

        [SerializeField]
        private bool isFallen;

        [SerializeField]
        private float activeFallLimitY;

        public event Action Fell;

        public bool IsFallen
        {
            get
            {
                return isFallen;
            }
        }

        public float ActiveFallLimitY
        {
            get
            {
                return activeFallLimitY;
            }
        }

        public PlayerCheckpointTracker
            CheckpointTracker
        {
            get
            {
                return checkpointTracker;
            }
        }

        public CheckpointFallLimitSet
            FallLimitSet
        {
            get
            {
                return fallLimitSet;
            }
        }

        private void Awake()
        {
            ResolveReferences();
            RefreshActiveFallLimit();
        }

        private void Update()
        {
            EvaluateCurrentPosition();
        }

        public void Configure(
            PlayerCheckpointTracker tracker,
            CheckpointFallLimitSet limits
        )
        {
            checkpointTracker = tracker;
            fallLimitSet = limits;

            RefreshActiveFallLimit();
        }

        public bool EvaluateCurrentPosition()
        {
            return EvaluateHeight(
                transform.position.y
            );
        }

        public bool EvaluateHeight(
            float playerWorldY
        )
        {
            ResolveReferences();

            if (
                checkpointTracker == null ||
                fallLimitSet == null
            )
            {
                return false;
            }

            RefreshActiveFallLimit();

            if (isFallen)
            {
                return false;
            }

            if (
                playerWorldY >=
                activeFallLimitY
            )
            {
                return false;
            }

            isFallen = true;

            Fell?.Invoke();

            return true;
        }

        public void ResetFallenState()
        {
            isFallen = false;

            RefreshActiveFallLimit();
        }

        public void RefreshActiveFallLimit()
        {
            ResolveReferences();

            if (
                checkpointTracker == null ||
                fallLimitSet == null
            )
            {
                return;
            }

            activeFallLimitY =
                fallLimitSet.GetFallLimitY(
                    checkpointTracker
                        .CurrentCheckpointId
                );
        }

        private void ResolveReferences()
        {
            if (checkpointTracker == null)
            {
                checkpointTracker =
                    GetComponent<
                        PlayerCheckpointTracker
                    >();
            }

            if (fallLimitSet == null)
            {
                fallLimitSet =
                    FindFirstObjectByType<
                        CheckpointFallLimitSet
                    >();
            }
        }
    }
}
