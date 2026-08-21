using NUnit.Framework;
using ProjectJ.Obstacles;
using ProjectJ.Push;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class AirBagExternalForceTests
    {
        [Test]
        public void ExternalForceReceiver_SumsPushAndAirBag()
        {
            GameObject player =
                CreateExternalForcePlayer(
                    out Rigidbody body,
                    out PlayerExternalForceAccumulator
                        accumulator,
                    out PlayerExternalForceReceiver
                        receiver
                );

            try
            {
                body.linearVelocity =
                    new Vector3(
                        0f,
                        5f,
                        0f
                    );

                bool pushApplied =
                    receiver.TryApplyVelocityChange(
                        ExternalForceSource.Push,
                        new Vector3(
                            4f,
                            99f,
                            0f
                        )
                    );

                bool airBagApplied =
                    receiver.TryApplyVelocityChange(
                        ExternalForceSource.AirBag,
                        new Vector3(
                            0f,
                            -99f,
                            6f
                        )
                    );

                Assert.IsTrue(
                    pushApplied
                );

                Assert.IsTrue(
                    airBagApplied
                );

                Assert.AreEqual(
                    new Vector3(
                        4f,
                        0f,
                        6f
                    ),
                    accumulator
                        .CurrentExternalVelocity
                );

                Assert.AreEqual(
                    4f,
                    body.linearVelocity.x,
                    0.0001f
                );

                Assert.AreEqual(
                    5f,
                    body.linearVelocity.y,
                    0.0001f
                );

                Assert.AreEqual(
                    6f,
                    body.linearVelocity.z,
                    0.0001f
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
        public void ExternalForceReceiver_ResultIsOrderIndependent()
        {
            Vector3 firstOrder =
                ApplyTwoForces(
                    true
                );

            Vector3 secondOrder =
                ApplyTwoForces(
                    false
                );

            Assert.AreEqual(
                firstOrder.x,
                secondOrder.x,
                0.0001f
            );

            Assert.AreEqual(
                firstOrder.z,
                secondOrder.z,
                0.0001f
            );
        }

        [Test]
        public void AirBagDirection_CenterContactUsesForward()
        {
            GameObject airBag =
                new GameObject(
                    "AirBag"
                );

            try
            {
                Vector3 direction =
                    AirBagObstacle
                        .CalculatePushDirection(
                            airBag.transform,
                            airBag.transform.position +
                                Vector3.forward,
                            Vector3.forward,
                            0.35f
                        );

                Assert.AreEqual(
                    0f,
                    direction.x,
                    0.0001f
                );

                Assert.AreEqual(
                    0f,
                    direction.y,
                    0.0001f
                );

                Assert.AreEqual(
                    1f,
                    direction.z,
                    0.0001f
                );
            }
            finally
            {
                Object.DestroyImmediate(
                    airBag
                );
            }
        }

        [Test]
        public void AirBagDirection_EdgeContactAddsLateralSpread()
        {
            GameObject airBag =
                new GameObject(
                    "AirBag"
                );

            try
            {
                Vector3 direction =
                    AirBagObstacle
                        .CalculatePushDirection(
                            airBag.transform,
                            airBag.transform.position +
                                new Vector3(
                                    2f,
                                    0f,
                                    1f
                                ),
                            Vector3.forward,
                            0.5f
                        );

                Assert.Greater(
                    direction.x,
                    0f
                );

                Assert.Greater(
                    direction.z,
                    0f
                );

                Assert.AreEqual(
                    0f,
                    direction.y,
                    0.0001f
                );
            }
            finally
            {
                Object.DestroyImmediate(
                    airBag
                );
            }
        }

        [Test]
        public void AirBagDirection_RotatesWithInstallation()
        {
            GameObject airBag =
                new GameObject(
                    "AirBag"
                );

            try
            {
                airBag.transform.rotation =
                    Quaternion.Euler(
                        0f,
                        90f,
                        0f
                    );

                Vector3 direction =
                    AirBagObstacle
                        .CalculatePushDirection(
                            airBag.transform,
                            airBag.transform.position +
                                Vector3.right,
                            Vector3.forward,
                            0f
                        );

                Assert.AreEqual(
                    1f,
                    direction.x,
                    0.0001f
                );

                Assert.AreEqual(
                    0f,
                    direction.y,
                    0.0001f
                );

                Assert.AreEqual(
                    0f,
                    direction.z,
                    0.0001f
                );
            }
            finally
            {
                Object.DestroyImmediate(
                    airBag
                );
            }
        }

        private static Vector3 ApplyTwoForces(
            bool pushFirst
        )
        {
            GameObject player =
                CreateExternalForcePlayer(
                    out Rigidbody body,
                    out PlayerExternalForceAccumulator
                        accumulator,
                    out PlayerExternalForceReceiver
                        receiver
                );

            try
            {
                Vector3 push =
                    new Vector3(
                        7f,
                        0f,
                        0f
                    );

                Vector3 airBag =
                    new Vector3(
                        0f,
                        0f,
                        9f
                    );

                if (pushFirst)
                {
                    receiver.TryApplyVelocityChange(
                        ExternalForceSource.Push,
                        push
                    );

                    receiver.TryApplyVelocityChange(
                        ExternalForceSource.AirBag,
                        airBag
                    );
                }
                else
                {
                    receiver.TryApplyVelocityChange(
                        ExternalForceSource.AirBag,
                        airBag
                    );

                    receiver.TryApplyVelocityChange(
                        ExternalForceSource.Push,
                        push
                    );
                }

                return
                    accumulator
                        .CurrentExternalVelocity;
            }
            finally
            {
                Object.DestroyImmediate(
                    player
                );
            }
        }

        private static GameObject
            CreateExternalForcePlayer(
                out Rigidbody body,
                out PlayerExternalForceAccumulator
                    accumulator,
                out PlayerExternalForceReceiver
                    receiver
            )
        {
            GameObject player =
                new GameObject(
                    "External Force Player"
                );

            body =
                player.AddComponent<
                    Rigidbody
                >();

            body.useGravity =
                false;

            accumulator =
                player.AddComponent<
                    PlayerExternalForceAccumulator
                >();

            accumulator.Configure(
                body,
                12f,
                0.05f
            );

            receiver =
                player.AddComponent<
                    PlayerExternalForceReceiver
                >();

            receiver.Configure(
                body,
                null,
                accumulator
            );

            return player;
        }
    }
}
