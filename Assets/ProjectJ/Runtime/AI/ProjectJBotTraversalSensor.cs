using System.Collections.Generic; // 이동 후보 목록 사용
using ProjectJ.Player; // Player Layer 제외 규칙 사용
using UnityEngine; // Unity Physics와 Vector 타입 사용

namespace ProjectJ.AI // Bot AI Namespace
{
    [DisallowMultipleComponent] // 센서 중복 추가 방지
    public sealed class ProjectJBotTraversalSensor : MonoBehaviour // 물리 기반 주변 지형 센서
    {
        private const float NearProbeDistance = 0.65f; // 가까운 바닥 탐색 거리
        private const float FarProbeDistance = 1.4f; // 착지 바닥 탐색 거리
        private const float GroundProbeStartHeight = 2f; // 바닥 탐색 시작 높이
        private const float GroundProbeDistance = 3.2f; // 바닥 탐색 최대 거리
        private const float GroundProbeRadius = 0.12f; // 바닥 탐색 구 반경
        private const float MaximumStepHeight = 0.35f; // 기존 자동 계단 높이
        private const float MaximumSafeDrop = 0.6f; // 허용 가능한 작은 하강
        private const float JumpSafetyMargin = 0.85f; // 점프 도달 거리 안전 배율
        private const float MinimumGroundNormalY = 0.5f; // 걸을 수 있는 바닥 기울기
        private const float LandingSkin = 0.05f; // 착지 몸통 바닥 여유
        private const int ArcSampleCount = 3; // 점프 궤적 중간 검사 수

        private static readonly float[] SampleAngles = // 목표 기준 탐색 각도 목록
        {
            0f, // 정면 방향
            30f, // 오른쪽 30도
            -30f, // 왼쪽 30도
            60f, // 오른쪽 60도
            -60f, // 왼쪽 60도
            90f, // 오른쪽 방향
            -90f, // 왼쪽 방향
            120f, // 오른쪽 뒤 방향
            -120f, // 왼쪽 뒤 방향
            150f, // 오른쪽 후방 방향
            -150f, // 왼쪽 후방 방향
            180f // 완전 후방 방향
        };

        private readonly List<ProjectJBotTraversalCandidate> candidates = // 재사용 이동 후보 목록
            new List<ProjectJBotTraversalCandidate>(12); // 열두 방향 저장 공간
        private readonly Vector3[] directionBuffer = new Vector3[12]; // 런타임 탐색 방향 재사용 버퍼
        private readonly RaycastHit[] groundHitBuffer = new RaycastHit[16]; // 바닥 탐색 결과 버퍼
        private readonly RaycastHit[] pathHitBuffer = new RaycastHit[16]; // 이동 경로 결과 버퍼
        private readonly Collider[] clearanceHitBuffer = new Collider[16]; // 착지 공간 결과 버퍼
        private readonly Collider[] arcHitBuffer = new Collider[16]; // 점프 궤적 충돌 결과 버퍼

        public static Vector3[] BuildSampleDirections( // 목표 기준 열두 탐색 방향 생성
            Vector3 goalDirection // 장거리 목표 방향
        )
        {
            Vector3[] directions = new Vector3[SampleAngles.Length]; // Test와 외부 호출용 방향 배열 생성
            FillSampleDirections(goalDirection, directions); // 목표 기준 방향 배열 채우기
            return directions; // 열두 탐색 방향 반환
        }

        private static void FillSampleDirections( // 재사용 배열에 탐색 방향 채우기
            Vector3 goalDirection, // 장거리 목표 방향
            Vector3[] directions // 결과 저장 방향 배열
        )
        {
            Vector3 planarGoal = goalDirection; // 목표 방향 복사
            planarGoal.y = 0f; // 수직 성분 제거

            if (planarGoal.sqrMagnitude <= 0.0001f) // 목표 수평 방향 없음 확인
            {
                planarGoal = Vector3.forward; // 기본 전방 방향 사용
            }
            else // 유효 목표 방향 처리
            {
                planarGoal.Normalize(); // 목표 방향 정규화
            }

            for (int index = 0; index < SampleAngles.Length; index++) // 모든 탐색 각도 순회
            {
                directions[index] = Quaternion.AngleAxis(SampleAngles[index], Vector3.up) * planarGoal; // 회전된 수평 방향 저장
                directions[index].Normalize(); // 회전 오차 포함 방향 정규화
            }
        }

        public bool TrySelectTraversal( // 주변 지형에서 최적 이동 선택
            Vector3 footPosition, // 현재 Bot 발 위치
            Vector3 goalDirection, // 장거리 목표 방향
            float minimumSafeY, // 체크포인트 안전 높이
            float moveSpeed, // 현재 수평 이동 속도
            float jumpSpeed, // 현재 점프 속도
            float gravity, // 현재 중력 가속도
            float colliderRadius, // 현재 몸통 반경
            float colliderHeight, // 현재 몸통 높이
            Vector3 failedDirection, // 최근 정체 방향
            out ProjectJBotTraversalDecision decision // 최종 이동 판단
        )
        {
            candidates.Clear(); // 이전 탐색 후보 제거
            FillSampleDirections(goalDirection, directionBuffer); // 재사용 버퍼에 목표 기준 방향 생성
            int terrainMask = PlayerCollisionRules.ExcludePlayerLayer(Physics.AllLayers); // Player 제외 지형 Mask 생성
            float safeRadius = Mathf.Max(0.1f, colliderRadius); // 몸통 반경 최소값 보정
            float safeHeight = Mathf.Max(safeRadius * 2f, colliderHeight); // 몸통 높이 최소값 보정
            float safeGravity = Mathf.Min(-0.0001f, gravity); // 하향 중력 보장
            float maximumJumpHeight = Mathf.Max( // 실제 점프 최고 높이 계산
                MaximumStepHeight, // 계단 높이 이상 보장
                jumpSpeed * jumpSpeed / (-2f * safeGravity) - 0.1f // 점프 최고점 안전 여유 적용
            );

            for (int index = 0; index < directionBuffer.Length; index++) // 열두 탐색 방향 순회
            {
                ProjectJBotTraversalCandidate candidate = ObserveCandidate( // 현재 방향 지형 관측
                    footPosition, // 현재 발 위치 전달
                    directionBuffer[index], // 현재 탐색 방향 전달
                    moveSpeed, // 이동 속도 전달
                    jumpSpeed, // 점프 속도 전달
                    safeGravity, // 중력 전달
                    safeRadius, // 몸통 반경 전달
                    safeHeight, // 몸통 높이 전달
                    maximumJumpHeight, // 점프 높이 한계 전달
                    terrainMask // 지형 Mask 전달
                );

                candidates.Add(candidate); // 관측 후보 목록 추가
            }

            decision = ProjectJBotNavigationPolicy.SelectBestCandidate( // 정책 기반 최적 후보 선택
                candidates, // 관측 후보 목록 전달
                goalDirection, // 목표 방향 전달
                minimumSafeY, // 안전 높이 전달
                MaximumStepHeight, // 걷기 단차 한계 전달
                maximumJumpHeight, // 점프 높이 한계 전달
                MaximumSafeDrop, // 안전 하강 한계 전달
                failedDirection // 실패 방향 전달
            );

            return decision.IsValid; // 유효 이동 후보 존재 여부 반환
        }

        private ProjectJBotTraversalCandidate ObserveCandidate( // 단일 방향 지형 관측
            Vector3 footPosition, // 현재 발 위치
            Vector3 direction, // 탐색 방향
            float moveSpeed, // 현재 이동 속도
            float jumpSpeed, // 현재 점프 속도
            float gravity, // 현재 중력
            float colliderRadius, // 몸통 반경
            float colliderHeight, // 몸통 높이
            float maximumJumpHeight, // 최대 점프 높이
            int terrainMask // Player 제외 지형 Mask
        )
        {
            Vector3 nearPosition = footPosition + direction * NearProbeDistance; // 가까운 바닥 위치 계산
            Vector3 farPosition = footPosition + direction * FarProbeDistance; // 착지 바닥 위치 계산
            bool hasNearGround = TryFindGround(nearPosition, terrainMask, out Vector3 nearGround); // 가까운 바닥 탐색
            bool hasFarGround = TryFindGround(farPosition, terrainMask, out Vector3 farGround); // 착지 바닥 탐색
            bool crossesGap = !hasNearGround && hasFarGround; // 작은 틈 통과 여부 계산
            float heightDelta = hasFarGround // 착지 바닥 존재 확인
                ? farGround.y - footPosition.y // 착지 높이 차이 계산
                : float.NegativeInfinity; // 바닥 없음 높이 표시
            bool hasHeadroom = hasFarGround && IsLandingClear( // 착지 몸통 공간 검사
                farGround, // 착지 위치 전달
                colliderRadius, // 몸통 반경 전달
                colliderHeight, // 몸통 높이 전달
                terrainMask // 지형 Mask 전달
            );
            bool requiresJump = crossesGap || heightDelta > MaximumStepHeight; // 점프 필요 여부 계산
            bool pathClear = hasFarGround && hasHeadroom && ( // 기본 경로 안전 조건 확인
                requiresJump // 점프 후보 확인
                    ? IsJumpPathClear( // 점프 궤적 검사
                        footPosition, // 시작 발 위치 전달
                        farGround, // 착지 위치 전달
                        direction, // 이동 방향 전달
                        moveSpeed, // 이동 속도 전달
                        jumpSpeed, // 점프 속도 전달
                        gravity, // 중력 전달
                        colliderRadius, // 몸통 반경 전달
                        colliderHeight, // 몸통 높이 전달
                        maximumJumpHeight, // 점프 높이 한계 전달
                        terrainMask // 지형 Mask 전달
                    )
                    : IsWalkPathClear( // 걷기 경로 검사
                        footPosition, // 시작 발 위치 전달
                        direction, // 이동 방향 전달
                        heightDelta, // 착지 높이 차이 전달
                        colliderRadius, // 몸통 반경 전달
                        colliderHeight, // 몸통 높이 전달
                        terrainMask // 지형 Mask 전달
                    )
            );

            return new ProjectJBotTraversalCandidate( // 관측 이동 후보 반환
                direction, // 이동 방향 전달
                hasFarGround ? farGround : farPosition, // 착지 또는 탐색 위치 전달
                heightDelta, // 착지 높이 차이 전달
                hasFarGround, // 바닥 존재 상태 전달
                pathClear, // 경로 확보 상태 전달
                hasHeadroom, // 머리 공간 상태 전달
                crossesGap // 틈 통과 상태 전달
            );
        }

        private bool TryFindGround( // 탐색 지점 바닥 조회
            Vector3 samplePosition, // 수평 탐색 위치
            int terrainMask, // Player 제외 지형 Mask
            out Vector3 groundPosition // 발견 바닥 위치
        )
        {
            Vector3 origin = samplePosition + Vector3.up * GroundProbeStartHeight; // 바닥 탐색 시작점 계산
            int hitCount = Physics.SphereCastNonAlloc( // 할당 없는 하향 바닥 탐색
                origin, // 탐색 시작점 전달
                GroundProbeRadius, // 탐색 구 반경 전달
                Vector3.down, // 하향 탐색 방향 전달
                groundHitBuffer, // 재사용 결과 버퍼 전달
                GroundProbeDistance, // 탐색 최대 거리 전달
                terrainMask, // Player 제외 Mask 전달
                QueryTriggerInteraction.Ignore // Trigger 지형 제외
            );
            float closestDistance = float.PositiveInfinity; // 최근접 바닥 거리 초기화
            groundPosition = default; // 바닥 위치 초기화
            bool foundGround = false; // 바닥 발견 상태 초기화

            for (int index = 0; index < hitCount; index++) // 바닥 충돌 결과 순회
            {
                RaycastHit hit = groundHitBuffer[index]; // 현재 바닥 충돌 조회

                if (IsIgnoredTraversalCollider(hit.collider) || hit.normal.y < MinimumGroundNormalY || hit.distance >= closestDistance) // 유효 바닥 조건 확인
                {
                    continue; // 잘못된 바닥 제외
                }

                closestDistance = hit.distance; // 최근접 거리 갱신
                groundPosition = hit.point; // 바닥 위치 갱신
                foundGround = true; // 바닥 발견 표시
            }

            return foundGround; // 바닥 발견 여부 반환
        }

        private bool IsLandingClear( // 착지 위치 몸통 공간 검사
            Vector3 landingPosition, // 착지 바닥 위치
            float colliderRadius, // 몸통 반경
            float colliderHeight, // 몸통 높이
            int terrainMask // Player 제외 지형 Mask
        )
        {
            float queryRadius = colliderRadius * 0.9f; // 착지 검사 여유 반경 계산
            Vector3 bottomPoint = landingPosition + Vector3.up * (queryRadius + LandingSkin); // 착지 캡슐 하단점 계산
            Vector3 topPoint = landingPosition + Vector3.up * (colliderHeight - queryRadius); // 착지 캡슐 상단점 계산
            int overlapCount = Physics.OverlapCapsuleNonAlloc( // 할당 없는 착지 공간 검사
                bottomPoint, // 캡슐 하단점 전달
                topPoint, // 캡슐 상단점 전달
                queryRadius, // 캡슐 반경 전달
                clearanceHitBuffer, // 재사용 결과 버퍼 전달
                terrainMask, // Player 제외 지형 Mask 전달
                QueryTriggerInteraction.Ignore // Trigger 장애물 제외
            );

            for (int index = 0; index < overlapCount; index++) // 착지 공간 충돌 결과 순회
            {
                if (IsIgnoredTraversalCollider(clearanceHitBuffer[index])) // 자기 몸통과 Player 확인
                {
                    continue; // 이동을 막지 않는 Collider 제외
                }

                return false; // 실제 착지 장애물 발견
            }

            return true; // 착지 공간 비어 있음 반환
        }

        private bool IsWalkPathClear( // 걷기 경로 장애물 검사
            Vector3 footPosition, // 시작 발 위치
            Vector3 direction, // 이동 방향
            float heightDelta, // 착지 높이 차이
            float colliderRadius, // 몸통 반경
            float colliderHeight, // 몸통 높이
            int terrainMask // Player 제외 지형 Mask
        )
        {
            GetBodyCapsule(footPosition, colliderRadius, colliderHeight, out Vector3 bottomPoint, out Vector3 topPoint, out float queryRadius); // 현재 몸통 캡슐 계산
            int hitCount = Physics.CapsuleCastNonAlloc( // 할당 없는 수평 경로 검사
                bottomPoint, // 캡슐 하단점 전달
                topPoint, // 캡슐 상단점 전달
                queryRadius, // 캡슐 반경 전달
                direction, // 이동 방향 전달
                pathHitBuffer, // 재사용 결과 버퍼 전달
                FarProbeDistance, // 걷기 탐색 거리 전달
                terrainMask, // Player 제외 지형 Mask 전달
                QueryTriggerInteraction.Ignore // Trigger 장애물 제외
            );

            for (int index = 0; index < hitCount; index++) // 경로 충돌 결과 순회
            {
                Collider obstacle = pathHitBuffer[index].collider; // 현재 장애물 Collider 조회

                if (obstacle == null) // 빈 충돌 결과 확인
                {
                    continue; // 빈 결과 제외
                }

                if (IsIgnoredTraversalCollider(obstacle)) // 자기 몸통과 Player 확인
                {
                    continue; // 이동을 막지 않는 Collider 제외
                }

                float obstacleTop = obstacle.bounds.max.y - footPosition.y; // 장애물 상단 상대 높이 계산

                if (heightDelta <= MaximumStepHeight && obstacleTop <= MaximumStepHeight + 0.08f) // 낮은 계단 장애물 확인
                {
                    continue; // 기존 Step Up에 맡길 낮은 단차 제외
                }

                return false; // 실제 벽 충돌 차단
            }

            return true; // 걷기 경로 확보 반환
        }

        private bool IsJumpPathClear( // 점프 도달과 궤적 검사
            Vector3 footPosition, // 시작 발 위치
            Vector3 landingPosition, // 착지 바닥 위치
            Vector3 direction, // 이동 방향
            float moveSpeed, // 수평 이동 속도
            float jumpSpeed, // 점프 속도
            float gravity, // 중력 가속도
            float colliderRadius, // 몸통 반경
            float colliderHeight, // 몸통 높이
            float maximumJumpHeight, // 점프 높이 한계
            int terrainMask // Player 제외 지형 Mask
        )
        {
            float heightDelta = landingPosition.y - footPosition.y; // 착지 높이 차이 계산

            if (heightDelta > maximumJumpHeight) // 점프 높이 한계 초과 확인
            {
                return false; // 높은 착지 차단
            }

            if (!ProjectJBotNavigationPolicy.CanReachLanding(FarProbeDistance, heightDelta, moveSpeed, jumpSpeed, gravity, JumpSafetyMargin)) // 기존 능력 도달 가능 확인
            {
                return false; // 먼 착지 차단
            }

            float safeMoveSpeed = Mathf.Max(0.01f, moveSpeed); // 이동 속도 최소값 보정
            float travelTime = FarProbeDistance / safeMoveSpeed; // 착지 수평 도달 시간 계산
            float bodyCenterHeight = colliderHeight * 0.5f; // 몸통 중심 높이 계산
            float queryRadius = colliderRadius * 0.8f; // 궤적 검사 여유 반경 계산

            for (int index = 1; index <= ArcSampleCount; index++) // 점프 중간 궤적 순회
            {
                float ratio = index / (ArcSampleCount + 1f); // 현재 궤적 진행 비율 계산
                float time = travelTime * ratio; // 현재 궤적 시간 계산
                float horizontalDistance = FarProbeDistance * ratio; // 현재 수평 이동 거리 계산
                float verticalOffset = jumpSpeed * time + 0.5f * gravity * time * time; // 포물선 수직 높이 계산
                Vector3 center = footPosition + direction * horizontalDistance + Vector3.up * (bodyCenterHeight + verticalOffset); // 현재 몸통 중심 계산
                int overlapCount = Physics.OverlapSphereNonAlloc( // 현재 궤적 몸통 공간 검사
                    center, // 검사 중심 전달
                    queryRadius, // 검사 반경 전달
                    arcHitBuffer, // 재사용 충돌 버퍼 전달
                    terrainMask, // Player 제외 지형 Mask 전달
                    QueryTriggerInteraction.Ignore // Trigger 장애물 제외
                );

                for (int overlapIndex = 0; overlapIndex < overlapCount; overlapIndex++) // 점프 궤적 충돌 결과 순회
                {
                    if (IsIgnoredTraversalCollider(arcHitBuffer[overlapIndex])) // 자기 몸통과 Player 확인
                    {
                        continue; // 이동을 막지 않는 Collider 제외
                    }

                    return false; // 막힌 점프 차단
                }
            }

            return true; // 점프 경로 확보 반환
        }

        private bool IsIgnoredTraversalCollider( // 이동 판단에서 제외할 Collider 판정
            Collider candidate // 검사 대상 Collider
        )
        {
            if (candidate == null) // 빈 Collider 확인
            {
                return true; // 빈 결과 제외
            }

            Transform candidateTransform = candidate.transform; // Collider Transform 조회

            if (candidateTransform == transform || candidateTransform.IsChildOf(transform)) // 센서 소유 계층 확인
            {
                return true; // 자기 Collider 제외
            }

            int playerLayer = PlayerCollisionRules.GetPlayerLayer(); // 실행 중 Player Layer 번호 조회
            return playerLayer >= 0 && candidate.gameObject.layer == playerLayer; // 다른 Player Collider 제외
        }

        private static void GetBodyCapsule( // 발 위치 기준 몸통 캡슐 계산
            Vector3 footPosition, // 현재 발 위치
            float colliderRadius, // 몸통 반경
            float colliderHeight, // 몸통 높이
            out Vector3 bottomPoint, // 결과 캡슐 하단점
            out Vector3 topPoint, // 결과 캡슐 상단점
            out float queryRadius // 결과 캡슐 반경
        )
        {
            queryRadius = colliderRadius * 0.9f; // 충돌 여유 반경 계산
            bottomPoint = footPosition + Vector3.up * (queryRadius + LandingSkin); // 캡슐 하단점 계산
            topPoint = footPosition + Vector3.up * (colliderHeight - queryRadius); // 캡슐 상단점 계산
        }
    }
}
