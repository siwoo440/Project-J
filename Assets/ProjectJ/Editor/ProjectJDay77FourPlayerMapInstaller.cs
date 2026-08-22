using ProjectJ.Checkpoint; // Checkpoint과 낙하 한계 사용
using ProjectJ.Finish; // FINISH Trigger와 Manager 사용
using ProjectJ.Networking.Fusion; // Network Spawn Point 사용
using UnityEditor; // Editor Menu와 Scene 저장 사용
using UnityEditor.SceneManagement; // Game Scene 편집
using UnityEngine; // Greybox GameObject 생성
using UnityEngine.SceneManagement; // Scene 구조 사용

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay77FourPlayerMapInstaller
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private const string MapRootName =
            "=== DAY77 4 PLAYER TEST MAP ===";

        private const float PlatformHeight =
            1f;

        private const float StepRise =
            0.8f;

        [MenuItem(
            "Project J/Day77/Create or Update 4 Player Test Map"
        )]
        private static void CreateOrUpdateMap()
        {
            if (
                !EditorSceneManager
                    .SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                return;
            }

            SceneAsset gameSceneAsset =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    GameScenePath
                );

            if (gameSceneAsset == null)
            {
                Debug.LogError(
                    "[Project J] Game Scene을 찾지 못했습니다. / " +
                    GameScenePath
                );

                return;
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    GameScenePath,
                    OpenSceneMode.Single
                );

            RemovePreviousMap(scene);

            GameObject mapRoot =
                new GameObject(
                    MapRootName
                );

            SceneManager.MoveGameObjectToScene(
                mapRoot,
                scene
            );

            Transform startRoot =
                CreateSection(
                    mapRoot.transform,
                    "=== START ==="
                );

            Transform section1 =
                CreateSection(
                    mapRoot.transform,
                    "=== SECTION 01 / CP1 ==="
                );

            Transform section2 =
                CreateSection(
                    mapRoot.transform,
                    "=== SECTION 02 / CP2 ==="
                );

            Transform section3 =
                CreateSection(
                    mapRoot.transform,
                    "=== SECTION 03 / CP3 ==="
                );

            Transform section4 =
                CreateSection(
                    mapRoot.transform,
                    "=== SECTION 04 / CP4 ==="
                );

            Transform finishRoot =
                CreateSection(
                    mapRoot.transform,
                    "=== FINISH ==="
                );

            Transform systemRoot =
                CreateSection(
                    mapRoot.transform,
                    "=== SYSTEM ==="
                );

            CreateStartArea(
                startRoot
            );

            CreateSectionOne(
                section1
            );

            CreateSectionTwo(
                section2
            );

            CreateSectionThree(
                section3
            );

            CreateSectionFour(
                section4
            );

            CreateFinishArea(
                finishRoot
            );

            ConfigureSpawnPoints();
            ConfigureFallLimits(
                systemRoot
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                mapRoot;

            Debug.Log(
                "[Project J] 77일차 4인 전체 경기 Greybox 맵 생성 완료"
            );
        }

        private static void CreateStartArea(
            Transform parent
        )
        {
            CreatePlatform(
                parent,
                "Start_Plaza",
                new Vector3(
                    0f,
                    0f,
                    0f
                ),
                new Vector3(
                    28f,
                    PlatformHeight,
                    22f
                )
            );

            CreatePlatform(
                parent,
                "Start_Rail_Left",
                new Vector3(
                    -13.75f,
                    1f,
                    -1f
                ),
                new Vector3(
                    0.5f,
                    2f,
                    20f
                )
            );

            CreatePlatform(
                parent,
                "Start_Rail_Right",
                new Vector3(
                    13.75f,
                    1f,
                    -1f
                ),
                new Vector3(
                    0.5f,
                    2f,
                    20f
                )
            );

            CreatePlatform(
                parent,
                "Start_Rail_Back",
                new Vector3(
                    0f,
                    1f,
                    -10.75f
                ),
                new Vector3(
                    28f,
                    2f,
                    0.5f
                )
            );
        }

        private static void CreateSectionOne(
            Transform parent
        )
        {
            CreateStep(
                parent,
                "S1_Step_01",
                0f,
                14f,
                StepRise
            );

            CreateStep(
                parent,
                "S1_Step_02",
                4f,
                21f,
                StepRise * 2f
            );

            CreateStep(
                parent,
                "S1_Step_03",
                -4f,
                28f,
                StepRise * 3f
            );

            float deckY =
                StepRise * 4f;

            CreatePlatform(
                parent,
                "CP1_Push_Arena",
                new Vector3(
                    0f,
                    deckY,
                    38f
                ),
                new Vector3(
                    26f,
                    PlatformHeight,
                    14f
                )
            );

            CreateCheckpoint(
                parent,
                CheckpointId.CP1,
                new Vector3(
                    0f,
                    deckY,
                    38f
                ),
                26f
            );
        }

        private static void CreateSectionTwo(
            Transform parent
        )
        {
            float baseY =
                StepRise * 4f;

            CreateStep(
                parent,
                "S2_Step_01",
                4f,
                50f,
                baseY + StepRise
            );

            CreateStep(
                parent,
                "S2_Step_02",
                -4f,
                57f,
                baseY + StepRise * 2f
            );

            CreateStep(
                parent,
                "S2_Step_03",
                4f,
                64f,
                baseY + StepRise * 3f
            );

            float deckY =
                StepRise * 8f;

            CreatePlatform(
                parent,
                "CP2_Deck",
                new Vector3(
                    0f,
                    deckY,
                    74f
                ),
                new Vector3(
                    24f,
                    PlatformHeight,
                    14f
                )
            );

            CreateCheckpoint(
                parent,
                CheckpointId.CP2,
                new Vector3(
                    0f,
                    deckY,
                    74f
                ),
                24f
            );
        }

        private static void CreateSectionThree(
            Transform parent
        )
        {
            float baseY =
                StepRise * 8f;

            CreateStep(
                parent,
                "S3_Step_01",
                -4f,
                86f,
                baseY + StepRise
            );

            CreateStep(
                parent,
                "S3_Step_02",
                4f,
                93f,
                baseY + StepRise * 2f
            );

            CreateStep(
                parent,
                "S3_Step_03",
                -4f,
                100f,
                baseY + StepRise * 3f
            );

            float deckY =
                StepRise * 12f;

            CreatePlatform(
                parent,
                "CP3_Deck",
                new Vector3(
                    0f,
                    deckY,
                    110f
                ),
                new Vector3(
                    24f,
                    PlatformHeight,
                    14f
                )
            );

            CreateCheckpoint(
                parent,
                CheckpointId.CP3,
                new Vector3(
                    0f,
                    deckY,
                    110f
                ),
                24f
            );
        }

        private static void CreateSectionFour(
            Transform parent
        )
        {
            float baseY =
                StepRise * 12f;

            CreateStep(
                parent,
                "S4_Step_01",
                4f,
                122f,
                baseY + StepRise
            );

            CreateStep(
                parent,
                "S4_Step_02",
                -4f,
                129f,
                baseY + StepRise * 2f
            );

            CreateStep(
                parent,
                "S4_Step_03",
                4f,
                136f,
                baseY + StepRise * 3f
            );

            float deckY =
                StepRise * 16f;

            CreatePlatform(
                parent,
                "CP4_Deck",
                new Vector3(
                    0f,
                    deckY,
                    146f
                ),
                new Vector3(
                    26f,
                    PlatformHeight,
                    14f
                )
            );

            CreateCheckpoint(
                parent,
                CheckpointId.CP4,
                new Vector3(
                    0f,
                    deckY,
                    146f
                ),
                26f
            );
        }

        private static void CreateFinishArea(
            Transform parent
        )
        {
            float baseY =
                StepRise * 16f;

            CreateStep(
                parent,
                "Final_Step_01",
                -3f,
                158f,
                baseY + StepRise
            );

            CreateStep(
                parent,
                "Final_Step_02",
                3f,
                165f,
                baseY + StepRise * 2f
            );

            float finishY =
                baseY + StepRise * 3f;

            CreatePlatform(
                parent,
                "Finish_Deck",
                new Vector3(
                    0f,
                    finishY,
                    175f
                ),
                new Vector3(
                    30f,
                    PlatformHeight,
                    16f
                )
            );

            FinishOrderManager manager =
                Object.FindFirstObjectByType<
                    FinishOrderManager
                >();

            if (manager == null)
            {
                GameObject managerObject =
                    new GameObject(
                        "Day77_FinishOrderManager"
                    );

                managerObject.transform.SetParent(
                    parent,
                    false
                );

                manager =
                    managerObject.AddComponent<
                        FinishOrderManager
                    >();
            }

            GameObject triggerObject =
                new GameObject(
                    "Finish_Trigger"
                );

            triggerObject.transform.SetParent(
                parent,
                false
            );

            triggerObject.transform.position =
                new Vector3(
                    0f,
                    finishY + 2f,
                    178f
                );

            BoxCollider trigger =
                triggerObject.AddComponent<
                    BoxCollider
                >();

            trigger.isTrigger = true;
            trigger.size =
                new Vector3(
                    26f,
                    4f,
                    2f
                );

            FinishTrigger finishTrigger =
                triggerObject.AddComponent<
                    FinishTrigger
                >();

            finishTrigger.Configure(
                manager
            );

            CreatePlatform(
                parent,
                "Finish_Gate_Left",
                new Vector3(
                    -12f,
                    finishY + 2f,
                    178f
                ),
                new Vector3(
                    1f,
                    4f,
                    1f
                )
            );

            CreatePlatform(
                parent,
                "Finish_Gate_Right",
                new Vector3(
                    12f,
                    finishY + 2f,
                    178f
                ),
                new Vector3(
                    1f,
                    4f,
                    1f
                )
            );

            CreatePlatform(
                parent,
                "Finish_Gate_Top",
                new Vector3(
                    0f,
                    finishY + 4f,
                    178f
                ),
                new Vector3(
                    25f,
                    1f,
                    1f
                )
            );
        }

        private static void CreateCheckpoint(
            Transform parent,
            CheckpointId id,
            Vector3 deckCenter,
            float width
        )
        {
            GameObject respawnObject =
                new GameObject(
                    id + "_Respawn"
                );

            respawnObject.transform.SetParent(
                parent,
                false
            );

            respawnObject.transform.position =
                new Vector3(
                    deckCenter.x,
                    deckCenter.y +
                    PlatformHeight * 0.5f +
                    0.2f,
                    deckCenter.z -
                    2f
                );

            respawnObject.transform.rotation =
                Quaternion.identity;

            GameObject triggerObject =
                new GameObject(
                    id + "_Trigger"
                );

            triggerObject.transform.SetParent(
                parent,
                false
            );

            triggerObject.transform.position =
                new Vector3(
                    deckCenter.x,
                    deckCenter.y + 2f,
                    deckCenter.z
                );

            BoxCollider trigger =
                triggerObject.AddComponent<
                    BoxCollider
                >();

            trigger.isTrigger = true;
            trigger.size =
                new Vector3(
                    width - 2f,
                    4f,
                    3f
                );

            ProjectJ.Checkpoint.Checkpoint checkpoint =
                triggerObject.AddComponent<
                    ProjectJ.Checkpoint.Checkpoint
                >();

            checkpoint.Configure(
                id,
                respawnObject.transform
            );
        }

        private static void ConfigureFallLimits(
            Transform parent
        )
        {
            CheckpointFallLimitSet fallLimits =
                Object.FindFirstObjectByType<
                    CheckpointFallLimitSet
                >();

            if (fallLimits == null)
            {
                GameObject limitObject =
                    new GameObject(
                        "Day77_CheckpointFallLimits"
                    );

                limitObject.transform.SetParent(
                    parent,
                    false
                );

                fallLimits =
                    limitObject.AddComponent<
                        CheckpointFallLimitSet
                    >();
            }

            fallLimits.Configure(
                -6f,
                0f,
                3.5f,
                7f,
                10.5f
            );
        }

        private static void ConfigureSpawnPoints()
        {
            ProjectJNetworkSpawnPoint[] spawnPoints =
                Object.FindObjectsByType<
                    ProjectJNetworkSpawnPoint
                >(
                    FindObjectsSortMode.None
                );

            for (
                int index = 0;
                index < spawnPoints.Length;
                index++
            )
            {
                ProjectJNetworkSpawnPoint spawnPoint =
                    spawnPoints[index];

                if (spawnPoint == null)
                {
                    continue;
                }

                int slot =
                    spawnPoint.SlotIndex;

                int column =
                    slot % 4;

                int row =
                    slot / 4;

                float x =
                    (
                        column -
                        1.5f
                    ) * 4f;

                float z =
                    row == 0
                        ? -3f
                        : 3f;

                spawnPoint.transform.position =
                    new Vector3(
                        x,
                        2f,
                        z
                    );

                spawnPoint.transform.rotation =
                    Quaternion.identity;
            }
        }

        private static void CreateStep(
            Transform parent,
            string objectName,
            float x,
            float z,
            float y
        )
        {
            CreatePlatform(
                parent,
                objectName,
                new Vector3(
                    x,
                    y,
                    z
                ),
                new Vector3(
                    14f,
                    PlatformHeight,
                    8f
                )
            );
        }

        private static GameObject CreatePlatform(
            Transform parent,
            string objectName,
            Vector3 position,
            Vector3 scale
        )
        {
            GameObject platform =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            platform.name =
                objectName;

            platform.transform.SetParent(
                parent,
                false
            );

            platform.transform.position =
                position;

            platform.transform.rotation =
                Quaternion.identity;

            platform.transform.localScale =
                scale;

            return platform;
        }

        private static Transform CreateSection(
            Transform parent,
            string objectName
        )
        {
            GameObject section =
                new GameObject(
                    objectName
                );

            section.transform.SetParent(
                parent,
                false
            );

            return section.transform;
        }

        private static void RemovePreviousMap(
            Scene scene
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
                GameObject root =
                    roots[index];

                if (
                    root != null &&
                    root.name ==
                    MapRootName
                )
                {
                    Object.DestroyImmediate(
                        root
                    );

                    return;
                }
            }
        }
    }
}
