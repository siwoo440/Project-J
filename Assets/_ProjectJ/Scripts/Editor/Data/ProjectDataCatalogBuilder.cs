using System.Collections.Generic; // 데이터 에셋 목록 기능 참조
using ProjectJ.Data; // 프로젝트 데이터 카탈로그와 검증 형식 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEditor; // Unity 에셋 생성과 메뉴 기능 참조
using UnityEngine; // Unity ScriptableObject와 변경 표시 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal static class ProjectDataCatalogBuilder // 런타임 데이터 카탈로그 생성과 갱신 도구 선언
    {
        internal const string ResourcesRootPath = "Assets/_ProjectJ/Resources"; // 프로젝트 Resources 폴더 경로 선언
        internal const string CatalogAssetPath = ResourcesRootPath + "/ProjectDataCatalog.asset"; // 런타임 카탈로그 에셋 경로 선언

        [MenuItem("Project J/Day 17/Rebuild Runtime Data Catalog")] // 17일차 카탈로그 수동 갱신 메뉴 등록
        private static void RebuildFromMenu() // 메뉴를 통한 런타임 데이터 카탈로그 갱신
        {
            ProjectDataValidationReport report = RebuildAndValidate(true); // 카탈로그 갱신과 전체 데이터 검증 실행

            if (report.IsValid) // 전체 데이터 검증 성공 여부 확인
            {
                ProjectLog.Info(ProjectLogCategory.Data, "런타임 데이터 카탈로그 갱신을 완료했습니다.", "DATA_CATALOG_REBUILT"); // 카탈로그 갱신 성공 로그 출력
            }
        }

        internal static ProjectDataValidationReport RebuildAndValidate(bool logSuccess) // 전체 데이터 에셋을 카탈로그에 등록하고 검증
        {
            EnsureResourcesFolder(); // 런타임 카탈로그 저장 폴더 준비
            List<ProjectDataAsset> assets = ProjectDataAssetDatabase.LoadAll(); // 전체 프로젝트 데이터 에셋 불러오기
            ProjectDataCatalog catalog = LoadOrCreateCatalog(); // 기존 카탈로그 조회 또는 새 카탈로그 생성
            catalog.SetEditorAssets(assets); // 정렬된 전체 데이터 에셋 카탈로그 등록
            EditorUtility.SetDirty(catalog); // 카탈로그 변경 상태 표시
            AssetDatabase.SaveAssets(); // 카탈로그 에셋 변경 내용 저장
            return ProjectDataAssetDatabase.ValidateAll(logSuccess); // 전체 데이터 검증 결과 반환
        }

        private static ProjectDataCatalog LoadOrCreateCatalog() // 런타임 데이터 카탈로그 조회 또는 생성
        {
            ProjectDataCatalog catalog = AssetDatabase.LoadAssetAtPath<ProjectDataCatalog>(CatalogAssetPath); // 기존 런타임 카탈로그 에셋 조회

            if (catalog != null) // 기존 카탈로그 존재 여부 확인
            {
                return catalog; // 기존 카탈로그 재사용
            }

            catalog = ScriptableObject.CreateInstance<ProjectDataCatalog>(); // 새 런타임 카탈로그 인스턴스 생성
            AssetDatabase.CreateAsset(catalog, CatalogAssetPath); // Resources 경로에 카탈로그 에셋 생성
            return catalog; // 새로 생성한 카탈로그 반환
        }

        private static void EnsureResourcesFolder() // 프로젝트 Resources 폴더 존재 보장
        {
            const string projectRootPath = "Assets/_ProjectJ"; // 프로젝트 공통 에셋 루트 경로 선언

            if (!AssetDatabase.IsValidFolder(projectRootPath)) // 프로젝트 공통 에셋 루트 누락 여부 확인
            {
                AssetDatabase.CreateFolder("Assets", "_ProjectJ"); // 프로젝트 공통 에셋 루트 생성
            }

            if (!AssetDatabase.IsValidFolder(ResourcesRootPath)) // 프로젝트 Resources 폴더 누락 여부 확인
            {
                AssetDatabase.CreateFolder(projectRootPath, "Resources"); // 프로젝트 Resources 폴더 생성
            }
        }
    }
}
