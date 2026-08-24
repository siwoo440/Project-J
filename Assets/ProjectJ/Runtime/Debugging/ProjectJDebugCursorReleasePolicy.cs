namespace ProjectJ.Debugging // 개발용 디버깅 네임스페이스
{
    public static class ProjectJDebugCursorReleasePolicy // ALT 커서 상태 전환 정책
    {
        public static bool GetNextReleasedState( // 다음 커서 해제 상태 계산
            bool isCursorReleased // 현재 커서 해제 상태
        )
        {
            return !isCursorReleased; // 현재 상태 반전
        }

        public static bool CanProcessCameraInput( // 게임 카메라 입력 가능 여부 계산
            bool isCursorReleased // 현재 커서 해제 상태
        )
        {
            return !isCursorReleased; // 커서 잠금 상태에서만 입력 허용
        }
    }
}
