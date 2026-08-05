namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    public sealed class ProjectDataValidationIssue // 단일 데이터 검증 문제 정보 선언
    {
        public ProjectDataAsset Asset { get; } // 문제가 발생한 데이터 에셋 반환
        public DataValidationSeverity Severity { get; } // 문제 심각도 반환
        public string Code { get; } // 문제 분류 코드 반환
        public string Message { get; } // 문제 설명 반환

        public ProjectDataValidationIssue(ProjectDataAsset asset, DataValidationSeverity severity, string code, string message) // 데이터 검증 문제 정보 생성
        {
            Asset = asset; // 문제가 발생한 데이터 에셋 저장
            Severity = severity; // 문제 심각도 저장
            Code = code; // 문제 분류 코드 저장
            Message = message; // 문제 설명 저장
        }

        public override string ToString() // 데이터 검증 문제를 로그 문자열로 변환
        {
            string assetName = Asset != null ? Asset.name : "Null"; // 데이터 에셋 이름 또는 Null 표시 조회
            return $"[{Severity}] {Code} / {assetName} / {Message}"; // 문제 심각도와 코드와 에셋과 설명 조합 반환
        }
    }
}
