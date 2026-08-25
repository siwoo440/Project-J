using Fusion; // NetworkObject와 TickTimer 사용
using ProjectJ.Items; // 비눗방울 정책 사용
using UnityEngine; // Vector3와 GameObject 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        private const string SoapBubbleProjectileResourcePath =
            "ProjectJNetworkSoapBubbleProjectile";

        private NetworkObject soapBubbleProjectilePrefab;

        [Networked]
        private TickTimer NetworkSoapBubbleTimer
        {
            get;
            set;
        }

        [Networked]
        private int NetworkSoapBubbleJumpPressCount
        {
            get;
            set;
        }

        [Networked]
        private NetworkBool NetworkSoapBubblePreviousJumpPressed
        {
            get;
            set;
        }

        public bool IsSoapBubbleActive =>
            IsTimerActive(
                NetworkSoapBubbleTimer
            );

        public float SoapBubbleRemaining =>
            GetRemainingTime(
                NetworkSoapBubbleTimer
            );

        public int SoapBubbleJumpPressCount =>
            NetworkSoapBubbleJumpPressCount;

        private void InitializeSoapBubbleAuthority()
        {
            NetworkSoapBubbleTimer =
                TickTimer.None;
            NetworkSoapBubbleJumpPressCount =
                0;
            NetworkSoapBubblePreviousJumpPressed =
                false;
        }

        private bool UseSoapBubbleAuthority()
        {
            if (
                Runner == null ||
                !Runner.IsServer ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed
            )
            {
                return false;
            }

            NetworkObject projectilePrefab =
                ResolveSoapBubbleProjectilePrefab();

            if (projectilePrefab == null)
            {
                Debug.LogError(
                    "[Project J/Fusion] 119일차 비눗방울 Prefab을 찾을 수 없음",
                    this
                );

                return false;
            }

            Vector3 forward =
                transform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward =
                    Vector3.forward;
            }

            forward.Normalize();

            Vector3 spawnPosition =
                transform.position +
                Vector3.up * 1.2f +
                forward * 0.9f;

            NetworkObject projectileObject =
                Runner.Spawn(
                    projectilePrefab,
                    spawnPosition,
                    Quaternion.LookRotation(forward),
                    Object.InputAuthority
                );

            if (projectileObject == null)
            {
                return false;
            }

            ProjectJNetworkSoapBubbleProjectile projectile =
                projectileObject.GetComponent<ProjectJNetworkSoapBubbleProjectile>();

            if (
                projectile == null ||
                !projectile.ConfigureAuthority(
                    Object.InputAuthority,
                    forward
                )
            )
            {
                Runner.Despawn(
                    projectileObject
                );
                return false;
            }

            return true;
        }

        internal bool ApplySoapBubbleAuthority()
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return false;
            }

            bool wasActive =
                IsSoapBubbleActive;

            float duration =
                ProjectJSoapBubblePolicy.GetRefreshedDuration(
                    SoapBubbleRemaining
                );

            NetworkSoapBubbleTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    duration
                );

            if (!wasActive)
            {
                NetworkSoapBubbleJumpPressCount =
                    0;
            }

            return true;
        }

        private void UpdateSoapBubbleLifetimeAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            if (!gameplayAllowed)
            {
                ClearSoapBubbleAuthority();
                return;
            }

            if (!IsSoapBubbleActive)
            {
                ClearSoapBubbleEffectAuthority();
            }
        }

        private void UpdateSoapBubbleJumpInputAuthority(
            ProjectJNetworkInput input
        )
        {
            bool jumpPressed =
                input.Buttons.IsSet(
                    ProjectJNetworkButton.Jump
                );

            bool shouldCount =
                ProjectJSoapBubblePolicy.ShouldCountJumpPress(
                    IsSoapBubbleActive,
                    jumpPressed,
                    NetworkSoapBubblePreviousJumpPressed
                );

            NetworkSoapBubblePreviousJumpPressed =
                jumpPressed;

            if (!shouldCount)
            {
                return;
            }

            NetworkSoapBubbleJumpPressCount =
                ProjectJSoapBubblePolicy.GetNextJumpPressCount(
                    NetworkSoapBubbleJumpPressCount
                );

            if (
                ProjectJSoapBubblePolicy.HasEscaped(
                    NetworkSoapBubbleJumpPressCount
                )
            )
            {
                ClearSoapBubbleEffectAuthority();
            }
        }

        private void ClearSoapBubbleEffectAuthority()
        {
            NetworkSoapBubbleTimer =
                TickTimer.None;
            NetworkSoapBubbleJumpPressCount =
                0;
        }

        private void ClearSoapBubbleAuthority()
        {
            ClearSoapBubbleEffectAuthority();
            NetworkSoapBubblePreviousJumpPressed =
                false;
        }

        private NetworkObject ResolveSoapBubbleProjectilePrefab()
        {
            if (soapBubbleProjectilePrefab == null)
            {
                GameObject projectilePrefabObject =
                    Resources.Load<GameObject>(
                        SoapBubbleProjectileResourcePath
                    );

                soapBubbleProjectilePrefab =
                    projectilePrefabObject != null
                        ? projectilePrefabObject.GetComponent<NetworkObject>()
                        : null;
            }

            return soapBubbleProjectilePrefab;
        }
    }
}
