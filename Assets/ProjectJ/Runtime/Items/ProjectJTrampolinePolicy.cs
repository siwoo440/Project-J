using UnityEngine; // Mathf와 Vector3 사용

namespace ProjectJ.Items
{
    public static class ProjectJTrampolinePolicy
    {
        public const float LifetimeSeconds = 12f; // 최대 설치 유지 시간
        public const int MaximumUseCount = 3; // 최대 도약 횟수

        public const float FirstLaunchSpeed = 7f; // 첫 번째 도약
        public const float SecondLaunchSpeed = 9f; // 두 번째 도약
        public const float ThirdLaunchSpeed = 11f; // 세 번째 도약

        public const float InstallRayStartHeight = 0.5f; // 발밑 Ray 시작 높이
        public const float InstallRayDistance = 2.5f; // 발밑 설치 탐색 거리
        public const float MinimumGroundNormalY = 0.65f; // 설치 가능한 최소 바닥 Normal Y

        public const float ActivationRadius = 0.9f; // Owner 발동 수평 반경
        public const float ActivationMinVerticalOffset = -0.25f; // 발 위치 하한
        public const float ActivationMaxVerticalOffset = 0.75f; // 발 위치 상한

        public static bool CanInstall(
            bool runnerReady,
            bool gameplayAllowed
        )
        {
            return
                runnerReady &&
                gameplayAllowed;
        }

        public static bool IsValidInstallSurface(
            float normalY,
            float distance
        )
        {
            return
                normalY >= MinimumGroundNormalY &&
                distance >= 0f &&
                distance <= InstallRayDistance;
        }

        public static float GetLaunchSpeed(
            int useCount
        )
        {
            int safeCount =
                Mathf.Max(0, useCount);

            switch (safeCount)
            {
                case 0:
                    return FirstLaunchSpeed;

                case 1:
                    return SecondLaunchSpeed;

                case 2:
                    return ThirdLaunchSpeed;

                default:
                    return 0f;
            }
        }

        public static int GetNextUseCount(
            int useCount
        )
        {
            return Mathf.Clamp(
                Mathf.Max(0, useCount) + 1,
                0,
                MaximumUseCount
            );
        }

        public static bool HasConsumedAllUses(
            int useCount
        )
        {
            return
                useCount >=
                MaximumUseCount;
        }

        public static bool IsWithinActivationArea(
            float horizontalDistance,
            float verticalOffset
        )
        {
            return
                horizontalDistance >= 0f &&
                horizontalDistance <= ActivationRadius &&
                verticalOffset >= ActivationMinVerticalOffset &&
                verticalOffset <= ActivationMaxVerticalOffset;
        }

        public static bool CanActivateOwner(
            bool ownerValid,
            bool gameplayAllowed,
            int useCount,
            bool grounded,
            float verticalVelocity,
            float horizontalDistance,
            float verticalOffset
        )
        {
            bool landingOrGrounded =
                grounded ||
                verticalVelocity <= 0f;

            return
                ownerValid &&
                gameplayAllowed &&
                !HasConsumedAllUses(useCount) &&
                landingOrGrounded &&
                IsWithinActivationArea(
                    horizontalDistance,
                    verticalOffset
                );
        }

        public static bool ShouldDespawn(
            bool lifetimeActive,
            bool ownerMissing,
            int useCount
        )
        {
            return
                !lifetimeActive ||
                ownerMissing ||
                HasConsumedAllUses(useCount);
        }

        public static Vector3 ResolveLaunchVelocity(
            Vector3 currentExternalVelocity,
            float launchSpeed
        )
        {
            Vector3 result =
                currentExternalVelocity;

            result.y =
                Mathf.Max(0f, launchSpeed);

            return result;
        }
    }
}
