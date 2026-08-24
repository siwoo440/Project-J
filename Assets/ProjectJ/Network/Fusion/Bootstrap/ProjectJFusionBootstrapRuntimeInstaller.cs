using ProjectJ.Networking; // 공통 네트워크 실행 정책 사용
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
            bool shouldInstall = // Host·Client Bootstrap 설치 여부
                ProjectJNetworkExecutionPolicy.ShouldInstallHostClientBootstrap( // 공통 실행 정책 호출
                    ProjectJNetworkExecutionPolicy.IsDedicatedServerBuild // 현재 Server 빌드 여부 전달
                );

            if (!shouldInstall) // Dedicated Server 빌드 확인
            {
                Debug.Log(
                    "[Project J/Day98] " +
                    "Dedicated Server 빌드 감지 / " +
                    "Host·Client Bootstrap 자동 설치 생략"
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
