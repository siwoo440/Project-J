using System.Collections.Generic;
using ProjectJ.Items.Status;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectJ.UI
{
    [DisallowMultipleComponent]
    public sealed class ItemStatusHudView :
        MonoBehaviour
    {
        private const int MaxRows = 4;
        private const float RowWidth = 430f;
        private const float RowHeight = 60f;
        private const float RowGap = 8f;

        private static readonly Color
            RowBackgroundColor =
                new Color(
                    0.055f,
                    0.065f,
                    0.08f,
                    0.9f
                );

        private PlayerItemStatusTracker tracker;

        private readonly List<
            PlayerItemStatusEntry
        > statuses =
            new List<PlayerItemStatusEntry>(
                MaxRows
            );

        private GameObject panelObject;

        private GameObject[] rowObjects;
        private Image[] iconImages;
        private Text[] nameTexts;
        private Text[] detailTexts;
        private Text[] stateTexts;

        public static ItemStatusHudView Create(
            Transform parent
        )
        {
            GameObject canvasObject =
                new GameObject(
                    "=== Item Status HUD Canvas ==="
                );

            canvasObject.transform.SetParent(
                parent,
                false
            );

            Canvas canvas =
                canvasObject.AddComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 81;

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

            ItemStatusHudView view =
                canvasObject.AddComponent<
                    ItemStatusHudView
                >();

            view.BuildUi();

            return view;
        }

        public void Bind(
            PlayerItemStatusTracker
                newTracker
        )
        {
            tracker = newTracker;
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void BuildUi()
        {
            rowObjects =
                new GameObject[MaxRows];

            iconImages =
                new Image[MaxRows];

            nameTexts =
                new Text[MaxRows];

            detailTexts =
                new Text[MaxRows];

            stateTexts =
                new Text[MaxRows];

            Font font =
                Resources.GetBuiltinResource<
                    Font
                >(
                    "LegacyRuntime.ttf"
                );

            panelObject =
                CreateUiObject(
                    "StatusPanel",
                    transform
                );

            RectTransform panelRect =
                panelObject.GetComponent<
                    RectTransform
                >();

            panelRect.anchorMin =
                new Vector2(
                    1f,
                    0f
                );

            panelRect.anchorMax =
                new Vector2(
                    1f,
                    0f
                );

            panelRect.pivot =
                new Vector2(
                    1f,
                    0f
                );

            panelRect.anchoredPosition =
                new Vector2(
                    -28f,
                    280f
                );

            panelRect.sizeDelta =
                new Vector2(
                    RowWidth,
                    MaxRows *
                    (RowHeight + RowGap)
                );

            for (
                int i = 0;
                i < MaxRows;
                i++
            )
            {
                CreateRow(
                    panelObject.transform,
                    font,
                    i
                );
            }

            panelObject.SetActive(false);
        }

        private void CreateRow(
            Transform parent,
            Font font,
            int index
        )
        {
            GameObject row =
                CreateUiObject(
                    "StatusRow_" +
                    (index + 1),
                    parent
                );

            RectTransform rowRect =
                row.GetComponent<
                    RectTransform
                >();

            SetTopLeftRect(
                rowRect,
                new Vector2(
                    0f,
                    -index *
                    (RowHeight + RowGap)
                ),
                new Vector2(
                    RowWidth,
                    RowHeight
                )
            );

            Image rowBackground =
                row.AddComponent<Image>();

            rowBackground.color =
                RowBackgroundColor;

            rowObjects[index] =
                row;

            GameObject iconObject =
                CreateUiObject(
                    "Icon",
                    row.transform
                );

            RectTransform iconRect =
                iconObject.GetComponent<
                    RectTransform
                >();

            SetTopLeftRect(
                iconRect,
                new Vector2(
                    8f,
                    -6f
                ),
                new Vector2(
                    48f,
                    48f
                )
            );

            Image icon =
                iconObject.AddComponent<Image>();

            icon.preserveAspect = true;
            icon.raycastTarget = false;

            iconImages[index] =
                icon;

            Text name =
                CreateText(
                    "Name",
                    row.transform,
                    font,
                    string.Empty,
                    20,
                    TextAnchor.MiddleLeft
                );

            RectTransform nameRect =
                name.GetComponent<
                    RectTransform
                >();

            SetTopLeftRect(
                nameRect,
                new Vector2(
                    68f,
                    -4f
                ),
                new Vector2(
                    245f,
                    28f
                )
            );

            name.fontStyle =
                FontStyle.Bold;

            nameTexts[index] =
                name;

            Text detail =
                CreateText(
                    "Detail",
                    row.transform,
                    font,
                    string.Empty,
                    14,
                    TextAnchor.MiddleLeft
                );

            RectTransform detailRect =
                detail.GetComponent<
                    RectTransform
                >();

            SetTopLeftRect(
                detailRect,
                new Vector2(
                    68f,
                    -31f
                ),
                new Vector2(
                    245f,
                    23f
                )
            );

            detail.color =
                new Color(
                    0.75f,
                    0.78f,
                    0.84f,
                    1f
                );

            detailTexts[index] =
                detail;

            Text state =
                CreateText(
                    "State",
                    row.transform,
                    font,
                    string.Empty,
                    20,
                    TextAnchor.MiddleCenter
                );

            RectTransform stateRect =
                state.GetComponent<
                    RectTransform
                >();

            SetTopLeftRect(
                stateRect,
                new Vector2(
                    318f,
                    -12f
                ),
                new Vector2(
                    104f,
                    36f
                )
            );

            state.fontStyle =
                FontStyle.Bold;

            stateTexts[index] =
                state;

            row.SetActive(false);
        }

        private void Refresh()
        {
            if (
                panelObject == null ||
                rowObjects == null
            )
            {
                return;
            }

            if (tracker == null)
            {
                panelObject.SetActive(false);
                return;
            }

            tracker.CollectStatuses(
                statuses
            );

            int visibleCount =
                Mathf.Min(
                    statuses.Count,
                    MaxRows
                );

            panelObject.SetActive(
                visibleCount > 0
            );

            for (
                int i = 0;
                i < MaxRows;
                i++
            )
            {
                bool visible =
                    i < visibleCount;

                rowObjects[i].SetActive(
                    visible
                );

                if (!visible)
                {
                    continue;
                }

                PlayerItemStatusEntry status =
                    statuses[i];

                iconImages[i].sprite =
                    status.Icon;

                iconImages[i].color =
                    status.IconColor;

                nameTexts[i].text =
                    status.DisplayName;

                detailTexts[i].text =
                    status.Detail;

                stateTexts[i].text =
                    status.ShowsRemainingTime
                        ? status.RemainingTime
                            .ToString("0.0") +
                            "초"
                        : status.StateText;
            }
        }

        private static GameObject
            CreateUiObject(
                string objectName,
                Transform parent
            )
        {
            GameObject uiObject =
                new GameObject(
                    objectName,
                    typeof(RectTransform)
                );

            uiObject.transform.SetParent(
                parent,
                false
            );

            return uiObject;
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            Font font,
            string value,
            int fontSize,
            TextAnchor alignment
        )
        {
            GameObject textObject =
                CreateUiObject(
                    objectName,
                    parent
                );

            Text text =
                textObject.AddComponent<Text>();

            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;

            return text;
        }

        private static void SetTopLeftRect(
            RectTransform rect,
            Vector2 position,
            Vector2 size
        )
        {
            Vector2 anchor =
                new Vector2(
                    0f,
                    1f
                );

            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition =
                position;
            rect.sizeDelta = size;
        }
    }
}
