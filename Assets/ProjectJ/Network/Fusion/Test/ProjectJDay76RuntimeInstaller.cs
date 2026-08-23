using UnityEngine; // Runtime 테스트 Flow 자동 설치
using UnityEngine.SceneManagement; // 최초 실행 Scene 확인

namespace ProjectJ.Networking.Fusion
{
    public static class ProjectJDay76RuntimeInstaller
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private static bool launchSceneCaptured;

        private static bool directGameLaunch;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration
        )]
        private static void ResetLaunchState()
        {
            launchSceneCaptured =
                false; // Play 시작마다 최초 Scene 판정 초기화

            directGameLaunch =
                false; // 직접 Game 실행 여부 초기화
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void Install()
        {
            Scene activeScene =
                SceneManager.GetActiveScene();

            if (!launchSceneCaptured)
            {
                launchSceneCaptured =
                    true; // 첫 Scene만 시작 Scene으로 기록

                directGameLaunch =
                    activeScene.IsValid() &&
                    activeScene.path ==
                        GameScenePath; // Game 직접 실행 여부 확정
            }

            if (
                !directGameLaunch ||
                !activeScene.IsValid() ||
                activeScene.path !=
                    GameScenePath
            )
            {
                return; // 정상 Bootstrap Flow에서는 Day76 Test Flow 차단
            }

            ProjectJDay76TestFlow existing =
                Object.FindFirstObjectByType<
                    ProjectJDay76TestFlow
                >();

            if (existing != null)
            {
                return;
            }

            GameObject flowObject =
                new GameObject(
                    "=== Project J Day76 Test Flow ==="
                );

            Object.DontDestroyOnLoad(
                flowObject
            );

            flowObject.AddComponent<
                ProjectJDay76TestFlow
            >(); // Game Scene 직접 실행에서만 테스트 Flow 생성
        }
    }
}
