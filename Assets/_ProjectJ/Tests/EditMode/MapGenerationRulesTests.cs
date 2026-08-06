using NUnit.Framework; // NUnit 자동 테스트 기능 참조
using ProjectJ.MapGeneration; // 맵 생성 규칙 참조
using UnityEngine; // Unity 벡터와 영역 기능 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class MapGenerationRulesTests // 맵 생성 배치 규칙 자동 테스트 선언
    { // 맵 생성 배치 규칙 자동 테스트 묶음
        [Test] // 자동 테스트 항목 표시
        public void AllRotationOptionReturnsFourQuarterTurns() // 모든 직각 회전 목록 확인
        { // 모든 회전 목록 테스트 처리
            int[] result = MapGenerationRules.GetAllowedQuarterTurns(MapRotationOptions.All); // 전체 허용 회전 목록 조회
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, result); // 네 직각 회전 순서 확인
        } // 모든 회전 목록 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void SingleRotationOptionReturnsOnlySelectedTurn() // 단일 직각 회전 목록 확인
        { // 단일 회전 목록 테스트 처리
            int[] result = MapGenerationRules.GetAllowedQuarterTurns(MapRotationOptions.Degrees180); // 180도 허용 회전 목록 조회
            CollectionAssert.AreEqual(new[] { 2 }, result); // 180도 회전만 포함 확인
        } // 단일 회전 목록 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void NorthAndSouthWorldDirectionsAreOpposite() // 북쪽과 남쪽 마주 보기 확인
        { // 반대 방향 테스트 처리
            bool result = MapGenerationRules.AreWorldDirectionsOpposite(Vector3.forward, Vector3.back); // 앞쪽과 뒤쪽 방향 비교
            Assert.IsTrue(result); // 반대 방향 판정 확인
        } // 반대 방향 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void SameWorldDirectionsAreNotOpposite() // 같은 방향 마주 보기 제외 확인
        { // 같은 방향 테스트 처리
            bool result = MapGenerationRules.AreWorldDirectionsOpposite(Vector3.right, Vector3.right); // 같은 오른쪽 방향 비교
            Assert.IsFalse(result); // 반대 방향 제외 확인
        } // 같은 방향 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void RootAlignmentMovesCandidateConnectionToTarget() // 연결 지점 일치용 루트 이동 확인
        { // 연결 위치 정렬 테스트 처리
            Vector3 result = MapGenerationRules.CalculateAlignedRootPosition(new Vector3(10f, 0f, 5f), new Vector3(20f, 2f, 8f), new Vector3(12f, 1f, 7f)); // 현재 루트와 두 연결 지점으로 새 위치 계산
            Assert.AreEqual(new Vector3(18f, 1f, 6f), result); // 연결 위치 차이 적용 결과 확인
        } // 연결 위치 정렬 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void TouchingBoundsAreNotBlockingOverlap() // 경계만 맞닿은 모듈 허용 확인
        { // 맞닿은 영역 테스트 처리
            Bounds firstBounds = new Bounds(Vector3.zero, new Vector3(4f, 2f, 8f)); // 첫 모듈 영역 생성
            Bounds secondBounds = new Bounds(new Vector3(0f, 0f, 8f), new Vector3(4f, 2f, 8f)); // Z축 경계가 맞닿은 둘째 영역 생성
            bool result = MapGenerationRules.BoundsHaveBlockingOverlap(firstBounds, secondBounds, 0.05f); // 맞닿은 영역 겹침 검사
            Assert.IsFalse(result); // 경계 접촉 허용 확인
        } // 맞닿은 영역 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void IntersectingBoundsAreBlockingOverlap() // 실제 교차 모듈 차단 확인
        { // 실제 교차 영역 테스트 처리
            Bounds firstBounds = new Bounds(Vector3.zero, new Vector3(4f, 2f, 8f)); // 첫 모듈 영역 생성
            Bounds secondBounds = new Bounds(new Vector3(0f, 0f, 7f), new Vector3(4f, 2f, 8f)); // Z축 1미터 교차 영역 생성
            bool result = MapGenerationRules.BoundsHaveBlockingOverlap(firstBounds, secondBounds, 0.05f); // 실제 교차 영역 겹침 검사
            Assert.IsTrue(result); // 실제 교차 차단 확인
        } // 실제 교차 영역 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void QuarterTurnRotationFacesEast() // 90도 회전 앞 방향 확인
        { // 90도 회전 테스트 처리
            Quaternion rotation = MapGenerationRules.QuarterTurnRotation(1); // 시계 방향 90도 회전 계산
            Vector3 result = rotation * Vector3.forward; // 회전 후 앞 방향 계산
            Assert.That(Vector3.Dot(result, Vector3.right), Is.EqualTo(1f).Within(0.001f)); // 동쪽 방향 일치 확인
        } // 90도 회전 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void MatchingConnectionSizesAreCompatible() // 같은 연결부 크기 호환 확인
        { // 같은 연결부 크기 테스트 처리
            bool result = MapGenerationRules.AreConnectionSizesCompatible(2f, 2.2f, 2f, 2.2f, 0.05f); // 동일한 너비와 높이 비교
            Assert.IsTrue(result); // 같은 연결부 크기 허용 확인
        } // 같은 연결부 크기 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void DifferentConnectionWidthsAreRejected() // 차이가 큰 연결부 너비 차단 확인
        { // 연결부 너비 차이 테스트 처리
            bool result = MapGenerationRules.AreConnectionSizesCompatible(2f, 2.2f, 3f, 2.2f, 0.05f); // 다른 너비와 같은 높이 비교
            Assert.IsFalse(result); // 연결부 너비 불일치 차단 확인
        } // 연결부 너비 차이 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void NearbyConnectionPositionsAreAligned() // 허용 오차 안의 연결 위치 정렬 확인
        { // 연결 위치 허용 오차 테스트 처리
            bool result = MapGenerationRules.AreConnectionPositionsAligned(Vector3.zero, new Vector3(0.01f, 0f, 0f), 0.02f); // 가까운 두 연결 위치 비교
            Assert.IsTrue(result); // 허용 오차 안의 위치 일치 확인
        } // 연결 위치 허용 오차 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void DistantConnectionPositionsAreNotAligned() // 허용 오차 밖의 연결 위치 차단 확인
        { // 연결 위치 차단 테스트 처리
            bool result = MapGenerationRules.AreConnectionPositionsAligned(Vector3.zero, new Vector3(0.1f, 0f, 0f), 0.02f); // 떨어진 두 연결 위치 비교
            Assert.IsFalse(result); // 허용 오차 밖의 위치 차단 확인
        } // 연결 위치 차단 테스트 처리
    } // 맵 생성 배치 규칙 자동 테스트 묶음
} // EditMode 테스트 묶음
