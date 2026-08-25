using UnityEngine; // Mathf와 Vector3 사용

namespace ProjectJ.Items
{
    public static class ProjectJDronePolicy
    {
        public const float LifetimeSeconds = 12f; // 드론 최대 유지 시간
        public const float Speed = 9f; // 드론 이동 속도
        public const float AttackExternalSpeed = 7f; // 공격 외부 속도
        public const int MaximumReacquireCount = 1; // 목표 재탐색 최대 횟수
        public const float AttackDistance = 1f; // 접촉 공격 판정 거리

        public const float CollisionRadius = 0.4f; // 월드 충돌 스윕 반경
        public const float RouteNodeArrivalDistance = 0.4f; // 경로 노드 도착 거리
        public const float RouteNodeSearchRadius = 12f; // 주변 경로 노드 검색 반경
        public const float TargetHeightOffset = 0.9f; // 목표 중심 추적 높이

        public static bool CanUse(
            bool runnerReady,
            bool gameplayAllowed,
            int ownerRaceRank,
            bool targetFound
        )
        {
            return
                runnerReady &&
                gameplayAllowed &&
                ownerRaceRank > 1 &&
                targetFound;
        }

        public static bool CanTarget(
            bool objectValid,
            bool isOwner,
            bool gameplayAllowed,
            bool trackable
        )
        {
            return
                objectValid &&
                !isOwner &&
                gameplayAllowed &&
                trackable;
        }

        public static bool IsInitialLeaderRank(
            int raceRank
        )
        {
            return raceRank == 1;
        }

        public static bool IsBetterReacquireCandidate(
            int candidateRank,
            int bestRank,
            int candidatePlayerIndex,
            int bestPlayerIndex
        )
        {
            if (candidateRank <= 0)
            {
                return false;
            }

            if (bestRank == int.MaxValue)
            {
                return true;
            }

            if (candidateRank < bestRank)
            {
                return true;
            }

            if (candidateRank > bestRank)
            {
                return false;
            }

            return
                candidatePlayerIndex <
                bestPlayerIndex;
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

        public static bool HasReachedAttackDistance(
            float distance
        )
        {
            return
                Mathf.Max(
                    0f,
                    distance
                ) <=
                AttackDistance;
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

        public static Vector3 ResolveAttackVelocity(
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
                direction =
                    Vector3.forward;
            }

            return
                direction.normalized *
                AttackExternalSpeed;
        }

        public static bool HasLifetimeExpired(
            float elapsedSeconds
        )
        {
            return
                elapsedSeconds >=
                LifetimeSeconds;
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
    }
}
