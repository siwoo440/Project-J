using ProjectJ.Debugging;
using UnityEngine;

namespace ProjectJ.Checkpoint
{
    [DisallowMultipleComponent]
    public sealed class RespawnDebugView :
        MonoBehaviour
    {
        [SerializeField]
        private PlayerRespawnController
            respawnController;

        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;

        public void Configure(
            PlayerRespawnController controller
        )
        {
            respawnController =
                controller;
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

            ResolveController();

            if (respawnController == null)
            {
                return;
            }

            EnsureStyles();

            PlayerCheckpointTracker tracker =
                respawnController
                    .CheckpointTracker;

            string checkpointText =
                tracker != null
                    ? GetCheckpointLabel(
                        tracker.CurrentCheckpointId
                    )
                    : "없음";

            string text =
                "부활 지점 : " +
                checkpointText +
                "\n부활 횟수 : " +
                respawnController
                    .RespawnCount;

            GUI.Label(
                new Rect(
                    20f,
                    235f,
                    380f,
                    60f
                ),
                text,
                labelStyle
            );

            if (
                GUI.Button(
                    new Rect(
                        20f,
                        300f,
                        170f,
                        38f
                    ),
                    "즉시 부활",
                    buttonStyle
                )
            )
            {
                respawnController
                    .RequestRespawn();
            }

            if (
                GUI.Button(
                    new Rect(
                        200f,
                        300f,
                        170f,
                        38f
                    ),
                    "낙하 테스트",
                    buttonStyle
                )
            )
            {
                ForceTestFall();
            }
        }

        private void ForceTestFall()
        {
            PlayerFallTracker fallTracker =
                respawnController
                    .FallTracker;

            Rigidbody body =
                respawnController.Body;

            if (
                fallTracker == null ||
                body == null
            )
            {
                return;
            }

            Vector3 testPosition =
                body.position;

            testPosition.y =
                fallTracker
                    .ActiveFallLimitY -
                1f;

            body.position =
                testPosition;

            Physics.SyncTransforms();

            fallTracker
                .EvaluateCurrentPosition();
        }

        private void ResolveController()
        {
            if (respawnController != null)
            {
                return;
            }

            respawnController =
                FindFirstObjectByType<
                    PlayerRespawnController
                >();
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

        private void EnsureStyles()
        {
            if (labelStyle == null)
            {
                labelStyle =
                    new GUIStyle(
                        GUI.skin.label
                    );

                labelStyle.fontSize = 20;
                labelStyle.fontStyle =
                    FontStyle.Bold;

                labelStyle
                    .normal
                    .textColor =
                        Color.black;
            }

            if (buttonStyle == null)
            {
                buttonStyle =
                    new GUIStyle(
                        GUI.skin.button
                    );

                buttonStyle.fontSize = 16;
                buttonStyle.fontStyle =
                    FontStyle.Bold;

                buttonStyle
                    .normal
                    .textColor =
                        Color.black;
            }
        }
    }
}
