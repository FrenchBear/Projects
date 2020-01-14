// Quare.cs - One element of Board, contains optionally a tile and other attributes
// Qwirkle simulation project
// 2019-01-12   PV

using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace QwirkleLib
{
    public enum SquareState
    {
        /// <summary>
        /// Default, empty state, not playable
        /// </summary>
        Empty,

        /// <summary>
        /// Does not contain a tile, but playability and constraints unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Contains a tile
        /// </summary>
        Tiled,

        /// <summary>
        /// Empty but uneplayable because of constraints or conflicting constraints
        /// </summary>
        Blocked,

        /// <summary>
        /// Empty and playable
        /// </summary>
        Playable,
    }

    /// <summary>
    /// A square on the game board, that can be empty or contain a tile
    /// </summary>
    public class Square
    {
        internal static Square Empty => new Square(null);

        public SquareState State;
        public QTile? Tile;

        /// <summary>
        /// Shape constraint from above and below
        /// </summary>
        public ShapeConstraint? RowShapeConstraint;

        /// <summary>
        /// Color constraint from above and below
        /// </summary>
        public ColorConstraint? RowColorConstraint;

        /// <summary>
        /// Shape constraint from left and right
        /// </summary>
        public ShapeConstraint? ColShapeConstraint;

        /// <summary>
        /// Color constraint from left and right
        /// </summary>
        public ColorConstraint? ColColorConstraint;


        // Helpers to evaluate a play
        /// <summary>
        /// During points evaluation, true indicates that the tile has already been evaluated in a row
        /// </summary>
        internal bool pointsInRow;

        /// <summary>
        /// During points evaluation, true indicates that the tile has already been evaluated in a column
        /// </summary>
        internal bool pointsInCol;


        public Square(QTile? tile)
        {
            if (tile == null)
                State = SquareState.Empty;
            else
            {
                Tile = tile;
                State = SquareState.Tiled;
            }
        }

        public override string ToString() =>
            State switch
            {
                SquareState.Empty => "",
                SquareState.Unknown => "u",
                SquareState.Tiled => Tile!.ToString(),
                SquareState.Blocked => "b",
                SquareState.Playable => "p," + RowShapeConstraint!.ToString() + "," + RowColorConstraint!.ToString() + "," + ColShapeConstraint!.ToString() + "," + ColColorConstraint!.ToString(),
                _ => ""
            };
    }
}
