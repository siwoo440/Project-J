using System.Collections; // Coroutine 사용
using ProjectJ.UI; // 인벤토리 Canvas UI 사용
using UnityEngine; // 유니티 기능 사용
using UnityEngine.InputSystem; // PlayerInput 사용
using UnityEngine.SceneManagement; // Scene 변경 감지

namespace ProjectJ.Items // 아이템 시스템 네임스페이스
{
    public sealed class ItemInventoryRuntimeInstaller : MonoBehaviour // Local Player 인벤토리 자동 설치
    {
        private PlayerInput installedPlayer; // 현재 연결된 Local Player
        private ItemInventoryCanvasView canvasView; // 현재 Canvas UI

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] // Play 시작 후 자동 실행
        private static void CreateInstaller() // Installer 생성
        {
            ItemInventoryRuntimeInstaller existing =
                FindFirstObjectByType<ItemInventoryRuntimeInstaller>(); // 기존 Installer 탐색

            if (existing != null) // 중복 검사
            {
                return; // 기존 Installer 사용
            }

            GameObject installerObject =
                new GameObject("=== Item Inventory Runtime ==="); // Runtime Root 생성

            installerObject.AddComponent<ItemInventoryRuntimeInstaller>(); // Installer 추가
            DontDestroyOnLoad(installerObject); // Scene 전환에도 유지
        }

        private void OnEnable() // Scene 이벤트 연결
        {
            SceneManager.sceneLoaded += OnSceneLoaded; // Scene Load 이벤트 등록
        }

        private void OnDisable() // Scene 이벤트 해제
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; // Scene Load 이벤트 해제
        }

        private void Start() // 최초 Player 연결 시도
        {
            StartCoroutine(InstallWhenPlayerReady()); // Player 생성까지 기다림
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) // 새 Scene 진입 처리
        {
            installedPlayer = null; // Player 참조 초기화
            StartCoroutine(InstallWhenPlayerReady()); // 새 Local Player 연결
        }

        private IEnumerator InstallWhenPlayerReady() // Local Player 생성 대기
        {
            while (installedPlayer == null) // Player 연결 전까지 반복
            {
                PlayerInput playerInput = FindLocalPlayerInput(); // Local Player 탐색

                if (playerInput != null) // Player 발견 검사
                {
                    InstallForPlayer(playerInput); // Inventory와 UI 설치
                    yield break; // Coroutine 종료
                }

                yield return new WaitForSecondsRealtime(0.25f); // 짧게 대기 후 재시도
            }
        }

        private void InstallForPlayer(PlayerInput playerInput) // Local Player에 Inventory 구성
        {
            installedPlayer = playerInput; // 연결 Player 저장

            PlayerItemInventory inventory =
                playerInput.GetComponent<PlayerItemInventory>(); // 기존 Inventory 탐색

            if (inventory == null) // Inventory 누락 검사
            {
                inventory =
                    playerInput.gameObject.AddComponent<PlayerItemInventory>(); // 두 슬롯 Inventory 추가
            }

            PlayerItemInventoryInput input =
                playerInput.GetComponent<PlayerItemInventoryInput>(); // 기존 슬롯 입력 탐색

            if (input == null) // 슬롯 입력 누락 검사
            {
                playerInput.gameObject.AddComponent<PlayerItemInventoryInput>(); // Q/E 슬롯 입력 추가
            }

            if (canvasView == null) // Canvas UI 누락 검사
            {
                canvasView =
                    ItemInventoryCanvasView.Create(transform); // Persistent Canvas 생성
            }

            canvasView.Bind(inventory); // 현재 Player Inventory와 UI 연결
        }

        private static PlayerInput FindLocalPlayerInput() // 활성 Local Player 탐색
        {
            PlayerInput[] inputs =
                FindObjectsByType<PlayerInput>( // 활성 PlayerInput 전체 검색
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            for (int i = 0; i < inputs.Length; i++) // 모든 PlayerInput 반복
            {
                PlayerInput input = inputs[i]; // 현재 입력 저장

                if (
                    input != null &&
                    input.isActiveAndEnabled &&
                    input.actions != null
                ) // 사용 가능한 입력 검사
                {
                    return input; // 첫 활성 Local Player 반환
                }
            }

            return null; // Player 없음 반환
        }
    }
}
