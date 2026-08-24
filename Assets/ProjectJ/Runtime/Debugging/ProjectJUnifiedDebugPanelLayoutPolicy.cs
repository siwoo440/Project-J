using UnityEngine; // Rect와 Vector2 계산 기능

namespace ProjectJ.Debugging // 디버그 공통 네임스페이스
{
    public static class ProjectJUnifiedDebugPanelLayoutPolicy // 통합 패널 배치 정책
    {
        private const float OuterMargin = 16f; // 화면 가장자리 여백
        private const float MaximumPanelWidth = 1440f; // 패널 최대 너비
        private const float MaximumPanelHeight = 1000f; // 패널 최대 높이
        private const float MaximumNavigationWidth = 220f; // 목록 최대 너비
        private const float MinimumNavigationWidth = 120f; // 목록 권장 최소 너비
        private const float NavigationWidthRatio = 0.32f; // 패널 대비 목록 너비 비율
        private const float MinimumContentWidth = 120f; // 우측 내용 최소 너비
        private const float ContentHorizontalMargin = 34f; // 내용 영역 좌우 여백
        private const float MinimumLegacyCanvasWidth = 1280f; // 기존 진단창 가상 최소 너비
        private const float MinimumLegacyCanvasHeight = 1080f; // 기존 진단창 가상 최소 높이

        public static Rect CalculatePanelRect( // 화면 내부 패널 영역 계산
            float screenWidth, // 현재 화면 너비
            float screenHeight // 현재 화면 높이
        )
        {
            float safeScreenWidth = Mathf.Max(0f, screenWidth); // 음수 화면 너비 방지
            float safeScreenHeight = Mathf.Max(0f, screenHeight); // 음수 화면 높이 방지
            float availableWidth = Mathf.Max(0f, safeScreenWidth - OuterMargin * 2f); // 좌우 여백 제외 너비
            float availableHeight = Mathf.Max(0f, safeScreenHeight - OuterMargin * 2f); // 상하 여백 제외 높이
            float panelWidth = Mathf.Min(availableWidth, MaximumPanelWidth); // 최대 너비 제한
            float panelHeight = Mathf.Min(availableHeight, MaximumPanelHeight); // 최대 높이 제한
            float panelX = (safeScreenWidth - panelWidth) * 0.5f; // 가로 중앙 위치 계산
            float panelY = (safeScreenHeight - panelHeight) * 0.5f; // 세로 중앙 위치 계산

            return new Rect( // 최종 패널 영역 반환
                panelX, // 패널 가로 위치
                panelY, // 패널 세로 위치
                panelWidth, // 패널 너비
                panelHeight // 패널 높이
            );
        }

        public static float CalculateNavigationWidth( // 반응형 좌측 목록 너비 계산
            float panelWidth // 현재 패널 너비
        )
        {
            float safePanelWidth = Mathf.Max(0f, panelWidth); // 음수 패널 너비 방지
            float preferredWidth = Mathf.Clamp( // 권장 목록 너비 제한
                safePanelWidth * NavigationWidthRatio, // 패널 비율 기반 너비
                MinimumNavigationWidth, // 권장 최소 너비
                MaximumNavigationWidth // 최대 너비
            );

            float maximumAllowedWidth = Mathf.Max( // 우측 내용 영역 보존
                0f, // 음수 너비 방지
                safePanelWidth - MinimumContentWidth - ContentHorizontalMargin // 남길 내용 너비 제외
            );

            return Mathf.Min( // 사용 가능한 목록 너비 반환
                preferredWidth, // 권장 목록 너비
                maximumAllowedWidth // 현재 패널 허용 너비
            );
        }

        public static Vector2 CalculateLegacyCanvasSize( // 기존 진단창 가상 영역 계산
            float screenWidth, // 현재 화면 너비
            float screenHeight // 현재 화면 높이
        )
        {
            return new Vector2( // 가상 영역 크기 반환
                Mathf.Max(screenWidth, MinimumLegacyCanvasWidth), // 기존 고정 가로 좌표 수용
                Mathf.Max(screenHeight, MinimumLegacyCanvasHeight) // 기존 고정 세로 좌표 수용
            );
        }
    }
}
