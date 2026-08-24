using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 지뢰 정책 사용
using UnityEngine; // Vector3 계산 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJMinePolicyTests // 지뢰 정책 테스트
    {
        [Test] // 기획 수치 고정 사례
        public void Constants_MatchDay110PlannedValues() // Definition과 폭발 수치 변경 감지
        {
            Assert.AreEqual(25f, ProjectJMinePolicy.LifetimeSeconds, 0.0001f); // 유지 시간 검증
            Assert.AreEqual(0.75f, ProjectJMinePolicy.ArmSeconds, 0.0001f); // 활성화 시간 검증
            Assert.AreEqual(2.25f, ProjectJMinePolicy.TriggerRadius, 0.0001f); // 감지 반경 검증
            Assert.AreEqual(3.5f, ProjectJMinePolicy.ExplosionRadius, 0.0001f); // 폭발 반경 검증
            Assert.AreEqual(8f, ProjectJMinePolicy.OutwardVelocity, 0.0001f); // 바깥쪽 속도 검증
            Assert.AreEqual(6f, ProjectJMinePolicy.UpwardVelocity, 0.0001f); // 위쪽 속도 검증
        }

        [TestCase(true, 1f, true, true, true)] // 정상 지면 사례
        [TestCase(false, 1f, true, true, false)] // 지면 누락 사례
        [TestCase(true, 0.64f, true, true, false)] // 급경사 사례
        [TestCase(true, 0.65f, true, true, true)] // 경사 경계 사례
        [TestCase(true, 1f, false, true, false)] // 공통 금지 구역 사례
        [TestCase(true, 1f, true, false, false)] // 기존 지뢰 근접 사례
        public void CanPlace_WithPlacementState_ReturnsExpected( // 잘못된 설치 방지
            bool groundFound, // 지면 탐색 여부
            float groundDot, // 지면 위쪽 각도 값
            bool commonPlacementAllowed, // 공통 설치 허용 여부
            bool separatedFromMines, // 기존 지뢰 거리 허용 여부
            bool expected // 예상 설치 결과
        )
        {
            bool canPlace = ProjectJMinePolicy.CanPlace( // 설치 가능 여부 계산
                groundFound, // 지면 탐색 상태 전달
                groundDot, // 지면 각도 전달
                commonPlacementAllowed, // 공통 설치 상태 전달
                separatedFromMines // 지뢰 간격 상태 전달
            );

            Assert.AreEqual(expected, canPlace); // 설치 결과 검증
        }

        [TestCase(true, true, false, false, false, false, true)] // 정상 상대 사례
        [TestCase(false, true, false, false, false, false, false)] // Runner 누락 사례
        [TestCase(true, false, false, false, false, false, false)] // 경기 입력 잠금 사례
        [TestCase(true, true, true, false, false, false, false)] // 소유자 사례
        [TestCase(true, true, false, true, false, false, false)] // 완주자 사례
        [TestCase(true, true, false, false, true, false, false)] // 부활 보호 사례
        [TestCase(true, true, false, false, false, true, false)] // Jelly 보호막 사례
        public void CanAffectTarget_WithTargetState_ReturnsExpected( // 잘못된 폭발 대상 방지
            bool runnerReady, // Runner 준비 여부
            bool gameplayAllowed, // 경기 입력 허용 여부
            bool isOwner, // 소유자 여부
            bool isFinished, // 완주 여부
            bool isRespawnProtected, // 부활 보호 여부
            bool isShielded, // Jelly 보호막 여부
            bool expected // 예상 적용 결과
        )
        {
            bool canAffect = ProjectJMinePolicy.CanAffectTarget( // 폭발 적용 가능 여부 계산
                runnerReady, // Runner 상태 전달
                gameplayAllowed, // 경기 상태 전달
                isOwner, // 소유자 상태 전달
                isFinished, // 완주 상태 전달
                isRespawnProtected, // 부활 보호 전달
                isShielded // 보호막 상태 전달
            );

            Assert.AreEqual(expected, canAffect); // 적용 결과 검증
        }

        [TestCase(false, true, false)] // 활성화 전 사례
        [TestCase(true, false, false)] // 유효 Target 없음 사례
        [TestCase(true, true, true)] // 활성화 후 유효 Target 사례
        public void ShouldTrigger_WithArmedAndTargetState_ReturnsExpected( // 조기 폭발 방지
            bool isArmed, // 활성화 여부
            bool hasValidTarget, // 유효 Target 존재 여부
            bool expected // 예상 폭발 결과
        )
        {
            bool shouldTrigger = ProjectJMinePolicy.ShouldTrigger( // 폭발 시작 여부 계산
                isArmed, // 활성화 상태 전달
                hasValidTarget // Target 상태 전달
            );

            Assert.AreEqual(expected, shouldTrigger); // 폭발 시작 결과 검증
        }

        [TestCase(1.49f, false)] // 최소 간격 미만 사례
        [TestCase(1.5f, true)] // 최소 간격 경계 사례
        [TestCase(3f, true)] // 충분한 간격 사례
        public void IsSeparatedFromMine_WithDistance_ReturnsExpected( // 지뢰 중첩 설치 방지
            float distance, // 기존 지뢰와 거리
            bool expected // 예상 간격 결과
        )
        {
            bool isSeparated = ProjectJMinePolicy.IsSeparatedFromMine( // 지뢰 간격 여부 계산
                distance // 거리 전달
            );

            Assert.AreEqual(expected, isSeparated); // 간격 결과 검증
        }

        [TestCase(2.49f, 0f, true)] // 시작 지점 수평 보호 반경 내부 사례
        [TestCase(2.5f, 3f, true)] // 수평·수직 보호 경계 사례
        [TestCase(2.51f, 0f, false)] // 수평 보호 반경 초과 사례
        [TestCase(1f, 3.01f, false)] // 수직 보호 범위 초과 사례
        public void IsInsideProtectedStartRadius_WithOffsets_ReturnsExpected( // Fusion 시작 지점 설치 차단 검증
            float horizontalDistance, // 수평 거리
            float verticalDistance, // 수직 거리
            bool expected // 예상 보호 범위 결과
        )
        {
            bool isProtected = ProjectJMinePolicy.IsInsideProtectedStartRadius( // 시작 지점 보호 범위 계산
                new Vector3(horizontalDistance, verticalDistance, 0f), // 설치 후보 위치
                Vector3.zero // 시작 부활 위치
            );

            Assert.AreEqual(expected, isProtected); // 보호 범위 결과 검증
        }

        [Test] // 일반 폭발 방향 사례
        public void CreateExplosionVelocityChange_TargetOffset_ReturnsOutwardAndUpwardForce() // 3차원 폭발 외력 검증
        {
            Vector3 velocityChange = ProjectJMinePolicy.CreateExplosionVelocityChange( // 폭발 외력 계산
                Vector3.zero, // 지뢰 위치
                new Vector3(3f, 2f, 4f), // Target 위치
                Vector3.forward // 같은 위치 대체 방향
            );

            Assert.AreEqual(ProjectJMinePolicy.UpwardVelocity, velocityChange.y, 0.0001f); // 위쪽 속도 검증
            Assert.AreEqual(ProjectJMinePolicy.OutwardVelocity, new Vector2(velocityChange.x, velocityChange.z).magnitude, 0.0001f); // 바깥쪽 속도 검증
            Assert.Greater(velocityChange.x, 0f); // 바깥 X 방향 검증
            Assert.Greater(velocityChange.z, 0f); // 바깥 Z 방향 검증
        }

        [Test] // 같은 위치 폭발 방향 사례
        public void CreateExplosionVelocityChange_SamePosition_UsesFallbackDirection() // 영벡터 폭발 방지
        {
            Vector3 velocityChange = ProjectJMinePolicy.CreateExplosionVelocityChange( // 폭발 외력 계산
                Vector3.zero, // 지뢰 위치
                Vector3.zero, // 같은 Target 위치
                Vector3.right // 대체 방향
            );

            Assert.AreEqual(new Vector3(8f, 6f, 0f), velocityChange); // 대체 방향 결과 검증
        }

        [Test] // 잘못된 대체 방향 사례
        public void CreateExplosionVelocityChange_ZeroFallback_UsesForwardDirection() // 대체 방향 누락 방지
        {
            Vector3 velocityChange = ProjectJMinePolicy.CreateExplosionVelocityChange( // 폭발 외력 계산
                Vector3.zero, // 지뢰 위치
                Vector3.zero, // 같은 Target 위치
                Vector3.zero // 잘못된 대체 방향
            );

            Assert.AreEqual(new Vector3(0f, 6f, 8f), velocityChange); // 기본 전방 결과 검증
        }
    }
}
