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
            float pitch =
                PlayerThirdPersonCamera.ClampPitch(
                    -60f,
                    -45f,
                    70f
                );

            Assert.That(
                pitch,
                Is.EqualTo(-45f)
            );
        }

        [Test]
        public void Pitch_ClampsAtMaximum()
        {
            float pitch =
                PlayerThirdPersonCamera.ClampPitch(
                    90f,
                    -45f,
                    70f
                );

            Assert.That(
                pitch,
                Is.EqualTo(70f)
            );
        }

        [Test]
        public void Zoom_WheelUpMovesCameraCloser()
        {
            float distance =
                PlayerThirdPersonCamera.CalculateZoomDistance(
                    7.5f,
                    120f,
                    0.75f,
                    3.5f,
                    10f
                );

            Assert.That(
                distance,
                Is.EqualTo(6.75f)
                    .Within(0.0001f)
            );
        }

        [Test]
        public void Zoom_WheelDownMovesCameraFarther()
        {
            float distance =
                PlayerThirdPersonCamera.CalculateZoomDistance(
                    7.5f,
                    -120f,
                    0.75f,
                    3.5f,
                    10f
                );

            Assert.That(
                distance,
                Is.EqualTo(8.25f)
                    .Within(0.0001f)
            );
        }

        [Test]
        public void Zoom_DoesNotPassMinimum()
        {
            float distance =
                PlayerThirdPersonCamera.CalculateZoomDistance(
                    3.5f,
                    120f,
                    0.75f,
                    3.5f,
                    10f
                );

            Assert.That(
                distance,
                Is.EqualTo(3.5f)
            );
        }

        [Test]
        public void Zoom_DoesNotPassMaximum()
        {
            float distance =
                PlayerThirdPersonCamera.CalculateZoomDistance(
                    10f,
                    -120f,
                    0.75f,
                    3.5f,
                    10f
                );

            Assert.That(
                distance,
                Is.EqualTo(10f)
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
                    10f
                );

            Assert.That(
                distance,
                Is.EqualTo(10f)
                    .Within(0.0001f)
            );
        }

        [Test]
        public void CameraCollision_WallOverridesZoomDistance()
        {
            float distance =
                PlayerThirdPersonCamera.CalculateCollisionAdjustedDistance(
                    true,
                    4f,
                    0.15f,
                    10f
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
        public void Fov_NormalStateUsesNormalValue()
        {
            float fov =
                PlayerThirdPersonCamera.CalculateTargetFov(
                    false,
                    60f,
                    68f
                );

            Assert.That(
                fov,
                Is.EqualTo(60f)
            );
        }

        [Test]
        public void Fov_SprintStateUsesSprintValue()
        {
            float fov =
                PlayerThirdPersonCamera.CalculateTargetFov(
                    true,
                    60f,
                    68f
                );

            Assert.That(
                fov,
                Is.EqualTo(68f)
            );
        }

        [Test]
        public void Fov_ChangesTowardTarget()
        {
            float fov =
                PlayerThirdPersonCamera.MoveFovTowards(
                    60f,
                    68f,
                    8f,
                    0.5f
                );

            Assert.That(
                fov,
                Is.EqualTo(64f)
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
    }
}
