using System.Collections.Generic; // Player 후보 목록 사용
using ProjectJ.AI; // Bot 경쟁 판단 정책 사용
using UnityEngine; // MonoBehaviour와 Vector 타입 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProjectJNetworkPlayer))]
    [RequireComponent(typeof(ProjectJNetworkBotMarker))]
    public sealed class ProjectJNetworkBotActionController :
        MonoBehaviour
    {
        private const float PushMaximumDistance =
            1.35f; // Route 진행을 직접 막는 근접 상대 Push 거리

        private const float PushMinimumForwardDot =
            0.94f; // 거의 정면의 진행 방해 상대만 Push 허용

        private const float PushDecisionCooldownSeconds =
            4f; // Bot Push 판단 자체 재사용 간격

        private const float ItemOpponentAwarenessDistance =
            5f; // 공격형 Item 상대 탐색 거리 축소

        private const float ItemDecisionCooldownSeconds =
            4.5f; // Route 진행 우선 Item 판단 간격

        private const float FailedItemRetrySeconds =
            1.25f; // Item 사용 실패 재판단 간격

        private const float HeldItemDurationSeconds =
            0.9f; // 물총·저격 Hold 유지 시간

        private readonly List<ProjectJNetworkExternalGameplay> activePlayers =
            new List<ProjectJNetworkExternalGameplay>(); // 동일 Runner Player 후보 목록

        private ProjectJNetworkItemInventory itemInventory; // Bot Item 상태 조회 대상
        private float itemDecisionCooldown; // Item 판단 남은 시간
        private float pushDecisionCooldown; // Push 판단 남은 시간
        private float heldItemRemaining; // Hold Item 남은 시간

        public void TickActions(
            ProjectJNetworkPlayer player,
            ProjectJNetworkExternalGameplay externalGameplay
        )
        {
            if (
                player == null ||
                externalGameplay == null ||
                !player.HasLocalStateAuthority
            )
            {
                return; // State Authority Bot 외 경쟁 판단 차단
            }

            if (itemInventory == null)
            {
                itemInventory =
                    GetComponent<ProjectJNetworkItemInventory>(); // Bot Inventory 지연 조회
            }

            float deltaTime =
                player.Runner != null
                    ? Mathf.Max(
                        0f,
                        player.Runner.DeltaTime
                    )
                    : 0f; // Fusion Simulation 시간 조회

            itemDecisionCooldown =
                Mathf.Max(
                    0f,
                    itemDecisionCooldown -
                    deltaTime
                ); // Item 판단 쿨타임 감소

            pushDecisionCooldown =
                Mathf.Max(
                    0f,
                    pushDecisionCooldown -
                    deltaTime
                ); // Push 판단 쿨타임 감소

            if (!externalGameplay.GameplayInputAllowed)
            {
                ReleaseHeldItem(); // 경기 잠금 중 Hold Item 해제

                return; // 경기 외 Push·Item 판단 차단
            }

            ProjectJNetworkExternalGameplay nearestOpponent =
                FindNearestOpponent(
                    player,
                    externalGameplay,
                    out float nearestDistance,
                    out float nearestForwardDot
                ); // Route 전방 기준 가장 가까운 유효 상대 검색

            bool hasNearbyOpponent =
                nearestOpponent != null &&
                nearestDistance <=
                ItemOpponentAwarenessDistance; // Item 대상 탐지 여부 계산

            TryPushBlockingOpponent(
                player,
                externalGameplay,
                nearestOpponent,
                nearestDistance,
                nearestForwardDot
            ); // 바로 앞 진행 방해 상대만 Push 시도

            UpdateHeldItem(
                deltaTime
            ); // 물총·저격 Hold 입력 갱신

            TryUseItem(
                player,
                hasNearbyOpponent
            ); // Route 이동을 유지한 채 상황 기반 Item 사용 시도
        }

        private ProjectJNetworkExternalGameplay FindNearestOpponent(
            ProjectJNetworkPlayer player,
            ProjectJNetworkExternalGameplay ownGameplay,
            out float nearestDistance,
            out float nearestForwardDot
        )
        {
            nearestDistance =
                float.PositiveInfinity; // 최근접 상대 거리 초기화

            nearestForwardDot =
                -1f; // 최근접 상대 전방 Dot 초기화

            if (player.Runner == null)
            {
                return null; // Runner 없음 처리
            }

            ProjectJNetworkExternalGameplay.CollectActivePlayers(
                player.Runner,
                activePlayers
            ); // 동일 Runner 경쟁 Player 수집

            ProjectJNetworkExternalGameplay nearestOpponent =
                null; // 최근접 상대 초기화

            Vector3 forward =
                player.transform.forward; // 현재 Route 진행 방향과 같은 Bot 전방 조회

            forward.y =
                0f; // 수평 전방 사용

            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward =
                    Vector3.forward; // 유효하지 않은 전방 기본값 적용
            }
            else
            {
                forward.Normalize(); // Bot 전방 정규화
            }

            Vector3 currentPosition =
                player.CurrentPosition; // Bot 현재 위치 조회

            for (
                int index = 0;
                index < activePlayers.Count;
                index++
            )
            {
                ProjectJNetworkExternalGameplay candidate =
                    activePlayers[index]; // 현재 경쟁 상대 후보 조회

                if (
                    candidate == null ||
                    candidate == ownGameplay ||
                    candidate.Object == null ||
                    !candidate.Object.IsValid ||
                    candidate.IsFinished ||
                    candidate.IsResultLocked ||
                    candidate.IsRespawnProtected
                )
                {
                    continue; // 본인·무효·완주·보호 대상 제외
                }

                Vector3 targetDirection =
                    candidate.transform.position -
                    currentPosition; // 상대 방향 계산

                targetDirection.y =
                    0f; // 수평 상대 방향 사용

                float distance =
                    targetDirection.magnitude; // 상대 수평 거리 계산

                if (
                    distance <= 0.0001f ||
                    distance >= nearestDistance
                )
                {
                    continue; // 동일 위치·더 먼 상대 제외
                }

                Vector3 normalizedDirection =
                    targetDirection /
                    distance; // 상대 방향 정규화

                nearestOpponent =
                    candidate; // 최근접 상대 갱신

                nearestDistance =
                    distance; // 최근접 거리 갱신

                nearestForwardDot =
                    Vector3.Dot(
                        forward,
                        normalizedDirection
                    ); // 최근접 상대 전방 Dot 갱신
            }

            return nearestOpponent; // 최근접 유효 상대 반환
        }

        private void TryPushBlockingOpponent(
            ProjectJNetworkPlayer player,
            ProjectJNetworkExternalGameplay ownGameplay,
            ProjectJNetworkExternalGameplay opponent,
            float targetDistance,
            float targetForwardDot
        )
        {
            if (opponent == null)
            {
                return; // Push 대상 없음 처리
            }

            bool shouldPush =
                ProjectJBotCompetitionPolicy.ShouldAttemptProgressPush(
                    targetDistance,
                    PushMaximumDistance,
                    targetForwardDot,
                    PushMinimumForwardDot,
                    opponent.IsRespawnProtected,
                    opponent.IsFinished,
                    ownGameplay.PushCooldownRemaining,
                    pushDecisionCooldown
                ); // Route 진행 방해 상대 Push 상황 판단

            if (!shouldPush)
            {
                return; // Route 진행 우선으로 Push 보류
            }

            Vector3 pushForward =
                opponent.transform.position -
                player.CurrentPosition; // 진행을 막는 상대 방향 계산

            pushForward.y =
                0f; // 수평 Push 방향 사용

            if (
                ownGameplay.TryBotPushAuthority(
                    pushForward
                )
            )
            {
                pushDecisionCooldown =
                    PushDecisionCooldownSeconds; // 성공적인 Push 시 장시간 재시도 제한
            }
        }

        private void TryUseItem(
            ProjectJNetworkPlayer player,
            bool hasNearbyOpponent
        )
        {
            if (
                itemInventory == null ||
                itemDecisionCooldown > 0f
            )
            {
                return; // Inventory 없음·Item 판단 쿨타임 처리
            }

            int preferredSlot =
                ProjectJBotCompetitionPolicy.ResolvePreferredItemSlot(
                    itemInventory.SelectedItemId,
                    itemInventory.SelectedSlotIndex,
                    itemInventory.SlotLeftItemId,
                    itemInventory.SlotRightItemId
                ); // 현재 사용 가능한 Item 슬롯 계산

            if (preferredSlot < 0)
            {
                itemDecisionCooldown =
                    FailedItemRetrySeconds; // 빈 Inventory 재확인 간격 적용

                return; // 사용 Item 없음 처리
            }

            if (
                itemInventory.SelectedSlotIndex !=
                preferredSlot
            )
            {
                itemInventory.TryBotSelectSlotAuthority(
                    preferredSlot
                ); // 사용 가능한 슬롯 서버 선택
            }

            int selectedItemId =
                itemInventory.SelectedItemId; // 선택 Item ID 갱신 조회

            bool requiresOpponent =
                RequiresNearbyOpponent(
                    selectedItemId
                ); // 공격형 Item 여부 계산

            bool stableMovementState =
                player.IsGrounded; // Route 진행 중 안정적인 지상 상태 우선 사용

            bool shouldUse =
                ProjectJBotCompetitionPolicy.ShouldAttemptItemUse(
                    selectedItemId,
                    itemDecisionCooldown,
                    requiresOpponent,
                    hasNearbyOpponent,
                    stableMovementState
                ); // Item 사용 상황 판단

            if (!shouldUse)
            {
                itemDecisionCooldown =
                    FailedItemRetrySeconds; // 사용 조건 재평가 간격 적용

                return; // 현재 Item 사용 보류
            }

            bool attempted =
                itemInventory.TryBotUseSelectedItemAuthority(); // 기존 서버 Item 사용 처리 호출

            itemDecisionCooldown =
                attempted
                    ? ItemDecisionCooldownSeconds
                    : FailedItemRetrySeconds; // 사용 결과별 다음 판단 시간 설정

            if (
                attempted &&
                (
                    itemInventory.IsWaterGunActive ||
                    selectedItemId ==
                    (int)ProjectJNetworkItemId.SniperWaterGun
                )
            )
            {
                heldItemRemaining =
                    HeldItemDurationSeconds; // Hold형 Item 입력 유지 시작
            }
        }

        private void UpdateHeldItem(
            float deltaTime
        )
        {
            if (itemInventory == null)
            {
                return; // Inventory 없음 처리
            }

            if (heldItemRemaining <= 0f)
            {
                itemInventory.UpdateBotHeldItemAuthority(
                    false
                ); // Hold형 Item Release 상태 유지

                return;
            }

            heldItemRemaining =
                Mathf.Max(
                    0f,
                    heldItemRemaining -
                    deltaTime
                ); // Hold형 Item 남은 시간 감소

            itemInventory.UpdateBotHeldItemAuthority(
                heldItemRemaining > 0f
            ); // 기존 Hold Item 처리에 상태 전달
        }

        private void ReleaseHeldItem()
        {
            heldItemRemaining =
                0f; // Hold Item 시간 즉시 종료

            if (itemInventory != null)
            {
                itemInventory.UpdateBotHeldItemAuthority(
                    false
                ); // 물총·저격 Release 처리
            }
        }

        private static bool RequiresNearbyOpponent(
            int networkItemId
        )
        {
            ProjectJNetworkItemId itemId =
                (ProjectJNetworkItemId)networkItemId; // Network Item ID 변환

            switch (itemId)
            {
                case ProjectJNetworkItemId.BalloonHorn:
                case ProjectJNetworkItemId.WaterGun:
                case ProjectJNetworkItemId.Snowball:
                case ProjectJNetworkItemId.PoolBall:
                case ProjectJNetworkItemId.Hammer:
                case ProjectJNetworkItemId.Bomb:
                case ProjectJNetworkItemId.InkOctopus:
                case ProjectJNetworkItemId.FishingRod:
                case ProjectJNetworkItemId.GrapplingHook:
                case ProjectJNetworkItemId.SmokeGrenade:
                case ProjectJNetworkItemId.HomingMissile:
                case ProjectJNetworkItemId.Drone:
                case ProjectJNetworkItemId.SniperWaterGun:
                    return true; // 상대가 필요한 공격·방해 Item 분류

                default:
                    return false; // 이동·방어·설치·자기 강화 Item 분류
            }
        }
    }
}
