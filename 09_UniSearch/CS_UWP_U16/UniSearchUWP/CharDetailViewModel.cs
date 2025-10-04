// CharDetailViewModel
// Support for CharDetailDialog binding
//
// In UWP version, selected character can change during drill down, so INotifyPropertyChanged is implemented.
//
// 2018-09-18   PV
// 2020-11-11   PV      Hyperlinks to block and subheader; nullable enable
// 2020-11-12   PV      Added Synonyms, Comments and Cross-refs.  Block/Subheaders hyperlinks.  Copy buttons.  Scrollviewer
// 2020-11-20   PV      Search fonts containing a given character
// 2023-11-19   PV      1.8 Net8 C#12
// 2024-05-13   PV      1.9 Added Julia information
// 2024-09-20   PV      Added confusables, changed hyperlink color

using Microsoft.Graphics.Canvas.Text;
using RelayCommandNS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using UniDataNS;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

#nullable enable

namespace UniSearchNS;

internal class CharDetailViewModel: INotifyPropertyChanged, IDisposable
{
    // Private variables
    private readonly Stack<int> History = new();
    private readonly CharDetailDialog window;
    private readonly ViewModel MainViewModel;

    // INotifyPropertyChanged interface
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string propertyName)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // Commands public interface
    public ICommand ShowDetailCommand { get; private set; }
    public ICommand NewFilterCommand { get; private set; }
    public ICommand SearchFontsCommand { get; private set; }

    // IDispose implementation for owned objects implementing IDisposable
    public void Dispose() => (bgWorkerExport as IDisposable)?.Dispose();

    // Constructor
    public CharDetailViewModel(CharDetailDialog window, CharacterRecord cr, ViewModel mainViewModel)
    {
        this.window = window;
        SelectedChar = cr;
        MainViewModel = mainViewModel;

        ShowDetailCommand = new RelayCommand<int>(ShowDetailExecute);
        NewFilterCommand = new RelayCommand<string>(NewFilterExecute);
        SearchFontsCommand = new RelayCommand<object>(SearchFontsExecute);
    }

    // ==============================================================================================
    // Bindable properties

    private CharacterRecord _SelectedChar = UniData.CharacterRecords[0];    // To avoid making it nullable
    public CharacterRecord SelectedChar
    {
        get => _SelectedChar;
        set
        {
            if (_SelectedChar != value)
            {
                _SelectedChar = value;
                NotifyPropertyChanged(nameof(SelectedChar));
                NotifyPropertyChanged(nameof(Title));
                NotifyPropertyChanged(nameof(NormalizationNFDContent));
                NotifyPropertyChanged(nameof(NormalizationNFKDContent));
                NotifyPropertyChanged(nameof(LowercaseContent));
                NotifyPropertyChanged(nameof(UppercaseContent));
                NotifyPropertyChanged(nameof(BlockContent));
                NotifyPropertyChanged(nameof(SubheaderContent));
                NotifyPropertyChanged(nameof(SynonymsContent));
                NotifyPropertyChanged(nameof(CrossRefsContent));
                NotifyPropertyChanged(nameof(CommentsContent));
            }
        }
    }

    public string Title => SelectedChar == null ? "Character Detail" : SelectedChar.CodepointHex + " – Character Detail";

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

        StackPanel sp = new();
        foreach (var cr in sn.EnumCharacterRecords())
            sp.Children.Add(ViewModel.GetStrContent(cr.Codepoint, ShowDetailCommand));
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

                        // ToDo: Optimize of empty
                        var t1 = new TextBlock { Text = sres };
                        Grid.SetColumn(t1, 0);
                        g.Children.Add(t1);
                        sres = "";

                        HyperlinkButton t2 = new()
                        {
                            Margin = new Thickness(0, 0, 0, 1),
                            Padding = new Thickness(0),
                            Content = cr.CodepointHex,
                            Command = ShowDetailCommand,
                            CommandParameter = c,
                            Foreground = new SolidColorBrush(Colors.DeepSkyBlue),
                        };
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

    private UIElement GetBlockHyperlink(string content, string commandParameter) => new HyperlinkButton
    {
        Margin = new Thickness(0),
        Padding = new Thickness(0),
        Content = content,
        Command = NewFilterCommand,
        CommandParameter = commandParameter,
        Foreground = new SolidColorBrush(Colors.DeepSkyBlue),
    };

    public UIElement BlockContent => GetBlockHyperlink(SelectedChar.Block.BlockNameAndRange, "b:\"" + SelectedChar?.Block.BlockName + "\"");

    public UIElement SubheaderContent => GetBlockHyperlink(SelectedChar.Subheader, "s:\"" + SelectedChar?.Subheader + "\"");

    public UIElement? LowercaseContent => CaseContent(true);

    public UIElement? UppercaseContent => CaseContent(false);

    private UIElement? CaseContent(bool lower)
    {
        if (!SelectedChar.IsPrintable)
            return null;

        string s = SelectedChar.Character;
        string sc = lower ? s.ToLowerInvariant() : s.ToUpperInvariant();
        if (s == sc)
            return null;

        StackPanel sp = new();
        foreach (var cr in sc.EnumCharacterRecords())
            sp.Children.Add(ViewModel.GetStrContent(cr.Codepoint, ShowDetailCommand));
        return sp;
    }

    public UIElement? SynonymsContent => GetExtraInfo(SelectedChar.Synonyms, false);
    public UIElement? CrossRefsContent => GetExtraInfo(SelectedChar.CrossRefs, true);
    public UIElement? CommentsContent => GetExtraInfo(SelectedChar.Comments, true);
    public string Julia => SelectedChar.Julia;

    private static readonly Regex reCP = new(@"\b1?[0-9A-F]{4,5}\b");

    private UIElement? GetExtraInfo(List<string>? list, bool autoHyperlink)
    {
        if (list == null)
            return null;

        TextBlock tb = new() { TextWrapping = TextWrapping.Wrap };
        foreach (string s in list)
        {
            if (tb.Inlines.Count > 0)
                tb.Inlines.Add(new LineBreak());

            if (autoHyperlink)
            {
                int sp = 0;
                for (; ; )
                {
                    var ma = reCP.Match(s, sp);
                    if (!ma.Success)
                    {
                        tb.Inlines.Add(new Run { Text = s.Substring(sp) });
                        break;
                    }

                    if (ma.Index > sp)
                        tb.Inlines.Add(new Run { Text = s.Substring(sp, ma.Index - sp) });
                    int cp = Convert.ToInt32(ma.ToString(), 16);
                    tb.Inlines.Add(GetCodepointHyperlink(cp, Hyperlink_Click, true));
                    sp = ma.Index + ma.Length;
                }
            }
            else
                tb.Inlines.Add(new Run { Text = s });
        }

        return tb;
    }

    // Returns an hyperlink to a specific codepoint, with optional tooltip containing codepoint glyph
    // Also called from CharDetailViewModel
    internal static Hyperlink GetCodepointHyperlink(int codepoint, TypedEventHandler<Hyperlink, HyperlinkClickEventArgs> Click, bool withToolTip)
    {
        // \x2060 is Unicode Word Joiner, to prevent line wrap here
        Run r = new() { Text = $"U\x2060+\x2060{codepoint:X4}" };

        Hyperlink h = new();
        h.Inlines.Add(r);
        h.Click += Click;
        h.Foreground = new SolidColorBrush(Colors.DeepSkyBlue);

        if (withToolTip)
        {
            var cps = UniData.AsString(codepoint);

            var tb = new TextBlock
            {
                FontSize = 64,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Black),
                Text = cps
            };

            var vb = new Viewbox { Child = tb };

            var b = new Border
            {
                Width = 96,
                Height = 96,
                Background = new SolidColorBrush(Colors.CornflowerBlue),
                Child = vb
            };

            ToolTip tooltip = new()
            {
                Content = b,
                HorizontalOffset = 120,
                VerticalOffset = 50
            };
            ToolTipService.SetToolTip(h, tooltip);
        }

        return h;
    }

    // Helper to work around the limitaion of a Hyperlink in UWP
    // Hyperlink can be placed in an Inlines(), but does not support Command/Parameter
    // HyperlinkButton do support Command/Parameter, but can't be added to Inlines()
    // Solution: handle the click event of Hyperlink...
    private void Hyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
    {
        if (sender.Inlines[0] is Run r)
        {
            string s = r.Text;
            if (s.StartsWith("U\x2060+\x2060", StringComparison.OrdinalIgnoreCase))
            {
                int cp = Convert.ToInt32(s.Substring(4), 16);
                NavigateToCodepoint(cp, true);
            }
        }
    }

    public ObservableCollection<object> FontsList { get; private set; } = [];

    private Visibility _SearchFontsButtonVisibility = Visibility.Visible;
    public Visibility SearchFontsButtonVisibility
    {
        get => _SearchFontsButtonVisibility;
        private set
        {
            if (value != _SearchFontsButtonVisibility)
            {
                _SearchFontsButtonVisibility = value;
                NotifyPropertyChanged(nameof(SearchFontsButtonVisibility));
            }
        }
    }

    private Visibility _SearchFontsProgressBarVisibility = Visibility.Collapsed;
    public Visibility SearchFontsProgressBarVisibility
    {
        get => _SearchFontsProgressBarVisibility;
        private set
        {
            if (value != _SearchFontsProgressBarVisibility)
            {
                _SearchFontsProgressBarVisibility = value;
                NotifyPropertyChanged(nameof(SearchFontsProgressBarVisibility));
            }
        }
    }

    private Visibility _FontsListVisibility = Visibility.Collapsed;
    public Visibility FontsListVisibility
    {
        get => _FontsListVisibility;
        private set
        {
            if (value != _FontsListVisibility)
            {
                _FontsListVisibility = value;
                NotifyPropertyChanged(nameof(FontsListVisibility));
            }
        }
    }

    private int _SearchFontsProgress;
    public int SearchFontsProgress
    {
        get => _SearchFontsProgress;
        private set
        {
            if (value != _SearchFontsProgress)
            {
                _SearchFontsProgress = value;
                NotifyPropertyChanged(nameof(SearchFontsProgress));
            }
        }
    }

    private string _FontsLabel = "Fonts";
    public string FontsLabel
    {
        get => _FontsLabel;
        set
        {
            if (_FontsLabel != value)
            {
                _FontsLabel = value;
                NotifyPropertyChanged(nameof(FontsLabel));
            }
        }
    }

    // ==============================================================================================
    // Commands

    internal void Back()
    {
        window.BackButton.IsEnabled = History.Count != 1;
        NavigateToCodepoint(History.Pop(), false);
    }

    private void ShowDetailExecute(int codepoint) => NavigateToCodepoint(codepoint, true);

    private void NavigateToCodepoint(int codepoint, bool trackHistory)
    {
        if (trackHistory)
        {
            History.Push(SelectedChar.Codepoint);
            window.BackButton.IsEnabled = true;
        }

        FontsList.Clear();
        SearchFontsButtonVisibility = Visibility.Visible;
        SearchFontsProgressBarVisibility = Visibility.Collapsed;
        FontsListVisibility = Visibility.Collapsed;
        SearchFontsProgress = 0;
        FontsLabel = "Fonts";

        SelectedChar = UniData.CharacterRecords[codepoint];
    }

    // From hyperlink
    private void NewFilterExecute(string filter)
    {
        window.Hide();
        MainViewModel.CharNameFilter = filter;
    }

    BackgroundWorker? bgWorkerExport;

    private void SearchFontsExecute(object _)
    {
        SearchFontsButtonVisibility = Visibility.Collapsed;
        SearchFontsProgress = 0;
        SearchFontsProgressBarVisibility = Visibility.Visible;
        FontsLabel = "Fonts (searching...)";

        bgWorkerExport = new BackgroundWorker { WorkerReportsProgress = true };
        bgWorkerExport.DoWork += Export_DoWork;
        bgWorkerExport.RunWorkerCompleted += Export_RunWorkerCompleted;
        bgWorkerExport.ProgressChanged += new ProgressChangedEventHandler(Export_ProgressChanged);
        bgWorkerExport.RunWorkerAsync();
    }

    private void Export_ProgressChanged(object? sender, ProgressChangedEventArgs e) => SearchFontsProgress = e.ProgressPercentage;

    private void Export_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        SearchFontsProgressBarVisibility = Visibility.Collapsed;
        FontsList.Clear();
        if (e.Result is List<string> ls)
            foreach (string s in ls)
            {
                var ts = s.Split('\t');
                string fontFamily = ts[0];
                var sp = new StackPanel { Orientation = Orientation.Horizontal };
                if (!string.IsNullOrEmpty(fontFamily))      // Ignore final placeholder
                {
                    var tb1 = new TextBlock
                    {
                        Text = SelectedChar.Character,
                        FontFamily = new FontFamily(fontFamily),
                        Width = 50,
                        Padding = new Thickness(0, 3, 0, 0)
                    };
                    var tb2 = new TextBlock { Text = fontFamily + " " + ts[1] };

                    sp.Children.Add(tb1);
                    sp.Children.Add(tb2);
                }
                else
                {
                    // Placeholder for an empty last line, hidden by horizontal scrollbar
                    sp.Children.Add(new TextBlock { Text = "" });
                }
                FontsList.Add(sp);
            }
        FontsListVisibility = Visibility.Visible;
        FontsLabel = $"Fonts ({FontsList.Count})";
    }

    private void Export_DoWork(object? sender, DoWorkEventArgs e)
    {
        var dic = new Dictionary<string, string>();
        var sfs = CanvasFontSet.GetSystemFontSet();
        var fontsCount = sfs.Fonts.Count;
        int nf = 0;
        foreach (var font in sfs.Fonts)
        {
            if (bgWorkerExport?.CancellationPending == true)
            {
                e.Cancel = true;
                break;
            }

            nf += 1;
            int progress = (int)(100 * nf / fontsCount + 0.5);
            bgWorkerExport?.ReportProgress(progress);

            if (font.Simulations != CanvasFontSimulations.None)
                continue;

            if (font.HasCharacter((uint)SelectedChar.Codepoint))
            {
                string familyName, faceName;
                familyName = font.FamilyNames.ContainsKey("en-us") ? font.FamilyNames["en-us"] : font.FamilyNames.Values.First();
                faceName = font.FaceNames.ContainsKey("en-us") ? font.FaceNames["en-us"] : font.FaceNames.Values.First();

                if (!dic.ContainsKey(familyName))
                    dic.Add(familyName, faceName);
                else
                    dic[familyName] += ", " + faceName;
            }
        }

        // Return sorted list of fonts to UI thread
        var list = new List<string>();
        foreach (var familyName in dic.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
            list.Add($"{familyName}\t({dic[familyName]})");
        // Add a placeholder for a fake last item so that horizontal scrollbar won't hide the last font
        list.Add("\t");

        e.Result = list;
    }

}
