using ProjectJ.Items; // 망치 정책 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    public sealed partial class ProjectJNetworkExternalGameplay // 망치 적용 Push 수치 조회
    {
        private bool IsHammerPushActive =>
            itemInventory != null && itemInventory.IsHammerActive; // 사용자 망치 활성 상태 조회

        private float CurrentPushSearchRange =>
            ProjectJHammerPolicy.ResolvePushRange( // 현재 Push 사거리 계산
                PushSearchRange, // 기존 Push 사거리 전달
                IsHammerPushActive // 망치 활성 상태 전달
            );

        private float CurrentPushForce =>
            ProjectJHammerPolicy.ResolvePushForce( // 현재 Push 외력 계산
                PushForce, // 기존 Push 외력 전달
                IsHammerPushActive // 망치 활성 상태 전달
            );

        private float CurrentPushCooldownSeconds =>
            ProjectJHammerPolicy.ResolvePushCooldown( // 현재 Push 재사용 시간 계산
                PushCooldownSeconds, // 기존 Push 재사용 시간 전달
                IsHammerPushActive // 망치 활성 상태 전달
            );
    }
}
