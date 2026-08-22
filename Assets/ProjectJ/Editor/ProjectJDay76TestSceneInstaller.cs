using System.Collections.Generic; // Build Scene 목록 편집
using ProjectJ.Networking.Fusion; // Spawn Point Component 사용
using UnityEditor; // Menu와 PlayerSettings 사용
using UnityEditor.SceneManagement; // Game Scene 편집
using UnityEngine; // GameObject와 Player 설정 사용
using UnityEngine.SceneManagement; // Scene 구조 사용

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay76TestSceneInstaller
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private const string ObsoleteDay76ScenePath =
            "Assets/ProjectJ/Tests/Manual/Day76/Game.unity";

        private const string ObsoleteDay76FolderPath =
            "Assets/ProjectJ/Tests/Manual/Day76";

        private const string TestRootName =
            "=== DAY76 MULTIPLAYER TEST ===";

        private const string SpawnRootName =
            "SpawnPoints";

        [MenuItem(
            "Project J/Day76/Create or Update Multiplayer Test Scene"
        )]
        private static void CreateOrUpdateTestScene()
        {
            if (
                !EditorSceneManager
                    .SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                return;
            }

            SceneAsset gameSceneAsset =
                AssetDatabase
                    .LoadAssetAtPath<SceneAsset>(
                        GameScenePath
                    );

            if (gameSceneAsset == null)
            {
                Debug.LogError(
                    "[Project J] 실제 Game Scene을 찾지 못했습니다. / " +
                    GameScenePath
                );

                return;
            }

            RemoveObsoleteTestScene();

            Scene scene =
                EditorSceneManager.OpenScene(
                    GameScenePath,
                    OpenSceneMode.Single
                );

            GameObject testRoot =
                FindDirectRoot(
                    scene,
                    TestRootName
                );

            if (testRoot == null)
            {
                testRoot =
                    new GameObject(
                        TestRootName
                    );

                SceneManager.MoveGameObjectToScene(
                    testRoot,
                    scene
                );
            }

            Transform spawnRoot =
                FindOrCreateChild(
                    testRoot.transform,
                    SpawnRootName
                );

            CreateSpawnPoints(
                spawnRoot
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );

            EnsureBuildSettings();
            ApplyMultiWindowPlayerSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                testRoot;

            Debug.Log(
                "[Project J] 76일차 실제 Game Scene 멀티 테스트 준비 완료 / " +
                GameScenePath
            );
        }

        private static void CreateSpawnPoints(
            Transform spawnRoot
        )
        {
            const float SpawnSpacing = 3f;
            const float SpawnY = 2f;
            const float SpawnZ = 4f;

            for (
                int slot = 0;
                slot < 8;
                slot++
            )
            {
                string pointName =
                    "Spawn_" +
                    slot.ToString("00");

                Transform point =
                    spawnRoot.Find(
                        pointName
                    );

                if (point == null)
                {
                    GameObject pointObject =
                        new GameObject(
                            pointName
                        );

                    point =
                        pointObject.transform;

                    point.SetParent(
                        spawnRoot,
                        false
                    );
                }

                point.position =
                    new Vector3(
                        slot * SpawnSpacing,
                        SpawnY,
                        SpawnZ
                    ); // 기존 Game Scene Spawn 규칙과 동일한 위치

                point.rotation =
                    Quaternion.identity;

                ProjectJNetworkSpawnPoint spawnPoint =
                    point.GetComponent<
                        ProjectJNetworkSpawnPoint
                    >();

                if (spawnPoint == null)
                {
                    spawnPoint =
                        point.gameObject.AddComponent<
                            ProjectJNetworkSpawnPoint
                        >();
                }

                spawnPoint.ConfigureSlot(
                    slot
                );
            }
        }

        private static void RemoveObsoleteTestScene()
        {
            if (
                AssetDatabase
                    .LoadAssetAtPath<SceneAsset>(
                        ObsoleteDay76ScenePath
                    ) != null
            )
            {
                AssetDatabase.DeleteAsset(
                    ObsoleteDay76ScenePath
                ); // 잘못 만든 Day49 복사 테스트 Scene 제거
            }

            if (
                AssetDatabase.IsValidFolder(
                    ObsoleteDay76FolderPath
                )
            )
            {
                string[] remainingAssets =
                    AssetDatabase.FindAssets(
                        string.Empty,
                        new[]
                        {
                            ObsoleteDay76FolderPath
                        }
                    );

                if (remainingAssets.Length == 0)
                {
                    AssetDatabase.DeleteAsset(
                        ObsoleteDay76FolderPath
                    );
                }
            }
        }

        private static void EnsureBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes =
                new List<EditorBuildSettingsScene>();

            EditorBuildSettingsScene[] currentScenes =
                EditorBuildSettings.scenes;

            bool gameSceneFound = false;

            for (
                int index = 0;
                index < currentScenes.Length;
                index++
            )
            {
                EditorBuildSettingsScene current =
                    currentScenes[index];

                if (
                    current.path ==
                    ObsoleteDay76ScenePath
                )
                {
                    continue; // 잘못 등록된 테스트 Scene 제거
                }

                if (
                    current.path ==
                    GameScenePath
                )
                {
                    scenes.Add(
                        new EditorBuildSettingsScene(
                            GameScenePath,
                            true
                        )
                    );

                    gameSceneFound = true;
                    continue;
                }

                scenes.Add(
                    current
                );
            }

            if (!gameSceneFound)
            {
                scenes.Add(
                    new EditorBuildSettingsScene(
                        GameScenePath,
                        true
                    )
                );
            }

            EditorBuildSettings.scenes =
                scenes.ToArray();
        }

        private static void
            ApplyMultiWindowPlayerSettings()
        {
            PlayerSettings.runInBackground = true;
            PlayerSettings.forceSingleInstance = false;
            PlayerSettings.fullScreenMode =
                FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 960;
            PlayerSettings.defaultScreenHeight = 540;
            PlayerSettings.resizableWindow = true;
        }

        private static GameObject FindDirectRoot(
            Scene scene,
            string objectName
        )
        {
            GameObject[] roots =
                scene.GetRootGameObjects();

            for (
                int index = 0;
                index < roots.Length;
                index++
            )
            {
                if (
                    roots[index].name ==
                    objectName
                )
                {
                    return roots[index];
                }
            }

            return null;
        }

        private static Transform FindOrCreateChild(
            Transform parent,
            string childName
        )
        {
            Transform child =
                parent.Find(
                    childName
                );

            if (child != null)
            {
                return child;
            }

            GameObject childObject =
                new GameObject(
                    childName
                );

            childObject.transform.SetParent(
                parent,
                false
            );

            return childObject.transform;
        }
    }
}
