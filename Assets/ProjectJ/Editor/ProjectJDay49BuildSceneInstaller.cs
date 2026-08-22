using System.Collections.Generic; // Build Scene 목록 편집
using System.IO; // Scene 파일명 비교
using UnityEditor; // AssetDatabase와 EditorBuildSettings 사용
using UnityEngine; // Debug 출력

namespace ProjectJ.EditorTools
{
    [InitializeOnLoad]
    public static class ProjectJDay49BuildSceneInstaller
    {
        private const string TargetSceneName =
            "Day49_AllSystemsTest"; // 자동 등록할 테스트 Scene

        static ProjectJDay49BuildSceneInstaller()
        {
            EditorApplication.delayCall +=
                EnsureTargetSceneInBuildSettings; // Script 컴파일 후 한 번 등록 시도
        }

        private static void EnsureTargetSceneInBuildSettings()
        {
            string targetPath =
                FindTargetScenePath();

            if (string.IsNullOrEmpty(targetPath))
            {
                Debug.LogError(
                    "[Project J] Day49_AllSystemsTest Scene을 프로젝트에서 찾지 못했습니다."
                );

                return;
            }

            EditorBuildSettingsScene[] currentScenes =
                EditorBuildSettings.scenes;

            List<EditorBuildSettingsScene> scenes =
                new List<EditorBuildSettingsScene>(
                    currentScenes
                );

            for (
                int index = 0;
                index < scenes.Count;
                index++
            )
            {
                EditorBuildSettingsScene scene =
                    scenes[index];

                if (scene.path != targetPath)
                {
                    continue;
                }

                if (!scene.enabled)
                {
                    scenes[index] =
                        new EditorBuildSettingsScene(
                            targetPath,
                            true
                        );

                    EditorBuildSettings.scenes =
                        scenes.ToArray();

                    Debug.Log(
                        "[Project J] Day49_AllSystemsTest Build Scene 활성화 완료"
                    );
                }

                return; // 이미 등록되어 있으면 추가 작업 없음
            }

            scenes.Add(
                new EditorBuildSettingsScene(
                    targetPath,
                    true
                )
            );

            EditorBuildSettings.scenes =
                scenes.ToArray();

            Debug.Log(
                "[Project J] Day49_AllSystemsTest Build Settings 자동 등록 완료 / " +
                targetPath
            );
        }

        private static string FindTargetScenePath()
        {
            string[] guids =
                AssetDatabase.FindAssets(
                    TargetSceneName + " t:Scene"
                );

            for (
                int index = 0;
                index < guids.Length;
                index++
            )
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(
                        guids[index]
                    );

                if (
                    Path.GetFileNameWithoutExtension(
                        path
                    ) == TargetSceneName
                )
                {
                    return path; // 폴더 위치와 무관하게 정확한 이름 Scene 반환
                }
            }

            return string.Empty;
        }
    }
}
