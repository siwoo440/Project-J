using UnityEngine; // Mathf 사용

namespace ProjectJ.AI
{
    public static class ProjectJBotSpawnPolicy
    {
        public static bool IsSpawnSlotClear(
            float nearestParticipantDistance,
            float minimumClearanceDistance
        )
        {
            if (float.IsNaN(nearestParticipantDistance))
            {
                return false; // 잘못된 거리값 Spawn 차단
            }

            float safeClearance =
                Mathf.Max(
                    0f,
                    minimumClearanceDistance
                ); // Spawn 최소 간격 보정

            return
                nearestParticipantDistance >=
                safeClearance; // 기존 참가자와 충분히 떨어진 Slot만 허용
        }

        public static float ResolveStartDelaySeconds(
            int existingBotCount,
            float intervalSeconds,
            float maximumDelaySeconds
        )
        {
            int safeBotCount =
                Mathf.Max(
                    0,
                    existingBotCount
                ); // 기존 Bot 수 보정

            float safeInterval =
                Mathf.Max(
                    0f,
                    intervalSeconds
                ); // Bot 출발 간격 보정

            float safeMaximumDelay =
                Mathf.Max(
                    0f,
                    maximumDelaySeconds
                ); // 최대 지연 시간 보정

            return Mathf.Min(
                safeBotCount *
                safeInterval,
                safeMaximumDelay
            ); // Bot마다 순차 출발 지연 계산
        }
    }
}
