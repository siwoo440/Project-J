using ProjectJ.Debugging;
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
            if (
                !ProjectJDebugOverlayController
                    .IsVisible
            )
            {
                return;
            }

            ResolveReferences();

            if (finishManager == null)
            {
                return;
            }

            EnsureStyle();

            string playerState =
                "미완주";

            if (
                localPlayer != null &&
                localPlayer.IsFinished
            )
            {
                playerState =
                    "완주 순위 " +
                    localPlayer.FinishOrder +
                    "위 / 기록 " +
                    localPlayer.FinishTime
                        .ToString("0.000") +
                    "초";
            }

            string text =
                "완주 인원 : " +
                finishManager.FinishCount +
                "\n내 플레이어 : " +
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
