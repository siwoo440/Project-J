using System.Collections.Generic;
using ProjectJ.Checkpoint;
using ProjectJ.Items.Effects;
using UnityEngine;

namespace ProjectJ.Items.Status
{
    [DisallowMultipleComponent]
    public sealed class PlayerItemStatusTracker :
        MonoBehaviour
    {
        public void CollectStatuses(
            List<PlayerItemStatusEntry> results
        )
        {
            if (results == null)
            {
                return;
            }

            results.Clear();

            CollectSpringShoes(results);
            CollectJellyShield(results);
            CollectWaterGun(results);
            CollectRespawnProtection(results);
        }

        private void CollectSpringShoes(
            List<PlayerItemStatusEntry> results
        )
        {
            SpringShoesBuffState state =
                GetComponent<
                    SpringShoesBuffState
                >();

            if (
                state == null ||
                !state.IsActive
            )
            {
                return;
            }

            ItemDefinition definition =
                state.Definition;

            results.Add(
                new PlayerItemStatusEntry(
                    GetIcon(definition),
                    GetIconColor(
                        definition,
                        new Color(
                            0.28f,
                            0.65f,
                            0.95f,
                            1f
                        )
                    ),
                    GetName(
                        definition,
                        "스프링 신발"
                    ),
                    state.ExtraJumpAvailable
                        ? "추가 점프 가능"
                        : "추가 점프 사용함",
                    state.RemainingTime,
                    true,
                    string.Empty
                )
            );
        }

        private void CollectJellyShield(
            List<PlayerItemStatusEntry> results
        )
        {
            JellyShieldState state =
                GetComponent<
                    JellyShieldState
                >();

            if (
                state == null ||
                !state.IsActive
            )
            {
                return;
            }

            ItemDefinition definition =
                state.Definition;

            results.Add(
                new PlayerItemStatusEntry(
                    GetIcon(definition),
                    GetIconColor(
                        definition,
                        new Color(
                            0.35f,
                            0.82f,
                            0.58f,
                            1f
                        )
                    ),
                    GetName(
                        definition,
                        "젤리 보호막"
                    ),
                    "Push / Item 방어",
                    state.RemainingTime,
                    true,
                    string.Empty
                )
            );
        }

        private void CollectWaterGun(
            List<PlayerItemStatusEntry> results
        )
        {
            WaterGunRuntime runtime =
                GetComponent<
                    WaterGunRuntime
                >();

            if (
                runtime == null ||
                !runtime.IsActive
            )
            {
                return;
            }

            ItemDefinition definition =
                runtime.Definition;

            results.Add(
                new PlayerItemStatusEntry(
                    GetIcon(definition),
                    GetIconColor(
                        definition,
                        new Color(
                            0.9f,
                            0.37f,
                            0.31f,
                            1f
                        )
                    ),
                    GetName(
                        definition,
                        "물총"
                    ),
                    "사용 버튼 유지 중",
                    0f,
                    false,
                    "HOLD"
                )
            );
        }

        private void
            CollectRespawnProtection(
                List<PlayerItemStatusEntry>
                    results
            )
        {
            PlayerRespawnProtection protection =
                GetComponent<
                    PlayerRespawnProtection
                >();

            if (
                protection == null ||
                !protection.IsProtected
            )
            {
                return;
            }

            results.Add(
                new PlayerItemStatusEntry(
                    null,
                    new Color(
                        0.35f,
                        0.72f,
                        1f,
                        1f
                    ),
                    "부활 보호",
                    "적대 효과 면역",
                    protection
                        .RemainingProtectionTime,
                    true,
                    string.Empty
                )
            );
        }

        private static Sprite GetIcon(
            ItemDefinition definition
        )
        {
            return
                definition != null
                    ? definition.Icon
                    : null;
        }

        private static string GetName(
            ItemDefinition definition,
            string fallback
        )
        {
            if (
                definition == null ||
                string.IsNullOrWhiteSpace(
                    definition.DisplayName
                )
            )
            {
                return fallback;
            }

            return definition.DisplayName;
        }

        private static Color GetIconColor(
            ItemDefinition definition,
            Color fallback
        )
        {
            if (
                definition != null &&
                definition.Icon != null
            )
            {
                return Color.white;
            }

            return fallback;
        }
    }
}
