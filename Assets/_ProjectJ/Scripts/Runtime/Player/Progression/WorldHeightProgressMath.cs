using UnityEngine; // Unity 수학 기능 참조

namespace ProjectJ.Player // 플레이어 진행 기능 네임스페이스 선언
{ // 월드 높이 계산 범위
    public static class WorldHeightProgressMath // 월드 높이와 수직 구간 계산 도구 선언
    { // 월드 높이 계산 기능 범위
        private const float MinimumCourseHeight = 0.001f; // 전체 코스 최소 높이
        private const float MinimumMetersPerUnit = 0.001f; // 유니티 단위당 최소 미터

        public static float CalculateHeight(float worldY, float originY, float metersPerUnityUnit) // 월드 Y 좌표를 높이 미터로 변환
        { // 월드 높이 계산 범위
            float validMetersPerUnit = Mathf.Max(MinimumMetersPerUnit, metersPerUnityUnit); // 유효한 미터 변환 비율 계산
            return Mathf.Max(0f, (worldY - originY) * validMetersPerUnit); // 기준점 아래를 제외한 높이 반환
        } // 월드 높이 계산 범위 종료

        public static float CalculateHighestHeight(float previousHighestHeight, float currentHeight) // 기존 기록과 현재 높이에서 최고 높이 계산
        { // 최고 높이 계산 범위
            return Mathf.Max(0f, Mathf.Max(previousHighestHeight, currentHeight)); // 음수가 없는 최고 높이 반환
        } // 최고 높이 계산 범위 종료

        public static int CalculateSectionIndex(float currentHeight, float totalCourseHeight, int sectionCount) // 현재 높이가 속한 수직 구간 번호 계산
        { // 수직 구간 번호 계산 범위
            int validSectionCount = Mathf.Max(1, sectionCount); // 최소 한 개의 구간 수 보장
            float validCourseHeight = Mathf.Max(MinimumCourseHeight, totalCourseHeight); // 유효한 전체 코스 높이 계산
            float normalizedHeight = Mathf.Clamp01(Mathf.Max(0f, currentHeight) / validCourseHeight); // 전체 코스 기준 높이 비율 계산
            int calculatedSection = Mathf.FloorToInt(normalizedHeight * validSectionCount) + 1; // 1부터 시작하는 구간 번호 계산
            return Mathf.Clamp(calculatedSection, 1, validSectionCount); // 마지막 구간을 넘지 않는 번호 반환
        } // 수직 구간 번호 계산 범위 종료

        public static float CalculateCourseProgress01(float currentHeight, float totalCourseHeight) // 전체 코스 진행 비율 계산
        { // 전체 진행 비율 계산 범위
            float validCourseHeight = Mathf.Max(MinimumCourseHeight, totalCourseHeight); // 유효한 전체 코스 높이 계산
            return Mathf.Clamp01(Mathf.Max(0f, currentHeight) / validCourseHeight); // 0부터 1 사이 전체 진행 비율 반환
        } // 전체 진행 비율 계산 범위 종료

        public static float CalculateSectionHeight(float totalCourseHeight, int sectionCount) // 한 구간의 높이 계산
        { // 구간 높이 계산 범위
            float validCourseHeight = Mathf.Max(MinimumCourseHeight, totalCourseHeight); // 유효한 전체 코스 높이 계산
            int validSectionCount = Mathf.Max(1, sectionCount); // 최소 한 개의 구간 수 보장
            return validCourseHeight / validSectionCount; // 균등 분할된 한 구간 높이 반환
        } // 구간 높이 계산 범위 종료

        public static float CalculateSectionStartHeight(int sectionIndex, float totalCourseHeight, int sectionCount) // 지정 구간의 시작 높이 계산
        { // 구간 시작 높이 계산 범위
            int validSectionCount = Mathf.Max(1, sectionCount); // 최소 한 개의 구간 수 보장
            int validSectionIndex = Mathf.Clamp(sectionIndex, 1, validSectionCount); // 유효한 구간 번호 계산
            return CalculateSectionHeight(totalCourseHeight, validSectionCount) * (validSectionIndex - 1); // 구간 시작 높이 반환
        } // 구간 시작 높이 계산 범위 종료

        public static float CalculateSectionEndHeight(int sectionIndex, float totalCourseHeight, int sectionCount) // 지정 구간의 종료 높이 계산
        { // 구간 종료 높이 계산 범위
            int validSectionCount = Mathf.Max(1, sectionCount); // 최소 한 개의 구간 수 보장
            int validSectionIndex = Mathf.Clamp(sectionIndex, 1, validSectionCount); // 유효한 구간 번호 계산
            return CalculateSectionHeight(totalCourseHeight, validSectionCount) * validSectionIndex; // 구간 종료 높이 반환
        } // 구간 종료 높이 계산 범위 종료

        public static float CalculateSectionProgress01(float currentHeight, float totalCourseHeight, int sectionCount) // 현재 구간 내부 진행 비율 계산
        { // 구간 내부 진행 비율 계산 범위
            int currentSectionIndex = CalculateSectionIndex(currentHeight, totalCourseHeight, sectionCount); // 현재 구간 번호 계산
            float sectionStartHeight = CalculateSectionStartHeight(currentSectionIndex, totalCourseHeight, sectionCount); // 현재 구간 시작 높이 계산
            float sectionHeight = CalculateSectionHeight(totalCourseHeight, sectionCount); // 한 구간 높이 계산
            return Mathf.Clamp01((Mathf.Max(0f, currentHeight) - sectionStartHeight) / sectionHeight); // 0부터 1 사이 구간 진행 비율 반환
        } // 구간 내부 진행 비율 계산 범위 종료

        public static bool HasReachedCourseTop(float currentHeight, float totalCourseHeight) // 전체 코스 정상 도달 여부 계산
        { // 정상 도달 판정 범위
            float validCourseHeight = Mathf.Max(MinimumCourseHeight, totalCourseHeight); // 유효한 전체 코스 높이 계산
            return currentHeight >= validCourseHeight; // 전체 높이 이상 도달 결과 반환
        } // 정상 도달 판정 범위 종료
    } // 월드 높이 계산 기능 범위 종료
} // 월드 높이 계산 범위 종료
