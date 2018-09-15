// UniSearch ViewModel
// 2018-12-09   PV

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using UniData;
using System.Windows.Media.Imaging;
using DirectDrawWrite;

namespace UniSearchNS
{
    internal class ViewModel : INotifyPropertyChanged
    {
        // Private variables
        private readonly SearchWindow window;       // Access to main window
        private Dictionary<int, CheckableNode> BlocksCheckableNodesDictionary;

        private CheckableNode root;


        // INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        // Commands public interface
        public ICommand CopyCharsCommand { get; private set; }
        public ICommand CopyLinesCommand { get; private set; }
        public ICommand ShowLevelCommand { get; private set; }
        public ICommand FlipVisibleCommand { get; private set; }


        // Constructor
        public ViewModel(SearchWindow w)
        {
            window = w;

            // Binding commands with behavior
            CopyCharsCommand = new RelayCommand<object>(CopyCharsExecute, CanCopyChars);
            CopyLinesCommand = new RelayCommand<object>(CopyLinesExecute, CanCopyLines);
            ShowLevelCommand = new RelayCommand<object>(ShowLevelExecute);
            FlipVisibleCommand = new RelayCommand<object>(FlipVisibleExecute);


            // Get Unicode data
            CharactersRecordsList = UnicodeData.CharacterRecords.Values.OrderBy(cr => cr.Codepoint).ToArray();
            ReadOnlyDictionary<int, BlockRecord>.ValueCollection BlockRecordsList = UnicodeData.BlockRecords.Values;


            // root is *not* added to the TreeView on purpose
            root = new CheckableNode("root", 4);
            // Dictionary of CheckableNodes indexed by Begin block value
            BlocksCheckableNodesDictionary = new Dictionary<int, CheckableNode>();

            foreach (var l3 in BlockRecordsList.GroupBy(b => b.Level3Name).OrderBy(g => g.Key))
            {
                var r3 = new CheckableNode(l3.Key, 3);
                root.Children.Add(r3);
                foreach (var l2 in l3.GroupBy(b => b.Level2Name))
                {
                    var r2 = new CheckableNode(l2.Key, 2);
                    r3.Children.Add(r2);
                    foreach (var l1 in l2.GroupBy(b => b.Level1Name))
                    {
                        // Special case, l1 can be empty
                        CheckableNode r1;
                        if (l1.Key.Length > 0)
                        {
                            r1 = new CheckableNode(l1.Key, 1);
                            r2.Children.Add(r1);
                        }
                        else
                            r1 = r2;
                        // Blocks
                        foreach (var l0 in l1)
                        {
                            var blockCheckableNode = new CheckableNode(l0.BlockName, 0);
                            r1.Children.Add(blockCheckableNode);
                            BlocksCheckableNodesDictionary.Add(l0.Begin, blockCheckableNode);
                        }
                    }
                }
            }

            root.Initialize();
            root.IsChecked = true;
            roots = root.Children;

            // Unselect some pretty useless blocks
            // ToDo: Maybe UniData should not even load the supplementary private areas?
            // ToDo: Unselect East Asian scripts, but need support to access L2 block
            BlocksCheckableNodesDictionary[0x0000].IsChecked = false;       // ASCII Controls C0
            BlocksCheckableNodesDictionary[0x0080].IsChecked = false;       // ASCII Controls C1
            BlocksCheckableNodesDictionary[0xE0100].IsChecked = false;      // Variation Selectors Supplement; Variation Selectors; Specials; Symbols and Punctuation
            BlocksCheckableNodesDictionary[0xF0000].IsChecked = false;      // Supplementary Private Use Area-A; ; Private Use; Symbols and Punctuation
            BlocksCheckableNodesDictionary[0x100000].IsChecked = false;     // Supplementary Private Use Area-B; ; Private Use; Symbols and Punctuation

            BlocksCheckableNodesDictionary[0xD800].IsChecked = false;       //  High Surrogates; ; Surrogates; Symbols and Punctuation
            BlocksCheckableNodesDictionary[0xDB80].IsChecked = false;       //  High Private Use Surrogates; ; Surrogates; Symbols and Punctuation
            BlocksCheckableNodesDictionary[0xDC00].IsChecked = false;       //  Low Surrogates; ; Surrogates; Symbols and Punctuation
            BlocksCheckableNodesDictionary[0xE000].IsChecked = false;       //  Private Use Area; ; Private Use; Symbols and Punctuation

            // To compute bound values based on initial blocks filtering
            FilterCharList();
            RefreshSelBlocks();
            RefreshFilBlocks();
        }


        // Bindable properties
        public List<CheckableNode> roots { get; set; }      // For TreeView binding
        public CharacterRecord[] CharactersRecordsList { get; set; }


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
                    StartOrResetDispatcherTimer();
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
                    FilterBlockList();
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
                    NotifyPropertyChanged(nameof(SelectedCharImage));
                }
            }
        }

        public BitmapSource SelectedCharImage
        {
            get
            {
                if (SelectedChar == null)
                    return null;
                else
                {
                    string s = SelectedChar.Character;
                    if (s.StartsWith("U+", StringComparison.Ordinal))
                        s = "U+ " + s.Substring(2);
                    return D2DDrawText.GetBitmapSource(s);
                }
            }
        }

        public int NumChars => UnicodeData.CharacterRecords.Count;

        public int NumBlocks => BlocksCheckableNodesDictionary.Count;


        private int _SelChars;
        public int SelChars
        {
            get { return _SelChars; }
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
            get { return _FilChars; }
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
            get { return _SelBlocks; }
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
            get { return _FilBlocks; }
            set
            {
                if (_FilBlocks != value)
                {
                    _FilBlocks = value;
                    NotifyPropertyChanged(nameof(FilBlocks));
                }
            }
        }



        // Events processing

        // Called from Window
        // ToDo: FInd a way to call VM directly
        internal void AfterCheckboxFlip()
        {
            RefreshSelBlocks();
            StartOrResetDispatcherTimer();
        }

        private void RefreshSelBlocks()
        {
            SelBlocks = BlocksCheckableNodesDictionary.Values.Count(cn => cn.IsChecked.Value && cn.Level == 0);
        }

        private void RefreshFilBlocks()
        {
            FilBlocks = BlocksCheckableNodesDictionary.Values.Count(cn => cn.NodeVisibility == Visibility.Visible);
        }



        // Delay processing of TextChanged event 250ms using a DispatcherTimer
        DispatcherTimer dispatcherTimer;

        private void DispatcherTimer_Tick(object sender, object e)
        {
            dispatcherTimer.Stop();
            dispatcherTimer.Tick -= DispatcherTimer_Tick;
            dispatcherTimer = null;

            FilterCharList();
        }

        // Temporarily public during transition to MVVM pattern
        // ToDo: Make it private
        public void StartOrResetDispatcherTimer()
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

        private void FilterCharList()
        {
            CollectionView view = CollectionViewSource.GetDefaultView(CharactersRecordsList) as CollectionView;

            // Block part of the filtering predicate
            Predicate<object> bp = (object o) => BlocksCheckableNodesDictionary[((CharacterRecord)o).BlockBegin].IsChecked.Value;

            // Character part of the filtering predicate
            if (string.IsNullOrEmpty(CharNameFilter))
                view.Filter = bp;
            else
            {
                // Create a new instance of FilterPredicateBuilder that prepares suitable predicate
                var cpb = new PredicateBuilder(CharNameFilter, false, false, false, false);
                view.Filter = (o) => bp(o) && cpb.GetCharacterRecordFilter(o);
            }

            FilChars = view.Count;
        }


        private void FilterBlockList()
        {
            string s = BlockNameFilter;
            var p = new PredicateBuilder(s, false, false, false, false);
            Predicate<object> f = p.GetCheckableNodeFilter;

            FilterBlock(root);
            RefreshFilBlocks();

            Visibility FilterBlock(CheckableNode n)
            {
                if (n.Level == 0)
                {
                    n.NodeVisibility = f(n) ? Visibility.Visible : Visibility.Collapsed;
                    return n.NodeVisibility;
                }
                else
                {
                    Visibility v = Visibility.Collapsed;
                    foreach (CheckableNode child in n.Children)
                        if (FilterBlock(child) == Visibility.Visible)
                            v = Visibility.Visible;
                    // Also filter on group name
                    if (f(n)) v = Visibility.Visible;
                    n.NodeVisibility = v;
                    return v;
                }
            }
        }



        // Commands

        private bool CanCopyChars(object obj)
        {
            // Trick to refresh selection count
            SelChars = window.CharListView.SelectedItems.Count;

            return SelectedChar != null;
        }

        private void CopyCharsExecute(object param)
        {
            System.Collections.IList items = (System.Collections.IList)param;
            var selectedCharRecords = items.Cast<CharacterRecord>();
            CopyCharRecords(true, selectedCharRecords);
        }


        private bool CanCopyLines(object obj)
        {
            return SelectedChar != null;
        }

        // With Shift, copies all file paths
        private void CopyLinesExecute(object param)
        {
            System.Collections.IList items = (System.Collections.IList)param;
            var selectedCharRecords = items.Cast<CharacterRecord>();
            CopyCharRecords(false, selectedCharRecords);
        }

        private static void CopyCharRecords(bool isJustCharacter, IEnumerable<CharacterRecord> selectedCharRecords)
        {
            var sb = new StringBuilder();
            foreach (CharacterRecord r in selectedCharRecords.OrderBy(cr => cr.Codepoint))
                if (isJustCharacter)
                    sb.Append(r.Character);
                else
                    sb.AppendLine(r.Character + "\t" + r.CodepointHexa + "\t" + r.Name);
            System.Windows.Clipboard.Clear();
            System.Windows.Clipboard.SetText(sb.ToString());
        }


        void ActionAllNodes(CheckableNode n, Action<CheckableNode> a)
        {
            a(n);
            foreach (CheckableNode child in n.Children)
                ActionAllNodes(child, a);
        }

        private void ShowLevelExecute(object param)
        {
            int level = int.Parse(param as string);
            ActionAllNodes(root, n => { n.IsNodeExpanded = (n.Level != level); });
            //tree.Focus();
        }

        private void FlipVisibleExecute(object param)
        {
            bool isChecked = int.Parse(param as string) != 0;
            ActionAllNodes(root, n => { if (n.Level == 0 && n.NodeVisibility == Visibility.Visible) n.IsChecked = isChecked; });
            //tree.Focus();
        }
    }
}
