using UnityEngine; // Unity 벡터와 수학 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 끝자락 올라오기 범위
    public enum PlayerLedgeClimbState // 끝자락 올라오기 진행 상태 종류
    { // 올라오기 상태 범위
        Idle, // 올라오기 대기 상태
        Lifting, // 몸을 끝자락 위로 올리는 상태
        Advancing // 발판 안쪽으로 이동하는 상태
    } // 올라오기 상태 범위 종료

    public sealed class PlayerLedgeClimbController // 끝자락 올라오기 경로 제어기 선언
    { // 올라오기 경로 제어 범위
        private const float PositionThreshold = 0.0001f; // 위치와 방향 유효성 판정 기준

        private Vector3 phaseStartPosition; // 현재 단계 시작 위치
        private Vector3 phaseTargetPosition; // 현재 단계 목표 위치
        private Vector3 landingPosition; // 최종 착지 위치
        private float phaseElapsedTime; // 현재 단계 경과 시간
        private float phaseDuration; // 현재 단계 목표 시간
        private float advancingDuration; // 전진 단계 목표 시간

        public PlayerLedgeClimbState CurrentState { get; private set; } // 현재 올라오기 상태
        public bool IsClimbing => CurrentState != PlayerLedgeClimbState.Idle; // 올라오기 진행 여부 반환
        public bool CompletedThisTick { get; private set; } // 현재 갱신의 올라오기 완료 여부
        public Vector3 LiftPosition { get; private set; } // 몸 올리기 목표 위치
        public Vector3 LandingPosition => landingPosition; // 최종 착지 목표 위치 반환

        public bool TryBegin(Vector3 currentPosition, Vector3 ledgePoint, Vector3 wallNormal, float wallClearance, float landingDepth, float footClearance, float liftingDuration, float forwardDuration) // 감지 결과 기반 끝자락 올라오기 시작 시도
        { // 올라오기 시작 범위
            if (IsClimbing) // 이미 올라오기 진행 중인지 확인
            { // 중복 시작 차단 범위
                return false; // 중복 시작 실패 반환
            } // 중복 시작 차단 범위 종료

            Vector3 horizontalNormal = Vector3.ProjectOnPlane(wallNormal, Vector3.up); // 벽 법선의 수평 성분 계산

            if (horizontalNormal.sqrMagnitude <= PositionThreshold) // 유효 벽 법선 없음 확인
            { // 벽 법선 누락 범위
                return false; // 올라오기 시작 실패 반환
            } // 벽 법선 누락 범위 종료

            horizontalNormal.Normalize(); // 벽 바깥 방향 정규화
            float safeWallClearance = Mathf.Max(0f, wallClearance); // 음수가 아닌 벽 여유 거리 보정
            float safeLandingDepth = Mathf.Max(0f, landingDepth); // 음수가 아닌 발판 진입 거리 보정
            float safeFootClearance = Mathf.Max(0f, footClearance); // 음수가 아닌 발 높이 여유 보정
            LiftPosition = ledgePoint + horizontalNormal * safeWallClearance + Vector3.up * safeFootClearance; // 벽 바깥쪽 몸 올리기 목표 계산
            landingPosition = ledgePoint - horizontalNormal * safeLandingDepth + Vector3.up * safeFootClearance; // 발판 안쪽 최종 착지 목표 계산
            phaseStartPosition = currentPosition; // 몸 올리기 시작 위치 저장
            phaseTargetPosition = LiftPosition; // 몸 올리기 목표 위치 저장
            phaseElapsedTime = 0f; // 현재 단계 경과 시간 초기화
            phaseDuration = Mathf.Max(0.01f, liftingDuration); // 몸 올리기 단계 시간 보정
            advancingDuration = Mathf.Max(0.01f, forwardDuration); // 발판 진입 단계 시간 보정
            CurrentState = PlayerLedgeClimbState.Lifting; // 몸 올리기 상태 적용
            CompletedThisTick = false; // 완료 결과 초기화
            return true; // 올라오기 시작 성공 반환
        } // 올라오기 시작 범위 종료

        public Vector3 Tick(float deltaTime, Vector3 currentPosition) // 프레임 시간 기반 올라오기 이동량 계산
        { // 올라오기 갱신 범위
            CompletedThisTick = false; // 현재 갱신 완료 결과 초기화

            if (!IsClimbing) // 올라오기 미진행 상태 확인
            { // 올라오기 대기 범위
                return Vector3.zero; // 이동 없는 결과 반환
            } // 올라오기 대기 범위 종료

            float safeDeltaTime = Mathf.Max(0f, deltaTime); // 음수가 아닌 프레임 시간 보정
            phaseElapsedTime = Mathf.Min(phaseDuration, phaseElapsedTime + safeDeltaTime); // 현재 단계 경과 시간 증가
            float normalizedTime = phaseDuration <= 0f ? 1f : Mathf.Clamp01(phaseElapsedTime / phaseDuration); // 현재 단계 진행 비율 계산
            float smoothedTime = normalizedTime * normalizedTime * (3f - 2f * normalizedTime); // 부드러운 시작과 종료 곡선 계산
            Vector3 desiredPosition = Vector3.LerpUnclamped(phaseStartPosition, phaseTargetPosition, smoothedTime); // 현재 단계 목표 위치 보간
            Vector3 movement = desiredPosition - currentPosition; // 실제 현재 위치 기준 이동량 계산

            if (normalizedTime >= 1f) // 현재 단계 완료 여부 확인
            { // 단계 완료 범위
                AdvanceState(desiredPosition); // 다음 올라오기 단계로 전환
            } // 단계 완료 범위 종료

            return movement; // 현재 프레임 올라오기 이동량 반환
        } // 올라오기 갱신 범위 종료

        public void Cancel() // 진행 중인 올라오기 취소
        { // 올라오기 취소 범위
            Reset(); // 모든 올라오기 상태 초기화
        } // 올라오기 취소 범위 종료

        public void Reset() // 올라오기 상태 전체 초기화
        { // 올라오기 초기화 범위
            CurrentState = PlayerLedgeClimbState.Idle; // 대기 상태 적용
            CompletedThisTick = false; // 완료 결과 제거
            LiftPosition = Vector3.zero; // 몸 올리기 목표 제거
            landingPosition = Vector3.zero; // 최종 착지 목표 제거
            phaseStartPosition = Vector3.zero; // 단계 시작 위치 제거
            phaseTargetPosition = Vector3.zero; // 단계 목표 위치 제거
            phaseElapsedTime = 0f; // 단계 경과 시간 제거
            phaseDuration = 0f; // 단계 목표 시간 제거
            advancingDuration = 0f; // 전진 목표 시간 제거
        } // 올라오기 초기화 범위 종료

        public static bool IsHeightReachable(float currentFeetHeight, float ledgeHeight, float minimumHeight, float maximumHeight) // 끝자락 높이의 올라오기 가능 범위 판정
        { // 올라오기 높이 판정 범위
            float heightDifference = ledgeHeight - currentFeetHeight; // 발 위치 대비 끝자락 높이 차이 계산
            float safeMinimumHeight = Mathf.Max(0f, minimumHeight); // 음수가 아닌 최소 높이 보정
            float safeMaximumHeight = Mathf.Max(safeMinimumHeight, maximumHeight); // 최소 이상 최대 높이 보정
            return heightDifference >= safeMinimumHeight && heightDifference <= safeMaximumHeight; // 허용 높이 범위 포함 여부 반환
        } // 올라오기 높이 판정 범위 종료

        public static bool IsApproachingWall(Vector3 desiredDirection, Vector3 wallNormal, float minimumDot) // 입력이 벽을 향하는지 판정
        { // 벽 접근 판정 범위
            Vector3 horizontalDirection = Vector3.ProjectOnPlane(desiredDirection, Vector3.up); // 입력 방향의 수평 성분 계산
            Vector3 horizontalNormal = Vector3.ProjectOnPlane(wallNormal, Vector3.up); // 벽 법선의 수평 성분 계산

            if (horizontalDirection.sqrMagnitude <= PositionThreshold || horizontalNormal.sqrMagnitude <= PositionThreshold) // 유효 입력과 벽 법선 확인
            { // 방향 누락 범위
                return false; // 벽 접근 아님 반환
            } // 방향 누락 범위 종료

            float approachDot = Vector3.Dot(horizontalDirection.normalized, -horizontalNormal.normalized); // 벽 안쪽 방향과 입력 방향 일치도 계산
            return approachDot >= Mathf.Clamp(minimumDot, -1f, 1f); // 최소 접근 일치도 충족 여부 반환
        } // 벽 접근 판정 범위 종료

        private void AdvanceState(Vector3 completedPosition) // 완료된 단계에서 다음 상태로 전환
        { // 올라오기 단계 전환 범위
            if (CurrentState == PlayerLedgeClimbState.Lifting) // 몸 올리기 단계 완료 확인
            { // 발판 진입 전환 범위
                CurrentState = PlayerLedgeClimbState.Advancing; // 발판 안쪽 이동 상태 적용
                phaseStartPosition = completedPosition; // 전진 단계 시작 위치 저장
                phaseTargetPosition = landingPosition; // 전진 단계 목표 위치 저장
                phaseElapsedTime = 0f; // 전진 단계 경과 시간 초기화
                phaseDuration = advancingDuration; // 전진 단계 목표 시간 적용
                return; // 최종 완료 처리 생략
            } // 발판 진입 전환 범위 종료

            CurrentState = PlayerLedgeClimbState.Idle; // 올라오기 완료 뒤 대기 상태 적용
            CompletedThisTick = true; // 현재 갱신의 완료 결과 기록
        } // 올라오기 단계 전환 범위 종료
    } // 올라오기 경로 제어 범위 종료
} // 끝자락 올라오기 범위 종료
