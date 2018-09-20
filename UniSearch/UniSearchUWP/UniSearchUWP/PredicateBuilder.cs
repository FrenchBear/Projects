// FilterPredicateBuilder
// Helper class to build a specific Predicate<object> to filter a List depending on a list of searched words and search options
//
// 2016-12-13   PV      v2.1 Full rewrite in a separate, clean class
// 2017-01-01   PV      v2.5.1 Exclusion of search words starting with -; move filter parsing from VM to here
// 2018-09-08   PV      Adaptation to UniSearch with dedicated predicates for CharacterRecord and BlockRecord
// 2018-09-20   PV      Filter on Subheader using s:

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UniDataNS;

namespace UniSearchUWP
{
    class PredicateBuilder
    {
        private readonly List<string> words;
        private readonly bool isCS;
        private readonly bool isAS;
        private readonly bool isRE;
        private readonly bool isWW;
        private readonly int options;

        public PredicateBuilder(string filter, bool isCS, bool isAS, bool isRE, bool isWW)
        {
            // Split query in separate words
            words = new List<string>();
            if (isRE)
            {
                // In Regex mode, the whole filter is considered as a single word, including spaces,
                // and possible leading - (not part of Regex)
                try
                {
                    // reTest is not used, it's there to raise an exception and skip words.Add(filter)
                    // if filter is an invalid or incomplete Regex, so we ignore it
                    Regex reTest = new Regex(filter);
                    words.Add(filter);
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch (ArgumentException)
                {
                }
            }
            else
            {
                IEnumerable<string> wordsList = ParseQuery(filter);

                // Preprocess list of words, remove accents for case-insensitive searches and transform
                // words in regular expressions for whole word searches
                foreach (string oneWord in wordsList)
                {
                    string word = isAS ? oneWord : RemoveDiacritics(oneWord);

                    // Special processing for WholeWords mode, transform each word in a Regex
                    if (isWW)
                    {
                        if (word == "-")
                            continue;

                        // But a leading - is not part of the word but an indicator for search exclusion,
                        // and remains ahead of the Regex so it can later be processed correctly by GetFilter
                        if (word.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                            word = "-" + @"\b" + Regex.Escape(word.Substring(1)) + @"\b";
                        else
                            word = @"\b" + Regex.Escape(word) + @"\b";
                    }
                    words.Add(word);
                }
            }

            // Memorize options
            this.isCS = isCS;       // Case Sensitive
            this.isAS = isAS;       // Accent Sensitive
            this.isRE = isRE;       // Regular Expression
            this.isWW = isWW;       // Whole Word

            // Compact version of options stored as bits in an integer
            this.options = (isCS ? 1 : 0) + (isAS ? 2 : 0) + (isRE ? 4 : 0) + (isWW ? 8 : 0);
        }

        // Helper that breaks white-separated words in a List<string>, but words "between quotes" are considered a single
        // word even if they include spaces
        private static IEnumerable<string> ParseQuery(string s)
        {
            var wordsList = new List<string>();
            var word = new StringBuilder();
            bool inQuote = false;

            void appendWordToList()
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
                        appendWordToList();
                    }
                    else
                        word.Append(c);
                }
                else
                {
                    if (c == '"')
                    {
                        inQuote = true;
                        appendWordToList();
                    }
                    else if (c == ' ')
                    {
                        appendWordToList();
                    }
                    else
                        word.Append(c);
                }
            }
            appendWordToList();

            return wordsList;
        }

        // Helper for Accent Insensitive comparisons, remove accents from a string
        private static string RemoveDiacritics(string text)
        {
            // Optimization, 1st pass just to check if there is at least 1 accent
            string denorm = text.Normalize(NormalizationForm.FormD);
            bool accentFound = false;
            foreach (char ch in denorm)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                {
                    accentFound = true;
                    break;
                }
            if (!accentFound)
                return text;

            StringBuilder sb = new StringBuilder();
            foreach (char ch in denorm)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public bool GetCheckableNodeFilter(object searched)
        {
            CheckableNode cn = searched as CheckableNode;

            foreach (string aWord in words)
            {
                bool invertFlag;
                string word;

                if (aWord.StartsWith("-", StringComparison.Ordinal))
                {
                    word = aWord.Substring(1);
                    invertFlag = true;
                    if (word.Length == 0)
                        continue;
                }
                else
                {
                    word = aWord;
                    invertFlag = false;
                }

                if (isRE || isWW)
                {
                    try
                    {
                        if (isAS)
                        {
                            if (invertFlag ^ !Regex.IsMatch(cn.Name, word, isCS ? 0 : RegexOptions.IgnoreCase))
                                return false;
                        }
                        else
                        {
                            if (invertFlag ^ !Regex.IsMatch(RemoveDiacritics(cn.Name), word, isCS ? 0 : RegexOptions.IgnoreCase))
                                return false;
                        }
                    }
                    catch (Exception)
                    {
                        return true;
                    }
                }
                else
                {
                    if (isAS)
                    {
                        if (invertFlag ^ (cn.Name.IndexOf(word, isCS ? StringComparison.CurrentCulture : StringComparison.InvariantCultureIgnoreCase) < 0))
                            return false;
                    }
                    else
                    {
                        if (invertFlag ^ RemoveDiacritics(cn.Name).IndexOf(word, isCS ? StringComparison.CurrentCulture : StringComparison.InvariantCultureIgnoreCase) < 0)
                            return false;
                    }
                }
            }
            return true;
        }


        // To search for U+hhhh words (1 to 6 hex digits)
        static readonly Regex CodepointRegex = new Regex(@"^U\+[0-9A-F]{1,6}$", RegexOptions.IgnoreCase);

        // Specific version to search CharacterRecords
        public bool GetCharacterRecordFilter(object searched)
        {
            CharacterRecord cr = searched as CharacterRecord;

            foreach (string aWord in words)
            {
                bool invertFlag;
                string word;

                if (aWord.StartsWith("-", StringComparison.Ordinal))
                {
                    word = aWord.Substring(1);
                    invertFlag = true;
                    if (word.Length == 0)
                        continue;
                }
                else
                {
                    word = aWord;
                    invertFlag = false;
                }


                bool wordFilter = true;

                // If searched word is exactly 1 Unicode character, test directly character itself
                // Don't care if searched word is denormalized
                if (UniData.UnicodeLength(word) == 1)
                {
                    switch (this.options & 3)
                    {
                        case 0:    // CI AI
                            wordFilter = RemoveDiacritics(cr.Character).ToUpperInvariant() == RemoveDiacritics(word).ToUpperInvariant();
                            break;
                        case 1:     // CS AI
                            wordFilter = RemoveDiacritics(cr.Character) == RemoveDiacritics(word);
                            break;
                        case 2:     // CI AS
                            wordFilter = cr.Character.ToUpperInvariant() == word.ToUpperInvariant();
                            break;
                        case 4:     // CS AS
                            wordFilter = cr.Character == word;
                            break;
                    }
                }

                // If searched string is U+ followed by 1 to 6 hex digits, search for Codepoint value
                else if (CodepointRegex.IsMatch(word))
                {
                    int n = int.Parse(word.Substring(2), NumberStyles.HexNumber);
                    wordFilter = cr.Codepoint == n;
                }

                // If searched string starts with gc:, it's a category filter
                else if (word.StartsWith("GC:", StringComparison.OrdinalIgnoreCase))
                {
                    wordFilter = cr.CategoryRecord.CategoriesList.Any(s => string.Compare(s, word.Substring(3), StringComparison.OrdinalIgnoreCase) == 0);
                }

                // Age filter
                else if (word.StartsWith("Age:", StringComparison.OrdinalIgnoreCase) || word.StartsWith("A:", StringComparison.OrdinalIgnoreCase))
                {
                    double age = double.Parse(cr.Age, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);
                    word = word.Substring(word.IndexOf(':') + 1);
                    string op = "";
                    foreach (string o in new string[] { ">=", ">", "<=", "<", "=" })
                        if (word.StartsWith(o, StringComparison.Ordinal))
                        {
                            op = o;
                            word = word.Substring(o.Length);
                            break;
                        }

                    if (word.Length > 0 && double.TryParse(word, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out double v))
                        switch (op)
                        {
                            case ">": wordFilter = age > v; break;
                            case ">=": wordFilter = age >= v; break;
                            case "<": wordFilter = age < v; break;
                            case "<=": wordFilter = age <= v; break;
                            default: wordFilter = Math.Abs(age - v) < 0.001; break;
                        }
                }

                // Block filter
                else if (word.StartsWith("Block:", StringComparison.OrdinalIgnoreCase) || word.StartsWith("B:", StringComparison.OrdinalIgnoreCase))
                {
                    word = word.Substring(word.IndexOf(':') + 1);
                    wordFilter = cr.BlockRecord.BlockName.IndexOf(word, 0, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                // Block filter
                else if (word.StartsWith("Sub:", StringComparison.OrdinalIgnoreCase) || word.StartsWith("S:", StringComparison.OrdinalIgnoreCase))
                {
                    word = word.Substring(word.IndexOf(':') + 1);
                    wordFilter = cr.Subheader.IndexOf(word, 0, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                // Otherwise search a part of Name
                else
                {
                    bool isWWLocal = isWW;
                    // Full word search
                    if (word.StartsWith("W:", StringComparison.OrdinalIgnoreCase))
                    {
                        isWWLocal = true;
                        word = @"\b" + Regex.Escape(word.Substring(2)) + @"\b";
                    }

                    if (isRE || isWWLocal)
                    {
                        try
                        {
                            if (isAS)
                                wordFilter = Regex.IsMatch(cr.Name, word, isCS ? 0 : RegexOptions.IgnoreCase);
                            else
                                wordFilter = Regex.IsMatch(RemoveDiacritics(cr.Name), word, isCS ? 0 : RegexOptions.IgnoreCase);
                        }
                        catch (Exception)
                        {
                            wordFilter = true;
                        }
                    }
                    else
                    {
                        if (isAS)
                            wordFilter = cr.Name.IndexOf(word, isCS ? StringComparison.CurrentCulture : StringComparison.InvariantCultureIgnoreCase) >= 0;
                        else
                            wordFilter = RemoveDiacritics(cr.Name).IndexOf(word, isCS ? StringComparison.CurrentCulture : StringComparison.InvariantCultureIgnoreCase) >= 0;
                    }
                }

                if (!(wordFilter ^ invertFlag))
                    return false;

            } // Words loop


            return true;
        }

    }
}
