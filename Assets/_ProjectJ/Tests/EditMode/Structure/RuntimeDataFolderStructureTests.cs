using System; // 문자열 비교 기능 참조
using System.Collections.Generic; // 중복 경로 검사 집합 기능 참조
using NUnit.Framework; // Unity EditMode 테스트 기능 참조
using UnityEditor; // Unity AssetDatabase 경로 검사 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스
{ // 47일차 Runtime·Data 기능별 폴더 구조 회귀 테스트 정의
    public sealed class RuntimeDataFolderStructureTests // Runtime·Data 스크립트 이동 완료 상태 테스트 선언
    { // 새 기능별 경로와 Runtime asmdef 경계 검증
        private const string RuntimeRootPath = "Assets/_ProjectJ/Scripts/Runtime"; // Runtime 스크립트 루트 경로
        private const string RuntimeAsmdefPath = RuntimeRootPath + "/ProjectJ.Runtime.asmdef"; // Runtime Assembly Definition 고정 경로

        private static readonly PathPair[] ExpectedMoves = // 47일차 전체 이동 대상 경로 목록
        { // 총 53개 기존·새 경로 쌍 시작
            new PathPair(RuntimeRootPath + "/Data/DataValidationService.cs", RuntimeRootPath + "/Data/Validation/DataValidationService.cs"), // Data 검증 서비스 경로
            new PathPair(RuntimeRootPath + "/Data/Definitions/ProjectDataCatalog.cs", RuntimeRootPath + "/Data/Catalog/ProjectDataCatalog.cs"), // Data 카탈로그 경로
            new PathPair(RuntimeRootPath + "/Data/Definitions/AudioDataDefinition.cs", RuntimeRootPath + "/Data/Definitions/Audio/AudioDataDefinition.cs"), // Audio 데이터 정의 경로
            new PathPair(RuntimeRootPath + "/Data/Definitions/CosmeticDataDefinition.cs", RuntimeRootPath + "/Data/Definitions/Cosmetic/CosmeticDataDefinition.cs"), // Cosmetic 데이터 정의 경로
            new PathPair(RuntimeRootPath + "/Data/Definitions/ItemDataDefinition.cs", RuntimeRootPath + "/Data/Definitions/Item/ItemDataDefinition.cs"), // Item 데이터 정의 경로
            new PathPair(RuntimeRootPath + "/Data/Definitions/MapDataDefinition.cs", RuntimeRootPath + "/Data/Definitions/Map/MapDataDefinition.cs"), // Map 데이터 정의 경로
            new PathPair(RuntimeRootPath + "/Data/Definitions/ObstacleDataDefinition.cs", RuntimeRootPath + "/Data/Definitions/Obstacle/ObstacleDataDefinition.cs"), // Obstacle 데이터 정의 경로
            new PathPair(RuntimeRootPath + "/Data/Definitions/PlayerDataDefinition.cs", RuntimeRootPath + "/Data/Definitions/Player/PlayerDataDefinition.cs"), // Player 데이터 정의 경로
            new PathPair(RuntimeRootPath + "/Data/Definitions/ProjectDataAsset.cs", RuntimeRootPath + "/Data/Definitions/Common/ProjectDataAsset.cs"), // 공통 Data 기반 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapGenerationGraph.cs", RuntimeRootPath + "/Map/Generation/MapGenerationGraph.cs"), // Map 생성 그래프 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapGenerationRules.cs", RuntimeRootPath + "/Map/Generation/MapGenerationRules.cs"), // Map 생성 규칙 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapGenerationSettings.cs", RuntimeRootPath + "/Map/Generation/MapGenerationSettings.cs"), // Map 생성 설정 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapVerticalBranchGenerationRules.cs", RuntimeRootPath + "/Map/Generation/MapVerticalBranchGenerationRules.cs"), // Map 수직 분기 생성 규칙 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapVerticalGenerationRules.cs", RuntimeRootPath + "/Map/Generation/MapVerticalGenerationRules.cs"), // Map 수직 생성 규칙 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/ProceduralMapGenerator.cs", RuntimeRootPath + "/Map/Generation/ProceduralMapGenerator.cs"), // 절차적 Map 생성기 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapModuleConnectionPoint.cs", RuntimeRootPath + "/Map/Modules/MapModuleConnectionPoint.cs"), // Map 모듈 연결 지점 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapModuleDefinition.cs", RuntimeRootPath + "/Map/Modules/MapModuleDefinition.cs"), // Map 모듈 정의 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapModuleTypes.cs", RuntimeRootPath + "/Map/Modules/MapModuleTypes.cs"), // Map 모듈 형식 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapVerticalModuleData.cs", RuntimeRootPath + "/Map/Modules/MapVerticalModuleData.cs"), // Map 수직 모듈 데이터 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapGenerationValidation.cs", RuntimeRootPath + "/Map/Validation/MapGenerationValidation.cs"), // Map 생성 검증 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapModuleValidationRules.cs", RuntimeRootPath + "/Map/Validation/MapModuleValidationRules.cs"), // Map 모듈 검증 규칙 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapPlayableRouteValidation.cs", RuntimeRootPath + "/Map/Validation/MapPlayableRouteValidation.cs"), // Map 플레이 가능 경로 검증 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapVerticalModuleValidationRules.cs", RuntimeRootPath + "/Map/Validation/MapVerticalModuleValidationRules.cs"), // Map 수직 모듈 검증 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapBranchObstaclePlanner.cs", RuntimeRootPath + "/Map/Obstacles/MapBranchObstaclePlanner.cs"), // Map 분기 장애물 계획 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapObstaclePlanning.cs", RuntimeRootPath + "/Map/Obstacles/MapObstaclePlanning.cs"), // Map 장애물 계획 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapObstacleSpawnPoint.cs", RuntimeRootPath + "/Map/Obstacles/MapObstacleSpawnPoint.cs"), // Map 장애물 생성 지점 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapPlacedObstacle.cs", RuntimeRootPath + "/Map/Obstacles/MapPlacedObstacle.cs"), // Map 배치 장애물 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapGenerationDebugVisualizer.cs", RuntimeRootPath + "/Map/Debug/MapGenerationDebugVisualizer.cs"), // Map 생성 디버그 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapObstacleDebugVisualizer.cs", RuntimeRootPath + "/Map/Debug/MapObstacleDebugVisualizer.cs"), // Map 장애물 디버그 경로
            new PathPair(RuntimeRootPath + "/MapGeneration/MapTraversalProfile.cs", RuntimeRootPath + "/Map/Traversal/MapTraversalProfile.cs"), // Map 이동 프로필 경로
            new PathPair(RuntimeRootPath + "/Items/PlayerItemInventory.cs", RuntimeRootPath + "/Items/Inventory/PlayerItemInventory.cs"), // Item 인벤토리 경로
            new PathPair(RuntimeRootPath + "/Items/ItemChestPickup.cs", RuntimeRootPath + "/Items/Chests/ItemChestPickup.cs"), // Item 상자 획득 경로
            new PathPair(RuntimeRootPath + "/Items/ItemChestSpawnPoint.cs", RuntimeRootPath + "/Items/Chests/ItemChestSpawnPoint.cs"), // Item 상자 생성 지점 경로
            new PathPair(RuntimeRootPath + "/Items/ItemChestSpawnRules.cs", RuntimeRootPath + "/Items/Chests/ItemChestSpawnRules.cs"), // Item 상자 생성 규칙 경로
            new PathPair(RuntimeRootPath + "/Items/ItemChestSpawner.cs", RuntimeRootPath + "/Items/Chests/ItemChestSpawner.cs"), // Item 상자 생성기 경로
            new PathPair(RuntimeRootPath + "/Items/ItemPlacementResult.cs", RuntimeRootPath + "/Items/Placement/ItemPlacementResult.cs"), // Item 설치 결과 경로
            new PathPair(RuntimeRootPath + "/Items/ItemPlacementRules.cs", RuntimeRootPath + "/Items/Placement/ItemPlacementRules.cs"), // Item 설치 규칙 경로
            new PathPair(RuntimeRootPath + "/Items/ItemPlacementValidator.cs", RuntimeRootPath + "/Items/Placement/ItemPlacementValidator.cs"), // Item 설치 검사 경로
            new PathPair(RuntimeRootPath + "/Items/ItemSelectionRules.cs", RuntimeRootPath + "/Items/Rules/ItemSelectionRules.cs"), // Item 선택 규칙 경로
            new PathPair(RuntimeRootPath + "/Items/P1ItemRules.cs", RuntimeRootPath + "/Items/Rules/P1ItemRules.cs"), // Item P1 규칙 경로
            new PathPair(RuntimeRootPath + "/Items/P2ItemRules.cs", RuntimeRootPath + "/Items/Rules/P2ItemRules.cs"), // Item P2 규칙 경로
            new PathPair(RuntimeRootPath + "/Items/PlayerItemUseController.cs", RuntimeRootPath + "/Items/Use/PlayerItemUseController.cs"), // Item 사용 제어 경로
            new PathPair(RuntimeRootPath + "/Items/HomingItemEffect.cs", RuntimeRootPath + "/Items/Effects/Common/HomingItemEffect.cs"), // Item 유도 효과 경로
            new PathPair(RuntimeRootPath + "/Items/ItemProjectileEffect.cs", RuntimeRootPath + "/Items/Effects/Common/ItemProjectileEffect.cs"), // Item 발사체 효과 경로
            new PathPair(RuntimeRootPath + "/Items/PlacedItemEffect.cs", RuntimeRootPath + "/Items/Effects/Common/PlacedItemEffect.cs"), // Item 설치 효과 경로
            new PathPair(RuntimeRootPath + "/Items/SmokeCloudEffect.cs", RuntimeRootPath + "/Items/Effects/Common/SmokeCloudEffect.cs"), // Item 연막 효과 경로
            new PathPair(RuntimeRootPath + "/Items/ThrownItemEffect.cs", RuntimeRootPath + "/Items/Effects/Common/ThrownItemEffect.cs"), // Item 투척 효과 경로
            new PathPair(RuntimeRootPath + "/Items/PlayerItemEffectController.cs", RuntimeRootPath + "/Items/Effects/Player/PlayerItemEffectController.cs"), // Item 플레이어 효과 제어 경로
            new PathPair(RuntimeRootPath + "/Items/PlayerP2ItemEffectController.cs", RuntimeRootPath + "/Items/Effects/Player/PlayerP2ItemEffectController.cs"), // Item 플레이어 P2 효과 제어 경로
            new PathPair(RuntimeRootPath + "/Items/PlayerScreenObscureView.cs", RuntimeRootPath + "/Items/Effects/Player/PlayerScreenObscureView.cs"), // Item 화면 가림 효과 경로
            new PathPair(RuntimeRootPath + "/Items/PlayerSniperWaterGunController.cs", RuntimeRootPath + "/Items/Effects/Player/PlayerSniperWaterGunController.cs"), // Item 저격 물총 효과 경로
            new PathPair(RuntimeRootPath + "/Items/PlayerRewindRecorder.cs", RuntimeRootPath + "/Items/Effects/Rewind/PlayerRewindRecorder.cs"), // Item 되감기 기록 경로
            new PathPair(RuntimeRootPath + "/Items/CartPath.cs", RuntimeRootPath + "/Items/Effects/Cart/CartPath.cs"), // Item 카트 경로
        }; // 총 53개 기존·새 경로 쌍 종료

        [Test] // Unity Test Runner 테스트 지정
        public void AllMovedScriptsExistOnlyAtFunctionalDestinations() // 53개 스크립트가 새 기능별 경로에만 존재하는지 검증
        { // 기존 경로 잔존과 대상 경로 누락 검사
            HashSet<string> destinationPaths = new HashSet<string>(StringComparer.Ordinal); // 새 대상 경로 중복 검사 집합 생성

            for (int index = 0; index < ExpectedMoves.Length; index++) // 53개 이동 계획 전체 순회
            { // 현재 이동 대상의 기존·새 경로 검증
                PathPair pair = ExpectedMoves[index]; // 현재 기존·새 경로 쌍 조회
                string sourceGuid = AssetDatabase.AssetPathToGUID(pair.SourcePath); // 기존 경로 asset GUID 조회
                string destinationGuid = AssetDatabase.AssetPathToGUID(pair.DestinationPath); // 새 경로 asset GUID 조회
                Assert.IsTrue(string.IsNullOrEmpty(sourceGuid), $"기존 경로에 스크립트가 남아 있습니다: {pair.SourcePath}"); // 기존 경로 제거 확인
                Assert.IsFalse(string.IsNullOrEmpty(destinationGuid), $"새 기능별 경로에 스크립트가 없습니다: {pair.DestinationPath}"); // 새 경로 존재 확인
                Assert.IsTrue(destinationPaths.Add(pair.DestinationPath), $"중복 대상 경로가 있습니다: {pair.DestinationPath}"); // 새 대상 경로 고유성 확인
            } // 53개 이동 계획 전체 검증 종료

            Assert.AreEqual(53, ExpectedMoves.Length); // 47일차 전체 이동 계획 개수 고정 확인
            Assert.AreEqual(53, destinationPaths.Count); // 47일차 새 대상 경로 고유 개수 확인
        } // 새 기능별 경로 전체 검증 종료

        [Test] // Unity Test Runner 테스트 지정
        public void RuntimeAssemblyDefinitionRemainsSingleAtRuntimeRoot() // 폴더 이동 뒤 Runtime 어셈블리 경계 유지 여부 검증
        { // ProjectJ.Runtime asmdef 위치와 중첩 asmdef 부재 검사
            string rootAsmdefGuid = AssetDatabase.AssetPathToGUID(RuntimeAsmdefPath); // Runtime 루트 asmdef GUID 조회
            Assert.IsFalse(string.IsNullOrEmpty(rootAsmdefGuid), $"Runtime asmdef가 루트에서 누락됐습니다: {RuntimeAsmdefPath}"); // Runtime 루트 asmdef 존재 확인
            string[] asmdefGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { RuntimeRootPath }); // Runtime 하위 Assembly Definition 전체 검색
            int asmdefCount = 0; // 실제 asmdef 파일 개수 초기화

            for (int index = 0; index < asmdefGuids.Length; index++) // 검색된 Assembly Definition 전체 순회
            { // 현재 검색 결과의 실제 asset 경로 확인
                string assetPath = AssetDatabase.GUIDToAssetPath(asmdefGuids[index]); // 현재 Assembly Definition 경로 조회

                if (assetPath.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase)) // 실제 asmdef 파일 여부 확인
                { // Runtime asmdef 개수 집계
                    asmdefCount++; // 실제 asmdef 파일 개수 증가
                } // Runtime asmdef 개수 집계 종료
            } // 검색된 Assembly Definition 전체 순회 종료

            Assert.AreEqual(1, asmdefCount, $"Runtime 내부 asmdef 개수가 달라졌습니다. 실제: {asmdefCount}"); // 단일 Runtime 어셈블리 경계 확인
        } // Runtime 어셈블리 경계 유지 검증 종료

        private readonly struct PathPair // 기존·새 asset 경로 쌍 값 선언
        { // 폴더 구조 회귀 테스트용 경로 저장
            public PathPair(string sourcePath, string destinationPath) // 기존·새 경로 쌍 생성
            { // 전달된 asset 경로 저장
                SourcePath = sourcePath; // 기존 스크립트 경로 저장
                DestinationPath = destinationPath; // 새 기능별 스크립트 경로 저장
            } // 기존·새 경로 쌍 생성 종료

            public string SourcePath { get; } // 기존 스크립트 경로 반환
            public string DestinationPath { get; } // 새 기능별 스크립트 경로 반환
        } // 기존·새 asset 경로 쌍 값 정의 종료
    } // Runtime·Data 스크립트 이동 완료 상태 테스트 정의 종료
} // 프로젝트 EditMode 테스트 네임스페이스 종료
