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

            Assert.That(direction.magnitude, Is.EqualTo(1f).Within(0.0001f));
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
    }
}
