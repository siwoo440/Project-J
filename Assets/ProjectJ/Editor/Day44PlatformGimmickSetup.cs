using ProjectJ.Platforms;
using ProjectJ.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day44PlatformGimmickSetup
    {
        private const string Phase4ScenePath =
            "Assets/ProjectJ/Tests/Manual/Phase4/" +
            "Phase4_InteractionTest.unity";

        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/" +
            "Player.prefab";

        private const string RootName =
            "=== Day44 Platform Gimmick Test Area ===";

        private const string MaterialFolder =
            "Assets/ProjectJ/Tests/Manual/Phase4/" +
            "Materials";

        [MenuItem(
            "ProjectJ/Day44/Setup Platform Gimmicks"
        )]
        public static void SetupPlatformGimmicks()
        {
            SetupPlayerPrefab();

            SceneAsset sceneAsset =
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    Phase4ScenePath
                );

            if (sceneAsset == null)
            {
                Debug.LogError(
                    "Phase4 테스트 Scene을 찾을 수 없습니다: " +
                    Phase4ScenePath
                );

                return;
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    Phase4ScenePath,
                    OpenSceneMode.Single
                );

            GameObject mainFloor =
                GameObject.Find(
                    "Phase4_Main_Floor"
                );

            if (mainFloor == null)
            {
                Debug.LogError(
                    "Phase4_Main_Floor를 찾을 수 없습니다."
                );

                return;
            }

            RemoveObjectIfExists(
                RootName
            );

            RemoveObjectIfExists(
                "Phase4_East_Wall"
            );

            BuildTestArea(
                scene,
                mainFloor
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
                GameObject.Find(
                    RootName
                );

            Debug.Log(
                "Day44 플랫폼·표면 기믹 5종 테스트 구역 " +
                "구성이 완료되었습니다."
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
                    "Player.prefab을 찾을 수 없습니다."
                );

                return;
            }

            try
            {
                PlayerSurfaceInteraction
                    surfaceInteraction =
                        prefabRoot.GetComponent<
                            PlayerSurfaceInteraction
                        >();

                if (surfaceInteraction == null)
                {
                    surfaceInteraction =
                        prefabRoot.AddComponent<
                            PlayerSurfaceInteraction
                        >();
                }

                int surfaceMask =
                    CreateSurfaceMask();

                surfaceInteraction.Configure(
                    surfaceMask,
                    0.55f,
                    0.25f
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

        private static void BuildTestArea(
            Scene scene,
            GameObject mainFloor
        )
        {
            EnsureFolder(
                MaterialFolder
            );

            Material floorMaterial =
                CreateOrLoadMaterial(
                    "Day44_TestFloor.mat",
                    new Color(
                        0.25f,
                        0.28f,
                        0.32f,
                        1f
                    )
                );

            Material movingMaterial =
                CreateOrLoadMaterial(
                    "Day44_Moving.mat",
                    new Color(
                        0.25f,
                        0.65f,
                        0.85f,
                        1f
                    )
                );

            Material rotatingMaterial =
                CreateOrLoadMaterial(
                    "Day44_Rotating.mat",
                    new Color(
                        0.75f,
                        0.45f,
                        0.85f,
                        1f
                    )
                );

            Material springMaterial =
                CreateOrLoadMaterial(
                    "Day44_Spring.mat",
                    new Color(
                        0.4f,
                        0.85f,
                        0.4f,
                        1f
                    )
                );

            Material iceMaterial =
                CreateOrLoadMaterial(
                    "Day44_Ice.mat",
                    new Color(
                        0.55f,
                        0.85f,
                        1f,
                        1f
                    )
                );

            Material ghostMaterial =
                CreateOrLoadMaterial(
                    "Day44_Ghost.mat",
                    new Color(
                        0.8f,
                        0.8f,
                        0.95f,
                        1f
                    )
                );

            GameObject root =
                new GameObject(
                    RootName
                );

            SceneManager.MoveGameObjectToScene(
                root,
                scene
            );

            Bounds mainBounds =
                mainFloor.GetComponent<
                    Collider
                >().bounds;

            float floorTopY =
                mainBounds.max.y;

            float bridgeLength =
                5f;

            float areaWidth =
                44f;

            float areaDepth =
                30f;

            float westEdge =
                mainBounds.max.x +
                bridgeLength;

            float centerX =
                westEdge +
                areaWidth *
                0.5f;

            float centerZ =
                mainBounds.center.z;

            GameObject bridge =
                CreateCube(
                    "Day44_Connector_Bridge",
                    new Vector3(
                        mainBounds.max.x +
                        bridgeLength *
                        0.5f,
                        floorTopY -
                        0.1f,
                        centerZ
                    ),
                    new Vector3(
                        bridgeLength,
                        0.2f,
                        5f
                    ),
                    floorMaterial,
                    WorldLayer()
                );

            bridge.transform.SetParent(
                root.transform,
                true
            );

            GameObject floor =
                CreateCube(
                    "Day44_Main_Floor",
                    new Vector3(
                        centerX,
                        floorTopY -
                        0.1f,
                        centerZ
                    ),
                    new Vector3(
                        areaWidth,
                        0.2f,
                        areaDepth
                    ),
                    floorMaterial,
                    WorldLayer()
                );

            floor.transform.SetParent(
                root.transform,
                true
            );

            CreateLabel(
                root.transform,
                "DAY44 PLATFORM GIMMICKS",
                new Vector3(
                    westEdge + 3f,
                    floorTopY + 2.6f,
                    centerZ + 12f
                )
            );

            BuildMovingPlatform(
                root.transform,
                westEdge + 7f,
                centerZ - 8f,
                floorTopY,
                movingMaterial
            );

            BuildRotatingPlatform(
                root.transform,
                westEdge + 19f,
                centerZ - 8f,
                floorTopY,
                rotatingMaterial
            );

            BuildSpringPlatform(
                root.transform,
                westEdge + 32f,
                centerZ - 8f,
                floorTopY,
                springMaterial,
                floorMaterial
            );

            BuildIceLane(
                root.transform,
                westEdge + 11f,
                centerZ + 6f,
                floorTopY,
                iceMaterial
            );

            BuildGhostPlatforms(
                root.transform,
                westEdge + 27f,
                centerZ + 6f,
                floorTopY,
                ghostMaterial
            );
        }

        private static void BuildMovingPlatform(
            Transform parent,
            float centerX,
            float centerZ,
            float floorTopY,
            Material material
        )
        {
            CreateLabel(
                parent,
                "MOVING PLATFORM",
                new Vector3(
                    centerX,
                    floorTopY + 2.3f,
                    centerZ - 4f
                )
            );

            GameObject routeRoot =
                new GameObject(
                    "MovingPlatform_Route"
                );

            routeRoot.transform.SetParent(
                parent,
                true
            );

            Transform pointA =
                CreatePoint(
                    routeRoot.transform,
                    "PointA",
                    new Vector3(
                        centerX - 3.5f,
                        floorTopY + 1.2f,
                        centerZ
                    )
                );

            Transform pointB =
                CreatePoint(
                    routeRoot.transform,
                    "PointB",
                    new Vector3(
                        centerX + 3.5f,
                        floorTopY + 1.2f,
                        centerZ
                    )
                );

            GameObject platform =
                CreateCube(
                    "MovingPlatform",
                    pointA.position,
                    new Vector3(
                        4f,
                        0.45f,
                        4f
                    ),
                    material,
                    WorldLayer()
                );

            platform.transform.SetParent(
                routeRoot.transform,
                true
            );

            Rigidbody body =
                platform.AddComponent<
                    Rigidbody
                >();

            body.isKinematic =
                true;

            body.useGravity =
                false;

            body.interpolation =
                RigidbodyInterpolation
                    .Interpolate;

            PlatformPassengerCarrier carrier =
                platform.AddComponent<
                    PlatformPassengerCarrier
                >();

            carrier.Configure(
                0.4f,
                0.92f,
                PlayerMask()
            );

            MovingPlatform moving =
                platform.AddComponent<
                    MovingPlatform
                >();

            moving.Configure(
                pointA,
                pointB,
                2.5f
            );
        }

        private static void BuildRotatingPlatform(
            Transform parent,
            float centerX,
            float centerZ,
            float floorTopY,
            Material material
        )
        {
            CreateLabel(
                parent,
                "ROTATING PLATFORM",
                new Vector3(
                    centerX,
                    floorTopY + 2.3f,
                    centerZ - 4f
                )
            );

            GameObject platform =
                CreateCube(
                    "RotatingPlatform",
                    new Vector3(
                        centerX,
                        floorTopY + 1.2f,
                        centerZ
                    ),
                    new Vector3(
                        6f,
                        0.45f,
                        6f
                    ),
                    material,
                    WorldLayer()
                );

            platform.transform.SetParent(
                parent,
                true
            );

            Rigidbody body =
                platform.AddComponent<
                    Rigidbody
                >();

            body.isKinematic =
                true;

            body.useGravity =
                false;

            body.interpolation =
                RigidbodyInterpolation
                    .Interpolate;

            PlatformPassengerCarrier carrier =
                platform.AddComponent<
                    PlatformPassengerCarrier
                >();

            carrier.Configure(
                0.4f,
                0.92f,
                PlayerMask()
            );

            RotatingPlatform rotating =
                platform.AddComponent<
                    RotatingPlatform
                >();

            rotating.Configure(
                Vector3.up,
                35f
            );
        }

        private static void BuildSpringPlatform(
            Transform parent,
            float centerX,
            float centerZ,
            float floorTopY,
            Material springMaterial,
            Material floorMaterial
        )
        {
            CreateLabel(
                parent,
                "SPRING JUMP x1.5",
                new Vector3(
                    centerX,
                    floorTopY + 2.3f,
                    centerZ - 4f
                )
            );

            GameObject spring =
                CreateCube(
                    "SpringPlatform",
                    new Vector3(
                        centerX,
                        floorTopY + 0.15f,
                        centerZ
                    ),
                    new Vector3(
                        4f,
                        0.3f,
                        4f
                    ),
                    springMaterial,
                    WorldLayer()
                );

            spring.transform.SetParent(
                parent,
                true
            );

            SpringPlatform springPlatform =
                spring.AddComponent<
                    SpringPlatform
                >();

            springPlatform.Configure(
                1.5f
            );

            GameObject target =
                CreateCube(
                    "SpringJump_HeightTarget",
                    new Vector3(
                        centerX,
                        floorTopY + 2.15f,
                        centerZ + 4.5f
                    ),
                    new Vector3(
                        5f,
                        0.4f,
                        4f
                    ),
                    floorMaterial,
                    WorldLayer()
                );

            target.transform.SetParent(
                parent,
                true
            );
        }

        private static void BuildIceLane(
            Transform parent,
            float centerX,
            float centerZ,
            float floorTopY,
            Material material
        )
        {
            CreateLabel(
                parent,
                "ICE LANE",
                new Vector3(
                    centerX,
                    floorTopY + 2.3f,
                    centerZ - 4f
                )
            );

            GameObject ice =
                CreateCube(
                    "IceSurface",
                    new Vector3(
                        centerX,
                        floorTopY + 0.08f,
                        centerZ
                    ),
                    new Vector3(
                        16f,
                        0.16f,
                        4f
                    ),
                    material,
                    WorldLayer()
                );

            ice.transform.SetParent(
                parent,
                true
            );

            IceSurface iceSurface =
                ice.AddComponent<
                    IceSurface
                >();

            iceSurface.Configure(
                6f,
                2.5f,
                3f
            );
        }

        private static void BuildGhostPlatforms(
            Transform parent,
            float centerX,
            float centerZ,
            float floorTopY,
            Material material
        )
        {
            CreateLabel(
                parent,
                "GHOST PLATFORMS",
                new Vector3(
                    centerX,
                    floorTopY + 2.3f,
                    centerZ - 4f
                )
            );

            for (
                int i = 0;
                i < 4;
                i++
            )
            {
                GameObject ghost =
                    CreateCube(
                        "GhostPlatform_" +
                        (i + 1),
                        new Vector3(
                            centerX -
                            5.25f +
                            i *
                            3.5f,
                            floorTopY + 1.1f,
                            centerZ
                        ),
                        new Vector3(
                            3f,
                            0.4f,
                            3f
                        ),
                        material,
                        WorldLayer()
                    );

                ghost.transform.SetParent(
                    parent,
                    true
                );

                GhostPlatform ghostPlatform =
                    ghost.AddComponent<
                        GhostPlatform
                    >();

                ghostPlatform.Configure(
                    3f,
                    1f,
                    2f,
                    i * 0.55f,
                    PlayerMask()
                );
            }
        }

        private static Transform CreatePoint(
            Transform parent,
            string objectName,
            Vector3 position
        )
        {
            GameObject point =
                new GameObject(
                    objectName
                );

            point.transform.SetParent(
                parent,
                true
            );

            point.transform.position =
                position;

            return point.transform;
        }

        private static GameObject CreateCube(
            string objectName,
            Vector3 position,
            Vector3 scale,
            Material material,
            int layer
        )
        {
            GameObject cube =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            cube.name =
                objectName;

            cube.transform.position =
                position;

            cube.transform.localScale =
                scale;

            cube.layer =
                layer;

            Renderer renderer =
                cube.GetComponent<
                    Renderer
                >();

            if (
                renderer != null &&
                material != null
            )
            {
                renderer.sharedMaterial =
                    material;
            }

            return cube;
        }

        private static void CreateLabel(
            Transform parent,
            string text,
            Vector3 position
        )
        {
            GameObject labelObject =
                new GameObject(
                    "Label_" +
                    text.Replace(
                        " ",
                        "_"
                    )
                );

            labelObject.transform.SetParent(
                parent,
                true
            );

            labelObject.transform.position =
                position;

            labelObject.transform.rotation =
                Quaternion.Euler(
                    0f,
                    180f,
                    0f
                );

            TextMesh textMesh =
                labelObject.AddComponent<
                    TextMesh
                >();

            textMesh.text =
                text;

            textMesh.anchor =
                TextAnchor.MiddleCenter;

            textMesh.alignment =
                TextAlignment.Center;

            textMesh.characterSize =
                0.28f;

            textMesh.fontSize =
                48;
        }

        private static Material CreateOrLoadMaterial(
            string fileName,
            Color color
        )
        {
            string path =
                MaterialFolder +
                "/" +
                fileName;

            Material material =
                AssetDatabase.LoadAssetAtPath<
                    Material
                >(
                    path
                );

            if (material == null)
            {
                Shader shader =
                    Shader.Find(
                        "Universal Render Pipeline/Lit"
                    );

                if (shader == null)
                {
                    shader =
                        Shader.Find(
                            "Standard"
                        );
                }

                material =
                    new Material(
                        shader
                    );

                AssetDatabase.CreateAsset(
                    material,
                    path
                );
            }

            material.color =
                color;

            EditorUtility.SetDirty(
                material
            );

            return material;
        }

        private static int CreateSurfaceMask()
        {
            int mask =
                0;

            int worldLayer =
                LayerMask.NameToLayer(
                    "World"
                );

            int obstacleLayer =
                LayerMask.NameToLayer(
                    "Obstacle"
                );

            if (worldLayer >= 0)
            {
                mask |=
                    1 <<
                    worldLayer;
            }

            if (obstacleLayer >= 0)
            {
                mask |=
                    1 <<
                    obstacleLayer;
            }

            return mask;
        }

        private static int PlayerMask()
        {
            int playerLayer =
                LayerMask.NameToLayer(
                    "Player"
                );

            return
                playerLayer >= 0
                    ? 1 << playerLayer
                    : 1 << 8;
        }

        private static int WorldLayer()
        {
            int worldLayer =
                LayerMask.NameToLayer(
                    "World"
                );

            return
                worldLayer >= 0
                    ? worldLayer
                    : 9;
        }

        private static void RemoveObjectIfExists(
            string objectName
        )
        {
            GameObject existing =
                GameObject.Find(
                    objectName
                );

            if (existing != null)
            {
                Object.DestroyImmediate(
                    existing
                );
            }
        }

        private static void EnsureFolder(
            string folderPath
        )
        {
            string[] parts =
                folderPath.Split(
                    '/'
                );

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
