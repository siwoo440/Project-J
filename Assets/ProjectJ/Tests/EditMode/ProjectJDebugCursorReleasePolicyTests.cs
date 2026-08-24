using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Debugging; // 개발용 커서 정책 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJDebugCursorReleasePolicyTests // ALT 커서 전환 정책 테스트
    {
        [Test] // 잠긴 커서 해제 전환 검증
        public void GetNextReleasedState_WhenLocked_ReturnsReleased() // 첫 ALT 입력 활성화 확인
        {
            bool nextReleasedState = // 다음 커서 해제 상태 계산
                ProjectJDebugCursorReleasePolicy.GetNextReleasedState( // 토글 정책 호출
                    false // 현재 커서 잠금 상태
                );

            Assert.IsTrue( // 활성화 결과 검증
                nextReleasedState // 실제 다음 상태
            );
        }

        [Test] // 해제된 커서 잠금 전환 검증
        public void GetNextReleasedState_WhenReleased_ReturnsLocked() // 두 번째 ALT 입력 복구 확인
        {
            bool nextReleasedState = // 다음 커서 해제 상태 계산
                ProjectJDebugCursorReleasePolicy.GetNextReleasedState( // 토글 정책 호출
                    true // 현재 커서 해제 상태
                );

            Assert.IsFalse( // 잠금 복구 결과 검증
                nextReleasedState // 실제 다음 상태
            );
        }

        [Test] // 일반 게임 카메라 입력 허용 검증
        public void CanProcessCameraInput_WhenCursorLocked_ReturnsTrue() // 잠금 상태 조작 유지 확인
        {
            bool canProcessCameraInput = // 카메라 입력 가능 여부 계산
                ProjectJDebugCursorReleasePolicy.CanProcessCameraInput( // 입력 정책 호출
                    false // 커서 해제 아님
                );

            Assert.IsTrue( // 카메라 입력 허용 검증
                canProcessCameraInput // 실제 입력 가능 상태
            );
        }

        [Test] // 커서 사용 중 카메라 입력 차단 검증
        public void CanProcessCameraInput_WhenCursorReleased_ReturnsFalse() // UI 조작 중 시점 이동 방지 확인
        {
            bool canProcessCameraInput = // 카메라 입력 가능 여부 계산
                ProjectJDebugCursorReleasePolicy.CanProcessCameraInput( // 입력 정책 호출
                    true // 커서 해제 상태
                );

            Assert.IsFalse( // 카메라 입력 차단 검증
                canProcessCameraInput // 실제 입력 가능 상태
            );
        }
    }
}
