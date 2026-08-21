using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Map // 맵 시스템 네임스페이스
{
    public enum MapModuleConnectionFailure // Module 연결 실패 원인
    {
        None, // 정상 연결
        MissingSocket, // Socket 누락
        InvalidStateOrder, // Exit Entrance 순서 오류
        PositionMismatch, // Socket 위치 불일치
        FacingMismatch // Socket 방향 불일치
    }

    public readonly struct MapModuleConnectionResult // Module 연결 검사 결과
    {
        public bool IsValid { get; } // 연결 성공 여부

        public MapModuleConnectionFailure Failure { get; } // 실패 원인

        private MapModuleConnectionResult(bool isValid, MapModuleConnectionFailure failure) // 결과 생성
        {
            IsValid = isValid; // 성공 여부 저장
            Failure = failure; // 실패 원인 저장
        }

        public static MapModuleConnectionResult Valid() // 정상 결과 생성
        {
            return new MapModuleConnectionResult(true, MapModuleConnectionFailure.None); // 정상 결과 반환
        }

        public static MapModuleConnectionResult Invalid(MapModuleConnectionFailure failure) // 실패 결과 생성
        {
            return new MapModuleConnectionResult(false, failure); // 실패 결과 반환
        }
    }

    public static class MapModuleConnectionValidator // Module Socket 연결 검사
    {
        public static MapModuleConnectionResult Validate( // Socket 연결 검사
            MapModuleSocket fromSocket, // 출발 Socket
            MapModuleSocket toSocket, // 도착 Socket
            float positionTolerance = 0.05f, // 위치 허용 오차
            float facingDotTolerance = 0.99f // 방향 허용 오차
        )
        {
            if (fromSocket == null || toSocket == null) // Socket 누락 검사
            {
                return MapModuleConnectionResult.Invalid(MapModuleConnectionFailure.MissingSocket); // 누락 결과 반환
            }

            if (fromSocket.State != MapModuleFaceState.Exit || toSocket.State != MapModuleFaceState.Entrance) // 상태 순서 검사
            {
                return MapModuleConnectionResult.Invalid(MapModuleConnectionFailure.InvalidStateOrder); // 상태 오류 반환
            }

            float safePositionTolerance = Mathf.Max(0f, positionTolerance); // 위치 오차 보정
            float sqrTolerance = safePositionTolerance * safePositionTolerance; // 제곱 오차 계산
            float sqrDistance = (fromSocket.transform.position - toSocket.transform.position).sqrMagnitude; // Socket 거리 계산

            if (sqrDistance > sqrTolerance) // 위치 정렬 검사
            {
                return MapModuleConnectionResult.Invalid(MapModuleConnectionFailure.PositionMismatch); // 위치 오류 반환
            }

            float safeFacingTolerance = Mathf.Clamp(facingDotTolerance, -1f, 1f); // 방향 오차 보정
            Vector3 fromForward = fromSocket.transform.forward.normalized; // 출발 외향 방향 계산
            Vector3 toForward = toSocket.transform.forward.normalized; // 도착 외향 방향 계산
            float oppositeDot = Vector3.Dot(fromForward, -toForward); // 서로 반대 방향 여부 계산

            if (oppositeDot < safeFacingTolerance) // 방향 정렬 검사
            {
                return MapModuleConnectionResult.Invalid(MapModuleConnectionFailure.FacingMismatch); // 방향 오류 반환
            }

            return MapModuleConnectionResult.Valid(); // 정상 연결 반환
        }
    }
}
