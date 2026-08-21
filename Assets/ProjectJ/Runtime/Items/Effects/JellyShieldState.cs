using ProjectJ.Push;
using UnityEngine;

namespace ProjectJ.Items.Effects
{
    [DisallowMultipleComponent]
    public sealed class JellyShieldState :
        MonoBehaviour
    {
        private float activeUntil;

        public bool IsActive =>
            Time.time < activeUntil;

        public void Activate(float duration)
        {
            activeUntil =
                Mathf.Max(
                    activeUntil,
                    Time.time + Mathf.Max(0.1f, duration)
                );
        }

        public bool Blocks(
            ExternalForceSource source
        )
        {
            if (!IsActive)
            {
                return false;
            }

            return
                source == ExternalForceSource.Push ||
                source == ExternalForceSource.Item;
        }
    }
}
