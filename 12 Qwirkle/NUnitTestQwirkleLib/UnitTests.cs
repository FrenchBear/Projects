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

            Assert.AreEqual(b[(-1, -1)].ToString(), "");
            Assert.AreEqual(b[(-1, 0)].ToString(), "u");
            Assert.AreEqual(b[(-1, 1)].ToString(), "u");
            Assert.AreEqual(b[(-1, 2)].ToString(), "");
            Assert.AreEqual(b[(0, -1)].ToString(), "u");
            Assert.AreEqual(b[(0, 0)].ToString(), "A3");
            Assert.AreEqual(b[(0, 1)].ToString(), "A4");
            Assert.AreEqual(b[(0, 2)].ToString(), "u");
            Assert.AreEqual(b[(1, -1)].ToString(), "");
            Assert.AreEqual(b[(1, 0)].ToString(), "u");
            Assert.AreEqual(b[(1, 1)].ToString(), "u");
            Assert.AreEqual(b[(1, 2)].ToString(), "");
        }

        [Test]
        public void Test3a_AddTileUpdateBoardPlayability()
        {
            var b = new Board();
            b.AddTile((0, 0), "A3");
            b.AddTile((0, 1), "A4");
            b.UpdateBoardPlayability();

            // +-------+-------+-------+-------+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // +-------+-------+-------+-------+
            // |Playabl|       |       |Playabl|
            // |A12__56|  A 3  |  A 4  |A12__56|
            // |N/A    |       |       |N/A    |
            // +-------+-------+-------+-------+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // +-------+-------+-------+-------+

            Assert.AreEqual(b[(-1, -1)].ToString(), "");
            Assert.AreEqual(b[(-1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(-1, 1)].ToString(), "p,A123_56,4_BCDEF");
            Assert.AreEqual(b[(-1, 2)].ToString(), "");
            Assert.AreEqual(b[(0, -1)].ToString(), "p,A12__56,N/A    ");
            Assert.AreEqual(b[(0, 0)].ToString(), "A3");
            Assert.AreEqual(b[(0, 1)].ToString(), "A4");
            Assert.AreEqual(b[(0, 2)].ToString(), "p,A12__56,N/A    ");
            Assert.AreEqual(b[(1, -1)].ToString(), "");
            Assert.AreEqual(b[(1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(1, 1)].ToString(), "p,A123_56,4_BCDEF");
            Assert.AreEqual(b[(1, 2)].ToString(), "");
        }

        [Test]
        public void Test3b_AddTileUpdateBoardPlayability()
        {
            var b = new Board();
            b.AddTile((0, 0), "A1");
            b.AddTile((0, 2), "B2");
            b.UpdateBoardPlayability();

            // +-------+-------+-------+-------+-------+
            // |       |Playabl|       |Playabl|       |
            // |       |A_23456|       |B1_3456|       |
            // |       |1_BCDEF|       |2A_CDEF|       |
            // +-------+-------+-------+-------+-------+
            // |Playabl|       |       |       |Playabl|
            // |A_23456|  A 1  |N/A    |  B 2  |B1_3456|
            // |1_BCDEF|       |       |       |2A_CDEF|
            // +-------+-------+-------+-------+-------+
            // |       |Playabl|       |Playabl|       |
            // |       |A_23456|       |B1_3456|       |
            // |       |1_BCDEF|       |2A_CDEF|       |
            // +-------+-------+-------+-------+-------+

            Assert.AreEqual(b[(-1, 0)].ToString()[0], 'p');
            Assert.AreEqual(b[(-1, 2)].ToString()[0], 'p');
            Assert.AreEqual(b[(0, -1)].ToString()[0], 'p');
            Assert.AreEqual(b[(0, 0)].ToString(), "A1");
            Assert.AreEqual(b[(0, 1)].ToString()[0], 'b');
            Assert.AreEqual(b[(0, 2)].ToString(), "B2");
            Assert.AreEqual(b[(0, 3)].ToString()[0], 'p');
            Assert.AreEqual(b[(1, 0)].ToString()[0], 'p');
            Assert.AreEqual(b[(1, 2)].ToString()[0], 'p');

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
            // +B0,-1--+B0,0---+B0,1---+
            // |Playabl|       |Playabl|
            // |A12_456|  A 3  |A12_456|
            // |3_BCDEF|       |3_BCDEF|
            // +D1,-1--+B1,0---+D1,1---+
            // |       |Playabl|       |
            // |       |A12_456|       |
            // |       |3_BCDEF|       |
            // +-------+-------+-------+

            Assert.AreEqual(b[(-1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(0, -1)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(0, 0)].ToString(), "A3");
            Assert.AreEqual(b[(0, 1)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(1, 0)].ToString(), "p,A12_456,3_BCDEF");


            b.PlayTile((0, 1), "A4");
            b.UpdatePlayedPlayability();

            // +D-1,-1-+B-1,0--+P-1,1--+D-1,2--+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // +P0,-1--+B0,0---+P0,1---+P0,2---+
            // |Playabl|       |       |Playabl|
            // |A12__56|  A 3  |  A 4  |A12__56|
            // |N/A    |       |       |N/A    |
            // +D1,-1--+B1,0---+P1,1---+D1,2---+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // +-------+-------+-------+-------+

            Assert.AreEqual(b[(-1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(-1, 1)].ToString(), "p,A123_56,4_BCDEF");
            Assert.AreEqual(b[(0, -1)].ToString(), "p,A12__56,N/A    ");
            Assert.AreEqual(b[(0, 0)].ToString(), "A3");
            Assert.AreEqual(b[(0, 1)].ToString(), "A4");
            Assert.AreEqual(b[(0, 2)].ToString(), "p,A12__56,N/A    ");
            Assert.AreEqual(b[(1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(1, 1)].ToString(), "p,A123_56,4_BCDEF");


            b.RollbackPlay();

            // +D-1,-1-+B-1,0--+D-1,1--+
            // |       |Playabl|       |
            // |       |A12_456|       |
            // |       |3_BCDEF|       |
            // +B0,-1--+B0,0---+B0,1---+
            // |Playabl|       |Playabl|
            // |A12_456|  A 3  |A12_456|
            // |3_BCDEF|       |3_BCDEF|
            // +D1,-1--+B1,0---+D1,1---+
            // |       |Playabl|       |
            // |       |A12_456|       |
            // |       |3_BCDEF|       |
            // +-------+-------+-------+

            Assert.AreEqual(b[(-1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(0, -1)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(0, 0)].ToString(), "A3");
            Assert.AreEqual(b[(0, 1)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(1, 0)].ToString(), "p,A12_456,3_BCDEF");
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
            // +B0,-1--+B0,0---+B0,1---+
            // |Playabl|       |Playabl|
            // |A12_456|  A 3  |A12_456|
            // |3_BCDEF|       |3_BCDEF|
            // +D1,-1--+B1,0---+D1,1---+
            // |       |Playabl|       |
            // |       |A12_456|       |
            // |       |3_BCDEF|       |
            // +-------+-------+-------+

            Assert.AreEqual(b[(-1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(0, -1)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(0, 0)].ToString(), "A3");
            Assert.AreEqual(b[(0, 1)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(1, 0)].ToString(), "p,A12_456,3_BCDEF");


            b.PlayTile((0, 1), "A4");
            b.UpdatePlayedPlayability();

            // +D-1,-1-+B-1,0--+P-1,1--+D-1,2--+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // +P0,-1--+B0,0---+P0,1---+P0,2---+
            // |Playabl|       |       |Playabl|
            // |A12__56|  A 3  |  A 4  |A12__56|
            // |N/A    |       |       |N/A    |
            // +D1,-1--+B1,0---+P1,1---+D1,2---+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // +-------+-------+-------+-------+

            Assert.AreEqual(b[(-1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(-1, 1)].ToString(), "p,A123_56,4_BCDEF");
            Assert.AreEqual(b[(0, -1)].ToString(), "p,A12__56,N/A    ");
            Assert.AreEqual(b[(0, 0)].ToString(), "A3");
            Assert.AreEqual(b[(0, 1)].ToString(), "A4");
            Assert.AreEqual(b[(0, 2)].ToString(), "p,A12__56,N/A    ");
            Assert.AreEqual(b[(1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(1, 1)].ToString(), "p,A123_56,4_BCDEF");


            b.CommitPlay();

            // +D-1,-1-+B-1,0--+B-1,1--+D-1,2--+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // +B0,-1--+B0,0---+B0,1---+B0,2---+
            // |Playabl|       |       |Playabl|
            // |A12__56|  A 3  |  A 4  |A12__56|
            // |N/A    |       |       |N/A    |
            // +D1,-1--+B1,0---+B1,1---+D1,2---+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // +-------+-------+-------+-------+

            Assert.AreEqual(b[(-1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(-1, 1)].ToString(), "p,A123_56,4_BCDEF");
            Assert.AreEqual(b[(0, -1)].ToString(), "p,A12__56,N/A    ");
            Assert.AreEqual(b[(0, 0)].ToString(), "A3");
            Assert.AreEqual(b[(0, 1)].ToString(), "A4");
            Assert.AreEqual(b[(0, 2)].ToString(), "p,A12__56,N/A    ");
            Assert.AreEqual(b[(1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(1, 1)].ToString(), "p,A123_56,4_BCDEF");
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
            // +B0,-1--+B0,0---+B0,1---+B0,2---+B0,3---+B0,4---+
            // |Playabl|       |       |       |       |Playabl|
            // |A12__56|  A 3  |  A 4  |N/A    |  B 1  |B_23456|
            // |N/A    |       |       |       |       |1A_CDEF|
            // +D1,-1--+B1,0---+B1,1---+D1,2---+B1,3---+D1,4---+
            // |       |Playabl|Playabl|       |Playabl|       |
            // |       |A12_456|A123_56|       |B_23456|       |
            // |       |3_BCDEF|4_BCDEF|       |1A_CDEF|       |
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
            Assert.AreEqual("Cette tuile ne respecte pas les contraintes", msg4);
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
            // +B0,-1--+B0,0---+P0,1---+B0,2---+P0,3---+P0,4---+
            // |Playabl|       |Playabl|       |       |Playabl|
            // |A_23456|  A 1  |A___456|  A 2  |  A 3  |A1__456|
            // |1_BCDEF|       |N/A    |       |       |N/A    |
            // +D1,-1--+B1,0---+D1,1---+B1,2---+P1,3---+D1,4---+
            // |       |Playabl|       |Playabl|Playabl|       |
            // |       |A_23456|       |A1_3456|A12_456|       |
            // |       |1_BCDEF|       |2_BCDEF|3_BCDEF|       |
            // +-------+-------+-------+-------+-------+-------+

            bool cp1 = b.CanPlayTile((0, 1),  "A4", out string msg1);
            Assert.IsTrue(cp1);
            bool cp2 = b.CanPlayTile((0, -1), "A4", out string msg2);
            Assert.IsFalse(cp2);
            Assert.AreEqual("Pas de trou entre les tuiles jouées", msg2);
            bool cp3 = b.CanPlayTile((1, 0),  "A2", out string msg3);
            Assert.IsFalse(cp3);
            Assert.AreEqual("Les tuiles doivent être jouées dans une seule ligne ou colonne", msg3);
        }
    }
}

