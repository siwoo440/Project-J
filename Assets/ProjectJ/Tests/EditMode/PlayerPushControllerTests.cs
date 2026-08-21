using NUnit.Framework;
using ProjectJ.Checkpoint;
using ProjectJ.Finish;
using ProjectJ.Player;
using ProjectJ.Push;
using ProjectJ.Ranking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerPushControllerTests
    {
        private const float SearchRange =
            3f;

        private const float SearchAngle =
            90f;

        private const float HorizontalPush =
            6f;

        private const float UpwardPush =
            0f;

        private const float Cooldown =
            1.5f;

        private int playerLayer;

        private GameObject selfObject;
        private PlayerFinishState selfFinishState;
        private PlayerPushTargetSelector selector;
        private PlayerPushController controller;

        [SetUp]
        public void SetUp()
        {
            playerLayer =
                LayerMask.NameToLayer(
                    "Player"
                );

            Assert.GreaterOrEqual(
                playerLayer,
                0
            );

            selfObject =
                CreatePlayer(
                    "Self",
                    Vector3.zero,
                    false,
                    out selfFinishState,
                    out _,
                    out _
                );

            selfObject.transform.forward =
                Vector3.forward;

            selector =
                selfObject.AddComponent<
                    PlayerPushTargetSelector
                >();

            selector.Configure(
                SearchRange,
                SearchAngle,
                1 << playerLayer,
                selfFinishState
            );

            PlayerInput input =
                selfObject.AddComponent<
                    PlayerInput
                >();

            controller =
                selfObject.AddComponent<
                    PlayerPushController
                >();

            controller.Configure(
                selector,
                input,
                selfFinishState,
                HorizontalPush,
                UpwardPush,
                Cooldown
            );

            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerFinishState[] players =
                Object.FindObjectsByType<
                    PlayerFinishState
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < players.Length;
                i++
            )
            {
                if (players[i] != null)
                {
                    Object.DestroyImmediate(
                        players[i].gameObject
                    );
                }
            }
        }

        [Test]
        public void SuccessfulPush_AppliesVelocityChangeAndStartsCooldown()
        {
            CreatePlayer(
                "Target",
                new Vector3(
                    0f,
                    0f,
                    1.5f
                ),
                false,
                out PlayerFinishState target,
                out Rigidbody targetBody,
                out _
            );

            Physics.SyncTransforms();

            PushAttemptResult result =
                controller.TryPushAt(
                    10d
                );

            Assert.AreEqual(
                PushAttemptResult.Success,
                result
            );

            Assert.AreSame(
                target,
                controller.LastTarget
            );

            Assert.AreEqual(
                0f,
                targetBody.linearVelocity.x,
                0.0001f
            );

            Assert.AreEqual(
                UpwardPush,
                targetBody.linearVelocity.y,
                0.0001f
            );

            Assert.AreEqual(
                HorizontalPush,
                targetBody.linearVelocity.z,
                0.0001f
            );

            Assert.IsTrue(
                controller.IsOnCooldown
            );

            Assert.AreEqual(
                Cooldown,
                controller.RemainingCooldown,
                0.0001f
            );
        }

        [Test]
        public void Miss_StartsCooldown()
        {
            PushAttemptResult first =
                controller.TryPushAt(
                    20d
                );

            PushAttemptResult second =
                controller.TryPushAt(
                    20.1d
                );

            Assert.AreEqual(
                PushAttemptResult.Miss,
                first
            );

            Assert.AreEqual(
                PushAttemptResult.Cooldown,
                second
            );
        }

        [Test]
        public void CooldownRequest_DoesNotApplySecondPush()
        {
            CreatePlayer(
                "Target",
                new Vector3(
                    0f,
                    0f,
                    1.5f
                ),
                false,
                out _,
                out Rigidbody targetBody,
                out _
            );

            Physics.SyncTransforms();

            PushAttemptResult first =
                controller.TryPushAt(
                    30d
                );

            Vector3 velocityAfterFirst =
                targetBody.linearVelocity;

            PushAttemptResult second =
                controller.TryPushAt(
                    30.5d
                );

            Assert.AreEqual(
                PushAttemptResult.Success,
                first
            );

            Assert.AreEqual(
                PushAttemptResult.Cooldown,
                second
            );

            Assert.AreEqual(
                velocityAfterFirst,
                targetBody.linearVelocity
            );
        }

        [Test]
        public void CooldownExpired_AllowsNextPush()
        {
            CreatePlayer(
                "Target",
                new Vector3(
                    0f,
                    0f,
                    1.5f
                ),
                false,
                out _,
                out Rigidbody targetBody,
                out _
            );

            Physics.SyncTransforms();

            controller.TryPushAt(
                40d
            );

            PushAttemptResult result =
                controller.TryPushAt(
                    41.5d
                );

            Assert.AreEqual(
                PushAttemptResult.Success,
                result
            );

            Assert.AreEqual(
                HorizontalPush * 2f,
                targetBody.linearVelocity.z,
                0.0001f
            );
        }

        [Test]
        public void RespawnProtectedTarget_RejectsPushAndStartsCooldown()
        {
            CreatePlayer(
                "ProtectedTarget",
                new Vector3(
                    0f,
                    0f,
                    1.5f
                ),
                true,
                out _,
                out Rigidbody targetBody,
                out PlayerRespawnProtection protection
            );

            protection.StartProtectionAt(
                50d
            );

            Physics.SyncTransforms();

            PushAttemptResult result =
                controller.TryPushAt(
                    50.1d
                );

            Assert.AreEqual(
                PushAttemptResult.Protected,
                result
            );

            Assert.AreEqual(
                Vector3.zero,
                targetBody.linearVelocity
            );

            Assert.IsTrue(
                controller.IsOnCooldown
            );
        }

        [Test]
        public void FinishedSelf_CannotPush()
        {
            CreatePlayer(
                "Target",
                new Vector3(
                    0f,
                    0f,
                    1.5f
                ),
                false,
                out _,
                out Rigidbody targetBody,
                out _
            );

            selfFinishState.TryConfirmFinish(
                1,
                60d
            );

            Physics.SyncTransforms();

            PushAttemptResult result =
                controller.TryPushAt(
                    60d
                );

            Assert.AreEqual(
                PushAttemptResult.InvalidState,
                result
            );

            Assert.AreEqual(
                Vector3.zero,
                targetBody.linearVelocity
            );

            Assert.IsFalse(
                controller.IsOnCooldown
            );
        }

        [Test]
        public void PushDirection_UsesTargetDirectionWithUpwardAmount()
        {
            Vector3 result =
                PlayerPushController
                    .CalculatePushVelocityChange(
                        Vector3.zero,
                        Vector3.forward,
                        new Vector3(
                            1f,
                            0f,
                            1f
                        ),
                        4f,
                        2f
                    );

            Vector3 expectedHorizontal =
                new Vector3(
                    1f,
                    0f,
                    1f
                ).normalized *
                4f;

            Assert.AreEqual(
                expectedHorizontal.x,
                result.x,
                0.0001f
            );

            Assert.AreEqual(
                0f,
                result.y,
                0.0001f
            );

            Assert.AreEqual(
                expectedHorizontal.z,
                result.z,
                0.0001f
            );
        }

        private GameObject CreatePlayer(
            string objectName,
            Vector3 position,
            bool addRespawnProtection,
            out PlayerFinishState finishState,
            out Rigidbody body,
            out PlayerRespawnProtection protection
        )
        {
            GameObject player =
                new GameObject(
                    objectName
                );

            player.layer =
                playerLayer;

            player.transform.position =
                position;

            player.AddComponent<
                CapsuleCollider
            >();

            body =
                player.AddComponent<
                    Rigidbody
                >();

            body.useGravity =
                false;

            PlayerHeightTracker height =
                player.AddComponent<
                    PlayerHeightTracker
                >();

            PlayerRankingParticipant ranking =
                player.AddComponent<
                    PlayerRankingParticipant
                >();

            ranking.Configure(
                -1,
                height
            );

            finishState =
                player.AddComponent<
                    PlayerFinishState
                >();

            finishState.Configure(
                ranking
            );

            protection =
                null;

            if (addRespawnProtection)
            {
                PlayerCheckpointTracker tracker =
                    player.GetComponent<
                        PlayerCheckpointTracker
                    >();

                if (tracker == null)
                {
                    tracker =
                        player.AddComponent<
                            PlayerCheckpointTracker
                        >();
                }

                PlayerFallTracker fallTracker =
                    player.GetComponent<
                        PlayerFallTracker
                    >();

                if (fallTracker == null)
                {
                    fallTracker =
                        player.AddComponent<
                            PlayerFallTracker
                        >();
                }

                PlayerRespawnController
                    respawnController =
                        player.GetComponent<
                            PlayerRespawnController
                        >();

                if (respawnController == null)
                {
                    respawnController =
                        player.AddComponent<
                            PlayerRespawnController
                        >();
                }

                respawnController.Configure(
                    body,
                    tracker,
                    fallTracker
                );

                protection =
                    player.GetComponent<
                        PlayerRespawnProtection
                    >();

                if (protection == null)
                {
                    protection =
                        player.AddComponent<
                            PlayerRespawnProtection
                        >();
                }

                protection.Configure(
                    respawnController,
                    3f
                );
            }

            PlayerPushReceiver receiver =
                player.AddComponent<
                    PlayerPushReceiver
                >();

            receiver.Configure(
                body,
                protection,
                finishState
            );

            return player;
        }
    }
}
