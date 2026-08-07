using System; // 문자열 비교 기능 참조
using System.IO; // Editor 소스 파일 읽기 기능 참조
using NUnit.Framework; // EditMode 테스트 기능 참조
using UnityEditor; // Unity AssetDatabase 검색 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스
{ // 46일차 Editor 메뉴 분류 회귀 테스트 정의
    public sealed class EditorMenuClassificationTests // 기능별 Editor 메뉴 구조 유지 테스트 선언
    { // 구형 Day 메뉴와 대분류 경로 규칙 검증
        private const string EditorRootPath = "Assets/_ProjectJ/Scripts/Editor"; // Editor 스크립트 검색 루트 경로
        private const string MenuPathsAssetPath = EditorRootPath + "/ProjectManagement/Menu/ProjectJEditorMenuPaths.cs"; // 48일차 기능별 폴더 통합 이후 공통 메뉴 경로 파일 위치
        private const string LegacyMenuToken = "Project J" + "/Day "; // 금지된 구형 일차 메뉴 문자열

        [Test] // Unity Test Runner EditMode 테스트 지정
        public void EditorScriptsDoNotContainLegacyDayMenuPaths() // Editor 소스에 구형 Day 메뉴가 없는지 검증
        { // 46일차 메뉴 재분류 누락과 회귀 방지
            string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { EditorRootPath }); // Editor 폴더 MonoScript 전체 검색

            for (int index = 0; index < guids.Length; index++) // 검색된 모든 Editor 스크립트 순회
            { // 현재 Editor 소스의 구형 메뉴 경로 확인
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[index]); // 현재 스크립트 프로젝트 경로 조회

                if (!assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || !File.Exists(assetPath)) // 실제 C# 소스 파일 여부 확인
                { // 검사 대상이 아닌 에셋 처리
                    continue; // 현재 에셋 검사 생략
                } // 비대상 에셋 처리 완료

                string source = File.ReadAllText(assetPath); // 현재 Editor 소스 전체 내용 읽기
                Assert.Less(source.IndexOf(LegacyMenuToken, StringComparison.Ordinal), 0, $"구형 Day 메뉴 경로가 남아 있습니다: {assetPath}"); // 구형 일차 메뉴 잔존 금지 검증
            } // Editor 스크립트 전체 검사 완료
        } // 구형 Day 메뉴 회귀 검증 완료

        [Test] // Unity Test Runner EditMode 테스트 지정
        public void CommonMenuPathFileContainsNineMajorCategories() // 공통 메뉴 경로 파일의 9개 대분류 존재 검증
        { // 메뉴 구조 누락과 이름 변경 회귀 방지
            Assert.IsTrue(File.Exists(MenuPathsAssetPath), $"공통 메뉴 경로 파일이 없습니다: {MenuPathsAssetPath}"); // 공통 메뉴 경로 파일 존재 검증
            string source = File.ReadAllText(MenuPathsAssetPath); // 공통 메뉴 경로 소스 전체 내용 읽기
            string[] requiredCategories = // 46일차 확정 9개 대분류 이름 목록
            { // 필수 대분류 문자열 묶음
                "01. 프로젝트 설정", // 프로젝트 설정 대분류
                "02. 플레이어와 입력", // 플레이어와 입력 대분류
                "03. 데이터", // 데이터 대분류
                "04. 테스트", // 테스트 대분류
                "05. 빌드", // 빌드 대분류
                "06. 맵", // 맵 대분류
                "07. 장애물", // 장애물 대분류
                "08. 아이템", // 아이템 대분류
                "09. UI" // UI 대분류
            }; // 필수 대분류 문자열 묶음 완료

            for (int index = 0; index < requiredCategories.Length; index++) // 9개 대분류 전체 순회
            { // 현재 대분류 문자열 존재 여부 검사
                Assert.GreaterOrEqual(source.IndexOf(requiredCategories[index], StringComparison.Ordinal), 0, $"공통 메뉴 경로에서 대분류가 누락됐습니다: {requiredCategories[index]}"); // 현재 대분류 존재 검증
            } // 9개 대분류 검사 완료
        } // 공통 메뉴 대분류 검증 완료
    } // 기능별 Editor 메뉴 구조 유지 테스트 정의
} // 프로젝트 EditMode 테스트 네임스페이스 종료
