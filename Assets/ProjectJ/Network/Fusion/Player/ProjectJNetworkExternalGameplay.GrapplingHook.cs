using Fusion; // NetworkBool 사용
using ProjectJ.Items; // 갈고리 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkExternalGameplay
    {
        [Networked]
        private NetworkBool NetworkGrapplingHookVelocityActive
        {
            get;
            set;
        }

        internal bool TrySetGrapplingHookVelocityAuthority(
            Vector3 desiredVelocity
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                !GameplayInputAllowed
            )
            {
                return false;
            }

            if (desiredVelocity.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            desiredVelocity =
                desiredVelocity.normalized *
                ProjectJGrapplingHookPolicy.PullSpeedMetersPerSecond;

            if (!NetworkGrapplingHookVelocityActive)
            {
                NetworkExternalForceApplyCount++;
            }

            NetworkGrapplingHookVelocityActive = true;
            NetworkExternalVelocity = desiredVelocity;
            NetworkLastExternalForceSource =
                (int)ProjectJExternalForceSource.Item;

            return true;
        }

        internal void ClearGrapplingHookVelocityAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                !NetworkGrapplingHookVelocityActive
            )
            {
                return;
            }

            NetworkGrapplingHookVelocityActive = false;
            NetworkExternalVelocity = Vector3.zero;
        }
    }
}
