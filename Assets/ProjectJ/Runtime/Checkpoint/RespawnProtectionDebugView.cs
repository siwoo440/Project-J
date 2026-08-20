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
            ResolveProtection();

            if (protection == null)
            {
                return;
            }

            EnsureStyles();

            string protectionState =
                protection.IsProtected
                    ? "ON"
                    : "OFF";

            string hostileState =
                "Not Tested";

            if (hasHostileEffectResult)
            {
                hostileState =
                    lastHostileEffectAccepted
                        ? "ACCEPTED"
                        : "BLOCKED";
            }

            string text =
                "Respawn Protection : " +
                protectionState +
                "\nRemaining : " +
                protection
                    .RemainingProtectionTime
                    .ToString("0.00") +
                "s" +
                "\nHostile Effect : " +
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
                    "Direct Respawn",
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
                    "Test Hostile Effect",
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
