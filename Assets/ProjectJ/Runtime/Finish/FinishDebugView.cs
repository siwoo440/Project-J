using UnityEngine;

namespace ProjectJ.Finish
{
    [DisallowMultipleComponent]
    public sealed class FinishDebugView :
        MonoBehaviour
    {
        [SerializeField]
        private FinishOrderManager finishManager;

        [SerializeField]
        private PlayerFinishState localPlayer;

        private GUIStyle labelStyle;

        public void Configure(
            FinishOrderManager manager,
            PlayerFinishState player
        )
        {
            finishManager = manager;
            localPlayer = player;
        }

        private void OnGUI()
        {
            ResolveReferences();

            if (finishManager == null)
            {
                return;
            }

            EnsureStyle();

            string playerState =
                "Not Finished";

            if (
                localPlayer != null &&
                localPlayer.IsFinished
            )
            {
                playerState =
                    "Finished #" +
                    localPlayer.FinishOrder +
                    " / Time " +
                    localPlayer.FinishTime
                        .ToString("0.000");
            }

            string text =
                "Finish Count : " +
                finishManager.FinishCount +
                "\nLocal Player : " +
                playerState;

            GUI.Label(
                new Rect(
                    470f,
                    20f,
                    460f,
                    70f
                ),
                text,
                labelStyle
            );
        }

        private void ResolveReferences()
        {
            if (finishManager == null)
            {
                finishManager =
                    FindFirstObjectByType<
                        FinishOrderManager
                    >();
            }

            if (localPlayer == null)
            {
                localPlayer =
                    FindFirstObjectByType<
                        PlayerFinishState
                    >();
            }
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
