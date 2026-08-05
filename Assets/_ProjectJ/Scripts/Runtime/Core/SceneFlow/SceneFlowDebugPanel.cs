using UnityEngine; // Unity 런타임과 IMGUI 기능 참조
using UnityEngine.SceneManagement; // 현재 활성 씬 조회 기능 참조

namespace ProjectJ.Core.SceneFlow // 프로젝트 씬 흐름 네임스페이스 선언
{
    [DisallowMultipleComponent] // 동일 게임 오브젝트의 중복 컴포넌트 추가 방지
    public sealed class SceneFlowDebugPanel : MonoBehaviour // 개발용 씬 이동 패널 선언
    {
        [SerializeField] private bool isVisible = true; // 개발용 패널 표시 여부 설정
        private Rect windowRect = new Rect(20f, 20f, 260f, 250f); // 개발용 패널 위치와 크기 저장

        private void OnGUI() // 개발 빌드와 에디터에서 씬 이동 패널 표시
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!isVisible) // 패널 표시가 비활성화되었는지 확인
            {
                return; // 패널 그리기 중단
            }

            windowRect = GUI.Window(GetInstanceID(), windowRect, DrawWindow, "Project J Scene Flow"); // 이동 가능한 디버그 창 표시
#endif
        }

        private void DrawWindow(int windowId) // 개발용 씬 이동 창 내부 구성
        {
            string activeSceneName = SceneManager.GetActiveScene().name; // 현재 활성 씬 이름 조회
            GUILayout.Label($"Current Scene: {activeSceneName}"); // 현재 씬 이름 표시
            GUILayout.Space(8f); // 버튼 목록과 제목 사이 간격 추가

            DrawSceneButton(GameSceneId.MainMenu); // MainMenu 이동 버튼 표시
            DrawSceneButton(GameSceneId.Lobby); // Lobby 이동 버튼 표시
            DrawSceneButton(GameSceneId.MatchLoading); // MatchLoading 이동 버튼 표시
            DrawSceneButton(GameSceneId.Game); // Game 이동 버튼 표시
            DrawSceneButton(GameSceneId.Tests); // Tests 이동 버튼 표시

            GUI.DragWindow(); // 패널 창 드래그 이동 허용
        }

        private static void DrawSceneButton(GameSceneId sceneId) // 지정된 씬 이동 버튼 표시
        {
            SceneFlowManager manager = SceneFlowManager.GetOrCreate(); // 씬 전환 관리자 조회 또는 생성
            string sceneName = GameSceneCatalog.GetSceneName(sceneId); // 대상 씬 이름 조회
            bool isCurrentScene = SceneManager.GetActiveScene().name == sceneName; // 현재 씬과 대상 씬 일치 여부 확인
            bool previousEnabledState = GUI.enabled; // 기존 GUI 활성 상태 저장

            GUI.enabled = !manager.IsLoading && !isCurrentScene; // 로딩 중이거나 현재 씬인 버튼 비활성화

            if (GUILayout.Button(sceneName)) // 대상 씬 이동 버튼 입력 확인
            {
                manager.LoadScene(sceneId); // 선택한 씬으로 전환 요청
            }

            GUI.enabled = previousEnabledState; // 기존 GUI 활성 상태 복원
        }
    }
}
