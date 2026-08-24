using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 폭죽 정책 사용
using UnityEngine; // Vector3 계산 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJFireworkPolicyTests // 폭죽 규칙 테스트
    {
        [TestCase(true, true, false, true)] // 정상 준비 시작 사례
        [TestCase(false, true, false, false)] // Runner 누락 사례
        [TestCase(true, false, false, false)] // 경기 입력 잠금 사례
        [TestCase(true, true, true, false)] // 중복 준비 사례
        public void CanBeginPreparation_WithRuntimeState_ReturnsExpected( // 잘못된 준비 시작 방지
            bool runnerAvailable, // Runner 존재 여부
            bool gameplayInputAllowed, // 경기 입력 허용 여부
            bool alreadyPreparing, // 기존 준비 상태
            bool expected // 예상 시작 결과
        )
        {
            bool canBegin = ProjectJFireworkPolicy.CanBeginPreparation( // 준비 시작 가능 여부 계산
                runnerAvailable, // Runner 상태 전달
                gameplayInputAllowed, // 경기 입력 상태 전달
                alreadyPreparing // 기존 준비 상태 전달
            );

            Assert.AreEqual(expected, canBegin); // 준비 시작 결과 검증
        }

        [TestCase(false, 2, 2, true)] // 경기 종료 취소 사례
        [TestCase(true, 2, 3, true)] // 부활 취소 사례
        [TestCase(true, 2, 2, false)] // 정상 준비 유지 사례
        public void ShouldCancelPreparation_WithMatchAndRespawnState_ReturnsExpected( // 잘못된 지연 발동 방지
            bool gameplayInputAllowed, // 경기 입력 허용 여부
            int startingRespawnCount, // 준비 시작 부활 횟수
            int currentRespawnCount, // 현재 부활 횟수
            bool expected // 예상 취소 결과
        )
        {
            bool shouldCancel = ProjectJFireworkPolicy.ShouldCancelPreparation( // 준비 취소 여부 계산
                gameplayInputAllowed, // 경기 입력 상태 전달
                startingRespawnCount, // 시작 부활 횟수 전달
                currentRespawnCount // 현재 부활 횟수 전달
            );

            Assert.AreEqual(expected, shouldCancel); // 준비 취소 결과 검증
        }

        [Test] // 전방 범위 포함 사례
        public void IsTargetWithinArea_TargetAtFortyFiveDegrees_ReturnsTrue() // 전체 100도 범위 축소 오류 방지
        {
            bool isWithin = ProjectJFireworkPolicy.IsTargetWithinDefaultArea( // 확정 전방 범위 포함 여부 계산
                Vector3.zero, // 사용자 위치
                Vector3.forward, // 사용자 전방
                new Vector3(4f, 0f, 4f) // 전방 45도 Target
            );

            Assert.IsTrue(isWithin); // 전방 범위 포함 검증
        }

        [Test] // 각도 범위 제외 사례
        public void IsTargetWithinArea_TargetAtSixtyDegrees_ReturnsFalse() // 전체 각도를 반각으로 오해하는 오류 방지
        {
            bool isWithin = ProjectJFireworkPolicy.IsTargetWithinDefaultArea( // 확정 전방 범위 포함 여부 계산
                Vector3.zero, // 사용자 위치
                Vector3.forward, // 사용자 전방
                new Vector3(6.928203f, 0f, 4f) // 전방 60도 Target
            );

            Assert.IsFalse(isWithin); // 각도 범위 제외 검증
        }

        [Test] // 사거리 경계 포함 사례
        public void IsTargetWithinArea_TargetAtExactRange_ReturnsTrue() // 8m 경계 누락 방지
        {
            bool isWithin = ProjectJFireworkPolicy.IsTargetWithinDefaultArea( // 확정 전방 범위 포함 여부 계산
                Vector3.zero, // 사용자 위치
                Vector3.forward, // 사용자 전방
                new Vector3(0f, 0f, 8f) // 정확히 8m Target
            );

            Assert.IsTrue(isWithin); // 사거리 경계 포함 검증
        }

        [Test] // 사거리 초과 사례
        public void IsTargetWithinArea_TargetBeyondRange_ReturnsFalse() // 원거리 오발 방지
        {
            bool isWithin = ProjectJFireworkPolicy.IsTargetWithinDefaultArea( // 확정 전방 범위 포함 여부 계산
                Vector3.zero, // 사용자 위치
                Vector3.forward, // 사용자 전방
                new Vector3(0f, 0f, 8.01f) // 사거리 초과 Target
            );

            Assert.IsFalse(isWithin); // 사거리 초과 제외 검증
        }

        [Test] // 후방 Target 제외 사례
        public void IsTargetWithinArea_TargetBehindUser_ReturnsFalse() // 후방 오발 방지
        {
            bool isWithin = ProjectJFireworkPolicy.IsTargetWithinDefaultArea( // 확정 전방 범위 포함 여부 계산
                Vector3.zero, // 사용자 위치
                Vector3.forward, // 사용자 전방
                new Vector3(0f, 0f, -2f) // 후방 Target
            );

            Assert.IsFalse(isWithin); // 후방 Target 제외 검증
        }

        [Test] // 수평 외력 생성 사례
        public void CreateHorizontalVelocityChange_TargetAboveUser_ReturnsHorizontalNineMetersPerSecond() // 수직 힘 혼입 방지
        {
            Vector3 velocityChange = ProjectJFireworkPolicy.CreateDefaultHorizontalVelocityChange( // 확정 수평 외력 계산
                Vector3.zero, // 사용자 위치
                new Vector3(3f, 5f, 4f), // 높이가 다른 Target
                Vector3.forward, // 거리 0 대비 전방
                Vector3.zero // 기존 외부 속도 없음
            );

            Assert.AreEqual(0f, velocityChange.y, 0.0001f); // 수직 외력 제거 검증
            Assert.AreEqual(9f, velocityChange.magnitude, 0.0001f); // 외력 크기 검증
            Assert.Greater(velocityChange.x, 0f); // Target 방향 X 검증
            Assert.Greater(velocityChange.z, 0f); // Target 방향 Z 검증
        }

        [Test] // 같은 위치 외력 생성 사례
        public void CreateHorizontalVelocityChange_TargetAtSamePosition_UsesFallbackForward() // 영벡터 발동 실패 방지
        {
            Vector3 velocityChange = ProjectJFireworkPolicy.CreateDefaultHorizontalVelocityChange( // 확정 수평 외력 계산
                Vector3.zero, // 사용자 위치
                Vector3.zero, // 같은 위치 Target
                Vector3.right, // 대체 전방
                Vector3.zero // 기존 외부 속도 없음
            );

            Assert.AreEqual(new Vector3(9f, 0f, 0f), velocityChange); // 대체 전방 외력 검증
        }

        [Test] // 기존 외부 속도 상한 사례
        public void CreateDefaultHorizontalVelocityChange_WithExistingVelocity_EndsAtNineMetersPerSecond() // 외부 속도 9m/s 초과 방지
        {
            Vector3 velocityChange = ProjectJFireworkPolicy.CreateDefaultHorizontalVelocityChange( // 확정 수평 외력 계산
                Vector3.zero, // 사용자 위치
                Vector3.forward, // 전방 Target 위치
                Vector3.forward, // 거리 0 대비 전방
                Vector3.forward * 5f // 기존 전방 외부 속도
            );

            Vector3 finalVelocity = Vector3.forward * 5f + velocityChange; // 적용 후 외부 속도 계산
            Assert.AreEqual(9f, finalVelocity.magnitude, 0.0001f); // 최종 속도 상한 검증
        }
    }
}
