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
                if (
                    existing.GetComponent<
                        ProjectJNetworkLobbyFlow
                    >() == null
                )
                {
                    existing.gameObject.AddComponent<
                        ProjectJNetworkLobbyFlow
                    >(); // 기존 Bootstrap에도 74일차 Flow 보장
                }

                if (
                    existing.GetComponent<
                        ProjectJPhase6GateDebugView
                    >() == null
                )
                {
                    existing.gameObject.AddComponent<
                        ProjectJPhase6GateDebugView
                    >(); // F3 Phase 6 통합 Gate 자동 설치
                }

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
            >(); // Lobby Ready → Game Flow 자동 설치

            bootstrapObject.AddComponent<
                ProjectJPhase6GateDebugView
            >(); // F3 Phase 6 통합 Gate 자동 설치
        }
    }
}
