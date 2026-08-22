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

            MonoBehaviour[] behaviours =
                other.GetComponentsInParent<MonoBehaviour>(
                    true
                ); // 부모 계층 FINISH 수신자 조회

            for (int index = 0; index < behaviours.Length; index++)
            {
                if (behaviours[index] is IFinishReceiver receiver)
                {
                    receiver.ReceiveFinish(); // 네트워크 또는 공통 FINISH 처리 전달
                    return; // 공통 수신자가 처리한 대상은 로컬 처리 차단
                }
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
