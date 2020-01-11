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
            // |Blocked|       |       |Blocked|
            // +-------+-------+-------+-------+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // +-------+-------+-------+-------+

            Assert.AreEqual(b[(-1, -1)].ToString(), "");
            Assert.AreEqual(b[(-1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(-1, 1)].ToString(), "p,A123_56,4_BCDEF");
            Assert.AreEqual(b[(-1, 2)].ToString(), "");
            Assert.AreEqual(b[(0, -1)].ToString(), "p,A12__56,Blocked");
            Assert.AreEqual(b[(0, 0)].ToString(), "A3");
            Assert.AreEqual(b[(0, 1)].ToString(), "A4");
            Assert.AreEqual(b[(0, 2)].ToString(), "p,A12__56,Blocked");
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
            // |A_23456|  A 1  |Blocked|  B 2  |B1_3456|
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
            // |Blocked|       |       |Blocked|
            // +D1,-1--+B1,0---+P1,1---+D1,2---+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // +-------+-------+-------+-------+

            Assert.AreEqual(b[(-1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(-1, 1)].ToString(), "p,A123_56,4_BCDEF");
            Assert.AreEqual(b[(0, -1)].ToString(), "p,A12__56,Blocked");
            Assert.AreEqual(b[(0, 0)].ToString(), "A3");
            Assert.AreEqual(b[(0, 1)].ToString(), "A4");
            Assert.AreEqual(b[(0, 2)].ToString(), "p,A12__56,Blocked");
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
            // |Blocked|       |       |Blocked|
            // +D1,-1--+B1,0---+P1,1---+D1,2---+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // +-------+-------+-------+-------+

            Assert.AreEqual(b[(-1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(-1, 1)].ToString(), "p,A123_56,4_BCDEF");
            Assert.AreEqual(b[(0, -1)].ToString(), "p,A12__56,Blocked");
            Assert.AreEqual(b[(0, 0)].ToString(), "A3");
            Assert.AreEqual(b[(0, 1)].ToString(), "A4");
            Assert.AreEqual(b[(0, 2)].ToString(), "p,A12__56,Blocked");
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
            // |Blocked|       |       |Blocked|
            // +D1,-1--+B1,0---+B1,1---+D1,2---+
            // |       |Playabl|Playabl|       |
            // |       |A12_456|A123_56|       |
            // |       |3_BCDEF|4_BCDEF|       |
            // +-------+-------+-------+-------+

            Assert.AreEqual(b[(-1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(-1, 1)].ToString(), "p,A123_56,4_BCDEF");
            Assert.AreEqual(b[(0, -1)].ToString(), "p,A12__56,Blocked");
            Assert.AreEqual(b[(0, 0)].ToString(), "A3");
            Assert.AreEqual(b[(0, 1)].ToString(), "A4");
            Assert.AreEqual(b[(0, 2)].ToString(), "p,A12__56,Blocked");
            Assert.AreEqual(b[(1, 0)].ToString(), "p,A12_456,3_BCDEF");
            Assert.AreEqual(b[(1, 1)].ToString(), "p,A123_56,4_BCDEF");
        }

    }
}

