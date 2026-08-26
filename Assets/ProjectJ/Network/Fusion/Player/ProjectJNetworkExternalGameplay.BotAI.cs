using UnityEngine; // Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkExternalGameplay
    {
        public bool TryBotPushAuthority(
            Vector3 desiredForward
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                !GameplayInputAllowed ||
                PushCooldownRemaining > 0f
            )
            {
                return false; // Bot Push 권한·경기·쿨타임 조건 차단
            }

            desiredForward.y =
                0f; // Push 기준 수직 성분 제거

            if (
                desiredForward.sqrMagnitude <=
                0.0001f
            )
            {
                return false; // 유효하지 않은 Push 방향 차단
            }

            NetworkPushForward =
                desiredForward.normalized; // 기존 Push 검색 기준 방향 적용

            int attemptCountBefore =
                NetworkPushAttemptCount; // Push 시도 전 횟수 저장

            ProcessPush(); // 기존 Player Push 서버 권한 처리 재사용

            return
                NetworkPushAttemptCount >
                attemptCountBefore; // 실제 Push 시도 발생 여부 반환
        }
    }
}
