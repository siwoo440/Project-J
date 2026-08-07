using ProjectJ.Data; // 아이템 효과 종류 참조
using UnityEngine; // Unity 수치 보정 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    public static class P2ItemRules // P2 아이템 공통 수치와 판정 규칙 선언
    { // P2 아이템 공통 규칙 묶음
        public const float RewindPlaybackDuration = 1.25f; // 되감기 기본 재생 시간
        public const float RewindSampleInterval = 0.05f; // 안전 위치 기본 기록 간격
        public const int MaximumRetargetCount = 1; // 유도탄과 드론 최대 목표 재선정 횟수
        public const float HomingMissileSpeed = 16f; // 유도탄 기본 비행 속도
        public const float HomingMissileForce = 12f; // 유도탄 기본 밀치기 힘
        public const float HomingMissileLifeTime = 8f; // 유도탄 기본 유지 시간
        public const float HomingMissileRadius = 0.35f; // 유도탄 기본 적중 반경
        public const float DroneSpeed = 12f; // 드론 기본 비행 속도
        public const float DroneForce = 10f; // 드론 기본 밀치기 힘
        public const float DroneLifeTime = 12f; // 드론 기본 유지 시간
        public const float DroneRadius = 0.5f; // 드론 기본 적중 반경
        public const float InvisibilityAlpha = 0.2f; // 투명 망토 기본 표시 비율
        public const float SniperForce = 14f; // 저격 물총 기본 밀치기 힘
        public const float MinimumSniperZoom = 1.5f; // 저격 물총 최소 배율
        public const float MaximumSniperZoom = 4f; // 저격 물총 최대 배율
        public const float SniperZoomStep = 0.5f; // 저격 물총 배율 변경 단위
        public const float CartSpeed = 10f; // 카트 기본 자동 주행 속도
        public const float CartRouteSearchRange = 4f; // 카트 경로 기본 검색 거리

        public static bool IsP2Effect(ItemEffectType effectType) // P2 7종 효과 포함 여부 반환
        { // P2 효과 포함 판정 처리
            return effectType >= ItemEffectType.RewindClock && effectType <= ItemEffectType.Cart; // 연속된 P2 효과 범위 결과 반환
        } // P2 효과 포함 판정 처리 종료

        public static float CalculatePlaybackProgress(float elapsedTime, float playbackDuration) // 되감기 경과 시간 기반 진행률 계산
        { // 되감기 진행률 계산 처리
            return Mathf.Clamp01(Mathf.Max(0f, elapsedTime) / Mathf.Max(0.01f, playbackDuration)); // 0부터 1 사이 되감기 진행률 반환
        } // 되감기 진행률 계산 처리 종료

        public static int CalculateRewindSampleIndex(float progress, int sampleCount) // 되감기 진행률 기반 표본 번호 계산
        { // 되감기 표본 번호 계산 처리
            if (sampleCount <= 0) // 기록 표본 없음 여부 확인
            { // 빈 기록 처리
                return -1; // 유효하지 않은 표본 번호 반환
            } // 빈 기록 처리 종료

            return Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(progress) * sampleCount), 0, sampleCount - 1); // 진행률에 맞는 역순 표본 번호 반환
        } // 되감기 표본 번호 계산 처리 종료

        public static float TruncateHeightToTwoDecimals(float height) // 순위 기준 소수점 둘째 자리 높이 절삭
        { // 높이 절삭 처리
            return Mathf.Floor(height * 100f) / 100f; // 소수점 셋째 자리 이하를 버린 높이 반환
        } // 높이 절삭 처리 종료

        public static bool IsHigherPriorityTarget(float candidateHeight, float currentHeight, float candidateDistance, float currentDistance) // 드론 목표 높이와 거리 우선순위 비교
        { // 드론 목표 우선순위 비교 처리
            float truncatedCandidateHeight = TruncateHeightToTwoDecimals(candidateHeight); // 후보 높이 절삭
            float truncatedCurrentHeight = TruncateHeightToTwoDecimals(currentHeight); // 현재 목표 높이 절삭

            if (truncatedCandidateHeight > truncatedCurrentHeight) // 후보가 더 높은 위치인지 확인
            { // 더 높은 후보 처리
                return true; // 후보 우선 반환
            } // 더 높은 후보 처리 종료

            if (truncatedCandidateHeight < truncatedCurrentHeight) // 후보가 더 낮은 위치인지 확인
            { // 더 낮은 후보 처리
                return false; // 기존 목표 우선 반환
            } // 더 낮은 후보 처리 종료

            return candidateDistance < currentDistance; // 같은 높이에서 더 가까운 후보 우선 반환
        } // 드론 목표 우선순위 비교 처리 종료

        public static float ClampMiniatureScale(float scale) // 소형화 배율 안전 범위 보정
        { // 소형화 배율 보정 처리
            return Mathf.Clamp(scale, 0.4f, 1f); // 지나치게 작거나 커지지 않는 배율 반환
        } // 소형화 배율 보정 처리 종료

        public static float ClampSniperZoom(float zoom) // 저격 배율 안전 범위 보정
        { // 저격 배율 보정 처리
            return Mathf.Clamp(zoom, MinimumSniperZoom, MaximumSniperZoom); // 최소와 최대 사이 저격 배율 반환
        } // 저격 배율 보정 처리 종료
    } // P2 아이템 공통 규칙 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
