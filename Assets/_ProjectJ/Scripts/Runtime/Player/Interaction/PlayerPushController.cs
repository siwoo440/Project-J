using UnityEngine; // Unity 충돌과 벡터 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스
{
    [DisallowMultipleComponent] // 밀치기 컴포넌트 중복 방지
    [RequireComponent(typeof(PlayerInputReader))] // 입력 컴포넌트 자동 보장
    public sealed class PlayerPushController : MonoBehaviour // 플레이어 밀치기 제어 컴포넌트
    {
        private const int HitBufferCapacity = 16; // 밀치기 판정 최대 충돌 수

        [SerializeField] private Transform pushOrigin; // 밀치기 판정 시작 위치
        [SerializeField, Min(0.1f)] private float pushRange = 1.5f; // 밀치기 전방 사거리
        [SerializeField, Min(0.05f)] private float pushRadius = 0.45f; // 밀치기 판정 반지름
        [SerializeField, Min(0f)] private float pushForce = 6f; // 더미에 적용할 수평 속도
        [SerializeField, Min(0f)] private float cooldownDuration = 1.2f; // 밀치기 재사용 대기시간
        [SerializeField] private LayerMask collisionLayers = ~0; // 밀치기 대상과 장애물 검사 레이어
        [SerializeField] private bool drawDebugRay = true; // Scene 판정선 표시 여부

        private readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferCapacity]; // 밀치기 판정 결과 버퍼
        private readonly Collider[] overlapBuffer = new Collider[HitBufferCapacity]; // 시작 지점 겹침 결과 버퍼
        private PlayerInputReader inputReader; // 플레이어 입력 제공자

        public float CooldownRemaining { get; private set; } // 남은 밀치기 대기시간
        public float CooldownNormalized => cooldownDuration <= 0f ? 0f : Mathf.Clamp01(CooldownRemaining / cooldownDuration); // 대기시간 비율
        public bool IsReady => CooldownRemaining <= 0f; // 밀치기 사용 가능 상태

        private void Awake() // 밀치기 기능 준비
        {
            inputReader = GetComponent<PlayerInputReader>(); // 입력 컴포넌트 조회

            if (pushOrigin == null) // 판정 시작 위치 누락 확인
            {
                pushOrigin = transform; // 플레이어 Transform 대체 적용
            }
        }

        private void Update() // 밀치기 입력과 대기시간 갱신
        {
            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - Time.deltaTime); // 남은 대기시간 감소

            if (!IsReady || !inputReader.WasPushPressedThisFrame()) // 사용 가능 상태와 새 입력 확인
            {
                return; // 밀치기 처리 생략
            }

            CooldownRemaining = cooldownDuration; // 밀치기 대기시간 적용
            TryPushClosestTarget(); // 가장 가까운 더미 밀치기 시도
        }

        private void OnDrawGizmosSelected() // 선택 상태 판정 범위 표시
        {
            if (!drawDebugRay) // 판정선 표시 설정 확인
            {
                return; // 판정 범위 표시 생략
            }

            Vector3 origin = GetPushOriginPosition(); // 판정 시작 위치 계산
            Vector3 direction = GetPushDirection(); // 판정 방향 계산
            Gizmos.color = Color.yellow; // 판정선 색상 적용
            Gizmos.DrawWireSphere(origin, pushRadius); // 판정 시작 구체 표시
            Gizmos.DrawLine(origin, origin + direction * pushRange); // 판정 중심선 표시
            Gizmos.DrawWireSphere(origin + direction * pushRange, pushRadius); // 판정 종료 구체 표시
        }

        private void TryPushClosestTarget() // 전방에서 가장 가까운 더미 밀치기
        {
            Vector3 origin = GetPushOriginPosition(); // 판정 시작 위치 계산
            Vector3 direction = GetPushDirection(); // 판정 방향 계산
            PushableDummy closestTarget = null; // 가장 가까운 더미 참조
            float closestDistance = float.PositiveInfinity; // 가장 가까운 대상 거리
            float closestObstacleDistance = float.PositiveInfinity; // 가장 가까운 장애물 거리

            int overlapCount = Physics.OverlapSphereNonAlloc(origin, pushRadius, overlapBuffer, collisionLayers, QueryTriggerInteraction.Ignore); // 시작 구체 겹침 검사

            for (int index = 0; index < overlapCount; index++) // 겹친 충돌체 순회
            {
                Collider overlapCollider = overlapBuffer[index]; // 현재 겹친 충돌체 조회
                EvaluatePushCollider(overlapCollider, 0f, ref closestTarget, ref closestDistance, ref closestObstacleDistance); // 시작 지점 충돌체 평가
            }

            int hitCount = Physics.SphereCastNonAlloc(origin, pushRadius, direction, hitBuffer, pushRange, collisionLayers, QueryTriggerInteraction.Ignore); // 전방 구체 판정 실행

            for (int index = 0; index < hitCount; index++) // 전방 판정 결과 순회
            {
                RaycastHit hit = hitBuffer[index]; // 현재 판정 결과 조회
                EvaluatePushCollider(hit.collider, hit.distance, ref closestTarget, ref closestDistance, ref closestObstacleDistance); // 전방 충돌체 평가
            }

            if (closestTarget == null || closestDistance > closestObstacleDistance) // 대상 존재와 중간 장애물 확인
            {
                return; // 밀치기 적용 생략
            }

            Vector3 pushDirection = Vector3.ProjectOnPlane(closestTarget.transform.position - transform.position, Vector3.up); // 대상 방향 계산

            if (pushDirection.sqrMagnitude <= 0.0001f) // 대상 방향 계산 실패 확인
            {
                pushDirection = GetPushDirection(); // 플레이어 전방 대체 적용
            }

            closestTarget.TryReceivePush(pushDirection.normalized, pushForce); // 더미에 밀치기 속도 적용
        }

        private void EvaluatePushCollider(Collider hitCollider, float hitDistance, ref PushableDummy closestTarget, ref float closestDistance, ref float closestObstacleDistance) // 밀치기 충돌체 평가
        {
            if (hitCollider == null || hitCollider.transform.IsChildOf(transform)) // 빈 충돌체와 플레이어 자체 충돌체 확인
            {
                return; // 현재 충돌체 제외
            }

            PushableDummy target = hitCollider.GetComponentInParent<PushableDummy>(); // 더미 컴포넌트 조회

            if (target == null) // 밀치기 대상이 아닌 장애물 확인
            {
                closestObstacleDistance = Mathf.Min(closestObstacleDistance, hitDistance); // 가장 가까운 장애물 거리 저장
                return; // 장애물 처리 완료
            }

            if (hitDistance >= closestDistance) // 기존 대상보다 먼 대상 확인
            {
                return; // 현재 대상 제외
            }

            closestTarget = target; // 가장 가까운 대상 저장
            closestDistance = hitDistance; // 가장 가까운 대상 거리 저장
        }

        private Vector3 GetPushOriginPosition() // 밀치기 판정 시작 위치 반환
        {
            if (pushOrigin != null && pushOrigin != transform) // 별도 판정 시작점 연결 확인
            {
                return pushOrigin.position; // 별도 시작점 위치 반환
            }

            return transform.position + Vector3.up; // 플레이어 가슴 높이 위치 반환
        }

        private Vector3 GetPushDirection() // 수평 밀치기 방향 반환
        {
            Vector3 direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up); // 플레이어 전방의 수평 방향 계산

            if (direction.sqrMagnitude <= 0.0001f) // 전방 방향 유효성 확인
            {
                return Vector3.forward; // 월드 전방 대체 반환
            }

            return direction.normalized; // 정규화된 전방 반환
        }
    }
}
