using ProjectJ.Items; // 기존 ItemDefinition 사용

namespace ProjectJ.Networking.Fusion
{
    public enum ProjectJNetworkItemId // 대표 아이템 네트워크 ID
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
        PoolBall = 10, // pool_ball
        Jetpack = 11, // jetpack
        Hammer = 12, // hammer
        Bomb = 13, // bomb
        PufferBalloonSuit = 14, // puffer_balloon_suit
        InkOctopus = 15, // ink_octopus
        FishingRod = 16, // fishing_rod
        GrapplingHook = 17, // grappling_hook
        SoapBubble = 18 // soap_bubble
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

                case "mine":
                    networkItemId = (int)ProjectJNetworkItemId.Mine;
                    return true;

                case "pool_ball":
                    networkItemId = (int)ProjectJNetworkItemId.PoolBall;
                    return true;

                case "jetpack":
                    networkItemId = (int)ProjectJNetworkItemId.Jetpack;
                    return true;

                case "hammer":
                    networkItemId = (int)ProjectJNetworkItemId.Hammer;
                    return true;

                case "bomb":
                    networkItemId = (int)ProjectJNetworkItemId.Bomb;
                    return true;

                case "puffer_balloon_suit":
                    networkItemId = (int)ProjectJNetworkItemId.PufferBalloonSuit;
                    return true;

                case "ink_octopus":
                    networkItemId = (int)ProjectJNetworkItemId.InkOctopus;
                    return true;

                case "fishing_rod":
                    networkItemId = (int)ProjectJNetworkItemId.FishingRod;
                    return true;

                case "grappling_hook":
                    networkItemId = (int)ProjectJNetworkItemId.GrapplingHook;
                    return true;

                case "soap_bubble":
                    networkItemId = (int)ProjectJNetworkItemId.SoapBubble;
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

                case ProjectJNetworkItemId.Firework:
                    return "firework";

                case ProjectJNetworkItemId.FeatherShoes:
                    return "feather_shoes";

                case ProjectJNetworkItemId.Snowball:
                    return "snowball";

                case ProjectJNetworkItemId.Mine:
                    return "mine";

                case ProjectJNetworkItemId.PoolBall:
                    return "pool_ball";

                case ProjectJNetworkItemId.Jetpack:
                    return "jetpack";

                case ProjectJNetworkItemId.Hammer:
                    return "hammer";

                case ProjectJNetworkItemId.Bomb:
                    return "bomb";

                case ProjectJNetworkItemId.PufferBalloonSuit:
                    return "puffer_balloon_suit";

                case ProjectJNetworkItemId.InkOctopus:
                    return "ink_octopus";

                case ProjectJNetworkItemId.FishingRod:
                    return "fishing_rod";

                case ProjectJNetworkItemId.GrapplingHook:
                    return "grappling_hook";

                case ProjectJNetworkItemId.SoapBubble:
                    return "soap_bubble";

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

                case ProjectJNetworkItemId.Mine:
                    return "지뢰";

                case ProjectJNetworkItemId.PoolBall:
                    return "풀 공";

                case ProjectJNetworkItemId.Jetpack:
                    return "제트팩";

                case ProjectJNetworkItemId.Hammer:
                    return "망치";

                case ProjectJNetworkItemId.Bomb:
                    return "폭탄";

                case ProjectJNetworkItemId.PufferBalloonSuit:
                    return "복어 풍선옷";

                case ProjectJNetworkItemId.InkOctopus:
                    return "먹물 문어";

                case ProjectJNetworkItemId.FishingRod:
                    return "낚시대";

                case ProjectJNetworkItemId.GrapplingHook:
                    return "갈고리";

                case ProjectJNetworkItemId.SoapBubble:
                    return "비눗방울";

                default:
                    return "Empty";
            }
        }
    }
}
