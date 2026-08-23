using System.Collections.Generic; // Button/Text 목록 구성
using UnityEditor; // MenuItem, AssetDatabase 사용
using UnityEditor.SceneManagement; // Scene 열기/저장
using UnityEngine; // GameObject, Material, Camera 사용
using UnityEngine.EventSystems; // EventSystem 탐색
using UnityEngine.UI; // MainMenu UI 구성

namespace ProjectJ.Editor
{
    internal static class
        ProjectJDay86MainMenuSceneInstaller
    {
        private const string MenuPath =
            "Project J/Scene/86일차 MainMenu Scene 구성";

        private const string MainMenuScenePath =
            "Assets/ProjectJ/Scenes/MainMenu.unity";

        private const string PreviewMaterialPath =
            "Assets/ProjectJ/Art/UI/MainMenu/Day86PreviewCharacter.mat";

        private static readonly Color
            SelectedColor =
                new Color(
                    0.20f,
                    0.82f,
                    1f,
                    1f
                );

        [MenuItem(MenuPath)]
        private static void ConfigureMainMenuScene()
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

            RemoveLegacyObjects();

            GameObject cameraRoot =
                EnsureRoot(
                    "=== CAMERA ==="
                );

            GameObject previewRoot =
                BuildCharacterPreview(
                    cameraRoot
                );

            BuildEventSystemRoot();

            ProjectJMainMenuController controller =
                BuildMainMenuUi(
                    previewRoot
                );

            BuildControllerRoot(
                controller
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
                controller.gameObject;

            EditorGUIUtility.PingObject(
                controller.gameObject
            );

            Debug.Log(
                "[Project J/Day86] MainMenu Scene 구성을 완료했습니다."
            );
        }

        private static void RemoveLegacyObjects()
        {
            DestroyByName(
                "UI_MainMenu"
            );

            DestroyByName(
                "SceneNavigation"
            );

            DestroyByName(
                "Directional Light"
            );

            DestroyByName(
                "=== CHARACTER PREVIEW ==="
            );

            DestroyByName(
                "=== UI ==="
            );

            DestroyByName(
                "=== MENU SYSTEM ==="
            );
        }

        private static GameObject
            BuildCharacterPreview(
                GameObject cameraRoot
            )
        {
            Camera mainCamera =
                Camera.main;

            if (mainCamera == null)
            {
                GameObject cameraObject =
                    new GameObject(
                        "Main Camera"
                    );

                mainCamera =
                    cameraObject.AddComponent<
                        Camera
                    >();

                cameraObject.AddComponent<
                    AudioListener
                >();

                cameraObject.tag =
                    "MainCamera";
            }

            mainCamera.transform.SetParent(
                cameraRoot.transform,
                false
            );

            mainCamera.transform.position =
                new Vector3(
                    0f,
                    1.4f,
                    -6.2f
                );

            mainCamera.transform.rotation =
                Quaternion.LookRotation(
                    new Vector3(
                        0f,
                        1.2f,
                        0f
                    ) -
                    mainCamera.transform.position
                );

            mainCamera.clearFlags =
                CameraClearFlags.SolidColor;

            mainCamera.backgroundColor =
                new Color(
                    0.025f,
                    0.04f,
                    0.075f,
                    1f
                );

            AudioListener[]
                audioListeners =
                    Object.FindObjectsByType<
                        AudioListener
                    >(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None
                    );

            foreach (
                AudioListener listener
                in audioListeners
            )
            {
                if (
                    listener.gameObject !=
                    mainCamera.gameObject
                )
                {
                    Object.DestroyImmediate(
                        listener
                    );
                }
            }

            if (
                mainCamera.GetComponent<
                    AudioListener
                >() == null
            )
            {
                mainCamera.gameObject
                    .AddComponent<
                        AudioListener
                    >();
            }

            GameObject previewRoot =
                new GameObject(
                    "=== CHARACTER PREVIEW ==="
                );

            GameObject previewCharacter =
                new GameObject(
                    "CharacterPreviewRoot"
                );

            previewCharacter.transform
                .SetParent(
                    previewRoot.transform,
                    false
                );

            previewCharacter.transform.position =
                new Vector3(
                    0f,
                    0f,
                    0f
                );

            GameObject visualRoot =
                new GameObject(
                    "PreviewVisual"
                );

            visualRoot.transform.SetParent(
                previewCharacter.transform,
                false
            );

            Material previewMaterial =
                GetOrCreatePreviewMaterial();

            GameObject body =
                GameObject.CreatePrimitive(
                    PrimitiveType.Capsule
                );

            body.name =
                "Body";

            body.transform.SetParent(
                visualRoot.transform,
                false
            );

            body.transform.localPosition =
                new Vector3(
                    0f,
                    1.15f,
                    0f
                );

            body.transform.localScale =
                new Vector3(
                    0.85f,
                    1.05f,
                    0.62f
                );

            RemoveCollider(
                body
            );

            ApplyMaterial(
                body,
                previewMaterial
            );

            GameObject head =
                GameObject.CreatePrimitive(
                    PrimitiveType.Sphere
                );

            head.name =
                "Head";

            head.transform.SetParent(
                visualRoot.transform,
                false
            );

            head.transform.localPosition =
                new Vector3(
                    0f,
                    2.42f,
                    0f
                );

            head.transform.localScale =
                new Vector3(
                    0.72f,
                    0.72f,
                    0.72f
                );

            RemoveCollider(
                head
            );

            ApplyMaterial(
                head,
                previewMaterial
            );

            GameObject leftArm =
                GameObject.CreatePrimitive(
                    PrimitiveType.Capsule
                );

            leftArm.name =
                "LeftArm";

            leftArm.transform.SetParent(
                visualRoot.transform,
                false
            );

            leftArm.transform.localPosition =
                new Vector3(
                    -0.75f,
                    1.3f,
                    0f
                );

            leftArm.transform.localScale =
                new Vector3(
                    0.28f,
                    0.78f,
                    0.28f
                );

            leftArm.transform.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    -10f
                );

            RemoveCollider(
                leftArm
            );

            ApplyMaterial(
                leftArm,
                previewMaterial
            );

            GameObject rightArm =
                Object.Instantiate(
                    leftArm,
                    visualRoot.transform
                );

            rightArm.name =
                "RightArm";

            rightArm.transform.localPosition =
                new Vector3(
                    0.75f,
                    1.3f,
                    0f
                );

            rightArm.transform.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    10f
                );

            GameObject lightObject =
                new GameObject(
                    "CharacterPreviewLight"
                );

            lightObject.transform.SetParent(
                previewRoot.transform,
                false
            );

            lightObject.transform.rotation =
                Quaternion.Euler(
                    35f,
                    -30f,
                    0f
                );

            Light previewLight =
                lightObject.AddComponent<
                    Light
                >();

            previewLight.type =
                LightType.Directional;

            previewLight.intensity =
                1.6f;

            previewLight.color =
                new Color(
                    0.78f,
                    0.90f,
                    1f,
                    1f
                );

            return
                previewCharacter;
        }

        private static ProjectJMainMenuController
            BuildMainMenuUi(
                GameObject previewRoot
            )
        {
            GameObject uiRoot =
                new GameObject(
                    "=== UI ==="
                );

            GameObject canvasObject =
                new GameObject(
                    "Canvas_MainMenu",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster),
                    typeof(RuntimeUIFontBinder)
                );

            canvasObject.transform.SetParent(
                uiRoot.transform,
                false
            );

            Canvas canvas =
                canvasObject.GetComponent<
                    Canvas
                >();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

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

            BuildBackground(
                canvasObject.transform
            );

            Transform topNavigation =
                BuildTopNavigation(
                    canvasObject.transform
                );

            GameObject contentRoot =
                CreateUiObject(
                    "ContentRoot",
                    canvasObject.transform
                );

            RectTransform contentRect =
                contentRoot.GetComponent<
                    RectTransform
                >();

            contentRect.anchorMin =
                new Vector2(
                    0f,
                    0f
                );

            contentRect.anchorMax =
                new Vector2(
                    1f,
                    1f
                );

            contentRect.offsetMin =
                Vector2.zero;

            contentRect.offsetMax =
                new Vector2(
                    0f,
                    -86f
                );

            GameObject homePanel =
                BuildHomePanel(
                    contentRoot.transform
                );

            GameObject playPanel =
                BuildPlaceholderPanel(
                    contentRoot.transform,
                    "PlayPanel",
                    "PLAY",
                    "게임 모드 카드는 87일차에 구성됩니다.",
                    false
                );

            GameObject customizePanel =
                BuildPlaceholderPanel(
                    contentRoot.transform,
                    "CustomizePanel",
                    "CUSTOMIZE",
                    "캐릭터 꾸미기 기능은 후속 일차에서 연결됩니다.",
                    true
                );

            GameObject profilePanel =
                BuildPlaceholderPanel(
                    contentRoot.transform,
                    "ProfilePanel",
                    "PROFILE",
                    "Player Name\nLevel --\nWins --\nMatches --\nBest Height --",
                    false
                );

            GameObject settingsPanel =
                BuildPlaceholderPanel(
                    contentRoot.transform,
                    "SettingsPanel",
                    "SETTINGS",
                    "그래픽 · 사운드 · 조작 설정은 후속 일차에서 연결됩니다.",
                    false
                );

            Text versionText =
                CreateText(
                    canvasObject.transform,
                    "VersionText",
                    "DEV  •  DAY 86",
                    16,
                    TextAnchor.MiddleRight,
                    FontStyle.Normal
                );

            RectTransform versionRect =
                versionText.rectTransform;

            versionRect.anchorMin =
                new Vector2(
                    1f,
                    0f
                );

            versionRect.anchorMax =
                new Vector2(
                    1f,
                    0f
                );

            versionRect.pivot =
                new Vector2(
                    1f,
                    0f
                );

            versionRect.anchoredPosition =
                new Vector2(
                    -28f,
                    20f
                );

            versionRect.sizeDelta =
                new Vector2(
                    400f,
                    34f
                );

            versionText.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.62f
                );

            Button[] tabButtons =
                new Button[5];

            Text[] tabLabels =
                new Text[5];

            Image[] selectedBars =
                new Image[5];

            CreateNavigationButton(
                topNavigation,
                "HomeButton",
                "HOME",
                300f,
                150f,
                out tabButtons[0],
                out tabLabels[0],
                out selectedBars[0]
            );

            CreateNavigationButton(
                topNavigation,
                "PlayButton",
                "PLAY",
                455f,
                150f,
                out tabButtons[1],
                out tabLabels[1],
                out selectedBars[1]
            );

            CreateNavigationButton(
                topNavigation,
                "CustomizeButton",
                "CUSTOMIZE",
                625f,
                190f,
                out tabButtons[2],
                out tabLabels[2],
                out selectedBars[2]
            );

            CreateNavigationButton(
                topNavigation,
                "ProfileButton",
                "PROFILE",
                825f,
                170f,
                out tabButtons[3],
                out tabLabels[3],
                out selectedBars[3]
            );

            CreateNavigationButton(
                topNavigation,
                "SettingsButton",
                "SETTINGS",
                1530f,
                170f,
                out tabButtons[4],
                out tabLabels[4],
                out selectedBars[4]
            );

            Button exitButton;
            Text exitLabel;
            Image exitBar;

            CreateNavigationButton(
                topNavigation,
                "ExitButton",
                "EXIT",
                1710f,
                140f,
                out exitButton,
                out exitLabel,
                out exitBar
            );

            Object.DestroyImmediate(
                exitBar.gameObject
            );

            GameObject controllerObject =
                new GameObject(
                    "MainMenuController"
                );

            ProjectJMainMenuController
                controller =
                    controllerObject
                        .AddComponent<
                            ProjectJMainMenuController
                        >();

            controller.Configure(
                tabButtons,
                tabLabels,
                selectedBars,
                new[]
                {
                    homePanel,
                    playPanel,
                    customizePanel,
                    profilePanel,
                    settingsPanel
                },
                exitButton,
                previewRoot
            );

            EditorUtility.SetDirty(
                controller
            );

            return
                controller;
        }

        private static void BuildBackground(
            Transform parent
        )
        {
            GameObject background =
                CreateUiObject(
                    "Background",
                    parent
                );

            StretchFull(
                background.GetComponent<
                    RectTransform
                >()
            );

            Image image =
                background.AddComponent<
                    Image
                >();

            image.color =
                new Color(
                    0.82f,
                    0.77f,
                    0.94f,
                    0.90f
                );

            image.raycastTarget =
                false;

            GameObject shade =
                CreateUiObject(
                    "BackgroundShade",
                    parent
                );

            StretchFull(
                shade.GetComponent<
                    RectTransform
                >()
            );

            Image shadeImage =
                shade.AddComponent<
                    Image
                >();

            shadeImage.color =
                new Color(
                    0.23f,
                    0.16f,
                    0.34f,
                    0.10f
                );

            shadeImage.raycastTarget =
                false;
        }

        private static Transform
            BuildTopNavigation(
                Transform parent
            )
        {
            GameObject topBar =
                CreateUiObject(
                    "TopNavigation",
                    parent
                );

            RectTransform rect =
                topBar.GetComponent<
                    RectTransform
                >();

            rect.anchorMin =
                new Vector2(
                    0f,
                    1f
                );

            rect.anchorMax =
                new Vector2(
                    1f,
                    1f
                );

            rect.pivot =
                new Vector2(
                    0.5f,
                    1f
                );

            rect.anchoredPosition =
                Vector2.zero;

            rect.sizeDelta =
                new Vector2(
                    0f,
                    86f
                );

            Image barImage =
                topBar.AddComponent<
                    Image
                >();

            barImage.color =
                new Color(
                    0.025f,
                    0.045f,
                    0.09f,
                    0.96f
                );

            Text logo =
                CreateText(
                    topBar.transform,
                    "Logo",
                    "JUMP IT!",
                    32,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                logo.rectTransform,
                new Vector2(
                    0f,
                    0.5f
                ),
                new Vector2(
                    34f,
                    0f
                ),
                new Vector2(
                    230f,
                    72f
                ),
                new Vector2(
                    0f,
                    0.5f
                )
            );

            logo.color =
                Color.white;

            return
                topBar.transform;
        }

        private static GameObject
            BuildHomePanel(
                Transform parent
            )
        {
            GameObject panel =
                CreatePanel(
                    parent,
                    "HomePanel"
                );

            Text title =
                CreateText(
                    panel.transform,
                    "HomeTitle",
                    "HOME",
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
                    -125f
                ),
                new Vector2(
                    500f,
                    80f
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
                    panel.transform,
                    "HomeSubtitle",
                    "WELCOME TO JUMP IT!",
                    24,
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
                    -190f
                ),
                new Vector2(
                    520f,
                    54f
                ),
                new Vector2(
                    0f,
                    1f
                )
            );

            subtitle.color =
                new Color(
                    0.72f,
                    0.80f,
                    0.90f,
                    1f
                );

            Text hint =
                CreateText(
                    panel.transform,
                    "CharacterPreviewHint",
                    "MY CHARACTER",
                    18,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                hint.rectTransform,
                new Vector2(
                    0.5f,
                    0f
                ),
                new Vector2(
                    0f,
                    62f
                ),
                new Vector2(
                    420f,
                    42f
                ),
                new Vector2(
                    0.5f,
                    0f
                )
            );

            hint.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.72f
                );

            return
                panel;
        }

        private static GameObject
            BuildPlaceholderPanel(
                Transform parent,
                string objectName,
                string titleValue,
                string message,
                bool placeMessageLeft
            )
        {
            GameObject panel =
                CreatePanel(
                    parent,
                    objectName
                );

            Text title =
                CreateText(
                    panel.transform,
                    "Title",
                    titleValue,
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
                    -125f
                ),
                new Vector2(
                    700f,
                    80f
                ),
                new Vector2(
                    0f,
                    1f
                )
            );

            title.color =
                Color.white;

            Text body =
                CreateText(
                    panel.transform,
                    "Description",
                    message,
                    23,
                    placeMessageLeft
                        ? TextAnchor.UpperLeft
                        : TextAnchor.MiddleCenter,
                    FontStyle.Normal
                );

            if (placeMessageLeft)
            {
                SetAnchoredRect(
                    body.rectTransform,
                    new Vector2(
                        0f,
                        1f
                    ),
                    new Vector2(
                        74f,
                        -235f
                    ),
                    new Vector2(
                        600f,
                        180f
                    ),
                    new Vector2(
                        0f,
                        1f
                    )
                );
            }
            else
            {
                SetAnchoredRect(
                    body.rectTransform,
                    new Vector2(
                        0.5f,
                        0.5f
                    ),
                    Vector2.zero,
                    new Vector2(
                        1100f,
                        260f
                    ),
                    new Vector2(
                        0.5f,
                        0.5f
                    )
                );
            }

            body.color =
                new Color(
                    0.82f,
                    0.87f,
                    0.94f,
                    1f
                );

            panel.SetActive(
                false
            );

            return
                panel;
        }

        private static void
            CreateNavigationButton(
                Transform parent,
                string objectName,
                string labelValue,
                float centerX,
                float width,
                out Button button,
                out Text label,
                out Image selectedBar
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

            rect.anchorMin =
                new Vector2(
                    0f,
                    0.5f
                );

            rect.anchorMax =
                new Vector2(
                    0f,
                    0.5f
                );

            rect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.anchoredPosition =
                new Vector2(
                    centerX,
                    0f
                );

            rect.sizeDelta =
                new Vector2(
                    width,
                    86f
                );

            Image background =
                buttonObject.AddComponent<
                    Image
                >();

            background.color =
                new Color(
                    0.04f,
                    0.07f,
                    0.12f,
                    0.86f
                );

            button =
                buttonObject.AddComponent<
                    Button
                >();

            ColorBlock colors =
                button.colors;

            colors.normalColor =
                Color.white;

            colors.highlightedColor =
                new Color(
                    1f,
                    1f,
                    1f,
                    1f
                );

            colors.pressedColor =
                new Color(
                    0.72f,
                    0.92f,
                    1f,
                    1f
                );

            colors.selectedColor =
                Color.white;

            button.colors =
                colors;

            label =
                CreateText(
                    buttonObject.transform,
                    "Label",
                    labelValue,
                    20,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            StretchFull(
                label.rectTransform
            );

            label.color =
                new Color(
                    0.82f,
                    0.86f,
                    0.92f,
                    1f
                );

            GameObject barObject =
                CreateUiObject(
                    "SelectedBar",
                    buttonObject.transform
                );

            RectTransform barRect =
                barObject.GetComponent<
                    RectTransform
                >();

            barRect.anchorMin =
                new Vector2(
                    0f,
                    0f
                );

            barRect.anchorMax =
                new Vector2(
                    1f,
                    0f
                );

            barRect.pivot =
                new Vector2(
                    0.5f,
                    0f
                );

            barRect.anchoredPosition =
                Vector2.zero;

            barRect.sizeDelta =
                new Vector2(
                    0f,
                    5f
                );

            selectedBar =
                barObject.AddComponent<
                    Image
                >();

            selectedBar.color =
                SelectedColor;

            selectedBar.raycastTarget =
                false;

            selectedBar.gameObject.SetActive(
                false
            );
        }

        private static void
            BuildControllerRoot(
                ProjectJMainMenuController
                    controller
            )
        {
            GameObject root =
                new GameObject(
                    "=== MENU SYSTEM ==="
                );

            controller.transform.SetParent(
                root.transform,
                false
            );
        }

        private static void
            BuildEventSystemRoot()
        {
            EventSystem eventSystem =
                Object.FindFirstObjectByType<
                    EventSystem
                >(
                    FindObjectsInactive.Include
                );

            if (eventSystem == null)
            {
                Debug.LogWarning(
                    "[Project J/Day86] 기존 EventSystem을 찾지 못했습니다. " +
                    "현재 프로젝트의 Input System용 EventSystem을 직접 추가해주세요."
                );

                return;
            }

            GameObject root =
                EnsureRoot(
                    "=== EVENT SYSTEM ==="
                );

            eventSystem.transform.SetParent(
                root.transform,
                false
            );
        }

        private static Material
            GetOrCreatePreviewMaterial()
        {
            EnsureFolder(
                "Assets/ProjectJ/Art",
                "UI"
            );

            EnsureFolder(
                "Assets/ProjectJ/Art/UI",
                "MainMenu"
            );

            Material material =
                AssetDatabase.LoadAssetAtPath<
                    Material
                >(
                    PreviewMaterialPath
                );

            if (material != null)
            {
                return material;
            }

            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/Lit"
                );

            if (shader == null)
            {
                shader =
                    Shader.Find(
                        "Standard"
                    );
            }

            material =
                new Material(
                    shader
                );

            material.color =
                new Color(
                    0.18f,
                    0.72f,
                    0.92f,
                    1f
                );

            AssetDatabase.CreateAsset(
                material,
                PreviewMaterialPath
            );

            return material;
        }

        private static void EnsureFolder(
            string parent,
            string child
        )
        {
            string fullPath =
                parent +
                "/" +
                child;

            if (
                AssetDatabase.IsValidFolder(
                    fullPath
                )
            )
            {
                return;
            }

            AssetDatabase.CreateFolder(
                parent,
                child
            );
        }

        private static void ApplyMaterial(
            GameObject target,
            Material material
        )
        {
            Renderer renderer =
                target.GetComponent<
                    Renderer
                >();

            if (renderer != null)
            {
                renderer.sharedMaterial =
                    material;
            }
        }

        private static void RemoveCollider(
            GameObject target
        )
        {
            Collider collider =
                target.GetComponent<
                    Collider
                >();

            if (collider != null)
            {
                Object.DestroyImmediate(
                    collider
                );
            }
        }

        private static GameObject
            CreatePanel(
                Transform parent,
                string objectName
            )
        {
            GameObject panel =
                CreateUiObject(
                    objectName,
                    parent
                );

            StretchFull(
                panel.GetComponent<
                    RectTransform
                >()
            );

            return panel;
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
                fontStyle;

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

        private static GameObject EnsureRoot(
            string rootName
        )
        {
            GameObject root =
                GameObject.Find(
                    rootName
                );

            if (root == null)
            {
                root =
                    new GameObject(
                        rootName
                    );
            }

            return root;
        }

        private static void DestroyByName(
            string objectName
        )
        {
            GameObject target =
                GameObject.Find(
                    objectName
                );

            if (target != null)
            {
                Object.DestroyImmediate(
                    target
                );
            }
        }
    }
}
