// GraphemesEnumerator
// A simple way to break a string in Graphemes in C#
// Actually this may not support some advanced complex unicode constructs, but should work for common cases
//
// 2024-12-17   PV

namespace Graphemes;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class GraphemeEnumerator
{
    public static IEnumerable<string> EnumerateGraphemes(string text)
    {
        if (string.IsNullOrEmpty(text))
            yield break;
        TextElementEnumerator graphemeEnumerator = StringInfo.GetTextElementEnumerator(text);
        while (graphemeEnumerator.MoveNext())
            yield return (string)graphemeEnumerator.Current;
    }

    public static void ExampleUsage()
    {
        string text1 = "नमस्ते"; // Hindi for "hello"
        string text2 = "👩‍👩‍👧‍👦"; // Family emoji
        string text3 = "école"; // e with acute accent (composed)
        string text4 = "école"; // e with acute accent (precomposed)
        string text5 = "👨🏾‍⚕️"; // Man health worker with skin tone
        string text6 = "👨‍❤‍👩"; // {MAN}{ZERO WIDTH JOINER}{HEAVY BLACK HEART}{ZERO WIDTH JOINER}{WOMAN}
        string text7 = "👨🏾‍❤️‍💋‍👨🏻"; // {MAN}{EMOJI MODIFIER FITZPATRICK TYPE-5}{ZERO WIDTH JOINER}{HEAVY BLACK HEART}{VARIATION SELECTOR-16}{ZERO WIDTH JOINER}{KISS MARK}{ZERO WIDTH JOINER}{MAN}{EMOJI MODIFIER FITZPATRICK TYPE-1-2}

        PrintGraphemes(text1);
        PrintGraphemes(text2);
        PrintGraphemes(text3);
        PrintGraphemes(text4);
        PrintGraphemes(text5);
        PrintGraphemes(text6);
        PrintGraphemes(text7);

        static void PrintGraphemes(string text)
        {
            Console.WriteLine($"Text: \"{text}\"");
            Console.Write($"{EnumerateGraphemes(text).Count()} grapheme(s): ");
            foreach (string grapheme in EnumerateGraphemes(text))
                Console.Write($"\"{grapheme}\" ");
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        GraphemeEnumerator.ExampleUsage();

    }
}