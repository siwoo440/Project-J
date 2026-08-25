using NUnit.Framework;
using ProjectJ.Items;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJHandMirrorPolicyTests
    {
        [Test]
        public void Constants_MatchDay131Design()
        {
            Assert.AreEqual(
                30,
                ProjectJHandMirrorPolicy.NetworkItemId
            );

            Assert.AreEqual(
                4f,
                ProjectJHandMirrorPolicy.DurationSeconds,
                0.0001f
            );

            Assert.Greater(
                ProjectJHandMirrorPolicy.ReflectionSeparationMeters,
                0f
            );
        }

        [TestCase(true, true, true, true)]
        [TestCase(false, true, true, false)]
        [TestCase(true, false, true, false)]
        [TestCase(true, true, false, false)]
        public void CanActivate_RequiresAuthorityRunnerAndGameplay(
            bool authorityReady,
            bool runnerReady,
            bool gameplayAllowed,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJHandMirrorPolicy.CanActivate(
                    authorityReady,
                    runnerReady,
                    gameplayAllowed
                )
            );
        }

        [TestCase(true, true, true, false, false, true)]
        [TestCase(false, true, true, false, false, false)]
        [TestCase(true, false, true, false, false, false)]
        [TestCase(true, true, false, false, false, false)]
        [TestCase(true, true, true, true, false, false)]
        [TestCase(true, true, true, false, true, false)]
        public void CanReflect_RejectsInvalidStates(
            bool authorityReady,
            bool mirrorActive,
            bool gameplayAllowed,
            bool isIncomingOwner,
            bool isRewinding,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJHandMirrorPolicy.CanReflect(
                    authorityReady,
                    mirrorActive,
                    gameplayAllowed,
                    isIncomingOwner,
                    isRewinding
                )
            );
        }

        [Test]
        public void ResolveReflectedDirection_ReversesFullThreeDimensionalDirection()
        {
            Vector3 incoming =
                new Vector3(
                    1f,
                    2f,
                    -3f
                );

            Vector3 expected =
                -incoming.normalized;

            Vector3 actual =
                ProjectJHandMirrorPolicy.ResolveReflectedDirection(
                    incoming,
                    Vector3.right
                );

            Assert.That(
                Vector3.Distance(
                    expected,
                    actual
                ),
                Is.LessThan(0.0001f)
            );
        }

        [Test]
        public void ResolveReflectedDirection_UsesFallbackForZeroDirection()
        {
            Vector3 actual =
                ProjectJHandMirrorPolicy.ResolveReflectedDirection(
                    Vector3.zero,
                    Vector3.right
                );

            Assert.That(
                Vector3.Distance(
                    Vector3.left,
                    actual
                ),
                Is.LessThan(0.0001f)
            );
        }

        [Test]
        public void ResolveSeparatedPosition_MovesAwayAlongReflectedDirection()
        {
            Vector3 contact =
                new Vector3(
                    10f,
                    2f,
                    -4f
                );

            Vector3 actual =
                ProjectJHandMirrorPolicy.ResolveSeparatedPosition(
                    contact,
                    Vector3.left
                );

            Assert.AreEqual(
                ProjectJHandMirrorPolicy.ReflectionSeparationMeters,
                Vector3.Distance(
                    contact,
                    actual
                ),
                0.0001f
            );

            Assert.Less(
                actual.x,
                contact.x
            );
        }

        [TestCase(true, true, true, false, true)]
        [TestCase(false, true, true, false, false)]
        [TestCase(true, false, true, false, false)]
        [TestCase(true, true, false, false, false)]
        [TestCase(true, true, true, true, false)]
        public void ShouldPreferPreviousOwnerAsTarget_RequiresValidTrackableFormerOwner(
            bool previousOwnerExists,
            bool previousOwnerGameplayAllowed,
            bool previousOwnerTrackable,
            bool previousOwnerIsNewOwner,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJHandMirrorPolicy.ShouldPreferPreviousOwnerAsTarget(
                    previousOwnerExists,
                    previousOwnerGameplayAllowed,
                    previousOwnerTrackable,
                    previousOwnerIsNewOwner
                )
            );
        }
    }
}
