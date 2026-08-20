using ProjectJ.Checkpoint;
using CheckpointComponent =
    ProjectJ.Checkpoint.Checkpoint;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day30CheckpointSetup
    {
        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/Player.prefab";

        private const string FixedMapScenePath =
            "Assets/ProjectJ/Tests/Manual/Day25/" +
            "Day25_ModuleFixedMap.unity";

        private const string Day30Folder =
            "Assets/ProjectJ/Tests/Manual/Day30";

        private const string Day30ScenePath =
            Day30Folder +
            "/Day30_CheckpointTest.unity";

        private static readonly string[]
            FixedMapCheckpointNames =
        {
            "Checkpoint_01_200m",
            "Checkpoint_02_400m",
            "Checkpoint_03_600m",
            "Checkpoint_04_800m"
        };

        private static readonly CheckpointId[]
            FixedMapCheckpointIds =
        {
            CheckpointId.CP1,
            CheckpointId.CP2,
            CheckpointId.CP3,
            CheckpointId.CP4
        };

        [MenuItem(
            "ProjectJ/Day30/Setup Basic Checkpoints"
        )]
        public static void SetupBasicCheckpoints()
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
                Day30Folder
            );

            CreateManualTestScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Day30 체크포인트 기본 활성화 설정 완료."
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
                    prefabRoot.AddComponent<
                        PlayerCheckpointTracker
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

            for (
                int i = 0;
                i <
                FixedMapCheckpointNames.Length;
                i++
            )
            {
                GameObject anchor =
                    FindSceneObjectByName(
                        scene,
                        FixedMapCheckpointNames[i]
                    );

                if (anchor == null)
                {
                    Debug.LogError(
                        "체크포인트 앵커를 찾을 수 없습니다: " +
                        FixedMapCheckpointNames[i]
                    );

                    continue;
                }

                CreateOrUpdateCheckpoint(
                    anchor.transform,
                    FixedMapCheckpointIds[i],
                    new Vector3(
                        14f,
                        2f,
                        14f
                    )
                );
            }

            PlayerCheckpointTracker tracker =
                Object.FindFirstObjectByType<
                    PlayerCheckpointTracker
                >();

            SetupDebugView(
                tracker
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
            Scene scene =
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single
                );

            CreateDirectionalLight();

            PlayerCheckpointTracker tracker =
                CreatePlayer();

            CreateCamera();

            CreateGround();

            float[] checkpointZ =
            {
                -2f,
                4f,
                10f,
                16f
            };

            CheckpointId[] checkpointIds =
            {
                CheckpointId.CP1,
                CheckpointId.CP2,
                CheckpointId.CP3,
                CheckpointId.CP4
            };

            for (
                int i = 0;
                i < checkpointIds.Length;
                i++
            )
            {
                CreateTestCheckpoint(
                    checkpointIds[i],
                    checkpointZ[i]
                );
            }

            SetupDebugView(
                tracker
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene,
                Day30ScenePath
            );

            if (tracker != null)
            {
                Selection.activeGameObject =
                    tracker.gameObject;
            }
        }

        private static PlayerCheckpointTracker
            CreatePlayer()
        {
            GameObject playerPrefab =
                AssetDatabase.LoadAssetAtPath<
                    GameObject
                >(
                    PlayerPrefabPath
                );

            if (playerPrefab == null)
            {
                Debug.LogError(
                    "Player.prefab을 찾을 수 없습니다: " +
                    PlayerPrefabPath
                );

                return null;
            }

            GameObject player =
                PrefabUtility.InstantiatePrefab(
                    playerPrefab
                ) as GameObject;

            if (player == null)
            {
                return null;
            }

            player.name =
                "Player";

            player.transform.position =
                new Vector3(
                    0f,
                    1.1f,
                    -8f
                );

            return player.GetComponent<
                PlayerCheckpointTracker
            >();
        }

        private static void CreateGround()
        {
            GameObject ground =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            ground.name =
                "Ground";

            ground.transform.position =
                new Vector3(
                    0f,
                    -0.5f,
                    5f
                );

            ground.transform.localScale =
                new Vector3(
                    10f,
                    1f,
                    36f
                );

            SetLayerIfExists(
                ground,
                "World"
            );
        }

        private static void CreateTestCheckpoint(
            CheckpointId checkpointId,
            float z
        )
        {
            GameObject floor =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            floor.name =
                checkpointId +
                "_Floor";

            floor.transform.position =
                new Vector3(
                    0f,
                    0.05f,
                    z
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

            CreateOrUpdateCheckpoint(
                floor.transform,
                checkpointId,
                new Vector3(
                    7f,
                    2f,
                    3f
                )
            );
        }

        private static void CreateOrUpdateCheckpoint(
            Transform anchor,
            CheckpointId checkpointId,
            Vector3 triggerSize
        )
        {
            Transform triggerTransform =
                anchor.Find(
                    "CheckpointTrigger"
                );

            GameObject triggerObject;

            if (triggerTransform == null)
            {
                triggerObject =
                    new GameObject(
                        "CheckpointTrigger"
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

            Transform respawnPoint =
                triggerObject.transform.Find(
                    "RespawnPoint"
                );

            if (respawnPoint == null)
            {
                GameObject respawnObject =
                    new GameObject(
                        "RespawnPoint"
                    );

                respawnPoint =
                    respawnObject.transform;

                respawnPoint.SetParent(
                    triggerObject.transform,
                    false
                );
            }

            respawnPoint.localPosition =
                new Vector3(
                    0f,
                    1.1f,
                    0f
                );

            respawnPoint.localRotation =
                Quaternion.identity;

            CheckpointComponent checkpoint =
                triggerObject.GetComponent<
                    CheckpointComponent
                >();

            if (checkpoint == null)
            {
                checkpoint =
                    triggerObject.AddComponent<
                        CheckpointComponent
                    >();
            }

            checkpoint.Configure(
                checkpointId,
                respawnPoint
            );

            EditorUtility.SetDirty(
                checkpoint
            );

            EditorUtility.SetDirty(
                trigger
            );
        }

        private static void SetupDebugView(
            PlayerCheckpointTracker tracker
        )
        {
            CheckpointDebugView debugView =
                Object.FindFirstObjectByType<
                    CheckpointDebugView
                >();

            if (debugView == null)
            {
                GameObject debugObject =
                    new GameObject(
                        "Checkpoint Debug"
                    );

                debugView =
                    debugObject.AddComponent<
                        CheckpointDebugView
                    >();
            }

            debugView.Configure(
                tracker
            );

            EditorUtility.SetDirty(
                debugView
            );
        }

        private static void CreateCamera()
        {
            GameObject cameraObject =
                new GameObject(
                    "Main Camera"
                );

            cameraObject.tag =
                "MainCamera";

            Camera camera =
                cameraObject.AddComponent<
                    Camera
                >();

            cameraObject.AddComponent<
                AudioListener
            >();

            cameraObject.transform.position =
                new Vector3(
                    0f,
                    8f,
                    -16f
                );

            cameraObject.transform.LookAt(
                new Vector3(
                    0f,
                    1f,
                    5f
                )
            );

            camera.fieldOfView =
                60f;
        }

        private static void CreateDirectionalLight()
        {
            GameObject lightObject =
                new GameObject(
                    "Directional Light"
                );

            Light light =
                lightObject.AddComponent<
                    Light
                >();

            light.type =
                LightType.Directional;

            light.intensity =
                1.2f;

            lightObject.transform.rotation =
                Quaternion.Euler(
                    50f,
                    -30f,
                    0f
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

                current =
                    next;
            }
        }
    }
}
