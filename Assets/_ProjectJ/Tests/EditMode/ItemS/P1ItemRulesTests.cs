using NUnit.Framework; // EditMode 단위 테스트 기능 참조
using ProjectJ.Data; // 아이템 효과 종류 참조
using ProjectJ.Items; // P1 공통 판정 규칙 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 프로젝트 EditMode 테스트 묶음
    public sealed class P1ItemRulesTests // P1 아이템 판정 규칙 테스트 선언
    { // P1 아이템 판정 규칙 테스트 묶음
        [Test] // Unity Test Runner 테스트 지정
        public void AlternatingAAndDInputsReachSixEscapes() // A와 D 교대 여섯 번 탈출 확인
        { // 교대 탈출 입력 테스트 처리
            int count = 0; // 최초 입력 횟수 저장
            int lastDirection = 0; // 최초 입력 방향 저장
            int[] directions = // A와 D 여섯 번 교대 입력 배열 선언
            { // A와 D 여섯 번 교대 입력 묶음
                -1, // 첫 A 입력
                1, // 첫 D 입력
                -1, // 둘째 A 입력
                1, // 둘째 D 입력
                -1, // 셋째 A 입력
                1 // 셋째 D 입력
            }; // A와 D 여섯 번 교대 입력 묶음 종료

            for (int index = 0; index < directions.Length; index++) // 여섯 입력 순회
            { // 현재 교대 입력 처리
                count = P1ItemRules.RegisterAlternatingEscapeInput(count, lastDirection, directions[index], 6, out lastDirection); // 현재 방향 탈출 횟수 누적
            } // 현재 교대 입력 처리 종료

            Assert.AreEqual(6, count); // 여섯 번 탈출 입력 완료 확인
        } // 교대 탈출 입력 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void RepeatedSameDirectionDoesNotIncreaseEscapeCount() // 같은 방향 반복 입력 제외 확인
        { // 같은 방향 반복 테스트 처리
            int count = P1ItemRules.RegisterAlternatingEscapeInput(1, -1, -1, 6, out int lastDirection); // A 다음 A 입력 처리
            Assert.AreEqual(1, count); // 반복 A 입력 미누적 확인
            Assert.AreEqual(-1, lastDirection); // 마지막 유효 방향 유지 확인
        } // 같은 방향 반복 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void P1RangeIncludesExactlyElevenEffects() // P1 효과 범위 열한 종 확인
        { // P1 효과 범위 테스트 처리
            int p1Count = 0; // P1 효과 개수 초기화

            foreach (ItemEffectType effectType in System.Enum.GetValues(typeof(ItemEffectType))) // 전체 아이템 효과 순회
            { // 현재 아이템 우선순위 확인
                p1Count += P1ItemRules.IsP1Effect(effectType) ? 1 : 0; // P1 효과 개수 누적
            } // 현재 아이템 우선순위 확인 종료

            Assert.AreEqual(11, p1Count); // 확정된 P1 열한 종 확인
        } // P1 효과 범위 테스트 처리 종료
    } // P1 아이템 판정 규칙 테스트 묶음 종료
} // 프로젝트 EditMode 테스트 묶음 종료
