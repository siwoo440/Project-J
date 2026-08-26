using UnityEngine; // Mathf 사용

namespace ProjectJ.AI
{
    public static class ProjectJBotCompetitionPolicy
    {
        public static bool ShouldAttemptPush(
            float targetDistance,
            float maximumDistance,
            float forwardDot,
            float minimumForwardDot,
            bool targetProtected,
            bool targetFinished,
            float cooldownRemaining
        )
        {
            if (
                targetProtected ||
                targetFinished ||
                cooldownRemaining > 0f
            )
            {
                return false; // 보호·완주·쿨타임 대상 Push 차단
            }

            float safeMaximumDistance =
                Mathf.Max(
                    0f,
                    maximumDistance
                ); // Push 최대 거리 보정

            float safeMinimumForwardDot =
                Mathf.Clamp(
                    minimumForwardDot,
                    -1f,
                    1f
                ); // 전방 Dot 기준 보정

            return
                targetDistance >= 0f &&
                targetDistance <= safeMaximumDistance &&
                forwardDot >= safeMinimumForwardDot; // 거리·전방 조건 Push 판정
        }

        public static bool ShouldAttemptProgressPush(
            float targetDistance,
            float maximumDistance,
            float forwardDot,
            float minimumForwardDot,
            bool targetProtected,
            bool targetFinished,
            float pushCooldownRemaining,
            float botDecisionCooldownRemaining
        )
        {
            if (botDecisionCooldownRemaining > 0f)
            {
                return false; // Bot 자체 Push 판단 간격 유지
            }

            return ShouldAttemptPush(
                targetDistance,
                maximumDistance,
                forwardDot,
                minimumForwardDot,
                targetProtected,
                targetFinished,
                pushCooldownRemaining
            ); // Route 진행을 직접 막는 근접 상대만 Push 판정
        }

        public static int ResolveDesiredBotCount(
            int targetParticipantCount,
            int humanCount,
            int maximumBotCount
        )
        {
            int safeTargetCount =
                Mathf.Max(
                    0,
                    targetParticipantCount
                ); // 목표 참가 인원 보정

            int safeHumanCount =
                Mathf.Max(
                    0,
                    humanCount
                ); // 실제 Player 인원 보정

            int safeMaximumBotCount =
                Mathf.Max(
                    0,
                    maximumBotCount
                ); // 최대 Bot 수 보정

            return Mathf.Clamp(
                safeTargetCount -
                safeHumanCount,
                0,
                safeMaximumBotCount
            ); // 부족 인원만 Bot 수로 계산
        }

        public static bool IsRosterFilled(
            int targetParticipantCount,
            int humanCount,
            int botCount
        )
        {
            int safeTargetCount =
                Mathf.Max(
                    1,
                    targetParticipantCount
                ); // 최소 목표 참가 인원 보정

            int safeHumanCount =
                Mathf.Max(
                    0,
                    humanCount
                ); // Human 인원 보정

            int safeBotCount =
                Mathf.Max(
                    0,
                    botCount
                ); // Bot 인원 보정

            return
                safeHumanCount > 0 &&
                safeHumanCount +
                safeBotCount ==
                safeTargetCount; // 최소 한 Human과 정확한 전체 충원 확인
        }

        public static bool ShouldStartCountdown(
            bool rosterFilled,
            float filledSeconds,
            float requiredStableSeconds
        )
        {
            if (!rosterFilled)
            {
                return false; // Roster 미충원 Countdown 차단
            }

            float safeFilledSeconds =
                Mathf.Max(
                    0f,
                    filledSeconds
                ); // 충원 유지 시간 보정

            float safeRequiredStableSeconds =
                Mathf.Max(
                    0f,
                    requiredStableSeconds
                ); // 안정화 요구 시간 보정

            return
                safeFilledSeconds >=
                safeRequiredStableSeconds; // 안정화 시간 충족 후 Countdown 허용
        }

        public static int ResolvePreferredItemSlot(
            int selectedItemId,
            int selectedSlotIndex,
            int leftItemId,
            int rightItemId
        )
        {
            if (
                selectedItemId > 0 &&
                (
                    selectedSlotIndex == 0 ||
                    selectedSlotIndex == 1
                )
            )
            {
                return selectedSlotIndex; // 현재 유효 선택 슬롯 유지
            }

            if (leftItemId > 0)
            {
                return 0; // 왼쪽 슬롯 우선 선택
            }

            if (rightItemId > 0)
            {
                return 1; // 오른쪽 슬롯 대체 선택
            }

            return -1; // 사용 가능한 Item 없음
        }

        public static bool ShouldAttemptItemUse(
            int selectedItemId,
            float cooldownRemaining,
            bool requiresOpponent,
            bool hasNearbyOpponent,
            bool stableMovementState
        )
        {
            if (
                selectedItemId <= 0 ||
                cooldownRemaining > 0f ||
                !stableMovementState
            )
            {
                return false; // 빈 슬롯·쿨타임·불안정 이동 상태 사용 차단
            }

            if (
                requiresOpponent &&
                !hasNearbyOpponent
            )
            {
                return false; // 공격형 Item 대상 없음 차단
            }

            return true; // 현재 상황 Item 사용 허용
        }
    }
}
