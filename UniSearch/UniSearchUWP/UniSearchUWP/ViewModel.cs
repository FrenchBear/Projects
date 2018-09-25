// UniSearch ViewModel
// Interaction and commands support
//
// 2018-12-09   PV

using RelayCommandNS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UniDataNS;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace UniSearchUWPNS
{
    internal class ViewModel : INotifyPropertyChanged
    {
        // Private variables
        private readonly SearchPage page;                               // Access to main window
        private readonly BlockNode BlocksRoot;                          // TreeView root
        private HashSet<BlockRecord> SelectedBlocksSet = new HashSet<BlockRecord>();    // Set of selected blocks in TreeView

        // INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string propertyName)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Commands public interface
        public ICommand CopyRecordsCommand { get; private set; }

        public ICommand AboutCommand { get; private set; }
        public ICommand ShowLevelCommand { get; private set; }
        public ICommand ShowDetailCommand { get; private set; }

        // Constructor
        public ViewModel(SearchPage p)
        {
            page = p;

            // Binding commands with behavior
            CopyRecordsCommand = new RelayCommand<object>(CopyRecordsExecute, CanCopyRecords);
            AboutCommand = new AwaitableRelayCommand<object>(AboutExecute);
            ShowLevelCommand = new RelayCommand<object>(ShowLevelExecute);
            ShowDetailCommand = new AwaitableRelayCommand<int>(ShowDetailExecute);

            // Get Unicode data
            CharactersRecordsList = UniData.CharacterRecords.Values.OrderBy(cr => cr.Codepoint).ToArray();
            BlocksRecordsList = UniData.BlockRecords.Values.OrderBy(br => br.Begin).ToArray();

            // root is *not* added to the TreeView on purpose
            BlocksRoot = new BlockNode("root", 4);
            // Dictionary of BlockNodes indexed by Begin block value, to help uncheck some blocks at the end
            var BlocksBlockNodesDictionary = new Dictionary<int, BlockNode>();

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

            // ToDo: Do it once Windows 10 oct 2018 is available.  Build 1803 does not let change selection by program.

            /*
            // Unselect some pretty useless blocks
            BlocksBlockNodesDictionary[0x0000].IsChecked = false;       // ASCII Controls C0
            BlocksBlockNodesDictionary[0x0080].IsChecked = false;       // ASCII Controls C1
            BlocksBlockNodesDictionary[0xE0100].IsChecked = false;      // Variation Selectors Supplement; Variation Selectors; Specials; Symbols and Punctuation
            BlocksBlockNodesDictionary[0xF0000].IsChecked = false;      // Supplementary Private Use Area-A; ; Private Use; Symbols and Punctuation
            BlocksBlockNodesDictionary[0x100000].IsChecked = false;     // Supplementary Private Use Area-B; ; Private Use; Symbols and Punctuation
            ActionAllNodes(BlocksRoot, n =>
            {
                if (n.Name == "East Asian Scripts") n.IsChecked = false;
                if (n.Name.IndexOf("CJK", StringComparison.Ordinal) >= 0) n.IsChecked = false;
                if (n.Name == "Specials") n.IsChecked = false;
            });

            BlocksBlockNodesDictionary[0xD800].IsChecked = false;       //  High Surrogates; ; Surrogates; Symbols and Punctuation
            BlocksBlockNodesDictionary[0xDB80].IsChecked = false;       //  High Private Use Surrogates; ; Surrogates; Symbols and Punctuation
            BlocksBlockNodesDictionary[0xDC00].IsChecked = false;       //  Low Surrogates; ; Surrogates; Symbols and Punctuation
            BlocksBlockNodesDictionary[0xE000].IsChecked = false;       //  Private Use Area; ; Private Use; Symbols and Punctuation
            */
        }

        // ==============================================================================================
        // Bindable properties

        // ToDo: Property needed?
        public CharacterRecord[] CharactersRecordsList { get; set; }

        // Source of CharListView and used to build the grouped version
        public ObservableCollection<CharacterRecord> CharactersRecordsFilteredList { get; set; } = new ObservableCollection<CharacterRecord>();

        private CollectionViewSource _CharactersRecordsCVS = new CollectionViewSource();
        public CollectionViewSource CharactersRecordsCVS => _CharactersRecordsCVS;

        public List<BlockNode> NodesList { get; set; }

        public BlockRecord[] BlocksRecordsList { get; set; }

        private string _CharNameFilter;

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

        private string _BlockNameFilter;

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

        private CharacterRecord _SelectedChar;

        public CharacterRecord SelectedChar
        {
            get { return _SelectedChar; }
            set
            {
                if (_SelectedChar != value)
                {
                    _SelectedChar = value;
                    NotifyPropertyChanged(nameof(SelectedChar));
                    NotifyPropertyChanged(nameof(StrContent));
                    CopyRecordsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public static int NumChars => UniData.CharacterRecords.Count;

        public static int NumBlocks => UniData.BlockRecords.Count;

        public int SelChars => page.CharCurrentView.SelectedItems.Count;

        public int FilChars => CharactersRecordsFilteredList.Count;

        public int FilBlocks => SelectedBlocksSet.Count;

        public UIElement StrContent
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
                Margin = new Thickness(0),
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

        // ==============================================================================================
        // Delay processing of TextChanged event 250ms using a DispatcherTimer

        private DispatcherTimer dispatcherTimer;
        private readonly object dispatcherTimerLock = new object();

        private void DispatcherTimer_Tick(object sender, object e)
        {
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
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
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

            //CharactersRecordsFilteredList.Clear();
            //foreach (CharacterRecord c in CharactersRecordsList.Where(cr => p2(cr)))
            //    CharactersRecordsFilteredList.Add(c);

            CharactersRecordsFilteredList = new ObservableCollection<CharacterRecord>(CharactersRecordsList.Where(cr => p2(cr)));

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
                var b = (item as BlockNode).Block;
                if (b != null)
                    newSelectedBlocksSet.Add((item as BlockNode).Block);
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

        private bool CanCopyRecords(object obj) => SelectedChar != null;

        private void CopyRecordsExecute(object param)
        {
            var selectedCharRecords = page.CharCurrentView.SelectedItems.Cast<CharacterRecord>();

            var sb = new StringBuilder();
            foreach (CharacterRecord r in selectedCharRecords.OrderBy(cr => cr.Codepoint))
                switch (param)
                {
                    case "0":
                        sb.Append(r.Character);
                        break;

                    case "1":
                        sb.AppendLine(r.Character + "\t" + r.CodepointHex + "\t" + r.Name);
                        break;

                    case "2":
                        sb.AppendLine(r.Character + "\t" + r.CodepointHex + "\t" + r.Name + "\t" +
                            r.CategoryRecord.Categories + "\t" + r.Age + "\t" + r.Block.BlockNameAndRange + "\t" +
                            r.Subheader + "\t" + r.UTF16 + "\t" + r.UTF8);
                        break;
                }

            ClipboardSetText(sb.ToString());
        }

        // Convenient helper
        internal static void ClipboardSetText(string s)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(s);
            try
            {
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error copying text into clipboard: " + ex.Message);
            }

        }

        // Show app information
        private async Task AboutExecute(object param)
        {
            Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            AssemblyTitleAttribute aTitleAttr = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyTitleAttribute));
            string sAssemblyVersion = myAssembly.GetName().Version.Major.ToString() + "." + myAssembly.GetName().Version.Minor.ToString() + "." + myAssembly.GetName().Version.Build.ToString();
            AssemblyDescriptionAttribute aDescAttr = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyDescriptionAttribute));
            AssemblyCopyrightAttribute aCopyrightAttr = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyCopyrightAttribute));
            AssemblyProductAttribute aProductAttr = (AssemblyProductAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyProductAttribute));

            string s = aTitleAttr.Title + " version " + sAssemblyVersion + "\r\n" + aDescAttr.Description + "\r\n\n" + aProductAttr.Product + "\r\n" + aCopyrightAttr.Copyright;

            var dialog = new MessageDialog(s, "About " + aTitleAttr.Title);
            await dialog.ShowAsync();
        }

        // From Hyperlink
        private async Task ShowDetailExecute(int codepoint)
        {
            await CharDetailDialog.ShowDetail(codepoint);
        }

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