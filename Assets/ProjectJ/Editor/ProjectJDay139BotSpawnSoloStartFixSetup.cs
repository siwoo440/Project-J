using System.IO; // Source 읽기와 저장 사용
using System.Text; // UTF8 저장 사용
using UnityEditor; // Editor 메뉴와 Asset 갱신 사용
using UnityEngine; // Debug 출력 사용

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay139BotSpawnSoloStartFixSetup
    {
        private const string BotRosterSourcePath =
            "Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkBotRosterManager.cs"; // Bot Roster Source 경로

        private const string BotControllerSourcePath =
            "Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkBotController.cs"; // Bot Controller Source 경로

        private const string LobbyFlowSourcePath =
            "Assets/ProjectJ/Network/Fusion/Session/ProjectJNetworkLobbyFlow.cs"; // Lobby Flow Source 경로

        private static readonly UTF8Encoding Utf8WithoutBom =
            new UTF8Encoding(
                false
            ); // BOM 없는 UTF8 저장

        [MenuItem(
            "Project J/Day139/Fix Bot Spawn And Solo Start"
        )]
        private static void FixBotSpawnAndSoloStart()
        {
            if (
                !File.Exists(
                    BotRosterSourcePath
                ) ||
                !File.Exists(
                    BotControllerSourcePath
                ) ||
                !File.Exists(
                    LobbyFlowSourcePath
                )
            )
            {
                Debug.LogError(
                    "[Project J/Day139] Bot Roster, Bot Controller 또는 Lobby Flow Source를 찾지 못했습니다."
                ); // 필수 Source 누락 오류 출력

                return;
            }

            string rosterSource =
                File.ReadAllText(
                    BotRosterSourcePath
                ); // 최신 Bot Roster Source 읽기

            string controllerSource =
                File.ReadAllText(
                    BotControllerSourcePath
                ); // 최신 Bot Controller Source 읽기

            string lobbySource =
                File.ReadAllText(
                    LobbyFlowSourcePath
                ); // 최신 Lobby Flow Source 읽기

            if (
                !TryPatchBotRoster(
                    rosterSource,
                    out string patchedRosterSource
                ) ||
                !TryPatchBotController(
                    controllerSource,
                    out string patchedControllerSource
                ) ||
                !TryPatchLobbyFlow(
                    lobbySource,
                    out string patchedLobbySource
                )
            )
            {
                Debug.LogError(
                    "[Project J/Day139] 최신 main Source 패턴과 일치하지 않아 어떤 파일도 수정하지 않았습니다."
                ); // 부분 적용 방지 오류 출력

                return;
            }

            WriteIfChanged(
                BotRosterSourcePath,
                rosterSource,
                patchedRosterSource
            ); // Bot Spawn Slot 선택과 순차 출발 설정 저장

            WriteIfChanged(
                BotControllerSourcePath,
                controllerSource,
                patchedControllerSource
            ); // Bot 경기 시작 순차 출발 처리 저장

            WriteIfChanged(
                LobbyFlowSourcePath,
                lobbySource,
                patchedLobbySource
            ); // Lobby 최소 Ready 인원 1명 적용

            AssetDatabase.Refresh(); // 수정 Source 재컴파일 요청

            Debug.Log(
                "[Project J/Day139] Bot 겹침 Spawn·동시 출발 완화 및 1인 Lobby 시작 수정 적용 완료."
            ); // Day139 추가 수정 적용 결과 출력
        }

        private static bool TryPatchBotRoster(
            string source,
            out string patchedSource
        )
        {
            patchedSource =
                source; // 기본 Source 유지

            string newline =
                ResolveNewline(
                    source
                ); // 기존 줄바꿈 형식 확인

            string constantsAnchor =
                "        private const float RosterStableSeconds =" + newline +
                "            0.75f; // 전원 충원 후 Countdown 전 안정화 시간"; // 기존 Roster Constant 기준

            string constantsPatch =
                constantsAnchor + newline + newline +
                "        private const float BotSpawnParticipantClearance =" + newline +
                "            1f; // Bot Spawn 최소 참가자 수평 간격" + newline + newline +
                "        private const float BotStartDelayIntervalSeconds =" + newline +
                "            0.25f; // Bot별 경기 시작 출발 간격" + newline + newline +
                "        private const float BotStartDelayMaximumSeconds =" + newline +
                "            1.5f; // 마지막 Bot 최대 출발 지연"; // 신규 Spawn·출발 Constant

            if (
                !patchedSource.Contains(
                    "BotSpawnParticipantClearance"
                )
            )
            {
                if (
                    !patchedSource.Contains(
                        constantsAnchor
                    )
                )
                {
                    return false; // Roster Constant 기준 불일치
                }

                patchedSource =
                    patchedSource.Replace(
                        constantsAnchor,
                        constantsPatch
                    ); // Spawn 안전 간격과 출발 지연 Constant 추가
            }

            string oldSpawnPointSelection =
                "            Transform spawnPoint =" + newline +
                "                ResolveBotSpawnPoint(" + newline +
                "                    bots.Count" + newline +
                "                ); // 현재 Bot 수 기준 후방 Spawn Point 선택" + newline + newline +
                "            Vector3 spawnPosition =" + newline +
                "                spawnPoint != null" + newline +
                "                    ? spawnPoint.position" + newline +
                "                    : transform.position; // Spawn 위치 선택" + newline + newline +
                "            Quaternion spawnRotation =" + newline +
                "                spawnPoint != null" + newline +
                "                    ? spawnPoint.rotation" + newline +
                "                    : Quaternion.identity; // Spawn 회전 선택"; // 기존 Bot Count 기반 Spawn 선택

            string newSpawnPointSelection =
                "            Transform spawnPoint =" + newline +
                "                ResolveBotSpawnPoint(); // 현재 참가자가 점유하지 않은 Spawn Point 선택" + newline + newline +
                "            if (spawnPoint == null)" + newline +
                "            {" + newline +
                "                Debug.LogWarning(" + newline +
                "                    \"[Project J/Day139] 비어 있는 Bot Spawn Slot이 없어 다음 Roster Tick까지 Spawn을 보류합니다.\"" + newline +
                "                ); // 안전 Spawn Slot 대기 출력" + newline + newline +
                "                return; // 겹친 위치 강제 Spawn 차단" + newline +
                "            }" + newline + newline +
                "            Vector3 spawnPosition =" + newline +
                "                spawnPoint.position; // 안전 Spawn 위치 적용" + newline + newline +
                "            Quaternion spawnRotation =" + newline +
                "                spawnPoint.rotation; // 안전 Spawn 회전 적용"; // 안전 Spawn 선택

            if (
                !patchedSource.Contains(
                    "ResolveBotSpawnPoint(); // 현재 참가자가 점유하지 않은 Spawn Point 선택"
                )
            )
            {
                if (
                    !patchedSource.Contains(
                        oldSpawnPointSelection
                    )
                )
                {
                    return false; // Spawn Point 선택 기준 불일치
                }

                patchedSource =
                    patchedSource.Replace(
                        oldSpawnPointSelection,
                        newSpawnPointSelection
                    ); // 점유 확인형 Spawn 선택으로 변경
            }

            string refreshRouteAnchor =
                "            controller.RefreshRoute(" + newline +
                "                spawnedBot.GetComponent<ProjectJNetworkPlayer>()" + newline +
                "            ); // Spawn 직후 Route 목록 갱신"; // 기존 Route Refresh 기준

            string refreshRoutePatch =
                refreshRouteAnchor + newline + newline +
                "            controller.ConfigureStartDelay(" + newline +
                "                ProjectJBotSpawnPolicy.ResolveStartDelaySeconds(" + newline +
                "                    bots.Count," + newline +
                "                    BotStartDelayIntervalSeconds," + newline +
                "                    BotStartDelayMaximumSeconds" + newline +
                "                )" + newline +
                "            ); // Bot별 경기 시작 순차 출발 지연 적용"; // 신규 출발 지연 설정

            if (
                !patchedSource.Contains(
                    "controller.ConfigureStartDelay("
                )
            )
            {
                if (
                    !patchedSource.Contains(
                        refreshRouteAnchor
                    )
                )
                {
                    return false; // Route Refresh 기준 불일치
                }

                patchedSource =
                    patchedSource.Replace(
                        refreshRouteAnchor,
                        refreshRoutePatch
                    ); // Spawn 순서 기반 출발 지연 설정 추가
            }

            string oldResolverStart =
                "        private Transform ResolveBotSpawnPoint(" + newline +
                "            int existingBotCount" + newline +
                "        )" + newline +
                "        {" + newline +
                "            if (spawnPoints.Count == 0)" + newline +
                "            {" + newline +
                "                return null; // Spawn Point 없음 처리" + newline +
                "            }" + newline + newline +
                "            int spawnIndex =" + newline +
                "                spawnPoints.Count -" + newline +
                "                1 -" + newline +
                "                existingBotCount; // 후방 Spawn Slot부터 Bot 배치" + newline + newline +
                "            spawnIndex =" + newline +
                "                Mathf.Clamp(" + newline +
                "                    spawnIndex," + newline +
                "                    0," + newline +
                "                    spawnPoints.Count - 1" + newline +
                "                ); // Spawn Index 범위 보정" + newline + newline +
                "            return spawnPoints[spawnIndex]; // Bot Spawn Point 반환" + newline +
                "        }"; // 기존 Bot 수 기반 Resolver

            string newResolver =
                "        private Transform ResolveBotSpawnPoint()" + newline +
                "        {" + newline +
                "            Transform bestSpawnPoint =" + newline +
                "                null; // 현재 가장 안전한 Spawn Point 초기화" + newline + newline +
                "            float bestNearestDistance =" + newline +
                "                -1f; // 가장 가까운 참가자 거리 최대값 초기화" + newline + newline +
                "            for (" + newline +
                "                int index = spawnPoints.Count - 1;" + newline +
                "                index >= 0;" + newline +
                "                index--" + newline +
                "            )" + newline +
                "            {" + newline +
                "                Transform candidate =" + newline +
                "                    spawnPoints[index]; // 현재 Spawn Point 후보 조회" + newline + newline +
                "                if (candidate == null)" + newline +
                "                {" + newline +
                "                    continue; // 누락 Spawn Point 제외" + newline +
                "                }" + newline + newline +
                "                float nearestDistance =" + newline +
                "                    ResolveNearestParticipantHorizontalDistance(" + newline +
                "                        candidate.position" + newline +
                "                    ); // 기존 Human·Bot과 최근접 수평 거리 계산" + newline + newline +
                "                if (" + newline +
                "                    !ProjectJBotSpawnPolicy.IsSpawnSlotClear(" + newline +
                "                        nearestDistance," + newline +
                "                        BotSpawnParticipantClearance" + newline +
                "                    )" + newline +
                "                )" + newline +
                "                {" + newline +
                "                    continue; // 참가자와 겹치는 Spawn Point 제외" + newline +
                "                }" + newline + newline +
                "                if (" + newline +
                "                    bestSpawnPoint != null &&" + newline +
                "                    nearestDistance <=" + newline +
                "                    bestNearestDistance" + newline +
                "                )" + newline +
                "                {" + newline +
                "                    continue; // 더 가까운 Spawn Point 제외" + newline +
                "                }" + newline + newline +
                "                bestSpawnPoint =" + newline +
                "                    candidate; // 가장 안전한 Spawn Point 갱신" + newline + newline +
                "                bestNearestDistance =" + newline +
                "                    nearestDistance; // 최근접 거리 기준 갱신" + newline +
                "            }" + newline + newline +
                "            return bestSpawnPoint; // 점유되지 않은 최적 Spawn Point 반환" + newline +
                "        }" + newline + newline +
                "        private float ResolveNearestParticipantHorizontalDistance(" + newline +
                "            Vector3 spawnPosition" + newline +
                "        )" + newline +
                "        {" + newline +
                "            float nearestDistance =" + newline +
                "                float.PositiveInfinity; // 주변 참가자 없음 기본 거리" + newline + newline +
                "            for (" + newline +
                "                int index = 0;" + newline +
                "                index < humans.Count;" + newline +
                "                index++" + newline +
                "            )" + newline +
                "            {" + newline +
                "                nearestDistance =" + newline +
                "                    ResolveParticipantDistance(" + newline +
                "                        humans[index]," + newline +
                "                        spawnPosition," + newline +
                "                        nearestDistance" + newline +
                "                    ); // Human과 최근접 거리 갱신" + newline +
                "            }" + newline + newline +
                "            for (" + newline +
                "                int index = 0;" + newline +
                "                index < bots.Count;" + newline +
                "                index++" + newline +
                "            )" + newline +
                "            {" + newline +
                "                nearestDistance =" + newline +
                "                    ResolveParticipantDistance(" + newline +
                "                        bots[index]," + newline +
                "                        spawnPosition," + newline +
                "                        nearestDistance" + newline +
                "                    ); // 기존 Bot과 최근접 거리 갱신" + newline +
                "            }" + newline + newline +
                "            return nearestDistance; // Spawn Point 최근접 참가자 거리 반환" + newline +
                "        }" + newline + newline +
                "        private static float ResolveParticipantDistance(" + newline +
                "            ProjectJNetworkPlayer participant," + newline +
                "            Vector3 spawnPosition," + newline +
                "            float currentNearestDistance" + newline +
                "        )" + newline +
                "        {" + newline +
                "            if (participant == null)" + newline +
                "            {" + newline +
                "                return currentNearestDistance; // 누락 참가자 제외" + newline +
                "            }" + newline + newline +
                "            Vector3 delta =" + newline +
                "                participant.CurrentPosition -" + newline +
                "                spawnPosition; // Spawn Point 기준 참가자 위치 차이 계산" + newline + newline +
                "            delta.y =" + newline +
                "                0f; // 수평 간격만 Spawn 점유 판정에 사용" + newline + newline +
                "            return Mathf.Min(" + newline +
                "                currentNearestDistance," + newline +
                "                delta.magnitude" + newline +
                "            ); // 최근접 참가자 거리 갱신" + newline +
                "        }"; // 안전 Spawn Resolver

            if (
                !patchedSource.Contains(
                    "ResolveNearestParticipantHorizontalDistance("
                )
            )
            {
                if (
                    !patchedSource.Contains(
                        oldResolverStart
                    )
                )
                {
                    return false; // Spawn Resolver 기준 불일치
                }

                patchedSource =
                    patchedSource.Replace(
                        oldResolverStart,
                        newResolver
                    ); // 점유 검사형 Spawn Resolver 적용
            }

            return true; // Bot Roster Patch 준비 완료
        }

        private static bool TryPatchBotController(
            string source,
            out string patchedSource
        )
        {
            patchedSource =
                source; // 기본 Source 유지

            string newline =
                ResolveNewline(
                    source
                ); // 기존 줄바꿈 형식 확인

            string fieldAnchor =
                "        private bool progressTrackingInitialized; // Stuck 위치 측정 초기화 여부"; // 기존 마지막 Progress Field 기준

            string fieldPatch =
                fieldAnchor + newline +
                "        private float configuredStartDelaySeconds; // Bot별 최초 경기 시작 출발 지연" + newline +
                "        private float startDelayRemainingSeconds; // 최초 출발까지 남은 시간" + newline +
                "        private bool startDelayReleased; // 최초 출발 지연 종료 여부"; // 신규 Start Delay Field

            if (
                !patchedSource.Contains(
                    "configuredStartDelaySeconds"
                )
            )
            {
                if (
                    !patchedSource.Contains(
                        fieldAnchor
                    )
                )
                {
                    return false; // Bot Controller Field 기준 불일치
                }

                patchedSource =
                    patchedSource.Replace(
                        fieldAnchor,
                        fieldPatch
                    ); // 최초 출발 지연 상태 추가
            }

            string propertyAnchor =
                "        public float StalledSeconds =>" + newline +
                "            stalledSeconds; // 현재 정체 누적 시간 조회"; // 기존 Stalled Property 기준

            string configurePatch =
                propertyAnchor + newline + newline +
                "        public void ConfigureStartDelay(" + newline +
                "            float delaySeconds" + newline +
                "        )" + newline +
                "        {" + newline +
                "            configuredStartDelaySeconds =" + newline +
                "                Mathf.Max(" + newline +
                "                    0f," + newline +
                "                    delaySeconds" + newline +
                "                ); // Bot별 최초 출발 지연 저장" + newline + newline +
                "            startDelayRemainingSeconds =" + newline +
                "                configuredStartDelaySeconds; // 최초 출발 남은 시간 초기화" + newline + newline +
                "            startDelayReleased =" + newline +
                "                false; // 최초 Playing 진입까지 출발 잠금" + newline +
                "        }"; // 출발 지연 설정 Method

            if (
                !patchedSource.Contains(
                    "public void ConfigureStartDelay("
                )
            )
            {
                if (
                    !patchedSource.Contains(
                        propertyAnchor
                    )
                )
                {
                    return false; // Bot Controller Property 기준 불일치
                }

                patchedSource =
                    patchedSource.Replace(
                        propertyAnchor,
                        configurePatch
                    ); // Bot별 순차 출발 설정 Method 추가
            }

            string stuckAnchor =
                "            ObserveStuck(" + newline +
                "                player" + newline +
                "            ); // 정체 상태 확인 및 Route 복구"; // 기존 Stuck 호출 기준

            string startHoldPatch =
                stuckAnchor + newline + newline +
                "            if (ShouldHoldForInitialStartDelay(player))" + newline +
                "            {" + newline +
                "                input.AimDirection =" + newline +
                "                    player.transform.forward; // 대기 중 현재 몸 방향 유지" + newline + newline +
                "                return true; // 다른 Bot과 동시 출발하지 않고 현재 위치 유지" + newline +
                "            }"; // 최초 출발 지연 Gate

            if (
                !patchedSource.Contains(
                    "ShouldHoldForInitialStartDelay(player)"
                )
            )
            {
                if (
                    !patchedSource.Contains(
                        stuckAnchor
                    )
                )
                {
                    return false; // TryBuildInput Stuck 기준 불일치
                }

                patchedSource =
                    patchedSource.Replace(
                        stuckAnchor,
                        startHoldPatch
                    ); // 경쟁 행동 전 최초 출발 지연 Gate 추가
            }

            string refreshMarker =
                "        public void RefreshRoute(" + newline +
                "            ProjectJNetworkPlayer player" + newline +
                "        )"; // RefreshRoute Method 시작 기준

            string holdMethod =
                "        private bool ShouldHoldForInitialStartDelay(" + newline +
                "            ProjectJNetworkPlayer player" + newline +
                "        )" + newline +
                "        {" + newline +
                "            if (startDelayReleased)" + newline +
                "            {" + newline +
                "                return false; // 최초 출발 지연 종료 후 추가 대기 없음" + newline +
                "            }" + newline + newline +
                "            if (" + newline +
                "                externalGameplay != null &&" + newline +
                "                !externalGameplay.GameplayInputAllowed" + newline +
                "            )" + newline +
                "            {" + newline +
                "                startDelayRemainingSeconds =" + newline +
                "                    configuredStartDelaySeconds; // Countdown 동안 Bot별 지연 시간 유지" + newline + newline +
                "                return true; // 경기 시작 전 이동 입력 차단" + newline +
                "            }" + newline + newline +
                "            float deltaTime =" + newline +
                "                player != null &&" + newline +
                "                player.Runner != null" + newline +
                "                    ? Mathf.Max(" + newline +
                "                        0f," + newline +
                "                        player.Runner.DeltaTime" + newline +
                "                    )" + newline +
                "                    : 0f; // Fusion Simulation 시간 조회" + newline + newline +
                "            startDelayRemainingSeconds =" + newline +
                "                Mathf.Max(" + newline +
                "                    0f," + newline +
                "                    startDelayRemainingSeconds -" + newline +
                "                    deltaTime" + newline +
                "                ); // Bot별 최초 출발 지연 감소" + newline + newline +
                "            if (startDelayRemainingSeconds > 0f)" + newline +
                "            {" + newline +
                "                return true; // 아직 순차 출발 대기" + newline +
                "            }" + newline + newline +
                "            startDelayReleased =" + newline +
                "                true; // 최초 출발 지연 영구 종료" + newline + newline +
                "            return false; // Route 이동 시작 허용" + newline +
                "        }" + newline + newline +
                refreshMarker; // Start Delay Method와 기존 RefreshRoute 연결

            if (
                !patchedSource.Contains(
                    "private bool ShouldHoldForInitialStartDelay("
                )
            )
            {
                if (
                    !patchedSource.Contains(
                        refreshMarker
                    )
                )
                {
                    return false; // RefreshRoute 기준 불일치
                }

                patchedSource =
                    patchedSource.Replace(
                        refreshMarker,
                        holdMethod
                    ); // 최초 Playing 순차 출발 처리 Method 추가
            }

            return true; // Bot Controller Patch 준비 완료
        }

        private static bool TryPatchLobbyFlow(
            string source,
            out string patchedSource
        )
        {
            patchedSource =
                source; // 기본 Source 유지

            string newline =
                ResolveNewline(
                    source
                ); // 기존 줄바꿈 형식 확인

            string oldMinimum =
                "        private const int MinimumReadyPlayers =" + newline +
                "            2;"; // 기존 최소 Ready 2명

            string newMinimum =
                "        private const int MinimumReadyPlayers =" + newline +
                "            1;"; // Solo Lobby 최소 Ready 1명

            if (
                patchedSource.Contains(
                    newMinimum
                )
            )
            {
                return true; // 이미 1인 시작 적용 상태 유지
            }

            if (
                !patchedSource.Contains(
                    oldMinimum
                )
            )
            {
                return false; // Lobby 최소 인원 기준 불일치
            }

            patchedSource =
                patchedSource.Replace(
                    oldMinimum,
                    newMinimum
                ); // Lobby와 Game 준비 최소 인원 1명으로 변경

            return true; // Lobby Flow Patch 준비 완료
        }

        private static string ResolveNewline(
            string source
        )
        {
            return source.Contains(
                "\r\n"
            )
                ? "\r\n"
                : "\n"; // Source 기존 줄바꿈 반환
        }

        private static void WriteIfChanged(
            string path,
            string originalSource,
            string patchedSource
        )
        {
            if (
                string.Equals(
                    originalSource,
                    patchedSource,
                    System.StringComparison.Ordinal
                )
            )
            {
                return; // 이미 적용된 Source 재저장 차단
            }

            File.WriteAllText(
                path,
                patchedSource,
                Utf8WithoutBom
            ); // Patch Source 저장
        }
    }
}
