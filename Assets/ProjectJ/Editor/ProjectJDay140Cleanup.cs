using System.Collections.Generic; // 임시 대상 목록 사용
using UnityEditor; // Editor 메뉴와 Asset 삭제 사용
using UnityEditor.SceneManagement; // Scene 열기와 저장 사용
using UnityEngine; // GameObject와 MonoBehaviour 사용
using UnityEngine.SceneManagement; // Scene 조회 사용

namespace ProjectJ.EditorTools // Project J Editor 도구 영역
{
    public static class ProjectJDay140Cleanup // 140일차 안정성 정리 도구
    {
        private const string GameScenePath = "Assets/ProjectJ/Scenes/Game.unity"; // 정리 대상 Game Scene 경로
        private const string LegacyRouteRootName = "=== DAY136 BOT ROUTE ==="; // 구형 Bot Route Root 이름
        private const string LegacyRouteTypeName = "ProjectJ.AI.ProjectJBotRouteNode"; // 구형 Route Component 전체 이름

        private static readonly string[] LegacyAssetPaths = // 삭제 대상 구형 파일 경로 목록
        {
            "Assets/ProjectJ/Runtime/AI/ProjectJBotRouteNode.cs", // 구형 Waypoint Component 파일
            "Assets/ProjectJ/Editor/ProjectJDay137BotRouteSetup.cs", // 구형 Route 자동 생성 Editor 파일
            "Assets/ProjectJ/Runtime/SceneFlow/ProjectJPrivateMatchPanel.cs" // 기능 없는 이동 표식 파일
        };

        [MenuItem("Project J/Day140/Apply Stability Cleanup")] // Unity 상단 메뉴 등록
        private static void ApplyStabilityCleanup() // 승인된 140일차 정리 실행
        {
            bool approved = EditorUtility.DisplayDialog( // 실제 삭제 전 사용자 확인
                "Project J - Day140 Stability Cleanup", // 확인 창 제목
                "구형 Bot Route와 확인된 불필요 스크립트 3개를 제거합니다.\n\n" + // 첫 번째 삭제 안내
                "삭제 대상:\n" + // 삭제 목록 제목
                "- ProjectJBotRouteNode.cs\n" + // 구형 Route Component 안내
                "- ProjectJDay137BotRouteSetup.cs\n" + // 구형 Route Setup 안내
                "- ProjectJPrivateMatchPanel.cs\n\n" + // 빈 이동 표식 파일 안내
                "Game.unity의 === DAY136 BOT ROUTE === 오브젝트와 남은 Route Component도 함께 제거합니다.", // Scene 정리 안내
                "정리 실행", // 실행 버튼 문구
                "취소" // 취소 버튼 문구
            );

            if (!approved) // 사용자 취소 확인
            {
                return; // 정리 작업 중단
            }

            if (!CleanupGameScene(out int removedRouteRoots, out int removedRouteComponents)) // Scene 정리 성공 여부 확인
            {
                Debug.LogError("[Project J/Day140] Game Scene 정리에 실패하여 파일 삭제를 중단했습니다."); // 부분 삭제 방지 오류 출력
                return; // 파일 삭제 단계 차단
            }

            int deletedAssets = DeleteLegacyAssets(); // 승인된 구형 파일 삭제
            AssetDatabase.SaveAssets(); // Asset 변경 저장
            AssetDatabase.Refresh(); // 삭제 결과 Unity Asset DB 반영

            Debug.Log( // 최종 정리 결과 출력
                "[Project J/Day140] 안정성 정리 완료 / Route Root 제거: " + removedRouteRoots + // Route Root 결과 출력
                " / Route Component 제거: " + removedRouteComponents + // Route Component 결과 출력
                " / 구형 파일 제거: " + deletedAssets + // 파일 삭제 결과 출력
                "\nDebug/Test/Inventory 및 다른 Day Setup 파일은 변경하지 않았습니다." // 보존 범위 안내
            );
        }

        private static bool CleanupGameScene(out int removedRouteRoots, out int removedRouteComponents) // Game Scene 구형 Route 제거
        {
            removedRouteRoots = 0; // Route Root 제거 개수 초기화
            removedRouteComponents = 0; // Route Component 제거 개수 초기화
            Scene scene = SceneManager.GetSceneByPath(GameScenePath); // 현재 열린 Game Scene 조회
            bool openedByCleanup = !scene.IsValid() || !scene.isLoaded; // 도구가 Scene을 열어야 하는지 확인

            if (!openedByCleanup && scene.isDirty && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) // 기존 Game Scene 변경사항 저장 여부 확인
            {
                return false; // 사용자가 저장을 취소하면 정리 중단
            }

            if (openedByCleanup) // Game Scene 미로딩 확인
            {
                scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Additive); // 기존 Scene 유지 후 Game Scene 추가 열기
            }

            if (!scene.IsValid() || !scene.isLoaded) // Game Scene 로딩 실패 확인
            {
                return false; // 잘못된 Scene 상태 중단
            }

            bool cleanupSucceeded = false; // Scene 정리 성공 상태 초기화

            try // Scene 변경과 저장 보호 구간
            {
                removedRouteRoots = RemoveLegacyRouteRoots(scene); // 구형 Route Root 제거
                removedRouteComponents = RemoveLegacyRouteComponents(scene); // Root 밖 잔여 Route Component 제거

                if (removedRouteRoots > 0 || removedRouteComponents > 0) // Scene 변경 발생 여부 확인
                {
                    EditorSceneManager.MarkSceneDirty(scene); // Game Scene 변경 상태 표시

                    if (!EditorSceneManager.SaveScene(scene)) // 변경된 Game Scene 저장 시도
                    {
                        Debug.LogError("[Project J/Day140] Game.unity 저장에 실패했습니다."); // Scene 저장 실패 출력
                        return false; // 파일 삭제 전에 중단
                    }
                }

                cleanupSucceeded = true; // Scene 정리 성공 표시
                return true; // Scene 정리 성공 반환
            }
            finally // 추가로 연 Scene 정리 구간
            {
                if (openedByCleanup && scene.IsValid() && scene.isLoaded) // 도구가 연 Scene인지 확인
                {
                    EditorSceneManager.CloseScene(scene, true); // 도구가 연 Game Scene 닫기
                }

                if (!cleanupSucceeded) // Scene 정리 실패 확인
                {
                    removedRouteRoots = 0; // 실패 시 결과 개수 초기화
                    removedRouteComponents = 0; // 실패 시 결과 개수 초기화
                }
            }
        }

        private static int RemoveLegacyRouteRoots(Scene scene) // 구형 Route Root 오브젝트 제거
        {
            List<GameObject> removeTargets = new List<GameObject>(); // 삭제 대상 GameObject 목록 생성
            GameObject[] roots = scene.GetRootGameObjects(); // Scene Root 목록 조회

            for (int index = 0; index < roots.Length; index++) // 모든 Scene Root 순회
            {
                CollectObjectsByName(roots[index].transform, LegacyRouteRootName, removeTargets); // 구형 Route Root 이름 재귀 수집
            }

            for (int index = 0; index < removeTargets.Count; index++) // 수집된 Route Root 순회
            {
                Object.DestroyImmediate(removeTargets[index]); // 구형 Route Root 즉시 제거
            }

            return removeTargets.Count; // 제거한 Route Root 개수 반환
        }

        private static int RemoveLegacyRouteComponents(Scene scene) // 구형 Route Component 잔여 참조 제거
        {
            int removedCount = 0; // 제거 Component 개수 초기화
            GameObject[] roots = scene.GetRootGameObjects(); // Scene Root 목록 조회

            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++) // 모든 Scene Root 순회
            {
                MonoBehaviour[] behaviours = roots[rootIndex].GetComponentsInChildren<MonoBehaviour>(true); // 하위 MonoBehaviour 전체 수집

                for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; behaviourIndex++) // 수집된 Component 순회
                {
                    MonoBehaviour behaviour = behaviours[behaviourIndex]; // 현재 Component 조회

                    if (behaviour == null) // Missing Script 항목 확인
                    {
                        continue; // Missing Script는 별도 검사 대상으로 보존
                    }

                    System.Type behaviourType = behaviour.GetType(); // 현재 Component 실제 타입 조회

                    if (behaviourType.FullName != LegacyRouteTypeName) // 구형 Route Component 타입 일치 확인
                    {
                        continue; // 다른 Runtime Component 보존
                    }

                    Object.DestroyImmediate(behaviour); // 구형 Route Component 즉시 제거
                    removedCount++; // 제거 개수 증가
                }
            }

            return removedCount; // 제거한 Component 개수 반환
        }

        private static void CollectObjectsByName(Transform current, string targetName, List<GameObject> results) // 이름 기준 Scene 오브젝트 재귀 수집
        {
            if (current == null) // 잘못된 Transform 확인
            {
                return; // 재귀 탐색 중단
            }

            if (current.name == targetName) // 현재 오브젝트 이름 일치 확인
            {
                results.Add(current.gameObject); // 삭제 대상 목록 추가
                return; // 대상 Root 하위 중복 수집 방지
            }

            for (int index = 0; index < current.childCount; index++) // 모든 자식 Transform 순회
            {
                CollectObjectsByName(current.GetChild(index), targetName, results); // 자식 Hierarchy 재귀 탐색
            }
        }

        private static int DeleteLegacyAssets() // 승인된 구형 스크립트 파일 삭제
        {
            int deletedCount = 0; // 실제 삭제 파일 개수 초기화

            for (int index = 0; index < LegacyAssetPaths.Length; index++) // 모든 삭제 대상 경로 순회
            {
                string assetPath = LegacyAssetPaths[index]; // 현재 삭제 대상 경로 조회
                Object existingAsset = AssetDatabase.LoadMainAssetAtPath(assetPath); // 현재 Asset 존재 여부 조회

                if (existingAsset == null) // 이미 삭제된 Asset 확인
                {
                    Debug.Log("[Project J/Day140] 이미 없는 파일 유지: " + assetPath); // 중복 실행 안전 안내
                    continue; // 없는 파일 삭제 생략
                }

                if (!AssetDatabase.DeleteAsset(assetPath)) // Unity AssetDatabase를 통한 파일과 meta 삭제 시도
                {
                    Debug.LogError("[Project J/Day140] 파일 삭제 실패: " + assetPath); // 삭제 실패 경로 출력
                    continue; // 다음 삭제 대상 처리
                }

                deletedCount++; // 성공한 삭제 개수 증가
                Debug.Log("[Project J/Day140] 구형 파일 제거: " + assetPath); // 삭제 성공 경로 출력
            }

            return deletedCount; // 실제 삭제 파일 개수 반환
        }
    }
}
