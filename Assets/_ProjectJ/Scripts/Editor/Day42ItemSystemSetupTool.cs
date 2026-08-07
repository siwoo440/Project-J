using ProjectJ.Data; // 프로젝트 아이템 데이터 형식 참조
using ProjectJ.Gameplay; // 프로젝트 경기 관리자 형식 참조
using ProjectJ.Items; // 프로젝트 인벤토리와 아이템 사용 기능 참조
using ProjectJ.Player; // 프로젝트 플레이어 기능 참조
using ProjectJ.UI; // 프로젝트 Canvas 아이템 사용 표시 참조
using TMPro; // TextMeshPro UI 생성 기능 참조
using UnityEditor; // Unity Editor 에셋과 Undo 기능 참조
using UnityEditor.SceneManagement; // Unity Scene 변경 상태 기능 참조
using UnityEngine; // Unity 오브젝트와 색상 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 프로젝트 Editor 기능 묶음
    internal static class Day42ItemSystemSetupTool // 42일차 28종 데이터와 P0 사용 시스템 자동 설정 도구 선언
    { // 42일차 자동 설정 도구 묶음
        private const string MenuPath = "Project J/Day 42/Configure 28 Items And P0 Effects"; // 42일차 자동 설정 메뉴 경로
        private const string ItemDataFolderPath = "Assets/_ProjectJ/Data/Definitions/Item"; // 28종 아이템 데이터 저장 폴더 경로
        private const string StatusViewName = "ItemUseStatusText"; // 아이템 사용 결과 HUD 오브젝트 이름

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 42일차 자동 설정 항목 등록
        private static void ConfigureItemsAndP0Effects() // 28종 데이터와 플레이어 P0 사용 시스템 구성
        { // 42일차 자동 설정 처리
            PlayerMovementController movementController = Object.FindFirstObjectByType<PlayerMovementController>(); // 현재 Scene 플레이어 이동 관리자 조회
            PrototypeMatchController matchController = Object.FindFirstObjectByType<PrototypeMatchController>(); // 현재 Scene 경기 관리자 조회
            ItemPlacementValidator placementValidator = Object.FindFirstObjectByType<ItemPlacementValidator>(); // 41일차 공통 설치 검사기 조회

            if (movementController == null || matchController == null || placementValidator == null) // 필수 플레이어와 경기와 설치 검사 참조 확인
            { // 필수 참조 누락 처리
                Debug.LogError("[ProjectJ][Day42] PlayerMovementController, PrototypeMatchController, ItemPlacementValidator 연결을 확인합니다. Game Scene과 41일차 설정을 먼저 확인하세요."); // 누락 참조 오류 출력
                return; // 자동 설정 중단
            } // 필수 참조 누락 처리 종료

            EnsureFolder("Assets/_ProjectJ/Data", "Definitions"); // 데이터 정의 상위 폴더 보장
            EnsureFolder("Assets/_ProjectJ/Data/Definitions", "Item"); // 아이템 데이터 폴더 보장
            ItemSeed[] seeds = CreateItemSeeds(); // 확정된 28종 아이템 프로토타입 데이터 생성
            ItemDataDefinition[] itemDefinitions = CreateOrUpdateItemDefinitions(seeds); // 28종 ScriptableObject 생성 또는 갱신
            GameObject playerObject = movementController.gameObject; // 플레이어 구성 대상 오브젝트 저장
            PlayerItemInventory inventory = FindOrAddComponent<PlayerItemInventory>(playerObject); // 기존 2슬롯 인벤토리 조회 또는 추가
            PlayerExternalForceController externalForceController = playerObject.GetComponent<PlayerExternalForceController>(); // 플레이어 외부 힘 관리자 조회
            PlayerRespawnController respawnController = playerObject.GetComponent<PlayerRespawnController>(); // 플레이어 부활 관리자 조회
            PlayerStateController playerStateController = playerObject.GetComponent<PlayerStateController>(); // 플레이어 상태 관리자 조회
            PlayerItemEffectController effectController = FindOrAddComponent<PlayerItemEffectController>(playerObject); // 지속형 아이템 효과 관리자 조회 또는 추가
            effectController.ConfigureForEditor(movementController, externalForceController, respawnController, matchController); // 지속형 효과에 플레이어와 경기 참조 연결
            PlayerItemUseController useController = FindOrAddComponent<PlayerItemUseController>(playerObject); // 슬롯 선택과 P0 사용 관리자 조회 또는 추가
            Transform useOrigin = Camera.main == null ? playerObject.transform : Camera.main.transform; // Main Camera 또는 플레이어 기반 사용 시작점 선택
            LayerMask allLayersMask = ~0; // 모든 현재 물리 Layer 검사 마스크 선언
            useController.ConfigureForEditor(inventory, effectController, playerStateController, matchController, placementValidator, useOrigin, allLayersMask, 2.5f, 2.5f); // Q와 E와 우클릭 사용과 설치 검사 참조 연결
            UpdateChestItemPool(itemDefinitions); // 41일차 상자 후보를 28종 전체로 교체
            CreateOrUpdateStatusView(useController); // Canvas에 아이템 사용 성공과 실패 문구 추가
            ProjectDataCatalogBuilder.RebuildAndValidate(false); // 28종 아이템을 런타임 데이터 카탈로그에 등록
            EditorUtility.SetDirty(inventory); // 인벤토리 변경 상태 표시
            EditorUtility.SetDirty(effectController); // 지속형 효과 관리자 변경 상태 표시
            EditorUtility.SetDirty(useController); // 아이템 사용 관리자 변경 상태 표시
            EditorSceneManager.MarkSceneDirty(playerObject.scene); // 현재 Game Scene 저장 필요 상태 표시
            AssetDatabase.SaveAssets(); // 28종 아이템 데이터와 카탈로그 저장
            Selection.activeGameObject = playerObject; // 설정된 플레이어 오브젝트 선택
            EditorGUIUtility.PingObject(playerObject); // Hierarchy에서 플레이어 강조
            Debug.Log("[ProjectJ][Day42] 28종 데이터, P0 10종 효과, Q/E 슬롯 선택, 우클릭 사용, 상자 가중치와 HUD 안내 구성을 완료했습니다. ITM-014 명칭은 복어 풍선옷입니다. Ctrl + S로 Game Scene을 저장합니다.", playerObject); // 자동 설정 완료 로그 출력
        } // 42일차 자동 설정 처리 종료

        [MenuItem(MenuPath, true)] // 42일차 자동 설정 메뉴 활성 조건 등록
        private static bool ValidateConfigureItemsAndP0Effects() // Play Mode가 아닐 때만 자동 설정 허용
        { // 메뉴 실행 조건 검사 처리
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Edit Mode 실행 가능 여부 반환
        } // 메뉴 실행 조건 검사 처리 종료

        private static ItemDataDefinition[] CreateOrUpdateItemDefinitions(ItemSeed[] seeds) // 28종 아이템 ScriptableObject 생성 또는 갱신
        { // 28종 아이템 데이터 구성 처리
            ItemDataDefinition[] itemDefinitions = new ItemDataDefinition[seeds.Length]; // 28종 아이템 결과 배열 생성

            for (int itemIndex = 0; itemIndex < seeds.Length; itemIndex++) // 모든 확정 아이템 순회
            { // 현재 아이템 데이터 구성 처리
                ItemSeed seed = seeds[itemIndex]; // 현재 아이템 프로토타입 데이터 조회
                string assetPath = ItemDataFolderPath + "/" + seed.Id + "_" + seed.FileName + ".asset"; // 현재 아이템 에셋 전체 경로 계산
                ItemDataDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<ItemDataDefinition>(assetPath); // 같은 경로 기존 아이템 데이터 조회

                if (itemDefinition == null) // 기존 아이템 데이터 누락 여부 확인
                { // 새 아이템 데이터 생성 처리
                    itemDefinition = ScriptableObject.CreateInstance<ItemDataDefinition>(); // 빈 아이템 데이터 인스턴스 생성
                    AssetDatabase.CreateAsset(itemDefinition, assetPath); // 지정 경로에 새 아이템 데이터 저장
                } // 새 아이템 데이터 생성 처리 종료

                itemDefinition.SetEditorIdentity(seed.Id, seed.DisplayName, new ProjectDataVersion(1, 1, 0)); // 아이템 ID와 한글 표시 이름과 버전 적용
                itemDefinition.ConfigureItemForEditor(seed.Description, null, seed.Color, 0.75f); // 설명과 임시 대표 색상 적용
                itemDefinition.ConfigureUsageForEditor(seed.Priority, seed.UseType, seed.EffectType, seed.SpawnWeight, seed.MaximumStackCount, seed.EffectDuration, seed.PrimaryValue, seed.SecondaryValue, seed.EffectRange, seed.EffectRadius, seed.Cooldown, seed.ProjectileSpeed, seed.PlacementHalfExtents); // 사용 방식과 프로토타입 효과 수치 적용
                EditorUtility.SetDirty(itemDefinition); // 현재 아이템 데이터 변경 상태 표시
                itemDefinitions[itemIndex] = itemDefinition; // 구성된 아이템 데이터 결과 저장
            } // 현재 아이템 데이터 구성 처리 종료

            return itemDefinitions; // 구성된 28종 아이템 데이터 반환
        } // 28종 아이템 데이터 구성 처리 종료

        private static void UpdateChestItemPool(ItemDataDefinition[] itemDefinitions) // 41일차 상자 생성기의 아이템 후보 28종 연결
        { // 상자 아이템 후보 갱신 처리
            ItemChestSpawner chestSpawner = Object.FindFirstObjectByType<ItemChestSpawner>(); // 현재 Scene 41일차 상자 생성기 조회

            if (chestSpawner == null) // 상자 생성기 누락 여부 확인
            { // 상자 후보 갱신 생략 처리
                Debug.LogWarning("[ProjectJ][Day42] ItemChestSpawner를 찾지 못해 28종 상자 후보 연결을 생략했습니다. Day 41 자동 설정을 확인하세요."); // 상자 생성기 누락 경고 출력
                return; // 상자 후보 갱신 중단
            } // 상자 후보 갱신 생략 처리 종료

            SerializedObject serializedSpawner = new SerializedObject(chestSpawner); // 비공개 직렬화 필드 편집 객체 생성
            SerializedProperty itemPoolProperty = serializedSpawner.FindProperty("itemPool"); // 상자 아이템 후보 배열 속성 조회
            itemPoolProperty.arraySize = itemDefinitions.Length; // 아이템 후보 배열 크기를 28로 적용

            for (int itemIndex = 0; itemIndex < itemDefinitions.Length; itemIndex++) // 28종 아이템 전체 순회
            { // 현재 상자 후보 연결 처리
                itemPoolProperty.GetArrayElementAtIndex(itemIndex).objectReferenceValue = itemDefinitions[itemIndex]; // 현재 아이템 데이터 참조 연결
            } // 현재 상자 후보 연결 처리 종료

            serializedSpawner.ApplyModifiedProperties(); // 28종 후보 배열 Scene 데이터에 적용
            EditorUtility.SetDirty(chestSpawner); // 상자 생성기 변경 상태 표시
        } // 상자 아이템 후보 갱신 처리 종료

        private static void CreateOrUpdateStatusView(PlayerItemUseController useController) // Canvas 아이템 사용 성공과 실패 문구 구성
        { // 아이템 사용 결과 HUD 구성 처리
            Canvas canvas = Object.FindFirstObjectByType<Canvas>(); // 현재 Scene Canvas 조회

            if (canvas == null) // Canvas 누락 여부 확인
            { // HUD 문구 생성 생략 처리
                Debug.LogWarning("[ProjectJ][Day42] Canvas를 찾지 못해 아이템 사용 결과 HUD 생성을 생략했습니다. Day 40 Canvas 설정을 확인하세요."); // Canvas 누락 경고 출력
                return; // HUD 문구 생성 중단
            } // HUD 문구 생성 생략 처리 종료

            Transform existingStatus = canvas.transform.Find(StatusViewName); // 기존 자동 생성 사용 결과 문구 조회

            if (existingStatus != null) // 기존 사용 결과 문구 존재 여부 확인
            { // 기존 사용 결과 문구 교체 처리
                Undo.DestroyObjectImmediate(existingStatus.gameObject); // 정확한 이름의 기존 문구 제거
            } // 기존 사용 결과 문구 교체 처리 종료

            GameObject statusObject = new GameObject(StatusViewName, typeof(RectTransform)); // 새 사용 결과 HUD 오브젝트 생성
            Undo.RegisterCreatedObjectUndo(statusObject, "Create Day 42 Item Use Status"); // HUD 오브젝트 생성 Undo 등록
            RectTransform rectTransform = statusObject.GetComponent<RectTransform>(); // 새 HUD RectTransform 조회
            rectTransform.SetParent(canvas.transform, false); // Canvas 아래 사용 결과 문구 배치
            rectTransform.anchorMin = new Vector2(0.5f, 0f); // 화면 아래 중앙 최소 Anchor 적용
            rectTransform.anchorMax = new Vector2(0.5f, 0f); // 화면 아래 중앙 최대 Anchor 적용
            rectTransform.pivot = new Vector2(0.5f, 0.5f); // 문구 중심 Pivot 적용
            rectTransform.anchoredPosition = new Vector2(0f, 190f); // 아이템 슬롯 위쪽 표시 위치 적용
            rectTransform.sizeDelta = new Vector2(760f, 56f); // 한 줄 안내 문구 크기 적용
            TextMeshProUGUI messageText = Undo.AddComponent<TextMeshProUGUI>(statusObject); // 사용 결과 TextMeshPro 추가
            messageText.text = string.Empty; // 시작 시 빈 문구 적용
            messageText.fontSize = 24f; // 읽기 쉬운 글자 크기 적용
            messageText.fontStyle = FontStyles.Bold; // 안내 문구 굵은 글꼴 적용
            messageText.alignment = TextAlignmentOptions.Center; // 안내 문구 가운데 정렬 적용
            messageText.color = Color.white; // 안내 문구 흰색 적용
            messageText.enableWordWrapping = false; // 한 줄 안내 유지
            ItemUseStatusView statusView = Undo.AddComponent<ItemUseStatusView>(statusObject); // 사용 결과 이벤트 표시 기능 추가
            statusView.ConfigureForEditor(useController, messageText); // 아이템 사용 관리자와 TextMeshPro 연결
            EditorUtility.SetDirty(statusView); // 사용 안내 표시 변경 상태 저장
        } // 아이템 사용 결과 HUD 구성 처리 종료

        private static T FindOrAddComponent<T>(GameObject targetObject) where T : Component // 대상 오브젝트 컴포넌트 조회 또는 추가
        { // 컴포넌트 조회 또는 추가 처리
            T component = targetObject.GetComponent<T>(); // 기존 대상 컴포넌트 조회

            if (component != null) // 기존 컴포넌트 존재 여부 확인
            { // 기존 컴포넌트 재사용 처리
                return component; // 기존 컴포넌트 반환
            } // 기존 컴포넌트 재사용 처리 종료

            return Undo.AddComponent<T>(targetObject); // Undo 가능한 새 컴포넌트 추가 후 반환
        } // 컴포넌트 조회 또는 추가 처리 종료

        private static void EnsureFolder(string parentPath, string folderName) // 지정 Unity 에셋 폴더 존재 보장
        { // 에셋 폴더 준비 처리
            string completePath = parentPath + "/" + folderName; // 전체 폴더 경로 조합

            if (!AssetDatabase.IsValidFolder(completePath)) // 지정 폴더 누락 여부 확인
            { // 지정 폴더 생성 처리
                AssetDatabase.CreateFolder(parentPath, folderName); // Unity 에셋 폴더 생성
            } // 지정 폴더 생성 처리 종료
        } // 에셋 폴더 준비 처리 종료

        private static ItemSeed[] CreateItemSeeds() // 확정된 28종 아이템과 프로토타입 기본 수치 생성
        { // 28종 아이템 프로토타입 데이터 생성 처리
            return new[] // 28종 아이템 프로토타입 배열 반환
            { // 28종 아이템 프로토타입 묶음
                new ItemSeed("ITM-001", "SpringShoes", "스프링 신발", "8초 동안 공중 추가 점프 1회를 제공", ItemImplementationPriority.P0, ItemUseType.Duration, ItemEffectType.SpringShoes, 10f, 1, 8f, 7.5f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.2f, 0.85f, 1f, 1f)), // P0 스프링 신발
                new ItemSeed("ITM-002", "JellyShield", "젤리 보호막", "4초 동안 밀치기와 방해 외부 힘을 무효화", ItemImplementationPriority.P0, ItemUseType.Duration, ItemEffectType.JellyShield, 10f, 1, 4f, 0f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.35f, 1f, 0.55f, 1f)), // P0 젤리 보호막
                new ItemSeed("ITM-003", "BananaCushion", "바나나 쿠션", "설치 후 밟은 대상을 전방으로 미끄러뜨림", ItemImplementationPriority.P0, ItemUseType.Placement, ItemEffectType.BananaCushion, 10f, 1, 20f, 8f, 1.2f, 0f, 0f, 0f, 0f, new Vector3(0.65f, 0.12f, 0.65f), new Color(1f, 0.85f, 0.15f, 1f)), // P0 바나나 쿠션
                new ItemSeed("ITM-004", "BalloonTrumpet", "풍선 나팔", "전방 7m와 60도 범위의 대상을 밀어냄", ItemImplementationPriority.P0, ItemUseType.Instant, ItemEffectType.BalloonTrumpet, 10f, 1, 0f, 8f, 60f, 7f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(1f, 0.35f, 0.7f, 1f)), // P0 풍선 나팔
                new ItemSeed("ITM-005", "WaterGun", "물총", "2.5초 동안 직선 물줄기로 대상을 연속 밀어냄", ItemImplementationPriority.P0, ItemUseType.Duration, ItemEffectType.WaterGun, 10f, 1, 2.5f, 2.2f, 0f, 9f, 0.16f, 0.1f, 0f, Vector3.one * 0.5f, new Color(0.15f, 0.65f, 1f, 1f)), // P0 물총
                new ItemSeed("ITM-006", "Firework", "폭죽", "0.9초 준비 후 전방 넓은 범위를 강하게 밀어냄", ItemImplementationPriority.P0, ItemUseType.Instant, ItemEffectType.Firework, 10f, 1, 0.9f, 10f, 0f, 4f, 5f, 0f, 0f, Vector3.one * 0.5f, new Color(1f, 0.25f, 0.2f, 1f)), // P0 폭죽
                new ItemSeed("ITM-007", "FeatherShoes", "깃털 신발", "7초 동안 이동과 달리기 속도를 25퍼센트 증가", ItemImplementationPriority.P0, ItemUseType.Duration, ItemEffectType.FeatherShoes, 10f, 1, 7f, 1.25f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(1f, 0.95f, 0.65f, 1f)), // P0 깃털 신발
                new ItemSeed("ITM-008", "Snowball", "눈덩이", "적중 대상의 이동 속도를 3초 동안 25퍼센트 감소", ItemImplementationPriority.P0, ItemUseType.Projectile, ItemEffectType.Snowball, 10f, 1, 3f, 0.75f, 0f, 24f, 0.22f, 0f, 14f, Vector3.one * 0.5f, Color.white), // P0 눈덩이
                new ItemSeed("ITM-009", "Mine", "지뢰", "접근한 대상을 위쪽과 바깥쪽으로 밀어냄", ItemImplementationPriority.P0, ItemUseType.Placement, ItemEffectType.Mine, 10f, 1, 30f, 10f, 0f, 0f, 2.2f, 0f, 0f, new Vector3(0.45f, 0.12f, 0.45f), new Color(0.25f, 0.25f, 0.3f, 1f)), // P0 지뢰
                new ItemSeed("ITM-010", "Ball", "풀 공", "한 슬롯에 최대 5개를 보유하는 약한 밀치기 투사체", ItemImplementationPriority.P0, ItemUseType.Projectile, ItemEffectType.Ball, 10f, 5, 0f, 4f, 0f, 28f, 0.24f, 0f, 16f, Vector3.one * 0.5f, new Color(0.35f, 1f, 0.25f, 1f)), // P0 풀 공
                new ItemSeed("ITM-011", "Jetpack", "제트팩", "5초 동안 Space를 유지할 때 초당 5m 속도로 상승", ItemImplementationPriority.P1, ItemUseType.Duration, ItemEffectType.Jetpack, 6f, 1, 5f, 5f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.7f, 0.7f, 0.75f, 1f)), // P1 제트팩
                new ItemSeed("ITM-012", "Hammer", "망치", "6초 동안 기본 밀치기 힘을 1.75배로 높이고 사거리를 2.5m로 변경", ItemImplementationPriority.P1, ItemUseType.Duration, ItemEffectType.Hammer, 6f, 1, 6f, 1.75f, 0f, 2.5f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.65f, 0.4f, 0.2f, 1f)), // P1 망치
                new ItemSeed("ITM-013", "Bomb", "폭탄", "투척 후 2.5초 뒤 반경 5m를 힘 10으로 밀어냄", ItemImplementationPriority.P1, ItemUseType.Projectile, ItemEffectType.Bomb, 6f, 1, 2.5f, 10f, 0f, 0f, 5f, 0f, 10f, Vector3.one * 0.5f, new Color(0.15f, 0.15f, 0.15f, 1f)), // P1 폭탄
                new ItemSeed("ITM-014", "PufferBalloonSuit", "복어 풍선옷", "5초 동안 가까이 접근한 상대를 바깥쪽으로 밀어냄", ItemImplementationPriority.P1, ItemUseType.Duration, ItemEffectType.PufferBalloonSuit, 6f, 1, 5f, 6f, 0.5f, 0f, 1.8f, 0.5f, 0f, Vector3.one * 0.5f, new Color(0.95f, 0.65f, 0.2f, 1f)), // P1 복어 풍선옷
                new ItemSeed("ITM-015", "InkOctopus", "먹물 문어", "적중한 상대의 화면 중앙 65퍼센트를 3.5초 동안 가림", ItemImplementationPriority.P1, ItemUseType.Projectile, ItemEffectType.InkOctopus, 6f, 1, 3.5f, 0.65f, 0f, 24f, 0.25f, 0f, 14f, Vector3.one * 0.5f, new Color(0.18f, 0.08f, 0.25f, 1f)), // P1 먹물 문어
                new ItemSeed("ITM-016", "FishingRod", "낚시대", "최대 14m 거리의 상대를 사용자 방향으로 힘 10만큼 끌어당김", ItemImplementationPriority.P1, ItemUseType.Instant, ItemEffectType.FishingRod, 6f, 1, 0f, 10f, 0f, 14f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.55f, 0.35f, 0.15f, 1f)), // P1 낚시대
                new ItemSeed("ITM-017", "GrapplingHook", "갈고리", "최대 20m 거리의 구조물 방향으로 초당 12m 속도로 이동", ItemImplementationPriority.P1, ItemUseType.Instant, ItemEffectType.GrapplingHook, 6f, 1, 0f, 12f, 0f, 20f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.45f, 0.5f, 0.55f, 1f)), // P1 갈고리
                new ItemSeed("ITM-018", "SoapBubble", "비눗방울", "이동과 달리기와 앉기를 제한하고 A와 D 교대 6회로 탈출", ItemImplementationPriority.P1, ItemUseType.Projectile, ItemEffectType.SoapBubble, 6f, 1, 0f, 6f, 0f, 24f, 0.3f, 0f, 14f, Vector3.one * 0.5f, new Color(0.7f, 0.9f, 1f, 0.7f)), // P1 비눗방울
                new ItemSeed("ITM-019", "SmokeGrenade", "연막탄", "투척 지점 반경 5m에 6초 동안 시야 방해 구역 생성", ItemImplementationPriority.P1, ItemUseType.Projectile, ItemEffectType.SmokeGrenade, 6f, 1, 6f, 0f, 0f, 0f, 5f, 0f, 10f, Vector3.one * 0.5f, new Color(0.35f, 0.35f, 0.38f, 1f)), // P1 연막탄
                new ItemSeed("ITM-020", "Trampoline", "트램폴린", "사용자 전용 발판을 설치해 최대 3회 상승 속도 12로 도약", ItemImplementationPriority.P1, ItemUseType.Placement, ItemEffectType.Trampoline, 6f, 1, 30f, 12f, 3f, 0f, 0f, 0f, 0f, new Vector3(0.8f, 0.15f, 0.8f), new Color(1f, 0.3f, 0.55f, 1f)), // P1 트램폴린
                new ItemSeed("ITM-021", "GiantBalloon", "거대 풍선", "6초 동안 초당 3.5m 속도로 자동 상승하며 수평 이동 가능", ItemImplementationPriority.P1, ItemUseType.Duration, ItemEffectType.GiantBalloon, 6f, 1, 6f, 3.5f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(1f, 0.45f, 0.65f, 1f)), // P1 거대 풍선
                new ItemSeed("ITM-022", "RewindClock", "되감기 시계", "안전하게 기록된 5초 전 이동 위치로 복귀", ItemImplementationPriority.P2, ItemUseType.Instant, ItemEffectType.RewindClock, 3f, 1, 5f, 0f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.75f, 0.55f, 1f, 1f)), // P2 되감기 시계
                new ItemSeed("ITM-023", "HomingMissile", "유도탄", "목표를 자동 추적해 충돌한 상대를 밀어냄", ItemImplementationPriority.P2, ItemUseType.Projectile, ItemEffectType.HomingMissile, 3f, 1, 0f, 0f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(1f, 0.25f, 0.15f, 1f)), // P2 유도탄
                new ItemSeed("ITM-024", "MiniaturePotion", "소형화 물약", "6초 동안 외형과 충돌체를 80퍼센트 크기로 축소", ItemImplementationPriority.P2, ItemUseType.Duration, ItemEffectType.MiniaturePotion, 3f, 1, 6f, 0.8f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.6f, 0.9f, 0.35f, 1f)), // P2 소형화 물약
                new ItemSeed("ITM-025", "Drone", "드론", "현재 1위 플레이어를 추적해 한 번 밀어냄", ItemImplementationPriority.P2, ItemUseType.Duration, ItemEffectType.Drone, 3f, 1, 0f, 0f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.45f, 0.55f, 0.65f, 1f)), // P2 드론
                new ItemSeed("ITM-026", "InvisibilityCloak", "투명 망토", "5초 동안 다른 화면에서 투명해지고 추적 대상에서 제외", ItemImplementationPriority.P2, ItemUseType.Duration, ItemEffectType.InvisibilityCloak, 3f, 1, 5f, 0f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.65f, 0.65f, 0.8f, 0.65f)), // P2 투명 망토
                new ItemSeed("ITM-027", "SniperWaterGun", "저격 물총", "최대 50m 거리에서 조준 사격으로 강하게 밀어냄", ItemImplementationPriority.P2, ItemUseType.Instant, ItemEffectType.SniperWaterGun, 3f, 1, 0f, 0f, 0f, 50f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.1f, 0.55f, 0.95f, 1f)), // P2 저격 물총
                new ItemSeed("ITM-028", "Cart", "카트", "최대 8초 동안 연결된 경로를 자동 주행", ItemImplementationPriority.P2, ItemUseType.Duration, ItemEffectType.Cart, 3f, 1, 8f, 0f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.85f, 0.25f, 0.2f, 1f)) // P2 카트
            }; // 28종 아이템 프로토타입 묶음 종료
        } // 28종 아이템 프로토타입 데이터 생성 처리 종료

        private readonly struct ItemSeed // 단일 아이템 자동 생성용 프로토타입 값 선언
        { // 단일 아이템 프로토타입 값 묶음
            public ItemSeed(string id, string fileName, string displayName, string description, ItemImplementationPriority priority, ItemUseType useType, ItemEffectType effectType, float spawnWeight, int maximumStackCount, float effectDuration, float primaryValue, float secondaryValue, float effectRange, float effectRadius, float cooldown, float projectileSpeed, Vector3 placementHalfExtents, Color color) // 단일 아이템 프로토타입 생성
            { // 단일 아이템 프로토타입 생성 처리
                Id = id; // 아이템 고유 ID 저장
                FileName = fileName; // 아이템 영문 파일명 저장
                DisplayName = displayName; // 아이템 한글 표시 이름 저장
                Description = description; // 아이템 설명 저장
                Priority = priority; // 구현 우선순위 저장
                UseType = useType; // 공통 사용 방식 저장
                EffectType = effectType; // 실제 효과 종류 저장
                SpawnWeight = spawnWeight; // 상자 생성 가중치 저장
                MaximumStackCount = maximumStackCount; // 최대 중첩 수 저장
                EffectDuration = effectDuration; // 효과 유지 또는 준비 시간 저장
                PrimaryValue = primaryValue; // 핵심 효과 수치 저장
                SecondaryValue = secondaryValue; // 보조 효과 수치 저장
                EffectRange = effectRange; // 효과 거리 저장
                EffectRadius = effectRadius; // 효과 반지름 저장
                Cooldown = cooldown; // 내부 반복 간격 저장
                ProjectileSpeed = projectileSpeed; // 투사체 속도 저장
                PlacementHalfExtents = placementHalfExtents; // 설치 크기 저장
                Color = color; // 임시 대표 색상 저장
            } // 단일 아이템 프로토타입 생성 처리 종료

            public string Id { get; } // 아이템 고유 ID 반환
            public string FileName { get; } // 아이템 영문 파일명 반환
            public string DisplayName { get; } // 아이템 한글 표시 이름 반환
            public string Description { get; } // 아이템 설명 반환
            public ItemImplementationPriority Priority { get; } // 구현 우선순위 반환
            public ItemUseType UseType { get; } // 공통 사용 방식 반환
            public ItemEffectType EffectType { get; } // 실제 효과 종류 반환
            public float SpawnWeight { get; } // 상자 생성 가중치 반환
            public int MaximumStackCount { get; } // 최대 중첩 수 반환
            public float EffectDuration { get; } // 효과 시간 반환
            public float PrimaryValue { get; } // 핵심 효과 수치 반환
            public float SecondaryValue { get; } // 보조 효과 수치 반환
            public float EffectRange { get; } // 효과 거리 반환
            public float EffectRadius { get; } // 효과 반지름 반환
            public float Cooldown { get; } // 내부 반복 간격 반환
            public float ProjectileSpeed { get; } // 투사체 속도 반환
            public Vector3 PlacementHalfExtents { get; } // 설치 크기 반환
            public Color Color { get; } // 임시 대표 색상 반환
        } // 단일 아이템 프로토타입 값 묶음 종료
    } // 42일차 자동 설정 도구 묶음 종료
} // 프로젝트 Editor 기능 묶음 종료
