using ProjectJ.Diagnostics; // Project J 공통 로그 출력 기능 참조
using UnityEngine; // Unity 런타임 애플리케이션과 초기화 기능 참조

namespace ProjectJ.Build // 프로젝트 빌드 공통 네임스페이스 선언
{
    public static class ProjectBuildRuntimeReporter // 실행된 플레이어의 빌드 정보와 로그 경로 출력 담당 형식 선언
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] // 첫 씬 로드 전 빌드 정보 출력 메서드 자동 실행 지정
        private static void ReportRuntimeBuildInformation() // 현재 실행 환경의 빌드 정보와 플레이어 로그 경로 출력
        {
            string buildType = GetBuildTypeName(); // 현재 실행 환경의 빌드 유형 이름 조회
            string consoleLogPath = string.IsNullOrWhiteSpace(Application.consoleLogPath) // 현재 플랫폼의 콘솔 로그 경로 지원 여부 확인
                ? "(unsupported)" // 로그 파일 경로를 지원하지 않는 플랫폼 대체 문구 지정
                : Application.consoleLogPath; // Unity가 실제로 사용하는 콘솔 로그 파일 경로 지정

            ProjectLog.Info( // 공통 로그 형식을 사용하는 런타임 빌드 정보 출력
                ProjectLogCategory.Core, // 공통 초기화 로그 분류 지정
                $"BuildType={buildType}, Unity={Application.unityVersion}, Version={Application.version}, BuildGUID={Application.buildGUID}", // 현재 빌드 유형과 버전 정보 지정
                "BUILD_RUNTIME_INFO"); // 런타임 빌드 정보 로그 코드 지정

            ProjectLog.Info( // 공통 로그 형식을 사용하는 플레이어 로그 경로 출력
                ProjectLogCategory.Core, // 공통 초기화 로그 분류 지정
                $"PlayerLogPath={consoleLogPath}", // 현재 플랫폼의 실제 플레이어 로그 파일 경로 지정
                "BUILD_LOG_PATH"); // 플레이어 로그 경로 로그 코드 지정
        }

        private static string GetBuildTypeName() // 현재 실행 환경의 빌드 유형 이름 반환
        {
            if (Application.isEditor) // Unity 에디터 Play Mode 실행 여부 확인
            {
                return "Editor"; // 에디터 실행 환경 이름 반환
            }

#if DEVELOPMENT_BUILD
            return "Development"; // DEVELOPMENT_BUILD 기호가 포함된 플레이어 이름 반환
#else
            return "Release"; // 일반 릴리스 플레이어 이름 반환
#endif
        }
    }
}
