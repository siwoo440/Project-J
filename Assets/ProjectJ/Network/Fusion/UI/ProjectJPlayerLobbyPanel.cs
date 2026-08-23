using System.Collections.Generic; // PlayerRef 정렬 목록 사용
using Fusion; // NetworkRunner와 PlayerRef 사용
using UnityEngine; // MonoBehaviour와 Mathf 사용
using UnityEngine.UI; // Button과 Text 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJPlayerLobbyPanel :
        MonoBehaviour
    {
        private const int PlayersPerPage =
            8; // 한 페이지 표시 인원

        private const int MaximumSupportedPlayers =
            32; // 페이지 구조 최대 인원

        private const int CurrentPrivateRoomCapacity =
            8; // 현재 비공개 방 최대 인원

        [SerializeField]
        private Button previousPageButton; // 이전 페이지 버튼

        [SerializeField]
        private Button nextPageButton; // 다음 페이지 버튼

        [SerializeField]
        private Text pageText; // 페이지 표시

        [SerializeField]
        private Text readySummaryText; // Ready 요약

        [SerializeField]
        private Text roomNameText; // Preview 방 이름 / Network 방 코드

        [SerializeField]
        private Text playerCountText; // 참가 인원 표시

        [SerializeField]
        private Text roundText; // Preview 라운드 / Network 로컬 역할

        [SerializeField]
        private Text difficultyText; // Preview 난이도 / Network Ready 수

        [SerializeField]
        private Text passwordText; // Preview 비밀번호 / Network Flow 상태

        [SerializeField]
        private GameObject[] slotRoots; // Player Slot 루트

        [SerializeField]
        private Text[] slotIndexTexts; // Slot 번호

        [SerializeField]
        private Text[] slotNameTexts; // Player 이름

        [SerializeField]
        private Text[] slotStateTexts; // 역할과 Ready 상태

        [SerializeField]
        private Button readyButton; // 로컬 Ready 버튼

        [SerializeField]
        private Button leaveButton; // Lobby 나가기 버튼

        [SerializeField]
        private bool useNetworkData; // 실제 Fusion 데이터 사용 여부

        private readonly List<PlayerRef> activePlayers =
            new List<PlayerRef>(
                MaximumSupportedPlayers
            ); // 활성 Player 정렬 버퍼

        private readonly PlayerRef[] slotPlayers =
            new PlayerRef[
                MaximumSupportedPlayers
            ]; // Slot별 PlayerRef

        private readonly bool[] slotOccupied =
            new bool[
                MaximumSupportedPlayers
            ]; // Slot 사용 상태

        private readonly ProjectJNetworkExternalGameplay[] slotGameplay =
            new ProjectJNetworkExternalGameplay[
                MaximumSupportedPlayers
            ]; // Slot별 Ready 상태 컴포넌트

        private ProjectJDay82SceneFlowCoordinator sceneFlow; // 전체 Scene Flow

        private ProjectJFusionBootstrap bootstrap; // Fusion Bootstrap

        private int hostPlayerIndex =
            -1; // 현재 Host PlayerRef Index

        private int currentPage; // 현재 페이지

        private int totalSlots =
            CurrentPrivateRoomCapacity; // 현재 표시 Slot 수

        public int CurrentPage =>
            currentPage; // 현재 페이지 조회

        public int PageCount =>
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    totalSlots /
                    (float)PlayersPerPage
                )
            ); // 전체 페이지 수 계산

        public void Configure(
            Button previousButton,
            Button nextButton,
            Text pageLabel,
            Text readySummary,
            Text roomName,
            Text playerCount,
            Text rounds,
            Text difficulty,
            Text password,
            GameObject[] roots,
            Text[] indexLabels,
            Text[] playerNames,
            Text[] states
        )
        {
            previousPageButton =
                previousButton; // 이전 페이지 연결

            nextPageButton =
                nextButton; // 다음 페이지 연결

            pageText =
                pageLabel; // 페이지 Text 연결

            readySummaryText =
                readySummary; // Ready 요약 연결

            roomNameText =
                roomName; // 방 정보 Text 연결

            playerCountText =
                playerCount; // 참가 인원 Text 연결

            roundText =
                rounds; // 세 번째 정보 Text 연결

            difficultyText =
                difficulty; // 네 번째 정보 Text 연결

            passwordText =
                password; // 다섯 번째 정보 Text 연결

            slotRoots =
                roots; // Slot 루트 배열 연결

            slotIndexTexts =
                indexLabels; // Slot 번호 배열 연결

            slotNameTexts =
                playerNames; // Player 이름 배열 연결

            slotStateTexts =
                states; // 상태 배열 연결
        }

        public void ConfigureNetwork(
            Button localReadyButton,
            Button lobbyLeaveButton
        )
        {
            readyButton =
                localReadyButton; // Ready 버튼 연결

            leaveButton =
                lobbyLeaveButton; // Leave 버튼 연결

            useNetworkData =
                true; // 실제 Fusion 표시 활성화
        }

        private void Awake()
        {
            BindUi(); // 버튼 이벤트 연결
            ResolveNetworkReferences(); // Fusion 참조 조회
            RefreshPage(); // 최초 페이지 표시
        }

        private void OnEnable()
        {
            ResolveNetworkReferences(); // Scene 전환 후 참조 보정

            if (useNetworkData)
            {
                RefreshNetworkData(); // 실제 Lobby 데이터 즉시 표시
                return;
            }

            RefreshPage(); // Preview 페이지 표시
        }

        private void Update()
        {
            if (!useNetworkData)
            {
                return; // Preview에서는 네트워크 갱신 제외
            }

            RefreshNetworkData(); // 참가·이탈·Ready 실시간 반영
        }

        private void OnDestroy()
        {
            UnbindUi(); // 버튼 이벤트 해제
        }

        public void SetPreviewData(
            string roomName,
            int maxPlayers,
            int roundCount,
            string difficulty,
            bool usePassword
        )
        {
            useNetworkData =
                false; // Preview 모드 유지

            totalSlots =
                Mathf.Clamp(
                    maxPlayers,
                    1,
                    MaximumSupportedPlayers
                ); // Preview 최대 인원 반영

            currentPage =
                Mathf.Clamp(
                    currentPage,
                    0,
                    PageCount - 1
                ); // 유효 페이지 유지

            if (roomNameText != null)
            {
                roomNameText.text =
                    string.IsNullOrWhiteSpace(
                        roomName
                    )
                        ? "MY ROOM"
                        : roomName; // Preview 방 이름 표시
            }

            if (playerCountText != null)
            {
                playerCountText.text =
                    "1 / " +
                    totalSlots; // Preview 참가 인원 표시
            }

            if (roundText != null)
            {
                roundText.text =
                    roundCount +
                    " ROUND"; // Preview 라운드 표시
            }

            if (difficultyText != null)
            {
                difficultyText.text =
                    string.IsNullOrWhiteSpace(
                        difficulty
                    )
                        ? "NORMAL"
                        : difficulty; // Preview 난이도 표시
            }

            if (passwordText != null)
            {
                passwordText.text =
                    usePassword
                        ? "ON"
                        : "OFF"; // Preview 비밀번호 표시
            }

            if (readySummaryText != null)
            {
                readySummaryText.text =
                    "READY  1 / " +
                    totalSlots; // Preview 임시 Ready 표시
            }

            RefreshPage(); // Preview Slot 갱신
        }

        public void PreviousPage()
        {
            if (currentPage <= 0)
            {
                return; // 첫 페이지 이전 이동 차단
            }

            currentPage--; // 이전 페이지 이동
            RefreshPage(); // 페이지 갱신
        }

        public void NextPage()
        {
            if (
                currentPage >=
                PageCount - 1
            )
            {
                return; // 마지막 페이지 다음 이동 차단
            }

            currentPage++; // 다음 페이지 이동
            RefreshPage(); // 페이지 갱신
        }

        private void BindUi()
        {
            if (previousPageButton != null)
            {
                previousPageButton.onClick
                    .AddListener(
                        PreviousPage
                    ); // 이전 페이지 이벤트 연결
            }

            if (nextPageButton != null)
            {
                nextPageButton.onClick
                    .AddListener(
                        NextPage
                    ); // 다음 페이지 이벤트 연결
            }

            if (readyButton != null)
            {
                readyButton.onClick
                    .AddListener(
                        ToggleLocalReady
                    ); // Ready 이벤트 연결
            }

            if (leaveButton != null)
            {
                leaveButton.onClick
                    .AddListener(
                        LeaveLobby
                    ); // Leave 이벤트 연결
            }
        }

        private void UnbindUi()
        {
            if (previousPageButton != null)
            {
                previousPageButton.onClick
                    .RemoveListener(
                        PreviousPage
                    ); // 이전 페이지 이벤트 해제
            }

            if (nextPageButton != null)
            {
                nextPageButton.onClick
                    .RemoveListener(
                        NextPage
                    ); // 다음 페이지 이벤트 해제
            }

            if (readyButton != null)
            {
                readyButton.onClick
                    .RemoveListener(
                        ToggleLocalReady
                    ); // Ready 이벤트 해제
            }

            if (leaveButton != null)
            {
                leaveButton.onClick
                    .RemoveListener(
                        LeaveLobby
                    ); // Leave 이벤트 해제
            }
        }

        private void ResolveNetworkReferences()
        {
            if (sceneFlow == null)
            {
                sceneFlow =
                    Object.FindFirstObjectByType<
                        ProjectJDay82SceneFlowCoordinator
                    >(); // 영구 Scene Flow 조회
            }

            if (
                bootstrap == null &&
                sceneFlow != null
            )
            {
                bootstrap =
                    sceneFlow.Bootstrap; // Scene Flow의 Bootstrap 사용
            }

            if (bootstrap == null)
            {
                bootstrap =
                    Object.FindFirstObjectByType<
                        ProjectJFusionBootstrap
                    >(); // 직접 Bootstrap 보정 조회
            }
        }

        private void RefreshNetworkData()
        {
            ResolveNetworkReferences(); // 네트워크 참조 재확인

            NetworkRunner runner =
                bootstrap != null
                    ? bootstrap.Runner
                    : null; // 현재 Runner 조회

            if (
                runner == null ||
                !runner.IsRunning
            )
            {
                ApplyDisconnectedView(); // 연결 전 표시
                return;
            }

            CollectActivePlayers(
                runner
            ); // ActivePlayers 정렬과 Slot 배치

            int participantCount =
                activePlayers.Count; // 실제 참가자 수

            int readyCount =
                CountReadyPlayers(); // 실제 Ready 수

            if (readySummaryText != null)
            {
                readySummaryText.text =
                    "READY  " +
                    readyCount +
                    " / " +
                    participantCount; // Ready 요약 표시
            }

            if (roomNameText != null)
            {
                roomNameText.text =
                    bootstrap != null
                        ? bootstrap.ConnectedRoomCode
                        : "-"; // Room Code 표시
            }

            if (playerCountText != null)
            {
                playerCountText.text =
                    participantCount +
                    " / " +
                    CurrentPrivateRoomCapacity; // 실제 참가 인원 표시
            }

            if (roundText != null)
            {
                roundText.text =
                    runner.IsSceneAuthority
                        ? "HOST"
                        : "CLIENT"; // 로컬 역할 표시
            }

            if (difficultyText != null)
            {
                difficultyText.text =
                    readyCount +
                    " / " +
                    participantCount; // Ready 수 Match Info 표시
            }

            if (passwordText != null)
            {
                passwordText.text =
                    sceneFlow != null &&
                    sceneFlow.LobbyFlow != null
                        ? sceneFlow.LobbyFlow.Phase
                            .ToString()
                        : "Lobby"; // 현재 Lobby Flow 표시
            }

            RefreshReadyButton(
                runner
            ); // 로컬 Ready 버튼 상태 갱신

            if (leaveButton != null)
            {
                leaveButton.interactable =
                    sceneFlow != null; // Scene Flow가 있을 때 Leave 허용
            }

            RefreshPage(); // 실제 Slot 화면 갱신
        }

        private void CollectActivePlayers(
            NetworkRunner runner
        )
        {
            activePlayers.Clear(); // 이전 참가자 목록 제거

            for (
                int index = 0;
                index < slotOccupied.Length;
                index++
            )
            {
                slotOccupied[index] =
                    false; // Slot 점유 초기화

                slotPlayers[index] =
                    default; // Slot PlayerRef 초기화

                slotGameplay[index] =
                    null; // Slot Ready 컴포넌트 초기화
            }

            foreach (
                PlayerRef player
                in runner.ActivePlayers
            )
            {
                activePlayers.Add(
                    player
                ); // ActivePlayers 수집
            }

            activePlayers.Sort(
                ComparePlayerRefs
            ); // PlayerRef Index 기준 정렬

            hostPlayerIndex =
                activePlayers.Count > 0
                    ? activePlayers[0]
                        .AsIndex
                    : -1; // 가장 먼저 참가한 Player를 Host Slot 기준으로 사용

            int highestAssignedSlot =
                CurrentPrivateRoomCapacity - 1; // 기본 8 Slot 유지

            for (
                int playerListIndex = 0;
                playerListIndex <
                    activePlayers.Count;
                playerListIndex++
            )
            {
                PlayerRef player =
                    activePlayers[
                        playerListIndex
                    ]; // 현재 PlayerRef 조회

                int preferredSlot =
                    player.AsIndex; // PlayerRef Index를 우선 Slot으로 사용

                int slotIndex =
                    IsAvailableSlot(
                        preferredSlot
                    )
                        ? preferredSlot
                        : FindFirstEmptySlot(); // 범위 밖 Index는 첫 빈 Slot에 배치

                if (slotIndex < 0)
                {
                    continue; // 표시 가능한 Slot 없음
                }

                slotOccupied[slotIndex] =
                    true; // Slot 사용 표시

                slotPlayers[slotIndex] =
                    player; // Slot PlayerRef 저장

                if (
                    runner.TryGetPlayerObject(
                        player,
                        out NetworkObject playerObject
                    ) &&
                    playerObject != null
                )
                {
                    slotGameplay[slotIndex] =
                        playerObject.GetComponent<
                            ProjectJNetworkExternalGameplay
                        >(); // Ready 상태 컴포넌트 저장
                }

                highestAssignedSlot =
                    Mathf.Max(
                        highestAssignedSlot,
                        slotIndex
                    ); // 필요한 페이지 범위 계산
            }

            totalSlots =
                Mathf.Clamp(
                    highestAssignedSlot + 1,
                    CurrentPrivateRoomCapacity,
                    MaximumSupportedPlayers
                ); // 최소 8 Slot과 미래 페이지 구조 유지

            currentPage =
                Mathf.Clamp(
                    currentPage,
                    0,
                    PageCount - 1
                ); // 참가자 변화 후 페이지 보정
        }

        private static int ComparePlayerRefs(
            PlayerRef left,
            PlayerRef right
        )
        {
            return left.AsIndex
                .CompareTo(
                    right.AsIndex
                ); // PlayerRef Index 오름차순
        }

        private bool IsAvailableSlot(
            int slotIndex
        )
        {
            return
                slotIndex >= 0 &&
                slotIndex <
                    MaximumSupportedPlayers &&
                !slotOccupied[
                    slotIndex
                ]; // 유효하고 비어 있는 Slot 확인
        }

        private int FindFirstEmptySlot()
        {
            for (
                int index = 0;
                index <
                    MaximumSupportedPlayers;
                index++
            )
            {
                if (!slotOccupied[index])
                {
                    return index; // 첫 빈 Slot 반환
                }
            }

            return -1; // 빈 Slot 없음
        }

        private int CountReadyPlayers()
        {
            int readyCount =
                0; // Ready 수 초기화

            for (
                int index = 0;
                index <
                    slotGameplay.Length;
                index++
            )
            {
                if (
                    slotOccupied[index] &&
                    slotGameplay[index] != null &&
                    slotGameplay[index]
                        .LobbyReady
                )
                {
                    readyCount++; // 실제 Ready Player 집계
                }
            }

            return readyCount; // Ready 수 반환
        }

        private void RefreshReadyButton(
            NetworkRunner runner
        )
        {
            if (readyButton == null)
            {
                return; // Ready 버튼 없음
            }

            ProjectJNetworkExternalGameplay
                localGameplay =
                    GetLocalGameplay(
                        runner
                    ); // 로컬 Player Ready 컴포넌트 조회

            bool canReady =
                localGameplay != null; // 로컬 Player 준비 여부

            readyButton.interactable =
                canReady; // Player Spawn 완료 후 버튼 활성화

            Text label =
                readyButton.GetComponentInChildren<
                    Text
                >(); // 버튼 Label 조회

            if (label == null)
            {
                return; // Label 없음
            }

            if (!canReady)
            {
                label.text =
                    "READY"; // 연결 대기 기본 표시
                return;
            }

            label.text =
                localGameplay.LobbyReady
                    ? "CANCEL READY"
                    : "READY"; // 현재 Ready 상태에 맞는 버튼 문구
        }

        private ProjectJNetworkExternalGameplay
            GetLocalGameplay(
                NetworkRunner runner
            )
        {
            if (
                runner == null ||
                !runner.IsRunning
            )
            {
                return null; // Runner 없음
            }

            PlayerRef localPlayer =
                runner.LocalPlayer; // 로컬 PlayerRef 조회

            if (
                runner.TryGetPlayerObject(
                    localPlayer,
                    out NetworkObject playerObject
                ) &&
                playerObject != null
            )
            {
                return playerObject.GetComponent<
                    ProjectJNetworkExternalGameplay
                >(); // 로컬 Ready 컴포넌트 반환
            }

            return null; // Player Object Spawn 대기
        }

        private void ToggleLocalReady()
        {
            ResolveNetworkReferences(); // 최신 Bootstrap 조회

            NetworkRunner runner =
                bootstrap != null
                    ? bootstrap.Runner
                    : null; // 현재 Runner 조회

            ProjectJNetworkExternalGameplay
                localGameplay =
                    GetLocalGameplay(
                        runner
                    ); // 로컬 Ready 컴포넌트 조회

            if (localGameplay == null)
            {
                return; // Player Spawn 전 입력 차단
            }

            localGameplay
                .RequestToggleLobbyReady(); // 기존 RPC Ready 흐름 호출
        }

        private void LeaveLobby()
        {
            ResolveNetworkReferences(); // 최신 Scene Flow 조회

            if (sceneFlow == null)
            {
                return; // Scene Flow 없음
            }

            sceneFlow
                .RequestLeaveToMainMenu(); // 기존 Session 종료와 MainMenu 복귀 호출
        }

        private void ApplyDisconnectedView()
        {
            totalSlots =
                CurrentPrivateRoomCapacity; // 기본 8 Slot 유지

            hostPlayerIndex =
                -1; // Host 표시 초기화

            for (
                int index = 0;
                index <
                    MaximumSupportedPlayers;
                index++
            )
            {
                slotOccupied[index] =
                    false; // 모든 Slot 비움

                slotGameplay[index] =
                    null; // Ready 참조 제거
            }

            if (readySummaryText != null)
            {
                readySummaryText.text =
                    "CONNECTING..."; // 연결 대기 표시
            }

            if (roomNameText != null)
            {
                roomNameText.text =
                    "-"; // Room Code 대기
            }

            if (playerCountText != null)
            {
                playerCountText.text =
                    "0 / " +
                    CurrentPrivateRoomCapacity; // 참가자 0명 표시
            }

            if (roundText != null)
            {
                roundText.text =
                    "-"; // 역할 대기
            }

            if (difficultyText != null)
            {
                difficultyText.text =
                    "0 / 0"; // Ready 대기
            }

            if (passwordText != null)
            {
                passwordText.text =
                    "Connecting"; // Flow 대기
            }

            if (readyButton != null)
            {
                readyButton.interactable =
                    false; // 연결 전 Ready 차단
            }

            if (leaveButton != null)
            {
                leaveButton.interactable =
                    sceneFlow != null; // Scene Flow 존재 시 나가기 허용
            }

            RefreshPage(); // 빈 Slot 표시
        }

        private void RefreshPage()
        {
            int pageCount =
                PageCount; // 현재 페이지 수 계산

            currentPage =
                Mathf.Clamp(
                    currentPage,
                    0,
                    pageCount - 1
                ); // 유효 페이지 보정

            bool multiplePages =
                pageCount > 1; // 다중 페이지 여부

            if (previousPageButton != null)
            {
                previousPageButton.gameObject
                    .SetActive(
                        multiplePages
                    ); // 2페이지 이상에서만 표시

                previousPageButton.interactable =
                    currentPage > 0; // 첫 페이지 이전 버튼 차단
            }

            if (nextPageButton != null)
            {
                nextPageButton.gameObject
                    .SetActive(
                        multiplePages
                    ); // 2페이지 이상에서만 표시

                nextPageButton.interactable =
                    currentPage <
                    pageCount - 1; // 마지막 페이지 다음 버튼 차단
            }

            if (pageText != null)
            {
                pageText.text =
                    "PAGE " +
                    (currentPage + 1) +
                    " / " +
                    pageCount; // 페이지 번호 표시

                pageText.gameObject.SetActive(
                    multiplePages
                ); // 1페이지에서는 숨김
            }

            for (
                int localIndex = 0;
                localIndex <
                    PlayersPerPage;
                localIndex++
            )
            {
                int globalIndex =
                    currentPage *
                    PlayersPerPage +
                    localIndex; // 실제 Slot Index 계산

                bool validSlot =
                    globalIndex <
                    totalSlots; // 현재 페이지 유효 Slot 확인

                SetSlotActive(
                    localIndex,
                    validSlot
                ); // Slot 활성 상태 적용

                if (!validSlot)
                {
                    continue; // 범위 밖 Slot 제외
                }

                if (useNetworkData)
                {
                    SetNetworkSlotText(
                        localIndex,
                        globalIndex
                    ); // 실제 Fusion Slot 표시
                    continue;
                }

                bool isHost =
                    globalIndex == 0; // Preview Host Slot 확인

                SetPreviewSlotText(
                    localIndex,
                    globalIndex,
                    isHost
                ); // Preview Slot 표시
            }
        }

        private void SetSlotActive(
            int localIndex,
            bool active
        )
        {
            if (
                slotRoots == null ||
                localIndex < 0 ||
                localIndex >=
                    slotRoots.Length
            )
            {
                return; // Slot 배열 범위 확인
            }

            if (
                slotRoots[
                    localIndex
                ] != null
            )
            {
                slotRoots[
                    localIndex
                ].SetActive(
                    active
                ); // Slot GameObject 활성화 적용
            }
        }

        private void SetNetworkSlotText(
            int localIndex,
            int globalIndex
        )
        {
            SetSlotIndexText(
                localIndex,
                globalIndex
            ); // Slot 번호 표시

            bool occupied =
                globalIndex >= 0 &&
                globalIndex <
                    slotOccupied.Length &&
                slotOccupied[
                    globalIndex
                ]; // 실제 참가자 존재 확인

            if (!occupied)
            {
                SetSlotNameText(
                    localIndex,
                    "WAITING..."
                ); // 빈 Slot 이름 표시

                SetSlotStateText(
                    localIndex,
                    "EMPTY"
                ); // 빈 Slot 상태 표시

                return;
            }

            PlayerRef player =
                slotPlayers[
                    globalIndex
                ]; // Slot PlayerRef 조회

            bool isLocalPlayer =
                bootstrap != null &&
                bootstrap.Runner != null &&
                player.AsIndex ==
                    bootstrap.Runner
                        .LocalPlayer.AsIndex; // 로컬 Player 여부

            string playerName =
                "PLAYER " +
                (player.AsIndex + 1)
                    .ToString("00"); // Player 표시명 생성

            if (isLocalPlayer)
            {
                playerName +=
                    " (YOU)"; // 로컬 Player 표시 추가
            }

            SetSlotNameText(
                localIndex,
                playerName
            ); // Player 이름 적용

            ProjectJNetworkExternalGameplay
                gameplay =
                    slotGameplay[
                        globalIndex
                    ]; // Ready 상태 컴포넌트 조회

            bool ready =
                gameplay != null &&
                gameplay.LobbyReady; // 실제 Ready 상태 확인

            string role =
                player.AsIndex ==
                    hostPlayerIndex
                    ? "HOST"
                    : "CLIENT"; // Host/Client 역할 계산

            string state =
                role +
                " / " +
                (
                    ready
                        ? "READY"
                        : "NOT READY"
                ); // 역할과 Ready 문자열 생성

            SetSlotStateText(
                localIndex,
                state
            ); // Slot 상태 적용
        }

        private void SetPreviewSlotText(
            int localIndex,
            int globalIndex,
            bool isHost
        )
        {
            SetSlotIndexText(
                localIndex,
                globalIndex
            ); // Preview Slot 번호 표시

            SetSlotNameText(
                localIndex,
                isHost
                    ? "PLAYER 01"
                    : "WAITING..."
            ); // Preview Player 이름 표시

            SetSlotStateText(
                localIndex,
                isHost
                    ? "HOST"
                    : "EMPTY"
            ); // Preview 상태 표시
        }

        private void SetSlotIndexText(
            int localIndex,
            int globalIndex
        )
        {
            if (
                slotIndexTexts != null &&
                localIndex <
                    slotIndexTexts.Length &&
                slotIndexTexts[
                    localIndex
                ] != null
            )
            {
                slotIndexTexts[
                    localIndex
                ].text =
                    "#" +
                    (globalIndex + 1)
                        .ToString("00"); // Slot 번호 적용
            }
        }

        private void SetSlotNameText(
            int localIndex,
            string value
        )
        {
            if (
                slotNameTexts != null &&
                localIndex <
                    slotNameTexts.Length &&
                slotNameTexts[
                    localIndex
                ] != null
            )
            {
                slotNameTexts[
                    localIndex
                ].text =
                    value; // Player 이름 적용
            }
        }

        private void SetSlotStateText(
            int localIndex,
            string value
        )
        {
            if (
                slotStateTexts != null &&
                localIndex <
                    slotStateTexts.Length &&
                slotStateTexts[
                    localIndex
                ] != null
            )
            {
                slotStateTexts[
                    localIndex
                ].text =
                    value; // 역할과 Ready 상태 적용
            }
        }
    }
}
