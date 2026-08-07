using System.Collections.Generic; // 읽기 전용 모듈 목록 기능 참조
using UnityEngine; // Unity 수학과 컴포넌트 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    public static class MapVerticalGenerationRules // 수직 생성 목표와 후보 선택 규칙 선언
    { // 수직 생성 규칙 묶음
        public const float HeightEpsilon = 0.01f; // 높이 비교 허용 오차

        public static float GetHeightGain(MapModuleDefinition module) // 모듈의 예상 상승량 조회
        { // 모듈 상승량 조회 처리
            if (module == null) // 모듈 누락 확인
            { // 모듈 누락 처리
                return 0f; // 높이 변화 없음 반환
            } // 모듈 누락 처리 종료

            MapVerticalModuleData verticalData = module.GetComponent<MapVerticalModuleData>(); // 같은 오브젝트의 수직 데이터 조회
            return verticalData == null ? 0f : verticalData.ExpectedHeightGain; // 수직 데이터 기반 상승량 반환
        } // 모듈 상승량 조회 처리 종료

        public static bool IsAscending(MapModuleDefinition module) // 모듈의 상승 여부 확인
        { // 모듈 상승 여부 확인 처리
            return GetHeightGain(module) > HeightEpsilon; // 허용 오차보다 큰 상승량 여부 반환
        } // 모듈 상승 여부 확인 처리 종료

        public static float GetMaximumHeightGain(IReadOnlyList<MapModuleDefinition> modules) // 후보 중 최대 상승량 계산
        { // 최대 상승량 계산 처리
            float maximumHeightGain = 0f; // 최대 상승량 초기화

            if (modules == null) // 후보 목록 누락 확인
            { // 후보 목록 누락 처리
                return maximumHeightGain; // 0미터 최대 상승량 반환
            } // 후보 목록 누락 처리 종료

            for (int moduleIndex = 0; moduleIndex < modules.Count; moduleIndex++) // 모든 후보 모듈 순회
            { // 후보 상승량 비교 처리
                maximumHeightGain = Mathf.Max(maximumHeightGain, GetHeightGain(modules[moduleIndex])); // 현재 최대 상승량 갱신
            } // 후보 상승량 비교 처리 종료

            return maximumHeightGain; // 계산된 최대 상승량 반환
        } // 최대 상승량 계산 처리 종료

        public static bool TryValidateConfiguration(int moduleCount, float minimumTargetHeight, int minimumAscendingModules, int maximumConsecutiveFlatModules, float maximumAvailableHeightGain, out string reason) // 수직 생성 설정의 목표 달성 가능성 검사
        { // 수직 생성 설정 검사 처리
            if (moduleCount <= 0) // 전체 모듈 수 오류 확인
            { // 전체 모듈 수 오류 처리
                reason = "Module Count는 1 이상이어야 합니다."; // 모듈 수 오류 사유 저장
                return false; // 설정 검사 실패 반환
            } // 전체 모듈 수 오류 처리 종료

            if (minimumAscendingModules < 0 || minimumAscendingModules > moduleCount) // 최소 상승 모듈 수 범위 확인
            { // 최소 상승 모듈 수 오류 처리
                reason = "Minimum Ascending Modules가 전체 모듈 수 범위를 벗어났습니다."; // 상승 모듈 수 오류 사유 저장
                return false; // 설정 검사 실패 반환
            } // 최소 상승 모듈 수 오류 처리 종료

            if (maximumConsecutiveFlatModules < 0) // 연속 평지 제한 오류 확인
            { // 연속 평지 제한 오류 처리
                reason = "Maximum Consecutive Flat Modules는 0 이상이어야 합니다."; // 연속 평지 오류 사유 저장
                return false; // 설정 검사 실패 반환
            } // 연속 평지 제한 오류 처리 종료

            if (minimumTargetHeight > HeightEpsilon && maximumAvailableHeightGain <= HeightEpsilon) // 상승 후보 부재 확인
            { // 상승 후보 부재 처리
                reason = "목표 높이를 달성할 상승 모듈 Prefab이 없습니다."; // 상승 후보 부재 사유 저장
                return false; // 설정 검사 실패 반환
            } // 상승 후보 부재 처리 종료

            if (maximumAvailableHeightGain * moduleCount + HeightEpsilon < minimumTargetHeight) // 전체 슬롯 최대 상승량 부족 확인
            { // 최대 상승량 부족 처리
                reason = "현재 모듈 수와 상승량으로 Minimum Target Height를 달성할 수 없습니다."; // 목표 달성 불가 사유 저장
                return false; // 설정 검사 실패 반환
            } // 최대 상승량 부족 처리 종료

            reason = string.Empty; // 정상 설정 사유 초기화
            return true; // 설정 검사 성공 반환
        } // 수직 생성 설정 검사 처리 종료

        public static bool IsCandidateFeasible(float currentHeight, int currentAscendingModules, int currentConsecutiveFlatModules, float candidateHeightGain, int remainingSlotsAfterCandidate, float targetHeight, int minimumAscendingModules, int maximumConsecutiveFlatModules, float maximumAvailableHeightGain, bool allowDescendingModules) // 현재 후보 선택 후 목표 달성 가능성 확인
        { // 후보 목표 달성 가능성 검사 처리
            if (!allowDescendingModules && candidateHeightGain < -HeightEpsilon) // 하강 후보 금지 확인
            { // 하강 후보 금지 처리
                return false; // 하강 후보 제외 반환
            } // 하강 후보 금지 처리 종료

            bool isAscending = candidateHeightGain > HeightEpsilon; // 현재 후보 상승 여부 계산
            bool isFlat = Mathf.Abs(candidateHeightGain) <= HeightEpsilon; // 현재 후보 평지 여부 계산
            int nextAscendingModules = currentAscendingModules + (isAscending ? 1 : 0); // 후보 선택 후 상승 모듈 수 계산
            int nextConsecutiveFlatModules = isFlat ? currentConsecutiveFlatModules + 1 : 0; // 후보 선택 후 연속 평지 수 계산
            float nextHeight = currentHeight + candidateHeightGain; // 후보 선택 후 누적 높이 계산

            if (isFlat && nextConsecutiveFlatModules > maximumConsecutiveFlatModules) // 연속 평지 제한 초과 확인
            { // 연속 평지 제한 초과 처리
                return false; // 평지 후보 제외 반환
            } // 연속 평지 제한 초과 처리 종료

            if (nextAscendingModules + remainingSlotsAfterCandidate < minimumAscendingModules) // 남은 슬롯 포함 상승 모듈 수 부족 확인
            { // 상승 모듈 수 부족 처리
                return false; // 목표 달성 불가 후보 제외 반환
            } // 상승 모듈 수 부족 처리 종료

            float maximumReachableHeight = nextHeight + maximumAvailableHeightGain * remainingSlotsAfterCandidate; // 후보 선택 후 도달 가능한 최대 높이 계산

            if (maximumReachableHeight + HeightEpsilon < targetHeight) // 남은 슬롯 포함 목표 높이 도달 불가 확인
            { // 목표 높이 도달 불가 처리
                return false; // 목표 달성 불가 후보 제외 반환
            } // 목표 높이 도달 불가 처리 종료

            if (remainingSlotsAfterCandidate == 0 && nextHeight + HeightEpsilon < targetHeight) // 마지막 슬롯 목표 높이 미달 확인
            { // 마지막 슬롯 목표 높이 미달 처리
                return false; // 마지막 후보 제외 반환
            } // 마지막 슬롯 목표 높이 미달 처리 종료

            return true; // 목표 달성 가능한 후보 반환
        } // 후보 목표 달성 가능성 검사 처리 종료

        public static bool TryValidateResult(float generatedHeight, float targetHeight, int ascendingModuleCount, int minimumAscendingModules, int maximumObservedConsecutiveFlatModules, int maximumConsecutiveFlatModules, out string reason) // 수직 생성 최종 결과 검사
        { // 수직 생성 최종 결과 검사 처리
            if (generatedHeight + HeightEpsilon < targetHeight) // 최종 높이 목표 미달 확인
            { // 최종 높이 목표 미달 처리
                reason = $"최종 높이 {generatedHeight:0.00}m가 목표 높이 {targetHeight:0.00}m보다 낮습니다."; // 목표 높이 미달 사유 저장
                return false; // 결과 검사 실패 반환
            } // 최종 높이 목표 미달 처리 종료

            if (ascendingModuleCount < minimumAscendingModules) // 최종 상승 모듈 수 부족 확인
            { // 최종 상승 모듈 수 부족 처리
                reason = $"상승 모듈 {ascendingModuleCount}개가 최소 기준 {minimumAscendingModules}개보다 적습니다."; // 상승 모듈 수 미달 사유 저장
                return false; // 결과 검사 실패 반환
            } // 최종 상승 모듈 수 부족 처리 종료

            if (maximumObservedConsecutiveFlatModules > maximumConsecutiveFlatModules) // 최종 연속 평지 수 초과 확인
            { // 최종 연속 평지 수 초과 처리
                reason = $"연속 평지 모듈 {maximumObservedConsecutiveFlatModules}개가 제한 {maximumConsecutiveFlatModules}개를 초과했습니다."; // 연속 평지 초과 사유 저장
                return false; // 결과 검사 실패 반환
            } // 최종 연속 평지 수 초과 처리 종료

            reason = string.Empty; // 정상 결과 사유 초기화
            return true; // 결과 검사 성공 반환
        } // 수직 생성 최종 결과 검사 처리 종료
    } // 수직 생성 규칙 묶음 종료
} // 맵 생성 기능 묶음 종료
