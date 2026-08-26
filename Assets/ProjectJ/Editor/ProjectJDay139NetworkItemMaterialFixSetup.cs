using System; // 문자열 비교 사용
using ProjectJ.Items; // Material 호환 정책 사용
using UnityEditor; // Asset과 Prefab 수정 사용
using UnityEngine; // Material과 Renderer 사용

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay139NetworkItemMaterialFixSetup
    {
        private const string NetworkResourceFolder =
            "Assets/ProjectJ/Network/Fusion/Player/Resources"; // Network Prefab 검색 폴더

        private const string ArtFolder =
            "Assets/ProjectJ/Art"; // Project J Art Root

        private const string MaterialFolder =
            "Assets/ProjectJ/Art/Materials"; // Runtime Material 저장 폴더

        private const string FallbackMaterialPath =
            "Assets/ProjectJ/Art/Materials/ProjectJ_NetworkItemPlaceholder_URP.mat"; // Network Item URP Material 경로

        private const string PlayerPrefabName =
            "ProjectJNetworkPlayer"; // Player Prefab 제외 이름

        private const string BotPrefabName =
            "ProjectJNetworkBot"; // Bot Prefab 제외 이름

        [MenuItem(
            "Project J/Day139/Fix Network Item Purple Materials"
        )]
        private static void FixNetworkItemPurpleMaterials()
        {
            Material fallbackMaterial =
                GetOrCreateFallbackMaterial(); // URP 호환 공통 Material 준비

            if (fallbackMaterial == null)
            {
                return; // Material 생성 실패 시 Prefab 수정 중단
            }

            string[] prefabGuids =
                AssetDatabase.FindAssets(
                    "t:Prefab",
                    new[]
                    {
                        NetworkResourceFolder
                    }
                ); // Network Resources Prefab 검색

            int changedPrefabCount =
                0; // 수정 Prefab 수 초기화

            int replacedMaterialSlotCount =
                0; // 교체 Material Slot 수 초기화

            for (
                int prefabIndex = 0;
                prefabIndex < prefabGuids.Length;
                prefabIndex++
            )
            {
                string prefabPath =
                    AssetDatabase.GUIDToAssetPath(
                        prefabGuids[prefabIndex]
                    ); // Prefab Asset 경로 변환

                string prefabName =
                    System.IO.Path.GetFileNameWithoutExtension(
                        prefabPath
                    ); // Prefab 이름 조회

                if (
                    string.Equals(
                        prefabName,
                        PlayerPrefabName,
                        StringComparison.Ordinal
                    ) ||
                    string.Equals(
                        prefabName,
                        BotPrefabName,
                        StringComparison.Ordinal
                    )
                )
                {
                    continue; // Player와 Bot Visual Material 자동 교체 제외
                }

                GameObject prefabRoot =
                    PrefabUtility.LoadPrefabContents(
                        prefabPath
                    ); // Prefab 편집 인스턴스 로드

                if (prefabRoot == null)
                {
                    continue; // 로드 실패 Prefab 제외
                }

                bool prefabChanged =
                    false; // 현재 Prefab 변경 여부 초기화

                try
                {
                    Renderer[] renderers =
                        prefabRoot.GetComponentsInChildren<Renderer>(
                            true
                        ); // Prefab 전체 Renderer 검색

                    for (
                        int rendererIndex = 0;
                        rendererIndex < renderers.Length;
                        rendererIndex++
                    )
                    {
                        Renderer renderer =
                            renderers[rendererIndex]; // 현재 Renderer 조회

                        if (renderer == null)
                        {
                            continue; // 누락 Renderer 제외
                        }

                        Material[] sharedMaterials =
                            renderer.sharedMaterials; // Serialized Material Slot 복사

                        bool rendererChanged =
                            false; // 현재 Renderer 변경 여부 초기화

                        for (
                            int materialIndex = 0;
                            materialIndex < sharedMaterials.Length;
                            materialIndex++
                        )
                        {
                            Material currentMaterial =
                                sharedMaterials[materialIndex]; // 현재 Material Slot 조회

                            if (
                                !ShouldReplaceMaterial(
                                    currentMaterial
                                )
                            )
                            {
                                continue; // URP 호환 Material 유지
                            }

                            sharedMaterials[materialIndex] =
                                fallbackMaterial; // URP 호환 Material 적용

                            rendererChanged =
                                true; // Renderer 변경 기록

                            prefabChanged =
                                true; // Prefab 변경 기록

                            replacedMaterialSlotCount++; // 교체 Slot 수 증가
                        }

                        if (rendererChanged)
                        {
                            renderer.sharedMaterials =
                                sharedMaterials; // 수정 Material 배열 저장

                            EditorUtility.SetDirty(
                                renderer
                            ); // Renderer 변경 표시
                        }
                    }

                    if (prefabChanged)
                    {
                        PrefabUtility.SaveAsPrefabAsset(
                            prefabRoot,
                            prefabPath
                        ); // 수정 Network Prefab 저장

                        changedPrefabCount++; // 수정 Prefab 수 증가
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(
                        prefabRoot
                    ); // Prefab 편집 인스턴스 해제
                }
            }

            AssetDatabase.SaveAssets(); // Material과 Prefab 변경 저장
            AssetDatabase.Refresh(); // 변경 Asset 재임포트

            Debug.Log(
                "[Project J/Day139] Network Item 보라색 Material 수정 완료 / Prefab: " +
                changedPrefabCount +
                " / Material Slot: " +
                replacedMaterialSlotCount
            ); // 자동 교체 결과 출력
        }

        private static Material GetOrCreateFallbackMaterial()
        {
            Material existingMaterial =
                AssetDatabase.LoadAssetAtPath<Material>(
                    FallbackMaterialPath
                ); // 기존 URP Placeholder Material 조회

            if (
                existingMaterial != null &&
                existingMaterial.shader != null &&
                existingMaterial.shader.isSupported &&
                !ProjectJNetworkItemMaterialPolicy.IsKnownIncompatibleShaderName(
                    existingMaterial.shader.name
                )
            )
            {
                return existingMaterial; // 기존 정상 URP Material 재사용
            }

            EnsureFolder(
                "Assets/ProjectJ",
                "Art"
            ); // Art 폴더 보장

            EnsureFolder(
                ArtFolder,
                "Materials"
            ); // Material 폴더 보장

            Shader urpShader =
                Shader.Find(
                    "Universal Render Pipeline/Lit"
                ); // URP Lit Shader 우선 검색

            if (
                urpShader == null ||
                !urpShader.isSupported
            )
            {
                urpShader =
                    Shader.Find(
                        "Universal Render Pipeline/Simple Lit"
                    ); // URP Simple Lit 대체 검색
            }

            if (
                urpShader == null ||
                !urpShader.isSupported
            )
            {
                Debug.LogError(
                    "[Project J/Day139] URP Lit Shader를 찾지 못해 Network Item Material 수정을 중단했습니다."
                ); // URP Shader 누락 오류 출력

                return null;
            }

            if (existingMaterial != null)
            {
                existingMaterial.shader =
                    urpShader; // 기존 깨진 Placeholder Shader 복구

                ApplyFallbackAppearance(
                    existingMaterial
                ); // 기존 Material 표시값 복구

                EditorUtility.SetDirty(
                    existingMaterial
                ); // 기존 Material 변경 표시

                AssetDatabase.SaveAssets(); // 기존 Material 수정 저장

                return existingMaterial; // 복구 Material 반환
            }

            Material createdMaterial =
                new Material(
                    urpShader
                ); // URP Placeholder Material 생성

            createdMaterial.name =
                "ProjectJ_NetworkItemPlaceholder_URP"; // Material 이름 지정

            ApplyFallbackAppearance(
                createdMaterial
            ); // 기본 표시값 적용

            AssetDatabase.CreateAsset(
                createdMaterial,
                FallbackMaterialPath
            ); // Project J 전용 Material Asset 생성

            AssetDatabase.SaveAssets(); // 신규 Material 저장

            return createdMaterial; // 신규 Material 반환
        }

        private static void ApplyFallbackAppearance(
            Material material
        )
        {
            if (material == null)
            {
                return; // Material 누락 처리
            }

            Color placeholderColor =
                new Color(
                    0.78f,
                    0.82f,
                    0.9f,
                    1f
                ); // 임시 Network Item 중립 색상

            if (
                material.HasProperty(
                    "_BaseColor"
                )
            )
            {
                material.SetColor(
                    "_BaseColor",
                    placeholderColor
                ); // URP Base Color 적용
            }

            if (
                material.HasProperty(
                    "_Color"
                )
            )
            {
                material.SetColor(
                    "_Color",
                    placeholderColor
                ); // 호환 Color Property 적용
            }

            if (
                material.HasProperty(
                    "_Metallic"
                )
            )
            {
                material.SetFloat(
                    "_Metallic",
                    0f
                ); // Placeholder 금속성 제거
            }

            if (
                material.HasProperty(
                    "_Smoothness"
                )
            )
            {
                material.SetFloat(
                    "_Smoothness",
                    0.25f
                ); // Placeholder 반사도 완화
            }
        }

        private static bool ShouldReplaceMaterial(
            Material material
        )
        {
            if (material == null)
            {
                return true; // 누락 Material 교체
            }

            string assetPath =
                AssetDatabase.GetAssetPath(
                    material
                ); // Material Asset 경로 조회

            if (
                ProjectJNetworkItemMaterialPolicy.IsBuiltinDefaultMaterialPath(
                    assetPath
                )
            )
            {
                return true; // Unity Built-in Default Material 교체
            }

            Shader shader =
                material.shader; // 현재 Material Shader 조회

            if (
                shader == null ||
                !shader.isSupported
            )
            {
                return true; // 누락 또는 현재 Pipeline 미지원 Shader 교체
            }

            return ProjectJNetworkItemMaterialPolicy.IsKnownIncompatibleShaderName(
                shader.name
            ); // Standard·Legacy·Error Shader 교체
        }

        private static void EnsureFolder(
            string parentFolder,
            string childFolderName
        )
        {
            string childPath =
                parentFolder +
                "/" +
                childFolderName; // 대상 폴더 전체 경로 계산

            if (
                AssetDatabase.IsValidFolder(
                    childPath
                )
            )
            {
                return; // 기존 폴더 유지
            }

            AssetDatabase.CreateFolder(
                parentFolder,
                childFolderName
            ); // 누락 Project 폴더 생성
        }
    }
}
