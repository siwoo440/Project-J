using ProjectJ.Data; // 아이템 효과 종류 참조
using UnityEngine; // 수치 보정 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    public static class P1ItemRules // P1 아이템 공통 판정 규칙 선언
    { // P1 아이템 공통 판정 규칙 묶음
        public static bool IsP1Effect(ItemEffectType effectType) // 지정 효과가 43일차 P1 대상인지 확인
        { // P1 효과 확인 처리
            return effectType >= ItemEffectType.Jetpack && effectType <= ItemEffectType.GiantBalloon; // 연속된 P1 열거형 범위 확인
        } // P1 효과 확인 처리 종료

        public static int RegisterAlternatingEscapeInput(int currentCount, int lastDirection, int newDirection, int requiredCount, out int nextLastDirection) // A와 D 교대 탈출 입력 누적
        { // 교대 탈출 입력 처리
            int safeRequiredCount = Mathf.Max(1, requiredCount); // 최소 한 번 이상의 탈출 입력 수 보정
            int safeDirection = newDirection < 0 ? -1 : newDirection > 0 ? 1 : 0; // 입력 방향을 A와 D 값으로 보정
            nextLastDirection = lastDirection; // 기존 마지막 방향을 기본 결과로 저장

            if (safeDirection == 0) // 방향 없는 입력 여부 확인
            { // 빈 입력 처리
                return Mathf.Clamp(currentCount, 0, safeRequiredCount); // 기존 누적 횟수 유지
            } // 빈 입력 처리 종료

            if (lastDirection == safeDirection) // 직전과 같은 방향 입력 여부 확인
            { // 같은 방향 반복 처리
                return Mathf.Clamp(currentCount, 0, safeRequiredCount); // 교대되지 않은 입력 횟수 제외
            } // 같은 방향 반복 처리 종료

            nextLastDirection = safeDirection; // 새 교대 방향 저장
            return Mathf.Clamp(currentCount + 1, 0, safeRequiredCount); // 유효 교대 입력 한 번 누적
        } // 교대 탈출 입력 처리 종료

        public static float CalculateProjectileLifeTime(float range, float speed, float fallbackLifeTime) // 거리와 속도 기반 투사체 수명 계산
        { // 투사체 수명 계산 처리
            if (range <= 0f || speed <= 0f) // 거리 또는 속도 누락 여부 확인
            { // 기본 수명 처리
                return Mathf.Max(0.1f, fallbackLifeTime); // 안전한 기본 수명 반환
            } // 기본 수명 처리 종료

            return Mathf.Max(0.1f, range / speed); // 최대 이동 거리를 지키는 수명 반환
        } // 투사체 수명 계산 처리 종료
    } // P1 아이템 공통 판정 규칙 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
