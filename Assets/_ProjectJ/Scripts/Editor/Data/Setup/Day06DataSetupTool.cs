using ProjectJ.Data; // 프로젝트 데이터 정의와 버전 형식 참조
using UnityEditor; // Unity 에디터 메뉴와 에셋 기능 참조
using UnityEngine; // Unity ScriptableObject와 로그 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal static class Day06DataSetupTool // 6일차 공통 데이터 ID 구조 자동 구성 도구 선언
    {
        private const string CreateMenuPath = ProjectJEditorMenuPaths.DataBase + "/샘플 데이터 에셋 생성 (Day 06일차)"; // 샘플 데이터 에셋 생성 메뉴 경로 선언
        private const string ValidateMenuPath = ProjectJEditorMenuPaths.DataBase + "/전체 데이터 에셋 검증 (Day 06일차)"; // 전체 데이터 검증 메뉴 경로 선언
        private static readonly ProjectDataVersion InitialVersion = new ProjectDataVersion(1, 0, 0); // 신규 샘플 데이터 초기 버전 선언

        [MenuItem(CreateMenuPath)] // Unity 상단 메뉴에 샘플 데이터 생성 항목 등록
        private static void CreateSampleDataAssets() // 데이터 분류 폴더와 샘플 데이터 에셋 생성
        {
            EnsureFolderExists(ProjectDataAssetDatabase.DefinitionsRootPath); // 데이터 정의 공통 루트 폴더 존재 상태 보장
            EnsureFolderExists($"{ProjectDataAssetDatabase.DefinitionsRootPath}/Player"); // 플레이어 데이터 폴더 존재 상태 보장
            EnsureFolderExists($"{ProjectDataAssetDatabase.DefinitionsRootPath}/Map"); // 맵 데이터 폴더 존재 상태 보장
            EnsureFolderExists($"{ProjectDataAssetDatabase.DefinitionsRootPath}/Obstacle"); // 장애물 데이터 폴더 존재 상태 보장
            EnsureFolderExists($"{ProjectDataAssetDatabase.DefinitionsRootPath}/Item"); // 아이템 데이터 폴더 존재 상태 보장
            EnsureFolderExists($"{ProjectDataAssetDatabase.DefinitionsRootPath}/Cosmetic"); // 꾸미기 데이터 폴더 존재 상태 보장
            EnsureFolderExists($"{ProjectDataAssetDatabase.DefinitionsRootPath}/Audio"); // 오디오 데이터 폴더 존재 상태 보장

            CreateAssetIfMissing<PlayerDataDefinition>("Player/PLY-001_DefaultPlayer.asset", "PLY-001", "Default Player"); // 기본 플레이어 샘플 에셋 생성
            CreateAssetIfMissing<MapDataDefinition>("Map/MAP-001_DefaultMap.asset", "MAP-001", "Default Map"); // 기본 맵 샘플 에셋 생성
            CreateAssetIfMissing<ObstacleDataDefinition>("Obstacle/OBS-001_DefaultObstacle.asset", "OBS-001", "Default Obstacle"); // 기본 장애물 샘플 에셋 생성
            CreateAssetIfMissing<ItemDataDefinition>("Item/ITM-001_SpringShoes.asset", "ITM-001", "Spring Shoes"); // 첫 번째 아이템 샘플 에셋 생성
            CreateAssetIfMissing<CosmeticDataDefinition>("Cosmetic/COS-001_DefaultCostume.asset", "COS-001", "Default Costume"); // 기본 꾸미기 샘플 에셋 생성
            CreateAssetIfMissing<AudioDataDefinition>("Audio/AUD-001_DefaultAudio.asset", "AUD-001", "Default Audio"); // 기본 오디오 샘플 에셋 생성

            AssetDatabase.SaveAssets(); // 새로 생성한 데이터 에셋 저장
            AssetDatabase.Refresh(); // Project 창 에셋 목록 새로고침
            ProjectDataAssetDatabase.ValidateAll(true); // 생성된 모든 데이터 에셋 ID와 필수 값 검사
        }

        [MenuItem(ValidateMenuPath)] // Unity 상단 메뉴에 전체 데이터 검증 항목 등록
        private static void ValidateAllDataAssets() // 모든 프로젝트 데이터 에셋 수동 검증
        {
            ProjectDataValidationReport report = ProjectDataAssetDatabase.ValidateAll(true); // 전체 데이터 에셋 검증 실행

            if (report.HasErrors) // 데이터 검증 오류 존재 여부 확인
            {
                Debug.LogError($"[Day06] 데이터 검증에서 오류 {report.ErrorCount}개를 발견했습니다."); // 전체 검증 실패 요약 로그 출력
            }
        }

        [MenuItem(CreateMenuPath, true)] // 샘플 데이터 생성 메뉴 활성 조건 등록
        [MenuItem(ValidateMenuPath, true)] // 전체 데이터 검증 메뉴 활성 조건 등록
        private static bool ValidateEditorMenu() // Play Mode가 아닐 때만 6일차 메뉴 실행 허용
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Play Mode 진입 또는 실행 중이 아닌 경우 활성화
        }

        private static void CreateAssetIfMissing<T>(string relativePath, string dataId, string displayName) where T : ProjectDataAsset // 지정 경로의 샘플 데이터 에셋 존재 상태 보장
        {
            string fullPath = $"{ProjectDataAssetDatabase.DefinitionsRootPath}/{relativePath}"; // 샘플 데이터 에셋 전체 경로 생성
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(fullPath); // 지정 경로의 기존 데이터 에셋 조회

            if (existingAsset != null) // 기존 샘플 데이터 에셋 존재 여부 확인
            {
                return; // 기존 에셋을 덮어쓰지 않고 메서드 종료
            }

            T newAsset = ScriptableObject.CreateInstance<T>(); // 지정 형식의 새 데이터 에셋 인스턴스 생성
            newAsset.SetEditorIdentity(dataId, displayName, InitialVersion); // 새 데이터 에셋 ID와 표시 이름과 초기 버전 설정
            AssetDatabase.CreateAsset(newAsset, fullPath); // 새 데이터 에셋을 지정 경로에 저장
            EditorUtility.SetDirty(newAsset); // 새 데이터 에셋 변경 상태 표시
        }

        private static void EnsureFolderExists(string folderPath) // 지정된 Unity 에셋 폴더 존재 상태 보장
        {
            if (AssetDatabase.IsValidFolder(folderPath)) // 대상 폴더가 이미 존재하는지 확인
            {
                return; // 폴더 생성 작업 생략
            }

            string[] pathParts = folderPath.Split('/'); // 폴더 경로를 각 단계로 분리
            string currentPath = pathParts[0]; // 첫 번째 Assets 경로 저장

            for (int index = 1; index < pathParts.Length; index++) // 하위 폴더 경로 순회
            {
                string nextPath = $"{currentPath}/{pathParts[index]}"; // 다음 단계 전체 경로 생성

                if (!AssetDatabase.IsValidFolder(nextPath)) // 다음 단계 폴더 존재 여부 확인
                {
                    AssetDatabase.CreateFolder(currentPath, pathParts[index]); // 누락된 하위 폴더 생성
                }

                currentPath = nextPath; // 현재 경로를 다음 단계로 갱신
            }
        }
    }
}
