// QTileSide.cs - Contains one placement of a tile during solution analysis
// Qwirkle simulation project
// 2019-01-15   PV

using System;
using System.Collections.Generic;
using System.Text;

namespace QwirkleLib
{
    struct QTileSide
    {
        public QTile tile;
        public char side;       // '+' or '-' (or '.' for 1st tile of the hand)

        internal QTileSide SetSide(char newSide) => new QTileSide { tile = this.tile, side = newSide };
    }
}
