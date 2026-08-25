using Fusion; // Networked와 TickTimer 사용
using ProjectJ.Items; // 저격 물총 정책 사용
using UnityEngine; // Raycast와 Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public enum ProjectJSniperWaterGunShotResult
    {
        None = 0,
        Miss = 1,
        BlockedByWorld = 2,
        HitApplied = 3,
        HitProtected = 4,
        Cancelled = 5
    }

    public sealed partial class ProjectJNetworkItemInventory
    {
        private const float SniperStandingEyeHeight = 1.5f;
        private const float SniperCrouchingEyeHeight = 0.85f;
        private const int SniperHitBufferSize = 64;

        private readonly RaycastHit[] sniperHitBuffer =
            new RaycastHit[SniperHitBufferSize];

        [Networked]
        private NetworkBool NetworkSniperWaterGunAiming
        {
            get;
            set;
        }

        [Networked]
        private TickTimer NetworkSniperWaterGunPreparationTimer
        {
            get;
            set;
        }

        [Networked]
        private int NetworkSniperWaterGunSlotIndex
        {
            get;
            set;
        }

        [Networked]
        private int NetworkSniperWaterGunStartRespawnCount
        {
            get;
            set;
        }

        [Networked]
        private int NetworkSniperWaterGunRevision
        {
            get;
            set;
        }

        [Networked]
        private int NetworkSniperWaterGunShotCount
        {
            get;
            set;
        }

        [Networked]
        private int NetworkSniperWaterGunHitCount
        {
            get;
            set;
        }

        [Networked]
        private int NetworkSniperWaterGunCancellationCount
        {
            get;
            set;
        }

        [Networked]
        private int NetworkSniperWaterGunLastResult
        {
            get;
            set;
        }

        public bool IsSniperWaterGunAiming =>
            NetworkSniperWaterGunAiming;

        public float SniperWaterGunPreparationRemaining
        {
            get
            {
                if (
                    Runner == null ||
                    !IsSniperWaterGunAiming
                )
                {
                    return 0f;
                }

                float? remaining =
                    NetworkSniperWaterGunPreparationTimer.RemainingTime(
                        Runner
                    );

                return
                    remaining.HasValue
                        ? Mathf.Max(
                            0f,
                            remaining.Value
                        )
                        : 0f;
            }
        }

        public float SniperWaterGunPreparationProgress01 =>
            IsSniperWaterGunAiming
                ? ProjectJSniperWaterGunPolicy.CalculatePreparationProgress(
                    SniperWaterGunPreparationRemaining
                )
                : 0f;

        public int SniperWaterGunShotCount =>
            NetworkSniperWaterGunShotCount;

        public int SniperWaterGunHitCount =>
            NetworkSniperWaterGunHitCount;

        public int SniperWaterGunCancellationCount =>
            NetworkSniperWaterGunCancellationCount;

        public ProjectJSniperWaterGunShotResult SniperWaterGunLastResult =>
            (ProjectJSniperWaterGunShotResult)NetworkSniperWaterGunLastResult;

        private void EnsureSniperWaterGunPresentation()
        {
            if (
                GetComponent<ProjectJSniperWaterGunLocalPresentation>() ==
                null
            )
            {
                gameObject.AddComponent<ProjectJSniperWaterGunLocalPresentation>();
            }
        }

        private void InitializeSniperWaterGunAuthority()
        {
            NetworkSniperWaterGunAiming =
                false;

            NetworkSniperWaterGunPreparationTimer =
                TickTimer.None;

            NetworkSniperWaterGunSlotIndex =
                -1;

            NetworkSniperWaterGunStartRespawnCount =
                0;

            NetworkSniperWaterGunRevision =
                0;

            NetworkSniperWaterGunShotCount =
                0;

            NetworkSniperWaterGunHitCount =
                0;

            NetworkSniperWaterGunCancellationCount =
                0;

            NetworkSniperWaterGunLastResult =
                (int)ProjectJSniperWaterGunShotResult.None;
        }

        private bool TryHandleSniperWaterGunUseInputAuthority()
        {
            int slotIndex =
                Mathf.Clamp(
                    NetworkSelectedSlotIndex,
                    0,
                    SlotCount - 1
                );

            int itemId =
                GetItemId(
                    slotIndex
                );

            if (
                itemId !=
                ProjectJSniperWaterGunPolicy.NetworkItemId
            )
            {
                return false;
            }

            ResolveReferences();

            bool authorityReady =
                Runner != null &&
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority;

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            if (
                !ProjectJSniperWaterGunPolicy.CanBeginAim(
                    authorityReady,
                    gameplayAllowed,
                    IsSniperWaterGunAiming,
                    true
                )
            )
            {
                NetworkUseFailCount++;
                return true;
            }

            NetworkSniperWaterGunAiming =
                true;

            NetworkSniperWaterGunPreparationTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    ProjectJSniperWaterGunPolicy.PreparationSeconds
                );

            NetworkSniperWaterGunSlotIndex =
                slotIndex;

            NetworkSniperWaterGunStartRespawnCount =
                externalGameplay.RespawnCount;

            NetworkSniperWaterGunLastResult =
                (int)ProjectJSniperWaterGunShotResult.None;

            NetworkSniperWaterGunRevision++;

            return true;
        }

        private void UpdateSniperWaterGunLifecycleAuthority()
        {
            if (!IsSniperWaterGunAiming)
            {
                return;
            }

            ResolveReferences();

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            bool slotStillContainsSniper =
                NetworkSniperWaterGunSlotIndex >= 0 &&
                NetworkSniperWaterGunSlotIndex < SlotCount &&
                GetItemId(
                    NetworkSniperWaterGunSlotIndex
                ) ==
                ProjectJSniperWaterGunPolicy.NetworkItemId;

            bool sameRespawnLife =
                externalGameplay != null &&
                externalGameplay.RespawnCount ==
                NetworkSniperWaterGunStartRespawnCount;

            if (
                !gameplayAllowed ||
                !slotStillContainsSniper ||
                !sameRespawnLife
            )
            {
                CancelSniperWaterGunAimAuthority(
                    true
                );
            }
        }

        private void UpdateSniperWaterGunInputAuthority(
            ProjectJNetworkInput input
        )
        {
            if (
                !IsSniperWaterGunAiming ||
                Runner == null
            )
            {
                return;
            }

            ResolveReferences();

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            bool useHeld =
                input.Buttons.IsSet(
                    ProjectJNetworkButton.ItemUseHeld
                );

            bool selectedSlotMatches =
                NetworkSelectedSlotIndex ==
                NetworkSniperWaterGunSlotIndex;

            bool slotStillContainsSniper =
                NetworkSniperWaterGunSlotIndex >= 0 &&
                NetworkSniperWaterGunSlotIndex < SlotCount &&
                GetItemId(
                    NetworkSniperWaterGunSlotIndex
                ) ==
                ProjectJSniperWaterGunPolicy.NetworkItemId;

            bool sameRespawnLife =
                externalGameplay != null &&
                externalGameplay.RespawnCount ==
                NetworkSniperWaterGunStartRespawnCount;

            if (
                ProjectJSniperWaterGunPolicy.ShouldCancelAim(
                    gameplayAllowed,
                    useHeld,
                    selectedSlotMatches,
                    slotStillContainsSniper,
                    sameRespawnLife
                )
            )
            {
                CancelSniperWaterGunAimAuthority(
                    true
                );

                return;
            }

            if (
                !NetworkSniperWaterGunPreparationTimer.ExpiredOrNotRunning(
                    Runner
                )
            )
            {
                return;
            }

            FireSniperWaterGunAuthority(
                input.AimDirection
            );
        }

        private void FireSniperWaterGunAuthority(
            Vector3 requestedAimDirection
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed ||
                NetworkSniperWaterGunSlotIndex < 0 ||
                NetworkSniperWaterGunSlotIndex >= SlotCount ||
                GetItemId(
                    NetworkSniperWaterGunSlotIndex
                ) !=
                ProjectJSniperWaterGunPolicy.NetworkItemId
            )
            {
                CancelSniperWaterGunAimAuthority(
                    true
                );

                return;
            }

            Vector3 aimDirection =
                ProjectJSniperWaterGunPolicy.ResolveAimDirection(
                    requestedAimDirection,
                    transform.forward
                );

            float eyeHeight =
                networkPlayer != null &&
                networkPlayer.IsCrouching
                    ? SniperCrouchingEyeHeight
                    : SniperStandingEyeHeight;

            Vector3 origin =
                transform.position +
                Vector3.up *
                eyeHeight;

            int hitCount =
                Physics.RaycastNonAlloc(
                    origin,
                    aimDirection,
                    sniperHitBuffer,
                    ProjectJSniperWaterGunPolicy.RangeMeters,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore
                );

            bool foundHit =
                TryFindClosestSniperHit(
                    hitCount,
                    out RaycastHit closestHit
                );

            ProjectJSniperWaterGunShotResult result =
                ProjectJSniperWaterGunShotResult.Miss;

            if (foundHit)
            {
                ProjectJNetworkExternalGameplay target =
                    closestHit.collider.GetComponentInParent<ProjectJNetworkExternalGameplay>();

                if (
                    target == null ||
                    target == externalGameplay
                )
                {
                    result =
                        ProjectJSniperWaterGunShotResult.BlockedByWorld;
                }
                else
                {
                    Vector3 velocityChange =
                        ProjectJSniperWaterGunPolicy.CreateHorizontalVelocityChange(
                            aimDirection,
                            transform.forward
                        );

                    bool applied =
                        target.TryApplyExternalVelocityChange(
                            ProjectJExternalForceSource.Item,
                            velocityChange
                        );

                    result =
                        applied
                            ? ProjectJSniperWaterGunShotResult.HitApplied
                            : ProjectJSniperWaterGunShotResult.HitProtected;

                    if (applied)
                    {
                        NetworkSniperWaterGunHitCount++;
                    }
                }
            }

            ConsumeSniperWaterGunShotAuthority(
                result
            );
        }

        private bool TryFindClosestSniperHit(
            int hitCount,
            out RaycastHit closestHit
        )
        {
            closestHit =
                default;

            float closestDistance =
                float.PositiveInfinity;

            bool found =
                false;

            int safeCount =
                Mathf.Clamp(
                    hitCount,
                    0,
                    sniperHitBuffer.Length
                );

            for (
                int i = 0;
                i < safeCount;
                i++
            )
            {
                RaycastHit candidate =
                    sniperHitBuffer[i];

                Collider hitCollider =
                    candidate.collider;

                if (hitCollider == null)
                {
                    continue;
                }

                Transform hitTransform =
                    hitCollider.transform;

                if (
                    hitTransform == transform ||
                    hitTransform.IsChildOf(transform)
                )
                {
                    continue;
                }

                if (
                    candidate.distance < 0f ||
                    candidate.distance >= closestDistance
                )
                {
                    continue;
                }

                closestDistance =
                    candidate.distance;

                closestHit =
                    candidate;

                found =
                    true;
            }

            return found;
        }

        private void ConsumeSniperWaterGunShotAuthority(
            ProjectJSniperWaterGunShotResult result
        )
        {
            int slotIndex =
                NetworkSniperWaterGunSlotIndex;

            BreakInvisibilityCloakForSuccessfulItemUseAuthority(
                ProjectJSniperWaterGunPolicy.NetworkItemId
            );

            SetSlotItemIdAuthority(
                slotIndex,
                EmptyItemId
            );

            NetworkInventoryRevision++;
            NetworkUseSuccessCount++;
            NetworkLastUsedItemId =
                ProjectJSniperWaterGunPolicy.NetworkItemId;

            NetworkSniperWaterGunShotCount++;
            NetworkSniperWaterGunLastResult =
                (int)result;

            ClearSniperWaterGunAimStateAuthority();
        }

        private void CancelSniperWaterGunAimAuthority(
            bool countCancellation
        )
        {
            if (!IsSniperWaterGunAiming)
            {
                return;
            }

            if (countCancellation)
            {
                NetworkSniperWaterGunCancellationCount++;
            }

            NetworkSniperWaterGunLastResult =
                (int)ProjectJSniperWaterGunShotResult.Cancelled;

            ClearSniperWaterGunAimStateAuthority();
        }

        private void ClearSniperWaterGunAuthority()
        {
            if (!IsSniperWaterGunAiming)
            {
                NetworkSniperWaterGunPreparationTimer =
                    TickTimer.None;

                NetworkSniperWaterGunSlotIndex =
                    -1;

                return;
            }

            ClearSniperWaterGunAimStateAuthority();
        }

        private void ClearSniperWaterGunAimStateAuthority()
        {
            NetworkSniperWaterGunAiming =
                false;

            NetworkSniperWaterGunPreparationTimer =
                TickTimer.None;

            NetworkSniperWaterGunSlotIndex =
                -1;

            NetworkSniperWaterGunRevision++;
        }
    }
}
