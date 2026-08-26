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

        [Test] // 자율 후보 선택 계약 검증
        public void SelectBestCandidate_RejectsUnsafeDrop() // 체크포인트 아래 후보 차단 검증
        {
            List<ProjectJBotTraversalCandidate> candidates = new List<ProjectJBotTraversalCandidate> // 이동 후보 목록 생성
            {
                new ProjectJBotTraversalCandidate( // 위험 후보 생성
                    Vector3.forward, // 전방 이동 방향
                    new Vector3(0f, -1f, 1f), // 안전 높이 아래 착지 위치
                    -1f, // 착지 높이 차이
                    true, // 바닥 존재 상태
                    true, // 경로 확보 상태
                    true, // 머리 공간 확보 상태
                    false // 틈 통과 아님
                )
            };

            ProjectJBotTraversalDecision result = ProjectJBotNavigationPolicy.SelectBestCandidate( // 최적 후보 선택
                candidates, // 이동 후보 목록 전달
                Vector3.forward, // 목표 방향 전달
                0f, // 체크포인트 안전 높이 전달
                0.35f, // 걷기 단차 한계 전달
                1.5f, // 점프 높이 한계 전달
                0.6f, // 안전 하강 한계 전달
                Vector3.zero // 실패 방향 없음
            );

            Assert.That(result.IsValid, Is.False); // 위험 후보 거부 검증
        }

        [Test] // 센서 탐색 방향 수 검증
        public void BuildSampleDirections_IncludesTwelveDirections() // 전후좌우 탐색 범위 검증
        {
            Vector3[] directions = ProjectJBotTraversalSensor.BuildSampleDirections(Vector3.forward); // 전방 기준 탐색 방향 생성

            Assert.That(directions.Length, Is.EqualTo(12)); // 열두 방향 생성 검증
            Assert.That(Vector3.Dot(directions[11], Vector3.back), Is.GreaterThan(0.99f)); // 후방 방향 포함 검증
        }

        [Test] // 바닥 없는 공간 차단 검증
        public void TrySelectTraversal_RejectsFieldWithoutGround() // 낭떠러지 진입 차단 검증
        {
            GameObject botObject = new GameObject("Bot Sensor Test"); // 센서 시험 객체 생성

            try // 시험 객체 정리 보장
            {
                ProjectJBotTraversalSensor sensor = botObject.AddComponent<ProjectJBotTraversalSensor>(); // 이동 센서 추가
                bool selected = sensor.TrySelectTraversal( // 바닥 없는 공간 탐색
                    Vector3.zero, // 현재 발 위치 전달
                    Vector3.forward, // 목표 방향 전달
                    0f, // 안전 높이 전달
                    5f, // 걷기 속도 전달
                    7f, // 점프 속도 전달
                    -20f, // 중력 전달
                    0.4f, // 몸 반경 전달
                    2f, // 몸 높이 전달
                    Vector3.zero, // 실패 방향 없음
                    out ProjectJBotTraversalDecision decision // 이동 판단 수신
                );

                Assert.That(selected, Is.False); // 이동 후보 없음 검증
                Assert.That(decision.IsValid, Is.False); // 무효 판단 검증
            }
            finally // 시험 객체 정리 구간
            {
                Object.DestroyImmediate(botObject); // 센서 시험 객체 제거
            }
        }

        [Test] // 평지 이동 후보 탐색 검증
        public void TrySelectTraversal_SelectsSafeFlatGround() // 안전한 평지 전진 검증
        {
            GameObject botObject = new GameObject("Bot Sensor Flat Test"); // 센서 시험 객체 생성
            GameObject floorObject = GameObject.CreatePrimitive(PrimitiveType.Cube); // 시험 바닥 생성
            floorObject.name = "Bot Sensor Floor"; // 시험 바닥 이름 설정
            floorObject.transform.position = new Vector3(0f, -0.1f, 0f); // 발 아래 바닥 위치 설정
            floorObject.transform.localScale = new Vector3(8f, 0.2f, 8f); // 모든 탐색 방향 바닥 크기 설정

            try // 시험 객체 정리 보장
            {
                Physics.SyncTransforms(); // 시험 Collider 위치 동기화
                ProjectJBotTraversalSensor sensor = botObject.AddComponent<ProjectJBotTraversalSensor>(); // 이동 센서 추가
                bool selected = sensor.TrySelectTraversal( // 안전한 평지 탐색
                    Vector3.zero, // 현재 발 위치 전달
                    Vector3.forward, // 목표 방향 전달
                    0f, // 안전 높이 전달
                    5f, // 걷기 속도 전달
                    7f, // 점프 속도 전달
                    -20f, // 중력 전달
                    0.4f, // 몸 반경 전달
                    2f, // 몸 높이 전달
                    Vector3.zero, // 실패 방향 없음
                    out ProjectJBotTraversalDecision decision // 이동 판단 수신
                );

                Assert.That(selected, Is.True); // 안전 이동 후보 존재 검증
                Assert.That(decision.Action, Is.EqualTo(ProjectJBotTraversalAction.Walk)); // 평지 걷기 판단 검증
                Assert.That(Vector3.Dot(decision.Direction, Vector3.forward), Is.GreaterThan(0.99f)); // 목표 방향 선택 검증
            }
            finally // 시험 객체 정리 구간
            {
                Object.DestroyImmediate(botObject); // 센서 시험 객체 제거
                Object.DestroyImmediate(floorObject); // 시험 바닥 제거
            }
        }

        [Test] // 다음 체크포인트 선택 검증
        public void FindNextCheckpointIndex_SkipsActivatedCheckpoints() // 활성 체크포인트 이전 목표 제외 검증
        {
            List<int> checkpointIds = new List<int> // 정렬된 체크포인트 ID 목록 생성
            {
                1, // CP1 ID
                2, // CP2 ID
                3, // CP3 ID
                4 // CP4 ID
            };

            int result = ProjectJBotNavigationPolicy.FindNextCheckpointIndex(2, checkpointIds); // CP2 이후 목표 검색

            Assert.That(result, Is.EqualTo(2)); // CP3 목록 Index 선택 검증
        }
    }
}
