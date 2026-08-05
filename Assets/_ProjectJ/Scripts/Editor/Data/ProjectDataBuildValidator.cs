using ProjectJ.Data; // 프로젝트 데이터 검증 결과 형식 참조
using UnityEditor.Build; // Unity 빌드 전처리와 실패 예외 기능 참조
using UnityEditor.Build.Reporting; // Unity 빌드 보고서 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal sealed class ProjectDataBuildValidator : IPreprocessBuildWithReport // 빌드 전 데이터 카탈로그 검증 처리기 선언
    {
        public int callbackOrder => -1000; // 다른 빌드 처리보다 먼저 실행할 순서 반환

        public void OnPreprocessBuild(BuildReport report) // 모든 Unity 빌드 시작 전 데이터 검증
        {
            ProjectDataValidationReport validationReport = ProjectDataCatalogBuilder.RebuildAndValidate(true); // 최신 카탈로그 갱신과 전체 데이터 검증 실행

            if (validationReport.HasErrors) // 빌드를 차단할 데이터 오류 존재 여부 확인
            {
                throw new BuildFailedException($"Project J 데이터 오류 {validationReport.ErrorCount}개로 빌드를 중단했습니다. Unity Console의 DATA_ 오류를 수정하세요."); // 잘못된 데이터 포함 빌드 차단
            }
        }
    }
}
