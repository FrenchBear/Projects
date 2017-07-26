// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM ViewModel
// 2017-07-22   PV  First version

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Bonza.Generator;
using Microsoft.Win32;
using System.IO;

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
        public ICommand CenterCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }


        // Constructor
        public BonzaViewModel(BonzaModel model, EditorWindow view)
        {
            // Binding commands with behavior
            LoadCommand = new RelayCommand<object>(LoadExecute, LoadCanExecute);
            SaveCommand = new RelayCommand<object>(SaveExecute, SaveCanExecute);
            ResetCommand = new RelayCommand<object>(ResetExecute, ResetCanExecute);
            CenterCommand = new RelayCommand<object>(CenterExecute, CenterCanExecute);
            UndoCommand = new RelayCommand<object>(UndoExecute, UndoCanExecute);

            // Initialize ViewModel
            this.model = model;
            this.view = view;
        }


        // -------------------------------------------------
        // Bindings

        public IEnumerable<WordPosition> WordPositionList => model.Layout.WordPositionList;

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

        private string _Caption;
        public string Caption
        {
            get { return _Caption; }
            set
            {
                if (_Caption != value)
                {
                    _Caption = value;
                    NotifyPropertyChanged(nameof(Caption));
                }
            }
        }

        // -------------------------------------------------
        // Undo support

        Stack<(List<WordPosition>, List<(int Left, int Top)>)> UndoStack = new Stack<(List<WordPosition>, List<(int, int)>)>();

        public void ClearUndoStack()
        {
            UndoStack = new Stack<(List<WordPosition>, List<(int, int)>)>();
        }

        private void PerformUndo()
        {
            if (UndoStack.Count == 0) return;

            var (wordPositionList, leftTopList) = UndoStack.Pop();
            UpdateWordPositionLocation(wordPositionList, leftTopList, false);   // Coordinates in wordPositionList are updated
            view.MoveWordPositionList(wordPositionList);
        }


        // -------------------------------------------------
        // Model helpers

        internal void NewLayout()
        {
            ClearUndoStack();
            view.NewLayout();
        }


        // -------------------------------------------------
        // View helpers

        // When a list of WordPositions have moved to their final location in view
        internal void UpdateWordPositionLocation(List<WordPosition> wordPositionList, List<(int Left, int Top)> leftTopList, bool isMemorizedForUndo)
        {
            // Memorize position before move for undo
            if (isMemorizedForUndo)
            {
                List<(int Left, int Top)> originalLeftTopList = new List<(int, int)>();
                foreach (WordPosition wp in wordPositionList)
                    originalLeftTopList.Add((wp.StartColumn, wp.StartRow));
                UndoStack.Push((wordPositionList, originalLeftTopList));
            }

            for (int i = 0; i < wordPositionList.Count; i++)
                model.UpdateWordPositionLocation(wordPositionList[i], leftTopList[i].Left, leftTopList[i].Top);
        }


        // -------------------------------------------------
        // Commands

        private bool SaveCanExecute(object obj)
        {
            return false;       // For now
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
            OpenFileDialog dlg = new OpenFileDialog()
            {
                DefaultExt = ".txt",
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                CheckFileExists = true,
                InitialDirectory = @"C:\Development\GitHub\Projects\Bonza\Lists"   // Path.Combine( Directory.GetCurrentDirectory(), @"..\Lists");
            };
            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true)
                LoadWordsList(dlg.FileName);
        }

        public void LoadWordsList(string filename)
        {
            try
            {
                view.ClearLayout();
                Caption = App.AppName;
                model.LoadGrille(filename);
                Caption = App.AppName + " - " + Path.GetFileName(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problème avec le fichier de mots " + filename + ":\r\n" + ex.Message, App.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }


        private bool ResetCanExecute(object obj)
        {
            return model.Layout != null;
        }

        private void ResetExecute(object obj)
        {
            try
            {
                view.ClearLayout();
                model.ResetLayout();
            }
            catch (Exception ex)
            {
                Caption = App.AppName;
                MessageBox.Show("Problème lors de la réinitialisation du layout:\r\n" + ex.Message, App.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }


        private bool CenterCanExecute(object obj)
        {
            return model.Layout != null;
        }

        private void CenterExecute(object obj)
        {
            view.RescaleAndCenter(true);        // Use animations
        }



        private bool UndoCanExecute(object obj)
        {
            return model.Layout != null && UndoStack.Count > 0;
        }

        private void UndoExecute(object obj)
        {
            PerformUndo();
        }

    }
}
