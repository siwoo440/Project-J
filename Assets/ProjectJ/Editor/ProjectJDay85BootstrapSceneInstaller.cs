using ProjectJ.Networking.Fusion; // 85일차 Status View 참조
using UnityEditor; // MenuItem와 AssetDatabase 사용
using UnityEditor.SceneManagement; // Bootstrap Scene 편집
using UnityEngine; // GameObject와 RectTransform 사용
using UnityEngine.UI; // Canvas와 UI 컴포넌트 사용

namespace ProjectJ.Editor
{
    internal static class
        ProjectJDay85BootstrapSceneInstaller
    {
        private const string MenuPath =
            "Project J/Scene/85일차 Bootstrap Scene 구성";

        private const string BootstrapScenePath =
            "Assets/ProjectJ/Scenes/Bootstrap.unity";

        private const string BackgroundFolderPath =
            "Assets/ProjectJ/Art/UI/Bootstrap";

        private static readonly string[]
            BackgroundCandidatePaths =
            {
                BackgroundFolderPath +
                "/BootstrapBackground.png",

                BackgroundFolderPath +
                "/BootstrapBackground.jpg",

                BackgroundFolderPath +
                "/BootstrapBackground.jpeg"
            };

        [MenuItem(MenuPath)]
        private static void ConfigureBootstrapScene()
        {
            if (
                !EditorSceneManager
                    .SaveCurrentModifiedScenesIfUserWantsTo()
            )
            {
                return;
            }

            EnsureBackgroundFolder();

            UnityEngine.SceneManagement.Scene scene =
                EditorSceneManager.OpenScene(
                    BootstrapScenePath,
                    OpenSceneMode.Single
                );

            RemoveLegacySceneFlow();
            OrganizeBaseScene();
            BuildBootstrapUi();
            BuildSceneFlowRoot();

            EditorSceneManager.MarkSceneDirty(
                scene
            );

            EditorSceneManager.SaveScene(
                scene
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "[Project J/Day85] Bootstrap Scene 구성을 완료했습니다."
            );
        }

        private static void EnsureBackgroundFolder()
        {
            EnsureFolder(
                "Assets/ProjectJ/Art",
                "UI"
            );

            EnsureFolder(
                "Assets/ProjectJ/Art/UI",
                "Bootstrap"
            );
        }

        private static void EnsureFolder(
            string parent,
            string child
        )
        {
            string path =
                parent +
                "/" +
                child;

            if (
                AssetDatabase.IsValidFolder(
                    path
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

        private static void RemoveLegacySceneFlow()
        {
            BootstrapSceneController[]
                legacyControllers =
                    Object.FindObjectsByType<
                        BootstrapSceneController
                    >(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None
                    );

            foreach (
                BootstrapSceneController controller
                in legacyControllers
            )
            {
                Object.DestroyImmediate(
                    controller
                );
            }

            GameObject oldFlow =
                GameObject.Find(
                    "Day3_SceneFlow"
                );

            if (oldFlow != null)
            {
                Object.DestroyImmediate(
                    oldFlow
                );
            }
        }

        private static void OrganizeBaseScene()
        {
            GameObject cameraRoot =
                RecreateRoot(
                    "=== CAMERA ==="
                );

            GameObject environmentRoot =
                RecreateRoot(
                    "=== ENVIRONMENT ==="
                );

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

                cameraObject.transform.position =
                    new Vector3(
                        0f,
                        1f,
                        -10f
                    );
            }

            mainCamera.transform.SetParent(
                cameraRoot.transform,
                true
            );

            GameObject volume =
                GameObject.Find(
                    "Global Volume"
                );

            if (volume != null)
            {
                volume.transform.SetParent(
                    environmentRoot.transform,
                    true
                );
            }

            GameObject directionalLight =
                GameObject.Find(
                    "Directional Light"
                );

            if (directionalLight != null)
            {
                Object.DestroyImmediate(
                    directionalLight
                );
            }
        }

        private static void BuildBootstrapUi()
        {
            DestroyRoot(
                "=== UI ==="
            );

            GameObject uiRoot =
                new GameObject(
                    "=== UI ==="
                );

            GameObject canvasObject =
                new GameObject(
                    "Canvas_Bootstrap",
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

            GameObject backgroundViewport =
                CreateUiObject(
                    "BackgroundViewport",
                    canvasObject.transform
                );

            StretchFull(
                backgroundViewport
                    .GetComponent<RectTransform>()
            );

            GameObject backgroundObject =
                CreateUiObject(
                    "Background",
                    backgroundViewport.transform
                );

            RawImage backgroundImage =
                backgroundObject.AddComponent<
                    RawImage
                >();

            backgroundImage.raycastTarget =
                false;

            RectTransform backgroundRect =
                backgroundObject
                    .GetComponent<RectTransform>();

            backgroundRect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            backgroundRect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            backgroundRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            backgroundRect.anchoredPosition =
                Vector2.zero;

            backgroundRect.sizeDelta =
                new Vector2(
                    1920f,
                    1080f
                );

            Texture2D backgroundTexture =
                LoadBackgroundTexture();

            if (backgroundTexture != null)
            {
                backgroundImage.texture =
                    backgroundTexture;

                AspectRatioFitter fitter =
                    backgroundObject.AddComponent<
                        AspectRatioFitter
                    >();

                fitter.aspectMode =
                    AspectRatioFitter.AspectMode
                        .EnvelopeParent;

                fitter.aspectRatio =
                    backgroundTexture.width /
                    (float) backgroundTexture.height;

                backgroundImage.color =
                    Color.white;
            }
            else
            {
                backgroundImage.color =
                    new Color(
                        0.07f,
                        0.10f,
                        0.18f,
                        1f
                    );

                Debug.LogWarning(
                    "[Project J/Day85] BootstrapBackground 이미지를 찾지 못했습니다. " +
                    BackgroundFolderPath +
                    " 폴더에 BootstrapBackground.png 또는 .jpg를 넣고 메뉴를 다시 실행하세요."
                );
            }

            GameObject dimObject =
                CreateUiObject(
                    "DimOverlay",
                    canvasObject.transform
                );

            StretchFull(
                dimObject
                    .GetComponent<RectTransform>()
            );

            Image dimImage =
                dimObject.AddComponent<
                    Image
                >();

            dimImage.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.24f
                );

            dimImage.raycastTarget =
                false;

            GameObject contentRoot =
                CreateUiObject(
                    "ContentRoot",
                    canvasObject.transform
                );

            StretchFull(
                contentRoot
                    .GetComponent<RectTransform>()
            );

            Text titleText =
                CreateText(
                    contentRoot.transform,
                    "TitleText",
                    "JUMP IT!",
                    74,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                titleText.rectTransform,
                new Vector2(
                    0.5f,
                    0.5f
                ),
                new Vector2(
                    0f,
                    170f
                ),
                new Vector2(
                    900f,
                    120f
                )
            );

            titleText.color =
                Color.white;

            Text loadingDots =
                CreateText(
                    contentRoot.transform,
                    "LoadingDots",
                    "○  ○  ○",
                    26,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                loadingDots.rectTransform,
                new Vector2(
                    0.5f,
                    0.5f
                ),
                new Vector2(
                    0f,
                    -5f
                ),
                new Vector2(
                    500f,
                    50f
                )
            );

            loadingDots.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.92f
                );

            Text statusText =
                CreateText(
                    contentRoot.transform,
                    "StatusText",
                    "시스템 준비 중...",
                    28,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold
                );

            SetAnchoredRect(
                statusText.rectTransform,
                new Vector2(
                    0.5f,
                    0.5f
                ),
                new Vector2(
                    0f,
                    -74f
                ),
                new Vector2(
                    1200f,
                    52f
                )
            );

            statusText.color =
                Color.white;

            Text detailText =
                CreateText(
                    contentRoot.transform,
                    "DetailText",
                    "Steam / Fusion 초기화 상태를 확인합니다.",
                    18,
                    TextAnchor.MiddleCenter,
                    FontStyle.Normal
                );

            SetAnchoredRect(
                detailText.rectTransform,
                new Vector2(
                    0.5f,
                    0.5f
                ),
                new Vector2(
                    0f,
                    -122f
                ),
                new Vector2(
                    1500f,
                    44f
                )
            );

            detailText.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.82f
                );

            Text versionText =
                CreateText(
                    contentRoot.transform,
                    "VersionText",
                    "DEV  •  DAY 85",
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
                    -32f,
                    24f
                );

            versionRect.sizeDelta =
                new Vector2(
                    420f,
                    36f
                );

            versionText.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.65f
                );

            BuildStatusView(
                loadingDots,
                statusText,
                detailText,
                versionText
            );

            Selection.activeGameObject =
                canvasObject;

            EditorGUIUtility.PingObject(
                canvasObject
            );
        }

        private static void BuildSceneFlowRoot()
        {
            GameObject root =
                GameObject.Find(
                    "=== SCENE FLOW ==="
                );

            if (root == null)
            {
                root =
                    new GameObject(
                        "=== SCENE FLOW ==="
                    );
            }

            ProjectJDay85BootstrapStatusView
                statusView =
                    Object.FindFirstObjectByType<
                        ProjectJDay85BootstrapStatusView
                    >();

            if (statusView != null)
            {
                statusView.transform.SetParent(
                    root.transform,
                    true
                );
            }
        }

        private static void BuildStatusView(
            Text loadingDots,
            Text statusText,
            Text detailText,
            Text versionText
        )
        {
            GameObject viewObject =
                new GameObject(
                    "BootstrapStatusView"
                );

            ProjectJDay85BootstrapStatusView
                statusView =
                    viewObject.AddComponent<
                        ProjectJDay85BootstrapStatusView
                    >();

            SerializedObject serializedView =
                new SerializedObject(
                    statusView
                );

            serializedView
                .FindProperty(
                    "loadingDotsText"
                )
                .objectReferenceValue =
                    loadingDots;

            serializedView
                .FindProperty(
                    "statusText"
                )
                .objectReferenceValue =
                    statusText;

            serializedView
                .FindProperty(
                    "detailText"
                )
                .objectReferenceValue =
                    detailText;

            serializedView
                .FindProperty(
                    "versionText"
                )
                .objectReferenceValue =
                    versionText;

            serializedView
                .ApplyModifiedPropertiesWithoutUndo();
        }

        private static Texture2D
            LoadBackgroundTexture()
        {
            foreach (
                string path
                in BackgroundCandidatePaths
            )
            {
                Texture2D texture =
                    AssetDatabase.LoadAssetAtPath<
                        Texture2D
                    >(
                        path
                    );

                if (texture != null)
                {
                    return texture;
                }
            }

            return null;
        }

        private static GameObject
            RecreateRoot(
                string rootName
            )
        {
            GameObject existing =
                GameObject.Find(
                    rootName
                );

            if (existing != null)
            {
                return existing;
            }

            return
                new GameObject(
                    rootName
                );
        }

        private static void DestroyRoot(
            string rootName
        )
        {
            GameObject existing =
                GameObject.Find(
                    rootName
                );

            if (existing != null)
            {
                Object.DestroyImmediate(
                    existing
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

        private static void StretchFull(
            RectTransform rectTransform
        )
        {
            rectTransform.anchorMin =
                Vector2.zero;

            rectTransform.anchorMax =
                Vector2.one;

            rectTransform.offsetMin =
                Vector2.zero;

            rectTransform.offsetMax =
                Vector2.zero;
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

        private static void SetAnchoredRect(
            RectTransform rectTransform,
            Vector2 anchor,
            Vector2 position,
            Vector2 size
        )
        {
            rectTransform.anchorMin =
                anchor;

            rectTransform.anchorMax =
                anchor;

            rectTransform.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rectTransform.anchoredPosition =
                position;

            rectTransform.sizeDelta =
                size;
        }
    }
}
