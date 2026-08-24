namespace ProjectJ.Debugging // Runtime 진단 정책 네임스페이스
{
    public static class ProjectJNetworkTransformDiagnosticsPolicy // NetworkTransform 적용 상태 진단 정책
    {
        public static string GetPhysicsForecastLabel( // Physics Forecast 상태 문자열 계산
            bool hasPhysicsBody, // Physics Body 존재 여부
            bool hasForecastEnabled // Forecast 활성 여부
        )
        {
            if (!hasPhysicsBody) // Physics Body 미존재 확인
            {
                return "NO PHYSICS BODY"; // 비물리 Player 상태 반환
            }

            if (!hasForecastEnabled) // Forecast 비활성 확인
            {
                return "FORECAST INACTIVE"; // Forecast 비활성 상태 반환
            }

            return "FORECAST ACTIVE"; // 실제 Physics Forecast 활성 상태 반환
        }

        public static bool IsPhysicsTuningApplicable( // Physics 보정값 조정 가능 여부 계산
            bool hasPhysicsBody, // Physics Body 존재 여부
            bool hasForecastEnabled // Forecast 활성 여부
        )
        {
            return // 실제 Physics 보정 적용 조건 반환
                hasPhysicsBody && // Physics Body 존재 확인
                hasForecastEnabled; // Forecast 활성 확인
        }

        public static string GetRenderPathLabel( // 실제 Render 경로 문자열 계산
            bool hasNetworkTransform, // NetworkTransform 존재 여부
            bool forceRemoteRenderTimeframe, // Remote Timeframe 강제 여부
            bool usesRemoteTimeframe // 실제 Remote Timeframe 여부
        )
        {
            if (!hasNetworkTransform) // NetworkTransform 미존재 확인
            {
                return "NO NETWORK TRANSFORM"; // 네트워크 보간 없음 반환
            }

            if (forceRemoteRenderTimeframe) // Remote Timeframe 강제 확인
            {
                return "FORCED REMOTE"; // 강제 Remote 경로 반환
            }

            if (usesRemoteTimeframe) // 실제 Remote Timeframe 확인
            {
                return "REMOTE INTERPOLATED"; // 정상 원격 보간 경로 반환
            }

            return "LOCAL TIMEFRAME"; // 로컬 시간축 경로 반환
        }
    }
}
