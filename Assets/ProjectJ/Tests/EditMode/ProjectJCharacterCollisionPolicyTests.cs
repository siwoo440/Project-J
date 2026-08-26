using NUnit.Framework; // EditMode Test 사용
using ProjectJ.Movement; // Player 충돌 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJCharacterCollisionPolicyTests
    {
        [Test]
        public void ResolveTravelDistance_StopsAtHitDistance()
        {
            float result =
                ProjectJCharacterCollisionPolicy.ResolveTravelDistance(
                    1f,
                    0.35f
                ); // 충돌 거리 제한 계산

            Assert.That(
                result,
                Is.EqualTo(
                    0.35f
                ).Within(
                    0.0001f
                )
            ); // 충돌 위치 이전 이동 검증
        }

        [Test]
        public void ResolveTravelDistance_RejectsNegativeHitDistance()
        {
            float result =
                ProjectJCharacterCollisionPolicy.ResolveTravelDistance(
                    1f,
                    -0.5f
                ); // 음수 충돌 거리 계산

            Assert.That(
                result,
                Is.EqualTo(
                    0f
                )
            ); // 역방향 이동 차단 검증
        }

        [Test]
        public void ResolveSlideDisplacement_RemovesWallNormalComponent()
        {
            Vector3 result =
                ProjectJCharacterCollisionPolicy.ResolveSlideDisplacement(
                    new Vector3(
                        1f,
                        0f,
                        1f
                    ),
                    Vector3.left
                ); // 정면 벽 충돌 후 Slide 계산

            Assert.That(
                result.x,
                Is.EqualTo(
                    0f
                ).Within(
                    0.0001f
                )
            ); // 벽 내부 방향 제거 검증

            Assert.That(
                result.z,
                Is.EqualTo(
                    1f
                ).Within(
                    0.0001f
                )
            ); // 벽 접선 방향 유지 검증
        }

        [Test]
        public void IsStepHeightAllowed_AllowsNormalStairHeight()
        {
            bool result =
                ProjectJCharacterCollisionPolicy.IsStepHeightAllowed(
                    0f,
                    0.2f,
                    0.35f
                ); // 일반 계단 높이 판정

            Assert.That(
                result,
                Is.True
            ); // 일반 계단 자동 오르기 허용 검증
        }

        [Test]
        public void IsStepHeightAllowed_RejectsTallPlatform()
        {
            bool result =
                ProjectJCharacterCollisionPolicy.IsStepHeightAllowed(
                    0f,
                    0.8f,
                    0.35f
                ); // 높은 발판 높이 판정

            Assert.That(
                result,
                Is.False
            ); // 높은 발판 자동 관통 차단 검증
        }

        [Test]
        public void IsWalkableGroundNormal_RejectsVerticalWall()
        {
            bool result =
                ProjectJCharacterCollisionPolicy.IsWalkableGroundNormal(
                    Vector3.right,
                    0.5f
                ); // 수직 벽 Ground 판정

            Assert.That(
                result,
                Is.False
            ); // 벽 바닥 오인 차단 검증
        }

        [Test]
        public void IsWalkableGroundNormal_AcceptsUpwardSurface()
        {
            bool result =
                ProjectJCharacterCollisionPolicy.IsWalkableGroundNormal(
                    Vector3.up,
                    0.5f
                ); // 수평 바닥 Ground 판정

            Assert.That(
                result,
                Is.True
            ); // 정상 바닥 허용 검증
        }
    }
}
