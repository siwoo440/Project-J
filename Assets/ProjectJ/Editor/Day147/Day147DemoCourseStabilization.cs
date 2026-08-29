using System; // 문자열 비교 기능 사용
using ProjectJ.Map; // 고정맵 Module 조회 기능 사용
using UnityEditor; // Prefab과 메뉴 기능 사용
using UnityEditor.SceneManagement; // Game Scene 열기와 저장 기능 사용
using UnityEngine; // GameObject와 Transform 기능 사용
using UnityEngine.SceneManagement; // Scene 루트 조회 기능 사용

namespace ProjectJ.Editor // Project J Editor 기능 네임스페이스
{
    public static class Day147DemoCourseStabilization // 147일차 시연용 고정맵 좌표 안정화 도구
    {
        private const string CoursePrefabPath = "Assets/ProjectJ/Prefabs/Map/Courses/PJ146_DemoCourse.prefab"; // Day146 고정 코스 Prefab 경로
        private const string GameScenePath = "Assets/ProjectJ/Scenes/Game.unity"; // 실제 Game Scene 경로
        private const string CourseRootName = "PJ146_DemoCourse"; // 고정 코스 루트 이름
        private const string ModulesRootName = "Modules"; // Module 그룹 이름
        private const string GameplayRootName = "Gameplay"; // Gameplay 그룹 이름
        private const string SafetyFloorName = "Day147_SafetyFloor"; // 낙하 방지 바닥 이름
        private const string StartSectionName = "START"; // 시작 구간 이름
        private const string FloorObjectName = "Floor"; // 기본 Module 바닥 이름
        private const float ModuleSize = 10f; // 기본 Module 크기
        private const float ModuleVerticalOffset = 5f; // Module 아랫면을 Y 0으로 올리는 오프셋
        private const float SafetyFloorTopY = -1f; // 낙하 방지 바닥 윗면 높이
        private const float SafetyFloorThickness = 1f; // 낙하 방지 바닥 두께
        private const float SafetyFloorPadding = 10f; // 고정맵 외곽 안전 여유 폭
        private const float PositionTolerance = 0.01f; // 좌표 검증 허용 오차

        [MenuItem("ProjectJ/Day147/1. Apply Demo Stabilization")] // 147일차 자동 적용 메뉴 등록
        public static void ApplyDemoStabilization() // 고정맵 좌표와 안전 바닥 자동 적용
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) // 현재 Scene 미저장 변경 보호
            {
                Debug.LogWarning("[ProjectJ][Day147] 현재 Scene 저장이 취소되어 적용을 중단했습니다."); // 적용 중단 로그 출력
                return; // 적용 중단
            }

            GameObject coursePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoursePrefabPath); // 기존 Day146 코스 Prefab 조회

            if (coursePrefab == null) // Day146 코스 Prefab 누락 검사
            {
                Day146DemoCourseSetup.BuildDemoCoursePrefabMenu(); // Day146 코스 Prefab 자동 생성
                coursePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoursePrefabPath); // 생성된 코스 Prefab 재조회
            }

            if (coursePrefab == null) // 자동 생성 후에도 Prefab 누락 검사
            {
                Debug.LogError("[ProjectJ][Day147] PJ146_DemoCourse Prefab을 준비하지 못했습니다."); // Prefab 준비 실패 로그 출력
                return; // 적용 중단
            }

            if (!PatchCoursePrefab()) // 고정 코스 Prefab 패치 실행
            {
                return; // Prefab 패치 실패 시 Scene 변경 차단
            }

            Scene gameScene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single); // 실제 Game Scene 열기
            RemoveCourseInstanceFromScene(gameScene); // 기존 고정 코스 Scene 인스턴스 제거
            coursePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoursePrefabPath); // 패치된 코스 Prefab 재조회
            GameObject courseInstance = PrefabUtility.InstantiatePrefab(coursePrefab) as GameObject; // 패치된 Prefab Scene 배치

            if (courseInstance == null) // Scene Prefab 인스턴스 생성 실패 검사
            {
                Debug.LogError("[ProjectJ][Day147] Game Scene에 고정 코스를 배치하지 못했습니다."); // Scene 배치 실패 로그 출력
                return; // 적용 중단
            }

            courseInstance.name = CourseRootName; // Scene 고정 코스 이름 통일
            courseInstance.transform.position = Vector3.zero; // 고정 코스 월드 원점 배치
            courseInstance.transform.rotation = Quaternion.identity; // 고정 코스 회전 초기화
            courseInstance.transform.localScale = Vector3.one; // 고정 코스 Scale 초기화
            EditorSceneManager.MarkSceneDirty(gameScene); // Game Scene 변경 상태 기록
            EditorSceneManager.SaveScene(gameScene); // Game Scene 자동 저장
            AssetDatabase.SaveAssets(); // 수정된 Prefab 자산 저장
            AssetDatabase.Refresh(); // Unity Project 창 갱신
            ValidateDemoStabilization(); // 적용 결과 즉시 검증
            Selection.activeGameObject = courseInstance; // Hierarchy에서 고정 코스 선택
            EditorGUIUtility.PingObject(courseInstance); // Hierarchy 고정 코스 강조
            Debug.Log("[ProjectJ][Day147] 적용 완료 / 맵 아랫면 Y=0 / 안전 바닥 윗면 Y=-1 / Game Scene 재배치.", courseInstance); // 적용 완료 로그 출력
        }

        [MenuItem("ProjectJ/Day147/2. Validate Demo Stabilization")] // 147일차 좌표 검증 메뉴 등록
        public static void ValidateDemoStabilization() // 고정맵 좌표와 안전 바닥 상태 검증
        {
            GameObject coursePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoursePrefabPath); // 검증용 코스 Prefab 조회

            if (coursePrefab == null) // 검증 대상 Prefab 누락 검사
            {
                Debug.LogError("[ProjectJ][Day147] 검증할 PJ146_DemoCourse Prefab이 없습니다."); // Prefab 누락 로그 출력
                return; // 검증 중단
            }

            int errorCount = 0; // 검증 오류 개수 초기화
            Transform modulesRoot = coursePrefab.transform.Find(ModulesRootName); // Module 그룹 조회
            Transform gameplayRoot = coursePrefab.transform.Find(GameplayRootName); // Gameplay 그룹 조회
            Transform safetyFloor = coursePrefab.transform.Find(SafetyFloorName); // 안전 바닥 조회

            if (modulesRoot == null) // Module 그룹 누락 검사
            {
                Debug.LogError("[ProjectJ][Day147] Modules 루트를 찾지 못했습니다.", coursePrefab); // Module 그룹 누락 로그 출력
                errorCount++; // 오류 개수 증가
            }
            else if (!Mathf.Approximately(modulesRoot.localPosition.y, ModuleVerticalOffset)) // Module 높이 오프셋 검사
            {
                Debug.LogError("[ProjectJ][Day147] Modules Y 오프셋이 5가 아닙니다: " + modulesRoot.localPosition.y, modulesRoot); // Module 높이 오류 로그 출력
                errorCount++; // 오류 개수 증가
            }

            if (gameplayRoot == null) // Gameplay 그룹 누락 검사
            {
                Debug.LogError("[ProjectJ][Day147] Gameplay 루트를 찾지 못했습니다.", coursePrefab); // Gameplay 그룹 누락 로그 출력
                errorCount++; // 오류 개수 증가
            }
            else if (!Mathf.Approximately(gameplayRoot.localPosition.y, ModuleVerticalOffset)) // Gameplay 높이 오프셋 검사
            {
                Debug.LogError("[ProjectJ][Day147] Gameplay Y 오프셋이 5가 아닙니다: " + gameplayRoot.localPosition.y, gameplayRoot); // Gameplay 높이 오류 로그 출력
                errorCount++; // 오류 개수 증가
            }

            if (safetyFloor == null) // 안전 바닥 누락 검사
            {
                Debug.LogError("[ProjectJ][Day147] Day147_SafetyFloor가 없습니다.", coursePrefab); // 안전 바닥 누락 로그 출력
                errorCount++; // 오류 개수 증가
            }
            else // 안전 바닥 존재 처리
            {
                BoxCollider safetyCollider = safetyFloor.GetComponent<BoxCollider>(); // 안전 바닥 Collider 조회

                if (safetyCollider == null) // 안전 바닥 Collider 누락 검사
                {
                    Debug.LogError("[ProjectJ][Day147] 안전 바닥 BoxCollider가 없습니다.", safetyFloor); // Collider 누락 로그 출력
                    errorCount++; // 오류 개수 증가
                }
                else // 안전 바닥 Collider 존재 처리
                {
                    GetBoxColliderWorldYRange(safetyCollider, out _, out float topY); // Prefab Asset에서도 실제 Collider 윗면 계산

                    if (Mathf.Abs(topY - SafetyFloorTopY) > PositionTolerance) // 안전 바닥 윗면 Y 검증
                    {
                        Debug.LogError("[ProjectJ][Day147] 안전 바닥 윗면이 Y=-1이 아닙니다: " + topY, safetyFloor); // 안전 바닥 높이 오류 로그 출력
                        errorCount++; // 오류 개수 증가
                    }
                }
            }

            if (modulesRoot != null) // START 바닥 검증 가능 여부 확인
            {
                Transform startSection = modulesRoot.Find(StartSectionName); // START 구간 조회
                Transform startFloor = startSection != null ? FindDescendantByName(startSection, FloorObjectName) : null; // START Floor 조회
                BoxCollider startFloorCollider = startFloor != null ? startFloor.GetComponent<BoxCollider>() : null; // START Floor Collider 조회

                if (startFloorCollider == null) // START Floor Collider 누락 검사
                {
                    Debug.LogError("[ProjectJ][Day147] START Floor BoxCollider를 찾지 못했습니다.", coursePrefab); // START Floor 누락 로그 출력
                    errorCount++; // 오류 개수 증가
                }
                else // START Floor Collider 존재 처리
                {
                    GetBoxColliderWorldYRange(startFloorCollider, out float bottomY, out _); // Prefab Asset에서도 실제 Collider 아랫면 계산

                    if (Mathf.Abs(bottomY) > PositionTolerance) // START 발판 아랫면 원점 검증
                    {
                        Debug.LogError("[ProjectJ][Day147] START 발판 아랫면이 Y=0이 아닙니다: " + bottomY, startFloor); // START 발판 높이 오류 로그 출력
                        errorCount++; // 오류 개수 증가
                    }
                }
            }

            if (errorCount == 0) // 전체 검증 성공 여부 확인
            {
                Debug.Log("[ProjectJ][Day147] Validation PASS / START Bottom Y=0 / Safety Floor Top Y=-1.", coursePrefab); // 검증 성공 로그 출력
            }
            else // 전체 검증 실패 처리
            {
                Debug.LogError("[ProjectJ][Day147] Validation FAIL / 오류 " + errorCount + "개.", coursePrefab); // 검증 실패 로그 출력
            }
        }

        private static bool PatchCoursePrefab() // Day146 고정 코스 Prefab을 147일차 좌표 규칙으로 패치
        {
            GameObject courseRoot = PrefabUtility.LoadPrefabContents(CoursePrefabPath); // Prefab 편집용 루트 로드

            if (courseRoot == null) // Prefab 편집 루트 로드 실패 검사
            {
                Debug.LogError("[ProjectJ][Day147] Prefab 편집 루트를 열지 못했습니다."); // Prefab 로드 실패 로그 출력
                return false; // 패치 실패 반환
            }

            bool success = false; // Prefab 패치 성공 상태 초기화

            try // Prefab 편집 예외 보호 시작
            {
                Transform modulesRoot = courseRoot.transform.Find(ModulesRootName); // Module 그룹 조회
                Transform gameplayRoot = courseRoot.transform.Find(GameplayRootName); // Gameplay 그룹 조회

                if (modulesRoot == null || gameplayRoot == null) // 필수 루트 누락 검사
                {
                    Debug.LogError("[ProjectJ][Day147] Modules 또는 Gameplay 루트를 찾지 못했습니다.", courseRoot); // 필수 루트 누락 로그 출력
                    return false; // 패치 실패 반환
                }

                modulesRoot.localPosition = new Vector3(0f, ModuleVerticalOffset, 0f); // 전체 Module을 +5m 이동하여 최저 아랫면 Y=0 정렬
                gameplayRoot.localPosition = new Vector3(0f, ModuleVerticalOffset, 0f); // START와 Checkpoint를 Module과 동일하게 +5m 이동
                CreateOrReplaceSafetyFloor(courseRoot.transform, modulesRoot); // 전체 코스 아래 안전 바닥 생성
                PrefabUtility.SaveAsPrefabAsset(courseRoot, CoursePrefabPath); // 변경된 고정 코스 Prefab 저장
                success = true; // Prefab 패치 성공 상태 기록
            }
            finally // Prefab 편집 루트 정리 시작
            {
                PrefabUtility.UnloadPrefabContents(courseRoot); // Prefab 편집용 임시 루트 해제
            }

            return success; // Prefab 패치 결과 반환
        }

        private static void CreateOrReplaceSafetyFloor(Transform courseRoot, Transform modulesRoot) // 전체 고정맵 아래 안전 바닥 생성
        {
            Transform existingFloor = courseRoot.Find(SafetyFloorName); // 기존 안전 바닥 조회

            if (existingFloor != null) // 기존 안전 바닥 존재 검사
            {
                UnityEngine.Object.DestroyImmediate(existingFloor.gameObject); // 중복 안전 바닥 제거
            }

            MapModule[] modules = modulesRoot.GetComponentsInChildren<MapModule>(true); // 전체 고정맵 Module 조회
            float minX = 0f; // 최소 X 초기화
            float maxX = 0f; // 최대 X 초기화
            float minZ = 0f; // 최소 Z 초기화
            float maxZ = 0f; // 최대 Z 초기화

            if (modules.Length > 0) // Module 존재 여부 확인
            {
                Vector3 firstPosition = courseRoot.InverseTransformPoint(modules[0].transform.position); // 첫 Module 코스 로컬 위치 계산
                minX = firstPosition.x - ModuleSize * 0.5f; // 첫 Module 최소 X 설정
                maxX = firstPosition.x + ModuleSize * 0.5f; // 첫 Module 최대 X 설정
                minZ = firstPosition.z - ModuleSize * 0.5f; // 첫 Module 최소 Z 설정
                maxZ = firstPosition.z + ModuleSize * 0.5f; // 첫 Module 최대 Z 설정
            }

            for (int index = 1; index < modules.Length; index++) // 나머지 Module 범위 순회
            {
                Vector3 localPosition = courseRoot.InverseTransformPoint(modules[index].transform.position); // 현재 Module 코스 로컬 위치 계산
                minX = Mathf.Min(minX, localPosition.x - ModuleSize * 0.5f); // 전체 최소 X 갱신
                maxX = Mathf.Max(maxX, localPosition.x + ModuleSize * 0.5f); // 전체 최대 X 갱신
                minZ = Mathf.Min(minZ, localPosition.z - ModuleSize * 0.5f); // 전체 최소 Z 갱신
                maxZ = Mathf.Max(maxZ, localPosition.z + ModuleSize * 0.5f); // 전체 최대 Z 갱신
            }

            float sizeX = Mathf.Max(ModuleSize, maxX - minX) + SafetyFloorPadding * 2f; // 안전 바닥 X 크기 계산
            float sizeZ = Mathf.Max(ModuleSize, maxZ - minZ) + SafetyFloorPadding * 2f; // 안전 바닥 Z 크기 계산
            float centerX = (minX + maxX) * 0.5f; // 안전 바닥 X 중심 계산
            float centerZ = (minZ + maxZ) * 0.5f; // 안전 바닥 Z 중심 계산
            float centerY = SafetyFloorTopY - SafetyFloorThickness * 0.5f; // 안전 바닥 Y 중심 계산
            GameObject safetyFloor = GameObject.CreatePrimitive(PrimitiveType.Cube); // Collider 포함 안전 바닥 Cube 생성
            safetyFloor.name = SafetyFloorName; // 안전 바닥 이름 지정
            safetyFloor.transform.SetParent(courseRoot, false); // 고정 코스 루트 아래 배치
            safetyFloor.transform.localPosition = new Vector3(centerX, centerY, centerZ); // 안전 바닥 중심 좌표 적용
            safetyFloor.transform.localRotation = Quaternion.identity; // 안전 바닥 회전 초기화
            safetyFloor.transform.localScale = new Vector3(sizeX, SafetyFloorThickness, sizeZ); // 고정맵 전체를 덮는 안전 바닥 크기 적용
        }

        private static bool RemoveCourseInstanceFromScene(Scene scene) // Game Scene의 기존 고정 코스 인스턴스 제거
        {
            GameObject[] roots = scene.GetRootGameObjects(); // Scene 루트 오브젝트 조회
            bool removed = false; // 제거 여부 초기화

            for (int index = roots.Length - 1; index >= 0; index--) // Scene 루트 역순 순회
            {
                if (!string.Equals(roots[index].name, CourseRootName, StringComparison.Ordinal)) // 고정 코스 루트 이름 검사
                {
                    continue; // 다른 Scene 오브젝트 보존
                }

                UnityEngine.Object.DestroyImmediate(roots[index]); // 기존 고정 코스 인스턴스 제거
                removed = true; // 제거 여부 기록
            }

            return removed; // 실제 제거 여부 반환
        }

        private static void GetBoxColliderWorldYRange(BoxCollider collider, out float minY, out float maxY) // Prefab Asset BoxCollider의 실제 Y 범위 계산
        {
            Transform colliderTransform = collider.transform; // Collider Transform 조회
            Vector3 center = colliderTransform.TransformPoint(collider.center); // Collider 중심 월드 좌표 계산
            Vector3 lossyScale = colliderTransform.lossyScale; // 계층 전체 Scale 조회
            Vector3 halfSize = new Vector3( // 실제 Collider 반크기 계산
                Mathf.Abs(collider.size.x * lossyScale.x) * 0.5f, // X 반크기 계산
                Mathf.Abs(collider.size.y * lossyScale.y) * 0.5f, // Y 반크기 계산
                Mathf.Abs(collider.size.z * lossyScale.z) * 0.5f // Z 반크기 계산
            );
            float extentY = // 회전을 반영한 월드 Y 반범위 계산
                Mathf.Abs(colliderTransform.right.y) * halfSize.x + // X축의 Y 기여도 계산
                Mathf.Abs(colliderTransform.up.y) * halfSize.y + // Y축의 Y 기여도 계산
                Mathf.Abs(colliderTransform.forward.y) * halfSize.z; // Z축의 Y 기여도 계산
            minY = center.y - extentY; // 실제 아랫면 Y 계산
            maxY = center.y + extentY; // 실제 윗면 Y 계산
        }

        private static Transform FindDescendantByName(Transform root, string objectName) // 하위 Transform 이름 재귀 검색
        {
            if (string.Equals(root.name, objectName, StringComparison.Ordinal)) // 현재 Transform 이름 일치 검사
            {
                return root; // 일치 Transform 반환
            }

            for (int index = 0; index < root.childCount; index++) // 모든 자식 Transform 순회
            {
                Transform found = FindDescendantByName(root.GetChild(index), objectName); // 자식 하위 재귀 검색

                if (found != null) // 검색 성공 여부 확인
                {
                    return found; // 검색된 Transform 반환
                }
            }

            return null; // 검색 실패 반환
        }
    }
}
