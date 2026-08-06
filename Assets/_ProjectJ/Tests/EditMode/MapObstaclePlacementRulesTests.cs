using NUnit.Framework; // NUnit 테스트와 검증 기능 참조
using ProjectJ.Data; // 장애물 데이터와 버전 형식 참조
using ProjectJ.MapGeneration; // 장애물 배치 규칙과 지점 기능 참조
using UnityEngine; // Unity 테스트 오브젝트 기능 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class MapObstaclePlacementRulesTests // 장애물 배치와 위험도 규칙 자동 테스트 선언
    { // 장애물 배치 규칙 테스트 묶음
        private GameObject temporaryPointObject; // 정리할 임시 배치 지점 오브젝트
        private GameObject temporaryObstaclePrefab; // 정리할 임시 장애물 Prefab 대체 오브젝트
        private ObstacleDataDefinition temporaryObstacleData; // 정리할 임시 장애물 데이터

        [TearDown] // 각 테스트 종료 뒤 정리 실행
        public void TearDown() // 임시 Unity 오브젝트와 에셋 인스턴스 정리
        { // 테스트 정리 처리
            if (temporaryPointObject != null) // 임시 배치 지점 오브젝트 존재 확인
            { // 임시 배치 지점 오브젝트 정리 처리
                Object.DestroyImmediate(temporaryPointObject); // 임시 배치 지점 오브젝트 즉시 제거
            } // 임시 배치 지점 오브젝트 정리 처리 종료

            if (temporaryObstaclePrefab != null) // 임시 장애물 오브젝트 존재 확인
            { // 임시 장애물 오브젝트 정리 처리
                Object.DestroyImmediate(temporaryObstaclePrefab); // 임시 장애물 오브젝트 즉시 제거
            } // 임시 장애물 오브젝트 정리 처리 종료

            if (temporaryObstacleData != null) // 임시 장애물 데이터 존재 확인
            { // 임시 장애물 데이터 정리 처리
                Object.DestroyImmediate(temporaryObstacleData); // 임시 장애물 데이터 즉시 제거
            } // 임시 장애물 데이터 정리 처리 종료
        } // 테스트 정리 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void EvenSeedUsesLeftSafeLane() // 짝수 시드의 왼쪽 안전 경로 선택 확인
        { // 짝수 시드 안전 경로 테스트 처리
            Assert.AreEqual(-1, MapObstaclePlacementRules.ResolveSafeLane(38000)); // 짝수 시드 왼쪽 안전 경로 확인
        } // 짝수 시드 안전 경로 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void OddSeedUsesRightSafeLane() // 홀수 시드의 오른쪽 안전 경로 선택 확인
        { // 홀수 시드 안전 경로 테스트 처리
            Assert.AreEqual(1, MapObstaclePlacementRules.ResolveSafeLane(38001)); // 홀수 시드 오른쪽 안전 경로 확인
        } // 홀수 시드 안전 경로 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void OppositeLaneBecomesHighRisk() // 안전 경로 반대편의 고위험 지정 확인
        { // 분기 난이도 지정 테스트 처리
            Assert.AreEqual(MapBranchDifficulty.Safe, MapObstaclePlacementRules.ResolveDifficulty(-1, -1)); // 왼쪽 안전 경로 지정 확인
            Assert.AreEqual(MapBranchDifficulty.HighRisk, MapObstaclePlacementRules.ResolveDifficulty(1, -1)); // 오른쪽 고위험 경로 지정 확인
            Assert.AreEqual(MapBranchDifficulty.None, MapObstaclePlacementRules.ResolveDifficulty(0, -1)); // 공통 경로 난이도 미지정 확인
        } // 분기 난이도 지정 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void RiskBudgetAcceptsBothBoundaries() // 위험도 예산 최소와 최대 경계 포함 확인
        { // 위험도 예산 경계 테스트 처리
            Assert.IsTrue(MapObstaclePlacementRules.IsRiskWithinBudget(6, 6, 12)); // 최소 위험도 경계 허용 확인
            Assert.IsTrue(MapObstaclePlacementRules.IsRiskWithinBudget(12, 6, 12)); // 최대 위험도 경계 허용 확인
            Assert.IsFalse(MapObstaclePlacementRules.IsRiskWithinBudget(13, 6, 12)); // 최대 위험도 초과 거부 확인
        } // 위험도 예산 경계 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void RiskGapRejectsSmallDifference() // 안전과 고위험 경로 차이 부족 거부 확인
        { // 위험도 차이 테스트 처리
            Assert.IsFalse(MapObstaclePlacementRules.HasRequiredRiskGap(12, 18, 8)); // 위험도 차이 6 거부 확인
            Assert.IsTrue(MapObstaclePlacementRules.HasRequiredRiskGap(12, 20, 8)); // 위험도 차이 8 허용 확인
        } // 위험도 차이 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void SpawnPointAcceptsSafeObstacleWidth() // 충분한 통로 폭의 장애물 배치 허용 확인
        { // 안전한 장애물 폭 테스트 처리
            MapObstacleSpawnPoint spawnPoint = CreateSpawnPoint(1.2f, 3f, 1.1f); // 안전 통로 폭 배치 지점 생성
            ObstacleDataDefinition obstacleData = CreateObstacleData(new Vector3(0.8f, 1f, 0.8f), true); // 폭 0.8미터 차단 장애물 생성
            Assert.IsTrue(spawnPoint.CanPlace(obstacleData, out string reason), reason); // 남은 통로 2.2미터 배치 허용 확인
            Assert.AreEqual(2.2f, spawnPoint.CalculateRemainingPassageWidth(obstacleData), 0.001f); // 배치 뒤 통로 폭 계산 확인
        } // 안전한 장애물 폭 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void SpawnPointRejectsBlockedPassageWidth() // 장애물 배치 뒤 최소 통로 폭 미달 거부 확인
        { // 막힌 통로 폭 테스트 처리
            MapObstacleSpawnPoint spawnPoint = CreateSpawnPoint(2.5f, 2f, 1.1f); // 좁은 통로 배치 지점 생성
            ObstacleDataDefinition obstacleData = CreateObstacleData(new Vector3(1.2f, 1f, 1.2f), true); // 통로 폭을 크게 차감하는 장애물 생성
            Assert.IsFalse(spawnPoint.CanPlace(obstacleData, out string reason)); // 남은 통로 0.8미터 배치 거부 확인
            StringAssert.Contains("최소", reason); // 최소 통로 폭 실패 이유 포함 확인
        } // 막힌 통로 폭 테스트 처리 종료

        private MapObstacleSpawnPoint CreateSpawnPoint(float maximumObstacleWidth, float passageWidth, float minimumRemainingWidth) // 테스트용 장애물 배치 지점 생성
        { // 테스트용 배치 지점 생성 처리
            temporaryPointObject = new GameObject("TestObstaclePoint"); // 빈 테스트 배치 지점 오브젝트 생성
            MapObstacleSpawnPoint spawnPoint = temporaryPointObject.AddComponent<MapObstacleSpawnPoint>(); // 장애물 배치 지점 컴포넌트 추가
            spawnPoint.ConfigureForEditor("TestPoint", maximumObstacleWidth, passageWidth, minimumRemainingWidth, 1f, true); // 테스트 통로 폭 설정 적용
            return spawnPoint; // 구성된 테스트 배치 지점 반환
        } // 테스트용 배치 지점 생성 처리 종료

        private ObstacleDataDefinition CreateObstacleData(Vector3 footprintSize, bool blocksPassage) // 테스트용 장애물 데이터 생성
        { // 테스트용 장애물 데이터 생성 처리
            temporaryObstaclePrefab = new GameObject("TestObstaclePrefab"); // 빈 테스트 장애물 Prefab 대체 오브젝트 생성
            temporaryObstacleData = ScriptableObject.CreateInstance<ObstacleDataDefinition>(); // 임시 장애물 데이터 인스턴스 생성
            temporaryObstacleData.SetEditorIdentity("OBS-TEST", "Test Obstacle", new ProjectDataVersion(1, 0, 0)); // 테스트 장애물 식별 정보 적용
            temporaryObstacleData.ConfigureObstacleForEditor(temporaryObstaclePrefab, 6, footprintSize, blocksPassage, ObstacleTraversalEffect.Slow); // 테스트 장애물 생성 데이터 적용
            return temporaryObstacleData; // 구성된 테스트 장애물 데이터 반환
        } // 테스트용 장애물 데이터 생성 처리 종료
    } // 장애물 배치 규칙 테스트 묶음 종료
} // EditMode 테스트 묶음 종료
