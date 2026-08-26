using NUnit.Framework; // EditMode 테스트 기능
using ProjectJ.Player; // Player Visual 정책 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJPlayerVisualIndexTests
    {
        [Test]
        public void Resolve_ReturnsMinusOne_WhenCandidateCountIsZero()
        {
            int result = ProjectJPlayerVisualIndex.Resolve(
                10,
                0
            ); // 빈 후보 처리

            Assert.AreEqual(-1, result); // 선택 불가 확인
        }

        [Test]
        public void Resolve_ReturnsMinusOne_WhenCandidateCountIsNegative()
        {
            int result = ProjectJPlayerVisualIndex.Resolve(
                10,
                -3
            ); // 잘못된 후보 수 처리

            Assert.AreEqual(-1, result); // 선택 불가 확인
        }

        [Test]
        public void Resolve_ReturnsSameIndex_ForSameIdentity()
        {
            int first = ProjectJPlayerVisualIndex.Resolve(
                123456,
                8
            ); // 첫 선택
            int second = ProjectJPlayerVisualIndex.Resolve(
                123456,
                8
            ); // 두 번째 선택

            Assert.AreEqual(first, second); // 결정성 확인
        }

        [Test]
        public void Resolve_WrapsPositiveIdentity()
        {
            int result = ProjectJPlayerVisualIndex.Resolve(
                9,
                8
            ); // 양수 범위 보정

            Assert.AreEqual(1, result); // 나머지 확인
        }

        [Test]
        public void Resolve_WrapsNegativeIdentity()
        {
            int result = ProjectJPlayerVisualIndex.Resolve(
                -1,
                8
            ); // 음수 범위 보정

            Assert.AreEqual(7, result); // 양수 인덱스 확인
        }

        [Test]
        public void Resolve_HandlesIntMinValue()
        {
            int result = ProjectJPlayerVisualIndex.Resolve(
                int.MinValue,
                8
            ); // 최소 정수 처리

            Assert.AreEqual(0, result); // 오버플로 방지 확인
        }

        [Test]
        public void StableHash_ReturnsSameValue_ForSameText()
        {
            int first = ProjectJPlayerVisualIndex.StableHash(
                "NetworkObject:42"
            ); // 첫 해시
            int second = ProjectJPlayerVisualIndex.StableHash(
                "NetworkObject:42"
            ); // 두 번째 해시

            Assert.AreEqual(first, second); // 안정 해시 확인
        }

        [Test] // 요청 외형 선택 사례
        public void ResolveByName_ReturnsRequestedChefIndex() // 요청 이름 무시 오류 방지
        { // 테스트 시작
            string[] candidates = // 등록된 외형 이름 준비
            { // 배열 시작
                "Character_040ae0", // 첫 외형 이름
                "Character_0413da", // 둘째 외형 이름
                "Character_4c4e64" // 검은 요리사 이름
            }; // 배열 종료

            int result = ProjectJPlayerVisualIndex.ResolveByName( // 이름 기반 인덱스 계산
                "Character_4c4e64", // 요청 외형 이름
                "Character_4c4e64", // 기본 외형 이름
                candidates // 등록 외형 전달
            ); // 선택 종료

            Assert.AreEqual(2, result); // 검은 요리사 위치 확인
        } // 테스트 종료

        [Test] // 잘못된 꾸미기 값 사례
        public void ResolveByName_UsesChefWhenRequestedNameIsMissing() // 기본 외형 복구 누락 방지
        { // 테스트 시작
            string[] candidates = // 등록된 외형 이름 준비
            { // 배열 시작
                "Character_040ae0", // 첫 외형 이름
                "Character_0413da", // 둘째 외형 이름
                "Character_4c4e64" // 검은 요리사 이름
            }; // 배열 종료

            int result = ProjectJPlayerVisualIndex.ResolveByName( // 이름 기반 fallback 계산
                "Unknown_Costume", // 잘못된 꾸미기 이름
                "Character_4c4e64", // 검은 요리사 기본값
                candidates // 등록 외형 전달
            ); // 선택 종료

            Assert.AreEqual(2, result); // 검은 요리사 fallback 확인
        } // 테스트 종료

        [Test] // 기본 외형 자산 누락 사례
        public void ResolveByName_UsesFirstAvailableWhenChefIsMissing() // 전체 외형 미표시 오류 방지
        { // 테스트 시작
            string[] candidates = // 일부 누락 외형 준비
            { // 배열 시작
                null, // 첫 후보 누락
                "Character_0413da", // 첫 유효 외형
                null // 마지막 후보 누락
            }; // 배열 종료

            int result = ProjectJPlayerVisualIndex.ResolveByName( // 최종 fallback 계산
                "Unknown_Costume", // 잘못된 꾸미기 이름
                "Character_4c4e64", // 누락된 기본 이름
                candidates // 등록 외형 전달
            ); // 선택 종료

            Assert.AreEqual(1, result); // 첫 유효 외형 복구 확인
        } // 테스트 종료

        [Test] // 전체 외형 누락 사례
        public void ResolveByName_ReturnsMinusOneWhenCandidatesAreEmpty() // 잘못된 인덱스 생성 방지
        { // 테스트 시작
            int result = ProjectJPlayerVisualIndex.ResolveByName( // 빈 후보 선택 계산
                "Character_4c4e64", // 요청 외형 이름
                "Character_4c4e64", // 기본 외형 이름
                new string[0] // 빈 후보 전달
            ); // 선택 종료

            Assert.AreEqual(-1, result); // 선택 불가 확인
        } // 테스트 종료

    }
}
