using Fusion; // NetworkBehaviour와 PlayerRef 사용
using ProjectJ.Items; // 비눗방울 정책 사용
using UnityEngine; // 물리 판정과 Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public sealed class ProjectJNetworkSoapBubbleProjectile :
        NetworkBehaviour
    {
        private readonly RaycastHit[] hitBuffer =
            new RaycastHit[24];

        [Networked]
        private NetworkBool NetworkInitialized
        {
            get;
            set;
        }

        [Networked]
        private PlayerRef NetworkOwner
        {
            get;
            set;
        }

        [Networked]
        private Vector3 NetworkDirection
        {
            get;
            set;
        }

        [Networked]
        private float NetworkTravelledDistance
        {
            get;
            set;
        }

        public PlayerRef Owner =>
            NetworkOwner;

        public float TravelledDistance =>
            NetworkTravelledDistance;

        public bool ConfigureAuthority(
            PlayerRef owner,
            Vector3 direction
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return false;
            }

            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            NetworkOwner = owner;
            NetworkDirection = direction.normalized;
            NetworkTravelledDistance = 0f;
            NetworkInitialized = true;

            return true;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
            {
                return;
            }

            if (!NetworkInitialized)
            {
                DespawnAuthority();
                return;
            }

            if (
                ProjectJSoapBubblePolicy.HasReachedTravelLimit(
                    NetworkTravelledDistance
                )
            )
            {
                DespawnAuthority();
                return;
            }

            float remainingDistance =
                ProjectJSoapBubblePolicy.MaximumTravelDistance -
                NetworkTravelledDistance;

            float stepDistance = Mathf.Min(
                ProjectJSoapBubblePolicy.ProjectileSpeed *
                Runner.DeltaTime,
                remainingDistance
            );

            if (TryResolveCollision(stepDistance))
            {
                DespawnAuthority();
                return;
            }

            transform.position +=
                NetworkDirection * stepDistance;
            NetworkTravelledDistance +=
                stepDistance;

            if (
                ProjectJSoapBubblePolicy.HasReachedTravelLimit(
                    NetworkTravelledDistance
                )
            )
            {
                DespawnAuthority();
            }
        }

        private bool TryResolveCollision(
            float stepDistance
        )
        {
            int hitCount =
                Physics.SphereCastNonAlloc(
                    transform.position,
                    ProjectJSoapBubblePolicy.CollisionRadius,
                    NetworkDirection,
                    hitBuffer,
                    stepDistance,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore
                );

            int nearestIndex = -1;
            float nearestDistance =
                float.MaxValue;

            for (int index = 0; index < hitCount; index++)
            {
                Collider hitCollider =
                    hitBuffer[index].collider;

                if (hitCollider == null)
                {
                    continue;
                }

                ProjectJNetworkExternalGameplay target =
                    hitCollider.GetComponentInParent<ProjectJNetworkExternalGameplay>();

                if (
                    target != null &&
                    target.Object != null &&
                    target.Object.IsValid &&
                    target.Object.InputAuthority ==
                    NetworkOwner
                )
                {
                    continue;
                }

                if (
                    hitBuffer[index].distance >=
                    nearestDistance
                )
                {
                    continue;
                }

                nearestIndex = index;
                nearestDistance =
                    hitBuffer[index].distance;
            }

            if (nearestIndex < 0)
            {
                return false;
            }

            Collider nearestCollider =
                hitBuffer[nearestIndex].collider;

            ProjectJNetworkExternalGameplay nearestTarget =
                nearestCollider.GetComponentInParent<ProjectJNetworkExternalGameplay>();

            ProjectJNetworkItemInventory mirrorInventory =
                nearestTarget != null
                    ? nearestTarget.GetComponent<ProjectJNetworkItemInventory>()
                    : null;

            if (
                mirrorInventory != null &&
                mirrorInventory.TryReflectHandMirrorProjectileAuthority(
                    NetworkOwner,
                    NetworkDirection,
                    out PlayerRef reflectedOwner,
                    out Vector3 reflectedDirection
                )
            )
            {
                NetworkOwner =
                    reflectedOwner;

                NetworkDirection =
                    reflectedDirection;

                transform.position =
                    ProjectJHandMirrorPolicy.ResolveSeparatedPosition(
                        hitBuffer[nearestIndex].point,
                        reflectedDirection
                    );

                return false;
            }

            if (nearestTarget != null)
            {
                TryApplySoapBubbleAuthority(
                    nearestTarget
                );
            }

            return true;
        }

        private void TryApplySoapBubbleAuthority(
            ProjectJNetworkExternalGameplay target
        )
        {
            if (
                target == null ||
                target.Object == null ||
                !target.Object.IsValid
            )
            {
                return;
            }

            bool runnerReady =
                target.Runner != null &&
                target.Object.HasStateAuthority;

            bool isOwner =
                target.Object.InputAuthority ==
                NetworkOwner;

            bool canAffect =
                ProjectJSoapBubblePolicy.CanAffectTarget(
                    runnerReady,
                    target.GameplayInputAllowed,
                    isOwner,
                    target.IsFinished,
                    target.IsRespawnProtected
                );

            if (!canAffect)
            {
                return;
            }

            ProjectJNetworkItemInventory targetInventory =
                target.GetComponent<ProjectJNetworkItemInventory>();

            if (targetInventory == null)
            {
                return;
            }

            if (targetInventory.IsRewindActive) // 되감기 중 비눗방울 상태 차단
            {
                return;
            }

            targetInventory.ApplySoapBubbleAuthority();
        }

        private void DespawnAuthority()
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid
            )
            {
                return;
            }

            Runner.Despawn(Object);
        }
    }
}
