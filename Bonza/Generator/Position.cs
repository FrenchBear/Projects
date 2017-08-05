// Bonza class Position
// A simple rectangle with int coordinates to represent layout bounds
//
// 2017-08-05   PV      Extracted


namespace Bonza.Generator
{
    public struct Position
    {
        public int Row, Column;

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}
