// Bonza class BoundingRectangle
// A simple rectangle with int coordinates to represent layout bounds
//
// 2017-08-05   PV      Extracted


namespace Bonza.Generator
{
    public struct BoundingRectangle
    {
        public Position Min, Max;

        public BoundingRectangle(int minRow, int maxRow, int minColumn, int maxColumn)
        {
            Min = new Position(minRow, minColumn);
            Max = new Position(maxRow, maxColumn);
        }

        public int MinRow { get => Min.Row; set => Min.Row=value; }
        public int MinColumn { get => Min.Column; set => Min.Column = value; }
        public int MaxRow { get => Max.Row; set => Max.Row = value; }
        public int MaxColumn { get => Max.Column; set => Max.Column = value; }
    }

    public struct BoundingRectangleBak
    {
        public int MinRow, MaxRow, MinColumn, MaxColumn;

        public BoundingRectangleBak(int minRow, int maxRow, int minColumn, int maxColumn)
        {
            MinRow = minRow;
            MaxRow = maxRow;
            MinColumn = minColumn;
            MaxColumn = maxColumn;
        }
    }


}