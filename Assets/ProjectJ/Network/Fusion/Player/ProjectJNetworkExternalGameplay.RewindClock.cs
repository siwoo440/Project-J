using ProjectJ.Items; // 되감기 시계 안전 위치 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkExternalGameplay
    {
        internal bool IsRewindTargetSafeAuthority(
            Vector3 position
        )
        {
            ResolveReferences();

            float fallLimitY =
                fallLimitSet != null
                    ? fallLimitSet.GetFallLimitY(
                        CurrentCheckpointId
                    )
                    : float.NegativeInfinity;

            return
                ProjectJRewindClockPolicy.IsTargetSafe(
                    position.y,
                    fallLimitY,
                    ProjectJRewindClockPolicy.IsFinitePosition(
                        position
                    )
                );
        }

        internal void BeginRewindSuppressionAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            NetworkExternalVelocity =
                Vector3.zero;

            NetworkLastExternalForceSource =
                (int)ProjectJExternalForceSource.None;
        }

        internal void MaintainRewindSuppressionAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            NetworkExternalVelocity =
                Vector3.zero;

            NetworkLastExternalForceSource =
                (int)ProjectJExternalForceSource.None;
        }
    }
}
