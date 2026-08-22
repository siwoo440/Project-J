using UnityEngine; // Runtime 구성요소 자동 설치

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
                EnsureRequiredComponents(
                    existing.gameObject
                );

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

            bootstrapObject.AddComponent<
                ProjectJNetworkLobbyFlow
            >();

            bootstrapObject.AddComponent<
                ProjectJPhase6GateDebugView
            >();

            bootstrapObject.AddComponent<
                ProjectJDay82SceneFlowCoordinator
            >();

            bootstrapObject.AddComponent<
                ProjectJDay82SceneFlowDebugView
            >();
        }

        private static void
            EnsureRequiredComponents(
                GameObject bootstrapObject
            )
        {
            if (
                bootstrapObject.GetComponent<
                    ProjectJNetworkLobbyFlow
                >() == null
            )
            {
                bootstrapObject.AddComponent<
                    ProjectJNetworkLobbyFlow
                >();
            }

            if (
                bootstrapObject.GetComponent<
                    ProjectJPhase6GateDebugView
                >() == null
            )
            {
                bootstrapObject.AddComponent<
                    ProjectJPhase6GateDebugView
                >();
            }

            if (
                bootstrapObject.GetComponent<
                    ProjectJDay82SceneFlowCoordinator
                >() == null
            )
            {
                bootstrapObject.AddComponent<
                    ProjectJDay82SceneFlowCoordinator
                >();
            }

            if (
                bootstrapObject.GetComponent<
                    ProjectJDay82SceneFlowDebugView
                >() == null
            )
            {
                bootstrapObject.AddComponent<
                    ProjectJDay82SceneFlowDebugView
                >();
            }
        }
    }
}
