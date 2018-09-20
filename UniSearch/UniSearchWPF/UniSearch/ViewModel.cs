// UniSearch ViewModel
// Interaction and commands support
//
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
using UniDataNS;
using System.Windows.Media.Imaging;
using DirectDrawWrite;
using System.Windows.Controls;
using System.Windows.Documents;

namespace UniSearchNS
{
    internal class ViewModel : INotifyPropertyChanged
    {
        // Private variables
        private readonly SearchWindow window;       // Access to main window
        private Dictionary<int, CheckableNode> BlocksCheckableNodesDictionary;
        private readonly CheckableNode root;        // TreeView root


        // INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        // Commands public interface
        public ICommand CopyRecordsCommand { get; private set; }
        public ICommand CopyImageCommand { get; private set; }
        public ICommand ShowLevelCommand { get; private set; }
        public ICommand FlipVisibleCommand { get; private set; }
        public ICommand ShowDetailCommand { get; private set; }


        // Constructor
        public ViewModel(SearchWindow w)
        {
            window = w;

            // Binding commands with behavior
            CopyRecordsCommand = new RelayCommand<object>(CopyRecordsExecute, CanCopyRecords);
            CopyImageCommand = new RelayCommand<object>(CopyImageExecute, CanCopyRecords);
            ShowLevelCommand = new RelayCommand<object>(ShowLevelExecute);
            FlipVisibleCommand = new RelayCommand<string>(FlipVisibleExecute);
            ShowDetailCommand = new RelayCommand<int>(ShowDetailExecute);

            // Get Unicode data
            CharactersRecordsList = UniData.CharacterRecords.Values.OrderBy(cr => cr.Codepoint).ToArray();
            ReadOnlyDictionary<int, BlockRecord>.ValueCollection BlockRecordsList = UniData.BlockRecords.Values;


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
            Roots = root.Children;

            // Unselect some pretty useless blocks
            BlocksCheckableNodesDictionary[0x0000].IsChecked = false;       // ASCII Controls C0
            BlocksCheckableNodesDictionary[0x0080].IsChecked = false;       // ASCII Controls C1
            BlocksCheckableNodesDictionary[0xE0100].IsChecked = false;      // Variation Selectors Supplement; Variation Selectors; Specials; Symbols and Punctuation
            BlocksCheckableNodesDictionary[0xF0000].IsChecked = false;      // Supplementary Private Use Area-A; ; Private Use; Symbols and Punctuation
            BlocksCheckableNodesDictionary[0x100000].IsChecked = false;     // Supplementary Private Use Area-B; ; Private Use; Symbols and Punctuation
            ActionAllNodes(root, n =>
            {
                if (n.Name == "East Asian Scripts") n.IsChecked = false;
                if (n.Name.IndexOf("CJK", StringComparison.Ordinal) >= 0) n.IsChecked = false;
                if (n.Name == "Specials") n.IsChecked = false;
            });


            BlocksCheckableNodesDictionary[0xD800].IsChecked = false;       //  High Surrogates; ; Surrogates; Symbols and Punctuation
            BlocksCheckableNodesDictionary[0xDB80].IsChecked = false;       //  High Private Use Surrogates; ; Surrogates; Symbols and Punctuation
            BlocksCheckableNodesDictionary[0xDC00].IsChecked = false;       //  Low Surrogates; ; Surrogates; Symbols and Punctuation
            BlocksCheckableNodesDictionary[0xE000].IsChecked = false;       //  Private Use Area; ; Private Use; Symbols and Punctuation

            // To compute bound values based on initial blocks filtering
            FilterCharList();
            RefreshSelBlocks();
            RefreshFilBlocks();
        }


        // ==============================================================================================
        // Bindable properties

        public IList<CheckableNode> Roots { get; set; }      // For TreeView binding
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
                    NotifyPropertyChanged(nameof(StrContent));
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

        public int NumChars => UniData.CharacterRecords.Count;

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

        public UIElement StrContent
        {
            get
            {
                if (SelectedChar == null) return null;
                return GetStrContent(SelectedChar.Codepoint, ShowDetailCommand);
            }
        }

        public static UIElement GetStrContent(int codepoint, ICommand command)
        {
            var cr = UniData.CharacterRecords[codepoint];

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            var t1 = new TextBlock { Text = cr.Character };
            Grid.SetColumn(t1, 0);
            g.Children.Add(t1);

            Run r = new Run(cr.CodepointHex);
            Hyperlink h = new Hyperlink(r)
            {
                Command = command,
                CommandParameter = cr.Codepoint
            };
            TextBlock t2 = new TextBlock(h);
            Grid.SetColumn(t2, 1);
            g.Children.Add(t2);

            var t3 = new TextBlock { Text = cr.Name };
            Grid.SetColumn(t3, 2);
            g.Children.Add(t3);

            return g;
        }



        // ==============================================================================================
        // Events processing

        // Called from Window
        // ToDo: FInd a way to call VM directly
        internal void AfterCheckboxFlip()
        {
            RefreshSelBlocks();
            StartOrResetCharFilterDispatcherTimer();
        }

        private void RefreshSelBlocks()
        {
            SelBlocks = BlocksCheckableNodesDictionary.Values.Count(cn => cn.IsChecked.Value && cn.Level == 0);
        }

        private void RefreshFilBlocks()
        {
            FilBlocks = BlocksCheckableNodesDictionary.Values.Count(cn => cn.NodeVisibility == Visibility.Visible);
        }



        // ==============================================================================================
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
        public void StartOrResetCharFilterDispatcherTimer()
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
            bool bp(object o) => BlocksCheckableNodesDictionary[((CharacterRecord)o).Block].IsChecked.Value;

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

            /*
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
            */

            bool FilterBlock(CheckableNode n)
            {
                if (n.Level == 0)
                {
                    bool v = f(n);
                    n.NodeVisibility = v ? Visibility.Visible : Visibility.Collapsed;
                    return v;
                }
                else
                {
                    bool exp = false;
                    foreach (CheckableNode child in n.Children)
                        exp|= FilterBlock(child);
                    exp |= f(n);
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
            SelChars = window.CharListView.SelectedItems.Count;

            return SelectedChar != null;
        }

        private void CopyRecordsExecute(object param)
        {
            var selectedCharRecords = window.CharListView.SelectedItems.Cast<CharacterRecord>();

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
                        sb.AppendLine(r.Character + "\t" + r.CodepointHex + "\t" + r.Name + "\t" + r.CategoryRecord.Categories + "\t" + r.Age + "\t" + r.BlockRecord.BlockNameAndRange + "\t" + r.UTF16 + "\t" + r.UTF8);
                        break;
                }
            System.Windows.Clipboard.Clear();
            System.Windows.Clipboard.SetText(sb.ToString());
        }


        private void CopyImageExecute(object param)
        {
            if (SelectedChar != null)
            {
                System.Windows.Clipboard.Clear();
                System.Windows.Clipboard.SetImage(SelectedCharImage);
            }
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
        }


        /*
        <Button Command="{Binding FlipVisibleCommand}" CommandParameter="0" Content=" Check _visible " Margin="0,0,6,0" />
        <Button Command="{Binding FlipVisibleCommand}" CommandParameter="1" Content=" _Uncheck visible " Margin="0,0,6,0" />
        <Button Command="{Binding FlipVisibleCommand}" CommandParameter="2" Content=" Check _all " Margin="0,0,6,0" />
        <Button Command="{Binding FlipVisibleCommand}" CommandParameter="3" Content=" Unchec_k all " Margin="0,0,6,0" />

        */
        private void FlipVisibleExecute(string sparam)
        {
            int.TryParse(sparam, out int param);
            switch (param)
            {
                case 0:
                case 1:
                    ActionAllNodes(root, n => { if (n.Level == 0 && n.NodeVisibility == Visibility.Visible) n.IsChecked = param==0; });
                    break;
                case 2:
                case 3:
                    ActionAllNodes(root, n => { if (n.Level == 0) n.IsChecked = param == 2; });
                    break;
            }
        }


        private void ShowDetailExecute(int codepoint)
        {
            CharDetailWindow.ShowDetail(codepoint);
        }

    }
}
