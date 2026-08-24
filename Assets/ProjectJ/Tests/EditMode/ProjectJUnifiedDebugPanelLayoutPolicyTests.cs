using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Debugging; // 통합 패널 배치 정책
using UnityEngine; // Rect와 Vector2 타입

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJUnifiedDebugPanelLayoutPolicyTests // 통합 패널 배치 테스트
    {
        [Test] // 큰 화면 최대 크기 검증
        public void CalculatePanelRect_LargeScreen_UsesMaximumSize() // 큰 화면 패널 제한 확인
        {
            Rect panelRect = // 패널 영역 계산
                ProjectJUnifiedDebugPanelLayoutPolicy.CalculatePanelRect( // 배치 정책 호출
                    1920f, // 화면 너비
                    1080f // 화면 높이
                );

            Assert.AreEqual(240f, panelRect.x, 0.001f); // 가로 중앙 위치 검증
            Assert.AreEqual(40f, panelRect.y, 0.001f); // 세로 중앙 위치 검증
            Assert.AreEqual(1440f, panelRect.width, 0.001f); // 최대 너비 검증
            Assert.AreEqual(1000f, panelRect.height, 0.001f); // 최대 높이 검증
        }

        [Test] // 일반 화면 여백 검증
        public void CalculatePanelRect_NormalScreen_PreservesOuterMargin() // 일반 화면 패널 여백 확인
        {
            Rect panelRect = // 패널 영역 계산
                ProjectJUnifiedDebugPanelLayoutPolicy.CalculatePanelRect( // 배치 정책 호출
                    1280f, // 화면 너비
                    720f // 화면 높이
                );

            Assert.AreEqual(16f, panelRect.x, 0.001f); // 왼쪽 여백 검증
            Assert.AreEqual(16f, panelRect.y, 0.001f); // 위쪽 여백 검증
            Assert.AreEqual(1248f, panelRect.width, 0.001f); // 여백 제외 너비 검증
            Assert.AreEqual(688f, panelRect.height, 0.001f); // 여백 제외 높이 검증
        }

        [Test] // 작은 화면 경계 검증
        public void CalculatePanelRect_SmallScreen_RemainsInsideScreen() // 작은 화면 패널 경계 확인
        {
            Rect panelRect = // 패널 영역 계산
                ProjectJUnifiedDebugPanelLayoutPolicy.CalculatePanelRect( // 배치 정책 호출
                    320f, // 작은 화면 너비
                    240f // 작은 화면 높이
                );

            Assert.GreaterOrEqual(panelRect.xMin, 0f); // 왼쪽 경계 검증
            Assert.GreaterOrEqual(panelRect.yMin, 0f); // 위쪽 경계 검증
            Assert.LessOrEqual(panelRect.xMax, 320f); // 오른쪽 경계 검증
            Assert.LessOrEqual(panelRect.yMax, 240f); // 아래쪽 경계 검증
        }

        [TestCase(1440f, 220f)] // 넓은 패널 목록 너비
        [TestCase(608f, 194.56f)] // 중간 패널 목록 너비
        [TestCase(288f, 120f)] // 작은 패널 목록 너비
        public void CalculateNavigationWidth_AdaptsToPanelWidth( // 좌측 목록 반응형 너비 확인
            float panelWidth, // 현재 패널 너비
            float expectedWidth // 예상 목록 너비
        )
        {
            float navigationWidth = // 좌측 목록 너비 계산
                ProjectJUnifiedDebugPanelLayoutPolicy.CalculateNavigationWidth( // 목록 너비 정책 호출
                    panelWidth // 현재 패널 너비 전달
                );

            Assert.AreEqual( // 목록 너비 결과 검증
                expectedWidth, // 예상 너비
                navigationWidth, // 실제 너비
                0.001f // 부동소수점 허용 오차
            );
        }

        [TestCase(800f, 600f, 1280f, 1080f)] // 작은 화면 가상 영역
        [TestCase(2560f, 1440f, 2560f, 1440f)] // 큰 화면 가상 영역
        public void CalculateLegacyCanvasSize_PreservesScrollableContent( // 기존 고정 좌표 내용 영역 확인
            float screenWidth, // 현재 화면 너비
            float screenHeight, // 현재 화면 높이
            float expectedWidth, // 예상 가상 너비
            float expectedHeight // 예상 가상 높이
        )
        {
            Vector2 canvasSize = // 가상 내용 영역 계산
                ProjectJUnifiedDebugPanelLayoutPolicy.CalculateLegacyCanvasSize( // 가상 영역 정책 호출
                    screenWidth, // 화면 너비 전달
                    screenHeight // 화면 높이 전달
                );

            Assert.AreEqual(expectedWidth, canvasSize.x, 0.001f); // 가상 너비 검증
            Assert.AreEqual(expectedHeight, canvasSize.y, 0.001f); // 가상 높이 검증
        }
    }
}
