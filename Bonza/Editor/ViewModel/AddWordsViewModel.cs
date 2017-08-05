// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// AddWords ViewModel, VM for a simple dialog box to enter words to add to current layout
//
// 2017-08-05   PV  First version


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Bonza.Editor.Support;

namespace Bonza.Editor.ViewModel
{
    internal class AddWordsViewModel : INotifyPropertyChanged
    {
        // Access to View
        private readonly View.AddWordsView view;

        // Implementation of INotifyPropertyChanged, standard since View is only linked through DataBinding
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        // Commands
        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }


        public AddWordsViewModel(View.AddWordsView view)
        {
            // Initialize ViewModel
            this.view = view;

            // Binding commands with behavior
            OkCommand = new RelayCommand<object>(OkExecute);
            CancelCommand = new RelayCommand<object>(CancelExecute);
        }


        // -------------------------------------------------
        // Bindings

        private string m_InputText;
        public string InputText
        {
            get => m_InputText;
            set
            {
                if (m_InputText != value)
                {
                    m_InputText = value;
                    NotifyPropertyChanged(nameof(InputText));
                }
            }
        }



        // -------------------------------------------------
        // Commands

        private void OkExecute(object obj)
        {
            MessageBox.Show("ToDo: Add\n" + m_InputText);
            view.Close();
        }

        private void CancelExecute(object obj)
        {
            view.Close();
        }
    }
}