using ProjectJ.Finish;
using ProjectJ.Ranking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day35FinishSetup
    {
        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/" +
            "Player.prefab";

        private const string FixedMapScenePath =
            "Assets/ProjectJ/Tests/Manual/Day25/" +
            "Day25_ModuleFixedMap.unity";

        private const string Day34ScenePath =
            "Assets/ProjectJ/Tests/Manual/Day34/" +
            "Day34_RespawnProtectionTest.unity";

        private const string Day35Folder =
            "Assets/ProjectJ/Tests/Manual/Day35";

        private const string Day35ScenePath =
            Day35Folder +
            "/Day35_FinishOrderTest.unity";

        private const string FixedMapFinishName =
            "FINISH_1000m";

        [MenuItem(
            "ProjectJ/Day35/Setup Finish Order"
        )]
        public static void SetupFinishOrder()
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
                Day35Folder
            );

            CreateManualTestScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Day35 정상 도달·도착 순서 설정 완료."
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
                PlayerRankingParticipant participant =
                    prefabRoot.GetComponent<
                        PlayerRankingParticipant
                    >();

                if (participant == null)
                {
                    Debug.LogError(
                        "PlayerRankingParticipant가 없습니다."
                    );

                    return;
                }

                participant.SetHeightRankingEligible(
                    true
                );

                PlayerFinishState finishState =
                    prefabRoot.GetComponent<
                        PlayerFinishState
                    >();

                if (finishState == null)
                {
                    finishState =
                        prefabRoot.AddComponent<
                            PlayerFinishState
                        >();
                }

                finishState.Configure(
                    participant
                );

                EditorUtility.SetDirty(
                    finishState
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

            FinishOrderManager manager =
                CreateOrFindManager();

            GameObject finishAnchor =
                FindSceneObjectByName(
                    scene,
                    FixedMapFinishName
                );

            if (finishAnchor == null)
            {
                Debug.LogError(
                    "FINISH 앵커를 찾을 수 없습니다: " +
                    FixedMapFinishName
                );
            }
            else
            {
                CreateOrUpdateFinishTrigger(
                    finishAnchor.transform,
                    manager,
                    new Vector3(
                        14f,
                        3f,
                        14f
                    )
                );
            }

            SetupDebugView(
                manager
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
                    Day34ScenePath
                ) == null
            )
            {
                Debug.LogError(
                    "Day34 테스트 Scene을 찾을 수 없습니다: " +
                    Day34ScenePath
                );

                return;
            }

            if (
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    Day35ScenePath
                ) != null
            )
            {
                AssetDatabase.DeleteAsset(
                    Day35ScenePath
                );
            }

            bool copied =
                AssetDatabase.CopyAsset(
                    Day34ScenePath,
                    Day35ScenePath
                );

            if (!copied)
            {
                Debug.LogError(
                    "Day35 테스트 Scene 복사에 실패했습니다."
                );

                return;
            }

            AssetDatabase.Refresh();

            Scene scene =
                EditorSceneManager.OpenScene(
                    Day35ScenePath,
                    OpenSceneMode.Single
                );

            FinishOrderManager manager =
                CreateOrFindManager();

            CreateManualFinishArea(
                manager
            );

            SetupDebugView(
                manager
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );

            PlayerFinishState player =
                Object.FindFirstObjectByType<
                    PlayerFinishState
                >();

            if (player != null)
            {
                Selection.activeGameObject =
                    player.gameObject;
            }
        }

        private static FinishOrderManager
            CreateOrFindManager()
        {
            FinishOrderManager manager =
                Object.FindFirstObjectByType<
                    FinishOrderManager
                >();

            if (manager != null)
            {
                return manager;
            }

            GameObject managerObject =
                new GameObject(
                    "=== Finish Manager ==="
                );

            return
                managerObject.AddComponent<
                    FinishOrderManager
                >();
        }

        private static void CreateManualFinishArea(
            FinishOrderManager manager
        )
        {
            GameObject existing =
                GameObject.Find(
                    "FINISH_Test"
                );

            if (existing != null)
            {
                Object.DestroyImmediate(
                    existing
                );
            }

            GameObject floor =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            floor.name =
                "FINISH_Test";

            floor.transform.position =
                new Vector3(
                    0f,
                    0.05f,
                    20f
                );

            floor.transform.localScale =
                new Vector3(
                    7f,
                    0.1f,
                    3f
                );

            SetLayerIfExists(
                floor,
                "World"
            );

            CreateOrUpdateFinishTrigger(
                floor.transform,
                manager,
                new Vector3(
                    7f,
                    2f,
                    3f
                )
            );
        }

        private static void CreateOrUpdateFinishTrigger(
            Transform anchor,
            FinishOrderManager manager,
            Vector3 triggerSize
        )
        {
            Transform triggerTransform =
                anchor.Find(
                    "FinishTrigger"
                );

            GameObject triggerObject;

            if (triggerTransform == null)
            {
                triggerObject =
                    new GameObject(
                        "FinishTrigger"
                    );

                triggerObject.transform.SetParent(
                    anchor,
                    false
                );
            }
            else
            {
                triggerObject =
                    triggerTransform.gameObject;
            }

            triggerObject.transform.localPosition =
                Vector3.zero;

            triggerObject.transform.localRotation =
                Quaternion.identity;

            SetLayerIfExists(
                triggerObject,
                "GameplayTrigger"
            );

            BoxCollider trigger =
                triggerObject.GetComponent<
                    BoxCollider
                >();

            if (trigger == null)
            {
                trigger =
                    triggerObject.AddComponent<
                        BoxCollider
                    >();
            }

            trigger.isTrigger = true;

            trigger.center =
                new Vector3(
                    0f,
                    1f,
                    0f
                );

            trigger.size =
                triggerSize;

            FinishTrigger finishTrigger =
                triggerObject.GetComponent<
                    FinishTrigger
                >();

            if (finishTrigger == null)
            {
                finishTrigger =
                    triggerObject.AddComponent<
                        FinishTrigger
                    >();
            }

            finishTrigger.Configure(
                manager
            );

            EditorUtility.SetDirty(
                trigger
            );

            EditorUtility.SetDirty(
                finishTrigger
            );
        }

        private static void SetupDebugView(
            FinishOrderManager manager
        )
        {
            FinishDebugView debugView =
                Object.FindFirstObjectByType<
                    FinishDebugView
                >();

            if (debugView == null)
            {
                GameObject debugObject =
                    new GameObject(
                        "Finish Debug"
                    );

                debugView =
                    debugObject.AddComponent<
                        FinishDebugView
                    >();
            }

            PlayerFinishState player =
                Object.FindFirstObjectByType<
                    PlayerFinishState
                >();

            debugView.Configure(
                manager,
                player
            );

            EditorUtility.SetDirty(
                debugView
            );
        }

        private static GameObject FindSceneObjectByName(
            Scene scene,
            string objectName
        )
        {
            GameObject[] roots =
                scene.GetRootGameObjects();

            for (
                int i = 0;
                i < roots.Length;
                i++
            )
            {
                Transform found =
                    FindRecursive(
                        roots[i].transform,
                        objectName
                    );

                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static Transform FindRecursive(
            Transform current,
            string objectName
        )
        {
            if (
                current.name ==
                objectName
            )
            {
                return current;
            }

            for (
                int i = 0;
                i < current.childCount;
                i++
            )
            {
                Transform found =
                    FindRecursive(
                        current.GetChild(i),
                        objectName
                    );

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void SetLayerIfExists(
            GameObject target,
            string layerName
        )
        {
            int layer =
                LayerMask.NameToLayer(
                    layerName
                );

            if (layer >= 0)
            {
                target.layer =
                    layer;
            }
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
