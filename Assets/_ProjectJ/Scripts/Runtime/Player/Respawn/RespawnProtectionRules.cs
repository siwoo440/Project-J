using UnityEngine; // Unity 수학 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 규칙 묶음
    public static class RespawnProtectionRules // 부활 보호 시간 계산 규칙 선언
    { // 보호 시간 규칙 묶음
        public static float ClampDuration(float duration) // 보호 시간 안전 보정
        { // 보호 시간 보정 처리
            return Mathf.Max(0f, duration); // 음수가 없는 보호 시간 반환
        } // 보호 시간 보정 종료

        public static float CalculateRemaining(float duration, float elapsedTime) // 남은 보호 시간 계산
        { // 남은 시간 계산 처리
            float safeDuration = ClampDuration(duration); // 안전한 전체 보호 시간 계산
            float safeElapsedTime = Mathf.Max(0f, elapsedTime); // 음수가 없는 경과 시간 계산
            return Mathf.Max(0f, safeDuration - safeElapsedTime); // 음수가 없는 남은 시간 반환
        } // 남은 시간 계산 종료

        public static bool IsProtected(float remainingTime) // 보호 활성 여부 판정
        { // 보호 상태 판정 처리
            return remainingTime > 0f; // 남은 시간이 있는 보호 상태 반환
        } // 보호 상태 판정 종료
    } // 보호 시간 규칙 묶음 종료
} // 플레이어 규칙 묶음 종료
