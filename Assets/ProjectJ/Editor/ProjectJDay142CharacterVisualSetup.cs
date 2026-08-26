using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectJ.Player;
using UnityEditor;
using UnityEngine;

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay142CharacterVisualSetup
    {
        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkPlayer.prefab"; // 실제 Fusion 생성 Prefab 경로
        private const string BotPrefabPath =
            "Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkBot.prefab"; // 실제 Fusion AI Bot Prefab 경로
        private const string ImportedPlayerFolder =
            "Assets/ProjectJ/Prefabs/Player/Imported";
        private const string VisualRootName =
            "VisualRoot";
        private const string LegacyVisualName =
            "Visual"; // 기존 캡슐 표시 자식 이름
        private const int MaxVisualCount = 8;

        [MenuItem(
            "Project J/Day142/1. Setup Character Visual"
        )]
        public static void SetupCharacterVisual() // 메뉴와 Batch 공용 진입점
        {
            GameObject[] characterPrefabs =
                FindCharacterPrefabs(); // 캐릭터 후보 검색

            if (characterPrefabs.Length == 0)
            {
                Debug.LogError(
                    "[Project J/Day142] Character_*.prefab을 찾지 못했습니다."
                ); // 후보 누락
                return;
            }

            bool playerConfigured = SetupPrefabVisual( // Player 외형 설정 시작
                PlayerPrefabPath, // Player Prefab 경로 전달
                characterPrefabs // 등록 외형 후보 전달
            ); // Player 외형 설정 종료
            bool botConfigured = SetupPrefabVisual( // AI Bot 외형 설정 시작
                BotPrefabPath, // AI Bot Prefab 경로 전달
                characterPrefabs // 등록 외형 후보 전달
            ); // AI Bot 외형 설정 종료

            AssetDatabase.SaveAssets(); // Asset 저장
            AssetDatabase.Refresh(); // Project 반영

            if (!playerConfigured || !botConfigured) // 하나 이상의 Prefab 설정 실패 조건
            {
                return; // 일부 설정 실패 종료
            }

            Debug.Log( // 전체 적용 완료 로그
                $"[Project J/Day142] Character Visual 적용 완료 - " + // 완료 로그 앞부분
                $"Player와 AI Bot, 후보 {characterPrefabs.Length}개, " + // 적용 대상과 후보 수
                $"VisualRoot 생성/정리 완료" // 완료 로그 뒷부분
            ); // 전체 적용 완료 로그 종료
        }

        private static bool SetupPrefabVisual( // 공통 Network Prefab 외형 설정
            string prefabPath, // 수정할 Prefab 경로
            GameObject[] characterPrefabs // 등록 외형 후보
        ) // 매개변수 종료
        {
            GameObject prefabRoot =
                PrefabUtility.LoadPrefabContents(
                    prefabPath
                ); // Network Prefab 열기

            if (prefabRoot == null)
            {
                Debug.LogError( // Prefab 누락 로그
                    $"[Project J/Day142] {prefabPath}을 열지 못했습니다." // 누락 Prefab 경로
                ); // Prefab 누락 로그 종료
                return false; // Prefab 설정 실패 반환
            }

            try
            {
                Transform visualRoot =
                    FindOrCreateVisualRoot(
                        prefabRoot.transform
                    ); // Visual Root 준비

                MeshRenderer markerRenderer =
                    PreparePresentationMarker(
                        prefabRoot,
                        visualRoot
                    ); // Presentation 기준 준비

                ProjectJPlayerVisualController controller =
                    prefabRoot.GetComponent<
                        ProjectJPlayerVisualController
                    >(); // Visual Controller 조회

                if (controller == null)
                {
                    controller =
                        prefabRoot.AddComponent<
                            ProjectJPlayerVisualController
                        >(); // Visual Controller 추가
                }

                controller.Configure( // Visual Controller 자동 설정
                    visualRoot, // Visual Root 전달
                    characterPrefabs, // 등록 외형 후보 전달
                    ProjectJPlayerVisualController.ChefVisualName // 검은색 요리사 기본값 전달
                ); // 후보 자동 연결

                EditorUtility.SetDirty(
                    controller
                ); // Component 변경 저장
                EditorUtility.SetDirty(
                    markerRenderer
                ); // Marker 변경 저장

                PrefabUtility.SaveAsPrefabAsset(
                    prefabRoot,
                    prefabPath
                ); // Network Prefab 저장

                return true; // Prefab 설정 성공 반환
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(
                    prefabRoot
                ); // Prefab Stage 해제
            }
        }

        private static GameObject[] FindCharacterPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets(
                "t:Prefab",
                new[]
                {
                    ImportedPlayerFolder
                }
            ); // Imported Prefab 검색

            List<string> paths = guids
                .Select(
                    AssetDatabase.GUIDToAssetPath
                )
                .Where(
                    path =>
                        Path.GetFileNameWithoutExtension(
                            path
                        ).StartsWith(
                            "Character_",
                            StringComparison.Ordinal
                        )
                )
                .OrderBy(
                    path => path,
                    StringComparer.Ordinal
                )
                .Take(
                    MaxVisualCount
                )
                .ToList(); // 최대 8개 정렬

            return paths
                .Select(
                    AssetDatabase.LoadAssetAtPath<GameObject>
                )
                .Where(
                    prefab => prefab != null
                )
                .ToArray(); // 실제 Prefab 로드
        }

        private static Transform FindOrCreateVisualRoot(
            Transform playerRoot
        )
        {
            Transform visualRoot = null; // 기존 Root

            for (
                int index = 0;
                index < playerRoot.childCount;
                index++
            )
            {
                Transform child =
                    playerRoot.GetChild(index); // 직접 자식 조회

                if (child.name == VisualRootName)
                {
                    visualRoot = child; // 기존 Root 재사용
                    break;
                }
            }

            if (visualRoot == null)
            {
                GameObject visualRootObject =
                    new GameObject(
                        VisualRootName
                    ); // Visual Root 생성
                visualRoot =
                    visualRootObject.transform; // Transform 조회
                visualRoot.SetParent(
                    playerRoot,
                    false
                ); // Player 하위 배치
            }

            visualRoot.localPosition =
                new Vector3(0f, 1f, 0f); // 기존 표시 높이 유지
            visualRoot.localRotation =
                Quaternion.identity; // 회전 초기화
            visualRoot.localScale =
                Vector3.one; // Scale 초기화
            visualRoot.gameObject.layer =
                playerRoot.gameObject.layer; // Player Layer 적용

            return visualRoot;
        }

        private static MeshRenderer PreparePresentationMarker(
            GameObject playerRoot,
            Transform visualRoot
        )
        {
            MeshRenderer rootRenderer =
                playerRoot.GetComponent<MeshRenderer>(); // 임시 Renderer 조회
            Transform legacyVisual =
                playerRoot.transform.Find(
                    LegacyVisualName
                ); // 기존 캡슐 표시 자식 조회
            MeshRenderer legacyRenderer =
                legacyVisual != null
                    ? legacyVisual.GetComponent<MeshRenderer>()
                    : null; // 기존 자식 Renderer 조회
            Material[] preservedMaterials =
                rootRenderer != null
                    ? rootRenderer.sharedMaterials
                    : legacyRenderer != null
                        ? legacyRenderer.sharedMaterials
                        : null; // 기존 Material 보존

            MeshRenderer markerRenderer =
                visualRoot.GetComponent<MeshRenderer>(); // Marker 조회

            if (markerRenderer == null)
            {
                markerRenderer =
                    visualRoot.gameObject.AddComponent<
                        MeshRenderer
                    >(); // Marker Renderer 추가
            }

            markerRenderer.enabled = false; // Marker 비표시
            markerRenderer.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off; // 그림자 차단
            markerRenderer.receiveShadows = false; // 그림자 수신 차단

            if (preservedMaterials != null &&
                preservedMaterials.Length > 0)
            {
                markerRenderer.sharedMaterials =
                    preservedMaterials; // 기존 표시 Material 보존
            }

            MeshFilter rootMeshFilter =
                playerRoot.GetComponent<MeshFilter>(); // 임시 Mesh 조회

            if (rootRenderer != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    rootRenderer
                ); // Root 임시 Renderer 제거
            }

            if (rootMeshFilter != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    rootMeshFilter
                ); // Root 임시 Mesh 제거
            }

            if (legacyVisual != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    legacyVisual.gameObject
                ); // 기존 캡슐 표시 자식 제거
            }

            return markerRenderer;
        }
    }
}
