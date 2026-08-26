using UnityEngine; // Unity 런타임 기능

namespace ProjectJ.Player // Player 네임스페이스
{
    public sealed class ProjectJAnimatorDirectPlayTest : MonoBehaviour // Animator 직접 재생 테스트
    {
        private Animator animator; // 테스트 대상 Animator

        private void Awake() // Animator 연결
        {
            animator = GetComponentInChildren<Animator>(true); // 자식 포함 Animator 조회
        }

        [ContextMenu("Test Running State")] // Inspector 테스트 메뉴
        private void TestRunningState() // Running 직접 재생
        {
            if (animator == null) // Animator 누락 검사
            {
                animator = GetComponentInChildren<Animator>(true); // 자식 포함 Animator 재검색
            }

            if (animator == null) // Animator 최종 누락 검사
            {
                Debug.LogError("[Animator Test] Animator가 없습니다.", this); // 오류 출력
                return;
            }

            int runningHash = Animator.StringToHash("running"); // Running 상태 Hash 생성
            bool hasRunningState = animator.HasState(0, runningHash); // Base Layer 상태 존재 확인

            Debug.Log(
                $"[Animator Test] Controller={animator.runtimeAnimatorController?.name}, HasRunning={hasRunningState}",
                this
            ); // 상태 진단 출력

            if (!hasRunningState) // Running 상태 누락 검사
            {
                return;
            }

            animator.Play(runningHash, 0, 0f); // Running 상태 직접 재생
        }
    }
}