using ProjectJ.Networking.Fusion; // Day96 Server Bootstrap 사용
using UnityEditor; // Editor Menu 사용
using UnityEditor.SceneManagement; // Test Scene 생성·저장
using UnityEngine; // GameObject 사용
using UnityEngine.SceneManagement; // Scene 이동 사용

namespace ProjectJ.EditorTools
{
    public static class
        ProjectJDay96ServerModeInstaller
    {
        private const string ServerScenePath =
            "Assets/ProjectJ/Scenes/Day96_ServerModeTest.unity";

        [MenuItem(
            "Project J/Scene/96일차 Server Mode Test Scene 구성"
        )]
        private static void Install()
        {
            if (
                !EditorSceneManager
                    .SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                return;
            }

            Scene scene =
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single
                );

            GameObject serverRoot =
                new GameObject(
                    "=== Day96 Server Mode ==="
                );

            SceneManager.MoveGameObjectToScene(
                serverRoot,
                scene
            );

            serverRoot.AddComponent<
                ProjectJDay96ServerModeBootstrap
            >(); // Room Code 960001로 Server Mode 자동 시작

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            bool saved =
                EditorSceneManager.SaveScene(
                    scene,
                    ServerScenePath
                );

            if (!saved)
            {
                Debug.LogError(
                    "[Project J/Day96] " +
                    "Server Mode Test Scene 저장 실패 / " +
                    ServerScenePath
                );

                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                serverRoot;

            EditorGUIUtility.PingObject(
                serverRoot
            );

            Debug.Log(
                "[Project J/Day96] " +
                "Server Mode Test Scene 구성 완료 / " +
                ServerScenePath +
                " / RoomCode: 960001"
            );
        }
    }
}
