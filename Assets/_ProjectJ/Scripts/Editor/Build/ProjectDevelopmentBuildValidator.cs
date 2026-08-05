using System; // 문자열 비교 기능 참조
using System.Collections.Generic; // 검증 오류 목록 기능 참조
using System.Linq; // 씬 경로와 스크립팅 정의 검색 기능 참조
using ProjectJ.Build; // Project J 빌드 경로와 씬 목록 참조
using UnityEditor; // Unity 에디터 빌드 설정과 에셋 기능 참조
using UnityEditor.Build.Profile; // Unity Build Profile 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal static class ProjectDevelopmentBuildValidator // Windows 개발 Build Profile 설정 검증 담당 형식 선언
    {
        internal static BuildProfile LoadDevelopmentProfile() // 고정 경로의 Windows 개발 Build Profile 에셋 불러오기
        {
            return AssetDatabase.LoadAssetAtPath<BuildProfile>(ProjectBuildConfiguration.DevelopmentProfileAssetPath); // 고정 경로에서 Build Profile 에셋 반환
        }

        internal static List<string> CollectValidationErrors(BuildProfile profile, bool requireActiveProfile) // Windows 개발 Build Profile 전체 설정 오류 목록 수집
        {
            List<string> errors = new List<string>(); // Build Profile 검증 오류 목록 생성

            if (profile == null) // Windows 개발 Build Profile 에셋 존재 여부 확인
            {
                errors.Add($"개발 Build Profile 에셋이 없습니다: {ProjectBuildConfiguration.DevelopmentProfileAssetPath}"); // Build Profile 에셋 누락 오류 추가
                return errors; // Profile 세부 검사 없이 현재 오류 목록 반환
            }

            string profileAssetPath = AssetDatabase.GetAssetPath(profile); // 현재 Build Profile 에셋 경로 조회

            if (!string.Equals(profileAssetPath, ProjectBuildConfiguration.DevelopmentProfileAssetPath, StringComparison.Ordinal)) // Build Profile 에셋 경로 일치 여부 확인
            {
                errors.Add($"개발 Build Profile 경로가 다릅니다. 예상: {ProjectBuildConfiguration.DevelopmentProfileAssetPath}, 현재: {profileAssetPath}"); // Build Profile 경로 오류 추가
            }

            if (!string.Equals(profile.name, ProjectBuildConfiguration.DevelopmentProfileName, StringComparison.Ordinal)) // Build Profile 에셋 이름 일치 여부 확인
            {
                errors.Add($"개발 Build Profile 이름이 다릅니다. 예상: {ProjectBuildConfiguration.DevelopmentProfileName}, 현재: {profile.name}"); // Build Profile 이름 오류 추가
            }

            if (!profile.overrideGlobalScenes) // 개발 Build Profile의 글로벌 씬 목록 오버라이드 여부 확인
            {
                errors.Add("개발 Build Profile의 Override Global Scene List가 비활성화되어 있습니다."); // 글로벌 씬 목록 오버라이드 누락 오류 추가
            }

            EditorBuildSettingsScene[] profileScenes = profile.scenes ?? Array.Empty<EditorBuildSettingsScene>(); // Build Profile에 저장된 씬 목록 또는 빈 배열 조회
            string[] enabledScenePaths = profileScenes // Build Profile에 저장된 씬 목록 조회
                .Where(scene => scene.enabled) // 빌드에 활성화된 씬만 선택
                .Select(scene => scene.path) // 활성 씬의 에셋 경로 선택
                .ToArray(); // 활성 씬 경로 배열 생성

            if (!enabledScenePaths.SequenceEqual(ProjectBuildConfiguration.DevelopmentScenePaths)) // 활성 씬 순서와 개발 클라이언트 기준 순서 일치 여부 확인
            {
                string currentScenes = enabledScenePaths.Length == 0 // 현재 활성 씬 존재 여부 확인
                    ? "(none)" // 활성 씬이 없는 경우 대체 문구 지정
                    : string.Join(", ", enabledScenePaths); // 현재 활성 씬 경로를 한 줄 문자열로 결합

                errors.Add($"개발 Build Profile 씬 순서가 다릅니다. 현재: {currentScenes}"); // 개발 클라이언트 씬 순서 오류 추가
            }

            if (enabledScenePaths.Contains(ProjectBuildConfiguration.TestsScenePath)) // Tests 씬이 개발 클라이언트에 포함됐는지 확인
            {
                errors.Add("Tests 씬이 개발 클라이언트 Build Profile에 포함되어 있습니다."); // Tests 씬 포함 오류 추가
            }

            bool hasDevelopmentDefine = profile.scriptingDefines != null // Build Profile 스크립팅 정의 배열 존재 여부 확인
                && profile.scriptingDefines.Contains(ProjectBuildConfiguration.DevelopmentScriptingDefine); // 개발 전용 스크립팅 정의 포함 여부 확인

            if (!hasDevelopmentDefine) // 개발 전용 스크립팅 정의 누락 여부 확인
            {
                errors.Add($"개발 Build Profile에 {ProjectBuildConfiguration.DevelopmentScriptingDefine} 정의가 없습니다."); // 개발 전용 스크립팅 정의 누락 오류 추가
            }

            if (!requireActiveProfile) // 활성 Build Profile과 공유 빌드 설정 검사가 필요한지 확인
            {
                return errors; // 현재까지 수집된 에셋 설정 오류 목록 반환
            }

            BuildProfile activeProfile = BuildProfile.GetActiveBuildProfile(); // Unity 에디터의 현재 활성 Build Profile 조회

            if (activeProfile != profile) // 고정 Windows 개발 Build Profile의 활성화 여부 확인
            {
                errors.Add("ProjectJ_Windows_Development Build Profile이 현재 활성 프로필이 아닙니다."); // 활성 Build Profile 불일치 오류 추가
                return errors; // 다른 Profile 설정을 읽지 않도록 현재 오류 목록 반환
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64) // 현재 활성 빌드 대상의 Windows 64비트 여부 확인
            {
                errors.Add($"활성 빌드 대상이 StandaloneWindows64가 아닙니다. 현재: {EditorUserBuildSettings.activeBuildTarget}"); // 활성 빌드 대상 오류 추가
            }

            if (!EditorUserBuildSettings.development) // Development Build 활성화 여부 확인
            {
                errors.Add("Development Build가 비활성화되어 있습니다."); // Development Build 누락 오류 추가
            }

            if (!EditorUserBuildSettings.allowDebugging) // Script Debugging 활성화 여부 확인
            {
                errors.Add("Script Debugging이 비활성화되어 있습니다."); // Script Debugging 누락 오류 추가
            }

            if (EditorUserBuildSettings.connectProfiler) // Autoconnect Profiler 활성화 여부 확인
            {
                errors.Add("Autoconnect Profiler는 기본 개발 프로필에서 비활성화해야 합니다."); // 자동 Profiler 연결 설정 오류 추가
            }

            if (EditorUserBuildSettings.buildWithDeepProfilingSupport) // Deep Profiling 활성화 여부 확인
            {
                errors.Add("Deep Profiling은 기본 개발 프로필에서 비활성화해야 합니다."); // Deep Profiling 설정 오류 추가
            }

            if (EditorUserBuildSettings.waitForManagedDebugger) // Managed Debugger 연결 대기 활성화 여부 확인
            {
                errors.Add("Wait for Managed Debugger는 기본 개발 프로필에서 비활성화해야 합니다."); // Managed Debugger 대기 설정 오류 추가
            }

            return errors; // 수집된 모든 개발 Build Profile 검증 오류 반환
        }
    }
}
