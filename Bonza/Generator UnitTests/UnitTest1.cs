// Generator UnitTests
//
// 2017-08-04   PV      First version


using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonza.Generator.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private Grille g;

        //[AssemblyInitialize()]
        //public static void AssemblyInit(TestContext context)
        //{
        //    MessageBox.Show("Assembly Initialize " + context.TestResultsDirectory);
        //}

        //[AssemblyCleanup()]
        //public static void AssemblyCleanup()
        //{
        //    MessageBox.Show("AssemblyCleanup");
        //}


        //// ClassInitialize and ClassCleanup are executed once for all tests
        //// Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void ClassInit(TestContext context)
        //{
        //    //MessageBox.Show("ClassInit");
        //}

        //// Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void ClassCleanup()
        //{
        //    //MessageBox.Show("ClassCleanup");
        //}


        // TestInitialize and TestCleanup are executed before each test
        // Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void Initialize()
        {
            g = new Grille();
            g.NewLayout();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void Cleanup()
        {
        }



        [TestMethod]
        public void TestAddWordPositionAndSquares1()
        {
            Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPositionAndSquares(new WordPosition { StartRow = 0, StartColumn = 0, Word = "PIERRE", IsVertical = false }));
            Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPositionAndSquares(new WordPosition { StartRow = -1, StartColumn = 1, Word = "VIOLENT", IsVertical = true }));
            Assert.IsTrue(PlaceWordStatus.TooClose == g.Layout.AddWordPositionAndSquares(new WordPosition { StartRow = 1, StartColumn = 0, Word = "BOB", IsVertical = false }));
            Assert.IsTrue(PlaceWordStatus.Invalid == g.Layout.AddWordPositionAndSquares(new WordPosition { StartRow = 3, StartColumn = 0, Word = "JOE", IsVertical = false }));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddWordPositionAndSquares2()
        {
            Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPositionAndSquares(new WordPosition { StartRow = 0, StartColumn = 0, Word = "PIERRE", IsVertical = false }));
            // Word already in the list of words, must raise ArgumentException
            Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPositionAndSquares(new WordPosition { StartRow = -1, StartColumn = 1, Word = "PIERRE", IsVertical = true }));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddWordPositionAndSquares3()
        {
            WordPosition wp = new WordPosition { StartRow = 0, StartColumn = 0, Word = "PIERRE", IsVertical = false };
            Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPositionAndSquares(wp));
            Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPositionAndSquares(new WordPosition { StartRow = -1, StartColumn = 1, Word = "VIOLENT", IsVertical = true }));
            // WordPosition already placed, must raise ArgumentException
            g.Layout.AddWordPositionAndSquares(wp);
        }

    }
}
