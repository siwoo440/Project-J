using UnityEngine; // Unity 수학 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 기능 범위
    public static class RespawnFallLimitRules // 체크포인트 기준 추락 한계 계산 규칙 선언
    { // 추락 한계 규칙 범위
        private const float MinimumFallDistance = 0.1f; // 최소 추락 거리

        public static float ClampFallDistance(float fallDistance) // 추락 거리 안전 보정
        { // 추락 거리 보정 범위
            return Mathf.Max(MinimumFallDistance, fallDistance); // 최소 추락 거리 적용
        } // 추락 거리 보정 범위 종료

        public static float CalculateFallLimitY(float minimumWorldFallLimitY, float respawnPointY, float fallDistanceBelowCheckpoint) // 현재 체크포인트 기준 추락 한계 계산
        { // 추락 한계 계산 범위
            float safeFallDistance = ClampFallDistance(fallDistanceBelowCheckpoint); // 안전한 추락 거리 계산
            float checkpointFallLimitY = respawnPointY - safeFallDistance; // 체크포인트 아래 추락 한계 계산
            return Mathf.Max(minimumWorldFallLimitY, checkpointFallLimitY); // 월드 최저선과 체크포인트 기준선 중 높은 값 반환
        } // 추락 한계 계산 범위 종료

        public static bool HasReachedFallLimit(float playerPositionY, float fallLimitY) // 플레이어 추락 한계 도달 여부 판정
        { // 추락 한계 판정 범위
            return playerPositionY <= fallLimitY; // 한계선 이하 진입 여부 반환
        } // 추락 한계 판정 범위 종료
    } // 추락 한계 규칙 범위 종료
} // 플레이어 기능 범위 종료
