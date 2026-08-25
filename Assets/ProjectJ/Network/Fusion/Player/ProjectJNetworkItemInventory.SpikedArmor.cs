using System.Collections.Generic; // 대상별 재발동 제한 저장
using Fusion; // Networked와 TickTimer 사용
using ProjectJ.Items; // 가시 갑옷 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        private readonly List<ProjectJNetworkExternalGameplay> spikedArmorTargetBuffer =
            new List<ProjectJNetworkExternalGameplay>(8); // 현재 Runner 대상 재사용 목록

        private readonly Dictionary<int, TickTimer> spikedArmorTargetCooldowns =
            new Dictionary<int, TickTimer>(8); // PlayerRef Index별 재발동 제한

        [Networked]
        private TickTimer NetworkSpikedArmorTimer
        {
            get;
            set;
        }

        public bool IsSpikedArmorActive =>
            IsTimerActive(
                NetworkSpikedArmorTimer
            );

        public float SpikedArmorRemaining =>
            GetRemainingTime(
                NetworkSpikedArmorTimer
            );

        private void InitializeSpikedArmorAuthority()
        {
            NetworkSpikedArmorTimer =
                TickTimer.None;

            spikedArmorTargetCooldowns.Clear();
        }

        private bool UseSpikedArmorAuthority()
        {
            bool authorityReady =
                Runner != null &&
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority;

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            if (
                !ProjectJSpikedArmorPolicy.CanActivate(
                    IsSpikedArmorActive,
                    gameplayAllowed,
                    authorityReady
                )
            )
            {
                return false;
            }

            spikedArmorTargetCooldowns.Clear();

            NetworkSpikedArmorTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    ProjectJSpikedArmorPolicy.DurationSeconds
                );

            return true;
        }

        private void UpdateSpikedArmorAuthority()
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            if (!IsSpikedArmorActive)
            {
                NetworkSpikedArmorTimer =
                    TickTimer.None;

                spikedArmorTargetCooldowns.Clear();

                return;
            }

            if (
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed
            )
            {
                ClearSpikedArmorAuthority();

                return;
            }

            ProjectJNetworkExternalGameplay.CollectActivePlayers(
                Runner,
                spikedArmorTargetBuffer
            );

            Vector3 origin =
                transform.position;

            for (
                int index = 0;
                index < spikedArmorTargetBuffer.Count;
                index++
            )
            {
                ProjectJNetworkExternalGameplay target =
                    spikedArmorTargetBuffer[index];

                if (
                    target == null ||
                    target.Object == null ||
                    !target.Object.IsValid
                )
                {
                    continue;
                }

                bool isSelf =
                    target ==
                    externalGameplay;

                int targetIndex =
                    target.Object.InputAuthority.AsIndex;

                bool cooldownActive =
                    IsSpikedArmorTargetCooldownActive(
                        targetIndex
                    );

                if (
                    !ProjectJSpikedArmorPolicy.CanTriggerTarget(
                        isSelf,
                        cooldownActive,
                        target.GameplayInputAllowed
                    )
                )
                {
                    continue;
                }

                Vector3 offset =
                    target.transform.position -
                    origin;

                float distance =
                    offset.magnitude;

                if (
                    !ProjectJSpikedArmorPolicy.IsInsideDetectionRadius(
                        distance
                    )
                )
                {
                    continue;
                }

                Vector3 velocityChange =
                    ProjectJSpikedArmorPolicy.ResolvePushVelocity(
                        offset,
                        transform.forward
                    );

                bool applied =
                    target.TryApplyExternalVelocityChange(
                        ProjectJExternalForceSource.Item,
                        velocityChange
                    );

                if (!applied)
                {
                    continue;
                }

                spikedArmorTargetCooldowns[targetIndex] =
                    TickTimer.CreateFromSeconds(
                        Runner,
                        ProjectJSpikedArmorPolicy.PerTargetCooldownSeconds
                    );
            }
        }

        private bool IsSpikedArmorTargetCooldownActive(
            int targetIndex
        )
        {
            if (
                !spikedArmorTargetCooldowns.TryGetValue(
                    targetIndex,
                    out TickTimer cooldown
                )
            )
            {
                return false;
            }

            if (
                cooldown.ExpiredOrNotRunning(
                    Runner
                )
            )
            {
                spikedArmorTargetCooldowns.Remove(
                    targetIndex
                );

                return false;
            }

            return true;
        }

        private void ClearSpikedArmorAuthority()
        {
            NetworkSpikedArmorTimer =
                TickTimer.None;

            spikedArmorTargetCooldowns.Clear();
        }
    }
}
