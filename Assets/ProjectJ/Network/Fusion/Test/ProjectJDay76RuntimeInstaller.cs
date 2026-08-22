using UnityEngine; // Runtime 테스트 Flow 자동 설치

namespace ProjectJ.Networking.Fusion
{
    public static class ProjectJDay76RuntimeInstaller
    {
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad
        )]
        private static void Install()
        {
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
            >(); // Day76 Host Start Flow 자동 실행
        }
    }
}
