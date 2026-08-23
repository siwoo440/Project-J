using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ProjectJ.EditorTools
{
    public static class
        ProjectJDay97DedicatedServerBuilder
    {
        private const string ServerScenePath =
            "Assets/ProjectJ/Scenes/Day96_ServerModeTest.unity";

        private const string OutputDirectory =
            "Build/Server/Windows";

        private const string OutputExecutable =
            OutputDirectory +
            "/ProjectJ_Server.exe";

        [MenuItem(
            "Project J/Build/97일차 Windows Dedicated Server Build"
        )]
        private static void BuildServer()
        {
            SceneAsset serverScene =
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    ServerScenePath
                );

            if (serverScene == null)
            {
                Debug.LogError(
                    "[Project J/Day97] " +
                    "Server 시작 Scene을 찾지 못했습니다. / " +
                    ServerScenePath
                );

                return;
            }

            Directory.CreateDirectory(
                OutputDirectory
            ); // Server Build 출력 폴더 생성

            BuildPlayerOptions buildOptions =
                new BuildPlayerOptions
                {
                    scenes =
                        new[]
                        {
                            ServerScenePath
                        },
                    locationPathName =
                        OutputExecutable,
                    target =
                        BuildTarget
                            .StandaloneWindows64,
                    subtarget =
                        (int)
                        StandaloneBuildSubtarget
                            .Server,
                    options =
                        BuildOptions
                            .Development
                };

            Debug.Log(
                "[Project J/Day97] " +
                "Windows Dedicated Server Build 시작 / " +
                OutputExecutable
            );

            BuildReport report =
                BuildPipeline.BuildPlayer(
                    buildOptions
                );

            BuildSummary summary =
                report.summary;

            if (
                summary.result !=
                BuildResult.Succeeded
            )
            {
                Debug.LogError(
                    "[Project J/Day97] " +
                    "Dedicated Server Build 실패 / " +
                    "Result: " +
                    summary.result +
                    " / Errors: " +
                    summary.totalErrors
                );

                return;
            }

            Debug.Log(
                "[Project J/Day97] " +
                "Dedicated Server Build 완료 / " +
                OutputExecutable +
                " / Size: " +
                summary.totalSize +
                " bytes / Time: " +
                summary.totalTime
            );
        }

        [MenuItem(
            "Project J/Build/97일차 Windows Dedicated Server 폴더 열기"
        )]
        private static void OpenServerFolder()
        {
            string fullPath =
                Path.GetFullPath(
                    OutputDirectory
                );

            if (
                !Directory.Exists(
                    fullPath
                )
            )
            {
                Debug.LogWarning(
                    "[Project J/Day97] " +
                    "아직 Server Build 폴더가 없습니다."
                );

                return;
            }

            EditorUtility.RevealInFinder(
                fullPath
            );
        }
    }
}
