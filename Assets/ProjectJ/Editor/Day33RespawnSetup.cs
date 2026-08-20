using ProjectJ.Checkpoint;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day33RespawnSetup
    {
        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/" +
            "Player.prefab";

        private const string FixedMapScenePath =
            "Assets/ProjectJ/Tests/Manual/Day25/" +
            "Day25_ModuleFixedMap.unity";

        private const string Day32ScenePath =
            "Assets/ProjectJ/Tests/Manual/Day32/" +
            "Day32_FallLimitTest.unity";

        private const string Day33Folder =
            "Assets/ProjectJ/Tests/Manual/Day33";

        private const string Day33ScenePath =
            Day33Folder +
            "/Day33_CheckpointRespawnTest.unity";

        [MenuItem(
            "ProjectJ/Day33/Setup Checkpoint Respawn"
        )]
        public static void SetupCheckpointRespawn()
        {
            bool canContinue =
                EditorSceneManager
                    .SaveCurrentModifiedScenesIfUserWantsTo();

            if (!canContinue)
            {
                return;
            }

            SetupPlayerPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SetupFixedMapScene();

            EnsureFolder(
                Day33Folder
            );

            CreateManualTestScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Day33 체크포인트 부활 설정 완료."
            );
        }

        private static void SetupPlayerPrefab()
        {
            GameObject prefabRoot =
                PrefabUtility.LoadPrefabContents(
                    PlayerPrefabPath
                );

            if (prefabRoot == null)
            {
                Debug.LogError(
                    "Player.prefab을 찾을 수 없습니다: " +
                    PlayerPrefabPath
                );

                return;
            }

            try
            {
                Rigidbody body =
                    prefabRoot.GetComponent<
                        Rigidbody
                    >();

                PlayerCheckpointTracker tracker =
                    prefabRoot.GetComponent<
                        PlayerCheckpointTracker
                    >();

                PlayerFallTracker fallTracker =
                    prefabRoot.GetComponent<
                        PlayerFallTracker
                    >();

                if (
                    body == null ||
                    tracker == null ||
                    fallTracker == null
                )
                {
                    Debug.LogError(
                        "Day33 선행 컴포넌트가 부족합니다. " +
                        "Day30~Day32 설정을 확인하세요."
                    );

                    return;
                }

                PlayerRespawnController controller =
                    prefabRoot.GetComponent<
                        PlayerRespawnController
                    >();

                if (controller == null)
                {
                    controller =
                        prefabRoot.AddComponent<
                            PlayerRespawnController
                        >();
                }

                controller.Configure(
                    body,
                    tracker,
                    fallTracker
                );

                EditorUtility.SetDirty(
                    controller
                );

                PrefabUtility.SaveAsPrefabAsset(
                    prefabRoot,
                    PlayerPrefabPath
                );
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(
                    prefabRoot
                );
            }
        }

        private static void SetupFixedMapScene()
        {
            if (
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    FixedMapScenePath
                ) == null
            )
            {
                Debug.LogError(
                    "Day25 고정맵 Scene을 찾을 수 없습니다: " +
                    FixedMapScenePath
                );

                return;
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    FixedMapScenePath,
                    OpenSceneMode.Single
                );

            PlayerRespawnController controller =
                FindAndConfigureController();

            SetupDebugView(
                controller
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );
        }

        private static void CreateManualTestScene()
        {
            if (
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    Day32ScenePath
                ) == null
            )
            {
                Debug.LogError(
                    "Day32 테스트 Scene을 찾을 수 없습니다: " +
                    Day32ScenePath
                );

                return;
            }

            if (
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    Day33ScenePath
                ) != null
            )
            {
                AssetDatabase.DeleteAsset(
                    Day33ScenePath
                );
            }

            bool copied =
                AssetDatabase.CopyAsset(
                    Day32ScenePath,
                    Day33ScenePath
                );

            if (!copied)
            {
                Debug.LogError(
                    "Day33 테스트 Scene 복사에 실패했습니다."
                );

                return;
            }

            AssetDatabase.Refresh();

            Scene scene =
                EditorSceneManager.OpenScene(
                    Day33ScenePath,
                    OpenSceneMode.Single
                );

            PlayerRespawnController controller =
                FindAndConfigureController();

            SetupDebugView(
                controller
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );

            if (controller != null)
            {
                Selection.activeGameObject =
                    controller.gameObject;
            }
        }

        private static PlayerRespawnController
            FindAndConfigureController()
        {
            PlayerRespawnController controller =
                Object.FindFirstObjectByType<
                    PlayerRespawnController
                >();

            if (controller == null)
            {
                Debug.LogError(
                    "Scene에서 PlayerRespawnController를 " +
                    "찾을 수 없습니다."
                );

                return null;
            }

            Rigidbody body =
                controller.GetComponent<
                    Rigidbody
                >();

            PlayerCheckpointTracker tracker =
                controller.GetComponent<
                    PlayerCheckpointTracker
                >();

            PlayerFallTracker fallTracker =
                controller.GetComponent<
                    PlayerFallTracker
                >();

            controller.Configure(
                body,
                tracker,
                fallTracker
            );

            EditorUtility.SetDirty(
                controller
            );

            return controller;
        }

        private static void SetupDebugView(
            PlayerRespawnController controller
        )
        {
            if (controller == null)
            {
                return;
            }

            RespawnDebugView debugView =
                Object.FindFirstObjectByType<
                    RespawnDebugView
                >();

            if (debugView == null)
            {
                GameObject debugObject =
                    new GameObject(
                        "Respawn Debug"
                    );

                debugView =
                    debugObject.AddComponent<
                        RespawnDebugView
                    >();
            }

            debugView.Configure(
                controller
            );

            EditorUtility.SetDirty(
                debugView
            );
        }

        private static void EnsureFolder(
            string fullPath
        )
        {
            string[] parts =
                fullPath.Split('/');

            string current =
                parts[0];

            for (
                int i = 1;
                i < parts.Length;
                i++
            )
            {
                string next =
                    current +
                    "/" +
                    parts[i];

                if (
                    !AssetDatabase.IsValidFolder(
                        next
                    )
                )
                {
                    AssetDatabase.CreateFolder(
                        current,
                        parts[i]
                    );
                }

                current = next;
            }
        }
    }
}
