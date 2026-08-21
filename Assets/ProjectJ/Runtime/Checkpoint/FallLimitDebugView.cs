using ProjectJ.Debugging;
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
            if (
                !ProjectJDebugOverlayController
                    .IsVisible
            )
            {
                return;
            }

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
                    ? GetCheckpointLabel(
                        checkpointTracker
                            .CurrentCheckpointId
                    )
                    : "없음";

            string fallenText =
                fallTracker.IsFallen
                    ? "추락"
                    : "안전";

            string text =
                "추락 판정 : " +
                fallenText +
                "\n체크포인트 : " +
                checkpointText +
                "\n플레이어 Y : " +
                fallTracker.transform
                    .position.y
                    .ToString("0.00") +
                "\n추락 기준 Y : " +
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

        private static string
            GetCheckpointLabel(
                CheckpointId checkpointId
            )
        {
            if (
                checkpointId ==
                CheckpointId.Start
            )
            {
                return "시작 지점";
            }

            string value =
                checkpointId.ToString();

            if (
                value.StartsWith("CP")
            )
            {
                return
                    "체크포인트 " +
                    value.Substring(2);
            }

            return value;
        }
    }
}
