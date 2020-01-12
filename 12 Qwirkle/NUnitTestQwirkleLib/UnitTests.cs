using NUnit.Framework;
using QwirkleLib;

#nullable enable

namespace NUnitTestQwirkleLib
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1_NewQTile()
        {
            var t = new QTile("D2");
            Assert.IsTrue(t.Shape == 3 && t.Color == 1);
        }

        [Test]
        public void Test2_AddTile()
        {
            var b = new Board();
            b.AddTile((0, 0), new QTile("A3"));
            b.AddTile((0, 1), new QTile("A4"));

            // +-------+-------+-------+-------+
            // |       |       |       |       |
            // |       |Unknown|Unknown|       |
            // |       |       |       |       |
            // +-------+-------+-------+-------+
            // |       |       |       |       |
            // |Unknown|  A 3  |  A 4  |Unknown|
            // |       |       |       |       |
            // +-------+-------+-------+-------+
            // |       |       |       |       |
            // |       |Unknown|Unknown|       |
            // |       |       |       |       |
            // +-------+-------+-------+-------+

            Assert.AreEqual("", b[(-1, -1)].ToString());
            Assert.AreEqual("u", b[(-1, 0)].ToString());
            Assert.AreEqual("u", b[(-1, 1)].ToString());
            Assert.AreEqual("", b[(-1, 2)].ToString());
            Assert.AreEqual("u", b[(0, -1)].ToString());
            Assert.AreEqual("A3", b[(0, 0)].ToString());
            Assert.AreEqual("A4", b[(0, 1)].ToString());
            Assert.AreEqual("u", b[(0, 2)].ToString());
            Assert.AreEqual("", b[(1, -1)].ToString());
            Assert.AreEqual("u", b[(1, 0)].ToString());
            Assert.AreEqual("u", b[(1, 1)].ToString());
            Assert.AreEqual("", b[(1, 2)].ToString());
        }

        [Test]
        public void Test3a_AddTileUpdateBoardPlayability()
        {
            var b = new Board();
            b.AddTile((0, 0), "A3");
            b.AddTile((0, 1), "A4");
            b.UpdateBoardPlayability();

            // +D-1,-1-+B-1,0--+B-1,1--+D-1,2--+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // |       |Unconst|Unconst|       |
            // |       |Unconst|Unconst|       |
            // +B0,-1--+B0,0---+B0,1---+B0,2---+
            // |Playabl|       |       |Playabl|
            // |Unconst|  A 3  |  A 4  |Unconst|
            // |Unconst|       |       |Unconst|
            // |A12__56|       |       |A12__56|
            // |Impossi|       |       |Impossi|
            // +D1,-1--+B1,0---+B1,1---+D1,2---+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // |       |Unconst|Unconst|       |
            // |       |Unconst|Unconst|       |
            // +-------+-------+-------+-------+

            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(-1, 0)].ToString());
            Assert.AreEqual("p,A123_56,4_BCDEF,Unconst,Unconst", b[(-1, 1)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12__56,Impossi", b[(0, -1)].ToString());
            Assert.AreEqual("A3", b[(0, 0)].ToString());
            Assert.AreEqual("A4", b[(0, 1)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12__56,Impossi", b[(0, 2)].ToString());
            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(1, 0)].ToString());
            Assert.AreEqual("p,A123_56,4_BCDEF,Unconst,Unconst", b[(1, 1)].ToString());
        }

        [Test]
        public void Test3b_AddTileUpdateBoardPlayability()
        {
            var b = new Board();
            b.AddTile((0, 0), "A1");
            b.AddTile((0, 2), "B2");
            b.UpdateBoardPlayability();

            // +D-1,-1-+B-1,0--+D-1,1--+B-1,2--+D-1,3--+
            // |       |Playabl|       |Playabl|       |
            // |       |A_23456|       |B1_3456|       |
            // |       |1_BCDEF|       |2A_CDEF|       |
            // |       |Unconst|       |Unconst|       |
            // |       |Unconst|       |Unconst|       |
            // +B0,-1--+B0,0---+B0,1---+B0,2---+B0,3---+
            // |Playabl|       |       |       |Playabl|
            // |Unconst|  A 1  |Blocked|  B 2  |Unconst|
            // |Unconst|       |       |       |Unconst|
            // |A_23456|       |       |       |B1_3456|
            // |1_BCDEF|       |       |       |2A_CDEF|
            // +D1,-1--+B1,0---+D1,1---+B1,2---+D1,3---+
            // |       |Playabl|       |Playabl|       |
            // |       |A_23456|       |B1_3456|       |
            // |       |1_BCDEF|       |2A_CDEF|       |
            // |       |Unconst|       |Unconst|       |
            // |       |Unconst|       |Unconst|       |
            // +-------+-------+-------+-------+-------+

            Assert.AreEqual("p,A_23456,1_BCDEF,Unconst,Unconst", b[(-1, 0)].ToString());
            Assert.AreEqual("p,B1_3456,2A_CDEF,Unconst,Unconst", b[(-1, 2)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A_23456,1_BCDEF", b[(0, -1)].ToString());
            Assert.AreEqual("A1", b[(0, 0)].ToString());
            Assert.AreEqual("b", b[(0, 1)].ToString());
            Assert.AreEqual("B2", b[(0, 2)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,B1_3456,2A_CDEF", b[(0, 3)].ToString());
            Assert.AreEqual("p,A_23456,1_BCDEF,Unconst,Unconst", b[(1, 0)].ToString());
            Assert.AreEqual("p,B1_3456,2A_CDEF,Unconst,Unconst", b[(1, 2)].ToString());
        }


        [Test]
        public void Test4a_PlayAndRollback()
        {
            var b = new Board();
            b.AddTile((0, 0), "A3");
            b.UpdateBoardPlayability();

            // +D-1,-1-+B-1,0--+D-1,1--+
            // |       |Playabl|       |
            // |       |A12_456|       |
            // |       |3_BCDEF|       |
            // |       |Unconst|       |
            // |       |Unconst|       |
            // +B0,-1--+B0,0---+B0,1---+
            // |Playabl|       |Playabl|
            // |Unconst|  A 3  |Unconst|
            // |Unconst|       |Unconst|
            // |A12_456|       |A12_456|
            // |3_BCDEF|       |3_BCDEF|
            // +D1,-1--+B1,0---+D1,1---+
            // |       |Playabl|       |
            // |       |A12_456|       |
            // |       |3_BCDEF|       |
            // |       |Unconst|       |
            // |       |Unconst|       |
            // +-------+-------+-------+

            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(-1, 0)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12_456,3_BCDEF", b[(0, -1)].ToString());
            Assert.AreEqual("A3", b[(0, 0)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12_456,3_BCDEF", b[(0, 1)].ToString());
            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(1, 0)].ToString());

            b.PlayTile((0, 1), "A4");
            b.UpdatePlayedPlayability();

            // +D-1,-1-+B-1,0--+P-1,1--+D-1,2--+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // |       |Unconst|Unconst|       |
            // |       |Unconst|Unconst|       |
            // +P0,-1--+B0,0---+P0,1---+P0,2---+
            // |Playabl|       |       |Playabl|
            // |Unconst|  A 3  |  A 4  |Unconst|
            // |Unconst|       |       |Unconst|
            // |A12__56|       |       |A12__56|
            // |Impossi|       |       |Impossi|
            // +D1,-1--+B1,0---+P1,1---+D1,2---+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // |       |Unconst|Unconst|       |
            // |       |Unconst|Unconst|       |
            // +-------+-------+-------+-------+

            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(-1, 0)].ToString());
            Assert.AreEqual("p,A123_56,4_BCDEF,Unconst,Unconst", b[(-1, 1)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12__56,Impossi", b[(0, -1)].ToString());
            Assert.AreEqual("A3", b[(0, 0)].ToString());
            Assert.AreEqual("A4", b[(0, 1)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12__56,Impossi", b[(0, 2)].ToString());
            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(1, 0)].ToString());
            Assert.AreEqual("p,A123_56,4_BCDEF,Unconst,Unconst", b[(1, 1)].ToString());

            b.RollbackPlay();

            // +D-1,-1-+B-1,0--+D-1,1--+
            // |       |Playabl|       |
            // |       |A12_456|       |
            // |       |3_BCDEF|       |
            // |       |Unconst|       |
            // |       |Unconst|       |
            // +B0,-1--+B0,0---+B0,1---+
            // |Playabl|       |Playabl|
            // |Unconst|  A 3  |Unconst|
            // |Unconst|       |Unconst|
            // |A12_456|       |A12_456|
            // |3_BCDEF|       |3_BCDEF|
            // +D1,-1--+B1,0---+D1,1---+
            // |       |Playabl|       |
            // |       |A12_456|       |
            // |       |3_BCDEF|       |
            // |       |Unconst|       |
            // |       |Unconst|       |
            // +-------+-------+-------+

            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(-1, 0)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12_456,3_BCDEF", b[(0, -1)].ToString());
            Assert.AreEqual("A3", b[(0, 0)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12_456,3_BCDEF", b[(0, 1)].ToString());
            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(1, 0)].ToString());
        }


        [Test]
        public void Test4b_PlayAndCommit()
        {
            var b = new Board();
            b.AddTile((0, 0), "A3");
            b.UpdateBoardPlayability();

            // +D-1,-1-+B-1,0--+D-1,1--+
            // |       |Playabl|       |
            // |       |A12_456|       |
            // |       |3_BCDEF|       |
            // |       |Unconst|       |
            // |       |Unconst|       |
            // +B0,-1--+B0,0---+B0,1---+
            // |Playabl|       |Playabl|
            // |Unconst|  A 3  |Unconst|
            // |Unconst|       |Unconst|
            // |A12_456|       |A12_456|
            // |3_BCDEF|       |3_BCDEF|
            // +D1,-1--+B1,0---+D1,1---+
            // |       |Playabl|       |
            // |       |A12_456|       |
            // |       |3_BCDEF|       |
            // |       |Unconst|       |
            // |       |Unconst|       |
            // +-------+-------+-------+

            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(-1, 0)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12_456,3_BCDEF", b[(0, -1)].ToString());
            Assert.AreEqual("A3", b[(0, 0)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12_456,3_BCDEF", b[(0, 1)].ToString());
            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(1, 0)].ToString());


            b.PlayTile((0, 1), "A4");
            b.UpdatePlayedPlayability();

            // +D-1,-1-+B-1,0--+P-1,1--+D-1,2--+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // |       |Unconst|Unconst|       |
            // |       |Unconst|Unconst|       |
            // +P0,-1--+B0,0---+P0,1---+P0,2---+
            // |Playabl|       |       |Playabl|
            // |Unconst|  A 3  |  A 4  |Unconst|
            // |Unconst|       |       |Unconst|
            // |A12__56|       |       |A12__56|
            // |Impossi|       |       |Impossi|
            // +D1,-1--+B1,0---+P1,1---+D1,2---+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // |       |Unconst|Unconst|       |
            // |       |Unconst|Unconst|       |
            // +-------+-------+-------+-------+

            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(-1, 0)].ToString());
            Assert.AreEqual("p,A123_56,4_BCDEF,Unconst,Unconst", b[(-1, 1)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12__56,Impossi", b[(0, -1)].ToString());
            Assert.AreEqual("A3", b[(0, 0)].ToString());
            Assert.AreEqual("A4", b[(0, 1)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12__56,Impossi", b[(0, 2)].ToString());
            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(1, 0)].ToString());
            Assert.AreEqual("p,A123_56,4_BCDEF,Unconst,Unconst", b[(1, 1)].ToString());


            b.CommitPlay();

            // +D-1,-1-+B-1,0--+B-1,1--+D-1,2--+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // |       |Unconst|Unconst|       |
            // |       |Unconst|Unconst|       |
            // +B0,-1--+B0,0---+B0,1---+B0,2---+
            // |Playabl|       |       |Playabl|
            // |Unconst|  A 3  |  A 4  |Unconst|
            // |Unconst|       |       |Unconst|
            // |A12__56|       |       |A12__56|
            // |Impossi|       |       |Impossi|
            // +D1,-1--+B1,0---+B1,1---+D1,2---+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // |       |Unconst|Unconst|       |
            // |       |Unconst|Unconst|       |
            // +-------+-------+-------+-------+

            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(-1, 0)].ToString());
            Assert.AreEqual("p,A123_56,4_BCDEF,Unconst,Unconst", b[(-1, 1)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12__56,Impossi", b[(0, -1)].ToString());
            Assert.AreEqual("A3", b[(0, 0)].ToString());
            Assert.AreEqual("A4", b[(0, 1)].ToString());
            Assert.AreEqual("p,Unconst,Unconst,A12__56,Impossi", b[(0, 2)].ToString());
            Assert.AreEqual("p,A12_456,3_BCDEF,Unconst,Unconst", b[(1, 0)].ToString());
            Assert.AreEqual("p,A123_56,4_BCDEF,Unconst,Unconst", b[(1, 1)].ToString());
        }


        [Test]
        public void Test5a_CanPlay()
        {
            var b = new Board();
            b.AddTile((0, 0), "A3");
            b.AddTile((0, 1), "A4");
            b.AddTile((0, 3), "B1");
            b.UpdateBoardPlayability();

            // +D-1,-1-+B-1,0--+B-1,1--+D-1,2--+B-1,3--+D-1,4--+
            // |       |Playabl|Playabl|       |Playabl|       |
            // |       |A12_456|A123_56|       |B_23456|       |
            // |       |3_BCDEF|4_BCDEF|       |1A_CDEF|       |
            // |       |Unconst|Unconst|       |Unconst|       |
            // |       |Unconst|Unconst|       |Unconst|       |
            // +B0,-1--+B0,0---+B0,1---+B0,2---+B0,3---+B0,4---+
            // |Playabl|       |       |       |       |Playabl|
            // |Unconst|  A 3  |  A 4  |Blocked|  B 1  |Unconst|
            // |Unconst|       |       |       |       |Unconst|
            // |A12__56|       |       |       |       |B_23456|
            // |Impossi|       |       |       |       |1A_CDEF|
            // +D1,-1--+B1,0---+B1,1---+D1,2---+B1,3---+D1,4---+
            // |       |Playabl|Playabl|       |Playabl|       |
            // |       |A12_456|A123_56|       |B_23456|       |
            // |       |3_BCDEF|4_BCDEF|       |1A_CDEF|       |
            // |       |Unconst|Unconst|       |Unconst|       |
            // |       |Unconst|Unconst|       |Unconst|       |
            // +-------+-------+-------+-------+-------+-------+

            bool cp1 = b.CanPlayTile((0, 0), "B4", out string msg1);
            bool cp2 = b.CanPlayTile((1, 2), "B4", out string msg2);
            bool cp3 = b.CanPlayTile((0, 2), "B4", out string msg3);
            bool cp4 = b.CanPlayTile((0, -1), "B4", out string msg4);
            Assert.IsFalse(cp1);
            Assert.AreEqual("Il y a déjà une tuile à cet emplacement", msg1);
            Assert.IsFalse(cp2);
            Assert.AreEqual("Il y n'a pas de tuile adjecente à cet emplacement", msg2);
            Assert.IsFalse(cp3);
            Assert.AreEqual("Cet emplacement n'est pas jouable", msg3);
            Assert.IsFalse(cp4);
            Assert.AreEqual("Cette tuile ne respecte pas les contraintes de colonne", msg4);
        }

        [Test]
        public void Test5b_CanPlay()
        {
            var b = new Board();
            b.AddTile((0, 0), "A1");
            b.AddTile((0, 2), "A2");
            b.UpdateBoardPlayability();
            b.PlayTile((0, 3), "A3");
            b.UpdatePlayedPlayability();

            // +D-1,-1-+B-1,0--+D-1,1--+B-1,2--+P-1,3--+D-1,4--+
            // |       |Playabl|       |Playabl|Playabl|       |
            // |       |A_23456|       |A1_3456|A12_456|       |
            // |       |1_BCDEF|       |2_BCDEF|3_BCDEF|       |
            // |       |Unconst|       |Unconst|Unconst|       |
            // |       |Unconst|       |Unconst|Unconst|       |
            // +B0,-1--+B0,0---+P0,1---+B0,2---+P0,3---+P0,4---+
            // |Playabl|       |Playabl|       |       |Playabl|
            // |Unconst|  A 1  |Unconst|  A 2  |  A 3  |Unconst|
            // |Unconst|       |Unconst|       |       |Unconst|
            // |A_23456|       |A___456|       |       |A1__456|
            // |1_BCDEF|       |Impossi|       |       |Impossi|
            // +D1,-1--+B1,0---+D1,1---+B1,2---+P1,3---+D1,4---+
            // |       |Playabl|       |Playabl|Playabl|       |
            // |       |A_23456|       |A1_3456|A12_456|       |
            // |       |1_BCDEF|       |2_BCDEF|3_BCDEF|       |
            // |       |Unconst|       |Unconst|Unconst|       |
            // |       |Unconst|       |Unconst|Unconst|       |
            // +-------+-------+-------+-------+-------+-------+

            bool cp1 = b.CanPlayTile((0, 1), "A4", out _);
            Assert.IsTrue(cp1);
            bool cp2 = b.CanPlayTile((0, -1), "A4", out string msg2);
            Assert.IsFalse(cp2);
            Assert.AreEqual("Pas de trou entre les tuiles jouées", msg2);
            bool cp3 = b.CanPlayTile((1, 0), "A2", out string msg3);
            Assert.IsFalse(cp3);
            Assert.AreEqual("Les tuiles doivent être jouées dans une seule ligne ou colonne", msg3);
        }


        [Test]
        public void Test6a_Points()
        {
            var b = new Board();
            b.AddTile((0, 0), "A1");
            b.AddTile((0, 1), "A2");
            b.AddTile((1, 0), "A3");
            b.PlayTile((1, 1), "A4");
            Assert.AreEqual(4, b.PlayPoints());
        }

        [Test]
        public void Test6b_12Points1()
        {
            var b = new Board();
            b.AddTile((0, 0), "A1");
            b.AddTile((0, 1), "A2");
            b.AddTile((0, 2), "A3");
            b.AddTile((0, 3), "A4");
            b.AddTile((0, 4), "A5");
            b.PlayTile((0, 5), "A6");
            Assert.AreEqual(12, b.PlayPoints());
        }

        [Test]
        public void Test6b_12Points2()
        {
            var b = new Board();
            b.AddTile((0, 0), "A1");
            b.AddTile((0, 2), "A3");
            b.AddTile((0, 4), "A5");
            b.PlayTile((0, 1), "A2");
            b.PlayTile((0, 3), "A4");
            b.PlayTile((0, 5), "A6");
            Assert.AreEqual(12, b.PlayPoints());
        }

        [Test]
        public void Test6c_84Points()
        {
            var b = new Board();
            b.AddTile((0, 0), "A1"); b.AddTile((0, 1), "A2"); b.AddTile((0, 2), "A3"); b.AddTile((0, 3), "A4"); b.AddTile((0, 4), "A5"); b.AddTile((0, 5), "A6");
            b.AddTile((1, 0), "B1"); b.AddTile((1, 1), "B2"); b.AddTile((1, 2), "B3"); b.AddTile((1, 3), "B4"); b.AddTile((1, 4), "B5"); b.AddTile((1, 5), "B6");
            b.AddTile((2, 0), "C1"); b.AddTile((2, 1), "C2"); b.AddTile((2, 2), "C3"); b.AddTile((2, 3), "C4"); b.AddTile((2, 4), "C5"); b.AddTile((2, 5), "C6");
            b.AddTile((3, 0), "D1"); b.AddTile((3, 1), "D2"); b.AddTile((3, 2), "D3"); b.AddTile((3, 3), "D4"); b.AddTile((3, 4), "D5"); b.AddTile((3, 5), "D6");
            b.AddTile((4, 0), "E1"); b.AddTile((4, 1), "E2"); b.AddTile((4, 2), "E3"); b.AddTile((4, 3), "E4"); b.AddTile((4, 4), "E5"); b.AddTile((4, 5), "E6");
            b.PlayTile((5, 0), "F1"); b.PlayTile((5, 1), "F2"); b.PlayTile((5, 2), "F3"); b.PlayTile((5, 3), "F4"); b.PlayTile((5, 4), "F5"); b.PlayTile((5, 5), "F6");
            Assert.AreEqual(84, b.PlayPoints());
        }

    }
}
