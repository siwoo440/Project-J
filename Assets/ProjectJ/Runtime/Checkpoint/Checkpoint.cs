using UnityEngine;

namespace ProjectJ.Checkpoint
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class Checkpoint : MonoBehaviour
    {
        [SerializeField]
        private CheckpointId checkpointId =
            CheckpointId.CP1;

        [SerializeField]
        private Transform respawnPoint;

        public CheckpointId Id
        {
            get
            {
                return checkpointId;
            }
        }

        public Vector3 RespawnPosition
        {
            get
            {
                if (respawnPoint != null)
                {
                    return respawnPoint.position;
                }

                return transform.position;
            }
        }

        public Quaternion RespawnRotation
        {
            get
            {
                if (respawnPoint != null)
                {
                    return respawnPoint.rotation;
                }

                return transform.rotation;
            }
        }

        private void Awake()
        {
            Collider trigger =
                GetComponent<Collider>();

            if (trigger != null)
            {
                trigger.isTrigger = true;
            }
        }

        private void OnTriggerEnter(
            Collider other
        )
        {
            if (other == null)
            {
                return;
            }

            PlayerCheckpointTracker tracker =
                other.GetComponentInParent<
                    PlayerCheckpointTracker
                >();

            if (tracker == null)
            {
                return;
            }

            tracker.ActivateCheckpoint(
                this
            );
        }

        public void Configure(
            CheckpointId id,
            Transform targetRespawnPoint
        )
        {
            checkpointId = id;
            respawnPoint =
                targetRespawnPoint;
        }
    }
}
