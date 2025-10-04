// EmojiAndZWJSequences.cs
// Static class to support Emoji and ZWJ sequences as provided by Unicode consortium
//
// 2023-08-17   PV

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UniView_WinUI3;

namespace UniDataNS;

public static class EmojiAndZWJSequences
{
    // Real internal dictionary used to store Unicode data
    private static readonly List<UnicodeSequence> EmojiSequenceList = new();

    /// <summary>List of all valid EmojiSequence records.</summary>
    public static ReadOnlyCollection<UnicodeSequence> EmojiSequenceRecords { get; } = new(EmojiSequenceList);

    static EmojiAndZWJSequences()
    {
        var totalStopwatch = Stopwatch.StartNew();
        Stopwatch emojiSequencesStopwatch = ReadEmojiSequences();
        Stopwatch zwjSequencesStopwatch = ReadZWJSequences();
        totalStopwatch.Stop();

        Debug.WriteLine("EmojiAndZWJSequences initialization times:");
        Debug.WriteLine($"EmojiSequences: {emojiSequencesStopwatch.Elapsed}");
        Debug.WriteLine($"ZWJSequences:   {zwjSequencesStopwatch.Elapsed}");
        Debug.WriteLine($"TOTAL:          {totalStopwatch.Elapsed}");
    }

    private static Stopwatch ReadEmojiSequences()
    {
        var emojiSequencesStopwatch = Stopwatch.StartNew();
        using (var sr = new StreamReader(UniData.GetResourceStream("emoji-sequences.txt"), Encoding.UTF8))
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine() ?? string.Empty;
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                string[] fields = line.Split(';');
                SequenceSubtype sequenceSubtype = SequenceSubtype.None;
                switch (fields[1].Trim())
                {
                    case "Basic_Emoji":
                        continue;

                    case "Emoji_Keycap_Sequence":
                        sequenceSubtype = SequenceSubtype.SequenceKeycap;
                        break;

                    case "RGI_Emoji_Flag_Sequence":
                    case "RGI_Emoji_Tag_Sequence":
                        sequenceSubtype = SequenceSubtype.SequenceFlag;
                        break;

                    case "RGI_Emoji_Modifier_Sequence":
                        sequenceSubtype = SequenceSubtype.SequenceModifier;
                        break;

                    default:
                        Debugger.Break();
                        break;
                }

                int[] sequence = fields[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x, NumberStyles.HexNumber)).ToArray();
                string name = fields[2].Split('#')[0].Trim().ToUpper().Replace("\\X{23}", "#");
                EmojiSequenceList.Add(new UnicodeSequence(name, sequence, SequenceType.EmojiSequence, sequenceSubtype));
            }

        emojiSequencesStopwatch.Stop();
        return emojiSequencesStopwatch;
    }

    private static Stopwatch ReadZWJSequences()
    {
        var emojiSequencesStopwatch = Stopwatch.StartNew();
        SequenceSubtype sequenceSubtype = SequenceSubtype.None;
        using (var sr = new StreamReader(UniData.GetResourceStream("emoji-zwj-sequences.txt"), Encoding.UTF8))
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine() ?? string.Empty;

                // In emoji-zwj-sequences.txt, fields[1] is always RGI_Emoji_ZWJ_Sequence. Subtype is coming from comments
                if (line.StartsWith("# RGI_Emoji_ZWJ_Sequence: "))
                {
                    switch (line[26..])
                    {
                        case "Family":
                            sequenceSubtype = SequenceSubtype.ZWJSequenceFamily;
                            break;

                        case "Role":
                            sequenceSubtype = SequenceSubtype.ZWJSequenceRoles;
                            break;

                        case "Gendered":
                            sequenceSubtype = SequenceSubtype.ZWJSequenceGendered;
                            break;

                        case "Hair":
                            sequenceSubtype = SequenceSubtype.ZWJSequenceHair;
                            break;

                        case "Other":
                            sequenceSubtype = SequenceSubtype.ZWJSequenceOther;
                            break;
                    }
                }
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                string[] fields = line.Split(';');
                int[] sequence = fields[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x, NumberStyles.HexNumber)).ToArray();
                string name = fields[2].Split('#')[0].Trim().ToUpper();
                EmojiSequenceList.Add(new UnicodeSequence(name, sequence, SequenceType.ZWJSequence, sequenceSubtype));
            }

        emojiSequencesStopwatch.Stop();
        return emojiSequencesStopwatch;
    }

    internal static UnicodeSequence? GetSequenceFromName(string name)
    {
        name = name.Replace('-', ' ').Replace(':', ' ').Replace(" ", "").ToUpper();
        // Should use a dictionary rather than a linear search!
        return EmojiSequenceList.FirstOrDefault(s => string.Compare(name, s.CanonizedName, StringComparison.CurrentCultureIgnoreCase) == 0);
    }
}