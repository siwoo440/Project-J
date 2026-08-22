using UnityEngine; // Runtime 테스트 Flow 자동 설치
using UnityEngine.SceneManagement; // 직접 Game Scene 실행 여부 확인

namespace ProjectJ.Networking.Fusion
{
    public static class ProjectJDay76RuntimeInstaller
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void Install()
        {
            Scene activeScene =
                SceneManager.GetActiveScene();

            if (
                !activeScene.IsValid() ||
                activeScene.path !=
                    GameScenePath
            )
            {
                return;
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
            >();
        }
    }
}
