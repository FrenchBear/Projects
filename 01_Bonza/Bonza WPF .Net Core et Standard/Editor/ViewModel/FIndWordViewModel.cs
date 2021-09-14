// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// FindWord ViewModel, VM for a simple dialog box to enter a text to search
//
// 2017-08-05   PV  First version


using System.ComponentModel;
using System.Windows.Input;

using Bonza.Editor.Support;


namespace Bonza.Editor.ViewModel
{
    public class FindWordViewModel : INotifyPropertyChanged
    {
        // Access to View and model (EditorModel, not AddWordsModel that doesn't exist)
        private View.FindWordView view;


        // Implementation of INotifyPropertyChanged, standard since View is only linked through DataBinding
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        // Commands
        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }



        public FindWordViewModel()
        {
            // Binding commands with behavior
            OkCommand = new RelayCommand<object>(OkExecute, OkCanExecute);
            CancelCommand = new RelayCommand<object>(CancelExecute);
        }

        public void SetView(View.FindWordView view)
        {
            this.view = view;
        }

        // -------------------------------------------------
        // Bindings

        private string m_SearchText;

        public string SearchText
        {
            get => m_SearchText;
            set
            {
                if (m_SearchText != value)
                {
                    m_SearchText = value;
                    NotifyPropertyChanged(nameof(SearchText));
                }
            }
        }


        // -------------------------------------------------
        // Commands

        private bool OkCanExecute(object obj)
        {
            return !string.IsNullOrEmpty(SearchText);
        }

        private void OkExecute(object obj)
        {
            view.DialogResult = true;
        }

        private void CancelExecute(object obj)
        {
            view.DialogResult = false;
        }

    }
}
