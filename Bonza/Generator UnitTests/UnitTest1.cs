// Generator UnitTests
//
// 2017-08-04   PV      First version


using System;
using System.Diagnostics;
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
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void Cleanup()
        {
        }



        [TestMethod]
        public void TestAddWordPositionAndSquares1()
        {
            Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPositionAndSquares(new WordPosition("PIERRE", "Pierre", new PositionOrientation(0, 0, false))));
            Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPositionAndSquares(new WordPosition("VIOLENT", "Violent", new PositionOrientation(-1, 1, true))));
            Assert.IsTrue(PlaceWordStatus.TooClose == g.Layout.AddWordPositionAndSquares(new WordPosition("BOB", "Bob", new PositionOrientation(1, 0, false))));
            Assert.IsTrue(PlaceWordStatus.Invalid == g.Layout.AddWordPositionAndSquares(new WordPosition("JOE", "Joe", new PositionOrientation(3, 0, false))));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddWordPositionAndSquares2()
        {
            Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPositionAndSquares(new WordPosition("PIERRE", "Pierre", new PositionOrientation(0, 0, false))));
            // Word already in the list of words, must raise ArgumentException
            g.Layout.AddWordPositionAndSquares(new WordPosition("PIERRE", "Pierre", new PositionOrientation(-1, 1, true)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddWordPositionAndSquares3()
        {
            WordPosition wp = new WordPosition("PIERRE", "Pierre", new PositionOrientation(0, 0, false));
            Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPositionAndSquares(wp));
            Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPositionAndSquares(new WordPosition("VIOLENT", "Violent", new PositionOrientation(-1, 1, true))));
            // WordPosition already placed, must raise ArgumentException
            g.Layout.AddWordPositionAndSquares(wp);
        }

        [TestMethod]
        public void TestBoundingRectangle()
        {
            g.Layout.AddWordPositionAndSquares(new WordPosition("THURSDAY", "thursday", new PositionOrientation(0, 0, true)));
            g.Layout.AddWordPositionAndSquares(new WordPosition("MONDAY", "monday", new PositionOrientation(5, -3, false)));
            g.Layout.AddWordPositionAndSquares(new WordPosition("TUESDAY", "tuesday", new PositionOrientation(2, -1, false)));
            g.Layout.AddWordPositionAndSquares(new WordPosition("WEDNESDAY", "wednesday", new PositionOrientation(-4, 3, true)));

            BoundingRectangle r1 = g.Layout.GetBounds();
            BoundingRectangle r2 = new BoundingRectangle(-4, 7, -3, 5);
            Assert.AreEqual(r1, r2);
            Assert.IsTrue(r1.MinRow == -4 && r1.MaxRow == 7 && r1.MinColumn == -3 && r1.MaxColumn == 5);
        }
    }
}
