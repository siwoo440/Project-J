using ProjectJ.Debugging;
using UnityEngine;

namespace ProjectJ.Checkpoint
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(PlayerCheckpointTracker)
    )]
    public sealed class CheckpointDebugView :
        MonoBehaviour
    {
        [SerializeField]
        private PlayerCheckpointTracker tracker;

        [SerializeField]
        private bool showDebug = true;

        private void Awake()
        {
            if (tracker == null)
            {
                tracker =
                    GetComponent<
                        PlayerCheckpointTracker
                    >();
            }
        }

        private void OnGUI()
        {
            if (
                !showDebug ||
                !ProjectJDebugOverlayController
                    .IsVisible ||
                tracker == null
            )
            {
                return;
            }

            GUI.Label(
                new Rect(
                    20f,
                    122f,
                    460f,
                    24f
                ),
                "체크포인트: " +
                GetCheckpointLabel(
                    tracker.CurrentCheckpointId
                )
            );

            GUI.Label(
                new Rect(
                    20f,
                    146f,
                    680f,
                    24f
                ),
                "부활 위치: " +
                tracker.RespawnPosition
                    .ToString("F2")
            );
        }

        public void SetVisible(
            bool value
        )
        {
            showDebug = value;
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
