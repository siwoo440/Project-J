using UnityEngine; // Mathf와 Vector3 사용

namespace ProjectJ.Items
{
    public static class ProjectJHomingMissilePolicy
    {
        public const float SearchRadius = 35f; // 자동 목표 탐색 반경
        public const float Speed = 11f; // 유도탄 이동 속도
        public const float LifetimeSeconds = 10f; // 유도탄 최대 수명
        public const float HitExternalSpeed = 8f; // 적중 외부 속도
        public const int MaximumReacquireCount = 1; // 목표 재탐색 최대 횟수

        public const float CollisionRadius = 0.3f; // 투사체 충돌 반경
        public const float RouteNodeArrivalDistance = 0.4f; // 경로 노드 도착 거리
        public const float RouteNodeSearchRadius = 12f; // 주변 경로 노드 검색 반경
        public const float TargetHeightOffset = 0.8f; // Player 중심 조준 높이

        public static bool IsWithinSearchRadius(
            float distance
        )
        {
            return
                Mathf.Max(
                    0f,
                    distance
                ) <=
                SearchRadius;
        }

        public static float CalculateStepDistance(
            float deltaTime
        )
        {
            return
                Speed *
                Mathf.Max(
                    0f,
                    deltaTime
                );
        }

        public static bool CanReacquire(
            int currentCount
        )
        {
            return
                Mathf.Max(
                    0,
                    currentCount
                ) <
                MaximumReacquireCount;
        }

        public static bool CanTarget(
            bool objectValid,
            bool owner,
            bool gameplayAllowed,
            bool visible
        )
        {
            return
                objectValid &&
                !owner &&
                gameplayAllowed &&
                visible;
        }

        public static bool HasReachedRouteNode(
            float distance
        )
        {
            return
                Mathf.Max(
                    0f,
                    distance
                ) <=
                RouteNodeArrivalDistance;
        }

        public static bool IsWithinRouteNodeSearchRadius(
            float distance
        )
        {
            return
                Mathf.Max(
                    0f,
                    distance
                ) <=
                RouteNodeSearchRadius;
        }

        public static Vector3 ResolveHitVelocity(
            Vector3 direction
        )
        {
            direction.y =
                0f;

            if (
                direction.sqrMagnitude <=
                0.0001f
            )
            {
                return Vector3.zero;
            }

            return
                direction.normalized *
                HitExternalSpeed;
        }

        public static bool ShouldTrackDirectly(
            bool clearLine
        )
        {
            return clearLine;
        }

        public static bool HasLifetimeExpired(
            float elapsedSeconds
        )
        {
            return
                elapsedSeconds >=
                LifetimeSeconds;
        }

        public static bool CanSpawn(
            bool runnerReady,
            bool targetFound
        )
        {
            return
                runnerReady &&
                targetFound;
        }
    }
}
