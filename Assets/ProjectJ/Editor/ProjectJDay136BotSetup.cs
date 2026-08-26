using System.Collections.Generic; // Asset Label 목록 사용
using System.IO; // 기존 C# Source 수정 사용
using System.Text; // UTF8 BOM 없는 저장 사용
using ProjectJ.AI; // Bot Route Node 생성 사용
using ProjectJ.Networking.Fusion; // Bot Component와 Spawner 사용
using UnityEditor; // Editor 메뉴와 Prefab 수정 사용
using UnityEditor.SceneManagement; // Game Scene 열기와 저장 사용
using UnityEngine; // GameObject와 Component 사용
using UnityEngine.SceneManagement; // Scene 탐색 사용

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay136BotSetup
    {
        private const string NetworkPlayerSourcePath =
            "Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkPlayer.cs"; // Network Player Source 경로

        private const string ExternalGameplaySourcePath =
            "Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkExternalGameplay.cs"; // 경기 Source 경로

        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkPlayer.prefab"; // 기존 Player Prefab 경로

        private const string BotPrefabPath =
            "Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkBot.prefab"; // Day136 Bot Prefab 경로

        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity"; // Game Scene 경로

        private const string Day136RootName =
            "=== DAY136 AI BOT ==="; // Day136 Scene Root 이름

        private const string RouteRootName =
            "=== DAY136 BOT ROUTE ==="; // Bot Route Root 이름

        private static readonly UTF8Encoding Utf8WithoutBom =
            new UTF8Encoding(
                false
            ); // 기존 Source BOM 없는 UTF8 저장

        private readonly struct RouteAnchorDefinition
        {
            public RouteAnchorDefinition(
                string nodeName,
                int routeOrder,
                string primaryAnchorName,
                string fallbackAnchorName
            )
            {
                NodeName =
                    nodeName; // 생성 Route Node 이름 저장

                RouteOrder =
                    routeOrder; // Route 순서 저장

                PrimaryAnchorName =
                    primaryAnchorName; // 우선 Anchor 이름 저장

                FallbackAnchorName =
                    fallbackAnchorName; // 대체 Anchor 이름 저장
            }

            public string NodeName
            {
                get;
            } // Route Node 이름 조회

            public int RouteOrder
            {
                get;
            } // Route 순서 조회

            public string PrimaryAnchorName
            {
                get;
            } // 우선 Anchor 이름 조회

            public string FallbackAnchorName
            {
                get;
            } // 대체 Anchor 이름 조회
        }

        private static readonly RouteAnchorDefinition[] DefaultRouteAnchors =
        {
            new RouteAnchorDefinition(
                "BotRoute_000_Start",
                0,
                "Spawn_07",
                "Spawn_00"
            ),
            new RouteAnchorDefinition(
                "BotRoute_100_CP1",
                100,
                "CP1_Trigger",
                "CP1_Respawn"
            ),
            new RouteAnchorDefinition(
                "BotRoute_200_CP2",
                200,
                "CP2_Trigger",
                "CP2_Respawn"
            ),
            new RouteAnchorDefinition(
                "BotRoute_300_CP3",
                300,
                "CP3_Trigger",
                "CP3_Respawn"
            ),
            new RouteAnchorDefinition(
                "BotRoute_400_CP4",
                400,
                "CP4_Trigger",
                "CP4_Respawn"
            ),
            new RouteAnchorDefinition(
                "BotRoute_500_Finish",
                500,
                "Finish_Trigger",
                "=== FINISH ==="
            )
        }; // Game Scene 기본 Route Anchor 정의

        [MenuItem(
            "Project J/Day136/Apply Bot Foundation"
        )]
        private static void ApplyBotFoundation()
        {
            if (
                !PatchNetworkPlayerSource() ||
                !PatchExternalGameplaySource()
            )
            {
                Debug.LogError(
                    "[Project J/Day136] 최신 main Source 패턴과 일치하지 않아 자동 Source 수정이 중단되었습니다."
                ); // Source 패턴 불일치 오류 출력

                return;
            }

            if (!CreateOrUpdateBotPrefab())
            {
                return; // Bot Prefab 생성 실패 시 중단
            }

            if (!SetupGameScene())
            {
                return; // Game Scene 구성 실패 시 중단
            }

            AssetDatabase.SaveAssets(); // 생성·수정 Asset 저장
            AssetDatabase.Refresh(); // Source 재컴파일 요청

            Debug.Log(
                "[Project J/Day136] Bot Foundation 적용 완료. " +
                "컴파일 후 Game Scene의 === DAY136 BOT ROUTE === Node를 실제 코스에 맞게 조정하십시오."
            ); // Day136 적용 결과 출력
        }

        private static bool PatchNetworkPlayerSource()
        {
            if (!File.Exists(NetworkPlayerSourcePath))
            {
                Debug.LogError(
                    "[Project J/Day136] ProjectJNetworkPlayer.cs를 찾지 못했습니다. / " +
                    NetworkPlayerSourcePath
                ); // Player Source 누락 오류 출력

                return false;
            }

            string source =
                File.ReadAllText(
                    NetworkPlayerSourcePath
                ); // Player Source 전체 읽기

            string newline =
                source.Contains(
                    "\r\n"
                )
                    ? "\r\n"
                    : "\n"; // 기존 줄바꿈 형식 유지

            bool changed =
                false; // Player Source 변경 여부 초기화

            string inventoryField =
                "        private ProjectJNetworkItemInventory itemInventory; // 이동 아이템 효과 상태 조회"; // 기존 Inventory Field 패턴

            string botField =
                "        private ProjectJNetworkBotController botController; // State Authority AI Bot 입력 공급자"; // 신규 Bot Field 내용

            if (!source.Contains(botField))
            {
                if (!source.Contains(inventoryField))
                {
                    return false; // 최신 Player Field 패턴 불일치
                }

                source =
                    source.Replace(
                        inventoryField,
                        inventoryField +
                        newline +
                        botField
                    ); // Bot Controller Cache Field 삽입

                changed =
                    true; // Player Field 변경 기록
            }

            string oldInputBlock =
                "            bool hasInput =" + newline +
                "                GetInput<ProjectJNetworkInput>(" + newline +
                "                    out input" + newline +
                "                );"; // 기존 Fusion 입력 조회 블록

            string newInputBlock =
                "            bool hasInput =" + newline +
                "                botController != null &&" + newline +
                "                botController.TryBuildInput(" + newline +
                "                    this," + newline +
                "                    out input" + newline +
                "                ); // AI Bot State Authority 입력 생성" + newline +
                newline +
                "            if (!hasInput)" + newline +
                "            {" + newline +
                "                hasInput =" + newline +
                "                    GetInput<ProjectJNetworkInput>(" + newline +
                "                        out input" + newline +
                "                    ); // 실제 Player Fusion 입력 조회" + newline +
                "            }"; // Bot 우선 입력 후 Player 입력 Fallback 블록

            if (
                !source.Contains(
                    "botController.TryBuildInput("
                )
            )
            {
                if (!source.Contains(oldInputBlock))
                {
                    return false; // 최신 Player 입력 패턴 불일치
                }

                source =
                    source.Replace(
                        oldInputBlock,
                        newInputBlock
                    ); // Bot 합성 입력 Fallback 구조 적용

                changed =
                    true; // Player 입력 변경 기록
            }

            string inventoryCache =
                "            itemInventory = GetComponent<ProjectJNetworkItemInventory>(); // 이동 아이템 효과 컴포넌트 조회"; // 기존 Inventory Cache 패턴

            string botCache =
                "            botController = GetComponent<ProjectJNetworkBotController>(); // Bot Prefab Controller 조회"; // 신규 Bot Cache 내용

            if (!source.Contains(botCache))
            {
                if (!source.Contains(inventoryCache))
                {
                    return false; // 최신 Player Cache 패턴 불일치
                }

                source =
                    source.Replace(
                        inventoryCache,
                        inventoryCache +
                        newline +
                        botCache
                    ); // Bot Controller Cache 추가

                changed =
                    true; // Player Cache 변경 기록
            }

            if (changed)
            {
                File.WriteAllText(
                    NetworkPlayerSourcePath,
                    source,
                    Utf8WithoutBom
                ); // Player Source 수정 저장
            }

            return true; // Player Source Patch 성공
        }

        private static bool PatchExternalGameplaySource()
        {
            if (!File.Exists(ExternalGameplaySourcePath))
            {
                Debug.LogError(
                    "[Project J/Day136] ProjectJNetworkExternalGameplay.cs를 찾지 못했습니다. / " +
                    ExternalGameplaySourcePath
                ); // External Gameplay Source 누락 오류 출력

                return false;
            }

            string source =
                File.ReadAllText(
                    ExternalGameplaySourcePath
                ); // External Gameplay Source 읽기

            if (
                source.Contains(
                    "candidate.GetComponent<ProjectJNetworkBotMarker>() != null"
                )
            )
            {
                return true; // 이미 Bot Coordinator 제외 Patch 적용됨
            }

            string newline =
                source.Contains(
                    "\r\n"
                )
                    ? "\r\n"
                    : "\n"; // 기존 줄바꿈 형식 유지

            string oldCoordinatorBlock =
                "                if (!IsValidPlayer(candidate))" + newline +
                "                {" + newline +
                "                    continue;" + newline +
                "                }" + newline +
                newline +
                "                if (" + newline +
                "                    coordinator == null ||"; // 기존 Match Coordinator 후보 조건

            string newCoordinatorBlock =
                "                if (" + newline +
                "                    !IsValidPlayer(candidate) ||" + newline +
                "                    candidate.GetComponent<ProjectJNetworkBotMarker>() != null" + newline +
                "                )" + newline +
                "                {" + newline +
                "                    continue; // InputAuthority 없는 Bot은 Match Coordinator 후보에서 제외" + newline +
                "                }" + newline +
                newline +
                "                if (" + newline +
                "                    coordinator == null ||"; // Bot 제외 Match Coordinator 후보 조건

            if (!source.Contains(oldCoordinatorBlock))
            {
                return false; // 최신 Coordinator 패턴 불일치
            }

            source =
                source.Replace(
                    oldCoordinatorBlock,
                    newCoordinatorBlock
                ); // Bot Match Coordinator 제외 적용

            File.WriteAllText(
                ExternalGameplaySourcePath,
                source,
                Utf8WithoutBom
            ); // External Gameplay Source 수정 저장

            return true; // External Gameplay Source Patch 성공
        }

        private static bool CreateOrUpdateBotPrefab()
        {
            GameObject playerPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    PlayerPrefabPath
                ); // 기존 Network Player Prefab 로드

            if (playerPrefab == null)
            {
                Debug.LogError(
                    "[Project J/Day136] ProjectJNetworkPlayer.prefab을 찾지 못했습니다. / " +
                    PlayerPrefabPath
                ); // Player Prefab 누락 오류 출력

                return false;
            }

            if (
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    BotPrefabPath
                ) == null
            )
            {
                if (
                    !AssetDatabase.CopyAsset(
                        PlayerPrefabPath,
                        BotPrefabPath
                    )
                )
                {
                    Debug.LogError(
                        "[Project J/Day136] Bot Prefab 복제에 실패했습니다. / " +
                        BotPrefabPath
                    ); // Bot Prefab 복제 실패 출력

                    return false;
                }

                AssetDatabase.ImportAsset(
                    BotPrefabPath,
                    ImportAssetOptions.ForceUpdate
                ); // 복제 Bot Prefab 즉시 Import
            }

            GameObject prefabRoot =
                PrefabUtility.LoadPrefabContents(
                    BotPrefabPath
                ); // Bot Prefab 편집 Root 로드

            if (prefabRoot == null)
            {
                Debug.LogError(
                    "[Project J/Day136] Bot Prefab Contents를 열지 못했습니다."
                ); // Bot Prefab 편집 실패 출력

                return false;
            }

            try
            {
                prefabRoot.name =
                    "ProjectJNetworkBot"; // Bot Prefab Root 이름 변경

                if (
                    prefabRoot.GetComponent<ProjectJNetworkBotMarker>() ==
                    null
                )
                {
                    prefabRoot.AddComponent<ProjectJNetworkBotMarker>(); // Bot 식별 Marker 추가
                }

                if (
                    prefabRoot.GetComponent<ProjectJNetworkBotController>() ==
                    null
                )
                {
                    prefabRoot.AddComponent<ProjectJNetworkBotController>(); // Bot 합성 입력 Controller 추가
                }

                PrefabUtility.SaveAsPrefabAsset(
                    prefabRoot,
                    BotPrefabPath
                ); // Bot Prefab 변경 저장
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(
                    prefabRoot
                ); // Prefab 편집 Root 해제
            }

            GameObject botPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    BotPrefabPath
                ); // 저장된 Bot Prefab 다시 로드

            if (botPrefab == null)
            {
                return false; // 저장 후 Bot Prefab 누락 처리
            }

            string[] currentLabels =
                AssetDatabase.GetLabels(
                    botPrefab
                ); // 기존 Asset Label 조회

            List<string> labels =
                new List<string>(
                    currentLabels
                ); // 수정 가능한 Label 목록 생성

            if (
                !labels.Contains(
                    "FusionPrefab"
                )
            )
            {
                labels.Add(
                    "FusionPrefab"
                ); // Fusion Prefab Label 추가
            }

            AssetDatabase.SetLabels(
                botPrefab,
                labels.ToArray()
            ); // Bot Prefab Label 저장

            EditorUtility.SetDirty(
                botPrefab
            ); // Bot Prefab 변경 상태 표시

            return true; // Bot Prefab 생성 성공
        }

        private static bool SetupGameScene()
        {
            Scene scene =
                SceneManager.GetSceneByPath(
                    GameScenePath
                ); // 현재 열린 Game Scene 조회

            bool openedBySetup =
                !scene.IsValid() ||
                !scene.isLoaded; // Setup이 Scene을 열어야 하는지 확인

            if (
                !openedBySetup &&
                scene.isDirty &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                return false; // 기존 Scene 저장 취소 시 중단
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
                GameObject day136Root =
                    FindSceneObjectByName(
                        scene,
                        Day136RootName
                    ); // Day136 Root 검색

                if (day136Root == null)
                {
                    day136Root =
                        new GameObject(
                            Day136RootName
                        ); // Day136 Root 생성

                    SceneManager.MoveGameObjectToScene(
                        day136Root,
                        scene
                    ); // Day136 Root를 Game Scene에 배치
                }

                ProjectJDay136BotTestSpawner spawner =
                    day136Root.GetComponent<ProjectJDay136BotTestSpawner>(); // Bot Test Spawner 조회

                if (spawner == null)
                {
                    spawner =
                        day136Root.AddComponent<ProjectJDay136BotTestSpawner>(); // Bot Test Spawner 추가
                }

                spawner.Configure(
                    "Spawn_07"
                ); // 개발 Bot Spawn Point 설정

                GameObject routeRoot =
                    FindSceneObjectByName(
                        scene,
                        RouteRootName
                    ); // Route Root 검색

                if (routeRoot == null)
                {
                    routeRoot =
                        new GameObject(
                            RouteRootName
                        ); // Route Root 생성

                    SceneManager.MoveGameObjectToScene(
                        routeRoot,
                        scene
                    ); // Route Root를 Game Scene에 배치

                    routeRoot.transform.SetParent(
                        day136Root.transform,
                        true
                    ); // Day136 Root 하위로 정리
                }

                for (
                    int index = 0;
                    index < DefaultRouteAnchors.Length;
                    index++
                )
                {
                    EnsureRouteNode(
                        scene,
                        routeRoot.transform,
                        DefaultRouteAnchors[index]
                    ); // 기본 Spawn·CP·Finish Route Node 생성
                }

                EditorSceneManager.MarkSceneDirty(
                    scene
                ); // Game Scene 변경 표시

                if (
                    !EditorSceneManager.SaveScene(
                        scene
                    )
                )
                {
                    Debug.LogError(
                        "[Project J/Day136] Game Scene 저장에 실패했습니다."
                    ); // Game Scene 저장 실패 출력

                    return false;
                }

                return true; // Game Scene Setup 성공
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
        }

        private static void EnsureRouteNode(
            Scene scene,
            Transform routeRoot,
            RouteAnchorDefinition definition
        )
        {
            GameObject existingNode =
                FindSceneObjectByName(
                    scene,
                    definition.NodeName
                ); // 기존 자동 Route Node 검색

            if (existingNode != null)
            {
                return; // 사용자가 조정한 기존 Node 보존
            }

            GameObject anchor =
                FindSceneObjectByName(
                    scene,
                    definition.PrimaryAnchorName
                ); // 우선 Anchor 검색

            if (
                anchor == null &&
                !string.IsNullOrWhiteSpace(
                    definition.FallbackAnchorName
                )
            )
            {
                anchor =
                    FindSceneObjectByName(
                        scene,
                        definition.FallbackAnchorName
                    ); // 대체 Anchor 검색
            }

            if (anchor == null)
            {
                Debug.LogWarning(
                    "[Project J/Day136] Route Anchor를 찾지 못해 Node 생성을 건너뜁니다. / " +
                    definition.NodeName
                ); // Route Anchor 누락 경고 출력

                return;
            }

            GameObject nodeObject =
                new GameObject(
                    definition.NodeName
                ); // Route Node GameObject 생성

            SceneManager.MoveGameObjectToScene(
                nodeObject,
                scene
            ); // Route Node를 Game Scene에 배치

            nodeObject.transform.SetParent(
                routeRoot,
                true
            ); // Route Root 하위로 정리

            nodeObject.transform.SetPositionAndRotation(
                anchor.transform.position,
                anchor.transform.rotation
            ); // Anchor 위치·회전 복사

            ProjectJBotRouteNode routeNode =
                nodeObject.AddComponent<ProjectJBotRouteNode>(); // Bot Route Node Component 추가

            routeNode.Configure(
                definition.RouteOrder,
                false
            ); // 기본 Route 순서와 일반 이동 설정
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
