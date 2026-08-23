using ProjectJ.Networking.Fusion; // PlayerLobby UI Controller
using UnityEditor; // Editor 메뉴 사용
using UnityEditor.SceneManagement; // Scene 저장
using UnityEngine; // GameObject와 RectTransform 사용
using UnityEngine.UI; // UI 생성

namespace ProjectJ.Editor
{
    internal static class
        ProjectJDay90PlayerLobbyInstaller
    {
        private const string MenuPath =
            "Project J/Scene/90일차 Player Lobby UI 구성";

        private const string MainMenuScenePath =
            "Assets/ProjectJ/Scenes/MainMenu.unity";

        private const int SlotCount =
            8;

        [MenuItem(MenuPath)]
        private static void ConfigurePlayerLobby()
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
                    "[Project J/Day90] PlayPanel을 찾지 못했습니다."
                );

                return;
            }

            Transform playerLobbyRoot =
                FindDirectChild(
                    playPanel,
                    "PlayerLobbyPreviewRoot"
                );

            if (playerLobbyRoot == null)
            {
                Debug.LogError(
                    "[Project J/Day90] PlayerLobbyPreviewRoot가 없습니다. 먼저 89일차 UI 구성을 적용해주세요."
                );

                return;
            }

            ProjectJHostRoomCreatePanel hostController =
                playPanel.GetComponent<
                    ProjectJHostRoomCreatePanel
                >();

            if (hostController == null)
            {
                Debug.LogError(
                    "[Project J/Day90] ProjectJHostRoomCreatePanel이 없습니다."
                );

                return;
            }

            ClearChildren(
                playerLobbyRoot
            );

            ProjectJPlayerLobbyPanel
                playerLobbyController =
                    playerLobbyRoot.GetComponent<
                        ProjectJPlayerLobbyPanel
                    >();

            if (playerLobbyController == null)
            {
                playerLobbyController =
                    playerLobbyRoot.gameObject
                        .AddComponent<
                            ProjectJPlayerLobbyPanel
                        >();
            }

            BuildHeader(
                playerLobbyRoot,
                out Text readySummaryText
            );

            GameObject playerArea =
                CreatePanel(
                    playerLobbyRoot,
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
                        0.92f
                    )
                );

            GameObject[] slotRoots =
                new GameObject[SlotCount];

            Text[] slotIndexTexts =
                new Text[SlotCount];

            Text[] slotNameTexts =
                new Text[SlotCount];

            Text[] slotStateTexts =
                new Text[SlotCount];

            BuildPlayerSlots(
                playerArea.transform,
                slotRoots,
                slotIndexTexts,
                slotNameTexts,
                slotStateTexts
            );

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
                );

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
                );

            Text pageText =
                CreateText(
                    playerArea.transform,
                    "PageText",
                    "PAGE 1 / 1",
                    17,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

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
            );

            pageText.color =
                new Color(
                    0.86f,
                    0.89f,
                    1f,
                    1f
                );

            GameObject matchInfo =
                CreatePanel(
                    playerLobbyRoot,
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
                );

            BuildMatchInfo(
                matchInfo.transform,
                out Text roomNameText,
                out Text playerCountText,
                out Text roundText,
                out Text difficultyText,
                out Text passwordText
            );

            BuildBottomButtons(
                playerLobbyRoot,
                out Button backButton
            );

            playerLobbyController.Configure(
                previousPageButton,
                nextPageButton,
                pageText,
                readySummaryText,
                roomNameText,
                playerCountText,
                roundText,
                difficultyText,
                passwordText,
                slotRoots,
                slotIndexTexts,
                slotNameTexts,
                slotStateTexts
            );

            hostController.ConfigurePlayerLobbyPanel(
                playerLobbyController,
                backButton
            );

            EditorUtility.SetDirty(
                playerLobbyController
            );

            EditorUtility.SetDirty(
                hostController
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
                playerLobbyRoot.gameObject;

            EditorGUIUtility.PingObject(
                playerLobbyRoot.gameObject
            );

            Debug.Log(
                "[Project J/Day90] Player Lobby UI 구성을 완료했습니다."
            );
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
                );

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
            );

            title.color =
                Color.white;

            Text subtitle =
                CreateText(
                    parent,
                    "LobbySubtitle",
                    "친구들과 함께 플레이할 준비를 해주세요.",
                    19,
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
                    56f,
                    -114f
                ),
                new Vector2(
                    760f,
                    38f
                ),
                new Vector2(
                    0f,
                    1f
                )
            );

            subtitle.color =
                new Color(
                    0.85f,
                    0.88f,
                    0.98f,
                    1f
                );

            GameObject readyPanel =
                CreatePanel(
                    parent,
                    "ReadySummaryPanel",
                    new Vector2(
                        0f,
                        350f
                    ),
                    new Vector2(
                        300f,
                        84f
                    ),
                    new Color(
                        0.16f,
                        0.24f,
                        0.46f,
                        0.98f
                    )
                );

            readySummaryText =
                CreateText(
                    readyPanel.transform,
                    "ReadySummaryText",
                    "READY  1 / 8",
                    24,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            StretchFull(
                readySummaryText.rectTransform
            );

            readySummaryText.color =
                new Color(
                    0.40f,
                    1f,
                    0.58f,
                    1f
                );
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
                index < SlotCount;
                index++
            )
            {
                int column =
                    index % 4;

                int row =
                    index / 4;

                float x =
                    -405f +
                    column * 270f;

                float y =
                    145f -
                    row * 300f;

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
                        index == 0
                            ? new Color(
                                0.18f,
                                0.47f,
                                0.73f,
                                1f
                            )
                            : new Color(
                                0.21f,
                                0.18f,
                                0.40f,
                                0.96f
                            )
                    );

                slotRoots[index] =
                    slot;

                slotIndexTexts[index] =
                    BuildSlotIndex(
                        slot.transform,
                        index
                    );

                BuildCharacterStand(
                    slot.transform,
                    index == 0
                );

                slotNameTexts[index] =
                    BuildSlotName(
                        slot.transform,
                        index
                    );

                slotStateTexts[index] =
                    BuildSlotState(
                        slot.transform,
                        index
                    );
            }
        }

        private static Text BuildSlotIndex(
            Transform parent,
            int index
        )
        {
            Text text =
                CreateText(
                    parent,
                    "SlotIndex",
                    "#" +
                    (index + 1).ToString("00"),
                    14,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold
                );

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
            );

            text.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.55f
                );

            return text;
        }

        private static void BuildCharacterStand(
            Transform parent,
            bool host
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
                    host
                        ? new Color(
                            0.48f,
                            0.30f,
                            0.78f,
                            1f
                        )
                        : new Color(
                            0.28f,
                            0.24f,
                            0.46f,
                            1f
                        )
                );

            Image pedestalImage =
                pedestal.GetComponent<
                    Image
                >();

            pedestalImage.raycastTarget =
                false;

            if (!host)
            {
                Text empty =
                    CreateText(
                        parent,
                        "EmptyCharacter",
                        "+",
                        72,
                        TextAnchor.MiddleCenter,
                        FontStyle.Bold
                    );

                SetAnchoredRect(
                    empty.rectTransform,
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
                );

                empty.color =
                    new Color(
                        1f,
                        1f,
                        1f,
                        0.20f
                    );

                return;
            }

            GameObject body =
                CreatePanel(
                    parent,
                    "CharacterBody",
                    new Vector2(
                        0f,
                        35f
                    ),
                    new Vector2(
                        74f,
                        92f
                    ),
                    new Color(
                        0.28f,
                        0.82f,
                        1f,
                        1f
                    )
                );

            body.GetComponent<
                Image
            >().raycastTarget =
                false;

            GameObject head =
                CreatePanel(
                    parent,
                    "CharacterHead",
                    new Vector2(
                        0f,
                        100f
                    ),
                    new Vector2(
                        76f,
                        66f
                    ),
                    new Color(
                        0.37f,
                        0.90f,
                        1f,
                        1f
                    )
                );

            head.GetComponent<
                Image
            >().raycastTarget =
                false;
        }

        private static Text BuildSlotName(
            Transform parent,
            int index
        )
        {
            Text text =
                CreateText(
                    parent,
                    "PlayerName",
                    index == 0
                        ? "PLAYER 01"
                        : "WAITING...",
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
                    42f
                ),
                new Vector2(
                    200f,
                    34f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            text.color =
                Color.white;

            return text;
        }

        private static Text BuildSlotState(
            Transform parent,
            int index
        )
        {
            Text text =
                CreateText(
                    parent,
                    "PlayerState",
                    index == 0
                        ? "HOST"
                        : "EMPTY",
                    14,
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
                    14f
                ),
                new Vector2(
                    200f,
                    28f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            text.color =
                index == 0
                    ? new Color(
                        0.44f,
                        1f,
                        0.58f,
                        1f
                    )
                    : new Color(
                        0.72f,
                        0.74f,
                        0.86f,
                        1f
                    );

            return text;
        }

        private static void BuildMatchInfo(
            Transform parent,
            out Text roomNameText,
            out Text playerCountText,
            out Text roundText,
            out Text difficultyText,
            out Text passwordText
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
                );

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
            );

            title.color =
                Color.white;

            roomNameText =
                CreateInfoRow(
                    parent,
                    "RoomNameValue",
                    "ROOM",
                    "MY ROOM",
                    170f
                );

            playerCountText =
                CreateInfoRow(
                    parent,
                    "PlayerCountValue",
                    "PLAYERS",
                    "1 / 8",
                    100f
                );

            roundText =
                CreateInfoRow(
                    parent,
                    "RoundValue",
                    "ROUNDS",
                    "1 ROUND",
                    30f
                );

            difficultyText =
                CreateInfoRow(
                    parent,
                    "DifficultyValue",
                    "DIFFICULTY",
                    "NORMAL",
                    -40f
                );

            passwordText =
                CreateInfoRow(
                    parent,
                    "PasswordValue",
                    "PASSWORD",
                    "OFF",
                    -110f
                );

            Text note =
                CreateText(
                    parent,
                    "NetworkNote",
                    "실제 참가자/Ready 정보는\n91일차에 Fusion 데이터와 연결합니다.",
                    14,
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
            );

            note.color =
                new Color(
                    0.72f,
                    0.75f,
                    0.88f,
                    1f
                );
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
                    valueName + "Label",
                    labelValue,
                    15,
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
            );

            label.color =
                new Color(
                    0.78f,
                    0.81f,
                    0.93f,
                    1f
                );

            Text value =
                CreateText(
                    parent,
                    valueName,
                    initialValue,
                    17,
                    TextAnchor.MiddleRight,
                    FontStyle.Bold
                );

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
            );

            value.color =
                Color.white;

            return value;
        }

        private static void BuildBottomButtons(
            Transform parent,
            out Button backButton
        )
        {
            Button customize =
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
                );

            customize.interactable =
                false;

            Button ready =
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
                );

            ready.interactable =
                false;

            backButton =
                CreateButton(
                    parent,
                    "PlayerLobbyPreviewBackButton",
                    "BACK",
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
                );
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
                    0.54f,
                    0.42f,
                    0.92f,
                    0.38f
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

        private static void ClearChildren(
            Transform parent
        )
        {
            for (
                int index =
                    parent.childCount - 1;
                index >= 0;
                index--
            )
            {
                Object.DestroyImmediate(
                    parent.GetChild(index)
                        .gameObject
                );
            }
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
    }
}
