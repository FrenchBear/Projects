// Grille.cs
// A grille represent a set of crossing words
//
// 2017-07-21   PV      Split from program.cs
// 2017-08-05   PV      Deep rewrite of AddWordsFromFile/AddWordsList/AddWord


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Bonza.Generator
{
    public partial class Grille
    {
        /// <summary>X</summary>
        public WordPositionLayout Layout { get; private set; }
        private readonly Random rnd;        // Initialized in constructor


        /// <summary>Creation of a new grille with an empty layout.</summary>
        /// <param name="seed">Default value of 0 gets a different layout each time.  Use a specific value for reproducible behavior.</param>
        public Grille(int seed = 0)
        {
            rnd = new Random(seed);
            NewLayout();
        }

        /// <summary>Complete reinitialization of layout, WordPosition and Words</summary>
        public void NewLayout()
        {
            Layout = new WordPositionLayout();
        }

        /// <summary>
        /// Adds a list of words from a file to current layout.
        /// Will raise an exception if there is a problem with the file, or if some words are rejected, initial layout preserved in this case.
        /// </summary>
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
            return AddWordsList(wordsList, false) != null;
        }

        /// <summary>Shuffle words after reinitializing layout.</summary>
        /// <returns>Returns true if new placement is successful, or false if placement failed (current layout is preserved in this case)</returns>
        public bool PlaceWordsAgain()
        {
            // AddWordsList keeps a backup of Layout and restore it if placement failed...
            // but since we call it after reinitializing the layout, it won't work for us
            WordPositionLayout backupLayout = new WordPositionLayout(Layout);
            var wordsList = Layout.WordPositionList.Select(wp => wp.OriginalWord).ToList();
            NewLayout();
            if (AddWordsList(wordsList, false) == null)
            {
                Layout = backupLayout;
                return false;
            }
            return true;
        }

        /// <summary>Validate a list of words before adding them to current layout, returning a description of errors.</summary>
        /// <param name="words">List of words to check</param>
        /// <returns>Returns an empty string if no problem is detected, or a descriptive error message</returns>
        /// <remarks>Adding the same words will raise an exception with the same text.</remarks>
        public string CheckWordsList(IList<string> words)
        {
            if (words == null) throw new ArgumentNullException(nameof(words));

            // Plural case helper (French)
            string S(int n) => n > 1 ? "s" : "";

            string result = string.Empty;
            string shortWords = words.Where(w => w.Length <= 2)
                .Aggregate("", (list, next) => string.IsNullOrEmpty(list) ? next : list + ", " + next);
            if (!string.IsNullOrEmpty(shortWords))
                result = $"Mot{S(shortWords.Length)} de longueur <= 2 non autorisé{S(shortWords.Length)}: " + shortWords;

            // Check for duplicates, start with current list of words
            HashSet<string> wordsSet = new HashSet<string>(Layout.WordPositionList.Select(wp => CanonizeWord(wp.OriginalWord)));
            List<string> duplicates = null;
            foreach (string w in words)
                if (w.Length > 2)
                {
                    string cw = CanonizeWord(w);
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


        /// <summary>Returns canonized form of a word that will be displayed.</summary>
        /// <param name="word">Word to canonize</param>
        /// <returns>Canonized form, for instance "NON·SEQUITUR" for word "Non Sequitur"</returns>
        private static string CanonizeWord(string word) => word.ToUpper().Replace(' ', '·');


        /// <summary>Core placement function, adds a list of words to current layout</summary>
        /// <param name="wordsToAddList">List of words to place</param>
        /// <param name="withLayoutBackup">If true, current layout is backed up, and restored if placement of all words failed</param>
        /// <returns>Returns a list of WordPosition for placed words in case of success, or false if placement failed, current layout is preserved in this case</returns>
        public List<WordPosition> AddWordsList(List<string> wordsToAddList, bool withLayoutBackup)
        {
            if (wordsToAddList == null)
                throw new ArgumentNullException(nameof(wordsToAddList));

            // Keep a copy of current layout to restore if placement fails at some point
            WordPositionLayout backupLayout = null;
            if (withLayoutBackup) backupLayout = new WordPositionLayout(Layout);

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
                    if (withLayoutBackup) Layout = backupLayout;      // Restore initial layout
                    return null;
                }
                // On the other hand, if pass was successful, remove all placed words and go for a new pass
                foreach (string placedWord in placedWords)
                    shuffledList.Remove(placedWord);
            }

            return placedWordPositionList;
        }

        /// <summary>Internal core function, adds a word to current layout and place it.</summary>
        /// <param name="originalWord">Word to place</param>
        /// <returns>Returns WordPosition of placed word, or null if the word couldn't be placed</returns>
        private WordPosition AddWord(string originalWord)
        {
            // We need layout
            if (Layout == null) NewLayout();

            string canonizedWord = CanonizeWord(originalWord);

            // If it's the first canonizedWord of the layout, chose random canonizedWord and orientation to start,
            // place it at position (0, 0)
            if (Layout.WordPositionList.Count == 0)
            {
                WordPosition wp = new WordPosition(canonizedWord, originalWord, new PositionOrientation(0, 0, rnd.NextDouble() > 0.5));
                Layout.AddWordPositionAndSquaresNoCheck(wp);
                return wp;
            }

            // Find first all positions where the canonizedWord can be added to current layout;
            List<WordPosition> possibleWordPositions = new List<WordPosition>();
            foreach (WordPosition wordPosition in Layout.WordPositionList)
                TryPlace(canonizedWord, originalWord, wordPosition, possibleWordPositions);
            if (possibleWordPositions.Count == 0)
                return null;

#if false
            // ==========================================================================================================================================
            // Old version, bugged

            WordPosition selectedWordPosition = null;
            (int minRow, int maxRow, int minColumn, int maxColumn) = Layout.GetBoundsOld();
            int surface = int.MaxValue;

            // Take position that minimize layout surface, random selection generates a 'diagonal' puzzle
            // Adjust surface calculation to avoid too much extension in one direction
            foreach (WordPosition wp in possibleWordPositions)
            {
                (int newMinRow, int newMaxRow, int newMinCol, int newMaxCol) = Layout.ExtendBoundsOld(minRow, maxRow, minColumn, maxColumn, wp);
                int newSurface = ComputeAdjustedSurface(newMaxCol - newMinCol + 1, newMaxRow - newMinRow + 1);
                if (newSurface < surface)
                {
                    surface = newSurface;
                    selectedWordPosition = wp;
                }
            }

            // Position is now selected, we can add it to current layout
            Layout.AddWordPositionAndSquaresNoCheck(selectedWordPosition);
            return selectedWordPosition;
            // ==========================================================================================================================================
#else
            // Optimization
            if (possibleWordPositions.Count == 1)
            {
                WordPosition wp = possibleWordPositions.First();
                Layout.AddWordPositionAndSquaresNoCheck(wp);
                return wp;
            }

            // Select a "best position" among those that minimize layout surface extension
            List<WordPositionSurface> selectedWordPositionList = new List<WordPositionSurface>();
            BoundingRectangle r = Layout.GetBounds();

            // Adjust surface calculation to avoid too much extension in one direction
            foreach (WordPosition wp in possibleWordPositions)
            {
                BoundingRectangle newR = Layout.ExtendBounds(r, wp);
                int newSurface = ComputeAdjustedSurface(newR.Max.Column - newR.Min.Column + 1, newR.Max.Row - newR.Min.Row + 1);
                selectedWordPositionList.Add(new WordPositionSurface(wp, newSurface));
            }

            // Take a possibility among those who minimally extend surface, and add it to current layout
            // Chose a random candidate among the 15% best
            selectedWordPositionList.Sort(Comparer<WordPositionSurface>.Create((wps1, wps2) => wps1.Surface - wps2.Surface));
            int index = (int)(selectedWordPositionList.Count * 0.15 * rnd.NextDouble());
            Layout.AddWordPositionAndSquaresNoCheck(selectedWordPositionList[index].WordPosition);
            return selectedWordPositionList[index].WordPosition;
#endif
        }


        /// <summary>Internal class to sort possible WordPositions placement by surface to select the best.</summary>
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


        /// <summary>Adjusted surface calculation that penalize extensions that would make bounding rectangle width/height unbalanced</summary>
        /// <param name="width">Bounding rectangle width</param>
        /// <param name="height">Bounding rectangle height</param>
        /// <returns>Empirically adjusted: surface x (difference width-height)</returns>
        private int ComputeAdjustedSurface(int width, int height)
        {
            return width * height * (width - height) * (width - height);
        }


        /// <summary>Find all the ways to add WordToPlace to placedWord.</summary>
        /// <param name="canonizedWordToPlace">Canonized form of Word to place (ex: NON·SEQUITUR)</param>
        /// <param name="originalWordToPlace">Original form of Word to place (ex: Non Sequitur)</param>
        /// <param name="placedWord">WordPosition to connect to</param>
        /// <param name="possibleWordPositions">A list into all possible possibilities are added</param>
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




        /// <summary>Writes a text representation of current layout on stdout.</summary>
        public void Print() => Print(Console.Out);


        /// <summary>Writes a text representation of current layout in a file.</summary>
        /// <param name="filename">Name of file to write.</param>
        public void Print(string filename)
        {
            using (Stream s = new FileStream(filename, FileMode.CreateNew))
            using (TextWriter tw = new StreamWriter(s, Encoding.UTF8))
                Print(tw);
        }

        /// <summary>Internal low-level text output generation, writes a text representation of current layout.</summary>
        /// <param name="tw">TextWriter to write to.</param>
        private void Print(TextWriter tw)
        {
            // First compute actual bounds
            BoundingRectangle r = Layout.GetBounds();

            // Then print
            for (int row = r.Min.Row; row <= r.Max.Row; row++)
            {
                for (int col = r.Min.Column; col <= r.Max.Column; col++)
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
