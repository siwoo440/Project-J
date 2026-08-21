using UnityEngine;

namespace ProjectJ.Player
{
    public static class PlayerCollisionRules
    {
        public const string PlayerLayerName =
            "Player";

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad
        )]
        private static void ApplyBeforeSceneLoad()
        {
            Apply();
        }

        public static bool Apply()
        {
            int playerLayer =
                GetPlayerLayer();

            if (playerLayer < 0)
            {
                Debug.LogError(
                    "Player Layer를 찾을 수 없습니다."
                );

                return false;
            }

            Physics.IgnoreLayerCollision(
                playerLayer,
                playerLayer,
                true
            );

            return true;
        }

        public static bool IsPlayerCollisionIgnored()
        {
            int playerLayer =
                GetPlayerLayer();

            if (playerLayer < 0)
            {
                return false;
            }

            return Physics.GetIgnoreLayerCollision(
                playerLayer,
                playerLayer
            );
        }

        public static int GetPlayerLayer()
        {
            return LayerMask.NameToLayer(
                PlayerLayerName
            );
        }
    }
}
