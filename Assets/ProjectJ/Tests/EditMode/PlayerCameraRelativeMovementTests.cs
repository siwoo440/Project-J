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

        [Test]
        public void GroundedJump_ReturnsJumpVelocity()
        {
            float velocity =
                PlayerCameraRelativeMovement.CalculateVerticalVelocity(
                    0f,
                    true,
                    true,
                    8f,
                    -22f,
                    0.02f
                );

            Assert.That(velocity, Is.EqualTo(8f));
        }

        [Test]
        public void GroundedWithoutJump_StopsDownwardVelocity()
        {
            float velocity =
                PlayerCameraRelativeMovement.CalculateVerticalVelocity(
                    -5f,
                    true,
                    false,
                    8f,
                    -22f,
                    0.02f
                );

            Assert.That(velocity, Is.EqualTo(0f));
        }

        [Test]
        public void AirborneWithoutValidJump_AppliesGravity()
        {
            float velocity =
                PlayerCameraRelativeMovement.CalculateVerticalVelocity(
                    -2f,
                    false,
                    false,
                    8f,
                    -22f,
                    0.02f
                );

            Assert.That(velocity, Is.LessThan(-2f));
            Assert.That(velocity, Is.Not.EqualTo(8f));
        }

        [Test]
        public void Gravity_ReducesUpwardVelocity()
        {
            float velocity =
                PlayerCameraRelativeMovement.CalculateVerticalVelocity(
                    8f,
                    false,
                    false,
                    8f,
                    -22f,
                    0.02f
                );

            Assert.That(velocity, Is.LessThan(8f));
            Assert.That(velocity, Is.GreaterThan(0f));
        }

        [Test]
        public void Gravity_IncreasesDownwardSpeed()
        {
            float velocity =
                PlayerCameraRelativeMovement.CalculateVerticalVelocity(
                    -2f,
                    false,
                    false,
                    8f,
                    -22f,
                    0.02f
                );

            Assert.That(velocity, Is.LessThan(-2f));
        }

        [Test]
        public void UpwardVelocity_IsNotCanceledByGroundOverlap()
        {
            float velocity =
                PlayerCameraRelativeMovement.CalculateVerticalVelocity(
                    7f,
                    true,
                    false,
                    8f,
                    -22f,
                    0.02f
                );

            Assert.That(velocity, Is.LessThan(7f));
            Assert.That(velocity, Is.GreaterThan(0f));
        }

        [Test]
        public void CoyoteTimer_RefreshesWhileGrounded()
        {
            float timer =
                PlayerCameraRelativeMovement.CalculateCoyoteTimer(
                    0f,
                    true,
                    0.12f,
                    0.02f
                );

            Assert.That(timer, Is.EqualTo(0.12f).Within(0.0001f));
        }

        [Test]
        public void CoyoteTimer_CountsDownAfterLeavingGround()
        {
            float timer =
                PlayerCameraRelativeMovement.CalculateCoyoteTimer(
                    0.12f,
                    false,
                    0.12f,
                    0.02f
                );

            Assert.That(timer, Is.EqualTo(0.10f).Within(0.0001f));
        }

        [Test]
        public void CoyoteTimer_StopsAtZero()
        {
            float timer =
                PlayerCameraRelativeMovement.CalculateCoyoteTimer(
                    0.01f,
                    false,
                    0.12f,
                    0.02f
                );

            Assert.That(timer, Is.EqualTo(0f));
        }

        [Test]
        public void JumpBufferTimer_CountsDown()
        {
            float timer =
                PlayerCameraRelativeMovement.CalculateJumpBufferTimer(
                    0.12f,
                    0.02f
                );

            Assert.That(timer, Is.EqualTo(0.10f).Within(0.0001f));
        }

        [Test]
        public void JumpBufferTimer_StopsAtZero()
        {
            float timer =
                PlayerCameraRelativeMovement.CalculateJumpBufferTimer(
                    0.01f,
                    0.02f
                );

            Assert.That(timer, Is.EqualTo(0f));
        }

        [Test]
        public void BufferedJump_RequiresCoyoteAndBufferTime()
        {
            bool canJump =
                PlayerCameraRelativeMovement.CanUseBufferedJump(
                    0.10f,
                    0.08f
                );

            Assert.That(canJump, Is.True);
        }

        [Test]
        public void BufferedJump_FailsAfterCoyoteExpires()
        {
            bool canJump =
                PlayerCameraRelativeMovement.CanUseBufferedJump(
                    0f,
                    0.08f
                );

            Assert.That(canJump, Is.False);
        }

        [Test]
        public void BufferedJump_FailsAfterBufferExpires()
        {
            bool canJump =
                PlayerCameraRelativeMovement.CanUseBufferedJump(
                    0.10f,
                    0f
                );

            Assert.That(canJump, Is.False);
        }

        [Test]
        public void CoyoteJump_CanApplyJumpVelocityOffGround()
        {
            float velocity =
                PlayerCameraRelativeMovement.CalculateVerticalVelocity(
                    -0.5f,
                    false,
                    true,
                    8f,
                    -22f,
                    0.02f
                );

            Assert.That(velocity, Is.EqualTo(8f));
        }

        [Test]
        public void BufferedLandingJump_CanApplyJumpVelocity()
        {
            float velocity =
                PlayerCameraRelativeMovement.CalculateVerticalVelocity(
                    -4f,
                    true,
                    true,
                    8f,
                    -22f,
                    0.02f
                );

            Assert.That(velocity, Is.EqualTo(8f));
        }

        [Test]
        public void AirControl_JumpFrameIsAirborne()
        {
            bool isAirborne =
                PlayerCameraRelativeMovement.IsAirborneForHorizontalControl(
                    true,
                    0f,
                    true
                );

            Assert.That(isAirborne, Is.True);
        }

        [Test]
        public void AirControl_NotGroundedIsAirborne()
        {
            bool isAirborne =
                PlayerCameraRelativeMovement.IsAirborneForHorizontalControl(
                    false,
                    -2f,
                    false
                );

            Assert.That(isAirborne, Is.True);
        }

        [Test]
        public void AirControl_RisingGroundOverlapIsAirborne()
        {
            bool isAirborne =
                PlayerCameraRelativeMovement.IsAirborneForHorizontalControl(
                    true,
                    7f,
                    false
                );

            Assert.That(isAirborne, Is.True);
        }

        [Test]
        public void AirControl_StandingGroundStateIsNotAirborne()
        {
            bool isAirborne =
                PlayerCameraRelativeMovement.IsAirborneForHorizontalControl(
                    true,
                    0f,
                    false
                );

            Assert.That(isAirborne, Is.False);
        }

        [Test]
        public void AirControl_SelectsAirChangeRates()
        {
            Vector2 rates =
                PlayerCameraRelativeMovement.SelectHorizontalChangeRates(
                    true,
                    30f,
                    40f,
                    12f,
                    6f
                );

            Assert.That(rates.x, Is.EqualTo(12f));
            Assert.That(rates.y, Is.EqualTo(6f));
        }

        [Test]
        public void AirControl_SelectsGroundChangeRates()
        {
            Vector2 rates =
                PlayerCameraRelativeMovement.SelectHorizontalChangeRates(
                    false,
                    30f,
                    40f,
                    12f,
                    6f
                );

            Assert.That(rates.x, Is.EqualTo(30f));
            Assert.That(rates.y, Is.EqualTo(40f));
        }

        [Test]
        public void AirAcceleration_IsSlowerThanGroundAcceleration()
        {
            Vector3 groundVelocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    Vector3.zero,
                    Vector3.forward,
                    6f,
                    30f,
                    40f,
                    0.02f
                );

            Vector3 airVelocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    Vector3.zero,
                    Vector3.forward,
                    6f,
                    12f,
                    6f,
                    0.02f
                );

            Assert.That(
                airVelocity.magnitude,
                Is.LessThan(groundVelocity.magnitude)
            );
        }

        [Test]
        public void AirDeceleration_PreservesMoreMomentumThanGround()
        {
            Vector3 groundVelocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    Vector3.forward * 6f,
                    Vector3.zero,
                    6f,
                    30f,
                    40f,
                    0.02f
                );

            Vector3 airVelocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    Vector3.forward * 6f,
                    Vector3.zero,
                    6f,
                    12f,
                    6f,
                    0.02f
                );

            Assert.That(
                airVelocity.magnitude,
                Is.GreaterThan(groundVelocity.magnitude)
            );
        }

        [Test]
        public void AirControl_DoesNotExceedMoveSpeed()
        {
            Vector3 velocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    Vector3.forward * 5.9f,
                    Vector3.forward,
                    6f,
                    12f,
                    6f,
                    0.02f
                );

            Assert.That(
                velocity.magnitude,
                Is.EqualTo(6f).Within(0.0001f)
            );
        }

        [Test]
        public void Sprint_RequiresHeldInputGroundAndStamina()
        {
            bool canSprint =
                PlayerCameraRelativeMovement.CanSprint(
                    true,
                    Vector2.up,
                    false,
                    100f,
                    false,
                    false
                );

            Assert.That(canSprint, Is.True);
        }

        [Test]
        public void Sprint_DoesNotStartWithoutMoveInput()
        {
            bool canSprint =
                PlayerCameraRelativeMovement.CanSprint(
                    true,
                    Vector2.zero,
                    false,
                    100f,
                    false,
                    false
                );

            Assert.That(canSprint, Is.False);
        }

        [Test]
        public void Sprint_DoesNotStartInAir()
        {
            bool canSprint =
                PlayerCameraRelativeMovement.CanSprint(
                    true,
                    Vector2.up,
                    true,
                    100f,
                    false,
                    false
                );

            Assert.That(canSprint, Is.False);
        }

        [Test]
        public void Sprint_DoesNotStartAtZeroStamina()
        {
            bool canSprint =
                PlayerCameraRelativeMovement.CanSprint(
                    true,
                    Vector2.up,
                    false,
                    0f,
                    false,
                    false
                );

            Assert.That(canSprint, Is.False);
        }

        [Test]
        public void Sprint_DoesNotRestartWhileExhausted()
        {
            bool canSprint =
                PlayerCameraRelativeMovement.CanSprint(
                    true,
                    Vector2.up,
                    false,
                    50f,
                    true,
                    false
                );

            Assert.That(canSprint, Is.False);
        }

        [Test]
        public void Sprint_DoesNotStartWhileCrouching()
        {
            bool canSprint =
                PlayerCameraRelativeMovement.CanSprint(
                    true,
                    Vector2.up,
                    false,
                    100f,
                    false,
                    true
                );

            Assert.That(canSprint, Is.False);
        }

        [Test]
        public void Sprint_SelectsSprintMoveSpeed()
        {
            float speed =
                PlayerCameraRelativeMovement.SelectMoveSpeed(
                    false,
                    true,
                    3.5f,
                    6f,
                    9f
                );

            Assert.That(speed, Is.EqualTo(9f));
        }

        [Test]
        public void Walk_SelectsNormalMoveSpeed()
        {
            float speed =
                PlayerCameraRelativeMovement.SelectMoveSpeed(
                    false,
                    false,
                    3.5f,
                    6f,
                    9f
                );

            Assert.That(speed, Is.EqualTo(6f));
        }

        [Test]
        public void Crouch_SelectsCrouchMoveSpeed()
        {
            float speed =
                PlayerCameraRelativeMovement.SelectMoveSpeed(
                    true,
                    true,
                    3.5f,
                    6f,
                    9f
                );

            Assert.That(speed, Is.EqualTo(3.5f));
        }

        [Test]
        public void CrouchCenter_PreservesStandingBottom()
        {
            float crouchCenterY =
                PlayerCameraRelativeMovement.CalculateCrouchCenterY(
                    0f,
                    2f,
                    1.2f
                );

            float standingBottom = 0f - 2f * 0.5f;
            float crouchingBottom =
                crouchCenterY - 1.2f * 0.5f;

            Assert.That(
                crouchCenterY,
                Is.EqualTo(-0.4f).Within(0.0001f)
            );

            Assert.That(
                crouchingBottom,
                Is.EqualTo(standingBottom).Within(0.0001f)
            );
        }

        [Test]
        public void CrouchCenter_NoHeightChangeKeepsCenter()
        {
            float crouchCenterY =
                PlayerCameraRelativeMovement.CalculateCrouchCenterY(
                    0.25f,
                    2f,
                    2f
                );

            Assert.That(
                crouchCenterY,
                Is.EqualTo(0.25f).Within(0.0001f)
            );
        }

        [Test]
        public void Sprint_DrainsStamina()
        {
            float stamina =
                PlayerCameraRelativeMovement.CalculateStamina(
                    100f,
                    100f,
                    true,
                    25f,
                    20f,
                    1f
                );

            Assert.That(stamina, Is.EqualTo(75f));
        }

        [Test]
        public void NonSprint_RecoversStamina()
        {
            float stamina =
                PlayerCameraRelativeMovement.CalculateStamina(
                    50f,
                    100f,
                    false,
                    25f,
                    20f,
                    1f
                );

            Assert.That(stamina, Is.EqualTo(70f));
        }

        [Test]
        public void Stamina_DoesNotGoBelowZero()
        {
            float stamina =
                PlayerCameraRelativeMovement.CalculateStamina(
                    5f,
                    100f,
                    true,
                    25f,
                    20f,
                    1f
                );

            Assert.That(stamina, Is.EqualTo(0f));
        }

        [Test]
        public void Stamina_DoesNotExceedMaximum()
        {
            float stamina =
                PlayerCameraRelativeMovement.CalculateStamina(
                    95f,
                    100f,
                    false,
                    25f,
                    20f,
                    1f
                );

            Assert.That(stamina, Is.EqualTo(100f));
        }

        [Test]
        public void Sprint_DiagonalTargetDoesNotExceedSprintSpeed()
        {
            Vector3 diagonalDirection =
                new Vector3(1f, 0f, 1f).normalized;

            Vector3 velocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    Vector3.zero,
                    diagonalDirection,
                    9f,
                    1000f,
                    40f,
                    0.02f
                );

            Assert.That(
                velocity.magnitude,
                Is.EqualTo(9f).Within(0.0001f)
            );
        }

        [Test]
        public void Crouch_DiagonalTargetDoesNotExceedCrouchSpeed()
        {
            Vector3 diagonalDirection =
                new Vector3(1f, 0f, 1f).normalized;

            Vector3 velocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    Vector3.zero,
                    diagonalDirection,
                    3.5f,
                    1000f,
                    40f,
                    0.02f
                );

            Assert.That(
                velocity.magnitude,
                Is.EqualTo(3.5f).Within(0.0001f)
            );
        }

        [Test]
        public void SprintJump_AirControlPreservesMomentumGradually()
        {
            Vector3 velocity =
                PlayerCameraRelativeMovement.CalculateHorizontalVelocity(
                    Vector3.forward * 9f,
                    Vector3.forward,
                    6f,
                    12f,
                    6f,
                    0.02f
                );

            Assert.That(velocity.z, Is.LessThan(9f));
            Assert.That(velocity.z, Is.GreaterThan(6f));
        }
    }
}
