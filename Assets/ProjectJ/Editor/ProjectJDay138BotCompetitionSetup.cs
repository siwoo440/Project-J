using System.IO; // Bot Controller Source 수정 사용
using System.Text; // UTF8 저장 사용
using ProjectJ.Networking.Fusion; // Bot Component와 Roster Manager 사용
using UnityEditor; // Editor 메뉴와 Prefab 수정 사용
using UnityEditor.SceneManagement; // Game Scene 열기와 저장 사용
using UnityEngine; // GameObject와 Debug 사용
using UnityEngine.SceneManagement; // Scene 탐색 사용

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay138BotCompetitionSetup
    {
        private const string BotControllerSourcePath =
            "Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkBotController.cs"; // Bot Controller Source 경로

        private const string BotPrefabPath =
            "Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkBot.prefab"; // Bot Prefab 경로

        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity"; // Game Scene 경로

        private const string RosterRootName =
            "=== DAY138 BOT ROSTER ==="; // Day138 Roster Root 이름

        private static readonly UTF8Encoding Utf8WithoutBom =
            new UTF8Encoding(
                false
            ); // BOM 없는 UTF8 저장

        [MenuItem(
            "Project J/Day138/Apply Bot Competition"
        )]
        private static void ApplyBotCompetition()
        {
            if (!PatchBotControllerSource())
            {
                Debug.LogError(
                    "[Project J/Day138] 최신 main Bot Controller 패턴과 일치하지 않아 자동 Source 수정을 중단했습니다."
                ); // Source 패턴 불일치 오류 출력

                return;
            }

            if (!CreateOrUpdateBotPrefab())
            {
                return; // Bot Prefab 적용 실패 시 중단
            }

            if (!SetupGameScene())
            {
                return; // Game Scene 적용 실패 시 중단
            }

            AssetDatabase.SaveAssets(); // 생성·수정 Asset 저장
            AssetDatabase.Refresh(); // Source 재컴파일 요청

            Debug.Log(
                "[Project J/Day138] Push·Item 판단 및 부족 인원 Bot Roster 적용 완료."
            ); // Day138 적용 완료 출력
        }

        private static bool PatchBotControllerSource()
        {
            if (!File.Exists(BotControllerSourcePath))
            {
                return false; // Bot Controller Source 누락 처리
            }

            string source =
                File.ReadAllText(
                    BotControllerSourcePath
                ); // 최신 Bot Controller Source 읽기

            string newline =
                source.Contains(
                    "\r\n"
                )
                    ? "\r\n"
                    : "\n"; // 기존 줄바꿈 형식 확인

            bool changed =
                false; // Source 변경 여부 초기화

            string gameplayField =
                "        private ProjectJNetworkExternalGameplay externalGameplay; // Respawn 상태 조회 대상"; // 기존 Gameplay Field 패턴

            string actionField =
                "        private ProjectJNetworkBotActionController actionController; // Day138 Push·Item 판단 Controller"; // 신규 Action Field 내용

            if (!source.Contains(actionField))
            {
                if (!source.Contains(gameplayField))
                {
                    return false; // 최신 Field 패턴 불일치
                }

                source =
                    source.Replace(
                        gameplayField,
                        gameplayField +
                        newline +
                        actionField
                    ); // Action Controller Field 추가

                changed =
                    true; // Field 변경 기록
            }

            string observeStuckBlock =
                "            ObserveStuck(" + newline +
                "                player" + newline +
                "            ); // 정체 상태 확인 및 Route 복구"; // 기존 Stuck 호출 패턴

            string actionTickBlock =
                observeStuckBlock +
                newline +
                newline +
                "            if (actionController != null)" +
                newline +
                "            {" +
                newline +
                "                actionController.TickActions(" +
                newline +
                "                    player," +
                newline +
                "                    externalGameplay" +
                newline +
                "                ); // State Authority Push·Item 경쟁 행동 갱신" +
                newline +
                "            }"; // 신규 경쟁 행동 Tick 블록

            if (
                !source.Contains(
                    "actionController.TickActions("
                )
            )
            {
                if (!source.Contains(observeStuckBlock))
                {
                    return false; // 최신 TryBuildInput 패턴 불일치
                }

                source =
                    source.Replace(
                        observeStuckBlock,
                        actionTickBlock
                    ); // 경쟁 행동 Tick 호출 추가

                changed =
                    true; // TryBuildInput 변경 기록
            }

            string gameplayCacheBlock =
                "            externalGameplay =" + newline +
                "                GetComponent<ProjectJNetworkExternalGameplay>(); // Respawn 상태 컴포넌트 조회"; // 기존 Gameplay Cache 패턴

            string actionCacheBlock =
                gameplayCacheBlock +
                newline +
                newline +
                "            actionController =" +
                newline +
                "                GetComponent<ProjectJNetworkBotActionController>(); // Day138 경쟁 행동 컴포넌트 조회"; // 신규 Action Cache 내용

            if (
                !source.Contains(
                    "GetComponent<ProjectJNetworkBotActionController>()"
                )
            )
            {
                if (!source.Contains(gameplayCacheBlock))
                {
                    return false; // 최신 EnsureInitialized 패턴 불일치
                }

                source =
                    source.Replace(
                        gameplayCacheBlock,
                        actionCacheBlock
                    ); // Action Controller Cache 추가

                changed =
                    true; // 초기화 변경 기록
            }

            if (changed)
            {
                File.WriteAllText(
                    BotControllerSourcePath,
                    source,
                    Utf8WithoutBom
                ); // Bot Controller Source 수정 저장
            }

            return true; // Bot Controller Source Patch 성공
        }

        private static bool CreateOrUpdateBotPrefab()
        {
            GameObject prefabRoot =
                PrefabUtility.LoadPrefabContents(
                    BotPrefabPath
                ); // Bot Prefab 편집 Root 로드

            if (prefabRoot == null)
            {
                Debug.LogError(
                    "[Project J/Day138] ProjectJNetworkBot.prefab을 찾지 못했습니다."
                ); // Bot Prefab 누락 오류 출력

                return false;
            }

            try
            {
                if (
                    prefabRoot.GetComponent<ProjectJNetworkBotActionController>() ==
                    null
                )
                {
                    prefabRoot.AddComponent<ProjectJNetworkBotActionController>(); // Bot 경쟁 행동 Component 추가
                }

                PrefabUtility.SaveAsPrefabAsset(
                    prefabRoot,
                    BotPrefabPath
                ); // Bot Prefab 저장
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(
                    prefabRoot
                ); // Prefab 편집 Root 해제
            }

            return true; // Bot Prefab 적용 성공
        }

        private static bool SetupGameScene()
        {
            Scene scene =
                SceneManager.GetSceneByPath(
                    GameScenePath
                ); // 현재 열린 Game Scene 조회

            bool openedBySetup =
                !scene.IsValid() ||
                !scene.isLoaded; // Setup이 Game Scene을 열어야 하는지 확인

            if (
                !openedBySetup &&
                scene.isDirty &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                return false; // 현재 Scene 저장 취소 시 중단
            }

            if (openedBySetup)
            {
                scene =
                    EditorSceneManager.OpenScene(
                        GameScenePath,
                        OpenSceneMode.Additive
                    ); // 현재 Scene 유지 후 Game Scene 추가 열기
            }

            try
            {
                RemoveDay136DevelopmentSpawners(
                    scene
                ); // 기존 1 Bot 개발 Spawner 제거

                GameObject rosterRoot =
                    FindSceneObjectByName(
                        scene,
                        RosterRootName
                    ); // 기존 Day138 Roster Root 검색

                if (rosterRoot == null)
                {
                    rosterRoot =
                        new GameObject(
                            RosterRootName
                        ); // Day138 Roster Root 생성

                    SceneManager.MoveGameObjectToScene(
                        rosterRoot,
                        scene
                    ); // Game Scene에 Roster Root 배치
                }

                ProjectJNetworkBotRosterManager rosterManager =
                    rosterRoot.GetComponent<ProjectJNetworkBotRosterManager>(); // Roster Manager 조회

                if (rosterManager == null)
                {
                    rosterManager =
                        rosterRoot.AddComponent<ProjectJNetworkBotRosterManager>(); // Roster Manager 추가
                }

                int spawnPointCount =
                    CountSpawnPoints(
                        scene
                    ); // 현재 Game Scene Spawn Slot 수 계산

                rosterManager.Configure(
                    Mathf.Max(
                        1,
                        spawnPointCount
                    )
                ); // Spawn Slot 수를 목표 경기 인원으로 적용

                EditorUtility.SetDirty(
                    rosterManager
                ); // Roster 설정 Dirty 처리

                EditorSceneManager.MarkSceneDirty(
                    scene
                ); // Game Scene 변경 표시

                if (!EditorSceneManager.SaveScene(scene))
                {
                    Debug.LogError(
                        "[Project J/Day138] Game Scene 저장에 실패했습니다."
                    ); // Scene 저장 실패 출력

                    return false;
                }
            }
            finally
            {
                if (
                    openedBySetup &&
                    scene.IsValid() &&
                    scene.isLoaded
                )
                {
                    EditorSceneManager.CloseScene(
                        scene,
                        true
                    ); // Setup이 연 Game Scene만 닫기
                }
            }

            return true; // Game Scene 적용 성공
        }

        private static void RemoveDay136DevelopmentSpawners(
            Scene scene
        )
        {
            ProjectJDay136BotTestSpawner[] spawners =
                Object.FindObjectsByType<ProjectJDay136BotTestSpawner>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                ); // 현재 Day136 개발 Spawner 수집

            for (
                int index = 0;
                index < spawners.Length;
                index++
            )
            {
                ProjectJDay136BotTestSpawner spawner =
                    spawners[index]; // 현재 개발 Spawner 조회

                if (
                    spawner == null ||
                    spawner.gameObject.scene !=
                    scene
                )
                {
                    continue; // 다른 Scene Spawner 제외
                }

                Object.DestroyImmediate(
                    spawner
                ); // Day138 Roster와 중복되는 개발 Spawner 제거
            }
        }

        private static int CountSpawnPoints(
            Scene scene
        )
        {
            int count =
                0; // Spawn Point 개수 초기화

            GameObject[] roots =
                scene.GetRootGameObjects(); // Scene Root 목록 조회

            for (
                int index = 0;
                index < roots.Length;
                index++
            )
            {
                count +=
                    CountSpawnPointsRecursive(
                        roots[index].transform
                    ); // Root 하위 Spawn Point 개수 누적
            }

            return count; // 전체 Spawn Point 개수 반환
        }

        private static int CountSpawnPointsRecursive(
            Transform current
        )
        {
            if (current == null)
            {
                return 0; // null Transform 처리
            }

            int count =
                current.name.StartsWith(
                    "Spawn_",
                    System.StringComparison.Ordinal
                )
                    ? 1
                    : 0; // 현재 Transform Spawn 이름 판정

            for (
                int index = 0;
                index < current.childCount;
                index++
            )
            {
                count +=
                    CountSpawnPointsRecursive(
                        current.GetChild(
                            index
                        )
                    ); // 자식 Spawn Point 개수 누적
            }

            return count; // 현재 Hierarchy Spawn Point 개수 반환
        }

        private static GameObject FindSceneObjectByName(
            Scene scene,
            string objectName
        )
        {
            if (
                !scene.IsValid() ||
                string.IsNullOrWhiteSpace(
                    objectName
                )
            )
            {
                return null; // 잘못된 Scene 또는 이름 처리
            }

            GameObject[] roots =
                scene.GetRootGameObjects(); // Scene Root 목록 조회

            for (
                int index = 0;
                index < roots.Length;
                index++
            )
            {
                Transform found =
                    FindChildRecursive(
                        roots[index].transform,
                        objectName
                    ); // Root 하위 이름 재귀 검색

                if (found != null)
                {
                    return found.gameObject; // 대상 GameObject 반환
                }
            }

            return null; // 대상 GameObject 미발견
        }

        private static Transform FindChildRecursive(
            Transform current,
            string objectName
        )
        {
            if (
                current != null &&
                current.name ==
                objectName
            )
            {
                return current; // 현재 Transform 이름 일치
            }

            if (current == null)
            {
                return null; // null Transform 처리
            }

            for (
                int index = 0;
                index < current.childCount;
                index++
            )
            {
                Transform found =
                    FindChildRecursive(
                        current.GetChild(
                            index
                        ),
                        objectName
                    ); // 자식 Hierarchy 재귀 검색

                if (found != null)
                {
                    return found; // 자식 검색 결과 반환
                }
            }

            return null; // 현재 Hierarchy 미발견
        }
    }
}
