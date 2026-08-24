using ProjectJ.Items; // 기존 ItemDefinition 사용

namespace ProjectJ.Networking.Fusion
{
    public enum ProjectJNetworkItemId // 73일차 대표 아이템 네트워크 ID
    {
        None = 0, // 빈 슬롯
        SpringShoes = 1, // spring_shoes
        JellyShield = 2, // jelly_shield
        BananaCushion = 3, // banana_cushion
        BalloonHorn = 4, // balloon_horn
        WaterGun = 5, // water_gun
        Firework = 6, // firework
        FeatherShoes = 7, // feather_shoes
        Snowball = 8, // snowball
        Mine = 9, // mine
        PoolBall = 10 // pool_ball
    }

    public static class ProjectJNetworkItemCatalog
    {
        public static bool TryGetNetworkId(
            ItemDefinition definition,
            out int networkItemId
        )
        {
            networkItemId = (int)ProjectJNetworkItemId.None;

            if (
                definition == null ||
                string.IsNullOrWhiteSpace(definition.ItemId)
            )
            {
                return false;
            }

            return TryGetNetworkId(
                definition.ItemId,
                out networkItemId
            );
        }

        public static bool TryGetNetworkId(
            string itemId,
            out int networkItemId
        )
        {
            networkItemId = (int)ProjectJNetworkItemId.None;

            switch (itemId)
            {
                case "spring_shoes":
                    networkItemId = (int)ProjectJNetworkItemId.SpringShoes;
                    return true;

                case "jelly_shield":
                    networkItemId = (int)ProjectJNetworkItemId.JellyShield;
                    return true;

                case "banana_cushion":
                    networkItemId = (int)ProjectJNetworkItemId.BananaCushion;
                    return true;

                case "balloon_horn":
                    networkItemId = (int)ProjectJNetworkItemId.BalloonHorn;
                    return true;

                case "water_gun":
                    networkItemId = (int)ProjectJNetworkItemId.WaterGun;
                    return true;

                case "firework":
                    networkItemId = (int)ProjectJNetworkItemId.Firework;
                    return true;

                case "feather_shoes":
                    networkItemId = (int)ProjectJNetworkItemId.FeatherShoes;
                    return true;

                case "snowball":
                    networkItemId = (int)ProjectJNetworkItemId.Snowball;
                    return true;

                case "mine": // 지뢰 문자열 ID
                    networkItemId = (int)ProjectJNetworkItemId.Mine; // 지뢰 네트워크 ID 저장
                    return true; // 문자열 변환 성공

                case "pool_ball": // 풀 공 문자열 ID
                    networkItemId = (int)ProjectJNetworkItemId.PoolBall; // 풀 공 네트워크 ID 저장
                    return true; // 문자열 변환 성공

                default:
                    return false;
            }
        }

        public static string GetKey(int networkItemId)
        {
            switch ((ProjectJNetworkItemId)networkItemId)
            {
                case ProjectJNetworkItemId.SpringShoes:
                    return "spring_shoes";

                case ProjectJNetworkItemId.JellyShield:
                    return "jelly_shield";

                case ProjectJNetworkItemId.BananaCushion:
                    return "banana_cushion";

                case ProjectJNetworkItemId.BalloonHorn:
                    return "balloon_horn";

                case ProjectJNetworkItemId.WaterGun:
                    return "water_gun";

                case ProjectJNetworkItemId.Firework:
                    return "firework";

                case ProjectJNetworkItemId.FeatherShoes:
                    return "feather_shoes";

                case ProjectJNetworkItemId.Snowball:
                    return "snowball";

                case ProjectJNetworkItemId.Mine: // 지뢰 네트워크 ID
                    return "mine"; // 지뢰 문자열 Key 반환

                case ProjectJNetworkItemId.PoolBall: // 풀 공 네트워크 ID
                    return "pool_ball"; // 풀 공 문자열 Key 반환

                default:
                    return "empty";
            }
        }

        public static string GetDisplayName(int networkItemId)
        {
            switch ((ProjectJNetworkItemId)networkItemId)
            {
                case ProjectJNetworkItemId.SpringShoes:
                    return "스프링 신발";

                case ProjectJNetworkItemId.JellyShield:
                    return "젤리 보호막";

                case ProjectJNetworkItemId.BananaCushion:
                    return "바나나 쿠션";

                case ProjectJNetworkItemId.BalloonHorn:
                    return "풍선 나팔";

                case ProjectJNetworkItemId.WaterGun:
                    return "물총";

                case ProjectJNetworkItemId.Firework:
                    return "폭죽";

                case ProjectJNetworkItemId.FeatherShoes:
                    return "깃털 신발";

                case ProjectJNetworkItemId.Snowball:
                    return "눈덩이";

                case ProjectJNetworkItemId.Mine: // 지뢰 네트워크 ID
                    return "지뢰"; // 지뢰 표시 이름 반환

                case ProjectJNetworkItemId.PoolBall: // 풀 공 네트워크 ID
                    return "풀 공"; // 풀 공 표시 이름 반환

                default:
                    return "Empty";
            }
        }
    }
}
