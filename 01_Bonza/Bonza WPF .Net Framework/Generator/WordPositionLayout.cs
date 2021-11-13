// WordPositionLayout.cs
// A class representing placed words on a virtual grid of 1-letter squares
// 2017-07-24   PV      Moved as an external class during refactoring


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private readonly Dictionary<int, Square> m_Squares = new Dictionary<int, Square>();

        // Builds m_Squares index for performance (rather than a ValueTuple<int,int> or worse performance-wise, a Position)
        internal static int Index(int row, int column) => (((short)row) << 16) + (short)column;


        public ReadOnlyCollection<WordPosition> WordPositionList => m_WordPositionList.AsReadOnly();

        // Default constructor
        // Needed because we have another constructor defined
        public WordPositionLayout()
        {
        }

        // Copy constructor
        // m_WordPositionsList contains the same references as tho copied layout
        // m_Squares contains new copies of copied layout squares
        // m_WordsList is a new copy of original list
        internal WordPositionLayout(WordPositionLayout copy)
        {
            m_WordPositionList.AddRange(copy.m_WordPositionList);
            using (var e = copy.m_Squares.GetEnumerator())
                while (e.MoveNext())
                    m_Squares.Add(e.Current.Key, new Square(e.Current.Value));
        }


        // Safe version, public
        public PlaceWordStatus AddWordPosition(WordPosition wordPosition)
        {
            if (wordPosition == null)
                throw new ArgumentNullException(nameof(wordPosition));
            if (m_WordPositionList.Contains(wordPosition))
                throw new ArgumentException("WordPosition already in the layout");

            var res = CanPlaceWord(wordPosition, true);
            if (res != PlaceWordStatus.Invalid)
                AddWordPositionNoCheck(wordPosition);
            return res;
        }


        // Low-level function to add a WordPosition to Layout, do not check that placement is correct
        public void AddWordPositionNoCheck(WordPosition wordPosition)
        {
            m_WordPositionList.Add(wordPosition);
            AddSquares(wordPosition);
        }

        // Low-level removal function, public
        public void RemoveWordPosition(WordPosition wordPosition)
        {
            if (wordPosition == null)
                throw new ArgumentNullException(nameof(wordPosition));
            if (!m_WordPositionList.Contains(wordPosition))
                throw new ArgumentException("WordPosition not in the layout");

            RemoveSquares(wordPosition);
            m_WordPositionList.Remove(wordPosition);
        }


        // Private helper
        private void AddSquares(WordPosition wordPosition)
        {
            Debug.Assert(wordPosition != null);

            int row = wordPosition.StartRow;
            int column = wordPosition.StartColumn;
            foreach (char c in wordPosition.Word)
            {
                if (GetSquare(row, column) == null)
                {
                    Square sq = new Square(row, column, c, false, 1);
                    m_Squares.Add(Index(row, column), sq);
                }
                else
                {
                    Square sq = GetSquare(row, column);
                    Debug.Assert(sq.Letter == c);
                    sq.ShareCount++;
                }
                if (wordPosition.IsVertical) row++; else column++;
            }
        }

        private void RemoveSquares(WordPosition wordPosition)
        {
            Debug.Assert(wordPosition != null);

            int row = wordPosition.StartRow;
            int column = wordPosition.StartColumn;
            for (int i = 0; i < wordPosition.Word.Length; i++)
            {
                Square sq = GetSquare(row, column);
                Debug.Assert(sq != null);
                if (sq.ShareCount == 1)
                    m_Squares.Remove(Index(row, column));
                else
                    sq.ShareCount--;

                if (wordPosition.IsVertical) row++; else column++;
            }
        }


        // Returns square at a given position, or null if there is nothing in current layout
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Square GetSquare(int row, int column)
        {
            m_Squares.TryGetValue(Index(row, column), out Square sq);
            return sq;
        }

        // Specialized helper for performance, returns true if there's no square at this position
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsOccupiedSquare(int row, int column)
        {
            return m_Squares.ContainsKey(Index(row, column));
        }

        // Helper: Return letter placed at coordinates (row, column), or EmptyLetter if there is nothing there
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char GetLetter(int row, int column)
        {
            m_Squares.TryGetValue(Index(row, column), out Square sq);
            return sq?.Letter ?? '\0';
        }


        // Compute layout external bounds, note that cell (0,0) is always included in bounding rectangle
        public BoundingRectangle Bounds => m_WordPositionList.Aggregate(new BoundingRectangle(), ExtendBounds);


        // Return layout bounds extended with a WordPosition added
        // Don't use Position version of BoundingRectangle constructor, too slow
        internal static BoundingRectangle ExtendBounds(BoundingRectangle r, WordPosition wordPosition)
        {
            if (wordPosition.IsVertical)
                return new BoundingRectangle(
                    Math.Min(r.Min.Row, wordPosition.StartRow),
                    Math.Max(r.Max.Row, wordPosition.StartRow + wordPosition.Word.Length - 1),
                    Math.Min(r.Min.Column, wordPosition.StartColumn),
                    Math.Max(r.Max.Column, wordPosition.StartColumn));
            else
                return new BoundingRectangle(
                    Math.Min(r.Min.Row, wordPosition.StartRow),
                    Math.Max(r.Max.Row, wordPosition.StartRow),
                    Math.Min(r.Min.Column, wordPosition.StartColumn),
                    Math.Max(r.Max.Column, wordPosition.StartColumn + wordPosition.Word.Length - 1));
        }

        // Try to place a word in current layout, following rules of puzzle layout
        // If withTooClose if false, a too close condition returns Invalid
        // Part of public interface
        public PlaceWordStatus CanPlaceWord(WordPosition wordPosition, bool withTooClose)
        {
            if (wordPosition == null)
                throw new ArgumentNullException(nameof(wordPosition));

            return wordPosition.IsVertical ? CanPlaceVerticalWord(wordPosition, withTooClose)
                                           : CanPlaceHorizontalWord(wordPosition, withTooClose);
        }

        // Helper to reduce cyclomatic complexity
        private PlaceWordStatus CanPlaceVerticalWord(WordPosition wordPosition, bool withTooClose)
        {
            PlaceWordStatus result = PlaceWordStatus.Valid;
            int row = wordPosition.StartRow;
            int column = wordPosition.StartColumn;

            // Need free cell above
            if (IsOccupiedSquare(row - 1, column))
                if (withTooClose)
                    result = PlaceWordStatus.TooClose;
                else
                    return PlaceWordStatus.Invalid;
            // Need free cell below
            if (IsOccupiedSquare(row + wordPosition.Word.Length, column))
                if (withTooClose)
                    result = PlaceWordStatus.TooClose;
                else
                    return PlaceWordStatus.Invalid;

            for (int i = 0; i < wordPosition.Word.Length; i++)
            {
                char l = GetLetter(row + i, column);
                if (l == wordPosition.Word[i])
                {
                    // It's OK to already have a matching letter only if it only belongs to a crossing word (of opposite direction)
                    // In this case, we don't need to check left and right cells
                    foreach (WordPosition loop in GetWordPositionsFromSquare(row + i, column))
                        if (loop.IsVertical == wordPosition.IsVertical)
                            return PlaceWordStatus.Invalid;
                }
                else
                {
                    // We need an empty cell for this letter
                    if (l != '\0')
                        return PlaceWordStatus.Invalid;

                    // We need a free cell on the left and on the right, or else we're too close
                    if (IsOccupiedSquare(row + i, column - 1) || IsOccupiedSquare(row + i, column + 1))
                        if (withTooClose)
                            result = PlaceWordStatus.TooClose;
                        else
                            return PlaceWordStatus.Invalid;
                }
            }

            return result;
        }

        // Helper to reduce cyclomatic complexity
        private PlaceWordStatus CanPlaceHorizontalWord(WordPosition wordPosition, bool withTooClose)
        {
            PlaceWordStatus result = PlaceWordStatus.Valid;
            int row = wordPosition.StartRow;
            int column = wordPosition.StartColumn;

            // Free cell left
            if (IsOccupiedSquare(row, column - 1))
                if (withTooClose)
                    result = PlaceWordStatus.TooClose;
                else
                    return PlaceWordStatus.Invalid;
            // Free cell right
            if (IsOccupiedSquare(row, column + wordPosition.Word.Length))
                if (withTooClose)
                    result = PlaceWordStatus.TooClose;
                else
                    return PlaceWordStatus.Invalid;

            for (int i = 0; i < wordPosition.Word.Length; i++)
            {
                char l = GetLetter(row, column + i);
                if (l == wordPosition.Word[i])
                {
                    // It's OK to already have a matching letter only if it only belongs to a crossing word (of opposite direction)
                    foreach (WordPosition loop in GetWordPositionsFromSquare(row, column + i))
                        if (loop.IsVertical == wordPosition.IsVertical)
                            return PlaceWordStatus.Invalid;
                }
                else
                {
                    // We need an empty cell for this letter
                    if (l != '\0')
                        return PlaceWordStatus.Invalid;

                    // We need a free cell above and below, or else we're too close
                    if (IsOccupiedSquare(row - 1, column + i) || IsOccupiedSquare(row + 1, column + i))
                        if (withTooClose)
                            result = PlaceWordStatus.TooClose;
                        else
                            return PlaceWordStatus.Invalid;
                }
            }

            return result;
        }



        // Helper for CanPlaceWord, returns an enumerable of words covering square (row, column) in current layout
        private IEnumerable<WordPosition> GetWordPositionsFromSquare(int row, int column)
        {
            foreach (var wordPosition in m_WordPositionList)
                if (wordPosition.IsVertical)
                {
                    if (wordPosition.StartColumn == column && row >= wordPosition.StartRow && row < wordPosition.StartRow + wordPosition.Word.Length)
                        yield return wordPosition;
                }
                else
                {
                    if (wordPosition.StartRow == row && column >= wordPosition.StartColumn && column < wordPosition.StartColumn + wordPosition.Word.Length)
                        yield return wordPosition;
                }
        }

        // Helper, returns the number of words that do not intersect with any other word
        public int WordsNotConnectedCount()
        {
            List<WordPosition> tempList = new List<WordPosition>(m_WordPositionList);

            int blocksCount = 0;
            for (; ; )
            {
                if (tempList.Count == 0)
                    break;
                // Start a new block of connected WordPosition
                blocksCount++;

                WordPosition wordPosition = tempList[0];
                tempList.RemoveAt(0);
                foreach (WordPosition w in GetConnectedWordPositions(wordPosition, tempList))
                    if (tempList.Contains(w))
                        tempList.Remove(w);
            }

            return blocksCount;
        }

        // Private version, returns a list of WordPosition that wordPosition is connected to from wordPositionList
        private static List<WordPosition> GetConnectedWordPositions(WordPosition wordPosition, List<WordPosition> wordPositionList)
        {
            List<WordPosition> tempList = new List<WordPosition>(wordPositionList);
            Stack<WordPosition> toExamine = new Stack<WordPosition>();
            List<WordPosition> connected = new List<WordPosition>();
            toExamine.Push(wordPosition);
            if (tempList.Contains(wordPosition))
                tempList.Remove(wordPosition);

            while (toExamine.Count > 0)
            {
                WordPosition w1 = toExamine.Pop();
                if (!connected.Contains(w1))
                {
                    if (wordPosition != w1)
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


        // Public version, returns words connected to wordPosition (not including wordPosition) from current layout
        public IEnumerable<WordPosition> GetConnectedWordPositions(WordPosition wordPosition)
        {
            return GetConnectedWordPositions(wordPosition, m_WordPositionList);
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

        public void SaveLayoutAsCode(string outFile)
        {
            using (StreamWriter sw = new StreamWriter(outFile, false, Encoding.UTF8))
                foreach (WordPosition wordPosition in m_WordPositionList)
                    sw.WriteLine($"g.Layout.AddWordPosition(new WordPosition(\"{wordPosition.Word}\", \"{wordPosition.OriginalWord}\", new PositionOrientation({wordPosition.StartRow}, {wordPosition.StartColumn}, {wordPosition.IsVertical.ToString().ToLower()})));");
        }
    }
}