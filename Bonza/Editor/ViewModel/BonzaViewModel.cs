// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM ViewModel
// 2017-07-22   PV  First version

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Bonza.Editor.Model;
using Bonza.Generator;
using Microsoft.Win32;
using Bonza.Editor.Support;

namespace Bonza.Editor.ViewModel
{
    class BonzaViewModel : INotifyPropertyChanged
    {
        // Model and View
        private readonly BonzaModel model;
        private readonly View.BonzaView view;

        // Implementation of INotifyPropertyChanged, standard since View is only linked through DataBinding
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        // Commands

        // Menus
        public ICommand NewLayoutCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand RegenerateLayoutCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand QuitCommand { get; }

        // Edit
        public ICommand DeleteCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand SwapOrientationCommand { get; }
        public ICommand AutoPlaceCommand { get; }

        // View
        public ICommand RecenterLayoutViewCommand { get; }

        // About
        public ICommand AboutCommand { get; }


        // Constructor
        public BonzaViewModel(View.BonzaView view)
        {
            // Initialize ViewModel
            this.view = view;
            model = new BonzaModel(this);


            // Binding commands with behavior

            // File
            NewLayoutCommand = new RelayCommand<object>(NewLayoutExecute);
            LoadCommand = new RelayCommand<object>(LoadExecute, LoadCanExecute);
            RegenerateLayoutCommand = new RelayCommand<object>(RegenerateLayoutExecute, RegenerateLayoutCanExecute);
            SaveCommand = new RelayCommand<object>(SaveExecute, SaveCanExecute);
            QuitCommand = new RelayCommand<object>(QuitExecute);

            // Edit
            DeleteCommand = new RelayCommand<object>(DeleteExecute, DeleteCanExecute);
            UndoCommand = new RelayCommand<object>(UndoExecute, UndoCanExecute);
            SwapOrientationCommand = new RelayCommand<object>(SwapOrientationExecute, SwapOrientationCanExecute);
            AutoPlaceCommand = new RelayCommand<object>(AutoPlaceExecute, AutoPlaceCanExecute);

            // View
            RecenterLayoutViewCommand = new RelayCommand<object>(RecenterLayoutViewExecute, RecenterLayoutViewCanExecute);

            // Help
            AboutCommand = new RelayCommand<object>(AboutExecute);
        }


        // -------------------------------------------------
        // Simple relays to model

        public WordPositionLayout Layout => model?.Layout;

        public IEnumerable<WordPosition> WordPositionList => model?.Layout?.WordPositionList;

        internal void BuildMoveTestLayout(ReadOnlyCollection<WordPosition> wordPositionList)
        {
            model.BuildMoveTestLayout(wordPositionList);
        }

        internal bool CanPlaceWordInMoveTestLayout(WordPosition wordPosition, PositionOrientation positionOrientation)
        {
            return model.CanPlaceWordInMoveTestLayout(wordPosition, positionOrientation);
        }



        // -------------------------------------------------
        // Bindings

        private int m_WordsNotConnected;
        public int WordsNotConnected
        {
            get => m_WordsNotConnected;
            set
            {
                if (m_WordsNotConnected != value)
                {
                    m_WordsNotConnected = value;
                    NotifyPropertyChanged(nameof(WordsNotConnected));
                }
            }
        }

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

        private string m_LayoutName;
        public string LayoutName
        {
            get => m_LayoutName;
            set
            {
                if (m_LayoutName != value)
                {
                    m_LayoutName = value;
                    NotifyPropertyChanged(nameof(LayoutName));
                    NotifyPropertyChanged(nameof(Caption));
                }
            }
        }


        private int m_SelectedWordCount;
        public int SelectedWordCount
        {
            get => m_SelectedWordCount;
            set
            {
                if (m_SelectedWordCount != value)
                {
                    m_SelectedWordCount = value;
                    NotifyPropertyChanged(nameof(SelectedWordCount));
                }
            }
        }



        // -------------------------------------------------
        // Undo support
        // Encapsulate class for easier debugging (and also because it's a good practice...)

        public class UndoStackClass
        {
            private Stack<(List<WordPosition>, List<PositionOrientation>)> undoStack;

            public void Clear()
            {
                undoStack = null;
            }

            // Memorize current position of a list of WordPosition so it can be restored layer
            public void Push(IList<WordPosition> wordPositionList)
            {
                if (wordPositionList == null) throw new ArgumentNullException(nameof(wordPositionList));
                Debug.Assert(wordPositionList.Count >= 1);

                // Memorize position in a separate list since WordPosition objects position will change
                List<PositionOrientation> topLeftList = wordPositionList.Select(wp => new PositionOrientation { StartRow = wp.StartRow, StartColumn = wp.StartColumn, IsVertical = wp.IsVertical }).ToList();

                if (undoStack == null)
                    undoStack = new Stack<(List<WordPosition>, List<PositionOrientation>)>();
                // Since wordPositionList is a list belonging to view, we need to clone it
                undoStack.Push((new List<WordPosition>(wordPositionList), topLeftList));
            }

            public bool CanUndo => undoStack != null && undoStack.Count > 0;

            public (List<WordPosition> wordPositionList, List<PositionOrientation> topLeftList) Pop()
            {
                Debug.Assert(CanUndo);
                return undoStack.Pop();
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
        internal void UpdateWordPositionLocation(IList<WordPosition> wordPositionList, List<PositionOrientation> topLeftList, bool memorizeForUndo)
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
            if (result.HasValue && result.Value)
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



        private bool DeleteCanExecute(object obj)
        {
            return model.Layout != null && SelectedWordCount>0;
        }

        private void DeleteExecute(object obj)
        {
            MessageBox.Show("Delete: ToDo");
        }


        private bool UndoCanExecute(object obj)
        {
            return model.Layout != null && UndoStack.CanUndo;
        }

        private void UndoExecute(object obj)
        {
            PerformUndo();
        }


        private bool SwapOrientationCanExecute(object obj)
        {
            return model.Layout != null && SelectedWordCount == 1;
        }

        private void SwapOrientationExecute(object obj)
        {
            view.Sel.SwapOrientation();
        }


        private bool AutoPlaceCanExecute(object obj)
        {
            return model.Layout != null && SelectedWordCount >= 1;
        }

        private void AutoPlaceExecute(object obj)
        {
            MessageBox.Show("AutoPlace: ToDo");
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
            Environment.Exit(0);
        }


        private void AboutExecute(object obj)
        {
            var aw = new View.AboutWindow();
            aw.ShowDialog();
        }

    }
}
