using System.IO; // App ID 파일 복사
using UnityEditor; // Editor Build 상태
using UnityEditor.Build; // Build callback
using UnityEditor.Build.Reporting; // Build 결과
using UnityEngine; // Project 경로와 로그

namespace ProjectJ.EditorTools
{
    public sealed class ProjectJDay80SteamAppIdBuildProcessor :
        IPostprocessBuildWithReport
    {
        public int callbackOrder =>
            1000;

        public void OnPostprocessBuild(
            BuildReport report
        )
        {
            string outputDirectory =
                Path.GetDirectoryName(
                    report.summary.outputPath
                );

            if (
                string.IsNullOrEmpty(
                    outputDirectory
                )
            )
            {
                return;
            }

            string destination =
                Path.Combine(
                    outputDirectory,
                    "steam_appid.txt"
                );

            bool isDevelopment =
                (
                    report.summary.options &
                    BuildOptions.Development
                ) != 0;

            if (!isDevelopment)
            {
                if (File.Exists(destination))
                {
                    File.Delete(
                        destination
                    );
                }

                return;
            }

            string projectRoot =
                Path.GetFullPath(
                    Path.Combine(
                        Application.dataPath,
                        ".."
                    )
                );

            string source =
                Path.Combine(
                    projectRoot,
                    "steam_appid.txt"
                );

            if (!File.Exists(source))
            {
                Debug.LogWarning(
                    "[Project J/Day80] 프로젝트 루트의 steam_appid.txt가 없습니다."
                );

                return;
            }

            string appIdText =
                File.ReadAllText(
                    source
                ).Trim();

            if (
                !uint.TryParse(
                    appIdText,
                    out uint appId
                ) ||
                appId == 0u
            )
            {
                Debug.LogWarning(
                    "[Project J/Day80] steam_appid.txt를 실제 Steam App ID로 수정해야 합니다."
                );

                return;
            }

            File.Copy(
                source,
                destination,
                true
            );

            Debug.Log(
                "[Project J/Day80] Development Build에 steam_appid.txt 복사 완료 / AppID: " +
                appId
            );
        }
    }
}
