using ProjectJ.Networking.Fusion; // PRIVATE MATCH 네트워크 UI 연결
using UnityEditor; // MenuItem, EditorUtility 사용
using UnityEditor.SceneManagement; // Scene 열기와 저장
using UnityEngine; // GameObject, RectTransform 사용
using UnityEngine.UI; // Button, InputField, Text 사용

namespace ProjectJ.Editor
{
    internal static class
        ProjectJDay88PrivateMatchPanelInstaller
    {
        private const string MenuPath =
            "Project J/Scene/88일차 PRIVATE MATCH UI 구성";

        private const string MainMenuScenePath =
            "Assets/ProjectJ/Scenes/MainMenu.unity";

        [MenuItem(MenuPath)]
        private static void ConfigurePrivateMatchPanel()
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
                    "[Project J/Day88] PlayPanel을 찾지 못했습니다. 먼저 87일차 PLAY 카드 구성을 적용해주세요."
                );

                return;
            }

            ProjectJPlayModePanel playModePanel =
                playPanel.GetComponent<
                    ProjectJPlayModePanel
                >();

            if (playModePanel == null)
            {
                Debug.LogError(
                    "[Project J/Day88] ProjectJPlayModePanel이 없습니다. 먼저 87일차를 적용해주세요."
                );

                return;
            }

            Transform modeSelectRoot =
                EnsureModeSelectRoot(
                    playPanel
                );

            Transform existingPrivateMatch =
                FindDirectChild(
                    playPanel,
                    "PrivateMatchRoot"
                );

            if (existingPrivateMatch != null)
            {
                Object.DestroyImmediate(
                    existingPrivateMatch.gameObject
                );
            }

            ProjectJPrivateMatchPanel
                privateMatchController =
                    playPanel.GetComponent<
                        ProjectJPrivateMatchPanel
                    >();

            if (privateMatchController == null)
            {
                privateMatchController =
                    playPanel.gameObject
                        .AddComponent<
                            ProjectJPrivateMatchPanel
                        >();
            }

            GameObject privateMatchRoot =
                CreateUiObject(
                    "PrivateMatchRoot",
                    playPanel
                );

            StretchFull(
                privateMatchRoot.GetComponent<
                    RectTransform
                >()
            );

            BuildHeader(
                privateMatchRoot.transform,
                out Button backButton
            );

            Transform cardContainer =
                BuildCardContainer(
                    privateMatchRoot.transform
                );

            Button createButton =
                BuildCreateCard(
                    cardContainer
                );

            BuildJoinCard(
                cardContainer,
                out InputField roomCodeInput,
                out Button joinButton
            );

            Text statusText =
                BuildStatusText(
                    privateMatchRoot.transform
                );

            privateMatchController.Configure(
                playModePanel,
                modeSelectRoot.gameObject,
                privateMatchRoot,
                createButton,
                joinButton,
                backButton,
                roomCodeInput,
                statusText
            );

            privateMatchRoot.SetActive(false);
            modeSelectRoot.gameObject.SetActive(true);

            EditorUtility.SetDirty(
                privateMatchController
            );

            EditorUtility.SetDirty(
                playModePanel
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
                privateMatchRoot;

            EditorGUIUtility.PingObject(
                privateMatchRoot
            );

            Debug.Log(
                "[Project J/Day88] PRIVATE MATCH 정사각형 Create/Join 카드 구성을 완료했습니다."
            );
        }

        private static Transform EnsureModeSelectRoot(
            Transform playPanel
        )
        {
            Transform existing =
                FindDirectChild(
                    playPanel,
                    "ModeSelectRoot"
                );

            if (existing != null)
            {
                return existing;
            }

            GameObject root =
                CreateUiObject(
                    "ModeSelectRoot",
                    playPanel
                );

            StretchFull(
                root.GetComponent<
                    RectTransform
                >()
            );

            string[] names =
            {
                "PlayTitle",
                "PlaySubtitle",
                "ModeCardContainer",
                "ModeDetailPanel"
            };

            foreach (
                string childName in names
            )
            {
                Transform child =
                    FindDirectChild(
                        playPanel,
                        childName
                    );

                if (child != null)
                {
                    child.SetParent(
                        root.transform,
                        false
                    );
                }
            }

            return root.transform;
        }

        private static void BuildHeader(
            Transform parent,
            out Button backButton
        )
        {
            Text title =
                CreateText(
                    parent,
                    "PrivateMatchTitle",
                    "PRIVATE MATCH",
                    52,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                title.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(72f, -82f),
                new Vector2(700f, 72f),
                new Vector2(0f, 1f)
            );

            title.color = Color.white;

            Text subtitle =
                CreateText(
                    parent,
                    "PrivateMatchSubtitle",
                    "친구와 비공개 방을 만들거나 Room Code로 참가하세요.",
                    21,
                    TextAnchor.MiddleLeft,
                    FontStyle.Normal
                );

            SetAnchoredRect(
                subtitle.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(74f, -136f),
                new Vector2(900f, 44f),
                new Vector2(0f, 1f)
            );

            subtitle.color =
                new Color(
                    0.80f,
                    0.84f,
                    0.92f,
                    1f
                );

            backButton =
                CreateButton(
                    parent,
                    "BackButton",
                    "BACK",
                    new Vector2(1f, 1f),
                    new Vector2(-58f, -98f),
                    new Vector2(160f, 54f),
                    new Vector2(1f, 1f),
                    new Color(
                        0.16f,
                        0.18f,
                        0.28f,
                        0.96f
                    )
                );
        }

        private static Transform BuildCardContainer(
            Transform parent
        )
        {
            GameObject container =
                CreateUiObject(
                    "PrivateMatchCardContainer",
                    parent
                );

            RectTransform rect =
                container.GetComponent<
                    RectTransform
                >();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                new Vector2(0f, -6f);

            rect.sizeDelta =
                new Vector2(980f, 470f);

            return container.transform;
        }

        private static Button BuildCreateCard(
            Transform parent
        )
        {
            GameObject card =
                BuildSquareCard(
                    parent,
                    "CreateRoomCard",
                    -235f,
                    new Color(
                        0.25f,
                        0.34f,
                        0.58f,
                        0.96f
                    )
                );

            Text title =
                CreateText(
                    card.transform,
                    "Title",
                    "CREATE ROOM",
                    30,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                title.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0f, -58f),
                new Vector2(360f, 58f),
                new Vector2(0.5f, 1f)
            );

            title.color = Color.white;

            Text description =
                CreateText(
                    card.transform,
                    "Description",
                    "새로운 비공개 방을 생성합니다.\\n생성 후 Lobby로 이동합니다.",
                    19,
                    TextAnchor.MiddleCenter,
                    FontStyle.Normal
                );

            SetAnchoredRect(
                description.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 35f),
                new Vector2(350f, 130f),
                new Vector2(0.5f, 0.5f)
            );

            description.color =
                new Color(
                    0.88f,
                    0.91f,
                    0.98f,
                    1f
                );

            return
                CreateButton(
                    card.transform,
                    "CreateButton",
                    "CREATE",
                    new Vector2(0.5f, 0f),
                    new Vector2(0f, 46f),
                    new Vector2(260f, 68f),
                    new Vector2(0.5f, 0f),
                    new Color(
                        0.15f,
                        0.66f,
                        0.92f,
                        1f
                    )
                );
        }

        private static void BuildJoinCard(
            Transform parent,
            out InputField roomCodeInput,
            out Button joinButton
        )
        {
            GameObject card =
                BuildSquareCard(
                    parent,
                    "JoinRoomCard",
                    235f,
                    new Color(
                        0.43f,
                        0.29f,
                        0.57f,
                        0.96f
                    )
                );

            Text title =
                CreateText(
                    card.transform,
                    "Title",
                    "JOIN ROOM",
                    30,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                title.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0f, -58f),
                new Vector2(360f, 58f),
                new Vector2(0.5f, 1f)
            );

            title.color = Color.white;

            Text description =
                CreateText(
                    card.transform,
                    "Description",
                    "친구에게 받은 6자리\\nRoom Code를 입력하세요.",
                    19,
                    TextAnchor.MiddleCenter,
                    FontStyle.Normal
                );

            SetAnchoredRect(
                description.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 80f),
                new Vector2(350f, 96f),
                new Vector2(0.5f, 0.5f)
            );

            description.color =
                new Color(
                    0.91f,
                    0.88f,
                    0.98f,
                    1f
                );

            roomCodeInput =
                BuildRoomCodeInput(
                    card.transform
                );

            joinButton =
                CreateButton(
                    card.transform,
                    "JoinButton",
                    "JOIN",
                    new Vector2(0.5f, 0f),
                    new Vector2(0f, 46f),
                    new Vector2(260f, 68f),
                    new Vector2(0.5f, 0f),
                    new Color(
                        0.58f,
                        0.35f,
                        0.78f,
                        1f
                    )
                );
        }

        private static GameObject BuildSquareCard(
            Transform parent,
            string objectName,
            float xPosition,
            Color color
        )
        {
            GameObject card =
                CreateUiObject(
                    objectName,
                    parent
                );

            RectTransform rect =
                card.GetComponent<
                    RectTransform
                >();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                new Vector2(
                    xPosition,
                    0f
                );

            rect.sizeDelta =
                new Vector2(
                    420f,
                    420f
                );

            Image background =
                card.AddComponent<
                    Image
                >();

            background.color = color;

            Outline outline =
                card.AddComponent<
                    Outline
                >();

            outline.effectColor =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.18f
                );

            outline.effectDistance =
                new Vector2(
                    2f,
                    -2f
                );

            return card;
        }

        private static InputField BuildRoomCodeInput(
            Transform parent
        )
        {
            GameObject inputObject =
                CreateUiObject(
                    "RoomCodeInput",
                    parent
                );

            RectTransform inputRect =
                inputObject.GetComponent<
                    RectTransform
                >();

            inputRect.anchorMin =
                new Vector2(0.5f, 0.5f);

            inputRect.anchorMax =
                new Vector2(0.5f, 0.5f);

            inputRect.pivot =
                new Vector2(0.5f, 0.5f);

            inputRect.anchoredPosition =
                new Vector2(0f, -26f);

            inputRect.sizeDelta =
                new Vector2(300f, 64f);

            Image inputBackground =
                inputObject.AddComponent<
                    Image
                >();

            inputBackground.color =
                new Color(
                    0.07f,
                    0.08f,
                    0.14f,
                    0.94f
                );

            InputField inputField =
                inputObject.AddComponent<
                    InputField
                >();

            Text inputText =
                CreateText(
                    inputObject.transform,
                    "Text",
                    string.Empty,
                    25,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            RectTransform textRect =
                inputText.rectTransform;

            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;

            textRect.offsetMin =
                new Vector2(14f, 4f);

            textRect.offsetMax =
                new Vector2(-14f, -4f);

            inputText.color = Color.white;

            Text placeholder =
                CreateText(
                    inputObject.transform,
                    "Placeholder",
                    "R7K2QM",
                    23,
                    TextAnchor.MiddleCenter,
                    FontStyle.Normal
                );

            StretchFull(
                placeholder.rectTransform
            );

            placeholder.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.34f
                );

            inputField.textComponent = inputText;
            inputField.placeholder = placeholder;
            inputField.characterLimit = 6;

            inputField.contentType =
                InputField.ContentType.Alphanumeric;

            inputField.characterValidation =
                InputField.CharacterValidation.Alphanumeric;

            inputField.lineType =
                InputField.LineType.SingleLine;

            return inputField;
        }

        private static Text BuildStatusText(
            Transform parent
        )
        {
            Text status =
                CreateText(
                    parent,
                    "ConnectionStatusText",
                    "비공개 방을 만들거나 Room Code로 참가하세요.",
                    18,
                    TextAnchor.MiddleCenter,
                    FontStyle.Normal
                );

            SetAnchoredRect(
                status.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0f, 34f),
                new Vector2(1100f, 46f),
                new Vector2(0.5f, 0f)
            );

            status.color =
                new Color(
                    0.87f,
                    0.90f,
                    0.98f,
                    1f
                );

            return status;
        }

        private static Button CreateButton(
            Transform parent,
            string objectName,
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
                    objectName,
                    parent
                );

            RectTransform rect =
                buttonObject.GetComponent<
                    RectTransform
                >();

            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image background =
                buttonObject.AddComponent<
                    Image
                >();

            background.color = color;

            Button button =
                buttonObject.AddComponent<
                    Button
                >();

            button.targetGraphic = background;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;

            colors.highlightedColor =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.88f
                );

            colors.pressedColor =
                new Color(
                    0.78f,
                    0.88f,
                    1f,
                    1f
                );

            colors.disabledColor =
                new Color(
                    0.40f,
                    0.40f,
                    0.46f,
                    0.72f
                );

            button.colors = colors;

            Text label =
                CreateText(
                    buttonObject.transform,
                    "Label",
                    labelValue,
                    19,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            StretchFull(
                label.rectTransform
            );

            label.color = Color.white;

            return button;
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
            if (parent.name == objectName)
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

                if (child.name == objectName)
                {
                    return child;
                }
            }

            return null;
        }

        private static GameObject CreateUiObject(
            string objectName,
            Transform parent
        )
        {
            GameObject gameObject =
                new GameObject(
                    objectName,
                    typeof(RectTransform)
                );

            gameObject.transform.SetParent(
                parent,
                false
            );

            return gameObject;
        }

        private static Text CreateText(
            Transform parent,
            string objectName,
            string value,
            int fontSize,
            TextAnchor alignment,
            FontStyle fontStyle
        )
        {
            GameObject textObject =
                CreateUiObject(
                    objectName,
                    parent
                );

            Text text =
                textObject.AddComponent<
                    Text
                >();

            text.text = value;

            text.font =
                Resources.GetBuiltinResource<
                    Font
                >(
                    "LegacyRuntime.ttf"
                );

            text.fontSize = fontSize;
            text.alignment = alignment;
            text.fontStyle = fontStyle;

            text.horizontalOverflow =
                HorizontalWrapMode.Wrap;

            text.verticalOverflow =
                VerticalWrapMode.Truncate;

            text.raycastTarget = false;

            return text;
        }

        private static void StretchFull(
            RectTransform rect
        )
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetAnchoredRect(
            RectTransform rect,
            Vector2 anchor,
            Vector2 position,
            Vector2 size,
            Vector2 pivot
        )
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
