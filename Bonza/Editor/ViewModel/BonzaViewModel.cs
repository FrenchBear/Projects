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

        // Menus
        public ICommand NewLayoutCommand { get; private set; }
        public ICommand LoadCommand { get; private set; }
        public ICommand RegenerateLayoutCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand QuitCommand { get; private set; }

        // Edit
        public ICommand UndoCommand { get; private set; }

        // View
        public ICommand RecenterLayoutViewCommand { get; private set; }

        // About
        public ICommand AboutCommand { get; private set; }


        // Constructor
        public BonzaViewModel(BonzaModel model, EditorWindow view)
        {
            // Binding commands with behavior

            // File
            NewLayoutCommand = new RelayCommand<object>(NewLayoutExecute);
            LoadCommand = new RelayCommand<object>(LoadExecute, LoadCanExecute);
            RegenerateLayoutCommand = new RelayCommand<object>(RegenerateLayoutExecute, RegenerateLayoutCanExecute);
            SaveCommand = new RelayCommand<object>(SaveExecute, SaveCanExecute);
            QuitCommand = new RelayCommand<object>(QuitExecute);

            // Edit
            UndoCommand = new RelayCommand<object>(UndoExecute, UndoCanExecute);

            // View
            RecenterLayoutViewCommand = new RelayCommand<object>(RecenterLayoutViewExecute, RecenterLayoutViewCanExecute);

            // Help
            AboutCommand = new RelayCommand<object>(AboutExecute);

            // Initialize ViewModel
            this.model = model;
            this.view = view;
        }


        // -------------------------------------------------
        // Bindings

        public IEnumerable<WordPosition> WordPositionList => model?.Layout?.WordPositionList;

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

        private string _Caption = App.AppName;
        public string Caption
        {
            get
            {
                if (LayoutName == null)
                    return App.AppName;
                else
                    return App.AppName + " - " + LayoutName;
            }
        }

        private string _LayoutName = null;
        public string LayoutName
        {
            get { return _LayoutName; }
            set
            {
                if (_LayoutName != value)
                {
                    _LayoutName = value;
                    NotifyPropertyChanged(nameof(LayoutName));
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

        internal void ClearLayout()
        {
            LayoutName = null;
            ClearUndoStack();
            view.ClearLayout();
        }

        internal void InitialLayoutDisplay()
        {
            view.InitialLayoutDisplay();
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
                LayoutName = null; ;
                model.LoadGrille(filename);
                LayoutName = Path.GetFileNameWithoutExtension(filename) + ".layout";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problème avec le fichier de mots " + filename + ":\r\n" + ex.Message, App.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }


        private bool RegenerateLayoutCanExecute(object obj)
        {
            return model.Layout != null;
        }

        private void RegenerateLayoutExecute(object obj)
        {
            try
            {
                view.ClearLayout();
                model.ResetLayout();
            }
            catch (Exception ex)
            {
                LayoutName = null;
                MessageBox.Show("Problème lors de la réinitialisation du layout:\r\n" + ex.Message, App.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }


        private bool RecenterLayoutViewCanExecute(object obj)
        {
            return model.Layout != null;
        }

        private void RecenterLayoutViewExecute(object obj)
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



        // -------------------------------------------------
        // Commands

        private void NewLayoutExecute(object obj)
        {
            model.NewGrille();
        }

        private void QuitExecute(object obj)
        {
            // ToDo: Add detection of unsaved changes once Save is implemented
            System.Environment.Exit(0);
        }


        private void AboutExecute(object obj)
        {
            var aw = new AboutWindow();
            aw.ShowDialog();
        }


    }
}
