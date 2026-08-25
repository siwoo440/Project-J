using Fusion; // PlayerRef 사용
using ProjectJ.Items; // 낚시대 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkExternalGameplay
    {
        internal bool CanReceiveFishingRodPullAuthority(
            PlayerRef sourceOwner
        )
        {
            ResolveReferences();

            bool runnerReady =
                Runner != null &&
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority;

            bool isOwner =
                Object != null &&
                Object.IsValid &&
                Object.InputAuthority == sourceOwner;

            bool isShielded =
                itemInventory != null &&
                itemInventory.BlocksExternalForce(
                    ProjectJExternalForceSource.Item
                );

            return ProjectJFishingRodPolicy.CanAffectTarget(
                runnerReady,
                GameplayInputAllowed,
                isOwner,
                IsFinished,
                IsRespawnProtected,
                isShielded
            );
        }

        internal bool TrySetFishingRodPullVelocityAuthority(
            PlayerRef sourceOwner,
            Vector3 pullVelocity
        )
        {
            if (!CanReceiveFishingRodPullAuthority(sourceOwner))
            {
                return false;
            }

            pullVelocity.y = 0f;

            if (pullVelocity.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            pullVelocity =
                pullVelocity.normalized *
                ProjectJFishingRodPolicy.PullSpeedMetersPerSecond;

            NetworkExternalVelocity = pullVelocity;
            NetworkLastExternalForceSource =
                (int)ProjectJExternalForceSource.Item;
            NetworkExternalForceApplyCount++;

            return true;
        }
    }
}
