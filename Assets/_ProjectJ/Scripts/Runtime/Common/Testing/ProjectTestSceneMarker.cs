using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEngine; // Unity MonoBehaviour와 프레임 정보 기능 참조

namespace ProjectJ.Testing // 프로젝트 테스트 프레임워크 네임스페이스 선언
{
    [DisallowMultipleComponent] // 동일 게임 오브젝트의 테스트 씬 마커 중복 추가 방지
    public sealed class ProjectTestSceneMarker : MonoBehaviour // Tests 씬 준비 상태 확인 컴포넌트 선언
    {
        [SerializeField] private int frameworkVersion = ProjectTestFramework.FrameworkVersion; // 씬에 저장된 테스트 프레임워크 버전 저장

        public int FrameworkVersion => frameworkVersion; // 씬에 저장된 테스트 프레임워크 버전 반환
        public bool IsInitialized { get; private set; } // 런타임 Awake 완료 여부 저장
        public int InitializationFrame { get; private set; } // 테스트 씬 마커 초기화 프레임 저장
        public string LoadedSceneName { get; private set; } // 마커가 포함된 로드 씬 이름 저장

        public void Configure(int newFrameworkVersion) // Editor 구성 도구에서 테스트 프레임워크 버전 설정
        {
            frameworkVersion = newFrameworkVersion; // 전달된 테스트 프레임워크 버전 저장
        }

        private void Awake() // Tests 씬 로드 시 마커 준비 상태 초기화
        {
            LoadedSceneName = gameObject.scene.name; // 현재 마커가 포함된 씬 이름 저장
            InitializationFrame = Time.frameCount; // 마커가 초기화된 현재 프레임 저장
            IsInitialized = true; // Tests 씬 마커 초기화 완료 상태 설정

            ProjectLog.Info( // 공통 로그 규칙을 사용하는 Tests 씬 준비 로그 출력
                ProjectLogCategory.Test, // 테스트 로그 분류 지정
                ProjectTestFramework.SceneMarkerReadyMessage, // 테스트 씬 마커 준비 메시지 지정
                "TEST_SCENE_READY", // 테스트 씬 준비 로그 코드 지정
                this); // 현재 테스트 씬 마커를 Console 문맥으로 지정
        }
    }
}
