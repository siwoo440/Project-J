using System; // 문자열 비교 기능 참조
using UnityEngine; // Unity 수학과 좌표 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    public static class MapVerticalModuleValidationRules // 수직 모듈 공통 검증 규칙 선언
    { // 수직 모듈 공통 검증 규칙 묶음
        private const float HeightTolerance = 0.01f; // 높이 비교 허용 오차
        private const float MaximumWalkStepRise = 0.3f; // 걷기로 넘을 단일 계단 최대 높이

        public static bool TryValidateVerticalModule(MapModuleDefinition module, MapVerticalModuleData verticalData, out string reason) // 수직 모듈 전체 데이터 검사
        { // 수직 모듈 전체 데이터 검사 처리
            if (module == null) // 기본 모듈 정의 누락 확인
            { // 기본 모듈 정의 누락 처리
                reason = "Map Module Definition이 연결되지 않았습니다."; // 기본 모듈 누락 사유 저장
                return false; // 수직 모듈 검사 실패 반환
            } // 기본 모듈 정의 누락 처리 종료

            if (verticalData == null) // 수직 모듈 데이터 누락 확인
            { // 수직 모듈 데이터 누락 처리
                reason = "Map Vertical Module Data가 연결되지 않았습니다."; // 수직 데이터 누락 사유 저장
                return false; // 수직 모듈 검사 실패 반환
            } // 수직 모듈 데이터 누락 처리 종료

            if (verticalData.LayoutKind == MapVerticalLayoutKind.Flat) // 상승 형태 미지정 확인
            { // 상승 형태 미지정 처리
                reason = "수직 모듈 형태는 Flat이 아닌 상승 형태여야 합니다."; // 상승 형태 오류 사유 저장
                return false; // 수직 모듈 검사 실패 반환
            } // 상승 형태 미지정 처리 종료

            if (verticalData.TraversalProfile == null) // 이동 능력 기준 누락 확인
            { // 이동 능력 기준 누락 처리
                reason = "Map Traversal Profile이 연결되지 않았습니다."; // 이동 능력 기준 누락 사유 저장
                return false; // 수직 모듈 검사 실패 반환
            } // 이동 능력 기준 누락 처리 종료

            if (module.TraversalProfile != verticalData.TraversalProfile) // 기본과 수직 이동 기준 불일치 확인
            { // 이동 기준 불일치 처리
                reason = "기본 모듈과 수직 모듈의 Map Traversal Profile이 다릅니다."; // 이동 기준 불일치 사유 저장
                return false; // 수직 모듈 검사 실패 반환
            } // 이동 기준 불일치 처리 종료

            if (verticalData.ExpectedHeightGain <= HeightTolerance) // 예상 상승량 부족 확인
            { // 예상 상승량 부족 처리
                reason = "상승 모듈의 예상 높이 증가량은 0보다 커야 합니다."; // 예상 상승량 오류 사유 저장
                return false; // 수직 모듈 검사 실패 반환
            } // 예상 상승량 부족 처리 종료

            if (!TryFindConnectionPoint(module, verticalData.EntranceConnectionId, out MapModuleConnectionPoint entrancePoint)) // 기준 입구 조회 실패 확인
            { // 기준 입구 조회 실패 처리
                reason = $"기준 입구 연결 지점을 찾을 수 없습니다: {verticalData.EntranceConnectionId}"; // 기준 입구 누락 사유 저장
                return false; // 수직 모듈 검사 실패 반환
            } // 기준 입구 조회 실패 처리 종료

            if (!TryFindConnectionPoint(module, verticalData.ExitConnectionId, out MapModuleConnectionPoint exitPoint)) // 기준 출구 조회 실패 확인
            { // 기준 출구 조회 실패 처리
                reason = $"기준 출구 연결 지점을 찾을 수 없습니다: {verticalData.ExitConnectionId}"; // 기준 출구 누락 사유 저장
                return false; // 수직 모듈 검사 실패 반환
            } // 기준 출구 조회 실패 처리 종료

            if (entrancePoint.Role != MapConnectionRole.Entrance || exitPoint.Role != MapConnectionRole.Exit) // 입구와 출구 역할 오류 확인
            { // 연결 역할 오류 처리
                reason = "기준 연결 지점의 역할은 Entrance와 Exit 순서여야 합니다."; // 연결 역할 오류 사유 저장
                return false; // 수직 모듈 검사 실패 반환
            } // 연결 역할 오류 처리 종료

            float actualHeightGain = CalculateConnectionHeightGain(module, entrancePoint, exitPoint); // 실제 연결 지점 상승량 계산

            if (Mathf.Abs(actualHeightGain - verticalData.ExpectedHeightGain) > HeightTolerance) // 실제와 예상 상승량 불일치 확인
            { // 실제와 예상 상승량 불일치 처리
                reason = $"연결 지점 실제 상승량 {actualHeightGain:0.00}m와 예상 상승량 {verticalData.ExpectedHeightGain:0.00}m가 다릅니다."; // 상승량 불일치 사유 저장
                return false; // 수직 모듈 검사 실패 반환
            } // 실제와 예상 상승량 불일치 처리 종료

            MapVerticalTraversalSegment[] segments = verticalData.TraversalSegments; // 수직 이동 구간 목록 조회

            if (segments == null || segments.Length == 0) // 수직 이동 구간 누락 확인
            { // 수직 이동 구간 누락 처리
                reason = "상승 모듈에는 수직 이동 구간이 하나 이상 필요합니다."; // 이동 구간 누락 사유 저장
                return false; // 수직 모듈 검사 실패 반환
            } // 수직 이동 구간 누락 처리 종료

            float segmentHeightSum = 0f; // 이동 구간 상승량 합계 초기화
            bool containsJumpSegment = false; // 점프 구간 포함 여부 초기화

            for (int segmentIndex = 0; segmentIndex < segments.Length; segmentIndex++) // 모든 수직 이동 구간 순회
            { // 수직 이동 구간 검사 처리
                MapVerticalTraversalSegment segment = segments[segmentIndex]; // 현재 수직 이동 구간 조회

                if (!TryValidateSegment(segment, verticalData.TraversalProfile, out string segmentReason)) // 현재 이동 구간 안전성 검사
                { // 현재 이동 구간 오류 처리
                    reason = $"수직 이동 구간 {segmentIndex + 1} 오류: {segmentReason}"; // 이동 구간 오류 사유 저장
                    return false; // 수직 모듈 검사 실패 반환
                } // 현재 이동 구간 오류 처리 종료

                for (int compareIndex = segmentIndex + 1; compareIndex < segments.Length; compareIndex++) // 뒤쪽 이동 구간 ID 비교
                { // 이동 구간 ID 중복 검사 처리
                    if (segments[compareIndex] != null && string.Equals(segment.SegmentId, segments[compareIndex].SegmentId, StringComparison.OrdinalIgnoreCase)) // 대소문자 무시 중복 ID 확인
                    { // 이동 구간 ID 중복 처리
                        reason = $"중복된 수직 이동 구간 ID가 있습니다: {segment.SegmentId}"; // 중복 구간 ID 사유 저장
                        return false; // 수직 모듈 검사 실패 반환
                    } // 이동 구간 ID 중복 처리 종료
                } // 이동 구간 ID 중복 검사 종료

                segmentHeightSum += segment.HeightGain; // 현재 구간 상승량 합계 누적
                containsJumpSegment |= segment.TraversalRequirement == MapTraversalRequirement.Jump; // 점프 구간 포함 여부 누적
            } // 수직 이동 구간 검사 처리 종료

            if (Mathf.Abs(segmentHeightSum - verticalData.ExpectedHeightGain) > HeightTolerance) // 구간 합계와 예상 상승량 불일치 확인
            { // 구간 상승량 합계 불일치 처리
                reason = $"수직 이동 구간 합계 {segmentHeightSum:0.00}m와 예상 상승량 {verticalData.ExpectedHeightGain:0.00}m가 다릅니다."; // 구간 합계 오류 사유 저장
                return false; // 수직 모듈 검사 실패 반환
            } // 구간 상승량 합계 불일치 처리 종료

            if ((verticalData.LayoutKind == MapVerticalLayoutKind.ZigzagRise || verticalData.LayoutKind == MapVerticalLayoutKind.JumpRise) && !containsJumpSegment) // 점프형 모듈의 점프 구간 누락 확인
            { // 점프 구간 누락 처리
                reason = "ZigzagRise와 JumpRise에는 Jump 이동 구간이 하나 이상 필요합니다."; // 점프 구간 누락 사유 저장
                return false; // 수직 모듈 검사 실패 반환
            } // 점프 구간 누락 처리 종료

            reason = string.Empty; // 검사 성공 사유 초기화
            return true; // 수직 모듈 검사 성공 반환
        } // 수직 모듈 전체 데이터 검사 종료

        public static float CalculateConnectionHeightGain(MapModuleDefinition module, MapModuleConnectionPoint entrancePoint, MapModuleConnectionPoint exitPoint) // 두 연결 지점의 모듈 기준 상승량 계산
        { // 연결 지점 상승량 계산 처리
            if (module == null || entrancePoint == null || exitPoint == null) // 계산 대상 누락 확인
            { // 계산 대상 누락 처리
                return 0f; // 안전 기본 상승량 반환
            } // 계산 대상 누락 처리 종료

            float entranceHeight = module.transform.InverseTransformPoint(entrancePoint.transform.position).y; // 입구의 모듈 기준 높이 계산
            float exitHeight = module.transform.InverseTransformPoint(exitPoint.transform.position).y; // 출구의 모듈 기준 높이 계산
            return exitHeight - entranceHeight; // 출구와 입구 높이 차이 반환
        } // 연결 지점 상승량 계산 종료

        public static bool TryFindConnectionPoint(MapModuleDefinition module, string connectionId, out MapModuleConnectionPoint result) // ID 기반 연결 지점 조회
        { // 연결 지점 조회 처리
            result = null; // 연결 지점 결과 초기화

            if (module == null || string.IsNullOrWhiteSpace(connectionId)) // 조회 조건 누락 확인
            { // 조회 조건 누락 처리
                return false; // 연결 지점 조회 실패 반환
            } // 조회 조건 누락 처리 종료

            MapModuleConnectionPoint[] points = module.ConnectionPoints; // 기본 모듈 연결 지점 목록 조회

            if (points == null) // 연결 지점 목록 누락 확인
            { // 연결 지점 목록 누락 처리
                return false; // 연결 지점 조회 실패 반환
            } // 연결 지점 목록 누락 처리 종료

            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++) // 모든 연결 지점 순회
            { // 연결 지점 ID 비교 처리
                MapModuleConnectionPoint point = points[pointIndex]; // 현재 연결 지점 조회

                if (point != null && string.Equals(point.ConnectionId, connectionId, StringComparison.Ordinal)) // 요청 ID 일치 확인
                { // 요청 ID 일치 처리
                    result = point; // 일치 연결 지점 저장
                    return true; // 연결 지점 조회 성공 반환
                } // 요청 ID 일치 처리 종료
            } // 연결 지점 ID 비교 처리 종료

            return false; // 연결 지점 조회 실패 반환
        } // 연결 지점 조회 처리 종료

        private static bool TryValidateSegment(MapVerticalTraversalSegment segment, MapTraversalProfile traversalProfile, out string reason) // 단일 수직 이동 구간 검사
        { // 단일 수직 이동 구간 검사 처리
            if (segment == null) // 이동 구간 누락 확인
            { // 이동 구간 누락 처리
                reason = "수직 이동 구간 참조가 없습니다."; // 이동 구간 누락 사유 저장
                return false; // 이동 구간 검사 실패 반환
            } // 이동 구간 누락 처리 종료

            if (string.IsNullOrWhiteSpace(segment.SegmentId)) // 이동 구간 ID 누락 확인
            { // 이동 구간 ID 누락 처리
                reason = "수직 이동 구간 ID가 필요합니다."; // 이동 구간 ID 누락 사유 저장
                return false; // 이동 구간 검사 실패 반환
            } // 이동 구간 ID 누락 처리 종료

            if (segment.HeightGain <= HeightTolerance) // 구간 상승량 부족 확인
            { // 구간 상승량 부족 처리
                reason = "상승 구간의 높이 증가량은 0보다 커야 합니다."; // 구간 상승량 오류 사유 저장
                return false; // 이동 구간 검사 실패 반환
            } // 구간 상승량 부족 처리 종료

            if (segment.TraversalRequirement == MapTraversalRequirement.Walk || segment.TraversalRequirement == MapTraversalRequirement.Crouch) // 걷기 기반 구간 확인
            { // 걷기 기반 구간 처리
                if (segment.HeightGain > MaximumWalkStepRise + HeightTolerance) // 단일 계단 높이 초과 확인
                { // 단일 계단 높이 초과 처리
                    reason = $"걷기 구간의 단일 상승량은 {MaximumWalkStepRise:0.00}m 이하여야 합니다."; // 계단 높이 오류 사유 저장
                    return false; // 이동 구간 검사 실패 반환
                } // 단일 계단 높이 초과 처리 종료

                reason = string.Empty; // 걷기 구간 검사 성공 사유 초기화
                return true; // 걷기 구간 검사 성공 반환
            } // 걷기 기반 구간 처리 종료

            bool jumpIsSafe = MapModuleValidationRules.IsJumpPassageValid(segment.HorizontalDistance, segment.HeightGain, traversalProfile.MaximumSafeJumpDistance, traversalProfile.MaximumSafeJumpRise, traversalProfile.MaximumSafeDropHeight); // 점프 또는 오르기 안전성 계산

            if (!jumpIsSafe) // 점프 또는 오르기 안전 범위 초과 확인
            { // 점프 또는 오르기 안전 범위 초과 처리
                reason = $"이동 구간이 안전 범위를 넘었습니다. 최대 거리 {traversalProfile.MaximumSafeJumpDistance:0.00}m, 최대 상승 {traversalProfile.MaximumSafeJumpRise:0.00}m"; // 안전 범위 오류 사유 저장
                return false; // 이동 구간 검사 실패 반환
            } // 점프 또는 오르기 안전 범위 초과 처리 종료

            reason = string.Empty; // 점프 구간 검사 성공 사유 초기화
            return true; // 점프 구간 검사 성공 반환
        } // 단일 수직 이동 구간 검사 종료
    } // 수직 모듈 공통 검증 규칙 묶음 종료
} // 맵 생성 기능 묶음 종료
