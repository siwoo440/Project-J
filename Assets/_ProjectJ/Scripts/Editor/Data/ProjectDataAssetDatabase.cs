using System.Collections.Generic; // 데이터 에셋 목록 기능 참조
using ProjectJ.Data; // 프로젝트 데이터 형식 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEditor; // Unity 에셋 데이터베이스 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal static class ProjectDataAssetDatabase // 프로젝트 데이터 에셋 검색과 검증 도구 선언
    {
        internal const string DefinitionsRootPath = "Assets/_ProjectJ/Data/Definitions"; // 데이터 정의 에셋 공통 루트 경로 선언

        internal static List<ProjectDataAsset> LoadAll() // 프로젝트에 등록된 모든 데이터 정의 에셋 불러오기
        {
            List<ProjectDataAsset> assets = new List<ProjectDataAsset>(); // 검색된 데이터 에셋 목록 생성

            if (!AssetDatabase.IsValidFolder(DefinitionsRootPath)) // 데이터 정의 루트 폴더 존재 여부 확인
            {
                return assets; // 빈 데이터 에셋 목록 반환
            }

            string[] assetGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { DefinitionsRootPath }); // 데이터 정의 폴더의 모든 ScriptableObject GUID 검색

            foreach (string assetGuid in assetGuids) // 검색된 모든 에셋 GUID 순회
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid); // 에셋 GUID를 프로젝트 경로로 변환
                ProjectDataAsset asset = AssetDatabase.LoadAssetAtPath<ProjectDataAsset>(assetPath); // 프로젝트 데이터 에셋 형식으로 불러오기

                if (asset == null) // 현재 에셋이 프로젝트 데이터 형식인지 확인
                {
                    continue; // 다른 ScriptableObject는 검사 대상에서 제외
                }

                assets.Add(asset); // 프로젝트 데이터 에셋 목록에 추가
            }

            assets.Sort(CompareAssets); // 데이터 ID 기준으로 카탈로그 순서 고정
            return assets; // 검색된 모든 프로젝트 데이터 에셋 반환
        }

        internal static ProjectDataValidationReport ValidateAll(bool logSuccess) // 프로젝트의 모든 데이터 정의 에셋과 필수 분류 검사
        {
            List<ProjectDataAsset> assets = LoadAll(); // 프로젝트 데이터 정의 에셋 전체 불러오기
            ProjectDataValidationReport report = ProjectDataValidator.ValidateCatalog(assets); // 공통 값과 필수 분류 전체 검증 실행
            LogReport(report, assets.Count, logSuccess); // 검증 문제와 선택적 성공 로그 출력
            return report; // 전체 데이터 검증 결과 반환
        }

        internal static ProjectDataAsset FindById(IReadOnlyList<ProjectDataAsset> assets, string dataId) // 데이터 ID가 일치하는 기존 에셋 검색
        {
            for (int index = 0; index < assets.Count; index++) // 전체 데이터 에셋 순회
            {
                ProjectDataAsset asset = assets[index]; // 현재 데이터 에셋 조회

                if (asset != null && string.Equals(asset.DataId, dataId, System.StringComparison.OrdinalIgnoreCase)) // 현재 에셋 ID 일치 여부 확인
                {
                    return asset; // 일치하는 기존 데이터 에셋 반환
                }
            }

            return null; // 일치하는 데이터 에셋 없음 반환
        }

        private static void LogReport(ProjectDataValidationReport report, int assetCount, bool logSuccess) // 데이터 검증 결과 등급별 출력
        {
            foreach (ProjectDataValidationIssue issue in report.Issues) // 발견된 모든 검증 문제 순회
            {
                if (issue.Severity == DataValidationSeverity.Error) // 현재 문제가 오류 수준인지 확인
                {
                    ProjectLog.Error(ProjectLogCategory.Data, issue.Message, issue.Code, issue.Asset); // 오류 수준 데이터 문제 출력
                    continue; // 다음 검증 문제 처리
                }

                ProjectLog.Warning(ProjectLogCategory.Data, issue.Message, issue.Code, issue.Asset); // 경고 수준 데이터 문제 출력
            }

            if (logSuccess && report.IsValid) // 성공 로그 출력 설정과 검증 성공 여부 확인
            {
                ProjectLog.Info(ProjectLogCategory.Data, $"데이터 에셋 {assetCount}개의 카탈로그 검증을 완료했습니다.", "DATA_VALIDATION_PASSED"); // 전체 데이터 검증 성공 로그 출력
            }
        }

        private static int CompareAssets(ProjectDataAsset left, ProjectDataAsset right) // 두 데이터 에셋의 정렬 순서 비교
        {
            string leftId = left != null ? left.DataId : string.Empty; // 왼쪽 데이터 에셋 ID 조회
            string rightId = right != null ? right.DataId : string.Empty; // 오른쪽 데이터 에셋 ID 조회
            return string.CompareOrdinal(leftId, rightId); // 데이터 ID 오름차순 비교 결과 반환
        }
    }
}
