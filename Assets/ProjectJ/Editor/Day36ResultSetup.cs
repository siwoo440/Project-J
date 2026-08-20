using ProjectJ.Checkpoint;
using ProjectJ.Finish;
using ProjectJ.Match;
using ProjectJ.Player;
using ProjectJ.Ranking;
using ProjectJ.Results;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day36ResultSetup
    {
        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/" +
            "Player.prefab";

        private const string Day35ScenePath =
            "Assets/ProjectJ/Tests/Manual/Day35/" +
            "Day35_FinishOrderTest.unity";

        private const string Day36Folder =
            "Assets/ProjectJ/Tests/Manual/Day36";

        private const string Day36ScenePath =
            Day36Folder +
            "/Day36_PersonalResultTest.unity";

        [MenuItem(
            "ProjectJ/Day36/Setup Personal Result"
        )]
        public static void SetupPersonalResult()
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

            EnsureFolder(
                Day36Folder
            );

            CreateManualTestScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Day36 개인 결과 데이터 설정 완료."
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
                PlayerFinishState finishState =
                    prefabRoot.GetComponent<
                        PlayerFinishState
                    >();

                PlayerRankingParticipant ranking =
                    prefabRoot.GetComponent<
                        PlayerRankingParticipant
                    >();

                PlayerHeightTracker height =
                    prefabRoot.GetComponent<
                        PlayerHeightTracker
                    >();

                PlayerCheckpointTracker checkpoint =
                    prefabRoot.GetComponent<
                        PlayerCheckpointTracker
                    >();

                if (
                    finishState == null ||
                    ranking == null ||
                    height == null ||
                    checkpoint == null
                )
                {
                    Debug.LogError(
                        "Day36 선행 컴포넌트가 부족합니다. " +
                        "Day26, Day30, Day35 설정을 확인하세요."
                    );

                    return;
                }

                PlayerMatchResultCollector collector =
                    prefabRoot.GetComponent<
                        PlayerMatchResultCollector
                    >();

                if (collector == null)
                {
                    collector =
                        prefabRoot.AddComponent<
                            PlayerMatchResultCollector
                        >();
                }

                collector.Configure(
                    finishState,
                    ranking,
                    height,
                    checkpoint,
                    null
                );

                EditorUtility.SetDirty(
                    collector
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

        private static void CreateManualTestScene()
        {
            if (
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    Day35ScenePath
                ) == null
            )
            {
                Debug.LogError(
                    "Day35 테스트 Scene을 찾을 수 없습니다: " +
                    Day35ScenePath
                );

                return;
            }

            if (
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    Day36ScenePath
                ) != null
            )
            {
                AssetDatabase.DeleteAsset(
                    Day36ScenePath
                );
            }

            bool copied =
                AssetDatabase.CopyAsset(
                    Day35ScenePath,
                    Day36ScenePath
                );

            if (!copied)
            {
                Debug.LogError(
                    "Day36 테스트 Scene 복사에 실패했습니다."
                );

                return;
            }

            AssetDatabase.Refresh();

            Scene scene =
                EditorSceneManager.OpenScene(
                    Day36ScenePath,
                    OpenSceneMode.Single
                );

            PlayerMatchResultCollector collector =
                FindOrCreateCollector();

            SetupDebugView(
                collector
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );

            if (collector != null)
            {
                Selection.activeGameObject =
                    collector.gameObject;
            }
        }

        private static PlayerMatchResultCollector
            FindOrCreateCollector()
        {
            PlayerFinishState finishState =
                Object.FindFirstObjectByType<
                    PlayerFinishState
                >();

            if (finishState == null)
            {
                Debug.LogError(
                    "Scene에서 PlayerFinishState를 " +
                    "찾을 수 없습니다."
                );

                return null;
            }

            GameObject playerObject =
                finishState.gameObject;

            PlayerRankingParticipant ranking =
                playerObject.GetComponent<
                    PlayerRankingParticipant
                >();

            PlayerHeightTracker height =
                playerObject.GetComponent<
                    PlayerHeightTracker
                >();

            PlayerCheckpointTracker checkpoint =
                playerObject.GetComponent<
                    PlayerCheckpointTracker
                >();

            PlayerMatchResultCollector collector =
                playerObject.GetComponent<
                    PlayerMatchResultCollector
                >();

            if (collector == null)
            {
                collector =
                    playerObject.AddComponent<
                        PlayerMatchResultCollector
                    >();
            }

            MatchTimer timer =
                Object.FindFirstObjectByType<
                    MatchTimer
                >();

            collector.Configure(
                finishState,
                ranking,
                height,
                checkpoint,
                timer
            );

            EditorUtility.SetDirty(
                collector
            );

            return collector;
        }

        private static void SetupDebugView(
            PlayerMatchResultCollector collector
        )
        {
            if (collector == null)
            {
                return;
            }

            PlayerMatchResultDebugView debugView =
                Object.FindFirstObjectByType<
                    PlayerMatchResultDebugView
                >();

            if (debugView == null)
            {
                GameObject debugObject =
                    new GameObject(
                        "Personal Result Debug"
                    );

                debugView =
                    debugObject.AddComponent<
                        PlayerMatchResultDebugView
                    >();
            }

            debugView.Configure(
                collector
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
