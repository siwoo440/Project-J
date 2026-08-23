using ProjectJ.Networking.Fusion; // 89일차 Host Room UI 연결
using UnityEditor; // MenuItem와 EditorUtility 사용
using UnityEditor.SceneManagement; // Scene 열기와 저장
using UnityEngine; // GameObject, RectTransform 사용
using UnityEngine.UI; // UI 생성

namespace ProjectJ.Editor
{
    internal static class
        ProjectJDay89HostRoomCreateInstaller
    {
        private const string MenuPath =
            "Project J/Scene/89일차 Host Room Create UI 구성";

        private const string MainMenuScenePath =
            "Assets/ProjectJ/Scenes/MainMenu.unity";

        [MenuItem(MenuPath)]
        private static void ConfigureHostRoomCreateUi()
        {
            if (
                !EditorSceneManager
                    .SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                return;
            }

            UnityEngine.SceneManagement.Scene scene =
                EditorSceneManager.OpenScene(
                    MainMenuScenePath,
                    OpenSceneMode.Single
                );

            Transform playPanel =
                FindTransformInScene(
                    scene,
                    "PlayPanel"
                );

            if (playPanel == null)
            {
                Debug.LogError(
                    "[Project J/Day89] PlayPanel을 찾지 못했습니다."
                );

                return;
            }

            Transform privateMatchRoot =
                FindDirectChild(
                    playPanel,
                    "PrivateMatchRoot"
                );

            if (privateMatchRoot == null)
            {
                Debug.LogError(
                    "[Project J/Day89] PrivateMatchRoot가 없습니다. 먼저 88일차 UI 구성을 적용해주세요."
                );

                return;
            }

            ProjectJPrivateMatchPanel privateMatchPanel =
                playPanel.GetComponent<
                    ProjectJPrivateMatchPanel
                >();

            if (privateMatchPanel == null)
            {
                Debug.LogError(
                    "[Project J/Day89] ProjectJPrivateMatchPanel이 없습니다."
                );

                return;
            }

            DeleteDirectChild(
                playPanel,
                "HostRoomCreateRoot"
            );

            DeleteDirectChild(
                playPanel,
                "PlayerLobbyPreviewRoot"
            );

            ProjectJHostRoomCreatePanel controller =
                playPanel.GetComponent<
                    ProjectJHostRoomCreatePanel
                >();

            if (controller == null)
            {
                controller =
                    playPanel.gameObject
                        .AddComponent<
                            ProjectJHostRoomCreatePanel
                        >();
            }

            GameObject hostRoot =
                CreateUiObject(
                    "HostRoomCreateRoot",
                    playPanel
                );

            StretchFull(
                hostRoot.GetComponent<
                    RectTransform
                >()
            );

            BuildHeader(
                hostRoot.transform,
                out Button backButton
            );

            GameObject modePanel =
                CreatePanel(
                    hostRoot.transform,
                    "GameModePanel",
                    new Vector2(
                        -610f,
                        -10f
                    ),
                    new Vector2(
                        420f,
                        650f
                    ),
                    new Color(
                        0.19f,
                        0.16f,
                        0.38f,
                        0.96f
                    )
                );

            BuildGameModePanel(
                modePanel.transform
            );

            GameObject settingsPanel =
                CreatePanel(
                    hostRoot.transform,
                    "RoomSettingsPanel",
                    new Vector2(
                        -100f,
                        -10f
                    ),
                    new Vector2(
                        520f,
                        650f
                    ),
                    new Color(
                        0.20f,
                        0.17f,
                        0.42f,
                        0.96f
                    )
                );

            BuildSettingsPanel(
                settingsPanel.transform,
                out InputField roomNameInput,
                out Text maxPlayersText,
                out Text passwordText,
                out Text roundText,
                out Text difficultyText,
                out Button maxPlayersPrevious,
                out Button maxPlayersNext,
                out Button passwordToggle,
                out Button roundPrevious,
                out Button roundNext,
                out Button difficultyPrevious,
                out Button difficultyNext
            );

            GameObject previewPanel =
                CreatePanel(
                    hostRoot.transform,
                    "RoomPreviewPanel",
                    new Vector2(
                        515f,
                        -10f
                    ),
                    new Vector2(
                        600f,
                        650f
                    ),
                    new Color(
                        0.18f,
                        0.15f,
                        0.36f,
                        0.96f
                    )
                );

            BuildRoomPreview(
                previewPanel.transform,
                out Button characterPreviewButton,
                out Text previewRoomName,
                out Text previewPlayerCount,
                out Text previewRound,
                out Text previewDifficulty,
                out Text previewPassword
            );

            Text statusText =
                CreateText(
                    hostRoot.transform,
                    "HostRoomStatusText",
                    "방 설정을 확인한 뒤 CREATE ROOM을 눌러주세요.",
                    18,
                    TextAnchor.MiddleCenter,
                    FontStyle.Normal
                );

            SetAnchoredRect(
                statusText.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    90f
                ),
                new Vector2(
                    980f,
                    40f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            statusText.color =
                new Color(
                    0.88f,
                    0.91f,
                    1f,
                    1f
                );

            Button createRoomButton =
                CreateButton(
                    hostRoot.transform,
                    "ConfirmCreateRoomButton",
                    "CREATE ROOM",
                    new Vector2(
                        0.5f,
                        0f
                    ),
                    new Vector2(
                        0f,
                        20f
                    ),
                    new Vector2(
                        380f,
                        72f
                    ),
                    new Vector2(
                        0.5f,
                        0f
                    ),
                    new Color(
                        0.95f,
                        0.62f,
                        0.12f,
                        1f
                    )
                );

            GameObject playerLobbyPreviewRoot =
                BuildPlayerLobbyPreviewRoot(
                    playPanel,
                    out Button playerLobbyBackButton
                );

            controller.Configure(
                privateMatchRoot.gameObject,
                hostRoot,
                playerLobbyPreviewRoot,
                roomNameInput,
                maxPlayersText,
                passwordText,
                roundText,
                difficultyText,
                previewRoomName,
                previewPlayerCount,
                previewRound,
                previewDifficulty,
                previewPassword,
                statusText,
                maxPlayersPrevious,
                maxPlayersNext,
                passwordToggle,
                roundPrevious,
                roundNext,
                difficultyPrevious,
                difficultyNext,
                characterPreviewButton,
                backButton,
                createRoomButton,
                playerLobbyBackButton
            );

            privateMatchPanel
                .ConfigureHostRoomCreatePanel(
                    controller
                );

            hostRoot.SetActive(false);
            playerLobbyPreviewRoot.SetActive(false);

            EditorUtility.SetDirty(
                controller
            );

            EditorUtility.SetDirty(
                privateMatchPanel
            );

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject =
                hostRoot;

            EditorGUIUtility.PingObject(
                hostRoot
            );

            Debug.Log(
                "[Project J/Day89] Host Room Create UI 구성을 완료했습니다."
            );
        }

        private static void BuildHeader(
            Transform parent,
            out Button backButton
        )
        {
            Text title =
                CreateText(
                    parent,
                    "HostRoomTitle",
                    "CREATE PRIVATE ROOM",
                    48,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                title.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    56f,
                    -70f
                ),
                new Vector2(
                    850f,
                    70f
                ),
                new Vector2(
                    0f,
                    1f
                )
            );

            title.color =
                Color.white;

            Text subtitle =
                CreateText(
                    parent,
                    "HostRoomSubtitle",
                    "게임 설정을 선택하고 친구들과 플레이할 비공개 방을 만들어보세요.",
                    20,
                    TextAnchor.MiddleLeft,
                    FontStyle.Normal
                );

            SetAnchoredRect(
                subtitle.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    58f,
                    -122f
                ),
                new Vector2(
                    980f,
                    40f
                ),
                new Vector2(
                    0f,
                    1f
                )
            );

            subtitle.color =
                new Color(
                    0.87f,
                    0.89f,
                    0.98f,
                    1f
                );

            backButton =
                CreateButton(
                    parent,
                    "HostRoomBackButton",
                    "BACK",
                    new Vector2(
                        1f,
                        1f
                    ),
                    new Vector2(
                        -52f,
                        -78f
                    ),
                    new Vector2(
                        160f,
                        54f
                    ),
                    new Vector2(
                        1f,
                        1f
                    ),
                    new Color(
                        0.20f,
                        0.18f,
                        0.36f,
                        1f
                    )
                );
        }

        private static void BuildGameModePanel(
            Transform parent
        )
        {
            CreatePanelTitle(
                parent,
                "GAME MODE"
            );

            GameObject visual =
                CreatePanel(
                    parent,
                    "ModeVisual",
                    new Vector2(
                        0f,
                        70f
                    ),
                    new Vector2(
                        340f,
                        340f
                    ),
                    new Color(
                        0.19f,
                        0.52f,
                        0.78f,
                        1f
                    )
                );

            Text icon =
                CreateText(
                    visual.transform,
                    "ModeIcon",
                    "?",
                    96,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            StretchFull(
                icon.rectTransform
            );

            icon.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.72f
                );

            Text mode =
                CreateText(
                    parent,
                    "ModeName",
                    "PRIVATE MATCH",
                    28,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                mode.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    118f
                ),
                new Vector2(
                    350f,
                    54f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            mode.color =
                Color.white;

            Text desc =
                CreateText(
                    parent,
                    "ModeDescription",
                    "친구와 Room Code로 참가하는 비공개 경기",
                    17,
                    TextAnchor.MiddleCenter,
                    FontStyle.Normal
                );

            SetAnchoredRect(
                desc.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    70f
                ),
                new Vector2(
                    350f,
                    44f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            desc.color =
                new Color(
                    0.86f,
                    0.89f,
                    0.98f,
                    1f
                );
        }

        private static void BuildSettingsPanel(
            Transform parent,
            out InputField roomNameInput,
            out Text maxPlayersText,
            out Text passwordText,
            out Text roundText,
            out Text difficultyText,
            out Button maxPlayersPrevious,
            out Button maxPlayersNext,
            out Button passwordToggle,
            out Button roundPrevious,
            out Button roundNext,
            out Button difficultyPrevious,
            out Button difficultyNext
        )
        {
            CreatePanelTitle(
                parent,
                "ROOM SETTINGS"
            );

            roomNameInput =
                CreateInputField(
                    parent,
                    "RoomNameInput",
                    "MY ROOM",
                    new Vector2(
                        120f,
                        190f
                    ),
                    new Vector2(
                        300f,
                        54f
                    )
                );

            CreateSettingLabel(
                parent,
                "RoomNameLabel",
                "ROOM NAME",
                -195f,
                190f
            );

            CreateSettingLabel(
                parent,
                "MaxPlayersLabel",
                "MAX PLAYERS",
                -195f,
                105f
            );

            maxPlayersPrevious =
                CreateArrowButton(
                    parent,
                    "MaxPlayersPrevious",
                    "<",
                    45f,
                    105f
                );

            maxPlayersText =
                CreateSettingValue(
                    parent,
                    "MaxPlayersValue",
                    "8",
                    120f,
                    105f
                );

            maxPlayersNext =
                CreateArrowButton(
                    parent,
                    "MaxPlayersNext",
                    ">",
                    195f,
                    105f
                );

            CreateSettingLabel(
                parent,
                "PasswordLabel",
                "PASSWORD",
                -195f,
                20f
            );

            passwordToggle =
                CreateButton(
                    parent,
                    "PasswordToggleButton",
                    "TOGGLE",
                    new Vector2(
                        0.5f,
                        0.5f
                    ),
                    new Vector2(
                        200f,
                        20f
                    ),
                    new Vector2(
                        110f,
                        48f
                    ),
                    new Vector2(
                        0.5f,
                        0.5f
                    ),
                    new Color(
                        0.49f,
                        0.25f,
                        0.78f,
                        1f
                    )
                );

            passwordText =
                CreateSettingValue(
                    parent,
                    "PasswordValue",
                    "OFF",
                    70f,
                    20f
                );

            CreateSettingLabel(
                parent,
                "RoundLabel",
                "ROUNDS",
                -195f,
                -65f
            );

            roundPrevious =
                CreateArrowButton(
                    parent,
                    "RoundPrevious",
                    "<",
                    45f,
                    -65f
                );

            roundText =
                CreateSettingValue(
                    parent,
                    "RoundValue",
                    "1 ROUND",
                    120f,
                    -65f
                );

            roundNext =
                CreateArrowButton(
                    parent,
                    "RoundNext",
                    ">",
                    195f,
                    -65f
                );

            CreateSettingLabel(
                parent,
                "DifficultyLabel",
                "DIFFICULTY",
                -195f,
                -150f
            );

            difficultyPrevious =
                CreateArrowButton(
                    parent,
                    "DifficultyPrevious",
                    "<",
                    45f,
                    -150f
                );

            difficultyText =
                CreateSettingValue(
                    parent,
                    "DifficultyValue",
                    "NORMAL",
                    120f,
                    -150f
                );

            difficultyNext =
                CreateArrowButton(
                    parent,
                    "DifficultyNext",
                    ">",
                    195f,
                    -150f
                );

            Text note =
                CreateText(
                    parent,
                    "SettingsNote",
                    "89일차에서는 설정값이 Room Preview에 반영됩니다.\n실제 Fusion Session 설정 연결은 이후 일차에서 진행합니다.",
                    15,
                    TextAnchor.MiddleCenter,
                    FontStyle.Normal
                );

            SetAnchoredRect(
                note.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    58f
                ),
                new Vector2(
                    440f,
                    70f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            note.color =
                new Color(
                    0.75f,
                    0.78f,
                    0.90f,
                    1f
                );
        }

        private static void BuildRoomPreview(
            Transform parent,
            out Button characterPreviewButton,
            out Text previewRoomName,
            out Text previewPlayerCount,
            out Text previewRound,
            out Text previewDifficulty,
            out Text previewPassword
        )
        {
            CreatePanelTitle(
                parent,
                "ROOM PREVIEW"
            );

            characterPreviewButton =
                CreateButton(
                    parent,
                    "CharacterPreviewButton",
                    string.Empty,
                    new Vector2(
                        0.5f,
                        0.5f
                    ),
                    new Vector2(
                        0f,
                        80f
                    ),
                    new Vector2(
                        500f,
                        300f
                    ),
                    new Vector2(
                        0.5f,
                        0.5f
                    ),
                    new Color(
                        0.14f,
                        0.27f,
                        0.55f,
                        1f
                    )
                );

            BuildSimpleCharacter(
                characterPreviewButton.transform
            );

            Text clickHint =
                CreateText(
                    characterPreviewButton.transform,
                    "ClickHint",
                    "CLICK TO VIEW PLAYER LOBBY",
                    15,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                clickHint.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    20f
                ),
                new Vector2(
                    420f,
                    34f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            clickHint.color =
                new Color(
                    0.85f,
                    0.92f,
                    1f,
                    1f
                );

            previewRoomName =
                CreateText(
                    parent,
                    "PreviewRoomName",
                    "MY ROOM",
                    28,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                previewRoomName.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    165f
                ),
                new Vector2(
                    500f,
                    44f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            previewRoomName.color =
                Color.white;

            previewPlayerCount =
                CreatePreviewInfo(
                    parent,
                    "PreviewPlayerCount",
                    "1 / 8 PLAYERS",
                    118f
                );

            previewRound =
                CreatePreviewInfo(
                    parent,
                    "PreviewRound",
                    "1 ROUND",
                    82f
                );

            previewDifficulty =
                CreatePreviewInfo(
                    parent,
                    "PreviewDifficulty",
                    "NORMAL",
                    46f
                );

            previewPassword =
                CreatePreviewInfo(
                    parent,
                    "PreviewPassword",
                    "PASSWORD : OFF",
                    10f
                );
        }

        private static GameObject BuildPlayerLobbyPreviewRoot(
            Transform playPanel,
            out Button backButton
        )
        {
            GameObject root =
                CreateUiObject(
                    "PlayerLobbyPreviewRoot",
                    playPanel
                );

            StretchFull(
                root.GetComponent<
                    RectTransform
                >()
            );

            Text title =
                CreateText(
                    root.transform,
                    "PlayerLobbyPreviewTitle",
                    "PLAYER LOBBY",
                    48,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                title.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    56f,
                    -72f
                ),
                new Vector2(
                    700f,
                    70f
                ),
                new Vector2(
                    0f,
                    1f
                )
            );

            title.color =
                Color.white;

            Text subtitle =
                CreateText(
                    root.transform,
                    "PlayerLobbyPreviewSubtitle",
                    "90일차 PlayerLobbyPanel 연결 위치 미리보기",
                    20,
                    TextAnchor.MiddleLeft,
                    FontStyle.Normal
                );

            SetAnchoredRect(
                subtitle.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    58f,
                    -122f
                ),
                new Vector2(
                    850f,
                    40f
                ),
                new Vector2(
                    0f,
                    1f
                )
            );

            subtitle.color =
                new Color(
                    0.84f,
                    0.88f,
                    0.98f,
                    1f
                );

            GameObject slotsRoot =
                CreateUiObject(
                    "LobbySlotsPreview",
                    root.transform
                );

            RectTransform slotsRect =
                slotsRoot.GetComponent<
                    RectTransform
                >();

            slotsRect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            slotsRect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            slotsRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            slotsRect.anchoredPosition =
                new Vector2(
                    -80f,
                    -20f
                );

            slotsRect.sizeDelta =
                new Vector2(
                    1420f,
                    630f
                );

            for (
                int index = 0;
                index < 8;
                index++
            )
            {
                int column =
                    index % 4;

                int row =
                    index / 4;

                float x =
                    -510f +
                    column * 340f;

                float y =
                    150f -
                    row * 300f;

                BuildLobbyPlaceholderSlot(
                    slotsRoot.transform,
                    index,
                    x,
                    y
                );
            }

            backButton =
                CreateButton(
                    root.transform,
                    "PlayerLobbyPreviewBackButton",
                    "BACK TO ROOM SETTINGS",
                    new Vector2(
                        0.5f,
                        0f
                    ),
                    new Vector2(
                        0f,
                        28f
                    ),
                    new Vector2(
                        340f,
                        62f
                    ),
                    new Vector2(
                        0.5f,
                        0f
                    ),
                    new Color(
                        0.19f,
                        0.50f,
                        0.82f,
                        1f
                    )
                );

            return root;
        }

        private static void BuildLobbyPlaceholderSlot(
            Transform parent,
            int index,
            float x,
            float y
        )
        {
            GameObject slot =
                CreatePanel(
                    parent,
                    "PlayerSlotPreview_" +
                    index,
                    new Vector2(
                        x,
                        y
                    ),
                    new Vector2(
                        280f,
                        250f
                    ),
                    index == 0
                        ? new Color(
                            0.19f,
                            0.50f,
                            0.76f,
                            1f
                        )
                        : new Color(
                            0.18f,
                            0.16f,
                            0.34f,
                            0.92f
                        )
                );

            Text player =
                CreateText(
                    slot.transform,
                    "PlayerName",
                    index == 0
                        ? "PLAYER 01"
                        : "EMPTY",
                    20,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                player.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    42f
                ),
                new Vector2(
                    230f,
                    40f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            player.color =
                Color.white;

            Text state =
                CreateText(
                    slot.transform,
                    "State",
                    index == 0
                        ? "HOST"
                        : "WAITING",
                    16,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                state.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    10f
                ),
                new Vector2(
                    230f,
                    32f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            state.color =
                index == 0
                    ? new Color(
                        0.45f,
                        1f,
                        0.58f,
                        1f
                    )
                    : new Color(
                        0.72f,
                        0.74f,
                        0.84f,
                        1f
                    );

            if (index == 0)
            {
                BuildSimpleCharacter(
                    slot.transform
                );
            }
            else
            {
                Text plus =
                    CreateText(
                        slot.transform,
                        "EmptyIcon",
                        "+",
                        64,
                        TextAnchor.MiddleCenter,
                        FontStyle.Bold
                    );

                SetAnchoredRect(
                    plus.rectTransform,
                    new Vector2(
                        0.5f,
                        0.5f
                    ),
                    new Vector2(
                        0f,
                        45f
                    ),
                    new Vector2(
                        100f,
                        100f
                    ),
                    new Vector2(
                        0.5f,
                        0.5f
                    )
                );

                plus.color =
                    new Color(
                        1f,
                        1f,
                        1f,
                        0.30f
                    );
            }
        }

        private static void BuildSimpleCharacter(
            Transform parent
        )
        {
            GameObject body =
                CreateUiObject(
                    "CharacterBody",
                    parent
                );

            RectTransform bodyRect =
                body.GetComponent<
                    RectTransform
                >();

            bodyRect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            bodyRect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            bodyRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            bodyRect.anchoredPosition =
                new Vector2(
                    0f,
                    18f
                );

            bodyRect.sizeDelta =
                new Vector2(
                    100f,
                    130f
                );

            Image bodyImage =
                body.AddComponent<
                    Image
                >();

            bodyImage.color =
                new Color(
                    0.33f,
                    0.85f,
                    1f,
                    1f
                );

            GameObject head =
                CreateUiObject(
                    "CharacterHead",
                    parent
                );

            RectTransform headRect =
                head.GetComponent<
                    RectTransform
                >();

            headRect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            headRect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            headRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            headRect.anchoredPosition =
                new Vector2(
                    0f,
                    103f
                );

            headRect.sizeDelta =
                new Vector2(
                    92f,
                    82f
                );

            Image headImage =
                head.AddComponent<
                    Image
                >();

            headImage.color =
                new Color(
                    0.40f,
                    0.91f,
                    1f,
                    1f
                );
        }

        private static void CreatePanelTitle(
            Transform parent,
            string value
        )
        {
            Text title =
                CreateText(
                    parent,
                    "PanelTitle",
                    value,
                    26,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                title.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    24f,
                    -40f
                ),
                new Vector2(
                    360f,
                    50f
                ),
                new Vector2(
                    0f,
                    1f
                )
            );

            title.color =
                Color.white;
        }

        private static void CreateSettingLabel(
            Transform parent,
            string name,
            string value,
            float x,
            float y
        )
        {
            Text label =
                CreateText(
                    parent,
                    name,
                    value,
                    18,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                label.rectTransform,
                new Vector2(
                    0.5f,
                    0.5f
                ),
                new Vector2(
                    x,
                    y
                ),
                new Vector2(
                    180f,
                    44f
                ),
                new Vector2(
                    0.5f,
                    0.5f
                )
            );

            label.color =
                new Color(
                    0.90f,
                    0.92f,
                    1f,
                    1f
                );
        }

        private static Text CreateSettingValue(
            Transform parent,
            string name,
            string value,
            float x,
            float y
        )
        {
            Text text =
                CreateText(
                    parent,
                    name,
                    value,
                    20,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                text.rectTransform,
                new Vector2(
                    0.5f,
                    0.5f
                ),
                new Vector2(
                    x,
                    y
                ),
                new Vector2(
                    130f,
                    46f
                ),
                new Vector2(
                    0.5f,
                    0.5f
                )
            );

            text.color =
                Color.white;

            return text;
        }

        private static Text CreatePreviewInfo(
            Transform parent,
            string name,
            string value,
            float y
        )
        {
            Text text =
                CreateText(
                    parent,
                    name,
                    value,
                    18,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                text.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    y
                ),
                new Vector2(
                    480f,
                    32f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            text.color =
                new Color(
                    0.88f,
                    0.91f,
                    1f,
                    1f
                );

            return text;
        }

        private static Button CreateArrowButton(
            Transform parent,
            string name,
            string label,
            float x,
            float y
        )
        {
            return
                CreateButton(
                    parent,
                    name,
                    label,
                    new Vector2(
                        0.5f,
                        0.5f
                    ),
                    new Vector2(
                        x,
                        y
                    ),
                    new Vector2(
                        52f,
                        48f
                    ),
                    new Vector2(
                        0.5f,
                        0.5f
                    ),
                    new Color(
                        0.48f,
                        0.28f,
                        0.78f,
                        1f
                    )
                );
        }

        private static InputField CreateInputField(
            Transform parent,
            string name,
            string defaultValue,
            Vector2 position,
            Vector2 size
        )
        {
            GameObject inputObject =
                CreateUiObject(
                    name,
                    parent
                );

            RectTransform rect =
                inputObject.GetComponent<
                    RectTransform
                >();

            rect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.anchoredPosition =
                position;

            rect.sizeDelta =
                size;

            Image background =
                inputObject.AddComponent<
                    Image
                >();

            background.color =
                new Color(
                    0.09f,
                    0.08f,
                    0.20f,
                    0.96f
                );

            InputField input =
                inputObject.AddComponent<
                    InputField
                >();

            Text text =
                CreateText(
                    inputObject.transform,
                    "Text",
                    defaultValue,
                    19,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            StretchFull(
                text.rectTransform
            );

            text.color =
                Color.white;

            input.textComponent =
                text;

            input.text =
                defaultValue;

            input.characterLimit =
                24;

            input.lineType =
                InputField.LineType.SingleLine;

            return input;
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
                );

            RectTransform rect =
                panel.GetComponent<
                    RectTransform
                >();

            rect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.anchoredPosition =
                position;

            rect.sizeDelta =
                size;

            Image image =
                panel.AddComponent<
                    Image
                >();

            image.color =
                color;

            Outline outline =
                panel.AddComponent<
                    Outline
                >();

            outline.effectColor =
                new Color(
                    0.55f,
                    0.42f,
                    0.92f,
                    0.45f
                );

            outline.effectDistance =
                new Vector2(
                    2f,
                    -2f
                );

            return panel;
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
                );

            RectTransform rect =
                buttonObject.GetComponent<
                    RectTransform
                >();

            rect.anchorMin =
                anchor;

            rect.anchorMax =
                anchor;

            rect.pivot =
                pivot;

            rect.anchoredPosition =
                position;

            rect.sizeDelta =
                size;

            Image image =
                buttonObject.AddComponent<
                    Image
                >();

            image.color =
                color;

            Button button =
                buttonObject.AddComponent<
                    Button
                >();

            button.targetGraphic =
                image;

            Text label =
                CreateText(
                    buttonObject.transform,
                    "Label",
                    labelValue,
                    18,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            StretchFull(
                label.rectTransform
            );

            label.color =
                Color.white;

            return button;
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
                );

            Text text =
                textObject.AddComponent<
                    Text
                >();

            text.text =
                value;

            text.font =
                Resources.GetBuiltinResource<
                    Font
                >(
                    "LegacyRuntime.ttf"
                );

            text.fontSize =
                fontSize;

            text.alignment =
                alignment;

            text.fontStyle =
                style;

            text.horizontalOverflow =
                HorizontalWrapMode.Wrap;

            text.verticalOverflow =
                VerticalWrapMode.Truncate;

            text.raycastTarget =
                false;

            return text;
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
                );

            gameObject.transform.SetParent(
                parent,
                false
            );

            return gameObject;
        }

        private static void StretchFull(
            RectTransform rect
        )
        {
            rect.anchorMin =
                Vector2.zero;

            rect.anchorMax =
                Vector2.one;

            rect.offsetMin =
                Vector2.zero;

            rect.offsetMax =
                Vector2.zero;
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
                anchor;

            rect.anchorMax =
                anchor;

            rect.pivot =
                pivot;

            rect.anchoredPosition =
                position;

            rect.sizeDelta =
                size;
        }

        private static Transform FindTransformInScene(
            UnityEngine.SceneManagement.Scene scene,
            string objectName
        )
        {
            foreach (
                GameObject root
                in scene.GetRootGameObjects()
            )
            {
                Transform found =
                    FindRecursive(
                        root.transform,
                        objectName
                    );

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindRecursive(
            Transform parent,
            string objectName
        )
        {
            if (
                parent.name ==
                objectName
            )
            {
                return parent;
            }

            for (
                int index = 0;
                index < parent.childCount;
                index++
            )
            {
                Transform found =
                    FindRecursive(
                        parent.GetChild(index),
                        objectName
                    );

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindDirectChild(
            Transform parent,
            string objectName
        )
        {
            for (
                int index = 0;
                index < parent.childCount;
                index++
            )
            {
                Transform child =
                    parent.GetChild(index);

                if (
                    child.name ==
                    objectName
                )
                {
                    return child;
                }
            }

            return null;
        }

        private static void DeleteDirectChild(
            Transform parent,
            string objectName
        )
        {
            Transform child =
                FindDirectChild(
                    parent,
                    objectName
                );

            if (child != null)
            {
                Object.DestroyImmediate(
                    child.gameObject
                );
            }
        }
    }
}
