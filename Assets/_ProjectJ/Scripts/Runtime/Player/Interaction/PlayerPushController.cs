using UnityEngine; // Unity 충돌과 벡터 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 밀치기 범위
    [DisallowMultipleComponent] // 밀치기 컴포넌트 중복 방지
    [RequireComponent(typeof(PlayerInputReader))] // 입력 컴포넌트 보장
    [RequireComponent(typeof(PlayerStateController))] // 상태 컴포넌트 보장
    public sealed class PlayerPushController : MonoBehaviour // 플레이어 밀치기 제어 컴포넌트 선언
    { // 밀치기 제어 범위
        private const int HitBufferCapacity = 16; // 밀치기 판정 최대 충돌 수

        [SerializeField] private Transform pushOrigin; // 밀치기 판정 시작 위치
        [SerializeField, Min(0.1f)] private float pushRange = 1.5f; // 밀치기 전방 사거리
        [SerializeField, Min(0.05f)] private float pushRadius = 0.45f; // 밀치기 판정 반지름
        [SerializeField, Min(0f)] private float pushForce = 6f; // 대상에 적용할 수평 속도
        [SerializeField, Min(0f)] private float cooldownDuration = 1.2f; // 밀치기 재사용 대기시간
        [SerializeField] private LayerMask collisionLayers = ~0; // 밀치기 대상과 장애물 검사 레이어
        [SerializeField] private bool drawDebugRay = true; // Scene 판정선 표시 여부

        private readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferCapacity]; // 밀치기 판정 결과 버퍼
        private readonly Collider[] overlapBuffer = new Collider[HitBufferCapacity]; // 시작 지점 겹침 결과 버퍼
        private PlayerInputReader inputReader; // 플레이어 입력 제공자
        private PlayerStateController stateController; // 플레이어 상태 관리자

        public float CooldownRemaining { get; private set; } // 남은 밀치기 대기시간
        public float CooldownNormalized => cooldownDuration <= 0f ? 0f : Mathf.Clamp01(CooldownRemaining / cooldownDuration); // 대기시간 비율 반환
        public bool IsReady => CooldownRemaining <= 0f; // 밀치기 사용 가능 상태 반환

        private void Awake() // 밀치기 기능 준비
        { // 밀치기 준비 범위
            inputReader = GetComponent<PlayerInputReader>(); // 입력 컴포넌트 조회
            stateController = GetComponent<PlayerStateController>(); // 상태 컴포넌트 조회

            if (pushOrigin == null) // 판정 시작 위치 누락 확인
            { // 판정 시작점 대체 범위
                pushOrigin = transform; // 플레이어 위치 대체 적용
            } // 판정 시작점 대체 범위 종료
        } // 밀치기 준비 범위 종료

        private void Update() // 밀치기 입력과 대기시간 갱신
        { // 밀치기 갱신 범위
            if (!stateController.CanUseAction) // 상호작용 가능 상태 확인
            { // 상호작용 차단 범위
                return; // 밀치기 갱신 생략
            } // 상호작용 차단 범위 종료

            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - Time.deltaTime); // 남은 대기시간 감소

            if (!IsReady || !inputReader.WasPushPressedThisFrame()) // 사용 가능 상태와 새 입력 확인
            { // 밀치기 입력 없음 범위
                return; // 밀치기 처리 생략
            } // 밀치기 입력 없음 범위 종료

            CooldownRemaining = cooldownDuration; // 밀치기 대기시간 적용
            TryPushClosestTarget(); // 가장 가까운 외부 힘 대상 밀치기 시도
        } // 밀치기 갱신 범위 종료

        private void OnDrawGizmosSelected() // 선택 상태 판정 범위 표시
        { // 밀치기 기즈모 범위
            if (!drawDebugRay) // 판정선 표시 설정 확인
            { // 기즈모 생략 범위
                return; // 판정 범위 표시 생략
            } // 기즈모 생략 범위 종료

            Vector3 origin = GetPushOriginPosition(); // 판정 시작 위치 계산
            Vector3 direction = GetPushDirection(); // 판정 방향 계산
            Gizmos.color = Color.yellow; // 판정선 색상 적용
            Gizmos.DrawWireSphere(origin, pushRadius); // 판정 시작 구체 표시
            Gizmos.DrawLine(origin, origin + direction * pushRange); // 판정 중심선 표시
            Gizmos.DrawWireSphere(origin + direction * pushRange, pushRadius); // 판정 종료 구체 표시
        } // 밀치기 기즈모 범위 종료

        private void TryPushClosestTarget() // 전방에서 가장 가까운 외부 힘 대상 밀치기
        { // 밀치기 대상 검색 범위
            Vector3 origin = GetPushOriginPosition(); // 판정 시작 위치 계산
            Vector3 direction = GetPushDirection(); // 판정 방향 계산
            ExternalForceReceiver closestTarget = null; // 가장 가까운 외부 힘 대상
            float closestDistance = float.PositiveInfinity; // 가장 가까운 대상 거리
            float closestObstacleDistance = float.PositiveInfinity; // 가장 가까운 장애물 거리
            int overlapCount = Physics.OverlapSphereNonAlloc(origin, pushRadius, overlapBuffer, collisionLayers, QueryTriggerInteraction.Ignore); // 시작 구체 겹침 검사

            for (int index = 0; index < overlapCount; index++) // 겹친 충돌체 순회
            { // 겹침 결과 순회 범위
                Collider overlapCollider = overlapBuffer[index]; // 현재 겹친 충돌체 조회
                EvaluatePushCollider(overlapCollider, 0f, ref closestTarget, ref closestDistance, ref closestObstacleDistance); // 시작 지점 충돌체 평가
            } // 겹침 결과 순회 범위 종료

            int hitCount = Physics.SphereCastNonAlloc(origin, pushRadius, direction, hitBuffer, pushRange, collisionLayers, QueryTriggerInteraction.Ignore); // 전방 구체 판정 실행

            for (int index = 0; index < hitCount; index++) // 전방 판정 결과 순회
            { // 전방 결과 순회 범위
                RaycastHit hit = hitBuffer[index]; // 현재 판정 결과 조회
                EvaluatePushCollider(hit.collider, hit.distance, ref closestTarget, ref closestDistance, ref closestObstacleDistance); // 전방 충돌체 평가
            } // 전방 결과 순회 범위 종료

            if (closestTarget == null || closestDistance > closestObstacleDistance) // 대상 존재와 중간 장애물 확인
            { // 밀치기 대상 없음 범위
                return; // 밀치기 적용 생략
            } // 밀치기 대상 없음 범위 종료

            Vector3 targetPosition = closestTarget.ForceReceiverTransform.position; // 대상 위치 조회
            Vector3 pushDirection = Vector3.ProjectOnPlane(targetPosition - transform.position, Vector3.up); // 대상 수평 방향 계산

            if (pushDirection.sqrMagnitude <= 0.0001f) // 대상 방향 계산 실패 확인
            { // 밀치기 방향 대체 범위
                pushDirection = GetPushDirection(); // 플레이어 전방 대체 적용
            } // 밀치기 방향 대체 범위 종료

            ExternalForceRequest request = ExternalForceRequest.CreatePush(pushDirection, pushForce); // 원인과 결합 방식을 포함한 밀치기 요청 생성
            closestTarget.TryReceiveExternalForce(request); // 대상에 통합 외부 힘 요청 적용
        } // 밀치기 대상 검색 범위 종료

        private void EvaluatePushCollider(Collider hitCollider, float hitDistance, ref ExternalForceReceiver closestTarget, ref float closestDistance, ref float closestObstacleDistance) // 밀치기 충돌체 평가
        { // 충돌체 평가 범위
            if (hitCollider == null || hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform)) // 빈 충돌체와 플레이어 자체 충돌체 확인
            { // 자기 충돌 제외 범위
                return; // 현재 충돌체 제외
            } // 자기 충돌 제외 범위 종료

            ExternalForceReceiver target = hitCollider.GetComponentInParent<ExternalForceReceiver>(); // 외부 힘 수신 컴포넌트 조회

            if (target == null) // 밀치기 대상이 아닌 장애물 확인
            { // 장애물 처리 범위
                closestObstacleDistance = Mathf.Min(closestObstacleDistance, hitDistance); // 가장 가까운 장애물 거리 저장
                return; // 장애물 처리 완료
            } // 장애물 처리 범위 종료

            if (hitDistance >= closestDistance) // 기존 대상보다 먼 대상 확인
            { // 먼 대상 제외 범위
                return; // 현재 대상 제외
            } // 먼 대상 제외 범위 종료

            closestTarget = target; // 가장 가까운 대상 저장
            closestDistance = hitDistance; // 가장 가까운 대상 거리 저장
        } // 충돌체 평가 범위 종료

        private Vector3 GetPushOriginPosition() // 밀치기 판정 시작 위치 반환
        { // 판정 시작점 계산 범위
            if (pushOrigin != null && pushOrigin != transform) // 별도 판정 시작점 연결 확인
            { // 별도 시작점 범위
                return pushOrigin.position; // 별도 시작점 위치 반환
            } // 별도 시작점 범위 종료

            return transform.position + Vector3.up; // 플레이어 가슴 높이 위치 반환
        } // 판정 시작점 계산 범위 종료

        private Vector3 GetPushDirection() // 수평 밀치기 방향 반환
        { // 밀치기 방향 계산 범위
            Vector3 direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up); // 플레이어 전방의 수평 방향 계산

            if (direction.sqrMagnitude <= 0.0001f) // 전방 방향 유효성 확인
            { // 전방 방향 대체 범위
                return Vector3.forward; // 월드 전방 대체 반환
            } // 전방 방향 대체 범위 종료

            return direction.normalized; // 정규화된 전방 반환
        } // 밀치기 방향 계산 범위 종료
    } // 밀치기 제어 범위 종료
} // 플레이어 밀치기 범위 종료
