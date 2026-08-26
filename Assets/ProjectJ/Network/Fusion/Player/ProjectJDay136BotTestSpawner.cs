using Fusion; // NetworkRunner와 NetworkObject 사용
using UnityEngine; // MonoBehaviour와 Resources 사용
using UnityEngine.SceneManagement; // Game Scene 확인

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJDay136BotTestSpawner :
        MonoBehaviour
    {
        private const string GameSceneName =
            "Game"; // Bot 테스트 대상 Scene 이름

        private const string BotResourceName =
            "ProjectJNetworkBot"; // Bot Resource Prefab 이름

        [SerializeField]
        private string spawnPointName =
            "Spawn_07"; // 기본 Bot Spawn Point 이름

        private bool spawnAttempted; // 현재 Scene Spawn 시도 여부

        public void Configure(
            string targetSpawnPointName
        )
        {
            spawnPointName =
                string.IsNullOrWhiteSpace(
                    targetSpawnPointName
                )
                    ? "Spawn_07"
                    : targetSpawnPointName; // Bot Spawn Point 이름 적용
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            TrySpawnDevelopmentBot(); // 개발 환경 Host Bot 생성 시도
#endif
        }

        private void TrySpawnDevelopmentBot()
        {
            if (spawnAttempted)
            {
                return; // 현재 Scene 중복 Spawn 차단
            }

            Scene activeScene =
                SceneManager.GetActiveScene(); // 현재 활성 Scene 조회

            if (
                !activeScene.IsValid() ||
                activeScene.name !=
                GameSceneName
            )
            {
                return; // Game Scene 외 Spawn 차단
            }

            NetworkRunner runner =
                FindServerRunner(); // 현재 Host Runner 검색

            if (
                runner == null ||
                !runner.IsRunning ||
                !runner.IsServer
            )
            {
                return; // Host Runner 준비 전 대기
            }

            if (
                HasExistingBot(
                    runner
                )
            )
            {
                spawnAttempted =
                    true; // 기존 Bot 존재 시 추가 생성 차단

                return;
            }

            GameObject botPrefabObject =
                Resources.Load<GameObject>(
                    BotResourceName
                ); // Bot Resource Prefab 로드

            NetworkObject botPrefab =
                botPrefabObject != null
                    ? botPrefabObject.GetComponent<NetworkObject>()
                    : null; // Bot NetworkObject 조회

            if (botPrefab == null)
            {
                Debug.LogError(
                    "[Project J/Day136] ProjectJNetworkBot Resource Prefab을 찾지 못했습니다. " +
                    "Project J > Day136 > Apply Bot Foundation 메뉴를 다시 실행하십시오."
                ); // Bot Prefab 누락 오류 출력

                spawnAttempted =
                    true; // 반복 오류 출력 차단

                return;
            }

            Transform spawnPoint =
                FindSceneObjectByName(
                    activeScene,
                    spawnPointName
                )?.transform; // 지정 Spawn Point 검색

            Vector3 spawnPosition =
                spawnPoint != null
                    ? spawnPoint.position
                    : new Vector3(
                        21f,
                        2f,
                        4f
                    ); // Spawn Point 누락 시 기존 8번 슬롯 위치 사용

            Quaternion spawnRotation =
                spawnPoint != null
                    ? spawnPoint.rotation
                    : Quaternion.identity; // Spawn 회전 선택

            NetworkObject spawnedBot =
                runner.Spawn(
                    botPrefab,
                    spawnPosition,
                    spawnRotation,
                    PlayerRef.None
                ); // Input Authority 없는 Host Bot 생성

            spawnAttempted =
                true; // 현재 Scene Spawn 시도 완료

            if (spawnedBot == null)
            {
                Debug.LogError(
                    "[Project J/Day136] Network Bot Spawn에 실패했습니다."
                ); // Fusion Bot Spawn 실패 출력

                return;
            }

            ProjectJNetworkBotController controller =
                spawnedBot.GetComponent<ProjectJNetworkBotController>(); // Bot Controller 조회

            ProjectJNetworkBotMarker marker =
                spawnedBot.GetComponent<ProjectJNetworkBotMarker>(); // Bot Marker 조회

            if (
                controller == null ||
                marker == null
            )
            {
                Debug.LogError(
                    "[Project J/Day136] Bot Prefab에 필수 Bot Component가 없습니다."
                ); // Bot Prefab 구성 오류 출력

                runner.Despawn(
                    spawnedBot
                ); // 잘못된 Bot NetworkObject 정리

                return;
            }

            controller.RefreshRoute(
                spawnedBot.GetComponent<ProjectJNetworkPlayer>()
            ); // Spawn 직후 Route 목록 갱신

            spawnedBot.name =
                "NetworkBot_Day136"; // Host Hierarchy Bot 이름 지정

            Debug.Log(
                "[Project J/Day136] Host Bot 1명 Spawn 완료 / Route Nodes: " +
                controller.RouteCount +
                " / Spawn: " +
                spawnPointName
            ); // Bot Spawn 결과 출력
        }

        private static NetworkRunner FindServerRunner()
        {
            NetworkRunner[] runners =
                Object.FindObjectsByType<NetworkRunner>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                ); // 현재 활성 Runner 수집

            for (
                int index = 0;
                index < runners.Length;
                index++
            )
            {
                NetworkRunner candidate =
                    runners[index]; // 현재 Runner 후보 조회

                if (
                    candidate != null &&
                    candidate.IsRunning &&
                    candidate.IsServer
                )
                {
                    return candidate; // 첫 Host Runner 반환
                }
            }

            return null; // Host Runner 미발견
        }

        private static bool HasExistingBot(
            NetworkRunner runner
        )
        {
            ProjectJNetworkBotMarker[] markers =
                Object.FindObjectsByType<ProjectJNetworkBotMarker>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                ); // 현재 Bot Marker 수집

            for (
                int index = 0;
                index < markers.Length;
                index++
            )
            {
                ProjectJNetworkBotMarker marker =
                    markers[index]; // 현재 Bot Marker 조회

                ProjectJNetworkPlayer player =
                    marker != null
                        ? marker.GetComponent<ProjectJNetworkPlayer>()
                        : null; // Bot Network Player 조회

                if (
                    player != null &&
                    player.Runner ==
                    runner
                )
                {
                    return true; // 동일 Runner Bot 존재 확인
                }
            }

            return false; // 동일 Runner Bot 없음
        }

        private static GameObject FindSceneObjectByName(
            Scene scene,
            string objectName
        )
        {
            if (
                !scene.IsValid() ||
                string.IsNullOrWhiteSpace(
                    objectName
                )
            )
            {
                return null; // 잘못된 Scene 또는 이름 처리
            }

            GameObject[] roots =
                scene.GetRootGameObjects(); // Scene Root 목록 조회

            for (
                int index = 0;
                index < roots.Length;
                index++
            )
            {
                Transform found =
                    FindChildRecursive(
                        roots[index].transform,
                        objectName
                    ); // Root 하위 이름 검색

                if (found != null)
                {
                    return found.gameObject; // 검색 GameObject 반환
                }
            }

            return null; // 대상 GameObject 미발견
        }

        private static Transform FindChildRecursive(
            Transform current,
            string objectName
        )
        {
            if (
                current != null &&
                current.name ==
                objectName
            )
            {
                return current; // 현재 Transform 이름 일치
            }

            if (current == null)
            {
                return null; // null Transform 처리
            }

            for (
                int index = 0;
                index < current.childCount;
                index++
            )
            {
                Transform found =
                    FindChildRecursive(
                        current.GetChild(
                            index
                        ),
                        objectName
                    ); // 자식 Hierarchy 재귀 검색

                if (found != null)
                {
                    return found; // 자식 검색 결과 반환
                }
            }

            return null; // 현재 Hierarchy 미발견
        }
    }
}
