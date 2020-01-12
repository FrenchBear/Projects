﻿using System;
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
    public class Board
    {
        private readonly Dictionary<(int, int), Square> BoardDict = new Dictionary<(int, int), Square>();
        private readonly Dictionary<(int, int), Square> PlayedDict = new Dictionary<(int, int), Square>();

        private int RowMinBoard = 0;
        private int RowMaxBoard = 0;
        private int ColMinBoard = 0;
        private int ColMaxBoard = 0;
        private int RowMinPlayed = 999;
        private int RowMaxPlayed = -999;
        private int ColMinPlayed = 999;
        private int ColMaxPlayed = -999;

        public int RowMin => Math.Min(RowMinBoard, RowMinPlayed);
        public int RowMax => Math.Max(RowMaxBoard, RowMaxPlayed);
        public int ColMin => Math.Min(ColMinBoard, ColMinPlayed);
        public int ColMax => Math.Max(ColMaxBoard, ColMaxPlayed);


        /// <summary>
        /// Create a new empty Qwirkle board to play squares
        /// </summary>
        public Board() { }

        public Square this[(int row, int col) coord] => PlayedDict.GetValueOrDefault(coord, BoardDict.GetValueOrDefault(coord, Square.Empty));

        private bool IsTiled((int, int) coord) => this[coord].Tile != null;

        public void SetBoardSquare((int row, int col) coord, Square value)
        {
            if (BoardDict.ContainsKey(coord))
            {
                var s = BoardDict[coord];
                Debug.Assert(s.State == SquareState.Playable || s.State == SquareState.Unknown || s.State == SquareState.Empty);
            }
            BoardDict[coord] = value;
            if (coord.row < RowMinBoard) RowMinBoard = coord.row;
            if (coord.row > RowMaxBoard) RowMaxBoard = coord.row;
            if (coord.col < ColMinBoard) ColMinBoard = coord.col;
            if (coord.col > ColMaxBoard) ColMaxBoard = coord.col;
        }

        public void SetPlayedSquare((int row, int col) coord, Square value)
        {
            // Can't play over a tiled board square
            if (BoardDict.ContainsKey(coord))
                Debug.Assert(BoardDict[coord].Tile == null);
            // Can't play over a tiled or blocked played square
            if (PlayedDict.ContainsKey(coord))
            {
                Square s = PlayedDict[coord];
                Debug.Assert(s.State == SquareState.Playable || s.State == SquareState.Unknown || s.State == SquareState.Empty);
            }

            // If we place a tile, check constraints
            if (value.Tile != null)
                Debug.Assert(CanPlayTile(coord, value.Tile, out _));

            PlayedDict[coord] = value;
            if (coord.row < RowMinPlayed) RowMinPlayed = coord.row;
            if (coord.row > RowMaxPlayed) RowMaxPlayed = coord.row;
            if (coord.col < ColMinPlayed) ColMinPlayed = coord.col;
            if (coord.col > ColMaxPlayed) ColMaxPlayed = coord.col;
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
                PlayedDict.Count + BoardDict.Count > 0)
            {
                msg = "Il y n'a pas de tuile adjecente à cet emplacement";
                return false;
            }

            // Check playability
            if (!PlayedDict.ContainsKey(coord))
                PlayedDict[coord] = Square.Empty;
            UpdateSquarePlayability(coord, true);
            var ps = PlayedDict[coord];
            Debug.Assert(ps.State == SquareState.Blocked || ps.State == SquareState.Playable);
            if (ps.State == SquareState.Blocked)
            {
                msg = "Cet emplacement n'est pas jouable";
                return false;
            }

            // Check compatibility with constraints
            Debug.Assert(ps.ShapeConstraint != null);
            var sc = ps.ShapeConstraint.Value;
            bool matchShapeConstraint = false;
            if (sc.LineAttribute >= 0) matchShapeConstraint = sc.LineAttribute == tile.Shape && (sc.BlockedMask & (1 << tile.Color)) == 0;

            Debug.Assert(ps.ColorConstraint != null);
            bool matchColorConstraint = false;
            var cc = ps.ColorConstraint.Value;
            if (cc.LineAttribute >= 0) matchColorConstraint = cc.LineAttribute == tile.Color && (cc.BlockedMask & (1 << tile.Shape)) == 0;

            if (!matchShapeConstraint && !matchColorConstraint)
            {
                msg = "Cette tuile ne respecte pas les contraintes";
                return false;
            }

            // Check that all played tiles are in the same direction
            int nt = 0;
            int playRow = 0, playCol = 0;
            bool playInRow = false, playInCol = false;
            foreach (var pt in PlayedDict.Where(kv => kv.Value.Tile != null))
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
            foreach (var kv in PlayedDict)
                SetBoardSquare(kv.Key, kv.Value);
            ClearPlayedDict();
        }

        /// <summary>
        /// Cancel all played tiles
        /// </summary>
        private void ClearPlayedDict()
        {
            PlayedDict.Clear();
            RowMinPlayed = 999;
            RowMaxPlayed = -999;
            ColMinPlayed = 999;
            ColMaxPlayed = -999;
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

            var dic = isPlayed ? PlayedDict : BoardDict;
            var s = dic.GetValueOrDefault(coord, Square.Empty);
            if (s.State == SquareState.Blocked) return;     // Blocked state is final
            if (s.State == SquareState.Unknown) return;     // Already unknown, won't change

            s.State = SquareState.Unknown;
            s.ColorConstraint = null;
            s.ShapeConstraint = null;

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
            for (int row = RowMinBoard; row <= RowMaxBoard; row++)
                for (int col = ColMinBoard; col <= ColMaxBoard; col++)
                    if (BoardDict.ContainsKey((row, col)))
                    {
                        var s = BoardDict[(row, col)];
                        if (s.State == SquareState.Unknown)
                            UpdateSquarePlayability((row, col), false);
                    }
        }

        /// <summary>
        /// Evaluate playability of all played squares in Unknown state
        /// </summary>
        public void UpdatePlayedPlayability()
        {
            Debug.Assert(RowMinPlayed != 999 && ColMinPlayed != 999);
            for (int row = RowMinPlayed; row <= RowMaxPlayed; row++)
                for (int col = ColMinPlayed; col <= ColMaxPlayed; col++)
                    if (PlayedDict.ContainsKey((row, col)))
                    {
                        var s = PlayedDict[(row, col)];
                        if (s.State == SquareState.Unknown)
                            UpdateSquarePlayability((row, col), true);
                    }
        }

        private void UpdateSquarePlayability((int row, int col) coord, bool isPlayed)
        {
            if (coord == (5,1))
                Debugger.Break();

            var dic = isPlayed ? PlayedDict : BoardDict;
            Debug.Assert(dic.ContainsKey(coord));

            // First get constraints from all directions
            var (sc1, cc1) = GetConstraintsFromDirection(coord, -1, 0, isPlayed);
            var (sc2, cc2) = GetConstraintsFromDirection(coord, 1, 0, isPlayed);
            var (sc3, cc3) = GetConstraintsFromDirection(coord, 0, 1, isPlayed);
            var (sc4, cc4) = GetConstraintsFromDirection(coord, 0, -1, isPlayed);

            var sc1s = sc1.ToShapeConstraint(); var cc1s = cc1.ToColorConstraint();
            var sc2s = sc2.ToShapeConstraint(); var cc2s = cc2.ToColorConstraint();
            var sc3s = sc3.ToShapeConstraint(); var cc3s = cc3.ToColorConstraint();
            var sc4s = sc4.ToShapeConstraint(); var cc4s = cc4.ToColorConstraint();

            var zzca = cc1.Inter(cc2); var zzcas = zzca.ToColorConstraint();
            var zzcb = zzca.Inter(cc3); var zzcbs = zzcb.ToColorConstraint();
            var zzcc = zzcb.Inter(cc4); var zzccs = zzcc.ToColorConstraint();

            var sc = sc1.Inter(sc2).Inter(sc3).Inter(sc4);
            var cc = cc1.Inter(cc2).Inter(cc3).Inter(cc4);
            var scs = sc.ToShapeConstraint(); var ccs = cc.ToColorConstraint();

            if ((cc.LineAttribute == -2 || cc.BlockedMask == 63) &&
                 (sc.LineAttribute == -2 || sc.BlockedMask == 63))
            {
                dic[coord].State = SquareState.Blocked;
            }
            else
            {
                dic[coord].State = SquareState.Playable;
                dic[coord].ColorConstraint = cc;
                dic[coord].ShapeConstraint = sc;
            }
        }

        private (Constraint, Constraint) GetConstraintsFromDirection((int row, int col) coord, int deltaRow, int deltaCol, bool isPlayed)
        {
            Square s;
            var shapeConstraint = Constraint.None;
            var colorConstraint = Constraint.None;

            for (; ; )
            {
                coord = (coord.row + deltaRow, coord.col + deltaCol);
                s = isPlayed ? this[coord] : BoardDict.GetValueOrDefault(coord, Square.Empty);
                if (s.Tile == null) return (shapeConstraint, colorConstraint);
                shapeConstraint = shapeConstraint.Inter(new Constraint(s.Tile.Shape, 1 << s.Tile.Color));
                colorConstraint = colorConstraint.Inter(new Constraint(s.Tile.Color, 1 << s.Tile.Shape));
            }
        }

        public int PlayPoints()
        {
            // Reset evaluation helpers
            foreach (var pt in PlayedDict.Where(kv => kv.Value.Tile != null))
            {
                pt.Value.pointsInRow = false;
                pt.Value.pointsInCol = false;
            }

            // ToDo: replace with sum
            var points = 0;
            foreach (var pt in PlayedDict.Where(kv => kv.Value.Tile != null))
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
                if (PlayedDict.ContainsKey(coord))
                {
                    // PlayedDict cannot contain anything if the board is contining a tile
                    // So if we have something in PlayedDict, it must be a tile since we
                    // know that ccord is tiled
                    Debug.Assert(PlayedDict[coord].Tile != null);

                    if (isInRow)
                    {
                        Debug.Assert(!PlayedDict[coord].pointsInRow);
                        PlayedDict[coord].pointsInRow = true;
                    }
                    else
                    {
                        Debug.Assert(!PlayedDict[coord].pointsInCol);
                        PlayedDict[coord].pointsInCol = true;
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
                    if (PlayedDict.ContainsKey((row, col)))
                        source = 'P';
                    else if (BoardDict.ContainsKey((row, col)))
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
                        SquareState.Playable => "|" + s.ShapeConstraint!.Value.ToShapeConstraint(),
                        SquareState.Unknown => "|Unknown",
                        _ => "|       "
                    });
                }
                WriteLine("|");
                Write(linePrefix);
                for (int col = ColMin; col <= ColMax; col++)
                    if (this[(row, col)].State == SquareState.Playable)
                        Write("|" + this[(row, col)].ColorConstraint!.Value.ToColorConstraint());
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
    }
}
