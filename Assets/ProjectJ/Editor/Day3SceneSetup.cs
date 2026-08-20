using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectJ.Editor
{
    public static class Day3SceneSetup
    {
        private const string SceneFolder = "Assets/ProjectJ/Scenes";
        private const string BootstrapPath = SceneFolder + "/Bootstrap.unity";
        private const string MainMenuPath = SceneFolder + "/MainMenu.unity";
        private const string GamePath = SceneFolder + "/Game.unity";

        private static int setupStep;
        private static bool isSettingUp;

        [MenuItem("Project J/Day 3/Scene Flow 구성")]
        public static void ConfigureSceneFlow()
        {
            if (isSettingUp)
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    "Project J - 3일차",
                    "Play Mode를 종료한 뒤 다시 실행해주세요.",
                    "확인"
                );
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EnsureSceneFolder();

            Selection.activeObject = null;
            setupStep = 0;
            isSettingUp = true;

            EditorApplication.delayCall += RunNextStep;
        }

        private static void RunNextStep()
        {
            try
            {
                Selection.activeObject = null;

                switch (setupStep)
                {
                    case 0:
                        ConfigureBootstrapScene();
                        break;

                    case 1:
                        ConfigureMainMenuScene();
                        break;

                    case 2:
                        ConfigureGameScene();
                        break;

                    case 3:
                        ConfigureBuildSettings();
                        break;

                    case 4:
                        EditorSceneManager.OpenScene(BootstrapPath, OpenSceneMode.Single);
                        break;

                    default:
                        FinishSetup();
                        return;
                }

                setupStep++;
                EditorApplication.RepaintHierarchyWindow();
                EditorApplication.delayCall += RunNextStep;
            }
            catch (System.Exception exception)
            {
                isSettingUp = false;
                Debug.LogException(exception);

                EditorUtility.DisplayDialog(
                    "Project J - 3일차",
                    "Scene Flow 구성 중 오류가 발생했습니다. Console을 확인해주세요.",
                    "확인"
                );
            }
        }

        private static void FinishSetup()
        {
            isSettingUp = false;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorApplication.RepaintHierarchyWindow();

            EditorUtility.DisplayDialog(
                "Project J - 3일차",
                "Bootstrap → MainMenu → Game → MainMenu Scene Flow 구성이 완료되었습니다.",
                "확인"
            );
        }

        private static void ConfigureBootstrapScene()
        {
            Scene scene = OpenOrCreateScene(BootstrapPath);

            GameObject sceneFlow = GetOrCreateRoot("Day3_SceneFlow");

            if (sceneFlow.GetComponent<BootstrapSceneController>() == null)
            {
                sceneFlow.AddComponent<BootstrapSceneController>();
            }

            SaveScene(scene, BootstrapPath);
        }

        private static void ConfigureMainMenuScene()
        {
            Scene scene = OpenOrCreateScene(MainMenuPath);

            SceneNavigator navigator = GetOrCreateNavigator();

            Canvas canvas = GetOrCreateCanvas("UI_MainMenu");
            GetOrCreateBackground(canvas.transform);

            Text title = GetOrCreateText(
                canvas.transform,
                "Title",
                "JUMP IT!",
                72,
                TextAnchor.MiddleCenter
            );

            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0f, 150f);
            titleRect.sizeDelta = new Vector2(900f, 120f);

            Button startButton = GetOrCreateButton(
                canvas.transform,
                "StartButton",
                "START",
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -20f),
                new Vector2(360f, 90f)
            );

            startButton.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddPersistentListener(startButton.onClick, navigator.OpenGame);

            EnsureEventSystem();

            SaveScene(scene, MainMenuPath);
        }

        private static void ConfigureGameScene()
        {
            Scene scene = OpenOrCreateScene(GamePath);

            SceneNavigator navigator = GetOrCreateNavigator();

            Canvas canvas = GetOrCreateCanvas("UI_Game");

            Button menuButton = GetOrCreateButton(
                canvas.transform,
                "MainMenuButton",
                "BACK TO MENU",
                new Vector2(0f, 1f),
                new Vector2(30f, -30f),
                new Vector2(300f, 70f)
            );

            RectTransform menuButtonRect = menuButton.GetComponent<RectTransform>();
            menuButtonRect.pivot = new Vector2(0f, 1f);

            menuButton.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddPersistentListener(menuButton.onClick, navigator.OpenMainMenu);

            EnsureEventSystem();

            SaveScene(scene, GamePath);
        }

        private static SceneNavigator GetOrCreateNavigator()
        {
            GameObject navigationObject = GameObject.Find("SceneNavigation");

            if (navigationObject == null)
            {
                navigationObject = new GameObject("SceneNavigation");
            }

            SceneNavigator navigator = navigationObject.GetComponent<SceneNavigator>();

            if (navigator == null)
            {
                navigator = navigationObject.AddComponent<SceneNavigator>();
            }

            return navigator;
        }

        private static Canvas GetOrCreateCanvas(string name)
        {
            GameObject canvasObject = GameObject.Find(name);

            if (canvasObject == null)
            {
                canvasObject = new GameObject(name, typeof(RectTransform));
            }

            Canvas canvas = GetOrAddComponent<Canvas>(canvasObject);
            CanvasScaler scaler = GetOrAddComponent<CanvasScaler>(canvasObject);
            GetOrAddComponent<GraphicRaycaster>(canvasObject);
            GetOrAddComponent<RuntimeUIFontBinder>(canvasObject);

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static void GetOrCreateBackground(Transform parent)
        {
            Transform existing = parent.Find("Background");
            GameObject backgroundObject;

            if (existing == null)
            {
                backgroundObject = new GameObject(
                    "Background",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                );

                backgroundObject.transform.SetParent(parent, false);
            }
            else
            {
                backgroundObject = existing.gameObject;
            }

            RectTransform rect = GetOrAddComponent<RectTransform>(backgroundObject);
            Image image = GetOrAddComponent<Image>(backgroundObject);

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            image.color = new Color(0.08f, 0.09f, 0.12f, 1f);
            image.raycastTarget = false;
        }

        private static Button GetOrCreateButton(
            Transform parent,
            string name,
            string label,
            Vector2 anchor,
            Vector2 anchoredPosition,
            Vector2 size
        )
        {
            Transform existing = parent.Find(name);
            GameObject buttonObject;

            if (existing == null)
            {
                buttonObject = new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(Button)
                );

                buttonObject.transform.SetParent(parent, false);
            }
            else
            {
                buttonObject = existing.gameObject;
            }

            RectTransform rect = GetOrAddComponent<RectTransform>(buttonObject);
            Image image = GetOrAddComponent<Image>(buttonObject);
            Button button = GetOrAddComponent<Button>(buttonObject);

            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            image.color = new Color(0.20f, 0.23f, 0.30f, 1f);

            Text text = GetOrCreateText(
                buttonObject.transform,
                "Label",
                label,
                32,
                TextAnchor.MiddleCenter
            );

            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private static Text GetOrCreateText(
            Transform parent,
            string name,
            string value,
            int fontSize,
            TextAnchor alignment
        )
        {
            Transform existing = parent.Find(name);
            GameObject textObject;

            if (existing == null)
            {
                textObject = new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Text)
                );

                textObject.transform.SetParent(parent, false);
            }
            else
            {
                textObject = existing.gameObject;
            }

            Text text = GetOrAddComponent<Text>(textObject);

            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;

            return text;
        }

        private static void EnsureEventSystem()
        {
            GameObject eventSystemObject = GameObject.Find("EventSystem");

            if (eventSystemObject == null)
            {
                eventSystemObject = new GameObject("EventSystem");
            }

            GetOrAddComponent<EventSystem>(eventSystemObject);

            StandaloneInputModule legacyModule =
                eventSystemObject.GetComponent<StandaloneInputModule>();

            if (legacyModule != null)
            {
                legacyModule.enabled = false;
            }

            GetOrAddComponent<InputSystemUIInputModule>(eventSystemObject);
        }

        private static Scene OpenOrCreateScene(string path)
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

            if (sceneAsset != null)
            {
                return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            }

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single
            );

            EditorSceneManager.SaveScene(scene, path);
            return scene;
        }

        private static GameObject GetOrCreateRoot(string objectName)
        {
            GameObject target = GameObject.Find(objectName);

            if (target == null)
            {
                target = new GameObject(objectName);
            }

            return target;
        }

        private static T GetOrAddComponent<T>(GameObject target)
            where T : Component
        {
            T component = target.GetComponent<T>();

            if (component == null)
            {
                component = target.AddComponent<T>();
            }

            return component;
        }

        private static void SaveScene(Scene scene, string path)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootstrapPath, true),
                new EditorBuildSettingsScene(MainMenuPath, true),
                new EditorBuildSettingsScene(GamePath, true)
            };
        }

        private static void EnsureSceneFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ProjectJ"))
            {
                AssetDatabase.CreateFolder("Assets", "ProjectJ");
            }

            if (!AssetDatabase.IsValidFolder(SceneFolder))
            {
                AssetDatabase.CreateFolder("Assets/ProjectJ", "Scenes");
            }
        }
    }
}
