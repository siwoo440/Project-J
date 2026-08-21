using UnityEngine;

namespace ProjectJ.Items.Status
{
    public readonly struct PlayerItemStatusEntry
    {
        public Sprite Icon { get; }
        public Color IconColor { get; }
        public string DisplayName { get; }
        public string Detail { get; }
        public float RemainingTime { get; }
        public bool ShowsRemainingTime { get; }
        public string StateText { get; }

        public PlayerItemStatusEntry(
            Sprite icon,
            Color iconColor,
            string displayName,
            string detail,
            float remainingTime,
            bool showsRemainingTime,
            string stateText
        )
        {
            Icon = icon;
            IconColor = iconColor;
            DisplayName =
                displayName ?? string.Empty;
            Detail =
                detail ?? string.Empty;
            RemainingTime =
                Mathf.Max(
                    0f,
                    remainingTime
                );
            ShowsRemainingTime =
                showsRemainingTime;
            StateText =
                stateText ?? string.Empty;
        }
    }
}
