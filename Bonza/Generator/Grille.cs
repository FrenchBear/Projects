// Grille.cs
// A grille represent a set of crossing words
// 2017-07-21   PV      Split from program.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Console;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace Bonza.Generator
{
    public partial class Grille
    {
        readonly bool TraceBuild = false;
        readonly Random rnd;        // Initialized in constructor

        const char EmptyLetter = '\0';        // When there is nothing on the grid

        WordPositionLayout Layout = new WordPositionLayout();


        public Grille(int seed = 0)
        {
            rnd = new Random(seed);
        }


        public bool PlaceWords(string[] wordsTable)
        {
            if (wordsTable == null)
                throw new ArgumentNullException(nameof(wordsTable));

            Stopwatch sw = Stopwatch.StartNew();
            List<string> Words = new List<string>();

            // Only work with an empty puzzle; Basic word checks
            Debug.Assert(Layout.WordPositionList.Count == 0);
            Debug.Assert(wordsTable.Length >= 2);
            foreach (string word in wordsTable)
            {
                // A small centered dot is visually better in the grid than a space in words containing spaces
                string canonizedWord = word.ToUpper(CultureInfo.InvariantCulture).Replace(' ', '·');
                if (Words.Contains(canonizedWord))
                    throw new BonzaException("Duplicate word in the list: " + word);
                Words.Add(canonizedWord);
            }

            // Chose random word and orientation to start, anchored at position (0, 0)
            Words = Words.Shuffle();
            AddWordPositionToLayout(new WordPosition { Word = Words[0], StartColumn = 0, StartRow = 0, IsVertical = rnd.NextDouble() > 0.5 });
            Words.RemoveAt(0);

            // Place remaining words
            while (Words.Count > 0)
            {
                List<WordPosition> possibleWordPositions = new List<WordPosition>();

                // Try to place one word from list words, after that we restart the outer loop
                bool placed = false;
                for (int i = 0; i < Words.Count && !placed; i++)
                {
                    for (int j = 0; j < Layout.WordPositionList.Count; j++)
                        TryPlace(Words[i], Layout.WordPositionList[j], possibleWordPositions);

                    if (possibleWordPositions.Count > 0)
                    {
                        WordPosition selectedWordPosition = null;
                        (int minRow, int maxRow, int minColumn, int maxColumn) = Layout.GetBounds();
                        int surface = int.MaxValue;

                        // Take position that minimize layout surface, random selection generates a 'diagonal' puzzle
                        // Adjust surface calculation to avoid too much extension in one direction
                        foreach (WordPosition wp in possibleWordPositions.Shuffle())
                        {
                            (int newMinRow, int newMaxRow, int newMinCol, int newMaxCol) = Layout.ExtendBounds(minRow, maxRow, minColumn, maxColumn, wp);
                            int newSurface = ComputeAdjustedSurface(newMaxCol - newMinCol + 1, newMaxRow - newMinRow + 1);
                            if (newSurface < surface)
                            {
                                surface = newSurface;
                                selectedWordPosition = wp;
                            }
                        }

                        // Position is now selected, we can add it to current layout
                        AddWordPositionToLayout(selectedWordPosition);

                        Words.Remove(Words[i]);

                        placed = true;
                    }
                }

                if (!placed)
                {
                    // throw new BonzaException("Error during words placement, no possible way to place remaining words");
                    Layout = null;
                    return false;
                }
            }

            // Check that the number of squares is correct
            if (TraceBuild)
            {
                (int minRow, int maxRow, int minColumn, int maxColumn) = Layout.GetBounds();
                WriteLine($"\n{Layout.WordPositionList.Count} words and {Layout.SquaresCount} squares on a {maxRow - minRow + 1}x{maxColumn - minColumn + 1} grid in {sw.Elapsed}\n");
            }
            return true;
        }

        // Add a WordPosition to current layout, and add squares to squares dictionary
        private void AddWordPositionToLayout(WordPosition wp)
        {
            if (TraceBuild)
                WriteLine(wp.ToString());

            Layout.AddWordPositionAndSquares(wp);
        }

        // Adjusted surface calculation that penalize extensions that would make bounding 
        // rectangle width/height unbalanced: compute surface x (difference width-height)²
        private int ComputeAdjustedSurface(int width, int height)
        {
            return width * height * (width - height) * (width - height);
        }

        // Try to connect wordToPlace to placedWord
        // Adds all possible solutions as WordPosition objects to provided possibleWordPositions
        private void TryPlace(string wordToPlace, WordPosition placedWord, List<WordPosition> possibleWordPositions)
        {
            // Build a dictionary of (letter, count) for each word
            Dictionary<char, int> wordToPlaceLetters = BreakLetters(wordToPlace);
            Dictionary<char, int> placedWordLetters = BreakLetters(placedWord.Word);

            // Internal helper function, returns a dictionary of (letter, count) for word w
            Dictionary<char, int> BreakLetters(string w)
            {
                var dic = new Dictionary<char, int>();
                foreach (char letter in w)
                {
                    if (dic.ContainsKey(letter))
                        dic[letter]++;
                    else
                        dic.Add(letter, 1);
                }
                return dic;
            }

            // For each letter of wordToPlace, look if placedWord contains this letter at least once
            foreach (char letter in wordToPlaceLetters.Keys.ToList().Shuffle())
                if (placedWordLetters.Keys.Contains(letter))
                {
                    // Matching letter!
                    List<int> positionsInWordToPlace = Enumerable.Range(0, wordToPlace.Length).Where(i => wordToPlace[i] == letter).ToList().Shuffle();
                    List<int> positionsInPlacedWord = Enumerable.Range(0, placedWord.Word.Length).Where(i => placedWord.Word[i] == letter).ToList().Shuffle();

                    // Look for all possible combinations of letter in both words if it's possible to place wordToPlace.
                    foreach (int positionInWordToPlace in positionsInWordToPlace)
                        foreach (int positionInPlacedWord in positionsInPlacedWord)
                        {
                            WordPosition testWP = new WordPosition
                            {
                                Word = wordToPlace,
                                IsVertical = !placedWord.IsVertical,
                                StartRow = placedWord.IsVertical ? (placedWord.StartRow + positionInPlacedWord) : (placedWord.StartRow - positionInWordToPlace),
                                StartColumn = placedWord.IsVertical ? (placedWord.StartColumn - positionInWordToPlace) : (placedWord.StartColumn + positionInPlacedWord)
                            };
                            if (Layout.CanPlaceWord(testWP))
                                possibleWordPositions.Add(testWP);
                        }
                }
        }


        // Write a text representation of current layout on stdout
        public void Print() => Print(Console.Out);

        // Write a text representation of current layout in a file
        public void Print(string outFile)
        {
            using (TextWriter tw = new StreamWriter(outFile, false, Encoding.UTF8))
                Print(tw);
        }

        // Write a text representation of current layout on provided TextWriter
        // Internal helper
        private void Print(TextWriter tw)
        {
            // First compute actual bounds
            (int minRow, int maxRow, int minColumn, int maxColumn) = Layout.GetBounds();

            // Then print
            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minColumn; col <= maxColumn; col++)
                {
                    bool found = false;

                    foreach (WordPosition wp in Layout.WordPositionList)
                    {
                        if (wp.IsVertical && wp.StartColumn == col && row >= wp.StartRow && row < wp.StartRow + wp.Word.Length)
                        {
                            tw.Write(wp.Word[row - wp.StartRow]);
                            found = true;
                            break;
                        }
                        else if (!wp.IsVertical && wp.StartRow == row && col >= wp.StartColumn && col < wp.StartColumn + wp.Word.Length)
                        {
                            tw.Write(wp.Word[col - wp.StartColumn]);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        tw.Write(' ');
                    tw.Write(' ');
                }
                tw.WriteLine();
            }
        }

        // Save layout in a .json file
        public void SaveLayout(string outFile)
        {
            Layout.SaveToFile(outFile);
        }

        // Load layout from a .json file
        public void LoadLayout(string inFile)
        {
            Layout.LoadFromFile(inFile);
        }

    }
}
