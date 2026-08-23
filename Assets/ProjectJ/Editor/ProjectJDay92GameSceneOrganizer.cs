using UnityEditor; // Editor 메뉴와 AssetDatabase 사용
using UnityEditor.SceneManagement; // Scene 열기와 저장
using UnityEngine; // GameObject와 Transform 사용
using UnityEngine.SceneManagement; // Scene 구조 사용

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay92GameSceneOrganizer
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private const string PlayfieldRootName =
            "=== DAY77 4 PLAYER TEST MAP ===";

        private const string Day76TestRootName =
            "=== DAY76 MULTIPLAYER TEST ===";

        private const string SpawnRootName =
            "SpawnPoints";

        private static readonly string[]
            OrderedPlayfieldChildren =
            {
                "=== SYSTEM ===",
                "=== START ===",
                "=== SECTION 01 / CP1 ===",
                "=== SECTION 02 / CP2 ===",
                "=== SECTION 03 / CP3 ===",
                "=== SECTION 04 / CP4 ===",
                "=== FINISH ==="
            };

        [MenuItem(
            "Project J/Scene/92일차 Game Scene 테스트 경기장 정리"
        )]
        private static void OrganizeGameScene()
        {
            if (
                !EditorSceneManager
                    .SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                return; // 현재 Scene 저장 취소 시 중단
            }

            SceneAsset gameSceneAsset =
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    GameScenePath
                );

            if (gameSceneAsset == null)
            {
                Debug.LogError(
                    "[Project J/Day92] Game Scene을 찾지 못했습니다. / " +
                    GameScenePath
                );

                return;
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    GameScenePath,
                    OpenSceneMode.Single
                ); // 실제 Game Scene 열기

            GameObject playfieldRoot =
                FindDirectRoot(
                    scene,
                    PlayfieldRootName
                );

            if (playfieldRoot == null)
            {
                Debug.LogError(
                    "[Project J/Day92] 고정 테스트 경기장 Root를 찾지 못했습니다. / " +
                    PlayfieldRootName
                );

                return;
            }

            playfieldRoot.transform.SetSiblingIndex(
                0
            ); // 경기장 Root를 Hierarchy 위쪽에 배치

            OrganizePlayfield(
                playfieldRoot.transform
            ); // SYSTEM→START→SECTION→FINISH 순서 정리

            OrganizeSpawnPoints(
                scene
            ); // Day76 Spawn_00~07 순서 정리

            ValidateSceneViewObjects(
                scene
            ); // Camera와 AudioListener 중복 경고

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            ); // Game Scene 변경 저장

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                playfieldRoot;

            EditorGUIUtility.PingObject(
                playfieldRoot
            );

            Debug.Log(
                "[Project J/Day92] Game Scene 테스트 경기장 Hierarchy 정리를 완료했습니다."
            );
        }

        private static void OrganizePlayfield(
            Transform playfieldRoot
        )
        {
            int nextSiblingIndex =
                0;

            for (
                int index = 0;
                index <
                    OrderedPlayfieldChildren.Length;
                index++
            )
            {
                string childName =
                    OrderedPlayfieldChildren[index];

                Transform child =
                    playfieldRoot.Find(
                        childName
                    );

                if (child == null)
                {
                    Debug.LogWarning(
                        "[Project J/Day92] 필수 경기장 Group을 찾지 못했습니다. / " +
                        childName
                    );

                    continue;
                }

                child.SetSiblingIndex(
                    nextSiblingIndex
                ); // 필수 Group 순서 고정

                nextSiblingIndex++;
            }

            ValidateDirectChildDuplicates(
                playfieldRoot
            ); // 필수 Group 중복 검사
        }

        private static void OrganizeSpawnPoints(
            Scene scene
        )
        {
            GameObject testRoot =
                FindDirectRoot(
                    scene,
                    Day76TestRootName
                );

            if (testRoot == null)
            {
                Debug.LogWarning(
                    "[Project J/Day92] Day76 Test Root가 없습니다. Spawn Point 정리를 건너뜁니다."
                );

                return;
            }

            testRoot.transform.SetSiblingIndex(
                Mathf.Min(
                    1,
                    scene.rootCount - 1
                )
            ); // 테스트 Root를 경기장 다음에 배치

            Transform spawnRoot =
                testRoot.transform.Find(
                    SpawnRootName
                );

            if (spawnRoot == null)
            {
                Debug.LogWarning(
                    "[Project J/Day92] SpawnPoints Root가 없습니다."
                );

                return;
            }

            int foundCount =
                0;

            for (
                int slot = 0;
                slot < 8;
                slot++
            )
            {
                string spawnName =
                    "Spawn_" +
                    slot.ToString("00");

                Transform spawn =
                    spawnRoot.Find(
                        spawnName
                    );

                if (spawn == null)
                {
                    Debug.LogWarning(
                        "[Project J/Day92] Spawn Point 누락 / " +
                        spawnName
                    );

                    continue;
                }

                spawn.SetSiblingIndex(
                    slot
                ); // Spawn_00~07 순서 고정

                foundCount++;
            }

            if (foundCount != 8)
            {
                Debug.LogWarning(
                    "[Project J/Day92] Spawn Point가 8개가 아닙니다. / 현재 " +
                    foundCount
                );
            }
        }

        private static void ValidateDirectChildDuplicates(
            Transform parent
        )
        {
            for (
                int requiredIndex = 0;
                requiredIndex <
                    OrderedPlayfieldChildren.Length;
                requiredIndex++
            )
            {
                string requiredName =
                    OrderedPlayfieldChildren[
                        requiredIndex
                    ];

                int count =
                    0;

                for (
                    int childIndex = 0;
                    childIndex <
                        parent.childCount;
                    childIndex++
                )
                {
                    Transform child =
                        parent.GetChild(
                            childIndex
                        );

                    if (
                        child.name ==
                        requiredName
                    )
                    {
                        count++;
                    }
                }

                if (count > 1)
                {
                    Debug.LogWarning(
                        "[Project J/Day92] 중복 Group 발견 / " +
                        requiredName +
                        " / " +
                        count +
                        "개"
                    );
                }
            }
        }

        private static void ValidateSceneViewObjects(
            Scene scene
        )
        {
            Camera[] cameras =
                Object.FindObjectsByType<
                    Camera
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            int activeCameraCount =
                0;

            for (
                int index = 0;
                index < cameras.Length;
                index++
            )
            {
                Camera camera =
                    cameras[index];

                if (
                    camera.gameObject.scene ==
                        scene &&
                    camera.enabled &&
                    camera.gameObject.activeInHierarchy
                )
                {
                    activeCameraCount++;
                }
            }

            if (activeCameraCount > 1)
            {
                Debug.LogWarning(
                    "[Project J/Day92] Game Scene 활성 Camera가 여러 개입니다. / " +
                    activeCameraCount
                );
            }

            AudioListener[] listeners =
                Object.FindObjectsByType<
                    AudioListener
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            int activeListenerCount =
                0;

            for (
                int index = 0;
                index < listeners.Length;
                index++
            )
            {
                AudioListener listener =
                    listeners[index];

                if (
                    listener.gameObject.scene ==
                        scene &&
                    listener.enabled &&
                    listener.gameObject.activeInHierarchy
                )
                {
                    activeListenerCount++;
                }
            }

            if (activeListenerCount > 1)
            {
                Debug.LogWarning(
                    "[Project J/Day92] Game Scene 활성 AudioListener가 여러 개입니다. / " +
                    activeListenerCount
                );
            }
        }

        private static GameObject FindDirectRoot(
            Scene scene,
            string objectName
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
                if (
                    roots[index].name ==
                    objectName
                )
                {
                    return roots[index];
                }
            }

            return null;
        }
    }
}
