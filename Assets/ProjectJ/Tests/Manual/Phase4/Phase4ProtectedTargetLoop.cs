using ProjectJ.Checkpoint;
using UnityEngine;

namespace ProjectJ.Tests.Manual
{
    [DisallowMultipleComponent]
    public sealed class Phase4ProtectedTargetLoop :
        MonoBehaviour
    {
        [SerializeField]
        private PlayerRespawnProtection
            protection;

        [SerializeField]
        private bool keepProtected =
            true;

        private void OnEnable()
        {
            ResolveReference();

            if (
                keepProtected &&
                protection != null
            )
            {
                protection.StartProtection();
            }
        }

        private void Update()
        {
            if (!keepProtected)
            {
                return;
            }

            ResolveReference();

            if (
                protection != null &&
                !protection.IsProtected
            )
            {
                protection.StartProtection();
            }
        }

        public void Configure(
            PlayerRespawnProtection
                targetProtection
        )
        {
            protection =
                targetProtection;

            ResolveReference();
        }

        private void ResolveReference()
        {
            if (protection == null)
            {
                protection =
                    GetComponent<
                        PlayerRespawnProtection
                    >();
            }
        }
    }
}
