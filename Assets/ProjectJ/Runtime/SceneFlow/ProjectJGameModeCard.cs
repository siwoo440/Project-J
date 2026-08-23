using UnityEngine; // 카드 Transform과 색상 애니메이션
using UnityEngine.EventSystems; // Pointer Hover / Click 처리
using UnityEngine.UI; // 카드 Image, Text, Outline 사용

namespace ProjectJ
{
    public enum ProjectJGameModeId
    {
        QuickPlay = 0,
        PrivateMatch = 1,
        Training = 2,
        CustomGame = 3
    }

    [DisallowMultipleComponent]
    public sealed class ProjectJGameModeCard :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        [SerializeField]
        private ProjectJPlayModePanel owner;

        [SerializeField]
        private ProjectJGameModeId modeId;

        [SerializeField]
        private string displayName;

        [SerializeField]
        private string description;

        [SerializeField]
        private bool comingSoon;

        [SerializeField]
        private RectTransform cardRect;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Outline selectedOutline;

        [SerializeField]
        private Text titleText;

        [SerializeField]
        private Text statusText;

        [SerializeField]
        private Color normalColor =
            new Color(
                0.14f,
                0.16f,
                0.28f,
                0.96f
            );

        [SerializeField]
        private Color hoverColor =
            new Color(
                0.23f,
                0.29f,
                0.48f,
                1f
            );

        [SerializeField]
        private Color selectedColor =
            new Color(
                0.13f,
                0.34f,
                0.52f,
                1f
            );

        private const float HoverScale =
            1.10f;

        private const float SelectedScale =
            1.06f;

        private const float HoverLift =
            20f;

        private const float AnimationSpeed =
            12f;

        private bool isHovered;

        private bool isSelected;

        private bool basePositionCaptured;

        private Vector2 baseAnchoredPosition;

        public ProjectJGameModeId ModeId =>
            modeId;

        public string DisplayName =>
            displayName;

        public string Description =>
            description;

        public bool ComingSoon =>
            comingSoon;

        public void Configure(
            ProjectJPlayModePanel panel,
            ProjectJGameModeId id,
            string modeName,
            string modeDescription,
            bool isComingSoon,
            RectTransform targetRect,
            Image targetBackground,
            Outline targetOutline,
            Text targetTitle,
            Text targetStatus,
            Color baseColor
        )
        {
            owner =
                panel;

            modeId =
                id;

            displayName =
                modeName;

            description =
                modeDescription;

            comingSoon =
                isComingSoon;

            cardRect =
                targetRect;

            backgroundImage =
                targetBackground;

            selectedOutline =
                targetOutline;

            titleText =
                targetTitle;

            statusText =
                targetStatus;

            normalColor =
                baseColor;

            hoverColor =
                Color.Lerp(
                    baseColor,
                    Color.white,
                    0.18f
                );

            selectedColor =
                Color.Lerp(
                    baseColor,
                    new Color(
                        0.15f,
                        0.75f,
                        1f,
                        1f
                    ),
                    0.34f
                );

            RefreshStaticVisuals();
        }

        private void Awake()
        {
            CaptureBasePosition();
            RefreshStaticVisuals();
            RefreshImmediateVisual();
        }

        private void OnEnable()
        {
            CaptureBasePosition();
            RefreshImmediateVisual();
        }

        private void Update()
        {
            if (cardRect == null)
            {
                return;
            }

            float targetScale =
                1f;

            float targetLift =
                0f;

            if (isHovered)
            {
                targetScale =
                    HoverScale;

                targetLift =
                    HoverLift;
            }
            else if (isSelected)
            {
                targetScale =
                    SelectedScale;
            }

            Vector3 desiredScale =
                Vector3.one *
                targetScale;

            cardRect.localScale =
                Vector3.Lerp(
                    cardRect.localScale,
                    desiredScale,
                    Time.unscaledDeltaTime *
                    AnimationSpeed
                );

            Vector2 desiredPosition =
                baseAnchoredPosition +
                new Vector2(
                    0f,
                    targetLift
                );

            cardRect.anchoredPosition =
                Vector2.Lerp(
                    cardRect.anchoredPosition,
                    desiredPosition,
                    Time.unscaledDeltaTime *
                    AnimationSpeed
                );

            RefreshColor();
        }

        public void OnPointerEnter(
            PointerEventData eventData
        )
        {
            isHovered =
                true;

            transform.SetAsLastSibling();
        }

        public void OnPointerExit(
            PointerEventData eventData
        )
        {
            isHovered =
                false;
        }

        public void OnPointerClick(
            PointerEventData eventData
        )
        {
            if (owner == null)
            {
                return;
            }

            owner.SelectCard(
                this
            );
        }

        public void SetSelected(
            bool selected
        )
        {
            isSelected =
                selected;

            if (
                selectedOutline != null
            )
            {
                selectedOutline.enabled =
                    isSelected;
            }

            if (
                isSelected &&
                !isHovered
            )
            {
                transform.SetAsLastSibling();
            }

            RefreshColor();
        }

        private void CaptureBasePosition()
        {
            if (
                basePositionCaptured ||
                cardRect == null
            )
            {
                return;
            }

            baseAnchoredPosition =
                cardRect.anchoredPosition;

            basePositionCaptured =
                true;
        }

        private void RefreshStaticVisuals()
        {
            if (titleText != null)
            {
                titleText.text =
                    displayName;
            }

            if (statusText != null)
            {
                statusText.text =
                    comingSoon
                        ? "COMING SOON"
                        : "AVAILABLE";
            }

            if (
                selectedOutline != null
            )
            {
                selectedOutline.enabled =
                    isSelected;
            }
        }

        private void RefreshImmediateVisual()
        {
            if (cardRect != null)
            {
                cardRect.localScale =
                    isSelected
                        ? Vector3.one *
                            SelectedScale
                        : Vector3.one;
            }

            RefreshColor();
        }

        private void RefreshColor()
        {
            if (backgroundImage == null)
            {
                return;
            }

            Color targetColor =
                normalColor;

            if (isHovered)
            {
                targetColor =
                    hoverColor;
            }
            else if (isSelected)
            {
                targetColor =
                    selectedColor;
            }

            backgroundImage.color =
                Color.Lerp(
                    backgroundImage.color,
                    targetColor,
                    Time.unscaledDeltaTime *
                    AnimationSpeed
                );
        }
    }
}
