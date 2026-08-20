using NUnit.Framework;
using ProjectJ.Player;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerLedgeDetectorTests
    {
        [Test]
        public void Height_BelowMinimumIsInvalid()
        {
            bool valid =
                PlayerLedgeDetector.IsLedgeHeightValid(
                    0.3f,
                    0.45f,
                    1.4f
                );

            Assert.That(valid, Is.False);
        }

        [Test]
        public void Height_WithinRangeIsValid()
        {
            bool valid =
                PlayerLedgeDetector.IsLedgeHeightValid(
                    0.8f,
                    0.45f,
                    1.4f
                );

            Assert.That(valid, Is.True);
        }

        [Test]
        public void Height_MaximumBoundaryIsValid()
        {
            bool valid =
                PlayerLedgeDetector.IsLedgeHeightValid(
                    1.4f,
                    0.45f,
                    1.4f
                );

            Assert.That(valid, Is.True);
        }

        [Test]
        public void Height_AboveMaximumIsInvalid()
        {
            bool valid =
                PlayerLedgeDetector.IsLedgeHeightValid(
                    1.7f,
                    0.45f,
                    1.4f
                );

            Assert.That(valid, Is.False);
        }

        [Test]
        public void TopSurface_ThirtyDegreesIsWalkable()
        {
            Vector3 normal =
                Quaternion.Euler(
                    30f,
                    0f,
                    0f
                ) *
                Vector3.up;

            bool walkable =
                PlayerLedgeDetector.IsTopSurfaceWalkable(
                    normal,
                    45f
                );

            Assert.That(walkable, Is.True);
        }

        [Test]
        public void TopSurface_SixtyDegreesIsNotWalkable()
        {
            Vector3 normal =
                Quaternion.Euler(
                    60f,
                    0f,
                    0f
                ) *
                Vector3.up;

            bool walkable =
                PlayerLedgeDetector.IsTopSurfaceWalkable(
                    normal,
                    45f
                );

            Assert.That(walkable, Is.False);
        }

        [Test]
        public void Candidate_AllConditionsValidReturnsTrue()
        {
            bool valid =
                PlayerLedgeDetector.IsLedgeCandidateValid(
                    true,
                    true,
                    true,
                    true,
                    true,
                    true
                );

            Assert.That(valid, Is.True);
        }

        [Test]
        public void Candidate_UpperBlockedReturnsFalse()
        {
            bool valid =
                PlayerLedgeDetector.IsLedgeCandidateValid(
                    true,
                    false,
                    true,
                    true,
                    true,
                    true
                );

            Assert.That(valid, Is.False);
        }

        [Test]
        public void Candidate_NoTopSurfaceReturnsFalse()
        {
            bool valid =
                PlayerLedgeDetector.IsLedgeCandidateValid(
                    true,
                    true,
                    false,
                    true,
                    true,
                    true
                );

            Assert.That(valid, Is.False);
        }

        [Test]
        public void Candidate_BadHeightReturnsFalse()
        {
            bool valid =
                PlayerLedgeDetector.IsLedgeCandidateValid(
                    true,
                    true,
                    true,
                    false,
                    true,
                    true
                );

            Assert.That(valid, Is.False);
        }

        [Test]
        public void Candidate_SteepTopReturnsFalse()
        {
            bool valid =
                PlayerLedgeDetector.IsLedgeCandidateValid(
                    true,
                    true,
                    true,
                    true,
                    false,
                    true
                );

            Assert.That(valid, Is.False);
        }

        [Test]
        public void Candidate_BlockedLandingReturnsFalse()
        {
            bool valid =
                PlayerLedgeDetector.IsLedgeCandidateValid(
                    true,
                    true,
                    true,
                    true,
                    true,
                    false
                );

            Assert.That(valid, Is.False);
        }
    }
}
