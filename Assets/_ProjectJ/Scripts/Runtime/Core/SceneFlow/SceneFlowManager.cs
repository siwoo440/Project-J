using System.Collections; // 코루틴 기능 참조
using UnityEngine; // Unity 런타임 기능 참조
using UnityEngine.SceneManagement; // Unity 씬 관리 기능 참조

namespace ProjectJ.Core.SceneFlow // 프로젝트 씬 흐름 네임스페이스 선언
{
    [DisallowMultipleComponent] // 동일 게임 오브젝트의 중복 컴포넌트 추가 방지
    public sealed class SceneFlowManager : MonoBehaviour // 프로젝트 전역 씬 전환 관리자 선언
    {
        private static SceneFlowManager instance; // 현재 씬 전환 관리자 인스턴스 저장
        private Coroutine loadSceneCoroutine; // 현재 실행 중인 씬 로드 코루틴 저장

        public static SceneFlowManager Instance => instance; // 현재 씬 전환 관리자 인스턴스 공개
        public bool IsLoading => loadSceneCoroutine != null; // 씬 로딩 진행 여부 공개

        public static SceneFlowManager GetOrCreate() // 씬 전환 관리자 조회 또는 생성
        {
            if (instance != null) // 기존 인스턴스 존재 여부 확인
            {
                return instance; // 기존 인스턴스 반환
            }

            instance = FindFirstObjectByType<SceneFlowManager>(); // 현재 씬에서 씬 전환 관리자 검색

            if (instance != null) // 씬 안에서 인스턴스를 찾았는지 확인
            {
                return instance; // 검색된 인스턴스 반환
            }

            GameObject managerObject = new GameObject(nameof(SceneFlowManager)); // 씬 전환 관리자 게임 오브젝트 생성
            instance = managerObject.AddComponent<SceneFlowManager>(); // 씬 전환 관리자 컴포넌트 추가
            return instance; // 새로 생성된 인스턴스 반환
        }

        private void Awake() // 씬 전환 관리자 단일 인스턴스 설정
        {
            if (instance != null && instance != this) // 다른 씬 전환 관리자 존재 여부 확인
            {
                Destroy(gameObject); // 중복 게임 오브젝트 제거
                return; // 초기화 중단
            }

            instance = this; // 현재 컴포넌트를 전역 인스턴스로 저장
            DontDestroyOnLoad(gameObject); // 씬 전환 후에도 관리자 유지
        }

        public bool LoadScene(GameSceneId sceneId) // 지정된 씬으로 비동기 전환 요청
        {
            if (IsLoading) // 다른 씬 로드가 진행 중인지 확인
            {
                Debug.LogWarning("[SceneFlow] 이미 다른 씬을 불러오는 중입니다."); // 중복 요청 경고 출력
                return false; // 씬 전환 요청 실패 반환
            }

            string sceneName = GameSceneCatalog.GetSceneName(sceneId); // 씬 식별자에서 씬 이름 조회
            string activeSceneName = SceneManager.GetActiveScene().name; // 현재 활성 씬 이름 조회

            if (activeSceneName == sceneName) // 현재 씬과 대상 씬이 같은지 확인
            {
                Debug.LogWarning($"[SceneFlow] 이미 {sceneName} 씬에 있습니다."); // 동일 씬 요청 경고 출력
                return false; // 씬 전환 요청 실패 반환
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName)) // 대상 씬이 빌드 목록에서 로드 가능한지 확인
            {
                Debug.LogError($"[SceneFlow] {sceneName} 씬이 Build Settings 또는 Build Profile에 등록되지 않았습니다."); // 미등록 씬 오류 출력
                return false; // 씬 전환 요청 실패 반환
            }

            loadSceneCoroutine = StartCoroutine(LoadSceneRoutine(sceneName)); // 비동기 씬 로드 코루틴 시작
            return true; // 씬 전환 요청 성공 반환
        }

        private IEnumerator LoadSceneRoutine(string sceneName) // 지정된 씬 비동기 로드 처리
        {
            Debug.Log($"[SceneFlow] {sceneName} 씬 로드를 시작합니다."); // 씬 로드 시작 로그 출력

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single); // 대상 씬 비동기 로드 요청

            if (loadOperation == null) // 비동기 로드 요청 생성 실패 여부 확인
            {
                Debug.LogError($"[SceneFlow] {sceneName} 씬 로드 요청을 생성하지 못했습니다."); // 로드 요청 생성 실패 오류 출력
                loadSceneCoroutine = null; // 현재 로드 코루틴 상태 초기화
                yield break; // 코루틴 종료
            }

            while (!loadOperation.isDone) // 씬 로드 완료 전까지 반복
            {
                yield return null; // 다음 프레임까지 대기
            }

            loadSceneCoroutine = null; // 현재 로드 코루틴 상태 초기화
            Debug.Log($"[SceneFlow] {sceneName} 씬 로드를 완료했습니다."); // 씬 로드 완료 로그 출력
        }

        private void OnDestroy() // 씬 전환 관리자 제거 상태 정리
        {
            if (instance == this) // 제거되는 컴포넌트가 전역 인스턴스인지 확인
            {
                instance = null; // 전역 인스턴스 참조 초기화
            }
        }
    }
}
