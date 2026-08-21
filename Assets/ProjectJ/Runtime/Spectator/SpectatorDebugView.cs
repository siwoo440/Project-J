using ProjectJ.Debugging;
using UnityEngine;

namespace ProjectJ.Spectator
{
    [DisallowMultipleComponent]
    public sealed class SpectatorDebugView :
        MonoBehaviour
    {
        [SerializeField]
        private SpectatorController controller;

        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;

        public void Configure(
            SpectatorController
                newController
        )
        {
            controller =
                newController;
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

            if (controller == null)
            {
                return;
            }

            EnsureStyles();

            string targetName =
                controller.CurrentTarget != null
                    ? controller
                        .CurrentTarget
                        .gameObject
                        .name
                    : "--";

            string localInputName =
                controller.LocalInputSource != null
                    ? controller
                        .LocalInputSource
                        .gameObject
                        .name
                    : "--";

            string gameplayState =
                controller.LocalGameplayController != null &&
                controller
                    .LocalGameplayController
                    .enabled
                    ? "활성"
                    : "비활성";

            string spectatingState =
                controller.IsSpectating
                    ? "예"
                    : "아니오";

            string text =
                "관전 중 : " +
                spectatingState +
                "\n관전 대상 : " +
                targetName +
                "\n대상 수 : " +
                controller.ValidTargetCount +
                "\n카메라 입력 소유자 : " +
                localInputName +
                "\n로컬 조작 : " +
                gameplayState;

            GUI.Label(
                new Rect(
                    470f,
                    340f,
                    480f,
                    120f
                ),
                text,
                labelStyle
            );

            if (
                GUI.Button(
                    new Rect(
                        470f,
                        465f,
                        150f,
                        36f
                    ),
                    "관전 시작",
                    buttonStyle
                )
            )
            {
                controller.BeginSpectating();
            }

            if (
                GUI.Button(
                    new Rect(
                        630f,
                        465f,
                        100f,
                        36f
                    ),
                    "이전",
                    buttonStyle
                )
            )
            {
                controller.PreviousTarget();
            }

            if (
                GUI.Button(
                    new Rect(
                        740f,
                        465f,
                        100f,
                        36f
                    ),
                    "다음",
                    buttonStyle
                )
            )
            {
                controller.NextTarget();
            }

            if (
                GUI.Button(
                    new Rect(
                        850f,
                        465f,
                        130f,
                        36f
                    ),
                    "관전 종료",
                    buttonStyle
                )
            )
            {
                controller.ExitSpectating();
            }
        }

        private void ResolveController()
        {
            if (controller != null)
            {
                return;
            }

            controller =
                FindFirstObjectByType<
                    SpectatorController
                >();
        }

        private void EnsureStyles()
        {
            if (labelStyle == null)
            {
                labelStyle =
                    new GUIStyle(
                        GUI.skin.label
                    );

                labelStyle.fontSize = 18;
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

                buttonStyle.fontSize = 14;
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
