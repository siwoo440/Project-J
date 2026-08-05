namespace ProjectJ.Diagnostics // 프로젝트 공통 로그 네임스페이스
{ // 네임스페이스 범위
    public enum ProjectLogLevel // 프로젝트 로그 최소 등급 종류
    { // 열거형 범위
        Verbose = 0, // 개발 과정 상세 정보 등급
        Info = 1, // 정상 흐름 확인 정보 등급
        Warning = 2, // 복구 가능한 문제 경고 등급
        Error = 3, // 기능 진행 불가 오류 등급
        Off = 4 // 전체 로그 비활성화 등급
    } // 열거형 범위
} // 네임스페이스 범위
