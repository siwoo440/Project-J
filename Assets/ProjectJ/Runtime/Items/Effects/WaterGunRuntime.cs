using System;
using ProjectJ.Push;
using UnityEngine;

namespace ProjectJ.Items.Effects
{
    [DisallowMultipleComponent]
    public sealed class WaterGunRuntime :
        MonoBehaviour,
        IItemUseReleaseHandler
    {
        private const float Range = 12f;
        private const float CastRadius = 0.3f;
        private const float TickInterval = 0.1f;
        private const float ForcePerTick = 0.55f;

        private bool active;
        private float nextTickTime;

        public ItemDefinition Definition
        {
            get;
            private set;
        }

        public bool IsActive =>
            active;

        public void Begin(
            ItemDefinition definition
        )
        {
            Definition = definition;
            active = true;
            nextTickTime = Time.time;
        }

        public void OnUseReleased()
        {
            active = false;
            Destroy(this);
        }

        private void OnDisable()
        {
            active = false;
        }

        private void Update()
        {
            if (
                !active ||
                Time.time < nextTickTime
            )
            {
                return;
            }

            nextTickTime =
                Time.time +
                TickInterval;

            ApplyWaterForce();
        }

        private void ApplyWaterForce()
        {
            Vector3 forward =
                transform.forward;

            Vector3 origin =
                transform.position +
                Vector3.up * 1.2f +
                forward * 0.4f;

            RaycastHit[] hits =
                Physics.SphereCastAll(
                    origin,
                    CastRadius,
                    forward,
                    Range,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore
                );

            Array.Sort(
                hits,
                (
                    left,
                    right
                ) =>
                    left.distance.CompareTo(
                        right.distance
                    )
            );

            for (
                int i = 0;
                i < hits.Length;
                i++
            )
            {
                Collider hitCollider =
                    hits[i].collider;

                if (hitCollider == null)
                {
                    continue;
                }

                if (
                    hitCollider.transform ==
                        transform ||
                    hitCollider.transform
                        .IsChildOf(transform)
                )
                {
                    continue;
                }

                PlayerExternalForceReceiver receiver =
                    hitCollider
                        .GetComponentInParent<
                            PlayerExternalForceReceiver
                        >();

                if (receiver != null)
                {
                    if (
                        receiver.gameObject !=
                        gameObject
                    )
                    {
                        receiver.TryApplyVelocityChange(
                            ExternalForceSource.Item,
                            forward *
                            ForcePerTick
                        );
                    }

                    return;
                }

                if (!hitCollider.isTrigger)
                {
                    return;
                }
            }
        }
    }
}
