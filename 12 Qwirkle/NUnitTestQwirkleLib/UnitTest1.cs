using NUnit.Framework;
using QwirkleLib;

namespace NUnitTestQwirkleLib
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var t = new QTile(3, 1);
            Assert.IsTrue(t.Shape == 3 && t.Color == 1);
        }
    }
}