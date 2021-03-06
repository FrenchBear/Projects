﻿// Board.cs - Main class simulating a Qwirkle grid of Squares
// Qwirkle simulation project
// 2019-01-12   PV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static System.Console;

#nullable enable

namespace QwirkleLib
{
    /// <summary>
    /// Qwirkle game board, a grid of BoardSquare
    /// </summary>
    public partial class Board
    {
        //private readonly Dictionary<(int, int), Square> Base.dict = new Dictionary<(int, int), Square>();
        //private readonly Dictionary<(int, int), Square> Played.dict = new Dictionary<(int, int), Square>();

        private readonly BoardLayer Base = new BoardLayer();
        private readonly BoardLayer Played = new BoardLayer();

        //private int RowMinBoard = 0;
        //private int RowMaxBoard = 0;
        //private int ColMinBoard = 0;
        //private int ColMaxBoard = 0;
        //private int RowMinPlayed = 999;
        //private int RowMaxPlayed = -999;
        //private int ColMinPlayed = 999;
        //private int ColMaxPlayed = -999;

        public int RowMin => Math.Min(Base.RowMin, Played.RowMin);
        public int RowMax => Math.Max(Base.RowMax, Played.RowMax);
        public int ColMin => Math.Min(Base.ColMin, Played.ColMin);
        public int ColMax => Math.Max(Base.ColMax, Played.ColMax);


        /// <summary>
        /// Create a new empty Qwirkle board to play squares
        /// </summary>
        public Board() { }

        public Square this[(int row, int col) coord] => Played.dict.GetValueOrDefault(coord, Base.dict.GetValueOrDefault(coord, Square.Empty));

        private bool IsTiled((int, int) coord) => this[coord].Tile != null;

        public void SetBoardSquare((int row, int col) coord, Square value)
        {
            if (Base.dict.ContainsKey(coord))
            {
                var s = Base.dict[coord];
                Debug.Assert(s.State == SquareState.Playable || s.State == SquareState.Unknown || s.State == SquareState.Empty);
            }
            Base.dict[coord] = value;
            if (coord.row < Base.RowMin) Base.RowMin = coord.row;
            if (coord.row > Base.RowMax) Base.RowMax = coord.row;
            if (coord.col < Base.ColMin) Base.ColMin = coord.col;
            if (coord.col > Base.ColMax) Base.ColMax = coord.col;
        }

        public void SetPlayedSquare((int row, int col) coord, Square value)
        {
            // Can't play over a tiled board square
            if (Base.dict.ContainsKey(coord))
                Debug.Assert(Base.dict[coord].Tile == null);
            // Can't play over a tiled or blocked played square
            if (Played.dict.ContainsKey(coord))
            {
                Square s = Played.dict[coord];
                Debug.Assert(s.State == SquareState.Playable || s.State == SquareState.Unknown || s.State == SquareState.Empty);
            }

            // If we place a tile, check constraints
            if (value.Tile != null)
                Debug.Assert(CanPlayTile(coord, value.Tile, out _));

            Played.dict[coord] = value;
            if (coord.row < Played.RowMin) Played.RowMin = coord.row;
            if (coord.row > Played.RowMax) Played.RowMax = coord.row;
            if (coord.col < Played.ColMin) Played.ColMin = coord.col;
            if (coord.col > Played.ColMax) Played.ColMax = coord.col;
        }


        private void ResetSquarePlayability((int, int) coord, bool isPlay)
        {
            FindAndResetSquarePlayability(coord, -1, 0, isPlay);
            FindAndResetSquarePlayability(coord, 1, 0, isPlay);
            FindAndResetSquarePlayability(coord, 0, -1, isPlay);
            FindAndResetSquarePlayability(coord, 0, 1, isPlay);
        }

        /// <summary>
        /// Add a permanent tile to the board 
        /// </summary>
        public void AddTile((int, int) coord, QTile tile)
        {
            SetBoardSquare(coord, new Square(tile));
            ResetSquarePlayability(coord, false);
        }

        /// <summary>
        /// Add a permanent tile to the board 
        /// </summary>
        public void AddTile((int, int) coord, string tile) => AddTile(coord, new QTile(tile));


        /// <summary>
        /// Add a played tile to the board that will be evaluated later for score
        /// </summary>
        public void PlayTile((int, int) coord, QTile tile)
        {
            SetPlayedSquare(coord, new Square(tile));
            ResetSquarePlayability(coord, true);
        }

        /// <summary>
        /// Add a played tile to the board that will be evaluated later for score
        /// </summary>
        public void PlayTile((int, int) coord, string tile) => PlayTile(coord, new QTile(tile));


        /// <summary>
        /// Check it a tile can be played at a given location given rules and constraints 
        /// </summary>
        public bool CanPlayTile((int row, int col) coord, QTile tile, out string msg)
        {
            // Check that there is no tile already there
            if (IsTiled(coord))
            {
                msg = "Il y a déjà une tuile à cet emplacement";
                return false;
            }

            // Check that there is a tile in the 4 adjacent locations (skipped for very 1st tile)
            if (!IsTiled((coord.row + 1, coord.col)) &&
                !IsTiled((coord.row - 1, coord.col)) &&
                !IsTiled((coord.row, coord.col + 1)) &&
                !IsTiled((coord.row, coord.col - 1)) &&
                Played.dict.Count + Base.dict.Count > 0)
            {
                msg = "Il y n'a pas de tuile adjecente à cet emplacement";
                return false;
            }

            // Check playability
            if (!Played.dict.ContainsKey(coord))
                Played.dict[coord] = Square.Empty;
            UpdateSquarePlayability(coord, true);
            var ps = Played.dict[coord];
            Debug.Assert(ps.State == SquareState.Blocked || ps.State == SquareState.Playable);
            if (ps.State == SquareState.Blocked)
            {
                msg = "Cet emplacement n'est pas jouable";
                return false;
            }

            // Check compatibility with constraints
            bool matchRowShapeConstraint = true, matchRowColorConstraint = true;
            if (ps.RowShapeConstraint != null)
            {
                var sc = ps.RowShapeConstraint;
                if (sc.LineAttribute >= 0)
                    matchRowShapeConstraint = sc.LineAttribute == tile.Shape && (sc.BlockedMask & (1 << tile.Color)) == 0;
                else if (sc.LineAttribute == -2)
                    matchRowShapeConstraint = false;
            }
            if (ps.RowColorConstraint != null)
            {
                var cc = ps.RowColorConstraint;
                if (cc.LineAttribute >= 0)
                    matchRowColorConstraint = cc.LineAttribute == tile.Color && (cc.BlockedMask & (1 << tile.Shape)) == 0;
                else if (cc.LineAttribute == -2)
                    matchRowColorConstraint = false;
            }
            if (!matchRowShapeConstraint && !matchRowColorConstraint)
            {
                msg = "Cette tuile ne respecte pas les contraintes de ligne";
                return false;
            }

            bool matchColShapeConstraint = true, matchColColorConstraint = true;
            if (ps.ColShapeConstraint != null)
            {
                var sc = ps.ColShapeConstraint;
                if (sc.LineAttribute >= 0)
                    matchColShapeConstraint = sc.LineAttribute == tile.Shape && (sc.BlockedMask & (1 << tile.Color)) == 0;
                else if (sc.LineAttribute == -2)
                    matchColShapeConstraint = false;
            }
            if (ps.ColColorConstraint != null)
            {
                var cc = ps.ColColorConstraint;
                if (cc.LineAttribute >= 0)
                    matchColColorConstraint = cc.LineAttribute == tile.Color && (cc.BlockedMask & (1 << tile.Shape)) == 0;
                else if (cc.LineAttribute == -2)
                    matchColColorConstraint = false;
            }
            if (!matchColShapeConstraint && !matchColColorConstraint)
            {
                msg = "Cette tuile ne respecte pas les contraintes de colonne";
                return false;
            }

            // Check that all played tiles are in the same direction
            int nt = 0;
            int playRow = 0, playCol = 0;
            bool playInRow = false, playInCol = false;
            foreach (var pt in Played.dict.Where(kv => kv.Value.Tile != null))
            {
                nt++;
                if (nt == 1)
                {
                    playRow = pt.Key.Item1;
                    playCol = pt.Key.Item2;
                }
                else if (nt == 2)
                {
                    if (pt.Key.Item1 == playRow)
                        playInRow = true;
                    else
                    {
                        Debug.Assert(pt.Key.Item2 == playCol);
                        playInCol = true;
                    }
                    break;
                }
            }
            if (nt >= 1)
            {
                if (nt == 1 && coord.row != playRow && coord.col != playCol
                    || nt > 1 && (playInRow && coord.row != playRow || playInCol && coord.col != playCol))
                {
                    msg = "Les tuiles doivent être jouées dans une seule ligne ou colonne";
                    return false;
                }

                // Check that play is contiguous
                int deltaRow = Math.Sign(playRow - coord.row);
                int deltaCol = Math.Sign(playCol - coord.col);
                var ic = coord;
                for (; ; )
                {
                    ic = (ic.row + deltaRow, ic.col + deltaCol);
                    if (ic == (playRow, playCol))
                        break;
                    if (!IsTiled(ic))
                    {
                        msg = "Pas de trou entre les tuiles jouées";
                        return false;
                    }
                }
            }

            msg = "";
            return true;
        }

        /// <summary>
        /// Check it a tile can be played at a given location given rules and constraints 
        /// </summary>
        public bool CanPlayTile((int, int) coord, string tile, out string msg) => CanPlayTile(coord, new QTile(tile), out msg);


        /// <summary>
        /// Make played tiles permanent
        /// </summary>
        public void CommitPlay()
        {
            foreach (var kv in Played.dict)
                SetBoardSquare(kv.Key, kv.Value);
            Played.dict.Clear();
        }

        /// <summary>
        /// Cancel all played tiles
        /// </summary>
        private void ClearPlayedDict()
        {
            Played.dict.Clear();
            Played.RowMin = 999;
            Played.RowMax = -999;
            Played.ColMin = 999;
            Played.ColMax = -999;
        }

        public void RollbackPlay()
        {
            ClearPlayedDict();
        }


        private void FindAndResetSquarePlayability((int row, int col) coord, int deltaRow, int deltaCol, bool isPlayed)
        {
            // Look for 1st square that is either playable or unknown, skipping tiled squares
            // A blocked square terminate the operation
            for (; ; )
            {
                coord = (coord.row + deltaRow, coord.col + deltaCol);
                if (this[coord].Tile == null)
                    break;
            }

            var dic = isPlayed ? Played.dict : Base.dict;
            var s = dic.GetValueOrDefault(coord, Square.Empty);
            if (s.State == SquareState.Blocked) return;     // Blocked state is final
            if (s.State == SquareState.Unknown) return;     // Already unknown, won't change

            s.State = SquareState.Unknown;
            s.RowColorConstraint = null;
            s.RowShapeConstraint = null;
            s.ColColorConstraint = null;
            s.ColShapeConstraint = null;

            if (isPlayed)
                SetPlayedSquare(coord, s);
            else
                SetBoardSquare(coord, s);
        }

        /// <summary>
        /// Evaluate playability of all board squares in Unknown state
        /// </summary>
        public void UpdateBoardPlayability()
        {
            if (Base.dict.Count == 0)
                return;

            for (int row = Base.RowMin; row <= Base.RowMax; row++)
                for (int col = Base.ColMin; col <= Base.ColMax; col++)
                    if (Base.dict.ContainsKey((row, col)))
                    {
                        var s = Base.dict[(row, col)];
                        if (s.State == SquareState.Unknown)
                            UpdateSquarePlayability((row, col), false);
                    }
        }

        /// <summary>
        /// Evaluate playability of all played squares in Unknown state
        /// </summary>
        public void UpdatePlayedPlayability()
        {
            if (Played.dict.Count == 0)
                return;

            Debug.Assert(Played.RowMin != 999 && Played.ColMin != 999);
            for (int row = Played.RowMin; row <= Played.RowMax; row++)
                for (int col = Played.ColMin; col <= Played.ColMax; col++)
                    if (Played.dict.ContainsKey((row, col)))
                    {
                        var s = Played.dict[(row, col)];
                        if (s.State == SquareState.Unknown)
                            UpdateSquarePlayability((row, col), true);
                    }
        }

        private void UpdateSquarePlayability((int row, int col) coord, bool isPlayed)
        {
            var dic = isPlayed ? Played.dict : Base.dict;
            Debug.Assert(dic.ContainsKey(coord));

            // First get constraints from all directions
            var (sc1r, cc1r) = GetConstraintsFromDirection(coord, -1, 0, isPlayed);
            var (sc2r, cc2r) = GetConstraintsFromDirection(coord, 1, 0, isPlayed);
            var (sc3c, cc3c) = GetConstraintsFromDirection(coord, 0, 1, isPlayed);
            var (sc4c, cc4c) = GetConstraintsFromDirection(coord, 0, -1, isPlayed);

            var scr = sc1r.Inter(sc2r);
            var ccr = cc1r.Inter(cc2r);
            var scc = sc3c.Inter(sc4c);
            var ccc = cc3c.Inter(cc4c);

            if ((scr.LineAttribute == -2 || scr.BlockedMask == 63) && (ccr.LineAttribute == -2 || ccr.BlockedMask == 63) ||
                (scc.LineAttribute == -2 || scc.BlockedMask == 63) && (ccc.LineAttribute == -2 || ccc.BlockedMask == 63)
                )
            {
                dic[coord].State = SquareState.Blocked;
            }
            else
            {
                dic[coord].State = SquareState.Playable;
                dic[coord].RowShapeConstraint = scr;
                dic[coord].RowColorConstraint = ccr;
                dic[coord].ColShapeConstraint = scc;
                dic[coord].ColColorConstraint = ccc;
            }
        }

        private (ShapeConstraint, ColorConstraint) GetConstraintsFromDirection((int row, int col) coord, int deltaRow, int deltaCol, bool isPlayed)
        {
            Square s;
            var shapeConstraint = ShapeConstraint.None;
            var colorConstraint = ColorConstraint.None;

            for (; ; )
            {
                coord = (coord.row + deltaRow, coord.col + deltaCol);
                s = isPlayed ? this[coord] : Base.dict.GetValueOrDefault(coord, Square.Empty);
                if (s.Tile == null) return (shapeConstraint, colorConstraint);
                shapeConstraint = shapeConstraint.Inter(new ShapeConstraint(s.Tile.Shape, 1 << s.Tile.Color));
                colorConstraint = colorConstraint.Inter(new ColorConstraint(s.Tile.Color, 1 << s.Tile.Shape));
            }
        }

        public int PlayPoints()
        {
            // Reset evaluation helpers
            foreach (var pt in Played.dict.Where(kv => kv.Value.Tile != null))
            {
                pt.Value.pointsInRow = false;
                pt.Value.pointsInCol = false;
            }

            // ToDo: replace with sum
            var points = 0;
            foreach (var pt in Played.dict.Where(kv => kv.Value.Tile != null))
                points += PlayPointsForTile(pt.Key);

            return points;
        }

        private int PlayPointsForTile((int, int) coord)
        {
            var points = 0;
            var s = this[coord];
            Debug.Assert(s.Tile != null);
            if (!s.pointsInRow)
            {
                int r = GetTilesCount(coord, 0, -1, true) + 1 + GetTilesCount(coord, 0, +1, true);
                s.pointsInRow = true;
                if (r > 1)
                {
                    if (r == 6) r = 12;
                    points += r;
                }
            }
            if (!s.pointsInCol)
            {
                int r = GetTilesCount(coord, -1, 0, false) + 1 + GetTilesCount(coord, 1, 0, false);
                s.pointsInCol = true;
                if (r > 1)
                {
                    if (r == 6) r = 12;
                    points += r;
                }
            }
            return points;
        }

        private int GetTilesCount((int row, int col) coord, int deltaRow, int deltaCol, bool isInRow)
        {
            int n = 0;
            for (; ; )
            {
                coord = (coord.row + deltaRow, coord.col + deltaCol);
                if (!IsTiled(coord))
                    return n;
                n++;
                if (Played.dict.ContainsKey(coord))
                {
                    // Played.dict cannot contain anything if the board is contining a tile
                    // So if we have something in Played.dict, it must be a tile since we
                    // know that ccord is tiled
                    Debug.Assert(Played.dict[coord].Tile != null);

                    if (isInRow)
                    {
                        Debug.Assert(!Played.dict[coord].pointsInRow);
                        Played.dict[coord].pointsInRow = true;
                    }
                    else
                    {
                        Debug.Assert(!Played.dict[coord].pointsInCol);
                        Played.dict[coord].pointsInCol = true;
                    }
                }
            }
        }


        /*
        +-------+-------+
        |       |       |
        |  A 3  |Blocked|
        |       |       |
        +-------+-------+
        |Playabl|       |
        |A123456|Unknown|
        |3ABCDEF|       |
        +-------+-------+
        */

        /// <summary>
        /// Simple text print of the board
        /// </summary>
        public void Print(string linePrefix = "")
        {
            for (int row = RowMin; row <= RowMax; row++)
            {
                Write(linePrefix);
                for (int col = ColMin; col <= ColMax; col++)
                {
                    char source;
                    if (Played.dict.ContainsKey((row, col)))
                        source = 'P';
                    else if (Base.dict.ContainsKey((row, col)))
                        source = 'B';
                    else
                        source = 'D';
                    Write($"+{source}{row},{col}-------"[0..8]);
                }
                WriteLine("+");
                Write(linePrefix);
                for (int col = ColMin; col <= ColMax; col++)
                    if (this[(row, col)].State == SquareState.Playable)
                        Write("|Playabl");
                    else
                        Write("|       ");
                WriteLine("|");
                Write(linePrefix);
                for (int col = ColMin; col <= ColMax; col++)
                {
                    var s = this[(row, col)];
                    Write(s.State switch
                    {
                        SquareState.Tiled => $"|  {(char)(65 + s.Tile!.Shape)} {(char)(49 + s.Tile!.Color)}  ",
                        SquareState.Blocked => "|Blocked",
                        SquareState.Playable => "|" + s.RowShapeConstraint!.ToString(),
                        SquareState.Unknown => "|Unknown",
                        _ => "|       "
                    });
                }
                WriteLine("|");
                Write(linePrefix);
                for (int col = ColMin; col <= ColMax; col++)
                    if (this[(row, col)].State == SquareState.Playable)
                        Write("|" + this[(row, col)].RowColorConstraint!.ToString());
                    else
                        Write("|       ");
                WriteLine("|");
                Write(linePrefix);
                for (int col = ColMin; col <= ColMax; col++)
                    if (this[(row, col)].State == SquareState.Playable)
                        Write("|" + this[(row, col)].ColShapeConstraint!.ToString());
                    else
                        Write("|       ");
                WriteLine("|");
                Write(linePrefix);
                for (int col = ColMin; col <= ColMax; col++)
                    if (this[(row, col)].State == SquareState.Playable)
                        Write("|" + this[(row, col)].ColColorConstraint!.ToString());
                    else
                        Write("|       ");
                WriteLine("|");
            }
            Write(linePrefix);
            for (int col = ColMin; col <= ColMax; col++)
                Write("+-------");
            WriteLine("+");
            WriteLine();
        }


        public void PrettyPrint()
        {
            string shapes = "○¤■♦☼♣";
            int[] colors = { 12,4,14,10,11,13 };
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            for (int row = RowMin; row <= RowMax; row++)
            {
                for (int col = ColMin; col <= ColMax; col++)
                {
                    var t = this[(row, col)].Tile;
                    if (t!=null)
                    {
                        Console.ForegroundColor = (ConsoleColor)colors[t.Color];
                        Write(shapes[t.Shape]);
                    }
                    else
                        Write(' ');
                }
                WriteLine();
            }
            Console.ForegroundColor = ConsoleColor.White;
            WriteLine();
        }
    }
}

