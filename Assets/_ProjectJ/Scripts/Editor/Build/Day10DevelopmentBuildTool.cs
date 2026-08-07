using System; // 날짜와 예외 기능 참조
using System.Collections.Generic; // 검증 오류 목록 기능 참조
using System.IO; // 빌드 폴더와 로그 파일 입출력 기능 참조
using System.Linq; // 스크립팅 정의 중복 제거 기능 참조
using System.Text; // 빌드 요약 로그 문자열 생성 기능 참조
using ProjectJ.Build; // Project J 빌드 경로와 씬 목록 참조
using ProjectJ.Diagnostics; // Project J 공통 로그 출력 기능 참조
using UnityEditor; // Unity 에디터 메뉴와 빌드 설정 기능 참조
using UnityEditor.Build; // Unity 빌드 실패 예외 기능 참조
using UnityEditor.Build.Profile; // Unity Build Profile 기능 참조
using UnityEditor.Build.Reporting; // Unity 빌드 결과와 요약 기능 참조
using UnityEngine; // Unity 애플리케이션 경로와 대화상자 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    public static class Day10DevelopmentBuildTool // 10일차 Windows 개발 Build Profile 구성과 빌드 도구 선언
    {
        private const string ConfigureMenuPath = ProjectJEditorMenuPaths.DevelopmentBuild + "/개발 Build Profile 구성 (Day 10일차)"; // 개발 Build Profile 자동 구성 메뉴 경로 선언
        private const string ValidateMenuPath = ProjectJEditorMenuPaths.DevelopmentBuild + "/개발 Build Profile 검증 (Day 10일차)"; // 개발 Build Profile 검증 메뉴 경로 선언
        private const string BuildMenuPath = ProjectJEditorMenuPaths.DevelopmentBuild + "/개발 클라이언트 빌드 (Day 10일차)"; // 개발 클라이언트 빌드 메뉴 경로 선언
        private const string BuildAndRunMenuPath = ProjectJEditorMenuPaths.DevelopmentBuild + "/개발 클라이언트 빌드 후 실행 (Day 10일차)"; // 개발 클라이언트 빌드 후 실행 메뉴 경로 선언
        private const string OpenSummaryMenuPath = ProjectJEditorMenuPaths.DevelopmentBuild + "/최신 빌드 요약 열기 (Day 10일차)"; // 최신 빌드 요약 로그 열기 메뉴 경로 선언

        [MenuItem(ConfigureMenuPath)] // Unity 상단 메뉴에 개발 Build Profile 구성 항목 등록
        private static void ConfigureDevelopmentProfile() // 기존 Windows Build Profile 에셋에 10일차 개발 설정 적용
        {
            BuildProfile profile = ProjectDevelopmentBuildValidator.LoadDevelopmentProfile(); // 고정 경로의 Windows 개발 Build Profile 불러오기

            if (profile == null) // 개발 Build Profile 에셋 존재 여부 확인
            {
                string missingMessage = $"먼저 File > Build Profiles에서 Windows Build Profile을 만들고 다음 경로에 저장합니다.\n{ProjectBuildConfiguration.DevelopmentProfileAssetPath}"; // Build Profile 생성 안내 메시지 생성
                ProjectLog.Error(ProjectLogCategory.Core, missingMessage, "BUILD_PROFILE_MISSING"); // 공통 로그 규칙을 사용하는 Build Profile 누락 오류 출력
                EditorUtility.DisplayDialog("Project J Day 10", missingMessage, "확인"); // Build Profile 생성 안내 대화상자 표시
                return; // 개발 Build Profile 구성 작업 중단
            }

            Undo.RecordObject(profile, "Configure Project J Development Build Profile"); // Build Profile 설정 변경 Undo 기록
            profile.overrideGlobalScenes = true; // 개발 프로필 전용 씬 목록 오버라이드 활성화
            profile.scenes = ProjectBuildConfiguration.DevelopmentScenePaths // 개발 클라이언트 씬 경로 목록 조회
                .Select(scenePath => new EditorBuildSettingsScene(scenePath, true)) // 각 씬을 활성 Build Settings 항목으로 변환
                .ToArray(); // Build Profile 씬 배열 생성

            string[] existingDefines = profile.scriptingDefines ?? Array.Empty<string>(); // 기존 Build Profile 스크립팅 정의 또는 빈 배열 조회
            profile.scriptingDefines = existingDefines // 기존 스크립팅 정의 목록 조회
                .Append(ProjectBuildConfiguration.DevelopmentScriptingDefine) // Project J 개발 전용 정의 추가
                .Where(define => !string.IsNullOrWhiteSpace(define)) // 비어 있는 스크립팅 정의 제외
                .Distinct(StringComparer.Ordinal) // 동일한 스크립팅 정의 중복 제거
                .ToArray(); // 정리된 스크립팅 정의 배열 적용

            BuildProfile.SetActiveBuildProfile(profile); // Windows 개발 Build Profile을 현재 활성 Profile로 설정
            EditorUserBuildSettings.development = true; // Development Build 활성화
            EditorUserBuildSettings.allowDebugging = true; // 원격 Script Debugging 활성화
            EditorUserBuildSettings.connectProfiler = false; // 기본 실행 시 Profiler 자동 연결 비활성화
            EditorUserBuildSettings.buildWithDeepProfilingSupport = false; // 기본 실행 시 Deep Profiling 비활성화
            EditorUserBuildSettings.waitForManagedDebugger = false; // 플레이어 시작 시 Managed Debugger 연결 대기 비활성화

            EditorUtility.SetDirty(profile); // Build Profile 에셋 변경 상태 표시
            AssetDatabase.SaveAssets(); // 변경된 Build Profile 에셋 저장
            AssetDatabase.Refresh(); // Project 창과 Build Profile 데이터 새로고침

            List<string> errors = ProjectDevelopmentBuildValidator.CollectValidationErrors(profile, true); // 구성 직후 개발 Build Profile 전체 검증

            if (errors.Count > 0) // 개발 Build Profile 검증 오류 존재 여부 확인
            {
                LogValidationErrors(errors); // 발견된 모든 Build Profile 오류 Console 출력
                EditorUtility.DisplayDialog("Project J Day 10", $"개발 Build Profile 구성 후 오류 {errors.Count}개를 발견했습니다. Console을 확인합니다.", "확인"); // Build Profile 구성 실패 대화상자 표시
                return; // 성공 로그와 대화상자 표시 생략
            }

            ProjectLog.Info(ProjectLogCategory.Core, "Windows 개발 Build Profile 구성을 완료했습니다.", "BUILD_PROFILE_READY"); // 개발 Build Profile 구성 완료 로그 출력
            EditorUtility.DisplayDialog("Project J Day 10", "Windows 개발 Build Profile 구성을 완료했습니다.", "확인"); // 개발 Build Profile 구성 성공 대화상자 표시
        }

        [MenuItem(ValidateMenuPath)] // Unity 상단 메뉴에 개발 Build Profile 검증 항목 등록
        private static void ValidateDevelopmentProfile() // 현재 Windows 개발 Build Profile 설정 검증
        {
            BuildProfile profile = ProjectDevelopmentBuildValidator.LoadDevelopmentProfile(); // 고정 경로의 Windows 개발 Build Profile 불러오기
            List<string> errors = ProjectDevelopmentBuildValidator.CollectValidationErrors(profile, true); // 개발 Build Profile 전체 검증 오류 수집

            if (errors.Count == 0) // 개발 Build Profile 검증 오류가 없는지 확인
            {
                ProjectLog.Info(ProjectLogCategory.Core, "Windows 개발 Build Profile 검증을 통과했습니다.", "BUILD_PROFILE_VALID"); // 개발 Build Profile 검증 성공 로그 출력
                EditorUtility.DisplayDialog("Project J Day 10", "Windows 개발 Build Profile 설정이 정상입니다.", "확인"); // 개발 Build Profile 검증 성공 대화상자 표시
                return; // 오류 로그 처리 생략
            }

            LogValidationErrors(errors); // 발견된 모든 Build Profile 오류 Console 출력
            EditorUtility.DisplayDialog("Project J Day 10", $"개발 Build Profile 오류 {errors.Count}개를 발견했습니다. Console을 확인합니다.", "확인"); // 개발 Build Profile 검증 실패 대화상자 표시
        }

        [MenuItem(BuildMenuPath)] // Unity 상단 메뉴에 개발 클라이언트 빌드 항목 등록
        private static void BuildDevelopmentClient() // Windows 개발 클라이언트를 빌드하고 실행하지 않음
        {
            BuildDevelopmentClientInternal(false); // 자동 실행 없이 Windows 개발 클라이언트 빌드
        }

        [MenuItem(BuildAndRunMenuPath)] // Unity 상단 메뉴에 개발 클라이언트 빌드 후 실행 항목 등록
        private static void BuildAndRunDevelopmentClient() // Windows 개발 클라이언트를 빌드한 뒤 자동 실행
        {
            BuildDevelopmentClientInternal(true); // 자동 실행을 포함하여 Windows 개발 클라이언트 빌드
        }

        [MenuItem(OpenSummaryMenuPath)] // Unity 상단 메뉴에 최신 빌드 요약 로그 열기 항목 등록
        private static void OpenLatestBuildSummary() // 마지막 개발 빌드 요약 로그 파일 열기
        {
            string summaryPath = GetAbsoluteProjectPath(ProjectBuildConfiguration.DevelopmentBuildSummaryPath); // 빌드 요약 로그 절대 경로 생성

            if (!File.Exists(summaryPath)) // 빌드 요약 로그 파일 존재 여부 확인
            {
                ProjectLog.Warning(ProjectLogCategory.Core, $"빌드 요약 로그가 없습니다: {summaryPath}", "BUILD_SUMMARY_MISSING"); // 빌드 요약 로그 누락 경고 출력
                EditorUtility.DisplayDialog("Project J Day 10", "아직 생성된 개발 빌드 요약 로그가 없습니다.", "확인"); // 빌드 요약 로그 누락 대화상자 표시
                return; // 로그 파일 열기 작업 중단
            }

            EditorUtility.OpenWithDefaultApp(summaryPath); // 운영체제 기본 프로그램으로 빌드 요약 로그 열기
        }

        [MenuItem(ConfigureMenuPath, true)] // 개발 Build Profile 구성 메뉴 활성 조건 등록
        [MenuItem(ValidateMenuPath, true)] // 개발 Build Profile 검증 메뉴 활성 조건 등록
        [MenuItem(BuildMenuPath, true)] // 개발 클라이언트 빌드 메뉴 활성 조건 등록
        [MenuItem(BuildAndRunMenuPath, true)] // 개발 클라이언트 빌드 후 실행 메뉴 활성 조건 등록
        [MenuItem(OpenSummaryMenuPath, true)] // 최신 빌드 요약 로그 메뉴 활성 조건 등록
        private static bool ValidateEditorMenu() // Play Mode와 빌드 실행 중이 아닐 때만 10일차 메뉴 실행 허용
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode && !BuildPipeline.isBuildingPlayer; // Play Mode 진입·실행과 플레이어 빌드 중이 아닌 경우 활성화
        }

        public static void BuildDevelopmentClientFromCommandLine() // PowerShell과 CI에서 호출하는 Windows 개발 클라이언트 빌드 진입점
        {
            BuildDevelopmentClientInternal(false); // 자동 실행 없이 Windows 개발 클라이언트 빌드
        }

        private static BuildReport BuildDevelopmentClientInternal(bool autoRunPlayer) // Windows 개발 Build Profile을 사용하여 플레이어 빌드 실행
        {
            BuildProfile profile = ProjectDevelopmentBuildValidator.LoadDevelopmentProfile(); // 고정 경로의 Windows 개발 Build Profile 불러오기
            List<string> errors = ProjectDevelopmentBuildValidator.CollectValidationErrors(profile, true); // 빌드 전 개발 Build Profile 전체 검증

            if (errors.Count > 0) // 빌드 전 검증 오류 존재 여부 확인
            {
                LogValidationErrors(errors); // 발견된 모든 빌드 전 검증 오류 출력
                throw new BuildFailedException($"개발 Build Profile 검증에 실패했습니다. 오류 수: {errors.Count}"); // 잘못된 설정으로 빌드하지 않도록 빌드 실패 예외 발생
            }

            string outputDirectory = GetAbsoluteProjectPath(ProjectBuildConfiguration.DevelopmentBuildDirectory); // 개발 클라이언트 출력 폴더 절대 경로 생성
            string summaryDirectory = GetAbsoluteProjectPath(ProjectBuildConfiguration.DevelopmentLogDirectory); // 개발 빌드 로그 폴더 절대 경로 생성
            Directory.CreateDirectory(outputDirectory); // 개발 클라이언트 출력 폴더 생성
            Directory.CreateDirectory(summaryDirectory); // 개발 빌드 로그 폴더 생성

            BuildOptions options = BuildOptions.Development // Development Build 옵션 적용
                | BuildOptions.AllowDebugging // Script Debugging 허용 옵션 적용
                | BuildOptions.CompressWithLz4 // 빠른 개발 반복을 위한 LZ4 압축 적용
                | BuildOptions.StrictMode // 빌드 중 오류 발생 시 실패 처리 옵션 적용
                | BuildOptions.DetailedBuildReport; // 상세 BuildReport 생성 옵션 적용

            if (autoRunPlayer && !Application.isBatchMode) // 에디터 수동 빌드에서 자동 실행 요청 여부 확인
            {
                options |= BuildOptions.AutoRunPlayer; // 빌드 성공 후 플레이어 자동 실행 옵션 추가
            }

            BuildPlayerWithProfileOptions buildOptions = new BuildPlayerWithProfileOptions // Build Profile 기반 플레이어 빌드 옵션 생성
            {
                buildProfile = profile, // Windows 개발 Build Profile 지정
                locationPathName = ProjectBuildConfiguration.DevelopmentBuildPath, // 개발 클라이언트 실행 파일 출력 경로 지정
                options = options // 개발·디버깅·압축·검증 빌드 옵션 지정
            };

            DateTime startedAt = DateTime.Now; // 개발 클라이언트 빌드 시작 시각 저장
            ProjectLog.Info(ProjectLogCategory.Core, $"개발 클라이언트 빌드를 시작합니다: {ProjectBuildConfiguration.DevelopmentBuildPath}", "BUILD_STARTED"); // 개발 클라이언트 빌드 시작 로그 출력

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions); // Windows 개발 Build Profile 기반 플레이어 빌드 실행
            WriteBuildSummary(report, profile, startedAt, autoRunPlayer); // 빌드 결과와 설정을 별도 요약 로그 파일로 저장

            if (report.summary.result != BuildResult.Succeeded) // 개발 클라이언트 빌드 성공 여부 확인
            {
                throw new BuildFailedException($"개발 클라이언트 빌드에 실패했습니다. 결과: {report.summary.result}"); // 실패 결과를 명령행 종료 코드와 Console에 전달
            }

            ProjectLog.Info(ProjectLogCategory.Core, $"개발 클라이언트 빌드를 완료했습니다: {report.summary.outputPath}", "BUILD_SUCCEEDED"); // 개발 클라이언트 빌드 성공 로그 출력
            return report; // 개발 클라이언트 BuildReport 반환
        }

        private static void WriteBuildSummary(BuildReport report, BuildProfile profile, DateTime startedAt, bool autoRunPlayer) // 개발 빌드 결과와 설정을 텍스트 로그로 저장
        {
            string summaryPath = GetAbsoluteProjectPath(ProjectBuildConfiguration.DevelopmentBuildSummaryPath); // 빌드 요약 로그 절대 경로 생성
            StringBuilder builder = new StringBuilder(); // 빌드 요약 로그 문자열 생성기 준비

            builder.AppendLine("Project J Development Build Summary"); // 빌드 요약 로그 제목 추가
            builder.AppendLine($"Started At: {startedAt:yyyy-MM-dd HH:mm:ss}"); // 빌드 시작 시각 추가
            builder.AppendLine($"Finished At: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"); // 빌드 종료 시각 추가
            builder.AppendLine($"Unity Version: {Application.unityVersion}"); // 빌드에 사용한 Unity 버전 추가
            builder.AppendLine($"Build Profile: {AssetDatabase.GetAssetPath(profile)}"); // 빌드에 사용한 Build Profile 경로 추가
            builder.AppendLine($"Build Result: {report.summary.result}"); // Unity BuildReport 결과 추가
            builder.AppendLine($"Build Target: {report.summary.platform}"); // 빌드 대상 플랫폼 추가
            builder.AppendLine($"Build Options: {report.summary.options}"); // 적용된 빌드 옵션 추가
            builder.AppendLine($"Output Path: {report.summary.outputPath}"); // 생성된 플레이어 출력 경로 추가
            builder.AppendLine($"Total Time: {report.summary.totalTime}"); // 전체 빌드 소요 시간 추가
            builder.AppendLine($"Total Size: {report.summary.totalSize} bytes"); // 전체 빌드 크기 추가
            builder.AppendLine($"Auto Run Requested: {autoRunPlayer}"); // 빌드 후 자동 실행 요청 여부 추가
            builder.AppendLine($"Development Build: {EditorUserBuildSettings.development}"); // 활성 Profile Development Build 설정 추가
            builder.AppendLine($"Script Debugging: {EditorUserBuildSettings.allowDebugging}"); // 활성 Profile Script Debugging 설정 추가
            builder.AppendLine($"Autoconnect Profiler: {EditorUserBuildSettings.connectProfiler}"); // 활성 Profile Profiler 자동 연결 설정 추가
            builder.AppendLine($"Deep Profiling: {EditorUserBuildSettings.buildWithDeepProfilingSupport}"); // 활성 Profile Deep Profiling 설정 추가
            builder.AppendLine($"Wait For Managed Debugger: {EditorUserBuildSettings.waitForManagedDebugger}"); // 활성 Profile Managed Debugger 대기 설정 추가
            builder.AppendLine("Scenes:"); // 개발 클라이언트 씬 목록 제목 추가

            EditorBuildSettingsScene[] profileScenes = profile.scenes ?? Array.Empty<EditorBuildSettingsScene>(); // Build Profile에 저장된 씬 목록 또는 빈 배열 조회

            foreach (EditorBuildSettingsScene scene in profileScenes.Where(scene => scene.enabled)) // Build Profile의 모든 활성 씬 순회
            {
                builder.AppendLine($"- {scene.path}"); // 현재 활성 씬 경로 추가
            }

            File.WriteAllText(summaryPath, builder.ToString()); // 완성된 개발 빌드 요약 로그 파일 저장
            ProjectLog.Info(ProjectLogCategory.Core, $"개발 빌드 요약 로그를 저장했습니다: {summaryPath}", "BUILD_SUMMARY_SAVED"); // 개발 빌드 요약 로그 저장 완료 출력
        }

        private static string GetAbsoluteProjectPath(string projectRelativePath) // 프로젝트 상대 경로를 운영체제 절대 경로로 변환
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName; // Assets 폴더의 상위 Unity 프로젝트 루트 경로 조회

            if (string.IsNullOrWhiteSpace(projectRoot)) // Unity 프로젝트 루트 경로 조회 성공 여부 확인
            {
                throw new InvalidOperationException("Unity 프로젝트 루트 경로를 확인할 수 없습니다."); // 프로젝트 루트 경로 조회 실패 예외 발생
            }

            string normalizedRelativePath = projectRelativePath.Replace('/', Path.DirectorySeparatorChar); // Unity 상대 경로 구분자를 현재 운영체제 형식으로 변환
            return Path.GetFullPath(Path.Combine(projectRoot, normalizedRelativePath)); // 프로젝트 루트와 상대 경로를 결합한 절대 경로 반환
        }

        private static void LogValidationErrors(IReadOnlyList<string> errors) // 개발 Build Profile 검증 오류 전체 Console 출력
        {
            for (int index = 0; index < errors.Count; index++) // 모든 Build Profile 검증 오류 순회
            {
                ProjectLog.Error(ProjectLogCategory.Core, errors[index], "BUILD_PROFILE_INVALID"); // 공통 로그 규칙을 사용하는 Build Profile 오류 출력
            }
        }
    }
}
