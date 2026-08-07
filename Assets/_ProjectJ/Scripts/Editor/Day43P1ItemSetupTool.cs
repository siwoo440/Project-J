using ProjectJ.Data; // 프로젝트 아이템 데이터 형식 참조
using ProjectJ.Gameplay; // 프로젝트 경기 관리자 형식 참조
using ProjectJ.Items; // 프로젝트 P1 아이템 효과 기능 참조
using ProjectJ.Player; // 프로젝트 플레이어 기능 참조
using UnityEditor; // Unity Editor 에셋과 Undo 기능 참조
using UnityEditor.SceneManagement; // Unity Scene 변경 상태 기능 참조
using UnityEngine; // Unity 오브젝트와 색상 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 프로젝트 Editor 기능 묶음
    internal static class Day43P1ItemSetupTool // 43일차 P1 아이템 11종 자동 설정 도구 선언
    { // 43일차 자동 설정 도구 묶음
        private const string MenuPath = "Project J/Day 43/Configure P1 11 Item Effects"; // 43일차 자동 설정 메뉴 경로
        private const string ItemDataFolderPath = "Assets/_ProjectJ/Data/Definitions/Item"; // 아이템 데이터 저장 폴더 경로
        private const string NewPufferAssetPath = ItemDataFolderPath + "/ITM-014_PufferBalloonSuit.asset"; // 새 복어 풍선옷 에셋 경로

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 43일차 자동 설정 항목 등록
        private static void ConfigureP1ItemEffects() // P1 11종 데이터와 플레이어 효과 시스템 구성
        { // 43일차 자동 설정 처리
            PlayerMovementController movementController = Object.FindFirstObjectByType<PlayerMovementController>(); // 현재 Scene 플레이어 이동 관리자 조회
            PrototypeMatchController matchController = Object.FindFirstObjectByType<PrototypeMatchController>(); // 현재 Scene 경기 관리자 조회
            ItemPlacementValidator placementValidator = Object.FindFirstObjectByType<ItemPlacementValidator>(); // 공통 설치 검사기 조회

            if (movementController == null || matchController == null || placementValidator == null) // 필수 플레이어와 경기와 설치 검사 참조 확인
            { // 필수 참조 누락 처리
                Debug.LogError("[ProjectJ][Day43] PlayerMovementController, PrototypeMatchController, ItemPlacementValidator 연결을 확인합니다. Game Scene과 Day 42 설정을 먼저 확인하세요."); // 누락 참조 오류 출력
                return; // 자동 설정 중단
            } // 필수 참조 누락 처리 종료

            RenamePufferBalloonSuitAsset(); // 기존 ITM-014 에셋 파일명 교체
            ItemSeed[] seeds = CreateP1Seeds(); // 확정된 P1 11종 프로토타입 수치 생성
            CreateOrUpdateP1Definitions(seeds); // P1 11종 ScriptableObject 데이터 갱신
            GameObject playerObject = movementController.gameObject; // 플레이어 구성 대상 오브젝트 저장
            PlayerScreenObscureView obscureView = FindOrAddComponent<PlayerScreenObscureView>(playerObject); // 먹물과 연막과 비눗방울 화면 표시 조회 또는 추가
            PlayerItemEffectController effectController = FindOrAddComponent<PlayerItemEffectController>(playerObject); // 지속형 아이템 효과 관리자 조회 또는 추가
            PlayerExternalForceController externalForceController = playerObject.GetComponent<PlayerExternalForceController>(); // 플레이어 외부 힘 관리자 조회
            PlayerRespawnController respawnController = playerObject.GetComponent<PlayerRespawnController>(); // 플레이어 부활 관리자 조회
            effectController.ConfigureForEditor(movementController, externalForceController, respawnController, matchController); // P0와 P1 효과에 플레이어와 경기 참조 연결
            EditorUtility.SetDirty(obscureView); // 화면 방해 표시 변경 상태 저장
            EditorUtility.SetDirty(effectController); // P1 효과 관리자 변경 상태 저장
            ProjectDataCatalogBuilder.RebuildAndValidate(false); // 변경된 P1 데이터를 런타임 카탈로그에 등록
            EditorSceneManager.MarkSceneDirty(playerObject.scene); // 현재 Game Scene 저장 필요 상태 표시
            AssetDatabase.SaveAssets(); // P1 11종 데이터와 카탈로그 저장
            Selection.activeGameObject = playerObject; // 설정된 플레이어 오브젝트 선택
            EditorGUIUtility.PingObject(playerObject); // Hierarchy에서 플레이어 강조
            Debug.Log("[ProjectJ][Day43] P1 아이템 11종 효과와 화면 방해와 A/D 교대 탈출 구성을 완료했습니다. ITM-014 명칭은 복어 풍선옷입니다. Ctrl + S로 Game Scene을 저장합니다.", playerObject); // 자동 설정 완료 로그 출력
        } // 43일차 자동 설정 처리 종료

        [MenuItem(MenuPath, true)] // 43일차 자동 설정 메뉴 활성 조건 등록
        private static bool ValidateConfigureP1ItemEffects() // Play Mode가 아닐 때만 자동 설정 허용
        { // 메뉴 실행 조건 검사 처리
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Edit Mode 실행 가능 여부 반환
        } // 메뉴 실행 조건 검사 처리 종료

        private static void RenamePufferBalloonSuitAsset() // 기존 ITM-014 에셋 파일명을 복어 풍선옷으로 교체
        { // 복어 풍선옷 에셋 이름 변경 처리
            ItemDataDefinition newAsset = AssetDatabase.LoadAssetAtPath<ItemDataDefinition>(NewPufferAssetPath); // 이미 이름이 변경된 새 에셋 조회

            if (newAsset != null) // 새 복어 풍선옷 에셋 존재 여부 확인
            { // 이미 변경된 에셋 처리
                return; // 중복 이름 변경 생략
            } // 이미 변경된 에셋 처리 종료

            string[] itemAssetGuids = AssetDatabase.FindAssets("t:ItemDataDefinition", new[] { ItemDataFolderPath }); // 아이템 데이터 폴더 전체 에셋 GUID 검색
            string currentAssetPath = string.Empty; // 현재 ITM-014 에셋 경로 저장

            for (int index = 0; index < itemAssetGuids.Length; index++) // 검색된 아이템 데이터 전체 순회
            { // 현재 아이템 데이터 확인
                string candidatePath = AssetDatabase.GUIDToAssetPath(itemAssetGuids[index]); // 현재 GUID 에셋 경로 변환
                ItemDataDefinition candidate = AssetDatabase.LoadAssetAtPath<ItemDataDefinition>(candidatePath); // 현재 아이템 데이터 조회

                if (candidate != null && candidate.DataId == "ITM-014") // 복어 풍선옷 데이터 ID 일치 여부 확인
                { // 이름 변경 대상 저장 처리
                    currentAssetPath = candidatePath; // 현재 ITM-014 에셋 경로 저장
                    break; // 대상 검색 종료
                } // 이름 변경 대상 저장 처리 종료
            } // 현재 아이템 데이터 확인 종료

            if (string.IsNullOrWhiteSpace(currentAssetPath)) // 기존 ITM-014 에셋 누락 여부 확인
            { // 이름 변경 대상 없음 처리
                return; // 새 데이터 생성 단계로 진행
            } // 이름 변경 대상 없음 처리 종료

            string moveError = AssetDatabase.MoveAsset(currentAssetPath, NewPufferAssetPath); // GUID를 유지한 에셋 파일명 변경 실행

            if (!string.IsNullOrWhiteSpace(moveError)) // 에셋 이름 변경 오류 여부 확인
            { // 이름 변경 실패 처리
                Debug.LogError($"[ProjectJ][Day43] 복어 풍선옷 에셋 이름 변경 실패: {moveError}"); // 에셋 이동 오류 출력
            } // 이름 변경 실패 처리 종료
        } // 복어 풍선옷 에셋 이름 변경 처리 종료

        private static void CreateOrUpdateP1Definitions(ItemSeed[] seeds) // P1 11종 ScriptableObject 생성 또는 갱신
        { // P1 아이템 데이터 구성 처리
            for (int index = 0; index < seeds.Length; index++) // P1 11종 전체 순회
            { // 현재 P1 아이템 구성 처리
                ItemSeed seed = seeds[index]; // 현재 P1 프로토타입 데이터 조회
                string assetPath = ItemDataFolderPath + "/" + seed.Id + "_" + seed.FileName + ".asset"; // 현재 아이템 에셋 전체 경로 계산
                ItemDataDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<ItemDataDefinition>(assetPath); // 같은 경로 기존 아이템 데이터 조회

                if (itemDefinition == null) // 기존 아이템 데이터 누락 여부 확인
                { // 새 아이템 데이터 생성 처리
                    itemDefinition = ScriptableObject.CreateInstance<ItemDataDefinition>(); // 빈 아이템 데이터 인스턴스 생성
                    AssetDatabase.CreateAsset(itemDefinition, assetPath); // 지정 경로에 새 아이템 데이터 저장
                } // 새 아이템 데이터 생성 처리 종료

                itemDefinition.name = seed.Id + "_" + seed.FileName; // Project 창 표시용 내부 에셋 이름 적용
                Sprite existingIcon = itemDefinition.InventoryIcon; // 기존 인벤토리 아이콘 보존
                float existingVisualScale = itemDefinition.PickupVisualScale; // 기존 상자 표시 크기 보존
                itemDefinition.SetEditorIdentity(seed.Id, seed.DisplayName, new ProjectDataVersion(1, 2, 0)); // 아이템 ID와 한글 표시 이름과 43일차 버전 적용
                itemDefinition.ConfigureItemForEditor(seed.Description, existingIcon, seed.Color, existingVisualScale); // 설명과 기존 아이콘과 대표 색상 적용
                itemDefinition.ConfigureUsageForEditor(ItemImplementationPriority.P1, seed.UseType, seed.EffectType, 6f, 1, seed.EffectDuration, seed.PrimaryValue, seed.SecondaryValue, seed.EffectRange, seed.EffectRadius, seed.Cooldown, seed.ProjectileSpeed, seed.PlacementHalfExtents); // P1 사용 방식과 확정 프로토타입 수치 적용
                EditorUtility.SetDirty(itemDefinition); // 현재 P1 아이템 데이터 변경 상태 표시
            } // 현재 P1 아이템 구성 처리 종료
        } // P1 아이템 데이터 구성 처리 종료

        private static T FindOrAddComponent<T>(GameObject targetObject) where T : Component // 대상 오브젝트 컴포넌트 조회 또는 추가
        { // 컴포넌트 조회 또는 추가 처리
            T component = targetObject.GetComponent<T>(); // 기존 대상 컴포넌트 조회

            if (component != null) // 기존 컴포넌트 존재 여부 확인
            { // 기존 컴포넌트 재사용 처리
                return component; // 기존 컴포넌트 반환
            } // 기존 컴포넌트 재사용 처리 종료

            return Undo.AddComponent<T>(targetObject); // Undo 가능한 새 컴포넌트 추가 후 반환
        } // 컴포넌트 조회 또는 추가 처리 종료

        private static ItemSeed[] CreateP1Seeds() // 확정된 P1 11종 프로토타입 기본 수치 생성
        { // P1 프로토타입 데이터 생성 처리
            return new[] // P1 11종 프로토타입 배열 반환
            { // P1 11종 프로토타입 묶음
                new ItemSeed("ITM-011", "Jetpack", "제트팩", "5초 동안 Space를 유지할 때 초당 5m 속도로 상승", ItemUseType.Duration, ItemEffectType.Jetpack, 5f, 5f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.7f, 0.7f, 0.75f, 1f)), // P1 제트팩
                new ItemSeed("ITM-012", "Hammer", "망치", "6초 동안 기본 밀치기 힘을 1.75배로 높이고 사거리를 2.5m로 변경", ItemUseType.Duration, ItemEffectType.Hammer, 6f, 1.75f, 0f, 2.5f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.65f, 0.4f, 0.2f, 1f)), // P1 망치
                new ItemSeed("ITM-013", "Bomb", "폭탄", "투척 후 2.5초 뒤 반경 5m를 힘 10으로 밀어냄", ItemUseType.Projectile, ItemEffectType.Bomb, 2.5f, 10f, 0f, 0f, 5f, 0f, 10f, Vector3.one * 0.5f, new Color(0.15f, 0.15f, 0.15f, 1f)), // P1 폭탄
                new ItemSeed("ITM-014", "PufferBalloonSuit", "복어 풍선옷", "5초 동안 반경 1.8m의 상대를 0.5초 간격으로 힘 6만큼 밀어냄", ItemUseType.Duration, ItemEffectType.PufferBalloonSuit, 5f, 6f, 0.5f, 0f, 1.8f, 0.5f, 0f, Vector3.one * 0.5f, new Color(0.95f, 0.65f, 0.2f, 1f)), // P1 복어 풍선옷
                new ItemSeed("ITM-015", "InkOctopus", "먹물 문어", "적중한 상대의 화면 중앙 65퍼센트를 3.5초 동안 가림", ItemUseType.Projectile, ItemEffectType.InkOctopus, 3.5f, 0.65f, 0f, 24f, 0.25f, 0f, 14f, Vector3.one * 0.5f, new Color(0.18f, 0.08f, 0.25f, 1f)), // P1 먹물 문어
                new ItemSeed("ITM-016", "FishingRod", "낚시대", "최대 14m 거리의 조준 상대를 사용자 방향으로 힘 10만큼 끌어당김", ItemUseType.Instant, ItemEffectType.FishingRod, 0f, 10f, 0f, 14f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.55f, 0.35f, 0.15f, 1f)), // P1 낚시대
                new ItemSeed("ITM-017", "GrapplingHook", "갈고리", "최대 20m 거리의 구조물 방향으로 초당 12m 속도로 이동", ItemUseType.Instant, ItemEffectType.GrapplingHook, 0f, 12f, 0f, 20f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(0.45f, 0.5f, 0.55f, 1f)), // P1 갈고리
                new ItemSeed("ITM-018", "SoapBubble", "비눗방울", "적중 대상의 이동과 달리기와 앉기를 제한하고 A와 D 교대 6회로 탈출", ItemUseType.Projectile, ItemEffectType.SoapBubble, 0f, 6f, 0f, 24f, 0.3f, 0f, 14f, Vector3.one * 0.5f, new Color(0.7f, 0.9f, 1f, 0.7f)), // P1 비눗방울
                new ItemSeed("ITM-019", "SmokeGrenade", "연막탄", "투척 지점 반경 5m에 6초 동안 시야 방해 구역 생성", ItemUseType.Projectile, ItemEffectType.SmokeGrenade, 6f, 0f, 0f, 0f, 5f, 0f, 10f, Vector3.one * 0.5f, new Color(0.35f, 0.35f, 0.38f, 1f)), // P1 연막탄
                new ItemSeed("ITM-020", "Trampoline", "트램폴린", "사용자 전용 발판을 설치해 최대 3회 상승 속도 12로 도약", ItemUseType.Placement, ItemEffectType.Trampoline, 30f, 12f, 3f, 0f, 0f, 0f, 0f, new Vector3(0.8f, 0.15f, 0.8f), new Color(1f, 0.3f, 0.55f, 1f)), // P1 트램폴린
                new ItemSeed("ITM-021", "GiantBalloon", "거대 풍선", "6초 동안 초당 3.5m 속도로 자동 상승하며 수평 이동 가능", ItemUseType.Duration, ItemEffectType.GiantBalloon, 6f, 3.5f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f, new Color(1f, 0.45f, 0.65f, 1f)) // P1 거대 풍선
            }; // P1 11종 프로토타입 묶음 종료
        } // P1 프로토타입 데이터 생성 처리 종료

        private readonly struct ItemSeed // P1 아이템 프로토타입 기본값 묶음 선언
        { // P1 아이템 프로토타입 값 묶음
            public ItemSeed(string id, string fileName, string displayName, string description, ItemUseType useType, ItemEffectType effectType, float effectDuration, float primaryValue, float secondaryValue, float effectRange, float effectRadius, float cooldown, float projectileSpeed, Vector3 placementHalfExtents, Color color) // P1 아이템 프로토타입 값 생성
            { // P1 아이템 프로토타입 값 저장 처리
                Id = id; // 아이템 데이터 ID 저장
                FileName = fileName; // 에셋 파일 이름 저장
                DisplayName = displayName; // 한글 표시 이름 저장
                Description = description; // 아이템 설명 저장
                UseType = useType; // 공통 사용 방식 저장
                EffectType = effectType; // 실제 효과 종류 저장
                EffectDuration = effectDuration; // 효과 유지 시간 저장
                PrimaryValue = primaryValue; // 핵심 효과 수치 저장
                SecondaryValue = secondaryValue; // 보조 효과 수치 저장
                EffectRange = effectRange; // 효과 거리 저장
                EffectRadius = effectRadius; // 효과 반경 저장
                Cooldown = cooldown; // 반복 판정 간격 저장
                ProjectileSpeed = projectileSpeed; // 투사체 속도 저장
                PlacementHalfExtents = placementHalfExtents; // 설치 공간 절반 크기 저장
                Color = color; // 대표 색상 저장
            } // P1 아이템 프로토타입 값 저장 처리 종료

            public string Id { get; } // 아이템 데이터 ID 반환
            public string FileName { get; } // 에셋 파일 이름 반환
            public string DisplayName { get; } // 한글 표시 이름 반환
            public string Description { get; } // 아이템 설명 반환
            public ItemUseType UseType { get; } // 공통 사용 방식 반환
            public ItemEffectType EffectType { get; } // 실제 효과 종류 반환
            public float EffectDuration { get; } // 효과 유지 시간 반환
            public float PrimaryValue { get; } // 핵심 효과 수치 반환
            public float SecondaryValue { get; } // 보조 효과 수치 반환
            public float EffectRange { get; } // 효과 거리 반환
            public float EffectRadius { get; } // 효과 반경 반환
            public float Cooldown { get; } // 반복 판정 간격 반환
            public float ProjectileSpeed { get; } // 투사체 속도 반환
            public Vector3 PlacementHalfExtents { get; } // 설치 공간 절반 크기 반환
            public Color Color { get; } // 대표 색상 반환
        } // P1 아이템 프로토타입 값 묶음 종료
    } // 43일차 자동 설정 도구 묶음 종료
} // 프로젝트 Editor 기능 묶음 종료
