using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 제트팩 정책 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJJetpackPolicyTests // 제트팩 정책 테스트
    {
        [Test] // 확정 지속 시간 검증
        public void DurationSeconds_ReturnsFiveSeconds() // 5초 연료 값 오류 방지
        {
            Assert.AreEqual(5f, ProjectJJetpackPolicy.DurationSeconds); // 확정 5초 값 확인
        }

        [Test] // 프로토타입 상승 속도 검증
        public void PrototypeAscentSpeed_ReturnsFourMetersPerSecond() // 상승 프로토타입 값 변경 감지
        {
            Assert.AreEqual(4f, ProjectJJetpackPolicy.PrototypeAscentSpeedMetersPerSecond); // 프로토타입 4m/s 확인
        }

        [Test] // 프로토타입 수평 배율 검증
        public void PrototypeHorizontalMultiplier_ReturnsOne() // 기존 WASD 속도 유지 확인
        {
            Assert.AreEqual(1f, ProjectJJetpackPolicy.PrototypeHorizontalControlMultiplier); // 프로토타입 1배 확인
        }

        [Test] // 천장 판정 여유값 검증
        public void CeilingProbeSkin_ReturnsFiveCentimeters() // 천장 판정 여유값 변경 감지
        {
            Assert.AreEqual(0.05f, ProjectJJetpackPolicy.CeilingProbeSkinMeters); // 5cm 여유값 확인
        }

        [TestCase(true, true, true)] // 활성·경기 허용 사례
        [TestCase(false, true, false)] // 연료 종료 사례
        [TestCase(true, false, false)] // Gameplay Lock 사례
        [TestCase(false, false, false)] // 비활성·잠금 사례
        public void CanApplyMovement_WithState_ReturnsExpected( // 상태 전환별 이동 허용 검증
            bool isActive, // 제트팩 활성 상태
            bool gameplayInputAllowed, // 경기 조작 허용 상태
            bool expected // 예상 이동 허용 결과
        )
        {
            bool canApply = ProjectJJetpackPolicy.CanApplyMovement( // 이동 허용 상태 계산
                isActive, // 활성 상태 전달
                gameplayInputAllowed // 경기 허용 상태 전달
            );

            Assert.AreEqual(expected, canApply); // 상태 전환 결과 검증
        }

        [TestCase(5f, true, 5f)] // 걷기 속도 유지 사례
        [TestCase(8f, true, 8f)] // 달리기 속도 유지 사례
        [TestCase(5f, false, 5f)] // 비활성 기존 속도 사례
        [TestCase(-2f, true, 0f)] // 잘못된 음수 속도 사례
        public void CalculateHorizontalMovementSpeed_WithEffectState_ReturnsExpected( // 수평 조정 배율 검증
            float baseSpeed, // 기존 이동 속도
            bool isActive, // 제트팩 활성 상태
            float expected // 예상 최종 속도
        )
        {
            float movementSpeed = ProjectJJetpackPolicy.CalculateHorizontalMovementSpeed( // 최종 수평 속도 계산
                baseSpeed, // 기존 속도 전달
                isActive // 활성 상태 전달
            );

            Assert.AreEqual(expected, movementSpeed, 0.0001f); // 수평 속도 결과 검증
        }

        [TestCase(-6f, true, true, false, 4f)] // 추락 중 활성 시 상승 전환 사례
        [TestCase(2f, true, true, false, 4f)] // 낮은 상승 속도 보정 사례
        [TestCase(6f, true, true, false, 6f)] // 기존 점프 상승 속도 보존 사례
        [TestCase(6f, true, true, true, 0f)] // 천장 접촉 위쪽 속도 제거 사례
        [TestCase(-3f, true, true, true, -3f)] // 천장 아래 하강 속도 보존 사례
        [TestCase(-3f, false, true, false, -3f)] // 연료 종료 후 중력 상태 유지 사례
        [TestCase(3f, true, false, false, 3f)] // Gameplay Lock에서 제트팩 개입 차단 사례
        public void ResolveVerticalVelocity_WithState_ReturnsExpected( // 수직 상태 전환 검증
            float gravityResolvedVelocity, // 기존 중력 계산 결과
            bool isActive, // 제트팩 활성 상태
            bool gameplayInputAllowed, // 경기 조작 허용 상태
            bool ceilingBlocked, // 천장 차단 상태
            float expected // 예상 수직 속도
        )
        {
            float verticalVelocity = ProjectJJetpackPolicy.ResolveVerticalVelocity( // 최종 수직 속도 계산
                gravityResolvedVelocity, // 기존 수직 속도 전달
                isActive, // 활성 상태 전달
                gameplayInputAllowed, // 경기 허용 상태 전달
                ceilingBlocked // 천장 상태 전달
            );

            Assert.AreEqual(expected, verticalVelocity, 0.0001f); // 수직 속도 결과 검증
        }
    }
}
