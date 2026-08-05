using ProjectJ.Data; // 플레이어 데이터 설정과 검증 형식 참조
using UnityEditor; // Unity 에디터 메뉴와 에셋 기능 참조
using UnityEngine; // Unity ScriptableObject와 로그 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal static class Day07PlayerSettingsSetupTool // 7일차 기본 플레이어 설정 에셋 구성 도구 선언
    {
        private const string ConfigureMenuPath = "Project J/Day 07/Configure Default Player Settings"; // 기본 플레이어 설정 구성 메뉴 경로 선언
        private const string SelectMenuPath = "Project J/Day 07/Select Default Player Settings"; // 기본 플레이어 설정 선택 메뉴 경로 선언
        private const string DefaultPlayerFolderPath = "Assets/_ProjectJ/Data/Definitions/Player"; // 기본 플레이어 데이터 폴더 경로 선언
        private const string DefaultPlayerAssetPath = DefaultPlayerFolderPath + "/PLY-001_DefaultPlayer.asset"; // 기본 플레이어 데이터 에셋 경로 선언
        private static readonly ProjectDataVersion PlayerSettingsVersion = new ProjectDataVersion(1, 1, 0); // 플레이어 설정 필드 추가 버전 선언

        [MenuItem(ConfigureMenuPath)] // Unity 상단 메뉴에 기본 플레이어 설정 구성 항목 등록
        private static void ConfigureDefaultPlayerSettings() // 기본 플레이어 데이터 에셋 생성 또는 7일차 값 적용
        {
            EnsureFolderExists(DefaultPlayerFolderPath); // 기본 플레이어 데이터 폴더 존재 상태 보장
            PlayerDataDefinition playerData = AssetDatabase.LoadAssetAtPath<PlayerDataDefinition>(DefaultPlayerAssetPath); // 기존 기본 플레이어 데이터 에셋 조회

            if (playerData == null) // 기존 기본 플레이어 데이터 에셋 존재 여부 확인
            {
                playerData = ScriptableObject.CreateInstance<PlayerDataDefinition>(); // 새 플레이어 데이터 에셋 인스턴스 생성
                AssetDatabase.CreateAsset(playerData, DefaultPlayerAssetPath); // 새 기본 플레이어 데이터 에셋 저장
            }

            string dataId = string.IsNullOrWhiteSpace(playerData.DataId) ? "PLY-001" : playerData.DataId; // 기존 ID 또는 기본 플레이어 ID 결정
            string displayName = string.IsNullOrWhiteSpace(playerData.DisplayName) ? "Default Player" : playerData.DisplayName; // 기존 표시 이름 또는 기본 이름 결정

            Undo.RecordObject(playerData, "Configure Default Player Settings"); // 플레이어 데이터 변경 Undo 기록
            playerData.SetEditorIdentity(dataId, displayName, PlayerSettingsVersion); // 식별 정보 유지와 데이터 버전 갱신
            playerData.ResetEditorSettingsToDefaults(); // 7일차 기본 플레이어 설정 값 적용
            EditorUtility.SetDirty(playerData); // 플레이어 데이터 에셋 변경 상태 표시
            AssetDatabase.SaveAssets(); // 변경된 플레이어 데이터 에셋 저장
            AssetDatabase.Refresh(); // Project 창 에셋 목록 새로고침

            ProjectDataValidationReport report = ProjectDataAssetDatabase.ValidateAll(true); // 모든 프로젝트 데이터 에셋 검증

            if (report.HasErrors) // 전체 데이터 검증 오류 존재 여부 확인
            {
                Debug.LogError($"[Day07] 플레이어 설정 적용 후 데이터 오류 {report.ErrorCount}개를 발견했습니다.", playerData); // 설정 적용 후 오류 요약 출력
                return; // 성공 선택과 완료 로그 생략
            }

            Selection.activeObject = playerData; // 구성 완료된 기본 플레이어 에셋 선택
            EditorGUIUtility.PingObject(playerData); // Project 창에서 기본 플레이어 에셋 위치 강조
            Debug.Log("[Day07] PLY-001 기본 플레이어 설정과 1.1.0 버전 적용을 완료했습니다.", playerData); // 플레이어 설정 구성 완료 로그 출력
        }

        [MenuItem(SelectMenuPath)] // Unity 상단 메뉴에 기본 플레이어 설정 선택 항목 등록
        private static void SelectDefaultPlayerSettings() // 기본 플레이어 설정 에셋 선택
        {
            PlayerDataDefinition playerData = AssetDatabase.LoadAssetAtPath<PlayerDataDefinition>(DefaultPlayerAssetPath); // 기본 플레이어 데이터 에셋 조회

            if (playerData == null) // 기본 플레이어 데이터 에셋 존재 여부 확인
            {
                Debug.LogError($"[Day07] 기본 플레이어 데이터 에셋을 찾을 수 없습니다: {DefaultPlayerAssetPath}"); // 기본 플레이어 데이터 누락 오류 출력
                return; // 에셋 선택 작업 중단
            }

            Selection.activeObject = playerData; // 기본 플레이어 데이터 에셋 선택
            EditorGUIUtility.PingObject(playerData); // Project 창에서 기본 플레이어 에셋 위치 강조
        }

        [MenuItem(ConfigureMenuPath, true)] // 기본 플레이어 설정 구성 메뉴 활성 조건 등록
        [MenuItem(SelectMenuPath, true)] // 기본 플레이어 설정 선택 메뉴 활성 조건 등록
        private static bool ValidateEditorMenu() // Play Mode가 아닐 때만 7일차 메뉴 실행 허용
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Play Mode 진입 또는 실행 중이 아닌 경우 활성화
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
