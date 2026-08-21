using System; // 기본 시스템 기능 사용
using System.IO; // 파일 읽기 쓰기 사용
using ProjectJ.Interaction; // 상호작용 시스템 사용
using UnityEditor; // 유니티 에디터 기능 사용
using UnityEditor.SceneManagement; // 에디터 Scene 기능 사용
using UnityEngine; // 유니티 기능 사용
using UnityEngine.SceneManagement; // Scene 기능 사용

namespace ProjectJ.Editor // 프로젝트 에디터 네임스페이스
{
    public static class Day47CommonInteractionSetup // 47일차 공통 상호작용 설정 도구
    {
        private const string PlayerPrefabPath = "Assets/ProjectJ/Prefabs/Player/Player.prefab"; // Player Prefab 경로
        private const string Phase4ScenePath = "Assets/ProjectJ/Tests/Manual/Phase4/Phase4_InteractionTest.unity"; // Phase 4 테스트 Scene 경로
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions"; // Input Actions 파일 경로
        private const string RootName = "=== Day47 Common Interaction Test Area ==="; // 테스트 구역 루트 이름
        private const string InteractionOriginName = "InteractionOrigin"; // 상호작용 기준점 이름
        private const float InteractionRange = 3f; // 테스트 상호작용 거리
        private const float BridgeLength = 4f; // 테스트 구역 연결 다리 길이
        private const float AreaWidth = 18f; // 테스트 구역 폭
        private const float AreaDepth = 18f; // 테스트 구역 깊이

        [MenuItem("ProjectJ/Day47/Setup Common Interaction")] // 47일차 자동 설정 메뉴 등록
        public static void Setup() // 전체 설정 실행
        {
            FixInteractGamepadBinding(); // 게임패드 Interact 바인딩 수정
            SetupPlayerPrefab(); // Player Prefab 상호작용 기능 추가
            SetupPhase4Scene(); // Phase 4 테스트 구역 생성
            AssetDatabase.SaveAssets(); // 변경 에셋 저장
            AssetDatabase.Refresh(); // 에셋 데이터 갱신
            Debug.Log("Day47 공통 F 상호작용 설정 완료"); // 완료 로그 출력
        }

        private static void FixInteractGamepadBinding() // Interact 게임패드 바인딩 교정
        {
            string fullPath = Path.GetFullPath(InputActionsPath); // 실제 파일 경로 계산

            if (!File.Exists(fullPath)) // Input Actions 파일 존재 검사
            {
                Debug.LogError($"Input Actions 파일을 찾을 수 없습니다: {InputActionsPath}"); // 파일 누락 오류 출력
                return; // 바인딩 수정 중단
            }

            string[] lines = File.ReadAllLines(fullPath); // Input Actions 전체 줄 읽기
            bool changed = false; // 파일 변경 여부 초기화
            bool interactBindingFound = false; // Interact 바인딩 발견 여부 초기화

            for (int i = 0; i < lines.Length; i++) // 모든 줄 반복
            {
                if (!lines[i].Contains("\"action\": \"Interact\"", StringComparison.Ordinal)) // Interact Action 줄 검사
                {
                    continue; // 다른 Action 건너뛰기
                }

                for (int j = i - 1; j >= Mathf.Max(0, i - 10); j--) // 가까운 이전 Binding 속성 탐색
                {
                    if (!lines[j].Contains("\"path\":", StringComparison.Ordinal)) // 입력 경로 줄 검사
                    {
                        continue; // 다른 속성 건너뛰기
                    }

                    if (lines[j].Contains("<Keyboard>/f", StringComparison.Ordinal)) // 키보드 F 바인딩 검사
                    {
                        interactBindingFound = true; // Interact 바인딩 확인 기록
                        break; // 현재 Binding 탐색 종료
                    }

                    if (lines[j].Contains("<Gamepad>/dpad/down", StringComparison.Ordinal)) // 이미 수정된 게임패드 바인딩 검사
                    {
                        interactBindingFound = true; // Interact 바인딩 확인 기록
                        break; // 현재 Binding 탐색 종료
                    }

                    if (lines[j].Contains("<Gamepad>/buttonWest", StringComparison.Ordinal)) // 기존 잘못된 게임패드 바인딩 검사
                    {
                        lines[j] = lines[j].Replace("<Gamepad>/buttonWest", "<Gamepad>/dpad/down"); // 방향 패드 아래로 경로 변경
                        interactBindingFound = true; // Interact 바인딩 확인 기록
                        changed = true; // 파일 변경 기록
                        break; // 현재 Binding 탐색 종료
                    }

                    break; // 관련 없는 다른 경로에서 탐색 종료
                }
            }

            if (!interactBindingFound) // Interact 바인딩 탐색 실패 검사
            {
                Debug.LogWarning("Interact 바인딩을 자동 확인하지 못했습니다. InputSystem_Actions를 직접 확인하세요."); // 바인딩 확인 경고 출력
            }

            if (!changed) // 실제 파일 변경 필요 여부 검사
            {
                return; // 이미 올바른 상태면 종료
            }

            File.WriteAllLines(fullPath, lines, new System.Text.UTF8Encoding(false)); // 수정 Input Actions 저장
            AssetDatabase.ImportAsset(InputActionsPath, ImportAssetOptions.ForceUpdate); // Input Actions 강제 재임포트
            Debug.Log("Interact 게임패드 입력을 D-Pad Down으로 수정했습니다."); // 바인딩 수정 로그 출력
        }

        private static void SetupPlayerPrefab() // Player Prefab 설정
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath); // Player Prefab 편집 인스턴스 열기

            if (prefabRoot == null) // Prefab 로드 실패 검사
            {
                Debug.LogError($"Player Prefab을 찾을 수 없습니다: {PlayerPrefabPath}"); // Prefab 누락 오류 출력
                return; // 설정 중단
            }

            try
            {
                PlayerInteractionController controller = prefabRoot.GetComponent<PlayerInteractionController>(); // 기존 상호작용 Controller 탐색

                if (controller == null) // Controller 누락 검사
                {
                    controller = prefabRoot.AddComponent<PlayerInteractionController>(); // 공통 상호작용 Controller 추가
                }

                Transform origin = prefabRoot.transform.Find(InteractionOriginName); // 기존 상호작용 기준점 탐색

                if (origin == null) // 기준점 누락 검사
                {
                    GameObject originObject = new GameObject(InteractionOriginName); // 상호작용 기준점 생성
                    originObject.transform.SetParent(prefabRoot.transform, false); // Player 하위 배치
                    originObject.transform.localPosition = new Vector3(0f, 1f, 0f); // 플레이어 몸통 높이 배치
                    originObject.transform.localRotation = Quaternion.identity; // 기본 회전 적용
                    origin = originObject.transform; // 생성 기준점 저장
                }

                controller.Configure(origin, InteractionRange, ~0); // 상호작용 탐색 설정 적용
                EditorUtility.SetDirty(controller); // Controller 변경 표시
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath); // Player Prefab 저장
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot); // Prefab 편집 인스턴스 닫기
            }
        }

        private static void SetupPhase4Scene() // Phase 4 테스트 Scene 설정
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(Phase4ScenePath); // 테스트 Scene 에셋 로드

            if (sceneAsset == null) // Scene 존재 검사
            {
                Debug.LogError($"Phase 4 테스트 Scene을 찾을 수 없습니다: {Phase4ScenePath}"); // Scene 누락 오류 출력
                return; // 테스트 구역 생성 중단
            }

            Scene scene = EditorSceneManager.OpenScene(Phase4ScenePath, OpenSceneMode.Single); // 테스트 Scene 열기
            GameObject anchorFloor = GameObject.Find("Day45_Main_Floor"); // Day45 기준 바닥 탐색

            if (anchorFloor == null) // Day45 바닥 누락 검사
            {
                anchorFloor = GameObject.Find("Day44_Main_Floor"); // Day44 바닥 대체 탐색
            }

            if (anchorFloor == null) // 기준 바닥 최종 누락 검사
            {
                Debug.LogError("Day44 또는 Day45 기준 바닥을 찾을 수 없습니다."); // 기준 바닥 누락 오류 출력
                return; // 테스트 구역 생성 중단
            }

            RemoveObjectIfExists(RootName); // 기존 Day47 테스트 구역 제거
            BuildTestArea(scene, anchorFloor); // 새로운 테스트 구역 생성
            EditorSceneManager.MarkSceneDirty(scene); // Scene 변경 표시
            EditorSceneManager.SaveScene(scene); // Scene 저장
        }

        private static void BuildTestArea(Scene scene, GameObject anchorFloor) // 공통 상호작용 테스트 구역 생성
        {
            Collider anchorCollider = anchorFloor.GetComponent<Collider>(); // 기준 바닥 Collider 탐색

            if (anchorCollider == null) // 기준 Collider 누락 검사
            {
                Debug.LogError("Day47 기준 바닥에 Collider가 없습니다."); // Collider 누락 오류 출력
                return; // 테스트 구역 생성 중단
            }

            Bounds anchorBounds = anchorCollider.bounds; // 기준 바닥 Bounds 저장
            float floorTopY = anchorBounds.max.y; // 바닥 상단 높이 저장
            float centerX = anchorBounds.center.x; // 테스트 구역 X 중심 계산
            float southEdge = anchorBounds.max.z + BridgeLength; // 연결 다리 끝 위치 계산
            float centerZ = southEdge + AreaDepth * 0.5f; // 테스트 구역 Z 중심 계산
            GameObject root = new GameObject(RootName); // 테스트 구역 루트 생성
            SceneManager.MoveGameObjectToScene(root, scene); // 테스트 Scene으로 루트 이동

            GameObject bridge = CreateCube( // 연결 다리 생성
                "Day47_Connector_Bridge", // 연결 다리 이름
                new Vector3(centerX, floorTopY - 0.1f, anchorBounds.max.z + BridgeLength * 0.5f), // 연결 다리 위치
                new Vector3(5f, 0.2f, BridgeLength), // 연결 다리 크기
                WorldLayer() // World Layer
            );
            bridge.transform.SetParent(root.transform, true); // 연결 다리 루트 연결

            GameObject floor = CreateCube( // 메인 테스트 바닥 생성
                "Day47_Main_Floor", // 테스트 바닥 이름
                new Vector3(centerX, floorTopY - 0.1f, centerZ), // 테스트 바닥 위치
                new Vector3(AreaWidth, 0.2f, AreaDepth), // 테스트 바닥 크기
                WorldLayer() // World Layer
            );
            floor.transform.SetParent(root.transform, true); // 테스트 바닥 루트 연결

            CreateTestButton( // 가까운 정상 버튼 생성
                root.transform, // 테스트 루트
                "Day47_Button_Near", // 버튼 이름
                new Vector3(centerX - 2f, floorTopY + 0.3f, centerZ - 2f), // 버튼 위치
                true // 사용 가능 상태
            );

            CreateTestButton( // 두 번째 정상 버튼 생성
                root.transform, // 테스트 루트
                "Day47_Button_Second", // 버튼 이름
                new Vector3(centerX + 2f, floorTopY + 0.3f, centerZ - 2f), // 버튼 위치
                true // 사용 가능 상태
            );

            CreateTestButton( // 가까운 사용 불가 버튼 생성
                root.transform, // 테스트 루트
                "Day47_Button_Disabled", // 버튼 이름
                new Vector3(centerX, floorTopY + 0.3f, centerZ + 2f), // 버튼 위치
                false // 사용 불가 상태
            );

            CreateTestButton( // 범위 밖 확인 버튼 생성
                root.transform, // 테스트 루트
                "Day47_Button_Far", // 버튼 이름
                new Vector3(centerX + 7f, floorTopY + 0.3f, centerZ + 7f), // 버튼 위치
                true // 사용 가능 상태
            );

            CreateMarker( // 상호작용 테스트 시작 위치 표시
                root.transform, // 테스트 루트
                "Day47_Test_Start_Marker", // 시작 마커 이름
                new Vector3(centerX, floorTopY + 0.05f, centerZ - 5f) // 마커 위치
            );

            Selection.activeGameObject = root; // 생성 테스트 구역 선택
        }

        private static void CreateTestButton(Transform parent, string objectName, Vector3 position, bool canInteract) // 테스트 버튼 생성
        {
            GameObject button = CreateCube( // 버튼 본체 생성
                objectName, // 버튼 이름
                position, // 버튼 위치
                new Vector3(1.4f, 0.6f, 1.4f), // 버튼 크기
                WorldLayer() // World Layer
            );
            button.transform.SetParent(parent, true); // 테스트 루트 연결

            GameObject indicator = CreateCube( // 작동 표시등 생성
                objectName + "_ActiveIndicator", // 표시등 이름
                position + Vector3.up * 0.55f, // 표시등 위치
                new Vector3(0.65f, 0.2f, 0.65f), // 표시등 크기
                WorldLayer() // World Layer
            );
            indicator.transform.SetParent(button.transform, true); // 버튼 하위 연결

            Collider indicatorCollider = indicator.GetComponent<Collider>(); // 표시등 Collider 탐색

            if (indicatorCollider != null) // 표시등 Collider 존재 검사
            {
                UnityEngine.Object.DestroyImmediate(indicatorCollider); // 불필요한 표시등 Collider 제거
            }

            indicator.SetActive(false); // 초기 표시등 비활성화
            TestInteractableButton interactable = button.AddComponent<TestInteractableButton>(); // 테스트 상호작용 기능 추가
            interactable.Configure(canInteract, indicator); // 사용 가능 상태와 표시등 연결
        }

        private static void CreateMarker(Transform parent, string objectName, Vector3 position) // 테스트 시작 마커 생성
        {
            GameObject marker = CreateCube( // 마커 오브젝트 생성
                objectName, // 마커 이름
                position, // 마커 위치
                new Vector3(2f, 0.1f, 2f), // 마커 크기
                WorldLayer() // World Layer
            );
            marker.transform.SetParent(parent, true); // 테스트 루트 연결

            Collider markerCollider = marker.GetComponent<Collider>(); // 마커 Collider 탐색

            if (markerCollider != null) // 마커 Collider 존재 검사
            {
                UnityEngine.Object.DestroyImmediate(markerCollider); // 마커 충돌 제거
            }
        }

        private static GameObject CreateCube(string objectName, Vector3 position, Vector3 scale, int layer) // 기본 Cube 생성
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube); // Cube 오브젝트 생성
            cube.name = objectName; // 오브젝트 이름 적용
            cube.transform.position = position; // 월드 위치 적용
            cube.transform.rotation = Quaternion.identity; // 기본 회전 적용
            cube.transform.localScale = scale; // 오브젝트 크기 적용
            cube.layer = layer; // Layer 적용
            return cube; // 생성 Cube 반환
        }

        private static int WorldLayer() // World Layer 번호 조회
        {
            int layer = LayerMask.NameToLayer("World"); // World Layer 탐색
            return layer >= 0 ? layer : 0; // 누락 시 Default Layer 반환
        }

        private static void RemoveObjectIfExists(string objectName) // 기존 자동 생성 오브젝트 정리
        {
            GameObject existing = GameObject.Find(objectName); // 기존 오브젝트 탐색

            if (existing == null) // 기존 오브젝트 존재 검사
            {
                return; // 삭제 작업 생략
            }

            UnityEngine.Object.DestroyImmediate(existing); // 기존 테스트 구역 삭제
        }
    }
}
