// UniSearch ViewModel
// Interaction and commands support
//
// 2018-09-12   PV
// 2018-10-08   PV      v1.2 CopyImage (finally!)
// 2020-11-11   PV      nullable enable


using RelayCommandNS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UniDataNS;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;

#nullable enable


namespace UniSearchNS
{
    internal class ViewModel : INotifyPropertyChanged
    {
        // Private variables
        private readonly SearchPage page;                               // Access to main window

        private readonly BlockNode BlocksRoot;                          // TreeView root
        private HashSet<BlockRecord> SelectedBlocksSet = new HashSet<BlockRecord>();    // Set of selected blocks in TreeView


        // INotifyPropertyChanged interface
        public event PropertyChangedEventHandler? PropertyChanged;

        internal void NotifyPropertyChanged(string propertyName)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        // Commands public interface
        public ICommand CopyRecordsCommand { get; private set; }
        public ICommand CopyImageCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }
        public ICommand ShowLevelCommand { get; private set; }
        public ICommand ShowDetailCommand { get; private set; }
        public ICommand NewFilterCommand { get; private set; }


        // Dictionary of BlockNodes indexed by Begin block value, to help uncheck some blocks at the end
        private readonly Dictionary<int, BlockNode> BlocksBlockNodesDictionary = new Dictionary<int, BlockNode>();


        // Constructor
        public ViewModel(SearchPage p)
        {
            page = p;

            // Binding commands with behavior
            CopyRecordsCommand = new RelayCommand<object>(CopyRecordsExecute, CanCopy);
            CopyImageCommand = new AwaitableRelayCommand<object>(CopyImageExecute, CanCopy);
            AboutCommand = new AwaitableRelayCommand<object>(AboutExecute);
            ShowLevelCommand = new RelayCommand<object>(ShowLevelExecute);
            ShowDetailCommand = new AwaitableRelayCommand<int>(ShowDetailExecute);
            NewFilterCommand = new RelayCommand<string>(NewFilterExecute);

            // Get Unicode data
            CharactersRecordsList = UniData.CharacterRecords.Values.OrderBy(cr => cr.Codepoint).ToArray();
            BlocksRecordsList = UniData.BlockRecords.Values.OrderBy(br => br.Begin).ToArray();

            // root is *not* added to the TreeView on purpose
            BlocksRoot = new BlockNode("root", 4);

            NodesList = new List<BlockNode>();

            foreach (var l3 in BlocksRecordsList.GroupBy(b => b.Level3Name).OrderBy(g => g.Key))
            {
                var r3 = new BlockNode(l3.Key, 3);
                BlocksRoot.Children.Add(r3);
                NodesList.Add(r3);
                foreach (var l2 in l3.GroupBy(b => b.Level2Name))
                {
                    var r2 = new BlockNode(l2.Key, 2);
                    r3.Children.Add(r2);
                    NodesList.Add(r2);
                    foreach (var l1 in l2.GroupBy(b => b.Level1Name))
                    {
                        // Special case, l1 can be empty
                        BlockNode r1;
                        if (l1.Key.Length > 0)
                        {
                            r1 = new BlockNode(l1.Key, 1);
                            r2.Children.Add(r1);
                            NodesList.Add(r1);
                        }
                        else
                            r1 = r2;
                        // Blocks
                        foreach (var b in l1)
                        {
                            var blockBlockNode = new BlockNode(b.BlockName, 0, b);
                            r1.Children.Add(blockBlockNode);
                            BlocksBlockNodesDictionary.Add(b.Begin, blockBlockNode);
                            NodesList.Add(blockBlockNode);
                        }
                    }
                }
            }

            // Current version of TreeView does not support binding
            foreach (var item in BlocksRoot.Children)
                page.BlocksTreeView.RootNodes.Add(item);
        }


        internal void InitialBlocksUnselect()
        {
            // Unselect some pretty useless blocks
            UnselectBlock(0x0000);       // ASCII Controls C0
            UnselectBlock(0x0080);       // ASCII Controls C1
            UnselectBlock(0xE0100);      // Variation Selectors Supplement; Variation Selectors; Specials; Symbols and Punctuation
            UnselectBlock(0xF0000);      // Supplementary Private Use Area-A; ; Private Use; Symbols and Punctuation
            UnselectBlock(0x100000);     // Supplementary Private Use Area-B; ; Private Use; Symbols and Punctuation
            UnselectBlock(0xD800);       // High Surrogates; ; Surrogates; Symbols and Punctuation
            UnselectBlock(0xDB80);       // High Private Use Surrogates; ; Surrogates; Symbols and Punctuation
            UnselectBlock(0xDC00);       // Low Surrogates; ; Surrogates; Symbols and Punctuation
            UnselectBlock(0xE000);       // Private Use Area; ; Private Use; Symbols and Punctuation

            // Unselect blocks using string match in hierarchy of groups
            UnselectName(BlocksRoot, "East Asian Scripts", false);
            UnselectName(BlocksRoot, "CJK", false);
            UnselectName(BlocksRoot, "Specials", false);

            void UnselectName(BlockNode bn, string name, bool apply)
            {
                if (bn.Level == 0)
                {
                    if (apply && page.BlocksTreeView.SelectedNodes.Contains(bn as TreeViewNode))
                        page.BlocksTreeView.SelectedNodes.Remove(bn as TreeViewNode);
                }
                else
                {
                    apply |= bn.Name.IndexOf(name, StringComparison.Ordinal) >= 0;
                    foreach (var child in bn.Children)
                        UnselectName((child as BlockNode)!, name, apply);
                }
            }


            void UnselectBlock(int blockBegin)
            {
                var tn = BlocksBlockNodesDictionary[blockBegin] as TreeViewNode;
                if (page.BlocksTreeView.SelectedNodes.Contains(tn))
                    page.BlocksTreeView.SelectedNodes.Remove(tn);
            }
        }


        // ==============================================================================================
        // Bindable properties

        // Permanent list of CharacterRecords returned by UniData
        public CharacterRecord[] CharactersRecordsList;

        // Source of CharListView and used to build the grouped version
        public ObservableCollection<CharacterRecord> CharactersRecordsFilteredList { get; set; } = new ObservableCollection<CharacterRecord>();

        private readonly CollectionViewSource _CharactersRecordsCVS = new CollectionViewSource();
        public CollectionViewSource CharactersRecordsCVS => _CharactersRecordsCVS;

        public List<BlockNode> NodesList { get; set; }

        public BlockRecord[] BlocksRecordsList { get; set; }

        private string _CharNameFilter = "";

        public string CharNameFilter
        {
            get { return _CharNameFilter; }
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
            get { return _BlockNameFilter; }
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
            get { return _SelectedChar; }
            set
            {
                if (_SelectedChar != value)
                {
                    _SelectedChar = value;
                    NotifyPropertyChanged(nameof(SelectedChar));
                    NotifyPropertyChanged(nameof(StrContent));
                    NotifyPropertyChanged(nameof(BlockContent));
                    NotifyPropertyChanged(nameof(SubheaderContent));
                    // In a UWP app, it's done manually...
                    CopyRecordsCommand.RaiseCanExecuteChanged();
                    CopyImageCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public static int NumChars => UniData.CharacterRecords.Count;

        public static int NumBlocks => UniData.BlockRecords.Count;

        public int SelChars => page.CharCurrentView.SelectedItems.Count;

        public int FilChars => CharactersRecordsFilteredList.Count;

        public int FilBlocks => SelectedBlocksSet.Count;

        public UIElement? StrContent
        {
            get
            {
                if (SelectedChar == null) return null;
                return GetStrContent(SelectedChar.Codepoint, ShowDetailCommand);
            }
        }

        // Returns a grid containing char information (character itself, codepoint, name) and an hyperlink on
        // CodePoint that executes Command
        public static UIElement GetStrContent(int codepoint, ICommand command)
        {
            var cr = UniData.CharacterRecords[codepoint];

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(70) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            var t1 = new TextBlock { Text = cr.Character };
            Grid.SetColumn(t1, 0);
            g.Children.Add(t1);

            HyperlinkButton t2 = new HyperlinkButton
            {
                Margin = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0),
                Content = cr.CodepointHex,
                Command = command,
                CommandParameter = codepoint
            };

            Grid.SetColumn(t2, 1);
            g.Children.Add(t2);

            var t3 = new TextBlock { Text = cr.Name };
            Grid.SetColumn(t3, 2);
            g.Children.Add(t3);

            return g;
        }

        // Returns a hyperlink to NewFilterCommand with commandParameter parameter, using content as text
        private UIElement? GetBlockHyperlink(string? content, string commandParameter)
        {
            if (content == null) return null;

            return new HyperlinkButton
            {
                Margin = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0),
                Content = content,
                Command = NewFilterCommand,
                CommandParameter = commandParameter
            };
        }

        public UIElement? BlockContent => GetBlockHyperlink(SelectedChar?.Block.BlockNameAndRange, "b:\"" + SelectedChar?.Block.BlockName + "\"");

        public UIElement? SubheaderContent => GetBlockHyperlink(SelectedChar?.Subheader, "s:\"" + SelectedChar?.Subheader + "\"");


        // ==============================================================================================
        // Delay processing of TextChanged event 250ms using a DispatcherTimer

        private DispatcherTimer? dispatcherTimer;
        private readonly object dispatcherTimerLock = new object();

        private void DispatcherTimer_Tick(object sender, object e)
        {
            // Avoid a nullability warning
            if (dispatcherTimer == null)
                return;

            lock (dispatcherTimerLock)
            {
                dispatcherTimer.Stop();
                dispatcherTimer.Tick -= DispatcherTimer_Tick;
                dispatcherTimer = null;
            }

            FilterCharList();
        }

        // Temporarily public during transition to MVVM pattern
        public void StartOrResetCharFilterDispatcherTimer()
        {
            lock (dispatcherTimerLock)
            {
                if (dispatcherTimer == null)
                {
                    dispatcherTimer = new DispatcherTimer();
                    dispatcherTimer.Tick += DispatcherTimer_Tick;
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 350);
                }
                else
                    dispatcherTimer.Stop();

                dispatcherTimer.Start();
            }
        }

        internal void FilterCharList()
        {
            // Just in case there is a direct call while a timer is pending
            lock (dispatcherTimerLock)
                if (dispatcherTimer != null)
                {
                    dispatcherTimer.Stop();
                    dispatcherTimer.Tick -= DispatcherTimer_Tick;
                    dispatcherTimer = null;
                }

            // Block part of the filtering predicate
            bool p1(CharacterRecord cr) => SelectedBlocksSet.Contains(cr.Block);
            Predicate<CharacterRecord> p2;

            // Character part of the filtering predicate
            if (!string.IsNullOrEmpty(CharNameFilter))
            {
                // Create a new instance of FilterPredicateBuilder that prepares suitable predicate
                var cpb = new PredicateBuilder(CharNameFilter, false, false, false, false);
                p2 = (cr) => p1(cr) && cpb.GetCharacterRecordFilter(cr);
            }
            else
                p2 = p1;

            var SavedSelectedChar = SelectedChar;

            // Build filtered list, used directly by ListView and to build grouped version for GridView
            CharactersRecordsFilteredList.Clear();
            foreach (CharacterRecord c in CharactersRecordsList.Where(cr => p2(cr)).OrderBy(cr => cr.Block.Rank))
                CharactersRecordsFilteredList.Add(c);

            // Build the grouped version
            var groups = CharactersRecordsFilteredList.GroupBy(cr => new BSHGroupKey(cr.BlockBegin, cr.Subheader), new GroupKeyComparer()).OrderBy(grp => UniData.BlockRecords[grp.Key.BlockBegin].Rank).ToList();
            int lastbb = 0;
            foreach (var group in groups)
            {
                BSHGroupKey k = group.Key;
                if (k.BlockBegin != lastbb)
                {
                    k.NewBlock = true;
                    lastbb = k.BlockBegin;
                }
            }
            var g0 = groups.FirstOrDefault();
            if (g0 != null) g0.Key.NewBlock = false;

            _CharactersRecordsCVS.Source = groups;
            _CharactersRecordsCVS.IsSourceGrouped = true;
            //NotifyPropertyChanged(nameof(CharactersRecordsCVS));

            // Restore selected char if it's still part of filtered list
            if (SavedSelectedChar != null && CharactersRecordsFilteredList.Contains(SavedSelectedChar))
            {
                SelectedChar = SavedSelectedChar;
                page.CharCurrentView.ScrollIntoView(SelectedChar);
            }

            // Refresh filtered count
            NotifyPropertyChanged(nameof(FilChars));
        }

        // Block and subheader group
        private class BSHGroupKey
        {
            public int BlockBegin { get; set; }
            public string Subheader { get; set; }
            public bool NewBlock { get; set; }

            internal BSHGroupKey(int blockBegin, string subheader)
            {
                BlockBegin = blockBegin;
                Subheader = subheader;
            }

            // Used by ZoomIn semantic view
            public UIElement Content
            {
                get
                {
                    var tb = new TextBlock();
                    if (NewBlock) tb.Inlines.Add(new LineBreak());
                    var br = new Run { Text = UniData.BlockRecords[BlockBegin].BlockName, FontWeight = FontWeights.SemiBold };
                    tb.Inlines.Add(br);
                    if (!string.IsNullOrEmpty(Subheader))
                    {
                        var sr = new Run { Text = ": " + Subheader };
                        tb.Inlines.Add(sr);
                    }
                    return tb;
                }
            }

            // Used by ZoomOut semantic view
            public override string ToString() => (NewBlock ? "\r\n" : "") + UniData.BlockRecords[BlockBegin].BlockName + (string.IsNullOrEmpty(Subheader) ? "" : ": " + Subheader);
        }

        private class GroupKeyComparer : IEqualityComparer<BSHGroupKey>
        {
            public bool Equals(BSHGroupKey x, BSHGroupKey y) => x.BlockBegin == y.BlockBegin && x.Subheader == y.Subheader;

            public int GetHashCode(BSHGroupKey obj)
            {
                if (obj.Subheader == null)
                    return obj.BlockBegin.GetHashCode();
                else
                    return obj.BlockBegin.GetHashCode() ^ obj.Subheader.GetHashCode(StringComparison.Ordinal);
            }
        }

        // When BlockNameFilter has changed, collapse/expand the nodes
        internal void ApplyBlockNameFilter()
        {
            FilterBlock(BlocksRoot);

            // Filtering Predicate
            bool fp(BlockNode bn) => String.IsNullOrEmpty(BlockNameFilter) || bn.Name.IndexOf(BlockNameFilter, 0, StringComparison.InvariantCultureIgnoreCase) >= 0;

            bool FilterBlock(BlockNode n)
            {
                if (n.Level == 0)
                    return fp(n);
                else
                {
                    bool exp = false;
                    foreach (BlockNode child in n.Children)
                        exp |= FilterBlock(child);
                    exp |= fp(n);
                    n.IsExpanded = exp;
                    return exp;
                }
            }
        }

        // After a change in Blocks selection, rebuilds SelectedBlocksSet and calls FilterCharList()
        internal void RefreshSelectedBlocks(bool immediateCharFilter = false)
        {
            var newSelectedBlocksSet = new HashSet<BlockRecord>();
            foreach (var item in page.BlocksTreeView.SelectedNodes)
            {
                if (item is BlockNode bn)
                    if (bn.Block != null)
                        newSelectedBlocksSet.Add(bn.Block);
            }

            // Optimization: Don't filter chars if blocks selection has not changed and filtering
            // has only expanded/closed nodes in TreeeView
            if (newSelectedBlocksSet.SetEquals(SelectedBlocksSet))
            {
                //Debug.WriteLine("FilterBlockTree: No change in SelectedBlocks");
                return;
            }

            SelectedBlocksSet = newSelectedBlocksSet;
            //Debug.WriteLine($"Filtered blocks: {SelectedBlocksSet.Count}");
            NotifyPropertyChanged(nameof(FilBlocks));

            if (immediateCharFilter)
                FilterCharList();
            else
                StartOrResetCharFilterDispatcherTimer();
        }


        // ==============================================================================================
        // Commands

        private bool CanCopy(object obj) => SelectedChar != null;

        private void CopyRecordsExecute(object param)
        {
            var selectedCharRecords = page.CharCurrentView.SelectedItems.Cast<CharacterRecord>();
            DoCopyRecords(param?.ToString() ?? "0", selectedCharRecords);
        }

        internal static void DoCopyRecords(string param, IEnumerable<CharacterRecord>? records)
        { 
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
                        sb.AppendLine(r.Character + "\t" + r.CodepointHex + "\t" + r.Name + "\t" + r.Script + "\t" +
                            r.CategoryRecord.Categories + "\t" + r.Age + "\t" + r.Block.BlockNameAndRange + "\t" +
                            r.Subheader + "\t" + r.UTF16 + "\t" + r.UTF8);
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

            ClipboardSetData(sb.ToString());
        }

        private static void AppendNormalizationContent(StringBuilder sb, CharacterRecord SelectedChar, NormalizationForm form)
        {
            if (!SelectedChar.IsPrintable) return;

            string s = SelectedChar.Character;
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

        private static void AppendCaseContent(StringBuilder sb, CharacterRecord SelectedChar, bool lower)
        {
            if (!SelectedChar.IsPrintable) return;

            string s = SelectedChar.Character;
            string sc = lower ? s.ToLowerInvariant() : s.ToUpperInvariant();
            if (s == sc) return;

            foreach (var cr in sc.EnumCharacterRecords())
                sb.AppendLine("\t\t" + cr.AsDetailedString);
        }


        private async Task CopyImageExecute(object obj)
        {
            // Get RenderTargetBitmap of character image
            var control = page.CharImageBorder;
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(control); //, (int)control.Width, (int)control.Height);

            // Get the pixels BGRA8-format.
            // IBuffer represents a referenced array of bytes used by byte stream read and write interfaces.
            IBuffer pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            int width = renderTargetBitmap.PixelWidth;
            int height = renderTargetBitmap.PixelHeight;

            // Convert IBuffer in a RandomAccessStreamReference (not easy to find!!!)
            var rasr = await CopyImageUsingMemoryStream(pixelBuffer, width, height);

            ClipboardSetData(null, rasr);
        }


        private static InMemoryRandomAccessStream? imas;

        // Copy image using a stream provided by InMemoryRandomAccessStream
        // Key point: declare ma at class level to prevent GC destruction
        internal async static Task<RandomAccessStreamReference> CopyImageUsingMemoryStream(IBuffer pixelBuffer, int width, int height)
        {
            byte[] pixelArray = pixelBuffer.ToArray();

            imas = new InMemoryRandomAccessStream();
            BitmapEncoder be = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, imas);
            be.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)width, (uint)height, 96, 96, pixelArray);
            await be.FlushAsync();
            return RandomAccessStreamReference.CreateFromStream(imas);
        }


        // Convenient helper
        internal static void ClipboardSetData(string? s, RandomAccessStreamReference? bmp = null)
        {
            var dataPackage = new DataPackage();
            if (s != null) dataPackage.SetText(s);
            if (bmp != null) dataPackage.SetBitmap(bmp);
            try
            {
                Clipboard.SetContent(dataPackage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error copying text into clipboard: " + ex.Message);
            }
        }



        // Show app information
        private async Task AboutExecute(object param) =>
            await AboutDialog.ShowAbout();

        // From Hyperlink
        private async Task ShowDetailExecute(int codepoint) =>
            await CharDetailDialog.ShowDetail(codepoint, this);

        // From hyperlink
        private void NewFilterExecute(string filter) =>
            CharNameFilter = filter;

        // Helper performing a given action on a node and all its descendants
        private void ActionAllNodes(BlockNode n, Action<BlockNode> a)
        {
            a(n);
            foreach (BlockNode child in n.Children)
                ActionAllNodes(child, a);
        }

        private void ShowLevelExecute(object param)
        {
            int level = int.Parse(param as string);
            ActionAllNodes(BlocksRoot, n => { n.IsExpanded = (n.Level != level); });
        }
    }
}