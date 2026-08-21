using ProjectJ.Push;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjectJ.Editor
{
    public static class Day43PushFeedbackSetup
    {
        private const string PlayerPrefabPath =
            "Assets/ProjectJ/Prefabs/Player/" +
            "Player.prefab";

        private const string FeedbackRootName =
            "=== Push Feedback UI ===";

        private const string SwingRootName =
            "PushSwingVfx";

        private const string MaterialFolder =
            "Assets/ProjectJ/Tests/Manual/Phase4/" +
            "Materials";

        private const string SwingMaterialPath =
            MaterialFolder +
            "/Day43_PushSwing.mat";

        [MenuItem(
            "ProjectJ/Day43/Setup Push Feedback UI"
        )]
        public static void SetupPushFeedbackUI()
        {
            GameObject prefabRoot =
                PrefabUtility.LoadPrefabContents(
                    PlayerPrefabPath
                );

            if (prefabRoot == null)
            {
                Debug.LogError(
                    "Player.prefab을 찾을 수 없습니다: " +
                    PlayerPrefabPath
                );

                return;
            }

            try
            {
                PlayerInput playerInput =
                    prefabRoot.GetComponent<
                        PlayerInput
                    >();

                PlayerPushController
                    pushController =
                        prefabRoot.GetComponent<
                            PlayerPushController
                        >();

                PlayerPushReceiver pushReceiver =
                    prefabRoot.GetComponent<
                        PlayerPushReceiver
                    >();

                PlayerPushTargetSelector
                    targetSelector =
                        prefabRoot.GetComponent<
                            PlayerPushTargetSelector
                        >();

                if (
                    playerInput == null ||
                    pushController == null ||
                    pushReceiver == null ||
                    targetSelector == null
                )
                {
                    Debug.LogError(
                        "Day43 설정에 필요한 PlayerInput, " +
                        "PlayerPushController, PlayerPushReceiver, " +
                        "PlayerPushTargetSelector를 확인해주세요."
                    );

                    return;
                }

                Transform oldFeedbackRoot =
                    prefabRoot.transform.Find(
                        FeedbackRootName
                    );

                if (oldFeedbackRoot != null)
                {
                    Object.DestroyImmediate(
                        oldFeedbackRoot.gameObject
                    );
                }

                Transform oldSwingRoot =
                    prefabRoot.transform.Find(
                        SwingRootName
                    );

                if (oldSwingRoot != null)
                {
                    Object.DestroyImmediate(
                        oldSwingRoot.gameObject
                    );
                }

                Canvas canvas =
                    CreateCanvas(
                        prefabRoot.transform
                    );

                Text judgmentText =
                    CreateJudgmentText(
                        canvas.transform
                    );

                LineRenderer swingLine =
                    CreateSwingLine(
                        prefabRoot.transform
                    );

                PlayerPushFeedbackUI feedback =
                    prefabRoot.GetComponent<
                        PlayerPushFeedbackUI
                    >();

                if (feedback == null)
                {
                    feedback =
                        prefabRoot.AddComponent<
                            PlayerPushFeedbackUI
                        >();
                }

                feedback.Configure(
                    playerInput,
                    pushController,
                    pushReceiver,
                    targetSelector,
                    canvas,
                    judgmentText,
                    swingLine
                );

                EditorUtility.SetDirty(
                    feedback
                );

                PrefabUtility.SaveAsPrefabAsset(
                    prefabRoot,
                    PlayerPrefabPath
                );
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(
                    prefabRoot
                );
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Day43 Push Feedback UI 설정 완료. " +
                "Player UI에 HIT/MISS/COOLDOWN/PROTECTED " +
                "임시 판정 Text와 밀치기 범위 VFX를 추가했습니다."
            );
        }

        private static Canvas CreateCanvas(
            Transform parent
        )
        {
            GameObject canvasObject =
                new GameObject(
                    FeedbackRootName,
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler)
                );

            canvasObject.transform.SetParent(
                parent,
                false
            );

            Canvas canvas =
                canvasObject.GetComponent<
                    Canvas
                >();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder =
                500;

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

            scaler.matchWidthOrHeight =
                0.5f;

            return canvas;
        }

        private static Text CreateJudgmentText(
            Transform parent
        )
        {
            GameObject textObject =
                new GameObject(
                    "PushJudgmentText",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Text),
                    typeof(Outline)
                );

            textObject.transform.SetParent(
                parent,
                false
            );

            RectTransform rect =
                textObject.GetComponent<
                    RectTransform
                >();

            rect.anchorMin =
                new Vector2(
                    0.5f,
                    1f
                );

            rect.anchorMax =
                new Vector2(
                    0.5f,
                    1f
                );

            rect.pivot =
                new Vector2(
                    0.5f,
                    1f
                );

            rect.anchoredPosition =
                new Vector2(
                    0f,
                    -80f
                );

            rect.sizeDelta =
                new Vector2(
                    720f,
                    80f
                );

            Text text =
                textObject.GetComponent<Text>();

            text.font =
                Resources.GetBuiltinResource<
                    Font
                >(
                    "LegacyRuntime.ttf"
                );

            text.fontSize =
                34;

            text.alignment =
                TextAnchor.MiddleCenter;

            text.horizontalOverflow =
                HorizontalWrapMode.Overflow;

            text.verticalOverflow =
                VerticalWrapMode.Overflow;

            text.raycastTarget =
                false;

            text.text =
                "PUSH READY";

            Outline outline =
                textObject.GetComponent<
                    Outline
                >();

            outline.effectDistance =
                new Vector2(
                    2f,
                    -2f
                );

            return text;
        }

        private static LineRenderer CreateSwingLine(
            Transform parent
        )
        {
            GameObject swingObject =
                new GameObject(
                    SwingRootName
                );

            swingObject.transform.SetParent(
                parent,
                false
            );

            LineRenderer line =
                swingObject.AddComponent<
                    LineRenderer
                >();

            line.useWorldSpace =
                false;

            line.widthMultiplier =
                0.08f;

            line.numCornerVertices =
                2;

            line.numCapVertices =
                2;

            line.loop =
                false;

            line.enabled =
                false;

            Material material =
                LoadOrCreateSwingMaterial();

            if (material != null)
            {
                line.sharedMaterial =
                    material;
            }

            return line;
        }

        private static Material
            LoadOrCreateSwingMaterial()
        {
            EnsureFolder(
                MaterialFolder
            );

            Material material =
                AssetDatabase.LoadAssetAtPath<
                    Material
                >(
                    SwingMaterialPath
                );

            if (material != null)
            {
                return material;
            }

            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/Unlit"
                );

            if (shader == null)
            {
                shader =
                    Shader.Find(
                        "Sprites/Default"
                    );
            }

            if (shader == null)
            {
                return null;
            }

            material =
                new Material(
                    shader
                );

            AssetDatabase.CreateAsset(
                material,
                SwingMaterialPath
            );

            return material;
        }

        private static void EnsureFolder(
            string folderPath
        )
        {
            string[] parts =
                folderPath.Split(
                    '/'
                );

            string current =
                parts[0];

            for (
                int i = 1;
                i < parts.Length;
                i++
            )
            {
                string next =
                    current +
                    "/" +
                    parts[i];

                if (
                    !AssetDatabase.IsValidFolder(
                        next
                    )
                )
                {
                    AssetDatabase.CreateFolder(
                        current,
                        parts[i]
                    );
                }

                current =
                    next;
            }
        }
    }
}
