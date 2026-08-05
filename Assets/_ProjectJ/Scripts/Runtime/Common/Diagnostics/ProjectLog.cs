using System; // 문자열 비교와 대문자 변환 기능 참조
using UnityEngine; // Unity Console 로그 기능 참조

namespace ProjectJ.Diagnostics // 프로젝트 공통 로그 네임스페이스 선언
{
    public static class ProjectLog // Project J 공통 로그 형식과 출력 담당 형식 선언
    {
        private const string ProjectPrefix = "ProjectJ"; // 모든 프로젝트 로그에 사용하는 고정 접두사 선언
        private const string EmptyMessage = "(no message)"; // 빈 로그 메시지 대체 문구 선언

        public static string Format(ProjectLogCategory category, string message, string code = null) // 분류와 선택적 코드가 포함된 공통 로그 문자열 생성
        {
            string normalizedMessage = NormalizeMessage(message); // 로그 메시지 앞뒤 공백 제거와 빈 값 보정
            string normalizedCode = NormalizeCode(code); // 선택적 로그 코드 형식 정규화

            if (string.IsNullOrEmpty(normalizedCode)) // 로그 코드 사용 여부 확인
            {
                return $"[{ProjectPrefix}][{category}] {normalizedMessage}"; // 코드가 없는 공통 로그 문자열 반환
            }

            return $"[{ProjectPrefix}][{category}][{normalizedCode}] {normalizedMessage}"; // 코드가 포함된 공통 로그 문자열 반환
        }

        public static void Info(ProjectLogCategory category, string message, string code = null, UnityEngine.Object context = null) // 일반 정보 로그 출력
        {
            string formattedMessage = Format(category, message, code); // 공통 규칙이 적용된 정보 로그 문자열 생성

            if (context == null) // Unity Object 문맥 사용 여부 확인
            {
                Debug.Log(formattedMessage); // 문맥이 없는 정보 로그 출력
                return; // 문맥 로그 처리 생략
            }

            Debug.Log(formattedMessage, context); // 관련 Unity Object 문맥이 포함된 정보 로그 출력
        }

        public static void Warning(ProjectLogCategory category, string message, string code = null, UnityEngine.Object context = null) // 경고 로그 출력
        {
            string formattedMessage = Format(category, message, code); // 공통 규칙이 적용된 경고 로그 문자열 생성

            if (context == null) // Unity Object 문맥 사용 여부 확인
            {
                Debug.LogWarning(formattedMessage); // 문맥이 없는 경고 로그 출력
                return; // 문맥 로그 처리 생략
            }

            Debug.LogWarning(formattedMessage, context); // 관련 Unity Object 문맥이 포함된 경고 로그 출력
        }

        public static void Error(ProjectLogCategory category, string message, string code = null, UnityEngine.Object context = null) // 오류 로그 출력
        {
            string formattedMessage = Format(category, message, code); // 공통 규칙이 적용된 오류 로그 문자열 생성

            if (context == null) // Unity Object 문맥 사용 여부 확인
            {
                Debug.LogError(formattedMessage); // 문맥이 없는 오류 로그 출력
                return; // 문맥 로그 처리 생략
            }

            Debug.LogError(formattedMessage, context); // 관련 Unity Object 문맥이 포함된 오류 로그 출력
        }

        private static string NormalizeMessage(string message) // 로그 메시지 앞뒤 공백 제거와 빈 값 보정
        {
            if (string.IsNullOrWhiteSpace(message)) // 로그 메시지 누락 여부 확인
            {
                return EmptyMessage; // 빈 로그 메시지 대체 문구 반환
            }

            return message.Trim(); // 앞뒤 공백이 제거된 로그 메시지 반환
        }

        private static string NormalizeCode(string code) // 로그 코드를 대문자 밑줄 형식으로 정규화
        {
            if (string.IsNullOrWhiteSpace(code)) // 로그 코드 누락 여부 확인
            {
                return string.Empty; // 코드가 없는 로그 형식 사용을 위한 빈 문자열 반환
            }

            string trimmedCode = code.Trim(); // 로그 코드 앞뒤 공백 제거
            string underscoredCode = trimmedCode.Replace(' ', '_'); // 코드 내부 공백을 밑줄로 변경
            return underscoredCode.ToUpperInvariant(); // 문화권과 무관한 대문자 로그 코드 반환
        }
    }
}
