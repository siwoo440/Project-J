using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Player; // 체크포인트 진행 규칙 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 체크포인트 진행 규칙 테스트 범위
    public sealed class CheckpointProgressRulesTests // 체크포인트 순서와 진행률 계산 테스트 선언
    { // 체크포인트 진행 규칙 테스트 기능 범위
        private const float ComparisonTolerance = 0.0001f; // 부동소수점 비교 허용 오차

        [Test] // Unity Test Runner 테스트 지정
        public void CheckpointCountNeverFallsBelowOne() // 잘못된 전체 체크포인트 개수 보정 검증
        { // 체크포인트 개수 보정 검증 범위
            int result = CheckpointProgressRules.ClampCheckpointCount(0); // 0개 체크포인트 설정 보정
            Assert.That(result, Is.EqualTo(1)); // 최소 한 개 보장 확인
        } // 체크포인트 개수 보정 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void StartingIndexRemainsZero() // 시작 지점의 0번 유지 검증
        { // 시작 지점 번호 검증 범위
            int result = CheckpointProgressRules.ClampCheckpointIndex(0, 4); // 시작 지점 번호 보정
            Assert.That(result, Is.EqualTo(0)); // 0번 시작 지점 유지 확인
        } // 시작 지점 번호 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void IndexClampsAboveLastCheckpoint() // 마지막 체크포인트 초과 번호 보정 검증
        { // 마지막 번호 보정 검증 범위
            int result = CheckpointProgressRules.ClampCheckpointIndex(8, 4); // 전체 개수를 넘는 번호 보정
            Assert.That(result, Is.EqualTo(4)); // 마지막 체크포인트 번호 제한 확인
        } // 마지막 번호 보정 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void FirstCheckpointIsInsideValidRange() // 첫 번째 체크포인트 유효 범위 검증
        { // 첫 번째 체크포인트 범위 검증
            bool result = CheckpointProgressRules.IsCheckpointIndexInRange(1, 4); // 첫 번째 체크포인트 번호 판정
            Assert.That(result, Is.True); // 유효한 번호 확인
        } // 첫 번째 체크포인트 범위 검증 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ZeroIsNotAValidCheckpointTriggerIndex() // 시작 번호의 트리거 사용 차단 검증
        { // 시작 번호 트리거 차단 검증 범위
            bool result = CheckpointProgressRules.IsCheckpointIndexInRange(0, 4); // 0번 체크포인트 트리거 번호 판정
            Assert.That(result, Is.False); // 트리거 번호 사용 차단 확인
        } // 시작 번호 트리거 차단 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void HigherCheckpointCanActivate() // 현재보다 높은 체크포인트 활성화 검증
        { // 높은 체크포인트 활성화 검증 범위
            bool result = CheckpointProgressRules.CanActivateCheckpoint(1, 3, 4); // 1번 이후 3번 활성화 판정
            Assert.That(result, Is.True); // 더 높은 체크포인트 허용 확인
        } // 높은 체크포인트 활성화 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void SameCheckpointCannotActivateAgain() // 동일 체크포인트 중복 활성화 차단 검증
        { // 동일 체크포인트 차단 검증 범위
            bool result = CheckpointProgressRules.CanActivateCheckpoint(2, 2, 4); // 동일한 2번 체크포인트 재활성화 판정
            Assert.That(result, Is.False); // 중복 활성화 차단 확인
        } // 동일 체크포인트 차단 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void LowerCheckpointCannotReplaceRespawnPoint() // 낮은 체크포인트 부활 지점 변경 차단 검증
        { // 낮은 체크포인트 차단 검증 범위
            bool result = CheckpointProgressRules.CanActivateCheckpoint(3, 1, 4); // 3번 이후 1번 활성화 판정
            Assert.That(result, Is.False); // 부활 지점 하향 변경 차단 확인
        } // 낮은 체크포인트 차단 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void SecondCheckpointReportsHalfProgress() // 두 번째 체크포인트 진행률 검증
        { // 체크포인트 진행률 검증 범위
            float result = CheckpointProgressRules.CalculateProgress01(2, 4); // 4개 중 2번 진행률 계산
            Assert.That(result, Is.EqualTo(0.5f).Within(ComparisonTolerance)); // 50퍼센트 진행률 확인
        } // 체크포인트 진행률 검증 범위 종료
    } // 체크포인트 진행 규칙 테스트 기능 범위 종료
} // 체크포인트 진행 규칙 테스트 범위 종료
