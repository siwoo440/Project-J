using System; // 문자열 정렬 기능 사용
using ProjectJ.Items; // 기존 ItemPickup 검증 사용
using ProjectJ.Networking.Fusion; // 네트워크 아이템 상자 타입 사용
using UnityEditor; // Unity Editor 기능 사용
using UnityEditor.SceneManagement; // Scene 열기와 저장 기능 사용
using UnityEngine; // Unity 기본 오브젝트 기능 사용
using UnityEngine.SceneManagement; // Scene 탐색 기능 사용

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay134PickupTestAreaSetup
    {
        private const string GameScenePath = "Assets/ProjectJ/Scenes/Game.unity"; // 수정 대상 게임 Scene 경로
        private const string StartPlazaName = "Start_Plaza"; // 출발 광장 이름
        private const string TestMapRootName = "=== ITEM PICKUP TEST MAP ==="; // 테스트 맵 루트 이름
        private const string PickupRootName = "=== ITEM PICKUPS ==="; // Pickup 루트 이름
        private const string RouteLeftName = "RouteNode_001_L"; // 첫 코스 왼쪽 기준점 이름
        private const string RouteRightName = "RouteNode_001_R"; // 첫 코스 오른쪽 기준점 이름
        private const string PickupBoxVisualName = "Day134_PickupBoxVisual"; // 테스트 상자 시각 오브젝트 이름
        private const int PickupColumns = 5; // Pickup 열 수
        private const int PickupRows = 6; // Pickup 행 수
        private const float FirstRowLocalX = 28f; // 테스트장 첫 행 로컬 X 위치
        private const float PickupRowSpacing = 6f; // 테스트장 행 간격
        private const float PickupColumnSpacing = 6f; // 테스트장 열 간격
        private const float RespawnSeconds = 5f; // 테스트용 재생성 시간

        [MenuItem("Project J/Day134/Setup Pickup Respawn Test Area")] // 134일차 테스트장 자동 설정 메뉴
        private static void SetupPickupRespawnTestArea()
        {
            Scene scene = OpenGameSceneSafely(); // Game Scene 안전하게 열기

            if (!scene.IsValid())
            {
                return; // Scene 열기 취소 시 설정 중단
            }

            Transform startPlaza = FindTransformByName(scene, StartPlazaName); // 출발 광장 검색
            Transform testMapRoot = FindTransformByName(scene, TestMapRootName); // 테스트 맵 루트 검색
            Transform pickupRoot = FindTransformByName(scene, PickupRootName); // Pickup 루트 검색

            if (startPlaza == null || testMapRoot == null || pickupRoot == null)
            {
                Debug.LogError("[Day134] Start_Plaza 또는 Pickup 테스트 루트를 찾지 못했습니다."); // 필수 오브젝트 누락 로그
                return; // 잘못된 Scene 수정 방지
            }

            ProjectJNetworkItemBox[] pickupBoxes =
                pickupRoot.GetComponentsInChildren<ProjectJNetworkItemBox>(true); // 30개 네트워크 상자 수집

            if (!ValidatePickupBoxes(pickupBoxes))
            {
                return; // 잘못된 Pickup 구성 수정 방지
            }

            Array.Sort(pickupBoxes, ComparePickupBoxesByName); // Pickup 이름 번호 순서 정렬

            Vector3 courseForward = ResolveCourseForward(scene, startPlaza); // 실제 경기 코스 진행 방향 계산
            Vector3 testDirection = -courseForward; // 출발 지점 뒤쪽 방향 계산
            Quaternion testRotation = Quaternion.FromToRotation(Vector3.right, testDirection); // 기존 오른쪽 테스트장을 뒤쪽으로 회전
            Vector3 rootPosition = testMapRoot.position; // 테스트 루트 기존 높이 보관
            rootPosition.x = startPlaza.position.x; // 테스트 루트 X를 출발점에 정렬
            rootPosition.z = startPlaza.position.z; // 테스트 루트 Z를 출발점에 정렬
            testMapRoot.position = rootPosition; // 테스트 루트 중심 이동
            testMapRoot.rotation = testRotation; // 테스트 맵 전체 방향 변경

            for (int index = 0; index < pickupBoxes.Length; index++)
            {
                int row = index / PickupColumns; // 6개 행 번호 계산
                int column = index % PickupColumns; // 5개 열 번호 계산
                float localX = FirstRowLocalX + (row * PickupRowSpacing); // 출발점에서 멀어지는 행 위치 계산
                float localZ = (column - ((PickupColumns - 1) * 0.5f)) * PickupColumnSpacing; // 좌우 열 위치 계산
                Vector3 localPosition =
                    testMapRoot.InverseTransformPoint(pickupBoxes[index].transform.position); // 기존 Pickup 높이 보관
                localPosition.x = localX; // 6행 진행 위치 적용
                localPosition.z = localZ; // 5열 좌우 위치 적용
                pickupBoxes[index].transform.position =
                    testMapRoot.TransformPoint(localPosition); // 5열 6행 세계 위치 적용
                ConfigurePickupBox(pickupBoxes[index]); // 상자 시각과 5초 재생성 설정
            }

            EditorSceneManager.MarkSceneDirty(scene); // Scene 변경 상태 표시
            EditorSceneManager.SaveScene(scene); // 변경된 Game Scene 저장
            Selection.activeGameObject = testMapRoot.gameObject; // Hierarchy에서 테스트 맵 선택
            Debug.Log("[Day134] Pickup 테스트장을 출발 지점 뒤쪽으로 재배치하고 30개 상자의 5초 재생성을 설정했습니다."); // 설정 완료 로그
        }

        private static Scene OpenGameSceneSafely()
        {
            Scene activeScene = SceneManager.GetActiveScene(); // 현재 활성 Scene 확인

            if (activeScene.IsValid() && activeScene.path == GameScenePath)
            {
                return activeScene; // 이미 Game Scene이면 재사용
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return default; // 다른 Scene 저장 취소 시 종료
            }

            return EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single); // Game Scene 단독 열기
        }

        private static bool ValidatePickupBoxes(ProjectJNetworkItemBox[] pickupBoxes)
        {
            if (pickupBoxes == null || pickupBoxes.Length != PickupColumns * PickupRows)
            {
                int count = pickupBoxes == null ? 0 : pickupBoxes.Length; // 실제 Pickup 개수 계산
                Debug.LogError($"[Day134] Network Item Box가 30개여야 합니다. 현재 개수: {count}"); // 개수 오류 출력
                return false; // 구성 검증 실패
            }

            for (int index = 0; index < pickupBoxes.Length; index++)
            {
                ProjectJNetworkItemBox pickupBox = pickupBoxes[index]; // 현재 Pickup 상자 참조

                if (pickupBox == null)
                {
                    Debug.LogError($"[Day134] {index + 1}번째 Network Item Box 참조가 없습니다."); // 상자 참조 누락 로그
                    return false; // 구성 검증 실패
                }

                ItemPickup pickup = pickupBox.GetComponent<ItemPickup>(); // 실제 133일차 ItemPickup 검색

                if (pickup == null || pickup.Definition == null)
                {
                    Debug.LogError($"[Day134] {pickupBox.name}의 ItemPickup 또는 Definition 참조가 없습니다."); // ItemDefinition 누락 로그
                    return false; // 구성 검증 실패
                }
            }

            return true; // 30개 Pickup 검증 성공
        }

        private static int ComparePickupBoxesByName(
            ProjectJNetworkItemBox left,
            ProjectJNetworkItemBox right
        )
        {
            return string.CompareOrdinal(left.name, right.name); // Pickup_01 형식 이름 기준 오름차순 정렬
        }

        private static Vector3 ResolveCourseForward(Scene scene, Transform startPlaza)
        {
            Transform routeLeft = FindTransformByName(scene, RouteLeftName); // 첫 코스 왼쪽 기준점 검색
            Transform routeRight = FindTransformByName(scene, RouteRightName); // 첫 코스 오른쪽 기준점 검색
            Vector3 targetPosition = startPlaza.position + startPlaza.forward; // 기본 진행 방향 기준점 생성

            if (routeLeft != null && routeRight != null)
            {
                targetPosition = (routeLeft.position + routeRight.position) * 0.5f; // 첫 코스 중앙점 계산
            }
            else if (routeLeft != null)
            {
                targetPosition = routeLeft.position; // 왼쪽 기준점만 존재할 때 사용
            }
            else if (routeRight != null)
            {
                targetPosition = routeRight.position; // 오른쪽 기준점만 존재할 때 사용
            }

            Vector3 direction = targetPosition - startPlaza.position; // 출발점에서 코스까지 방향 계산
            direction.y = 0f; // 수평 진행 방향만 유지

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = startPlaza.forward; // Route Node 방향 계산 실패 시 Start 전방 사용
                direction.y = 0f; // 수직 성분 제거
            }

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.forward; // 최종 기본 진행 방향 사용
            }

            return direction.normalized; // 정규화된 경기 진행 방향 반환
        }

        private static void ConfigurePickupBox(ProjectJNetworkItemBox pickupBox)
        {
            Transform visualTransform = pickupBox.transform.Find(PickupBoxVisualName); // 기존 테스트 상자 시각 검색
            GameObject visualObject =
                visualTransform == null ? null : visualTransform.gameObject; // 기존 시각 오브젝트 참조 변환

            if (visualObject == null)
            {
                visualObject = GameObject.CreatePrimitive(PrimitiveType.Cube); // 기본 Cube 상자 생성
                visualObject.name = PickupBoxVisualName; // 테스트 상자 이름 지정
                visualObject.transform.SetParent(pickupBox.transform, false); // Pickup 하위 오브젝트로 연결
                visualTransform = visualObject.transform; // 생성된 Transform 참조 저장
            }

            Collider visualCollider = visualObject.GetComponent<Collider>(); // 시각 Cube의 불필요 Collider 검색

            if (visualCollider != null)
            {
                UnityEngine.Object.DestroyImmediate(visualCollider); // 기존 Pickup Trigger 간섭 방지용 Collider 제거
            }

            visualTransform.localPosition = new Vector3(0f, -0.55f, 0f); // 아이템 아래 상자 위치 설정
            visualTransform.localRotation = Quaternion.identity; // 상자 로컬 회전 초기화
            visualTransform.localScale = new Vector3(1.8f, 1f, 1.8f); // 테스트 상자 크기 설정

            SerializedObject serializedBox = new SerializedObject(pickupBox); // private 직렬화 필드 수정 객체 생성
            SerializedProperty respawnProperty =
                serializedBox.FindProperty("respawnSeconds"); // 재생성 시간 필드 검색

            if (respawnProperty == null)
            {
                Debug.LogError($"[Day134] {pickupBox.name}에서 respawnSeconds 필드를 찾지 못했습니다."); // 런타임 스크립트 버전 불일치 로그
                return; // 잘못된 직렬화 변경 방지
            }

            respawnProperty.floatValue = RespawnSeconds; // 5초 재생성 시간 적용
            serializedBox.ApplyModifiedPropertiesWithoutUndo(); // Inspector 직렬화 변경 적용
            EditorUtility.SetDirty(pickupBox); // Network Item Box 변경 상태 표시
            EditorUtility.SetDirty(visualObject); // 새 상자 시각 변경 상태 표시
        }

        private static Transform FindTransformByName(Scene scene, string targetName)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects(); // Scene 루트 오브젝트 수집

            for (int index = 0; index < rootObjects.Length; index++)
            {
                Transform found = FindTransformRecursive(
                    rootObjects[index].transform,
                    targetName
                ); // 하위 계층 재귀 검색

                if (found != null)
                {
                    return found; // 발견된 Transform 반환
                }
            }

            return null; // 대상 오브젝트 미발견 반환
        }

        private static Transform FindTransformRecursive(Transform current, string targetName)
        {
            if (current.name == targetName)
            {
                return current; // 현재 오브젝트 이름 일치 시 반환
            }

            for (int index = 0; index < current.childCount; index++)
            {
                Transform found = FindTransformRecursive(
                    current.GetChild(index),
                    targetName
                ); // 자식 계층 재귀 검색

                if (found != null)
                {
                    return found; // 발견된 Transform 반환
                }
            }

            return null; // 현재 계층 미발견 반환
        }
    }
}
