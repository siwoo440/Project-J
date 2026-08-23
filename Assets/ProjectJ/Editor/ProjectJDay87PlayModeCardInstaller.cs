using System.Collections.Generic; // 카드 목록 구성
using UnityEditor; // MenuItem과 EditorUtility 사용
using UnityEditor.SceneManagement; // MainMenu Scene 저장
using UnityEngine; // GameObject, RectTransform 사용
using UnityEngine.UI; // 카드 UI 구성

namespace ProjectJ.Editor
{
    internal static class
        ProjectJDay87PlayModeCardInstaller
    {
        private const string MenuPath =
            "Project J/Scene/87일차 PLAY 게임 모드 카드 구성";

        private const string MainMenuScenePath =
            "Assets/ProjectJ/Scenes/MainMenu.unity";

        [MenuItem(MenuPath)]
        private static void ConfigurePlayModeCards()
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
                    "[Project J/Day87] PlayPanel을 찾지 못했습니다. 먼저 86일차 MainMenu Scene 구성을 적용해주세요."
                );

                return;
            }

            ClearChildren(
                playPanel
            );

            ProjectJPlayModePanel panelController =
                playPanel.GetComponent<
                    ProjectJPlayModePanel
                >();

            if (panelController == null)
            {
                panelController =
                    playPanel.gameObject
                        .AddComponent<
                            ProjectJPlayModePanel
                        >();
            }

            BuildHeader(
                playPanel
            );

            Transform cardContainer =
                BuildCardContainer(
                    playPanel
                );

            List<ProjectJGameModeCard>
                cards =
                    new List<
                        ProjectJGameModeCard
                    >();

            cards.Add(
                BuildCard(
                    panelController,
                    cardContainer,
                    "QuickPlayCard",
                    ProjectJGameModeId
                        .QuickPlay,
                    "QUICK PLAY",
                    "빠르게 참가할 수 있는 공개 경기 모드입니다.",
                    true,
                    -504f,
                    new Color(
                        0.43f,
                        0.32f,
                        0.64f,
                        1f
                    )
                )
            );

            cards.Add(
                BuildCard(
                    panelController,
                    cardContainer,
                    "PrivateMatchCard",
                    ProjectJGameModeId
                        .PrivateMatch,
                    "PRIVATE MATCH",
                    "친구와 비공개 방을 만들거나 Room Code를 사용해 참가합니다.",
                    false,
                    -168f,
                    new Color(
                        0.18f,
                        0.43f,
                        0.66f,
                        1f
                    )
                )
            );

            cards.Add(
                BuildCard(
                    panelController,
                    cardContainer,
                    "TrainingCard",
                    ProjectJGameModeId
                        .Training,
                    "TRAINING",
                    "혼자 이동, 점프, 장애물과 조작을 연습하는 모드입니다.",
                    true,
                    168f,
                    new Color(
                        0.27f,
                        0.55f,
                        0.45f,
                        1f
                    )
                )
            );

            cards.Add(
                BuildCard(
                    panelController,
                    cardContainer,
                    "CustomGameCard",
                    ProjectJGameModeId
                        .CustomGame,
                    "CUSTOM GAME",
                    "규칙과 플레이 조건을 직접 설정하는 사용자 지정 경기입니다.",
                    true,
                    504f,
                    new Color(
                        0.67f,
                        0.40f,
                        0.27f,
                        1f
                    )
                )
            );

            BuildDetailPanel(
                playPanel,
                out Text detailTitle,
                out Text detailDescription,
                out Text detailStatus,
                out Button selectButton,
                out Text selectButtonText
            );

            panelController.Configure(
                cards.ToArray(),
                detailTitle,
                detailDescription,
                detailStatus,
                selectButton,
                selectButtonText
            );

            foreach (
                ProjectJGameModeCard card
                in cards
            )
            {
                EditorUtility.SetDirty(
                    card
                );
            }

            EditorUtility.SetDirty(
                panelController
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
                playPanel.gameObject;

            EditorGUIUtility.PingObject(
                playPanel.gameObject
            );

            Debug.Log(
                "[Project J/Day87] PLAY 세로형 게임 모드 카드 구성을 완료했습니다."
            );
        }

        private static void BuildHeader(
            Transform parent
        )
        {
            Text title =
                CreateText(
                    parent,
                    "PlayTitle",
                    "PLAY",
                    54,
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
                    72f,
                    -82f
                ),
                new Vector2(
                    500f,
                    72f
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
                    "PlaySubtitle",
                    "게임 모드를 선택하세요",
                    22,
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
                    74f,
                    -135f
                ),
                new Vector2(
                    600f,
                    44f
                ),
                new Vector2(
                    0f,
                    1f
                )
            );

            subtitle.color =
                new Color(
                    0.79f,
                    0.83f,
                    0.90f,
                    1f
                );
        }

        private static Transform
            BuildCardContainer(
                Transform parent
            )
        {
            GameObject container =
                CreateUiObject(
                    "ModeCardContainer",
                    parent
                );

            RectTransform rect =
                container.GetComponent<
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
                new Vector2(
                    0f,
                    34f
                );

            rect.sizeDelta =
                new Vector2(
                    1360f,
                    570f
                );

            return
                container.transform;
        }

        private static ProjectJGameModeCard
            BuildCard(
                ProjectJPlayModePanel owner,
                Transform parent,
                string objectName,
                ProjectJGameModeId modeId,
                string displayName,
                string description,
                bool comingSoon,
                float x,
                Color cardColor
            )
        {
            GameObject cardObject =
                CreateUiObject(
                    objectName,
                    parent
                );

            RectTransform cardRect =
                cardObject.GetComponent<
                    RectTransform
                >();

            cardRect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            cardRect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            cardRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            cardRect.anchoredPosition =
                new Vector2(
                    x,
                    0f
                );

            cardRect.sizeDelta =
                new Vector2(
                    300f,
                    540f
                );

            Image background =
                cardObject.AddComponent<
                    Image
                >();

            background.color =
                cardColor;

            Outline outline =
                cardObject.AddComponent<
                    Outline
                >();

            outline.effectColor =
                new Color(
                    0.18f,
                    0.90f,
                    1f,
                    1f
                );

            outline.effectDistance =
                new Vector2(
                    4f,
                    -4f
                );

            outline.useGraphicAlpha =
                true;

            outline.enabled =
                false;

            GameObject artArea =
                CreateUiObject(
                    "ModeVisual",
                    cardObject.transform
                );

            RectTransform artRect =
                artArea.GetComponent<
                    RectTransform
                >();

            artRect.anchorMin =
                new Vector2(
                    0f,
                    0.30f
                );

            artRect.anchorMax =
                new Vector2(
                    1f,
                    1f
                );

            artRect.offsetMin =
                new Vector2(
                    14f,
                    14f
                );

            artRect.offsetMax =
                new Vector2(
                    -14f,
                    -14f
                );

            RawImage artImage =
                artArea.AddComponent<
                    RawImage
                >();

            // 87일차 임시 상태:
            // 카드 이미지는 나중에 각 모드별 Texture를 직접 연결한다.
            artImage.texture =
                null;

            artImage.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0f
                );

            artImage.raycastTarget =
                false;

            Text title =
                CreateText(
                    cardObject.transform,
                    "ModeTitle",
                    displayName,
                    25,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            RectTransform titleRect =
                title.rectTransform;

            titleRect.anchorMin =
                new Vector2(
                    0f,
                    0.12f
                );

            titleRect.anchorMax =
                new Vector2(
                    1f,
                    0.28f
                );

            titleRect.offsetMin =
                new Vector2(
                    12f,
                    0f
                );

            titleRect.offsetMax =
                new Vector2(
                    -12f,
                    0f
                );

            title.color =
                Color.white;

            Text status =
                CreateText(
                    cardObject.transform,
                    "StatusText",
                    comingSoon
                        ? "COMING SOON"
                        : "AVAILABLE",
                    14,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            RectTransform statusRect =
                status.rectTransform;

            statusRect.anchorMin =
                new Vector2(
                    0f,
                    0.02f
                );

            statusRect.anchorMax =
                new Vector2(
                    1f,
                    0.11f
                );

            statusRect.offsetMin =
                new Vector2(
                    12f,
                    0f
                );

            statusRect.offsetMax =
                new Vector2(
                    -12f,
                    0f
                );

            status.color =
                comingSoon
                    ? new Color(
                        1f,
                        0.85f,
                        0.55f,
                        1f
                    )
                    : new Color(
                        0.55f,
                        1f,
                        0.86f,
                        1f
                    );

            ProjectJGameModeCard card =
                cardObject.AddComponent<
                    ProjectJGameModeCard
                >();

            card.Configure(
                owner,
                modeId,
                displayName,
                description,
                comingSoon,
                cardRect,
                background,
                outline,
                title,
                status,
                cardColor
            );

            return
                card;
        }

        private static void BuildDetailPanel(
            Transform parent,
            out Text detailTitle,
            out Text detailDescription,
            out Text detailStatus,
            out Button selectButton,
            out Text selectButtonText
        )
        {
            GameObject detailPanel =
                CreateUiObject(
                    "ModeDetailPanel",
                    parent
                );

            RectTransform detailRect =
                detailPanel.GetComponent<
                    RectTransform
                >();

            detailRect.anchorMin =
                new Vector2(
                    0.5f,
                    0f
                );

            detailRect.anchorMax =
                new Vector2(
                    0.5f,
                    0f
                );

            detailRect.pivot =
                new Vector2(
                    0.5f,
                    0f
                );

            detailRect.anchoredPosition =
                new Vector2(
                    0f,
                    26f
                );

            detailRect.sizeDelta =
                new Vector2(
                    1360f,
                    118f
                );

            Image panelImage =
                detailPanel.AddComponent<
                    Image
                >();

            panelImage.color =
                new Color(
                    0.05f,
                    0.06f,
                    0.13f,
                    0.88f
                );

            detailTitle =
                CreateText(
                    detailPanel.transform,
                    "DetailTitle",
                    "게임 모드를 선택하세요",
                    23,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                detailTitle.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    24f,
                    -14f
                ),
                new Vector2(
                    700f,
                    36f
                ),
                new Vector2(
                    0f,
                    1f
                )
            );

            detailTitle.color =
                Color.white;

            detailDescription =
                CreateText(
                    detailPanel.transform,
                    "DetailDescription",
                    "카드에 마우스를 올리면 강조되고, 클릭하면 선택 상태가 유지됩니다.",
                    17,
                    TextAnchor.MiddleLeft,
                    FontStyle.Normal
                );

            SetAnchoredRect(
                detailDescription.rectTransform,
                new Vector2(
                    0f,
                    0f
                ),
                new Vector2(
                    24f,
                    15f
                ),
                new Vector2(
                    900f,
                    56f
                ),
                new Vector2(
                    0f,
                    0f
                )
            );

            detailDescription.color =
                new Color(
                    0.80f,
                    0.84f,
                    0.92f,
                    1f
                );

            detailStatus =
                CreateText(
                    detailPanel.transform,
                    "DetailStatus",
                    string.Empty,
                    15,
                    TextAnchor.MiddleRight,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                detailStatus.rectTransform,
                new Vector2(
                    1f,
                    1f
                ),
                new Vector2(
                    -260f,
                    -20f
                ),
                new Vector2(
                    400f,
                    34f
                ),
                new Vector2(
                    1f,
                    1f
                )
            );

            detailStatus.color =
                new Color(
                    0.56f,
                    0.90f,
                    1f,
                    1f
                );

            GameObject buttonObject =
                CreateUiObject(
                    "SelectButton",
                    detailPanel.transform
                );

            RectTransform buttonRect =
                buttonObject.GetComponent<
                    RectTransform
                >();

            buttonRect.anchorMin =
                new Vector2(
                    1f,
                    0f
                );

            buttonRect.anchorMax =
                new Vector2(
                    1f,
                    0f
                );

            buttonRect.pivot =
                new Vector2(
                    1f,
                    0f
                );

            buttonRect.anchoredPosition =
                new Vector2(
                    -22f,
                    18f
                );

            buttonRect.sizeDelta =
                new Vector2(
                    220f,
                    58f
                );

            Image buttonImage =
                buttonObject.AddComponent<
                    Image
                >();

            buttonImage.color =
                new Color(
                    0.12f,
                    0.62f,
                    0.88f,
                    1f
                );

            selectButton =
                buttonObject.AddComponent<
                    Button
                >();

            selectButton.targetGraphic =
                buttonImage;

            selectButton.interactable =
                false;

            selectButtonText =
                CreateText(
                    buttonObject.transform,
                    "Label",
                    "SELECT",
                    19,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            StretchFull(
                selectButtonText.rectTransform
            );

            selectButtonText.color =
                Color.white;
        }

        private static Transform
            FindTransformInScene(
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
                        parent.GetChild(
                            index
                        ),
                        objectName
                    );

                if (found != null)
                {
                    return found;
                }
            }

            return null;
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
                    parent.GetChild(
                        index
                    ).gameObject
                );
            }
        }

        private static GameObject
            CreateUiObject(
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
            FontStyle style
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
    }
}
