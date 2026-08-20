using ProjectJ.Checkpoint;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day34RespawnProtectionSetup
    {
        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/" +
            "Player.prefab";

        private const string FixedMapScenePath =
            "Assets/ProjectJ/Tests/Manual/Day25/" +
            "Day25_ModuleFixedMap.unity";

        private const string Day33ScenePath =
            "Assets/ProjectJ/Tests/Manual/Day33/" +
            "Day33_CheckpointRespawnTest.unity";

        private const string Day34Folder =
            "Assets/ProjectJ/Tests/Manual/Day34";

        private const string Day34ScenePath =
            Day34Folder +
            "/Day34_RespawnProtectionTest.unity";

        private const float ProtectionDuration =
            3f;

        [MenuItem(
            "ProjectJ/Day34/Setup Respawn Protection"
        )]
        public static void SetupRespawnProtection()
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
                Day34Folder
            );

            CreateManualTestScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Day34 3초 부활 보호 설정 완료."
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
                PlayerRespawnController controller =
                    prefabRoot.GetComponent<
                        PlayerRespawnController
                    >();

                if (controller == null)
                {
                    Debug.LogError(
                        "PlayerRespawnController가 없습니다. " +
                        "Day33 설정을 먼저 확인하세요."
                    );

                    return;
                }

                PlayerRespawnProtection protection =
                    prefabRoot.GetComponent<
                        PlayerRespawnProtection
                    >();

                if (protection == null)
                {
                    protection =
                        prefabRoot.AddComponent<
                            PlayerRespawnProtection
                        >();
                }

                protection.Configure(
                    controller,
                    ProtectionDuration
                );

                EditorUtility.SetDirty(
                    protection
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

            PlayerRespawnProtection protection =
                FindAndConfigureProtection();

            SetupDebugView(
                protection
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
                    Day33ScenePath
                ) == null
            )
            {
                Debug.LogError(
                    "Day33 테스트 Scene을 찾을 수 없습니다: " +
                    Day33ScenePath
                );

                return;
            }

            if (
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    Day34ScenePath
                ) != null
            )
            {
                AssetDatabase.DeleteAsset(
                    Day34ScenePath
                );
            }

            bool copied =
                AssetDatabase.CopyAsset(
                    Day33ScenePath,
                    Day34ScenePath
                );

            if (!copied)
            {
                Debug.LogError(
                    "Day34 테스트 Scene 복사에 실패했습니다."
                );

                return;
            }

            AssetDatabase.Refresh();

            Scene scene =
                EditorSceneManager.OpenScene(
                    Day34ScenePath,
                    OpenSceneMode.Single
                );

            PlayerRespawnProtection protection =
                FindAndConfigureProtection();

            SetupDebugView(
                protection
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );

            if (protection != null)
            {
                Selection.activeGameObject =
                    protection.gameObject;
            }
        }

        private static PlayerRespawnProtection
            FindAndConfigureProtection()
        {
            PlayerRespawnProtection protection =
                Object.FindFirstObjectByType<
                    PlayerRespawnProtection
                >();

            if (protection == null)
            {
                Debug.LogError(
                    "Scene에서 PlayerRespawnProtection을 " +
                    "찾을 수 없습니다."
                );

                return null;
            }

            PlayerRespawnController controller =
                protection.GetComponent<
                    PlayerRespawnController
                >();

            protection.Configure(
                controller,
                ProtectionDuration
            );

            EditorUtility.SetDirty(
                protection
            );

            return protection;
        }

        private static void SetupDebugView(
            PlayerRespawnProtection protection
        )
        {
            if (protection == null)
            {
                return;
            }

            RespawnProtectionDebugView debugView =
                Object.FindFirstObjectByType<
                    RespawnProtectionDebugView
                >();

            if (debugView == null)
            {
                GameObject debugObject =
                    new GameObject(
                        "Respawn Protection Debug"
                    );

                debugView =
                    debugObject.AddComponent<
                        RespawnProtectionDebugView
                    >();
            }

            debugView.Configure(
                protection
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
