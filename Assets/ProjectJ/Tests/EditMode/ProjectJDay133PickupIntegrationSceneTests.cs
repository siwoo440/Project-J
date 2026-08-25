using System.Collections.Generic; // Pickup 정렬과 Item ID 검증 사용
using NUnit.Framework; // EditMode 테스트 사용
using ProjectJ.Items; // Runtime ItemPickup과 ItemDefinition 사용
using UnityEditor.SceneManagement; // Game Scene 로드 사용
using UnityEngine; // GameObject와 물리 컴포넌트 사용
using UnityEngine.SceneManagement; // Scene 조회 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJDay133PickupIntegrationSceneTests
    {
        private const string GameScenePath =
            "Assets/ProjectJ/Scenes/Game.unity";

        private const string TestMapRootName =
            "=== ITEM PICKUP TEST MAP ===";

        private const string PickupRootName =
            "=== ITEM PICKUPS ===";

        private const int ExpectedPickupCount =
            30;

        private static readonly string[] ExpectedItemIds =
        {
            "spring_shoes",
            "jelly_shield",
            "banana_cushion",
            "balloon_horn",
            "water_gun",
            "firework",
            "feather_shoes",
            "snowball",
            "mine",
            "pool_ball",
            "jetpack",
            "hammer",
            "bomb",
            "puffer_balloon_suit",
            "ink_octopus",
            "fishing_rod",
            "grappling_hook",
            "soap_bubble",
            "smoke_grenade",
            "trampoline",
            "giant_balloon",
            "cart",
            "rewind_clock",
            "homing_missile",
            "shrink_potion",
            "spiked_armor",
            "drone",
            "invisibility_cloak",
            "sniper_water_gun",
            "hand_mirror"
        };

        private Scene gameScene;
        private bool openedByTest;

        [SetUp]
        public void SetUp()
        {
            gameScene =
                SceneManager.GetSceneByPath(
                    GameScenePath
                );

            if (
                !gameScene.IsValid() ||
                !gameScene.isLoaded
            )
            {
                gameScene =
                    EditorSceneManager.OpenScene(
                        GameScenePath,
                        OpenSceneMode.Additive
                    );

                openedByTest =
                    true;
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (
                openedByTest &&
                gameScene.IsValid() &&
                gameScene.isLoaded
            )
            {
                EditorSceneManager.CloseScene(
                    gameScene,
                    true
                );
            }

            openedByTest =
                false;
        }

        [Test]
        public void GameScene_HasPickupIntegrationTestMap()
        {
            GameObject testMapRoot =
                FindObjectByName(
                    gameScene,
                    TestMapRootName
                );

            Assert.IsNotNull(
                testMapRoot,
                TestMapRootName
            );

            Assert.IsNotNull(
                FindChildByName(
                    testMapRoot.transform,
                    "TestArena_Floor"
                )
            );

            Assert.IsNotNull(
                FindChildByName(
                    testMapRoot.transform,
                    "TestArena_Bridge"
                )
            );

            Assert.IsNotNull(
                FindChildByName(
                    testMapRoot.transform,
                    "TestArena_PickupGallery"
                )
            );
        }

        [Test]
        public void PickupGallery_HasExactlyThirtyConfiguredPickups()
        {
            ItemPickup[] pickups =
                CollectPickups();

            Assert.AreEqual(
                ExpectedPickupCount,
                pickups.Length
            );
        }

        [Test]
        public void PickupGallery_CoversExpectedItemIdsInNetworkOrder()
        {
            ItemPickup[] pickups =
                CollectPickups();

            List<ItemPickup> ordered =
                new List<ItemPickup>(
                    pickups
                );

            ordered.Sort(
                (
                    left,
                    right
                ) =>
                    string.CompareOrdinal(
                        left.name,
                        right.name
                    )
            );

            Assert.AreEqual(
                ExpectedItemIds.Length,
                ordered.Count
            );

            for (
                int index = 0;
                index < ordered.Count;
                index++
            )
            {
                ItemPickup pickup =
                    ordered[index];

                Assert.IsNotNull(
                    pickup.Definition,
                    pickup.name
                );

                string expectedPrefix =
                    "Pickup_" +
                    (
                        index + 1
                    ).ToString("00") +
                    "_";

                Assert.IsTrue(
                    pickup.name.StartsWith(
                        expectedPrefix
                    ),
                    pickup.name
                );

                Assert.AreEqual(
                    ExpectedItemIds[index],
                    pickup.Definition.ItemId,
                    pickup.name
                );
            }
        }

        [Test]
        public void EveryPickup_HasRequiredNetworkAndPhysicsComponents()
        {
            ItemPickup[] pickups =
                CollectPickups();

            for (
                int index = 0;
                index < pickups.Length;
                index++
            )
            {
                ItemPickup pickup =
                    pickups[index];

                Assert.IsNotNull(
                    pickup.Definition,
                    pickup.name
                );

                Assert.IsFalse(
                    pickup.enabled,
                    pickup.name
                );

                BoxCollider trigger =
                    pickup.GetComponent<BoxCollider>();

                Rigidbody body =
                    pickup.GetComponent<Rigidbody>();

                Assert.IsNotNull(
                    trigger,
                    pickup.name
                );

                Assert.IsTrue(
                    trigger.isTrigger,
                    pickup.name
                );

                Assert.IsNotNull(
                    body,
                    pickup.name
                );

                Assert.IsTrue(
                    body.isKinematic,
                    pickup.name
                );

                Assert.IsFalse(
                    body.useGravity,
                    pickup.name
                );

                Assert.IsTrue(
                    HasComponentByFullName(
                        pickup.gameObject,
                        "Fusion.NetworkObject"
                    ),
                    pickup.name +
                    " / Fusion.NetworkObject"
                );

                Assert.IsTrue(
                    HasComponentByFullName(
                        pickup.gameObject,
                        "ProjectJ.Networking.Fusion.ProjectJNetworkItemBox"
                    ),
                    pickup.name +
                    " / ProjectJNetworkItemBox"
                );
            }
        }

        [Test]
        public void EveryPickupDefinition_IsValid()
        {
            ItemPickup[] pickups =
                CollectPickups();

            HashSet<string> itemIds =
                new HashSet<string>();

            for (
                int index = 0;
                index < pickups.Length;
                index++
            )
            {
                ItemDefinition definition =
                    pickups[index].Definition;

                Assert.IsNotNull(
                    definition,
                    pickups[index].name
                );

                Assert.IsTrue(
                    definition.IsDefinitionValid(
                        out string errorMessage
                    ),
                    definition.ItemId +
                    " / " +
                    errorMessage
                );

                Assert.IsTrue(
                    itemIds.Add(
                        definition.ItemId
                    ),
                    "Duplicate Item ID: " +
                    definition.ItemId
                );
            }

            Assert.AreEqual(
                ExpectedPickupCount,
                itemIds.Count
            );
        }

        private ItemPickup[] CollectPickups()
        {
            GameObject pickupRoot =
                FindObjectByName(
                    gameScene,
                    PickupRootName
                );

            Assert.IsNotNull(
                pickupRoot,
                PickupRootName
            );

            ItemPickup[] pickups =
                pickupRoot.GetComponentsInChildren<ItemPickup>(
                    true
                );

            Assert.AreEqual(
                ExpectedPickupCount,
                pickups.Length
            );

            return pickups;
        }

        private static bool HasComponentByFullName(
            GameObject target,
            string fullName
        )
        {
            if (
                target == null ||
                string.IsNullOrWhiteSpace(
                    fullName
                )
            )
            {
                return false;
            }

            Component[] components =
                target.GetComponents<Component>();

            for (
                int index = 0;
                index < components.Length;
                index++
            )
            {
                Component component =
                    components[index];

                if (
                    component != null &&
                    component.GetType().FullName ==
                    fullName
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static GameObject FindObjectByName(
            Scene scene,
            string objectName
        )
        {
            if (
                !scene.IsValid() ||
                !scene.isLoaded
            )
            {
                return null;
            }

            GameObject[] roots =
                scene.GetRootGameObjects();

            for (
                int rootIndex = 0;
                rootIndex < roots.Length;
                rootIndex++
            )
            {
                Transform[] transforms =
                    roots[rootIndex].GetComponentsInChildren<Transform>(
                        true
                    );

                for (
                    int transformIndex = 0;
                    transformIndex < transforms.Length;
                    transformIndex++
                )
                {
                    Transform candidate =
                        transforms[transformIndex];

                    if (
                        candidate != null &&
                        candidate.name ==
                        objectName
                    )
                    {
                        return candidate.gameObject;
                    }
                }
            }

            return null;
        }

        private static GameObject FindChildByName(
            Transform root,
            string objectName
        )
        {
            if (root == null)
            {
                return null;
            }

            Transform[] transforms =
                root.GetComponentsInChildren<Transform>(
                    true
                );

            for (
                int index = 0;
                index < transforms.Length;
                index++
            )
            {
                Transform candidate =
                    transforms[index];

                if (
                    candidate != null &&
                    candidate.name ==
                    objectName
                )
                {
                    return candidate.gameObject;
                }
            }

            return null;
        }
    }
}
