namespace ProjectJ.Items // 아이템 공통 정책 네임스페이스
{
    public static class ProjectJPoolBallPolicy // 111일차 풀 공 공통 정책
    {
        public const int MaximumStackCount = 5; // 한 슬롯 최대 수량
        public const float HitForce = 4f; // 적중 수평 외력
        public const float MaximumTravelDistance = 28f; // 최대 이동 거리
        public const float CollisionRadius = 0.24f; // 충돌 반경
        public const float ProjectileSpeed = 16f; // 투사체 초당 속도

        public static int ClampStackCount(int count) // Stack 수량 범위 보정
        {
            if (count <= 0) // 0 이하 수량 확인
            {
                return 0; // 빈 Stack 반환
            }

            if (count >= MaximumStackCount) // 최대 수량 이상 확인
            {
                return MaximumStackCount; // 최대 5개 제한
            }

            return count; // 정상 수량 반환
        }

        public static bool CanAddOne(int currentCount) // Pickup 1개 합산 가능 여부
        {
            return ClampStackCount(currentCount) < MaximumStackCount; // 최대 수량 미만 판정
        }

        public static int AddOne(int currentCount) // Pickup 1개 합산 결과
        {
            return ClampStackCount(currentCount + 1); // 최대 5개로 합산
        }

        public static bool CanConsumeOne(int currentCount) // 투척 가능 수량 여부
        {
            return ClampStackCount(currentCount) > 0; // 1개 이상 보유 판정
        }

        public static int ConsumeOne(int currentCount) // 투척 1회 소비 결과
        {
            return ClampStackCount(currentCount - 1); // 수량 1개 감소
        }

        public static bool HasReachedTravelLimit(float travelledDistance) // 최대 이동 거리 도달 여부
        {
            return travelledDistance >= MaximumTravelDistance; // 28m 이상 판정
        }
    }
}
