using System.Collections.Generic; // Route 위치와 순서 목록 사용
using NUnit.Framework; // EditMode Test 사용
using ProjectJ.AI; // Bot Navigation 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJBotNavigationPolicyTests
    {
        [Test]
        public void ResolvePlanarDirection_RemovesVerticalComponent()
        {
            Vector3 result =
                ProjectJBotNavigationPolicy.ResolvePlanarDirection(
                    new Vector3(
                        0f,
                        10f,
                        0f
                    ),
                    new Vector3(
                        3f,
                        100f,
                        4f
                    )
                ); // 수직 차이가 큰 Target 방향 계산

            Assert.That(
                result.y,
                Is.EqualTo(
                    0f
                ).Within(
                    0.0001f
                )
            ); // 수직 성분 제거 검증

            Assert.That(
                result.magnitude,
                Is.EqualTo(
                    1f
                ).Within(
                    0.0001f
                )
            ); // 수평 방향 정규화 검증
        }

        [Test]
        public void ResolvePlanarDirection_ReturnsZeroForSamePlanarPosition()
        {
            Vector3 result =
                ProjectJBotNavigationPolicy.ResolvePlanarDirection(
                    new Vector3(
                        2f,
                        0f,
                        3f
                    ),
                    new Vector3(
                        2f,
                        20f,
                        3f
                    )
                ); // 동일 수평 위치 방향 계산

            Assert.That(
                result,
                Is.EqualTo(
                    Vector3.zero
                )
            ); // 수평 이동 없음 검증
        }

        [Test]
        public void HasReached_UsesThreeDimensionalDistance()
        {
            bool reached =
                ProjectJBotNavigationPolicy.HasReached(
                    Vector3.zero,
                    new Vector3(
                        0f,
                        2f,
                        0f
                    ),
                    0.75f
                ); // 상단 플랫폼 아래 위치 도달 판정

            Assert.That(
                reached,
                Is.False
            ); // 수평만 같은 위치 조기 통과 차단 검증
        }

        [Test]
        public void ShouldPulseJump_RequiresGroundAndUnusedJump()
        {
            bool canJump =
                ProjectJBotNavigationPolicy.ShouldPulseJump(
                    true,
                    true,
                    1f,
                    1.5f,
                    false
                ); // 정상 점프 조건 판정

            bool airborneJump =
                ProjectJBotNavigationPolicy.ShouldPulseJump(
                    true,
                    false,
                    1f,
                    1.5f,
                    false
                ); // 공중 점프 조건 판정

            bool consumedJump =
                ProjectJBotNavigationPolicy.ShouldPulseJump(
                    true,
                    true,
                    1f,
                    1.5f,
                    true
                ); // 이미 소비한 점프 조건 판정

            Assert.That(
                canJump,
                Is.True
            ); // 정상 점프 허용 검증

            Assert.That(
                airborneJump,
                Is.False
            ); // 공중 재점프 차단 검증

            Assert.That(
                consumedJump,
                Is.False
            ); // 동일 Node 중복 점프 차단 검증
        }

        [Test]
        public void ShouldPulseJump_RejectsOutsideTriggerDistance()
        {
            bool result =
                ProjectJBotNavigationPolicy.ShouldPulseJump(
                    true,
                    true,
                    1.51f,
                    1.5f,
                    false
                ); // 점프 거리 경계 밖 판정

            Assert.That(
                result,
                Is.False
            ); // 점프 거리 초과 차단 검증
        }

        [Test]
        public void ResolveCheckpointMinimumRouteOrder_UsesHundredStep()
        {
            int result =
                ProjectJBotNavigationPolicy.ResolveCheckpointMinimumRouteOrder(
                    3
                ); // CP3 최소 Route Order 계산

            Assert.That(
                result,
                Is.EqualTo(
                    300
                )
            ); // CP3 이전 Route 차단 기준 검증
        }

        [Test]
        public void FindFirstRouteIndexAtOrAfterOrder_SkipsPreviousCheckpointRoutes()
        {
            List<int> routeOrders =
                new List<int>
                {
                    0,
                    25,
                    100,
                    150,
                    200,
                    250,
                    300
                }; // Checkpoint 포함 Route Order 목록 생성

            int result =
                ProjectJBotNavigationPolicy.FindFirstRouteIndexAtOrAfterOrder(
                    routeOrders,
                    200
                ); // CP2 이후 첫 Route 검색

            Assert.That(
                result,
                Is.EqualTo(
                    4
                )
            ); // CP2 이전 Route 제외 검증
        }

        [Test]
        public void ShouldRecoverFromStuck_ReturnsTrueAfterTimeoutWithoutProgress()
        {
            bool result =
                ProjectJBotNavigationPolicy.ShouldRecoverFromStuck(
                    Vector3.zero,
                    new Vector3(
                        0.1f,
                        0f,
                        0f
                    ),
                    0.25f,
                    2.5f,
                    2.5f
                ); // 제한 시간 동안 최소 거리 미만 이동 상태 판정

            Assert.That(
                result,
                Is.True
            ); // Stuck 복구 허용 검증
        }

        [Test]
        public void ShouldRecoverFromStuck_ReturnsFalseWhenProgressIsEnough()
        {
            bool result =
                ProjectJBotNavigationPolicy.ShouldRecoverFromStuck(
                    Vector3.zero,
                    new Vector3(
                        0.25f,
                        0f,
                        0f
                    ),
                    0.25f,
                    3f,
                    2.5f
                ); // 최소 이동 거리 충족 상태 판정

            Assert.That(
                result,
                Is.False
            ); // 정상 진행 상태 복구 차단 검증
        }

        [Test]
        public void ShouldRecoverFromStuck_ReturnsFalseBeforeTimeout()
        {
            bool result =
                ProjectJBotNavigationPolicy.ShouldRecoverFromStuck(
                    Vector3.zero,
                    Vector3.zero,
                    0.25f,
                    2.49f,
                    2.5f
                ); // 제한 시간 직전 정체 상태 판정

            Assert.That(
                result,
                Is.False
            ); // 조기 Stuck 복구 차단 검증
        }

        [Test]
        public void ShouldRecoverFromStuck_IgnoresVerticalOnlyMovement()
        {
            bool result =
                ProjectJBotNavigationPolicy.ShouldRecoverFromStuck(
                    Vector3.zero,
                    new Vector3(
                        0f,
                        2f,
                        0f
                    ),
                    0.25f,
                    2.5f,
                    2.5f
                ); // 제자리 점프와 같은 수직 이동만 존재하는 상태 판정

            Assert.That(
                result,
                Is.True
            ); // 수평 진행 없는 상태 Stuck 복구 검증
        }

        [Test]
        public void FindNearestRouteIndex_ReturnsClosestAllowedNode()
        {
            List<Vector3> route =
                new List<Vector3>
                {
                    new Vector3(
                        0f,
                        0f,
                        0f
                    ),
                    new Vector3(
                        10f,
                        0f,
                        0f
                    ),
                    new Vector3(
                        20f,
                        0f,
                        0f
                    )
                }; // Route 위치 목록 생성

            int result =
                ProjectJBotNavigationPolicy.FindNearestRouteIndex(
                    new Vector3(
                        11f,
                        0f,
                        0f
                    ),
                    route,
                    1
                ); // 두 번째 Node 이후 최근접 검색

            Assert.That(
                result,
                Is.EqualTo(
                    1
                )
            ); // 최근접 Route Index 검증
        }
    }
}
