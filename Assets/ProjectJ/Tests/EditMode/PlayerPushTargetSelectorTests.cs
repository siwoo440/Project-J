using NUnit.Framework;
using ProjectJ.Finish;
using ProjectJ.Player;
using ProjectJ.Push;
using ProjectJ.Ranking;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerPushTargetSelectorTests
    {
        private const float SearchRange =
            3f;

        private const float SearchAngle =
            90f;

        private int playerLayer;

        private GameObject selfObject;
        private PlayerFinishState selfFinishState;
        private PlayerPushTargetSelector selector;

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
                    out selfFinishState
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
        public void FrontPlayer_IsSelected()
        {
            GameObject front =
                CreatePlayer(
                    "Front",
                    new Vector3(
                        0f,
                        0f,
                        2f
                    ),
                    out PlayerFinishState
                        frontFinishState
                );

            Physics.SyncTransforms();

            bool found =
                selector.TryFindTarget(
                    out PlayerFinishState target
                );

            Assert.IsTrue(
                found
            );

            Assert.AreSame(
                frontFinishState,
                target
            );

            Assert.AreSame(
                frontFinishState,
                selector.CurrentTarget
            );

            Assert.IsNotNull(
                front
            );
        }

        [Test]
        public void NearestFrontPlayer_IsSelected()
        {
            CreatePlayer(
                "Far",
                new Vector3(
                    0f,
                    0f,
                    2.5f
                ),
                out _
            );

            CreatePlayer(
                "Near",
                new Vector3(
                    0f,
                    0f,
                    1.25f
                ),
                out PlayerFinishState
                    nearFinishState
            );

            Physics.SyncTransforms();

            bool found =
                selector.TryFindTarget(
                    out PlayerFinishState target
                );

            Assert.IsTrue(
                found
            );

            Assert.AreSame(
                nearFinishState,
                target
            );
        }

        [Test]
        public void PlayerBehind_IsNotSelected()
        {
            CreatePlayer(
                "Behind",
                new Vector3(
                    0f,
                    0f,
                    -1f
                ),
                out _
            );

            Physics.SyncTransforms();

            bool found =
                selector.TryFindTarget(
                    out PlayerFinishState target
                );

            Assert.IsFalse(
                found
            );

            Assert.IsNull(
                target
            );
        }

        [Test]
        public void PlayerOutsideAngle_IsNotSelected()
        {
            CreatePlayer(
                "Side",
                new Vector3(
                    2f,
                    0f,
                    0f
                ),
                out _
            );

            Physics.SyncTransforms();

            bool found =
                selector.TryFindTarget(
                    out PlayerFinishState target
                );

            Assert.IsFalse(
                found
            );

            Assert.IsNull(
                target
            );
        }

        [Test]
        public void PlayerOutsideRange_IsNotSelected()
        {
            CreatePlayer(
                "FarOutside",
                new Vector3(
                    0f,
                    0f,
                    4f
                ),
                out _
            );

            Physics.SyncTransforms();

            bool found =
                selector.TryFindTarget(
                    out PlayerFinishState target
                );

            Assert.IsFalse(
                found
            );

            Assert.IsNull(
                target
            );
        }

        [Test]
        public void Self_IsNeverSelected()
        {
            Physics.SyncTransforms();

            bool found =
                selector.TryFindTarget(
                    out PlayerFinishState target
                );

            Assert.IsFalse(
                found
            );

            Assert.IsNull(
                target
            );
        }

        [Test]
        public void FinishedPlayer_IsNotSelected()
        {
            CreatePlayer(
                "Finished",
                new Vector3(
                    0f,
                    0f,
                    1f
                ),
                out PlayerFinishState
                    finishedState
            );

            finishedState.TryConfirmFinish(
                1,
                10d
            );

            CreatePlayer(
                "Active",
                new Vector3(
                    0f,
                    0f,
                    2f
                ),
                out PlayerFinishState
                    activeState
            );

            Physics.SyncTransforms();

            bool found =
                selector.TryFindTarget(
                    out PlayerFinishState target
                );

            Assert.IsTrue(
                found
            );

            Assert.AreSame(
                activeState,
                target
            );
        }

        [Test]
        public void FinishedSelf_CannotSelectTarget()
        {
            CreatePlayer(
                "Active",
                new Vector3(
                    0f,
                    0f,
                    1f
                ),
                out _
            );

            selfFinishState.TryConfirmFinish(
                1,
                10d
            );

            Physics.SyncTransforms();

            bool found =
                selector.TryFindTarget(
                    out PlayerFinishState target
                );

            Assert.IsFalse(
                found
            );

            Assert.IsNull(
                target
            );
        }

        [Test]
        public void ClearTarget_RemovesCurrentTarget()
        {
            CreatePlayer(
                "Front",
                new Vector3(
                    0f,
                    0f,
                    1f
                ),
                out _
            );

            Physics.SyncTransforms();

            selector.TryFindTarget(
                out _
            );

            Assert.IsNotNull(
                selector.CurrentTarget
            );

            selector.ClearTarget();

            Assert.IsNull(
                selector.CurrentTarget
            );
        }

        private GameObject CreatePlayer(
            string objectName,
            Vector3 position,
            out PlayerFinishState finishState
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

            return player;
        }
    }
}
