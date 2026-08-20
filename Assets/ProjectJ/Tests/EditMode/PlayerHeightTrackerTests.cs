using NUnit.Framework;
using ProjectJ.Player;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerHeightTrackerTests
    {
        [TestCase(0f, 0)]
        [TestCase(0.009f, 0)]
        [TestCase(0.019f, 1)]
        [TestCase(12.345f, 1234)]
        [TestCase(12.399f, 1239)]
        [TestCase(199.999f, 19999)]
        [TestCase(200f, 20000)]
        [TestCase(999.999f, 99999)]
        [TestCase(1000f, 100000)]
        public void TruncateToCentimeters_PositiveValues(
            float input,
            int expected
        )
        {
            Assert.That(
                PlayerHeightTracker
                    .TruncateToCentimeters(
                        input
                    ),
                Is.EqualTo(
                    expected
                )
            );
        }

        [TestCase(-0.009f, 0)]
        [TestCase(-0.019f, -1)]
        [TestCase(-1.239f, -123)]
        public void TruncateToCentimeters_NegativeValues(
            float input,
            int expected
        )
        {
            Assert.That(
                PlayerHeightTracker
                    .TruncateToCentimeters(
                        input
                    ),
                Is.EqualTo(
                    expected
                )
            );
        }

        [Test]
        public void CapsuleHeightTwo_CenterZero_FootIsMinusOne()
        {
            Vector3 foot =
                PlayerHeightTracker
                    .CalculateCapsuleFootLocalPosition(
                        Vector3.zero,
                        2f,
                        1
                    );

            Assert.That(
                foot,
                Is.EqualTo(
                    new Vector3(
                        0f,
                        -1f,
                        0f
                    )
                )
            );
        }

        [Test]
        public void Tracker_UsesFootReferenceInsteadOfRootHeight()
        {
            GameObject player =
                new GameObject(
                    "Player"
                );

            try
            {
                player.transform.position =
                    new Vector3(
                        0f,
                        1f,
                        0f
                    );

                CapsuleCollider capsule =
                    player.AddComponent<
                        CapsuleCollider
                    >();

                capsule.height = 2f;
                capsule.center = Vector3.zero;

                GameObject footObject =
                    new GameObject(
                        "HeightReference_Foot"
                    );

                footObject.transform.SetParent(
                    player.transform,
                    false
                );

                footObject.transform.localPosition =
                    new Vector3(
                        0f,
                        -1f,
                        0f
                    );

                PlayerHeightTracker tracker =
                    player.AddComponent<
                        PlayerHeightTracker
                    >();

                tracker.Configure(
                    footObject.transform
                );

                tracker.ResetTracking();

                Assert.That(
                    tracker.RawHeight,
                    Is.EqualTo(0f)
                        .Within(0.0001f)
                );

                Assert.That(
                    tracker.CurrentHeight,
                    Is.EqualTo(0f)
                        .Within(0.0001f)
                );
            }
            finally
            {
                Object.DestroyImmediate(
                    player
                );
            }
        }

        [Test]
        public void HighestHeight_DoesNotDecreaseAfterFalling()
        {
            GameObject player =
                new GameObject(
                    "Player"
                );

            try
            {
                GameObject footObject =
                    new GameObject(
                        "HeightReference_Foot"
                    );

                footObject.transform.SetParent(
                    player.transform,
                    false
                );

                PlayerHeightTracker tracker =
                    player.AddComponent<
                        PlayerHeightTracker
                    >();

                tracker.Configure(
                    footObject.transform
                );

                player.transform.position =
                    new Vector3(
                        0f,
                        450.789f,
                        0f
                    );

                tracker.ResetTracking();

                Assert.That(
                    tracker.CurrentHeightCentimeters,
                    Is.EqualTo(45078)
                );

                Assert.That(
                    tracker.HighestHeightCentimeters,
                    Is.EqualTo(45078)
                );

                player.transform.position =
                    new Vector3(
                        0f,
                        250.123f,
                        0f
                    );

                tracker.RefreshHeight();

                Assert.That(
                    tracker.CurrentHeightCentimeters,
                    Is.EqualTo(25012)
                );

                Assert.That(
                    tracker.HighestHeightCentimeters,
                    Is.EqualTo(45078)
                );
            }
            finally
            {
                Object.DestroyImmediate(
                    player
                );
            }
        }

        [Test]
        public void TruncatedMeterValue_UsesTwoDecimalPlaces()
        {
            float height =
                PlayerHeightTracker
                    .TruncateToTwoDecimals(
                        283.47891f
                    );

            Assert.That(
                height,
                Is.EqualTo(283.47f)
                    .Within(0.0001f)
            );
        }
    }
}
