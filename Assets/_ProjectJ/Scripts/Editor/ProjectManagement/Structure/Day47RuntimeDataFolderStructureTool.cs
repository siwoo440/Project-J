using System; // 기본 예외와 문자열 비교 기능 참조
using System.Collections.Generic; // 이동 완료 목록과 검증 오류 목록 기능 참조
using System.IO; // 기존 폴더의 남은 파일 검사 기능 참조
using UnityEditor; // Unity AssetDatabase 이동과 메뉴 기능 참조
using UnityEngine; // Unity Console과 대화상자 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스
{ // 47일차 Runtime·Data 폴더 통합 도구 정의
    internal static class Day47RuntimeDataFolderStructureTool // 기능별 Runtime·Data 폴더 이동과 검증 도구 선언
    { // Data·Map·Item 세 단계 안전 이동 기능 정의
        private const string RuntimeRootPath = "Assets/_ProjectJ/Scripts/Runtime"; // Runtime 스크립트 루트 경로
        private const string RuntimeAsmdefPath = RuntimeRootPath + "/ProjectJ.Runtime.asmdef"; // Runtime Assembly Definition 경로
        private const string DataMenuPath = ProjectJEditorMenuPaths.DataBase + "/47일차 Runtime·Data 폴더 정리/01. Data 스크립트 폴더 통합 적용 (Day 47일차)"; // Data 단계 실행 메뉴 경로
        private const string MapMenuPath = ProjectJEditorMenuPaths.DataBase + "/47일차 Runtime·Data 폴더 정리/02. Map 스크립트 폴더 통합 적용 (Day 47일차)"; // Map 단계 실행 메뉴 경로
        private const string ItemMenuPath = ProjectJEditorMenuPaths.DataBase + "/47일차 Runtime·Data 폴더 정리/03. Item 스크립트 폴더 통합 적용 (Day 47일차)"; // Item 단계 실행 메뉴 경로
        private const string ValidateMenuPath = ProjectJEditorMenuPaths.DataBase + "/47일차 Runtime·Data 폴더 정리/04. 전체 Runtime·Data 폴더 구조 검증 (Day 47일차)"; // 전체 구조 검증 메뉴 경로
        private const string LegacyMapGenerationFolderPath = RuntimeRootPath + "/MapGeneration"; // 제거 대상 기존 MapGeneration 폴더 경로

        private static readonly MovePlan[] DataMovePlans = // Data 기능별 이동 계획 목록
        { // Data 스크립트 9개 이동 계획 시작
            new MovePlan(RuntimeRootPath + "/Data/DataValidationService.cs", RuntimeRootPath + "/Data/Validation/DataValidationService.cs"), // 데이터 검증 서비스 이동
            new MovePlan(RuntimeRootPath + "/Data/Definitions/ProjectDataCatalog.cs", RuntimeRootPath + "/Data/Catalog/ProjectDataCatalog.cs"), // 런타임 데이터 카탈로그 이동
            new MovePlan(RuntimeRootPath + "/Data/Definitions/AudioDataDefinition.cs", RuntimeRootPath + "/Data/Definitions/Audio/AudioDataDefinition.cs"), // 오디오 데이터 정의 이동
            new MovePlan(RuntimeRootPath + "/Data/Definitions/CosmeticDataDefinition.cs", RuntimeRootPath + "/Data/Definitions/Cosmetic/CosmeticDataDefinition.cs"), // 꾸미기 데이터 정의 이동
            new MovePlan(RuntimeRootPath + "/Data/Definitions/ItemDataDefinition.cs", RuntimeRootPath + "/Data/Definitions/Item/ItemDataDefinition.cs"), // 아이템 데이터 정의 이동
            new MovePlan(RuntimeRootPath + "/Data/Definitions/MapDataDefinition.cs", RuntimeRootPath + "/Data/Definitions/Map/MapDataDefinition.cs"), // 맵 데이터 정의 이동
            new MovePlan(RuntimeRootPath + "/Data/Definitions/ObstacleDataDefinition.cs", RuntimeRootPath + "/Data/Definitions/Obstacle/ObstacleDataDefinition.cs"), // 장애물 데이터 정의 이동
            new MovePlan(RuntimeRootPath + "/Data/Definitions/PlayerDataDefinition.cs", RuntimeRootPath + "/Data/Definitions/Player/PlayerDataDefinition.cs"), // 플레이어 데이터 정의 이동
            new MovePlan(RuntimeRootPath + "/Data/Definitions/ProjectDataAsset.cs", RuntimeRootPath + "/Data/Definitions/Common/ProjectDataAsset.cs"), // 공통 프로젝트 데이터 기반 형식 이동
        }; // Data 스크립트 9개 이동 계획 종료

        private static readonly MovePlan[] MapMovePlans = // Map 기능별 이동 계획 목록
        { // Map 스크립트 21개 이동 계획 시작
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapGenerationGraph.cs", RuntimeRootPath + "/Map/Generation/MapGenerationGraph.cs"), // 맵 생성 그래프 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapGenerationRules.cs", RuntimeRootPath + "/Map/Generation/MapGenerationRules.cs"), // 맵 생성 규칙 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapGenerationSettings.cs", RuntimeRootPath + "/Map/Generation/MapGenerationSettings.cs"), // 맵 생성 설정 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapVerticalBranchGenerationRules.cs", RuntimeRootPath + "/Map/Generation/MapVerticalBranchGenerationRules.cs"), // 수직 분기 생성 규칙 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapVerticalGenerationRules.cs", RuntimeRootPath + "/Map/Generation/MapVerticalGenerationRules.cs"), // 수직 생성 규칙 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/ProceduralMapGenerator.cs", RuntimeRootPath + "/Map/Generation/ProceduralMapGenerator.cs"), // 절차적 맵 생성기 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapModuleConnectionPoint.cs", RuntimeRootPath + "/Map/Modules/MapModuleConnectionPoint.cs"), // 맵 모듈 연결 지점 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapModuleDefinition.cs", RuntimeRootPath + "/Map/Modules/MapModuleDefinition.cs"), // 맵 모듈 정의 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapModuleTypes.cs", RuntimeRootPath + "/Map/Modules/MapModuleTypes.cs"), // 맵 모듈 형식 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapVerticalModuleData.cs", RuntimeRootPath + "/Map/Modules/MapVerticalModuleData.cs"), // 수직 맵 모듈 데이터 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapGenerationValidation.cs", RuntimeRootPath + "/Map/Validation/MapGenerationValidation.cs"), // 생성 결과 검증 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapModuleValidationRules.cs", RuntimeRootPath + "/Map/Validation/MapModuleValidationRules.cs"), // 맵 모듈 검증 규칙 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapPlayableRouteValidation.cs", RuntimeRootPath + "/Map/Validation/MapPlayableRouteValidation.cs"), // 플레이 가능 경로 검증 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapVerticalModuleValidationRules.cs", RuntimeRootPath + "/Map/Validation/MapVerticalModuleValidationRules.cs"), // 수직 모듈 검증 규칙 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapBranchObstaclePlanner.cs", RuntimeRootPath + "/Map/Obstacles/MapBranchObstaclePlanner.cs"), // 분기 장애물 계획기 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapObstaclePlanning.cs", RuntimeRootPath + "/Map/Obstacles/MapObstaclePlanning.cs"), // 장애물 계획 데이터 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapObstacleSpawnPoint.cs", RuntimeRootPath + "/Map/Obstacles/MapObstacleSpawnPoint.cs"), // 장애물 생성 지점 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapPlacedObstacle.cs", RuntimeRootPath + "/Map/Obstacles/MapPlacedObstacle.cs"), // 배치 장애물 상태 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapGenerationDebugVisualizer.cs", RuntimeRootPath + "/Map/Debug/MapGenerationDebugVisualizer.cs"), // 맵 생성 디버그 시각화 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapObstacleDebugVisualizer.cs", RuntimeRootPath + "/Map/Debug/MapObstacleDebugVisualizer.cs"), // 장애물 디버그 시각화 이동
            new MovePlan(RuntimeRootPath + "/MapGeneration/MapTraversalProfile.cs", RuntimeRootPath + "/Map/Traversal/MapTraversalProfile.cs"), // 맵 이동 기준 프로필 이동
        }; // Map 스크립트 21개 이동 계획 종료

        private static readonly MovePlan[] ItemMovePlans = // Item 기능별 이동 계획 목록
        { // Item 스크립트 23개 이동 계획 시작
            new MovePlan(RuntimeRootPath + "/Items/PlayerItemInventory.cs", RuntimeRootPath + "/Items/Inventory/PlayerItemInventory.cs"), // 2슬롯 아이템 인벤토리 이동
            new MovePlan(RuntimeRootPath + "/Items/ItemChestPickup.cs", RuntimeRootPath + "/Items/Chests/ItemChestPickup.cs"), // 아이템 상자 획득 기능 이동
            new MovePlan(RuntimeRootPath + "/Items/ItemChestSpawnPoint.cs", RuntimeRootPath + "/Items/Chests/ItemChestSpawnPoint.cs"), // 아이템 상자 생성 지점 이동
            new MovePlan(RuntimeRootPath + "/Items/ItemChestSpawnRules.cs", RuntimeRootPath + "/Items/Chests/ItemChestSpawnRules.cs"), // 아이템 상자 생성 규칙 이동
            new MovePlan(RuntimeRootPath + "/Items/ItemChestSpawner.cs", RuntimeRootPath + "/Items/Chests/ItemChestSpawner.cs"), // 아이템 상자 생성기 이동
            new MovePlan(RuntimeRootPath + "/Items/ItemPlacementResult.cs", RuntimeRootPath + "/Items/Placement/ItemPlacementResult.cs"), // 설치 위치 검사 결과 이동
            new MovePlan(RuntimeRootPath + "/Items/ItemPlacementRules.cs", RuntimeRootPath + "/Items/Placement/ItemPlacementRules.cs"), // 설치 위치 규칙 이동
            new MovePlan(RuntimeRootPath + "/Items/ItemPlacementValidator.cs", RuntimeRootPath + "/Items/Placement/ItemPlacementValidator.cs"), // 설치 위치 검사기 이동
            new MovePlan(RuntimeRootPath + "/Items/ItemSelectionRules.cs", RuntimeRootPath + "/Items/Rules/ItemSelectionRules.cs"), // 아이템 선택 규칙 이동
            new MovePlan(RuntimeRootPath + "/Items/P1ItemRules.cs", RuntimeRootPath + "/Items/Rules/P1ItemRules.cs"), // P1 아이템 규칙 이동
            new MovePlan(RuntimeRootPath + "/Items/P2ItemRules.cs", RuntimeRootPath + "/Items/Rules/P2ItemRules.cs"), // P2 아이템 규칙 이동
            new MovePlan(RuntimeRootPath + "/Items/PlayerItemUseController.cs", RuntimeRootPath + "/Items/Use/PlayerItemUseController.cs"), // 플레이어 아이템 사용 제어 이동
            new MovePlan(RuntimeRootPath + "/Items/HomingItemEffect.cs", RuntimeRootPath + "/Items/Effects/Common/HomingItemEffect.cs"), // 유도형 아이템 효과 이동
            new MovePlan(RuntimeRootPath + "/Items/ItemProjectileEffect.cs", RuntimeRootPath + "/Items/Effects/Common/ItemProjectileEffect.cs"), // 발사체 아이템 효과 이동
            new MovePlan(RuntimeRootPath + "/Items/PlacedItemEffect.cs", RuntimeRootPath + "/Items/Effects/Common/PlacedItemEffect.cs"), // 설치형 아이템 효과 이동
            new MovePlan(RuntimeRootPath + "/Items/SmokeCloudEffect.cs", RuntimeRootPath + "/Items/Effects/Common/SmokeCloudEffect.cs"), // 연막 효과 이동
            new MovePlan(RuntimeRootPath + "/Items/ThrownItemEffect.cs", RuntimeRootPath + "/Items/Effects/Common/ThrownItemEffect.cs"), // 투척 아이템 효과 이동
            new MovePlan(RuntimeRootPath + "/Items/PlayerItemEffectController.cs", RuntimeRootPath + "/Items/Effects/Player/PlayerItemEffectController.cs"), // 플레이어 P0·P1 효과 제어 이동
            new MovePlan(RuntimeRootPath + "/Items/PlayerP2ItemEffectController.cs", RuntimeRootPath + "/Items/Effects/Player/PlayerP2ItemEffectController.cs"), // 플레이어 P2 효과 제어 이동
            new MovePlan(RuntimeRootPath + "/Items/PlayerScreenObscureView.cs", RuntimeRootPath + "/Items/Effects/Player/PlayerScreenObscureView.cs"), // 화면 가림 아이템 효과 UI 이동
            new MovePlan(RuntimeRootPath + "/Items/PlayerSniperWaterGunController.cs", RuntimeRootPath + "/Items/Effects/Player/PlayerSniperWaterGunController.cs"), // 저격 물총 효과 제어 이동
            new MovePlan(RuntimeRootPath + "/Items/PlayerRewindRecorder.cs", RuntimeRootPath + "/Items/Effects/Rewind/PlayerRewindRecorder.cs"), // 되감기 이동 기록 기능 이동
            new MovePlan(RuntimeRootPath + "/Items/CartPath.cs", RuntimeRootPath + "/Items/Effects/Cart/CartPath.cs"), // 카트 이동 경로 기능 이동
        }; // Item 스크립트 23개 이동 계획 종료

        [MenuItem(DataMenuPath)] // 47일차 Data 단계 메뉴 등록
        private static void ApplyDataFolderIntegration() // Data 스크립트 9개 기능별 폴더 이동
        { // Data 폴더 통합 실행 처리
            ApplyMovePlans("Data", DataMovePlans, false); // Data 단계 안전 이동 실행
        } // Data 폴더 통합 실행 처리 종료

        [MenuItem(MapMenuPath)] // 47일차 Map 단계 메뉴 등록
        private static void ApplyMapFolderIntegration() // Map 스크립트 21개 기능별 폴더 이동
        { // Map 폴더 통합 실행 처리
            ApplyMovePlans("Map", MapMovePlans, true); // Map 단계 안전 이동과 기존 폴더 정리 실행
        } // Map 폴더 통합 실행 처리 종료

        [MenuItem(ItemMenuPath)] // 47일차 Item 단계 메뉴 등록
        private static void ApplyItemFolderIntegration() // Item 스크립트 23개 기능별 폴더 이동
        { // Item 폴더 통합 실행 처리
            ApplyMovePlans("Item", ItemMovePlans, false); // Item 단계 안전 이동 실행
        } // Item 폴더 통합 실행 처리 종료

        [MenuItem(ValidateMenuPath)] // 47일차 전체 구조 검증 메뉴 등록
        private static void ValidateAllRuntimeDataFolders() // Data·Map·Item 전체 폴더 통합 결과 검증
        { // 전체 폴더 구조 검증 처리
            List<string> errors = new List<string>(); // 전체 구조 검증 오류 목록 생성
            ValidateCompletedPlans("Data", DataMovePlans, errors); // Data 단계 완료 상태 검증
            ValidateCompletedPlans("Map", MapMovePlans, errors); // Map 단계 완료 상태 검증
            ValidateCompletedPlans("Item", ItemMovePlans, errors); // Item 단계 완료 상태 검증
            ValidateRuntimeAssemblyDefinition(errors); // Runtime asmdef 위치와 개수 검증

            if (errors.Count > 0) // 전체 구조 검증 오류 존재 여부 확인
            { // 전체 구조 검증 실패 처리
                LogErrors(errors); // 검증 오류 전체 Console 출력
                EditorUtility.DisplayDialog("Project J Day 47", $"Runtime·Data 폴더 구조 검증 실패\n\n오류: {errors.Count}개\nConsole을 확인합니다.", "확인"); // 전체 구조 검증 실패 안내
                return; // 실패 상태에서 완료 처리 중단
            } // 전체 구조 검증 실패 처리 종료

            Debug.Log($"[ProjectJ][Day47] Runtime·Data 폴더 구조 검증 완료 | Data {DataMovePlans.Length}개 | Map {MapMovePlans.Length}개 | Item {ItemMovePlans.Length}개 | 총 {DataMovePlans.Length + MapMovePlans.Length + ItemMovePlans.Length}개"); // 전체 구조 검증 성공 로그
            EditorUtility.DisplayDialog("Project J Day 47", "Runtime·Data 스크립트 기능별 폴더 통합 검증 완료\n\nData 9개\nMap 21개\nItem 23개\n총 53개", "확인"); // 전체 구조 검증 성공 안내
        } // 전체 폴더 구조 검증 처리 종료

        private static void ApplyMovePlans(string categoryName, IReadOnlyList<MovePlan> plans, bool cleanupLegacyMapFolder) // 지정 기능의 스크립트 이동 계획 안전 적용
        { // 사전 검증·일괄 이동·부분 완료 복구 처리
            List<string> validationErrors = ValidateBeforeMove(categoryName, plans, out int pendingCount); // 현재 소스와 대상 경로 전체 사전 검증

            if (validationErrors.Count > 0) // 사전 검증 오류 존재 여부 확인
            { // 일부 이동 방지용 안전 중단 처리
                LogErrors(validationErrors); // 사전 검증 오류 전체 Console 출력
                EditorUtility.DisplayDialog("Project J Day 47", $"{categoryName} 폴더 통합을 시작하지 않았습니다.\n\n오류: {validationErrors.Count}개\nConsole을 확인합니다.", "확인"); // 사전 검증 실패 안내
                return; // 어떤 파일도 이동하지 않고 종료
            } // 사전 검증 실패 처리 종료

            if (pendingCount == 0) // 현재 기능의 모든 파일이 이미 새 위치인지 확인
            { // 재실행 또는 이전 완료 상태 처리
                List<string> completedErrors = new List<string>(); // 완료 상태 재검증 오류 목록 생성
                ValidateCompletedPlans(categoryName, plans, completedErrors); // 현재 기능의 최종 위치 재검증

                if (completedErrors.Count > 0) // 이미 이동된 상태의 검증 오류 여부 확인
                { // 완료 상태 불일치 처리
                    LogErrors(completedErrors); // 완료 상태 검증 오류 출력
                    EditorUtility.DisplayDialog("Project J Day 47", $"{categoryName} 폴더 구조에 검증 오류가 있습니다.\nConsole을 확인합니다.", "확인"); // 완료 상태 오류 안내
                    return; // 완료 상태 불일치로 종료
                } // 완료 상태 불일치 처리 종료

                EditorUtility.DisplayDialog("Project J Day 47", $"{categoryName} 스크립트는 이미 기능별 폴더 통합이 완료된 상태입니다.", "확인"); // 중복 실행 안내
                return; // 추가 이동 없이 종료
            } // 이미 완료된 상태 처리 종료

            EnsureDestinationFolders(plans); // 모든 대상 기능별 폴더 사전 생성
            List<CompletedMove> pendingMoves = BuildPendingMoves(categoryName, plans, validationErrors); // 현재 실제 이동 대상과 원래 GUID 수집

            if (validationErrors.Count > 0) // Unity MoveAsset 사전 검증 오류 존재 여부 확인
            { // 이동 시작 전 안전 중단 처리
                LogErrors(validationErrors); // MoveAsset 사전 검증 오류 전체 출력
                EditorUtility.DisplayDialog("Project J Day 47", $"{categoryName} 파일 이동 사전 검사에 실패했습니다.\n\n오류: {validationErrors.Count}개\nConsole을 확인합니다.", "확인"); // 이동 사전 검사 실패 안내
                return; // 어떤 새 파일도 이동하지 않고 종료
            } // 이동 시작 전 안전 중단 처리 종료

            List<CompletedMove> movedThisRun = new List<CompletedMove>(); // 이번 실행에서 실제 이동된 파일 기록 목록 생성
            string batchMoveError = string.Empty; // 일괄 이동 오류 메시지 초기화

            AssetDatabase.StartAssetEditing(); // 이동 중 자동 Import 일시 정지 시작

            try // 일괄 MoveAsset 처리 시작
            { // Import 정지 구간에서는 경로 재조회 없이 MoveAsset만 수행
                for (int index = 0; index < pendingMoves.Count; index++) // 아직 이동되지 않은 파일 전체 순회
                { // 현재 파일 일괄 이동 처리
                    CompletedMove pendingMove = pendingMoves[index]; // 현재 이동 대상과 원래 GUID 조회
                    string moveError = AssetDatabase.MoveAsset(pendingMove.SourcePath, pendingMove.DestinationPath); // Unity AssetDatabase로 .cs와 .meta 함께 이동

                    if (!string.IsNullOrEmpty(moveError)) // 현재 파일 이동 오류 여부 확인
                    { // 일괄 이동 실패 상태 기록
                        batchMoveError = $"{pendingMove.SourcePath} -> {pendingMove.DestinationPath} | {moveError}"; // 실패 경로와 Unity 오류 저장
                        break; // 추가 이동 중단
                    } // 현재 파일 이동 오류 처리 종료

                    movedThisRun.Add(pendingMove); // 성공한 현재 이동 기록 추가
                } // 아직 이동되지 않은 파일 전체 순회 종료
            } // 일괄 MoveAsset 처리 종료
            finally // 성공·실패 공통 AssetDatabase 편집 상태 복원
            { // 자동 Import 재개 처리
                AssetDatabase.StopAssetEditing(); // 이동 일괄 처리 종료와 자동 Import 재개
            } // 성공·실패 공통 AssetDatabase 편집 상태 복원 종료

            AssetDatabase.SaveAssets(); // 이동된 asset과 폴더 메타 저장
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport); // 이동 결과를 AssetDatabase에 동기 반영

            if (!string.IsNullOrEmpty(batchMoveError)) // 일괄 이동 중 오류 발생 여부 확인
            { // Import 재개 이후 안전 롤백 처리
                Debug.LogError($"[ProjectJ][Day47] {categoryName} 폴더 통합 실패 | {batchMoveError}"); // 일괄 이동 실패 원인 출력
                RollbackCompletedMoves(movedThisRun); // 이번 실행에서 성공한 파일만 원래 위치로 복구
                EditorUtility.DisplayDialog("Project J Day 47", $"{categoryName} 폴더 통합 중 오류가 발생했습니다.\n이번 실행에서 이동된 파일은 복구를 시도했습니다.\nConsole을 확인합니다.", "확인"); // 일괄 이동 실패 안내
                return; // 실패한 단계 종료
            } // 일괄 이동 중 오류 처리 종료

            List<string> guidErrors = ValidateMovedGuids(categoryName, movedThisRun); // Import 완료 후 이동 전후 GUID 보존 상태 검증

            if (guidErrors.Count > 0) // 이동 후 GUID 오류 존재 여부 확인
            { // GUID 불일치 안전 처리
                LogErrors(guidErrors); // GUID 검증 오류 전체 출력
                EditorUtility.DisplayDialog("Project J Day 47", $"{categoryName} 이동은 완료됐지만 GUID 검증 오류가 있습니다.\nConsole을 확인하고 다음 단계로 진행하지 않습니다.", "확인"); // GUID 검증 실패 안내
                return; // 다음 단계 진행 방지
            } // GUID 불일치 안전 처리 종료

            if (cleanupLegacyMapFolder) // Map 단계 완료 뒤 기존 폴더 정리 필요 여부 확인
            { // 기존 MapGeneration 폴더 안전 정리
                TryDeleteEmptyLegacyMapGenerationFolder(); // 남은 파일이 없을 때만 기존 MapGeneration 폴더 삭제
            } // 기존 MapGeneration 폴더 안전 정리 종료

            List<string> postValidationErrors = new List<string>(); // 이동 후 최종 검증 오류 목록 생성
            ValidateCompletedPlans(categoryName, plans, postValidationErrors); // 이동된 모든 파일의 최종 경로 검증

            if (postValidationErrors.Count > 0) // 이동 후 구조 오류 존재 여부 확인
            { // 이동은 성공했지만 최종 구조 검증 실패 처리
                LogErrors(postValidationErrors); // 이동 후 검증 오류 출력
                EditorUtility.DisplayDialog("Project J Day 47", $"{categoryName} 파일 이동은 완료됐지만 구조 검증 오류 {postValidationErrors.Count}개가 있습니다.\nConsole을 확인합니다.", "확인"); // 이동 후 검증 실패 안내
                return; // 현재 단계 완료 로그 생략
            } // 이동 후 구조 검증 실패 처리 종료

            Debug.Log($"[ProjectJ][Day47] {categoryName} 기능별 폴더 통합 완료 | 이번 실행 이동 {movedThisRun.Count}개 | 전체 계획 {plans.Count}개"); // 현재 기능 폴더 통합 성공 로그
            EditorUtility.DisplayDialog("Project J Day 47", $"{categoryName} 스크립트 기능별 폴더 통합 완료\n\n이번 실행 이동: {movedThisRun.Count}개\n전체 계획: {plans.Count}개\n\n컴파일 완료 후 Console Error를 확인한 다음 다음 단계로 진행합니다.", "확인"); // 현재 기능 폴더 통합 성공 안내
        } // 지정 기능의 스크립트 이동 계획 안전 적용 종료

        private static List<CompletedMove> BuildPendingMoves(string categoryName, IReadOnlyList<MovePlan> plans, List<string> errors) // 아직 이동되지 않은 파일의 일괄 이동 목록 구성
        { // 부분 완료 상태를 건너뛰고 이동 전 GUID와 MoveAsset 가능 여부 수집
            List<CompletedMove> pendingMoves = new List<CompletedMove>(); // 실제 이동 예정 파일 목록 생성

            for (int index = 0; index < plans.Count; index++) // 현재 기능의 모든 이동 계획 순회
            { // 현재 계획의 부분 완료 여부와 이동 가능 여부 검사
                MovePlan plan = plans[index]; // 현재 이동 계획 조회
                string sourceGuid = AssetDatabase.AssetPathToGUID(plan.SourcePath); // 기존 경로 GUID 조회
                string destinationGuid = AssetDatabase.AssetPathToGUID(plan.DestinationPath); // 대상 경로 GUID 조회

                if (string.IsNullOrEmpty(sourceGuid) && !string.IsNullOrEmpty(destinationGuid)) // 이미 새 위치로 이동된 항목 여부 확인
                { // 최신 커밋의 부분 완료 상태 호환 처리
                    continue; // 이미 완료된 항목은 이번 실행에서 생략
                } // 이미 새 위치로 이동된 항목 처리 종료

                if (string.IsNullOrEmpty(sourceGuid)) // 이동할 원본 asset GUID 누락 여부 확인
                { // 사전 검증 이후 예상하지 못한 원본 누락 처리
                    errors.Add($"{categoryName} 이동 원본 GUID 누락: {plan.SourcePath}"); // 원본 GUID 누락 오류 추가
                    continue; // 다음 계획 검사
                } // 이동할 원본 asset GUID 누락 처리 종료

                string moveValidationError = AssetDatabase.ValidateMoveAsset(plan.SourcePath, plan.DestinationPath); // 실제 이동 전 Unity 이동 가능 여부 검사

                if (!string.IsNullOrEmpty(moveValidationError)) // Unity 이동 사전 검증 오류 여부 확인
                { // 이동 전 충돌이나 경로 문제 처리
                    errors.Add($"{categoryName} MoveAsset 사전 검사 실패: {plan.SourcePath} -> {plan.DestinationPath} | {moveValidationError}"); // Unity 사전 검사 오류 추가
                    continue; // 다음 계획 검사
                } // Unity 이동 사전 검증 오류 처리 종료

                pendingMoves.Add(new CompletedMove(plan.SourcePath, plan.DestinationPath, sourceGuid)); // 이동 대상과 원래 GUID 기록 추가
            } // 현재 기능의 모든 이동 계획 순회 종료

            return pendingMoves; // 실제 일괄 이동 예정 파일 목록 반환
        } // 아직 이동되지 않은 파일의 일괄 이동 목록 구성 종료

        private static List<string> ValidateMovedGuids(string categoryName, IReadOnlyList<CompletedMove> movedThisRun) // Import 완료 후 이번 실행 이동 파일 GUID 보존 검증
        { // Asset Editing 정지 구간 밖에서만 새 경로 GUID 조회
            List<string> errors = new List<string>(); // GUID 검증 오류 목록 생성

            for (int index = 0; index < movedThisRun.Count; index++) // 이번 실행에서 이동한 파일 전체 순회
            { // 현재 이동 파일의 최종 경로와 GUID 검사
                CompletedMove completedMove = movedThisRun[index]; // 현재 이동 기록 조회
                string sourceGuid = AssetDatabase.AssetPathToGUID(completedMove.SourcePath); // 이동 전 경로에 남은 asset GUID 조회
                string destinationGuid = AssetDatabase.AssetPathToGUID(completedMove.DestinationPath); // Import 반영된 새 경로 GUID 조회

                if (!string.IsNullOrEmpty(sourceGuid)) // 기존 경로에 asset이 다시 남아 있는지 확인
                { // 기존 경로 잔존 오류 처리
                    errors.Add($"{categoryName} 이동 후 기존 경로 잔존: {completedMove.SourcePath}"); // 기존 경로 잔존 오류 추가
                } // 기존 경로 잔존 오류 처리 종료

                if (string.IsNullOrEmpty(destinationGuid)) // 새 경로 GUID 확인 가능 여부 검사
                { // 새 경로 AssetDatabase 반영 실패 처리
                    errors.Add($"{categoryName} 이동 후 대상 GUID 누락: {completedMove.DestinationPath}"); // 대상 GUID 누락 오류 추가
                    continue; // 현재 항목 GUID 비교 생략
                } // 새 경로 AssetDatabase 반영 실패 처리 종료

                if (!string.Equals(completedMove.Guid, destinationGuid, StringComparison.Ordinal)) // 이동 전후 GUID 동일 여부 확인
                { // .meta 참조 보존 실패 처리
                    errors.Add($"{categoryName} GUID 불일치: {completedMove.DestinationPath} | 이전 {completedMove.Guid} | 이후 {destinationGuid}"); // GUID 불일치 오류 추가
                } // .meta 참조 보존 실패 처리 종료
            } // 이번 실행에서 이동한 파일 전체 순회 종료

            return errors; // GUID 검증 오류 목록 반환
        } // Import 완료 후 이번 실행 이동 파일 GUID 보존 검증 종료

        private static List<string> ValidateBeforeMove(string categoryName, IReadOnlyList<MovePlan> plans, out int pendingCount) // 이동 전 소스·대상 상태 전체 검증
        { // 누락·중복·부분 충돌을 이동 전에 차단
            List<string> errors = new List<string>(); // 이동 전 검증 오류 목록 생성
            pendingCount = 0; // 아직 이동할 파일 개수 초기화

            for (int index = 0; index < plans.Count; index++) // 현재 기능의 모든 계획 순회
            { // 현재 소스와 대상 경로 상태 검사
                MovePlan plan = plans[index]; // 현재 이동 계획 조회
                bool sourceExists = !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(plan.SourcePath)); // 기존 소스 asset 존재 여부 확인
                bool destinationExists = !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(plan.DestinationPath)); // 새 대상 asset 존재 여부 확인

                if (sourceExists && destinationExists) // 소스와 대상이 동시에 존재하는 충돌 상태 확인
                { // 중복 파일 충돌 처리
                    errors.Add($"{categoryName} 중복 경로: 소스와 대상이 모두 존재 | {plan.SourcePath} | {plan.DestinationPath}"); // 중복 경로 오류 추가
                    continue; // 다음 계획 검증
                } // 중복 파일 충돌 처리 종료

                if (!sourceExists && !destinationExists) // 소스와 대상이 모두 없는 누락 상태 확인
                { // 최신 커밋과 다른 구조 처리
                    errors.Add($"{categoryName} 파일 누락: {plan.SourcePath} 또는 {plan.DestinationPath}"); // 누락 파일 오류 추가
                    continue; // 다음 계획 검증
                } // 파일 누락 처리 종료

                if (sourceExists) // 아직 기존 위치에 있는 정상 이동 대상 여부 확인
                { // 실제 이동 예정 파일 집계
                    pendingCount++; // 이동 예정 파일 개수 증가
                } // 실제 이동 예정 파일 집계 종료
            } // 현재 기능의 모든 계획 검증 종료

            return errors; // 이동 전 검증 오류 목록 반환
        } // 이동 전 소스·대상 상태 전체 검증 종료

        private static void ValidateCompletedPlans(string categoryName, IReadOnlyList<MovePlan> plans, List<string> errors) // 기능별 최종 폴더 이동 완료 상태 검증
        { // 기존 위치 부재와 새 위치 존재 여부 전체 검사
            for (int index = 0; index < plans.Count; index++) // 현재 기능의 모든 이동 계획 순회
            { // 현재 파일의 최종 위치 검사
                MovePlan plan = plans[index]; // 현재 이동 계획 조회
                string sourceGuid = AssetDatabase.AssetPathToGUID(plan.SourcePath); // 기존 경로 GUID 조회
                string destinationGuid = AssetDatabase.AssetPathToGUID(plan.DestinationPath); // 새 경로 GUID 조회

                if (!string.IsNullOrEmpty(sourceGuid)) // 기존 경로에 파일이 아직 남아 있는지 확인
                { // 기존 위치 잔존 오류 처리
                    errors.Add($"{categoryName} 기존 경로 잔존: {plan.SourcePath}"); // 기존 위치 잔존 오류 추가
                } // 기존 위치 잔존 오류 처리 종료

                if (string.IsNullOrEmpty(destinationGuid)) // 새 대상 경로의 파일 존재 여부 확인
                { // 새 위치 파일 누락 오류 처리
                    errors.Add($"{categoryName} 대상 경로 누락: {plan.DestinationPath}"); // 대상 위치 누락 오류 추가
                } // 새 위치 파일 누락 오류 처리 종료
            } // 현재 기능의 모든 최종 위치 검사 종료
        } // 기능별 최종 폴더 이동 완료 상태 검증 종료

        private static void ValidateRuntimeAssemblyDefinition(List<string> errors) // Runtime Assembly Definition 위치와 단일성 검증
        { // 폴더 이동으로 어셈블리 경계가 바뀌지 않았는지 확인
            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(RuntimeAsmdefPath))) // Runtime 루트 asmdef 존재 여부 확인
            { // Runtime asmdef 누락 처리
                errors.Add($"Runtime asmdef 누락: {RuntimeAsmdefPath}"); // Runtime asmdef 누락 오류 추가
                return; // 추가 asmdef 검사 중단
            } // Runtime asmdef 누락 처리 종료

            string[] asmdefGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { RuntimeRootPath }); // Runtime 내부 Assembly Definition 전체 검색
            int runtimeAsmdefCount = 0; // Runtime 내부 asmdef 파일 개수 초기화

            for (int index = 0; index < asmdefGuids.Length; index++) // 검색된 Assembly Definition 전체 순회
            { // 현재 asmdef 프로젝트 경로 확인
                string assetPath = AssetDatabase.GUIDToAssetPath(asmdefGuids[index]); // 현재 asmdef 경로 조회

                if (assetPath.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase)) // Assembly Definition 실제 파일 여부 확인
                { // Runtime asmdef 개수 집계
                    runtimeAsmdefCount++; // Runtime asmdef 개수 증가
                } // Runtime asmdef 개수 집계 종료
            } // Runtime 내부 Assembly Definition 전체 순회 종료

            if (runtimeAsmdefCount != 1) // Runtime 어셈블리 경계가 하나인지 확인
            { // 예상하지 않은 중첩 asmdef 존재 처리
                errors.Add($"Runtime asmdef 개수 불일치: 예상 1개 | 실제 {runtimeAsmdefCount}개"); // Runtime asmdef 개수 오류 추가
            } // 예상하지 않은 중첩 asmdef 존재 처리 종료
        } // Runtime Assembly Definition 위치와 단일성 검증 종료

        private static void EnsureDestinationFolders(IReadOnlyList<MovePlan> plans) // 모든 이동 대상 부모 폴더 생성
        { // Unity AssetDatabase를 이용한 새 폴더와 .meta 생성
            for (int index = 0; index < plans.Count; index++) // 현재 기능의 모든 이동 계획 순회
            { // 현재 대상 파일의 부모 폴더 생성
                string destinationFolder = GetParentFolder(plans[index].DestinationPath); // 대상 파일 부모 폴더 경로 계산
                EnsureFolder(destinationFolder); // 대상 기능별 폴더 재귀 생성
            } // 모든 대상 파일 부모 폴더 생성 종료

            AssetDatabase.SaveAssets(); // 새 폴더 메타 저장
            AssetDatabase.Refresh(); // 새 폴더 상태 Unity에 반영
        } // 모든 이동 대상 부모 폴더 생성 종료

        private static void EnsureFolder(string folderPath) // 지정 Unity asset 폴더 재귀 생성
        { // 이미 존재하는 폴더를 유지하며 필요한 부모부터 생성
            if (AssetDatabase.IsValidFolder(folderPath)) // 대상 폴더 기존 존재 여부 확인
            { // 이미 존재하는 폴더 처리
                return; // 추가 생성 없이 종료
            } // 이미 존재하는 폴더 처리 종료

            string parentFolder = GetParentFolder(folderPath); // 현재 폴더의 부모 경로 계산
            string folderName = folderPath.Substring(parentFolder.Length + 1); // 생성할 마지막 폴더 이름 계산
            EnsureFolder(parentFolder); // 부모 폴더가 없으면 먼저 재귀 생성
            string folderGuid = AssetDatabase.CreateFolder(parentFolder, folderName); // Unity AssetDatabase로 폴더와 .meta 생성

            if (string.IsNullOrEmpty(folderGuid)) // Unity 폴더 생성 실패 여부 확인
            { // 폴더 생성 실패 처리
                throw new InvalidOperationException($"폴더 생성 실패: {folderPath}"); // 이동 전 중단용 예외 발생
            } // 폴더 생성 실패 처리 종료
        } // 지정 Unity asset 폴더 재귀 생성 종료

        private static string GetParentFolder(string assetPath) // Unity asset 경로의 부모 폴더 계산
        { // 마지막 슬래시 기준 부모 경로 추출
            int slashIndex = assetPath.LastIndexOf('/'); // 마지막 폴더 구분자 위치 검색

            if (slashIndex <= 0) // 유효한 부모 경로 존재 여부 확인
            { // 비정상 asset 경로 처리
                throw new ArgumentException($"유효하지 않은 Asset 경로: {assetPath}", nameof(assetPath)); // 잘못된 경로 예외 발생
            } // 비정상 asset 경로 처리 종료

            return assetPath.Substring(0, slashIndex); // 부모 폴더 경로 반환
        } // Unity asset 경로의 부모 폴더 계산 종료

        private static void RollbackCompletedMoves(IReadOnlyList<CompletedMove> completedMoves) // 이번 실행에서 성공한 이동을 Import 재개 뒤 역순 복구
        { // AssetDatabase가 최신 이동 경로를 인식한 상태에서 안전 복구
            if (completedMoves.Count == 0) // 실제 이동된 파일 존재 여부 확인
            { // 복구 대상 없음 처리
                return; // 추가 AssetDatabase 작업 없이 종료
            } // 복구 대상 없음 처리 종료

            AssetDatabase.StartAssetEditing(); // 롤백 파일 일괄 이동 중 자동 Import 정지 시작

            try // 성공한 이동 역순 롤백 처리 시작
            { // 각 파일을 새 위치에서 기존 위치로 복구
                for (int index = completedMoves.Count - 1; index >= 0; index--) // 성공한 이동을 역순으로 순회
                { // 현재 파일 원래 위치 복구 처리
                    CompletedMove completedMove = completedMoves[index]; // 현재 복구 대상 이동 기록 조회
                    string destinationGuid = AssetDatabase.AssetPathToGUID(completedMove.DestinationPath); // 새 위치의 현재 asset GUID 조회
                    string sourceGuid = AssetDatabase.AssetPathToGUID(completedMove.SourcePath); // 기존 위치의 현재 asset GUID 조회

                    if (string.IsNullOrEmpty(destinationGuid) || !string.IsNullOrEmpty(sourceGuid)) // 롤백 가능한 현재 경로 상태 여부 확인
                    { // 예상하지 못한 경로 상태 처리
                        Debug.LogError($"[ProjectJ][Day47] 롤백 경로 상태 오류 | 기존 {completedMove.SourcePath} | 대상 {completedMove.DestinationPath}"); // 롤백 경로 불일치 로그
                        continue; // 나머지 파일 복구 계속 진행
                    } // 예상하지 못한 경로 상태 처리 종료

                    string rollbackError = AssetDatabase.MoveAsset(completedMove.DestinationPath, completedMove.SourcePath); // 새 위치에서 기존 위치로 asset 복구

                    if (!string.IsNullOrEmpty(rollbackError)) // 복구 이동 실패 여부 확인
                    { // 롤백 실패 처리
                        Debug.LogError($"[ProjectJ][Day47] 롤백 실패 | {completedMove.DestinationPath} -> {completedMove.SourcePath} | {rollbackError}"); // 롤백 실패 상세 로그
                    } // 롤백 실패 처리 종료
                } // 성공한 이동 역순 롤백 종료
            } // 성공한 이동 역순 롤백 처리 종료
            finally // 롤백 성공·실패 공통 AssetDatabase 상태 복원
            { // 자동 Import 재개 처리
                AssetDatabase.StopAssetEditing(); // 롤백 일괄 이동 종료와 자동 Import 재개
            } // 롤백 성공·실패 공통 AssetDatabase 상태 복원 종료

            AssetDatabase.SaveAssets(); // 롤백된 asset과 meta 저장
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport); // 롤백 경로 상태 동기 반영

            for (int index = 0; index < completedMoves.Count; index++) // 롤백 대상 전체 GUID 재검증
            { // 현재 복구 파일의 원래 GUID 확인
                CompletedMove completedMove = completedMoves[index]; // 현재 복구 기록 조회
                string restoredGuid = AssetDatabase.AssetPathToGUID(completedMove.SourcePath); // 복구된 기존 위치 GUID 조회

                if (!string.Equals(restoredGuid, completedMove.Guid, StringComparison.Ordinal)) // 복구 전후 GUID 동일 여부 확인
                { // 롤백 GUID 불일치 처리
                    Debug.LogError($"[ProjectJ][Day47] 롤백 GUID 불일치 | {completedMove.SourcePath} | 예상 {completedMove.Guid} | 실제 {restoredGuid}"); // 롤백 GUID 문제 로그
                } // 롤백 GUID 불일치 처리 종료
            } // 롤백 대상 전체 GUID 재검증 종료
        } // 이번 실행에서 성공한 이동을 Import 재개 뒤 역순 복구 종료

        private static void TryDeleteEmptyLegacyMapGenerationFolder() // 기존 MapGeneration 폴더가 완전히 비었을 때만 삭제
        { // 알 수 없는 파일이 남은 폴더의 실수 삭제 방지
            if (!AssetDatabase.IsValidFolder(LegacyMapGenerationFolderPath)) // 기존 MapGeneration 폴더 존재 여부 확인
            { // 이미 제거된 기존 폴더 처리
                return; // 추가 삭제 없이 종료
            } // 이미 제거된 기존 폴더 처리 종료

            string[] remainingEntries = Directory.GetFileSystemEntries(LegacyMapGenerationFolderPath); // 기존 폴더 내부 실제 파일과 하위 폴더 검색

            if (remainingEntries.Length > 0) // 예상하지 않은 남은 항목 존재 여부 확인
            { // 기존 폴더 보존 처리
                Debug.LogWarning($"[ProjectJ][Day47] 기존 MapGeneration 폴더에 알 수 없는 항목 {remainingEntries.Length}개가 남아 있어 폴더를 삭제하지 않았습니다."); // 안전 보존 경고 출력
                return; // 알 수 없는 파일 보호를 위해 폴더 삭제 중단
            } // 기존 폴더 보존 처리 종료

            if (!AssetDatabase.DeleteAsset(LegacyMapGenerationFolderPath)) // 빈 기존 폴더와 폴더 meta 삭제 성공 여부 확인
            { // 빈 폴더 삭제 실패 처리
                Debug.LogWarning($"[ProjectJ][Day47] 빈 기존 MapGeneration 폴더 삭제에 실패했습니다: {LegacyMapGenerationFolderPath}"); // 빈 폴더 삭제 실패 경고
            } // 빈 폴더 삭제 실패 처리 종료
        } // 기존 MapGeneration 폴더 안전 삭제 종료

        private static void LogErrors(IReadOnlyList<string> errors) // 검증 오류 전체 Console 출력
        { // 사용자 확인을 위한 오류별 독립 로그 처리
            for (int index = 0; index < errors.Count; index++) // 모든 오류 순회
            { // 현재 오류 Console 출력
                Debug.LogError($"[ProjectJ][Day47] {errors[index]}"); // 47일차 폴더 구조 오류 로그
            } // 모든 오류 Console 출력 종료
        } // 검증 오류 전체 Console 출력 종료

        private readonly struct MovePlan // 단일 Runtime 스크립트 이동 계획 값 선언
        { // 기존 경로와 새 기능별 경로 저장
            public MovePlan(string sourcePath, string destinationPath) // 단일 파일 이동 계획 생성
            { // 전달된 이동 경로 저장
                SourcePath = sourcePath; // 기존 스크립트 경로 저장
                DestinationPath = destinationPath; // 새 기능별 스크립트 경로 저장
            } // 단일 파일 이동 계획 생성 종료

            public string SourcePath { get; } // 기존 스크립트 경로 반환
            public string DestinationPath { get; } // 새 기능별 스크립트 경로 반환
        } // 단일 Runtime 스크립트 이동 계획 값 정의 종료

        private readonly struct CompletedMove // 현재 실행에서 완료한 이동 기록 값 선언
        { // 롤백에 필요한 이전·새 경로와 GUID 저장
            public CompletedMove(string sourcePath, string destinationPath, string guid) // 완료 이동 기록 생성
            { // 전달된 롤백 정보 저장
                SourcePath = sourcePath; // 이동 전 경로 저장
                DestinationPath = destinationPath; // 이동 후 경로 저장
                Guid = guid; // 원래 asset GUID 저장
            } // 완료 이동 기록 생성 종료

            public string SourcePath { get; } // 이동 전 경로 반환
            public string DestinationPath { get; } // 이동 후 경로 반환
            public string Guid { get; } // 이동 전 asset GUID 반환
        } // 현재 실행에서 완료한 이동 기록 값 정의 종료
    } // Data·Map·Item 세 단계 안전 이동 기능 정의 종료
} // 프로젝트 Editor 기능 네임스페이스 종료
