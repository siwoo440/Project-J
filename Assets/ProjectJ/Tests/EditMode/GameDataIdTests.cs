using NUnit.Framework;
using ProjectJ.Data;

namespace ProjectJ.Tests.EditMode
{
    public sealed class GameDataIdTests
    {
        [Test]
        public void Create_ReturnsValidId()
        {
            string id = GameDataId.Create();

            Assert.That(id.Length, Is.EqualTo(32));
            Assert.That(GameDataId.IsValid(id), Is.True);
        }

        [Test]
        public void Create_ReturnsDifferentIds()
        {
            string firstId = GameDataId.Create();
            string secondId = GameDataId.Create();

            Assert.That(firstId, Is.Not.EqualTo(secondId));
        }

        [TestCase("")]
        [TestCase("invalid")]
        [TestCase("1234")]
        public void IsValid_ReturnsFalseForInvalidValue(string value)
        {
            Assert.That(GameDataId.IsValid(value), Is.False);
        }
    }
}
