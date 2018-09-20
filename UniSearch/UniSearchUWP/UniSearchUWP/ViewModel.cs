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
using System.Windows.Input;
using System.Linq;
using System.Windows;
using UniDataNS;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Data;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using System.Reflection;
using Windows.UI.Popups;


namespace UniSearchUWP
{
    internal class ViewModel : INotifyPropertyChanged
    {
        // Private variables
        private readonly SearchPage window;       // Access to main window
        private Dictionary<int, CheckableNode> BlocksCheckableNodesDictionary;
        private readonly CheckableNode root;        // TreeView root


        // INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string propertyName)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        // Commands public interface
        public ICommand CopyRecordsCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }
        public ICommand ShowLevelCommand { get; private set; }
        public ICommand FlipVisibleCommand { get; private set; }
        public ICommand ShowDetailCommand { get; private set; }


        // Constructor
        public ViewModel(SearchPage w)
        {
            window = w;

            // Binding commands with behavior
            CopyRecordsCommand = new RelayCommand<object>(CopyRecordsExecute, CanCopyRecords);
            AboutCommand = new AwaitableRelayCommand<object>(AboutExecute);
            ShowLevelCommand = new RelayCommand<object>(ShowLevelExecute);
            FlipVisibleCommand = new RelayCommand<string>(FlipVisibleExecute);
            ShowDetailCommand = new AwaitableRelayCommand<int>(ShowDetailExecute);

            // Get Unicode data
            CharactersRecordsList = UniData.CharacterRecords.Values.OrderBy(cr => cr.Codepoint).ToArray();
            BlocksRecordsList = UniData.BlockRecords.Values.OrderBy(br => br.Begin).ToArray();

            // root is *not* added to the TreeView on purpose
            root = new CheckableNode("root", 4);
            // Dictionary of CheckableNodes indexed by Begin block value
            BlocksCheckableNodesDictionary = new Dictionary<int, CheckableNode>();

            NodesList = new List<CheckableNode>();

            foreach (var l3 in BlocksRecordsList.GroupBy(b => b.Level3Name).OrderBy(g => g.Key))
            {
                var r3 = new CheckableNode(l3.Key, 3);
                root.Children.Add(r3);
                NodesList.Add(r3);
                foreach (var l2 in l3.GroupBy(b => b.Level2Name))
                {
                    var r2 = new CheckableNode(l2.Key, 2);
                    r3.Children.Add(r2);
                    NodesList.Add(r2);
                    foreach (var l1 in l2.GroupBy(b => b.Level1Name))
                    {
                        // Special case, l1 can be empty
                        CheckableNode r1;
                        if (l1.Key.Length > 0)
                        {
                            r1 = new CheckableNode(l1.Key, 1);
                            r2.Children.Add(r1);
                            NodesList.Add(r1);
                        }
                        else
                            r1 = r2;
                        // Blocks
                        foreach (var l0 in l1)
                        {
                            var blockCheckableNode = new CheckableNode(l0.BlockName, 0);
                            r1.Children.Add(blockCheckableNode);
                            BlocksCheckableNodesDictionary.Add(l0.Begin, blockCheckableNode);
                            NodesList.Add(blockCheckableNode);

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

            // To compute bound values based on initial blocks filteringFilterCharList();
            RefreshSelBlocks();
            RefreshFilBlocks();
            FilterCharList();
        }

        // ==============================================================================================
        // Bindable properties

        public IList<CheckableNode> Roots { get; set; }      // For TreeView binding
        public CharacterRecord[] CharactersRecordsList { get; set; }
        public ObservableCollection<CharacterRecord> CharactersRecordsFilteredList { get; set; } = new ObservableCollection<CharacterRecord>();

        public List<CheckableNode> NodesList { get; set; }

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
                    NotifyPropertyChanged(nameof(StrContent));
                    CopyRecordsCommand.RaiseCanExecuteChanged();


                }
            }
        }

        public int NumChars => UniData.CharacterRecords.Count;

        public int NumBlocks => BlocksCheckableNodesDictionary.Count;


        public int SelChars => window.CharCurrentView.SelectedItems.Count;


        public int FilChars => CharactersRecordsFilteredList.Count;


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
            SelBlocks = BlocksCheckableNodesDictionary.Values.Count(cn => cn.IsChecked != null && cn.IsChecked.Value && cn.Level == 0);
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
            // Block part of the filtering predicate
            bool p1(CharacterRecord cr) => BlocksCheckableNodesDictionary[cr.Block].IsChecked != null && BlocksCheckableNodesDictionary[cr.Block].IsChecked.Value;
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

            CharactersRecordsFilteredList.Clear();
            foreach (CharacterRecord c in CharactersRecordsList.Where(cr => p2(cr)))
                CharactersRecordsFilteredList.Add(c);

            // Refresh filtered count
            NotifyPropertyChanged(nameof(FilChars));
        }


        private void FilterBlockList()
        {
            string s = BlockNameFilter;
            var p = new PredicateBuilder(s, false, false, false, false);
            Predicate<object> f = p.GetCheckableNodeFilter;

            FilterBlock(root);
            RefreshFilBlocks();

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
                        exp |= FilterBlock(child);
                    exp |= f(n);
                    n.IsNodeExpanded = exp;
                    return exp;
                }
            }
        }



        // ==============================================================================================
        // Commands

        private bool CanCopyRecords(object obj) => SelectedChar != null;

        private void CopyRecordsExecute(object param)
        {
            var selectedCharRecords = window.CharCurrentView.SelectedItems.Cast<CharacterRecord>();

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

            var dataPackage = new DataPackage();
            dataPackage.SetText(sb.ToString());
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

        private void FlipVisibleExecute(string sparam)
        {
            int.TryParse(sparam, out int param);
            switch (param)
            {
                case 0:
                case 1:
                    ActionAllNodes(root, n => { if (n.Level == 0 && n.NodeVisibility == Visibility.Visible) n.IsChecked = param == 0; });
                    break;
                case 2:
                case 3:
                    ActionAllNodes(root, n => { if (n.Level == 0) n.IsChecked = param == 2; });
                    break;
            }
        }


    }
}
