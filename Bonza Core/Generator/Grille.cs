// Grille.cs
// A grille represent a set of crossing words
//
// 2017-07-21   PV      Split from program.cs
// 2017-08-05   PV      Deep rewrite of AddWordsFromFile/AddWordsList/AddWord
// 2017-08-07	PV		Performance refactoring


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
            return AddWordsList(wordsList, true) != null;
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

            // Check for duplicates, start with current list of words (canonized)
            HashSet<string> wordsSet = new HashSet<string>(Layout.WordPositionList.Select(wp => wp.Word));
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
                        Debug.WriteLine($"Placed {placedWordPositionList.Count}/{wordsToAddList.Count}: {wp.Word}");
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


        // Core function, adds a canonizedWord to current layout and place it
        // Returns WordPosition of placed word, or null if the canonizedWord couldn't be placed
        private WordPosition AddWord(string originalWord)
        {
            // We need layout, only of a previous call invalidated current layout (ToDo: Don't allow null for layout)
            if (Layout == null) NewLayout();

            string canonizedWord = CanonizeWord(originalWord);

            // If it's the first canonizedWord of the layout, chose random canonizedWord and orientation to start,
            // place it at position (0, 0)
            if (Layout.WordPositionList.Count == 0)
            {
                WordPosition wp = new WordPosition(canonizedWord, originalWord, new PositionOrientation(0, 0, rnd.NextDouble() > 0.5));
                Layout.AddWordPositionNoCheck(wp);
                return wp;
            }

            BoundingRectangle r = Layout.GetBounds();

            // Find first all positions where the canonizedWord can be added to current layout;
            List<WordPositionSurface> possibleWordPositions = new List<WordPositionSurface>();
            foreach (WordPosition wordPosition in Layout.WordPositionList)
                foreach (WordPosition wp in TryPlace(canonizedWord, originalWord, wordPosition))    //, possibleWordPositions))
                {
                    BoundingRectangle newR = Layout.ExtendBounds(r, wp);
//#if TryPlaceOptimization
                    // Serious optimisation, no need to continue if we found a solution that does not extand layout
                    if (newR.Equals(r))
                    {
                        Layout.AddWordPositionNoCheck(wp);
                        return wp;
                    }
//#endif
                    int newSurface = ComputeAdjustedSurface(newR.Max.Column - newR.Min.Column + 1, newR.Max.Row - newR.Min.Row + 1);
                    possibleWordPositions.Add(new WordPositionSurface(wp, newSurface));
                }
            if (possibleWordPositions.Count == 0)
                return null;

            // Optimization
            if (possibleWordPositions.Count == 1)
            {
                WordPosition wp = possibleWordPositions.First().WordPosition;
                Layout.AddWordPositionNoCheck(wp);
                return wp;
            }

            // Take a possibility among those who minimally extend surface, and add it to current layout
            // Chose a random candidate among the 15% best
            possibleWordPositions.Sort(Comparer<WordPositionSurface>.Create((wps1, wps2) => wps1.Surface - wps2.Surface));
            int index = (int)(possibleWordPositions.Count * 0.15 * rnd.NextDouble());
            Layout.AddWordPositionNoCheck(possibleWordPositions[index].WordPosition);
            return possibleWordPositions[index].WordPosition;
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


        /// <summary>Find all the ways to add WordToPlace to placedWord, iterator returning an enumeration of matching WordPosition</summary>
        /// <param name="canonizedWordToPlace">Canonized form of Word to place (ex: NON·SEQUITUR)</param>
        /// <param name="originalWordToPlace">Original form of Word to place (ex: Non Sequitur)</param>
        /// <param name="placedWord">WordPosition to connect to</param>
        private IEnumerable<WordPosition> TryPlace(string canonizedWordToPlace, string originalWordToPlace, WordPosition placedWord)
        {
            // Build a dictionary of (letter, count) for each canonizedWord
            List<char> wordToPlaceLetters = BreakLetters(canonizedWordToPlace).Shuffle();
            List<char> placedWordLetters = BreakLetters(placedWord.Word);

            // Internal helper function, returns a dictionary of (letter, count) for canonizedWord w
            List<char> BreakLetters(string w)
            {
                var set = new HashSet<char>();
                foreach (char letter in w)
                    if (!set.Contains(letter))
                        set.Add(letter);
                return set.ToList();
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
                        for (; ; )
                        {
                            int q = word.IndexOf(letter, p);
                            if (q < 0) break;
                            list.Add(q);
                            p = q + 1;
                        }
                        return list;
                    }

                    // Look for all possible combinations of letter in both words if it's possible to place canonizedWordToPlace.
                    foreach (int positionInWordToPlace in FindPositions(canonizedWordToPlace))              // positionsInWordToPlace)
                        foreach (int positionInPlacedWord in FindPositions(placedWord.Word))                // positionsInPlacedWord)
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
                            if (Layout.CanPlaceWord(test, false) == PlaceWordStatus.Valid)
                                yield return test;
                        }
                }
            yield break;
        }




        /// <summary>Writes a text representation of current layout on stdout.</summary>
        public void Print() => Print(Console.Out);


        /// <summary>Writes a text representation of current layout in a file.</summary>
        /// <param name="filename">Name of file to write.</param>
        public void Print(string filename)
        {
            using (Stream s = new FileStream(filename, FileMode.Create))
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
