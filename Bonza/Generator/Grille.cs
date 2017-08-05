// Grille.cs
// A grille represent a set of crossing words
//
// 2017-07-21   PV      Split from program.cs
// 2017-08-05   PV      Deep rewrite of AddWordsFromFile/AddWordsList/AddWord


using System;
using System.Collections;
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

        // Helper, Adds a list of words from a file
        // Will raise an exception if there is a problem with the file, or if some words are rejected
        public bool AddWordsFromFile(string filename)
        {
            // Will throw an exception in case of problem with file
            var wordsList = new List<string>();
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
            return AddWordsList(wordsList) != null;
        }

        // ToDo: Rewrite
        public bool PlaceWordsAgain()
        {
            //return AddWordsList(m_LayoutWordsList.ToArray());
            return true;
        }

        // Helper to validate words before adding them
        // This function will return a string error message, while adding the same words will raise an exception
        public string CheckWordsList(IList<string> words)
        {
            if (words == null) throw new ArgumentNullException(nameof(words));

            string S(int n) => n > 1 ? "s" : "";

            string result = string.Empty;
            string shortWords = words.Where(w => w.Length <= 2)
                .Aggregate("", (list, next) => string.IsNullOrEmpty(list) ? next : list + ", " + next);
            if (!string.IsNullOrEmpty(shortWords))
                result = $"Mot{S(shortWords.Length)} de longueur <= 2 non autorisé{S(shortWords.Length)}: " + shortWords;

            // Check for duplicates, start with current list of words
            HashSet<string> wordsSet = new HashSet<string>(Layout.WordsList.Select(CanonizedWord));
            List<string> duplicates = null;
            foreach (string w in words)
                if (w.Length > 2)
                {
                    string cw = CanonizedWord(w);
                    if (wordsSet.Contains(cw))
                    {
                        if (duplicates == null) duplicates = new List<string>();
                        duplicates.Add(w);
                    }
                    else
                        wordsSet.Add(cw);
                }
            if (duplicates?.Count > 0)
            {
                if (!string.IsNullOrEmpty(result)) result += "\r\n";
                result += $"Mot{S(duplicates.Count)} en double: " + duplicates.Aggregate("",
                    (list, next) => string.IsNullOrEmpty(list) ? next : list + ", " + next);
            }

            return result;
        }

        private static string CanonizedWord(string word) => word.ToUpper().Replace(' ', '·');


        // Returns False if placement failed, true in case of success
        public List<WordPosition> AddWordsList(List<string> wordsToAddList)
        {
            if (wordsToAddList == null)
                throw new ArgumentNullException(nameof(wordsToAddList));

            // Keep a copy of current layout to restore if placement fails at some point
            WordPositionLayout backupLayout = new WordPositionLayout(Layout);

            string checkMessage = CheckWordsList(wordsToAddList);
            if (!string.IsNullOrEmpty(checkMessage))
                throw new BonzaException(checkMessage);

            List<string> shuffledList = new List<string>(wordsToAddList).Shuffle();
            List<WordPosition> placedWordPositionList = new List<WordPosition>();

            while (shuffledList.Count > 0)
            {
                List<string> placedWords = new List<string>();
                foreach (string word in shuffledList)
                {
                    WordPosition wp = AddWord(word);
                    if (wp != null)
                    {
                        placedWords.Add(word);
                        placedWordPositionList.Add(wp);
                    }
                }
                // If at the end of this loop no canonizedWord has been placed, we have a problem...
                if (placedWords.Count == 0)
                {
                    Layout = backupLayout;      // Restore initial layout
                    return null;
                }
                // On the other hand, if pass was successful, remove all placed words and go for a new pass
                foreach (string placedWord in placedWords)
                    shuffledList.Remove(placedWord);
            }

            Debug.Assert(Layout.WordsList.Count == Layout.WordPositionList.Count);
            return placedWordPositionList;
        }

        // Core function, adds a canonizedWord to current layout and place it
        // Returns null if the canonizedWord couldn't be placed
        private WordPosition AddWord(string originalWord)
        {
            // We need layout
            if (Layout == null) NewLayout();

            string canonizedWord = CanonizedWord(originalWord);

            // If it's the first canonizedWord of the layout, chose random canonizedWord and orientation to start,
            // place it at position (0, 0)
            if (Layout.WordPositionList.Count == 0)
            {
                WordPosition wp = new WordPosition(canonizedWord, originalWord, new PositionOrientation(0,0, rnd.NextDouble() > 0.5));
                AddWordPositionToLayout(wp);
                return wp;
            }

            // Find first all positions where the canonizedWord can be added to current layout;
            List<WordPosition> possibleWordPositions = new List<WordPosition>();
            foreach (WordPosition wordPosition in Layout.WordPositionList)
                TryPlace(canonizedWord, originalWord, wordPosition, possibleWordPositions);
            if (possibleWordPositions.Count == 0)
                return null;

            // Optimization
            if (possibleWordPositions.Count == 1)
            {
                WordPosition wp = possibleWordPositions.First();
                AddWordPositionToLayout(wp);
                return wp;
            }

            // Select a "best position" among those that minimize layout surface extension
            List<WordPositionSurface> selectedWordPositionList = new List<WordPositionSurface>();
            BoundingRectangle r = Layout.GetBounds();

            // Adjust surface calculation to avoid too much extension in one direction
            foreach (WordPosition wp in possibleWordPositions)
            {
                BoundingRectangle newR = Layout.ExtendBounds(r, wp);
                int newSurface = ComputeAdjustedSurface(newR.MaxColumn - newR.MinColumn + 1, newR.MaxRow - newR.MinRow + 1);
                selectedWordPositionList.Add(new WordPositionSurface(wp, newSurface));
            }

            // Take a possibility among those who minimally extend surface, and add it to current layout
            // Chose a random candidate among the 15% best
            selectedWordPositionList.Sort(Comparer<WordPositionSurface>.Create((wps1, wps2) => wps1.Surface - wps2.Surface));
            int index = (int)(selectedWordPositionList.Count * 0.15 * rnd.NextDouble());
            AddWordPositionToLayout(selectedWordPositionList[index].WordPosition);
            return selectedWordPositionList[index].WordPosition;
        }

        private class WordPositionSurface
        {
            internal readonly WordPosition WordPosition;
            internal readonly int Surface;

            public WordPositionSurface(WordPosition wordPosition, int surface)
            {
                WordPosition = wordPosition;
                Surface = surface;
            }
        }


        // Add a WordPosition to current layout, and add squares to squares dictionary
        private void AddWordPositionToLayout(WordPosition wp)
        {
            Layout.AddWordPositionAndSquaresNoCheck(wp);
        }

        // Adjusted surface calculation that penalize extensions that would make bounding
        // rectangle width/height unbalanced: compute surface x (difference width-height)²
        private int ComputeAdjustedSurface(int width, int height)
        {
            return width * height * (width - height) * (width - height);
        }

        // Try to connect canonizedWordToPlace to placedWord
        // Adds all possible solutions as WordPosition objects to provided possibleWordPositions
        private void TryPlace(string canonizedWordToPlace, string originalWordToPlace, WordPosition placedWord, List<WordPosition> possibleWordPositions)
        {
            // Build a dictionary of (letter, count) for each canonizedWord
            HashSet<char> wordToPlaceLetters = BreakLetters(canonizedWordToPlace);
            HashSet<char> placedWordLetters = BreakLetters(placedWord.Word);

            // Internal helper function, returns a dictionary of (letter, count) for canonizedWord w
            HashSet<char> BreakLetters(string w)
            {
                var set = new HashSet<char>();
                foreach (char letter in w)
                    if (!set.Contains(letter))
                        set.Add(letter);
                return set;
            }

            // For each letter of canonizedWordToPlace, look if placedWord contains this letter at least once
            foreach (char letter in wordToPlaceLetters)
                if (placedWordLetters.Contains(letter))
                {
                    // Matching letter!

                    // Helper, returns a list of positions of letter (external variable) in canonizedWord
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

                    // Look for all possible combinations of letter in both words if it's possible to place canonizedWordToPlace.
                    foreach (int positionInWordToPlace in FindPositions(canonizedWordToPlace))           // positionsInWordToPlace)
                        foreach (int positionInPlacedWord in FindPositions(placedWord.Word))    // positionsInPlacedWord)
                        {
                            WordPosition test = new WordPosition(canonizedWordToPlace, originalWordToPlace,
                                new PositionOrientation(
                                    placedWord.IsVertical
                                        ? placedWord.StartRow + positionInPlacedWord
                                        : placedWord.StartRow - positionInWordToPlace,
                                    placedWord.IsVertical
                                        ? placedWord.StartColumn - positionInWordToPlace
                                        : placedWord.StartColumn + positionInPlacedWord,
                                    !placedWord.IsVertical));
                            if (Layout.CanPlaceWord(test) == PlaceWordStatus.Valid)
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

    }
}
