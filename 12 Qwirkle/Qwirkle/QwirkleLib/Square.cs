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
        public Constraint? ColorConstraint;
        public Constraint? ShapeConstraint;

        // Helpers to evaluate a play
        internal bool pointsInRow;      // When true, the tile has already been evaluated in a row
        internal bool pointsInCol;      // When true, the tile has already been evaluated in a Column

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
                SquareState.Playable => "p," + ShapeConstraint!.Value.ToShapeConstraint() + "," + ColorConstraint!.Value.ToColorConstraint(),
                _ => ""
            };
    }
}
