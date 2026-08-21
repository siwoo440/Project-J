using ProjectJ.Debugging;
using UnityEngine;

namespace ProjectJ.Checkpoint
{
    [DisallowMultipleComponent]
    public sealed class RespawnProtectionDebugView :
        MonoBehaviour
    {
        [SerializeField]
        private PlayerRespawnProtection protection;

        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;

        private bool hasHostileEffectResult;
        private bool lastHostileEffectAccepted;

        public void Configure(
            PlayerRespawnProtection targetProtection
        )
        {
            protection =
                targetProtection;
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

            ResolveProtection();

            if (protection == null)
            {
                return;
            }

            EnsureStyles();

            string protectionState =
                protection.IsProtected
                    ? "켜짐"
                    : "꺼짐";

            string hostileState =
                "미테스트";

            if (hasHostileEffectResult)
            {
                hostileState =
                    lastHostileEffectAccepted
                        ? "허용"
                        : "차단";
            }

            string text =
                "부활 보호 : " +
                protectionState +
                "\n남은 시간 : " +
                protection
                    .RemainingProtectionTime
                    .ToString("0.00") +
                "초" +
                "\n적대 효과 : " +
                hostileState;

            GUI.Label(
                new Rect(
                    20f,
                    350f,
                    420f,
                    90f
                ),
                text,
                labelStyle
            );

            if (
                GUI.Button(
                    new Rect(
                        20f,
                        445f,
                        170f,
                        38f
                    ),
                    "즉시 부활",
                    buttonStyle
                )
            )
            {
                PlayerRespawnController
                    controller =
                        protection
                            .RespawnController;

                if (controller != null)
                {
                    controller
                        .RequestRespawn();
                }
            }

            if (
                GUI.Button(
                    new Rect(
                        200f,
                        445f,
                        210f,
                        38f
                    ),
                    "적대 효과 테스트",
                    buttonStyle
                )
            )
            {
                lastHostileEffectAccepted =
                    protection
                        .TryAcceptHostileEffect();

                hasHostileEffectResult = true;
            }
        }

        private void ResolveProtection()
        {
            if (protection != null)
            {
                return;
            }

            protection =
                FindFirstObjectByType<
                    PlayerRespawnProtection
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
