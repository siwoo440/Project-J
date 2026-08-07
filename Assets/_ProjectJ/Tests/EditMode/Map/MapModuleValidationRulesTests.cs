using NUnit.Framework; // NUnit 자동 테스트 기능 참조
using ProjectJ.MapGeneration; // 맵 모듈 검증 규칙 참조
using UnityEngine; // Unity 벡터 기능 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class MapModuleValidationRulesTests // 맵 모듈 규격 자동 테스트 선언
    { // 맵 모듈 규격 자동 테스트 묶음
        [Test] // 자동 테스트 항목 표시
        public void ValidModuleIdUsesMapPrefix() // MAP 접두사 ID 허용 확인
        { // 모듈 ID 허용 테스트 처리
            bool result = MapModuleValidationRules.IsValidModuleId("MAP-001"); // 정상 모듈 ID 검사
            Assert.IsTrue(result); // 정상 모듈 ID 허용 확인
        } // 모듈 ID 허용 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void InvalidModuleIdWithoutMapPrefixIsRejected() // MAP 접두사 없는 ID 차단 확인
        { // 모듈 ID 차단 테스트 처리
            bool result = MapModuleValidationRules.IsValidModuleId("001"); // 잘못된 모듈 ID 검사
            Assert.IsFalse(result); // 잘못된 모듈 ID 차단 확인
        } // 모듈 ID 차단 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void BoundsSizeNeverContainsZeroAxis() // 모듈 영역의 0 크기 축 보정 확인
        { // 모듈 영역 보정 테스트 처리
            Vector3 result = MapModuleValidationRules.ClampBoundsSize(new Vector3(4f, 0f, -2f)); // 0과 음수 축 크기 보정
            Assert.AreEqual(new Vector3(4f, 0.1f, 0.1f), result); // 모든 축 최소 크기 확인
        } // 모듈 영역 보정 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void NorthExitConnectsToSouthEntrance() // 북쪽 출구와 남쪽 입구 연결 확인
        { // 반대 방향 연결 테스트 처리
            bool result = MapModuleValidationRules.AreDirectionsCompatible(MapConnectionDirection.North, MapConnectionDirection.South); // 북쪽과 남쪽 방향 비교
            Assert.IsTrue(result); // 서로 마주 보는 방향 확인
        } // 반대 방향 연결 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void SameConnectionDirectionsAreRejected() // 같은 방향 연결 차단 확인
        { // 같은 방향 연결 테스트 처리
            bool result = MapModuleValidationRules.AreDirectionsCompatible(MapConnectionDirection.North, MapConnectionDirection.North); // 같은 방향 비교
            Assert.IsFalse(result); // 같은 방향 연결 차단 확인
        } // 같은 방향 연결 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void ClockwiseQuarterTurnRotatesNorthToEast() // 시계 방향 90도 회전 확인
        { // 연결 방향 회전 테스트 처리
            MapConnectionDirection result = MapModuleValidationRules.RotateDirection(MapConnectionDirection.North, 1); // 북쪽 방향 90도 회전
            Assert.AreEqual(MapConnectionDirection.East, result); // 동쪽 방향 결과 확인
        } // 연결 방향 회전 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void DefaultLowPassageHeightRequiresCrouching() // 기본 낮은 통로의 앉기 전용 조건 확인
        { // 낮은 통로 테스트 처리
            bool result = MapModuleValidationRules.IsCrouchPassageValid(1.5f, 1.2f, 2f, 0.1f); // 1.5미터 통로 검사
            Assert.IsTrue(result); // 앉기 전용 통로 판정 확인
        } // 낮은 통로 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void PassageLowerThanCrouchHeightIsRejected() // 앉기 높이보다 낮은 통로 차단 확인
        { // 낮은 통로 차단 테스트 처리
            bool result = MapModuleValidationRules.IsCrouchPassageValid(1.2f, 1.2f, 2f, 0.1f); // 여유 없는 통로 검사
            Assert.IsFalse(result); // 너무 낮은 통로 차단 확인
        } // 낮은 통로 차단 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void StandingClearanceIsNotClassifiedAsCrouchOnly() // 서서 통과 가능한 높이의 앉기 전용 제외 확인
        { // 서기 통로 테스트 처리
            bool result = MapModuleValidationRules.IsCrouchPassageValid(2f, 1.2f, 2f, 0.1f); // 서기 높이 통로 검사
            Assert.IsFalse(result); // 앉기 전용 분류 제외 확인
        } // 서기 통로 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void DefaultPlayerValuesAllowFourMeterJumpGap() // 기본 플레이어의 4미터 점프 허용 확인
        { // 안전 점프 거리 테스트 처리
            float maximumDistance = MapModuleValidationRules.CalculateSafeJumpDistance(6f, 2.4f, 25f, 0.8f); // 기본 수치 안전 점프 거리 계산
            bool result = MapModuleValidationRules.IsJumpPassageValid(4f, 0f, maximumDistance, 2.3f, 3f); // 4미터 평지 점프 검사
            Assert.IsTrue(result); // 4미터 점프 허용 확인
        } // 안전 점프 거리 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void JumpBeyondSafeDistanceIsRejected() // 안전 거리 초과 점프 차단 확인
        { // 점프 거리 초과 테스트 처리
            float maximumDistance = MapModuleValidationRules.CalculateSafeJumpDistance(6f, 2.4f, 25f, 0.8f); // 기본 수치 안전 점프 거리 계산
            bool result = MapModuleValidationRules.IsJumpPassageValid(maximumDistance + 0.2f, 0f, maximumDistance, 2.3f, 3f); // 안전 거리 초과 점프 검사
            Assert.IsFalse(result); // 안전 거리 초과 차단 확인
        } // 점프 거리 초과 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void JumpRiseBeyondSafeHeightIsRejected() // 안전 상승 높이 초과 점프 차단 확인
        { // 점프 상승 초과 테스트 처리
            bool result = MapModuleValidationRules.IsJumpPassageValid(2f, 2.4f, 4.2f, 2.3f, 3f); // 안전 상승 높이 초과 점프 검사
            Assert.IsFalse(result); // 안전 상승 높이 초과 차단 확인
        } // 점프 상승 초과 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void DropBeyondSafeHeightIsRejected() // 안전 낙하 높이 초과 차단 확인
        { // 낙하 높이 초과 테스트 범위
            bool result = MapModuleValidationRules.IsJumpPassageValid(2f, -3.2f, 4.2f, 2.3f, 3f); // 3미터 초과 낙하 검사
            Assert.IsFalse(result); // 과도한 낙하 차단 확인
        } // 낙하 높이 초과 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void WorldBoundsIncludesQuarterTurnRotation() // 90도 회전 영역 크기 확인
        { // 회전 영역 테스트 범위
            GameObject moduleObject = new GameObject("RotatedMapModule"); // 테스트용 모듈 오브젝트 생성

            try // 테스트 오브젝트 정리 보장
            { // 회전 영역 검사 범위
                MapModuleDefinition definition = moduleObject.AddComponent<MapModuleDefinition>(); // 모듈 정의 컴포넌트 추가
                moduleObject.transform.rotation = Quaternion.Euler(0f, 90f, 0f); // Y축 90도 회전 적용
                Bounds result = definition.WorldBounds; // 회전된 월드 영역 조회
                Assert.That(result.size.x, Is.EqualTo(8f).Within(0.001f)); // 회전 후 X축 크기 확인
                Assert.That(result.size.y, Is.EqualTo(2f).Within(0.001f)); // 회전 후 Y축 크기 확인
                Assert.That(result.size.z, Is.EqualTo(4f).Within(0.001f)); // 회전 후 Z축 크기 확인
            } // 회전 영역 검사 종료
            finally // 테스트 종료 정리
            { // 테스트 오브젝트 정리 범위
                UnityEngine.Object.DestroyImmediate(moduleObject); // 테스트용 오브젝트 제거
            } // 테스트 오브젝트 정리 종료
        } // 회전 영역 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void EastConnectionGizmoFacesEast() // 동쪽 연결 기즈모 방향 확인
        { // 동쪽 기즈모 테스트 범위
            Quaternion rotation = MapModuleValidationRules.CalculateConnectionGizmoRotation(Vector3.right); // 동쪽 연결 회전 계산
            Vector3 result = rotation * Vector3.forward; // 기즈모 앞 방향 계산
            Assert.That(Vector3.Dot(result, Vector3.right), Is.EqualTo(1f).Within(0.001f)); // 동쪽 방향 일치 확인
        } // 동쪽 기즈모 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void WestConnectionGizmoFacesWest() // 서쪽 연결 기즈모 방향 확인
        { // 서쪽 기즈모 테스트 범위
            Quaternion rotation = MapModuleValidationRules.CalculateConnectionGizmoRotation(Vector3.left); // 서쪽 연결 회전 계산
            Vector3 result = rotation * Vector3.forward; // 기즈모 앞 방향 계산
            Assert.That(Vector3.Dot(result, Vector3.left), Is.EqualTo(1f).Within(0.001f)); // 서쪽 방향 일치 확인
        } // 서쪽 기즈모 테스트 종료
    } // 맵 모듈 규격 자동 테스트 묶음 종료
} // EditMode 테스트 묶음 종료

