using System;
using UnityEngine;

namespace ProjectJ.Checkpoint
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(PlayerRespawnController)
    )]
    public sealed class PlayerRespawnProtection :
        MonoBehaviour
    {
        [SerializeField]
        [Min(0f)]
        private float protectionDuration = 3f;

        [SerializeField]
        private PlayerRespawnController
            respawnController;

        [SerializeField]
        private bool isProtected;

        [SerializeField]
        private float remainingProtectionTime;

        private double protectionEndsAt;
        private bool isSubscribed;

        public event Action ProtectionStarted;
        public event Action ProtectionEnded;

        public bool IsProtected
        {
            get
            {
                return isProtected;
            }
        }

        public float ProtectionDuration
        {
            get
            {
                return protectionDuration;
            }
        }

        public float RemainingProtectionTime
        {
            get
            {
                return remainingProtectionTime;
            }
        }

        public bool CanReceiveHostileEffect
        {
            get
            {
                return !isProtected;
            }
        }

        public PlayerRespawnController
            RespawnController
        {
            get
            {
                return respawnController;
            }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeToRespawn();
        }

        private void OnDisable()
        {
            UnsubscribeFromRespawn();
        }

        private void Update()
        {
            EvaluateProtectionAt(
                Time.unscaledTimeAsDouble
            );
        }

        public void Configure(
            PlayerRespawnController controller,
            float duration
        )
        {
            UnsubscribeFromRespawn();

            respawnController = controller;
            protectionDuration =
                Mathf.Max(
                    0f,
                    duration
                );

            ResolveReferences();

            if (isActiveAndEnabled)
            {
                SubscribeToRespawn();
            }
        }

        public void StartProtection()
        {
            StartProtectionAt(
                Time.unscaledTimeAsDouble
            );
        }

        public void StartProtectionAt(
            double currentTime
        )
        {
            protectionDuration =
                Mathf.Max(
                    0f,
                    protectionDuration
                );

            if (protectionDuration <= 0f)
            {
                EndProtection();
                return;
            }

            protectionEndsAt =
                currentTime +
                protectionDuration;

            isProtected = true;
            remainingProtectionTime =
                protectionDuration;

            ProtectionStarted?.Invoke();
        }

        public bool EvaluateProtectionAt(
            double currentTime
        )
        {
            if (!isProtected)
            {
                remainingProtectionTime = 0f;
                return false;
            }

            double remaining =
                protectionEndsAt -
                currentTime;

            if (remaining <= 0d)
            {
                EndProtection();
                return false;
            }

            remainingProtectionTime =
                Mathf.Max(
                    0f,
                    (float)remaining
                );

            return true;
        }

        public bool TryAcceptHostileEffect()
        {
            return CanReceiveHostileEffect;
        }

        public void EndProtection()
        {
            if (!isProtected)
            {
                remainingProtectionTime = 0f;
                return;
            }

            isProtected = false;
            remainingProtectionTime = 0f;
            protectionEndsAt = 0d;

            ProtectionEnded?.Invoke();
        }

        private void HandleRespawned(
            CheckpointId checkpointId
        )
        {
            StartProtection();
        }

        private void ResolveReferences()
        {
            if (respawnController == null)
            {
                respawnController =
                    GetComponent<
                        PlayerRespawnController
                    >();
            }
        }

        private void SubscribeToRespawn()
        {
            if (
                isSubscribed ||
                respawnController == null
            )
            {
                return;
            }

            respawnController.Respawned +=
                HandleRespawned;

            isSubscribed = true;
        }

        private void UnsubscribeFromRespawn()
        {
            if (
                !isSubscribed ||
                respawnController == null
            )
            {
                isSubscribed = false;
                return;
            }

            respawnController.Respawned -=
                HandleRespawned;

            isSubscribed = false;
        }
    }
}
