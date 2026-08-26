using System.Collections.Generic; // Route Node 목록 사용
using ProjectJ.AI; // Bot Route Node 사용
using UnityEditor; // Editor 메뉴와 Dirty 처리 사용
using UnityEditor.SceneManagement; // Game Scene 열기와 저장 사용
using UnityEngine; // GameObject와 Vector 타입 사용
using UnityEngine.SceneManagement; // Scene 탐색 사용

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay137BotRouteSetup
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity"; // Game Scene 경로

        private const string RouteRootName =
            "=== DAY136 BOT ROUTE ==="; // 기존 Bot Route Root 이름

        private const string AutoNodeSuffix =
            "_Auto"; // 자동 생성 Route Node 접미사

        [MenuItem(
            "Project J/Day137/Apply Bot Route Upgrade"
        )]
        private static void ApplyBotRouteUpgrade()
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
                return; // 현재 Scene 저장 취소 시 중단
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
                GameObject routeRoot =
                    FindSceneObjectByName(
                        scene,
                        RouteRootName
                    ); // 기존 Route Root 검색

                if (routeRoot == null)
                {
                    Debug.LogError(
                        "[Project J/Day137] === DAY136 BOT ROUTE === 를 찾지 못했습니다. " +
                        "Day136 Bot Foundation을 먼저 적용하십시오."
                    ); // Day136 Route Root 누락 오류 출력

                    return;
                }

                List<ProjectJBotRouteNode> anchorNodes =
                    CollectCheckpointAnchorNodes(
                        routeRoot.transform
                    ); // 0·100·200·300·400·500 기준 Node 수집

                if (anchorNodes.Count < 2)
                {
                    Debug.LogError(
                        "[Project J/Day137] Route Anchor가 2개 미만이라 자동 세분화를 진행할 수 없습니다."
                    ); // Route Anchor 부족 오류 출력

                    return;
                }

                int createdCount =
                    0; // 신규 자동 Route Node 개수 초기화

                for (
                    int index = 0;
                    index < anchorNodes.Count - 1;
                    index++
                )
                {
                    ProjectJBotRouteNode startNode =
                        anchorNodes[index]; // 현재 구간 시작 Anchor 조회

                    ProjectJBotRouteNode endNode =
                        anchorNodes[index + 1]; // 현재 구간 종료 Anchor 조회

                    createdCount +=
                        CreateIntermediateNodes(
                            scene,
                            routeRoot.transform,
                            startNode,
                            endNode
                        ); // 현재 Checkpoint 구간 3등분 보조 Node 생성
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
                        "[Project J/Day137] Game Scene 저장에 실패했습니다."
                    ); // Game Scene 저장 실패 출력

                    return;
                }

                Debug.Log(
                    "[Project J/Day137] Bot Route 세분화 적용 완료 / 신규 Node: " +
                    createdCount +
                    " / 자동 Node를 실제 장애물과 점프 발판 위치에 맞게 이동하십시오."
                ); // 자동 Route 생성 결과 출력
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

        [MenuItem(
            "Project J/Day137/Remove Auto Route Nodes"
        )]
        private static void RemoveAutoRouteNodes()
        {
            Scene scene =
                SceneManager.GetSceneByPath(
                    GameScenePath
                ); // 현재 열린 Game Scene 조회

            bool openedBySetup =
                !scene.IsValid() ||
                !scene.isLoaded; // 제거 작업 Scene Open 여부 확인

            if (
                !openedBySetup &&
                scene.isDirty &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                return; // 현재 Scene 저장 취소 시 중단
            }

            if (openedBySetup)
            {
                scene =
                    EditorSceneManager.OpenScene(
                        GameScenePath,
                        OpenSceneMode.Additive
                    ); // Game Scene 추가 열기
            }

            try
            {
                GameObject routeRoot =
                    FindSceneObjectByName(
                        scene,
                        RouteRootName
                    ); // 기존 Route Root 검색

                if (routeRoot == null)
                {
                    return; // Route Root 없음 처리
                }

                List<GameObject> removeTargets =
                    new List<GameObject>(); // 자동 Node 제거 대상 목록

                for (
                    int index = 0;
                    index < routeRoot.transform.childCount;
                    index++
                )
                {
                    Transform child =
                        routeRoot.transform.GetChild(
                            index
                        ); // Route Root 직계 자식 조회

                    if (
                        child.name.EndsWith(
                            AutoNodeSuffix,
                            System.StringComparison.Ordinal
                        )
                    )
                    {
                        removeTargets.Add(
                            child.gameObject
                        ); // 자동 Node 제거 대상으로 추가
                    }
                }

                for (
                    int index = 0;
                    index < removeTargets.Count;
                    index++
                )
                {
                    Object.DestroyImmediate(
                        removeTargets[index]
                    ); // 자동 생성 Route Node 제거
                }

                EditorSceneManager.MarkSceneDirty(
                    scene
                ); // Game Scene 변경 표시

                EditorSceneManager.SaveScene(
                    scene
                ); // 제거 결과 저장

                Debug.Log(
                    "[Project J/Day137] 자동 Route Node 제거 완료 / 제거: " +
                    removeTargets.Count
                ); // 자동 Route 제거 결과 출력
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

        private static List<ProjectJBotRouteNode> CollectCheckpointAnchorNodes(
            Transform routeRoot
        )
        {
            List<ProjectJBotRouteNode> anchors =
                new List<ProjectJBotRouteNode>(); // Checkpoint 기준 Route 목록 생성

            HashSet<int> collectedOrders =
                new HashSet<int>(); // 동일 Checkpoint Route Order 중복 방지

            ProjectJBotRouteNode[] nodes =
                routeRoot.GetComponentsInChildren<ProjectJBotRouteNode>(
                    true
                ); // Route Root 하위 Node 수집

            for (
                int index = 0;
                index < nodes.Length;
                index++
            )
            {
                ProjectJBotRouteNode node =
                    nodes[index]; // 현재 Route Node 조회

                if (
                    node == null ||
                    node.RouteOrder < 0 ||
                    node.RouteOrder > 500 ||
                    node.RouteOrder % 100 != 0
                )
                {
                    continue; // 기본 Checkpoint Anchor 외 Node 제외
                }

                if (
                    !collectedOrders.Add(
                        node.RouteOrder
                    )
                )
                {
                    continue; // 동일 Route Order 중복 Anchor 제외
                }

                anchors.Add(
                    node
                ); // Checkpoint Anchor 추가
            }

            anchors.Sort(
                CompareRouteNodes
            ); // Route Order 기준 정렬

            return anchors; // 정렬된 Checkpoint Anchor 반환
        }

        private static int CreateIntermediateNodes(
            Scene scene,
            Transform routeRoot,
            ProjectJBotRouteNode startNode,
            ProjectJBotRouteNode endNode
        )
        {
            if (
                startNode == null ||
                endNode == null
            )
            {
                return 0; // 잘못된 구간 처리
            }

            int orderDifference =
                endNode.RouteOrder -
                startNode.RouteOrder; // 구간 Route Order 차이 계산

            if (orderDifference < 4)
            {
                return 0; // 세분화 가능한 Order 간격 부족
            }

            int createdCount =
                0; // 현재 구간 생성 개수 초기화

            for (
                int step = 1;
                step <= 3;
                step++
            )
            {
                float normalizedStep =
                    step /
                    4f; // 25%·50%·75% 보간 비율 계산

                int routeOrder =
                    Mathf.RoundToInt(
                        Mathf.Lerp(
                            startNode.RouteOrder,
                            endNode.RouteOrder,
                            normalizedStep
                        )
                    ); // 보조 Node Route Order 계산

                string nodeName =
                    "BotRoute_" +
                    routeOrder.ToString(
                        "000"
                    ) +
                    AutoNodeSuffix; // 자동 Route Node 이름 생성

                if (
                    FindSceneObjectByName(
                        scene,
                        nodeName
                    ) != null
                )
                {
                    continue; // 기존 자동 Node 보존
                }

                GameObject nodeObject =
                    new GameObject(
                        nodeName
                    ); // 자동 Route Node 생성

                SceneManager.MoveGameObjectToScene(
                    nodeObject,
                    scene
                ); // Game Scene에 Route Node 배치

                nodeObject.transform.SetParent(
                    routeRoot,
                    true
                ); // 기존 Route Root 하위로 정리

                nodeObject.transform.position =
                    Vector3.Lerp(
                        startNode.transform.position,
                        endNode.transform.position,
                        normalizedStep
                    ); // 두 Anchor 사이 초기 위치 보간

                nodeObject.transform.rotation =
                    Quaternion.Slerp(
                        startNode.transform.rotation,
                        endNode.transform.rotation,
                        normalizedStep
                    ); // 두 Anchor 사이 초기 회전 보간

                ProjectJBotRouteNode routeNode =
                    nodeObject.AddComponent<ProjectJBotRouteNode>(); // Route Node Component 추가

                routeNode.Configure(
                    routeOrder,
                    false
                ); // 일반 이동 보조 Node로 설정

                createdCount++; // 생성 개수 증가
            }

            return createdCount; // 현재 구간 생성 개수 반환
        }

        private static int CompareRouteNodes(
            ProjectJBotRouteNode left,
            ProjectJBotRouteNode right
        )
        {
            if (left == null)
            {
                return right == null
                    ? 0
                    : 1; // null Node 뒤로 정렬
            }

            if (right == null)
            {
                return -1; // 유효 Node 앞으로 정렬
            }

            return
                left.RouteOrder.CompareTo(
                    right.RouteOrder
                ); // Route Order 오름차순 정렬
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
