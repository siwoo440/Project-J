using System.Collections;
using ProjectJ.Items;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectJ.UI
{
    [DisallowMultipleComponent]
    public sealed class ItemUseFeedbackCanvasView :
        MonoBehaviour
    {
        private const float MessageDuration =
            1.6f;

        private PlayerItemUseController
            useController;

        private Text messageText;
        private Coroutine hideRoutine;

        public static ItemUseFeedbackCanvasView
            Create(
                Transform parent
            )
        {
            GameObject canvasObject =
                new GameObject(
                    "=== Item Use Feedback Canvas ==="
                );

            canvasObject.transform.SetParent(
                parent,
                false
            );

            Canvas canvas =
                canvasObject.AddComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 82;

            CanvasScaler scaler =
                canvasObject.AddComponent<
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

            ItemUseFeedbackCanvasView view =
                canvasObject.AddComponent<
                    ItemUseFeedbackCanvasView
                >();

            view.BuildUi();

            return view;
        }

        public void Bind(
            PlayerItemUseController
                newUseController
        )
        {
            if (useController != null)
            {
                useController.UseCompleted -=
                    HandleUseCompleted;
            }

            useController =
                newUseController;

            if (useController != null)
            {
                useController.UseCompleted +=
                    HandleUseCompleted;
            }

            HideMessage();
        }

        private void OnDestroy()
        {
            if (useController != null)
            {
                useController.UseCompleted -=
                    HandleUseCompleted;
            }
        }

        private void BuildUi()
        {
            Font font =
                Resources.GetBuiltinResource<
                    Font
                >(
                    "LegacyRuntime.ttf"
                );

            GameObject textObject =
                new GameObject(
                    "ItemUseFailureMessage",
                    typeof(RectTransform)
                );

            textObject.transform.SetParent(
                transform,
                false
            );

            RectTransform rect =
                textObject.GetComponent<
                    RectTransform
                >();

            rect.anchorMin =
                new Vector2(
                    0.5f,
                    0f
                );

            rect.anchorMax =
                new Vector2(
                    0.5f,
                    0f
                );

            rect.pivot =
                new Vector2(
                    0.5f,
                    0f
                );

            rect.anchoredPosition =
                new Vector2(
                    0f,
                    72f
                );

            rect.sizeDelta =
                new Vector2(
                    760f,
                    52f
                );

            messageText =
                textObject.AddComponent<Text>();

            messageText.font = font;
            messageText.fontSize = 28;
            messageText.fontStyle =
                FontStyle.Bold;
            messageText.alignment =
                TextAnchor.MiddleCenter;
            messageText.color =
                new Color(
                    1f,
                    0.2f,
                    0.2f,
                    1f
                );
            messageText.raycastTarget =
                false;
            messageText.resizeTextForBestFit =
                true;
            messageText.resizeTextMinSize =
                18;
            messageText.resizeTextMaxSize =
                28;

            Outline outline =
                textObject.AddComponent<
                    Outline
                >();

            outline.effectColor =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.9f
                );

            outline.effectDistance =
                new Vector2(
                    2f,
                    -2f
                );

            textObject.SetActive(false);
        }

        private void HandleUseCompleted(
            ItemUseResult result
        )
        {
            if (
                result.Status !=
                ItemUseStatus.InvalidPosition
            )
            {
                return;
            }

            ShowMessage(
                "해당 위치는 설치할 수 없습니다."
            );
        }

        private void ShowMessage(
            string message
        )
        {
            if (messageText == null)
            {
                return;
            }

            if (hideRoutine != null)
            {
                StopCoroutine(
                    hideRoutine
                );
            }

            messageText.text = message;
            messageText.gameObject
                .SetActive(true);

            hideRoutine =
                StartCoroutine(
                    HideAfterDelay()
                );
        }

        private IEnumerator HideAfterDelay()
        {
            yield return
                new WaitForSecondsRealtime(
                    MessageDuration
                );

            hideRoutine = null;

            if (messageText != null)
            {
                messageText.text =
                    string.Empty;

                messageText.gameObject
                    .SetActive(false);
            }
        }

        private void HideMessage()
        {
            if (hideRoutine != null)
            {
                StopCoroutine(
                    hideRoutine
                );

                hideRoutine = null;
            }

            if (messageText != null)
            {
                messageText.text =
                    string.Empty;

                messageText.gameObject
                    .SetActive(false);
            }
        }
    }
}
