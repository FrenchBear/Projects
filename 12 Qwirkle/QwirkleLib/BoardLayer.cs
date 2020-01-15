// BoardLayer.cs - Qwirkle grid of Squares
// Qwirkle simulation project
// 2019-01-15   PV

using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace QwirkleLib
{
    public class BoardLayer
    {
        public readonly Dictionary<(int, int), Square> dict;
        public int RowMin;
        public int RowMax;
        public int ColMin;
        public int ColMax;

        public BoardLayer()
        {
            dict = new Dictionary<(int, int), Square>();
            RowMin = 999;
            RowMax = -999;
            ColMin = 999;
            ColMax = -999;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public BoardLayer(BoardLayer copy)
        {
            dict = new Dictionary<(int, int), Square>();
            foreach (var kv in copy.dict)
                dict.Add(kv.Key, new Square(kv.Value));
            RowMin = copy.RowMin;
            RowMax = copy.RowMax;
            ColMin = copy.ColMin;
            ColMax = copy.ColMax;
        }
    }
}
