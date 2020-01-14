// Solution.cs - Contains one solution during solution analysis
// Qwirkle simulation project
// 2019-01-12   PV

using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace QwirkleLib
{
    public class Solution
    {
        public int Points;
        public List<CoordSquare> Moves = new List<CoordSquare>();
        public int RowMin, RowMax, ColMin, ColMax;
    }
}
