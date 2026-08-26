using System.IO; // 파일과 폴더 상태 확인 기능
using UnityEditor; // Unity Editor 자산 관리 기능
using UnityEngine; // Unity 로그 기능

namespace ProjectJ.Editor // Project J Editor 기능 네임스페이스
{
    internal static class Day144LegacyFlatModuleCleanupTool // 잘못 추가된 평면형 프로토타입만 정리하는 도구
    {
        private const string MenuPath = "ProjectJ/Day144/Delete Legacy Flat Prototype Only"; // 안전 정리 메뉴 경로

        private static readonly string[] LegacyAssetPaths = // 이전 잘못된 ZIP의 정확한 자산 경로 목록
        {
            "Assets/ProjectJ/Editor/ProjectJ144MapModuleSetup.cs", // 이전 평면형 생성 도구
            "Assets/ProjectJ/Runtime/Map/Modules/ProjectJMapModule.cs", // 이전 중복 모듈 정의
            "Assets/ProjectJ/Runtime/Map/Modules/ProjectJMapModuleBounds.cs", // 이전 중복 Bounds 정의
            "Assets/ProjectJ/Runtime/Map/Modules/ProjectJMapModuleCatalog.cs", // 이전 중복 Catalog 정의
            "Assets/ProjectJ/Runtime/Map/Modules/ProjectJMapModuleConnectionPoint.cs", // 이전 중복 연결점 정의
            "Assets/ProjectJ/Runtime/Map/Modules/ProjectJMapModulePlacement.cs", // 이전 중복 배치 정의
            "Assets/ProjectJ/Runtime/Map/Modules/ProjectJMapModuleRules.cs", // 이전 중복 규칙 정의
            "Assets/ProjectJ/Runtime/Map/Modules/ProjectJMapModuleTypes.cs", // 이전 중복 타입 정의
            "Assets/ProjectJ/Runtime/Map/Modules/ProjectJMapModuleValidator.cs", // 이전 중복 검증 정의
            "Assets/ProjectJ/Runtime/Map/Modules/ProjectJMapObstacleSlot.cs", // 이전 중복 장애물 슬롯 정의
            "Assets/ProjectJ/Prefabs/Map/Modules/Module_Floor_Straight.prefab", // 이전 평면 직선 Prefab
            "Assets/ProjectJ/Prefabs/Map/Modules/Module_Floor_Wide.prefab", // 이전 평면 넓은 바닥 Prefab
            "Assets/ProjectJ/Prefabs/Map/Modules/Module_Platform_Small.prefab", // 이전 평면 작은 발판 Prefab
            "Assets/ProjectJ/Prefabs/Map/Modules/Module_Platform_Large.prefab", // 이전 평면 큰 발판 Prefab
            "Assets/ProjectJ/Prefabs/Map/Modules/Module_Jump_Short.prefab", // 이전 평면 점프 Prefab
            "Assets/ProjectJ/Prefabs/Map/Modules/Module_Ramp.prefab", // 이전 평면 Ramp Prefab
            "Assets/ProjectJ/Prefabs/Map/Modules/Module_Stairs.prefab", // 이전 평면 계단 Prefab
            "Assets/ProjectJ/Prefabs/Map/Modules/Module_LowPassage.prefab", // 이전 평면 낮은 통로 Prefab
            "Assets/ProjectJ/Prefabs/Map/Modules/Module_Turn_90.prefab", // 이전 평면 회전 Prefab
            "Assets/ProjectJ/Prefabs/Map/Modules/Module_Junction.prefab", // 이전 평면 분기 Prefab
            "Assets/ProjectJ/Prefabs/Map/Modules/Module_Merge_3.prefab", // 이전 평면 합류 Prefab
            "Assets/ProjectJ/Data/Map/ProjectJMapModuleCatalog.asset" // 이전 평면 Catalog 자산
        }; // 이전 잘못된 ZIP 자산 목록 종료

        private static readonly string[] EmptyFolderCandidates = // 비어 있을 때만 제거할 이전 전용 폴더 목록
        {
            "Assets/ProjectJ/Runtime/Map/Modules", // 이전 중복 Runtime 폴더
            "Assets/ProjectJ/Data/Map" // 이전 Catalog 폴더
        }; // 빈 폴더 후보 목록 종료

        [MenuItem(MenuPath)] // Unity 상단 메뉴 등록
        private static void CleanupLegacyFlatPrototype() // 이전 평면형 프로토타입만 안전 정리
        {
            int deletedCount = 0; // 삭제 성공 개수 초기화

            for (int index = 0; index < LegacyAssetPaths.Length; index++) // 정확한 삭제 대상 전체 순회
            {
                string assetPath = LegacyAssetPaths[index]; // 현재 삭제 대상 경로 조회

                if (AssetDatabase.LoadMainAssetAtPath(assetPath) == null && !AssetDatabase.IsValidFolder(assetPath)) // 대상 존재 여부 검사
                {
                    continue; // 없는 대상 건너뛰기
                }

                if (AssetDatabase.DeleteAsset(assetPath)) // 정확한 자산 하나만 삭제 시도
                {
                    deletedCount++; // 삭제 성공 개수 증가
                }
                else // 삭제 실패 처리
                {
                    Debug.LogWarning("[ProjectJ][Day144] 이전 평면형 자산 삭제 실패: " + assetPath); // 실패 경로 출력
                }
            }

            for (int index = 0; index < EmptyFolderCandidates.Length; index++) // 전용 빈 폴더 후보 순회
            {
                DeleteFolderWhenEmpty(EmptyFolderCandidates[index]); // 비어 있는 경우만 폴더 삭제
            }

            AssetDatabase.SaveAssets(); // 자산 변경 상태 저장
            AssetDatabase.Refresh(); // Project 창 갱신
            Debug.Log("[ProjectJ][Day144] 이전 평면형 프로토타입 안전 정리 완료. 삭제 항목: " + deletedCount); // 정리 완료 로그 출력
        }

        private static void DeleteFolderWhenEmpty(string folderPath) // 비어 있는 전용 폴더만 삭제
        {
            if (!AssetDatabase.IsValidFolder(folderPath)) // Unity 폴더 존재 검사
            {
                return; // 없는 폴더 처리 생략
            }

            string absolutePath = Path.GetFullPath(folderPath); // 실제 폴더 경로 계산

            if (!Directory.Exists(absolutePath)) // 실제 폴더 존재 검사
            {
                return; // 실제 폴더가 없으면 처리 생략
            }

            string[] entries = Directory.GetFileSystemEntries(absolutePath); // 폴더 내부 항목 조회

            if (entries.Length > 0) // 폴더 내부 자산 존재 여부 검사
            {
                return; // 사용 중인 폴더 보존
            }

            AssetDatabase.DeleteAsset(folderPath); // 빈 전용 폴더 삭제
        }
    }
}
