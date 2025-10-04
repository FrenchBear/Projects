// UniView CS UWP
// Simple prototype of an app showing Unicode text details
// Main window: Interactions and display
//
// 2019-09-03   PV
// 2019-05-07   PV      Added Copy UTF-8 command
// 2020-11-17   PV      C#8; nullable enable; decode U+xxx sequences
// 2020-11-26   PV      Allow {U+xxxx}; Start to allow selection in list, and show selected items in input text -- Reaaaaally complex
// 2020-11-27   PV      1.3 Selection in list highlights characters in text
// 2020-12-20   PV      1.5.0 Merge changes of WPF .Net Core version (refactoring and clean rewrites)
// 2020-12-21   PV      1.5.1 List shows again transformed details, not source details
// 2020-12-14   PV      1.5.3: Correct handling of control characters
// 2020-12-14   PV      1.5.4: Characters ranges; Macros do not ignore unassigned codepoints (except for surrogates)
// 2020-12-29   PV      1.5.5: Macros are case insensitive, U+CAFE and u+cafe are both accepted
// 2020-12-30	PV		1.6: Scripts

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas;
using System.Threading.Tasks;

#nullable enable

namespace UniViewNS
{
    public sealed partial class MainPage : Page
    {
        private readonly ViewModel vm;
        private bool IgnoreListSelectionChanged;
        private bool IgnoreTextSelectionChanged;
        private string transformed = string.Empty;

        public MainPage()
        {
            InitializeComponent();
            vm = new ViewModel();
            DataContext = vm;

            Case_None.IsChecked = true;
            Norm_None.IsChecked = true;
            Seq_CN.IsChecked = true;

            Loaded += (sender, e) => { InputText.Focus(FocusState.Programmatic); InputText.SelectionStart = 9999; };

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

            vm.SamplesCollection.Add(new TextSample("Complete", "Aé♫山𝄞🐗\r\nœæĳøß≤≠Ⅷﬁﬆ\r\n🐱‍🏍 🐱‍👓 🐱‍🚀 🐱‍👤 🐱‍🐉 🐱‍💻\r\n🧝 🧝‍♂️ 🧝‍♀️ 🧝🏽 🧝🏽‍♂️ 🧝🏽‍♀️"));
            vm.SamplesCollection.Add(new TextSample("Simple glyphs", "Aé♫山𝄞🐗"));
            vm.SamplesCollection.Add(new TextSample("Combining accent", "Où ça? Là!"));
            vm.SamplesCollection.Add(new TextSample("Outside BMP", "𝄞𝄡𝄢"));
            vm.SamplesCollection.Add(new TextSample("Multiple lines", "Line 1\r\nLine 2\rLine 3\nLine 4"));
            vm.SamplesCollection.Add(new TextSample("Macros", "U+0041{semicolon}{U+0042}"));
            vm.SamplesCollection.Add(new TextSample("Extreme combining", "aU+0300-036F"));
            vm.SamplesCollection.Add(new TextSample("Control characters C0+C1", "{U+0000-001F}{U+007F}{U+0080-009F}"));
            vm.SamplesCollection.Add(new TextSample("Line breakers", "{U+000A}{U+000D}{U+2028}{U+2029}"));
            vm.SamplesCollection.Add(new TextSample("Unassigned codepoints", "U+0380U+0381U+0382U+0383"));
            vm.SamplesCollection.Add(new TextSample("Not a character", "{U+FDD0-U+FDEF}{U+FFFE}{U+FFFF}"));
            vm.SamplesCollection.Add(new TextSample("Invalid surrogates", "{U+D834}{U+DD1E}"));
            vm.SamplesCollection.Add(new TextSample("Beyond U+10FFFF", "{U+110000}"));
            vm.SamplesCollection.Add(new TextSample("BMP", "U+0000..FFFF"));
            vm.SamplesCollection.Add(new TextSample("Ranges members", "U+0000U+001FU+3400U+4E00U+AC00U+E000U+17000U+18D00U+20000U+2A700U+2B740U+2B820U+2CEB0U+30000U+F0000U+100000"));
            vm.SamplesCollection.Add(new TextSample("Arrows", "←↑→↓\r￩￪￫￬\r🠀🠁🠂🠃\r🠄🠅🠆🠇\r🠈🠉🠊🠋\r🠐🠑🠒🠓\r🠔🠕🠖🠗\r🠘🠙🠚🠛\r🠜🠝🠞🠟\r🠠🠡🠢🠣\r🠤🠥🠦🠧\r🠨🠩🠪🠫\r🠬🠭🠮🠯\r🠰🠱🠲🠳\r🠴🠵🠶🠷\r🠸🠹🠺🠻\r🠼🠽🠾🠿\r🡀🡁🡂🡃\r🡄🡅🡆🡇\r🡐🡑🡒🡓\r🡠🡡🡢🡣\r🡰🡱🡲🡳\r🢀🢁🢂🢃\r🢐🢑🢒🢓\r🢔🢕🢖🢗\r🢘🢙🢚🢛\r◀▲▶▼\r◁△▷▽\r◂▴▸▾\r⏴⏵⏶⏷"));
            vm.SamplesCollection.Add(new TextSample("Digits", "U+0030-U+0039\rU+0660-U+0669\rU+06F0-U+06F9\rU+07C0-U+07C9\rU+0966-U+096F\rU+09E6-U+09EF\rU+0A66-U+0A6F\rU+0AE6-U+0AEF\rU+0B66-U+0B6F\rU+0BE6-U+0BEF\rU+0C66-U+0C6F\rU+0CE6-U+0CEF\rU+0D66-U+0D6F\rU+0DE6-U+0DEF\rU+0E50-U+0E59\rU+0ED0-U+0ED9\rU+0F20-U+0F29\rU+1040-U+1049\rU+1090-U+1099\rU+17E0-U+17E9\rU+1810-U+1819\rU+1946-U+194F\rU+19D0-U+19D9\rU+1A80-U+1A89\rU+1A90-U+1A99\rU+1B50-U+1B59\rU+1BB0-U+1BB9\rU+1C40-U+1C49\rU+1C50-U+1C59\rU+A620-U+A629\rU+A8D0-U+A8D9\rU+A8E0-U+A8E9\rU+A900-U+A909\rU+A9D0-U+A9D9\rU+A9F0-U+A9F9\rU+AA50-U+AA59\rU+ABF0-U+ABF9\rU+FF10-U+FF19\rU+104A0-U+104A9\rU+10D30-U+10D39\rU+11066-U+1106F\rU+110F0-U+110F9\rU+11136-U+1113F\rU+111D0-U+111D9\rU+112F0-U+112F9\rU+11450-U+11459\rU+114D0-U+114D9\rU+11650-U+11659\rU+116C0-U+116C9\rU+11730-U+11739\rU+118E0-U+118E9\rU+11950-U+11959\rU+11C50-U+11C59\rU+11D50-U+11D59\rU+11DA0-U+11DA9\rU+16A60-U+16A69\rU+16B50-U+16B59\rU+16E80-U+16E89\rU+1D7CE-U+1D7D7\rU+1D7D8-U+1D7E1\rU+1D7E2-U+1D7EB\rU+1D7EC-U+1D7F5\rU+1D7F6-U+1D7FF\rU+1E140-U+1E149\rU+1E2F0-U+1E2F9\rU+1E950-U+1E959\rU+1F101-U+1F10A\rU+1FBF0-U+1FBF9\r\rU+0F2A-U+0F32\rU+1369-U+1371\rU+2460-U+2468\rU+2488-U+2490\rU+24F5-U+24FD\rU+2776-U+277E\rU+278A-U+2792\rU+102E1-U+102E9\rU+10E60-U+10E68\rU+111E1-U+111E9\rU+1D360-U+1D368\rU+1E8C7-U+1E8CF\r"));
            vm.SamplesCollection.Add(new TextSample("Single-script confusable", "ǉeto ljeto"));     // Croatian word for “summer”
            vm.SamplesCollection.Add(new TextSample("Mixed-script confusable", "paypal pаypаl"));   // Cyrillic a in 2nd form
            vm.SamplesCollection.Add(new TextSample("Whole-script confusable", "scope ѕсоре"));     // Full Cyrillic for 2nd form
        }

        // Can't use Window.Current.Close(): Error 'Closing main window is not allowed.'
        // Instead terminate app forcefully
        private void CloseButton_Click(object sender, RoutedEventArgs e) => CoreApplication.Exit();

        private async void AboutButton_Click(object sender, RoutedEventArgs e) => await DoAbout();

        private async static Task DoAbout()
        {
            var myAssembly = Assembly.GetExecutingAssembly();
            var aTitleAttr = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyTitleAttribute));
            string sAssemblyVersion = myAssembly.GetName().Version.Major.ToString() + "." + myAssembly.GetName().Version.Minor.ToString() + "." + myAssembly.GetName().Version.Build.ToString();
            var aDescAttr = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyDescriptionAttribute));
            var aCopyrightAttr = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyCopyrightAttribute));
            var aProductAttr = (AssemblyProductAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyProductAttribute));

            string s = aTitleAttr.Title + " version " + sAssemblyVersion + "\r\n" + aDescAttr.Description + "\r\n\n" + aProductAttr.Product + "\r\n" + aCopyrightAttr.Copyright;
            s += "\n\nUnicode Data: " + UniData.GetUnicodeVersion();

            var dialog = new MessageDialog(s, "About " + aTitleAttr.Title);
            await dialog.ShowAsync();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e) => DoCopy();

        // If there is a selection in the list, just copy the selection, otherwise copy full list
        private void DoCopy()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Character\tCodepointIndex\tCharIndex\tGlyphIndex\tCodepoint\tName\tScript\tCategory\tUTF16\tUTF8");
            foreach (CodepointDetail cd in CodepointsList.SelectedItems.Count == 0 ? CodepointsList.Items : CodepointsList.SelectedItems)
                sb.AppendLine($"{cd.ToDisplayStringFull()}\t{cd.CodepointIndex}\t{cd.CharIndexStr}\t{cd.GlyphIndexStr}\t{cd.CodepointUString}\t{cd.Name}\t{cd.Script}\t{cd.Category}\t{cd.UTF16}\t{cd.UTF8}");
            ClipboardSetTextData(sb.ToString());
        }

        // Convenient helper for UWP
        internal static void ClipboardSetTextData(string s)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(s);
            // Sometimes clipboard access raises an error
            try
            {
                Clipboard.SetContent(dataPackage);
            }
            catch (Exception)
            {
            }
        }

        // Delay processing of TextChanged event 250ms using a DispatcherTimer
        DispatcherTimer? dispatcherTimer;

        private void DispatcherTimer_Tick(object sender, object e)
        {
            if (!(dispatcherTimer is null))
            {
                dispatcherTimer.Stop();
                dispatcherTimer.Tick -= DispatcherTimer_Tick;
                dispatcherTimer = null;
            }

            if (TextSamplesCombo.SelectedIndex >= 0)
                TextSamplesCombo.SelectedIndex = -1;

            Transform();
        }

        private bool IgnoreInputTextChanged;

        private void InputText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreInputTextChanged)
            {
                IgnoreInputTextChanged = false;
                return;
            }

            if (dispatcherTimer == null)
            {
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += DispatcherTimer_Tick;
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            }
            else
                dispatcherTimer.Stop();

            dispatcherTimer.Start();
        }

        private void Option_Click(object sender, RoutedEventArgs e) => Transform();

        // UWP version of glyphs identification uses Win2D, but it can only be used during a LoadResources or a Draw event from a Win2D canvas
        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            static CanvasTextLayout CreateTextLayout(ICanvasResourceCreator resourceCreator, float canvasWidth, float canvasHeight, string testString)
            {
                var textFormat = new CanvasTextFormat()
                {
                    FontSize = 8,
                    HorizontalAlignment = CanvasHorizontalAlignment.Left,
                    VerticalAlignment = CanvasVerticalAlignment.Top,
                    FontFamily = "Segoe UI",
                    TrimmingGranularity = CanvasTextTrimmingGranularity.Word,
                    TrimmingSign = CanvasTrimmingSign.Ellipsis,
                };
                return new CanvasTextLayout(resourceCreator, testString, textFormat, canvasWidth * 100, canvasHeight * 100);
            }

            ICanvasResourceCreatorWithDpi resourceCreator = sender;
            var targetSize = ConversionCanvas.Size;
            float canvasWidth = (float)targetSize.Width;
            float canvasHeight = (float)targetSize.Height;

            using var textLayout = CreateTextLayout(resourceCreator, canvasWidth, canvasHeight, transformed);
            // Build table char index -> glyph index
            var citogi = new List<int>();
            int glyphIndex = 0;
            foreach (var item in textLayout.ClusterMetrics)
            {
                for (int i = 0; i < item.CharacterCount; i++)
                    citogi.Add(glyphIndex);
                glyphIndex++;
            }

            // Update GlyphIndex for each CodepointDetail in the list, with the Glyph # a Codepoint belongs to
            int maxGlyphIndex = -1;
            for (int i = 0; i < vm.CodepointsCollection.Count; i++)
            {
                var cd = vm.CodepointsCollection[i];
                cd.GlyphIndex = citogi[cd.CodepointIndexStart];
                if (cd.GlyphIndex > maxGlyphIndex) maxGlyphIndex = cd.GlyphIndex;
            }

            vm.GlyphsCount = maxGlyphIndex + 1;
        }

        // To detect and replace special elements between braces in input text
        private static readonly Regex reUPlusXBraces = new Regex(@"{U\+(1?[0-9A-F]{4,5})}", RegexOptions.IgnoreCase);    // {U+1234}
        private static readonly Regex reUPlusX = new Regex(@"U\+(1?[0-9A-F]{4,5})", RegexOptions.IgnoreCase);            // U+1234       Simplified form when not followed by valid hex character
        private static readonly Regex reRangeUPlusXBraces = new Regex(@"{U\+(1?[0-9A-F]{4,5})(\.\.|-)(?:U\+)?(1?[0-9A-F]{4,5})}", RegexOptions.IgnoreCase);    // {U+1234..U+2345} or {U+1234..2345}, use - instead of .. to avoid inserting a CR at the end of each page of 32 characters
        private static readonly Regex reRangeUPlusX = new Regex(@"U\+(1?[0-9A-F]{4,5})(\.\.|-)(?:U\+)?(1?[0-9A-F]{4,5})", RegexOptions.IgnoreCase);            // U+1234..U+2345
        private static readonly Regex reName = new Regex(@"{([^}]+)}");                         // {semicolon}

        private void Transform()
        {
            string s = InputText.Text;

            // In UWP, test that text has changed from template can't use simple text comparison since assigning a text to a TextBox modifies it (for example, LF disappear)

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
            s = reRangeUPlusXBraces.Replace(s, ma => ReplaceRange(ma));
            s = reRangeUPlusX.Replace(s, ma => ReplaceRange(ma));

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
            s = reUPlusXBraces.Replace(s, ma => ReplaceCodepoint(ma));
            s = reUPlusX.Replace(s, ma => ReplaceCodepoint(ma));

            // And finally replace {semicolon} by ;
            s = reName.Replace(s, ma =>
            {
                int cp = UniData.GetCPFromName(ma.Groups[1].ToString());
                if (cp >= 0)
                    return UniData.AsString(cp);
                return ma.Groups[0].ToString();          // If not recognized
            });

            // Process normalization and case transformations
            if (Seq_CN.IsChecked!.Value)
            {
                if (Case_Lower.IsChecked!.Value)
                    s = s.ToLower();
                else if (Case_Upper.IsChecked!.Value)
                    s = s.ToUpper();
            }

            if (Norm_NFC.IsChecked!.Value)
                s = s.Normalize(NormalizationForm.FormC);
            else if (Norm_NFD.IsChecked!.Value)
                s = s.Normalize(NormalizationForm.FormD);
            else if (Norm_NFKC.IsChecked!.Value)
                s = s.Normalize(NormalizationForm.FormKC);
            else if (Norm_NFKD.IsChecked!.Value)
                s = s.Normalize(NormalizationForm.FormKD);

            if (Seq_NC.IsChecked!.Value)
            {
                if (Case_Lower.IsChecked!.Value)
                    s = s.ToLower();
                else if (Case_Upper.IsChecked!.Value)
                    s = s.ToUpper();
            }

            // Assemble source characters into Codepoints, handling surrogates and regex transformations
            var codepointsList = GetCodepointsDetails(s);

            // Display List of Codepoints
            var sbTransformed = new StringBuilder();
            var sbDisplay = new StringBuilder();
            vm.CodepointsCollection.Clear();
            foreach (var cd in codepointsList)
            {
                vm.CodepointsCollection.Add(cd);
                sbTransformed.Append(cd.ToString());
                sbDisplay.Append(cd.ToDisplayString());
            }

            // transformed is the actual text analyzed after replacing macros
            transformed = sbTransformed.ToString();

            // displayed string is identical to transformed string with most control characters 0..31 and 127 replaced by a visual placeholder
            string displayed = sbDisplay.ToString();
            ResultText.Text = displayed;

            // Assembling in glyphs is asynchronous in UWP
            ConversionCanvas.Invalidate();

            // Update counts
            vm.CharactersCount = transformed.Length;
            vm.CodepointsCount = codepointsList.Count;
            vm.GlyphsCount = 0;     // handled asynchronously
        }

        // Assemble source characters into Codepoints, handling surrogates
        private static IList<CodepointDetail> GetCodepointsDetails(string s)
        {
            var l = new List<CodepointDetail>();
            int codepointCount = 0;

            int charIndexStart, charIndexEnd;               // Before surrogates processing
            int codepointIndexStart, codepointIndexEnd;     // After surrogates processing
            int codepointIndex = 0;
            for (int i = 0; i < s.Length; i++)
            {
                // Ordinary char
                int cp = s[i];
                charIndexStart = i;
                if (cp >= 0xD800 && cp <= 0xDBFF)           // Surrogate?
                {
                    int c2 = s[++i];
                    cp = 0x10000 + ((cp & 0x3ff) << 10) + (0x3ff & c2);
                }
                charIndexEnd = i;

                // For codepoint index, we just care whether cp is represented using 1 or 2 chars
                codepointIndexStart = codepointIndex++;
                codepointIndexEnd = (cp > 0xFFFF) ? codepointIndex++ : codepointIndexStart;

                // Finally add new cd to the list
                l.Add(new CodepointDetail(cp, codepointCount++, charIndexStart, charIndexEnd, codepointIndexStart, codepointIndexEnd));
            }
            return l;
        }

        private void InputText_AccessKeyInvoked(UIElement sender, Windows.UI.Xaml.Input.AccessKeyInvokedEventArgs args) 
            => InputText.Focus(FocusState.Programmatic);
        private void ResultText_AccessKeyInvoked(UIElement sender, Windows.UI.Xaml.Input.AccessKeyInvokedEventArgs args) 
            => ResultText.Focus(FocusState.Programmatic);

        internal void CodepointsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreListSelectionChanged) return;

            if (!(sender is ListView lv))
                return;

            // No selection, nothing to do!
            if (lv.SelectedRanges.Count == 0)
                return;

            IgnoreTextSelectionChanged = true;

            // Get 1st range extent from the list
            int codepointIndexStart = 0, codepointIndexEnd = 0;
            foreach (var selRange in lv.SelectedRanges)
            {
                codepointIndexStart = selRange.FirstIndex;
                codepointIndexEnd = selRange.LastIndex;
                break;      // Only show 1st range
            }

            // Adjust codepointIndexStart/codepointIndexEnd to be the first/last belonging to a glyph, since we can only highlight glyphs
            IgnoreListSelectionChanged = true;
            int glyphIndexStart = vm.CodepointsCollection[codepointIndexStart].GlyphIndex;
            while (codepointIndexStart > 0 && glyphIndexStart == vm.CodepointsCollection[codepointIndexStart - 1].GlyphIndex)
                CodepointsList.SelectedItems.Add(vm.CodepointsCollection[--codepointIndexStart]);
            int glyphIndexEnd = vm.CodepointsCollection[codepointIndexEnd].GlyphIndex;
            while (codepointIndexEnd < vm.CodepointsCollection.Count - 1 && glyphIndexEnd == vm.CodepointsCollection[codepointIndexEnd + 1].GlyphIndex)
                CodepointsList.SelectedItems.Add(vm.CodepointsCollection[++codepointIndexEnd]);
            IgnoreListSelectionChanged = false;

            // Convert back from codepointIndex to charIndex
            int charIndexStart = vm.CodepointsCollection[codepointIndexStart].CharIndexStart;
            int charIndexEnd = vm.CodepointsCollection[codepointIndexEnd].CharIndexEnd;

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

            // ToDo: Ensure that end of selection is visible
            // https://stackoverflow.com/questions/40114620/uwp-c-sharp-scroll-to-the-bottom-of-textbox
            // Looks complex for a ScrollToCaret()...

            IgnoreTextSelectionChanged = false;
        }

        private void ResultText_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (IgnoreTextSelectionChanged) return;
            if (!(sender is TextBox tb)) return;
            if (tb.FocusState == FocusState.Unfocused) return;

            IgnoreListSelectionChanged = true;

            CodepointsList.SelectedItems.Clear();
            if (tb.SelectionLength > 0)
            {
                CodepointDetail lastCd = vm.CodepointsCollection[0];        // To avoid unassigned variable warning
                foreach (var cd in vm.CodepointsCollection)
                    if (cd.CharIndexStart >= tb.SelectionStart && cd.CharIndexEnd < tb.SelectionStart + tb.SelectionLength)
                    {
                        CodepointsList.SelectedItems.Add(cd);
                        lastCd = cd;
                    }
                CodepointsList.ScrollIntoView(lastCd);                      // Make last selected item visible
            }

            IgnoreListSelectionChanged = false;
        }

        private void SamplesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox samplesList)
                if (samplesList.SelectedValue is TextSample ts)
                {
                    InputText.Text = ts.Text;
                    InputText.SelectionStart = InputText.Text.Length;

                    IgnoreInputTextChanged = true;  // Ignore next event TextChanged on InputText

                    // Transform immediately, no need to wait for timer in this case
                    Transform();
                }
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
}
