using UnityEngine; // Unity 벡터와 영역 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    public static class MapGenerationRules // 맵 배치 공통 규칙 선언
    { // 맵 배치 공통 규칙 묶음
        private const float DirectionTolerance = 0.001f; // 방향 비교 허용 오차

        public static bool IsRotationAllowed(MapRotationOptions options, int clockwiseQuarterTurns) // 특정 직각 회전 허용 여부 검사
        { // 직각 회전 허용 검사 처리
            int normalizedTurns = ((clockwiseQuarterTurns % 4) + 4) % 4; // 회전 횟수 0부터 3까지 정규화
            MapRotationOptions requiredOption = (MapRotationOptions)(1 << normalizedTurns); // 회전 횟수 대응 플래그 계산
            return (options & requiredOption) != 0; // 회전 플래그 포함 여부 반환
        } // 직각 회전 허용 검사 처리

        public static int[] GetAllowedQuarterTurns(MapRotationOptions options) // 허용된 직각 회전 목록 계산
        { // 허용 회전 목록 계산 처리
            int allowedCount = 0; // 허용 회전 개수 초기화

            for (int quarterTurns = 0; quarterTurns < 4; quarterTurns++) // 네 직각 회전 순회
            { // 직각 회전 순회 처리
                allowedCount += IsRotationAllowed(options, quarterTurns) ? 1 : 0; // 허용 회전 개수 누적
            } // 직각 회전 순회 처리

            int[] allowedQuarterTurns = new int[allowedCount]; // 허용 회전 결과 배열 생성
            int resultIndex = 0; // 결과 저장 위치 초기화

            for (int quarterTurns = 0; quarterTurns < 4; quarterTurns++) // 네 직각 회전 재순회
            { // 회전 결과 저장 처리
                if (!IsRotationAllowed(options, quarterTurns)) // 현재 회전 비허용 확인
                { // 비허용 회전 처리
                    continue; // 현재 회전 저장 생략
                } // 비허용 회전 처리

                allowedQuarterTurns[resultIndex] = quarterTurns; // 허용 회전 횟수 저장
                resultIndex++; // 다음 결과 저장 위치 이동
            } // 회전 결과 저장 처리

            return allowedQuarterTurns; // 허용 직각 회전 목록 반환
        } // 허용 회전 목록 계산 처리

        public static Quaternion QuarterTurnRotation(int clockwiseQuarterTurns) // 직각 회전 Quaternion 계산
        { // 직각 회전 Quaternion 계산 처리
            int normalizedTurns = ((clockwiseQuarterTurns % 4) + 4) % 4; // 회전 횟수 0부터 3까지 정규화
            return Quaternion.Euler(0f, normalizedTurns * 90f, 0f); // Y축 직각 회전 반환
        } // 직각 회전 Quaternion 계산 처리

        public static bool AreWorldDirectionsOpposite(Vector3 firstDirection, Vector3 secondDirection) // 두 월드 방향 마주 보기 검사
        { // 월드 방향 마주 보기 검사 처리
            if (firstDirection.sqrMagnitude <= DirectionTolerance || secondDirection.sqrMagnitude <= DirectionTolerance) // 유효하지 않은 방향 확인
            { // 유효하지 않은 방향 처리
                return false; // 방향 비교 실패 반환
            } // 유효하지 않은 방향 처리

            float directionDot = Vector3.Dot(firstDirection.normalized, secondDirection.normalized); // 두 방향 내적 계산
            return directionDot <= -1f + DirectionTolerance; // 반대 방향 여부 반환
        } // 월드 방향 마주 보기 검사 처리

        public static Vector3 CalculateAlignedRootPosition(Vector3 currentRootPosition, Vector3 targetConnectionPosition, Vector3 candidateConnectionPosition) // 연결 지점 일치용 루트 위치 계산
        { // 루트 위치 계산 처리
            Vector3 connectionOffset = targetConnectionPosition - candidateConnectionPosition; // 두 연결 지점 위치 차이 계산
            return currentRootPosition + connectionOffset; // 위치 차이가 적용된 루트 위치 반환
        } // 루트 위치 계산 처리

        public static bool BoundsHaveBlockingOverlap(Bounds firstBounds, Bounds secondBounds, float tolerance) // 두 모듈 영역의 실제 겹침 검사
        { // 모듈 영역 겹침 검사 처리
            float safeTolerance = Mathf.Max(0f, tolerance); // 음수가 없는 허용 크기 계산
            float overlapX = Mathf.Min(firstBounds.max.x, secondBounds.max.x) - Mathf.Max(firstBounds.min.x, secondBounds.min.x); // X축 겹침 크기 계산
            float overlapY = Mathf.Min(firstBounds.max.y, secondBounds.max.y) - Mathf.Max(firstBounds.min.y, secondBounds.min.y); // Y축 겹침 크기 계산
            float overlapZ = Mathf.Min(firstBounds.max.z, secondBounds.max.z) - Mathf.Max(firstBounds.min.z, secondBounds.min.z); // Z축 겹침 크기 계산
            return overlapX > safeTolerance && overlapY > safeTolerance && overlapZ > safeTolerance; // 세 축 실제 겹침 여부 반환
        } // 모듈 영역 겹침 검사 처리
    } // 맵 배치 공통 규칙 묶음
} // 맵 생성 기능 묶음
