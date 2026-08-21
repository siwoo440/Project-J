using UnityEngine;

namespace ProjectJ.Networking.Fusion
{
    public static class
        ProjectJFusionBootstrapRuntimeInstaller
    {
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void Install()
        {
            ProjectJFusionBootstrap existing =
                Object.FindFirstObjectByType<
                    ProjectJFusionBootstrap
                >();

            if (existing != null)
            {
                return;
            }

            GameObject bootstrapObject =
                new GameObject(
                    "=== Project J Fusion Bootstrap ==="
                );

            Object.DontDestroyOnLoad(
                bootstrapObject
            );

            bootstrapObject.AddComponent<
                ProjectJFusionBootstrap
            >();

            bootstrapObject.AddComponent<
                ProjectJFusionBootstrapDebugView
            >();
        }
    }
}
