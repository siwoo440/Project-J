using ProjectJ.Data; // 프로젝트 아이템 데이터 형식 참조
using ProjectJ.Gameplay; // 프로젝트 경기 관리자 형식 참조
using ProjectJ.Items; // 프로젝트 상자와 설치 위치 검사 기능 참조
using ProjectJ.MapGeneration; // 프로젝트 절차 맵 생성기 형식 참조
using UnityEditor; // Unity Editor Undo와 에셋 기능 참조
using UnityEditor.SceneManagement; // Unity Scene 변경 상태 기능 참조
using UnityEngine; // Unity 오브젝트와 Layer 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 프로젝트 Editor 기능 묶음
    internal static class Day41ItemChestPlacementSetupTool // 41일차 상자와 설치 검사 자동 설정 도구 선언
    { // 41일차 자동 설정 도구 묶음
        private const string MenuPath = ProjectJEditorMenuPaths.ItemChests + "/아이템 상자와 설치 위치 검사 구성 (Day 41일차)"; // 41일차 자동 설정 메뉴 경로
        private const string SystemRootName = "Day41_ItemChestSystem"; // 상자 시스템 루트 오브젝트 이름
        private const string Day39ChestRootName = "Day39_ItemChests"; // 제거할 이전 테스트 상자 루트 이름

        private static readonly string[] ItemAssetPaths = // 상자 지급 아이템 에셋 경로 목록 선언
        { // 상자 지급 아이템 에셋 경로 묶음
            "Assets/_ProjectJ/Data/Definitions/Item/ITM-001_SpringShoes.asset", // 스프링 신발 데이터 경로
            "Assets/_ProjectJ/Data/Definitions/Item/ITM-002_JellyShield.asset", // 젤리 보호막 데이터 경로
            "Assets/_ProjectJ/Data/Definitions/Item/ITM-003_BananaCushion.asset" // 바나나 쿠션 데이터 경로
        }; // 상자 지급 아이템 에셋 경로 묶음 종료

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 41일차 자동 설정 항목 등록
        private static void ConfigureItemChestsAndPlacementValidator() // 상자 생성기와 공통 설치 위치 검사기 구성
        { // 41일차 자동 설정 처리
            ProceduralMapGenerator mapGenerator = Object.FindFirstObjectByType<ProceduralMapGenerator>(); // 현재 Scene 절차 맵 생성기 조회
            PrototypeMatchController matchController = Object.FindFirstObjectByType<PrototypeMatchController>(); // 현재 Scene 경기 관리자 조회

            if (mapGenerator == null) // 절차 맵 생성기 누락 여부 확인
            { // 맵 생성기 누락 처리
                Debug.LogError("[ProjectJ][Day41] ProceduralMapGenerator를 찾을 수 없습니다. Game Scene을 열고 다시 실행하세요."); // 맵 생성기 누락 오류 출력
                return; // 자동 설정 중단
            } // 맵 생성기 누락 처리 종료

            if (matchController == null) // 경기 관리자 누락 여부 확인
            { // 경기 관리자 누락 처리
                Debug.LogError("[ProjectJ][Day41] PrototypeMatchController를 찾을 수 없습니다. Game Scene 구성을 확인하세요."); // 경기 관리자 누락 오류 출력
                return; // 자동 설정 중단
            } // 경기 관리자 누락 처리 종료

            ItemDataDefinition[] itemPool = LoadItemPool(); // 39일차 아이템 데이터 세 개 조회

            if (itemPool == null) // 아이템 후보 누락 여부 확인
            { // 아이템 후보 누락 처리
                return; // 자동 설정 중단
            } // 아이템 후보 누락 처리 종료

            RemoveDay39TestChests(); // 플레이어 앞 고정 테스트 상자 제거
            GameObject systemRoot = RecreateSystemRoot(); // 41일차 상자 시스템 루트 새로 구성
            ItemPlacementValidator placementValidator = Undo.AddComponent<ItemPlacementValidator>(systemRoot); // 공통 설치 위치 검사기 추가
            LayerMask allLayersMask = ~0; // 모든 물리 Layer 검사 마스크 선언
            placementValidator.ConfigureForEditor(allLayersMask, allLayersMask, 3f, 8f, 35f, 0.03f); // 지면과 경사와 장애물 기본 검사값 적용
            ItemChestSpawner chestSpawner = Undo.AddComponent<ItemChestSpawner>(systemRoot); // 절차 맵 상자 생성기 추가
            chestSpawner.ConfigureForEditor(mapGenerator, placementValidator, matchController, itemPool, 0.35f, 4, 2, 8, 0.9f, new Vector3(0.6f, 0.6f, 0.6f), 20f, 1); // 41일차 상자 생성과 재생성 기본 규칙 적용
            EditorUtility.SetDirty(placementValidator); // 공통 검사기 변경 상태 표시
            EditorUtility.SetDirty(chestSpawner); // 상자 생성기 변경 상태 표시
            EditorSceneManager.MarkSceneDirty(mapGenerator.gameObject.scene); // 현재 Game Scene 저장 필요 상태 표시
            Selection.activeGameObject = systemRoot; // 새 상자 시스템 루트 선택
            EditorGUIUtility.PingObject(systemRoot); // Hierarchy에서 상자 시스템 강조
            Debug.Log("[ProjectJ][Day41] 모듈별 35% 확률, 최대 4개, 20초 뒤 1회 재생성과 공통 설치 위치 검사 구성을 완료했습니다. Ctrl + S로 Game Scene을 저장합니다.", systemRoot); // 자동 설정 완료 로그 출력
        } // 41일차 자동 설정 처리 종료

        [MenuItem(MenuPath, true)] // 41일차 자동 설정 메뉴 활성 조건 등록
        private static bool ValidateConfigureItemChestsAndPlacementValidator() // Play Mode가 아닐 때만 자동 설정 허용
        { // 메뉴 실행 조건 검사 처리
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Edit Mode 실행 가능 여부 반환
        } // 메뉴 실행 조건 검사 처리 종료

        private static ItemDataDefinition[] LoadItemPool() // 39일차 아이템 데이터 세 개 조회
        { // 아이템 데이터 조회 처리
            ItemDataDefinition[] itemPool = new ItemDataDefinition[ItemAssetPaths.Length]; // 아이템 후보 결과 배열 생성

            for (int itemIndex = 0; itemIndex < ItemAssetPaths.Length; itemIndex++) // 모든 아이템 에셋 경로 순회
            { // 현재 아이템 데이터 조회 처리
                ItemDataDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<ItemDataDefinition>(ItemAssetPaths[itemIndex]); // 현재 경로 아이템 데이터 로드

                if (itemDefinition == null) // 현재 아이템 데이터 누락 여부 확인
                { // 아이템 데이터 누락 처리
                    Debug.LogError($"[ProjectJ][Day41] 아이템 데이터를 찾을 수 없습니다: {ItemAssetPaths[itemIndex]} | 먼저 Day 39 자동 설정을 실행하세요."); // 누락 아이템 경로와 선행 작업 출력
                    return null; // 아이템 후보 조회 실패 반환
                } // 아이템 데이터 누락 처리 종료

                itemPool[itemIndex] = itemDefinition; // 유효 아이템 데이터 결과 저장
            } // 현재 아이템 데이터 조회 처리 종료

            return itemPool; // 세 아이템 후보 목록 반환
        } // 아이템 데이터 조회 처리 종료

        private static void RemoveDay39TestChests() // 이전 플레이어 앞 고정 테스트 상자 제거
        { // 39일차 테스트 상자 제거 처리
            GameObject day39ChestRoot = GameObject.Find(Day39ChestRootName); // 정확한 이름의 이전 테스트 상자 루트 검색

            if (day39ChestRoot != null) // 이전 테스트 상자 루트 존재 여부 확인
            { // 이전 테스트 상자 제거 처리
                Undo.DestroyObjectImmediate(day39ChestRoot); // 고정 테스트 상자 루트와 세 상자 제거
            } // 이전 테스트 상자 제거 처리 종료
        } // 39일차 테스트 상자 제거 처리 종료

        private static GameObject RecreateSystemRoot() // 41일차 상자 시스템 루트 새로 구성
        { // 상자 시스템 루트 교체 처리
            GameObject existingRoot = GameObject.Find(SystemRootName); // 정확한 이름의 기존 41일차 루트 검색

            if (existingRoot != null) // 기존 상자 시스템 루트 존재 여부 확인
            { // 기존 상자 시스템 교체 처리
                Undo.DestroyObjectImmediate(existingRoot); // 기존 자동 생성 루트만 제거
            } // 기존 상자 시스템 교체 처리 종료

            GameObject systemRoot = new GameObject(SystemRootName); // 새 상자 시스템 루트 생성
            Undo.RegisterCreatedObjectUndo(systemRoot, "Create Day 41 Item Chest System"); // 상자 시스템 루트 생성 Undo 등록
            return systemRoot; // 새 상자 시스템 루트 반환
        } // 상자 시스템 루트 교체 처리 종료
    } // 41일차 자동 설정 도구 묶음 종료
} // 프로젝트 Editor 기능 묶음 종료
