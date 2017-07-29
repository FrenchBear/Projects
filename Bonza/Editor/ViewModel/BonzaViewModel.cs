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
using System.Diagnostics;
using System.Linq;

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
        public ICommand SwapCommand { get; private set; }

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
            SwapCommand = new RelayCommand<object>(SwapExecute, SwapCanExecute);

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

        // Simple relay to model

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

        private string _LayoutName;
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


        //List<WordPosition> selectedWordPositionList;



        // -------------------------------------------------
        // Undo support
        // Encapsulate class for easier debugging (and also because it's a good practice...)

        public class UndoStackClass
        {
            private Stack<(List<WordPosition>, List<PositionOrientation>)> UndoStack;

            public void Clear()
            {
                UndoStack = null;
            }

            // Memorize current position of a list of WordPosition so it can be restored layer
            public void Push(List<WordPosition> wordPositionList)
            {
                if (wordPositionList == null) throw new ArgumentNullException(nameof(wordPositionList));
                Debug.Assert(wordPositionList.Count >= 1);

                // Memorize position in a separate list since WordPosition objects position will change
                List<PositionOrientation> topLeftList = wordPositionList.Select(wp => new PositionOrientation { StartRow = wp.StartRow, StartColumn = wp.StartColumn, IsVertical = wp.IsVertical }).ToList();

                if (UndoStack == null)
                    UndoStack = new Stack<(List<WordPosition>, List<PositionOrientation>)>();
                // Since wordPositionList is a list belonging to view, we need to clone it
                UndoStack.Push((new List<WordPosition>(wordPositionList), topLeftList));
            }

            public bool CanUndo => UndoStack != null && UndoStack.Count > 0;

            public (List<WordPosition> wordPositionList, List<PositionOrientation> topLeftList) Pop()
            {
                Debug.Assert(CanUndo);
                return UndoStack.Pop();
            }
        }

        // Should implement singleton pattern, and probably move its code in a separate file
        public UndoStackClass UndoStack = new UndoStackClass();

        public void PerformUndo()
        {
            var (wordPositionList, topLeftList) = UndoStack.Pop();
            UpdateWordPositionLocation(wordPositionList, topLeftList, false);   // Coordinates in wordPositionList are updated
            view.MoveWordPositionList(wordPositionList);
        }

        // -------------------------------------------------
        // Model helpers

        internal void ClearLayout()
        {
            LayoutName = null;
            UndoStack.Clear();
            view.ClearLayout();
        }

        internal void InitialLayoutDisplay()
        {
            view.InitialLayoutDisplay();
        }


        // -------------------------------------------------
        // View helpers

        // When a list of WordPositions have moved to their final location in view
        internal void UpdateWordPositionLocation(List<WordPosition> wordPositionList, List<PositionOrientation> topLeftList, bool memorizeForUndo)
        {
            if (wordPositionList == null) throw new ArgumentNullException(nameof(wordPositionList));
            if (topLeftList == null) throw new ArgumentNullException(nameof(topLeftList));
            Debug.Assert(wordPositionList.Count == topLeftList.Count);

            // If we don't really move, there is nothing more to do
            bool isRealMove = wordPositionList[0].StartRow != topLeftList[0].StartRow || wordPositionList[0].StartColumn != topLeftList[0].StartColumn || wordPositionList[0].IsVertical != topLeftList[0].IsVertical;
            if (!isRealMove)
                return;

            // Memorize position before move for undo, unless we're undoing or the move
            if (memorizeForUndo)
                UndoStack.Push(wordPositionList);

            for (int i = 0; i < wordPositionList.Count; i++)
                model.UpdateWordPositionLocation(wordPositionList[i], topLeftList[i]);
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
                LayoutName = null;
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
            return model.Layout != null && UndoStack.CanUndo;
        }

        private void UndoExecute(object obj)
        {
            PerformUndo();
        }


        private bool SwapCanExecute(object obj)
        {
            return model.Layout != null;        // ToDo: Need to manage selection at viewmodel and check that selection.count==1
        }

        private void SwapExecute(object obj)
        {
            MessageBox.Show("Swap: ToDo");
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
