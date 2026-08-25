using System.Collections.Generic; // 대상 검색 재사용 List
using Fusion; // Networked와 TickTimer 사용
using ProjectJ.Items; // 낚시대 정책 사용
using UnityEngine; // Raycast와 Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        private readonly RaycastHit[] fishingRodHitBuffer =
            new RaycastHit[32];

        private readonly List<ProjectJNetworkExternalGameplay> fishingRodTargetBuffer =
            new List<ProjectJNetworkExternalGameplay>(8);

        [Networked]
        private TickTimer NetworkFishingRodTimer
        {
            get;
            set;
        }

        [Networked]
        private int NetworkFishingRodTargetIndex
        {
            get;
            set;
        }

        public bool IsFishingRodActive =>
            NetworkFishingRodTargetIndex >= 0 &&
            IsTimerActive(NetworkFishingRodTimer);

        public float FishingRodRemaining =>
            GetRemainingTime(NetworkFishingRodTimer);

        public int FishingRodTargetIndex =>
            NetworkFishingRodTargetIndex;

        private void InitializeFishingRodAuthority()
        {
            NetworkFishingRodTimer = TickTimer.None;
            NetworkFishingRodTargetIndex = -1;
        }

        private bool UseFishingRodAuthority()
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

            ClearFishingRodAuthority();

            if (
                !TryFindFishingRodTargetAuthority(
                    out ProjectJNetworkExternalGameplay target
                )
            )
            {
                return true; // 빗나가거나 벽에 막혀도 사용 자체는 소비
            }

            if (
                target == null ||
                target.Object == null ||
                !target.Object.IsValid ||
                !target.CanReceiveFishingRodPullAuthority(
                    Object.InputAuthority
                )
            )
            {
                return true; // 보호 상태 Target 적중은 연결 없이 소비
            }

            NetworkFishingRodTargetIndex =
                target.Object.InputAuthority.AsIndex;

            NetworkFishingRodTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    ProjectJFishingRodPolicy.PullDurationSeconds
                );

            return true;
        }

        private void UpdateFishingRodAuthority()
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

            if (!IsFishingRodActive)
            {
                if (NetworkFishingRodTargetIndex >= 0)
                {
                    ClearFishingRodAuthority();
                }

                return;
            }

            if (
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed
            )
            {
                ClearFishingRodAuthority();
                return;
            }

            ProjectJNetworkExternalGameplay target =
                ResolveFishingRodTargetAuthority();

            if (
                target == null ||
                target.Object == null ||
                !target.Object.IsValid ||
                !target.CanReceiveFishingRodPullAuthority(
                    Object.InputAuthority
                )
            )
            {
                ClearFishingRodAuthority();
                return;
            }

            Vector3 sourcePoint =
                transform.position +
                Vector3.up * 1.1f;

            Vector3 targetPoint =
                target.transform.position +
                Vector3.up * 1.1f;

            float distanceMeters =
                Vector3.Distance(
                    sourcePoint,
                    targetPoint
                );

            bool lineClear =
                IsFishingRodLineClearAuthority(
                    target,
                    sourcePoint,
                    targetPoint
                );

            if (
                !ProjectJFishingRodPolicy.CanMaintainConnection(
                    true,
                    externalGameplay.GameplayInputAllowed,
                    distanceMeters,
                    lineClear
                )
            )
            {
                ClearFishingRodAuthority();
                return;
            }

            Vector3 pullVelocity =
                ProjectJFishingRodPolicy.CalculatePullVelocity(
                    transform.position,
                    target.transform.position
                );

            if (
                !target.TrySetFishingRodPullVelocityAuthority(
                    Object.InputAuthority,
                    pullVelocity
                )
            )
            {
                ClearFishingRodAuthority();
                return;
            }

            Debug.DrawLine(
                sourcePoint,
                targetPoint,
                Color.cyan,
                Runner.DeltaTime
            );
        }

        private bool TryFindFishingRodTargetAuthority(
            out ProjectJNetworkExternalGameplay target
        )
        {
            target = null;

            Vector3 forward = transform.forward;
            forward.y = 0f;

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
                fishingRodHitBuffer,
                ProjectJFishingRodPolicy.MaximumRangeMeters,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            );

            float nearestDistance = float.MaxValue;
            ProjectJNetworkExternalGameplay nearestTarget = null;
            bool nearestHitIsBlocker = false;

            for (int index = 0; index < hitCount; index++)
            {
                Collider hitCollider =
                    fishingRodHitBuffer[index].collider;

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

                float hitDistance =
                    fishingRodHitBuffer[index].distance;

                if (hitDistance >= nearestDistance)
                {
                    continue;
                }

                ProjectJNetworkExternalGameplay candidate =
                    hitCollider.GetComponentInParent<ProjectJNetworkExternalGameplay>();

                if (candidate == externalGameplay)
                {
                    continue;
                }

                nearestDistance = hitDistance;
                nearestTarget = candidate;
                nearestHitIsBlocker = candidate == null;
            }

            if (
                nearestHitIsBlocker ||
                nearestTarget == null
            )
            {
                return false;
            }

            target = nearestTarget;
            return true;
        }

        private ProjectJNetworkExternalGameplay ResolveFishingRodTargetAuthority()
        {
            if (
                Runner == null ||
                NetworkFishingRodTargetIndex < 0
            )
            {
                return null;
            }

            ProjectJNetworkExternalGameplay.CollectActivePlayers(
                Runner,
                fishingRodTargetBuffer
            );

            for (
                int index = 0;
                index < fishingRodTargetBuffer.Count;
                index++
            )
            {
                ProjectJNetworkExternalGameplay candidate =
                    fishingRodTargetBuffer[index];

                if (
                    candidate == null ||
                    candidate.Object == null ||
                    !candidate.Object.IsValid
                )
                {
                    continue;
                }

                if (
                    candidate.Object.InputAuthority.AsIndex ==
                    NetworkFishingRodTargetIndex
                )
                {
                    return candidate;
                }
            }

            return null;
        }

        private bool IsFishingRodLineClearAuthority(
            ProjectJNetworkExternalGameplay target,
            Vector3 sourcePoint,
            Vector3 targetPoint
        )
        {
            Vector3 direction =
                targetPoint - sourcePoint;

            float distance =
                direction.magnitude;

            if (distance <= 0.01f)
            {
                return true;
            }

            direction /= distance;

            int hitCount = Physics.RaycastNonAlloc(
                sourcePoint,
                direction,
                fishingRodHitBuffer,
                distance,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            );

            for (int index = 0; index < hitCount; index++)
            {
                Collider hitCollider =
                    fishingRodHitBuffer[index].collider;

                if (hitCollider == null)
                {
                    continue;
                }

                if (
                    hitCollider.transform == transform ||
                    hitCollider.transform.IsChildOf(transform) ||
                    hitCollider.transform == target.transform ||
                    hitCollider.transform.IsChildOf(target.transform)
                )
                {
                    continue;
                }

                ProjectJNetworkExternalGameplay player =
                    hitCollider.GetComponentInParent<ProjectJNetworkExternalGameplay>();

                if (player != null)
                {
                    continue; // 벽 차폐만 연결을 끊고 다른 Player는 무시
                }

                return false;
            }

            return true;
        }

        private void ClearFishingRodAuthority()
        {
            NetworkFishingRodTimer = TickTimer.None;
            NetworkFishingRodTargetIndex = -1;
        }
    }
}
