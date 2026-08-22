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
        WaterGun = 5 // water_gun
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

                default:
                    return "Empty";
            }
        }
    }
}
