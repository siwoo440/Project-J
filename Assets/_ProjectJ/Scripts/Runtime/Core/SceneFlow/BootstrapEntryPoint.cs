using ProjectJ.Core.Services; // 공통 서비스 초기화 형식 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using ProjectJ.UI; // 치명 오류 안내 화면 형식 참조
using UnityEngine; // Unity 런타임 기능 참조

namespace ProjectJ.Core.SceneFlow // 프로젝트 씬 흐름 네임스페이스 선언
{
    [DisallowMultipleComponent] // 동일 게임 오브젝트의 중복 컴포넌트 추가 방지
    public sealed class BootstrapEntryPoint : MonoBehaviour // Bootstrap 시작 흐름 처리 형식 선언
    {
        [SerializeField] private GameSceneId firstScene = GameSceneId.MainMenu; // 초기화 완료 후 이동할 첫 씬 설정
        private SceneFlowManager sceneFlowManager; // 사용할 씬 전환 관리자 저장
        private CommonServiceInitializer commonServiceInitializer; // 사용할 공통 서비스 초기화 컴포넌트 저장
        private FatalErrorScreen fatalErrorScreen; // 초기화 실패 안내 화면 컴포넌트 저장

        private void Awake() // Bootstrap 씬의 필수 관리자와 초기화 컴포넌트 준비
        {
            sceneFlowManager = SceneFlowManager.GetOrCreate(); // 씬 전환 관리자 조회 또는 생성
            commonServiceInitializer = GetComponent<CommonServiceInitializer>(); // 같은 게임 오브젝트의 공통 서비스 초기화 컴포넌트 조회

            if (commonServiceInitializer == null) // 공통 서비스 초기화 컴포넌트 누락 여부 확인
            {
                commonServiceInitializer = gameObject.AddComponent<CommonServiceInitializer>(); // 누락된 공통 서비스 초기화 컴포넌트 자동 추가
            }

            fatalErrorScreen = GetComponent<FatalErrorScreen>(); // 같은 게임 오브젝트의 치명 오류 화면 조회

            if (fatalErrorScreen == null) // 치명 오류 화면 컴포넌트 누락 여부 확인
            {
                fatalErrorScreen = gameObject.AddComponent<FatalErrorScreen>(); // 누락된 치명 오류 화면 컴포넌트 자동 추가
            }
        }

        private void Start() // 공통 서비스 초기화 완료 후 첫 씬 전환
        {
            if (!commonServiceInitializer.InitializeServices()) // 공통 서비스 초기화 성공 여부 확인
            {
                fatalErrorScreen.Show("게임을 시작할 수 없습니다", commonServiceInitializer.LastFatalErrorMessage); // 초기화 실패 원인 화면 표시
                ProjectLog.Error(ProjectLogCategory.Scene, "MainMenu 전환을 중단하고 치명 오류 화면을 표시했습니다.", "BOOTSTRAP_BLOCKED", this); // Bootstrap 진입 차단 로그 출력
                return; // 초기화 실패 시 MainMenu 전환 중단
            }

            fatalErrorScreen.Hide(); // 남아 있는 치명 오류 화면 숨김
            sceneFlowManager.LoadScene(firstScene); // 설정된 첫 씬으로 전환 요청
        }
    }
}
