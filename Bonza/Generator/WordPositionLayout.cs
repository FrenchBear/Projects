// WordPositionLayout.cs
// A class representing placed words on a virtual grid of 1-letter squares
// 2017-07-24   PV      Moved as an external class during refactoring

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public Square GetSquare(int row, int column)
        {
            if (m_Squares.ContainsKey((row, column)))
                return m_Squares[(row, column)];
            else
                return null;
        }


        // Helper: Return letter placed at coordinates (row, column), or EmptyLetter if there is nothing there
        public char GetLetter(int row, int column)
        {
            return GetSquare(row, column)?.Letter ?? '\0';
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
                if (GetSquare(row - 1, column) != null) return false;
                // Need free cell below
                if (GetSquare(row + wp.Word.Length, column) != null) return false;

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
                        if (GetLetter(row + i, column - 1) != '\0' || GetLetter(row + i, column) != '\0' || GetLetter(row + i, column + 1) != '\0')
                            return false;
                        prevousLetterMatch = false;
                    }
            }
            else
            {
                // Free cell left
                if (GetSquare(row, column - 1) != null) return false;
                // Free cell right
                if (GetSquare(row, column + wp.Word.Length) != null) return false;

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
                        if (GetLetter(row - 1, column + i) != '\0' || GetLetter(row, column + i) != '\0' || GetLetter(row + 1, column + i) != '\0')
                            return false;
                        prevousLetterMatch = false;
                    }
            }
            return true;
        }

        // Helper, returns the number of words that do not intersect with any other word
        public int GetWordsNotConnected()
        {
            int count = 0;
            for (int i = 0; i < m_WordPositionList.Count; i++)
            {
                WordPosition word1 = m_WordPositionList[i];
                bool intersect = false;
                for (int j = i + 1; j < m_WordPositionList.Count; j++)
                {
                    WordPosition word2 = m_WordPositionList[j];
                    if (IntersectWords(word1, word2))
                    {
                        intersect = true;
                        break;
                    }
                }
                if (!intersect)
                    count++;
            }
            return count;
        }


        // Returns true if the two WordPosition intersect
        private bool IntersectWords(WordPosition word1, WordPosition word2)
        {
            if (word1.IsVertical && word2.IsVertical)
            {
                // Both vertical, different column: no problem
                if (word1.StartColumn != word2.StartColumn) return false;

                // On the same column, check that one row ends before the other starts
                if (word1.StartRow + word1.Word.Length - 1 < word2.StartRow
                    || word2.StartRow + word2.Word.Length - 1 < word1.StartRow)
                    return false;

                // They overlap
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
