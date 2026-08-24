using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 눈덩이 정책 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJSnowballPolicyTests // 눈덩이 정책 테스트
    {
        [TestCase(5f, true, 3.75f)] // 일반 이동 감속 사례
        [TestCase(8f, true, 6f)] // 달리기 감속 사례
        [TestCase(6.25f, true, 4.6875f)] // 깃털 신발 동시 적용 사례
        [TestCase(5f, false, 5f)] // 효과 비활성 사례
        [TestCase(-2f, true, 0f)] // 잘못된 음수 속도 사례
        public void CalculateMovementSpeed_WithSlowState_ReturnsExpected( // 감속 배율 오류 방지
            float baseSpeed, // 감속 전 이동 속도
            bool isActive, // 감속 활성 여부
            float expected // 예상 최종 속도
        )
        {
            float movementSpeed = ProjectJSnowballPolicy.CalculateMovementSpeed( // 최종 이동 속도 계산
                baseSpeed, // 감속 전 속도 전달
                isActive // 감속 상태 전달
            );

            Assert.AreEqual(expected, movementSpeed, 0.0001f); // 이동 속도 결과 검증
        }

        [TestCase(true, true, false, false, false, false, true)] // 정상 적중 사례
        [TestCase(false, true, false, false, false, false, false)] // Runner 누락 사례
        [TestCase(true, false, false, false, false, false, false)] // 경기 입력 잠금 사례
        [TestCase(true, true, true, false, false, false, false)] // 소유자 자기 적중 사례
        [TestCase(true, true, false, true, false, false, false)] // 완주 Target 사례
        [TestCase(true, true, false, false, true, false, false)] // 부활 보호 사례
        [TestCase(true, true, false, false, false, true, false)] // 젤리 보호막 사례
        public void CanAffectTarget_WithTargetState_ReturnsExpected( // 잘못된 Target 감속 방지
            bool runnerReady, // Runner 준비 여부
            bool gameplayAllowed, // 경기 입력 허용 여부
            bool isOwner, // 사용자 본인 여부
            bool isFinished, // 완주 여부
            bool isRespawnProtected, // 부활 보호 여부
            bool isShielded, // 아이템 보호막 여부
            bool expected // 예상 적용 가능 여부
        )
        {
            bool canAffect = ProjectJSnowballPolicy.CanAffectTarget( // Target 적용 조건 계산
                runnerReady, // Runner 상태 전달
                gameplayAllowed, // 경기 상태 전달
                isOwner, // 소유자 여부 전달
                isFinished, // 완주 여부 전달
                isRespawnProtected, // 부활 보호 전달
                isShielded // 보호막 전달
            );

            Assert.AreEqual(expected, canAffect); // Target 적용 결과 검증
        }

        [TestCase(0f, 3f)] // 최초 적중 사례
        [TestCase(1.5f, 3f)] // 효과 중 재적중 사례
        [TestCase(3f, 3f)] // 최대 시간 재적중 사례
        public void GetRefreshedDuration_WithExistingTime_ReturnsThreeSeconds( // 중첩 대신 갱신 규칙 검증
            float currentRemaining, // 기존 남은 시간
            float expected // 예상 갱신 시간
        )
        {
            float refreshedDuration = ProjectJSnowballPolicy.GetRefreshedDuration( // 재적중 지속 시간 계산
                currentRemaining // 기존 시간 전달
            );

            Assert.AreEqual(expected, refreshedDuration, 0.0001f); // 3초 갱신 결과 검증
        }

        [TestCase(14.99f, false)] // 최대 거리 직전 사례
        [TestCase(15f, true)] // 최대 거리 경계 사례
        [TestCase(20f, true)] // 최대 거리 초과 사례
        public void HasReachedTravelLimit_WithDistance_ReturnsExpected( // 투사체 무한 이동 방지
            float travelledDistance, // 누적 이동 거리
            bool expected // 예상 제거 여부
        )
        {
            bool reachedLimit = ProjectJSnowballPolicy.HasReachedTravelLimit( // 최대 거리 도달 계산
                travelledDistance // 누적 거리 전달
            );

            Assert.AreEqual(expected, reachedLimit); // 거리 제한 결과 검증
        }
    }
}
