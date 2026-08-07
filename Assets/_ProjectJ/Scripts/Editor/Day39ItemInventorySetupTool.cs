using ProjectJ.Data; // 프로젝트 아이템 데이터 형식 참조
using ProjectJ.Items; // 프로젝트 인벤토리와 상자 기능 참조
using ProjectJ.Player; // 프로젝트 플레이어 이동 컴포넌트 참조
using UnityEditor; // Unity Editor 에셋과 Undo 기능 참조
using UnityEditor.SceneManagement; // Unity Scene 변경 상태 기능 참조
using UnityEngine; // Unity 오브젝트와 물리 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 프로젝트 Editor 기능 묶음
    internal static class Day39ItemInventorySetupTool // 39일차 아이템과 2슬롯 인벤토리 자동 설정 도구 선언
    { // 39일차 자동 설정 도구 묶음
        private const string MenuPath = ProjectJEditorMenuPaths.ItemInventory + "/2슬롯 인벤토리와 테스트 상자 구성 (Day 39일차)"; // 39일차 자동 설정 메뉴 경로
        private const string ItemDataFolderPath = "Assets/_ProjectJ/Data/Definitions/Item"; // 아이템 데이터 저장 폴더 경로
        private const string ChestRootName = "Day39_ItemChests"; // 테스트 상자 묶음 오브젝트 이름

        private static readonly string[] ItemAssetPaths = // 테스트 아이템 에셋 경로 목록 선언
        { // 테스트 아이템 에셋 경로 묶음
            ItemDataFolderPath + "/ITM-001_SpringShoes.asset", // 스프링 신발 데이터 경로
            ItemDataFolderPath + "/ITM-002_JellyShield.asset", // 젤리 보호막 데이터 경로
            ItemDataFolderPath + "/ITM-003_BananaCushion.asset" // 바나나 쿠션 데이터 경로
        }; // 테스트 아이템 에셋 경로 묶음 종료

        private static readonly string[] ItemIds = // 테스트 아이템 고유 ID 목록 선언
        { // 테스트 아이템 고유 ID 묶음
            "ITM-001", // 스프링 신발 고유 ID
            "ITM-002", // 젤리 보호막 고유 ID
            "ITM-003" // 바나나 쿠션 고유 ID
        }; // 테스트 아이템 고유 ID 묶음 종료

        private static readonly string[] ItemNames = // 테스트 아이템 표시 이름 목록 선언
        { // 테스트 아이템 표시 이름 묶음
            "Spring Shoes", // 스프링 신발 표시 이름
            "Jelly Shield", // 젤리 보호막 표시 이름
            "Banana Cushion" // 바나나 쿠션 표시 이름
        }; // 테스트 아이템 표시 이름 묶음 종료

        private static readonly string[] ItemDescriptions = // 테스트 아이템 설명 목록 선언
        { // 테스트 아이템 설명 묶음
            "41일차 효과 구현 예정인 스프링 신발 공통 데이터", // 스프링 신발 데이터 전용 설명
            "41일차 효과 구현 예정인 젤리 보호막 공통 데이터", // 젤리 보호막 데이터 전용 설명
            "42일차 효과 구현 예정인 바나나 쿠션 공통 데이터" // 바나나 쿠션 데이터 전용 설명
        }; // 테스트 아이템 설명 묶음 종료

        private static readonly Color[] ItemColors = // 테스트 아이템 대표 색상 목록 선언
        { // 테스트 아이템 대표 색상 묶음
            new Color(0.25f, 0.85f, 1f, 1f), // 스프링 신발 청록색
            new Color(0.4f, 1f, 0.55f, 1f), // 젤리 보호막 연두색
            new Color(1f, 0.85f, 0.2f, 1f) // 바나나 쿠션 노란색
        }; // 테스트 아이템 대표 색상 묶음 종료

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 39일차 자동 설정 항목 등록
        private static void ConfigureItemInventoryAndTestChests() // 아이템 데이터와 플레이어 인벤토리와 테스트 상자 구성
        { // 39일차 자동 설정 처리
            PlayerMovementController movementController = Object.FindFirstObjectByType<PlayerMovementController>(); // 현재 Scene 플레이어 이동 컴포넌트 조회

            if (movementController == null) // 플레이어 이동 컴포넌트 누락 여부 확인
            { // 플레이어 누락 처리
                Debug.LogError("[ProjectJ][Day39] PlayerMovementController를 찾을 수 없습니다. Game Scene을 열고 다시 실행하세요."); // 플레이어 누락 오류 출력
                return; // 자동 설정 중단
            } // 플레이어 누락 처리 종료

            EnsureFolder("Assets/_ProjectJ/Data", "Definitions"); // 데이터 정의 상위 폴더 보장
            EnsureFolder("Assets/_ProjectJ/Data/Definitions", "Item"); // 아이템 데이터 폴더 보장
            ItemDataDefinition[] itemDefinitions = CreateOrUpdateItemDefinitions(); // 세 테스트 아이템 데이터 생성 또는 갱신
            PlayerItemInventory inventory = FindOrAddInventory(movementController.gameObject); // 플레이어 2슬롯 인벤토리 조회 또는 추가
            Transform chestRoot = RecreateChestRoot(); // 기존 테스트 상자 묶음 교체
            CreateTestChests(chestRoot, movementController.transform, itemDefinitions); // 슬롯 확인용 테스트 상자 세 개 생성
            ProjectDataCatalogBuilder.RebuildAndValidate(false); // 새 아이템을 런타임 데이터 카탈로그에 등록
            EditorUtility.SetDirty(inventory); // 인벤토리 변경 상태 표시
            EditorSceneManager.MarkSceneDirty(movementController.gameObject.scene); // 현재 Game Scene 저장 필요 상태 표시
            AssetDatabase.SaveAssets(); // 아이템 데이터와 카탈로그 저장
            Selection.activeGameObject = inventory.gameObject; // 설정된 플레이어 오브젝트 선택
            EditorGUIUtility.PingObject(inventory.gameObject); // Hierarchy에서 플레이어 강조
            Debug.Log("[ProjectJ][Day39] 아이템 공통 데이터 3개, 플레이어 2슬롯 인벤토리, 접촉 획득 테스트 상자 3개 구성을 완료했습니다.", inventory); // 자동 설정 완료 로그 출력
        } // 39일차 자동 설정 처리 종료

        [MenuItem(MenuPath, true)] // 39일차 자동 설정 메뉴 활성 조건 등록
        private static bool ValidateConfigureItemInventoryAndTestChests() // Play Mode가 아닐 때만 자동 설정 허용
        { // 메뉴 실행 조건 검사 처리
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Edit Mode 실행 가능 여부 반환
        } // 메뉴 실행 조건 검사 처리 종료

        private static ItemDataDefinition[] CreateOrUpdateItemDefinitions() // 세 아이템 공통 데이터 생성 또는 갱신
        { // 아이템 데이터 생성 처리
            ItemDataDefinition[] itemDefinitions = new ItemDataDefinition[ItemAssetPaths.Length]; // 세 아이템 결과 배열 생성

            for (int itemIndex = 0; itemIndex < ItemAssetPaths.Length; itemIndex++) // 세 아이템 데이터 순회
            { // 현재 아이템 데이터 구성 처리
                ItemDataDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<ItemDataDefinition>(ItemAssetPaths[itemIndex]); // 기존 아이템 데이터 조회

                if (itemDefinition == null) // 기존 아이템 데이터 누락 여부 확인
                { // 새 아이템 데이터 생성 처리
                    itemDefinition = ScriptableObject.CreateInstance<ItemDataDefinition>(); // 빈 아이템 데이터 인스턴스 생성
                    AssetDatabase.CreateAsset(itemDefinition, ItemAssetPaths[itemIndex]); // 지정 경로에 아이템 데이터 저장
                } // 새 아이템 데이터 생성 처리 종료

                itemDefinition.SetEditorIdentity(ItemIds[itemIndex], ItemNames[itemIndex], new ProjectDataVersion(1, 0, 0)); // 아이템 공통 식별 정보 적용
                itemDefinition.ConfigureItemForEditor(ItemDescriptions[itemIndex], null, ItemColors[itemIndex], 0.75f); // 39일차 표시 데이터 적용
                EditorUtility.SetDirty(itemDefinition); // 현재 아이템 데이터 변경 상태 표시
                itemDefinitions[itemIndex] = itemDefinition; // 구성된 아이템 데이터 결과 저장
            } // 현재 아이템 데이터 구성 처리 종료

            return itemDefinitions; // 구성된 세 아이템 데이터 반환
        } // 아이템 데이터 생성 처리 종료

        private static PlayerItemInventory FindOrAddInventory(GameObject playerObject) // 플레이어 인벤토리 조회 또는 추가
        { // 플레이어 인벤토리 준비 처리
            PlayerItemInventory inventory = playerObject.GetComponent<PlayerItemInventory>(); // 기존 2슬롯 인벤토리 조회

            if (inventory != null) // 기존 인벤토리 존재 여부 확인
            { // 기존 인벤토리 처리
                return inventory; // 기존 인벤토리 재사용
            } // 기존 인벤토리 처리 종료

            return Undo.AddComponent<PlayerItemInventory>(playerObject); // Undo 가능한 새 인벤토리 추가 후 반환
        } // 플레이어 인벤토리 준비 처리 종료

        private static Transform RecreateChestRoot() // 테스트 상자 묶음 오브젝트 새로 구성
        { // 테스트 상자 묶음 교체 처리
            GameObject existingRoot = GameObject.Find(ChestRootName); // 기존 테스트 상자 묶음 조회

            if (existingRoot != null) // 기존 테스트 상자 묶음 존재 여부 확인
            { // 기존 테스트 상자 묶음 제거 처리
                Undo.DestroyObjectImmediate(existingRoot); // 정확히 일치하는 기존 자동 생성 묶음 제거
            } // 기존 테스트 상자 묶음 제거 처리 종료

            GameObject chestRootObject = new GameObject(ChestRootName); // 새 테스트 상자 묶음 생성
            Undo.RegisterCreatedObjectUndo(chestRootObject, "Create Day 39 Item Chests"); // 상자 묶음 생성 Undo 등록
            return chestRootObject.transform; // 새 테스트 상자 묶음 Transform 반환
        } // 테스트 상자 묶음 교체 처리 종료

        private static void CreateTestChests(Transform chestRoot, Transform playerTransform, ItemDataDefinition[] itemDefinitions) // 플레이어 앞에 테스트 상자 세 개 생성
        { // 테스트 상자 일괄 생성 처리
            float[] horizontalOffsets = { -1.8f, 0f, 1.8f }; // 플레이어 기준 상자 좌우 간격 선언

            for (int itemIndex = 0; itemIndex < itemDefinitions.Length; itemIndex++) // 세 아이템 데이터 순회
            { // 현재 테스트 상자 생성 처리
                Vector3 chestPosition = playerTransform.position // 플레이어 현재 위치 기준 선언
                    + playerTransform.forward * 3f // 플레이어 앞쪽 3미터 이동
                    + playerTransform.right * horizontalOffsets[itemIndex] // 아이템 번호별 좌우 간격 적용
                    + Vector3.up * 0.6f; // 바닥 위 상자 중심 높이 적용
                CreateTestChest(chestRoot, chestPosition, itemDefinitions[itemIndex], itemIndex + 1); // 현재 아이템 테스트 상자 생성
            } // 현재 테스트 상자 생성 처리 종료
        } // 테스트 상자 일괄 생성 처리 종료

        private static void CreateTestChest(Transform chestRoot, Vector3 worldPosition, ItemDataDefinition itemDefinition, int chestNumber) // 단일 접촉 획득 테스트 상자 생성
        { // 단일 테스트 상자 생성 처리
            GameObject chestObject = new GameObject($"Day39_ItemChest_{chestNumber:00}_{itemDefinition.DataId}"); // 아이템 ID가 포함된 상자 루트 생성
            Undo.RegisterCreatedObjectUndo(chestObject, "Create Day 39 Item Chest"); // 상자 생성 Undo 등록
            chestObject.transform.SetParent(chestRoot, true); // 테스트 상자 묶음 아래 배치
            chestObject.transform.position = worldPosition; // 플레이어 앞 시험 위치 적용
            BoxCollider pickupTrigger = Undo.AddComponent<BoxCollider>(chestObject); // 상자 접촉 감지 Collider 추가
            pickupTrigger.isTrigger = true; // 통과 가능한 Trigger로 설정
            pickupTrigger.size = new Vector3(1.2f, 1.2f, 1.2f); // 넉넉한 접촉 범위 적용
            Rigidbody rigidbody = Undo.AddComponent<Rigidbody>(chestObject); // CharacterController 접촉용 Rigidbody 추가
            rigidbody.isKinematic = true; // 물리 힘에 움직이지 않는 상자 설정
            rigidbody.useGravity = false; // 상자 중력 비활성화
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete; // 고정 상자 기본 충돌 검사 적용
            GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Cube); // 임시 상자 표시 큐브 생성
            Undo.RegisterCreatedObjectUndo(visualObject, "Create Day 39 Item Chest Visual"); // 표시 큐브 생성 Undo 등록
            visualObject.name = "Visual"; // 상자 표시 오브젝트 이름 지정
            visualObject.transform.SetParent(chestObject.transform, false); // 상자 루트 아래 표시 큐브 배치
            visualObject.transform.localScale = Vector3.one * itemDefinition.PickupVisualScale; // 아이템 데이터 표시 크기 적용
            Collider visualCollider = visualObject.GetComponent<Collider>(); // 임시 큐브 Collider 조회

            if (visualCollider != null) // 임시 큐브 Collider 존재 여부 확인
            { // 불필요 Collider 제거 처리
                Undo.DestroyObjectImmediate(visualCollider); // 루트 Trigger와 중복되는 Collider 제거
            } // 불필요 Collider 제거 처리 종료

            ItemChestPickup pickup = Undo.AddComponent<ItemChestPickup>(chestObject); // 접촉 아이템 지급 기능 추가
            pickup.ConfigureForEditor(itemDefinition, pickupTrigger, visualObject, true, true); // 아이템 데이터와 상자 참조 연결
            EditorUtility.SetDirty(pickup); // 상자 설정 변경 상태 표시
        } // 단일 테스트 상자 생성 처리 종료

        private static void EnsureFolder(string parentPath, string folderName) // 지정 Unity 에셋 폴더 존재 보장
        { // 에셋 폴더 준비 처리
            string completePath = parentPath + "/" + folderName; // 전체 폴더 경로 조합

            if (!AssetDatabase.IsValidFolder(completePath)) // 지정 폴더 누락 여부 확인
            { // 지정 폴더 생성 처리
                AssetDatabase.CreateFolder(parentPath, folderName); // Unity 에셋 폴더 생성
            } // 지정 폴더 생성 처리 종료
        } // 에셋 폴더 준비 처리 종료
    } // 39일차 자동 설정 도구 묶음 종료
} // 프로젝트 Editor 기능 묶음 종료
