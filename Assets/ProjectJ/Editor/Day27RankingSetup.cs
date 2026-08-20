using ProjectJ.Player;
using ProjectJ.Ranking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day27RankingSetup
    {
        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/Player.prefab";

        private const string SceneFolder =
            "Assets/ProjectJ/Tests/Manual/Day27";

        private const string ScenePath =
            SceneFolder +
            "/Day27_RankingTest.unity";

        [MenuItem("ProjectJ/Day27/Setup Ranking System")]
        public static void SetupRankingSystem()
        {
            SetupPlayerPrefab();

            EnsureFolder(
                SceneFolder
            );

            CreateRankingTestScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Day27 실시간 공동 순위 시스템 설정 완료."
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
                    "Player.prefab을 열 수 없습니다: " +
                    PlayerPrefabPath
                );

                return;
            }

            try
            {
                PlayerHeightTracker heightTracker =
                    prefabRoot.GetComponent<
                        PlayerHeightTracker
                    >();

                if (heightTracker == null)
                {
                    Debug.LogError(
                        "Player.prefab에 PlayerHeightTracker가 없습니다. Day26 설정을 먼저 확인하세요."
                    );

                    return;
                }

                PlayerRankingParticipant participant =
                    prefabRoot.GetComponent<
                        PlayerRankingParticipant
                    >();

                if (participant == null)
                {
                    participant =
                        prefabRoot.AddComponent<
                            PlayerRankingParticipant
                        >();
                }

                participant.Configure(
                    -1,
                    heightTracker
                );

                EditorUtility.SetDirty(
                    participant
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

        private static void CreateRankingTestScene()
        {
            Scene scene =
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single
                );

            GameObject managerObject =
                new GameObject(
                    "=== Ranking Manager ==="
                );

            managerObject.AddComponent<
                PlayerRankingManager
            >();

            CreateRankingDummy(
                "Player_01",
                0,
                520.359f
            );

            CreateRankingDummy(
                "Player_02",
                1,
                480.129f
            );

            CreateRankingDummy(
                "Player_03",
                2,
                480.129f
            );

            CreateRankingDummy(
                "Player_04",
                3,
                450f
            );

            CreateDirectionalLight();

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene,
                ScenePath
            );
        }

        private static void CreateRankingDummy(
            string name,
            int playerId,
            float worldHeight
        )
        {
            GameObject playerObject =
                GameObject.CreatePrimitive(
                    PrimitiveType.Capsule
                );

            playerObject.name =
                name;

            playerObject.transform.position =
                new Vector3(
                    playerId * 3f,
                    worldHeight,
                    0f
                );

            Transform footReference =
                new GameObject(
                    "HeightReference_Foot"
                ).transform;

            footReference.SetParent(
                playerObject.transform,
                false
            );

            footReference.localPosition =
                Vector3.zero;

            PlayerHeightTracker heightTracker =
                playerObject.AddComponent<
                    PlayerHeightTracker
                >();

            heightTracker.Configure(
                footReference
            );

            PlayerRankingParticipant participant =
                playerObject.AddComponent<
                    PlayerRankingParticipant
                >();

            participant.Configure(
                playerId,
                heightTracker
            );
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
