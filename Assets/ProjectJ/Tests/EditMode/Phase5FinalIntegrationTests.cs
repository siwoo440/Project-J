using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectJ.Debugging;
using ProjectJ.Items;
using ProjectJ.Items.Effects;
using ProjectJ.Items.Status;
using ProjectJ.Push;
using UnityEditor;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class Phase5FinalIntegrationTests
    {
        private const string SpringShoesPath =
            "Assets/ProjectJ/Data/Items/Item_SpringShoes.asset";

        private const string JellyShieldPath =
            "Assets/ProjectJ/Data/Items/Item_JellyShield.asset";

        private const string BananaCushionPath =
            "Assets/ProjectJ/Data/Items/Item_BananaCushion.asset";

        private const string BalloonHornPath =
            "Assets/ProjectJ/Data/Items/Item_BalloonHorn.asset";

        private const string WaterGunPath =
            "Assets/ProjectJ/Data/Items/Item_WaterGun.asset";

        private GameObject playerObject;
        private PlayerItemInventory inventory;
        private PlayerItemUseController useController;

        private ItemDefinition springShoes;
        private ItemDefinition jellyShield;
        private ItemDefinition bananaCushion;
        private ItemDefinition balloonHorn;
        private ItemDefinition waterGun;

        [SetUp]
        public void SetUp()
        {
            ItemUseEffectRegistry.Clear();

            springShoes =
                LoadItem(SpringShoesPath);

            jellyShield =
                LoadItem(JellyShieldPath);

            bananaCushion =
                LoadItem(BananaCushionPath);

            balloonHorn =
                LoadItem(BalloonHornPath);

            waterGun =
                LoadItem(WaterGunPath);

            playerObject =
                new GameObject(
                    "Phase5 Test Player"
                );

            playerObject.transform.position =
                new Vector3(
                    0f,
                    10000f,
                    0f
                );

            inventory =
                playerObject.AddComponent<
                    PlayerItemInventory
                >();

            useController =
                playerObject.AddComponent<
                    PlayerItemUseController
                >();

            RegisterRepresentativeEffects();
        }

        [TearDown]
        public void TearDown()
        {
            ItemUseEffectRegistry.Clear();

            if (playerObject != null)
            {
                Object.DestroyImmediate(
                    playerObject
                );
            }
        }

        [Test]
        public void RepresentativeItemAssets_AreValid()
        {
            AssertItem(
                springShoes,
                "spring_shoes",
                "스프링 신발"
            );

            AssertItem(
                jellyShield,
                "jelly_shield",
                "젤리 보호막"
            );

            AssertItem(
                bananaCushion,
                "banana_cushion",
                "바나나 쿠션"
            );

            AssertItem(
                balloonHorn,
                "balloon_horn",
                "풍선 나팔"
            );

            AssertItem(
                waterGun,
                "water_gun",
                "물총"
            );
        }

        [Test]
        public void Inventory_UsesTwoSlots_AndReplacesSelectedSlot()
        {
            Assert.AreEqual(
                2,
                PlayerItemInventory.SlotCount
            );

            Assert.IsTrue(
                inventory.TryAdd(
                    springShoes,
                    out int springSlot
                )
            );

            Assert.AreEqual(
                0,
                springSlot
            );

            Assert.IsTrue(
                inventory.TryAdd(
                    jellyShield,
                    out int jellySlot
                )
            );

            Assert.AreEqual(
                1,
                jellySlot
            );

            Assert.IsTrue(
                inventory.SelectSlot(1)
            );

            Assert.IsTrue(
                inventory.TryAdd(
                    bananaCushion,
                    out int bananaSlot
                )
            );

            Assert.AreEqual(
                1,
                bananaSlot
            );

            Assert.AreSame(
                springShoes,
                inventory.GetItem(0)
            );

            Assert.AreSame(
                bananaCushion,
                inventory.GetItem(1)
            );
        }

        [Test]
        public void SpringShoes_SuccessfullyUsesAndConsumesItem()
        {
            AddAndSelect(
                springShoes
            );

            ItemUseResult result =
                useController
                    .TryUseSelectedItem();

            Assert.IsTrue(
                result.IsSuccess
            );

            Assert.IsNull(
                inventory.SelectedItem
            );

            SpringShoesBuffState state =
                playerObject.GetComponent<
                    SpringShoesBuffState
                >();

            Assert.IsNotNull(
                state
            );

            Assert.IsTrue(
                state.IsActive
            );

            Assert.Greater(
                state.RemainingTime,
                0f
            );

            Assert.IsTrue(
                state.ExtraJumpAvailable
            );
        }

        [Test]
        public void JellyShield_BlocksHostileForce_ButAllowsAirBag()
        {
            JellyShieldState state =
                playerObject.AddComponent<
                    JellyShieldState
                >();

            state.Activate(
                4f,
                jellyShield
            );

            Assert.IsTrue(
                state.IsActive
            );

            Assert.IsTrue(
                state.Blocks(
                    ExternalForceSource.Push
                )
            );

            Assert.IsTrue(
                state.Blocks(
                    ExternalForceSource.Item
                )
            );

            Assert.IsFalse(
                state.Blocks(
                    ExternalForceSource.AirBag
                )
            );
        }

        [Test]
        public void BananaCushion_InvalidPosition_DoesNotConsumeItem()
        {
            AddAndSelect(
                bananaCushion
            );

            playerObject.transform.position =
                new Vector3(
                    0f,
                    10000f,
                    0f
                );

            ItemUseResult result =
                useController
                    .TryUseSelectedItem();

            Assert.IsFalse(
                result.IsSuccess
            );

            Assert.AreEqual(
                ItemUseStatus.InvalidPosition,
                result.Status
            );

            Assert.AreSame(
                bananaCushion,
                inventory.SelectedItem
            );
        }

        [Test]
        public void BalloonHorn_WithoutTargets_StillCompletesAndConsumesItem()
        {
            AddAndSelect(
                balloonHorn
            );

            playerObject.transform.position =
                new Vector3(
                    0f,
                    10000f,
                    0f
                );

            ItemUseResult result =
                useController
                    .TryUseSelectedItem();

            Assert.IsTrue(
                result.IsSuccess
            );

            Assert.IsNull(
                inventory.SelectedItem
            );
        }

        [Test]
        public void WaterGun_HoldStarts_AndReleaseStopsRuntime()
        {
            AddAndSelect(
                waterGun
            );

            ItemUseResult result =
                useController
                    .TryUseSelectedItem();

            Assert.IsTrue(
                result.IsSuccess
            );

            Assert.IsNull(
                inventory.SelectedItem
            );

            WaterGunRuntime runtime =
                playerObject.GetComponent<
                    WaterGunRuntime
                >();

            Assert.IsNotNull(
                runtime
            );

            Assert.IsTrue(
                runtime.IsActive
            );

            useController
                .NotifyUseInputReleased();

            Assert.IsFalse(
                runtime.IsActive
            );
        }

        [Test]
        public void StatusTracker_CollectsActiveRepresentativeStates()
        {
            PlayerItemStatusTracker tracker =
                playerObject.AddComponent<
                    PlayerItemStatusTracker
                >();

            SpringShoesBuffState springState =
                playerObject.AddComponent<
                    SpringShoesBuffState
                >();

            springState.Activate(
                8f,
                springShoes
            );

            JellyShieldState jellyState =
                playerObject.AddComponent<
                    JellyShieldState
                >();

            jellyState.Activate(
                4f,
                jellyShield
            );

            WaterGunRuntime waterRuntime =
                playerObject.AddComponent<
                    WaterGunRuntime
                >();

            waterRuntime.Begin(
                waterGun
            );

            List<PlayerItemStatusEntry> statuses =
                new List<
                    PlayerItemStatusEntry
                >();

            tracker.CollectStatuses(
                statuses
            );

            Assert.AreEqual(
                3,
                statuses.Count
            );

            Assert.AreEqual(
                "스프링 신발",
                statuses[0].DisplayName
            );

            Assert.AreEqual(
                "젤리 보호막",
                statuses[1].DisplayName
            );

            Assert.AreEqual(
                "물총",
                statuses[2].DisplayName
            );

            Assert.AreEqual(
                "HOLD",
                statuses[2].StateText
            );
        }

        [Test]
        public void DebugOverlay_ResetState_StartsHidden()
        {
            MethodInfo resetMethod =
                typeof(
                    ProjectJDebugOverlayController
                ).GetMethod(
                    "ResetState",
                    BindingFlags.Static |
                    BindingFlags.NonPublic
                );

            Assert.IsNotNull(
                resetMethod
            );

            resetMethod.Invoke(
                null,
                null
            );

            Assert.IsFalse(
                ProjectJDebugOverlayController
                    .IsVisible
            );
        }

        private void RegisterRepresentativeEffects()
        {
            Assert.IsTrue(
                ItemUseEffectRegistry.Register(
                    "spring_shoes",
                    new SpringShoesEffect()
                )
            );

            Assert.IsTrue(
                ItemUseEffectRegistry.Register(
                    "jelly_shield",
                    new JellyShieldEffect()
                )
            );

            Assert.IsTrue(
                ItemUseEffectRegistry.Register(
                    "banana_cushion",
                    new BananaCushionEffect()
                )
            );

            Assert.IsTrue(
                ItemUseEffectRegistry.Register(
                    "balloon_horn",
                    new BalloonHornEffect()
                )
            );

            Assert.IsTrue(
                ItemUseEffectRegistry.Register(
                    "water_gun",
                    new WaterGunEffect()
                )
            );
        }

        private void AddAndSelect(
            ItemDefinition definition
        )
        {
            inventory.Clear();

            Assert.IsTrue(
                inventory.SelectSlot(0)
            );

            Assert.IsTrue(
                inventory.TryAdd(
                    definition,
                    out int slotIndex
                )
            );

            Assert.AreEqual(
                0,
                slotIndex
            );

            Assert.AreSame(
                definition,
                inventory.SelectedItem
            );
        }

        private static ItemDefinition LoadItem(
            string assetPath
        )
        {
            ItemDefinition definition =
                AssetDatabase.LoadAssetAtPath<
                    ItemDefinition
                >(
                    assetPath
                );

            Assert.IsNotNull(
                definition,
                $"아이템 데이터를 찾을 수 없습니다: {assetPath}"
            );

            return definition;
        }

        private static void AssertItem(
            ItemDefinition definition,
            string expectedId,
            string expectedName
        )
        {
            Assert.IsNotNull(
                definition
            );

            Assert.AreEqual(
                expectedId,
                definition.ItemId
            );

            Assert.AreEqual(
                expectedName,
                definition.DisplayName
            );

            Assert.IsTrue(
                definition.IsDefinitionValid(
                    out string errorMessage
                ),
                errorMessage
            );

            Assert.IsNotNull(
                definition.Icon,
                $"{expectedName} 아이콘이 연결되지 않았습니다."
            );
        }
    }
}
