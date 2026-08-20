using System;
using UnityEngine;

namespace ProjectJ.Checkpoint
{
    [DisallowMultipleComponent]
    public sealed class PlayerCheckpointTracker :
        MonoBehaviour
    {
        [SerializeField]
        private CheckpointId currentCheckpointId =
            CheckpointId.Start;

        [SerializeField]
        private Checkpoint currentCheckpoint;

        [SerializeField]
        private Vector3 respawnPosition;

        [SerializeField]
        private Quaternion respawnRotation =
            Quaternion.identity;

        public event Action<CheckpointId>
            CheckpointChanged;

        public CheckpointId CurrentCheckpointId
        {
            get
            {
                return currentCheckpointId;
            }
        }

        public Checkpoint CurrentCheckpoint
        {
            get
            {
                return currentCheckpoint;
            }
        }

        public Vector3 RespawnPosition
        {
            get
            {
                return respawnPosition;
            }
        }

        public Quaternion RespawnRotation
        {
            get
            {
                return respawnRotation;
            }
        }

        private void Awake()
        {
            if (
                currentCheckpointId ==
                CheckpointId.Start &&
                currentCheckpoint == null
            )
            {
                CaptureStartPoint();
            }
        }

        public bool ActivateCheckpoint(
            Checkpoint checkpoint
        )
        {
            if (checkpoint == null)
            {
                return false;
            }

            currentCheckpoint =
                checkpoint;

            currentCheckpointId =
                checkpoint.Id;

            respawnPosition =
                checkpoint.RespawnPosition;

            respawnRotation =
                checkpoint.RespawnRotation;

            CheckpointChanged?.Invoke(
                currentCheckpointId
            );

            return true;
        }

        public void CaptureStartPoint()
        {
            currentCheckpointId =
                CheckpointId.Start;

            currentCheckpoint = null;

            respawnPosition =
                transform.position;

            respawnRotation =
                transform.rotation;
        }
    }
}
