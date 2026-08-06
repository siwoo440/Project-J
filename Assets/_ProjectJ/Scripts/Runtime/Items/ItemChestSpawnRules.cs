using UnityEngine; // Unity 수치 보정 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    public static class ItemChestSpawnRules // 아이템 상자 생성 공통 규칙 선언
    { // 아이템 상자 생성 공통 규칙 묶음
        public static float ClampProbability(float probability) // 생성 확률 안전 범위 보정
        { // 생성 확률 보정 처리
            return Mathf.Clamp01(probability); // 0부터 1 사이 확률 반환
        } // 생성 확률 보정 처리 종료

        public static bool ShouldSpawn(float randomValue, float probability) // 난수와 확률 기반 생성 여부 판정
        { // 생성 여부 판정 처리
            return Mathf.Clamp01(randomValue) < ClampProbability(probability); // 보정된 난수가 확률보다 작은지 반환
        } // 생성 여부 판정 처리 종료

        public static bool IsEligibleModuleIndex(int moduleIndex, int moduleCount) // 시작과 종료 모듈 제외 여부 판정
        { // 생성 대상 모듈 판정 처리
            return moduleCount >= 3 && moduleIndex > 0 && moduleIndex < moduleCount - 1; // 중간 모듈 포함 여부 반환
        } // 생성 대상 모듈 판정 처리 종료

        public static bool HasRequiredModuleGap(int previousModuleIndex, int currentModuleIndex, int minimumGap) // 직전 상자와 모듈 간격 충족 여부 판정
        { // 모듈 간격 판정 처리
            return previousModuleIndex < 0 || currentModuleIndex - previousModuleIndex >= Mathf.Max(1, minimumGap); // 최소 모듈 간격 충족 여부 반환
        } // 모듈 간격 판정 처리 종료

        public static bool CanRespawn(int completedRespawnCount, int maximumRespawnCount) // 상자 추가 재생성 가능 여부 판정
        { // 상자 재생성 횟수 판정 처리
            return completedRespawnCount < Mathf.Max(0, maximumRespawnCount); // 남은 재생성 횟수 여부 반환
        } // 상자 재생성 횟수 판정 처리 종료
    } // 아이템 상자 생성 공통 규칙 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
