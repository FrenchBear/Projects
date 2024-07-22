// Generator UnitTests
//
// 2017-08-04   PV      First version
// 2021-11-13   PV      Net6 C#10

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonza.Generator.UnitTests;

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
    public void Initialize() => g = new Grille(123);        // Use seed to be reproducible

    // Use TestCleanup to run code after each test has run
    [TestCleanup()]
    public void Cleanup()
    {
    }

    [TestMethod]
    public void TestAddWordPositionAndSquares1()
    {
        Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPosition(new WordPosition("PIERRE", "Pierre", new PositionOrientation(0, 0, false))));
        Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPosition(new WordPosition("VIOLENT", "Violent", new PositionOrientation(-1, 1, true))));
        Assert.IsTrue(PlaceWordStatus.TooClose == g.Layout.AddWordPosition(new WordPosition("BOB", "Bob", new PositionOrientation(1, 0, false))));
        Assert.IsTrue(PlaceWordStatus.Invalid == g.Layout.AddWordPosition(new WordPosition("JOE", "Joe", new PositionOrientation(3, 0, false))));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestAddWordPositionAndSquares2()
    {
        var wp = new WordPosition("PIERRE", "Pierre", new PositionOrientation(0, 0, false));
        Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPosition(wp));
        Assert.IsTrue(PlaceWordStatus.Valid == g.Layout.AddWordPosition(new WordPosition("VIOLENT", "Violent", new PositionOrientation(-1, 1, true))));
        // WordPosition already placed, must raise ArgumentException
        g.Layout.AddWordPosition(wp);
    }

    [TestMethod]
    public void TestBoundingRectangle()
    {
        g.Layout.AddWordPosition(new WordPosition("THURSDAY", "Thursday", new PositionOrientation(0, 0, true)));
        g.Layout.AddWordPosition(new WordPosition("MONDAY", "Monday", new PositionOrientation(5, -3, false)));
        g.Layout.AddWordPosition(new WordPosition("TUESDAY", "Tuesday", new PositionOrientation(2, -1, false)));
        g.Layout.AddWordPosition(new WordPosition("WEDNESDAY", "Wednesday", new PositionOrientation(-4, 3, true)));

        BoundingRectangle r1 = g.Layout.Bounds;
        var r2 = new BoundingRectangle(-4, 7, -3, 5);
        Assert.AreEqual(r1, r2);
        Assert.IsTrue(r1.Min.Row == -4 && r1.Max.Row == 7 && r1.Min.Column == -3 && r1.Max.Column == 5);
    }

    [TestMethod]
    public void TestCoverage()
    {
        g.AddWordsFromFile(@"..\..\Lists\Prénoms.txt");
        WordPosition Pierre = g.Layout.WordPositionList.First(wp => string.Equals(wp.OriginalWord, "Pierre", StringComparison.OrdinalIgnoreCase));
        g.Layout.RemoveWordPosition(Pierre);
        g.Layout.AddWordPosition(Pierre);
        g.PlaceWordsAgain();
        int n = g.Layout.WordsNotConnectedCount();
        WordPosition w1 = g.Layout.WordPositionList.First(wp => string.Equals(wp.OriginalWord, "Barthélémy", StringComparison.OrdinalIgnoreCase));
        var l = g.Layout.GetConnectedWordPositions(w1);
        g.Print(@"C:\temp\Prénoms.Layout.txt");
        g.Layout.SaveLayoutAsCode(@"C:\temp\Prénoms.Layout.cs");
    }
}
