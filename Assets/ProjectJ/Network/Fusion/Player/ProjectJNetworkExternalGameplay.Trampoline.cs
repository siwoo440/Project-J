using ProjectJ.Items; // 트램폴린 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkExternalGameplay
    {
        internal bool TrySetTrampolineLaunchAuthority(
            float launchSpeed
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                !GameplayInputAllowed ||
                launchSpeed <= 0f
            )
            {
                return false;
            }

            NetworkExternalVelocity =
                ProjectJTrampolinePolicy.ResolveLaunchVelocity(
                    NetworkExternalVelocity,
                    launchSpeed
                );

            NetworkLastExternalForceSource =
                (int)ProjectJExternalForceSource.Item;

            NetworkExternalForceApplyCount++;

            return true;
        }
    }
}
