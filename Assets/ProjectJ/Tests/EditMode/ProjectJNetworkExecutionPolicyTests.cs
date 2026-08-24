using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Networking; // 실행 모드 정책 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJNetworkExecutionPolicyTests // 네트워크 실행 모드 정책 테스트
    {
        [Test] // 일반 실행 Bootstrap 설치 검증
        public void HostClientBuild_InstallsHostClientBootstrap() // 일반 실행에서 Host·Client Bootstrap 허용 확인
        {
            bool shouldInstall = // Bootstrap 설치 여부 계산
                ProjectJNetworkExecutionPolicy.ShouldInstallHostClientBootstrap( // 실행 모드 정책 호출
                    false // Dedicated Server 빌드 아님
                );

            Assert.IsTrue( // 설치 허용 결과 검증
                shouldInstall // 실제 정책 결과
            );
        }

        [Test] // Server 실행 Bootstrap 차단 검증
        public void DedicatedServerBuild_DoesNotInstallHostClientBootstrap() // Server 빌드에서 일반 Bootstrap 차단 확인
        {
            bool shouldInstall = // Bootstrap 설치 여부 계산
                ProjectJNetworkExecutionPolicy.ShouldInstallHostClientBootstrap( // 실행 모드 정책 호출
                    true // Dedicated Server 빌드 상태
                );

            Assert.IsFalse( // 설치 차단 결과 검증
                shouldInstall // 실제 정책 결과
            );
        }

        [TestCase(true, true, true)] // Server 빌드 자동 시작 허용
        [TestCase(true, false, false)] // 자동 시작 선택 해제 차단
        [TestCase(false, true, false)] // 일반 빌드 자동 시작 차단
        [TestCase(false, false, false)] // 일반 빌드 비활성 상태 차단
        public void DedicatedServer_AutoStartsOnlyWhenServerBuildAndEnabled( // Dedicated 자동 시작 조건 검증
            bool isDedicatedServerBuild, // Server 빌드 여부
            bool startOnPlay, // 자동 시작 설정
            bool expected // 예상 결과
        )
        {
            bool shouldStart = // Dedicated 자동 시작 여부 계산
                ProjectJNetworkExecutionPolicy.ShouldAutoStartDedicatedServer( // 실행 모드 정책 호출
                    isDedicatedServerBuild, // Server 빌드 여부 전달
                    startOnPlay // 자동 시작 설정 전달
                );

            Assert.AreEqual( // 예상 결과 비교
                expected, // 예상 정책 결과
                shouldStart // 실제 정책 결과
            );
        }
    }
}
