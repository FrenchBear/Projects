﻿// WordPositionLayout.cs
// A class representing placed words on a virtual grid of 1-letter squares
//
// 2017-07-24   PV      Moved as an external class during refactoring
// 2017-08-05   PV      Struct Position for .Net Core


using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
#if !NETCOREAPP1_1
using Newtonsoft.Json;
#endif

namespace Bonza.Generator
{
    public enum PlaceWordStatus
    {
        Valid,          // Placement is good
        TooClose,       // No overlap, but word touches another word in an incorrect way
        Invalid         // Overlap or touching a word of same orientation
    }


    public struct BoundingRectangle
    {
        public int MinRow, MaxRow, MinColumn, MaxColumn;

        public BoundingRectangle(int minRow, int maxRow, int minColumn, int maxColumn)
        {
            MinRow = minRow;
            MaxRow = maxRow;
            MinColumn = minColumn;
            MaxColumn = maxColumn;
        }
    }

    public struct Position
    {
        public int Row, Column;

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }


    public class WordPositionLayout
    {
        private List<WordPosition> m_WordPositionList = new List<WordPosition>();
        private Dictionary<Position, Square> m_Squares = new Dictionary<Position, Square>();

        public ReadOnlyCollection<WordPosition> WordPositionList => m_WordPositionList.AsReadOnly();

        public int SquaresCount => m_Squares.Count;


        // Safe version
        public PlaceWordStatus AddWordPositionAndSquares(WordPosition wp)
        {
            if (wp == null)
                throw new ArgumentNullException(nameof(wp));
            if (m_WordPositionList.Contains(wp))
                throw new ArgumentException("WordPosition already in the layout");
            if (m_WordPositionList.Any(
                wordPosition => string.Compare(wordPosition.Word, wp.Word, StringComparison.OrdinalIgnoreCase) == 0))
                throw new ArgumentException("Word already in layout list of words");

            var res = CanPlaceWord(wp);
            if (res != PlaceWordStatus.Invalid)
                AddWordPositionAndSquaresNoCheck(wp);
            return res;
        }



        // Fast version, do not check that placement is correct
        public void AddWordPositionAndSquaresNoCheck(WordPosition wp)
        {
            m_WordPositionList.Add(wp);
            AddSquares(wp);
        }

        public void RemoveWordPositionAndSquares(WordPosition wp)
        {
            if (wp == null)
                throw new ArgumentNullException(nameof(wp));
            if (!m_WordPositionList.Contains(wp))
                throw new ArgumentException("WordPosition not in the layout");

            RemoveSquares(wp);
            m_WordPositionList.Remove(wp);
        }


        private void AddSquares(WordPosition wp)
        {
            Debug.Assert(wp != null);

            int row = wp.StartRow;
            int column = wp.StartColumn;
            foreach (char c in wp.Word)
            {
                if (GetSquare(row, column) == null)
                {
                    Square sq = new Square {Row = row, Column = column, Letter = c, IsInChunk = false, ShareCount = 1};
                    m_Squares.Add(new Position(row, column), sq);
                }
                else
                {
                    Square sq = GetSquare(row, column);
                    Debug.Assert(sq.Letter == c);
                    sq.ShareCount++;
                }
                if (wp.IsVertical) row++;
                else column++;
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

                if (wp.IsVertical) row++;
                else column++;
            }
        }


        // Returns square at a given position, or null if there is nothing in current layout
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetSquare(int row, int column)
        {
            return IsOccupiedSquare(row, column) ? m_Squares[new Position(row, column)] : null;
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
            //return GetSquare(row, column)?.Letter ?? '\0';
            return m_Squares.ContainsKey(new Position(row, column))
                ? m_Squares[new Position(row, column)].Letter
                : '\0';
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
            BoundingRectangle newR = new BoundingRectangle();
            if (wp.IsVertical)
            {
                newR.MinRow = Math.Min(r.MinRow, wp.StartRow);
                newR.MaxRow = Math.Max(r.MaxRow, wp.StartRow + wp.Word.Length - 1);
                newR.MinColumn = Math.Min(r.MinColumn, wp.StartColumn);
                newR.MaxColumn = Math.Max(r.MaxColumn, wp.StartColumn);
            }
            else
            {
                newR.MinRow = Math.Min(r.MinRow, wp.StartRow);
                newR.MaxRow = Math.Max(r.MaxRow, wp.StartRow);
                newR.MinColumn = Math.Min(r.MinColumn, wp.StartColumn);
                newR.MaxColumn = Math.Max(r.MaxColumn, wp.StartColumn + wp.Word.Length - 1);
            }
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
        private static List<WordPosition> GetConnectedWordPositions(WordPosition wp,
            List<WordPosition> wordPositionList)
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
                if (word2.StartColumn < word1.StartColumn ||
                    word2.StartColumn > word1.StartColumn + word1.Word.Length - 1)
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
                if (word1.StartColumn < word2.StartColumn ||
                    word1.StartColumn > word2.StartColumn + word2.Word.Length - 1)
                    return false;

                // If word1 rows do now overlap with word2 row, no problem
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (word2.StartRow < word1.StartRow || word2.StartRow > word1.StartRow + word1.Word.Length - 1)
                    return false;

                // Otherwise we have an intersection
                return true;
            }
        }

#if !NETCOREAPP1_1 // Save layout in a .json file
        public void SaveToFile(string outFile)
        {
            string json = JsonConvert.SerializeObject(m_WordPositionList, Formatting.Indented);
            File.WriteAllText(outFile, json);
        }

        // Load layout from a .json file
        public void LoadFromFile(string inFile)
        {
            string text = File.ReadAllText(inFile);
            m_WordPositionList = JsonConvert.DeserializeObject<List<WordPosition>>(text);

            m_Squares = new Dictionary<Position, Square>();
            foreach (var wp in m_WordPositionList)
                AddSquares(wp);
        }
    }
#endif
    }
}