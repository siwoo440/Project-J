using UnityEngine; // Unity Console 로그와 런타임 초기화 기능 참조

namespace ProjectJ.Diagnostics // 프로젝트 공통 로그 네임스페이스
{ // 네임스페이스 범위
    public static class ProjectLog // Project J 공통 로그 형식과 등급 필터 담당 형식
    { // 클래스 범위
        private const string ProjectPrefix = "ProjectJ"; // 모든 프로젝트 로그 고정 접두사
        private const string EmptyMessage = "(no message)"; // 빈 로그 메시지 대체 문구

        public static ProjectLogLevel MinimumLevel { get; private set; } = ProjectLogLevel.Info; // 현재 최소 출력 로그 등급

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 새 런타임 시작 전 정적 로그 상태 초기화
        private static void ResetRuntimeState() // 최소 로그 등급 기본값 복원
        { // 메서드 범위
            MinimumLevel = ProjectLogLevel.Info; // 기본 정보 로그 등급 적용
        } // 메서드 범위

        public static void ConfigureMinimumLevel(ProjectLogLevel minimumLevel) // 최소 출력 로그 등급 변경
        { // 메서드 범위
            MinimumLevel = minimumLevel; // 전달된 최소 로그 등급 저장
        } // 메서드 범위

        public static string Format(ProjectLogCategory category, string message, string code = null) // 분류와 선택 코드가 포함된 공통 로그 문자열 생성
        { // 메서드 범위
            string normalizedMessage = NormalizeMessage(message); // 로그 메시지 공백 제거와 빈 값 보정
            string normalizedCode = NormalizeCode(code); // 선택적 로그 코드 형식 정규화

            if (string.IsNullOrEmpty(normalizedCode)) // 로그 코드 사용 여부 확인
            { // 조건 범위
                return $"[{ProjectPrefix}][{category}] {normalizedMessage}"; // 코드 없는 공통 로그 문자열 반환
            } // 조건 범위

            return $"[{ProjectPrefix}][{category}][{normalizedCode}] {normalizedMessage}"; // 코드 포함 공통 로그 문자열 반환
        } // 메서드 범위

        public static void Verbose(ProjectLogCategory category, string message, string code = null, Object context = null) // 상세 개발 로그 출력
        { // 메서드 범위
            if (!ShouldWrite(ProjectLogLevel.Verbose)) // 상세 로그 출력 허용 여부 확인
            { // 조건 범위
                return; // 상세 로그 출력 생략
            } // 조건 범위

            WriteLog(Format(category, message, code), context); // 공통 형식 상세 로그 출력
        } // 메서드 범위

        public static void Info(ProjectLogCategory category, string message, string code = null, Object context = null) // 정상 흐름 정보 로그 출력
        { // 메서드 범위
            if (!ShouldWrite(ProjectLogLevel.Info)) // 정보 로그 출력 허용 여부 확인
            { // 조건 범위
                return; // 정보 로그 출력 생략
            } // 조건 범위

            WriteLog(Format(category, message, code), context); // 공통 형식 정보 로그 출력
        } // 메서드 범위

        public static void Warning(ProjectLogCategory category, string message, string code = null, Object context = null) // 복구 가능한 경고 로그 출력
        { // 메서드 범위
            if (!ShouldWrite(ProjectLogLevel.Warning)) // 경고 로그 출력 허용 여부 확인
            { // 조건 범위
                return; // 경고 로그 출력 생략
            } // 조건 범위

            string formattedMessage = Format(category, message, code); // 공통 형식 경고 문자열 생성

            if (context == null) // Unity 문맥 사용 여부 확인
            { // 조건 범위
                Debug.LogWarning(formattedMessage); // 문맥 없는 경고 로그 출력
                return; // 문맥 로그 처리 생략
            } // 조건 범위

            Debug.LogWarning(formattedMessage, context); // 관련 Unity 문맥 포함 경고 로그 출력
        } // 메서드 범위

        public static void Error(ProjectLogCategory category, string message, string code = null, Object context = null) // 기능 진행 불가 오류 로그 출력
        { // 메서드 범위
            if (!ShouldWrite(ProjectLogLevel.Error)) // 오류 로그 출력 허용 여부 확인
            { // 조건 범위
                return; // 오류 로그 출력 생략
            } // 조건 범위

            string formattedMessage = Format(category, message, code); // 공통 형식 오류 문자열 생성

            if (context == null) // Unity 문맥 사용 여부 확인
            { // 조건 범위
                Debug.LogError(formattedMessage); // 문맥 없는 오류 로그 출력
                return; // 문맥 로그 처리 생략
            } // 조건 범위

            Debug.LogError(formattedMessage, context); // 관련 Unity 문맥 포함 오류 로그 출력
        } // 메서드 범위

        private static bool ShouldWrite(ProjectLogLevel messageLevel) // 현재 최소 등급 기준 출력 여부 확인
        { // 메서드 범위
            return MinimumLevel != ProjectLogLevel.Off && messageLevel >= MinimumLevel; // 로그 활성 상태와 등급 충족 여부 반환
        } // 메서드 범위

        private static void WriteLog(string formattedMessage, Object context) // 일반 Unity 로그 출력 통합
        { // 메서드 범위
            if (context == null) // Unity 문맥 사용 여부 확인
            { // 조건 범위
                Debug.Log(formattedMessage); // 문맥 없는 일반 로그 출력
                return; // 문맥 로그 처리 생략
            } // 조건 범위

            Debug.Log(formattedMessage, context); // 관련 Unity 문맥 포함 일반 로그 출력
        } // 메서드 범위

        private static string NormalizeMessage(string message) // 로그 메시지 공백 제거와 빈 값 보정
        { // 메서드 범위
            if (string.IsNullOrWhiteSpace(message)) // 로그 메시지 누락 여부 확인
            { // 조건 범위
                return EmptyMessage; // 빈 로그 메시지 대체 문구 반환
            } // 조건 범위

            return message.Trim(); // 앞뒤 공백 제거 메시지 반환
        } // 메서드 범위

        private static string NormalizeCode(string code) // 로그 코드를 대문자 밑줄 형식으로 정규화
        { // 메서드 범위
            if (string.IsNullOrWhiteSpace(code)) // 로그 코드 누락 여부 확인
            { // 조건 범위
                return string.Empty; // 코드 없는 로그 형식용 빈 문자열 반환
            } // 조건 범위

            string trimmedCode = code.Trim(); // 로그 코드 앞뒤 공백 제거
            string underscoredCode = trimmedCode.Replace(' ', '_'); // 코드 내부 공백을 밑줄로 변경
            return underscoredCode.ToUpperInvariant(); // 문화권 무관 대문자 로그 코드 반환
        } // 메서드 범위
    } // 클래스 범위
} // 네임스페이스 범위
