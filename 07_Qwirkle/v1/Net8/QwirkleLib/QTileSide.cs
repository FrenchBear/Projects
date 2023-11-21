// QTileSide.cs - Contains one placement of a tile during solution analysis
// Qwirkle simulation project
//
// 2019-01-15   PV
// 2023-11-20   PV      Net8 C#12

namespace QwirkleLib;

struct QTileSide
{
    public QTile tile;
    public char side;       // '+' or '-' (or '.' for 1st tile of the hand)

    internal QTileSide SetSide(char newSide) => new() { tile = tile, side = newSide };
}
