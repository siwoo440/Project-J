using NUnit.Framework; // NUnit 테스트 기능 참조
using ProjectJ.Player; // 부활 보호 규칙 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class RespawnProtectionRulesTests // 부활 보호 시간 규칙 테스트 선언
    { // 보호 시간 테스트 묶음
        [Test] // 자동 테스트 항목 표시
        public void NegativeDurationClampsToZero() // 음수 보호 시간 보정 확인
        { // 음수 보호 시간 테스트 처리
            float result = RespawnProtectionRules.ClampDuration(-3f); // 음수 보호 시간 보정 실행
            Assert.AreEqual(0f, result, 0.0001f); // 영초 보정 결과 확인
        } // 음수 보호 시간 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void ThreeSecondDurationRemainsAtStart() // 보호 시작 시 전체 시간 확인
        { // 보호 시작 시간 테스트 처리
            float result = RespawnProtectionRules.CalculateRemaining(3f, 0f); // 시작 시 남은 보호 시간 계산
            Assert.AreEqual(3f, result, 0.0001f); // 전체 보호 시간 확인
        } // 보호 시작 시간 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void ElapsedTimeReducesRemainingDuration() // 경과 시간만큼 감소 확인
        { // 보호 시간 감소 테스트 처리
            float result = RespawnProtectionRules.CalculateRemaining(3f, 1.25f); // 일부 경과 뒤 남은 시간 계산
            Assert.AreEqual(1.75f, result, 0.0001f); // 감소한 보호 시간 확인
        } // 보호 시간 감소 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void RemainingDurationNeverBecomesNegative() // 남은 시간 음수 방지 확인
        { // 보호 시간 만료 테스트 처리
            float result = RespawnProtectionRules.CalculateRemaining(3f, 5f); // 전체 시간을 넘긴 경과 시간 계산
            Assert.AreEqual(0f, result, 0.0001f); // 영초 하한 확인
        } // 보호 시간 만료 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void PositiveRemainingTimeMeansProtected() // 양수 남은 시간 보호 판정 확인
        { // 보호 활성 판정 테스트 처리
            bool result = RespawnProtectionRules.IsProtected(0.01f); // 양수 남은 시간 판정
            Assert.IsTrue(result); // 보호 활성 상태 확인
        } // 보호 활성 판정 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void ZeroRemainingTimeMeansUnprotected() // 영초 보호 해제 판정 확인
        { // 보호 해제 판정 테스트 처리
            bool result = RespawnProtectionRules.IsProtected(0f); // 영초 남은 시간 판정
            Assert.IsFalse(result); // 보호 해제 상태 확인
        } // 보호 해제 판정 테스트 종료
    } // 보호 시간 테스트 묶음 종료
} // EditMode 테스트 묶음 종료
