﻿// WordPositionLayout.cs
// A class representing placed words on a virtual grid of 1-letter squares
// 2017-07-24   PV      Moved as an external class during refactoring

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonza.Generator
{
    public class WordPositionLayout
    {
        private List<WordPosition> m_WordPositionList = new List<WordPosition>();
        private Dictionary<(int, int), Square> m_Squares = new Dictionary<(int, int), Square>();

        public ReadOnlyCollection<WordPosition> WordPositionList => m_WordPositionList.AsReadOnly();

        public int SquaresCount => m_Squares.Count;

        public void AddWordPositionAndSquares(WordPosition wp)
        {
            if (wp == null)
                throw new ArgumentNullException(nameof(wp));
            if (m_WordPositionList.Contains(wp))
                throw new ArgumentException("WordPosition already in the layout");
            m_WordPositionList.Add(wp);

            // Add new squares
            AddSquares(wp);
        }

        private void AddSquares(WordPosition wp)
        {
            int row = wp.StartRow;
            int column = wp.StartColumn;
            for (int il = 0; il < wp.Word.Length; il++)
            {
                if (GetSquare(row, column) == null)
                {
                    Square sq = new Square { Row = row, Column = column, Letter = wp.Word[il], IsInChunk = false };
                    m_Squares.Add((row, column), sq);
                }
                else
                    Debug.Assert(GetSquare(row, column).Letter == wp.Word[il]);
                if (wp.IsVertical) row++; else column++;
            }
        }

        // Returns square at a given position, or null if there is nothing in current layout
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetSquare(int row, int column)
        {
            if (IsOccupiedSquare(row, column))
                return m_Squares[(row, column)];
            else
                return null;
        }

        // Specialized helper for performance, returns true if there's no square at this position
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsOccupiedSquare(int row, int column)
        {
            return m_Squares.ContainsKey((row, column));
        }


        // Helper: Return letter placed at coordinates (row, column), or EmptyLetter if there is nothing there
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char GetLetter(int row, int column)
        {
            //return GetSquare(row, column)?.Letter ?? '\0';
            if (m_Squares.ContainsKey((row, column)))
                return m_Squares[(row, column)].Letter;
            else
                return '\0';
        }

        // Compute layout external bounds
        public (int minRow, int maxRow, int minColumn, int maxColumn) GetBounds()
        {
            int minRow = 0, maxRow = 0, minColumn = 0, maxColumn = 0;
            foreach (WordPosition wp in m_WordPositionList)
                (minRow, maxRow, minColumn, maxColumn) = ExtendBounds(minRow, maxRow, minColumn, maxColumn, wp);
            return (minRow, maxRow, minColumn, maxColumn);
        }

        // Return layout (minRow, maxRow, minColumn, maxColumn) extended with a WordPosition added
        public (int newMinRow, int newMaxRow, int newMinColumn, int newMaxColumn) ExtendBounds(int minRow, int maxRow, int minColumn, int maxColumn, WordPosition wp)
        {
            int newMinRow, newMaxRow, newMinColumn, newMaxColumn;
            if (wp.IsVertical)
            {
                newMinRow = Math.Min(minRow, wp.StartRow);
                newMaxRow = Math.Max(maxRow, wp.StartRow + wp.Word.Length - 1);
                newMinColumn = Math.Min(minColumn, wp.StartColumn);
                newMaxColumn = Math.Max(maxColumn, wp.StartColumn);
            }
            else
            {
                newMinRow = Math.Min(minRow, wp.StartRow);
                newMaxRow = Math.Max(maxRow, wp.StartRow);
                newMinColumn = Math.Min(minColumn, wp.StartColumn);
                newMaxColumn = Math.Max(maxColumn, wp.StartColumn + wp.Word.Length - 1);
            }
            return (newMinRow, newMaxRow, newMinColumn, newMaxColumn);
        }

        // Try to place a word in current layout, following rules of puzzle layout
        // Return true of it's possible, false otherwise
        public bool CanPlaceWord(WordPosition wp)
        {
            int row = wp.StartRow;
            int column = wp.StartColumn;

            if (wp.IsVertical)
            {
                // Need free cell above
                if (IsOccupiedSquare(row - 1, column)) return false;
                // Need free cell below
                if (IsOccupiedSquare(row + wp.Word.Length, column)) return false;

                bool prevousLetterMatch = false;
                for (int i = 0; i < wp.Word.Length; i++)
                    if (GetLetter(row + i, column) == wp.Word[i])
                    {   // If we have a match, we're almost good, need to verify that previous
                        // letter was not a match to avoid overlapping a smaller word
                        if (prevousLetterMatch) return false;
                        prevousLetterMatch = true;
                    }
                    else
                    {
                        // We need an empty cell, and a free cell on the left and on the right
                        if (IsOccupiedSquare(row + i, column - 1) || IsOccupiedSquare(row + i, column) || IsOccupiedSquare(row + i, column + 1))
                            return false;
                        prevousLetterMatch = false;
                    }
            }
            else
            {
                // Free cell left
                if (IsOccupiedSquare(row, column - 1)) return false;
                // Free cell right
                if (IsOccupiedSquare(row, column + wp.Word.Length)) return false;

                bool prevousLetterMatch = false;
                for (int i = 0; i < wp.Word.Length; i++)
                    if (GetLetter(row, column + i) == wp.Word[i])
                    {   // If we have a match, we're almost good, need to verify that previous
                        // letter was not a match to avoid overlapping a smaller word
                        if (prevousLetterMatch) return false;
                        prevousLetterMatch = true;
                    }
                    else
                    {
                        if (IsOccupiedSquare(row - 1, column + i) || IsOccupiedSquare(row, column + i) || IsOccupiedSquare(row + 1, column + i))
                            return false;
                        prevousLetterMatch = false;
                    }
            }
            return true;
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
                if (word1.StartRow + word1.Word.Length - 1 < word2.StartRow
                    || word2.StartRow + word2.Word.Length - 1 < word1.StartRow)
                    return false;

                // They overlap, it's not really an intersection but still count as one
                return true;
            }
            else if (!word1.IsVertical && !word2.IsVertical)
            {
                // Both horizontal, different row, no problem
                if (word1.StartRow != word2.StartRow) return false;

                // On the same row, check that one column ends before the other starts
                if (word1.StartColumn + word1.Word.Length - 1 < word2.StartColumn
                    || word2.StartColumn + word2.Word.Length - 1 < word1.StartColumn)
                    return false;

                // Overlap of two horizontal words
                return true;
            }
            else if (!word1.IsVertical && word2.IsVertical)
            {
                // word1 horizontal, word2 vertical
                // if word2 column does not overlap with word1 columns, no problem
                if (word2.StartColumn < word1.StartColumn || word2.StartColumn > word1.StartColumn + word1.Word.Length - 1)
                    return false;

                // If word2 rows do now overlap with word1 row, no problem
                if (word1.StartRow < word2.StartRow || word1.StartRow > word2.StartRow + word2.Word.Length - 1)
                    return false;

                // Otherwise we have an intersection
                return true;
            }
            else
            {
                // word1 vertical, word2 horizontal
                // if word1 column does not overlap with word2 columns, no problem
                if (word1.StartColumn < word2.StartColumn || word1.StartColumn > word2.StartColumn + word2.Word.Length - 1)
                    return false;

                // If word1 rows do now overlap with word2 row, no problem
                if (word2.StartRow < word1.StartRow || word2.StartRow > word1.StartRow + word1.Word.Length - 1)
                    return false;

                // Otherwise we have an intersection
                return true;
            }
        }


        // Save layout in a .json file
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

            m_Squares = new Dictionary<(int, int), Square>();
            foreach (var wp in m_WordPositionList)
                AddSquares(wp);
        }
    }
}
