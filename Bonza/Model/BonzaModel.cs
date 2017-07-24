// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM Model
// 2017-07-22   PV  First version

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonza.Generator;


namespace Bonza.Editor
{
    class BonzaModel
    {
        public WordPositionLayout Layout = new WordPositionLayout();
        public WordPositionLayout MoveTestLayout;
        public BonzaModel()
        {
            // Demo
            Layout.Read("fruits.layout");
        }

        // Build a copy of Layout without a specific WordPosition to validate placement
        internal void BuildMoveTestLayout(WordPosition movedWordPosition)
        {
            MoveTestLayout = new WordPositionLayout();
            foreach (var wp in Layout.WordPositionList)
                if (wp != movedWordPosition)
                    MoveTestLayout.AddWordPositionAndSquares(wp);
        }

        internal bool CanPlaceWordInMoveTestLayout(WordPosition hitWordPosition, int left, int top)
        {
            WordPosition testWordPosition = new WordPosition
            {
                Word = hitWordPosition.Word,
                IsVertical = hitWordPosition.IsVertical,
                StartColumn = left,
                StartRow = top
            };
            return MoveTestLayout.CanPlaceWord(testWordPosition);
        }

        /*
        // returns True if WordPosition can start at (top, left) with either no intersection or a valid intersection
        internal bool OkPlaceWord(WordPosition word, int column, int row)
        {
            WordPosition test = new WordPosition
            {
                Word = word.Word,
                IsVertical = word.IsVertical,
                StartRow = row,
                StartColumn = column
            };
            return Layout.CanPlaceWord(test);
            //// First bad intersection returns false
            //foreach (WordPosition wp in Layout.WordPositionList)
            //    if (word != wp && BadWordIntersection(test, wp))
            //        return false;
            //// We're good!
            //return true;
        }

        // Returns true if word1 intersects incorrectly with word2
        private bool BadWordIntersection(WordPosition word1, WordPosition word2)
        {
            if (word1.IsVertical && word2.IsVertical)
            {
                // Both vertical, different column: no problem
                if (word1.StartColumn != word2.StartColumn) return false;

                // On the same column, check that one row ends before the other starts
                if (word1.StartRow + word1.Word.Length - 1 < word2.StartRow
                    || word2.StartRow + word2.Word.Length - 1 < word1.StartRow)
                    return false;

                // Overlap of two vertical words is never allowed
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

                // Overlap of two horizontal words is never allowed
                // if (Keyboard.IsKeyDown(Key.LeftShift)) Debugger.Break();
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

                // Otherwise we have an intersection, check if letter matches
                char letter1 = word1.Word[word2.StartColumn - word1.StartColumn];
                char letter2 = word2.Word[word1.StartRow - word2.StartRow];
                if (letter1 == letter2)
                    return false;           // No problem

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

                // Otherwise we have an intersection, check if letter matches
                char letter1 = word1.Word[word2.StartRow - word1.StartRow];
                char letter2 = word2.Word[word1.StartColumn - word2.StartColumn];
                if (letter2 == letter1)
                    return false;

                return true;
            }
        }
        */

        internal void UpdateWordPositionLocation(WordPosition word, int column, int row)
        {
            word.StartColumn = column;
            word.StartRow = row;
        }

    }
}
