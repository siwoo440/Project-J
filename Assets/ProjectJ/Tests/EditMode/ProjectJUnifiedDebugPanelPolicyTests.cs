using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Debugging; // 통합 디버그 패널 정책

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJUnifiedDebugPanelPolicyTests // 통합 패널 정책 테스트
    {
        [Test] // 초기 표시 상태 검증
        public void DefaultVisibility_IsHidden() // 기본 숨김 상태 확인
        {
            bool isVisible = // 초기 표시 상태 계산
                ProjectJUnifiedDebugPanelPolicy.DefaultVisibility; // 기본 상태 조회

            Assert.IsFalse( // 숨김 상태 검증
                isVisible // 실제 기본 상태
            );
        }

        [TestCase(false, true)] // 닫힌 패널 열기
        [TestCase(true, false)] // 열린 패널 닫기
        public void ToggleVisibility_ReversesCurrentState( // F1 전환 규칙 확인
            bool currentState, // 현재 표시 상태
            bool expectedState // 예상 표시 상태
        )
        {
            bool nextState = // 다음 표시 상태 계산
                ProjectJUnifiedDebugPanelPolicy.ToggleVisibility( // 표시 전환 정책 호출
                    currentState // 현재 상태 전달
                );

            Assert.AreEqual( // 전환 결과 검증
                expectedState, // 예상 상태
                nextState // 실제 상태
            );
        }

        [TestCase( // 개요 분류 예시
            "ProjectJFusionBootstrapDebugView", // Bootstrap 화면 이름
            ProjectJDebugPanelCategory.Overview // 예상 개요 탭
        )]
        [TestCase( // 네트워크 분류 예시
            "ProjectJDay79NetworkConditionDebugView", // 네트워크 화면 이름
            ProjectJDebugPanelCategory.Network // 예상 네트워크 탭
        )]
        [TestCase( // 플레이어 분류 예시
            "ProjectJDay78EightPlayerDebugView", // 플레이어 화면 이름
            ProjectJDebugPanelCategory.Player // 예상 플레이어 탭
        )]
        [TestCase( // 세션 분류 예시
            "ProjectJDay81SteamInviteDebugView", // Steam 화면 이름
            ProjectJDebugPanelCategory.Session // 예상 세션 탭
        )]
        [TestCase( // 게임 상태 분류 예시
            "RespawnProtectionDebugView", // 부활 보호 화면 이름
            ProjectJDebugPanelCategory.Gameplay // 예상 게임 상태 탭
        )]
        public void GetCategory_GroupsRelatedWindows( // 관련 창 탭 분류 확인
            string typeName, // 검사할 타입 이름
            ProjectJDebugPanelCategory expectedCategory // 예상 탭 분류
        )
        {
            ProjectJDebugPanelCategory category = // 실제 탭 분류 계산
                ProjectJUnifiedDebugPanelPolicy.GetCategory( // 분류 정책 호출
                    typeName // 타입 이름 전달
                );

            Assert.AreEqual( // 분류 결과 검증
                expectedCategory, // 예상 분류
                category // 실제 분류
            );
        }

        [TestCase(ProjectJDebugPanelCategory.Overview, "개요")] // 개요 탭 이름
        [TestCase(ProjectJDebugPanelCategory.Network, "네트워크")] // 네트워크 탭 이름
        [TestCase(ProjectJDebugPanelCategory.Player, "플레이어")] // 플레이어 탭 이름
        [TestCase(ProjectJDebugPanelCategory.Session, "세션·Steam")] // 세션 탭 이름
        [TestCase(ProjectJDebugPanelCategory.Gameplay, "게임 상태")] // 게임 탭 이름
        public void GetCategoryLabel_ReturnsReadableKoreanLabel( // 한글 탭 이름 확인
            ProjectJDebugPanelCategory category, // 검사할 탭 분류
            string expectedLabel // 예상 표시 이름
        )
        {
            string label = // 실제 표시 이름 계산
                ProjectJUnifiedDebugPanelPolicy.GetCategoryLabel( // 표시 이름 정책 호출
                    category // 탭 분류 전달
                );

            Assert.AreEqual( // 표시 이름 검증
                expectedLabel, // 예상 이름
                label // 실제 이름
            );
        }

        [TestCase("ProjectJDay76TestFlow")] // 멀티플레이 테스트 창
        [TestCase("ProjectJNetworkExternalGameplay")] // 네트워크 경기 상태 창
        [TestCase("ProjectJNetworkItemInventory")] // 네트워크 아이템 상태 창
        [TestCase("ProjectJLocalPlayerPresentationController")] // 로컬 플레이어 상태 창
        [TestCase("ProjectJNetworkLobbyFlow")] // Lobby 진행 상태 창
        public void IsKnownDiagnosticWindow_IncludesEmbeddedDebugPanels( // 기능 Component 내부 진단창 확인
            string typeName // 검사할 타입 이름
        )
        {
            bool isKnown = // 알려진 진단창 여부 계산
                ProjectJUnifiedDebugPanelPolicy.IsKnownDiagnosticWindow( // 진단창 정책 호출
                    typeName // 타입 이름 전달
                );

            Assert.IsTrue( // 진단창 포함 결과 검증
                isKnown // 실제 포함 여부
            );
        }
    }
}
