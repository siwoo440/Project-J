using UnityEngine;

namespace ProjectJ.Finish
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class FinishTrigger :
        MonoBehaviour
    {
        [SerializeField]
        private FinishOrderManager finishManager;

        public FinishOrderManager FinishManager
        {
            get
            {
                return finishManager;
            }
        }

        private void Awake()
        {
            Collider trigger =
                GetComponent<Collider>();

            trigger.isTrigger = true;

            ResolveManager();
        }

        private void OnTriggerEnter(
            Collider other
        )
        {
            if (other == null)
            {
                return;
            }

            PlayerFinishState player =
                other.GetComponentInParent<
                    PlayerFinishState
                >();

            TryFinish(
                player,
                Time.unscaledTimeAsDouble
            );
        }

        public void Configure(
            FinishOrderManager manager
        )
        {
            finishManager =
                manager;
        }

        public bool TryFinish(
            PlayerFinishState player,
            double timestamp
        )
        {
            ResolveManager();

            if (
                finishManager == null ||
                player == null
            )
            {
                return false;
            }

            return
                finishManager
                    .TryRegisterFinish(
                        player,
                        timestamp
                    );
        }

        private void ResolveManager()
        {
            if (finishManager != null)
            {
                return;
            }

            finishManager =
                FindFirstObjectByType<
                    FinishOrderManager
                >();
        }
    }
}
