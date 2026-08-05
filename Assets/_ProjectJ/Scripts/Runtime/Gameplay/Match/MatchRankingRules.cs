using UnityEngine; // Unity 수학 기능 참조

namespace ProjectJ.Gameplay // 경기 기능 네임스페이스 선언
{ // 경기 기능 묶음
    public static class MatchRankingRules // 실시간과 최종 순위 규칙 선언
    { // 순위 규칙 묶음
        public const float DefaultHeightTolerance = 0.05f; // 기본 공동 높이 허용 오차

        public static float ClampHeightTolerance(float heightTolerance) // 공동 높이 허용 오차 보정
        { // 허용 오차 보정 처리
            return Mathf.Max(0f, heightTolerance); // 음수가 없는 허용 오차 반환
        } // 허용 오차 보정 종료

        public static bool IsSameHeight(float leftHeight, float rightHeight, float heightTolerance) // 두 참가자의 공동 높이 여부 판정
        { // 공동 높이 판정 처리
            float safeTolerance = ClampHeightTolerance(heightTolerance); // 안전한 높이 허용 오차 계산
            return Mathf.Abs(leftHeight - rightHeight) <= safeTolerance; // 허용 오차 안의 동일 높이 상태 반환
        } // 공동 높이 판정 종료

        public static bool AreTied(bool leftReachedCourseTop, float leftHeight, bool rightReachedCourseTop, float rightHeight, float heightTolerance) // 공동 순위 여부 판정
        { // 공동 순위 판정 처리
            if (leftReachedCourseTop && rightReachedCourseTop) // 두 참가자 정상 도달 확인
            { // 정상 공동 순위 처리
                return true; // 정상 도달 참가자 공동 순위 반환
            } // 정상 공동 순위 처리 종료

            if (leftReachedCourseTop != rightReachedCourseTop) // 한 참가자만 정상 도달 확인
            { // 정상 우선 순위 처리
                return false; // 서로 다른 순위 그룹 반환
            } // 정상 우선 순위 처리 종료

            return IsSameHeight(leftHeight, rightHeight, heightTolerance); // 실제 높이 기반 공동 순위 반환
        } // 공동 순위 판정 종료

        public static bool ShouldComeBefore(bool leftReachedCourseTop, float leftHeight, float leftReachedAt, int leftStableOrder, bool rightReachedCourseTop, float rightHeight, float rightReachedAt, int rightStableOrder, float heightTolerance) // 표시 순서 우선순위 판정
        { // 표시 순서 판정 처리
            if (leftReachedCourseTop != rightReachedCourseTop) // 정상 도달 여부 차이 확인
            { // 정상 도달 우선 처리
                return leftReachedCourseTop; // 정상 도달 참가자 우선 반환
            } // 정상 도달 우선 처리 종료

            if (!AreTied(leftReachedCourseTop, leftHeight, rightReachedCourseTop, rightHeight, heightTolerance)) // 공동 순위가 아닌 높이 확인
            { // 실제 높이 비교 처리
                return leftHeight > rightHeight; // 더 높은 참가자 우선 반환
            } // 실제 높이 비교 처리 종료

            if (!Mathf.Approximately(leftReachedAt, rightReachedAt)) // 공동 높이 도달 시간 차이 확인
            { // 도달 시간 비교 처리
                return leftReachedAt < rightReachedAt; // 먼저 도달한 참가자 우선 반환
            } // 도달 시간 비교 처리 종료

            return leftStableOrder < rightStableOrder; // 완전 동점의 등록 순서 우선 반환
        } // 표시 순서 판정 종료

        public static int CalculateCompetitionRank(int sortedIndex, int previousRank, bool previousReachedCourseTop, float previousHeight, bool currentReachedCourseTop, float currentHeight, float heightTolerance) // 정렬 위치 기반 공동 순위 계산
        { // 공동 순위 계산 처리
            if (sortedIndex <= 0) // 첫 번째 표시 항목 확인
            { // 첫 순위 처리
                return 1; // 첫 번째 참가자 1위 반환
            } // 첫 순위 처리 종료

            bool isTied = AreTied(previousReachedCourseTop, previousHeight, currentReachedCourseTop, currentHeight, heightTolerance); // 앞 참가자와 공동 순위 여부 계산
            return isTied ? Mathf.Max(1, previousRank) : sortedIndex + 1; // 공동 순위 유지 또는 건너뛴 다음 순위 반환
        } // 공동 순위 계산 종료
    } // 순위 규칙 묶음 종료
} // 경기 기능 묶음 종료
