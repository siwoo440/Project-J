namespace ProjectJ.Player
{
    public static class ProjectJPlayerVisualIndex
    {
        public static int Resolve(
            int stableIdentity,
            int candidateCount
        )
        {
            if (candidateCount <= 0)
            {
                return -1; // 선택 불가
            }

            long normalized =
                (long)stableIdentity % candidateCount; // 범위 정규화

            if (normalized < 0)
            {
                normalized += candidateCount; // 음수 보정
            }

            return (int)normalized; // 최종 인덱스
        }

        public static int StableHash(
            string value
        )
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0; // 빈 값 처리
            }

            unchecked
            {
                uint hash = 2166136261; // FNV-1a 시작값

                foreach (char character in value)
                {
                    hash ^= character; // 문자 반영
                    hash *= 16777619; // FNV-1a 곱셈
                }

                return (int)hash; // 안정 해시 반환
            }
        }

        public static int ResolveByName( // 꾸미기 이름 기반 외형 선택
            string requestedName, // 요청 외형 이름
            string defaultName, // 기본 외형 이름
            string[] candidateNames // 등록 외형 이름
        ) // 매개변수 종료
        { // 메서드 시작
            if (candidateNames == null || candidateNames.Length == 0) // 후보 누락 검사
            { // 후보 없음 분기 시작
                return -1; // 선택 불가 반환
            } // 후보 없음 분기 종료

            for (int index = 0; index < candidateNames.Length; index++) // 요청 이름 검색
            { // 반복 시작
                if (string.Equals( // 정확한 이름 비교
                    candidateNames[index], // 현재 후보 이름
                    requestedName, // 요청 외형 이름
                    System.StringComparison.Ordinal // 대소문자 구분 비교
                )) // 이름 일치 검사 종료
                { // 요청 이름 발견 분기 시작
                    return index; // 요청 외형 위치 반환
                } // 요청 이름 발견 분기 종료
            } // 요청 이름 검색 종료

            for (int index = 0; index < candidateNames.Length; index++) // 기본 이름 검색
            { // 반복 시작
                if (string.Equals( // 정확한 기본 이름 비교
                    candidateNames[index], // 현재 후보 이름
                    defaultName, // 기본 외형 이름
                    System.StringComparison.Ordinal // 대소문자 구분 비교
                )) // 기본 이름 일치 검사 종료
                { // 기본 이름 발견 분기 시작
                    return index; // 기본 외형 위치 반환
                } // 기본 이름 발견 분기 종료
            } // 기본 이름 검색 종료

            for (int index = 0; index < candidateNames.Length; index++) // 첫 유효 후보 검색
            { // 반복 시작
                if (!string.IsNullOrEmpty(candidateNames[index])) // 유효 이름 검사
                { // 유효 후보 발견 분기 시작
                    return index; // 첫 유효 외형 위치 반환
                } // 유효 후보 발견 분기 종료
            } // 첫 유효 후보 검색 종료

            return -1; // 전체 후보 누락 반환
        } // 메서드 종료
    }
}
