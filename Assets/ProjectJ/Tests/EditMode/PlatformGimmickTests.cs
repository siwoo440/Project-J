using NUnit.Framework;
using ProjectJ.Platforms;
using ProjectJ.Player;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlatformGimmickTests
    {
        [Test]
        public void MovingPlatform_MovesTowardTarget()
        {
            Vector3 result =
                MovingPlatform
                    .CalculateNextPosition(
                        Vector3.zero,
                        new Vector3(
                            10f,
                            0f,
                            0f
                        ),
                        2f,
                        1f
                    );

            Assert.AreEqual(
                new Vector3(
                    2f,
                    0f,
                    0f
                ),
                result
            );
        }

        [Test]
        public void PassengerPosition_FollowsPlatformTranslation()
        {
            Vector3 result =
                PlatformPassengerCarrier
                    .CalculatePassengerPosition(
                        new Vector3(
                            1f,
                            1f,
                            0f
                        ),
                        Vector3.zero,
                        Quaternion.identity,
                        new Vector3(
                            3f,
                            0f,
                            0f
                        ),
                        Quaternion.identity
                    );

            Assert.AreEqual(
                new Vector3(
                    4f,
                    1f,
                    0f
                ),
                result
            );
        }

        [Test]
        public void PassengerPosition_FollowsPlatformRotation()
        {
            Vector3 result =
                PlatformPassengerCarrier
                    .CalculatePassengerPosition(
                        new Vector3(
                            1f,
                            0f,
                            0f
                        ),
                        Vector3.zero,
                        Quaternion.identity,
                        Vector3.zero,
                        Quaternion.Euler(
                            0f,
                            90f,
                            0f
                        )
                    );

            Assert.AreEqual(
                0f,
                result.x,
                0.0001f
            );

            Assert.AreEqual(
                -1f,
                result.z,
                0.0001f
            );
        }

        [Test]
        public void SpringPlatform_IncreasesJumpVelocity()
        {
            GameObject gameObject =
                new GameObject(
                    "Spring"
                );

            try
            {
                SpringPlatform spring =
                    gameObject.AddComponent<
                        SpringPlatform
                    >();

                spring.Configure(
                    1.5f
                );

                Assert.AreEqual(
                    12f,
                    spring.GetBoostedJumpVelocity(
                        8f
                    ),
                    0.0001f
                );
            }
            finally
            {
                Object.DestroyImmediate(
                    gameObject
                );
            }
        }

        [Test]
        public void IceSurface_UsesSlowDecelerationWhenInputStops()
        {
            GameObject gameObject =
                new GameObject(
                    "Ice"
                );

            try
            {
                IceSurface ice =
                    gameObject.AddComponent<
                        IceSurface
                    >();

                ice.Configure(
                    6f,
                    2.5f,
                    3f
                );

                float rate =
                    ice.SelectChangeRate(
                        new Vector3(
                            6f,
                            0f,
                            0f
                        ),
                        Vector3.zero
                    );

                Assert.AreEqual(
                    2.5f,
                    rate,
                    0.0001f
                );

                Vector3 nextVelocity =
                    PlayerSurfaceInteraction
                        .CalculateIceVelocity(
                            new Vector3(
                                6f,
                                0f,
                                0f
                            ),
                            Vector3.zero,
                            rate,
                            0.1f
                        );

                Assert.AreEqual(
                    5.75f,
                    nextVelocity.x,
                    0.0001f
                );
            }
            finally
            {
                Object.DestroyImmediate(
                    gameObject
                );
            }
        }

        [Test]
        public void GhostPlatform_FollowsActiveWarningHiddenCycle()
        {
            Assert.AreEqual(
                GhostPlatformState.Active,
                GhostPlatform.EvaluateState(
                    1f,
                    3f,
                    1f,
                    2f
                )
            );

            Assert.AreEqual(
                GhostPlatformState.Warning,
                GhostPlatform.EvaluateState(
                    3.5f,
                    3f,
                    1f,
                    2f
                )
            );

            Assert.AreEqual(
                GhostPlatformState.Hidden,
                GhostPlatform.EvaluateState(
                    4.5f,
                    3f,
                    1f,
                    2f
                )
            );

            Assert.AreEqual(
                GhostPlatformState.Active,
                GhostPlatform.EvaluateState(
                    6.1f,
                    3f,
                    1f,
                    2f
                )
            );
        }

        [Test]
        public void GhostPlatform_WarningSmoothlyFadesOut()
        {
            float startAlpha =
                GhostPlatform
                    .EvaluateVisibilityAlpha(
                        3f,
                        3f,
                        1f,
                        2f
                    );

            float middleAlpha =
                GhostPlatform
                    .EvaluateVisibilityAlpha(
                        3.5f,
                        3f,
                        1f,
                        2f
                    );

            float endAlpha =
                GhostPlatform
                    .EvaluateVisibilityAlpha(
                        4f,
                        3f,
                        1f,
                        2f
                    );

            Assert.AreEqual(
                1f,
                startAlpha,
                0.0001f
            );

            Assert.AreEqual(
                0.5f,
                middleAlpha,
                0.0001f
            );

            Assert.AreEqual(
                0f,
                endAlpha,
                0.0001f
            );
        }

        [Test]
        public void GhostPlatform_HiddenAlphaIsZero()
        {
            float alpha =
                GhostPlatform
                    .EvaluateVisibilityAlpha(
                        5f,
                        3f,
                        1f,
                        2f
                    );

            Assert.AreEqual(
                0f,
                alpha,
                0.0001f
            );
        }
    }
}
