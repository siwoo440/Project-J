using NUnit.Framework; // NUnit 테스트 기능 참조
using ProjectJ.Player; // 추락 한계 규칙 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 범위
    public sealed class RespawnFallLimitRulesTests // 추락 한계 규칙 테스트 선언
    { // 추락 한계 테스트 범위
        [Test] // 자동 테스트 항목 표시
        public void FallDistanceNeverFallsBelowMinimum() // 음수 추락 거리 최소값 보정 확인
        { // 추락 거리 최소값 테스트 범위
            float result = RespawnFallLimitRules.ClampFallDistance(-10f); // 음수 추락 거리 보정 실행
            Assert.AreEqual(0.1f, result, 0.0001f); // 최소 추락 거리 확인
        } // 추락 거리 최소값 테스트 범위 종료

        [Test] // 자동 테스트 항목 표시
        public void StartPointUsesMinimumWorldFallLimit() // 시작 지점 월드 최저선 사용 확인
        { // 시작 지점 추락 한계 테스트 범위
            float result = RespawnFallLimitRules.CalculateFallLimitY(-5f, 0f, 25f); // 시작 지점 추락 한계 계산
            Assert.AreEqual(-5f, result, 0.0001f); // 월드 최저 추락선 확인
        } // 시작 지점 추락 한계 테스트 범위 종료

        [Test] // 자동 테스트 항목 표시
        public void HighCheckpointUsesRelativeFallLimit() // 높은 체크포인트 상대 추락선 사용 확인
        { // 높은 체크포인트 추락 한계 테스트 범위
            float result = RespawnFallLimitRules.CalculateFallLimitY(-5f, 200f, 25f); // 높은 체크포인트 추락 한계 계산
            Assert.AreEqual(175f, result, 0.0001f); // 체크포인트 아래 25미터 추락선 확인
        } // 높은 체크포인트 추락 한계 테스트 범위 종료

        [Test] // 자동 테스트 항목 표시
        public void MinimumWorldLimitWinsNearCourseStart() // 시작 구간 월드 최저선 우선 확인
        { // 시작 구간 추락 한계 테스트 범위
            float result = RespawnFallLimitRules.CalculateFallLimitY(-5f, 10f, 25f); // 시작 구간 추락 한계 계산
            Assert.AreEqual(-5f, result, 0.0001f); // 더 높은 월드 최저선 확인
        } // 시작 구간 추락 한계 테스트 범위 종료

        [Test] // 자동 테스트 항목 표시
        public void PositionAboveFallLimitDoesNotTrigger() // 추락선 위 플레이어 미판정 확인
        { // 추락선 위 판정 테스트 범위
            bool result = RespawnFallLimitRules.HasReachedFallLimit(175.1f, 175f); // 추락선 위 위치 판정
            Assert.IsFalse(result); // 미추락 판정 확인
        } // 추락선 위 판정 테스트 범위 종료

        [Test] // 자동 테스트 항목 표시
        public void PositionOnFallLimitTriggers() // 추락선 동일 위치 판정 확인
        { // 추락선 동일 위치 테스트 범위
            bool result = RespawnFallLimitRules.HasReachedFallLimit(175f, 175f); // 추락선 동일 위치 판정
            Assert.IsTrue(result); // 추락 판정 확인
        } // 추락선 동일 위치 테스트 범위 종료

        [Test] // 자동 테스트 항목 표시
        public void PositionBelowFallLimitTriggers() // 추락선 아래 플레이어 판정 확인
        { // 추락선 아래 판정 테스트 범위
            bool result = RespawnFallLimitRules.HasReachedFallLimit(170f, 175f); // 추락선 아래 위치 판정
            Assert.IsTrue(result); // 추락 판정 확인
        } // 추락선 아래 판정 테스트 범위 종료
    } // 추락 한계 테스트 범위 종료
} // EditMode 테스트 범위 종료
