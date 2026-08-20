using System;
using System.Collections.Generic;
using ProjectJ.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day25ModuleSetup
    {
        private const float ModuleSize = 20f;
        private const float WallThickness = 0.5f;

        private const string PrefabRoot =
            "Assets/ProjectJ/Prefabs/Map/Modules/Day25";

        private const string SceneRoot =
            "Assets/ProjectJ/Tests/Manual/Day25";

        private const string ScenePath =
            SceneRoot +
            "/Day25_ModuleFixedMap.unity";

        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/Player.prefab";

        private static readonly MapModuleFaceDirection[] AllDirections =
        {
            MapModuleFaceDirection.North,
            MapModuleFaceDirection.South,
            MapModuleFaceDirection.East,
            MapModuleFaceDirection.West,
            MapModuleFaceDirection.Up,
            MapModuleFaceDirection.Down
        };

        [MenuItem("ProjectJ/Day25/Create Module Prefabs And Fixed Map")]
        public static void CreateModulePrefabsAndFixedMap()
        {
            EnsureFolder(
                PrefabRoot
            );

            EnsureFolder(
                SceneRoot
            );

            Dictionary<string, GameObject> prefabs =
                CreateModulePrefabs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CreateFixedMapScene(
                prefabs
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Day25 정육면체 Module Prefab과 0m→1000m 고정맵 생성 완료."
            );
        }

        private static Dictionary<string, GameObject> CreateModulePrefabs()
        {
            Dictionary<string, GameObject> prefabs =
                new Dictionary<string, GameObject>();

            prefabs.Add(
                "Straight",
                CreateModulePrefab(
                    "PJ_Module_Straight_SouthNorth",
                    CreateStates(
                        MapModuleFaceDirection.South,
                        MapModuleFaceState.Entrance,
                        MapModuleFaceDirection.North,
                        MapModuleFaceState.Exit
                    ),
                    false
                )
            );

            prefabs.Add(
                "Corner",
                CreateModulePrefab(
                    "PJ_Module_Corner_SouthEast",
                    CreateStates(
                        MapModuleFaceDirection.South,
                        MapModuleFaceState.Entrance,
                        MapModuleFaceDirection.East,
                        MapModuleFaceState.Exit
                    ),
                    false
                )
            );

            prefabs.Add(
                "Vertical",
                CreateModulePrefab(
                    "PJ_Module_Vertical_DownUp",
                    CreateStates(
                        MapModuleFaceDirection.Down,
                        MapModuleFaceState.Entrance,
                        MapModuleFaceDirection.Up,
                        MapModuleFaceState.Exit
                    ),
                    true
                )
            );

            prefabs.Add(
                "Branch",
                CreateModulePrefab(
                    "PJ_Module_Branch_SouthNorthEast",
                    CreateStates(
                        MapModuleFaceDirection.South,
                        MapModuleFaceState.Entrance,
                        MapModuleFaceDirection.North,
                        MapModuleFaceState.Exit,
                        MapModuleFaceDirection.East,
                        MapModuleFaceState.Exit
                    ),
                    false
                )
            );

            prefabs.Add(
                "Merge",
                CreateModulePrefab(
                    "PJ_Module_Merge_SouthWestNorth",
                    CreateStates(
                        MapModuleFaceDirection.South,
                        MapModuleFaceState.Entrance,
                        MapModuleFaceDirection.West,
                        MapModuleFaceState.Entrance,
                        MapModuleFaceDirection.North,
                        MapModuleFaceState.Exit
                    ),
                    false
                )
            );

            prefabs.Add(
                "Drop",
                CreateModulePrefab(
                    "PJ_Module_Drop_SouthNorth_EastDrop",
                    CreateStates(
                        MapModuleFaceDirection.South,
                        MapModuleFaceState.Entrance,
                        MapModuleFaceDirection.North,
                        MapModuleFaceState.Exit,
                        MapModuleFaceDirection.East,
                        MapModuleFaceState.Drop
                    ),
                    false
                )
            );

            prefabs.Add(
                "Start",
                CreateModulePrefab(
                    "PJ_Module_Start_SouthUp",
                    CreateStates(
                        MapModuleFaceDirection.South,
                        MapModuleFaceState.Entrance,
                        MapModuleFaceDirection.Up,
                        MapModuleFaceState.Exit
                    ),
                    true
                )
            );

            return prefabs;
        }

        private static GameObject CreateModulePrefab(
            string moduleId,
            Dictionary<
                MapModuleFaceDirection,
                MapModuleFaceState
            > states,
            bool createVerticalTraversal
        )
        {
            GameObject root =
                new GameObject(
                    moduleId
                );

            MapModule module =
                root.AddComponent<MapModule>();

            Transform geometry =
                CreateEmptyChild(
                    root.transform,
                    "Geometry"
                );

            Transform socketsRoot =
                CreateEmptyChild(
                    root.transform,
                    "Sockets"
                );

            Transform gameplay =
                CreateEmptyChild(
                    root.transform,
                    "Gameplay"
                );

            CreateEmptyChild(
                gameplay,
                "ObstacleSpawnAreas"
            );

            CreateEmptyChild(
                gameplay,
                "ItemSpawnAreas"
            );

            CreateEmptyChild(
                gameplay,
                "NoSpawnAreas"
            );

            MapModuleSocket[] sockets =
                new MapModuleSocket[6];

            for (
                int i = 0;
                i < AllDirections.Length;
                i++
            )
            {
                MapModuleFaceDirection direction =
                    AllDirections[i];

                MapModuleFaceState state =
                    states[direction];

                if (
                    state ==
                    MapModuleFaceState.Closed
                )
                {
                    CreateFaceGeometry(
                        geometry,
                        direction
                    );
                }

                sockets[i] =
                    CreateSocket(
                        socketsRoot,
                        direction,
                        state
                    );
            }

            if (createVerticalTraversal)
            {
                CreateVerticalTraversal(
                    geometry
                );
            }

            module.Configure(
                moduleId,
                ModuleSize,
                sockets
            );

            if (!module.IsDefinitionValid())
            {
                UnityEngine.Object.DestroyImmediate(
                    root
                );

                throw new InvalidOperationException(
                    moduleId +
                    " Module 정의가 유효하지 않습니다."
                );
            }

            string path =
                PrefabRoot +
                "/" +
                moduleId +
                ".prefab";

            GameObject prefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    path
                );

            UnityEngine.Object.DestroyImmediate(
                root
            );

            return prefab;
        }

        private static Dictionary<
            MapModuleFaceDirection,
            MapModuleFaceState
        > CreateStates(
            params object[] values
        )
        {
            Dictionary<
                MapModuleFaceDirection,
                MapModuleFaceState
            > result =
                new Dictionary<
                    MapModuleFaceDirection,
                    MapModuleFaceState
                >();

            for (
                int i = 0;
                i < AllDirections.Length;
                i++
            )
            {
                result.Add(
                    AllDirections[i],
                    MapModuleFaceState.Closed
                );
            }

            for (
                int i = 0;
                i < values.Length;
                i += 2
            )
            {
                MapModuleFaceDirection direction =
                    (MapModuleFaceDirection)
                    values[i];

                MapModuleFaceState state =
                    (MapModuleFaceState)
                    values[i + 1];

                result[direction] =
                    state;
            }

            return result;
        }

        private static void CreateFaceGeometry(
            Transform parent,
            MapModuleFaceDirection direction
        )
        {
            GameObject face =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            face.name =
                GetFaceObjectName(
                    direction
                );

            face.transform.SetParent(
                parent,
                false
            );

            float half =
                ModuleSize * 0.5f;

            float inset =
                half -
                WallThickness * 0.5f;

            switch (direction)
            {
                case MapModuleFaceDirection.North:
                    face.transform.localPosition =
                        new Vector3(
                            0f,
                            0f,
                            inset
                        );

                    face.transform.localScale =
                        new Vector3(
                            ModuleSize,
                            ModuleSize,
                            WallThickness
                        );
                    break;

                case MapModuleFaceDirection.South:
                    face.transform.localPosition =
                        new Vector3(
                            0f,
                            0f,
                            -inset
                        );

                    face.transform.localScale =
                        new Vector3(
                            ModuleSize,
                            ModuleSize,
                            WallThickness
                        );
                    break;

                case MapModuleFaceDirection.East:
                    face.transform.localPosition =
                        new Vector3(
                            inset,
                            0f,
                            0f
                        );

                    face.transform.localScale =
                        new Vector3(
                            WallThickness,
                            ModuleSize,
                            ModuleSize
                        );
                    break;

                case MapModuleFaceDirection.West:
                    face.transform.localPosition =
                        new Vector3(
                            -inset,
                            0f,
                            0f
                        );

                    face.transform.localScale =
                        new Vector3(
                            WallThickness,
                            ModuleSize,
                            ModuleSize
                        );
                    break;

                case MapModuleFaceDirection.Up:
                    face.transform.localPosition =
                        new Vector3(
                            0f,
                            inset,
                            0f
                        );

                    face.transform.localScale =
                        new Vector3(
                            ModuleSize,
                            WallThickness,
                            ModuleSize
                        );
                    break;

                case MapModuleFaceDirection.Down:
                    face.transform.localPosition =
                        new Vector3(
                            0f,
                            -inset,
                            0f
                        );

                    face.transform.localScale =
                        new Vector3(
                            ModuleSize,
                            WallThickness,
                            ModuleSize
                        );
                    break;
            }

            SetWorldLayerRecursively(
                face
            );
        }

        private static MapModuleSocket CreateSocket(
            Transform parent,
            MapModuleFaceDirection direction,
            MapModuleFaceState state
        )
        {
            GameObject socketObject =
                new GameObject(
                    "Socket_" +
                    direction
                );

            socketObject.transform.SetParent(
                parent,
                false
            );

            Vector3 directionVector =
                MapModule.GetDirectionVector(
                    direction
                );

            socketObject.transform.localPosition =
                directionVector *
                (ModuleSize * 0.5f);

            socketObject.transform.localRotation =
                GetSocketRotation(
                    direction
                );

            MapModuleSocket socket =
                socketObject.AddComponent<
                    MapModuleSocket
                >();

            socket.Configure(
                direction,
                state
            );

            return socket;
        }

        private static Quaternion GetSocketRotation(
            MapModuleFaceDirection direction
        )
        {
            switch (direction)
            {
                case MapModuleFaceDirection.Up:
                    return Quaternion.LookRotation(
                        Vector3.up,
                        Vector3.forward
                    );

                case MapModuleFaceDirection.Down:
                    return Quaternion.LookRotation(
                        Vector3.down,
                        Vector3.forward
                    );

                default:
                    return Quaternion.LookRotation(
                        MapModule.GetDirectionVector(
                            direction
                        ),
                        Vector3.up
                    );
            }
        }

        private static void CreateVerticalTraversal(
            Transform parent
        )
        {
            Transform traversal =
                CreateEmptyChild(
                    parent,
                    "VerticalTraversal"
                );

            CreatePlatform(
                traversal,
                "BottomLanding",
                new Vector3(
                    0f,
                    -10f,
                    -7.5f
                ),
                new Vector3(
                    8f,
                    0.5f,
                    4f
                )
            );

            CreateRamp(
                traversal,
                "Ramp_A",
                new Vector3(
                    -3.5f,
                    -9.5f,
                    -7.5f
                ),
                new Vector3(
                    -3.5f,
                    -0.75f,
                    7.5f
                ),
                6f
            );

            CreatePlatform(
                traversal,
                "MiddleLanding",
                new Vector3(
                    0f,
                    -0.25f,
                    7.5f
                ),
                new Vector3(
                    8f,
                    0.5f,
                    4f
                )
            );

            CreateRamp(
                traversal,
                "Ramp_B",
                new Vector3(
                    3.5f,
                    0f,
                    7.5f
                ),
                new Vector3(
                    3.5f,
                    9f,
                    -7.5f
                ),
                6f
            );

            CreatePlatform(
                traversal,
                "TopLanding",
                new Vector3(
                    0f,
                    9.75f,
                    -7.5f
                ),
                new Vector3(
                    8f,
                    0.5f,
                    4f
                )
            );
        }

        private static void CreateRamp(
            Transform parent,
            string name,
            Vector3 start,
            Vector3 end,
            float width
        )
        {
            Vector3 direction =
                end - start;

            float length =
                direction.magnitude;

            GameObject ramp =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            ramp.name = name;

            ramp.transform.SetParent(
                parent,
                false
            );

            ramp.transform.localPosition =
                (start + end) *
                0.5f;

            ramp.transform.localRotation =
                Quaternion.FromToRotation(
                    Vector3.forward,
                    direction.normalized
                );

            ramp.transform.localScale =
                new Vector3(
                    width,
                    0.5f,
                    length
                );

            SetWorldLayerRecursively(
                ramp
            );
        }

        private static void CreatePlatform(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale
        )
        {
            GameObject platform =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            platform.name = name;

            platform.transform.SetParent(
                parent,
                false
            );

            platform.transform.localPosition =
                localPosition;

            platform.transform.localScale =
                localScale;

            SetWorldLayerRecursively(
                platform
            );
        }

        private static void CreateFixedMapScene(
            Dictionary<string, GameObject> prefabs
        )
        {
            Scene scene =
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single
                );

            GameObject courseRoot =
                new GameObject(
                    "=== Day25 Fixed Module Map ==="
                );

            Transform[] sections =
                new Transform[5];

            for (
                int sectionIndex = 0;
                sectionIndex < 5;
                sectionIndex++
            )
            {
                GameObject section =
                    new GameObject(
                        "Section_" +
                        (sectionIndex + 1)
                            .ToString("00")
                    );

                section.transform.SetParent(
                    courseRoot.transform
                );

                sections[sectionIndex] =
                    section.transform;
            }

            const int verticalModuleCount = 50;

            for (
                int i = 0;
                i < verticalModuleCount;
                i++
            )
            {
                GameObject prefab =
                    i == 0
                        ? prefabs["Start"]
                        : prefabs["Vertical"];

                int sectionIndex =
                    Mathf.Clamp(
                        i / 10,
                        0,
                        4
                    );

                GameObject instance =
                    (GameObject)
                    PrefabUtility.InstantiatePrefab(
                        prefab,
                        sections[sectionIndex]
                    );

                instance.name =
                    "Module_" +
                    (i + 1)
                        .ToString("000") +
                    "_Y" +
                    (i * 20)
                        .ToString("0000") +
                    "to" +
                    ((i + 1) * 20)
                        .ToString("0000");

                instance.transform.position =
                    new Vector3(
                        0f,
                        10f +
                        i * ModuleSize,
                        0f
                    );
            }

            CreateAnchor(
                courseRoot.transform,
                "START_0m",
                new Vector3(
                    0f,
                    0f,
                    -7.5f
                )
            );

            CreateAnchor(
                courseRoot.transform,
                "Checkpoint_01_200m",
                new Vector3(
                    0f,
                    200f,
                    -7.5f
                )
            );

            CreateAnchor(
                courseRoot.transform,
                "Checkpoint_02_400m",
                new Vector3(
                    0f,
                    400f,
                    -7.5f
                )
            );

            CreateAnchor(
                courseRoot.transform,
                "Checkpoint_03_600m",
                new Vector3(
                    0f,
                    600f,
                    -7.5f
                )
            );

            CreateAnchor(
                courseRoot.transform,
                "Checkpoint_04_800m",
                new Vector3(
                    0f,
                    800f,
                    -7.5f
                )
            );

            CreateAnchor(
                courseRoot.transform,
                "FINISH_1000m",
                new Vector3(
                    0f,
                    1000f,
                    -7.5f
                )
            );

            CreatePlayerIfAvailable();
            CreateMainCamera();
            CreateDirectionalLight();

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene,
                ScenePath
            );
        }

        private static void CreatePlayerIfAvailable()
        {
            GameObject playerPrefab =
                AssetDatabase.LoadAssetAtPath<
                    GameObject
                >(
                    PlayerPrefabPath
                );

            if (playerPrefab == null)
            {
                Debug.LogWarning(
                    "Player.prefab을 찾지 못했습니다. Day25 Scene에 Player를 직접 배치하세요."
                );

                return;
            }

            GameObject player =
                (GameObject)
                PrefabUtility.InstantiatePrefab(
                    playerPrefab
                );

            player.name =
                "Player";

            player.transform.position =
                new Vector3(
                    0f,
                    1f,
                    -7.5f
                );

            player.transform.rotation =
                Quaternion.identity;
        }

        private static void CreateMainCamera()
        {
            GameObject cameraObject =
                new GameObject(
                    "Main Camera"
                );

            Camera camera =
                cameraObject.AddComponent<
                    Camera
                >();

            cameraObject.tag =
                "MainCamera";

            cameraObject.transform.position =
                new Vector3(
                    0f,
                    5f,
                    -12f
                );

            cameraObject.transform.rotation =
                Quaternion.Euler(
                    15f,
                    0f,
                    0f
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

        private static void CreateAnchor(
            Transform parent,
            string name,
            Vector3 position
        )
        {
            GameObject anchor =
                new GameObject(
                    name
                );

            anchor.transform.SetParent(
                parent
            );

            anchor.transform.position =
                position;
        }

        private static Transform CreateEmptyChild(
            Transform parent,
            string name
        )
        {
            GameObject child =
                new GameObject(
                    name
                );

            child.transform.SetParent(
                parent,
                false
            );

            return child.transform;
        }

        private static string GetFaceObjectName(
            MapModuleFaceDirection direction
        )
        {
            switch (direction)
            {
                case MapModuleFaceDirection.Up:
                    return "Ceiling";

                case MapModuleFaceDirection.Down:
                    return "Floor";

                default:
                    return "Wall_" +
                        direction;
            }
        }

        private static void SetWorldLayerRecursively(
            GameObject gameObject
        )
        {
            int worldLayer =
                LayerMask.NameToLayer(
                    "World"
                );

            if (worldLayer < 0)
            {
                return;
            }

            gameObject.layer =
                worldLayer;

            for (
                int i = 0;
                i <
                gameObject.transform.childCount;
                i++
            )
            {
                SetWorldLayerRecursively(
                    gameObject.transform
                        .GetChild(i)
                        .gameObject
                );
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
