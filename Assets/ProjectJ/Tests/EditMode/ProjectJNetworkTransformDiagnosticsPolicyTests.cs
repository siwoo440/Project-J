using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Debugging; // NetworkTransform 진단 정책 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJNetworkTransformDiagnosticsPolicyTests // NetworkTransform 진단 정책 테스트
    {
        [Test] // Physics Body 미존재 상태 검증
        public void GetPhysicsForecastLabel_WithoutPhysicsBody_ReturnsNoPhysicsBody() // 비물리 Player의 Forecast 오판 방지
        {
            string label = // Physics Forecast 상태 문자열 계산
                ProjectJNetworkTransformDiagnosticsPolicy.GetPhysicsForecastLabel( // 진단 정책 호출
                    false, // Physics Body 없음
                    true // Prefab Forecast 설정 존재
                );

            Assert.AreEqual( // Physics Body 미존재 결과 검증
                "NO PHYSICS BODY", // 예상 비물리 상태
                label // 실제 상태 문자열
            );
        }

        [Test] // 비활성 Forecast 상태 검증
        public void GetPhysicsForecastLabel_WithBodyAndDisabledForecast_ReturnsInactive() // 전역 비활성 Forecast 구분 확인
        {
            string label = // Physics Forecast 상태 문자열 계산
                ProjectJNetworkTransformDiagnosticsPolicy.GetPhysicsForecastLabel( // 진단 정책 호출
                    true, // Physics Body 존재
                    false // Forecast 비활성
                );

            Assert.AreEqual( // Forecast 비활성 결과 검증
                "FORECAST INACTIVE", // 예상 비활성 상태
                label // 실제 상태 문자열
            );
        }

        [Test] // 활성 Forecast 상태 검증
        public void GetPhysicsForecastLabel_WithBodyAndEnabledForecast_ReturnsActive() // 실제 Physics Forecast 적용 확인
        {
            string label = // Physics Forecast 상태 문자열 계산
                ProjectJNetworkTransformDiagnosticsPolicy.GetPhysicsForecastLabel( // 진단 정책 호출
                    true, // Physics Body 존재
                    true // Forecast 활성
                );

            Assert.AreEqual( // Forecast 활성 결과 검증
                "FORECAST ACTIVE", // 예상 활성 상태
                label // 실제 상태 문자열
            );
        }

        [TestCase(false, true, false)] // Physics Body 미존재 사례
        [TestCase(true, false, false)] // Forecast 비활성 사례
        [TestCase(true, true, true)] // Physics Forecast 실제 적용 사례
        public void IsPhysicsTuningApplicable_WithRuntimeState_ReturnsExpected( // Physics 보정값 조정 가능 여부 확인
            bool hasPhysicsBody, // Physics Body 존재 여부
            bool hasForecastEnabled, // Forecast 활성 여부
            bool expected // 예상 조정 가능 여부
        )
        {
            bool applicable = // Physics 보정값 조정 가능 여부 계산
                ProjectJNetworkTransformDiagnosticsPolicy.IsPhysicsTuningApplicable( // 진단 정책 호출
                    hasPhysicsBody, // Physics Body 상태 전달
                    hasForecastEnabled // Forecast 상태 전달
                );

            Assert.AreEqual( // 조정 가능 여부 검증
                expected, // 예상 판단 결과
                applicable // 실제 판단 결과
            );
        }

        [TestCase(false, false, false, "NO NETWORK TRANSFORM")] // NetworkTransform 미존재 사례
        [TestCase(true, true, true, "FORCED REMOTE")] // Remote Timeframe 강제 사례
        [TestCase(true, false, true, "REMOTE INTERPOLATED")] // 정상 Remote 보간 사례
        [TestCase(true, false, false, "LOCAL TIMEFRAME")] // 로컬 시간축 사례
        public void GetRenderPathLabel_WithRuntimeState_ReturnsExpected( // 실제 Render 경로 표시 확인
            bool hasNetworkTransform, // NetworkTransform 존재 여부
            bool forceRemoteRenderTimeframe, // Remote Timeframe 강제 여부
            bool usesRemoteTimeframe, // 실제 Remote Timeframe 여부
            string expected // 예상 Render 경로 문자열
        )
        {
            string label = // Render 경로 문자열 계산
                ProjectJNetworkTransformDiagnosticsPolicy.GetRenderPathLabel( // 진단 정책 호출
                    hasNetworkTransform, // NetworkTransform 상태 전달
                    forceRemoteRenderTimeframe, // 강제 Remote 상태 전달
                    usesRemoteTimeframe // 실제 Timeframe 상태 전달
                );

            Assert.AreEqual( // Render 경로 결과 검증
                expected, // 예상 Render 경로
                label // 실제 Render 경로
            );
        }
    }
}
