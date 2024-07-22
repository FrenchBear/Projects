// Grille.cs
// A grille represent a set of crossing words
//
// 2017-07-21   PV      Split from program.cs
// 2017-08-05   PV      Deep rewrite of AddWordsFromFile/AddWordsList/AddWord
// 2017-08-07	PV		Performance refactoring
// 2021-11-13   PV      Net6 C#10
// 2023-11-20   PV      Net8 C#12

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bonza.Generator;

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
    public void NewLayout() => Layout = new WordPositionLayout();

    /// <summary>
    /// Adds a list of words from a file to current layout.
    /// Will raise an exception if there is a problem with the file, or if some words are rejected, initial layout preserved in this case.
    /// </summary>
    public bool AddWordsFromFile(string fileName)
    {
        // Will throw an exception in case of problem with file
        var wordsList = new List<string>();

        // Since analyzer is *never* happy and still protest with following code on
        // if (s != null) s.Dispose(); with
        // Warning CA2202  Object 's' can be disposed more than once in method 'Grille.AddWordsFromFile(string)'.To avoid generating a System.ObjectDisposedException you should not call Dispose more than one time on an object.: Lines: 82 Generator C:\Development\GitHub\Projects\Bonza\Generator\Grille.cs    82  Active
        // The hell, with it, back to the one-using version now that .Net code knows it

        //Stream s = null;
        //StreamReader sr = null;
        //try
        //{
        //    s = new FileStream(fileName, FileMode.Open);
        //    try
        //    {
        //        sr = new StreamReader(s, Encoding.UTF8);

        //        while (!sr.EndOfStream)
        //        {
        //            string line = sr.ReadLine()?.Trim();
        //            if (!string.IsNullOrWhiteSpace(line))
        //                wordsList.Add(line);
        //        }

        //    }
        //    finally
        //    {
        //        if (sr != null) sr.Dispose();
        //    }
        //}
        //finally
        //{
        //    if (s != null) s.Dispose();
        //}
        using (var sr = new StreamReader(fileName, Encoding.UTF8))
        {
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(line))
                    wordsList.Add(line);
            }
        }

        return PlaceWordsList(wordsList, true) != null;
    }

    /// <summary>Shuffle words after reinitializing layout.</summary>
    /// <returns>Returns true if new placement is successful, or false if placement failed (current layout is preserved in this case)</returns>
    public bool PlaceWordsAgain()
    {
        // AddWordsList keeps a backup of Layout and restore it if placement failed...
        // but since we call it after reinitializing the layout, it won't work for us
        var backupLayout = new WordPositionLayout(Layout);
        var wordsList = Layout.WordPositionList.Select(wordPosition => wordPosition.OriginalWord).ToList();
        NewLayout();
        if (PlaceWordsList(wordsList, false) == null)
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
    public string CheckWordsList(IEnumerable<string> words)
    {
        ArgumentNullException.ThrowIfNull(words);

        // Plural case helper (French)
        static string S(int n) => n > 1 ? "s" : "";

        string result = string.Empty;
        string shortWords = words.Where(w => w.Length <= 2)
            .Aggregate("", (list, next) => string.IsNullOrEmpty(list) ? next : list + ", " + next);
        if (!string.IsNullOrEmpty(shortWords))
            result = $"Mot{S(shortWords.Length)} de longueur <= 2 non autorisé{S(shortWords.Length)}: " + shortWords;

        // Check for duplicates, start with current list of words (canonized)
        var wordsSet = new HashSet<string>(Layout.WordPositionList.Select(wordPosition => wordPosition.Word));
        List<string> duplicates = null;
        foreach (string w in words)
            if (w.Length > 2)
            {
                string cw = CanonizeWord(w);
                if (!wordsSet.Add(cw))
                {
                    duplicates ??= [];
                    duplicates.Add(w);
                }
            }
        if (duplicates?.Count > 0)
        {
            if (!string.IsNullOrEmpty(result))
                result += "\r\n";
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
    public IEnumerable<WordPosition> PlaceWordsList(IEnumerable<string> wordsToAddList, bool withLayoutBackup)
    {
        ArgumentNullException.ThrowIfNull(wordsToAddList);

        // Keep a copy of current layout to restore if placement fails at some point
        WordPositionLayout backupLayout = null;
        if (withLayoutBackup)
            backupLayout = new WordPositionLayout(Layout);

        string checkMessage = CheckWordsList(wordsToAddList);
        if (!string.IsNullOrEmpty(checkMessage))
            throw new BonzaException(checkMessage);

        List<string> shuffledList = new List<string>(wordsToAddList).Shuffle();
        var placedWordPositionList = new List<WordPosition>();

        while (shuffledList.Count > 0)
        {
            var placedWords = new List<string>();
            foreach (string word in shuffledList)
            {
                WordPosition wordPosition = PlaceWord(word);
                if (wordPosition != null)
                {
                    placedWords.Add(word);
                    placedWordPositionList.Add(wordPosition);
                    //Debug.WriteLine($"Placed {placedWordPositionList.Count}/{wordsToAddList.Count}: {wordPosition.Word}");
                }
            }
            // If at the end of this loop no canonizedWord has been placed, we have a problem...
            if (placedWords.Count == 0)
            {
                if (withLayoutBackup)
                    Layout = backupLayout;      // Restore initial layout
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
    private WordPosition PlaceWord(string originalWord)
    {
        var l = FindWordPossiblePlacements(originalWord, PlaceWordOptimization.High);
        if (l == null || l.Count == 0)
            return null;

        WordPosition wordPosition = l.TakeRandom();
        Layout.AddWordPositionNoCheck(wordPosition);
        return wordPosition;
    }

    /// <summary>
    /// Placement strategies for FindWordPossiblePlacements.
    /// </summary>
    public enum PlaceWordOptimization
    {
        Aggressive,         // Returns first wordPosition that does not extend surface, otherwise the position that minimally extend surface
        High,               // Returns all possible places that do not extend surface, otherwise the position that minimally extend surface
        Standard,           // Returns all possible solutions that do not extend surface by more than 15%, otherwise all solutions
        None                // Returns all possible solutions, mostly used as a base reference when visually comparing results of other levels
    }

    private List<WordPosition> FindWordPossiblePlacements(string originalWord, PlaceWordOptimization optimization)
    {
        string canonizedWord = CanonizeWord(originalWord);

        // If it's the first canonizedWord of the layout, chose random orientation and place it at position (0, 0)
        if (Layout.WordPositionList.Count == 0)
        {
            var wordPosition = new WordPosition(canonizedWord, originalWord, new PositionOrientation(0, 0, rnd.NextDouble() > 0.5));
            return [wordPosition];
        }

        // Get current layout since we'll prefer placements that minimize layout extension to keep words grouped
        BoundingRectangle r = Layout.Bounds;
        int surface = ComputeAdjustedSurface(r.Max.Column - r.Min.Column + 1, r.Max.Row - r.Min.Row + 1);

        // Find first all positions where the canonizedWord can be added to current layout;
        var possibleWordPositions = new List<WordPosition>();
        var possibleWordPositionsBelowThreshold = new List<WordPosition>();
        int minSurface = int.MaxValue;
        foreach (WordPosition wordPosition in Layout.WordPositionList)
            foreach (WordPosition placedWordPosition in TryPlace(canonizedWord, originalWord, wordPosition))
            {
                BoundingRectangle newR = WordPositionLayout.ExtendBounds(r, placedWordPosition);
                if (newR.Equals(r))
                {
                    if (optimization == PlaceWordOptimization.Aggressive)
                        return [placedWordPosition];
                    if (optimization == PlaceWordOptimization.High)
                        possibleWordPositionsBelowThreshold.Add(placedWordPosition);
                }
                int newSurface = ComputeAdjustedSurface(newR.Max.Column - newR.Min.Column + 1, newR.Max.Row - newR.Min.Row + 1);
                possibleWordPositions.Add(placedWordPosition);
                switch (optimization)
                {
                    case PlaceWordOptimization.Aggressive:
                    case PlaceWordOptimization.High:
                        if (possibleWordPositions.Count > 0 && minSurface > newSurface)
                        {
                            possibleWordPositions.RemoveAt(0);
                            minSurface = newSurface;
                        }
                        if (possibleWordPositions.Count == 0)
                            possibleWordPositions.Add(placedWordPosition);
                        break;

                    case PlaceWordOptimization.Standard:
                        if (newSurface < surface * 1.15)
                            possibleWordPositionsBelowThreshold.Add(placedWordPosition);
                        possibleWordPositions.Add(placedWordPosition);
                        break;

                    default:
                        possibleWordPositions.Add(placedWordPosition);
                        break;
                }
            }

        return possibleWordPositionsBelowThreshold.Count > 0 ? possibleWordPositionsBelowThreshold : possibleWordPositions;
    }

    ///// <summary>internal sealed class to sort possible WordPositions placement by surface to select the best.</summary>
    ///// <remarks>Not immutable because WordPosition is not immutable...  Though it shouldn't be modifiable???</remarks>
    //private class WordPositionSurface
    //{
    //    internal readonly WordPosition WordPosition;
    //    internal readonly int Surface;

    //    public WordPositionSurface(WordPosition wordPosition, int surface)
    //    {
    //        WordPosition = wordPosition;
    //        Surface = surface;
    //    }
    //}

    /// <summary>Adjusted surface calculation that penalize extensions that would make bounding rectangle width/height unbalanced</summary>
    /// <param name="width">Bounding rectangle width</param>
    /// <param name="height">Bounding rectangle height</param>
    /// <returns>Empirically adjusted: surface x (difference width-height)</returns>
    private static int ComputeAdjustedSurface(int width, int height) => width * height * (width - height) * (width - height);

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
        static List<char> BreakLetters(string w)
        {
            var set = new HashSet<char>();
            foreach (char letter in w)
                set.Add(letter);
            return [.. set];
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
                        if (q < 0)
                            break;
                        list.Add(q);
                        p = q + 1;
                    }
                    return list;
                }

                // Look for all possible combinations of letter in both words if it's possible to place canonizedWordToPlace.
                foreach (int positionInWordToPlace in FindPositions(canonizedWordToPlace))              // positionsInWordToPlace)
                    foreach (int positionInPlacedWord in FindPositions(placedWord.Word))                // positionsInPlacedWord)
                    {
                        var test = new WordPosition(canonizedWordToPlace, originalWordToPlace,
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

    /// <summary>Writes a text representation of current layout on standard output.</summary>
    public void Print() => Print(Console.Out);

    /// <summary>Writes a text representation of current layout in a file.</summary>
    /// <param name="fileName">Name of file to write.</param>
    public void Print(string fileName)
    {
        using TextWriter tw = new StreamWriter(fileName, false, Encoding.UTF8);
        Print(tw);
    }

    /// <summary>Internal low-level text output generation, writes a text representation of current layout.</summary>
    /// <param name="tw">TextWriter to write to.</param>
    private void Print(TextWriter tw)
    {
        // First compute actual bounds
        BoundingRectangle r = Layout.Bounds;

        // Then print
        for (int row = r.Min.Row; row <= r.Max.Row; row++)
        {
            for (int col = r.Min.Column; col <= r.Max.Column; col++)
            {
                bool found = false;

                foreach (WordPosition wordPosition in Layout.WordPositionList)
                {
                    if (wordPosition.IsVertical && wordPosition.StartColumn == col && row >= wordPosition.StartRow && row < wordPosition.StartRow + wordPosition.Word.Length)
                    {
                        tw.Write(wordPosition.Word[row - wordPosition.StartRow]);
                        found = true;
                        break;
                    }
                    if (!wordPosition.IsVertical && wordPosition.StartRow == row && col >= wordPosition.StartColumn && col < wordPosition.StartColumn + wordPosition.Word.Length)
                    {
                        tw.Write(wordPosition.Word[col - wordPosition.StartColumn]);
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
