using UnityEngine;

namespace ProjectJ.Checkpoint
{
    [DisallowMultipleComponent]
    public sealed class CheckpointDebugView :
        MonoBehaviour
    {
        [SerializeField]
        private PlayerCheckpointTracker tracker;

        private GUIStyle labelStyle;

        public void Configure(
            PlayerCheckpointTracker targetTracker
        )
        {
            tracker =
                targetTracker;
        }

        private void OnGUI()
        {
            ResolveTracker();

            if (tracker == null)
            {
                return;
            }

            EnsureStyle();

            GUI.Label(
                new Rect(
                    20f,
                    70f,
                    420f,
                    45f
                ),
                "Checkpoint : " +
                tracker.CurrentCheckpointId,
                labelStyle
            );
        }

        private void ResolveTracker()
        {
            if (tracker != null)
            {
                return;
            }

            tracker =
                FindFirstObjectByType<
                    PlayerCheckpointTracker
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

            labelStyle.fontSize = 24;
            labelStyle.fontStyle =
                FontStyle.Bold;

            labelStyle.normal.textColor =
                Color.black;
        }
    }
}
