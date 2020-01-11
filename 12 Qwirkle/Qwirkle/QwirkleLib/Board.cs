using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly Dictionary<(int, int), Square> SquaresDict = new Dictionary<(int, int), Square>();
        public int RowMin { get; private set; } = 0;
        public int RowMax { get; private set; } = 0;
        public int ColMin { get; private set; } = 0;
        public int ColMax { get; private set; } = 0;

        public Board()
        {

        }

        public Square this[(int row, int col) coord]
        {
            get => SquaresDict.GetValueOrDefault(coord, Square.Empty);
            set
            {
                if (SquaresDict.ContainsKey(coord))
                {
                    var s = SquaresDict[coord];
                    Debug.Assert(s.State == SquareState.Playable || s.State == SquareState.Unknown || s.State == SquareState.Empty);
                }
                SquaresDict[coord] = value;
                if (coord.row < RowMin) RowMin = coord.row;
                if (coord.row > RowMax) RowMax = coord.row;
                if (coord.col < ColMin) ColMin = coord.col;
                if (coord.col > ColMax) ColMax = coord.col;

            }
        }

        public void AddTile((int row, int col) coord, QTile tile)
        {
            this[coord] = new Square(tile);
            FindAndResetSquarePlayability(coord, -1, 0);
            FindAndResetSquarePlayability(coord, 1, 0);
            FindAndResetSquarePlayability(coord, 0, -1);
            FindAndResetSquarePlayability(coord, 0, 1);
        }

        private void FindAndResetSquarePlayability((int row, int col) coord, int deltaRow, int deltaCol)
        {
            Square s;

            // Look for 1st square that is either playable or unknown, skipping tiled squares
            // A blocked square terminate the operation
            for (; ; )
            {
                coord = (coord.row + deltaRow, coord.col + deltaCol);
                s = this[coord];
                if (s.State == SquareState.Blocked) return;     // Blocked state is final
                if (s.State == SquareState.Unknown) return;     // Already unknown, won't change
                if (s.State == SquareState.Playable || s.State == SquareState.Empty) break;
            }

            s.State = SquareState.Unknown;
            s.ColorConstraint = null;
            s.ShapeConstraint = null;
            this[coord] = s;
            return;
        }

        public void UpdatePlayability()
        {
            for (int row = RowMin; row <= RowMax; row++)
                for (int col = ColMin; col <= ColMax; col++)
                    if (this[(row, col)].State == SquareState.Unknown)
                        UpdateSquarePlayability((row, col));

        }

        private void UpdateSquarePlayability((int row, int col) coord)
        {
            // First get constraints from all directions
            var (cc1, sc1) = GetConstraintsFromDirection(coord, -1, 0);
            var (cc2, sc2) = GetConstraintsFromDirection(coord, 1, 0);
            var (cc3, sc3) = GetConstraintsFromDirection(coord, 0, 1);
            var (cc4, sc4) = GetConstraintsFromDirection(coord, 0, -1);

            var cc = cc1.Inter(cc2).Inter(cc3).Inter(cc4);
            var sc = sc1.Inter(sc2).Inter(sc3).Inter(sc4);

            if ((cc.LineAttribute == -2 || cc.BlockedMask == 63) &&
                 (sc.LineAttribute == -2 || sc.BlockedMask == 63))
            {
                this[coord].State = SquareState.Blocked;
            }
            else
            {
                this[coord].State = SquareState.Playable;
                this[coord].ColorConstraint = cc;
                this[coord].ShapeConstraint = sc;
            }
        }

        private (Constraint, Constraint) GetConstraintsFromDirection((int row, int col) coord, int deltaRow, int deltaCol)
        {
            Square s;
            var shapeConstraint = Constraint.None;
            var colorConstraint = Constraint.None;

            for (; ; )
            {
                coord = (coord.row + deltaRow, coord.col + deltaCol);
                s = this[coord];
                if (s.Tile == null) return (colorConstraint, shapeConstraint);
                shapeConstraint = shapeConstraint.Inter(new Constraint(s.Tile.Shape, 1 << s.Tile.Color));
                colorConstraint = colorConstraint.Inter(new Constraint(s.Tile.Color, 1 << s.Tile.Shape));
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
        public void Print()
        {
            for (int row = RowMin; row <= RowMax; row++)
            {
                for (int col = ColMin; col <= ColMax; col++)
                    Write("+-------");
                WriteLine("+");
                for (int col = ColMin; col <= ColMax; col++)
                    if (this[(row, col)].State == SquareState.Playable)
                        Write("|Playabl");
                    else
                        Write("|       ");
                WriteLine("|");
                for (int col = ColMin; col <= ColMax; col++)
                {
                    var s = this[(row, col)];
                    Write(s.State switch
                    {
                        SquareState.Tiled => $"|  {(char)(65 + s.Tile!.Shape)} {(char)(49 + s.Tile!.Color)}  ",
                        SquareState.Blocked => "|Blocked",
                        SquareState.Playable => "|" + s.ShapeConstraint!.Value.ToShapeConstraint,
                        SquareState.Unknown => "|Unknown",
                        _ => "|       "
                    });
                }
                WriteLine("|");
                for (int col = ColMin; col <= ColMax; col++)
                    if (this[(row, col)].State == SquareState.Playable)
                        Write("|" + this[(row, col)].ColorConstraint!.Value.ToColorConstraint);
                    else
                        Write("|       ");
                WriteLine("|");
            }
            for (int col = ColMin; col <= ColMax; col++)
                Write("+-------");
            WriteLine("+");
            WriteLine();
        }
    }
}

