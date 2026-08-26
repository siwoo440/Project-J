using NUnit.Framework;
using ProjectJ.Player;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerCollisionRulesTests
    {
        private int playerLayer;
        private bool originalPlayerCollisionIgnored;

        [SetUp]
        public void SetUp()
        {
            playerLayer =
                PlayerCollisionRules
                    .GetPlayerLayer();

            Assert.GreaterOrEqual(
                playerLayer,
                0,
                "Player Layer가 ProjectSettings에 필요합니다."
            );

            originalPlayerCollisionIgnored =
                Physics.GetIgnoreLayerCollision(
                    playerLayer,
                    playerLayer
                );
        }

        [TearDown]
        public void TearDown()
        {
            if (playerLayer < 0)
            {
                return;
            }

            Physics.IgnoreLayerCollision(
                playerLayer,
                playerLayer,
                originalPlayerCollisionIgnored
            );
        }

        [Test]
        public void Apply_IgnoresPlayerToPlayerCollision()
        {
            bool applied =
                PlayerCollisionRules.Apply();

            Assert.IsTrue(
                applied
            );

            Assert.IsTrue(
                PlayerCollisionRules
                    .IsPlayerCollisionIgnored()
            );
        }

        [Test]
        public void Apply_DoesNotChangeWorldOrObstacleRules()
        {
            int worldLayer =
                LayerMask.NameToLayer(
                    "World"
                );

            int obstacleLayer =
                LayerMask.NameToLayer(
                    "Obstacle"
                );

            Assert.GreaterOrEqual(
                worldLayer,
                0
            );

            Assert.GreaterOrEqual(
                obstacleLayer,
                0
            );

            bool originalWorldRule =
                Physics.GetIgnoreLayerCollision(
                    playerLayer,
                    worldLayer
                );

            bool originalObstacleRule =
                Physics.GetIgnoreLayerCollision(
                    playerLayer,
                    obstacleLayer
                );

            PlayerCollisionRules.Apply();

            Assert.AreEqual(
                originalWorldRule,
                Physics.GetIgnoreLayerCollision(
                    playerLayer,
                    worldLayer
                )
            );

            Assert.AreEqual(
                originalObstacleRule,
                Physics.GetIgnoreLayerCollision(
                    playerLayer,
                    obstacleLayer
                )
            );
        }

        [Test]
        public void Apply_DoesNotChangeGameplayTriggerRule()
        {
            int gameplayTriggerLayer =
                LayerMask.NameToLayer(
                    "GameplayTrigger"
                );

            Assert.GreaterOrEqual(
                gameplayTriggerLayer,
                0
            );

            bool originalTriggerRule =
                Physics.GetIgnoreLayerCollision(
                    playerLayer,
                    gameplayTriggerLayer
                );

            PlayerCollisionRules.Apply();

            Assert.AreEqual(
                originalTriggerRule,
                Physics.GetIgnoreLayerCollision(
                    playerLayer,
                    gameplayTriggerLayer
                )
            );
        }

        [Test]
        public void PlayerCollider_RemainsAvailableForPhysicsQueries()
        {
            GameObject target =
                new GameObject(
                    "Player Query Target"
                );

            try
            {
                target.layer =
                    playerLayer;

                target.transform.position =
                    Vector3.zero;

                target.AddComponent<
                    CapsuleCollider
                >();

                PlayerCollisionRules.Apply();

                Physics.SyncTransforms();

                Collider[] hits =
                    Physics.OverlapSphere(
                        Vector3.zero,
                        1f,
                        1 << playerLayer,
                        QueryTriggerInteraction.Collide
                    );

                bool foundTarget =
                    false;

                for (
                    int i = 0;
                    i < hits.Length;
                    i++
                )
                {
                    if (
                        hits[i] != null &&
                        hits[i].gameObject ==
                            target
                    )
                    {
                        foundTarget =
                            true;

                        break;
                    }
                }

                Assert.IsTrue(
                    foundTarget,
                    "Player끼리의 충돌을 무시해도 " +
                    "Player Collider는 Physics Query에서 " +
                    "계속 탐색되어야 합니다."
                );
            }
            finally
            {
                Object.DestroyImmediate(
                    target
                );
            }
        }

        [Test] // Player 이동 Query Layer 제외 검증
        public void ExcludePlayerLayer_RemovesOnlyPlayerLayer() // Player Layer만 제외하는 동작 검증
        {
            int worldLayer = LayerMask.NameToLayer("World"); // World Layer 번호 조회
            int sourceMask = (1 << playerLayer) | (1 << worldLayer); // Player와 World 포함 Mask 생성
            int result = PlayerCollisionRules.ExcludePlayerLayer(sourceMask); // Player 제외 Mask 계산

            Assert.That(result & (1 << playerLayer), Is.EqualTo(0)); // Player Layer 제외 검증
            Assert.That(result & (1 << worldLayer), Is.Not.EqualTo(0)); // World Layer 유지 검증
        }
    }
}
