using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 깃털 신발 정책 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJFeatherShoesPolicyTests // 깃털 신발 정책 테스트
    {
        [TestCase(5f, true, 6.25f)] // 일반 이동 강화 사례
        [TestCase(8f, true, 10f)] // 달리기 강화 사례
        [TestCase(5f, false, 5f)] // 효과 비활성 사례
        [TestCase(-2f, true, 0f)] // 잘못된 음수 속도 사례
        public void CalculateMovementSpeed_WithEffectState_ReturnsExpected( // 속도 배율 오류 방지
            float baseSpeed, // 기본 이동 속도
            bool isActive, // 효과 활성 여부
            float expected // 예상 최종 속도
        )
        {
            float movementSpeed = ProjectJFeatherShoesPolicy.CalculateMovementSpeed( // 최종 이동 속도 계산
                baseSpeed, // 기본 속도 전달
                isActive // 효과 상태 전달
            );

            Assert.AreEqual(expected, movementSpeed, 0.0001f); // 이동 속도 결과 검증
        }

        [TestCase(25f, true, 28.75f)] // 활성 중 추가 소모 사례
        [TestCase(25f, false, 25f)] // 비활성 중 기본 소모 사례
        [TestCase(-1f, true, 0f)] // 잘못된 음수 소모 사례
        public void CalculateSprintStaminaDrain_WithEffectState_ReturnsExpected( // 스태미나 배율 오류 방지
            float baseDrainPerSecond, // 기본 초당 소모량
            bool isActive, // 효과 활성 여부
            float expected // 예상 초당 소모량
        )
        {
            float staminaDrain = ProjectJFeatherShoesPolicy.CalculateSprintStaminaDrain( // 최종 소모량 계산
                baseDrainPerSecond, // 기본 소모량 전달
                isActive // 효과 상태 전달
            );

            Assert.AreEqual(expected, staminaDrain, 0.0001f); // 스태미나 소모 결과 검증
        }

        [Test] // 지속 시간 기획값 검증
        public void DurationSeconds_ReturnsSevenSeconds() // 잘못된 효과 시간 방지
        {
            Assert.AreEqual(7f, ProjectJFeatherShoesPolicy.DurationSeconds); // 7초 기획값 확인
        }

        [Test] // 반복 사용 시 배율 중첩 방지 검증
        public void CalculateMovementSpeed_RepeatedActivation_KeepsSingleMultiplier() // 재사용 강도 중첩 방지
        {
            float firstUseSpeed = ProjectJFeatherShoesPolicy.CalculateMovementSpeed( // 첫 사용 속도 계산
                5f, // 기본 이동 속도
                true // 효과 활성
            );
            float refreshedUseSpeed = ProjectJFeatherShoesPolicy.CalculateMovementSpeed( // 재사용 속도 계산
                5f, // 같은 기본 이동 속도
                true // 갱신된 효과 활성
            );

            Assert.AreEqual(6.25f, firstUseSpeed, 0.0001f); // 첫 사용 단일 배율 확인
            Assert.AreEqual(firstUseSpeed, refreshedUseSpeed, 0.0001f); // 재사용 강도 유지 확인
        }
    }
}
