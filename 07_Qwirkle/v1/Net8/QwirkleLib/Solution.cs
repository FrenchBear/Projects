// Solution.cs - Contains one solution during solution analysis
// Qwirkle simulation project
//
// 2019-01-12   PV
// 2023-11-20   PV      Net8 C#12

using System.Collections.Generic;

namespace QwirkleLib;

public class Solution
{
    public int Points;
    public List<CoordSquare> Moves = [];
    public int RowMin, RowMax, ColMin, ColMax;
}
