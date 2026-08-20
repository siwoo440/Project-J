using NUnit.Framework;
using ProjectJ.CameraSystem;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerThirdPersonCameraTests
    {
        [Test]
        public void LookRight_IncreasesYaw()
        {
            Vector2 angles =
                PlayerThirdPersonCamera.CalculateNextAngles(
                    0f,
                    0f,
                    new Vector2(
                        10f,
                        0f
                    ),
                    0.15f,
                    -45f,
                    70f
                );

            Assert.That(
                angles.x,
                Is.EqualTo(1.5f)
                    .Within(0.0001f)
            );
        }

        [Test]
        public void LookUp_DecreasesPitch()
        {
            Vector2 angles =
                PlayerThirdPersonCamera.CalculateNextAngles(
                    0f,
                    0f,
                    new Vector2(
                        0f,
                        10f
                    ),
                    0.15f,
                    -45f,
                    70f
                );

            Assert.That(
                angles.y,
                Is.EqualTo(-1.5f)
                    .Within(0.0001f)
            );
        }

        [Test]
        public void Pitch_ClampsAtMinimum()
        {
            Vector2 angles =
                PlayerThirdPersonCamera.CalculateNextAngles(
                    0f,
                    -44f,
                    new Vector2(
                        0f,
                        100f
                    ),
                    0.15f,
                    -45f,
                    70f
                );

            Assert.That(
                angles.y,
                Is.EqualTo(-45f)
            );
        }

        [Test]
        public void Pitch_ClampsAtMaximum()
        {
            Vector2 angles =
                PlayerThirdPersonCamera.CalculateNextAngles(
                    0f,
                    69f,
                    new Vector2(
                        0f,
                        -100f
                    ),
                    0.15f,
                    -45f,
                    70f
                );

            Assert.That(
                angles.y,
                Is.EqualTo(70f)
            );
        }

        [Test]
        public void Yaw_WrapsToSignedRange()
        {
            Vector2 angles =
                PlayerThirdPersonCamera.CalculateNextAngles(
                    179f,
                    0f,
                    new Vector2(
                        20f,
                        0f
                    ),
                    0.15f,
                    -45f,
                    70f
                );

            Assert.That(
                angles.x,
                Is.EqualTo(-178f)
                    .Within(0.0001f)
            );
        }

        [Test]
        public void RigPosition_AddsTargetHeight()
        {
            Vector3 position =
                PlayerThirdPersonCamera.CalculateRigPosition(
                    new Vector3(
                        2f,
                        3f,
                        4f
                    ),
                    1.5f
                );

            Assert.That(
                position,
                Is.EqualTo(
                    new Vector3(
                        2f,
                        4.5f,
                        4f
                    )
                )
            );
        }

        [Test]
        public void CameraCollision_NoWallUsesDesiredDistance()
        {
            float distance =
                PlayerThirdPersonCamera.CalculateCollisionAdjustedDistance(
                    false,
                    0f,
                    0.15f,
                    7.5f
                );

            Assert.That(
                distance,
                Is.EqualTo(7.5f)
                    .Within(0.0001f)
            );
        }

        [Test]
        public void CameraCollision_WallPullsCameraForward()
        {
            float distance =
                PlayerThirdPersonCamera.CalculateCollisionAdjustedDistance(
                    true,
                    4f,
                    0.15f,
                    7.5f
                );

            Assert.That(
                distance,
                Is.EqualTo(3.85f)
                    .Within(0.0001f)
            );
        }

        [Test]
        public void CameraCollision_NearWallNeverBecomesNegative()
        {
            float distance =
                PlayerThirdPersonCamera.CalculateCollisionAdjustedDistance(
                    true,
                    0.05f,
                    0.15f,
                    7.5f
                );

            Assert.That(
                distance,
                Is.EqualTo(0.05f)
                    .Within(0.0001f)
            );
        }

        [Test]
        public void CameraCollision_HitBeyondDesiredClampsToDesired()
        {
            float distance =
                PlayerThirdPersonCamera.CalculateCollisionAdjustedDistance(
                    true,
                    10f,
                    0.15f,
                    7.5f
                );

            Assert.That(
                distance,
                Is.EqualTo(7.5f)
                    .Within(0.0001f)
            );
        }
    }
}
