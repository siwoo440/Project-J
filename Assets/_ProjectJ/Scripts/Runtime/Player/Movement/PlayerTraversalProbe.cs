using UnityEngine; // Unity 물리 탐지 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 기능 범위
    public sealed class PlayerTraversalProbe // 경사와 모서리와 끝자락 탐지기 선언
    { // 지형 탐지 범위
        private const int HitBufferCapacity = 16; // 물리 탐지 최대 충돌 수
        private const float DirectionThreshold = 0.0001f; // 유효 방향 판정 기준
        private const float ProbeOriginPadding = 0.05f; // 탐지 시작점 여유 거리

        private readonly Transform playerTransform; // 플레이어 위치 기준 Transform
        private readonly CharacterController characterController; // 플레이어 충돌 제어기
        private readonly LayerMask collisionLayers; // 지형 탐지 대상 레이어
        private readonly float groundProbeDistance; // 지면 탐지 거리
        private readonly float cornerProbeDistance; // 모서리 탐지 거리
        private readonly float cornerCorrectionStrength; // 모서리 보정 강도
        private readonly float ledgeForwardDistance; // 끝자락 전방 탐지 거리
        private readonly float ledgeTopSearchHeight; // 끝자락 윗면 검색 높이
        private readonly RaycastHit[] groundHitBuffer = new RaycastHit[HitBufferCapacity]; // 지면 탐지 결과 버퍼
        private readonly RaycastHit[] forwardHitBuffer = new RaycastHit[HitBufferCapacity]; // 전방 탐지 결과 버퍼
        private readonly RaycastHit[] clearanceHitBuffer = new RaycastHit[HitBufferCapacity]; // 상단 공간 탐지 결과 버퍼
        private readonly RaycastHit[] topHitBuffer = new RaycastHit[HitBufferCapacity]; // 끝자락 윗면 탐지 결과 버퍼

        public Vector3 GroundNormal { get; private set; } // 현재 지면 법선
        public float GroundSlopeAngle { get; private set; } // 현재 지면 경사 각도
        public bool IsOnWalkableSlope { get; private set; } // 이동 가능한 경사면 여부
        public bool IsNearCorner { get; private set; } // 이동 방향의 모서리 감지 여부
        public bool IsLedgeDetected { get; private set; } // 올라올 수 있는 끝자락 감지 여부
        public Vector3 LedgePoint { get; private set; } // 감지된 끝자락 윗면 위치
        public Vector3 LedgeNormal { get; private set; } // 감지된 끝자락 벽 법선

        public PlayerTraversalProbe(Transform playerTransform, CharacterController characterController, LayerMask collisionLayers, float groundProbeDistance, float cornerProbeDistance, float cornerCorrectionStrength, float ledgeForwardDistance, float ledgeTopSearchHeight) // 지형 탐지 설정 기반 탐지기 생성
        { // 탐지기 생성 범위
            this.playerTransform = playerTransform; // 플레이어 Transform 저장
            this.characterController = characterController; // 캐릭터 충돌 제어기 저장
            this.collisionLayers = collisionLayers; // 지형 탐지 레이어 저장
            this.groundProbeDistance = Mathf.Max(0.01f, groundProbeDistance); // 지면 탐지 거리 보정
            this.cornerProbeDistance = Mathf.Max(0.01f, cornerProbeDistance); // 모서리 탐지 거리 보정
            this.cornerCorrectionStrength = Mathf.Clamp01(cornerCorrectionStrength); // 모서리 보정 강도 보정
            this.ledgeForwardDistance = Mathf.Max(0.01f, ledgeForwardDistance); // 끝자락 전방 거리 보정
            this.ledgeTopSearchHeight = Mathf.Max(0.01f, ledgeTopSearchHeight); // 끝자락 윗면 검색 높이 보정
            Reset(); // 초기 탐지 상태 적용
        } // 탐지기 생성 범위 종료

        public void Tick(Vector3 desiredDirection) // 현재 이동 방향 기반 지형 탐지 갱신
        { // 지형 탐지 갱신 범위
            UpdateGroundState(); // 현재 지면과 경사 상태 갱신
            UpdateLedgeState(desiredDirection); // 현재 방향의 끝자락 상태 갱신
        } // 지형 탐지 갱신 범위 종료

        public Vector3 CorrectDirectionAroundCorner(Vector3 desiredDirection) // 전방 장애물 기반 모서리 보정 방향 반환
        { // 모서리 보정 범위
            IsNearCorner = false; // 이전 모서리 감지 결과 제거

            if (desiredDirection.sqrMagnitude <= DirectionThreshold) // 유효 이동 방향 없음 확인
            { // 방향 없음 범위
                return Vector3.zero; // 보정 없는 방향 반환
            } // 방향 없음 범위 종료

            Vector3 horizontalDirection = Vector3.ProjectOnPlane(desiredDirection, Vector3.up).normalized; // 수평 탐지 방향 계산
            Vector3 origin = playerTransform.position + Vector3.up * characterController.height * 0.5f; // 몸통 중심 탐지 시작점 계산
            float distance = characterController.radius + cornerProbeDistance; // 충돌체 반지름을 포함한 모서리 탐지 거리 계산

            if (!TryFindClosestHit(origin, horizontalDirection, distance, forwardHitBuffer, out RaycastHit cornerHit)) // 전방 장애물 감지 여부 확인
            { // 모서리 없음 범위
                return horizontalDirection; // 원래 수평 방향 반환
            } // 모서리 없음 범위 종료

            float wallAngle = Vector3.Angle(cornerHit.normal, Vector3.up); // 감지 표면의 수직 벽 각도 계산

            if (wallAngle <= characterController.slopeLimit) // 이동 가능한 경사 표면 확인
            { // 경사 표면 범위
                return horizontalDirection; // 모서리 보정 없이 원래 방향 반환
            } // 경사 표면 범위 종료

            IsNearCorner = true; // 이동 방향 모서리 감지 기록
            return PlayerTraversalMath.CalculateCornerCorrectedDirection(horizontalDirection, cornerHit.normal, cornerCorrectionStrength); // 장애물 표면을 따르는 보정 방향 반환
        } // 모서리 보정 범위 종료

        public Vector3 AlignVelocityToGround(Vector3 velocity, bool canUseGroundAlignment) // 접지 상태 기반 경사 이동 속도 반환
        { // 경사 속도 적용 범위
            if (!canUseGroundAlignment || !IsOnWalkableSlope) // 경사 정렬 사용 가능 여부 확인
            { // 경사 정렬 제외 범위
                return velocity; // 기존 이동 속도 반환
            } // 경사 정렬 제외 범위 종료

            return PlayerTraversalMath.AlignVelocityToGround(velocity, GroundNormal, characterController.slopeLimit); // 지면 법선을 따르는 이동 속도 반환
        } // 경사 속도 적용 범위 종료

        public void Reset() // 모든 지형 탐지 상태 초기화
        { // 탐지 상태 초기화 범위
            GroundNormal = Vector3.up; // 기본 지면 법선 적용
            GroundSlopeAngle = 0f; // 지면 경사 각도 제거
            IsOnWalkableSlope = false; // 경사면 상태 제거
            IsNearCorner = false; // 모서리 상태 제거
            IsLedgeDetected = false; // 끝자락 상태 제거
            LedgePoint = Vector3.zero; // 끝자락 위치 제거
            LedgeNormal = Vector3.zero; // 끝자락 벽 법선 제거
        } // 탐지 상태 초기화 범위 종료

        private void UpdateGroundState() // 아래 방향 탐지 기반 지면 상태 갱신
        { // 지면 상태 갱신 범위
            Vector3 origin = playerTransform.position + Vector3.up * groundProbeDistance; // 발 위쪽 지면 탐지 시작점 계산
            float distance = groundProbeDistance * 2f + characterController.skinWidth; // 지면 탐지 전체 거리 계산

            if (!TryFindClosestHit(origin, Vector3.down, distance, groundHitBuffer, out RaycastHit groundHit)) // 아래쪽 지면 감지 여부 확인
            { // 지면 없음 범위
                GroundNormal = Vector3.up; // 기본 지면 법선 복원
                GroundSlopeAngle = 0f; // 지면 경사 각도 제거
                IsOnWalkableSlope = false; // 경사면 상태 제거
                return; // 지면 상태 갱신 종료
            } // 지면 없음 범위 종료

            GroundNormal = groundHit.normal.normalized; // 감지된 지면 법선 저장
            GroundSlopeAngle = Vector3.Angle(GroundNormal, Vector3.up); // 감지된 지면 경사 각도 저장
            IsOnWalkableSlope = GroundSlopeAngle > 0.01f && PlayerTraversalMath.IsWalkableSlope(GroundNormal, characterController.slopeLimit); // 평지가 아닌 이동 가능한 경사 여부 저장
        } // 지면 상태 갱신 범위 종료

        private void UpdateLedgeState(Vector3 desiredDirection) // 전방 벽과 상단 공간 기반 끝자락 상태 갱신
        { // 끝자락 상태 갱신 범위
            IsLedgeDetected = false; // 이전 끝자락 감지 결과 제거
            LedgePoint = Vector3.zero; // 이전 끝자락 위치 제거
            LedgeNormal = Vector3.zero; // 이전 끝자락 법선 제거
            Vector3 horizontalDirection = Vector3.ProjectOnPlane(desiredDirection, Vector3.up); // 끝자락 전방 탐지 방향 계산

            if (horizontalDirection.sqrMagnitude <= DirectionThreshold) // 유효 이동 방향 없음 확인
            { // 방향 없음 범위
                return; // 끝자락 탐지 생략
            } // 방향 없음 범위 종료

            horizontalDirection.Normalize(); // 끝자락 전방 방향 정규화
            float forwardDistance = characterController.radius + ledgeForwardDistance; // 충돌체 반지름을 포함한 전방 탐지 거리 계산
            float lowerHeight = Mathf.Max(characterController.radius, characterController.height * 0.5f); // 몸통 벽 탐지 높이 계산
            Vector3 lowerOrigin = playerTransform.position + Vector3.up * lowerHeight; // 몸통 벽 탐지 시작점 계산

            if (!TryFindClosestHit(lowerOrigin, horizontalDirection, forwardDistance, forwardHitBuffer, out RaycastHit wallHit)) // 몸통 높이 전방 벽 감지 여부 확인
            { // 전방 벽 없음 범위
                return; // 끝자락 탐지 생략
            } // 전방 벽 없음 범위 종료

            if (PlayerTraversalMath.IsWalkableSlope(wallHit.normal, characterController.slopeLimit)) // 감지 표면의 이동 가능한 경사 여부 확인
            { // 경사 표면 범위
                return; // 경사를 끝자락 벽에서 제외
            } // 경사 표면 범위 종료

            float upperHeight = characterController.height + ProbeOriginPadding; // 머리 위 공간 탐지 높이 계산
            Vector3 upperOrigin = playerTransform.position + Vector3.up * upperHeight; // 머리 위 공간 탐지 시작점 계산

            if (TryFindClosestHit(upperOrigin, horizontalDirection, forwardDistance, clearanceHitBuffer, out RaycastHit unusedClearanceHit)) // 머리 위 전방 장애물 확인
            { // 상단 공간 차단 범위
                return; // 막힌 위치의 끝자락 감지 제외
            } // 상단 공간 차단 범위 종료

            Vector3 topOrigin = wallHit.point + horizontalDirection * (characterController.radius + ProbeOriginPadding) + Vector3.up * ledgeTopSearchHeight; // 벽 너머 윗면 탐지 시작점 계산
            float topSearchDistance = ledgeTopSearchHeight + characterController.height; // 윗면 아래 방향 검색 거리 계산

            if (!TryFindClosestHit(topOrigin, Vector3.down, topSearchDistance, topHitBuffer, out RaycastHit topHit)) // 벽 너머 윗면 감지 여부 확인
            { // 윗면 없음 범위
                return; // 끝자락 탐지 생략
            } // 윗면 없음 범위 종료

            if (!PlayerTraversalMath.IsWalkableSlope(topHit.normal, characterController.slopeLimit)) // 끝자락 윗면 이동 가능 여부 확인
            { // 가파른 윗면 범위
                return; // 올라올 수 없는 윗면 제외
            } // 가파른 윗면 범위 종료

            if (topHit.point.y <= playerTransform.position.y + ProbeOriginPadding) // 플레이어 발보다 낮은 윗면 확인
            { // 낮은 윗면 범위
                return; // 끝자락 후보에서 제외
            } // 낮은 윗면 범위 종료

            IsLedgeDetected = true; // 올라올 수 있는 끝자락 감지 기록
            LedgePoint = topHit.point; // 끝자락 윗면 위치 저장
            LedgeNormal = wallHit.normal.normalized; // 끝자락 벽 법선 저장
        } // 끝자락 상태 갱신 범위 종료

        private bool TryFindClosestHit(Vector3 origin, Vector3 direction, float distance, RaycastHit[] hitBuffer, out RaycastHit closestHit) // 자기 충돌체를 제외한 가장 가까운 탐지 결과 반환
        { // 가장 가까운 충돌 검색 범위
            int hitCount = Physics.RaycastNonAlloc(origin, direction, hitBuffer, distance, collisionLayers, QueryTriggerInteraction.Ignore); // 할당 없는 물리 광선 탐지 실행
            float closestDistance = float.PositiveInfinity; // 가장 가까운 거리 초기화
            closestHit = default; // 가장 가까운 충돌 결과 초기화
            bool foundHit = false; // 유효 충돌 탐지 여부 초기화

            for (int index = 0; index < hitCount; index++) // 반환된 충돌 결과 순회
            { // 충돌 결과 순회 범위
                RaycastHit currentHit = hitBuffer[index]; // 현재 충돌 결과 조회

                if (currentHit.collider == null) // 빈 충돌체 결과 확인
                { // 빈 결과 범위
                    continue; // 빈 결과 건너뛰기
                } // 빈 결과 범위 종료

                if (currentHit.transform == playerTransform || currentHit.transform.IsChildOf(playerTransform)) // 플레이어 소유 충돌체 확인
                { // 자기 충돌 범위
                    continue; // 자기 충돌체 제외
                } // 자기 충돌 범위 종료

                if (currentHit.distance >= closestDistance) // 기존 결과보다 먼 충돌 확인
                { // 먼 충돌 범위
                    continue; // 먼 충돌 결과 제외
                } // 먼 충돌 범위 종료

                closestDistance = currentHit.distance; // 가장 가까운 거리 갱신
                closestHit = currentHit; // 가장 가까운 충돌 결과 갱신
                foundHit = true; // 유효 충돌 탐지 기록
            } // 충돌 결과 순회 범위 종료

            return foundHit; // 가장 가까운 유효 충돌 존재 여부 반환
        } // 가장 가까운 충돌 검색 범위 종료
    } // 지형 탐지 범위 종료
} // 플레이어 기능 범위 종료
