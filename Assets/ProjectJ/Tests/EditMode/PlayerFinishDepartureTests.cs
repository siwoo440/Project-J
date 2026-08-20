using NUnit.Framework;
using ProjectJ.Finish;
using ProjectJ.Player;
using ProjectJ.Ranking;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerFinishDepartureTests
    {
        private GameObject playerObject;
        private Rigidbody body;
        private CapsuleCollider playerCollider;
        private Renderer visualRenderer;
        private Animator animator;
        private PlayerFinishState finishState;

        [SetUp]
        public void SetUp()
        {
            playerObject =
                new GameObject(
                    "Finish Test Player"
                );

            body =
                playerObject.AddComponent<
                    Rigidbody
                >();

            body.useGravity =
                false;

            playerCollider =
                playerObject.AddComponent<
                    CapsuleCollider
                >();

            PlayerHeightTracker heightTracker =
                playerObject.AddComponent<
                    PlayerHeightTracker
                >();

            PlayerRankingParticipant ranking =
                playerObject.AddComponent<
                    PlayerRankingParticipant
                >();

            ranking.Configure(
                1,
                heightTracker
            );

            finishState =
                playerObject.AddComponent<
                    PlayerFinishState
                >();

            finishState.Configure(
                ranking
            );

            GameObject visual =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            visual.name =
                "Visual";

            visual.transform.SetParent(
                playerObject.transform,
                false
            );

            visualRenderer =
                visual.GetComponent<
                    Renderer
                >();

            Collider visualCollider =
                visual.GetComponent<
                    Collider
                >();

            Object.DestroyImmediate(
                visualCollider
            );

            animator =
                visual.AddComponent<
                    Animator
                >();
        }

        [TearDown]
        public void TearDown()
        {
            if (playerObject != null)
            {
                Object.DestroyImmediate(
                    playerObject
                );
            }
        }

        [Test]
        public void Finish_StopsPhysicsAndHidesCharacter()
        {
            body.linearVelocity =
                new Vector3(
                    3f,
                    0f,
                    7f
                );

            body.angularVelocity =
                new Vector3(
                    0f,
                    2f,
                    0f
                );

            bool finished =
                finishState.TryConfirmFinish(
                    1,
                    10d
                );

            Assert.IsTrue(
                finished
            );

            Assert.IsTrue(
                finishState
                    .FinishDepartureApplied
            );

            Assert.AreEqual(
                Vector3.zero,
                body.linearVelocity
            );

            Assert.AreEqual(
                Vector3.zero,
                body.angularVelocity
            );

            Assert.IsTrue(
                body.isKinematic
            );

            Assert.IsFalse(
                body.detectCollisions
            );

            Assert.IsFalse(
                playerCollider.enabled
            );

            Assert.IsFalse(
                visualRenderer.enabled
            );

            Assert.IsFalse(
                animator.enabled
            );
        }

        [Test]
        public void FinishDeparture_IsAppliedOnlyOnce()
        {
            finishState.TryConfirmFinish(
                1,
                10d
            );

            bool firstApplied =
                finishState
                    .FinishDepartureApplied;

            finishState
                .ApplyFinishedPlayerDeparture();

            Assert.IsTrue(
                firstApplied
            );

            Assert.IsTrue(
                finishState
                    .FinishDepartureApplied
            );
        }
    }
}
