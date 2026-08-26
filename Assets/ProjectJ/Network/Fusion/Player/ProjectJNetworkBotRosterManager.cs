using System.Collections.Generic; // Player와 Spawn Point 목록 사용
using Fusion; // NetworkRunner와 NetworkObject 사용
using ProjectJ.AI; // Bot Roster 정책 사용
using UnityEngine; // MonoBehaviour와 Resources 사용
using UnityEngine.SceneManagement; // Game Scene 확인

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJNetworkBotRosterManager :
        MonoBehaviour
    {
        private const string GameSceneName =
            "Game"; // Bot Roster 대상 Scene 이름

        private const string BotResourceName =
            "ProjectJNetworkBot"; // Bot Resource Prefab 이름

        private const float InitialDelaySeconds =
            1.5f; // Human Spawn 완료 대기 시간

        private const float ReconcileIntervalSeconds =
            1f; // Roster 재계산 간격

        private const float RosterStableSeconds =
            0.75f; // 전원 충원 후 Countdown 전 안정화 시간

        private const float BotSpawnParticipantClearance =
            1f; // Bot Spawn 최소 참가자 수평 간격

        private const float BotStartDelayIntervalSeconds =
            0.25f; // Bot별 경기 시작 출발 간격

        private const float BotStartDelayMaximumSeconds =
            1.5f; // 마지막 Bot 최대 출발 지연

        [SerializeField]
        [Min(1)]
        private int targetParticipantCount =
            8; // 기본 목표 경기 참가 인원

        private static ProjectJNetworkBotRosterManager activeInstance; // 현재 Game Scene Roster Manager

        private readonly List<ProjectJNetworkPlayer> humans =
            new List<ProjectJNetworkPlayer>(); // 현재 Human Player 목록

        private readonly List<ProjectJNetworkPlayer> bots =
            new List<ProjectJNetworkPlayer>(); // 현재 Bot Player 목록

        private readonly List<Transform> spawnPoints =
            new List<Transform>(); // Game Scene Spawn Point 목록

        private float nextReconcileTime; // 다음 Roster 재계산 시간
        private float rosterFilledObservedAt = -1f; // 전원 충원 최초 관측 시간
        private bool initialized; // Scene Spawn Point 초기화 여부
        private bool cachedRosterStable; // Countdown 허용 가능한 충원 안정 상태
        private int botNameSequence; // Host Bot 이름 일련번호
        private NetworkRunner cachedRunner; // 현재 관리 Host Runner

        public int TargetParticipantCount =>
            targetParticipantCount; // 목표 참가 인원 조회

        public bool IsRosterStable =>
            cachedRosterStable; // 현재 Countdown 허용 상태 조회

        public static bool IsCountdownAllowed(
            NetworkRunner runner
        )
        {
            ProjectJNetworkBotRosterManager manager =
                activeInstance; // 현재 Roster Manager 조회

            if (manager == null)
            {
                manager =
                    Object.FindFirstObjectByType<ProjectJNetworkBotRosterManager>(); // 초기화 순서 대비 Scene Manager 검색
            }

            if (manager == null)
            {
                return true; // Roster Manager 없는 기존 테스트 Scene은 기존 Countdown 허용
            }

            return
                runner != null &&
                manager.cachedRunner == runner &&
                manager.cachedRosterStable; // 현재 Host Roster 안정화 완료 시에만 Countdown 허용
        }

        public void Configure(
            int participantCount
        )
        {
            targetParticipantCount =
                Mathf.Max(
                    1,
                    participantCount
                ); // 목표 참가 인원 적용
        }

        private void OnEnable()
        {
            activeInstance =
                this; // 현재 Scene Roster Manager 등록

            nextReconcileTime =
                Time.unscaledTime +
                InitialDelaySeconds; // 최초 Human Spawn 대기 설정

            ResetRosterReadiness(); // 최초 Countdown 잠금 상태 설정
        }

        private void OnDisable()
        {
            if (activeInstance == this)
            {
                activeInstance =
                    null; // 현재 Roster Manager 등록 해제
            }

            cachedRunner =
                null; // Runner Cache 해제

            ResetRosterReadiness(); // 비활성화 시 Countdown 준비 상태 초기화
        }

        private void Update()
        {
            if (
                Time.unscaledTime <
                nextReconcileTime
            )
            {
                return; // Roster 재계산 간격 대기
            }

            nextReconcileTime =
                Time.unscaledTime +
                ReconcileIntervalSeconds; // 다음 재계산 시간 예약

            ReconcileRoster(); // Host 부족 인원 Bot 조정과 Countdown 준비 확인
        }

        private void ReconcileRoster()
        {
            Scene activeScene =
                SceneManager.GetActiveScene(); // 현재 활성 Scene 조회

            if (
                !activeScene.IsValid() ||
                activeScene.name !=
                GameSceneName
            )
            {
                return; // Game Scene 외 Roster 조정 차단
            }

            NetworkRunner runner =
                FindServerRunner(); // 현재 Host Runner 검색

            if (
                runner == null ||
                !runner.IsRunning ||
                !runner.IsServer
            )
            {
                cachedRunner =
                    null; // Host Runner 미준비 상태 저장

                ResetRosterReadiness(); // Host 준비 전 Countdown 잠금

                return;
            }

            cachedRunner =
                runner; // 현재 관리 Host Runner 저장

            if (!initialized)
            {
                CollectSpawnPoints(
                    activeScene
                ); // 최초 Spawn Point 수집

                initialized =
                    true; // Roster Scene 초기화 완료
            }

            CollectParticipants(
                runner
            ); // 현재 Human·Bot 인원 수집

            int maximumBotCount =
                Mathf.Max(
                    0,
                    targetParticipantCount
                ); // 현재 목표 기준 최대 Bot 수 계산

            int desiredBotCount =
                ProjectJBotCompetitionPolicy.ResolveDesiredBotCount(
                    targetParticipantCount,
                    humans.Count,
                    maximumBotCount
                ); // 부족 인원 Bot 목표 수 계산

            if (bots.Count < desiredBotCount)
            {
                MarkRosterIncompleteAndCancelCountdown(); // Bot 충원 중 Countdown 차단

                SpawnOneBot(
                    runner
                ); // 한 Tick에 Bot 1명 충원

                return;
            }

            if (bots.Count > desiredBotCount)
            {
                MarkRosterIncompleteAndCancelCountdown(); // Human 증가 조정 중 Countdown 차단

                DespawnOneBot(
                    runner
                ); // Human 증가 시 Bot 1명 제거

                return;
            }

            bool rosterFilled =
                ProjectJBotCompetitionPolicy.IsRosterFilled(
                    targetParticipantCount,
                    humans.Count,
                    bots.Count
                ); // Human과 Bot 전체 목표 인원 충원 확인

            UpdateRosterReadyState(
                runner,
                rosterFilled
            ); // 안정화 후 Countdown 시작 또는 부족 상태 취소
        }

        private void UpdateRosterReadyState(
            NetworkRunner runner,
            bool rosterFilled
        )
        {
            if (!rosterFilled)
            {
                MarkRosterIncompleteAndCancelCountdown(); // 목표 인원 부족 시 Countdown 취소

                return;
            }

            if (rosterFilledObservedAt < 0f)
            {
                rosterFilledObservedAt =
                    Time.unscaledTime; // 전원 충원 최초 관측 시간 저장

                cachedRosterStable =
                    false; // 안정화 시간 동안 Countdown 차단

                return;
            }

            float filledSeconds =
                Time.unscaledTime -
                rosterFilledObservedAt; // 전원 충원 유지 시간 계산

            bool shouldStartCountdown =
                ProjectJBotCompetitionPolicy.ShouldStartCountdown(
                    true,
                    filledSeconds,
                    RosterStableSeconds
                ); // 충원 안정화 Countdown 조건 계산

            cachedRosterStable =
                shouldStartCountdown; // 공통 Countdown Gate 상태 갱신

            if (!shouldStartCountdown)
            {
                return; // 아직 안정화 대기
            }

            TryBeginCountdown(
                runner
            ); // 전원 충원 후 3초 Countdown 시작 시도
        }

        private void MarkRosterIncompleteAndCancelCountdown()
        {
            ResetRosterReadiness(); // Roster 안정 상태 초기화

            if (humans.Count == 0)
            {
                return; // Coordinator가 될 Human 없음 처리
            }

            ProjectJNetworkExternalGameplay gameplay =
                humans[0] != null
                    ? humans[0].GetComponent<ProjectJNetworkExternalGameplay>()
                    : null; // Countdown 취소 요청용 Human Gameplay 조회

            if (gameplay != null)
            {
                gameplay.CancelCountdownForRosterAuthority(); // Countdown 중 인원 부족 시 Preparing 복귀
            }
        }

        private void TryBeginCountdown(
            NetworkRunner runner
        )
        {
            if (
                runner == null ||
                humans.Count == 0
            )
            {
                return; // Host Runner 또는 Human 없음 처리
            }

            ProjectJNetworkExternalGameplay gameplay =
                humans[0] != null
                    ? humans[0].GetComponent<ProjectJNetworkExternalGameplay>()
                    : null; // Countdown 요청용 Human Gameplay 조회

            if (gameplay == null)
            {
                return; // Human Gameplay 누락 처리
            }

            gameplay.TryBeginCountdownForFilledRosterAuthority(
                targetParticipantCount
            ); // 정확한 목표 인원 충원 후 Countdown 시작 요청
        }

        private void ResetRosterReadiness()
        {
            rosterFilledObservedAt =
                -1f; // 전원 충원 관측 시간 초기화

            cachedRosterStable =
                false; // Countdown Gate 잠금
        }

        private void CollectParticipants(
            NetworkRunner runner
        )
        {
            humans.Clear(); // 이전 Human 목록 제거
            bots.Clear(); // 이전 Bot 목록 제거

            ProjectJNetworkPlayer[] players =
                Object.FindObjectsByType<ProjectJNetworkPlayer>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                ); // 현재 활성 Network Player 수집

            for (
                int index = 0;
                index < players.Length;
                index++
            )
            {
                ProjectJNetworkPlayer player =
                    players[index]; // 현재 Player 후보 조회

                if (
                    player == null ||
                    player.Runner !=
                    runner
                )
                {
                    continue; // 다른 Runner·누락 Player 제외
                }

                if (
                    player.GetComponent<ProjectJNetworkBotMarker>() !=
                    null
                )
                {
                    bots.Add(
                        player
                    ); // Bot 참가자 추가

                    continue;
                }

                humans.Add(
                    player
                ); // Human 참가자 추가
            }
        }

        private void SpawnOneBot(
            NetworkRunner runner
        )
        {
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
                    "[Project J/Day138] ProjectJNetworkBot Resource Prefab을 찾지 못했습니다."
                ); // Bot Prefab 누락 오류 출력

                return;
            }

            Transform spawnPoint =
                ResolveBotSpawnPoint(); // 현재 참가자가 점유하지 않은 Spawn Point 선택

            if (spawnPoint == null)
            {
                Debug.LogWarning(
                    "[Project J/Day139] 비어 있는 Bot Spawn Slot이 없어 다음 Roster Tick까지 Spawn을 보류합니다."
                ); // 안전 Spawn Slot 대기 출력

                return; // 겹친 위치 강제 Spawn 차단
            }

            Vector3 spawnPosition =
                spawnPoint.position; // 안전 Spawn 위치 적용

            Quaternion spawnRotation =
                spawnPoint.rotation; // 안전 Spawn 회전 적용

            NetworkObject spawnedBot =
                runner.Spawn(
                    botPrefab,
                    spawnPosition,
                    spawnRotation,
                    PlayerRef.None
                ); // Input Authority 없는 Host Bot 생성

            if (spawnedBot == null)
            {
                Debug.LogError(
                    "[Project J/Day138] 부족 인원 Bot Spawn에 실패했습니다."
                ); // Fusion Bot Spawn 실패 출력

                return;
            }

            ProjectJNetworkBotController controller =
                spawnedBot.GetComponent<ProjectJNetworkBotController>(); // Bot Route Controller 조회

            ProjectJNetworkBotActionController actionController =
                spawnedBot.GetComponent<ProjectJNetworkBotActionController>(); // Bot 경쟁 Controller 조회

            if (
                controller == null ||
                actionController == null
            )
            {
                Debug.LogError(
                    "[Project J/Day138] Bot Prefab의 Day138 필수 Component가 누락되었습니다."
                ); // Bot Prefab 구성 오류 출력

                runner.Despawn(
                    spawnedBot
                ); // 잘못된 Bot NetworkObject 정리

                return;
            }

            controller.RefreshRoute(
                spawnedBot.GetComponent<ProjectJNetworkPlayer>()
            ); // Spawn 직후 Route 목록 갱신

            controller.ConfigureStartDelay(
                ProjectJBotSpawnPolicy.ResolveStartDelaySeconds(
                    bots.Count,
                    BotStartDelayIntervalSeconds,
                    BotStartDelayMaximumSeconds
                )
            ); // Bot별 경기 시작 순차 출발 지연 적용

            botNameSequence++; // Bot 이름 일련번호 증가

            spawnedBot.name =
                "NetworkBot_Day138_" +
                botNameSequence.ToString(
                    "00"
                ); // Host Hierarchy Bot 이름 지정
        }

        private void DespawnOneBot(
            NetworkRunner runner
        )
        {
            if (bots.Count == 0)
            {
                return; // 제거할 Bot 없음 처리
            }

            ProjectJNetworkPlayer target =
                bots[bots.Count - 1]; // 마지막 Bot 제거 대상으로 선택

            NetworkObject networkObject =
                target != null
                    ? target.GetComponent<NetworkObject>()
                    : null; // Bot NetworkObject 조회

            if (
                networkObject == null ||
                !networkObject.IsValid
            )
            {
                return; // 잘못된 Bot 제거 대상 차단
            }

            runner.Despawn(
                networkObject
            ); // Human 증가로 초과 Bot 제거
        }

        private Transform ResolveBotSpawnPoint()
        {
            Transform bestSpawnPoint =
                null; // 현재 가장 안전한 Spawn Point 초기화

            float bestNearestDistance =
                -1f; // 가장 가까운 참가자 거리 최대값 초기화

            for (
                int index = spawnPoints.Count - 1;
                index >= 0;
                index--
            )
            {
                Transform candidate =
                    spawnPoints[index]; // 현재 Spawn Point 후보 조회

                if (candidate == null)
                {
                    continue; // 누락 Spawn Point 제외
                }

                float nearestDistance =
                    ResolveNearestParticipantHorizontalDistance(
                        candidate.position
                    ); // 기존 Human·Bot과 최근접 수평 거리 계산

                if (
                    !ProjectJBotSpawnPolicy.IsSpawnSlotClear(
                        nearestDistance,
                        BotSpawnParticipantClearance
                    )
                )
                {
                    continue; // 참가자와 겹치는 Spawn Point 제외
                }

                if (
                    bestSpawnPoint != null &&
                    nearestDistance <=
                    bestNearestDistance
                )
                {
                    continue; // 더 가까운 Spawn Point 제외
                }

                bestSpawnPoint =
                    candidate; // 가장 안전한 Spawn Point 갱신

                bestNearestDistance =
                    nearestDistance; // 최근접 거리 기준 갱신
            }

            return bestSpawnPoint; // 점유되지 않은 최적 Spawn Point 반환
        }

        private float ResolveNearestParticipantHorizontalDistance(
            Vector3 spawnPosition
        )
        {
            float nearestDistance =
                float.PositiveInfinity; // 주변 참가자 없음 기본 거리

            for (
                int index = 0;
                index < humans.Count;
                index++
            )
            {
                nearestDistance =
                    ResolveParticipantDistance(
                        humans[index],
                        spawnPosition,
                        nearestDistance
                    ); // Human과 최근접 거리 갱신
            }

            for (
                int index = 0;
                index < bots.Count;
                index++
            )
            {
                nearestDistance =
                    ResolveParticipantDistance(
                        bots[index],
                        spawnPosition,
                        nearestDistance
                    ); // 기존 Bot과 최근접 거리 갱신
            }

            return nearestDistance; // Spawn Point 최근접 참가자 거리 반환
        }

        private static float ResolveParticipantDistance(
            ProjectJNetworkPlayer participant,
            Vector3 spawnPosition,
            float currentNearestDistance
        )
        {
            if (participant == null)
            {
                return currentNearestDistance; // 누락 참가자 제외
            }

            Vector3 delta =
                participant.CurrentPosition -
                spawnPosition; // Spawn Point 기준 참가자 위치 차이 계산

            delta.y =
                0f; // 수평 간격만 Spawn 점유 판정에 사용

            return Mathf.Min(
                currentNearestDistance,
                delta.magnitude
            ); // 최근접 참가자 거리 갱신
        }

        private void CollectSpawnPoints(
            Scene scene
        )
        {
            spawnPoints.Clear(); // 이전 Spawn Point 목록 제거

            GameObject[] roots =
                scene.GetRootGameObjects(); // Game Scene Root 목록 조회

            for (
                int index = 0;
                index < roots.Length;
                index++
            )
            {
                CollectSpawnPointsRecursive(
                    roots[index].transform
                ); // Scene Hierarchy Spawn Point 재귀 수집
            }

            spawnPoints.Sort(
                CompareSpawnPoints
            ); // Spawn 이름 순서 정렬
        }

        private void CollectSpawnPointsRecursive(
            Transform current
        )
        {
            if (current == null)
            {
                return; // null Transform 처리
            }

            if (
                current.name.StartsWith(
                    "Spawn_",
                    System.StringComparison.Ordinal
                )
            )
            {
                spawnPoints.Add(
                    current
                ); // Spawn Point 이름 대상 추가
            }

            for (
                int index = 0;
                index < current.childCount;
                index++
            )
            {
                CollectSpawnPointsRecursive(
                    current.GetChild(
                        index
                    )
                ); // 자식 Spawn Point 탐색
            }
        }

        private static int CompareSpawnPoints(
            Transform left,
            Transform right
        )
        {
            if (left == null)
            {
                return right == null
                    ? 0
                    : 1; // null Spawn 뒤로 정렬
            }

            if (right == null)
            {
                return -1; // 유효 Spawn 앞으로 정렬
            }

            return string.CompareOrdinal(
                left.name,
                right.name
            ); // Spawn 이름 오름차순 정렬
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
    }
}
