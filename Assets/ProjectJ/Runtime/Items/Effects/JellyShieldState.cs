using ProjectJ.Push;
using UnityEngine;

namespace ProjectJ.Items.Effects
{
    [DisallowMultipleComponent]
    public sealed class JellyShieldState :
        MonoBehaviour
    {
        private float activeUntil;

        public ItemDefinition Definition
        {
            get;
            private set;
        }

        public bool IsActive =>
            Time.time < activeUntil;

        public float RemainingTime =>
            Mathf.Max(
                0f,
                activeUntil - Time.time
            );

        public void Activate(
            float duration,
            ItemDefinition definition
        )
        {
            Definition = definition;

            activeUntil =
                Mathf.Max(
                    activeUntil,
                    Time.time +
                    Mathf.Max(
                        0.1f,
                        duration
                    )
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
                source ==
                    ExternalForceSource.Push ||
                source ==
                    ExternalForceSource.Item;
        }
    }
}
