using NUnit.Framework; // Unity EditMode 테스트 기능 사용
using ProjectJ.Debugging; // Debug Window 단축키 정책 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJDebugWindowRoutingPolicyTests // Debug Window 단축키 분리 정책 테스트
    {
        [Test] // Day79 전용 단축키 유지 검증
        public void Day79NetworkConditionView_UsesDedicatedHotkey() // Day79 화면의 F6 전용 처리 확인
        {
            bool usesDedicatedHotkey = // 전용 단축키 여부 계산
                ProjectJDebugWindowRoutingPolicy.UsesDedicatedHotkey( // 단축키 분리 정책 호출
                    "ProjectJ.Networking.Fusion.ProjectJDay79NetworkConditionDebugView" // Day79 전체 타입 이름 전달
                );

            Assert.IsTrue( // F6 전용 처리 결과 검증
                usesDedicatedHotkey // 실제 정책 결과
            );
        }

        [TestCase("ProjectJ.Networking.Fusion.ProjectJDay77InviteDebugView")] // 기존 F1·F2 대상 예시
        [TestCase("ProjectJ.Networking.Fusion.ProjectJDay78EightPlayerDebugView")] // 기존 F1·F2 대상 예시
        [TestCase("")] // 빈 타입 이름 예외 처리
        [TestCase(null)] // null 타입 이름 예외 처리
        public void OtherDebugViews_RemainInDirectHotkeyMenu( // 다른 화면의 기존 관리 유지 확인
            string typeName // 검사할 전체 타입 이름
        )
        {
            bool usesDedicatedHotkey = // 전용 단축키 여부 계산
                ProjectJDebugWindowRoutingPolicy.UsesDedicatedHotkey( // 단축키 분리 정책 호출
                    typeName // 검사할 타입 이름 전달
                );

            Assert.IsFalse( // F1·F2 관리 유지 결과 검증
                usesDedicatedHotkey // 실제 정책 결과
            );
        }
    }
}
