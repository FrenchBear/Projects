// WordPositionLayout.cs
// A class representing placed words on a virtual grid of 1-letter squares
//
// 2017-07-24   PV      Moved as an external class during refactoring
// 2017-08-05   PV      Struct Position for .Net Core
// 2017-08-05   PV      WordsList


using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Bonza.Generator
{
    /// <summary>Describes possible results of a WordPosition placement.</summary>
    public enum PlaceWordStatus
    {
        /// <summary>Placement is good.</summary>
        Valid,
        /// <summary>No overlap, but word touches another word in an incorrect way.</summary>
        TooClose,
        /// <summary>Overlap or touches a word of same orientation.</summary>
        Invalid
    }


    /// <summary>Management of a layout, that is, lists of WordPosition, Squares and Words.  Does not handle placement.</summary>
    public class WordPositionLayout
    {
        private readonly List<WordPosition> m_WordPositionList = new List<WordPosition>();
        private readonly Dictionary<Position, Square> m_Squares = new Dictionary<Position, Square>();

        public ReadOnlyCollection<WordPosition> WordPositionList => m_WordPositionList.AsReadOnly();


        // Default constructor
        // Needed because we have another constructor defined
        public WordPositionLayout()
        {
        }

        // Copy constructor
        // m_WordPositionsList contains the same references as tho copied layout
        // m_Squares contains new copies of copied layout squares
        internal WordPositionLayout(WordPositionLayout copy)
        {
            m_WordPositionList.AddRange(copy.m_WordPositionList);
            using (var e = copy.m_Squares.GetEnumerator())
                while (e.MoveNext())
                    m_Squares.Add(e.Current.Key, new Square(e.Current.Value));
        }


        // Safe version
        public PlaceWordStatus AddWordPositionAndSquares(WordPosition wp)
        {
            if (wp == null)
                throw new ArgumentNullException(nameof(wp));
            if (m_WordPositionList.Contains(wp))
                throw new ArgumentException("WordPosition already in the layout");
            if (m_WordPositionList.Any(wordPosition => string.Compare(wordPosition.Word, wp.Word, StringComparison.OrdinalIgnoreCase) == 0))
                throw new ArgumentException("Word already in layout list of words");

            var res = CanPlaceWord(wp);
            if (res != PlaceWordStatus.Invalid)
                AddWordPositionAndSquaresNoCheck(wp);
            return res;
        }


        // Low-level function to add a WordPosition to Layout, do not check that placement is correct
        public void AddWordPositionAndSquaresNoCheck(WordPosition wp)
        {
            m_WordPositionList.Add(wp);
            AddSquares(wp);
        }

        // Low-level removal function
        public void RemoveWordPositionAndSquares(WordPosition wp)
        {
            if (wp == null)
                throw new ArgumentNullException(nameof(wp));
            if (!m_WordPositionList.Contains(wp))
                throw new ArgumentException("WordPosition not in the layout");

            RemoveSquares(wp);
            m_WordPositionList.Remove(wp);
        }


        // Private helper
        private void AddSquares(WordPosition wp)
        {
            Debug.Assert(wp != null);

            int row = wp.StartRow;
            int column = wp.StartColumn;
            foreach (char c in wp.Word)
            {
                if (GetSquare(row, column) == null)
                {
                    Square sq = new Square( row, column,  c,  false, 1 );
                    m_Squares.Add(new Position(row, column), sq);
                }
                else
                {
                    Square sq = GetSquare(row, column);
                    Debug.Assert(sq.Letter == c);
                    sq.ShareCount++;
                }
                if (wp.IsVertical) row++; else column++;
            }
        }

        private void RemoveSquares(WordPosition wp)
        {
            Debug.Assert(wp != null);

            int row = wp.StartRow;
            int column = wp.StartColumn;
            for (int i = 0; i < wp.Word.Length; i++)
            {
                Square sq = GetSquare(row, column);
                Debug.Assert(sq != null);
                if (sq.ShareCount == 1)
                    m_Squares.Remove(new Position(row, column));
                else
                    sq.ShareCount--;

                if (wp.IsVertical) row++; else column++;
            }
        }


        // Returns square at a given position, or null if there is nothing in current layout
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetSquare(int row, int column)
        {
            //return IsOccupiedSquare(row, column) ? m_Squares[new Position(row, column)] : null;
            m_Squares.TryGetValue(new Position(row, column), out Square sq);
            return sq;
        }

        // Specialized helper for performance, returns true if there's no square at this position
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsOccupiedSquare(int row, int column)
        {
            return m_Squares.ContainsKey(new Position(row, column));
        }


        // Helper: Return letter placed at coordinates (row, column), or EmptyLetter if there is nothing there
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char GetLetter(int row, int column)
        {
            //return m_Squares.ContainsKey(new Position(row, column)) ? m_Squares[new Position(row, column)].Letter : '\0';
            m_Squares.TryGetValue(new Position(row, column), out Square sq);
            return sq?.Letter ?? '\0';
        }

        // Compute layout external bounds, note that cell (0,0) is always included in bounding rectangle
        public BoundingRectangle GetBounds()
        {
            BoundingRectangle r = new BoundingRectangle();
            //foreach (WordPosition wp in m_WordPositionList)
            //    r = ExtendBounds(r, wp);
            //return m_WordPositionList.Aggregate(r, (current, wp) => ExtendBounds(current, wp));
            return m_WordPositionList.Aggregate(r, ExtendBounds);
        }

        // Return layout bounds extended with a WordPosition added
        public BoundingRectangle ExtendBounds(BoundingRectangle r, WordPosition wp)
        {
            BoundingRectangle newR;
            if (wp.IsVertical)
                newR = new BoundingRectangle(
                    new Position(Math.Min(r.Min.Row, wp.StartRow), Math.Min(r.Min.Column, wp.StartColumn)),
                    new Position(Math.Max(r.Max.Row, wp.StartRow + wp.Word.Length - 1), Math.Max(r.Max.Column, wp.StartColumn)));
            else
                newR = new BoundingRectangle(
                    new Position(Math.Min(r.Min.Row, wp.StartRow), Math.Min(r.Min.Column, wp.StartColumn)),
                    new Position(Math.Max(r.Max.Row, wp.StartRow), Math.Max(r.Max.Column, wp.StartColumn + wp.Word.Length - 1)));
            return newR;
        }


        // Try to place a word in current layout, following rules of puzzle layout
        // Return true of it's possible, false otherwise
        public PlaceWordStatus CanPlaceWord(WordPosition wp)
        {
            int row = wp.StartRow;
            int column = wp.StartColumn;

            PlaceWordStatus result = PlaceWordStatus.Valid;

            if (wp.IsVertical)
            {
                // Need free cell above
                if (IsOccupiedSquare(row - 1, column)) result = PlaceWordStatus.TooClose;
                // Need free cell below
                if (IsOccupiedSquare(row + wp.Word.Length, column)) result = PlaceWordStatus.TooClose;

                for (int i = 0; i < wp.Word.Length; i++)
                {
                    char l = GetLetter(row + i, column);
                    if (l == wp.Word[i])
                    {
                        // It's Ok to already have a matching letter only if it only belongs to a crossing word (of opposite direction)
                        // In this case, we don't need to check left and right cells
                        foreach (WordPosition loop in GetWordPositionsFromSquare(row + i, column))
                            if (loop.IsVertical == wp.IsVertical)
                                return PlaceWordStatus.Invalid;
                    }
                    else
                    {
                        // We need an empty cell for this letter
                        if (l != '\0')
                            return PlaceWordStatus.Invalid;

                        // We need a free cell on the left and on the right, or else we're too close
                        if (IsOccupiedSquare(row + i, column - 1) || IsOccupiedSquare(row + i, column + 1))
                            result = PlaceWordStatus.TooClose;
                    }
                }
            }
            else
            {
                // Free cell left
                if (IsOccupiedSquare(row, column - 1)) result = PlaceWordStatus.TooClose;
                // Free cell right
                if (IsOccupiedSquare(row, column + wp.Word.Length)) result = PlaceWordStatus.TooClose;

                for (int i = 0; i < wp.Word.Length; i++)
                {
                    char l = GetLetter(row, column + i);
                    if (l == wp.Word[i])
                    {
                        // It's Ok to already have a matching letter only if it only belongs to a crossing word (of opposite direction)
                        foreach (WordPosition loop in GetWordPositionsFromSquare(row, column + i))
                            if (loop.IsVertical == wp.IsVertical)
                                return PlaceWordStatus.Invalid;
                    }
                    else
                    {
                        // We need an empty cell for this letter
                        if (l != '\0')
                            return PlaceWordStatus.Invalid;

                        // We need a free cell above and below, or else we're too close
                        if (IsOccupiedSquare(row - 1, column + i) || IsOccupiedSquare(row + 1, column + i))
                            result = PlaceWordStatus.TooClose;
                    }
                }
            }
            return result;
        }


        // Helper for CanPlaceWord, returns an enum of words covering square (row, column) in current layout
        private IEnumerable<WordPosition> GetWordPositionsFromSquare(int row, int column)
        {
            foreach (var wp in m_WordPositionList)
                if (wp.IsVertical)
                {
                    if (wp.StartColumn == column && row >= wp.StartRow && row < wp.StartRow + wp.Word.Length)
                        yield return wp;
                }
                else
                {
                    if (wp.StartRow == row && column >= wp.StartColumn && column < wp.StartColumn + wp.Word.Length)
                        yield return wp;
                }
        }

        // Helper, returns the number of words that do not intersect with any other word
        public int GetWordsNotConnected()
        {
            List<WordPosition> tempList = new List<WordPosition>(m_WordPositionList);

            int blocksCount = 0;
            for (;;)
            {
                if (tempList.Count == 0)
                    break;
                // Start a new block of connected WordPosition
                blocksCount++;

                WordPosition wp = tempList[0];
                tempList.RemoveAt(0);
                foreach (WordPosition w in GetConnectedWordPositions(wp, tempList))
                    if (tempList.Contains(w))
                        tempList.Remove(w);
            }

            return blocksCount;
        }

        // Private version, returns a list of WordPosition that wp is connected to from wordPositionList
        private static List<WordPosition> GetConnectedWordPositions(WordPosition wp, List<WordPosition> wordPositionList)
        {
            List<WordPosition> tempList = new List<WordPosition>(wordPositionList);
            Stack<WordPosition> toExamine = new Stack<WordPosition>();
            List<WordPosition> connected = new List<WordPosition>();
            toExamine.Push(wp);
            if (tempList.Contains(wp))
                tempList.Remove(wp);

            while (toExamine.Count > 0)
            {
                WordPosition w1 = toExamine.Pop();
                if (!connected.Contains(w1))
                {
                    if (wp != w1)
                        connected.Add(w1);
                    if (tempList.Contains(w1))
                        tempList.Remove(w1);
                    foreach (var w2 in tempList)
                        if (DoWordsIntersect(w1, w2) && !connected.Contains(w2))
                            toExamine.Push(w2);
                }
            }

            return connected;
        }


        // Public version, returns words connected to wp (not including wp) from current layout
        public List<WordPosition> GetConnectedWordPositions(WordPosition wp)
        {
            return GetConnectedWordPositions(wp, m_WordPositionList);
        }


        // Returns true if the two WordPosition intersect
        private static bool DoWordsIntersect(WordPosition word1, WordPosition word2)
        {
            if (word1.IsVertical && word2.IsVertical)
            {
                // Both vertical, different column: no problem
                if (word1.StartColumn != word2.StartColumn) return false;

                // On the same column, check that one row ends before the other starts
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (word1.StartRow + word1.Word.Length - 1 < word2.StartRow
                    || word2.StartRow + word2.Word.Length - 1 < word1.StartRow)
                    return false;

                // They overlap, it's not really an intersection but still count as one
                return true;
            }

            if (!word1.IsVertical && !word2.IsVertical)
            {
                // Both horizontal, different row, no problem
                if (word1.StartRow != word2.StartRow) return false;

                // On the same row, check that one column ends before the other starts
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (word1.StartColumn + word1.Word.Length - 1 < word2.StartColumn
                    || word2.StartColumn + word2.Word.Length - 1 < word1.StartColumn)
                    return false;

                // Overlap of two horizontal words
                return true;
            }

            if (!word1.IsVertical && word2.IsVertical)
            {
                // word1 horizontal, word2 vertical
                // if word2 column does not overlap with word1 columns, no problem
                if (word2.StartColumn < word1.StartColumn || word2.StartColumn > word1.StartColumn + word1.Word.Length - 1)
                    return false;

                // If word2 rows do now overlap with word1 row, no problem
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (word1.StartRow < word2.StartRow || word1.StartRow > word2.StartRow + word2.Word.Length - 1)
                    return false;

                // Otherwise we have an intersection
                return true;
            }

            {
                // word1 vertical, word2 horizontal
                // if word1 column does not overlap with word2 columns, no problem
                if (word1.StartColumn < word2.StartColumn || word1.StartColumn > word2.StartColumn + word2.Word.Length - 1)
                    return false;

                // If word1 rows do now overlap with word2 row, no problem
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (word2.StartRow < word1.StartRow || word2.StartRow > word1.StartRow + word1.Word.Length - 1)
                    return false;

                // Otherwise we have an intersection
                return true;
            }
        }

        public void SaveLayoutAsCode(string outfile)
        {
            using (Stream s = new FileStream(outfile, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(s, Encoding.UTF8))
            {
                foreach (WordPosition wp in m_WordPositionList)
                    sw.WriteLine($"g.Layout.AddWordPositionAndSquares(new WordPosition(\"{wp.Word}\", \"{wp.OriginalWord}\", new PositionOrientation({wp.StartRow}, {wp.StartColumn}, {wp.IsVertical.ToString().ToLower()})));");
            }
        }
    }
}
