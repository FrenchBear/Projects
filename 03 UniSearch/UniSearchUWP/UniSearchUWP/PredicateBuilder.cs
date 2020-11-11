// FilterPredicateBuilder
// Helper class to build a specific Predicate<object> to filter a List depending on a list of searched words and search options
//
// 2016-12-13   PV      v2.1 Full rewrite in a separate, clean class
// 2017-01-01   PV      v2.5.1 Exclusion of search words starting with -; move filter parsing from VM to here
// 2018-09-08   PV      Adaptation to UniSearch with dedicated predicates for CharacterRecord and BlockRecord
// 2018-09-20   PV      Filter on Subheader using s:
// 2018-09-20   PV      Filter on letters using l:
// 2018-09-26   PV      Use helper WordStartsWithPrefix for better code detecting special flags
// 2019-04-29   PV      ParseQuery accepts prefix:"words with spaces" since it's more natural than "prefix:words with spaces"
// 2020-11-11   PV      New rule for single letter search: only searches full word in the description, much more efficient than former rule (only search for letters equivalent to this one).
//                      New rule is always case-insensitive and accent-insensitive, a A â Ä all return the same matching set
//                      Note that for single letters, full word search is strict, for instance, F foes not match "PHASE-F" in char name, while W: prefix actually does.
//                      For T, the old rule found 17 matches (TtŢţŤťȚțṪṫṬṭṮṯṰṱẗ)
//                      The new rule finds 90 matches (TtŢţŤťŦŧƫƬƭƮȚțȶȾʇʈͭᑦᛏᛐᣕᰳᴛᵀᵗᵵᶵṪṫṬṭṮṯṰṱẗₜ⒯ⓉⓣⱦꞆꞇꞱꩅﬅ𐊗𐊭𐤯𑫞𖹈𖹨𛰃𛰲𛰳𛰶𛰷𝐓𝐭𝑇𝑡𝑻𝒕𝒯𝓉𝓣𝓽𝔗𝔱𝕋𝕥𝕿𝖙𝖳𝗍𝗧𝘁𝘛𝘵𝙏𝙩𝚃𝚝🄣🅃🅣🆃🇹)

#pragma warning disable CA1031 // Do not catch general exception types

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UniDataNS;

#nullable enable


namespace UniSearchUWPNS
{
    class PredicateBuilder
    {
        private readonly List<string> words;
        private readonly bool isCS;
        private readonly bool isAS;
        private readonly bool isRE;
        private readonly bool isWW;
        // private readonly int options;        // For old single-letter filtering rule

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
                    //string word = isAS ? oneWord : RemoveDiacritics(oneWord);
                    // Because of l: prefix, can't remove diacritics in this app
                    // But not really a problem since other searches use English words without accents
                    string word = oneWord;

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
            //this.options = (isCS ? 1 : 0) + (isAS ? 2 : 0) + (isRE ? 4 : 0) + (isWW ? 8 : 0);       // For old single-letter filtering rule
        }

        // Helper that breaks white-separated words in a List<string>, but words "between quotes" are considered a single
        // word even if they include spaces.
        // Syntax prefix:"words with spaces" is also valid
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
                        // New rule: a " can be in the middle of a no-space sequence such as «b:"Playing Cards"»
                        //appendWordToList();
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

        public bool GetBlockNodeFilter(object searched)
        {
            if (searched is not BlockNode cn)
                return true;

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
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception)
                    {
                        return true;
                    }
#pragma warning restore CA1031 // Do not catch general exception types
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
            if (searched is not CharacterRecord cr)
                return true;

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


                // Checks if word starts with the first letters of prefix followed by :
                // If there is a match, returns true and removes prefix from word
                bool WordStartsWithPrefix(string prefix)
                {
                    int p = word.IndexOf(':');
                    if (p <= 0 || p > prefix.Length) return false;      // p<=0 since starting with : is not a valid prefix
                    bool match= string.Compare(word.Substring(0, p), prefix.Substring(0, p), StringComparison.InvariantCultureIgnoreCase) == 0;
                    if (match)
                        word = word.Substring(p + 1);
                    return match;
                }


                bool wordFilter = true;

                // If searched word is exactly 1 Unicode character, test directly character itself
                // Don't care if searched word is denormalized
                // 2020-11-11: Change the rule, a single letter searches for a full word.  Otherwise searches for T for instance miss many letters that 
                // are not equivalent to T but are real T (math letters, upside-down letters, ...)
                if (UniData.UnicodeLength(word) == 1)
                {
                    // Old rule
                    /*
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
                    */
                    
                    // New rule
                    word = @" " + Regex.Escape(RemoveDiacritics(word).ToUpperInvariant()) + @" ";

                    try
                    {
                        wordFilter = Regex.IsMatch(" "+cr.Name+" ", word, isCS ? 0 : RegexOptions.IgnoreCase);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception)
                    {
                        wordFilter = true;
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                }

                // If searched string is U+ followed by 1 to 6 hex digits, search for Codepoint value
                // StartsWith U+ is for optimization, no need to start interpreting a regex for all chars
                else if (word.StartsWith("U+", StringComparison.OrdinalIgnoreCase) && CodepointRegex.IsMatch(word))
                {
                    int n = int.Parse(word.Substring(2), NumberStyles.HexNumber);
                    wordFilter = cr.Codepoint == n;
                }

                // If searched string starts with gc:, it's a category filter
                else if (WordStartsWithPrefix("GC"))
                {
                    wordFilter = cr.CategoryRecord.CategoriesList.Any(s => string.Compare(s, word, StringComparison.OrdinalIgnoreCase) == 0);
                }

                // Age filter
                else if (WordStartsWithPrefix("Age"))
                {
                    double crAge = double.Parse(cr.Age, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);
                    string op = "";
                    foreach (string o in new string[] { ">=", ">", "<=", "<", "=" })
                        if (word.StartsWith(o, StringComparison.Ordinal))
                        {
                            op = o;
                            word = word.Substring(o.Length);
                            break;
                        }

                    if (word.Length > 0 && double.TryParse(word, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out double v))
                        wordFilter = op switch
                        {
                            ">" => crAge > v,
                            ">=" => crAge >= v,
                            "<" => crAge < v,
                            "<=" => crAge <= v,
                            _ => Math.Abs(crAge - v) < 0.001,
                        };
                }

                // Block filter
                else if (WordStartsWithPrefix("Block"))
                {
                    wordFilter = cr.Block.BlockName.IndexOf(word, 0, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                // Subheader filter
                else if (WordStartsWithPrefix("Subheader"))
                {
                    if (word.Length > 0)
                        wordFilter = cr.Subheader != null && cr.Subheader.IndexOf(word, 0, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                // Letters filter: matches all letters of word
                else if (WordStartsWithPrefix("Letters"))
                {
                    wordFilter = word.EnumCharacterRecords().Any(r => r == cr);
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
#pragma warning disable CA1031 // Do not catch general exception types
                        catch (Exception)
                        {
                            wordFilter = true;
                        }
#pragma warning restore CA1031 // Do not catch general exception types
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
