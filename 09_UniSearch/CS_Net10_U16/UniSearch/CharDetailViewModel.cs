// CharDetailViewModel
// Support for CharDetailWindow binding
//
// 2018-09-15   PV
// 2020-11-11   PV      Hyperlinks to block and subheader; nullable enable
// 2020-11-12   PV      Added Synonyms, Comments and Cross-refs.  Block/Subheaders hyperlinks.  Copy buttons.  Scrollviewer
// 2020-11-20   PV      Search fonts containing a given character
// 2023-11-19   PV      1.8    Net8 C#12
// 2024-05-13   PV      1.9    Added Julia information
// 2024-09-13   PV      1.16.0 Net8 CS12 U16
// 2024-09-20   PV      1.16.1 Confusables
// 2026-01-20	PV		Net10 C#14

using DirectDrawWrite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UniDataNS;

namespace UniSearch;

internal sealed partial class CharDetailViewModel: INotifyPropertyChanged, IDisposable
{
    // Private variables
    private readonly CharDetailWindow Win;
    private readonly ViewModel MainVM;

    // INotifyPropertyChanged interface
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // Commands public interface
    public ICommand ShowDetailCommand { get; }
    public ICommand NewFilterCommand { get; }
    public ICommand CopyCharCommand { get; }
    public ICommand CopyAllInfoCommand { get; }
    public ICommand SearchFontsCommand { get; }

    // IDispose implementation for owned objects implementing IDisposable
    public void Dispose() => (ExportWorker as IDisposable)?.Dispose();

    // Constructor
    public CharDetailViewModel(CharDetailWindow win, CharacterRecord cr, ViewModel mainVm)
    {
        SelectedChar = cr;
        Win = win;
        MainVM = mainVm;

        ShowDetailCommand = new RelayCommand<int>(ShowDetailExecute);
        NewFilterCommand = new RelayCommand<string>(NewFilterExecute);
        CopyCharCommand = new RelayCommand<object>(CopyCharExecute);
        CopyAllInfoCommand = new RelayCommand<object>(CopyAllInfoExecute);
        SearchFontsCommand = new RelayCommand<object>(SearchFontsExecute);
    }

    // ==============================================================================================
    // Bindable properties

    public CharacterRecord SelectedChar { get; } = UniData.CharacterRecords[0];    // To avoid making it nullable

    public BitmapSource? SelectedCharImage
    {
        get
        {
            string s = SelectedChar.Character;
            if (s.StartsWith("U+", StringComparison.Ordinal))
                s = "U+ " + s[2..];
            return D2DDrawText.GetBitmapSource(s, MainVM.RenderingFont);
        }
    }

    public string Title => SelectedChar.CodepointHex + " – Character Detail";

    public UIElement? NormalizationNFDContent => NormalizationContent(NormalizationForm.FormD);

    public UIElement? NormalizationNFKDContent => NormalizationContent(NormalizationForm.FormKD);

    private UIElement? NormalizationContent(NormalizationForm form)
    {
        if (!SelectedChar.IsPrintable)
            return null;

        string s = SelectedChar.Character;
        string sn = s.Normalize(form);
        if (s == sn)
            return null;
        if (form == NormalizationForm.FormKD && sn == s.Normalize(NormalizationForm.FormD))
            return new TextBlock { Text = "Same as NFD" };

        var sp = new StackPanel();
        foreach (var cr in sn.EnumCharacterRecords())
            sp.Children.Add(MainVM.GetStrContent(cr.Codepoint, ShowDetailCommand, true));
        return sp;
    }

    public UIElement? ConfusablesContent
    {
        get
        {
            if (SelectedChar.Confusables is null)
                return null;

            var sp = new StackPanel();
            foreach (var cl in SelectedChar.Confusables.OrderBy(l => l[0]))
                if (cl.Count != 1 || cl[0] != SelectedChar.Codepoint)
                {
                    string sres = "";
                    foreach (var c in cl)
                        sres += UniData.CharacterRecords[c].AsString;

                    foreach (var c in cl)
                    {
                        var g = new Grid();
                        g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40) });
                        g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
                        g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                        var cr = UniData.CharacterRecords[c];

                        var t1 = new TextBlock { Text = sres };
                        Grid.SetColumn(t1, 0);
                        g.Children.Add(t1);
                        sres = "";

                        var t2 = new TextBlock(MainVM.GetCodepointHyperlink(cr.Codepoint, ShowDetailCommand, true));
                        Grid.SetColumn(t2, 1);
                        g.Children.Add(t2);

                        var t3 = new TextBlock { Text = cr.Name };
                        Grid.SetColumn(t3, 2);
                        g.Children.Add(t3);

                        sp.Children.Add(g);
                    }
                }
            return sp;
        }
    }

    private TextBlock GetBlockHyperlink(string? content, string commandParameter)
    {
        var h = new Hyperlink(new Run(content))
        {
            Command = NewFilterCommand,
            CommandParameter = commandParameter
        };
        return new TextBlock(h);
    }

    public TextBlock BlockContent => GetBlockHyperlink(SelectedChar.Block.BlockNameAndRange, "b:\"" + SelectedChar.Block.BlockName + "\"");

    public TextBlock SubheaderContent => GetBlockHyperlink(SelectedChar.Subheader, "s:\"" + SelectedChar.Subheader + "\"");

    public StackPanel? LowercaseContent => CaseContent(true);

    public StackPanel? UppercaseContent => CaseContent(false);

    private StackPanel? CaseContent(bool lower)
    {
        if (!SelectedChar.IsPrintable)
            return null;

        string s = SelectedChar.Character;
        string sc = lower ? s.ToLowerInvariant() : s.ToUpperInvariant();
        if (s == sc)
            return null;

        var sp = new StackPanel();
        foreach (var cr in sc.EnumCharacterRecords())
            sp.Children.Add(MainVM.GetStrContent(cr.Codepoint, ShowDetailCommand, true));
        return sp;
    }

    public TextBlock? SynonymsContent => GetExtraInfo(SelectedChar.Synonyms, false);
    public TextBlock? CrossRefsContent => GetExtraInfo(SelectedChar.CrossRefs, true);
    public TextBlock? CommentsContent => GetExtraInfo(SelectedChar.Comments, true);
    public string Julia => SelectedChar.Julia;

    [GeneratedRegex("\\b1?[0-9A-F]{4,5}\\b")]
    private static partial Regex reCP();

    private TextBlock? GetExtraInfo(List<string>? list, bool autoHyperlink)
    {
        if (list == null)
            return null;

        var tb = new TextBlock { TextWrapping = TextWrapping.Wrap };
        foreach (string s in list)
        {
            if (tb.Inlines.Count > 0)
                tb.Inlines.Add(new LineBreak());

            if (autoHyperlink)
            {
                int sp = 0;
                for (; ; )
                {
                    var ma = reCP().Match(s, sp);
                    if (!ma.Success)
                    {
                        tb.Inlines.Add(new Run(s[sp..]));
                        break;
                    }

                    if (ma.Index > sp)
                        tb.Inlines.Add(new Run(s[sp..ma.Index]));
                    int cp = Convert.ToInt32(ma.ToString(), 16);
                    tb.Inlines.Add(MainVM.GetCodepointHyperlink(cp, ShowDetailCommand, true));
                    sp = ma.Index + ma.Length;
                }
            }
            else
                tb.Inlines.Add(new Run(s));
        }

        return tb;
    }

    public ObservableCollection<object> FontsList { get; } = [];

    public Visibility SearchFontsButtonVisibility
    {
        get;
        private set
        {
            if (value != field)
            {
                field = value;
                NotifyPropertyChanged(nameof(SearchFontsButtonVisibility));
            }
        }
    } = Visibility.Visible;

    public Visibility SearchFontsProgressBarVisibility
    {
        get;
        private set
        {
            if (value != field)
            {
                field = value;
                NotifyPropertyChanged(nameof(SearchFontsProgressBarVisibility));
            }
        }
    } = Visibility.Hidden;

    public Visibility FontsListVisibility
    {
        get;
        private set
        {
            if (value != field)
            {
                field = value;
                NotifyPropertyChanged(nameof(FontsListVisibility));
            }
        }
    } = Visibility.Hidden;

    public int SearchFontsProgress
    {
        get;
        private set
        {
            if (value != field)
            {
                field = value;
                NotifyPropertyChanged(nameof(SearchFontsProgress));
            }
        }
    }

    public string FontsLabel
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                NotifyPropertyChanged(nameof(FontsLabel));
            }
        }
    } = "Fonts";

    // ==============================================================================================
    // Commands

    private void ShowDetailExecute(int codepoint) => CharDetailWindow.ShowDetail(codepoint, MainVM);

    private void NewFilterExecute(string filter)
    {
        Win.Close();
        MainVM.CharNameFilter = filter;
    }
    private void CopyCharExecute(object _)
    {
        var records = new List<CharacterRecord> { SelectedChar };
        ViewModel.DoCopyRecords("0", records);
    }
    private void CopyAllInfoExecute(object _)
    {
        var records = new List<CharacterRecord> { SelectedChar };
        ViewModel.DoCopyRecords("3", records);
    }

    BackgroundWorker? ExportWorker;

    private void SearchFontsExecute(object _)
    {
        SearchFontsButtonVisibility = Visibility.Hidden;
        SearchFontsProgress = 0;
        SearchFontsProgressBarVisibility = Visibility.Visible;
        FontsLabel = "Fonts (searching...)";

        ExportWorker = new BackgroundWorker { WorkerReportsProgress = true };
        ExportWorker.DoWork += DoWork;
        ExportWorker.RunWorkerCompleted += RunExportWorkerCompleted;
        ExportWorker.ProgressChanged += ProgressChanged;
        ExportWorker.RunWorkerAsync();
    }

    private void ProgressChanged(object? sender, ProgressChangedEventArgs e) => SearchFontsProgress = e.ProgressPercentage;

    private void RunExportWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        var sbres = new StringBuilder();
        SearchFontsProgressBarVisibility = Visibility.Hidden;
        FontsList.Clear();
        if (e.Result is List<string> ls)
            foreach (string s in ls)
            {
                sbres.AppendLine(s);
                var ts = s.Split('\t');
                string fontFamily = ts[0];
                var tb1 = new TextBlock
                {
                    Text = SelectedChar.Character,
                    FontFamily = new FontFamily(fontFamily),
                    Width = 50
                };
                var tb2 = new TextBlock { Text = fontFamily + " " + ts[1] };
                var sp = new StackPanel { Orientation = Orientation.Horizontal };
                sp.Children.Add(tb1);
                sp.Children.Add(tb2);
                FontsList.Add(sp);
            }
        FontsListVisibility = Visibility.Visible;
        FontsLabel = $"Fonts ({FontsList.Count})";

        try
        {
            Clipboard.Clear();
            Clipboard.SetText(sbres.ToString());
        }
        catch (Exception)
        {
        }
    }

    private void DoWork(object? sender, DoWorkEventArgs e)
    {
        var list = new List<string>();
        var nf = 0;
        var tnf = Fonts.SystemFontFamilies.Count;
        foreach (var family in Fonts.SystemFontFamilies.OrderBy(ff => ff.Source, StringComparer.OrdinalIgnoreCase))
        {
            if (ExportWorker?.CancellationPending == true)
            {
                e.Cancel = true;
                break;
            }

            nf += 1;
            int progress = (int)(100.0 * nf / tnf + 0.5);
            ExportWorker?.ReportProgress(progress);

            int p = family.Source.IndexOf('#');
            var familyName = new StringBuilder(p < 0 ? family.Source : family.Source[p..]);
            bool first = true;

            foreach (Typeface typeface in family.GetTypefaces())
            {
                if (typeface.IsBoldSimulated || typeface.IsObliqueSimulated)
                    continue;

                typeface.TryGetGlyphTypeface(out GlyphTypeface glyph);
                bool included = false;
                if (glyph != null)
                    included = glyph.CharacterToGlyphMap.ContainsKey(SelectedChar.Codepoint);

                if (included)
                {
                    if (first)
                    {
                        first = false;
                        familyName.Append("\t(");
                    }
                    else
                        familyName.Append(", ");
                    familyName.Append(GetTypefaceName(typeface));
                }
            }

            if (!first)
            {
                familyName.Append(')');
                list.Add(familyName.ToString());
            }
        }

        // Return list of fonts to UI thread
        e.Result = list;
    }

    private static string GetTypefaceName(Typeface typeface)
    {
        var englishLanguage = XmlLanguage.GetLanguage("en-US");
        if (!typeface.FaceNames.TryGetValue(englishLanguage, out string name))
            name = "[No name]";     // Find a better fallback!
        return name;
    }

}