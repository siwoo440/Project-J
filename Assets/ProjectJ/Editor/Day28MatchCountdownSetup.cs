using ProjectJ.Match;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day28MatchCountdownSetup
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/Player.prefab";

        private const string TestSceneFolder =
            "Assets/ProjectJ/Tests/Manual/Day28";

        private const string TestScenePath =
            TestSceneFolder +
            "/Day28_MatchCountdownTest.unity";

        [MenuItem(
            "ProjectJ/Day28/Setup Match Countdown"
        )]
        public static void SetupMatchCountdown()
        {
            bool canContinue =
                EditorSceneManager
                    .SaveCurrentModifiedScenesIfUserWantsTo();

            if (!canContinue)
            {
                return;
            }

            SetupGameScene();

            EnsureFolder(
                TestSceneFolder
            );

            CreateTestScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Day28 Ready 안정화 후 3-2-1-시작 카운트다운 설정 완료."
            );
        }

        private static void SetupGameScene()
        {
            Scene scene =
                EditorSceneManager.OpenScene(
                    GameScenePath,
                    OpenSceneMode.Single
                );

            MatchStateController controller =
                Object.FindFirstObjectByType<
                    MatchStateController
                >();

            if (controller == null)
            {
                GameObject managerObject =
                    new GameObject(
                        "=== Match State ==="
                    );

                controller =
                    managerObject.AddComponent<
                        MatchStateController
                    >();
            }

            ConfigureController(
                controller
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );
        }

        private static void CreateTestScene()
        {
            Scene scene =
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single
                );

            MatchStateController controller =
                CreateMatchManager();

            CreateFloor();
            CreatePlayer();
            CreateCamera();
            CreateDirectionalLight();

            MatchStateDebugView debugView =
                controller.gameObject
                    .AddComponent<
                        MatchStateDebugView
                    >();

            debugView.Configure(
                controller
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene,
                TestScenePath
            );

            Selection.activeGameObject =
                controller.gameObject;
        }

        private static MatchStateController
            CreateMatchManager()
        {
            GameObject managerObject =
                new GameObject(
                    "=== Match State ==="
                );

            MatchStateController controller =
                managerObject.AddComponent<
                    MatchStateController
                >();

            ConfigureController(
                controller
            );

            return controller;
        }

        private static void ConfigureController(
            MatchStateController controller
        )
        {
            SerializedObject serializedObject =
                new SerializedObject(
                    controller
                );

            SetFloat(
                serializedObject,
                "countdownStepDuration",
                1.25f
            );

            SetFloat(
                serializedObject,
                "readySettleDuration",
                0.5f
            );

            SetBool(
                serializedObject,
                "autoReadyInOfflineMode",
                true
            );

            serializedObject
                .ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(
                controller
            );
        }

        private static void SetFloat(
            SerializedObject serializedObject,
            string propertyName,
            float value
        )
        {
            SerializedProperty property =
                serializedObject.FindProperty(
                    propertyName
                );

            if (property != null)
            {
                property.floatValue =
                    value;
            }
        }

        private static void SetBool(
            SerializedObject serializedObject,
            string propertyName,
            bool value
        )
        {
            SerializedProperty property =
                serializedObject.FindProperty(
                    propertyName
                );

            if (property != null)
            {
                property.boolValue =
                    value;
            }
        }

        private static void CreateFloor()
        {
            GameObject floor =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            floor.name =
                "Floor";

            floor.transform.position =
                new Vector3(
                    0f,
                    -0.5f,
                    0f
                );

            floor.transform.localScale =
                new Vector3(
                    20f,
                    1f,
                    20f
                );

            int worldLayer =
                LayerMask.NameToLayer(
                    "World"
                );

            if (worldLayer >= 0)
            {
                floor.layer =
                    worldLayer;
            }
        }

        private static void CreatePlayer()
        {
            GameObject playerPrefab =
                AssetDatabase
                    .LoadAssetAtPath<
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

                return;
            }

            GameObject playerObject =
                PrefabUtility
                    .InstantiatePrefab(
                        playerPrefab
                    ) as GameObject;

            if (playerObject == null)
            {
                Debug.LogError(
                    "Player.prefab 인스턴스 생성에 실패했습니다."
                );

                return;
            }

            playerObject.name =
                "Player";

            playerObject.transform.position =
                new Vector3(
                    0f,
                    1.1f,
                    0f
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
                    5f,
                    -9f
                );

            cameraObject.transform.LookAt(
                new Vector3(
                    0f,
                    1f,
                    0f
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
