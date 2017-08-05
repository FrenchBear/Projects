// Grille.cs
// A grille represent a set of crossing words
// 2017-07-21   PV      Split from program.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Diagnostics;

namespace Bonza.Generator
{
    public partial class Grille
    {
        public WordPositionLayout Layout { get; private set; }
        private readonly Random rnd;        // Initialized in constructor


        public Grille(int seed = 0)
        {
            rnd = new Random(seed);
            NewLayout();
        }

        public void NewLayout()
        {
            Layout = new WordPositionLayout();
        }

        // Memorize the list of words to place as a class variable if later we call PlaceWordsAgain
        private List<string> wordsList;
        public bool PlaceWords(string filename)
        {
            // Will throw an exception in case of problem with file
            wordsList = new List<string>();
            using (Stream s = new FileStream(filename, FileMode.Open))
            using (StreamReader sr = new StreamReader(s, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine()?.Trim();
                    if (!string.IsNullOrWhiteSpace(line))
                        wordsList.Add(line);
                }
            }
            return PlaceWords(wordsList.ToArray());
        }

        public bool PlaceWordsAgain()
        {
            return PlaceWords(wordsList.ToArray());
        }

        // Private automatic placement of words in current grid
        // Returns False if placement failed, true in case of success
        private bool PlaceWords(string[] wordsTable)
        {
            if (wordsTable == null)
                throw new ArgumentNullException(nameof(wordsTable));
            if (wordsTable.Length < 1)
                throw new BonzaException("Au moins un mot requis");
            string shortWords = wordsTable.Where(w => w.Length <= 2)
                .Aggregate("", (list, next) => string.IsNullOrEmpty(list) ? next : list + ", " + next);
            if (!string.IsNullOrEmpty(shortWords))
                throw new BonzaException("Mot(s) de longueur <= 2 non autorisé(s): " + shortWords);

#if TraceBuild
            Stopwatch sw = Stopwatch.StartNew();
#endif
            List<string> words = new List<string>();

            // We need a fresh new layout
            Debug.Assert(Layout != null);
            Debug.Assert(Layout.WordPositionList.Count == 0);

            foreach (string word in wordsTable)
            {
                // A small centered dot is visually better in the grid than a space in words containing spaces
                string canonizedWord = word.ToUpper().Replace(' ', '·');
                if (words.Contains(canonizedWord))
                    throw new BonzaException("Mot en double dans la liste: " + word);
                words.Add(canonizedWord);
            }

            // Chose random word and orientation to start, anchored at position (0, 0)
            words.ShuffleInPlace();
            AddWordPositionToLayout(new WordPosition { Word = words[0], StartColumn = 0, StartRow = 0, IsVertical = rnd.NextDouble() > 0.5 });
            words.RemoveAt(0);

            // Place remaining words
            while (words.Count > 0)
            {
                List<WordPosition> possibleWordPositions = new List<WordPosition>();

                // Try to place one word from list words, after that we restart the outer loop
                bool placed = false;
                for (int i = 0; i < words.Count && !placed; i++)
                {
                    foreach (WordPosition wordPosition in Layout.WordPositionList)
                        TryPlace(words[i], wordPosition, possibleWordPositions);

                    if (possibleWordPositions.Count > 0)
                    {
                        List<WordPosition> selectedWordPositionList = null;
                        BoundingRectangle r = Layout.GetBounds();
                        int surface = int.MaxValue;

                        // Take position that minimize layout surface, random selection generates a 'diagonal' puzzle
                        // Adjust surface calculation to avoid too much extension in one direction
                        foreach (WordPosition wp in possibleWordPositions)
                        {
                            BoundingRectangle newR = Layout.ExtendBounds(r, wp);
                            int newSurface = ComputeAdjustedSurface(newR.MaxColumn - newR.MinColumn + 1, newR.MaxRow - newR.MinRow + 1);
                            if (newSurface < surface)
                            {
                                surface = newSurface;
                                selectedWordPositionList = new List<WordPosition> { wp };
                            }
                        }

                        // Take a possibility among those who minimally extend surface, and add it to current layout
                        AddWordPositionToLayout(selectedWordPositionList.TakeRandom());

                        words.Remove(words[i]);

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

#if TraceBuild
            // Check that the number of squares is correct
            (int MinRow, int MaxRow, int MinColumn, int MaxColumn) = layout.GetBounds();
            WriteLine($"\n{layout.WordPositionList.Count} words and {layout.SquaresCount} squares on a {MaxRow - MinRow + 1}x{MaxColumn - MinColumn + 1} grid in {sw.Elapsed}\n");
#endif
            return true;
        }

        // Add a WordPosition to current layout, and add squares to squares dictionary
        private void AddWordPositionToLayout(WordPosition wp)
        {
#if TraceBuild
            WriteLine(wp.ToString());
#endif

            Layout.AddWordPositionAndSquaresNoCheck(wp);
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
            HashSet<char> wordToPlaceLetters = BreakLetters(wordToPlace);
            HashSet<char> placedWordLetters = BreakLetters(placedWord.Word);

            // Internal helper function, returns a dictionary of (letter, count) for word w
            HashSet<char> BreakLetters(string w)
            {
                var set = new HashSet<char>();
                foreach (char letter in w)
                    if (!set.Contains(letter))
                        set.Add(letter);
                return set;
            }

            // For each letter of wordToPlace, look if placedWord contains this letter at least once
            foreach (char letter in wordToPlaceLetters)
                if (placedWordLetters.Contains(letter))
                {
                    // Matching letter!

                    // Helper, returns a list of positions of letter (external variable) in word
                    List<int> FindPositions(string word)
                    {
                        var list = new List<int>(3);
                        int p = 0;
                        for (;;)
                        {
                            int q = word.IndexOf(letter, p);
                            if (q < 0) break;
                            list.Add(q);
                            p = q + 1;
                        }
                        return list;
                    }

                    // Look for all possible combinations of letter in both words if it's possible to place wordToPlace.
                    foreach (int positionInWordToPlace in FindPositions(wordToPlace))           // positionsInWordToPlace)
                        foreach (int positionInPlacedWord in FindPositions(placedWord.Word))    // positionsInPlacedWord)
                        {
                            WordPosition test = new WordPosition
                            {
                                Word = wordToPlace,
                                IsVertical = !placedWord.IsVertical,
                                StartRow = placedWord.IsVertical ? placedWord.StartRow + positionInPlacedWord : placedWord.StartRow - positionInWordToPlace,
                                StartColumn = placedWord.IsVertical ? placedWord.StartColumn - positionInWordToPlace : placedWord.StartColumn + positionInPlacedWord
                            };
                            if (Layout.CanPlaceWord(test)==PlaceWordStatus.Valid)
                                possibleWordPositions.Add(test);
                        }
                }
        }


        // Write a text representation of current layout on stdout
        public void Print() => Print(Console.Out);

        // Write a text representation of current layout in a file
        public void Print(string outFile)
        {
            using (Stream s = new FileStream(outFile, FileMode.CreateNew))
            using (TextWriter tw = new StreamWriter(s, Encoding.UTF8))
                Print(tw);
        }

        // Write a text representation of current layout on provided TextWriter
        // Internal helper
        private void Print(TextWriter tw)
        {
            // First compute actual bounds
            BoundingRectangle r = Layout.GetBounds();

            // Then print
            for (int row = r.MinRow; row <= r.MaxRow; row++)
            {
                for (int col = r.MinColumn; col <= r.MaxColumn; col++)
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
                        if (!wp.IsVertical && wp.StartRow == row && col >= wp.StartColumn && col < wp.StartColumn + wp.Word.Length)
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

#if !NETCOREAPP1_1
        // Save layout in a .json file
        public void SaveLayout(string outFile)
        {
            Layout.SaveToFile(outFile);
        }

        // Load layout from a .json file
        public void LoadLayout(string inFile)
        {
            Layout = new WordPositionLayout();
            Layout.LoadFromFile(inFile);
        }
#endif

    }
}
