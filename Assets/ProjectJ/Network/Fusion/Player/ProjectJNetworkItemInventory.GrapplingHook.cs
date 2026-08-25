using Fusion; // Networked와 TickTimer 사용
using ProjectJ.Items; // 갈고리 정책 사용
using UnityEngine; // Raycast와 Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        private readonly RaycastHit[] grapplingHookHitBuffer =
            new RaycastHit[32];

        [Networked]
        private NetworkBool NetworkGrapplingHookActive
        {
            get;
            set;
        }

        [Networked]
        private TickTimer NetworkGrapplingHookTimer
        {
            get;
            set;
        }

        [Networked]
        private Vector3 NetworkGrapplingHookAnchor
        {
            get;
            set;
        }

        public bool IsGrapplingHookActive =>
            NetworkGrapplingHookActive &&
            IsTimerActive(NetworkGrapplingHookTimer);

        public float GrapplingHookRemaining =>
            GetRemainingTime(NetworkGrapplingHookTimer);

        public Vector3 GrapplingHookAnchor =>
            NetworkGrapplingHookAnchor;

        private void InitializeGrapplingHookAuthority()
        {
            NetworkGrapplingHookActive = false;
            NetworkGrapplingHookTimer = TickTimer.None;
            NetworkGrapplingHookAnchor = Vector3.zero;
        }

        private bool UseGrapplingHookAuthority()
        {
            if (
                Runner == null ||
                !Runner.IsServer ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed
            )
            {
                return false;
            }

            ClearGrapplingHookAuthority();

            if (
                !TryFindGrapplingHookAnchorAuthority(
                    out Vector3 anchor
                )
            )
            {
                return false; // 유효 표면을 잡지 못하면 아이템 유지
            }

            NetworkGrapplingHookAnchor = anchor;
            NetworkGrapplingHookActive = true;
            NetworkGrapplingHookTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    ProjectJGrapplingHookPolicy.DurationSeconds
                );

            return true;
        }

        private void UpdateGrapplingHookAuthority()
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            if (!NetworkGrapplingHookActive)
            {
                return;
            }

            float distanceToAnchor =
                Vector3.Distance(
                    transform.position,
                    NetworkGrapplingHookAnchor
                );

            bool timerActive =
                IsTimerActive(NetworkGrapplingHookTimer);

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            if (
                !ProjectJGrapplingHookPolicy.CanMaintainConnection(
                    timerActive,
                    gameplayAllowed,
                    distanceToAnchor
                )
            )
            {
                ClearGrapplingHookAuthority();
                return;
            }

            Vector3 pullVelocity =
                ProjectJGrapplingHookPolicy.CalculatePullVelocity(
                    transform.position,
                    NetworkGrapplingHookAnchor
                );

            if (pullVelocity.sqrMagnitude <= 0.0001f)
            {
                ClearGrapplingHookAuthority();
                return;
            }

            Vector3 direction =
                pullVelocity.normalized;

            float sweepDistance =
                ProjectJGrapplingHookPolicy.PullSpeedMetersPerSecond *
                Runner.DeltaTime;

            if (
                IsGrapplingHookSweepBlockedAuthority(
                    direction,
                    sweepDistance
                )
            )
            {
                ClearGrapplingHookAuthority();
                return;
            }

            if (
                externalGameplay == null ||
                !externalGameplay.TrySetGrapplingHookVelocityAuthority(
                    pullVelocity
                )
            )
            {
                ClearGrapplingHookAuthority();
                return;
            }

            Debug.DrawLine(
                transform.position + Vector3.up,
                NetworkGrapplingHookAnchor,
                Color.yellow,
                Runner.DeltaTime
            );
        }

        private bool TryFindGrapplingHookAnchorAuthority(
            out Vector3 anchor
        )
        {
            anchor = Vector3.zero;

            Vector3 forward =
                transform.forward;

            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();

            Vector3 origin =
                transform.position +
                Vector3.up * 1.2f +
                forward * 0.35f;

            int hitCount = Physics.RaycastNonAlloc(
                origin,
                forward,
                grapplingHookHitBuffer,
                ProjectJGrapplingHookPolicy.MaximumRangeMeters,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            );

            int nearestIndex = -1;
            float nearestDistance = float.MaxValue;

            for (int index = 0; index < hitCount; index++)
            {
                Collider hitCollider =
                    grapplingHookHitBuffer[index].collider;

                if (hitCollider == null)
                {
                    continue;
                }

                if (
                    hitCollider.transform == transform ||
                    hitCollider.transform.IsChildOf(transform)
                )
                {
                    continue;
                }

                if (
                    grapplingHookHitBuffer[index].distance >=
                    nearestDistance
                )
                {
                    continue;
                }

                nearestIndex = index;
                nearestDistance =
                    grapplingHookHitBuffer[index].distance;
            }

            if (nearestIndex < 0)
            {
                return false;
            }

            Collider nearestCollider =
                grapplingHookHitBuffer[nearestIndex].collider;

            bool isGrappleSurface =
                HasGrappleSurfaceTag(
                    nearestCollider
                );

            if (
                !ProjectJGrapplingHookPolicy.CanActivate(
                    Runner != null &&
                    Object != null &&
                    Object.IsValid &&
                    Object.HasStateAuthority,
                    externalGameplay != null &&
                    externalGameplay.GameplayInputAllowed,
                    isGrappleSurface,
                    nearestDistance
                )
            )
            {
                return false;
            }

            anchor =
                grapplingHookHitBuffer[nearestIndex].point;

            return true;
        }

        private bool IsGrapplingHookSweepBlockedAuthority(
            Vector3 direction,
            float sweepDistance
        )
        {
            Vector3 origin =
                transform.position +
                Vector3.up * 0.9f;

            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                ProjectJGrapplingHookPolicy.SweepRadiusMeters,
                direction,
                grapplingHookHitBuffer,
                sweepDistance + 0.05f,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            );

            for (int index = 0; index < hitCount; index++)
            {
                Collider hitCollider =
                    grapplingHookHitBuffer[index].collider;

                if (hitCollider == null)
                {
                    continue;
                }

                if (
                    hitCollider.transform == transform ||
                    hitCollider.transform.IsChildOf(transform)
                )
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private bool HasGrappleSurfaceTag(
            Collider hitCollider
        )
        {
            if (hitCollider == null)
            {
                return false;
            }

            Transform current =
                hitCollider.transform;

            while (
                current != null &&
                current != transform
            )
            {
                if (
                    current.gameObject.tag ==
                    ProjectJGrapplingHookPolicy.GrappleSurfaceTag
                )
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private void ClearGrapplingHookAuthority()
        {
            NetworkGrapplingHookActive = false;
            NetworkGrapplingHookTimer = TickTimer.None;
            NetworkGrapplingHookAnchor = Vector3.zero;

            if (externalGameplay != null)
            {
                externalGameplay.ClearGrapplingHookVelocityAuthority();
            }
        }
    }
}
