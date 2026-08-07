using System; // 문자열 비교와 예외 기능 참조
using System.Collections.Generic; // 검증 오류 목록 기능 참조
using ProjectJ.Core.Physics; // Project J 물리 레이어 이름과 충돌 규칙 참조
using UnityEditor; // Unity 프로젝트 설정 에셋 편집 기능 참조
using UnityEngine; // Unity Object와 LayerMask 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal static class ProjectPhysicsLayerEditorUtility // Project J 물리 레이어 프로젝트 설정 편집 도구 선언
    {
        private const string TagManagerPath = "ProjectSettings/TagManager.asset"; // Unity 태그와 레이어 설정 파일 경로 선언
        private const string PhysicsManagerPath = "ProjectSettings/DynamicsManager.asset"; // Unity 3D 물리 설정 파일 경로 선언

        internal static void Configure() // Project J 전용 레이어 이름과 충돌 행렬 구성
        {
            ConfigureLayerNames(); // 고정 번호에 Project J 전용 레이어 이름 적용
            AssetDatabase.SaveAssets(); // 레이어 이름 변경 내용 저장
            AssetDatabase.Refresh(); // Unity 레이어 이름 캐시와 Project 창 새로고침
            ConfigureCollisionMatrix(); // Project J 전용 레이어 사이 충돌 행렬 적용
            MarkProjectSettingsDirty(PhysicsManagerPath); // 3D 물리 설정 파일 변경 상태 표시
            AssetDatabase.SaveAssets(); // 레이어와 물리 설정 변경 내용 저장
            AssetDatabase.Refresh(); // 변경된 프로젝트 설정 다시 불러오기
        }

        internal static List<string> CollectValidationErrors() // 현재 레이어 이름과 충돌 행렬의 설정 오류 목록 반환
        {
            List<string> errors = new List<string>(); // 검증 오류 목록 생성

            foreach (ProjectPhysicsLayer layer in ProjectPhysicsLayers.All) // 모든 Project J 전용 레이어 순회
            {
                int layerIndex = ProjectPhysicsLayers.GetIndex(layer); // 현재 레이어의 고정 번호 조회
                string expectedName = ProjectPhysicsLayers.GetName(layer); // 현재 레이어의 예상 이름 조회
                string actualName = LayerMask.LayerToName(layerIndex); // 프로젝트에 등록된 실제 레이어 이름 조회

                if (string.Equals(actualName, expectedName, StringComparison.Ordinal)) // 실제 이름과 예상 이름 일치 여부 확인
                {
                    continue; // 정상 레이어는 다음 검사로 이동
                }

                errors.Add($"Layer {layerIndex}: 예상 이름 '{expectedName}', 현재 이름 '{actualName}'"); // 레이어 이름 불일치 오류 추가
            }

            IReadOnlyList<ProjectPhysicsLayer> layers = ProjectPhysicsLayers.All; // Project J 전용 레이어 전체 목록 조회

            for (int firstIndex = 0; firstIndex < layers.Count; firstIndex++) // 첫 번째 충돌 레이어 순회
            {
                for (int secondIndex = firstIndex; secondIndex < layers.Count; secondIndex++) // 중복 조합을 제외한 두 번째 충돌 레이어 순회
                {
                    ProjectPhysicsLayer firstLayer = layers[firstIndex]; // 첫 번째 프로젝트 물리 레이어 조회
                    ProjectPhysicsLayer secondLayer = layers[secondIndex]; // 두 번째 프로젝트 물리 레이어 조회
                    bool expectedCollision = ProjectPhysicsCollisionRules.ShouldCollide(firstLayer, secondLayer); // 코드에 정의된 예상 충돌 여부 조회
                    bool actualCollision = !UnityEngine.Physics.GetIgnoreLayerCollision( // Unity 3D 물리 충돌 행렬의 실제 충돌 여부 조회
                        ProjectPhysicsLayers.GetIndex(firstLayer), // 첫 번째 Unity 레이어 번호 전달
                        ProjectPhysicsLayers.GetIndex(secondLayer)); // 두 번째 Unity 레이어 번호 전달

                    if (actualCollision == expectedCollision) // 실제 충돌 여부와 예상 규칙 일치 여부 확인
                    {
                        continue; // 정상 충돌 조합은 다음 검사로 이동
                    }

                    errors.Add($"{ProjectPhysicsLayers.GetName(firstLayer)} ↔ {ProjectPhysicsLayers.GetName(secondLayer)}: 예상 충돌 {expectedCollision}, 실제 충돌 {actualCollision}"); // 충돌 행렬 불일치 오류 추가
                }
            }

            return errors; // 수집된 모든 설정 오류 반환
        }

        private static void ConfigureLayerNames() // TagManager의 사용자 레이어 8~15 이름 구성
        {
            UnityEngine.Object tagManagerAsset = LoadProjectSettingsAsset(TagManagerPath); // Unity 태그와 레이어 설정 에셋 불러오기
            SerializedObject tagManager = new SerializedObject(tagManagerAsset); // 태그와 레이어 설정 직렬화 객체 생성
            SerializedProperty layersProperty = tagManager.FindProperty("layers"); // Unity 레이어 이름 배열 프로퍼티 조회

            if (layersProperty == null || !layersProperty.isArray || layersProperty.arraySize < 32) // 레이어 이름 배열의 존재와 크기 확인
            {
                throw new InvalidOperationException("TagManager.asset의 layers 배열을 찾을 수 없습니다."); // Unity 레이어 설정 구조 오류 발생
            }

            tagManager.Update(); // 현재 프로젝트 레이어 설정값 다시 읽기

            foreach (ProjectPhysicsLayer layer in ProjectPhysicsLayers.All) // 모든 Project J 전용 레이어 순회
            {
                int layerIndex = ProjectPhysicsLayers.GetIndex(layer); // 적용할 Unity 레이어 번호 조회
                string expectedName = ProjectPhysicsLayers.GetName(layer); // 적용할 Unity 레이어 이름 조회
                SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(layerIndex); // 지정 번호의 레이어 이름 프로퍼티 조회
                string currentName = layerProperty.stringValue; // 현재 등록된 레이어 이름 조회

                if (!string.IsNullOrWhiteSpace(currentName) // 현재 레이어 번호가 이미 사용 중인지 확인
                    && !string.Equals(currentName, expectedName, StringComparison.Ordinal)) // 기존 이름이 Project J 예상 이름과 다른지 확인
                {
                    throw new InvalidOperationException($"Layer {layerIndex}는 이미 '{currentName}' 이름으로 사용 중입니다. 자동으로 덮어쓰지 않습니다."); // 기존 사용자 레이어 보호 예외 발생
                }

                layerProperty.stringValue = expectedName; // 지정 번호에 Project J 레이어 이름 적용
            }

            tagManager.ApplyModifiedPropertiesWithoutUndo(); // 레이어 이름 변경 내용을 프로젝트 설정에 적용
            EditorUtility.SetDirty(tagManagerAsset); // TagManager 설정 에셋 변경 상태 표시
        }

        private static void ConfigureCollisionMatrix() // Project J 전용 레이어 사이 3D 물리 충돌 행렬 구성
        {
            IReadOnlyList<ProjectPhysicsLayer> layers = ProjectPhysicsLayers.All; // Project J 전용 레이어 전체 목록 조회

            for (int firstIndex = 0; firstIndex < layers.Count; firstIndex++) // 첫 번째 충돌 레이어 순회
            {
                for (int secondIndex = firstIndex; secondIndex < layers.Count; secondIndex++) // 중복 조합을 제외한 두 번째 충돌 레이어 순회
                {
                    ProjectPhysicsLayer firstLayer = layers[firstIndex]; // 첫 번째 프로젝트 물리 레이어 조회
                    ProjectPhysicsLayer secondLayer = layers[secondIndex]; // 두 번째 프로젝트 물리 레이어 조회
                    bool shouldCollide = ProjectPhysicsCollisionRules.ShouldCollide(firstLayer, secondLayer); // 코드에 정의된 충돌 허용 여부 조회

                    UnityEngine.Physics.IgnoreLayerCollision( // Unity 3D 물리 충돌 행렬에 현재 조합 적용
                        ProjectPhysicsLayers.GetIndex(firstLayer), // 첫 번째 Unity 레이어 번호 전달
                        ProjectPhysicsLayers.GetIndex(secondLayer), // 두 번째 Unity 레이어 번호 전달
                        !shouldCollide); // 충돌 허용 규칙을 Ignore 값으로 반전하여 전달
                }
            }
        }

        private static UnityEngine.Object LoadProjectSettingsAsset(string assetPath) // 지정 경로의 Unity 프로젝트 설정 에셋 불러오기
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath); // 지정 프로젝트 설정 경로의 모든 에셋 불러오기

            if (assets == null || assets.Length == 0 || assets[0] == null) // 프로젝트 설정 에셋 존재 여부 확인
            {
                throw new InvalidOperationException($"프로젝트 설정 에셋을 불러올 수 없습니다: {assetPath}"); // 프로젝트 설정 에셋 누락 예외 발생
            }

            return assets[0]; // 첫 번째 프로젝트 설정 에셋 반환
        }

        private static void MarkProjectSettingsDirty(string assetPath) // 지정 프로젝트 설정 에셋 변경 상태 표시
        {
            UnityEngine.Object settingsAsset = LoadProjectSettingsAsset(assetPath); // 지정 프로젝트 설정 에셋 불러오기
            EditorUtility.SetDirty(settingsAsset); // 프로젝트 설정 에셋 변경 상태 표시
        }
    }
}
