// UniSearch ViewModel
// Interaction and commands support
//
// 2018-09-12   PV
// 2018-09-28   PV      New blocks filtering controlling node expansion and not visibility
// 2020-11-11   PV      Refilter char list after a block filter update; New 1-letter filtering rule; Dot Net 5.0
// 2020-11-12   PV      Copy Full Info
// 2021-01-04   PV      GetBlockHyperlink includes LastResortFont element
// 2023-11-19   PV      1.8 Net8 C#12

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DirectDrawWrite;
using UniDataNS;

namespace UniSearch;

internal class ViewModel : INotifyPropertyChanged
{
    // Private variables
    private readonly SearchWindow Win;               // Access to main Win
    private readonly CheckableNode BlocksRoot;          // TreeView root
    private readonly Dictionary<int, CheckableNode> BlocksCheckableNodesDictionary;

    // INotifyPropertyChanged interface
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // Commands public interface
    public ICommand CopyRecordsCommand { get; }
    public ICommand CopyImageCommand { get; }
    public ICommand ShowLevelCommand { get; }
    public ICommand SelectAllCommand { get; }
    public ICommand ShowDetailCommand { get; }
    public ICommand NewFilterCommand { get; }

    // Constructor
    public ViewModel(SearchWindow w)
    {
        Win = w;

        // Binding commands with behavior
        CopyRecordsCommand = new RelayCommand<object>(CopyRecordsExecute, CanCopyRecords);
        CopyImageCommand = new RelayCommand<object>(CopyImageExecute, CanCopyRecords);
        ShowLevelCommand = new RelayCommand<object>(ShowLevelExecute);
        SelectAllCommand = new RelayCommand<string>(SelectAllExecute);
        ShowDetailCommand = new RelayCommand<int>(ShowDetailExecute);
        NewFilterCommand = new RelayCommand<string>(NewFilterExecute);

        // Get Unicode data
        CharactersRecordsList = [.. UniData.CharacterRecords.Values.OrderBy(cr => cr.Codepoint)];
        ReadOnlyDictionary<int, BlockRecord>.ValueCollection blockRecordsList = UniData.BlockRecords.Values;

        // Add grouping
        CollectionView view = (CollectionViewSource.GetDefaultView(CharactersRecordsList) as CollectionView)!;
        var groupDescription = new PropertyGroupDescription("GroupName");
        view.GroupDescriptions?.Add(groupDescription);

        // root is *not* added to the TreeView on purpose
        BlocksRoot = new CheckableNode("root", 4, null);
        // Dictionary of CheckableNodes indexed by Begin block value
        BlocksCheckableNodesDictionary = [];

        foreach (var l3 in blockRecordsList.GroupBy(b => b.Level3Name).OrderBy(g => g.Key))
        {
            var r3 = new CheckableNode(l3.Key, 3, null);
            BlocksRoot.Children.Add(r3);
            foreach (var l2 in l3.GroupBy(b => b.Level2Name))
            {
                var r2 = new CheckableNode(l2.Key, 2, null);
                r3.Children.Add(r2);
                foreach (var l1 in l2.GroupBy(b => b.Level1Name))
                {
                    // Special case, l1 can be empty
                    CheckableNode r1;
                    if (l1.Key.Length > 0)
                    {
                        r1 = new CheckableNode(l1.Key, 1, null);
                        r2.Children.Add(r1);
                    }
                    else
                        r1 = r2;
                    // Blocks
                    foreach (var l0 in l1)
                    {
                        var blockCheckableNode = new CheckableNode(l0.BlockName, 0, l0.RepresentantCharacter);
                        r1.Children.Add(blockCheckableNode);
                        BlocksCheckableNodesDictionary.Add(l0.Begin, blockCheckableNode);
                    }
                }
            }
        }

        BlocksRoot.Initialize();
        BlocksRoot.IsChecked = true;
        Roots = BlocksRoot.Children;

        // Unselect some pretty useless blocks
        BlocksCheckableNodesDictionary[0x0000].IsChecked = false;       // ASCII Controls C0
        BlocksCheckableNodesDictionary[0x0080].IsChecked = false;       // ASCII Controls C1
        BlocksCheckableNodesDictionary[0xE0100].IsChecked = false;      // Variation Selectors Supplement; Variation Selectors; Specials; Symbols and Punctuation
        BlocksCheckableNodesDictionary[0xF0000].IsChecked = false;      // Supplementary Private Use Area-A; ; Private Use; Symbols and Punctuation
        BlocksCheckableNodesDictionary[0x100000].IsChecked = false;     // Supplementary Private Use Area-B; ; Private Use; Symbols and Punctuation
        ActionAllNodes(BlocksRoot, n =>
        {
            if (n.Name == "East Asian Scripts") n.IsChecked = false;
            if (n.Name.Contains("CJK")) n.IsChecked = false;
            if (n.Name == "Specials") n.IsChecked = false;
        });

        BlocksCheckableNodesDictionary[0xD800].IsChecked = false;       //  High Surrogates; ; Surrogates; Symbols and Punctuation
        BlocksCheckableNodesDictionary[0xDB80].IsChecked = false;       //  High Private Use Surrogates; ; Surrogates; Symbols and Punctuation
        BlocksCheckableNodesDictionary[0xDC00].IsChecked = false;       //  Low Surrogates; ; Surrogates; Symbols and Punctuation
        BlocksCheckableNodesDictionary[0xE000].IsChecked = false;       //  Private Use Area; ; Private Use; Symbols and Punctuation

        // To compute bound values based on initial blocks filtering
        FilterCharList();
        RefreshSelBlocks();
    }

    // ==============================================================================================
    // Bindable properties

    public IList<CheckableNode> Roots { get; }      // For TreeView binding
    public CharacterRecord[] CharactersRecordsList { get; }

    private string _CharNameFilter = "";
    public string CharNameFilter
    {
        get => _CharNameFilter;
        set
        {
            if (_CharNameFilter != value)
            {
                _CharNameFilter = value;
                NotifyPropertyChanged(nameof(CharNameFilter));
                StartOrResetCharFilterDispatcherTimer();
            }
        }
    }

    private string _BlockNameFilter = "";
    public string BlockNameFilter
    {
        get => _BlockNameFilter;
        set
        {
            if (_BlockNameFilter != value)
            {
                _BlockNameFilter = value;
                NotifyPropertyChanged(nameof(BlockNameFilter));
                ApplyBlockNameFilter();
            }
        }
    }

    private CharacterRecord? _SelectedChar;
    public CharacterRecord? SelectedChar
    {
        get => _SelectedChar;
        set
        {
            if (_SelectedChar != value)
            {
                _SelectedChar = value;
                NotifyPropertyChanged(nameof(SelectedChar));
                NotifyPropertyChanged(nameof(SelectedCharImage));
                NotifyPropertyChanged(nameof(StrContent));
                NotifyPropertyChanged(nameof(BlockContent));
                NotifyPropertyChanged(nameof(SubheaderContent));
            }
        }
    }

    public BitmapSource? SelectedCharImage
    {
        get
        {
            if (SelectedChar == null)
                return null;
            else
            {
                string s = SelectedChar.Character;
                if (s.StartsWith("U+", StringComparison.Ordinal))
                    s = "U+ " + s[2..];
                return D2DDrawText.GetBitmapSource(s);
            }
        }
    }

    public static int NumChars => UniData.CharacterRecords.Count;

    public int NumBlocks => BlocksCheckableNodesDictionary.Count;

    private int _SelChars;
    public int SelChars
    {
        get => _SelChars;
        set
        {
            if (_SelChars != value)
            {
                _SelChars = value;
                NotifyPropertyChanged(nameof(SelChars));
            }
        }
    }

    private int _FilChars;
    public int FilChars
    {
        get => _FilChars;
        set
        {
            if (_FilChars != value)
            {
                _FilChars = value;
                NotifyPropertyChanged(nameof(FilChars));
            }
        }
    }

    private int _SelBlocks;
    public int SelBlocks
    {
        get => _SelBlocks;
        set
        {
            if (_SelBlocks != value)
            {
                _SelBlocks = value;
                NotifyPropertyChanged(nameof(SelBlocks));
            }
        }
    }

    private int _FilBlocks;
    public int FilBlocks
    {
        get => _FilBlocks;
        set
        {
            if (_FilBlocks != value)
            {
                _FilBlocks = value;
                NotifyPropertyChanged(nameof(FilBlocks));
            }
        }
    }

    public UIElement? StrContent
    {
        get
        {
            if (SelectedChar == null) return null;
            return GetStrContent(SelectedChar.Codepoint, ShowDetailCommand, false);
        }
    }

    public static UIElement GetStrContent(int codepoint, ICommand command, bool withToolTip)
    {
        var cr = UniData.CharacterRecords[codepoint];

        var g = new Grid();
        g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40) });
        g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
        g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

        var t1 = new TextBlock { Text = cr.Character };
        Grid.SetColumn(t1, 0);
        g.Children.Add(t1);

        var t2 = new TextBlock(GetCodepointHyperlink(cr.Codepoint, command, withToolTip));
        Grid.SetColumn(t2, 1);
        g.Children.Add(t2);

        var t3 = new TextBlock { Text = cr.Name };
        Grid.SetColumn(t3, 2);
        g.Children.Add(t3);

        return g;
    }

    // Returns an hyperlink to a specific codepoint, with optional tooltip containing codepoint glyph
    // Also called from CharDetailViewModel
    internal static Hyperlink GetCodepointHyperlink(int codepoint, ICommand command, bool withToolTip)
    {
        // \x2060 is Unicode Word Joiner, to prevent line wrap here
        var r = new Run($"U\x2060+\x2060{codepoint:X4}");
        var h = new Hyperlink(r)
        {
            Command = command,
            CommandParameter = codepoint
        };

        if (withToolTip)
        {
            var cps = UniData.AsString(codepoint);
            var tooltip = new ToolTip
            {
                Content = new Image { Source = D2DDrawText.GetBitmapSource(cps) },
                HorizontalOffset = 20,
                VerticalOffset = -50
            };
            h.ToolTip = tooltip;
        }

        return h;
    }

    // Returns a hyperlink to NewFilterCommand with commandParameter parameter, using content as text
    private TextBlock? GetBlockHyperlink(CharacterRecord? cr, string? content, string commandParameter)
    {
        if (cr == null || content == null) return null;

        var t1 = new TextBlock(new Run(cr.Block.RepresentantCharacter))
        {
            Style = Win.FindResource("ZoomableTB") as Style
        };

        var h = new Hyperlink(new Run(content))
        {
            Command = NewFilterCommand,
            CommandParameter = commandParameter
        };

        var tb = new TextBlock();
        tb.Inlines.Add(t1);
        tb.Inlines.Add(h);

        return tb;
    }

    // Returns a hyperlink to NewFilterCommand with commandParameter parameter, using content as text
    private TextBlock? GetSubheaderHyperlink(string? content, string commandParameter)
    {
        if (content == null) return null;

        var h = new Hyperlink(new Run(content))
        {
            Command = NewFilterCommand,
            CommandParameter = commandParameter
        };
        return new TextBlock(h);
    }

    public UIElement? BlockContent => GetBlockHyperlink(SelectedChar, SelectedChar?.Block.BlockNameAndRange, "b:\"" + SelectedChar?.Block.BlockName + "\"");

    public UIElement? SubheaderContent => GetSubheaderHyperlink(SelectedChar?.Subheader, "s:\"" + SelectedChar?.Subheader + "\"");

    // ==============================================================================================
    // Events processing

    // Called from Window
    internal void AfterCheckboxFlip()
    {
        RefreshSelBlocks();
        StartOrResetCharFilterDispatcherTimer();
    }

    private void RefreshSelBlocks() => SelBlocks = BlocksCheckableNodesDictionary.Values.Count(cn => (cn.IsChecked ?? false) && cn.Level == 0);

    // ==============================================================================================
    // Delay processing of TextChanged event 250ms using a DispatcherTimer

    private DispatcherTimer? DispatcherTimer;

    private void DispatcherTimer_Tick(object? sender, object e)
    {
        // Avoid a nullability warning
        if (DispatcherTimer == null)
            return;

        DispatcherTimer.Stop();
        DispatcherTimer.Tick -= DispatcherTimer_Tick;
        DispatcherTimer = null;

        FilterCharList();
    }

    // Temporarily public during transition to MVVM pattern
    public void StartOrResetCharFilterDispatcherTimer()
    {
        if (DispatcherTimer == null)
        {
            DispatcherTimer = new DispatcherTimer();
            DispatcherTimer.Tick += DispatcherTimer_Tick;
            DispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
        }
        else
            DispatcherTimer.Stop();

        DispatcherTimer.Start();
    }

    private void FilterCharList()
    {
        CollectionView view = (CollectionViewSource.GetDefaultView(CharactersRecordsList) as CollectionView)!;

        // Block part of the filtering predicate
        bool Bp(object o) => BlocksCheckableNodesDictionary[((CharacterRecord)o).BlockBegin].IsChecked ?? false;

        // Character part of the filtering predicate
        if (string.IsNullOrEmpty(CharNameFilter))
            view.Filter = Bp;
        else
        {
            // Create a new instance of FilterPredicateBuilder that prepares suitable predicate
            var cpb = new PredicateBuilder(CharNameFilter, false, false, false, false);
            view.Filter = (o) => Bp(o) && cpb.GetCharacterRecordFilter(o);
        }

        FilChars = view.Count;
    }

    private void ApplyBlockNameFilter()
    {
        //string s = BlockNameFilter;
        //var p = new PredicateBuilder(s, false, false, false, false);
        //Predicate<object> f = p.GetCheckableNodeFilter;

        FilterBlock(BlocksRoot);
        //RefreshFilBlocks();

        // Filtering Predicate
        bool Fp(CheckableNode bn) => string.IsNullOrEmpty(BlockNameFilter) || bn.Name.IndexOf(BlockNameFilter, 0, StringComparison.InvariantCultureIgnoreCase) >= 0;

        bool FilterBlock(CheckableNode n)
        {
            if (n.Level == 0)
                return Fp(n);
            else
            {
                bool exp = false;
                foreach (CheckableNode child in n.Children)
                    exp |= FilterBlock(child);
                exp |= Fp(n);
                n.IsNodeExpanded = exp;
                return exp;
            }
        }
    }

    // ==============================================================================================
    // Commands

    private bool CanCopyRecords(object obj)
    {
        // Trick to refresh selection count
        SelChars = Win.CharListView.SelectedItems.Count;

        return SelectedChar != null;
    }

    private void CopyRecordsExecute(object param)
    {
        var selectedCharRecords = Win.CharListView.SelectedItems.Cast<CharacterRecord>();
        DoCopyRecords(param.ToString() ?? "0", selectedCharRecords);
    }

    internal static void DoCopyRecords(string param, IEnumerable<CharacterRecord>? records)
    {
        if (records == null) return;
        var sb = new StringBuilder();
        if (param == "1")
            sb.AppendLine("Character\tCodepoint\tName");
        if (param == "2")
            sb.AppendLine("Character\tCodepoint\tName\tScript\tCategories\tAge\tBlock\tUTF16\tUTF8");
        foreach (CharacterRecord r in records.OrderBy(cr => cr.Codepoint))
            switch (param)
            {
                case "0":
                    sb.Append(r.Character);
                    break;

                case "1":
                    sb.AppendLine(r.Character + "\t" + r.CodepointHex + "\t" + r.Name);
                    break;

                case "2":
                    sb.AppendLine(r.Character + "\t" + r.CodepointHex + "\t" + r.Name + "\t" + r.Script + "\t" + r.CategoryRecord.Categories + "\t" + r.Age + "\t" + r.Block.BlockNameAndRange + "\t" + r.UTF16 + "\t" + r.UTF8);
                    break;

                case "3":
                    sb.AppendLine("Character");
                    sb.AppendLine("\tChar\t" + r.Character);
                    sb.AppendLine("\tCodepoint\t" + r.CodepointHex);
                    sb.AppendLine("\tName\t" + r.Name);
                    sb.AppendLine("\tScript\t" + r.Script);
                    sb.AppendLine("\tCategories\t" + r.CategoryRecord.Categories);
                    sb.AppendLine("\tSince\t" + r.Age);
                    sb.AppendLine("Block");
                    sb.AppendLine("\tLevel 3\t" + r.Block.Level3Name);
                    sb.AppendLine("\tLevel 2\t" + r.Block.Level2Name);
                    sb.AppendLine("\tLevel 1\t" + r.Block.Level1Name);
                    sb.AppendLine("\tBlock\t" + r.Block.BlockNameAndRange);
                    sb.AppendLine("\tSubheader\t" + r.Subheader);
                    sb.AppendLine("Encoding");
                    sb.AppendLine("\tUTF-8\t" + r.UTF8);
                    sb.AppendLine("\tUTF-16\t" + r.UTF16);
                    sb.AppendLine("Decomposition and Case");
                    sb.AppendLine("\tNFD");
                    AppendNormalizationContent(sb, r, NormalizationForm.FormD);
                    sb.AppendLine("\tNFKD");
                    AppendNormalizationContent(sb, r, NormalizationForm.FormKD);
                    sb.AppendLine("\tLowercase");
                    AppendCaseContent(sb, r, true);
                    sb.AppendLine("\tUppercase");
                    AppendCaseContent(sb, r, false);
                    sb.AppendLine("Extra Information");
                    sb.AppendLine("\tSynonyms");
                    if (r.Synonyms != null)
                        foreach (string line in r.Synonyms)
                            sb.AppendLine("\t\t" + line);
                    sb.AppendLine("\tCross-Refs");
                    if (r.CrossRefs != null)
                        foreach (string line in r.CrossRefs)
                            sb.AppendLine("\t\t" + line);
                    sb.AppendLine("\tComments");
                    if (r.Comments != null)
                        foreach (string line in r.Comments)
                            sb.AppendLine("\t\t" + line);
                    sb.AppendLine();
                    break;
            }
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

    private static void AppendNormalizationContent(StringBuilder sb, CharacterRecord selectedChar, NormalizationForm form)
    {
        if (!selectedChar.IsPrintable) return;

        string s = selectedChar.Character;
        string sn = s.Normalize(form);
        if (s == sn) return;
        if (form == NormalizationForm.FormKD && sn == s.Normalize(NormalizationForm.FormD))
        {
            sb.AppendLine("\t\tSame as NFD");
            return;
        }
        foreach (var cr in sn.EnumCharacterRecords())
            sb.AppendLine("\t\t" + cr.AsDetailedString);
    }

    private static void AppendCaseContent(StringBuilder sb, CharacterRecord selectedChar, bool lower)
    {
        if (!selectedChar.IsPrintable) return;

        string s = selectedChar.Character;
        string sc = lower ? s.ToLowerInvariant() : s.ToUpperInvariant();
        if (s == sc) return;

        foreach (var cr in sc.EnumCharacterRecords())
            sb.AppendLine("\t\t" + cr.AsDetailedString);
    }

    private void CopyImageExecute(object param)
    {
        if (SelectedChar != null)
            try
            {
                Clipboard.Clear();
                var sci = SelectedCharImage;
                if (sci != null)
                    Clipboard.SetImage(sci);
            }
            catch (Exception)
            {

            }
    }

    // Helper, performs an action on a node and all its decendants
    static void ActionAllNodes(CheckableNode n, Action<CheckableNode> a)
    {
        a(n);
        foreach (CheckableNode child in n.Children)
            ActionAllNodes(child, a);
    }

    private void ShowLevelExecute(object? param)
    {
        if (param != null && int.TryParse((string)param, out int level))
            ActionAllNodes(BlocksRoot, n => n.IsNodeExpanded = n.Level != level);
    }

    private void SelectAllExecute(string sparam)
    {
        _ = int.TryParse(sparam, out int param);
        ActionAllNodes(BlocksRoot, n => n.IsChecked = param == 1);
        RefreshSelBlocks();
        FilterCharList();
    }

    // From hyperlink
    private void ShowDetailExecute(int codepoint) =>
        CharDetailWindow.ShowDetail(codepoint, this);

    // From hyperlink
    private void NewFilterExecute(string filter) =>
        CharNameFilter = filter;

}