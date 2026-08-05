using System.Collections.Generic; // 검증 문제 목록 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    public sealed class ProjectDataValidationReport // 전체 데이터 검증 결과 선언
    {
        private readonly List<ProjectDataValidationIssue> issues = new List<ProjectDataValidationIssue>(); // 발견된 검증 문제 목록 저장

        public IReadOnlyList<ProjectDataValidationIssue> Issues => issues; // 발견된 모든 검증 문제 반환
        public int IssueCount => issues.Count; // 전체 검증 문제 수 반환
        public int ErrorCount { get; private set; } // 오류 수준 문제 수 저장
        public int WarningCount { get; private set; } // 경고 수준 문제 수 저장
        public bool HasErrors => ErrorCount > 0; // 오류 존재 여부 반환
        public bool IsValid => ErrorCount == 0; // 데이터 검증 성공 여부 반환

        public void Add(ProjectDataValidationIssue issue) // 검증 문제를 결과에 추가
        {
            if (issue == null) // 전달된 검증 문제의 null 여부 확인
            {
                return; // null 문제 추가 없이 메서드 종료
            }

            issues.Add(issue); // 검증 문제 목록에 문제 추가

            if (issue.Severity == DataValidationSeverity.Error) // 추가된 문제가 오류 수준인지 확인
            {
                ErrorCount++; // 오류 문제 수 증가
            }
            else // 추가된 문제가 경고 수준인 경우 처리
            {
                WarningCount++; // 경고 문제 수 증가
            }
        }

        public void AddError(ProjectDataAsset asset, string code, string message) // 오류 수준 검증 문제 추가
        {
            Add(new ProjectDataValidationIssue(asset, DataValidationSeverity.Error, code, message)); // 오류 문제 정보 생성과 추가
        }

        public void AddWarning(ProjectDataAsset asset, string code, string message) // 경고 수준 검증 문제 추가
        {
            Add(new ProjectDataValidationIssue(asset, DataValidationSeverity.Warning, code, message)); // 경고 문제 정보 생성과 추가
        }
    }
}
