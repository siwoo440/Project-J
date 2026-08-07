using System; // 기본 문자열 비교와 예외 기능 참조
using System.Collections.Generic; // 이동 계획과 오류 목록 기능 참조
using System.IO; // 스크립트 파일명과 소스 읽기 기능 참조
using UnityEditor; // Unity AssetDatabase와 Editor 메뉴 기능 참조
using UnityEngine; // Unity Console과 대화상자 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스
{ // 48일차 Editor·Tests 폴더 통합 도구 정의
    internal static class Day48EditorTestsFolderStructureTool // Editor·EditMode·PlayMode 기능별 폴더 이동 도구 선언
    { // 세 영역의 안전 이동과 최종 검증 기능 정의
        private const string EditorRootPath = "Assets/_ProjectJ/Scripts/Editor"; // Editor 스크립트 루트 경로
        private const string EditModeRootPath = "Assets/_ProjectJ/Tests/EditMode"; // EditMode 테스트 루트 경로
        private const string PlayModeRootPath = "Assets/_ProjectJ/Tests/PlayMode"; // PlayMode 테스트 루트 경로
        private const string EditorAsmdefPath = EditorRootPath + "/ProjectJ.Editor.asmdef"; // Editor Assembly Definition 고정 경로
        private const string EditModeAsmdefPath = EditModeRootPath + "/ProjectJ.Tests.EditMode.asmdef"; // EditMode Assembly Definition 고정 경로
        private const string PlayModeAsmdefPath = PlayModeRootPath + "/ProjectJ.Tests.PlayMode.asmdef"; // PlayMode Assembly Definition 고정 경로
        private const string MenuPathsTargetPath = EditorRootPath + "/ProjectManagement/Menu/ProjectJEditorMenuPaths.cs"; // 46일차 공통 메뉴 경로 파일 최종 위치
        private const string Day48ToolPath = EditorRootPath + "/ProjectManagement/Structure/Day48EditorTestsFolderStructureTool.cs"; // 48일차 이동 도구 자체 최종 위치
        private const string PreviewMenuPath = ProjectJEditorMenuPaths.TestFramework + "/48일차 Editor·Tests 폴더 정리/00. 전체 이동 계획 미리보기 (Day 48일차)"; // 이동 계획 미리보기 메뉴 경로
        private const string EditorMenuPath = ProjectJEditorMenuPaths.TestFramework + "/48일차 Editor·Tests 폴더 정리/01. Editor 스크립트 기능별 폴더 통합 (Day 48일차)"; // Editor 이동 실행 메뉴 경로
        private const string EditModeMenuPath = ProjectJEditorMenuPaths.TestFramework + "/48일차 Editor·Tests 폴더 정리/02. EditMode 테스트 기능별 폴더 통합 (Day 48일차)"; // EditMode 이동 실행 메뉴 경로
        private const string PlayModeMenuPath = ProjectJEditorMenuPaths.TestFramework + "/48일차 Editor·Tests 폴더 정리/03. PlayMode 테스트 기능별 폴더 통합 (Day 48일차)"; // PlayMode 이동 실행 메뉴 경로
        private const string ValidateMenuPath = ProjectJEditorMenuPaths.TestFramework + "/48일차 Editor·Tests 폴더 정리/04. 전체 Editor·Tests 폴더 구조 검증 (Day 48일차)"; // 전체 구조 검증 메뉴 경로

        [MenuItem(PreviewMenuPath)] // 48일차 이동 계획 미리보기 메뉴 등록
        private static void PreviewAllMovePlans() // 현재 저장소의 Editor·Tests 이동 계획 미리보기
        { // 세 영역의 실제 이동 대상과 기능 분류 Console 출력
            List<string> errors = new List<string>(); // 미리보기 중 발견된 경로 오류 목록 생성
            List<MovePlan> editorPlans = CollectEditorMovePlans(errors); // Editor 이동 계획 수집
            List<MovePlan> editModePlans = CollectTestMovePlans(EditModeRootPath, errors); // EditMode 이동 계획 수집
            List<MovePlan> playModePlans = CollectTestMovePlans(PlayModeRootPath, errors); // PlayMode 이동 계획 수집
            LogMovePlans("Editor", editorPlans); // Editor 이동 계획 상세 출력
            LogMovePlans("EditMode", editModePlans); // EditMode 이동 계획 상세 출력
            LogMovePlans("PlayMode", playModePlans); // PlayMode 이동 계획 상세 출력

            if (errors.Count > 0) // 미리보기 경로 오류 존재 여부 확인
            { // 오류가 있는 이동 계획 안내
                LogErrors(errors); // 발견된 오류 전체 Console 출력
                EditorUtility.DisplayDialog("Project J Day 48", $"이동 계획에서 오류 {errors.Count}개를 발견했습니다.\n실제 이동은 수행하지 않았습니다.\nConsole을 확인합니다.", "확인"); // 미리보기 실패 안내
                return; // 실제 변경 없이 미리보기 종료
            } // 오류가 있는 이동 계획 안내 종료

            Debug.Log($"[ProjectJ][Day48] 이동 계획 미리보기 완료 | Editor {editorPlans.Count}개 | EditMode {editModePlans.Count}개 | PlayMode {playModePlans.Count}개"); // 전체 이동 계획 요약 로그
            EditorUtility.DisplayDialog("Project J Day 48", $"이동 계획 미리보기 완료\n\nEditor: {editorPlans.Count}개\nEditMode: {editModePlans.Count}개\nPlayMode: {playModePlans.Count}개\n\nConsole에서 개별 경로를 확인합니다.", "확인"); // 미리보기 성공 안내
        } // 현재 저장소의 Editor·Tests 이동 계획 미리보기 종료

        [MenuItem(EditorMenuPath)] // 48일차 Editor 이동 메뉴 등록
        private static void ApplyEditorFolderIntegration() // Editor 스크립트 기능별 폴더 이동
        { // Editor 루트와 기존 하위 폴더의 C# 도구 정리
            List<string> errors = new List<string>(); // Editor 이동 사전 검증 오류 목록 생성
            List<MovePlan> plans = CollectEditorMovePlans(errors); // 현재 Editor 이동 계획 수집
            ApplyMovePlans("Editor", plans, errors); // Editor 이동 계획 안전 적용
        } // Editor 스크립트 기능별 폴더 이동 종료

        [MenuItem(EditModeMenuPath)] // 48일차 EditMode 이동 메뉴 등록
        private static void ApplyEditModeFolderIntegration() // EditMode 테스트 기능별 폴더 이동
        { // EditMode asmdef를 루트에 유지한 테스트 정리
            List<string> errors = new List<string>(); // EditMode 이동 사전 검증 오류 목록 생성
            List<MovePlan> plans = CollectTestMovePlans(EditModeRootPath, errors); // 현재 EditMode 이동 계획 수집
            ApplyMovePlans("EditMode", plans, errors); // EditMode 이동 계획 안전 적용
        } // EditMode 테스트 기능별 폴더 이동 종료

        [MenuItem(PlayModeMenuPath)] // 48일차 PlayMode 이동 메뉴 등록
        private static void ApplyPlayModeFolderIntegration() // PlayMode 테스트 기능별 폴더 이동
        { // PlayMode asmdef를 루트에 유지한 테스트 정리
            List<string> errors = new List<string>(); // PlayMode 이동 사전 검증 오류 목록 생성
            List<MovePlan> plans = CollectTestMovePlans(PlayModeRootPath, errors); // 현재 PlayMode 이동 계획 수집
            ApplyMovePlans("PlayMode", plans, errors); // PlayMode 이동 계획 안전 적용
        } // PlayMode 테스트 기능별 폴더 이동 종료

        [MenuItem(ValidateMenuPath)] // 48일차 전체 구조 검증 메뉴 등록
        private static void ValidateAllFolderStructures() // Editor·EditMode·PlayMode 기능별 폴더 구조 최종 검증
        { // 루트 스크립트·asmdef·공통 메뉴 파일 상태 검사
            List<string> errors = new List<string>(); // 최종 구조 검증 오류 목록 생성
            ValidateNoRootLevelScripts(EditorRootPath, errors); // Editor 루트 C# 잔존 여부 검증
            ValidateNoRootLevelScripts(EditModeRootPath, errors); // EditMode 루트 C# 잔존 여부 검증
            ValidateNoRootLevelScripts(PlayModeRootPath, errors); // PlayMode 루트 C# 잔존 여부 검증
            ValidateSingleAssemblyDefinition(EditorRootPath, EditorAsmdefPath, errors); // Editor asmdef 단일 구조 검증
            ValidateSingleAssemblyDefinition(EditModeRootPath, EditModeAsmdefPath, errors); // EditMode asmdef 단일 구조 검증
            ValidateSingleAssemblyDefinition(PlayModeRootPath, PlayModeAsmdefPath, errors); // PlayMode asmdef 단일 구조 검증
            ValidateRequiredAsset(MenuPathsTargetPath, errors); // 공통 메뉴 경로 파일 최종 위치 검증
            ValidateRequiredAsset(Day48ToolPath, errors); // 48일차 구조 도구 최종 위치 검증

            if (errors.Count > 0) // 최종 구조 오류 존재 여부 확인
            { // 최종 검증 실패 처리
                LogErrors(errors); // 전체 검증 오류 Console 출력
                EditorUtility.DisplayDialog("Project J Day 48", $"Editor·Tests 폴더 구조 검증 실패\n\n오류: {errors.Count}개\nConsole을 확인합니다.", "확인"); // 최종 검증 실패 안내
                return; // 검증 실패 상태로 종료
            } // 최종 검증 실패 처리 종료

            Debug.Log("[ProjectJ][Day48] Editor·Tests 기능별 폴더 구조 검증 완료"); // 최종 구조 검증 성공 로그
            EditorUtility.DisplayDialog("Project J Day 48", "Editor·EditMode·PlayMode 기능별 폴더 구조 검증 완료", "확인"); // 최종 구조 검증 성공 안내
        } // Editor·EditMode·PlayMode 기능별 폴더 구조 최종 검증 종료

        private static List<MovePlan> CollectEditorMovePlans(List<string> errors) // 현재 Editor 스크립트의 기능별 이동 계획 수집
        { // 메뉴 분류와 기존 유틸리티 경로를 이용한 목적 폴더 결정
            List<MovePlan> plans = new List<MovePlan>(); // Editor 이동 계획 목록 생성
            string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { EditorRootPath }); // Editor 하위 MonoScript 전체 검색

            for (int index = 0; index < guids.Length; index++) // 검색된 Editor 스크립트 전체 순회
            { // 현재 C# 스크립트 기능 분류 처리
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[index]); // 현재 Editor 스크립트 경로 조회

                if (!IsCSharpAsset(assetPath)) // 실제 C# 파일 여부 확인
                { // 비 C# 검색 결과 처리
                    continue; // 현재 검색 결과 생략
                } // 비 C# 검색 결과 처리 종료

                string source = File.ReadAllText(assetPath); // 현재 Editor 소스 전체 읽기
                string destinationFolder = GetEditorDestinationFolder(assetPath, source); // 메뉴와 파일 역할 기준 최종 폴더 결정
                AddMovePlanIfNeeded(assetPath, destinationFolder, plans, errors); // 현재 스크립트 이동 필요 여부와 충돌 검사
            } // 검색된 Editor 스크립트 전체 순회 종료

            return plans; // Editor 이동 계획 목록 반환
        } // 현재 Editor 스크립트의 기능별 이동 계획 수집 종료

        private static List<MovePlan> CollectTestMovePlans(string testRootPath, List<string> errors) // 현재 테스트 스크립트의 기능별 이동 계획 수집
        { // 테스트 파일명과 소스 키워드를 이용한 기능 폴더 결정
            List<MovePlan> plans = new List<MovePlan>(); // 테스트 이동 계획 목록 생성
            string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { testRootPath }); // 지정 테스트 루트 MonoScript 전체 검색

            for (int index = 0; index < guids.Length; index++) // 검색된 테스트 스크립트 전체 순회
            { // 현재 테스트 파일 기능 분류 처리
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[index]); // 현재 테스트 스크립트 경로 조회

                if (!IsCSharpAsset(assetPath)) // 실제 C# 테스트 파일 여부 확인
                { // 비 C# 검색 결과 처리
                    continue; // 현재 검색 결과 생략
                } // 비 C# 검색 결과 처리 종료

                string source = File.ReadAllText(assetPath); // 현재 테스트 소스 전체 읽기
                string destinationFolder = GetTestDestinationFolder(testRootPath, assetPath, source); // 테스트 대상 기능 기준 최종 폴더 결정
                AddMovePlanIfNeeded(assetPath, destinationFolder, plans, errors); // 현재 테스트 이동 필요 여부와 충돌 검사
            } // 검색된 테스트 스크립트 전체 순회 종료

            return plans; // 테스트 이동 계획 목록 반환
        } // 현재 테스트 스크립트의 기능별 이동 계획 수집 종료

        private static string GetEditorDestinationFolder(string assetPath, string source) // Editor 스크립트의 최종 기능 폴더 결정
        { // 46일차 메뉴 분류와 관리 도구 역할 기준 분류
            string fileName = Path.GetFileName(assetPath); // 현재 Editor 파일명 추출

            if (string.Equals(fileName, "ProjectJEditorMenuPaths.cs", StringComparison.Ordinal)) // 공통 메뉴 경로 파일 여부 확인
            { // 프로젝트 관리 메뉴 폴더 분류
                return EditorRootPath + "/ProjectManagement/Menu"; // 공통 메뉴 정의 최종 폴더 반환
            } // 공통 메뉴 경로 파일 분류 종료

            if (string.Equals(fileName, "Day47RuntimeDataFolderStructureTool.cs", StringComparison.Ordinal) || string.Equals(fileName, "Day48EditorTestsFolderStructureTool.cs", StringComparison.Ordinal)) // 구조 관리 도구 여부 확인
            { // 프로젝트 관리 구조 폴더 분류
                return EditorRootPath + "/ProjectManagement/Structure"; // 구조 정리 도구 최종 폴더 반환
            } // 구조 관리 도구 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.ProjectSettingsScenes")) // 씬 설정 메뉴 사용 여부 확인
            { // 프로젝트 설정 씬 폴더 분류
                return EditorRootPath + "/ProjectSettings/Scenes"; // 씬 설정 Editor 최종 폴더 반환
            } // 프로젝트 설정 씬 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.ProjectSettingsServices")) // 서비스 설정 메뉴 사용 여부 확인
            { // 프로젝트 설정 서비스 폴더 분류
                return EditorRootPath + "/ProjectSettings/Services"; // 서비스 설정 Editor 최종 폴더 반환
            } // 프로젝트 설정 서비스 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.ProjectSettingsPhysics") || ContainsOrdinal(assetPath, "/Physics/")) // 물리 설정 또는 기존 Physics 유틸리티 여부 확인
            { // 프로젝트 설정 물리 폴더 분류
                return EditorRootPath + "/ProjectSettings/Physics"; // 물리 설정 Editor 최종 폴더 반환
            } // 프로젝트 설정 물리 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.PlayerInput") || ContainsOrdinal(source, "ProjectJEditorMenuPaths.PlayerPlayMode") || ContainsOrdinal(source, "ProjectJEditorMenuPaths.PlayerSettings")) // 플레이어와 입력 메뉴 사용 여부 확인
            { // 플레이어 Editor 폴더 분류
                return EditorRootPath + "/Player"; // 플레이어와 입력 Editor 최종 폴더 반환
            } // 플레이어 Editor 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.DataCsv") || string.Equals(fileName, "ProjectDataCsvImporter.cs", StringComparison.Ordinal)) // CSV Editor 도구 여부 확인
            { // 데이터 CSV 폴더 분류
                return EditorRootPath + "/Data/CSV"; // CSV Editor 최종 폴더 반환
            } // 데이터 CSV 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.DataCatalog") || string.Equals(fileName, "ProjectDataCatalogBuilder.cs", StringComparison.Ordinal)) // 카탈로그 Editor 도구 여부 확인
            { // 데이터 카탈로그 폴더 분류
                return EditorRootPath + "/Data/Catalog"; // 데이터 카탈로그 Editor 최종 폴더 반환
            } // 데이터 카탈로그 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.DataBase") || ContainsOrdinal(assetPath, "/Data/")) // 기본 데이터 Editor 도구 여부 확인
            { // 데이터 설정 폴더 분류
                return EditorRootPath + "/Data/Setup"; // 기본 데이터 Editor 최종 폴더 반환
            } // 데이터 설정 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.TestFramework")) // 테스트 프레임워크 메뉴 사용 여부 확인
            { // 테스트 관리 Editor 폴더 분류
                return EditorRootPath + "/Testing"; // 테스트 프레임워크 Editor 최종 폴더 반환
            } // 테스트 관리 Editor 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.DevelopmentBuild") || ContainsOrdinal(assetPath, "/Build/") || ContainsOrdinal(fileName, "Build")) // 빌드 Editor 도구 여부 확인
            { // 빌드 Editor 폴더 분류
                return EditorRootPath + "/Build"; // 빌드 Editor 최종 폴더 반환
            } // 빌드 Editor 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.MapModules")) // 맵 모듈 메뉴 사용 여부 확인
            { // 맵 모듈 Editor 폴더 분류
                return EditorRootPath + "/Map/Modules"; // 맵 모듈 Editor 최종 폴더 반환
            } // 맵 모듈 Editor 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.MapGeneration")) // 맵 생성 메뉴 사용 여부 확인
            { // 맵 생성 Editor 폴더 분류
                return EditorRootPath + "/Map/Generation"; // 맵 생성 Editor 최종 폴더 반환
            } // 맵 생성 Editor 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.MapValidation")) // 맵 검증 메뉴 사용 여부 확인
            { // 맵 검증 Editor 폴더 분류
                return EditorRootPath + "/Map/Validation"; // 맵 검증 Editor 최종 폴더 반환
            } // 맵 검증 Editor 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.MapObstacles")) // 맵 장애물 메뉴 사용 여부 확인
            { // 맵 장애물 Editor 폴더 분류
                return EditorRootPath + "/Map/Obstacles"; // 맵 장애물 Editor 최종 폴더 반환
            } // 맵 장애물 Editor 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.ItemInventory")) // 아이템 인벤토리 메뉴 사용 여부 확인
            { // 아이템 인벤토리 Editor 폴더 분류
                return EditorRootPath + "/Items/Inventory"; // 인벤토리 Editor 최종 폴더 반환
            } // 아이템 인벤토리 Editor 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.ItemChests")) // 아이템 상자 메뉴 사용 여부 확인
            { // 아이템 상자 Editor 폴더 분류
                return EditorRootPath + "/Items/Chests"; // 아이템 상자 Editor 최종 폴더 반환
            } // 아이템 상자 Editor 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.ItemEffects")) // 아이템 효과 메뉴 사용 여부 확인
            { // 아이템 효과 Editor 폴더 분류
                return EditorRootPath + "/Items/Effects"; // 아이템 효과 Editor 최종 폴더 반환
            } // 아이템 효과 Editor 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.ItemValidation")) // 아이템 통합 검증 메뉴 사용 여부 확인
            { // 아이템 검증 Editor 폴더 분류
                return EditorRootPath + "/Items/Validation"; // 아이템 검증 Editor 최종 폴더 반환
            } // 아이템 통합 검증 Editor 분류 종료

            if (ContainsOrdinal(source, "ProjectJEditorMenuPaths.GameUI")) // 게임 UI 메뉴 사용 여부 확인
            { // UI Editor 폴더 분류
                return EditorRootPath + "/UI"; // UI Editor 최종 폴더 반환
            } // UI Editor 분류 종료

            return EditorRootPath + "/Common"; // 명확한 기능 메뉴가 없는 Editor 공통 폴더 반환
        } // Editor 스크립트의 최종 기능 폴더 결정 종료

        private static string GetTestDestinationFolder(string testRootPath, string assetPath, string source) // 테스트 스크립트의 최종 기능 폴더 결정
        { // 파일명을 우선 사용하고 소스 네임스페이스는 보조 기준으로 사용하는 분류
            string fileName = Path.GetFileName(assetPath).ToLowerInvariant(); // 대소문자 무시용 테스트 파일명 생성
            string sourceText = source.ToLowerInvariant(); // 대소문자 무시용 테스트 소스 문자열 생성

            if (ContainsAny(fileName, "folderstructure", "menuclassification")) // 구조 회귀 테스트 파일명 여부 확인
            { // 구조 테스트 폴더 분류
                return testRootPath + "/Structure"; // 구조 회귀 테스트 최종 폴더 반환
            } // 구조 테스트 분류 종료

            if (ContainsAny(fileName, "item", "inventory", "chest", "rewind", "homing", "projectile", "smoke", "cart")) // 아이템 관련 테스트 파일명 여부 확인
            { // 아이템 테스트 폴더 분류
                return testRootPath + "/Items"; // 아이템 테스트 최종 폴더 반환
            } // 아이템 테스트 분류 종료

            if (ContainsAny(fileName, "map", "procedural", "traversal", "obstacle", "vertical", "module")) // 맵 관련 테스트 파일명 여부 확인
            { // 맵 테스트 폴더 분류
                return testRootPath + "/Map"; // 맵 테스트 최종 폴더 반환
            } // 맵 테스트 분류 종료

            if (ContainsAny(fileName, "data", "catalog", "csv", "definition", "idformat")) // 데이터 관련 테스트 파일명 여부 확인
            { // 데이터 테스트 폴더 분류
                return testRootPath + "/Data"; // 데이터 테스트 최종 폴더 반환
            } // 데이터 테스트 분류 종료

            if (ContainsAny(fileName, "canvas", "hud", "fatalerror", "gameui", "menu", "ui")) // UI 관련 테스트 파일명 여부 확인
            { // UI 테스트 폴더 분류
                return testRootPath + "/UI"; // UI 테스트 최종 폴더 반환
            } // UI 테스트 분류 종료

            if (ContainsAny(fileName, "build")) // 빌드 관련 테스트 파일명 여부 확인
            { // 빌드 테스트 폴더 분류
                return testRootPath + "/Build"; // 빌드 테스트 최종 폴더 반환
            } // 빌드 테스트 분류 종료

            if (ContainsAny(fileName, "testframework", "testscene")) // 테스트 프레임워크 자체 테스트 파일명 여부 확인
            { // 테스트 프레임워크 테스트 폴더 분류
                return testRootPath + "/Testing"; // 테스트 프레임워크 테스트 최종 폴더 반환
            } // 테스트 프레임워크 테스트 분류 종료

            if (ContainsAny(fileName, "physicslayer", "sceneflow", "gamescene", "service")) // 프로젝트 설정 기반 테스트 파일명 여부 확인
            { // 프로젝트 설정 테스트 폴더 분류
                return testRootPath + "/ProjectSettings"; // 프로젝트 설정 테스트 최종 폴더 반환
            } // 프로젝트 설정 테스트 분류 종료

            if (ContainsAny(fileName, "player", "movement", "checkpoint", "respawn", "stamina", "push", "camera", "input")) // 플레이어 관련 테스트 파일명 여부 확인
            { // 플레이어 테스트 폴더 분류
                return testRootPath + "/Player"; // 플레이어 테스트 최종 폴더 반환
            } // 플레이어 테스트 분류 종료

            if (ContainsAny(fileName, "match", "ranking", "rank", "timer", "finish", "result")) // 경기 진행 관련 테스트 파일명 여부 확인
            { // Gameplay 테스트 폴더 분류
                return testRootPath + "/Gameplay"; // Gameplay 테스트 최종 폴더 반환
            } // Gameplay 테스트 분류 종료

            if (ContainsAny(fileName, "audio")) // 오디오 관련 테스트 파일명 여부 확인
            { // 오디오 테스트 폴더 분류
                return testRootPath + "/Audio"; // 오디오 테스트 최종 폴더 반환
            } // 오디오 테스트 분류 종료

            if (ContainsAny(sourceText, "projectj.items")) // 파일명으로 판단되지 않은 아이템 네임스페이스 테스트 여부 확인
            { // 아이템 소스 기반 보조 분류
                return testRootPath + "/Items"; // 아이템 테스트 최종 폴더 반환
            } // 아이템 소스 기반 보조 분류 종료

            if (ContainsAny(sourceText, "projectj.mapgeneration")) // 파일명으로 판단되지 않은 맵 네임스페이스 테스트 여부 확인
            { // 맵 소스 기반 보조 분류
                return testRootPath + "/Map"; // 맵 테스트 최종 폴더 반환
            } // 맵 소스 기반 보조 분류 종료

            if (ContainsAny(sourceText, "projectj.player")) // 파일명으로 판단되지 않은 플레이어 네임스페이스 테스트 여부 확인
            { // 플레이어 소스 기반 보조 분류
                return testRootPath + "/Player"; // 플레이어 테스트 최종 폴더 반환
            } // 플레이어 소스 기반 보조 분류 종료

            if (ContainsAny(sourceText, "projectj.data")) // 파일명으로 판단되지 않은 데이터 네임스페이스 테스트 여부 확인
            { // 데이터 소스 기반 보조 분류
                return testRootPath + "/Data"; // 데이터 테스트 최종 폴더 반환
            } // 데이터 소스 기반 보조 분류 종료

            return testRootPath + "/Common"; // 특정 기능으로 분류되지 않는 공통 테스트 폴더 반환
        } // 테스트 스크립트의 최종 기능 폴더 결정 종료

        private static void AddMovePlanIfNeeded(string assetPath, string destinationFolder, List<MovePlan> plans, List<string> errors) // 단일 스크립트의 이동 필요 여부와 경로 충돌 검사
        { // 동일 목적 경로는 유지하고 실제 이동 대상만 계획에 추가
            string destinationPath = destinationFolder + "/" + Path.GetFileName(assetPath); // 현재 파일의 최종 대상 경로 생성

            if (string.Equals(assetPath, destinationPath, StringComparison.Ordinal)) // 이미 최종 기능 폴더에 있는지 확인
            { // 이미 정리된 스크립트 처리
                return; // 추가 이동 계획 없이 종료
            } // 이미 정리된 스크립트 처리 종료

            string destinationGuid = AssetDatabase.AssetPathToGUID(destinationPath); // 대상 경로 기존 asset GUID 조회

            if (!string.IsNullOrEmpty(destinationGuid)) // 동일 이름 대상 asset 존재 여부 확인
            { // 파일 충돌 사전 차단 처리
                errors.Add($"대상 경로 충돌: {assetPath} -> {destinationPath}"); // 대상 파일 충돌 오류 추가
                return; // 충돌 파일 이동 계획 추가 생략
            } // 파일 충돌 사전 차단 처리 종료

            string sourceGuid = AssetDatabase.AssetPathToGUID(assetPath); // 현재 소스 asset GUID 조회

            if (string.IsNullOrEmpty(sourceGuid)) // 현재 소스 GUID 누락 여부 확인
            { // 잘못된 AssetDatabase 검색 결과 처리
                errors.Add($"원본 GUID 누락: {assetPath}"); // 소스 GUID 누락 오류 추가
                return; // 잘못된 스크립트 이동 계획 추가 생략
            } // 현재 소스 GUID 누락 처리 종료

            plans.Add(new MovePlan(assetPath, destinationPath, sourceGuid)); // 대상 폴더 생성 이후 검증할 실제 이동 계획 추가
        } // 단일 스크립트의 이동 필요 여부와 경로 충돌 검사 종료

        private static void ApplyMovePlans(string groupName, IReadOnlyList<MovePlan> plans, List<string> errors) // 검증된 이동 계획 일괄 적용
        { // 대상 폴더 생성·MoveAsset·Import·GUID 검증 처리
            if (errors.Count > 0) // 사전 검증 오류 존재 여부 확인
            { // 실제 이동 전 안전 중단 처리
                LogErrors(errors); // 사전 검증 오류 전체 출력
                EditorUtility.DisplayDialog("Project J Day 48", $"{groupName} 폴더 통합을 시작하지 않았습니다.\n\n오류: {errors.Count}개\nConsole을 확인합니다.", "확인"); // 사전 검증 실패 안내
                return; // 어떤 파일도 이동하지 않고 종료
            } // 실제 이동 전 안전 중단 처리 종료

            if (plans.Count == 0) // 실제 이동 대상 존재 여부 확인
            { // 이미 기능별 폴더 정리가 완료된 상태 처리
                EditorUtility.DisplayDialog("Project J Day 48", $"{groupName} 스크립트는 이미 기능별 폴더 통합이 완료된 상태입니다.", "확인"); // 중복 실행 안내
                return; // 추가 AssetDatabase 작업 없이 종료
            } // 이미 기능별 폴더 정리 완료 상태 처리 종료

            EnsureDestinationFolders(plans); // 모든 대상 기능별 폴더 사전 생성
            List<string> preparedMoveErrors = ValidatePreparedMovePlans(groupName, plans); // 대상 폴더 생성 이후 Unity 이동 가능 여부 전체 검사

            if (preparedMoveErrors.Count > 0) // 대상 폴더 생성 이후 이동 검증 오류 존재 여부 확인
            { // 실제 파일 이동 전 안전 중단 처리
                LogErrors(preparedMoveErrors); // 준비 완료 상태의 이동 검증 오류 전체 출력
                EditorUtility.DisplayDialog("Project J Day 48", $"{groupName} 파일 이동 준비 검사에 실패했습니다.\n\n오류: {preparedMoveErrors.Count}개\n실제 .cs 이동은 수행하지 않았습니다.\nConsole을 확인합니다.", "확인"); // 준비 검사 실패 안내
                return; // 실제 MoveAsset 실행 없이 종료
            } // 실제 파일 이동 전 안전 중단 처리 종료

            List<MovePlan> movedPlans = new List<MovePlan>(); // 이번 실행 성공 이동 기록 목록 생성
            string moveErrorMessage = string.Empty; // 일괄 이동 오류 메시지 초기화
            AssetDatabase.StartAssetEditing(); // 대량 이동 중 자동 Import 일시 정지 시작

            try // 일괄 MoveAsset 처리 시작
            { // Import 정지 구간에서는 파일 이동만 수행
                for (int index = 0; index < plans.Count; index++) // 전체 이동 계획 순회
                { // 현재 스크립트 이동 처리
                    MovePlan plan = plans[index]; // 현재 이동 계획 조회
                    string moveError = AssetDatabase.MoveAsset(plan.SourcePath, plan.DestinationPath); // 기존 .cs와 .meta를 대상 경로로 함께 이동

                    if (!string.IsNullOrEmpty(moveError)) // 현재 MoveAsset 오류 여부 확인
                    { // 일괄 이동 실패 상태 기록
                        moveErrorMessage = $"{plan.SourcePath} -> {plan.DestinationPath} | {moveError}"; // 실패 경로와 Unity 오류 저장
                        break; // 추가 이동 중단
                    } // 현재 MoveAsset 오류 처리 종료

                    movedPlans.Add(plan); // 성공한 이동 기록 추가
                } // 전체 이동 계획 순회 종료
            } // 일괄 MoveAsset 처리 종료
            finally // 성공·실패 공통 AssetDatabase 상태 복원
            { // 자동 Import 재개 처리
                AssetDatabase.StopAssetEditing(); // 대량 asset 편집 종료
            } // 성공·실패 공통 AssetDatabase 상태 복원 종료

            AssetDatabase.SaveAssets(); // 이동 결과와 폴더 meta 저장
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport); // 이동 결과 동기 Import와 스크립트 재컴파일 요청

            if (!string.IsNullOrEmpty(moveErrorMessage)) // 일괄 이동 실패 여부 확인
            { // 현재 실행 이동 롤백 처리
                Debug.LogError($"[ProjectJ][Day48] {groupName} 이동 실패 | {moveErrorMessage}"); // 이동 실패 원인 로그
                RollbackMoves(movedPlans); // 이번 실행에서 성공한 파일만 원래 경로로 복구
                EditorUtility.DisplayDialog("Project J Day 48", $"{groupName} 이동 중 오류가 발생했습니다.\n이번 실행에서 이동한 파일은 복구를 시도했습니다.\nConsole을 확인합니다.", "확인"); // 이동 실패 안내
                return; // 실패한 그룹 이동 종료
            } // 일괄 이동 실패 처리 종료

            List<string> postErrors = ValidateMovedPlans(groupName, movedPlans); // Import 이후 GUID와 최종 경로 검증

            if (postErrors.Count > 0) // 이동 후 검증 오류 존재 여부 확인
            { // GUID 또는 경로 검증 실패 처리
                LogErrors(postErrors); // 이동 후 검증 오류 전체 출력
                EditorUtility.DisplayDialog("Project J Day 48", $"{groupName} 이동은 완료됐지만 검증 오류 {postErrors.Count}개가 있습니다.\n다음 단계로 진행하지 않습니다.", "확인"); // 이동 후 검증 실패 안내
                return; // 다음 단계 진행 방지
            } // 이동 후 검증 실패 처리 종료

            Debug.Log($"[ProjectJ][Day48] {groupName} 기능별 폴더 통합 완료 | 이동 {movedPlans.Count}개"); // 현재 그룹 이동 성공 로그
            EditorUtility.DisplayDialog("Project J Day 48", $"{groupName} 기능별 폴더 통합 완료\n\n이동: {movedPlans.Count}개\n\n컴파일이 끝난 뒤 Console Error를 확인합니다.", "확인"); // 현재 그룹 이동 성공 안내
        } // 검증된 이동 계획 일괄 적용 종료

        private static List<string> ValidatePreparedMovePlans(string groupName, IReadOnlyList<MovePlan> plans) // 대상 폴더 생성 이후 실제 이동 가능 여부 전체 검증
        { // 존재하는 목적지 폴더를 기준으로 ValidateMoveAsset 안전 실행
            List<string> errors = new List<string>(); // 준비 완료 이동 검증 오류 목록 생성

            for (int index = 0; index < plans.Count; index++) // 전체 이동 계획 순회
            { // 현재 원본·대상 경로와 Unity 이동 가능 여부 확인
                MovePlan plan = plans[index]; // 현재 이동 계획 조회
                string sourceGuid = AssetDatabase.AssetPathToGUID(plan.SourcePath); // 현재 원본 asset GUID 재조회
                string destinationGuid = AssetDatabase.AssetPathToGUID(plan.DestinationPath); // 현재 대상 asset GUID 조회

                if (string.IsNullOrEmpty(sourceGuid)) // 원본 asset 존재 여부 확인
                { // 원본 누락 처리
                    errors.Add($"{groupName} 원본 asset 누락: {plan.SourcePath}"); // 원본 누락 오류 추가
                    continue; // 다음 이동 계획 검사
                } // 원본 asset 누락 처리 종료

                if (!string.IsNullOrEmpty(destinationGuid)) // 대상 경로에 기존 asset 존재 여부 확인
                { // 대상 충돌 처리
                    errors.Add($"{groupName} 대상 경로 충돌: {plan.DestinationPath}"); // 대상 충돌 오류 추가
                    continue; // 다음 이동 계획 검사
                } // 대상 경로 충돌 처리 종료

                string destinationFolder = GetParentFolder(plan.DestinationPath); // 현재 대상 파일의 부모 폴더 경로 계산

                if (!AssetDatabase.IsValidFolder(destinationFolder)) // 대상 부모 폴더 생성 완료 여부 확인
                { // 대상 폴더 누락 처리
                    errors.Add($"{groupName} 대상 폴더 누락: {destinationFolder}"); // 대상 폴더 누락 오류 추가
                    continue; // 다음 이동 계획 검사
                } // 대상 폴더 누락 처리 종료

                string moveValidationError = AssetDatabase.ValidateMoveAsset(plan.SourcePath, plan.DestinationPath); // 대상 폴더 생성 이후 Unity 이동 가능 여부 검사

                if (!string.IsNullOrEmpty(moveValidationError)) // Unity 이동 검증 오류 여부 확인
                { // 실제 이동 불가능 경로 처리
                    errors.Add($"{groupName} MoveAsset 사전 검사 실패: {plan.SourcePath} -> {plan.DestinationPath} | {moveValidationError}"); // 이동 사전 검사 오류 추가
                } // 실제 이동 불가능 경로 처리 종료
            } // 전체 이동 계획 순회 종료

            return errors; // 준비 완료 이동 검증 오류 목록 반환
        } // 대상 폴더 생성 이후 실제 이동 가능 여부 전체 검증 종료

        private static List<string> ValidateMovedPlans(string groupName, IReadOnlyList<MovePlan> movedPlans) // 이동 완료 파일의 경로와 GUID 검증
        { // 이동 전후 .meta GUID 동일 여부 검사
            List<string> errors = new List<string>(); // 이동 후 검증 오류 목록 생성

            for (int index = 0; index < movedPlans.Count; index++) // 이번 실행 이동 파일 전체 순회
            { // 현재 파일 최종 상태 검사
                MovePlan plan = movedPlans[index]; // 현재 이동 기록 조회
                string sourceGuid = AssetDatabase.AssetPathToGUID(plan.SourcePath); // 기존 경로 잔존 GUID 조회
                string destinationGuid = AssetDatabase.AssetPathToGUID(plan.DestinationPath); // 새 경로 GUID 조회

                if (!string.IsNullOrEmpty(sourceGuid)) // 기존 경로에 asset이 남아 있는지 확인
                { // 기존 위치 잔존 오류 처리
                    errors.Add($"{groupName} 기존 경로 잔존: {plan.SourcePath}"); // 기존 경로 잔존 오류 추가
                } // 기존 위치 잔존 오류 처리 종료

                if (string.IsNullOrEmpty(destinationGuid)) // 새 경로 asset 존재 여부 확인
                { // 새 위치 누락 오류 처리
                    errors.Add($"{groupName} 대상 경로 누락: {plan.DestinationPath}"); // 새 경로 누락 오류 추가
                    continue; // 현재 파일 GUID 비교 생략
                } // 새 위치 누락 오류 처리 종료

                if (!string.Equals(plan.Guid, destinationGuid, StringComparison.Ordinal)) // 이동 전후 GUID 동일 여부 확인
                { // .meta GUID 보존 실패 처리
                    errors.Add($"{groupName} GUID 불일치: {plan.DestinationPath} | 이전 {plan.Guid} | 이후 {destinationGuid}"); // GUID 불일치 오류 추가
                } // .meta GUID 보존 실패 처리 종료
            } // 이번 실행 이동 파일 전체 순회 종료

            return errors; // 이동 후 검증 오류 목록 반환
        } // 이동 완료 파일의 경로와 GUID 검증 종료

        private static void RollbackMoves(IReadOnlyList<MovePlan> movedPlans) // 이번 실행의 성공 이동 역순 복구
        { // 실패한 그룹을 가능한 범위에서 실행 전 상태로 복원
            if (movedPlans.Count == 0) // 실제 이동 완료 파일 존재 여부 확인
            { // 롤백 대상 없음 처리
                return; // 추가 AssetDatabase 작업 없이 종료
            } // 롤백 대상 없음 처리 종료

            AssetDatabase.StartAssetEditing(); // 롤백 중 자동 Import 일시 정지 시작

            try // 역순 asset 복구 처리 시작
            { // 현재 실행 성공 파일 역순 이동
                for (int index = movedPlans.Count - 1; index >= 0; index--) // 이동 성공 기록 역순 순회
                { // 현재 파일 기존 경로 복구 처리
                    MovePlan plan = movedPlans[index]; // 현재 롤백 대상 조회
                    string rollbackError = AssetDatabase.MoveAsset(plan.DestinationPath, plan.SourcePath); // 새 경로 asset을 기존 경로로 복구

                    if (!string.IsNullOrEmpty(rollbackError)) // 현재 롤백 오류 여부 확인
                    { // 개별 롤백 실패 로그 처리
                        Debug.LogError($"[ProjectJ][Day48] 롤백 실패 | {plan.DestinationPath} -> {plan.SourcePath} | {rollbackError}"); // 롤백 실패 상세 로그
                    } // 개별 롤백 오류 처리 종료
                } // 이동 성공 기록 역순 순회 종료
            } // 역순 asset 복구 처리 종료
            finally // 롤백 성공·실패 공통 AssetDatabase 상태 복원
            { // 자동 Import 재개 처리
                AssetDatabase.StopAssetEditing(); // 롤백 대량 asset 편집 종료
            } // 롤백 성공·실패 공통 AssetDatabase 상태 복원 종료

            AssetDatabase.SaveAssets(); // 롤백 결과 저장
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport); // 롤백 경로 동기 Import
        } // 이번 실행의 성공 이동 역순 복구 종료

        private static void EnsureDestinationFolders(IReadOnlyList<MovePlan> plans) // 모든 대상 부모 폴더 사전 생성
        { // Unity AssetDatabase를 이용한 폴더와 .meta 생성
            for (int index = 0; index < plans.Count; index++) // 전체 이동 계획 순회
            { // 현재 대상 부모 폴더 생성
                EnsureFolder(GetParentFolder(plans[index].DestinationPath)); // 현재 대상 부모 폴더 재귀 생성
            } // 전체 이동 계획 순회 종료

            AssetDatabase.SaveAssets(); // 새 폴더 meta 저장
            AssetDatabase.Refresh(); // 새 폴더 AssetDatabase 반영
        } // 모든 대상 부모 폴더 사전 생성 종료

        private static void EnsureFolder(string folderPath) // 지정 Unity asset 폴더 재귀 생성
        { // 이미 존재하는 부모 폴더를 유지한 안전 생성
            if (AssetDatabase.IsValidFolder(folderPath)) // 대상 폴더 기존 존재 여부 확인
            { // 이미 존재하는 폴더 처리
                return; // 추가 생성 없이 종료
            } // 이미 존재하는 폴더 처리 종료

            string parentFolder = GetParentFolder(folderPath); // 현재 폴더의 부모 경로 계산
            string folderName = folderPath.Substring(parentFolder.Length + 1); // 생성할 마지막 폴더 이름 계산
            EnsureFolder(parentFolder); // 상위 폴더가 없으면 먼저 재귀 생성
            string folderGuid = AssetDatabase.CreateFolder(parentFolder, folderName); // Unity AssetDatabase로 새 폴더 생성

            if (string.IsNullOrEmpty(folderGuid)) // 폴더 생성 실패 여부 확인
            { // 비정상 폴더 생성 처리
                throw new InvalidOperationException($"폴더 생성 실패: {folderPath}"); // 안전 중단용 예외 발생
            } // 비정상 폴더 생성 처리 종료
        } // 지정 Unity asset 폴더 재귀 생성 종료

        private static string GetParentFolder(string assetPath) // Unity asset 경로의 부모 폴더 계산
        { // 마지막 슬래시 기준 부모 경로 추출
            int slashIndex = assetPath.LastIndexOf('/'); // 마지막 폴더 구분자 위치 검색

            if (slashIndex <= 0) // 유효한 부모 경로 존재 여부 확인
            { // 잘못된 asset 경로 처리
                throw new ArgumentException($"유효하지 않은 Asset 경로: {assetPath}", nameof(assetPath)); // 잘못된 경로 예외 발생
            } // 잘못된 asset 경로 처리 종료

            return assetPath.Substring(0, slashIndex); // 부모 폴더 경로 반환
        } // Unity asset 경로의 부모 폴더 계산 종료

        private static void ValidateNoRootLevelScripts(string rootPath, List<string> errors) // 지정 루트 바로 아래 C# 스크립트 잔존 여부 검증
        { // asmdef만 루트에 남기는 48일차 구조 규칙 검사
            string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { rootPath }); // 지정 루트 하위 MonoScript 전체 검색

            for (int index = 0; index < guids.Length; index++) // 검색된 MonoScript 전체 순회
            { // 현재 스크립트의 부모 폴더 확인
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[index]); // 현재 스크립트 경로 조회

                if (!IsCSharpAsset(assetPath)) // 실제 C# 파일 여부 확인
                { // 비 C# 검색 결과 처리
                    continue; // 현재 결과 생략
                } // 비 C# 검색 결과 처리 종료

                if (string.Equals(GetParentFolder(assetPath), rootPath, StringComparison.Ordinal)) // 루트 바로 아래 C# 파일 여부 확인
                { // 미분류 루트 스크립트 오류 처리
                    errors.Add($"기능별 폴더로 이동되지 않은 루트 스크립트: {assetPath}"); // 루트 C# 잔존 오류 추가
                } // 미분류 루트 스크립트 오류 처리 종료
            } // 검색된 MonoScript 전체 순회 종료
        } // 지정 루트 바로 아래 C# 스크립트 잔존 여부 검증 종료

        private static void ValidateSingleAssemblyDefinition(string rootPath, string requiredAsmdefPath, List<string> errors) // 지정 영역의 asmdef 위치와 단일성 검증
        { // 새 하위 폴더가 기존 어셈블리 경계를 유지하는지 검사
            ValidateRequiredAsset(requiredAsmdefPath, errors); // 루트 필수 asmdef 존재 여부 확인
            string[] asmdefGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { rootPath }); // 지정 영역 Assembly Definition 전체 검색
            int asmdefCount = 0; // 실제 asmdef 파일 개수 초기화

            for (int index = 0; index < asmdefGuids.Length; index++) // 검색된 Assembly Definition 전체 순회
            { // 실제 .asmdef 파일만 집계
                string assetPath = AssetDatabase.GUIDToAssetPath(asmdefGuids[index]); // 현재 asmdef 경로 조회

                if (assetPath.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase)) // 실제 asmdef 파일 여부 확인
                { // asmdef 개수 집계
                    asmdefCount++; // 실제 asmdef 개수 증가
                } // asmdef 개수 집계 종료
            } // 검색된 Assembly Definition 전체 순회 종료

            if (asmdefCount != 1) // 지정 영역의 단일 asmdef 규칙 확인
            { // 중첩 또는 누락 asmdef 오류 처리
                errors.Add($"{rootPath} asmdef 개수 불일치: 예상 1개 | 실제 {asmdefCount}개"); // asmdef 개수 오류 추가
            } // 중첩 또는 누락 asmdef 오류 처리 종료
        } // 지정 영역의 asmdef 위치와 단일성 검증 종료

        private static void ValidateRequiredAsset(string assetPath, List<string> errors) // 필수 asset 경로 존재 여부 검증
        { // 구조 정리 후 반드시 유지돼야 하는 파일 검사
            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath))) // 필수 asset GUID 존재 여부 확인
            { // 필수 asset 누락 처리
                errors.Add($"필수 asset 누락: {assetPath}"); // 필수 asset 누락 오류 추가
            } // 필수 asset 누락 처리 종료
        } // 필수 asset 경로 존재 여부 검증 종료

        private static void LogMovePlans(string groupName, IReadOnlyList<MovePlan> plans) // 이동 계획 개별 Console 출력
        { // 사용자가 실행 전 경로를 직접 확인할 수 있는 로그 생성
            for (int index = 0; index < plans.Count; index++) // 현재 그룹 이동 계획 전체 순회
            { // 현재 이동 경로 출력
                MovePlan plan = plans[index]; // 현재 이동 계획 조회
                Debug.Log($"[ProjectJ][Day48][Preview][{groupName}] {plan.SourcePath} -> {plan.DestinationPath}"); // 개별 이동 계획 로그 출력
            } // 현재 그룹 이동 계획 전체 순회 종료
        } // 이동 계획 개별 Console 출력 종료

        private static void LogErrors(IReadOnlyList<string> errors) // 오류 목록 전체 Console 출력
        { // 사용자 확인을 위한 오류별 독립 로그 처리
            for (int index = 0; index < errors.Count; index++) // 모든 오류 순회
            { // 현재 오류 출력
                Debug.LogError($"[ProjectJ][Day48] {errors[index]}"); // 48일차 구조 오류 로그
            } // 모든 오류 순회 종료
        } // 오류 목록 전체 Console 출력 종료

        private static bool IsCSharpAsset(string assetPath) // 실제 C# 소스 경로 여부 확인
        { // AssetDatabase MonoScript 결과 중 .cs만 허용
            return !string.IsNullOrEmpty(assetPath) && assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && File.Exists(assetPath); // 유효한 C# 소스 여부 반환
        } // 실제 C# 소스 경로 여부 확인 종료

        private static bool ContainsOrdinal(string source, string value) // 대소문자 구분 문자열 포함 여부 확인
        { // 메뉴 상수와 기존 폴더 토큰 정확 검색
            return !string.IsNullOrEmpty(source) && source.IndexOf(value, StringComparison.Ordinal) >= 0; // Ordinal 기준 포함 여부 반환
        } // 대소문자 구분 문자열 포함 여부 확인 종료

        private static bool ContainsAny(string source, params string[] values) // 소문자 검색 문자열의 다중 키워드 포함 여부 확인
        { // 테스트 기능 분류용 키워드 묶음 검색
            for (int index = 0; index < values.Length; index++) // 전달된 키워드 전체 순회
            { // 현재 키워드 포함 여부 확인
                if (source.IndexOf(values[index], StringComparison.Ordinal) >= 0) // 현재 키워드 존재 여부 확인
                { // 하나 이상의 키워드 일치 처리
                    return true; // 현재 기능 분류 일치 반환
                } // 하나 이상의 키워드 일치 처리 종료
            } // 전달된 키워드 전체 순회 종료

            return false; // 모든 키워드 불일치 반환
        } // 소문자 검색 문자열의 다중 키워드 포함 여부 확인 종료

        private readonly struct MovePlan // 단일 C# asset 이동 계획 값 선언
        { // 원본·대상 경로와 기존 GUID 저장
            public MovePlan(string sourcePath, string destinationPath, string guid) // 이동 계획 생성
            { // 전달된 경로와 GUID 저장
                SourcePath = sourcePath; // 기존 asset 경로 저장
                DestinationPath = destinationPath; // 최종 기능별 asset 경로 저장
                Guid = guid; // 이동 전 .meta GUID 저장
            } // 이동 계획 생성 종료

            public string SourcePath { get; } // 기존 asset 경로 반환
            public string DestinationPath { get; } // 최종 기능별 asset 경로 반환
            public string Guid { get; } // 이동 전 .meta GUID 반환
        } // 단일 C# asset 이동 계획 값 정의 종료
    } // 세 영역의 안전 이동과 최종 검증 기능 정의 종료
} // 프로젝트 Editor 기능 네임스페이스 종료
