using UnityEngine; // Mathf와 Vector3 사용

namespace ProjectJ.Items
{
    public static class ProjectJCartPolicy
    {
        public const float LifetimeSeconds = 8f; // 카트 최대 탑승 시간
        public const float MovementSpeed = 10f; // 자동 이동 속도
        public const int MaximumRouteNodes = 3; // 한 번에 추적할 최대 노드 수
        public const float StartNodeSearchRadius = 4f; // 사용 위치 주변 시작 노드 검색 반경
        public const float NodeArrivalDistance = 0.4f; // 노드 도착 판정 거리
        public const float RiderVerticalOffset = 0.65f; // 카트 중심 기준 Rider 발 높이
        public const float ContactRadius = 1.15f; // 다른 Player 접촉 판정 반경
        public const float SidePushSpeed = 6f; // 접촉 대상 옆 방향 외부 속도
        public const float RehitCooldownSeconds = 0.5f; // 같은 대상 재적중 제한

        public static bool CanUse(
            bool runnerReady,
            bool gameplayAllowed,
            bool alreadyRiding,
            bool hasStartNode,
            bool hasExistingCart
        )
        {
            return runnerReady && gameplayAllowed && !alreadyRiding && hasStartNode && !hasExistingCart;
        }

        public static float CalculateTravelDistance(float deltaTime)
        {
            return MovementSpeed * Mathf.Max(0f, deltaTime);
        }

        public static bool HasReachedNode(float distance)
        {
            return Mathf.Max(0f, distance) <= NodeArrivalDistance;
        }

        public static bool CanAdvanceToNextNode(int visitedNodeCount, bool hasNextNode)
        {
            return visitedNodeCount < MaximumRouteNodes && hasNextNode;
        }

        public static bool ShouldFinishRide(
            bool lifetimeActive,
            bool gameplayAllowed,
            bool ownerValid,
            bool routeEnded
        )
        {
            return !lifetimeActive || !gameplayAllowed || !ownerValid || routeEnded;
        }

        public static bool IsRehitReady(float elapsedSeconds)
        {
            return elapsedSeconds >= RehitCooldownSeconds;
        }

        public static Vector3 ResolveSidePushDirection(
            Vector3 cartRight,
            Vector3 cartPosition,
            Vector3 targetPosition
        )
        {
            cartRight.y = 0f;

            if (cartRight.sqrMagnitude <= 0.0001f)
            {
                cartRight = Vector3.right;
            }

            cartRight.Normalize();

            Vector3 offset = targetPosition - cartPosition;
            offset.y = 0f;

            float side = Vector3.Dot(offset, cartRight);
            return side < 0f ? -cartRight : cartRight;
        }

        public static bool IsWithinStartNodeSearchRadius(float distance)
        {
            return Mathf.Max(0f, distance) <= StartNodeSearchRadius;
        }
    }
}
