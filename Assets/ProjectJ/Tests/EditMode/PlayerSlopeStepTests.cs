using NUnit.Framework;
using ProjectJ.Player;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerSlopeStepTests
    {
        [Test]
        public void Slope_ThirtyDegreesIsWalkable()
        {
            Vector3 normal =
                Quaternion.Euler(30f, 0f, 0f) *
                Vector3.up;

            bool walkable =
                PlayerCameraRelativeMovement.IsSlopeWalkable(
                    normal,
                    45f
                );

            Assert.That(walkable, Is.True);
        }

        [Test]
        public void Slope_SixtyDegreesIsNotWalkable()
        {
            Vector3 normal =
                Quaternion.Euler(60f, 0f, 0f) *
                Vector3.up;

            bool walkable =
                PlayerCameraRelativeMovement.IsSlopeWalkable(
                    normal,
                    45f
                );

            Assert.That(walkable, Is.False);
        }

        [Test]
        public void Slope_ProjectedDirectionIsParallelToSurface()
        {
            Vector3 normal =
                Quaternion.Euler(30f, 0f, 0f) *
                Vector3.up;

            Vector3 direction =
                PlayerCameraRelativeMovement.ProjectDirectionOnSlope(
                    Vector3.forward,
                    normal
                );

            float dot =
                Vector3.Dot(
                    direction,
                    normal.normalized
                );

            Assert.That(
                dot,
                Is.EqualTo(0f).Within(0.0001f)
            );

            Assert.That(
                direction.magnitude,
                Is.EqualTo(1f).Within(0.0001f)
            );
        }

        [Test]
        public void Slope_SurfaceVelocityDoesNotExceedMoveSpeed()
        {
            Vector3 normal =
                Quaternion.Euler(30f, 0f, 0f) *
                Vector3.up;

            Vector3 velocity =
                PlayerCameraRelativeMovement.CalculateSurfaceVelocity(
                    Vector3.zero,
                    Vector3.forward,
                    normal,
                    6f,
                    1000f,
                    40f,
                    0.02f
                );

            Assert.That(
                velocity.magnitude,
                Is.EqualTo(6f).Within(0.0001f)
            );

            Assert.That(
                Vector3.Dot(
                    velocity,
                    normal.normalized
                ),
                Is.EqualTo(0f).Within(0.0001f)
            );
        }

        [Test]
        public void GroundGap_ClampsPenetrationToZero()
        {
            float gap =
                PlayerCameraRelativeMovement.CalculateGroundGap(
                    0.95f,
                    1f
                );

            Assert.That(gap, Is.EqualTo(0f));
        }

        [Test]
        public void GroundSnap_NearGroundIsAllowed()
        {
            bool shouldSnap =
                PlayerCameraRelativeMovement.ShouldApplyGroundSnap(
                    true,
                    false,
                    -0.1f,
                    0.15f,
                    0.25f,
                    false
                );

            Assert.That(shouldSnap, Is.True);
        }

        [Test]
        public void GroundSnap_JumpDisablesSnap()
        {
            bool shouldSnap =
                PlayerCameraRelativeMovement.ShouldApplyGroundSnap(
                    true,
                    false,
                    0f,
                    0.15f,
                    0.25f,
                    true
                );

            Assert.That(shouldSnap, Is.False);
        }

        [Test]
        public void GroundSnap_UpwardMotionDisablesSnap()
        {
            bool shouldSnap =
                PlayerCameraRelativeMovement.ShouldApplyGroundSnap(
                    true,
                    false,
                    2f,
                    0.15f,
                    0.25f,
                    false
                );

            Assert.That(shouldSnap, Is.False);
        }

        [Test]
        public void StepAssist_LowStepIsAllowed()
        {
            bool canStep =
                PlayerCameraRelativeMovement.CanUseStepAssist(
                    true,
                    false,
                    true,
                    false,
                    0f
                );

            Assert.That(canStep, Is.True);
        }

        [Test]
        public void StepAssist_BlockedUpperProbePreventsStep()
        {
            bool canStep =
                PlayerCameraRelativeMovement.CanUseStepAssist(
                    true,
                    true,
                    true,
                    false,
                    0f
                );

            Assert.That(canStep, Is.False);
        }

        [Test]
        public void StepAssist_JumpDoesNotUseAssist()
        {
            bool canStep =
                PlayerCameraRelativeMovement.CanUseStepAssist(
                    true,
                    false,
                    true,
                    true,
                    0f
                );

            Assert.That(canStep, Is.False);
        }

        [Test]
        public void StepAssist_AirborneDoesNotUseAssist()
        {
            bool canStep =
                PlayerCameraRelativeMovement.CanUseStepAssist(
                    true,
                    false,
                    false,
                    false,
                    -1f
                );

            Assert.That(canStep, Is.False);
        }
    }
}
