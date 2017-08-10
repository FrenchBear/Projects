// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// ViewModel of main Editor window
//
// 2017-07-22   PV  First version


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Bonza.Editor.Model;
using Bonza.Generator;
using Microsoft.Win32;
using Bonza.Editor.Support;

namespace Bonza.Editor.ViewModel
{
    internal class EditorViewModel : INotifyPropertyChanged
    {
        // Model and View
        private readonly EditorModel model;
        private readonly View.EditorView view;

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
        public ICommand AddWordsCommand { get; }
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
        public EditorViewModel(View.EditorView view)
        {
            // Initialize ViewModel
            this.view = view;
            model = new EditorModel(this);


            // Binding commands with behavior

            // File
            NewLayoutCommand = new RelayCommand<object>(NewLayoutExecute);
            LoadCommand = new RelayCommand<object>(LoadExecute);
            AddWordsCommand = new RelayCommand<object>(AddWordsExecute);
            RegenerateLayoutCommand = new RelayCommand<object>(RegenerateLayoutExecute);
            SaveCommand = new RelayCommand<object>(SaveExecute, SaveCanExecute);
            QuitCommand = new RelayCommand<object>(QuitExecute);

            // Edit
            DeleteCommand = new RelayCommand<object>(DeleteExecute, DeleteCanExecute);
            UndoCommand = new RelayCommand<object>(UndoExecute, UndoCanExecute);
            SwapOrientationCommand = new RelayCommand<object>(SwapOrientationExecute, SwapOrientationCanExecute);
            AutoPlaceCommand = new RelayCommand<object>(AutoPlaceExecute, AutoPlaceCanExecute);

            // View
            RecenterLayoutViewCommand = new RelayCommand<object>(RecenterLayoutViewExecute);

            // Help
            AboutCommand = new RelayCommand<object>(AboutExecute);
        }


        // -------------------------------------------------
        // Simple relays to model

        public WordPositionLayout Layout => model?.Layout;

        public IEnumerable<WordPosition> WordPositionList => model?.Layout?.WordPositionList;

        internal WordPositionLayout GetLayoutExcludingWordPositionList(IEnumerable<WordPosition> wordPositionList)
        {
            return model.GetLayoutExcludingWordPositionList(wordPositionList);
        }

        internal WordPositionLayout GetLayoutExcludingWordPosition(WordPosition wordPosition)
        {
            return model.GetLayoutExcludingWordPositionList(new List<WordPosition> { wordPosition });
        }

        internal PlaceWordStatus CanPlaceWordAtPositionInLayout(WordPositionLayout layout, WordPosition wordPosition, PositionOrientation positionOrientation)
        {
            return model.CanPlaceWordAtPositionInLayout(layout, wordPosition, positionOrientation);
        }

        internal void RemoveWordPosition(WordPosition wordPosition)
        {
            model.RemoveWordPosition(wordPosition);
        }

        internal void AddWordPosition(WordPosition wordPosition)
        {
            model.AddWordPosition(wordPosition);
        }

        internal PlaceWordStatus CanPlaceWord(WordAndCanvas wac, bool withTooClose)
        {
            return Layout.CanPlaceWord(wac.WordPosition, withTooClose);
        }

        internal PlaceWordStatus CanPlaceWordInLayout(WordPositionLayout layout, WordAndCanvas wac)
        {
            return layout.CanPlaceWord(wac.WordPosition, true);
        }



        // -------------------------------------------------
        // Selection helpers

        public void RecolorizeWordAndCanvasList(List<WordAndCanvas> wordAndCanvasList)
        {
            view.RecolorizeWordAndCanvasList(wordAndCanvasList);
        }



        // -------------------------------------------------
        // Bindings

        private string m_StatusText;
        public string StatusText
        {
            get => m_StatusText;
            set
            {
                if (m_StatusText != value)
                {
                    m_StatusText = value;
                    NotifyPropertyChanged(nameof(StatusText));
                }
            }
        }


        public string Caption => LayoutName == null ? App.AppName : App.AppName + " - " + LayoutName;


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

        // Should implement singleton pattern
        public UndoStackClass UndoStack = new UndoStackClass();

        internal void PerformUndo()
        {
            UndoStackClass.UndoAction action = UndoStack.Pop();

            switch (action.Action)
            {
                case UndoStackClass.UndoActions.Move:
                    UpdateWordPositionLocation(action.WordAndCanvasList, action.PositionOrientationList, false);   // Coordinates in wordPositionList are updated
                    view.MoveWordAndCanvasList(action.WordAndCanvasList);
                    break;

                case UndoStackClass.UndoActions.Delete:
                    view.AddWordAndCanvasList(action.WordAndCanvasList, false);
                    break;

                case UndoStackClass.UndoActions.Add:
                    view.DeleteWordAndCanvasList(action.WordAndCanvasList, false);
                    break;

                case UndoStackClass.UndoActions.SwapOrientation:
                    UpdateWordPositionLocation(action.WordAndCanvasList, action.PositionOrientationList, false);   // Coordinates in wordPositionList are updated
                    view.SwapOrientation(action.WordAndCanvasList, false);
                    break;

                default:
                    Debug.Assert(false, "Unknown/Unsupported Undo Action");
                    break;
            }

            view.FinalRefreshAfterUpdate();
        }


        // -------------------------------------------------
        // Model helpers

        internal void ClearLayout()
        {
            LayoutName = null;
            UndoStack.Clear();
            view.ClearWordAndCanvas();
            view.RescaleAndCenter(false);
            view.FinalRefreshAfterUpdate();
        }

        internal void AddCanvasForWordPositionList(IEnumerable<WordPosition> wordPositionList)
        {
            view.AddCanvasForWordPositionList(wordPositionList);
        }


        // -------------------------------------------------
        // View helpers

        internal void UpdateStatus(PlaceWordStatus status)
        {
            if (Layout == null)
                StatusText = "";
            else
            {
                string s = "Isl=" + Layout.GetWordsNotConnected() + "  ";

                switch (status)
                {
                    case PlaceWordStatus.Valid:
                        s += "Val";
                        break;
                    case PlaceWordStatus.TooClose:
                        s += "TCl";
                        break;
                    case PlaceWordStatus.Invalid:
                        s += "Inv";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(status), status, null);
                }

                StatusText = s;
            }
        }


        // When a list of WordPositions have moved to their final location in view
        internal void UpdateWordPositionLocation(IList<WordAndCanvas> wordAndCanvasList, IList<PositionOrientation> topLeftList, bool memorizeForUndo)
        {
            if (wordAndCanvasList == null) throw new ArgumentNullException(nameof(wordAndCanvasList));
            if (topLeftList == null) throw new ArgumentNullException(nameof(topLeftList));
            Debug.Assert(wordAndCanvasList.Count > 0 && wordAndCanvasList.Count == topLeftList.Count);

            // If we don't really move, there is nothing more to do
            var firstWac = wordAndCanvasList.First();
            var firstTl = topLeftList.First();
            if (firstWac.WordPosition.StartRow == firstTl.StartRow && firstWac.WordPosition.StartColumn == firstTl.StartColumn && firstWac.WordPosition.IsVertical == firstTl.IsVertical)
                return;

            // Memorize position before move for undo, unless we're undoing or the move
            if (memorizeForUndo)
                UndoStack.MemorizeMove(wordAndCanvasList);

            // Move: Need to delete all, then add all again, otherwise if we move a word individually, it may collide with other
            // of the list that hasn't been moved by this loop yet...
            foreach (WordPosition wp in wordAndCanvasList.Select(wac => wac.WordPosition))
                model.RemoveWordPosition(wp);
            foreach (var item in wordAndCanvasList.Zip(topLeftList, (wac, tl) => (WordAndCanvas: wac, topLeft: tl)))
            {
                item.WordAndCanvas.WordPosition.SetNewPositionOrientation(new PositionOrientation(item.topLeft.StartRow, item.topLeft.StartColumn, item.topLeft.IsVertical));
                model.AddWordPosition(item.WordAndCanvas.WordPosition);
            }
        }


        // -------------------------------------------------
        // Commands

        private bool SaveCanExecute(object obj)
        {
            return true;       // For now
        }

        // Temp version that saves layout as C# code
        private void SaveExecute(object obj)
        {
            SaveFileDialog dlg = new SaveFileDialog()
            {
                DefaultExt = ".cs",
                Filter = "C# files (*.cs)|*.cs|All Files (*.*)|*.*",
                OverwritePrompt = true,
                InitialDirectory = @"C:\Temp"
            };
            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value)
                model.Layout.SaveLayoutAsCode(dlg.FileName);
        }


        private void LoadExecute(object obj)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                CheckFileExists = true,
                InitialDirectory = @"C:\Development\GitHub\Projects\Bonza\Lists"
            };
            var result = dlg.ShowDialog();
            if (result.HasValue)
                AddWordsFromFile(dlg.FileName);
        }

        private void AddWordsExecute(object obj)
        {
            var aww = new View.AddWordsView(model)
            {
                Owner = view        // Make sure that AddWordsView window is always on the top of Editor
            };
            aww.ShowDialog();
            if (aww.wordsList == null) return;
            AddWordsList(aww.wordsList);
        }

        internal IList<WordAndCanvas> AddWordsList(List<string> wordsList)
        {
            if (wordsList == null)
                throw new ArgumentNullException(nameof(wordsList));

            string checkStatus = model.CheckWordsList(wordsList);
            if (!string.IsNullOrEmpty(checkStatus))
            {
                MessageBox.Show("Problème avec la liste de mots:\n" + checkStatus, App.AppName, MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return null;
            }


            Stopwatch sw = Stopwatch.StartNew();
            List<WordPosition> wordPositionList = model.AddWordsList(wordsList);
            if (wordPositionList == null)
            {
                MessageBox.Show("L'ajout des mots a échoué.", App.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return null;
            }
            sw.Stop();

            IList<WordAndCanvas> wordAndCanvasList = view.AddCanvasForWordPositionList(wordPositionList);
            view.FinalRefreshAfterUpdate();
            StatusText = $"{wordsList.Count} mots placés en " + sw.Elapsed.ToString(@"hh\:mm\:ss\.fff");
            view.RescaleAndCenter(true);

            return wordAndCanvasList;
        }


        public void AddWordsFromFile(string filename)
        {
            try
            {
                var wordsList = new List<string>();
                using (Stream s = new FileStream(filename, FileMode.Open))
                using (StreamReader sr = new StreamReader(s, Encoding.UTF8))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine()?.Trim();
                        if (!string.IsNullOrWhiteSpace(line))
                            wordsList.Add(line);
                    }
                }
                if (AddWordsList(wordsList)!=null)
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problème avec le fichier de mots " + filename + ":\r\n" + ex.Message, App.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            LayoutName = Path.GetFileNameWithoutExtension(filename) + ".layout";
        }


        private void RegenerateLayoutExecute(object obj)
        {
            view.EndAnimationsInProgress();

            try
            {
                view.ClearWordAndCanvas();
                Stopwatch sw = Stopwatch.StartNew();
                model.ResetLayout();
                sw.Stop();

                view.AddCanvasForWordPositionList(Layout.WordPositionList);
                view.FinalRefreshAfterUpdate();
                StatusText = $"{Layout.WordPositionList.Count} mots placés en " + sw.Elapsed.ToString(@"hh\:mm\:ss\.fff");
                view.RescaleAndCenter(false);
            }
            catch (Exception ex)
            {
                LayoutName = null;
                MessageBox.Show("Problème lors de la réinitialisation du layout:\r\n" + ex.Message, App.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }


        private void RecenterLayoutViewExecute(object obj)
        {
            view.RescaleAndCenter(true);        // Use animations
        }



        private bool DeleteCanExecute(object obj)
        {
            return model.Layout != null && SelectedWordCount > 0;
        }

        private void DeleteExecute(object obj)
        {
            view.EndAnimationsInProgress();
            view.DeleteSelection();
            view.FinalRefreshAfterUpdate();
        }



        private bool UndoCanExecute(object obj)
        {
            return UndoStack.CanUndo;
        }

        private void UndoExecute(object obj)
        {
            view.EndAnimationsInProgress();
            PerformUndo();
        }



        private bool SwapOrientationCanExecute(object obj)
        {
            return SelectedWordCount == 1;
        }

        private void SwapOrientationExecute(object obj)
        {
            view.EndAnimationsInProgress();
            view.SwapOrientation();
        }


        private bool AutoPlaceCanExecute(object obj)
        {
            return model.Layout != null && SelectedWordCount >= 1;
        }

        private void AutoPlaceExecute(object obj)
        {
            view.EndAnimationsInProgress();
            // Delegate work to view since we have no access to Sel here
            view.AutoPlace();
        }



        private void NewLayoutExecute(object obj)
        {
            model.NewGrille();
            ClearLayout();
        }

        private void QuitExecute(object obj)
        {
            Environment.Exit(0);
        }


        private void AboutExecute(object obj)
        {
            var aw = new View.AboutWindow();
            aw.ShowDialog();
        }

    }
}
