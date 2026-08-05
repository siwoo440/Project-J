using System; // 문자열 비교와 예외 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    public static class ProjectDataIdRules // 프로젝트 공통 데이터 ID 규칙 관리 형식 선언
    {
        public const int NumericLength = 3; // ID 숫자 부분 고정 길이 선언
        public const int MinimumNumber = 1; // ID 숫자 부분 최소값 선언
        public const int MaximumNumber = 999; // ID 숫자 부분 최대값 선언

        public static string GetPrefix(ProjectDataCategory category) // 데이터 분류에 대응하는 ID 접두사 반환
        {
            switch (category) // 전달된 데이터 분류 분기
            {
                case ProjectDataCategory.Player: // 플레이어 데이터 분류 처리
                    return "PLY"; // 플레이어 ID 접두사 반환

                case ProjectDataCategory.Map: // 맵 데이터 분류 처리
                    return "MAP"; // 맵 ID 접두사 반환

                case ProjectDataCategory.Obstacle: // 장애물 데이터 분류 처리
                    return "OBS"; // 장애물 ID 접두사 반환

                case ProjectDataCategory.Item: // 아이템 데이터 분류 처리
                    return "ITM"; // 아이템 ID 접두사 반환

                case ProjectDataCategory.Cosmetic: // 꾸미기 데이터 분류 처리
                    return "COS"; // 꾸미기 ID 접두사 반환

                case ProjectDataCategory.Audio: // 오디오 데이터 분류 처리
                    return "AUD"; // 오디오 ID 접두사 반환

                default: // 정의되지 않은 데이터 분류 처리
                    throw new ArgumentOutOfRangeException(nameof(category), category, "정의되지 않은 데이터 분류입니다."); // 잘못된 데이터 분류 예외 발생
            }
        }

        public static string Create(ProjectDataCategory category, int number) // 데이터 분류와 번호로 ID 생성
        {
            if (number < MinimumNumber || number > MaximumNumber) // ID 번호가 허용 범위를 벗어났는지 확인
            {
                throw new ArgumentOutOfRangeException(nameof(number), number, $"{MinimumNumber}부터 {MaximumNumber} 사이의 번호만 사용할 수 있습니다."); // 잘못된 ID 번호 예외 발생
            }

            string prefix = GetPrefix(category); // 데이터 분류 접두사 조회
            return $"{prefix}-{number:000}"; // 접두사와 세 자리 번호를 조합한 ID 반환
        }

        public static bool IsValid(string dataId, ProjectDataCategory category, out string reason) // 데이터 ID 형식과 분류 일치 여부 검사
        {
            if (string.IsNullOrWhiteSpace(dataId)) // 데이터 ID가 비어 있는지 확인
            {
                reason = "데이터 ID가 비어 있습니다."; // ID 누락 사유 저장
                return false; // ID 검사 실패 반환
            }

            if (!string.Equals(dataId, dataId.Trim(), StringComparison.Ordinal)) // 데이터 ID 앞뒤에 공백이 있는지 확인
            {
                reason = "데이터 ID 앞뒤에 공백을 사용할 수 없습니다."; // 공백 사용 오류 사유 저장
                return false; // ID 검사 실패 반환
            }

            string expectedPrefix = GetPrefix(category); // 데이터 분류에 필요한 접두사 조회
            int expectedLength = expectedPrefix.Length + 1 + NumericLength; // 접두사와 구분자와 숫자를 포함한 전체 길이 계산

            if (dataId.Length != expectedLength) // 데이터 ID 전체 길이 일치 여부 확인
            {
                reason = $"데이터 ID는 {expectedPrefix}-001 형식이어야 합니다."; // 전체 형식 오류 사유 저장
                return false; // ID 검사 실패 반환
            }

            if (!dataId.StartsWith(expectedPrefix + "-", StringComparison.Ordinal)) // 데이터 ID 접두사와 하이픈 일치 여부 확인
            {
                reason = $"{category} 데이터 ID는 {expectedPrefix}- 접두사를 사용해야 합니다."; // 접두사 불일치 사유 저장
                return false; // ID 검사 실패 반환
            }

            int numberStartIndex = expectedPrefix.Length + 1; // 숫자 부분 시작 위치 계산
            int number = 0; // 숫자 부분 변환 결과 저장

            for (int index = numberStartIndex; index < dataId.Length; index++) // 데이터 ID 숫자 부분 순회
            {
                char currentCharacter = dataId[index]; // 현재 숫자 후보 문자 조회

                if (currentCharacter < '0' || currentCharacter > '9') // 현재 문자가 숫자인지 확인
                {
                    reason = "데이터 ID의 마지막 세 글자는 숫자여야 합니다."; // 숫자 형식 오류 사유 저장
                    return false; // ID 검사 실패 반환
                }

                number = number * 10 + currentCharacter - '0'; // 현재 문자를 누적 숫자 값으로 변환
            }

            if (number < MinimumNumber || number > MaximumNumber) // 변환된 번호가 허용 범위인지 확인
            {
                reason = $"데이터 ID 번호는 {MinimumNumber:000}부터 {MaximumNumber:000}까지 사용할 수 있습니다."; // 숫자 범위 오류 사유 저장
                return false; // ID 검사 실패 반환
            }

            reason = string.Empty; // 검사 성공 결과로 오류 사유 초기화
            return true; // ID 검사 성공 반환
        }
    }
}
