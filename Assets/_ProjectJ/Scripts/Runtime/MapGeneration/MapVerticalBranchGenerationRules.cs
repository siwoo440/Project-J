using UnityEngine; // Unity 수학 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    public static class MapVerticalBranchGenerationRules // 수직 분기 후보와 합류 규칙 선언
    { // 수직 분기 규칙 묶음
        public static int CalculateRouteOrdinarySlotCount(int moduleCount, int branchPairCount) // 시작부터 종료까지 단일 경로의 일반 모듈 슬롯 계산
        { // 경로 일반 슬롯 계산 처리
            int safeModuleCount = Mathf.Max(0, moduleCount); // 음수가 없는 전체 모듈 수 계산
            int safePairCount = Mathf.Max(0, branchPairCount); // 음수가 없는 병렬 단계 수 계산
            int fixedStructureCount = 3 + safePairCount * 2; // 시작·분기·병렬 쌍·합류 기본 구조 수 계산
            int sharedTailCount = Mathf.Max(0, safeModuleCount - fixedStructureCount); // 합류 뒤 공통 모듈 수 계산
            return 1 + safePairCount + sharedTailCount; // 시작·단일 분기·공통 후속 슬롯 합계 반환
        } // 경로 일반 슬롯 계산 처리 종료

        public static bool AreHeightGainsCompatible(float leftHeightGain, float rightHeightGain, float tolerance) // 좌우 후보 상승량 호환성 검사
        { // 좌우 상승량 호환성 검사 처리
            float safeTolerance = Mathf.Max(0f, tolerance); // 음수가 없는 높이 허용 오차 계산
            return Mathf.Abs(leftHeightGain - rightHeightGain) <= safeTolerance; // 좌우 상승량 차이 허용 여부 반환
        } // 좌우 상승량 호환성 검사 처리 종료

        public static bool IsBranchPairFeasible(float sharedHeight, int sharedAscendingModules, float leftBranchHeight, int leftAscendingModules, float rightBranchHeight, int rightAscendingModules, float leftCandidateHeightGain, float rightCandidateHeightGain, int remainingBranchSlotsAfterCandidate, int remainingSharedSlots, float targetHeight, int minimumAscendingModules, int minimumAscendingModulesPerBranch, float maximumAvailableHeightGain, float mergeHeightTolerance, bool allowDescendingModules) // 좌우 후보 선택 뒤 최종 목표와 합류 달성 가능성 검사
        { // 수직 분기 후보 가능성 검사 처리
            if (!AreHeightGainsCompatible(leftCandidateHeightGain, rightCandidateHeightGain, mergeHeightTolerance)) // 후보 상승량 불일치 확인
            { // 후보 상승량 불일치 처리
                return false; // 합류 불가 후보 제외 반환
            } // 후보 상승량 불일치 처리 종료

            if (!allowDescendingModules && (leftCandidateHeightGain < -MapVerticalGenerationRules.HeightEpsilon || rightCandidateHeightGain < -MapVerticalGenerationRules.HeightEpsilon)) // 하강 후보 금지 확인
            { // 하강 후보 금지 처리
                return false; // 하강 후보 제외 반환
            } // 하강 후보 금지 처리 종료

            float nextLeftHeight = leftBranchHeight + leftCandidateHeightGain; // 후보 선택 뒤 왼쪽 분기 높이 계산
            float nextRightHeight = rightBranchHeight + rightCandidateHeightGain; // 후보 선택 뒤 오른쪽 분기 높이 계산

            if (!AreHeightGainsCompatible(nextLeftHeight, nextRightHeight, mergeHeightTolerance)) // 누적 분기 높이 불일치 확인
            { // 누적 분기 높이 불일치 처리
                return false; // 합류 높이 불일치 후보 제외 반환
            } // 누적 분기 높이 불일치 처리 종료

            bool leftAscending = leftCandidateHeightGain > MapVerticalGenerationRules.HeightEpsilon; // 왼쪽 후보 상승 여부 계산
            bool rightAscending = rightCandidateHeightGain > MapVerticalGenerationRules.HeightEpsilon; // 오른쪽 후보 상승 여부 계산
            int nextLeftAscendingModules = leftAscendingModules + (leftAscending ? 1 : 0); // 후보 선택 뒤 왼쪽 상승 수 계산
            int nextRightAscendingModules = rightAscendingModules + (rightAscending ? 1 : 0); // 후보 선택 뒤 오른쪽 상승 수 계산

            if (nextLeftAscendingModules + remainingBranchSlotsAfterCandidate < minimumAscendingModulesPerBranch) // 왼쪽 분기 최소 상승 수 달성 불가 확인
            { // 왼쪽 분기 상승 수 부족 처리
                return false; // 왼쪽 목표 불가 후보 제외 반환
            } // 왼쪽 분기 상승 수 부족 처리 종료

            if (nextRightAscendingModules + remainingBranchSlotsAfterCandidate < minimumAscendingModulesPerBranch) // 오른쪽 분기 최소 상승 수 달성 불가 확인
            { // 오른쪽 분기 상승 수 부족 처리
                return false; // 오른쪽 목표 불가 후보 제외 반환
            } // 오른쪽 분기 상승 수 부족 처리 종료

            int remainingRouteSlots = remainingBranchSlotsAfterCandidate + remainingSharedSlots; // 후보 뒤 단일 경로 남은 상승 가능 슬롯 계산
            int leftRouteAscendingModules = sharedAscendingModules + nextLeftAscendingModules; // 현재 왼쪽 전체 경로 상승 수 계산
            int rightRouteAscendingModules = sharedAscendingModules + nextRightAscendingModules; // 현재 오른쪽 전체 경로 상승 수 계산

            if (leftRouteAscendingModules + remainingRouteSlots < minimumAscendingModules || rightRouteAscendingModules + remainingRouteSlots < minimumAscendingModules) // 좌우 전체 경로 최소 상승 수 달성 불가 확인
            { // 전체 경로 상승 수 부족 처리
                return false; // 전체 경로 목표 불가 후보 제외 반환
            } // 전체 경로 상승 수 부족 처리 종료

            float maximumRemainingGain = maximumAvailableHeightGain * remainingRouteSlots; // 남은 슬롯의 최대 상승량 계산
            float maximumLeftHeight = sharedHeight + nextLeftHeight + maximumRemainingGain; // 왼쪽 경로 최대 최종 높이 계산
            float maximumRightHeight = sharedHeight + nextRightHeight + maximumRemainingGain; // 오른쪽 경로 최대 최종 높이 계산
            return maximumLeftHeight + MapVerticalGenerationRules.HeightEpsilon >= targetHeight && maximumRightHeight + MapVerticalGenerationRules.HeightEpsilon >= targetHeight; // 좌우 목표 높이 달성 가능 여부 반환
        } // 수직 분기 후보 가능성 검사 처리 종료

        public static bool TryValidateMerge(float leftBranchHeight, float rightBranchHeight, int leftAscendingModules, int rightAscendingModules, int minimumAscendingModulesPerBranch, float mergeHeightTolerance, out string reason) // 좌우 분기 합류 조건 검사
        { // 좌우 분기 합류 검사 처리
            if (!AreHeightGainsCompatible(leftBranchHeight, rightBranchHeight, mergeHeightTolerance)) // 좌우 누적 높이 불일치 확인
            { // 좌우 누적 높이 불일치 처리
                reason = $"왼쪽 {leftBranchHeight:0.00}m와 오른쪽 {rightBranchHeight:0.00}m의 합류 높이가 다릅니다."; // 합류 높이 오류 사유 저장
                return false; // 합류 검사 실패 반환
            } // 좌우 누적 높이 불일치 처리 종료

            if (leftAscendingModules < minimumAscendingModulesPerBranch) // 왼쪽 최소 상승 수 미달 확인
            { // 왼쪽 최소 상승 수 미달 처리
                reason = $"왼쪽 분기 상승 모듈 {leftAscendingModules}개가 최소 기준 {minimumAscendingModulesPerBranch}개보다 적습니다."; // 왼쪽 상승 수 오류 사유 저장
                return false; // 합류 검사 실패 반환
            } // 왼쪽 최소 상승 수 미달 처리 종료

            if (rightAscendingModules < minimumAscendingModulesPerBranch) // 오른쪽 최소 상승 수 미달 확인
            { // 오른쪽 최소 상승 수 미달 처리
                reason = $"오른쪽 분기 상승 모듈 {rightAscendingModules}개가 최소 기준 {minimumAscendingModulesPerBranch}개보다 적습니다."; // 오른쪽 상승 수 오류 사유 저장
                return false; // 합류 검사 실패 반환
            } // 오른쪽 최소 상승 수 미달 처리 종료

            reason = string.Empty; // 정상 합류 사유 초기화
            return true; // 합류 검사 성공 반환
        } // 좌우 분기 합류 검사 처리 종료
    } // 수직 분기 규칙 묶음 종료
} // 맵 생성 기능 묶음 종료
