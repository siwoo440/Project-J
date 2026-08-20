using ProjectJ.Checkpoint;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day32FallLimitSetup
    {
        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/" +
            "Player.prefab";

        private const string FixedMapScenePath =
            "Assets/ProjectJ/Tests/Manual/Day25/" +
            "Day25_ModuleFixedMap.unity";

        private const string Day30ScenePath =
            "Assets/ProjectJ/Tests/Manual/Day30/" +
            "Day30_CheckpointTest.unity";

        private const string Day32Folder =
            "Assets/ProjectJ/Tests/Manual/Day32";

        private const string Day32ScenePath =
            Day32Folder +
            "/Day32_FallLimitTest.unity";

        private const float ProductionStartLimit =
            -20f;

        private const float ProductionCp1Limit =
            180f;

        private const float ProductionCp2Limit =
            380f;

        private const float ProductionCp3Limit =
            580f;

        private const float ProductionCp4Limit =
            780f;

        [MenuItem(
            "ProjectJ/Day32/Setup Section Fall Limits"
        )]
        public static void SetupSectionFallLimits()
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

            SetupFixedMap();

            EnsureFolder(
                Day32Folder
            );

            CreateManualTestScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Day32 구간별 Fall Limit 설정 완료."
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
                PlayerCheckpointTracker tracker =
                    prefabRoot.GetComponent<
                        PlayerCheckpointTracker
                    >();

                if (tracker == null)
                {
                    Debug.LogError(
                        "PlayerCheckpointTracker가 없습니다. " +
                        "Day30 설정을 먼저 확인하세요."
                    );

                    return;
                }

                PlayerFallTracker fallTracker =
                    prefabRoot.GetComponent<
                        PlayerFallTracker
                    >();

                if (fallTracker == null)
                {
                    prefabRoot.AddComponent<
                        PlayerFallTracker
                    >();
                }

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

        private static void SetupFixedMap()
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

            CheckpointFallLimitSet limits =
                CreateOrUpdateFallLimitSet(
                    ProductionStartLimit,
                    ProductionCp1Limit,
                    ProductionCp2Limit,
                    ProductionCp3Limit,
                    ProductionCp4Limit
                );

            PlayerFallTracker fallTracker =
                Object.FindFirstObjectByType<
                    PlayerFallTracker
                >();

            ConfigureFallTracker(
                fallTracker,
                limits
            );

            SetupDebugView(
                fallTracker
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
                    Day30ScenePath
                ) == null
            )
            {
                Debug.LogError(
                    "Day30 테스트 Scene을 찾을 수 없습니다: " +
                    Day30ScenePath
                );

                return;
            }

            if (
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    Day32ScenePath
                ) != null
            )
            {
                AssetDatabase.DeleteAsset(
                    Day32ScenePath
                );
            }

            bool copied =
                AssetDatabase.CopyAsset(
                    Day30ScenePath,
                    Day32ScenePath
                );

            if (!copied)
            {
                Debug.LogError(
                    "Day32 테스트 Scene 복사에 실패했습니다."
                );

                return;
            }

            AssetDatabase.Refresh();

            Scene scene =
                EditorSceneManager.OpenScene(
                    Day32ScenePath,
                    OpenSceneMode.Single
                );

            CheckpointFallLimitSet limits =
                CreateOrUpdateFallLimitSet(
                    -5f,
                    -4f,
                    -3f,
                    -2f,
                    -1f
                );

            PlayerFallTracker fallTracker =
                Object.FindFirstObjectByType<
                    PlayerFallTracker
                >();

            ConfigureFallTracker(
                fallTracker,
                limits
            );

            SetupDebugView(
                fallTracker
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );

            if (fallTracker != null)
            {
                Selection.activeGameObject =
                    fallTracker.gameObject;
            }
        }

        private static CheckpointFallLimitSet
            CreateOrUpdateFallLimitSet(
                float start,
                float cp1,
                float cp2,
                float cp3,
                float cp4
            )
        {
            CheckpointFallLimitSet limits =
                Object.FindFirstObjectByType<
                    CheckpointFallLimitSet
                >();

            if (limits == null)
            {
                GameObject root =
                    new GameObject(
                        "=== Fall Limits ==="
                    );

                limits =
                    root.AddComponent<
                        CheckpointFallLimitSet
                    >();
            }

            limits.Configure(
                start,
                cp1,
                cp2,
                cp3,
                cp4
            );

            CreateOrMoveMarker(
                limits.transform,
                "FallLimit_START",
                start
            );

            CreateOrMoveMarker(
                limits.transform,
                "FallLimit_CP1",
                cp1
            );

            CreateOrMoveMarker(
                limits.transform,
                "FallLimit_CP2",
                cp2
            );

            CreateOrMoveMarker(
                limits.transform,
                "FallLimit_CP3",
                cp3
            );

            CreateOrMoveMarker(
                limits.transform,
                "FallLimit_CP4",
                cp4
            );

            EditorUtility.SetDirty(
                limits
            );

            return limits;
        }

        private static void CreateOrMoveMarker(
            Transform parent,
            string markerName,
            float worldY
        )
        {
            Transform marker =
                parent.Find(
                    markerName
                );

            if (marker == null)
            {
                GameObject markerObject =
                    new GameObject(
                        markerName
                    );

                marker =
                    markerObject.transform;

                marker.SetParent(
                    parent,
                    false
                );
            }

            marker.position =
                new Vector3(
                    0f,
                    worldY,
                    0f
                );
        }

        private static void ConfigureFallTracker(
            PlayerFallTracker fallTracker,
            CheckpointFallLimitSet limits
        )
        {
            if (fallTracker == null)
            {
                Debug.LogError(
                    "Scene에서 PlayerFallTracker를 찾을 수 없습니다."
                );

                return;
            }

            PlayerCheckpointTracker tracker =
                fallTracker.GetComponent<
                    PlayerCheckpointTracker
                >();

            fallTracker.Configure(
                tracker,
                limits
            );

            EditorUtility.SetDirty(
                fallTracker
            );
        }

        private static void SetupDebugView(
            PlayerFallTracker fallTracker
        )
        {
            FallLimitDebugView debugView =
                Object.FindFirstObjectByType<
                    FallLimitDebugView
                >();

            if (debugView == null)
            {
                GameObject debugObject =
                    new GameObject(
                        "Fall Limit Debug"
                    );

                debugView =
                    debugObject.AddComponent<
                        FallLimitDebugView
                    >();
            }

            debugView.Configure(
                fallTracker
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
