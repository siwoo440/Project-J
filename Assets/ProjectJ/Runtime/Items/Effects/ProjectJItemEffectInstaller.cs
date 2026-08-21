using UnityEngine;

namespace ProjectJ.Items.Effects
{
    public static class ProjectJItemEffectInstaller
    {
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad
        )]
        private static void RegisterEffects()
        {
            ItemUseEffectRegistry.Register(
                "spring_shoes",
                new SpringShoesEffect()
            );

            ItemUseEffectRegistry.Register(
                "jelly_shield",
                new JellyShieldEffect()
            );

            ItemUseEffectRegistry.Register(
                "banana_cushion",
                new BananaCushionEffect()
            );

            ItemUseEffectRegistry.Register(
                "balloon_horn",
                new BalloonHornEffect()
            );

            ItemUseEffectRegistry.Register(
                "water_gun",
                new WaterGunEffect()
            );
        }
    }
}
