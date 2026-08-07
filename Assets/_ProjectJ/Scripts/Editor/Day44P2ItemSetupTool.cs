using ProjectJ.Data; // 프로젝트 아이템 데이터 형식 참조
using ProjectJ.Gameplay; // 프로젝트 경기 관리자 형식 참조
using ProjectJ.Items; // 프로젝트 P2 아이템 효과 기능 참조
using ProjectJ.Player; // 프로젝트 플레이어 기능 참조
using UnityEditor; // Unity Editor 에셋과 Undo 기능 참조
using UnityEditor.SceneManagement; // Unity Scene 변경 상태 기능 참조
using UnityEngine; // Unity 오브젝트와 색상과 경로 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 프로젝트 Editor 기능 묶음
    internal static class Day44P2ItemSetupTool // 44일차 P2 아이템 7종 자동 설정 도구 선언
    { // 44일차 자동 설정 도구 묶음
        private const string MenuPath = "Project J/Day 44/Configure P2 7 Item Effects"; // 44일차 자동 설정 메뉴 경로
        private const string ItemDataFolderPath = "Assets/_ProjectJ/Data/Definitions/Item"; // 아이템 데이터 저장 폴더 경로
        private const string CartRouteName = "Day44_CartRoute"; // 테스트용 카트 경로 루트 이름

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 44일차 자동 설정 항목 등록
        private static void ConfigureP2ItemEffects() // P2 7종 데이터와 플레이어 효과 시스템 구성
        { // 44일차 자동 설정 처리
            PlayerMovementController movementController = Object.FindFirstObjectByType<PlayerMovementController>(); // 현재 Scene 플레이어 이동 관리자 조회
            PrototypeMatchController matchController = Object.FindFirstObjectByType<PrototypeMatchController>(); // 현재 Scene 경기 관리자 조회

            if (movementController == null || matchController == null) // 필수 플레이어와 경기 관리자 참조 확인
            { // 필수 참조 누락 처리
                Debug.LogError("[ProjectJ][Day44] PlayerMovementController와 PrototypeMatchController 연결을 확인합니다. Game Scene과 Day 43 설정을 먼저 확인하세요."); // 누락 참조 오류 출력
                return; // 자동 설정 중단
            } // 필수 참조 누락 처리 종료

            CreateOrUpdateP2Definitions(CreateP2Seeds()); // P2 7종 ScriptableObject 데이터 갱신
            GameObject playerObject = movementController.gameObject; // 플레이어 구성 대상 오브젝트 저장
            PlayerItemInventory inventory = FindOrAddComponent<PlayerItemInventory>(playerObject); // 두 슬롯 인벤토리 조회 또는 추가
            PlayerItemUseController useController = FindOrAddComponent<PlayerItemUseController>(playerObject); // 아이템 실행 관리자 조회 또는 추가
            PlayerInputReader inputReader = FindOrAddComponent<PlayerInputReader>(playerObject); // 이동 입력 제공자 조회 또는 추가
            PlayerExternalForceController externalForceController = FindOrAddComponent<PlayerExternalForceController>(playerObject); // 외부 힘 관리자 조회 또는 추가
            PlayerRespawnController respawnController = FindOrAddComponent<PlayerRespawnController>(playerObject); // 부활 관리자 조회 또는 추가
            CharacterController characterController = FindOrAddComponent<CharacterController>(playerObject); // 플레이어 CharacterController 조회 또는 추가
            PlayerRewindRecorder rewindRecorder = FindOrAddComponent<PlayerRewindRecorder>(playerObject); // 최근 안전 위치 기록기 조회 또는 추가
            PlayerP2ItemEffectController p2EffectController = FindOrAddComponent<PlayerP2ItemEffectController>(playerObject); // P2 효과 관리자 조회 또는 추가
            PlayerSniperWaterGunController sniperController = FindOrAddComponent<PlayerSniperWaterGunController>(playerObject); // 저격 물총 조준 관리자 조회 또는 추가
            Transform visualRoot = playerObject.transform.Find("Visual"); // 플레이어 외형 루트 조회
            Camera aimCamera = Camera.main; // 저격 물총 기준 메인 카메라 조회
            rewindRecorder.ConfigureForEditor(movementController, respawnController, matchController, p2EffectController); // 되감기 기록기에 플레이어와 경기 참조 연결
            p2EffectController.ConfigureForEditor(movementController, externalForceController, respawnController, matchController, inputReader, rewindRecorder, characterController, visualRoot); // P2 효과에 이동과 부활과 외형 참조 연결
            sniperController.ConfigureForEditor(inventory, playerObject.GetComponent<PlayerStateController>(), respawnController, matchController, aimCamera); // 저격 조준에 슬롯과 상태와 카메라 참조 연결
            useController.ConfigureP2ForEditor(p2EffectController, sniperController); // 공통 아이템 실행기에 P2 관리자 연결
            CartPath cartPath = CreateOrReuseTestCartPath(playerObject.transform); // Scene 테스트용 카트 경로 생성 또는 재사용
            EditorUtility.SetDirty(rewindRecorder); // 되감기 기록기 변경 상태 저장
            EditorUtility.SetDirty(p2EffectController); // P2 효과 관리자 변경 상태 저장
            EditorUtility.SetDirty(sniperController); // 저격 조준 관리자 변경 상태 저장
            EditorUtility.SetDirty(useController); // 아이템 실행 관리자 변경 상태 저장
            EditorUtility.SetDirty(cartPath); // 카트 경로 변경 상태 저장
            ProjectDataCatalogBuilder.RebuildAndValidate(false); // 변경된 P2 데이터를 런타임 카탈로그에 등록
            EditorSceneManager.MarkSceneDirty(playerObject.scene); // 현재 Game Scene 저장 필요 상태 표시
            AssetDatabase.SaveAssets(); // P2 7종 데이터와 카탈로그 저장
            Selection.activeGameObject = playerObject; // 설정된 플레이어 오브젝트 선택
            EditorGUIUtility.PingObject(playerObject); // Hierarchy에서 플레이어 강조
            Debug.Log("[ProjectJ][Day44] P2 아이템 7종, 5초 충돌 없는 되감기, 추적 재선정 1회, 투명 추적 제외, 저격 조준, 카트 경로 구성을 완료했습니다. Ctrl + S로 Game Scene을 저장합니다.", playerObject); // 자동 설정 완료 로그 출력
        } // 44일차 자동 설정 처리 종료

        [MenuItem(MenuPath, true)] // 44일차 자동 설정 메뉴 활성 조건 등록
        private static bool ValidateConfigureP2ItemEffects() // Play Mode가 아닐 때만 자동 설정 허용
        { // 메뉴 실행 조건 검사 처리
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Edit Mode 실행 가능 여부 반환
        } // 메뉴 실행 조건 검사 처리 종료

        private static void CreateOrUpdateP2Definitions(ItemSeed[] seeds) // P2 7종 ScriptableObject 생성 또는 갱신
        { // P2 아이템 데이터 구성 처리
            for (int index = 0; index < seeds.Length; index++) // P2 7종 전체 순회
            { // 현재 P2 아이템 구성 처리
                ItemSeed seed = seeds[index]; // 현재 P2 프로토타입 데이터 조회
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
                itemDefinition.SetEditorIdentity(seed.Id, seed.DisplayName, new ProjectDataVersion(1, 3, 0)); // 아이템 ID와 한글 표시 이름과 44일차 버전 적용
                itemDefinition.ConfigureItemForEditor(seed.Description, existingIcon, seed.Color, existingVisualScale); // 설명과 기존 아이콘과 대표 색상 적용
                itemDefinition.ConfigureUsageForEditor(ItemImplementationPriority.P2, seed.UseType, seed.EffectType, 3f, 1, seed.EffectDuration, seed.PrimaryValue, seed.SecondaryValue, seed.EffectRange, seed.EffectRadius, seed.Cooldown, seed.ProjectileSpeed, Vector3.one * 0.5f); // P2 사용 방식과 프로토타입 수치 적용
                EditorUtility.SetDirty(itemDefinition); // 현재 P2 아이템 데이터 변경 상태 표시
            } // 현재 P2 아이템 구성 처리 종료
        } // P2 아이템 데이터 구성 처리 종료

        private static CartPath CreateOrReuseTestCartPath(Transform playerTransform) // Scene 테스트용 카트 경로 생성 또는 재사용
        { // 테스트 카트 경로 구성 처리
            GameObject routeObject = GameObject.Find(CartRouteName); // 기존 테스트 카트 경로 루트 조회

            if (routeObject == null) // 기존 테스트 경로 누락 여부 확인
            { // 새 테스트 경로 생성 처리
                routeObject = new GameObject(CartRouteName); // 테스트 카트 경로 루트 생성
                Undo.RegisterCreatedObjectUndo(routeObject, "Create Day 44 Cart Route"); // 경로 생성 Undo 등록
            } // 새 테스트 경로 생성 처리 종료

            CartPath cartPath = FindOrAddComponent<CartPath>(routeObject); // 카트 경로 컴포넌트 조회 또는 추가
            Transform[] waypoints = GetExistingWaypoints(routeObject.transform); // 기존 자식 경로 지점 조회

            if (waypoints.Length < 2) // 주행 가능한 기존 경로 지점 수 확인
            { // 기본 직선 테스트 경로 생성 처리
                waypoints = new Transform[5]; // 시작과 네 이동 지점 배열 생성
                Vector3 basePosition = playerTransform.position; // 현재 플레이어 위치를 경로 시작점으로 저장
                Vector3 forward = Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up).normalized; // 플레이어 수평 전방 방향 계산
                forward = forward.sqrMagnitude <= 0.0001f ? Vector3.forward : forward; // 전방 누락 시 세계 Z 방향 적용

                for (int index = 0; index < waypoints.Length; index++) // 다섯 기본 경로 지점 순회
                { // 현재 기본 경로 지점 생성 처리
                    GameObject waypointObject = new GameObject($"Waypoint_{index:00}"); // 번호 기반 경로 지점 오브젝트 생성
                    Undo.RegisterCreatedObjectUndo(waypointObject, "Create Day 44 Cart Waypoint"); // 경로 지점 생성 Undo 등록
                    waypointObject.transform.SetParent(routeObject.transform); // 테스트 경로 루트 아래 배치
                    waypointObject.transform.position = basePosition + forward * index * 4f; // 플레이어 앞쪽 4m 간격 위치 적용
                    waypoints[index] = waypointObject.transform; // 새 경로 지점 배열에 저장
                } // 현재 기본 경로 지점 생성 처리 종료
            } // 기본 직선 테스트 경로 생성 처리 종료

            cartPath.ConfigureForEditor(waypoints); // 카트 경로에 자식 지점 순서 연결
            return cartPath; // 구성된 테스트 카트 경로 반환
        } // 테스트 카트 경로 구성 처리 종료

        private static Transform[] GetExistingWaypoints(Transform routeTransform) // 기존 경로 루트의 자식 지점 배열 생성
        { // 기존 경로 지점 조회 처리
            Transform[] waypoints = new Transform[routeTransform.childCount]; // 현재 자식 수 기반 경로 지점 배열 생성

            for (int index = 0; index < routeTransform.childCount; index++) // 모든 자식 Transform 순회
            { // 현재 자식 경로 지점 저장 처리
                waypoints[index] = routeTransform.GetChild(index); // Hierarchy 순서 그대로 경로 지점 저장
            } // 현재 자식 경로 지점 저장 처리 종료

            return waypoints; // 기존 경로 지점 배열 반환
        } // 기존 경로 지점 조회 처리 종료

        private static T FindOrAddComponent<T>(GameObject targetObject) where T : Component // 대상 오브젝트 컴포넌트 조회 또는 추가
        { // 컴포넌트 조회 또는 추가 처리
            T component = targetObject.GetComponent<T>(); // 기존 대상 컴포넌트 조회

            if (component != null) // 기존 컴포넌트 존재 여부 확인
            { // 기존 컴포넌트 재사용 처리
                return component; // 기존 컴포넌트 반환
            } // 기존 컴포넌트 재사용 처리 종료

            return Undo.AddComponent<T>(targetObject); // Undo 가능한 새 컴포넌트 추가 후 반환
        } // 컴포넌트 조회 또는 추가 처리 종료

        private static ItemSeed[] CreateP2Seeds() // P2 7종 프로토타입 기본 수치 생성
        { // P2 프로토타입 데이터 생성 처리
            return new[] // P2 7종 프로토타입 배열 반환
            { // P2 7종 프로토타입 묶음
                new ItemSeed("ITM-022", "RewindClock", "되감기 시계", "최근 5초의 안전 위치를 1.25초 동안 충돌 없이 역재생", ItemUseType.Instant, ItemEffectType.RewindClock, 5f, 1.25f, 0f, 0f, 0f, 0f, 0f, new Color(0.75f, 0.55f, 1f, 1f)), // P2 되감기 시계
                new ItemSeed("ITM-023", "HomingMissile", "유도탄", "가장 가까운 대상을 초당 16m로 추적해 힘 12로 밀치며 한 번 재선정", ItemUseType.Projectile, ItemEffectType.HomingMissile, 8f, 12f, 1f, 60f, 0.35f, 0f, 16f, new Color(1f, 0.25f, 0.15f, 1f)), // P2 유도탄
                new ItemSeed("ITM-024", "MiniaturePotion", "소형화 물약", "6초 동안 외형과 충돌체를 80퍼센트 크기로 축소", ItemUseType.Duration, ItemEffectType.MiniaturePotion, 6f, 0.8f, 0f, 0f, 0f, 0f, 0f, new Color(0.6f, 0.9f, 0.35f, 1f)), // P2 소형화 물약
                new ItemSeed("ITM-025", "Drone", "드론", "현재 1위 대상을 초당 12m로 추적해 힘 10으로 밀치며 한 번 재선정", ItemUseType.Duration, ItemEffectType.Drone, 12f, 10f, 1f, 60f, 0.5f, 0f, 12f, new Color(0.45f, 0.55f, 0.65f, 1f)), // P2 드론
                new ItemSeed("ITM-026", "InvisibilityCloak", "투명 망토", "5초 동안 외형을 숨기고 유도탄과 드론의 추적 대상에서 제외", ItemUseType.Duration, ItemEffectType.InvisibilityCloak, 5f, 0.2f, 0f, 0f, 0f, 0f, 0f, new Color(0.65f, 0.65f, 0.8f, 0.65f)), // P2 투명 망토
                new ItemSeed("ITM-027", "SniperWaterGun", "저격 물총", "최대 50m를 조준해 힘 14로 밀치고 1.5배부터 4배까지 배율 변경", ItemUseType.Instant, ItemEffectType.SniperWaterGun, 0f, 14f, 0f, 50f, 0f, 0f, 0f, new Color(0.1f, 0.55f, 0.95f, 1f)), // P2 저격 물총
                new ItemSeed("ITM-028", "Cart", "카트", "가까운 연결 경로를 초당 10m 속도로 최대 8초 자동 주행", ItemUseType.Duration, ItemEffectType.Cart, 8f, 10f, 0f, 4f, 0f, 0f, 0f, new Color(0.85f, 0.25f, 0.2f, 1f)) // P2 카트
            }; // P2 7종 프로토타입 묶음 종료
        } // P2 프로토타입 데이터 생성 처리 종료

        private readonly struct ItemSeed // P2 아이템 프로토타입 기본값 묶음 선언
        { // P2 아이템 프로토타입 값 묶음
            public ItemSeed(string id, string fileName, string displayName, string description, ItemUseType useType, ItemEffectType effectType, float effectDuration, float primaryValue, float secondaryValue, float effectRange, float effectRadius, float cooldown, float projectileSpeed, Color color) // P2 아이템 프로토타입 값 생성
            { // P2 아이템 프로토타입 값 저장 처리
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
                Color = color; // 대표 색상 저장
            } // P2 아이템 프로토타입 값 저장 처리 종료

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
            public Color Color { get; } // 대표 색상 반환
        } // P2 아이템 프로토타입 값 묶음 종료
    } // 44일차 자동 설정 도구 묶음 종료
} // 프로젝트 Editor 기능 묶음 종료
