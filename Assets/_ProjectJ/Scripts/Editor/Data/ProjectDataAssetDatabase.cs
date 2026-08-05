using System.Collections.Generic; // 데이터 에셋 목록 기능 참조
using ProjectJ.Data; // 프로젝트 데이터 형식 참조
using UnityEditor; // Unity 에셋 데이터베이스 기능 참조
using UnityEngine; // Unity 로그 기능 참조

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

            return assets; // 검색된 모든 프로젝트 데이터 에셋 반환
        }

        internal static ProjectDataValidationReport ValidateAll(bool logSuccess) // 프로젝트의 모든 데이터 정의 에셋 검사
        {
            List<ProjectDataAsset> assets = LoadAll(); // 프로젝트 데이터 정의 에셋 전체 불러오기
            ProjectDataValidationReport report = ProjectDataValidator.Validate(assets); // 공통 데이터 검증 실행

            foreach (ProjectDataValidationIssue issue in report.Issues) // 발견된 모든 검증 문제 순회
            {
                if (issue.Severity == DataValidationSeverity.Error) // 현재 문제가 오류 수준인지 확인
                {
                    Debug.LogError($"[Data] {issue.Code}: {issue.Message}", issue.Asset); // 오류 수준 데이터 문제와 에셋 출력
                }
                else // 현재 문제가 경고 수준인 경우 처리
                {
                    Debug.LogWarning($"[Data] {issue.Code}: {issue.Message}", issue.Asset); // 경고 수준 데이터 문제와 에셋 출력
                }
            }

            if (logSuccess && report.IsValid) // 성공 로그 출력 설정과 검증 성공 여부 확인
            {
                Debug.Log($"[Data] 데이터 에셋 {assets.Count}개의 ID와 필수 값 검증을 완료했습니다."); // 전체 데이터 검증 성공 로그 출력
            }

            return report; // 전체 데이터 검증 결과 반환
        }
    }
}
