using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProjectJ.Tests.PlayMode
{
    public sealed class ProjectSmokeTests
    {
        [UnityTest]
        public IEnumerator PlayMode_StartsAndAdvancesOneFrame()
        {
            yield return null;

            Assert.That(Application.isPlaying, Is.True);
        }
    }
}
