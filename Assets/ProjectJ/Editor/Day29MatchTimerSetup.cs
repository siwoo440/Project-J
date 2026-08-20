using ProjectJ.Match;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectJ.Editor
{
    public static class Day29MatchTimerSetup
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private const string Day28TestScenePath =
            "Assets/ProjectJ/Tests/Manual/Day28/" +
            "Day28_MatchCountdownTest.unity";

        private const string Day29TestSceneFolder =
            "Assets/ProjectJ/Tests/Manual/Day29";

        private const string Day29TestScenePath =
            Day29TestSceneFolder +
            "/Day29_MatchTimerTest.unity";

        private const float ProductionDurationSeconds =
            MatchTimer.DefaultMatchDurationSeconds;

        private const float ManualTestDurationSeconds =
            65f;

        [MenuItem(
            "ProjectJ/Day29/Setup 15 Minute Match Timer"
        )]
        public static void Setup15MinuteMatchTimer()
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
                Day29TestSceneFolder
            );

            CreateManualTestScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Day29 15분 경기 타이머 설정 완료. " +
                "경고: 1분 / 30초 / 10초."
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
                Debug.LogError(
                    "Game Scene에 MatchStateController가 없습니다. " +
                    "Day28 설정을 먼저 확인하세요."
                );

                return;
            }

            MatchTimer timer =
                GetOrAddTimer(
                    controller.gameObject
                );

            timer.Configure(
                controller,
                ProductionDurationSeconds
            );

            MatchTimerDebugView debugView =
                GetOrAddDebugView(
                    controller.gameObject
                );

            debugView.Configure(
                timer
            );

            EditorUtility.SetDirty(
                timer
            );

            EditorUtility.SetDirty(
                debugView
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
                !AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    Day28TestScenePath
                )
            )
            {
                Debug.LogError(
                    "Day28 테스트 Scene을 찾을 수 없습니다: " +
                    Day28TestScenePath
                );

                return;
            }

            if (
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    Day29TestScenePath
                )
            )
            {
                AssetDatabase.DeleteAsset(
                    Day29TestScenePath
                );
            }

            bool copied =
                AssetDatabase.CopyAsset(
                    Day28TestScenePath,
                    Day29TestScenePath
                );

            if (!copied)
            {
                Debug.LogError(
                    "Day29 테스트 Scene 복사에 실패했습니다."
                );

                return;
            }

            AssetDatabase.Refresh();

            Scene scene =
                EditorSceneManager.OpenScene(
                    Day29TestScenePath,
                    OpenSceneMode.Single
                );

            MatchStateController controller =
                Object.FindFirstObjectByType<
                    MatchStateController
                >();

            if (controller == null)
            {
                Debug.LogError(
                    "Day29 테스트 Scene에 MatchStateController가 없습니다."
                );

                return;
            }

            MatchTimer timer =
                GetOrAddTimer(
                    controller.gameObject
                );

            timer.Configure(
                controller,
                ManualTestDurationSeconds
            );

            MatchTimerDebugView debugView =
                GetOrAddDebugView(
                    controller.gameObject
                );

            debugView.Configure(
                timer
            );

            EditorUtility.SetDirty(
                timer
            );

            EditorUtility.SetDirty(
                debugView
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );

            Selection.activeGameObject =
                controller.gameObject;
        }

        private static MatchTimer GetOrAddTimer(
            GameObject target
        )
        {
            MatchTimer timer =
                target.GetComponent<
                    MatchTimer
                >();

            if (timer == null)
            {
                timer =
                    target.AddComponent<
                        MatchTimer
                    >();
            }

            return timer;
        }

        private static MatchTimerDebugView
            GetOrAddDebugView(
                GameObject target
            )
        {
            MatchTimerDebugView debugView =
                target.GetComponent<
                    MatchTimerDebugView
                >();

            if (debugView == null)
            {
                debugView =
                    target.AddComponent<
                        MatchTimerDebugView
                    >();
            }

            return debugView;
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
