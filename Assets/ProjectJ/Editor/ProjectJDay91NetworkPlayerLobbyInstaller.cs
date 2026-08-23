using ProjectJ.Networking.Fusion; // Network Player Lobby Controller 사용
using UnityEditor; // Editor 메뉴 사용
using UnityEditor.SceneManagement; // Scene 열기와 저장
using UnityEngine; // GameObject와 RectTransform 사용
using UnityEngine.EventSystems; // EventSystem 사용
using UnityEngine.InputSystem.UI; // Input System UI 입력 사용
using UnityEngine.UI; // Canvas와 Button 사용

namespace ProjectJ.Editor
{
    internal static class
        ProjectJDay91NetworkPlayerLobbyInstaller
    {
        private const string MenuPath =
            "Project J/Scene/91일차 Network Player Lobby 구성"; // 설치 메뉴 경로

        private const string LobbyScenePath =
            "Assets/ProjectJ/Scenes/Lobby.unity"; // 실제 Fusion Lobby Scene

        private const int SlotCount =
            8; // 현재 비공개 방 Slot 수

        [MenuItem(MenuPath)]
        private static void ConfigureNetworkPlayerLobby()
        {
            if (
                !EditorSceneManager
                    .SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                return; // 저장 취소 시 설치 중단
            }

            UnityEngine.SceneManagement.Scene scene =
                EditorSceneManager.OpenScene(
                    LobbyScenePath,
                    OpenSceneMode.Single
                ); // Lobby Scene 열기

            RemoveExistingCanvas(
                scene
            ); // 재실행 시 기존 Day91 UI 제거

            Canvas canvas =
                CreateCanvas(); // Player Lobby Canvas 생성

            EnsureEventSystem(
                scene
            ); // UI 클릭용 EventSystem 보장

            GameObject background =
                CreatePanel(
                    canvas.transform,
                    "Background",
                    Vector2.zero,
                    new Vector2(
                        1920f,
                        1080f
                    ),
                    new Color(
                        0.06f,
                        0.055f,
                        0.14f,
                        1f
                    )
                ); // 전체 배경 생성

            StretchFull(
                background.GetComponent<
                    RectTransform
                >()
            ); // 배경 전체 화면 확장

            GameObject lobbyRoot =
                CreateUiObject(
                    "PlayerLobbyPanel",
                    canvas.transform
                ); // Lobby UI 루트 생성

            StretchFull(
                lobbyRoot.GetComponent<
                    RectTransform
                >()
            ); // Lobby UI 전체 화면 확장

            ProjectJPlayerLobbyPanel controller =
                lobbyRoot.AddComponent<
                    ProjectJPlayerLobbyPanel
                >(); // Network Lobby Controller 추가

            BuildHeader(
                lobbyRoot.transform,
                out Text readySummaryText
            ); // 제목과 Ready 요약 생성

            GameObject playerArea =
                CreatePanel(
                    lobbyRoot.transform,
                    "PlayerArea",
                    new Vector2(
                        -180f,
                        -15f
                    ),
                    new Vector2(
                        1320f,
                        700f
                    ),
                    new Color(
                        0.17f,
                        0.14f,
                        0.35f,
                        0.96f
                    )
                ); // Player Slot 영역 생성

            GameObject[] slotRoots =
                new GameObject[
                    SlotCount
                ]; // Slot 루트 배열 생성

            Text[] slotIndexTexts =
                new Text[
                    SlotCount
                ]; // Slot 번호 배열 생성

            Text[] slotNameTexts =
                new Text[
                    SlotCount
                ]; // Player 이름 배열 생성

            Text[] slotStateTexts =
                new Text[
                    SlotCount
                ]; // 역할과 Ready 배열 생성

            BuildPlayerSlots(
                playerArea.transform,
                slotRoots,
                slotIndexTexts,
                slotNameTexts,
                slotStateTexts
            ); // 8개 Player Slot 생성

            Button previousPageButton =
                CreateButton(
                    playerArea.transform,
                    "PreviousPageButton",
                    "<",
                    new Vector2(
                        0f,
                        0.5f
                    ),
                    new Vector2(
                        20f,
                        0f
                    ),
                    new Vector2(
                        64f,
                        120f
                    ),
                    new Vector2(
                        0f,
                        0.5f
                    ),
                    new Color(
                        0.44f,
                        0.25f,
                        0.76f,
                        1f
                    )
                ); // 이전 페이지 버튼 생성

            Button nextPageButton =
                CreateButton(
                    playerArea.transform,
                    "NextPageButton",
                    ">",
                    new Vector2(
                        1f,
                        0.5f
                    ),
                    new Vector2(
                        -20f,
                        0f
                    ),
                    new Vector2(
                        64f,
                        120f
                    ),
                    new Vector2(
                        1f,
                        0.5f
                    ),
                    new Color(
                        0.44f,
                        0.25f,
                        0.76f,
                        1f
                    )
                ); // 다음 페이지 버튼 생성

            Text pageText =
                CreateText(
                    playerArea.transform,
                    "PageText",
                    "PAGE 1 / 1",
                    17,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                ); // Page Text 생성

            SetAnchoredRect(
                pageText.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    18f
                ),
                new Vector2(
                    240f,
                    34f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            ); // Page Text 위치 설정

            pageText.color =
                new Color(
                    0.86f,
                    0.89f,
                    1f,
                    1f
                ); // Page Text 색상 설정

            GameObject matchInfo =
                CreatePanel(
                    lobbyRoot.transform,
                    "MatchInfoPanel",
                    new Vector2(
                        660f,
                        65f
                    ),
                    new Vector2(
                        360f,
                        540f
                    ),
                    new Color(
                        0.19f,
                        0.15f,
                        0.40f,
                        0.98f
                    )
                ); // 실제 Match Info 영역 생성

            BuildNetworkMatchInfo(
                matchInfo.transform,
                out Text roomCodeText,
                out Text playerCountText,
                out Text localRoleText,
                out Text readyCountText,
                out Text flowText
            ); // Network Match Info 생성

            BuildBottomButtons(
                lobbyRoot.transform,
                out Button readyButton,
                out Button leaveButton
            ); // Ready와 Leave 버튼 생성

            controller.Configure(
                previousPageButton,
                nextPageButton,
                pageText,
                readySummaryText,
                roomCodeText,
                playerCountText,
                localRoleText,
                readyCountText,
                flowText,
                slotRoots,
                slotIndexTexts,
                slotNameTexts,
                slotStateTexts
            ); // 공통 Player Lobby UI 연결

            controller.ConfigureNetwork(
                readyButton,
                leaveButton
            ); // 실제 Fusion 데이터 모드 활성화

            EditorUtility.SetDirty(
                controller
            ); // Controller 변경 저장 표시

            EditorSceneManager.MarkSceneDirty(
                scene
            ); // Lobby Scene 변경 표시

            EditorSceneManager.SaveScene(
                scene
            ); // Lobby Scene 저장

            AssetDatabase.SaveAssets(); // 에셋 저장
            AssetDatabase.Refresh(); // Project 갱신

            Selection.activeGameObject =
                lobbyRoot; // 생성 UI 선택

            EditorGUIUtility.PingObject(
                lobbyRoot
            ); // Hierarchy 위치 강조

            Debug.Log(
                "[Project J/Day91] 실제 Fusion Player Lobby UI 구성을 완료했습니다."
            ); // 설치 완료 로그
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject =
                new GameObject(
                    "Day91PlayerLobbyCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster)
                ); // Canvas GameObject 생성

            Canvas canvas =
                canvasObject.GetComponent<
                    Canvas
                >(); // Canvas 조회

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay; // Overlay Canvas 설정

            canvas.sortingOrder =
                20; // Lobby UI 우선 표시

            CanvasScaler scaler =
                canvasObject.GetComponent<
                    CanvasScaler
                >(); // Canvas Scaler 조회

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode
                    .ScaleWithScreenSize; // 화면 크기 기준 스케일

            scaler.referenceResolution =
                new Vector2(
                    1920f,
                    1080f
                ); // 기준 해상도 설정

            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode
                    .MatchWidthOrHeight; // 가로세로 비율 대응

            scaler.matchWidthOrHeight =
                0.5f; // 가로세로 균형 스케일

            return canvas; // 생성 Canvas 반환
        }

        private static void EnsureEventSystem(
            UnityEngine.SceneManagement.Scene scene
        )
        {
            foreach (
                GameObject root
                in scene.GetRootGameObjects()
            )
            {
                EventSystem eventSystem =
                    root.GetComponentInChildren<
                        EventSystem
                    >(
                        true
                    ); // 기존 EventSystem 탐색

                if (eventSystem != null)
                {
                    return; // 기존 EventSystem 재사용
                }
            }

            GameObject eventSystemObject =
                new GameObject(
                    "EventSystem",
                    typeof(EventSystem),
                    typeof(InputSystemUIInputModule)
                ); // Input System EventSystem 생성

            eventSystemObject.transform
                .SetAsLastSibling(); // Hierarchy 뒤쪽 배치
        }

        private static void RemoveExistingCanvas(
            UnityEngine.SceneManagement.Scene scene
        )
        {
            foreach (
                GameObject root
                in scene.GetRootGameObjects()
            )
            {
                if (
                    root.name ==
                    "Day91PlayerLobbyCanvas"
                )
                {
                    Object.DestroyImmediate(
                        root
                    ); // 기존 Day91 Canvas 제거
                    return;
                }
            }
        }

        private static void BuildHeader(
            Transform parent,
            out Text readySummaryText
        )
        {
            Text title =
                CreateText(
                    parent,
                    "LobbyTitle",
                    "PLAYER LOBBY",
                    48,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold
                ); // Lobby 제목 생성

            SetAnchoredRect(
                title.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    54f,
                    -66f
                ),
                new Vector2(
                    720f,
                    68f
                ),
                new Vector2(
                    0f,
                    1f
                )
            ); // 제목 위치 설정

            title.color =
                Color.white; // 제목 색상 설정

            Text subtitle =
                CreateText(
                    parent,
                    "LobbySubtitle",
                    "Fusion 참가자와 Ready 상태를 실시간으로 표시합니다.",
                    19,
                    TextAnchor.MiddleLeft,
                    FontStyle.Normal
                ); // 설명 Text 생성

            SetAnchoredRect(
                subtitle.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    56f,
                    -114f
                ),
                new Vector2(
                    820f,
                    38f
                ),
                new Vector2(
                    0f,
                    1f
                )
            ); // 설명 위치 설정

            subtitle.color =
                new Color(
                    0.85f,
                    0.88f,
                    0.98f,
                    1f
                ); // 설명 색상 설정

            GameObject readyPanel =
                CreatePanel(
                    parent,
                    "ReadySummaryPanel",
                    new Vector2(
                        0f,
                        350f
                    ),
                    new Vector2(
                        320f,
                        84f
                    ),
                    new Color(
                        0.16f,
                        0.24f,
                        0.46f,
                        0.98f
                    )
                ); // Ready 요약 Panel 생성

            readySummaryText =
                CreateText(
                    readyPanel.transform,
                    "ReadySummaryText",
                    "CONNECTING...",
                    24,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                ); // Ready 요약 Text 생성

            StretchFull(
                readySummaryText.rectTransform
            ); // Ready Text 전체 확장

            readySummaryText.color =
                new Color(
                    0.40f,
                    1f,
                    0.58f,
                    1f
                ); // Ready Text 색상 설정
        }

        private static void BuildPlayerSlots(
            Transform parent,
            GameObject[] slotRoots,
            Text[] slotIndexTexts,
            Text[] slotNameTexts,
            Text[] slotStateTexts
        )
        {
            for (
                int index = 0;
                index <
                    SlotCount;
                index++
            )
            {
                int column =
                    index % 4; // 4열 계산

                int row =
                    index / 4; // 2행 계산

                float x =
                    -405f +
                    column * 270f; // Slot X 위치 계산

                float y =
                    145f -
                    row * 300f; // Slot Y 위치 계산

                GameObject slot =
                    CreatePanel(
                        parent,
                        "PlayerSlot_" +
                        index,
                        new Vector2(
                            x,
                            y
                        ),
                        new Vector2(
                            230f,
                            260f
                        ),
                        new Color(
                            0.21f,
                            0.18f,
                            0.40f,
                            0.96f
                        )
                    ); // Player Slot 생성

                slotRoots[index] =
                    slot; // Slot 루트 저장

                slotIndexTexts[index] =
                    CreateSlotIndex(
                        slot.transform,
                        index
                    ); // Slot 번호 생성

                CreateCharacterPlaceholder(
                    slot.transform
                ); // 캐릭터 Placeholder 생성

                slotNameTexts[index] =
                    CreateSlotName(
                        slot.transform
                    ); // Player 이름 생성

                slotStateTexts[index] =
                    CreateSlotState(
                        slot.transform
                    ); // 역할과 Ready 상태 생성
            }
        }

        private static Text CreateSlotIndex(
            Transform parent,
            int index
        )
        {
            Text text =
                CreateText(
                    parent,
                    "SlotIndex",
                    "#" +
                    (index + 1)
                        .ToString("00"),
                    14,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold
                ); // Slot 번호 Text 생성

            SetAnchoredRect(
                text.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    12f,
                    -16f
                ),
                new Vector2(
                    80f,
                    28f
                ),
                new Vector2(
                    0f,
                    1f
                )
            ); // Slot 번호 위치 설정

            text.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.55f
                ); // Slot 번호 색상 설정

            return text; // Slot 번호 반환
        }

        private static void CreateCharacterPlaceholder(
            Transform parent
        )
        {
            GameObject pedestal =
                CreatePanel(
                    parent,
                    "Pedestal",
                    new Vector2(
                        0f,
                        -40f
                    ),
                    new Vector2(
                        150f,
                        36f
                    ),
                    new Color(
                        0.30f,
                        0.25f,
                        0.50f,
                        1f
                    )
                ); // 캐릭터 받침대 생성

            pedestal.GetComponent<
                Image
            >().raycastTarget =
                false; // 받침대 Raycast 차단

            Text character =
                CreateText(
                    parent,
                    "CharacterPlaceholder",
                    "+",
                    72,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                ); // 캐릭터 Placeholder 생성

            SetAnchoredRect(
                character.rectTransform,
                new Vector2(
                    0.5f,
                    0.5f
                ),
                new Vector2(
                    0f,
                    40f
                ),
                new Vector2(
                    100f,
                    120f
                ),
                new Vector2(
                    0.5f,
                    0.5f
                )
            ); // Placeholder 위치 설정

            character.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.22f
                ); // Placeholder 색상 설정
        }

        private static Text CreateSlotName(
            Transform parent
        )
        {
            Text text =
                CreateText(
                    parent,
                    "PlayerName",
                    "WAITING...",
                    17,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                ); // Player 이름 Text 생성

            SetAnchoredRect(
                text.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    44f
                ),
                new Vector2(
                    210f,
                    34f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            ); // Player 이름 위치 설정

            text.color =
                Color.white; // Player 이름 색상 설정

            return text; // Player 이름 반환
        }

        private static Text CreateSlotState(
            Transform parent
        )
        {
            Text text =
                CreateText(
                    parent,
                    "PlayerState",
                    "EMPTY",
                    13,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                ); // Player 상태 Text 생성

            SetAnchoredRect(
                text.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    14f
                ),
                new Vector2(
                    210f,
                    28f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            ); // Player 상태 위치 설정

            text.color =
                new Color(
                    0.55f,
                    0.94f,
                    0.72f,
                    1f
                ); // Player 상태 색상 설정

            return text; // Player 상태 반환
        }

        private static void BuildNetworkMatchInfo(
            Transform parent,
            out Text roomCodeText,
            out Text playerCountText,
            out Text localRoleText,
            out Text readyCountText,
            out Text flowText
        )
        {
            Text title =
                CreateText(
                    parent,
                    "MatchInfoTitle",
                    "MATCH INFO",
                    26,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                ); // Match Info 제목 생성

            SetAnchoredRect(
                title.rectTransform,
                new Vector2(
                    0.5f,
                    1f
                ),
                new Vector2(
                    0f,
                    -38f
                ),
                new Vector2(
                    310f,
                    48f
                ),
                new Vector2(
                    0.5f,
                    1f
                )
            ); // Match Info 제목 위치 설정

            title.color =
                Color.white; // Match Info 제목 색상 설정

            roomCodeText =
                CreateInfoRow(
                    parent,
                    "RoomCodeValue",
                    "ROOM CODE",
                    "-",
                    170f
                ); // Room Code 행 생성

            playerCountText =
                CreateInfoRow(
                    parent,
                    "PlayerCountValue",
                    "PLAYERS",
                    "0 / 8",
                    100f
                ); // 참가 인원 행 생성

            localRoleText =
                CreateInfoRow(
                    parent,
                    "LocalRoleValue",
                    "YOU ARE",
                    "-",
                    30f
                ); // Host/Client 행 생성

            readyCountText =
                CreateInfoRow(
                    parent,
                    "ReadyCountValue",
                    "READY",
                    "0 / 0",
                    -40f
                ); // Ready 수 행 생성

            flowText =
                CreateInfoRow(
                    parent,
                    "FlowValue",
                    "FLOW",
                    "Connecting",
                    -110f
                ); // Flow 상태 행 생성

            Text note =
                CreateText(
                    parent,
                    "NetworkNote",
                    "2명 이상이 모두 READY가 되면\n기존 Lobby Flow가 Game을 로드합니다.",
                    14,
                    TextAnchor.MiddleCenter,
                    FontStyle.Normal
                ); // Network 흐름 설명 생성

            SetAnchoredRect(
                note.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    44f
                ),
                new Vector2(
                    310f,
                    70f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            ); // Network 설명 위치 설정

            note.color =
                new Color(
                    0.72f,
                    0.75f,
                    0.88f,
                    1f
                ); // Network 설명 색상 설정
        }

        private static Text CreateInfoRow(
            Transform parent,
            string valueName,
            string labelValue,
            string initialValue,
            float y
        )
        {
            Text label =
                CreateText(
                    parent,
                    valueName +
                    "Label",
                    labelValue,
                    15,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold
                ); // 정보 Label 생성

            SetAnchoredRect(
                label.rectTransform,
                new Vector2(
                    0.5f,
                    0.5f
                ),
                new Vector2(
                    -85f,
                    y
                ),
                new Vector2(
                    150f,
                    36f
                ),
                new Vector2(
                    0.5f,
                    0.5f
                )
            ); // 정보 Label 위치 설정

            label.color =
                new Color(
                    0.78f,
                    0.81f,
                    0.93f,
                    1f
                ); // 정보 Label 색상 설정

            Text value =
                CreateText(
                    parent,
                    valueName,
                    initialValue,
                    17,
                    TextAnchor.MiddleRight,
                    FontStyle.Bold
                ); // 정보 Value 생성

            SetAnchoredRect(
                value.rectTransform,
                new Vector2(
                    0.5f,
                    0.5f
                ),
                new Vector2(
                    85f,
                    y
                ),
                new Vector2(
                    150f,
                    36f
                ),
                new Vector2(
                    0.5f,
                    0.5f
                )
            ); // 정보 Value 위치 설정

            value.color =
                Color.white; // 정보 Value 색상 설정

            return value; // 정보 Value 반환
        }

        private static void BuildBottomButtons(
            Transform parent,
            out Button readyButton,
            out Button leaveButton
        )
        {
            Button customizeButton =
                CreateButton(
                    parent,
                    "CustomizeButton",
                    "CUSTOMIZE",
                    new Vector2(
                        0f,
                        0f
                    ),
                    new Vector2(
                        56f,
                        28f
                    ),
                    new Vector2(
                        260f,
                        64f
                    ),
                    new Vector2(
                        0f,
                        0f
                    ),
                    new Color(
                        0.19f,
                        0.55f,
                        0.86f,
                        1f
                    )
                ); // Customize Placeholder 생성

            customizeButton.interactable =
                false; // Customize 아직 비활성화

            readyButton =
                CreateButton(
                    parent,
                    "ReadyButton",
                    "READY",
                    new Vector2(
                        0.5f,
                        0f
                    ),
                    new Vector2(
                        0f,
                        28f
                    ),
                    new Vector2(
                        300f,
                        70f
                    ),
                    new Vector2(
                        0.5f,
                        0f
                    ),
                    new Color(
                        0.95f,
                        0.64f,
                        0.13f,
                        1f
                    )
                ); // 실제 Ready 버튼 생성

            leaveButton =
                CreateButton(
                    parent,
                    "LeaveButton",
                    "LEAVE",
                    new Vector2(
                        1f,
                        0f
                    ),
                    new Vector2(
                        -56f,
                        28f
                    ),
                    new Vector2(
                        230f,
                        64f
                    ),
                    new Vector2(
                        1f,
                        0f
                    ),
                    new Color(
                        0.85f,
                        0.22f,
                        0.52f,
                        1f
                    )
                ); // 실제 Leave 버튼 생성
        }

        private static GameObject CreatePanel(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 size,
            Color color
        )
        {
            GameObject panel =
                CreateUiObject(
                    name,
                    parent
                ); // UI Panel 생성

            RectTransform rect =
                panel.GetComponent<
                    RectTransform
                >(); // RectTransform 조회

            rect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                ); // 중앙 Anchor 최소 설정

            rect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                ); // 중앙 Anchor 최대 설정

            rect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                ); // 중앙 Pivot 설정

            rect.anchoredPosition =
                position; // Panel 위치 설정

            rect.sizeDelta =
                size; // Panel 크기 설정

            Image image =
                panel.AddComponent<
                    Image
                >(); // Panel Image 추가

            image.color =
                color; // Panel 색상 설정

            Outline outline =
                panel.AddComponent<
                    Outline
                >(); // Panel Outline 추가

            outline.effectColor =
                new Color(
                    0.54f,
                    0.42f,
                    0.92f,
                    0.38f
                ); // Outline 색상 설정

            outline.effectDistance =
                new Vector2(
                    2f,
                    -2f
                ); // Outline 거리 설정

            return panel; // Panel 반환
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            string labelValue,
            Vector2 anchor,
            Vector2 position,
            Vector2 size,
            Vector2 pivot,
            Color color
        )
        {
            GameObject buttonObject =
                CreateUiObject(
                    name,
                    parent
                ); // Button GameObject 생성

            RectTransform rect =
                buttonObject.GetComponent<
                    RectTransform
                >(); // Button RectTransform 조회

            rect.anchorMin =
                anchor; // Button Anchor 최소 설정

            rect.anchorMax =
                anchor; // Button Anchor 최대 설정

            rect.pivot =
                pivot; // Button Pivot 설정

            rect.anchoredPosition =
                position; // Button 위치 설정

            rect.sizeDelta =
                size; // Button 크기 설정

            Image image =
                buttonObject.AddComponent<
                    Image
                >(); // Button Image 추가

            image.color =
                color; // Button 색상 설정

            Button button =
                buttonObject.AddComponent<
                    Button
                >(); // Button 컴포넌트 추가

            button.targetGraphic =
                image; // Button Target Graphic 설정

            Text label =
                CreateText(
                    buttonObject.transform,
                    "Label",
                    labelValue,
                    18,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                ); // Button Label 생성

            StretchFull(
                label.rectTransform
            ); // Label 전체 확장

            label.color =
                Color.white; // Button Label 색상 설정

            return button; // Button 반환
        }

        private static Text CreateText(
            Transform parent,
            string name,
            string value,
            int fontSize,
            TextAnchor alignment,
            FontStyle style
        )
        {
            GameObject textObject =
                CreateUiObject(
                    name,
                    parent
                ); // Text GameObject 생성

            Text text =
                textObject.AddComponent<
                    Text
                >(); // Text 컴포넌트 추가

            text.text =
                value; // 초기 문자열 설정

            text.font =
                Resources.GetBuiltinResource<
                    Font
                >(
                    "LegacyRuntime.ttf"
                ); // Unity 기본 Font 사용

            text.fontSize =
                fontSize; // Font 크기 설정

            text.alignment =
                alignment; // Text 정렬 설정

            text.fontStyle =
                style; // Font Style 설정

            text.horizontalOverflow =
                HorizontalWrapMode.Wrap; // 긴 문자열 줄바꿈

            text.verticalOverflow =
                VerticalWrapMode.Truncate; // 세로 범위 초과 잘라내기

            text.raycastTarget =
                false; // Text Raycast 차단

            return text; // Text 반환
        }

        private static GameObject CreateUiObject(
            string name,
            Transform parent
        )
        {
            GameObject gameObject =
                new GameObject(
                    name,
                    typeof(RectTransform)
                ); // RectTransform 기반 UI 생성

            gameObject.transform.SetParent(
                parent,
                false
            ); // 부모 연결

            return gameObject; // UI GameObject 반환
        }

        private static void StretchFull(
            RectTransform rect
        )
        {
            rect.anchorMin =
                Vector2.zero; // 전체 Anchor 최소 설정

            rect.anchorMax =
                Vector2.one; // 전체 Anchor 최대 설정

            rect.offsetMin =
                Vector2.zero; // 왼쪽 아래 여백 제거

            rect.offsetMax =
                Vector2.zero; // 오른쪽 위 여백 제거
        }

        private static void SetAnchoredRect(
            RectTransform rect,
            Vector2 anchor,
            Vector2 position,
            Vector2 size,
            Vector2 pivot
        )
        {
            rect.anchorMin =
                anchor; // Anchor 최소 설정

            rect.anchorMax =
                anchor; // Anchor 최대 설정

            rect.pivot =
                pivot; // Pivot 설정

            rect.anchoredPosition =
                position; // UI 위치 설정

            rect.sizeDelta =
                size; // UI 크기 설정
        }
    }
}
