using ProjectJ.Finish;
using ProjectJ.Obstacles;
using ProjectJ.Player;
using ProjectJ.Push;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day45AirBagExternalForceSetup
    {
        private const string Phase4ScenePath =
            "Assets/ProjectJ/Tests/Manual/Phase4/" +
            "Phase4_InteractionTest.unity";

        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/" +
            "Player.prefab";

        private const string RootName =
            "=== Day45 AirBag External Force Test Area ===";

        private const string MaterialFolder =
            "Assets/ProjectJ/Tests/Manual/Phase4/" +
            "Materials";

        [MenuItem(
            "ProjectJ/Day45/Setup AirBag External Force"
        )]
        public static void Setup()
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

            GameObject day44Floor =
                GameObject.Find(
                    "Day44_Main_Floor"
                );

            if (day44Floor == null)
            {
                Debug.LogError(
                    "Day44_Main_Floor를 찾을 수 없습니다. " +
                    "먼저 ProjectJ/Day44/Setup Platform Gimmicks를 " +
                    "실행해주세요."
                );

                return;
            }

            RemoveObjectIfExists(
                RootName
            );

            BuildTestArea(
                scene,
                day44Floor
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
                "Day45 에어백 + External Force 통합 " +
                "테스트 구역 구성이 완료되었습니다."
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
                Rigidbody body =
                    prefabRoot.GetComponent<
                        Rigidbody
                    >();

                PlayerFinishState finishState =
                    prefabRoot.GetComponent<
                        PlayerFinishState
                    >();

                PlayerExternalForceAccumulator
                    accumulator =
                        prefabRoot.GetComponent<
                            PlayerExternalForceAccumulator
                        >();

                PlayerExternalForceReceiver
                    receiver =
                        prefabRoot.GetComponent<
                            PlayerExternalForceReceiver
                        >();

                if (receiver == null)
                {
                    receiver =
                        prefabRoot.AddComponent<
                            PlayerExternalForceReceiver
                        >();
                }

                receiver.Configure(
                    body,
                    finishState,
                    accumulator
                );

                EditorUtility.SetDirty(
                    receiver
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
            GameObject day44Floor
        )
        {
            EnsureFolder(
                MaterialFolder
            );

            Material floorMaterial =
                CreateOrLoadMaterial(
                    "Day45_TestFloor.mat",
                    new Color(
                        0.24f,
                        0.27f,
                        0.31f,
                        1f
                    )
                );

            Material airBagMaterial =
                CreateOrLoadMaterial(
                    "Day45_AirBag.mat",
                    new Color(
                        1f,
                        0.45f,
                        0.25f,
                        1f
                    )
                );

            Material markerMaterial =
                CreateOrLoadMaterial(
                    "Day45_DirectionMarker.mat",
                    new Color(
                        1f,
                        0.82f,
                        0.2f,
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

            Collider day44Collider =
                day44Floor.GetComponent<
                    Collider
                >();

            Bounds day44Bounds =
                day44Collider.bounds;

            float floorTopY =
                day44Bounds.max.y;

            float bridgeLength =
                4f;

            float areaWidth =
                26f;

            float areaDepth =
                22f;

            float westEdge =
                day44Bounds.max.x +
                bridgeLength;

            float centerX =
                westEdge +
                areaWidth *
                0.5f;

            float centerZ =
                day44Bounds.center.z;

            GameObject bridge =
                CreateCube(
                    "Day45_Connector_Bridge",
                    new Vector3(
                        day44Bounds.max.x +
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
                    "Day45_Main_Floor",
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
                "DAY45 AIR BAG + EXTERNAL FORCE",
                new Vector3(
                    westEdge + 8f,
                    floorTopY + 3f,
                    centerZ + 9f
                )
            );

            CreateAirBagStation(
                root.transform,
                "AirBag_Basic",
                "BASIC : PUSH +X",
                new Vector3(
                    westEdge + 4f,
                    floorTopY + 1.5f,
                    centerZ - 6f
                ),
                Quaternion.Euler(
                    0f,
                    90f,
                    0f
                ),
                airBagMaterial,
                markerMaterial
            );

            CreateAirBagStation(
                root.transform,
                "AirBag_Rotated",
                "ROTATED : PUSH +Z",
                new Vector3(
                    westEdge + 11f,
                    floorTopY + 1.5f,
                    centerZ - 6f
                ),
                Quaternion.identity,
                airBagMaterial,
                markerMaterial
            );

            CreateAirBagStation(
                root.transform,
                "AirBag_Edge",
                "EDGE : PUSH TO DROP",
                new Vector3(
                    westEdge + 19f,
                    floorTopY + 1.5f,
                    centerZ + 7.5f
                ),
                Quaternion.identity,
                airBagMaterial,
                markerMaterial
            );

            BuildCombinedForceDummy(
                scene,
                root.transform,
                westEdge + 20f,
                centerZ - 5f,
                floorTopY,
                airBagMaterial,
                markerMaterial
            );
        }

        private static void BuildCombinedForceDummy(
            Scene scene,
            Transform parent,
            float centerX,
            float centerZ,
            float floorTopY,
            Material airBagMaterial,
            Material markerMaterial
        )
        {
            CreateLabel(
                parent,
                "PUSH DUMMY INTO AIR BAG",
                new Vector3(
                    centerX,
                    floorTopY + 3f,
                    centerZ - 3f
                )
            );

            CreateAirBagStation(
                parent,
                "AirBag_Combined",
                "",
                new Vector3(
                    centerX - 3f,
                    floorTopY + 1.5f,
                    centerZ
                ),
                Quaternion.Euler(
                    0f,
                    90f,
                    0f
                ),
                airBagMaterial,
                markerMaterial
            );

            GameObject playerPrefab =
                AssetDatabase.LoadAssetAtPath<
                    GameObject
                >(
                    PlayerPrefabPath
                );

            if (playerPrefab == null)
            {
                return;
            }

            GameObject dummy =
                PrefabUtility.InstantiatePrefab(
                    playerPrefab,
                    scene
                ) as GameObject;

            if (dummy == null)
            {
                return;
            }

            dummy.name =
                "Day45_ExternalForce_Dummy";

            dummy.transform.SetParent(
                parent,
                true
            );

            dummy.transform.position =
                new Vector3(
                    centerX,
                    floorTopY + 1.05f,
                    centerZ
                );

            DisableDummyLocalControl(
                dummy
            );
        }

        private static void DisableDummyLocalControl(
            GameObject dummy
        )
        {
            PlayerInput input =
                dummy.GetComponent<
                    PlayerInput
                >();

            if (input != null)
            {
                input.enabled =
                    false;
            }

            PlayerSurfaceInteraction
                surfaceInteraction =
                    dummy.GetComponent<
                        PlayerSurfaceInteraction
                    >();

            if (surfaceInteraction != null)
            {
                surfaceInteraction.enabled =
                    false;
            }

            PlayerPushController pushController =
                dummy.GetComponent<
                    PlayerPushController
                >();

            if (pushController != null)
            {
                pushController.enabled =
                    false;
            }

            PlayerPushFeedbackUI feedback =
                dummy.GetComponent<
                    PlayerPushFeedbackUI
                >();

            if (feedback != null)
            {
                feedback.enabled =
                    false;
            }

            Transform feedbackRoot =
                dummy.transform.Find(
                    "=== Push Feedback UI ==="
                );

            if (feedbackRoot != null)
            {
                feedbackRoot.gameObject
                    .SetActive(
                        false
                    );
            }

            Camera[] cameras =
                dummy.GetComponentsInChildren<
                    Camera
                >(
                    true
                );

            foreach (Camera camera in cameras)
            {
                camera.enabled =
                    false;
            }

            AudioListener[] listeners =
                dummy.GetComponentsInChildren<
                    AudioListener
                >(
                    true
                );

            foreach (
                AudioListener listener in listeners
            )
            {
                listener.enabled =
                    false;
            }
        }

        private static void CreateAirBagStation(
            Transform parent,
            string objectName,
            string label,
            Vector3 position,
            Quaternion rotation,
            Material material,
            Material markerMaterial
        )
        {
            GameObject airBag =
                CreateCube(
                    objectName,
                    position,
                    new Vector3(
                        0.9f,
                        3f,
                        4f
                    ),
                    material,
                    ObstacleLayer()
                );

            airBag.transform.SetParent(
                parent,
                true
            );

            airBag.transform.rotation =
                rotation;

            AirBagObstacle obstacle =
                airBag.AddComponent<
                    AirBagObstacle
                >();

            obstacle.Configure(
                12f,
                Vector3.forward,
                0.35f
            );

            GameObject marker =
                CreateCube(
                    objectName +
                    "_DirectionMarker",
                    position +
                    rotation *
                    Vector3.forward *
                    2.8f,
                    new Vector3(
                        0.35f,
                        0.15f,
                        2.2f
                    ),
                    markerMaterial,
                    WorldLayer()
                );

            marker.transform.SetParent(
                parent,
                true
            );

            marker.transform.rotation =
                rotation;

            Collider markerCollider =
                marker.GetComponent<
                    Collider
                >();

            if (markerCollider != null)
            {
                Object.DestroyImmediate(
                    markerCollider
                );
            }

            if (!string.IsNullOrEmpty(label))
            {
                CreateLabel(
                    parent,
                    label,
                    position +
                    Vector3.up *
                    2.7f
                );
            }
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
                0.22f;

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

        private static int WorldLayer()
        {
            int layer =
                LayerMask.NameToLayer(
                    "World"
                );

            return
                layer >= 0
                    ? layer
                    : 9;
        }

        private static int ObstacleLayer()
        {
            int layer =
                LayerMask.NameToLayer(
                    "Obstacle"
                );

            return
                layer >= 0
                    ? layer
                    : WorldLayer();
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
