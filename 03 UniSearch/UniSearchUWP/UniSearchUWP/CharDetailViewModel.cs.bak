// CharDetailViewModel
// Support for CharDetailDialog binding
//
// In UWP version, selected character can change during drill down, so INotifyPropertyChanged is implemented.
//
// 2018-09-18   PV
// 2020-11-11   PV      1.3 Hyperlinks to block and subheader; nullable enable


using RelayCommandNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using UniDataNS;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
