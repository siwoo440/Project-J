using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Debugging; // 이동 품질 진단 정책 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJMovementQualityPolicyTests // 이동 품질 진단 정책 테스트
    {
        [Test] // 로컬 입력 역할 우선순위 검증
        public void GetRoleLabel_WithInputAndStateAuthority_ReturnsLocalInput() // Host 로컬 Player 역할 확인
        {
            string roleLabel = // 역할 표시 문자열 계산
                ProjectJMovementQualityPolicy.GetRoleLabel( // 역할 판정 정책 호출
                    true, // Input Authority 보유
                    true // State Authority 보유
                );

            Assert.AreEqual( // 역할 표시 결과 검증
                "LOCAL INPUT", // 예상 로컬 입력 역할
                roleLabel // 실제 역할 표시
            );
        }

        [Test] // State Authority 역할 검증
        public void GetRoleLabel_WithOnlyStateAuthority_ReturnsStateAuthority() // 원격 Host Player 역할 확인
        {
            string roleLabel = // 역할 표시 문자열 계산
                ProjectJMovementQualityPolicy.GetRoleLabel( // 역할 판정 정책 호출
                    false, // Input Authority 없음
                    true // State Authority 보유
                );

            Assert.AreEqual( // 역할 표시 결과 검증
                "STATE AUTHORITY", // 예상 State Authority 역할
                roleLabel // 실제 역할 표시
            );
        }

        [Test] // 원격 Proxy 역할 검증
        public void GetRoleLabel_WithoutAuthority_ReturnsRemoteProxy() // Client의 상대 Player 역할 확인
        {
            string roleLabel = // 역할 표시 문자열 계산
                ProjectJMovementQualityPolicy.GetRoleLabel( // 역할 판정 정책 호출
                    false, // Input Authority 없음
                    false // State Authority 없음
                );

            Assert.AreEqual( // 역할 표시 결과 검증
                "REMOTE PROXY", // 예상 원격 Proxy 역할
                roleLabel // 실제 역할 표시
            );
        }

        [Test] // 최대 측정값 갱신 검증
        public void AccumulatePeak_WithLargerSample_ReturnsSample() // 새 최대값 저장 확인
        {
            float peak = // 누적 최대값 계산
                ProjectJMovementQualityPolicy.AccumulatePeak( // 최대값 누적 정책 호출
                    0.25f, // 기존 최대값
                    0.4f // 새 측정값
                );

            Assert.AreEqual( // 최대값 갱신 결과 검증
                0.4f, // 예상 새 최대값
                peak // 실제 누적 최대값
            );
        }

        [Test] // 기존 최대 측정값 유지 검증
        public void AccumulatePeak_WithSmallerSample_KeepsCurrentPeak() // 작은 표본의 최대값 덮어쓰기 방지
        {
            float peak = // 누적 최대값 계산
                ProjectJMovementQualityPolicy.AccumulatePeak( // 최대값 누적 정책 호출
                    0.4f, // 기존 최대값
                    0.25f // 새 측정값
                );

            Assert.AreEqual( // 최대값 유지 결과 검증
                0.4f, // 예상 기존 최대값
                peak // 실제 누적 최대값
            );
        }

        [Test] // 음수 측정값 방어 검증
        public void AccumulatePeak_WithNegativeValues_ReturnsZero() // 잘못된 음수 진단값 차단
        {
            float peak = // 누적 최대값 계산
                ProjectJMovementQualityPolicy.AccumulatePeak( // 최대값 누적 정책 호출
                    -0.4f, // 잘못된 기존 최대값
                    -0.25f // 잘못된 새 측정값
                );

            Assert.AreEqual( // 음수 차단 결과 검증
                0f, // 예상 최소값
                peak // 실제 누적 최대값
            );
        }

        [Test] // 측정 경과 시간 검증
        public void CalculateElapsed_WithLaterCurrentTime_ReturnsDifference() // 정상 시간 차이 계산 확인
        {
            float elapsed = // 측정 경과 시간 계산
                ProjectJMovementQualityPolicy.CalculateElapsed( // 경과 시간 정책 호출
                    10f, // 측정 시작 시각
                    12.5f // 현재 시각
                );

            Assert.AreEqual( // 시간 차이 결과 검증
                2.5f, // 예상 경과 시간
                elapsed // 실제 경과 시간
            );
        }

        [Test] // 역전된 시간 방어 검증
        public void CalculateElapsed_WithEarlierCurrentTime_ReturnsZero() // Scene 전환 시 음수 시간 방지
        {
            float elapsed = // 측정 경과 시간 계산
                ProjectJMovementQualityPolicy.CalculateElapsed( // 경과 시간 정책 호출
                    12.5f, // 측정 시작 시각
                    10f // 이전 현재 시각
                );

            Assert.AreEqual( // 음수 시간 차단 결과 검증
                0f, // 예상 최소 경과 시간
                elapsed // 실제 경과 시간
            );
        }
    }
}
