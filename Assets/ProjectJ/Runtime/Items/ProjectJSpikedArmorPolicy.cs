using UnityEngine; // Mathf와 Vector3 사용

namespace ProjectJ.Items
{
    public static class ProjectJSpikedArmorPolicy
    {
        public const float DurationSeconds = 5f; // 가시 갑옷 지속 시간
        public const float DetectionRadius = 1.2f; // 접촉 감지 반경
        public const float PushSpeedMetersPerSecond = 6f; // 바깥 방향 외부 속도
        public const float PerTargetCooldownSeconds = 1f; // 대상별 재발동 제한

        public static bool CanActivate(
            bool alreadyActive,
            bool gameplayAllowed,
            bool authorityReady
        )
        {
            return
                !alreadyActive &&
                gameplayAllowed &&
                authorityReady;
        }

        public static bool CanTriggerTarget(
            bool isSelf,
            bool cooldownActive,
            bool gameplayAllowed
        )
        {
            return
                !isSelf &&
                !cooldownActive &&
                gameplayAllowed;
        }

        public static bool IsInsideDetectionRadius(
            float distance
        )
        {
            return
                Mathf.Max(
                    0f,
                    distance
                ) <=
                DetectionRadius;
        }

        public static Vector3 ResolvePushVelocity(
            Vector3 outwardOffset,
            Vector3 fallbackForward
        )
        {
            outwardOffset.y =
                0f;

            if (
                outwardOffset.sqrMagnitude <=
                0.0001f
            )
            {
                outwardOffset =
                    fallbackForward;

                outwardOffset.y =
                    0f;
            }

            if (
                outwardOffset.sqrMagnitude <=
                0.0001f
            )
            {
                outwardOffset =
                    Vector3.forward;
            }

            return
                outwardOffset.normalized *
                PushSpeedMetersPerSecond;
        }

        public static bool HasCooldownExpired(
            float elapsedSeconds
        )
        {
            return
                elapsedSeconds >=
                PerTargetCooldownSeconds;
        }

        public static bool HasDurationExpired(
            float elapsedSeconds
        )
        {
            return
                elapsedSeconds >=
                DurationSeconds;
        }
    }
}
