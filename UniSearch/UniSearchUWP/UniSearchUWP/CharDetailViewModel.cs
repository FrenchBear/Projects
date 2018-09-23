// CharDetailViewModel
// Support for CharDetailDialog binding
//
// In UWP version, selected character can change during drill down, so INotifyPropertyChanged is implemented.
//
// 2018-09-18   PV

using RelayCommandNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UniDataNS;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace UniSearchUWP
{
    internal class CharDetailViewModel : INotifyPropertyChanged
    {
        // Private variables
        private readonly Stack<int> History = new Stack<int>();
        private readonly CharDetailDialog window;


        // INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        // Commands public interface
        public ICommand ShowDetailCommand { get; private set; }


        // Constructor
        public CharDetailViewModel(CharDetailDialog window, CharacterRecord cr)
        {
            this.window = window;
            SelectedChar = cr;
            ShowDetailCommand = new RelayCommand<int>(ShowDetailExecute);
        }


        // ==============================================================================================
        // Bindable properties

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
                    NotifyPropertyChanged(nameof(Title));
                    NotifyPropertyChanged(nameof(NormalizationNFDContent));
                    NotifyPropertyChanged(nameof(NormalizationNFKDContent));
                    NotifyPropertyChanged(nameof(LowercaseContent));
                    NotifyPropertyChanged(nameof(UppercaseContent));
                }
            }
        }

        public string Title => SelectedChar == null ? "Character Detail" : SelectedChar.CodepointHex + " – Character Detail";


        public Object NormalizationNFDContent => NormalizationContent(NormalizationForm.FormD);

        public Object NormalizationNFKDContent => NormalizationContent(NormalizationForm.FormKD);


        private UIElement NormalizationContent(NormalizationForm form)
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



        public Object LowercaseContent => CaseContent(true);

        public Object UppercaseContent => CaseContent(false);


        private UIElement CaseContent(bool lower)
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
    }
}
