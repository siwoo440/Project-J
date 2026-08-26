using System.Collections.Generic; // 후보 목록 사용
using NUnit.Framework; // EditMode Test 사용
using ProjectJ.AI; // Bot 자율 이동 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Tests.EditMode // EditMode Test Namespace
{
    public sealed class ProjectJBotAutonomousNavigationPolicyTests // 자율 이동 정책 Test 모음
    {
        [Test] // 낮은 단차 걷기 검증
        public void SelectBestCandidate_ClassifiesLowStepAsWalk() // 낮은 단차 이동 방식 검증
        {
            List<ProjectJBotTraversalCandidate> candidates = new List<ProjectJBotTraversalCandidate> // 이동 후보 목록 생성
            {
                CreateCandidate(Vector3.forward, 0.2f, true, true, false) // 안전한 낮은 단차 후보 생성
            };

            ProjectJBotTraversalDecision result = Select(candidates, Vector3.zero); // 최적 후보 선택

            Assert.That(result.IsValid, Is.True); // 유효 후보 선택 검증
            Assert.That(result.Action, Is.EqualTo(ProjectJBotTraversalAction.Walk)); // 걷기 분류 검증
        }

        [Test] // 높은 단차 점프 검증
        public void SelectBestCandidate_ClassifiesReachableStepAsJump() // 높은 단차 이동 방식 검증
        {
            List<ProjectJBotTraversalCandidate> candidates = new List<ProjectJBotTraversalCandidate> // 이동 후보 목록 생성
            {
                CreateCandidate(Vector3.forward, 0.8f, true, true, false) // 안전한 높은 단차 후보 생성
            };

            ProjectJBotTraversalDecision result = Select(candidates, Vector3.zero); // 최적 후보 선택

            Assert.That(result.IsValid, Is.True); // 유효 후보 선택 검증
            Assert.That(result.Action, Is.EqualTo(ProjectJBotTraversalAction.Jump)); // 점프 분류 검증
        }

        [Test] // 안전 높이 아래 후보 차단 검증
        public void SelectBestCandidate_RejectsLandingBelowSafetyFloor() // 체크포인트 아래 이동 차단 검증
        {
            List<ProjectJBotTraversalCandidate> candidates = new List<ProjectJBotTraversalCandidate> // 이동 후보 목록 생성
            {
                new ProjectJBotTraversalCandidate( // 낮은 착지 후보 생성
                    Vector3.forward, // 전방 이동 방향
                    new Vector3(0f, -1f, 1f), // 안전 높이 아래 착지 위치
                    -1f, // 큰 하강 높이
                    true, // 바닥 존재 상태
                    true, // 이동 경로 확보 상태
                    true, // 머리 공간 확보 상태
                    false // 틈 통과 아님
                )
            };

            ProjectJBotTraversalDecision result = Select(candidates, Vector3.zero); // 최적 후보 선택

            Assert.That(result.IsValid, Is.False); // 위험 후보 거부 검증
        }

        [Test] // 소환 높이 보정 검증
        public void SelectBestCandidate_AllowsCurrentFloorBelowSpawnMarker() // 현재 바닥 이동 허용 검증
        {
            ProjectJBotTraversalCandidate candidate = new ProjectJBotTraversalCandidate( // 현재 바닥 후보 생성
                Vector3.forward, // 전방 이동 방향
                Vector3.forward, // 현재 높이 착지 위치
                0f, // 평지 높이 차이
                true, // 바닥 존재 상태
                true, // 이동 경로 확보 상태
                true, // 머리 공간 확보 상태
                false // 틈 통과 아님
            );

            ProjectJBotTraversalDecision result = ProjectJBotNavigationPolicy.SelectBestCandidate( // 높은 소환 지점 기준 후보 선택
                new[] { candidate }, // 단일 평지 후보 전달
                Vector3.forward, // 전방 목표 방향 전달
                2f, // 착지 전 소환 지점 높이 전달
                0.35f, // 최대 걷기 단차 전달
                1.5f, // 최대 점프 높이 전달
                0.6f, // 최대 안전 하강 전달
                Vector3.zero // 실패 방향 없음
            );

            Assert.That(result.IsValid, Is.True); // 착지한 현재 평지 허용 검증
            Assert.That(result.Action, Is.EqualTo(ProjectJBotTraversalAction.Walk)); // 평지 걷기 판단 검증
        }

        [Test] // 머리 공간 없는 후보 차단 검증
        public void SelectBestCandidate_RejectsBlockedHeadroom() // 막힌 착지 공간 차단 검증
        {
            List<ProjectJBotTraversalCandidate> candidates = new List<ProjectJBotTraversalCandidate> // 이동 후보 목록 생성
            {
                CreateCandidate(Vector3.forward, 0.5f, true, false, false) // 머리 공간 없는 후보 생성
            };

            ProjectJBotTraversalDecision result = Select(candidates, Vector3.zero); // 최적 후보 선택

            Assert.That(result.IsValid, Is.False); // 막힌 후보 거부 검증
        }

        [Test] // 목표 방향 우선 검증
        public void SelectBestCandidate_PrefersGoalAlignedDirection() // 목표 정렬 점수 검증
        {
            List<ProjectJBotTraversalCandidate> candidates = new List<ProjectJBotTraversalCandidate> // 이동 후보 목록 생성
            {
                CreateCandidate(Vector3.right, 0f, true, true, false), // 옆 방향 후보 생성
                CreateCandidate(Vector3.forward, 0f, true, true, false) // 목표 방향 후보 생성
            };

            ProjectJBotTraversalDecision result = Select(candidates, Vector3.zero); // 최적 후보 선택

            Assert.That(result.Direction, Is.EqualTo(Vector3.forward)); // 목표 방향 선택 검증
        }

        [Test] // 실패 방향 회피 검증
        public void SelectBestCandidate_AvoidsRecentlyFailedDirection() // 정체 방향 감점 검증
        {
            List<ProjectJBotTraversalCandidate> candidates = new List<ProjectJBotTraversalCandidate> // 이동 후보 목록 생성
            {
                CreateCandidate(Vector3.forward, 0f, true, true, false), // 실패한 전방 후보 생성
                CreateCandidate(Vector3.right, 0f, true, true, false) // 대체 옆 방향 후보 생성
            };

            ProjectJBotTraversalDecision result = Select(candidates, Vector3.forward); // 실패 방향을 포함해 후보 선택

            Assert.That(result.Direction, Is.EqualTo(Vector3.right)); // 대체 방향 선택 검증
        }

        [TestCase(2f, 0.5f, true)] // 가까운 높은 착지 가능 사례
        [TestCase(10f, 0.5f, false)] // 먼 높은 착지 불가능 사례
        public void CanReachLanding_UsesExistingMovementStatistics( // 기존 이동 능력 기반 도달 검증
            float horizontalDistance, // 수평 착지 거리
            float heightDelta, // 착지 높이 차이
            bool expected // 기대 도달 가능 여부
        )
        {
            bool result = ProjectJBotNavigationPolicy.CanReachLanding( // 포물선 도달 가능 여부 계산
                horizontalDistance, // 수평 거리 전달
                heightDelta, // 높이 차이 전달
                5f, // 기존 걷기 속도 전달
                7f, // 기존 점프 속도 전달
                -20f, // 기존 중력 전달
                0.85f // 안전 여유 배율 전달
            );

            Assert.That(result, Is.EqualTo(expected)); // 도달 가능 결과 검증
        }

        private static ProjectJBotTraversalDecision Select( // 공통 후보 선택 실행
            IReadOnlyList<ProjectJBotTraversalCandidate> candidates, // 이동 후보 목록
            Vector3 failedDirection // 최근 실패 방향
        )
        {
            return ProjectJBotNavigationPolicy.SelectBestCandidate( // 정책 후보 선택 결과 반환
                candidates, // 이동 후보 목록 전달
                Vector3.forward, // 전방 목표 방향 전달
                0f, // 체크포인트 안전 높이 전달
                0.35f, // 최대 걷기 단차 전달
                1.5f, // 최대 점프 높이 전달
                0.6f, // 최대 안전 하강 전달
                failedDirection // 실패 방향 전달
            );
        }

        private static ProjectJBotTraversalCandidate CreateCandidate( // 공통 이동 후보 생성
            Vector3 direction, // 이동 방향
            float heightDelta, // 착지 높이 차이
            bool pathClear, // 이동 경로 확보 여부
            bool hasHeadroom, // 머리 공간 확보 여부
            bool crossesGap // 틈 통과 여부
        )
        {
            return new ProjectJBotTraversalCandidate( // 이동 후보 반환
                direction, // 이동 방향 전달
                new Vector3(direction.x, heightDelta, direction.z), // 착지 위치 생성
                heightDelta, // 착지 높이 차이 전달
                true, // 바닥 존재 상태 전달
                pathClear, // 이동 경로 상태 전달
                hasHeadroom, // 머리 공간 상태 전달
                crossesGap // 틈 통과 상태 전달
            );
        }
    }
}
