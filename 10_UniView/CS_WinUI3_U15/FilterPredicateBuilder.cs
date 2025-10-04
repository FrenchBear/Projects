// UniView CS UWP
// Helper class to build a specific Predicate<object> to filter a List depending on a list of searched words and search options
// This builder supports "regular expression" and "full words" searches, but these are currently not exposed in search interface
//
// 2023-08-16   PV      First version, built from FilterPredicateBuilder in QuickFileFilter

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UniView_WinUI3;

internal class FilterPredicateBuilder
{
    private readonly bool IsRe;         // Regular Expression
    private readonly bool IsWw;         // Whole Word
    private readonly SymbolFilter SymbolFilter;
    private readonly ScriptFilter ScriptFilter;
    private readonly EmojiSequenceFilter EmojiSequenceFilter;
    private readonly ZWJSequenceFilter ZWJSequenceFilter;

    private readonly List<string> Words;

    public FilterPredicateBuilder(string filter, bool isRe, bool isWw, SymbolFilter symbolFilter, ScriptFilter scriptFilter, EmojiSequenceFilter emojiSequenceFilter, ZWJSequenceFilter zwjSequenceFilter)
    {
        IsRe = isRe;
        IsWw = isWw;
        SymbolFilter = symbolFilter;
        ScriptFilter = scriptFilter;
        EmojiSequenceFilter = emojiSequenceFilter;
        ZWJSequenceFilter = zwjSequenceFilter;

        // Split query in separate words
        Words = new List<string>();

        IEnumerable<string> wordsList = ParseQuery(filter);

        // Pre-process list of words, remove accents for case-insensitive searches and transform
        // words in regular expressions for whole word searches
        foreach (string oneWord in wordsList)
        {
            var word = oneWord;

            // Special processing for WholeWords mode, transform each word in a Regex
            if (isWw)
            {
                if (word == "-")
                    continue;

                // But a leading - is not part of the word but an indicator for search exclusion,
                // and remains ahead of the Regex so it can later be processed correctly by GetFilter
                word = word.StartsWith('-')
                    ? "-" + @"\b" + Regex.Escape(word[1..]) + @"\b"
                    : @"\b" + Regex.Escape(word) + @"\b";
            }
            Words.Add(word);
        }
    }

    // Helper that breaks white-separated words in a List<string>, but words "between quotes" are considered a single
    // word even if they include spaces
    // When not between quotes, special quotes such as ’ are replaced by straight quote '
    private static IEnumerable<string> ParseQuery(string s)
    {
        var wordsList = new List<string>();
        var word = new StringBuilder();
        bool inQuote = false;

        void AppendWordToList()
        {
            if (word.Length > 0)
            {
                wordsList.Add(word.ToString());
                word = new StringBuilder();
            }
        }

        foreach (char c in s)
        {
            if (inQuote)
            {
                if (c == '"')
                {
                    inQuote = false;
                    AppendWordToList();
                }
                else
                    word.Append(c);
            }
            else
            {
                if (c == '"')
                {
                    inQuote = true;
                    AppendWordToList();
                }
                else if (c == ' ')
                {
                    AppendWordToList();
                }
                else
                {
                    word.Append(c == '’' ? '\'' : c);   // Replace a RIGHT SINGLE QUOTATION MARK by a 'Normal' straight APOSTROPHE
                }
            }
        }
        AppendWordToList();

        return wordsList;
    }

    public bool GetFilter(object searchedObject)
    {
        if (searchedObject is not UnicodeSequence searched)
            return false;

        // First filter on type and subtype
        switch (searched.SequenceType)
        {
            case SequenceType.CodepointScript:
                if (ScriptFilter == ScriptFilter.None)
                    return false;
                if (ScriptFilter == ScriptFilter.Latin && searched.SequenceSubtype != SequenceSubtype.CodepointLatinAndGreek)
                    return false;
                break;

            case SequenceType.CodepointSymbol:
                if (SymbolFilter == SymbolFilter.None)
                    return false;
                if (SymbolFilter == SymbolFilter.Emoji && searched.SequenceSubtype != SequenceSubtype.CodepointEmoji)
                    return false;
                break;

            case SequenceType.EmojiSequence:
                if (EmojiSequenceFilter is { Flag: false, Keycap: false, Modifier: false })
                    return false;
                if (searched.SequenceSubtype == SequenceSubtype.SequenceFlag && !EmojiSequenceFilter.Flag)
                    return false;
                if (searched.SequenceSubtype == SequenceSubtype.SequenceKeycap && !EmojiSequenceFilter.Keycap)
                    return false;
                if (searched.SequenceSubtype == SequenceSubtype.SequenceModifier && !EmojiSequenceFilter.Modifier)
                    return false;
                break;

            case SequenceType.ZWJSequence:
                if (ZWJSequenceFilter is { Family: false, Roles: false, Gendered: false, Hair: false, Other: false })
                    return false;
                if (searched.SequenceSubtype == SequenceSubtype.ZWJSequenceFamily && !ZWJSequenceFilter.Family)
                    return false;
                if (searched.SequenceSubtype == SequenceSubtype.ZWJSequenceRoles && !ZWJSequenceFilter.Roles)
                    return false;
                if (searched.SequenceSubtype == SequenceSubtype.ZWJSequenceGendered && !ZWJSequenceFilter.Gendered)
                    return false;
                if (searched.SequenceSubtype == SequenceSubtype.ZWJSequenceHair && !ZWJSequenceFilter.Hair)
                    return false;
                if (searched.SequenceSubtype == SequenceSubtype.ZWJSequenceOther && !ZWJSequenceFilter.Other)
                    return false;
                break;
        }

        foreach (string aWord in Words)
        {
            bool invertFlag;
            string word;

            if (aWord.StartsWith('-'))
            {
                word = aWord[1..];
                invertFlag = true;
                if (word.Length == 0)
                    continue;
            }
            else
            {
                word = aWord;
                invertFlag = false;
            }

            if (IsRe || IsWw)
            {
                try
                {
                    if (invertFlag ^ !Regex.IsMatch(searched.CanonizedName, word, RegexOptions.IgnoreCase))
                        return false;
                }
                catch (Exception)
                {
                    return true;
                }
            }
            else
            {
                word = word.Replace("-", "").Replace(":", "");  // - and : have been fitered out of CanonizedName
                if (invertFlag ^ (searched.CanonizedName.IndexOf(word, StringComparison.InvariantCultureIgnoreCase) < 0))
                    return false;
            }
        }
        return true;
    }
}
