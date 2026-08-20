using System;
using UnityEngine;

namespace ProjectJ.Checkpoint
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(Rigidbody)
    )]
    [RequireComponent(
        typeof(PlayerCheckpointTracker)
    )]
    [RequireComponent(
        typeof(PlayerFallTracker)
    )]
    public sealed class PlayerRespawnController :
        MonoBehaviour
    {
        [SerializeField]
        private Rigidbody body;

        [SerializeField]
        private PlayerCheckpointTracker
            checkpointTracker;

        [SerializeField]
        private PlayerFallTracker
            fallTracker;

        [SerializeField]
        private int respawnCount;

        private bool isSubscribed;
        private bool isRespawning;

        public event Action<CheckpointId>
            Respawned;

        public int RespawnCount
        {
            get
            {
                return respawnCount;
            }
        }

        public bool IsRespawning
        {
            get
            {
                return isRespawning;
            }
        }

        public Rigidbody Body
        {
            get
            {
                return body;
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

        public PlayerFallTracker FallTracker
        {
            get
            {
                return fallTracker;
            }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeToFall();
        }

        private void OnDisable()
        {
            UnsubscribeFromFall();
        }

        public void Configure(
            Rigidbody targetBody,
            PlayerCheckpointTracker tracker,
            PlayerFallTracker targetFallTracker
        )
        {
            UnsubscribeFromFall();

            body = targetBody;
            checkpointTracker = tracker;
            fallTracker = targetFallTracker;

            ResolveReferences();

            if (isActiveAndEnabled)
            {
                SubscribeToFall();
            }
        }

        public bool RequestRespawn()
        {
            return RespawnToSavedPoint();
        }

        public bool RespawnToSavedPoint()
        {
            if (isRespawning)
            {
                return false;
            }

            ResolveReferences();

            if (
                body == null ||
                checkpointTracker == null
            )
            {
                return false;
            }

            isRespawning = true;

            Vector3 targetPosition =
                checkpointTracker
                    .RespawnPosition;

            Quaternion targetRotation =
                checkpointTracker
                    .RespawnRotation;

            ClearRigidbodyMotion();

            body.position =
                targetPosition;

            body.rotation =
                targetRotation;

            Physics.SyncTransforms();

            if (fallTracker != null)
            {
                fallTracker
                    .ResetFallenState();
            }

            respawnCount++;

            CheckpointId respawnCheckpoint =
                checkpointTracker
                    .CurrentCheckpointId;

            isRespawning = false;

            Respawned?.Invoke(
                respawnCheckpoint
            );

            return true;
        }

        public void ClearRigidbodyMotion()
        {
            ResolveReferences();

            if (body == null)
            {
                return;
            }

            body.linearVelocity =
                Vector3.zero;

            body.angularVelocity =
                Vector3.zero;
        }

        private void HandleFell()
        {
            RespawnToSavedPoint();
        }

        private void ResolveReferences()
        {
            if (body == null)
            {
                body =
                    GetComponent<Rigidbody>();
            }

            if (checkpointTracker == null)
            {
                checkpointTracker =
                    GetComponent<
                        PlayerCheckpointTracker
                    >();
            }

            if (fallTracker == null)
            {
                fallTracker =
                    GetComponent<
                        PlayerFallTracker
                    >();
            }
        }

        private void SubscribeToFall()
        {
            if (
                isSubscribed ||
                fallTracker == null
            )
            {
                return;
            }

            fallTracker.Fell +=
                HandleFell;

            isSubscribed = true;
        }

        private void UnsubscribeFromFall()
        {
            if (
                !isSubscribed ||
                fallTracker == null
            )
            {
                isSubscribed = false;
                return;
            }

            fallTracker.Fell -=
                HandleFell;

            isSubscribed = false;
        }
    }
}
