using UnityEngine; // MonoBehaviour와 Gizmo 사용

namespace ProjectJ.AI
{
    [DisallowMultipleComponent]
    public sealed class ProjectJBotRouteNode :
        MonoBehaviour
    {
        [SerializeField]
        private int routeOrder; // Route 정렬 순서

        [SerializeField]
        private bool requiresJump; // 접근 중 점프 필요 여부

        [SerializeField]
        [Min(0.1f)]
        private float arrivalRadius =
            0.75f; // Node 도달 반경

        [SerializeField]
        [Min(0.1f)]
        private float jumpTriggerDistance =
            1.5f; // 점프 입력 시작 거리

        public int RouteOrder =>
            routeOrder; // Route 순서 조회

        public bool RequiresJump =>
            requiresJump; // 점프 필요 여부 조회

        public float ArrivalRadius =>
            arrivalRadius; // 도달 반경 조회

        public float JumpTriggerDistance =>
            jumpTriggerDistance; // 점프 시작 거리 조회

        public void Configure(
            int order,
            bool jumpRequired,
            float nodeArrivalRadius = 0.75f,
            float nodeJumpTriggerDistance = 1.5f
        )
        {
            routeOrder =
                order; // Route 순서 적용

            requiresJump =
                jumpRequired; // 점프 필요 여부 적용

            arrivalRadius =
                Mathf.Max(
                    0.1f,
                    nodeArrivalRadius
                ); // 최소 도달 반경 적용

            jumpTriggerDistance =
                Mathf.Max(
                    0.1f,
                    nodeJumpTriggerDistance
                ); // 최소 점프 거리 적용
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(
                transform.position,
                arrivalRadius
            ); // Scene Route 도달 반경 표시

            if (requiresJump)
            {
                Gizmos.DrawWireSphere(
                    transform.position,
                    jumpTriggerDistance
                ); // Scene 점프 시작 반경 표시
            }
        }
#endif
    }
}
