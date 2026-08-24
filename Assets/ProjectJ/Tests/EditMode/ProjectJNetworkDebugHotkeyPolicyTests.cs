using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Debugging; // 네트워크 디버그 단축키 정책 사용
using UnityEngine.InputSystem; // 키 열거형 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJNetworkDebugHotkeyPolicyTests // 네트워크 디버그 단축키 정책 테스트
    {
        [TestCase(ProjectJNetworkDebugAction.SoloStart, Key.F5)] // 단독 시작 키 사례
        [TestCase(ProjectJNetworkDebugAction.MovementDiagnostics, Key.F6)] // 이동 진단 키 사례
        [TestCase(ProjectJNetworkDebugAction.MeasurementReset, Key.F10)] // 측정 초기화 키 사례
        [TestCase(ProjectJNetworkDebugAction.ForceMatchEnd, Key.F11)] // 강제 종료 키 사례
        public void GetKey_WithKnownAction_ReturnsDedicatedKey( // 기능별 전용 키 반환 검증
            ProjectJNetworkDebugAction action, // 검사할 디버그 기능
            Key expectedKey // 예상 전용 키
        )
        {
            Key actualKey = // 실제 전용 키 조회
                ProjectJNetworkDebugHotkeyPolicy.GetKey( // 단축키 정책 호출
                    action // 검사할 디버그 기능 전달
                );

            Assert.AreEqual( // 전용 키 결과 검증
                expectedKey, // 예상 전용 키
                actualKey // 실제 전용 키
            );
        }

        [Test] // 단축키 전체 중복 검증
        public void HasUniqueBindings_WithDay102Bindings_ReturnsTrue() // F5·F6·F10·F11 중복 방지 확인
        {
            bool hasUniqueBindings = // 단축키 중복 여부 계산
                ProjectJNetworkDebugHotkeyPolicy.HasUniqueBindings(); // 전체 정책 검사

            Assert.IsTrue( // 중복 없음 결과 검증
                hasUniqueBindings // 실제 중복 검사 결과
            );
        }

        [Test] // 알 수 없는 기능 예외 검증
        public void GetKey_WithUnknownAction_ReturnsNone() // 미등록 기능의 안전한 키 반환 확인
        {
            Key actualKey = // 미등록 기능 키 조회
                ProjectJNetworkDebugHotkeyPolicy.GetKey( // 단축키 정책 호출
                    (ProjectJNetworkDebugAction)999 // 미등록 기능 전달
                );

            Assert.AreEqual( // 미지정 키 결과 검증
                Key.None, // 예상 미지정 키
                actualKey // 실제 반환 키
            );
        }

        [Test] // 강제 종료 허용 조건 검증
        public void CanForceMatchEnd_WithAllConditions_ReturnsTrue() // 정상 Host 경기의 강제 종료 허용 확인
        {
            bool canForceMatchEnd = // 강제 종료 허용 여부 계산
                ProjectJNetworkDebugHotkeyPolicy.CanForceMatchEnd( // 강제 종료 정책 호출
                    true, // Game Scene 활성 상태
                    true, // State Authority 보유
                    true, // Match Coordinator 일치
                    true // 경기 입력 허용 상태
                );

            Assert.IsTrue( // 강제 종료 허용 결과 검증
                canForceMatchEnd // 실제 허용 결과
            );
        }

        [TestCase(false, true, true, true)] // Game Scene 미활성 사례
        [TestCase(true, false, true, true)] // State Authority 미보유 사례
        [TestCase(true, true, false, true)] // Match Coordinator 불일치 사례
        [TestCase(true, true, true, false)] // 경기 진행 전후 사례
        public void CanForceMatchEnd_WithMissingCondition_ReturnsFalse( // 잘못된 강제 종료 차단 확인
            bool isGameSceneActive, // Game Scene 활성 여부
            bool hasStateAuthority, // State Authority 보유 여부
            bool isMatchCoordinator, // Match Coordinator 일치 여부
            bool gameplayInputAllowed // 경기 입력 허용 여부
        )
        {
            bool canForceMatchEnd = // 강제 종료 허용 여부 계산
                ProjectJNetworkDebugHotkeyPolicy.CanForceMatchEnd( // 강제 종료 정책 호출
                    isGameSceneActive, // Game Scene 상태 전달
                    hasStateAuthority, // State Authority 상태 전달
                    isMatchCoordinator, // Match Coordinator 상태 전달
                    gameplayInputAllowed // 경기 입력 상태 전달
                );

            Assert.IsFalse( // 강제 종료 차단 결과 검증
                canForceMatchEnd // 실제 허용 결과
            );
        }
    }
}
