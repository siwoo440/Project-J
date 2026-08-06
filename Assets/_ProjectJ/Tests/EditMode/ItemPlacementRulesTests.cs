using NUnit.Framework; // EditMode 단위 테스트 기능 참조
using ProjectJ.Items; // 설치 위치 순수 규칙 참조
using UnityEngine; // Unity 벡터와 영역 자료형 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 프로젝트 EditMode 테스트 묶음
    public sealed class ItemPlacementRulesTests // 설치 위치 순수 규칙 테스트 선언
    { // 설치 위치 순수 규칙 테스트 묶음
        [Test] // Unity Test Runner 테스트 지정
        public void IsSlopeAllowedAcceptsFlatGround() // 평평한 지면 허용 확인
        { // 평평한 지면 규칙 테스트 처리
            Assert.IsTrue(ItemPlacementRules.IsSlopeAllowed(Vector3.up, 35f)); // 0도 지면 허용 확인
        } // 평평한 지면 규칙 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void IsSlopeAllowedRejectsSteepGround() // 급경사 지면 차단 확인
        { // 급경사 지면 규칙 테스트 처리
            Vector3 steepNormal = Quaternion.Euler(60f, 0f, 0f) * Vector3.up; // 60도 경사 법선 생성
            Assert.IsFalse(ItemPlacementRules.IsSlopeAllowed(steepNormal, 35f)); // 허용 각도 초과 지면 차단 확인
        } // 급경사 지면 규칙 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void IsInsideBoundsAppliesEdgePadding() // 모듈 가장자리 여백 적용 확인
        { // 모듈 영역 규칙 테스트 처리
            Bounds allowedBounds = new Bounds(Vector3.zero, new Vector3(10f, 2f, 10f)); // 가로와 세로 10미터 검사 영역 생성
            Assert.IsTrue(ItemPlacementRules.IsInsideBounds(new Vector3(3f, 0f, 3f), allowedBounds, 1f)); // 여백 안쪽 위치 허용 확인
            Assert.IsFalse(ItemPlacementRules.IsInsideBounds(new Vector3(4.5f, 0f, 0f), allowedBounds, 1f)); // 가장자리 여백 위치 차단 확인
        } // 모듈 영역 규칙 테스트 처리 종료
    } // 설치 위치 순수 규칙 테스트 묶음 종료
} // 프로젝트 EditMode 테스트 묶음 종료
