using NUnit.Framework;
using ProjectJ.Player;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerLedgeClimberTests
    {
        [Test]
        public void StartClimb_ValidLedgeStarts()
        {
            bool canStart =
                PlayerLedgeClimber.CanStartClimb(
                    true,
                    false,
                    false
                );

            Assert.That(canStart, Is.True);
        }

        [Test]
        public void StartClimb_NoLedgeDoesNotStart()
        {
            bool canStart =
                PlayerLedgeClimber.CanStartClimb(
                    false,
                    false,
                    false
                );

            Assert.That(canStart, Is.False);
        }

        [Test]
        public void StartClimb_AlreadyClimbingDoesNotRestart()
        {
            bool canStart =
                PlayerLedgeClimber.CanStartClimb(
                    true,
                    true,
                    false
                );

            Assert.That(canStart, Is.False);
        }

        [Test]
        public void StartClimb_CrouchingDoesNotStart()
        {
            bool canStart =
                PlayerLedgeClimber.CanStartClimb(
                    true,
                    false,
                    true
                );

            Assert.That(canStart, Is.False);
        }

        [Test]
        public void FootOffset_UsesDistanceFromBodyToFeet()
        {
            float offset =
                PlayerLedgeClimber.CalculateFootToBodyOffset(
                    2f,
                    1f
                );

            Assert.That(
                offset,
                Is.EqualTo(1f).Within(0.0001f)
            );
        }

        [Test]
        public void TargetPosition_PlacesFeetOnLedgeTop()
        {
            Vector3 target =
                PlayerLedgeClimber.CalculateTargetBodyPosition(
                    new Vector3(2f, 3f, 4f),
                    1f
                );

            Assert.That(
                target,
                Is.EqualTo(
                    new Vector3(2f, 4f, 4f)
                )
            );
        }

        [Test]
        public void LiftPosition_RisesAboveTargetByClearance()
        {
            Vector3 lift =
                PlayerLedgeClimber.CalculateLiftPosition(
                    new Vector3(0f, 1f, 0f),
                    new Vector3(2f, 2f, 0f),
                    0.08f
                );

            Assert.That(
                lift,
                Is.EqualTo(
                    new Vector3(0f, 2.08f, 0f)
                )
            );
        }

        [Test]
        public void PhaseProgress_HalfDurationReturnsHalf()
        {
            float progress =
                PlayerLedgeClimber.CalculatePhaseProgress(
                    0.1f,
                    0.2f
                );

            Assert.That(
                progress,
                Is.EqualTo(0.5f).Within(0.0001f)
            );
        }

        [Test]
        public void PhaseProgress_PastDurationClampsToOne()
        {
            float progress =
                PlayerLedgeClimber.CalculatePhaseProgress(
                    0.5f,
                    0.2f
                );

            Assert.That(progress, Is.EqualTo(1f));
        }

        [Test]
        public void TargetRotation_FacesIntoWall()
        {
            Quaternion rotation =
                PlayerLedgeClimber.CalculateTargetRotation(
                    Vector3.back,
                    Quaternion.identity
                );

            Vector3 forward =
                rotation * Vector3.forward;

            Assert.That(
                forward.x,
                Is.EqualTo(0f).Within(0.0001f)
            );

            Assert.That(
                forward.y,
                Is.EqualTo(0f).Within(0.0001f)
            );

            Assert.That(
                forward.z,
                Is.EqualTo(1f).Within(0.0001f)
            );
        }
    }
}
