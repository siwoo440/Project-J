using System; // 문자열 비교 기능 참조
using UnityEngine; // Unity 벡터와 수학 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    public static class MapModuleValidationRules // 맵 모듈 공통 검증 규칙 선언
    { // 맵 모듈 공통 검증 규칙 묶음
        private const float MinimumBoundsAxis = 0.1f; // 모듈 영역 축 최소 크기
        private const float HeightTolerance = 0.01f; // 높이 비교 허용 오차

        public static Vector3 ClampBoundsSize(Vector3 boundsSize) // 모듈 영역 크기 보정
        { // 모듈 영역 크기 보정 처리
            return new Vector3(Mathf.Max(MinimumBoundsAxis, boundsSize.x), Mathf.Max(MinimumBoundsAxis, boundsSize.y), Mathf.Max(MinimumBoundsAxis, boundsSize.z)); // 모든 축 양수 크기 반환
        } // 모듈 영역 크기 보정 종료

        public static bool IsValidModuleId(string moduleId) // 모듈 ID 형식 검사
        { // 모듈 ID 형식 검사 처리
            return !string.IsNullOrWhiteSpace(moduleId) && moduleId.Trim().StartsWith("MAP-", StringComparison.Ordinal); // MAP 접두사 포함 여부 반환
        } // 모듈 ID 형식 검사 종료

        public static Vector3 ToLocalVector(MapConnectionDirection direction) // 연결 방향을 로컬 벡터로 변환
        { // 연결 방향 변환 처리
            switch (direction) // 연결 방향 선택
            { // 연결 방향 분기 처리
                case MapConnectionDirection.East: // 오른쪽 방향 확인
                    return Vector3.right; // 오른쪽 벡터 반환
                case MapConnectionDirection.South: // 뒤쪽 방향 확인
                    return Vector3.back; // 뒤쪽 벡터 반환
                case MapConnectionDirection.West: // 왼쪽 방향 확인
                    return Vector3.left; // 왼쪽 벡터 반환
                default: // 앞쪽 방향 확인
                    return Vector3.forward; // 앞쪽 벡터 반환
            } // 연결 방향 분기 처리 종료
        } // 연결 방향 변환 종료

        public static MapConnectionDirection RotateDirection(MapConnectionDirection direction, int clockwiseQuarterTurns) // 직각 회전 후 연결 방향 계산
        { // 회전 방향 계산 처리
            int normalizedTurns = ((clockwiseQuarterTurns % 4) + 4) % 4; // 음수를 포함한 회전 횟수 정규화
            int rotatedIndex = ((int)direction + normalizedTurns) % 4; // 회전된 열거형 위치 계산
            return (MapConnectionDirection)rotatedIndex; // 회전된 연결 방향 반환
        } // 회전 방향 계산 종료

        public static bool AreDirectionsCompatible(MapConnectionDirection exitDirection, MapConnectionDirection entranceDirection) // 출구와 입구 방향 호환성 검사
        { // 연결 방향 호환성 검사 처리
            return RotateDirection(exitDirection, 2) == entranceDirection; // 서로 마주 보는 방향 여부 반환
        } // 연결 방향 호환성 검사 종료

        public static Quaternion CalculateConnectionGizmoRotation(Vector3 worldDirection) // 연결 방향 기반 기즈모 회전 계산
        { // 기즈모 회전 계산 범위
            Vector3 safeDirection = worldDirection.sqrMagnitude > 0.0001f ? worldDirection.normalized : Vector3.forward; // 유효한 연결 방향 계산
            return Quaternion.LookRotation(safeDirection, Vector3.up); // 연결 방향을 향하는 회전 반환
        } // 기즈모 회전 계산 종료

        public static float CalculateSafeJumpDistance(float moveSpeed, float jumpHeight, float gravityMagnitude, float safetyRatio) // 플레이어 수치 기반 안전 점프 거리 계산
        { // 안전 점프 거리 계산 처리
            float safeMoveSpeed = Mathf.Max(0f, moveSpeed); // 음수가 없는 이동 속도 계산
            float safeJumpHeight = Mathf.Max(0f, jumpHeight); // 음수가 없는 점프 높이 계산
            float safeGravityMagnitude = Mathf.Max(0.01f, gravityMagnitude); // 0이 아닌 중력 크기 계산
            float safeRatio = Mathf.Clamp01(safetyRatio); // 안전 비율 범위 제한
            float airborneTime = 2f * Mathf.Sqrt(2f * safeJumpHeight / safeGravityMagnitude); // 같은 높이 착지 기준 체공 시간 계산
            return safeMoveSpeed * airborneTime * safeRatio; // 안전 비율이 적용된 수평 거리 반환
        } // 안전 점프 거리 계산 종료

        public static bool IsCrouchPassageValid(float clearanceHeight, float crouchingHeight, float standingHeight, float padding) // 낮은 통로 앉기 통과 조건 검사
        { // 낮은 통로 검사 처리
            float safePadding = Mathf.Max(0f, padding); // 음수가 없는 통로 여유 계산
            float minimumHeight = Mathf.Max(0f, crouchingHeight) + safePadding; // 앉아서 통과할 최소 높이 계산
            float maximumHeight = Mathf.Max(0f, standingHeight) - safePadding; // 서서 통과하지 못할 최대 높이 계산
            return clearanceHeight + HeightTolerance >= minimumHeight && clearanceHeight < maximumHeight - HeightTolerance; // 앉기 전용 통로 범위 여부 반환
        } // 낮은 통로 검사 종료

        public static bool IsJumpPassageValid(float jumpDistance, float jumpRise, float maximumSafeJumpDistance, float maximumSafeJumpRise, float maximumSafeDropHeight) // 점프 구간 통과 조건 검사
        { // 점프 구간 검사 처리
            float safeDropHeight = Mathf.Max(0f, maximumSafeDropHeight); // 음수가 없는 안전 낙하 높이 계산
            bool distanceIsValid = jumpDistance >= 0f && jumpDistance <= maximumSafeJumpDistance + HeightTolerance; // 안전 거리 이내 여부 계산
            bool riseIsValid = jumpRise <= maximumSafeJumpRise + HeightTolerance; // 안전 상승 높이 이내 여부 계산
            bool dropIsValid = jumpRise >= -safeDropHeight - HeightTolerance; // 안전 낙하 높이 이내 여부 계산
            return distanceIsValid && riseIsValid && dropIsValid; // 거리와 상승과 낙하 통합 결과 반환
        } // 점프 구간 검사 종료

        public static bool TryValidateModule(MapModuleDefinition module, MapTraversalProfile traversalProfile, out string reason) // 모듈 전체 데이터 검사
        { // 모듈 전체 데이터 검사 처리
            if (module == null) // 모듈 참조 누락 확인
            { // 모듈 참조 누락 처리
                reason = "맵 모듈 참조가 없습니다."; // 모듈 누락 사유 저장
                return false; // 모듈 검사 실패 반환
            } // 모듈 참조 누락 처리 종료

            if (!IsValidModuleId(module.ModuleId)) // 모듈 ID 형식 확인
            { // 모듈 ID 오류 처리
                reason = "모듈 ID는 MAP- 접두사로 시작해야 합니다."; // 모듈 ID 오류 사유 저장
                return false; // 모듈 검사 실패 반환
            } // 모듈 ID 오류 처리 종료

            if (traversalProfile == null) // 이동 능력 기준 누락 확인
            { // 이동 능력 기준 누락 처리
                reason = "Map Traversal Profile이 연결되지 않았습니다."; // 이동 능력 기준 누락 사유 저장
                return false; // 모듈 검사 실패 반환
            } // 이동 능력 기준 누락 처리 종료

            if (module.AllowedRotations == MapRotationOptions.None) // 허용 회전값 누락 확인
            { // 허용 회전값 누락 처리
                reason = "최소 한 개의 회전값을 허용해야 합니다."; // 회전값 누락 사유 저장
                return false; // 모듈 검사 실패 반환
            } // 허용 회전값 누락 처리 종료

            MapModuleConnectionPoint[] points = module.ConnectionPoints; // 모듈 연결 지점 목록 조회

            if (points == null || points.Length < 2) // 입구와 출구 수량 확인
            { // 연결 지점 수량 오류 처리
                reason = "입구와 출구를 포함한 연결 지점이 최소 2개 필요합니다."; // 연결 지점 부족 사유 저장
                return false; // 모듈 검사 실패 반환
            } // 연결 지점 수량 오류 처리 종료

            int entranceCount = 0; // 입구 개수 초기화
            int exitCount = 0; // 출구 개수 초기화

            for (int index = 0; index < points.Length; index++) // 모든 연결 지점 순회
            { // 연결 지점 순회 처리
                MapModuleConnectionPoint point = points[index]; // 현재 연결 지점 조회

                if (point == null) // 빈 연결 지점 확인
                { // 빈 연결 지점 처리
                    reason = "연결 지점 목록에 빈 항목이 있습니다."; // 빈 연결 지점 사유 저장
                    return false; // 모듈 검사 실패 반환
                } // 빈 연결 지점 처리 종료

                if (string.IsNullOrWhiteSpace(point.ConnectionId)) // 연결 지점 ID 누락 확인
                { // 연결 ID 누락 처리
                    reason = "모든 연결 지점에 ID가 필요합니다."; // 연결 ID 누락 사유 저장
                    return false; // 모듈 검사 실패 반환
                } // 연결 ID 누락 처리 종료

                for (int compareIndex = index + 1; compareIndex < points.Length; compareIndex++) // 뒤쪽 연결 지점 ID 비교
                { // 연결 ID 중복 검사 처리
                    if (points[compareIndex] != null && string.Equals(point.ConnectionId, points[compareIndex].ConnectionId, StringComparison.OrdinalIgnoreCase)) // 대소문자 무시 중복 ID 확인
                    { // 연결 ID 중복 처리
                        reason = $"중복된 연결 지점 ID가 있습니다: {point.ConnectionId}"; // 중복 연결 ID 사유 저장
                        return false; // 모듈 검사 실패 반환
                    } // 연결 ID 중복 처리 종료
                } // 연결 ID 중복 검사 종료

                entranceCount += point.Role == MapConnectionRole.Entrance ? 1 : 0; // 입구 개수 누적
                exitCount += point.Role == MapConnectionRole.Exit ? 1 : 0; // 출구 개수 누적
            } // 연결 지점 순회 종료

            if (entranceCount < 1 || exitCount < 1) // 입구 또는 출구 누락 확인
            { // 입구 또는 출구 누락 처리
                reason = "모듈에는 입구와 출구가 각각 하나 이상 필요합니다."; // 역할 누락 사유 저장
                return false; // 모듈 검사 실패 반환
            } // 입구 또는 출구 누락 처리 종료

            if (module.TraversalRequirement == MapTraversalRequirement.Crouch && !IsCrouchPassageValid(module.RequiredClearanceHeight, traversalProfile.CrouchingHeight, traversalProfile.StandingHeight, traversalProfile.ClearancePadding)) // 낮은 통로 수치 확인
            { // 낮은 통로 오류 처리
                reason = $"낮은 통로 높이는 {traversalProfile.MinimumCrouchClearance:0.00}m 이상 {traversalProfile.MaximumCrouchOnlyClearance:0.00}m 미만이어야 합니다."; // 낮은 통로 오류 사유 저장
                return false; // 모듈 검사 실패 반환
            } // 낮은 통로 오류 처리 종료

            if (module.TraversalRequirement == MapTraversalRequirement.Jump && !IsJumpPassageValid(module.RequiredJumpDistance, module.RequiredJumpRise, traversalProfile.MaximumSafeJumpDistance, traversalProfile.MaximumSafeJumpRise, traversalProfile.MaximumSafeDropHeight)) // 점프 구간 수치 확인
            { // 점프 구간 오류 처리
                reason = $"점프 구간이 안전 범위를 넘었습니다. 최대 거리 {traversalProfile.MaximumSafeJumpDistance:0.00}m, 최대 상승 {traversalProfile.MaximumSafeJumpRise:0.00}m, 최대 낙하 {traversalProfile.MaximumSafeDropHeight:0.00}m"; // 점프 구간 오류 사유 저장
                return false; // 모듈 검사 실패 반환
            } // 점프 구간 오류 처리 종료

            reason = string.Empty; // 검사 성공 사유 초기화
            return true; // 모듈 검사 성공 반환
        } // 모듈 전체 데이터 검사 종료
    } // 맵 모듈 공통 검증 규칙 묶음 종료
} // 맵 생성 기능 묶음 종료

