// UniView CS WPF
// Simple prototype of an app showing Unicode text details
// Main window: Interactions and display
//
// 2018-08-29   PV
// 2019-05-07   PV      Added Copy UTF-8 command
// 2020-11-17   PV      C#8; nullable enable; decode U+xxx sequences
// 2020-11-26   PV      .Net 5.0, C#9.  FInally support for glyphs and reverse selection list -> text
// 2020-12-18   PV      Process correctly {U+1234}.  Refactoring (ViewMode.cs, CodepointDetail.cs).  Display counts. Examples
// 2020-12-20   PV      1.5.0 Merge changes of WPF .Net Core version (refactoring and clean rewrites)
// 2020-12-21   PV      1.5.1 List shows again transformed details, not source details
// 2020-12-14   PV      1.5.3 Correct handling of control characters
// 2020-12-14   PV      1.5.4 Characters ranges; Macros do not ignore unassigned codepoints (except for surrogates)
// 2020-12-29   PV      1.5.5 Macros are case insensitive, U+CAFE and u+cafe are both accepted
// 2020-12-30	PV		1.6   Scripts
// 2021-07-13	PV		1.6.1 Code cleanup
// 2023-01-23   PV      1.8   C#11
// 2023-03-24	PV		1.7.1 Synonyms for names; Text/Emoji font for result; ZWJ and VS15/16 examples
// 2023-08-16   PV      2.0   Support for character search, and Emoji+ZWJ sequences
// 2023-08-24   PV      2.0.1 Added Emoji Sequences example
// 2023-09-21   PV      2.1   Unicode 15.1

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UniDataNS;

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

namespace UniViewNS;

public partial class MainWindow: Window
{
    private readonly MainViewModel VM;
    private static DpiScale DpiInfo;
    private bool IgnoreListSelectionChanged;
    private bool IgnoreTextSelectionChanged;
    private readonly object SyncObject = new();

    public MainWindow()
    {
        InitializeComponent();
        VM = new MainViewModel();
        DataContext = VM;

        CaseNone.IsChecked = true;
        NormNone.IsChecked = true;
        SeqCn.IsChecked = true;

        // Emoji
        // 🐗  Boar, U+1F417, UTF-8: 0xF0 0x9F 0x90 0x97, UTF-16: 0xD83D 0xDC17, UTF-32: 0x0001F417
        // 🧔  Bearded Person, U+1F9D4
        // 🧔🏻  Bearded Person+Light Skin Tone, U+1F9D4 U+1F3FB
        // 🧝  Elf, U+1F9DD
        // 🧝‍♂️  Man Elf, U+1F9DD(🧝) U+200D(ZWJ) U+2642(♂) U+FE0F(VS-16)
        // 🧝‍♀️  Woman Elf =  U+1F9DD(🧝) U+200D(ZWJ) U+2640(♀) U+FE0F(VS-16)
        // 🧝🏽  Elf: Medium Skin Tone, U+1F9DD (🧝) U+1F3FD (🏽)
        // 🧝🏽‍♂️  Man Elf: Medium Skin Tone, U+1F9DD (🧝) U+1F3FD (🏽) U+200D(ZWJ) U+2642(♂) U+FE0F(VS-16)
        // 🧝🏽‍♀️  Woman Elf: Medium Skin Tone U+1F9DD (🧝) U+1F3FD (🏽) U+200D(ZWJ) U+2640(♀) U+FE0F(VS-16)

        VM.SamplesCollection.Add(new TextSample("Complete", "Aé♫山𝄞🐗\r\nœæĳøß≤≠Ⅷﬁﬆ\r\n🐱‍🏍 🐱‍👓 🐱‍🚀 🐱‍👤 🐱‍🐉 🐱‍💻\r\n🧝 🧝‍♂️ 🧝‍♀️ 🧝🏽 🧝🏽‍♂️ 🧝🏽‍♀️"));
        VM.SamplesCollection.Add(new TextSample("Emoji Sequences", "{POLAR BEAR}\r\n{KEYCAP 5}\r\n{KISS: MAN, MAN, MEDIUM-DARK SKIN TONE, LIGHT SKIN TONE}"));
        VM.SamplesCollection.Add(new TextSample("Simple glyphs", "Aé♫山𝄞🐗"));
        VM.SamplesCollection.Add(new TextSample("{Waving white flag}{ZWJ}{Rainbow}", "{Waving white flag}{ZWJ}{Rainbow}"));
        VM.SamplesCollection.Add(new TextSample("VS15/VS16 selector", "#*09©®‼⁉™\r\n#{VS15}*{VS15}0{VS15}9{VS15}©{VS15}®{VS15}‼{VS15}⁉{VS15}™{VS15}\r\n#{VS16}*{VS16}0{VS16}9{VS16}©{VS16}®{VS16}‼{VS16}⁉{VS16}™{VS16}"));
        VM.SamplesCollection.Add(new TextSample("Combining accent", "Où ça? Là!"));
        VM.SamplesCollection.Add(new TextSample("Outside BMP", "𝄞𝄡𝄢"));
        VM.SamplesCollection.Add(new TextSample("Multiple lines", "Line 1\r\nLine 2\rLine 3\nLine 4"));
        VM.SamplesCollection.Add(new TextSample("Macros", "U+0041{semicolon}{U+0042}"));
        VM.SamplesCollection.Add(new TextSample("Extreme combining", "aU+0300-036F"));
        VM.SamplesCollection.Add(new TextSample("Control characters C0+C1", "{U+0000-001F}{U+007F}{U+0080-009F}"));
        VM.SamplesCollection.Add(new TextSample("Line breakers", "{U+000A}{U+000D}{U+2028}{U+2029}"));
        VM.SamplesCollection.Add(new TextSample("Unassigned codepoints", "U+0380U+0381U+0382U+0383"));
        VM.SamplesCollection.Add(new TextSample("Not a character", "{U+FDD0-U+FDEF}{U+FFFE}{U+FFFF}"));
        VM.SamplesCollection.Add(new TextSample("Invalid surrogates", "{U+D834}{U+DD1E}"));
        VM.SamplesCollection.Add(new TextSample("Beyond U+10FFFF", "{U+110000}"));
        VM.SamplesCollection.Add(new TextSample("BMP", "U+0000..FFFF"));
        VM.SamplesCollection.Add(new TextSample("Ranges members", "U+0000U+001FU+3400U+4E00U+AC00U+E000U+17000U+18D00U+20000U+2A700U+2B740U+2B820U+2CEB0U+30000U+F0000U+100000"));
        VM.SamplesCollection.Add(new TextSample("Arrows", "←↑→↓\r￩￪￫￬\r🠀🠁🠂🠃\r🠄🠅🠆🠇\r🠈🠉🠊🠋\r🠐🠑🠒🠓\r🠔🠕🠖🠗\r🠘🠙🠚🠛\r🠜🠝🠞🠟\r🠠🠡🠢🠣\r🠤🠥🠦🠧\r🠨🠩🠪🠫\r🠬🠭🠮🠯\r🠰🠱🠲🠳\r🠴🠵🠶🠷\r🠸🠹🠺🠻\r🠼🠽🠾🠿\r🡀🡁🡂🡃\r🡄🡅🡆🡇\r🡐🡑🡒🡓\r🡠🡡🡢🡣\r🡰🡱🡲🡳\r🢀🢁🢂🢃\r🢐🢑🢒🢓\r🢔🢕🢖🢗\r🢘🢙🢚🢛\r◀▲▶▼\r◁△▷▽\r◂▴▸▾\r⏴⏵⏶⏷"));
        VM.SamplesCollection.Add(new TextSample("Digits", "U+0030-U+0039\rU+0660-U+0669\rU+06F0-U+06F9\rU+07C0-U+07C9\rU+0966-U+096F\rU+09E6-U+09EF\rU+0A66-U+0A6F\rU+0AE6-U+0AEF\rU+0B66-U+0B6F\rU+0BE6-U+0BEF\rU+0C66-U+0C6F\rU+0CE6-U+0CEF\rU+0D66-U+0D6F\rU+0DE6-U+0DEF\rU+0E50-U+0E59\rU+0ED0-U+0ED9\rU+0F20-U+0F29\rU+1040-U+1049\rU+1090-U+1099\rU+17E0-U+17E9\rU+1810-U+1819\rU+1946-U+194F\rU+19D0-U+19D9\rU+1A80-U+1A89\rU+1A90-U+1A99\rU+1B50-U+1B59\rU+1BB0-U+1BB9\rU+1C40-U+1C49\rU+1C50-U+1C59\rU+A620-U+A629\rU+A8D0-U+A8D9\rU+A8E0-U+A8E9\rU+A900-U+A909\rU+A9D0-U+A9D9\rU+A9F0-U+A9F9\rU+AA50-U+AA59\rU+ABF0-U+ABF9\rU+FF10-U+FF19\rU+104A0-U+104A9\rU+10D30-U+10D39\rU+11066-U+1106F\rU+110F0-U+110F9\rU+11136-U+1113F\rU+111D0-U+111D9\rU+112F0-U+112F9\rU+11450-U+11459\rU+114D0-U+114D9\rU+11650-U+11659\rU+116C0-U+116C9\rU+11730-U+11739\rU+118E0-U+118E9\rU+11950-U+11959\rU+11C50-U+11C59\rU+11D50-U+11D59\rU+11DA0-U+11DA9\rU+16A60-U+16A69\rU+16B50-U+16B59\rU+16E80-U+16E89\rU+1D7CE-U+1D7D7\rU+1D7D8-U+1D7E1\rU+1D7E2-U+1D7EB\rU+1D7EC-U+1D7F5\rU+1D7F6-U+1D7FF\rU+1E140-U+1E149\rU+1E2F0-U+1E2F9\rU+1E950-U+1E959\rU+1F101-U+1F10A\rU+1FBF0-U+1FBF9\r\rU+0F2A-U+0F32\rU+1369-U+1371\rU+2460-U+2468\rU+2488-U+2490\rU+24F5-U+24FD\rU+2776-U+277E\rU+278A-U+2792\rU+102E1-U+102E9\rU+10E60-U+10E68\rU+111E1-U+111E9\rU+1D360-U+1D368\rU+1E8C7-U+1E8CF\r"));
        VM.SamplesCollection.Add(new TextSample("Single-_Script confusable", "ǉeto ljeto"));     // Croatian word for “summer”
        VM.SamplesCollection.Add(new TextSample("Mixed-_Script confusable", "paypal pаypаl"));   // Cyrillic a in 2nd form
        VM.SamplesCollection.Add(new TextSample("Whole-_Script confusable", "scope ѕсоре"));     // Full Cyrillic for 2nd form

        Loaded += (sender, e) =>
        {
            // screen-specific dpi awareness; don't forget app.manifest file
            DpiInfo = VisualTreeHelper.GetDpi(this);
            InputText.Focus();
            InputText.SelectionStart = 9999;
        };

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, (sender, e) => // Execute
        {
            e.Handled = true;
            DoAbout();
        }));

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, (sender, e) => // Execute
        {
            e.Handled = true;
            DoCopy();
        }));
    }

    private static void DoAbout()
    {
        var myAssembly = Assembly.GetExecutingAssembly();
        var aTitleAttr = (AssemblyTitleAttribute?)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyTitleAttribute));
        string sAssemblyVersion = myAssembly.GetName().Version?.Major + "." + myAssembly.GetName().Version?.Minor + "." + myAssembly.GetName().Version?.Build;
        var aDescAttr = (AssemblyDescriptionAttribute?)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyDescriptionAttribute));
        var aCopyrightAttr = (AssemblyCopyrightAttribute?)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyCopyrightAttribute));
        var aProductAttr = (AssemblyProductAttribute?)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyProductAttribute));

        string s = aTitleAttr?.Title + " version " + sAssemblyVersion + "\r\n" + aDescAttr?.Description + "\r\n\n" + aProductAttr?.Product + "\r\n" + aCopyrightAttr?.Copyright;
        s += "\n\nUnicode Data: " + UniData.GetUnicodeVersion();
        MessageBox.Show(s, "About " + aTitleAttr?.Title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // If there is a selection in the list, just copy the selection, otherwise copy full list
    private void DoCopy()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Character\tCodepointIndex\tCharIndex\tGlyphIndex\tCodepoint\tName\tScript\tCategory\tUTF16\tUTF8");
        foreach (CodepointDetail cd in CodepointsList.SelectedItems.Count == 0 ? CodepointsList.Items : CodepointsList.SelectedItems)
            sb.AppendLine($"{cd.ToDisplayStringFull()}\t{cd.CodepointIndex}\t{cd.CharIndexStr}\t{cd.GlyphIndexStr}\t{cd.CodepointUString}\t{cd.Name}\t{cd.Script}\t{cd.Category}\t{cd.UTF16}\t{cd.UTF8}");
        ClipboardSetTextData(sb.ToString());
    }

    internal static void ClipboardSetTextData(string s)
    {
        // Sometimes clipboard access raises an error
        try
        {
            Clipboard.Clear();
            Clipboard.SetText(s);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error accessing clipboard: " + ex, "UniSearch", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void SourceUpdated_Handler(object sender, System.Windows.Data.DataTransferEventArgs e) => Transform();

    private void Option_Click(object sender, RoutedEventArgs e) => Transform();

    // To detect and replace special elements between braces in input text
    private static readonly Regex UPlusXBracesRegex = new(@"{U\+(1?[0-9A-F]{4,5})}", RegexOptions.IgnoreCase);    // {U+1234}
    private static readonly Regex UPlusXRegex = new(@"U\+(1?[0-9A-F]{4,5})", RegexOptions.IgnoreCase);            // U+1234       Simplified form when not followed by valid hex character
    private static readonly Regex RangeUPlusXBracesRegex = new(@"{U\+(1?[0-9A-F]{4,5})(\.\.|-)(?:U\+)?(1?[0-9A-F]{4,5})}", RegexOptions.IgnoreCase);    // {U+1234..U+2345} or {U+1234..2345}, use - instead of .. to avoid inserting a CR at the end of each page of 32 characters
    private static readonly Regex RangeUPlusXRegex = new(@"U\+(1?[0-9A-F]{4,5})(\.\.|-)(?:U\+)?(1?[0-9A-F]{4,5})", RegexOptions.IgnoreCase);            // U+1234..U+2345
    private static readonly Regex NameRegex = new(@"{([^}]+)}");                         // {semicolon}

    private void Transform()
    {
        string s = InputText.Text;

        if (TextSamplesCombo.SelectedIndex >= 0 && s != (TextSamplesCombo.SelectedItem as TextSample)!.Text)
            TextSamplesCombo.SelectedIndex = -1;

        static string ReplaceRange(Match ma)
        {
            if (int.TryParse(ma.Groups[1].ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int cpFrom))
                if (int.TryParse(ma.Groups[3].ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int cpTo))
                {
                    if (cpFrom > UniData.MaxCodepoint || cpTo > UniData.MaxCodepoint)
                        return "[Codepoint limit is 10FFFF]";
                    if (cpTo - cpFrom >= 0x10000)
                        return "[Range limited to 64K codepoints]";
                    if (cpTo < cpFrom)
                        return "[Range from>to]";
                    var sb = new StringBuilder();
                    bool insertCr = ma.Groups[2].ToString()[0] == '.';
                    for (int cp = cpFrom; cp <= cpTo; cp++)
                        if (!UniData.IsSurrogate(cp))   // For ranges, we silently ignore surrogates to enable large ranges including U+D800..U+DFFF
                        {
                            sb.Append(UniData.AsString(cp));
                            if (insertCr && cp % 32 == 31)
                                sb.Append('\r');
                        }
                    return sb.ToString();
                }
            return ma.Groups[0].ToString();          // If not recognized, keep original text
        }

        // First replace ranges by unicode characters
        s = RangeUPlusXBracesRegex.Replace(s, ReplaceRange);         // s = RangeUPlusXBracesRegex.Replace(s, ma => ReplaceRange(ma));
        s = RangeUPlusXRegex.Replace(s, ReplaceRange);

        static string ReplaceCodepoint(Match ma)
        {
            if (int.TryParse(ma.Groups[1].ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int cp))
            {
                if (cp > UniData.MaxCodepoint)
                    return "[Codepoint limit is 10FFFF]";

                if (UniData.IsSurrogate(cp))
                    return "[Invalid codepoint in surrogates range D800..DFFFF]";

                return UniData.AsString(cp);
            }
            return ma.Groups[0].ToString();          // If not recognized
        }

        // Then replace U+xxxx and {U+xxxx} sequences by unicode character
        s = UPlusXBracesRegex.Replace(s, ReplaceCodepoint);
        s = UPlusXRegex.Replace(s, ReplaceCodepoint);

        // And finally replace {semicolon} by ;
        s = NameRegex.Replace(s, ma =>
        {
            string name = ma.Groups[1].ToString();

            // First seach for a Unicode name
            int cp = UniData.GetCpFromName(name);
            if (cp >= 0)
                return UniData.AsString(cp);

            // If not found, search for a sequence name such as POLAR BEAR
            UnicodeSequence? seq = EmojiAndZWJSequences.GetSequenceFromName(name);
            if (seq != null)
                return seq.SequenceAsString;

            return ma.Groups[0].ToString();          // If not recognized
        });

        // Process normalization and case transformations
        if (SeqCn.IsChecked!.Value)
        {
            if (CaseLower.IsChecked!.Value)
                s = s.ToLower();
            else if (CaseUpper.IsChecked!.Value)
                s = s.ToUpper();
        }

        if (NormNfc.IsChecked!.Value)
            s = s.Normalize(NormalizationForm.FormC);
        else if (NormNfd.IsChecked!.Value)
            s = s.Normalize(NormalizationForm.FormD);
        else if (NormNfkc.IsChecked!.Value)
            s = s.Normalize(NormalizationForm.FormKC);
        else if (NormNfkd.IsChecked!.Value)
            s = s.Normalize(NormalizationForm.FormKD);

        if (SeqNc.IsChecked!.Value)
        {
            if (CaseLower.IsChecked!.Value)
                s = s.ToLower();
            else if (CaseUpper.IsChecked!.Value)
                s = s.ToUpper();
        }

        // Assemble source characters into Codepoints, handling surrogates and regex transformations
        var codepointsList = GetCodepointsDetails(s);

        // Display List of Codepoints
        var sbTransformed = new StringBuilder();
        var sbDisplay = new StringBuilder();
        VM.CodepointsCollection.Clear();
        PreviousCodepointIndexStart = -1;
        PreviousCodepointIndexEnd = -1;
        foreach (var cd in codepointsList)
        {
            VM.CodepointsCollection.Add(cd);
            sbTransformed.Append(cd);
            sbDisplay.Append(cd.ToDisplayString());
        }

        // transformed is the actual text analyzed after replacing macros
        string transformed = sbTransformed.ToString();

        // displayed string is identical to transformed string with most control characters 0..31 and 127 replaced by a visual placeholder
        string displayed = sbDisplay.ToString();
        ResultText.Text = displayed;

        // Update GlyphIndex for each CodepointDetail in the list, with the Glyph # a Codepoint belongs to
        var (citogi, _) = GetCharIndexToGlyphIndex(displayed);
        int maxGlyphIndex = -1;
        foreach (var cd in VM.CodepointsCollection)
        {
            cd.GlyphIndex = citogi[cd.CodepointIndexStart];
            if (cd.GlyphIndex > maxGlyphIndex)
                maxGlyphIndex = cd.GlyphIndex;
        }

        ResizeListColumns();

        // Update counts
        VM.CharactersCount = transformed.Length;
        VM.CodepointsCount = codepointsList.Count;
        VM.GlyphsCount = maxGlyphIndex + 1;
    }

    // Switched to static column sizes except for name, since virtualization of list makes automatic resize useless for large lists
    private void ResizeListColumns()
    {
        if (CodepointsList.View is GridView gv)
        {
            gv.Columns[0].Width = 45;       // Codepoint index
            gv.Columns[1].Width = 55;       // Character index
            gv.Columns[2].Width = 45;       // Clyph index
            gv.Columns[3].Width = 66;       // Codepoint
            gv.Columns[5].Width = 80;       // Script
            gv.Columns[6].Width = 29;       // Category
            gv.Columns[7].Width = 75;       // UTF-16
            gv.Columns[8].Width = 79;       // UTF-8

            // Use remaining width for name
            double d = 0;
            for (int i = 0; i < gv.Columns.Count; i++)
                if (i != 4)
                    d += gv.Columns[i].ActualWidth;
            gv.Columns[4].Width = Math.Max(CodepointsList.ActualWidth - 30 - d, 100);       // With a minimum of 100
        }
    }

    // Assemble source characters into Codepoints, handling surrogates
    private static IList<CodepointDetail> GetCodepointsDetails(string s)
    {
        var l = new List<CodepointDetail>();
        int codepointCount = 0;

        int codepointIndex = 0;
        for (int i = 0; i < s.Length; i++)
        {
            // int charIndexStart, charIndexEnd;               // Before surrogates processing
            // int codepointIndexStart, codepointIndexEnd;     // After surrogates processing

            // Ordinary char
            int cp = s[i];
            int charIndexStart = i;
            if (cp is >= 0xD800 and <= 0xDBFF)           // Surrogate?
            {
                int c2 = s[++i];
                cp = 0x10000 + ((cp & 0x3ff) << 10) + (0x3ff & c2);
            }
            int charIndexEnd = i;

            // For codepoint index, we just care whether cp is represented using 1 or 2 chars
            int codepointIndexStart = codepointIndex++;
            int codepointIndexEnd = (cp > 0xFFFF) ? codepointIndex++ : codepointIndexStart;

            // Finally add new cd to the list
            l.Add(new CodepointDetail(cp, codepointCount++, charIndexStart, charIndexEnd, codepointIndexStart, codepointIndexEnd));
        }
        return l;
    }

    // DPI Awareness
    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        lock (SyncObject)
        {
            DpiInfo = newDpi;
            //Debug.WriteLine($"Updated Globals.DpiInfo: DpiScaleX={DpiInfo.DpiScaleX}, DpiScaleY={DpiInfo.DpiScaleY}, PixelsPerDip={DpiInfo.PixelsPerDip}, PixelsPerInchX={DpiInfo.PixelsPerInchX}, PixelsPerInchY={DpiInfo.PixelsPerInchY}");
        }
        // Probably also a good place to re-draw things that need to scale
        InvalidateVisual();
    }

    // High level function, accepts text with \r and \n
    // \r and \n always map to glyph -1 (non-existent)
    // Text without \r or \n is processed by GetCharIndexToGlyphIndexOneLine
    private (List<int>, int) GetCharIndexToGlyphIndex(string s)
    {
        var charIndexToGlyphIndex = new List<int>();
        int gc = 0;

        StringBuilder sb = new();
        foreach (var t in s)
        {
            if (t == '\r' || t == '\n')
            {
                if (sb.Length > 0)
                {
                    var (l, c) = GetCharIndexToGlyphIndexOneLine(sb.ToString(), gc);
                    charIndexToGlyphIndex.AddRange(l);
                    gc += c;
                    sb.Clear();
                }
                charIndexToGlyphIndex.Add(-1);
            }
            else
                sb.Append(t);
        }
        if (sb.Length > 0)
        {
            var (l, c) = GetCharIndexToGlyphIndexOneLine(sb.ToString(), gc);
            charIndexToGlyphIndex.AddRange(l);
            gc += c;
        }
        return (charIndexToGlyphIndex, gc);
    }

    private (List<int>, int) GetCharIndexToGlyphIndexOneLine(string s, int glyphOffset)
    {
        DrawingGroup drawing = new();
        DrawingContext drawingContext = drawing.Open();
        FormattedText formattedText = new(s, new CultureInfo("fr-FR"), FlowDirection.LeftToRight, new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 14.0, Brushes.Black, DpiInfo.PixelsPerDip);
        drawingContext.DrawText(formattedText, new Point(0, 0));
        drawingContext.Close();
        List<GlyphRun> glyphRuns = new();
        ObtainGlyphRuns(drawing, glyphRuns);
        // At this point glyphRuns contains all glyph runs associated with the string of text passed to FormattedText object.

        var charIndexToGlyphIndex = new List<int>();
        int gc = 0;                 // glyph count
        foreach (var gr in glyphRuns)
        {
            if (gr.ClusterMap == null)
            {
                for (int i = 0; i < gr.GlyphIndices.Count; i++)
                    charIndexToGlyphIndex.Add(gc + glyphOffset + i);
            }
            else
                foreach (var item in gr.ClusterMap)
                    charIndexToGlyphIndex.Add(gc + glyphOffset + item);
            gc += gr.GlyphIndices.Count;
        }

        return (charIndexToGlyphIndex, gc);
    }

    private void ObtainGlyphRuns(Drawing drawing, List<GlyphRun> glyphRuns)
    {
        if (drawing is DrawingGroup group)
        {
            // Recursively go down the DrawingGroup
            foreach (Drawing child in group.Children)
                ObtainGlyphRuns(child, glyphRuns);
        }
        else
        {
            if (drawing is GlyphRunDrawing glyphRunDrawing)
            {
                // Add the glyph run to the collection
                GlyphRun glyphRun = glyphRunDrawing.GlyphRun;
                if (glyphRun != null)
                    glyphRuns.Add(glyphRun);
            }
        }
    }

    private int PreviousCodepointIndexStart;
    private int PreviousCodepointIndexEnd;

    private void CodepointsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IgnoreListSelectionChanged)
            return;
        if (sender is not ListView lv)
            return;

        // No selection, nothing to do!
        if (lv.SelectedItems.Count == 0)
            return;

        IgnoreTextSelectionChanged = true;

        // Sort selected indexes
        var li = new List<int>();
        foreach (var item in lv.SelectedItems)
            li.Add(lv.Items.IndexOf(item));
        li.Sort();

        // Build 1st contiguous group codepointIndexStart..codepointIndexEnd
        int codepointIndexStart = -1, codepointIndexEnd = -1;
        int lastix = -1;
        foreach (int ix in li)
        {
            if (codepointIndexStart < 0)
            {
                codepointIndexStart = ix;
                codepointIndexEnd = ix;
            }
            else
            {
                if (ix == lastix + 1)
                    codepointIndexEnd = ix;
                else
                    break;
            }
            lastix = ix;
        }

        // Adjust codepointIndexStart/codepointIndexEnd to be the first/last belonging to a glyph, since we can only highlight glyphs
        IgnoreListSelectionChanged = true;
        int glyphIndexStart = VM.CodepointsCollection[codepointIndexStart].GlyphIndex;
        while (codepointIndexStart > 0 && glyphIndexStart == VM.CodepointsCollection[codepointIndexStart - 1].GlyphIndex)
            CodepointsList.SelectedItems.Add(VM.CodepointsCollection[--codepointIndexStart]);
        int glyphIndexEnd = VM.CodepointsCollection[codepointIndexEnd].GlyphIndex;
        while (codepointIndexEnd < VM.CodepointsCollection.Count - 1 && glyphIndexEnd == VM.CodepointsCollection[codepointIndexEnd + 1].GlyphIndex)
            CodepointsList.SelectedItems.Add(VM.CodepointsCollection[++codepointIndexEnd]);
        IgnoreListSelectionChanged = false;

        // Without checking if selection has changed, there is a weird bug raising endless List_SelectionChanged events because of focus trick...
        // If the selection has actually not changed from previous event, we just do nothing
        if (PreviousCodepointIndexStart != codepointIndexStart || PreviousCodepointIndexEnd != codepointIndexEnd)
        {
            PreviousCodepointIndexStart = codepointIndexStart;
            PreviousCodepointIndexEnd = codepointIndexEnd;

            // Convert back from codepointIndex to charIndex
            int charIndexStart = VM.CodepointsCollection[codepointIndexStart].CharIndexStart;
            int charIndexEnd = VM.CodepointsCollection[codepointIndexEnd].CharIndexEnd;

            // Finally, highlight characters in ResultText
            if (charIndexStart >= 0 && charIndexEnd >= 0)
            {
                ResultText.SelectionStart = charIndexStart;
                ResultText.SelectionLength = charIndexEnd - charIndexStart + 1;
            }
            else
            {
                ResultText.SelectionStart = 0;
                ResultText.SelectionLength = 0;
            }

            // To make sure selection is refreshed, ignore focus errors
            try
            {
                ResultText.Focus();
                ResultText.ScrollToLine(ResultText.GetLineIndexFromCharacterIndex(ResultText.SelectionStart + ResultText.SelectionLength));
                lv.Focus();
            }
            catch
            {
            }

        }

        IgnoreTextSelectionChanged = false;
    }

    private void ResultText_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (IgnoreTextSelectionChanged)
            return;
        if (sender is not TextBox tb)
            return;

        IgnoreListSelectionChanged = true;

        CodepointsList.SelectedItems.Clear();
        if (tb.SelectionLength > 0)
        {
            var selectedList = new List<CodepointDetail>();
            CodepointDetail lastCd = VM.CodepointsCollection[0];        // To avoid unassigned variable warning
            foreach (var cd in VM.CodepointsCollection)
                if (cd.CharIndexStart >= tb.SelectionStart && cd.CharIndexEnd < tb.SelectionStart + tb.SelectionLength)
                {
                    selectedList.Add(cd);
                    lastCd = cd;
                }

            // Use reflection to select items calling protected method SetSelectedItems to select multiple items at once
            // For 10000 items, adding 1 by 1 using CodepointsList.SelectedItems.Add(cd): 4.2s, calling SetSelectedItems on a list: 0.56s
            MethodInfo? setSelectedItemsMethod = CodepointsList.GetType().GetMethod("SetSelectedItems", BindingFlags.NonPublic | BindingFlags.Instance);
            setSelectedItemsMethod?.Invoke(CodepointsList, new object[] { selectedList });

            CodepointsList.ScrollIntoView(lastCd);                      // Make last selected item visible
        }

        IgnoreListSelectionChanged = false;
    }

    private void SamplesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox { SelectedValue: TextSample ts })
        {
            InputText.Text = ts.Text;
            InputText.SelectionStart = InputText.Text.Length;
        }
    }

    private void TextButton_Click(object sender, RoutedEventArgs e)
        => ResultText.FontFamily = new("Segoe UI Variable");

    private void EmojiButton_Click(object sender, RoutedEventArgs e)
        => ResultText.FontFamily = new("Segoe UI Emoji");

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        var sw = new SearchWindow();
        string? res = sw.GetChar();
        if (res != null)
            InputText.SelectedText = res;
    }
}

internal static class ExtensionMethods
{
    // Convenient helper doing a Regex Match and returning success
    internal static bool IsMatchMatch(this Regex re, string s, int startat, out Match ma)
    {
        ma = re.Match(s, startat);
        return ma.Success;
    }
}
