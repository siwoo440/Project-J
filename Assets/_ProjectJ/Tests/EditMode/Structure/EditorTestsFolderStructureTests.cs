using System; // 문자열 비교 기능 참조
using System.IO; // asset 경로의 부모 폴더 계산 기능 참조
using NUnit.Framework; // Unity EditMode 테스트 기능 참조
using UnityEditor; // Unity AssetDatabase 검색 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스
{ // 48일차 Editor·Tests 기능별 폴더 구조 회귀 테스트 정의
    public sealed class EditorTestsFolderStructureTests // Editor·EditMode·PlayMode 루트 구조 유지 테스트 선언
    { // 루트 C# 금지와 asmdef 단일 구조 검증
        private const string EditorRootPath = "Assets/_ProjectJ/Scripts/Editor"; // Editor 스크립트 루트 경로
        private const string EditModeRootPath = "Assets/_ProjectJ/Tests/EditMode"; // EditMode 테스트 루트 경로
        private const string PlayModeRootPath = "Assets/_ProjectJ/Tests/PlayMode"; // PlayMode 테스트 루트 경로
        private const string EditorAsmdefPath = EditorRootPath + "/ProjectJ.Editor.asmdef"; // Editor asmdef 고정 경로
        private const string EditModeAsmdefPath = EditModeRootPath + "/ProjectJ.Tests.EditMode.asmdef"; // EditMode asmdef 고정 경로
        private const string PlayModeAsmdefPath = PlayModeRootPath + "/ProjectJ.Tests.PlayMode.asmdef"; // PlayMode asmdef 고정 경로
        private const string MenuPathsAssetPath = EditorRootPath + "/ProjectManagement/Menu/ProjectJEditorMenuPaths.cs"; // 공통 Editor 메뉴 경로 파일 최종 위치

        [Test] // Unity Test Runner EditMode 테스트 지정
        public void RootFoldersDoNotContainLooseCSharpScripts() // Editor·EditMode·PlayMode 루트 바로 아래 C# 파일이 없는지 검증
        { // 모든 C# 구현 파일이 기능별 하위 폴더에 속하는지 검사
            AssertNoRootLevelScripts(EditorRootPath); // Editor 루트 미분류 C# 검사
            AssertNoRootLevelScripts(EditModeRootPath); // EditMode 루트 미분류 C# 검사
            AssertNoRootLevelScripts(PlayModeRootPath); // PlayMode 루트 미분류 C# 검사
        } // 루트 바로 아래 C# 파일 부재 검증 종료

        [Test] // Unity Test Runner EditMode 테스트 지정
        public void AssemblyDefinitionsRemainSingleAtTheirRoots() // Editor·EditMode·PlayMode asmdef 경계가 유지되는지 검증
        { // 기능별 하위 폴더가 기존 어셈블리에 그대로 포함되는지 검사
            AssertSingleAssemblyDefinition(EditorRootPath, EditorAsmdefPath); // Editor asmdef 단일 구조 확인
            AssertSingleAssemblyDefinition(EditModeRootPath, EditModeAsmdefPath); // EditMode asmdef 단일 구조 확인
            AssertSingleAssemblyDefinition(PlayModeRootPath, PlayModeAsmdefPath); // PlayMode asmdef 단일 구조 확인
        } // Editor·Tests asmdef 경계 유지 검증 종료

        [Test] // Unity Test Runner EditMode 테스트 지정
        public void CommonEditorMenuPathFileExistsAtFunctionalLocation() // 공통 메뉴 경로 파일이 프로젝트 관리 폴더에 있는지 검증
        { // 46일차 메뉴 구조와 48일차 파일 구조 연결 상태 확인
            string menuPathGuid = AssetDatabase.AssetPathToGUID(MenuPathsAssetPath); // 공통 메뉴 경로 파일 GUID 조회
            Assert.IsFalse(string.IsNullOrEmpty(menuPathGuid), $"공통 Editor 메뉴 경로 파일이 없습니다: {MenuPathsAssetPath}"); // 공통 메뉴 경로 파일 최종 위치 존재 확인
        } // 공통 메뉴 경로 파일 기능별 위치 검증 종료

        private static void AssertNoRootLevelScripts(string rootPath) // 지정 루트 바로 아래 C# 스크립트 부재 검증
        { // AssetDatabase 검색 결과의 부모 경로 비교
            string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { rootPath }); // 지정 루트 하위 MonoScript 전체 검색

            for (int index = 0; index < guids.Length; index++) // 검색된 스크립트 전체 순회
            { // 현재 검색 결과의 부모 폴더 검사
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[index]); // 현재 스크립트 asset 경로 조회

                if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) // 실제 C# 파일 여부 확인
                { // 비 C# 검색 결과 처리
                    continue; // 현재 결과 생략
                } // 비 C# 검색 결과 처리 종료

                string parentFolder = Path.GetDirectoryName(assetPath)?.Replace('\\', '/'); // 현재 스크립트 부모 폴더 Unity 경로 계산
                Assert.AreNotEqual(rootPath, parentFolder, $"기능별 폴더로 이동되지 않은 루트 C# 스크립트가 있습니다: {assetPath}"); // 루트 C# 파일 금지 검증
            } // 검색된 스크립트 전체 순회 종료
        } // 지정 루트 바로 아래 C# 스크립트 부재 검증 종료

        private static void AssertSingleAssemblyDefinition(string rootPath, string requiredAsmdefPath) // 지정 영역 asmdef 단일 구조 검증
        { // 필수 루트 asmdef와 중첩 asmdef 부재 검사
            string requiredGuid = AssetDatabase.AssetPathToGUID(requiredAsmdefPath); // 필수 루트 asmdef GUID 조회
            Assert.IsFalse(string.IsNullOrEmpty(requiredGuid), $"필수 asmdef가 없습니다: {requiredAsmdefPath}"); // 필수 루트 asmdef 존재 확인
            string[] asmdefGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { rootPath }); // 지정 영역 Assembly Definition 전체 검색
            int asmdefCount = 0; // 실제 asmdef 개수 초기화

            for (int index = 0; index < asmdefGuids.Length; index++) // 검색된 asmdef 전체 순회
            { // 실제 .asmdef 파일만 집계
                string assetPath = AssetDatabase.GUIDToAssetPath(asmdefGuids[index]); // 현재 Assembly Definition 경로 조회

                if (assetPath.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase)) // 실제 asmdef 파일 여부 확인
                { // asmdef 개수 집계
                    asmdefCount++; // 실제 asmdef 개수 증가
                } // asmdef 개수 집계 종료
            } // 검색된 asmdef 전체 순회 종료

            Assert.AreEqual(1, asmdefCount, $"{rootPath} 내부 asmdef 개수가 달라졌습니다. 실제: {asmdefCount}"); // 단일 어셈블리 경계 확인
        } // 지정 영역 asmdef 단일 구조 검증 종료
    } // Editor·EditMode·PlayMode 루트 구조 유지 테스트 정의 종료
} // 프로젝트 EditMode 테스트 네임스페이스 종료
