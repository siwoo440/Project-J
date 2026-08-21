using ProjectJ.Push;
using UnityEngine;

namespace ProjectJ.Items.Effects
{
    [DisallowMultipleComponent]
    public sealed class BananaCushionRuntime :
        MonoBehaviour
    {
        private const float Lifetime = 15f;
        private const float SlipForce = 6.5f;

        private GameObject owner;
        private bool triggered;

        public void Initialize(
            GameObject newOwner
        )
        {
            owner = newOwner;
            Destroy(
                gameObject,
                Lifetime
            );
        }

        private void OnTriggerEnter(
            Collider other
        )
        {
            if (
                triggered ||
                other == null
            )
            {
                return;
            }

            PlayerExternalForceReceiver receiver =
                other.GetComponentInParent<
                    PlayerExternalForceReceiver
                >();

            if (
                receiver == null ||
                receiver.gameObject == owner
            )
            {
                return;
            }

            triggered = true;

            float sideSign =
                receiver.GetInstanceID() % 2 == 0
                    ? 1f
                    : -1f;

            Vector3 slipDirection =
                (
                    receiver.transform.right *
                    sideSign -
                    receiver.transform.forward *
                    0.35f
                ).normalized;

            receiver.TryApplyVelocityChange(
                ExternalForceSource.Item,
                slipDirection * SlipForce
            );

            Destroy(gameObject);
        }
    }
}
