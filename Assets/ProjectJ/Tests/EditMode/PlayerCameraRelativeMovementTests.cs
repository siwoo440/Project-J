using NUnit.Framework;
using ProjectJ.Player;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerCameraRelativeMovementTests
    {
        [Test]
        public void ForwardInput_UsesCameraForward()
        {
            Vector3 direction =
                PlayerCameraRelativeMovement.CalculateMoveDirection(
                    Vector2.up,
                    Vector3.forward,
                    Vector3.right
                );

            Assert.That(direction, Is.EqualTo(Vector3.forward));
        }

        [Test]
        public void RotatedCamera_ChangesWorldMoveDirection()
        {
            Vector3 direction =
                PlayerCameraRelativeMovement.CalculateMoveDirection(
                    Vector2.up,
                    Vector3.right,
                    Vector3.back
                );

            Assert.That(direction, Is.EqualTo(Vector3.right));
        }

        [Test]
        public void CameraTilt_DoesNotCreateVerticalMovement()
        {
            Vector3 direction =
                PlayerCameraRelativeMovement.CalculateMoveDirection(
                    Vector2.up,
                    new Vector3(0f, 1f, 1f),
                    Vector3.right
                );

            Assert.That(direction.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(direction, Is.EqualTo(Vector3.forward));
        }

        [Test]
        public void DiagonalInput_DoesNotExceedUnitLength()
        {
            Vector3 direction =
                PlayerCameraRelativeMovement.CalculateMoveDirection(
                    new Vector2(1f, 1f),
                    Vector3.forward,
                    Vector3.right
                );

            Assert.That(
                direction.magnitude,
                Is.EqualTo(1f).Within(0.0001f)
            );
        }

        [Test]
        public void ZeroInput_ReturnsZeroDirection()
        {
            Vector3 direction =
                PlayerCameraRelativeMovement.CalculateMoveDirection(
                    Vector2.zero,
                    Vector3.forward,
                    Vector3.right
                );

            Assert.That(direction, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Acceleration_DoesNotReachMaxSpeedImmediately()
        {
            Vector3 velocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    Vector3.zero,
                    Vector3.forward,
                    6f,
                    30f,
                    40f,
                    0.02f
                );

            Assert.That(velocity.z, Is.GreaterThan(0f));
            Assert.That(velocity.z, Is.LessThan(6f));
        }

        [Test]
        public void Acceleration_DoesNotExceedMoveSpeed()
        {
            Vector3 velocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    new Vector3(0f, 0f, 5.9f),
                    Vector3.forward,
                    6f,
                    30f,
                    40f,
                    0.02f
                );

            Assert.That(
                velocity.magnitude,
                Is.EqualTo(6f).Within(0.0001f)
            );
        }

        [Test]
        public void Deceleration_ReducesSpeedWithoutInput()
        {
            Vector3 velocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    new Vector3(0f, 0f, 6f),
                    Vector3.zero,
                    6f,
                    30f,
                    40f,
                    0.02f
                );

            Assert.That(velocity.z, Is.GreaterThan(0f));
            Assert.That(velocity.z, Is.LessThan(6f));
        }

        [Test]
        public void Deceleration_EventuallyStopsAtZero()
        {
            Vector3 velocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    new Vector3(0f, 0f, 0.5f),
                    Vector3.zero,
                    6f,
                    30f,
                    40f,
                    0.02f
                );

            Assert.That(velocity, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void ReverseInput_ChangesDirectionGradually()
        {
            Vector3 velocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    new Vector3(0f, 0f, 6f),
                    Vector3.back,
                    6f,
                    30f,
                    40f,
                    0.02f
                );

            Assert.That(velocity.z, Is.GreaterThan(0f));
            Assert.That(velocity.z, Is.LessThan(6f));
        }

        [Test]
        public void DiagonalTarget_DoesNotExceedMoveSpeed()
        {
            Vector3 diagonalDirection =
                new Vector3(1f, 0f, 1f).normalized;

            Vector3 velocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    Vector3.zero,
                    diagonalDirection,
                    6f,
                    1000f,
                    40f,
                    0.02f
                );

            Assert.That(
                velocity.magnitude,
                Is.EqualTo(6f).Within(0.0001f)
            );
        }

        [Test]
        public void HorizontalCalculation_DoesNotUseVerticalVelocity()
        {
            Vector3 velocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    new Vector3(2f, 25f, 0f),
                    Vector3.zero,
                    6f,
                    30f,
                    40f,
                    0.02f
                );

            Assert.That(velocity.y, Is.EqualTo(0f));
        }
    }
}
