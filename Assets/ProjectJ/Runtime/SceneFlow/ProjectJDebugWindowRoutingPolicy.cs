using System; // 문자열 비교 기능 사용

namespace ProjectJ.Debugging // Debug Window 공통 정책 네임스페이스
{
    public static class ProjectJDebugWindowRoutingPolicy // Debug Window 단축키 분리 정책
    {
        private const string Day79NetworkConditionViewTypeName = // F6 전용 화면 타입 이름
            "ProjectJDay79NetworkConditionDebugView"; // Day79 네트워크 진단 화면 지정

        public static bool UsesDedicatedHotkey( // 전용 단축키 사용 여부 판정
            string typeName // 검사할 전체 타입 이름
        )
        {
            if (string.IsNullOrEmpty(typeName)) // 유효하지 않은 타입 이름 확인
            {
                return false; // 일반 F1·F2 관리 정책 유지
            }

            return typeName.IndexOf( // Day79 타입 이름 포함 여부 검색
                Day79NetworkConditionViewTypeName, // 찾을 F6 전용 타입 이름
                StringComparison.Ordinal // 정확한 서수 비교 적용
            ) >= 0; // 포함된 경우 전용 단축키 사용
        }
    }
}
