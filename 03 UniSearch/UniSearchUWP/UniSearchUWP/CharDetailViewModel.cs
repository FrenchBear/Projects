// CharDetailViewModel
// Support for CharDetailDialog binding
//
// In UWP version, selected character can change during drill down, so INotifyPropertyChanged is implemented.
//
// 2018-09-18   PV
// 2020-11-11   PV      Hyperlinks to block and subheader; nullable enable
// 2020-11-12   PV      Added Synonyms, Comments and Cross-refs.  Block/Subheaders hyperlinks.  Copy buttons.  Scrollviewer


using RelayCommandNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using UniDataNS;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

#nullable enable


namespace UniSearchUWPNS
{
    internal class CharDetailViewModel : INotifyPropertyChanged
    {
        // Private variables
        private readonly Stack<int> History = new Stack<int>();
        private readonly CharDetailDialog window;
        private readonly ViewModel MainViewModel;


        // INotifyPropertyChanged interface
        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        // Commands public interface
        public ICommand ShowDetailCommand { get; private set; }
        public ICommand NewFilterCommand { get; private set; }


        // Constructor
        public CharDetailViewModel(CharDetailDialog window, CharacterRecord cr, ViewModel mainViewModel)
        {
            this.window = window;
            SelectedChar = cr;
            MainViewModel = mainViewModel;

            ShowDetailCommand = new RelayCommand<int>(ShowDetailExecute);
            NewFilterCommand = new RelayCommand<string>(NewFilterExecute);
        }


        // ==============================================================================================
        // Bindable properties

        private CharacterRecord _SelectedChar = UniData.CharacterRecords[0];    // To avoid making it nullable
        public CharacterRecord SelectedChar
        {
            get { return _SelectedChar; }
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
            if (!SelectedChar.IsPrintable) return null;

            string s = SelectedChar.Character;
            string sn = s.Normalize(form);
            if (s == sn) return null;
            if (form == NormalizationForm.FormKD && sn == s.Normalize(NormalizationForm.FormD))
                return new TextBlock { Text = "Same as NFD" };

            StackPanel sp = new StackPanel();
            foreach (var cr in sn.EnumCharacterRecords())
                sp.Children.Add(ViewModel.GetStrContent(cr.Codepoint, ShowDetailCommand));
            return sp;
        }

        private UIElement GetBlockHyperlink(string content, string commandParameter)
        {
            return new HyperlinkButton
            {
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                Content = content,
                Command = NewFilterCommand,
                CommandParameter = commandParameter
            };
        }

        public UIElement BlockContent => GetBlockHyperlink(SelectedChar.Block.BlockNameAndRange, "b:\"" + SelectedChar?.Block.BlockName + "\"");

        public UIElement SubheaderContent => GetBlockHyperlink(SelectedChar.Subheader, "s:\"" + SelectedChar?.Subheader + "\"");


        public UIElement? LowercaseContent => CaseContent(true);

        public UIElement? UppercaseContent => CaseContent(false);


        private UIElement? CaseContent(bool lower)
        {
            if (!SelectedChar.IsPrintable) return null;

            string s = SelectedChar.Character;
            string sc = lower ? s.ToLowerInvariant() : s.ToUpperInvariant();
            if (s == sc) return null;

            StackPanel sp = new StackPanel();
            foreach (var cr in sc.EnumCharacterRecords())
                sp.Children.Add(ViewModel.GetStrContent(cr.Codepoint, ShowDetailCommand));
            return sp;
        }

        public UIElement? SynonymsContent => GetExtraInfo(SelectedChar.Synonyms, false);
        public UIElement? CrossRefsContent => GetExtraInfo(SelectedChar.CrossRefs, true);
        public UIElement? CommentsContent => GetExtraInfo(SelectedChar.Comments, true);

        private static readonly Regex reCP = new Regex(@"\b1?[0-9A-F]{4,5}\b");

        private UIElement? GetExtraInfo(List<string>? list, bool autoHyperlink)
        {
            if (list == null)
                return null;

            TextBlock tb = new TextBlock { TextWrapping = TextWrapping.Wrap };
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
            Run r = new Run { Text = $"U\x2060+\x2060{codepoint:X4}" };

            Hyperlink h = new Hyperlink();
            h.Inlines.Add(r);
            h.Click += Click;

            if (withToolTip)
            {
                var cps = UniData.CodepointToString(codepoint);

                var tb = new TextBlock
                {
                    FontSize = 64,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Windows.UI.Colors.Black),
                    Text = cps
                };

                var vb = new Viewbox { Child = tb };

                var b = new Border
                {
                    Width = 96,
                    Height = 96,
                    Background = new SolidColorBrush(Windows.UI.Colors.CornflowerBlue),
                    Child = vb
                };

                ToolTip tooltip = new ToolTip
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
        // HyperlinkButton do support COmmand/Parameter, but can't be added to Inlines()
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

            SelectedChar = UniData.CharacterRecords[codepoint];
        }

        // From hyperlink
        private void NewFilterExecute(string filter)
        {
            window.Hide();
            MainViewModel.CharNameFilter = filter;
        }

    }
}
