using UnityEngine;

namespace ProjectJ.Checkpoint
{
    [DisallowMultipleComponent]
    public sealed class FallLimitDebugView :
        MonoBehaviour
    {
        [SerializeField]
        private PlayerFallTracker fallTracker;

        private GUIStyle labelStyle;

        public void Configure(
            PlayerFallTracker tracker
        )
        {
            fallTracker = tracker;
        }

        private void OnGUI()
        {
            ResolveTracker();

            if (fallTracker == null)
            {
                return;
            }

            EnsureStyle();

            PlayerCheckpointTracker
                checkpointTracker =
                    fallTracker
                        .CheckpointTracker;

            string checkpointText =
                checkpointTracker != null
                    ? checkpointTracker
                        .CurrentCheckpointId
                        .ToString()
                    : "None";

            string fallenText =
                fallTracker.IsFallen
                    ? "FALLEN"
                    : "SAFE";

            string text =
                "Fall Check : " +
                fallenText +
                "\nCheckpoint : " +
                checkpointText +
                "\nPlayer Y : " +
                fallTracker.transform
                    .position.y
                    .ToString("0.00") +
                "\nFall Limit Y : " +
                fallTracker.ActiveFallLimitY
                    .ToString("0.00");

            GUI.Label(
                new Rect(
                    20f,
                    120f,
                    420f,
                    110f
                ),
                text,
                labelStyle
            );
        }

        private void ResolveTracker()
        {
            if (fallTracker != null)
            {
                return;
            }

            fallTracker =
                FindFirstObjectByType<
                    PlayerFallTracker
                >();
        }

        private void EnsureStyle()
        {
            if (labelStyle != null)
            {
                return;
            }

            labelStyle =
                new GUIStyle(
                    GUI.skin.label
                );

            labelStyle.fontSize = 20;
            labelStyle.fontStyle =
                FontStyle.Bold;

            labelStyle.normal.textColor =
                Color.black;
        }
    }
}
