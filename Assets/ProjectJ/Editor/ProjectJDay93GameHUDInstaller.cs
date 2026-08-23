using ProjectJ.Networking.Fusion; // Day93 HUD Component 사용
using UnityEditor; // Editor Menu와 SerializedObject 사용
using UnityEditor.SceneManagement; // Game Scene 편집
using UnityEngine; // GameObject와 RectTransform 사용
using UnityEngine.EventSystems; // EventSystem 확인
using UnityEngine.InputSystem.UI; // Input System UI Module 생성
using UnityEngine.SceneManagement; // Scene 구조 사용
using UnityEngine.UI; // Canvas와 UI Component 사용

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay93GameHUDInstaller
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private const string CanvasRootName =
            "Day93GameHUDCanvas";

        private static readonly Color PanelColor =
            new Color(
                0.05f,
                0.06f,
                0.09f,
                0.82f
            );

        private static readonly Color ResultPanelColor =
            new Color(
                0.035f,
                0.04f,
                0.065f,
                0.96f
            );

        private static readonly Color PrimaryTextColor =
            new Color(
                0.96f,
                0.97f,
                1f,
                1f
            );

        private static readonly Color SecondaryTextColor =
            new Color(
                0.72f,
                0.78f,
                0.88f,
                1f
            );

        private static readonly Color AccentColor =
            new Color(
                0.18f,
                0.55f,
                0.95f,
                1f
            );

        [MenuItem(
            "Project J/Scene/93일차 Game HUD 구성"
        )]
        private static void Install()
        {
            if (
                !EditorSceneManager
                    .SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                return;
            }

            SceneAsset gameSceneAsset =
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(
                    GameScenePath
                );

            if (gameSceneAsset == null)
            {
                Debug.LogError(
                    "[Project J/Day93] Game Scene을 찾지 못했습니다. / " +
                    GameScenePath
                );

                return;
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    GameScenePath,
                    OpenSceneMode.Single
                );

            RemoveExistingCanvas(
                scene
            ); // 재실행 시 Day93 HUD 중복 제거

            EnsureEventSystem(
                scene
            ); // Button 입력용 EventSystem 보장

            GameObject canvasObject =
                CreateCanvas(
                    scene
                );

            ProjectJDay93GameHUD hud =
                canvasObject.AddComponent<
                    ProjectJDay93GameHUD
                >();

            GameObject matchHudRoot =
                CreateFullScreenRoot(
                    canvasObject.transform,
                    "MatchHUD"
                );

            BuildMatchHud(
                matchHudRoot.transform,
                out Text timerText,
                out Text heightText,
                out Text rankText,
                out Text staminaText,
                out Image staminaFill,
                out Text leftItemText,
                out Text rightItemText,
                out Image leftItemBackground,
                out Image rightItemBackground
            );

            GameObject respawnProtectionRoot =
                BuildRespawnProtection(
                    canvasObject.transform,
                    out Text respawnProtectionText
                );

            GameObject countdownRoot =
                BuildCountdown(
                    canvasObject.transform,
                    out Text countdownText
                );

            GameObject resultRoot =
                BuildResult(
                    canvasObject.transform,
                    out Text resultTitleText,
                    out Text resultStatusText,
                    out Text resultRankText,
                    out Text resultTimeText,
                    out Text resultHeightText,
                    out Button returnLobbyButton,
                    out Text returnLobbyButtonText
                );

            SerializedObject serializedHud =
                new SerializedObject(
                    hud
                );

            SetReference(
                serializedHud,
                "matchHudRoot",
                matchHudRoot
            );

            SetReference(
                serializedHud,
                "countdownRoot",
                countdownRoot
            );

            SetReference(
                serializedHud,
                "resultRoot",
                resultRoot
            );

            SetReference(
                serializedHud,
                "respawnProtectionRoot",
                respawnProtectionRoot
            );

            SetReference(
                serializedHud,
                "timerText",
                timerText
            );

            SetReference(
                serializedHud,
                "heightText",
                heightText
            );

            SetReference(
                serializedHud,
                "rankText",
                rankText
            );

            SetReference(
                serializedHud,
                "staminaText",
                staminaText
            );

            SetReference(
                serializedHud,
                "staminaFill",
                staminaFill
            );

            SetReference(
                serializedHud,
                "leftItemText",
                leftItemText
            );

            SetReference(
                serializedHud,
                "rightItemText",
                rightItemText
            );

            SetReference(
                serializedHud,
                "leftItemBackground",
                leftItemBackground
            );

            SetReference(
                serializedHud,
                "rightItemBackground",
                rightItemBackground
            );

            SetReference(
                serializedHud,
                "respawnProtectionText",
                respawnProtectionText
            );

            SetReference(
                serializedHud,
                "countdownText",
                countdownText
            );

            SetReference(
                serializedHud,
                "resultTitleText",
                resultTitleText
            );

            SetReference(
                serializedHud,
                "resultStatusText",
                resultStatusText
            );

            SetReference(
                serializedHud,
                "resultRankText",
                resultRankText
            );

            SetReference(
                serializedHud,
                "resultTimeText",
                resultTimeText
            );

            SetReference(
                serializedHud,
                "resultHeightText",
                resultHeightText
            );

            SetReference(
                serializedHud,
                "returnLobbyButton",
                returnLobbyButton
            );

            SetReference(
                serializedHud,
                "returnLobbyButtonText",
                returnLobbyButtonText
            );

            serializedHud.ApplyModifiedPropertiesWithoutUndo();

            countdownRoot.SetActive(
                false
            );

            respawnProtectionRoot.SetActive(
                false
            );

            resultRoot.SetActive(
                false
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
                canvasObject;

            EditorGUIUtility.PingObject(
                canvasObject
            );

            Debug.Log(
                "[Project J/Day93] Game HUD·Countdown·Result Canvas 구성을 완료했습니다."
            );
        }

        private static GameObject CreateCanvas(
            Scene scene
        )
        {
            GameObject canvasObject =
                new GameObject(
                    CanvasRootName,
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster)
                );

            SceneManager.MoveGameObjectToScene(
                canvasObject,
                scene
            );

            Canvas canvas =
                canvasObject.GetComponent<
                    Canvas
                >();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder =
                93;

            CanvasScaler scaler =
                canvasObject.GetComponent<
                    CanvasScaler
                >();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode
                    .ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(
                    1920f,
                    1080f
                );

            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode
                    .MatchWidthOrHeight;

            scaler.matchWidthOrHeight =
                0.5f;

            return canvasObject;
        }

        private static void BuildMatchHud(
            Transform parent,
            out Text timerText,
            out Text heightText,
            out Text rankText,
            out Text staminaText,
            out Image staminaFill,
            out Text leftItemText,
            out Text rightItemText,
            out Image leftItemBackground,
            out Image rightItemBackground
        )
        {
            GameObject infoPanel =
                CreatePanel(
                    parent,
                    "MatchInfoPanel",
                    PanelColor
                );

            SetRect(
                infoPanel.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    24f,
                    -24f
                ),
                new Vector2(
                    330f,
                    154f
                ),
                new Vector2(
                    0f,
                    1f
                )
            );

            timerText =
                CreateText(
                    infoPanel.transform,
                    "TimerText",
                    "TIME --:--",
                    30f,
                    PrimaryTextColor,
                    TextAnchor.MiddleLeft
                );

            SetRect(
                timerText.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    1f,
                    1f
                ),
                new Vector2(
                    18f,
                    -16f
                ),
                new Vector2(
                    -18f,
                    42f
                ),
                new Vector2(
                    0.5f,
                    1f
                )
            );

            heightText =
                CreateText(
                    infoPanel.transform,
                    "HeightText",
                    "HEIGHT --.-- m",
                    24f,
                    SecondaryTextColor,
                    TextAnchor.MiddleLeft
                );

            SetRect(
                heightText.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    1f,
                    1f
                ),
                new Vector2(
                    18f,
                    -62f
                ),
                new Vector2(
                    -18f,
                    34f
                ),
                new Vector2(
                    0.5f,
                    1f
                )
            );

            rankText =
                CreateText(
                    infoPanel.transform,
                    "RankText",
                    "RANK -- / --",
                    28f,
                    PrimaryTextColor,
                    TextAnchor.MiddleLeft
                );

            SetRect(
                rankText.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    1f,
                    1f
                ),
                new Vector2(
                    18f,
                    -104f
                ),
                new Vector2(
                    -18f,
                    36f
                ),
                new Vector2(
                    0.5f,
                    1f
                )
            );

            GameObject staminaPanel =
                CreatePanel(
                    parent,
                    "StaminaPanel",
                    PanelColor
                );

            SetRect(
                staminaPanel.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0f,
                    0f
                ),
                new Vector2(
                    0f,
                    0f
                ),
                new Vector2(
                    24f,
                    24f
                ),
                new Vector2(
                    360f,
                    86f
                ),
                new Vector2(
                    0f,
                    0f
                )
            );

            staminaText =
                CreateText(
                    staminaPanel.transform,
                    "StaminaText",
                    "STAMINA -- / --",
                    20f,
                    PrimaryTextColor,
                    TextAnchor.MiddleLeft
                );

            SetRect(
                staminaText.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    1f,
                    1f
                ),
                new Vector2(
                    16f,
                    -10f
                ),
                new Vector2(
                    -16f,
                    28f
                ),
                new Vector2(
                    0.5f,
                    1f
                )
            );

            GameObject staminaBackground =
                CreatePanel(
                    staminaPanel.transform,
                    "StaminaBarBackground",
                    new Color(
                        0.12f,
                        0.13f,
                        0.17f,
                        1f
                    )
                );

            SetRect(
                staminaBackground.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0f,
                    0f
                ),
                new Vector2(
                    1f,
                    0f
                ),
                new Vector2(
                    16f,
                    14f
                ),
                new Vector2(
                    -16f,
                    28f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            GameObject staminaFillObject =
                CreatePanel(
                    staminaBackground.transform,
                    "StaminaFill",
                    AccentColor
                );

            RectTransform fillRect =
                staminaFillObject.GetComponent<
                    RectTransform
                >();

            fillRect.anchorMin =
                Vector2.zero;

            fillRect.anchorMax =
                Vector2.one;

            fillRect.offsetMin =
                Vector2.zero;

            fillRect.offsetMax =
                Vector2.zero;

            staminaFill =
                staminaFillObject.GetComponent<
                    Image
                >();

            staminaFill.type =
                Image.Type.Filled;

            staminaFill.fillMethod =
                Image.FillMethod.Horizontal;

            staminaFill.fillOrigin =
                0;

            staminaFill.fillAmount =
                1f;

            GameObject itemsRoot =
                CreateFullScreenRoot(
                    parent,
                    "ItemSlots"
                );

            GameObject leftSlot =
                CreatePanel(
                    itemsRoot.transform,
                    "ItemSlot01",
                    new Color(
                        0.12f,
                        0.14f,
                        0.18f,
                        0.92f
                    )
                );

            SetRect(
                leftSlot.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    1f,
                    0f
                ),
                new Vector2(
                    1f,
                    0f
                ),
                new Vector2(
                    -426f,
                    24f
                ),
                new Vector2(
                    190f,
                    104f
                ),
                new Vector2(
                    1f,
                    0f
                )
            );

            leftItemBackground =
                leftSlot.GetComponent<
                    Image
                >();

            leftItemText =
                CreateText(
                    leftSlot.transform,
                    "ItemSlot01Text",
                    "SLOT 1\nEmpty",
                    21f,
                    PrimaryTextColor,
                    TextAnchor.MiddleCenter
                );

            Stretch(
                leftItemText.rectTransform,
                10f
            );

            GameObject rightSlot =
                CreatePanel(
                    itemsRoot.transform,
                    "ItemSlot02",
                    new Color(
                        0.12f,
                        0.14f,
                        0.18f,
                        0.92f
                    )
                );

            SetRect(
                rightSlot.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    1f,
                    0f
                ),
                new Vector2(
                    1f,
                    0f
                ),
                new Vector2(
                    -222f,
                    24f
                ),
                new Vector2(
                    190f,
                    104f
                ),
                new Vector2(
                    1f,
                    0f
                )
            );

            rightItemBackground =
                rightSlot.GetComponent<
                    Image
                >();

            rightItemText =
                CreateText(
                    rightSlot.transform,
                    "ItemSlot02Text",
                    "SLOT 2\nEmpty",
                    21f,
                    PrimaryTextColor,
                    TextAnchor.MiddleCenter
                );

            Stretch(
                rightItemText.rectTransform,
                10f
            );
        }

        private static GameObject BuildRespawnProtection(
            Transform parent,
            out Text protectionText
        )
        {
            GameObject root =
                CreatePanel(
                    parent,
                    "RespawnProtection",
                    new Color(
                        0.18f,
                        0.55f,
                        0.95f,
                        0.88f
                    )
                );

            SetRect(
                root.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.5f,
                    1f
                ),
                new Vector2(
                    0.5f,
                    1f
                ),
                new Vector2(
                    0f,
                    -38f
                ),
                new Vector2(
                    360f,
                    52f
                ),
                new Vector2(
                    0.5f,
                    1f
                )
            );

            protectionText =
                CreateText(
                    root.transform,
                    "RespawnProtectionText",
                    "RESPAWN PROTECTION  3.0s",
                    22f,
                    PrimaryTextColor,
                    TextAnchor.MiddleCenter
                );

            Stretch(
                protectionText.rectTransform,
                8f
            );

            return root;
        }

        private static GameObject BuildCountdown(
            Transform parent,
            out Text countdownText
        )
        {
            GameObject root =
                CreateFullScreenRoot(
                    parent,
                    "CountdownPanel"
                );

            countdownText =
                CreateText(
                    root.transform,
                    "CountdownText",
                    "3",
                    132f,
                    PrimaryTextColor,
                    TextAnchor.MiddleCenter
                );

            SetRect(
                countdownText.rectTransform,
                new Vector2(
                    0.5f,
                    0.5f
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                Vector2.zero,
                new Vector2(
                    420f,
                    180f
                ),
                new Vector2(
                    0.5f,
                    0.5f
                )
            );

            return root;
        }

        private static GameObject BuildResult(
            Transform parent,
            out Text titleText,
            out Text statusText,
            out Text rankText,
            out Text timeText,
            out Text heightText,
            out Button returnButton,
            out Text returnButtonText
        )
        {
            GameObject root =
                CreateFullScreenRoot(
                    parent,
                    "ResultPanel"
                );

            GameObject dim =
                CreatePanel(
                    root.transform,
                    "Dim",
                    new Color(
                        0f,
                        0f,
                        0f,
                        0.55f
                    )
                );

            Stretch(
                dim.GetComponent<
                    RectTransform
                >(),
                0f
            );

            GameObject card =
                CreatePanel(
                    root.transform,
                    "ResultCard",
                    ResultPanelColor
                );

            SetRect(
                card.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                Vector2.zero,
                new Vector2(
                    620f,
                    470f
                ),
                new Vector2(
                    0.5f,
                    0.5f
                )
            );

            titleText =
                CreateText(
                    card.transform,
                    "ResultTitle",
                    "FINISH!",
                    54f,
                    PrimaryTextColor,
                    TextAnchor.MiddleCenter
                );

            SetRect(
                titleText.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    1f,
                    1f
                ),
                new Vector2(
                    24f,
                    -32f
                ),
                new Vector2(
                    -24f,
                    72f
                ),
                new Vector2(
                    0.5f,
                    1f
                )
            );

            statusText =
                CreateText(
                    card.transform,
                    "ResultStatus",
                    "PERSONAL RESULT",
                    20f,
                    SecondaryTextColor,
                    TextAnchor.MiddleCenter
                );

            SetRect(
                statusText.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    1f,
                    1f
                ),
                new Vector2(
                    24f,
                    -104f
                ),
                new Vector2(
                    -24f,
                    34f
                ),
                new Vector2(
                    0.5f,
                    1f
                )
            );

            rankText =
                CreateText(
                    card.transform,
                    "ResultRank",
                    "RANK  -- / --",
                    34f,
                    PrimaryTextColor,
                    TextAnchor.MiddleCenter
                );

            SetRect(
                rankText.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    1f,
                    1f
                ),
                new Vector2(
                    40f,
                    -164f
                ),
                new Vector2(
                    -40f,
                    46f
                ),
                new Vector2(
                    0.5f,
                    1f
                )
            );

            timeText =
                CreateText(
                    card.transform,
                    "ResultTime",
                    "TIME  --:--.--",
                    28f,
                    PrimaryTextColor,
                    TextAnchor.MiddleCenter
                );

            SetRect(
                timeText.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    1f,
                    1f
                ),
                new Vector2(
                    40f,
                    -224f
                ),
                new Vector2(
                    -40f,
                    40f
                ),
                new Vector2(
                    0.5f,
                    1f
                )
            );

            heightText =
                CreateText(
                    card.transform,
                    "ResultBestHeight",
                    "BEST HEIGHT  --.-- m",
                    26f,
                    PrimaryTextColor,
                    TextAnchor.MiddleCenter
                );

            SetRect(
                heightText.rectTransform,
                new Vector2(
                    0f,
                    1f
                ),
                new Vector2(
                    1f,
                    1f
                ),
                new Vector2(
                    40f,
                    -278f
                ),
                new Vector2(
                    -40f,
                    40f
                ),
                new Vector2(
                    0.5f,
                    1f
                )
            );

            GameObject buttonObject =
                CreatePanel(
                    card.transform,
                    "ReturnLobbyButton",
                    AccentColor
                );

            SetRect(
                buttonObject.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    34f
                ),
                new Vector2(
                    360f,
                    64f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            returnButton =
                buttonObject.AddComponent<
                    Button
                >();

            Image buttonImage =
                buttonObject.GetComponent<
                    Image
                >();

            buttonImage.raycastTarget =
                true; // Button Pointer 입력 허용

            returnButton.targetGraphic =
                buttonImage; // Button 시각·Raycast 대상 연결

            ColorBlock colors =
                returnButton.colors;

            colors.normalColor =
                Color.white;

            colors.highlightedColor =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.92f
                );

            colors.pressedColor =
                new Color(
                    0.8f,
                    0.8f,
                    0.8f,
                    1f
                );

            colors.disabledColor =
                new Color(
                    0.35f,
                    0.35f,
                    0.4f,
                    0.8f
                );

            returnButton.colors =
                colors;

            returnButtonText =
                CreateText(
                    buttonObject.transform,
                    "ReturnLobbyButtonText",
                    "WAIT FOR MATCH END",
                    23f,
                    PrimaryTextColor,
                    TextAnchor.MiddleCenter
                );

            Stretch(
                returnButtonText.rectTransform,
                8f
            );

            returnButtonText.raycastTarget =
                false;

            return root;
        }

        private static GameObject CreateFullScreenRoot(
            Transform parent,
            string objectName
        )
        {
            GameObject root =
                new GameObject(
                    objectName,
                    typeof(RectTransform)
                );

            root.transform.SetParent(
                parent,
                false
            );

            Stretch(
                root.GetComponent<
                    RectTransform
                >(),
                0f
            );

            return root;
        }

        private static GameObject CreatePanel(
            Transform parent,
            string objectName,
            Color color
        )
        {
            GameObject panel =
                new GameObject(
                    objectName,
                    typeof(RectTransform),
                    typeof(Image)
                );

            panel.transform.SetParent(
                parent,
                false
            );

            Image image =
                panel.GetComponent<
                    Image
                >();

            image.color =
                color;

            image.raycastTarget =
                false;

            return panel;
        }

        private static Text CreateText(
            Transform parent,
            string objectName,
            string value,
            float fontSize,
            Color color,
            TextAnchor alignment
        )
        {
            GameObject textObject =
                new GameObject(
                    objectName,
                    typeof(RectTransform),
                    typeof(Text)
                );

            textObject.transform.SetParent(
                parent,
                false
            );

            Text text =
                textObject.GetComponent<
                    Text
                >();

            text.text =
                value;

            text.fontSize =
                Mathf.RoundToInt(
                    fontSize
                );

            text.color =
                color;

            text.alignment =
                alignment;

            text.horizontalOverflow =
                HorizontalWrapMode.Overflow;

            text.verticalOverflow =
                VerticalWrapMode.Overflow;

            text.raycastTarget =
                false;

            Font runtimeFont =
                Resources.GetBuiltinResource<
                    Font
                >(
                    "LegacyRuntime.ttf"
                ); // 프로젝트 기존 UI와 동일한 Unity 기본 Font 사용

            if (runtimeFont == null)
            {
                Debug.LogError(
                    "[Project J/Day93] LegacyRuntime.ttf를 불러오지 못했습니다."
                );
            }
            else
            {
                text.font =
                    runtimeFont;
            }

            return text;
        }

        private static void EnsureEventSystem(
            Scene scene
        )
        {
            EventSystem[] eventSystems =
                Object.FindObjectsByType<
                    EventSystem
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            for (
                int index = 0;
                index < eventSystems.Length;
                index++
            )
            {
                if (
                    eventSystems[index] != null &&
                    eventSystems[index]
                        .gameObject.scene ==
                        scene
                )
                {
                    return; // 기존 Game EventSystem 재사용
                }
            }

            GameObject eventSystemObject =
                new GameObject(
                    "EventSystem",
                    typeof(EventSystem),
                    typeof(InputSystemUIInputModule)
                );

            SceneManager.MoveGameObjectToScene(
                eventSystemObject,
                scene
            );
        }

        private static void RemoveExistingCanvas(
            Scene scene
        )
        {
            GameObject[] roots =
                scene.GetRootGameObjects();

            for (
                int index = 0;
                index < roots.Length;
                index++
            )
            {
                if (
                    roots[index] != null &&
                    roots[index].name ==
                        CanvasRootName
                )
                {
                    Object.DestroyImmediate(
                        roots[index]
                    ); // 이전 Day93 HUD 재생성
                    return;
                }
            }
        }

        private static void SetReference(
            SerializedObject serializedObject,
            string propertyName,
            Object value
        )
        {
            SerializedProperty property =
                serializedObject.FindProperty(
                    propertyName
                );

            if (property == null)
            {
                Debug.LogError(
                    "[Project J/Day93] HUD Serialized Field를 찾지 못했습니다. / " +
                    propertyName
                );

                return;
            }

            property.objectReferenceValue =
                value;
        }

        private static void Stretch(
            RectTransform rect,
            float margin
        )
        {
            rect.anchorMin =
                Vector2.zero;

            rect.anchorMax =
                Vector2.one;

            rect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.offsetMin =
                new Vector2(
                    margin,
                    margin
                );

            rect.offsetMax =
                new Vector2(
                    -margin,
                    -margin
                );
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Vector2 pivot
        )
        {
            rect.anchorMin =
                anchorMin;

            rect.anchorMax =
                anchorMax;

            rect.pivot =
                pivot;

            rect.anchoredPosition =
                anchoredPosition;

            rect.sizeDelta =
                sizeDelta;
        }
    }
}
