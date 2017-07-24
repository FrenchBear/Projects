// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM ViewModel
// 2017-07-22   PV  First version

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Bonza.Generator;


namespace Bonza.Editor
{
    class BonzaViewModel : INotifyPropertyChanged
    {
        // Model and View
        private BonzaModel model;
        private EditorWindow view;

        // Implementation of INotifyPropertyChanged, standard since View is only linked through DataBinding
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        // Commands
        public ICommand LoadCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand ResetCommand { get; private set; }


        // Constructor
        public BonzaViewModel(BonzaModel model, EditorWindow view)
        {
            // Binding commands with behavior
            LoadCommand = new RelayCommand<object>(LoadExecute, LoadCanExecute);
            SaveCommand = new RelayCommand<object>(SaveExecute, SaveCanExecute);
            ResetCommand = new RelayCommand<object>(ResetExecute, ResetCanExecute);

            // Initialize ViewModel
            this.model = model;
            this.view = view;
        }

        // -------------------------------------------------
        // Bindings

        //private ObservableCollection<WordPosition> words = new ObservableCollection<WordPosition>();
        //public ObservableCollection<WordPosition> Words
        //{
        //    get => words;
        //}

        public IEnumerable<WordPosition> WordPositionList  => model.Layout.WordPositionList;

        private int _WordsNotConnected;
        public int WordsNotConnected
        {
            get { return _WordsNotConnected; }
            set
            {
                if (_WordsNotConnected != value)
                {
                    _WordsNotConnected = value;
                    NotifyPropertyChanged(nameof(WordsNotConnected));
                }
            }
        }


        // -------------------------------------------------
        // View helpers

        internal void UpdateWordPositionLocation(WordPosition wp, int left, int top)
        {
            model.UpdateWordPositionLocation(wp, left, top);
        }


        // -------------------------------------------------
        // Commands

        private bool SaveCanExecute(object obj)
        {
            return true;
        }

        private void SaveExecute(object obj)
        {
            MessageBox.Show("Save: ToDo");
        }


        private bool LoadCanExecute(object obj)
        {
            return true;
        }

        private void LoadExecute(object obj)
        {
            MessageBox.Show("Load: ToDo");
        }


        private bool ResetCanExecute(object obj)
        {
            return true;
        }

        private void ResetExecute(object obj)
        {
            MessageBox.Show("Reset: ToDo");
        }

    }
}
