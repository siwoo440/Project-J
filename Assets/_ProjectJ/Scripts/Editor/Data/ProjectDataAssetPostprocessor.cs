using System; // 경로 문자열 비교 기능 참조
using UnityEditor; // Unity 에셋 후처리와 지연 실행 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal sealed class ProjectDataAssetPostprocessor : AssetPostprocessor // 데이터 에셋 변경 자동 검증 후처리기 선언
    {
        private static bool validationQueued; // 지연 검증 예약 여부 저장

        private static void OnPostprocessAllAssets( // 모든 에셋 변경 후 자동 호출 메서드 선언
            string[] importedAssets, // 새로 가져오거나 변경된 에셋 경로 배열
            string[] deletedAssets, // 삭제된 에셋 경로 배열
            string[] movedAssets, // 이동된 에셋의 새 경로 배열
            string[] movedFromAssetPaths) // 이동된 에셋의 이전 경로 배열
        {
            if (!ContainsDataAssetPath(importedAssets) // 가져온 에셋에 데이터 정의가 있는지 확인
                && !ContainsDataAssetPath(deletedAssets) // 삭제된 에셋에 데이터 정의가 있는지 확인
                && !ContainsDataAssetPath(movedAssets) // 이동된 새 경로에 데이터 정의가 있는지 확인
                && !ContainsDataAssetPath(movedFromAssetPaths)) // 이동된 이전 경로에 데이터 정의가 있는지 확인
            {
                return; // 데이터 정의 에셋 변경이 없으면 자동 검증 생략
            }

            QueueValidation(); // 데이터 정의 에셋 전체 지연 검증 예약
        }

        internal static bool ContainsDataAssetPath(string[] assetPaths) // 경로 배열에 데이터 정의 에셋 경로가 있는지 확인
        {
            if (assetPaths == null) // 전달된 경로 배열의 null 여부 확인
            {
                return false; // 데이터 정의 경로 없음 반환
            }

            foreach (string assetPath in assetPaths) // 모든 에셋 경로 순회
            {
                if (assetPath.StartsWith(ProjectDataAssetDatabase.DefinitionsRootPath, StringComparison.Ordinal) // 데이터 정의 루트 내부 경로인지 확인
                    && assetPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)) // Unity 에셋 파일인지 확인
                {
                    return true; // 데이터 정의 에셋 경로 존재 반환
                }
            }

            return false; // 데이터 정의 에셋 경로 없음 반환
        }

        internal static void QueueValidation() // 중복 없이 데이터 검증 지연 실행 예약
        {
            if (validationQueued) // 데이터 검증이 이미 예약되었는지 확인
            {
                return; // 중복 지연 실행 예약 없이 메서드 종료
            }

            validationQueued = true; // 데이터 검증 예약 상태 설정
            EditorApplication.delayCall += RebuildCatalogAfterImport; // 현재 에셋 처리 완료 후 카탈로그 갱신 예약
        }

        private static void RebuildCatalogAfterImport() // 에셋 처리 완료 후 카탈로그 갱신과 전체 검증
        {
            validationQueued = false; // 데이터 검증 예약 상태 초기화

            if (EditorApplication.isPlayingOrWillChangePlaymode) // Play Mode 실행 또는 진입 중인지 확인
            {
                return; // Play Mode 중 자동 데이터 검증 생략
            }

            ProjectDataCatalogBuilder.RebuildAndValidate(false); // 런타임 카탈로그 갱신과 전체 데이터 검증 실행
        }
    }

    internal sealed class ProjectDataAssetSaveProcessor : AssetModificationProcessor // 데이터 에셋 저장 시 자동 검증 처리기 선언
    {
        private static string[] OnWillSaveAssets(string[] assetPaths) // Unity 에셋 저장 직전 데이터 검증 예약
        {
            if (ProjectDataAssetPostprocessor.ContainsDataAssetPath(assetPaths)) // 저장 대상에 데이터 정의 에셋이 포함됐는지 확인
            {
                ProjectDataAssetPostprocessor.QueueValidation(); // 에셋 저장 완료 후 카탈로그 갱신 예약
            }

            return assetPaths; // Unity가 원래 저장 대상 에셋을 계속 처리하도록 경로 배열 반환
        }
    }
}
