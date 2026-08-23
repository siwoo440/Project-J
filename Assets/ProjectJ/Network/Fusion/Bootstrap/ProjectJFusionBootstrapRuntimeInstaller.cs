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
            ProjectJDay96ServerModeBootstrap
                serverModeBootstrap =
                    Object.FindFirstObjectByType<
                        ProjectJDay96ServerModeBootstrap
                    >();

            if (serverModeBootstrap != null)
            {
                Debug.Log(
                    "[Project J/Day96] " +
                    "Server Mode Scene 감지 / " +
                    "일반 Host·Client Bootstrap 자동 설치 생략"
                );

                return;
            }

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

            bootstrapObject.AddComponent<
                ProjectJDay94SceneFlowGuard
            >(); // 실제 UI Scene 전환 잠금·수명 점검 자동 설치
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

            if (
                bootstrapObject.GetComponent<
                    ProjectJDay94SceneFlowGuard
                >() == null
            )
            {
                bootstrapObject.AddComponent<
                    ProjectJDay94SceneFlowGuard
                >(); // 기존 영구 Bootstrap에도 Day94 Guard 보장
            }
        }
    }
}
