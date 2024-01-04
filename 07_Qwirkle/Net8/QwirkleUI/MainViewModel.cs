// QwirkleUI
// ViewModel of Main Window
//
// 2017-07-22   PV      First version
// 2023-11-20   PV      Net8 C#12

using LibQwirkle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static QwirkleUI.App;

namespace QwirkleUI;

enum MoveStatus
{
    Empty,
    Invalid,
    Valid,
}

internal class MainViewModel: INotifyPropertyChanged
{
    // Model and View
    private readonly Model Model;
    private readonly MainWindow View;

    // Game state
    private HandViewModel[] HandViewModels = [];
    internal readonly HashSet<UITileRowCol> CurrentMoves = [];
    internal MoveStatus CurrentMovesStatus = MoveStatus.Empty;
    private PlaySuggestion? PlaySuggestion;     // Suggestion with maximum possible scire with current hand
    internal HistoryActions HistoryActions = [];

    // Helpers
    public Player CurrentPlayer => Model.CurrentPlayer;
    public HandViewModel CurrentHandViewModel => HandViewModels[Model.PlayerIndex];

    // Helper to initialize HandViewModel, since model is common to all ViewModels
    public Model GetModel => Model;

    // Implementation of INotifyPropertyChanged, standard since View is only linked through DataBinding
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Commands

    // Menus
    public ICommand NewGameCommand { get; }
    public ICommand NewPlayersCommand { get; }
    public ICommand AutoPlayCommand { get; }
    public ICommand AutoPlayForeverCommand { get; }
    public ICommand QuitCommand { get; }

    // Edit
    public ICommand ValidateCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand ReturnSelectedTilesCommand { get; }
    public ICommand ReturnAllTilesCommand { get; }
    public ICommand SuggestPlayCommand { get; }
    public ICommand ExchangeTilesCommand { get; }

    // View
    public ICommand RescaleAndCenterCommand { get; }

    // About
    public ICommand AboutCommand { get; }

    // Constructor
    public MainViewModel(MainWindow view)
    {
        // Initialize ViewModel
        View = view;
        Model = new Model(/*this*/);

        // Binding commands with behavior

        // File
        NewGameCommand = new RelayCommand<object>(NewGameExecute);
        NewPlayersCommand = new RelayCommand<object>(NewPlayersExecute);
        AutoPlayCommand = new RelayCommand<object>(AutoPlayExecute);
        AutoPlayForeverCommand = new RelayCommand<object>(AutoPlayForeverExecute);
        QuitCommand = new RelayCommand<object>(QuitExecute);

        // Edit
        ValidateCommand = new RelayCommand<object>(ValidateExecute, ValidateCanExecute);
        UndoCommand = new RelayCommand<object>(UndoExecute, UndoCanExecute);
        ReturnSelectedTilesCommand = new RelayCommand<object>(ReturnSelectedTilesExecute, ReturnSelectedTilesCanExecute);
        ReturnAllTilesCommand = new RelayCommand<object>(ReturnAllTilesExecute, ReturnAllTilesCanExecute);
        SuggestPlayCommand = new RelayCommand<object>(SuggestPlayExecute, SuggestPlayCanExecute);
        ExchangeTilesCommand = new RelayCommand<object>(ExchangeTilesExecute, ExchangeTilesCanExecute);

        // View
        RescaleAndCenterCommand = new RelayCommand<object>(RescaleAndCenterExecute);

        // Help
        AboutCommand = new RelayCommand<object>(AboutExecute);
    }

    // -------------------------------------------------
    // Relays to model

    //internal void NewBoard()
    //    => Model.NewBoard();

    public BoundingRectangle Bounds()
    {
        int rowMin = Model.Board.RowMin;
        int colMin = Model.Board.ColMin;
        int rowMax = Model.Board.RowMax;
        int colMax = Model.Board.ColMax;

        if (CurrentMoves.Count > 0)
        {
            rowMin = Math.Min(rowMin, CurrentMoves.Min(m => m.Row));
            colMin = Math.Min(colMin, CurrentMoves.Min(m => m.Col));
            rowMax = Math.Max(rowMax, CurrentMoves.Max(m => m.Row));
            colMax = Math.Max(colMax, CurrentMoves.Max(m => m.Col));
        }

        return new(new RowCol(rowMin, colMin), new RowCol(rowMax, colMax));
    }

    internal void EvaluateCurrentMoves()
    {
        // If there is no move, provide hint about possible best play
        if (CurrentMoves.Count == 0)
        {
            CurrentMovesStatus = MoveStatus.Empty;

            if (CurrentPlayer.Hand.Count == 0)
            {
                StatusMessage = "Info: La main est vide, pas de jeu possible.";
                return;
            }

            if (PlaySuggestion == null)
                PlaySuggestion = Model.Board.Play(CurrentPlayer.Hand);

            if (PlaySuggestion.PB.Points == 0)
                StatusMessage = "Info: Aucune tuile jouable, échangez les tuiles.";
            else
                StatusMessage = $"Info: Il existe un placement à {PlaySuggestion.PB.Points} points.";
            return;
        }

        bool status;
        string msg;
        var moves = new Moves(CurrentMoves.Select(uitrc => new TileRowCol(uitrc.Tile, uitrc.RC)));
        (status, msg) = Model.EvaluateMoves(new Moves(moves));
        if (status)
        {
            PointsBonus pb = Model.CountPoints(moves);
            StatusMessage = $"Placement valide à {pb.Points} points";
            CurrentMovesStatus = MoveStatus.Valid;
        }
        else
        {
            StatusMessage = $"Problème: {msg}.";
            CurrentMovesStatus = MoveStatus.Invalid;
        }
    }

    // -------------------------------------------------
    // Bindings

    private string _StatusMessage = "";

    public string StatusMessage
    {
        get => _StatusMessage;
        set
        {
            if (_StatusMessage != value)
            {
                _StatusMessage = value;
                NotifyPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    private string _RoundNumber = "";
    public string RoundNumber
    {
        get => _RoundNumber;
        set
        {
            if (_RoundNumber != value)
            {
                _RoundNumber = value;
                NotifyPropertyChanged(nameof(RoundNumber));
            }
        }
    }

    private string _RemainingTiles = "";
    public string RemainingTiles
    {
        get => _RemainingTiles;
        set
        {
            if (_RemainingTiles != value)
            {
                _RemainingTiles = value;
                NotifyPropertyChanged(nameof(RemainingTiles));
            }
        }
    }

    // -------------------------------------------------
    // Undo support

    internal void PerformUndo()
    {
        View.BoardIM.EndAnimationsInProgress();

        if (CurrentMoves.Count > 0)
        {
            PerformDelete(true);
            return;
        }

        // Remove last action
        int na = HistoryActions.Count;
        Debug.Assert(na > 1);

        // Replay history
        bool initialized = false;
        foreach (var action in HistoryActions)
        {
            na--;
            if (action is HistoryActionBag actionBag)
            {
                if (!initialized)
                {
                    PerformNewGame(actionBag.Tiles);
                    for (int p = 0; p < Model.PlayersCount; p++)
                        HandViewModels[p].Score = "0";
                    initialized = true;
                }
                else
                {
                    if (na == 0)    // Don't perform last exchange
                        break;
                    Model.Bag.SetTiles(actionBag.Tiles);
                }
            }
            else if (action is HistoryActionMoves actionMoves)
            {
                PlaySuggestion = new PlaySuggestion(new Moves(actionMoves.Moves), new PointsBonus(actionMoves.Points, 99), []);
                PerformSuggestPlay(false);
                if (na == 0)    // Don't validate last move
                {
                    Model.UpdateRanks();
                    for (int p = 0; p < Model.PlayersCount; p++)
                        HandViewModels[p].Rank = Model.Players[p].Rank;
                    UpdateRemainingTiles();
                    break;
                }
                PerformValidate(true);
            }
            else
                Debug.Assert(false);
        }

        // Remove last action from history
        HistoryActions.RemoveLast();
    }

    private void UpdateRemainingTiles()
    {
        int r = Model.Bag.Tiles.Count;
        if (r == 0)
            RemainingTiles = "Toutes les tuiles ont été jouées";
        else if (r == 1)
            RemainingTiles = "Une tuile restante";
        else
            RemainingTiles = $"{r} tuiles restantes";
    }

    void RefillPlayerHand()
    {
        CurrentHandViewModel.RefillAndDrawHand();
        UpdateRemainingTiles();

        // Refilling player hand invalidates current PlaySuggestion
        PlaySuggestion = null;
    }

    // Validate command
    bool PerformValidate(bool rebuildHistory = false)
    {
        if (CurrentMoves.Count == 0)
            return false;

        View.BoardIM.EndAnimationsInProgress();

        for (; ; )
        {
            var moves = new Moves(CurrentMoves.Select(uitrc => new TileRowCol(uitrc.Tile, uitrc.RC)));

            // Count points and update display
            var pb = Model.Board.CountPoints(moves);
            Debug.Assert(pb.Points > 0);
            CurrentPlayer.Score += pb.Points;
            CurrentHandViewModel.Score = CurrentPlayer.Score.ToString();
            Model.UpdateRanks();
            for (int p = 0; p < Model.PlayersCount; p++)
                HandViewModels[p].Rank = Model.Players[p].Rank;

            Model.Board.AddMoves(moves, true);
            foreach (var move in CurrentMoves)
            {
                move.UIT.SelectionBorder = false;
                move.UIT.GrayBackground = false;
                Debug.Assert(CurrentPlayer.Hand.Contains(move.Tile));
                CurrentPlayer.Hand.Remove(move.Tile);
            }
            if (!rebuildHistory)
                HistoryActions.AddLast(new HistoryActionMoves(moves, pb.Points));
            CurrentMoves.Clear();
            CurrentMovesStatus = MoveStatus.Empty;
            StatusMessage = string.Empty;

            RefillPlayerHand();

            if (!NextPlayer())
            {
                StatusMessage = "Info: Le jeu est terminé.";
                return false;
            }

            EvaluateCurrentMoves();

            if (!CurrentPlayer.IsComputer)
                break;

            int res = PerformSuggestPlay(false);

            if (res != 0)
                break;
        }

        return true;
    }

    // Returns false once game has ended (at least one player has an empty hand) and current player is last player
    bool NextPlayer()
    {
        bool endGame = Model.Players.Any(pl => pl.Hand.Count == 0);
        if (endGame && Model.PlayerIndex == Model.PlayersCount - 1)
            return false;

        if (Model.PlayersCount > 1)
        {
            var oldIndex = Model.PlayerIndex;
            Model.PlayerIndex = (Model.PlayerIndex + 1) % Model.PlayersCount;
            HandViewModels[oldIndex].UpdateTitleBrush();
            HandViewModels[Model.PlayerIndex].UpdateTitleBrush();
        }

        if (Model.PlayerIndex == 0)
            Model.RoundNumber++;

        if (endGame)
            RoundNumber = $"Tour #{Model.RoundNumber} (dernier tour)";
        else
            RoundNumber = $"Tour #{Model.RoundNumber}";

        return true;
    }

    // Delete command moves selection back to player hand
    // CurrentPlayer.Hand is not updated untit current moves are validated
    // Note that EvaluateCurrentMoves is not called here since this is used as a subprogram for Suggestion
    internal void PerformDelete(bool allCurrentMoves)
    {
        if (allCurrentMoves)
        {
            if (CurrentMoves.Count == 0)
                return;
        }
        else
        {
            if (View.BoardIM.Selection.IsEmpty)
                return;
        }

        View.BoardIM.EndAnimationsInProgress();

        foreach (var uitrc in new List<UITileRowCol>(allCurrentMoves ? CurrentMoves : View.BoardIM.Selection))
        {
            // Add to Player Hand
            CurrentHandViewModel.AddAndDrawTile(uitrc.Tile);

            // Remove from CurrentMoves and DrawingCanvas
            var todel = CurrentMoves.FirstOrDefault(item => item.UIT == uitrc.UIT);
            Debug.Assert(todel != null);
            CurrentMoves.Remove(todel);
            View.BoardDrawingCanvasRemoveUITile(uitrc.UIT);
        }
    }

    internal void PerformExchangeTiles()
    {
        if (Model.Bag.IsEmpty || CurrentPlayer.Hand.Count == 0)
            return;

        // Repeat exchange until we get something playable
        for (; ; )
        {
            Model.Bag.ReturnTiles(CurrentPlayer.Hand);
            var newBagTiles = new List<Tile>(Model.Bag.Tiles);      // Keep a copy before refilling player hand
            CurrentPlayer.Hand.Clear();
            CurrentHandViewModel.RemoveAllUITiles();
            RefillPlayerHand();
            EvaluateCurrentMoves();

            if (PlaySuggestion!.PB.Points > 0)
            {
                HistoryActions.AddLast(new HistoryActionBag(newBagTiles));
                return;
            }
        }
    }

    // Suggest a tiles placement
    // Returns true if a valid suggestion has been made, false otherwise
    internal int PerformSuggestPlay(bool interactive = true)
    {
        // First, move all tiles back to hand
        View.BoardIM.Selection.Clear();
        PerformDelete(true);

        // ToDo: Review this, actually as soon as one hand is empty after validation, last turn mode begins.
        // Once last player has played, it's endgame, so in theory we shouldn't meet this case
        if (CurrentPlayer.Hand.Count == 0)
        {
            if (interactive)
                MessageBox.Show("Désolé, la main est vide!", AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return 1;
        }

        Debug.Assert(PlaySuggestion != null);
        if (PlaySuggestion.PB.Points == 0)
        {
            if (interactive)
                MessageBox.Show("Désolé, aucune tuile ne peut être jouée. Échangez les tuiles!", AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return 2;
        }

        // Delete from hand, add to Board
        foreach (var trc in PlaySuggestion.Moves)
        {
            // Delete from Hand
            CurrentHandViewModel.RemoveUITileFromTile(trc.Tile);
            // Add to Board
            var uitile = View.BoardDrawingCanvasAddUITile(trc.Tile, trc.RC, true);
            CurrentMoves.Add(uitile);
        }

        EvaluateCurrentMoves();
        View.RescaleAndCenter(true);
        return 0;
    }

    private void PerformAutoPlay(bool autoRestartNewGame)
    {
        for (; ; )
        {
            for (; ; )
            {
                int res = PerformSuggestPlay(false);
                if (res == 2)
                {
                    PerformExchangeTiles();
                    continue;
                }
                if (res == 1)
                    break;
                if (!PerformValidate())
                    break;
                View.Refresh();
            }

            if (!autoRestartNewGame)
                return;
            PerformNewGame();
        }
    }

    // Remove from Model and HandViewModel
    internal void RemoveUITileFromHand(UITile uit)
        => CurrentHandViewModel.RemoveUITile(uit);

    // -------------------------------------------------

    // Keep existing players
    internal void PerformNewGame(List<Tile>? initialBagTiles = null)
    {
        Model.NewBoard(initialBagTiles);
        View.BoardDrawingCanvasRemoveAllUITiles();
        CurrentMoves.Clear();
        RoundNumber = "Tour #1";

        if (initialBagTiles == null)    // Don't clear or update history while replaying it
        {
            HistoryActions.Clear();
            Debug.Assert(Model.Bag.Tiles.Count == 3 * 6 * 6);
            HistoryActions.AddLast(new HistoryActionBag(new List<Tile>(Model.Bag.Tiles)));
        }

        for (int i = 0; i < Model.Players.Length; i++)
        {
            HandViewModels[i].RemoveAllUITiles();
            Model.Players[i].Hand.Clear();
            Model.Players[i].Score = 0;
            HandViewModels[i].RefillAndDrawHand();
        }

        UpdateRemainingTiles();

        View.RescaleAndCenter(true, true);
        PlaySuggestion = null;
        EvaluateCurrentMoves();
    }

    // Update players, then PerformNewGame()
    internal void PerformNewPlayers()
    {
        var ng = new NewGameWindow(Model);
        ng.Owner = View;
        var res = ng.ShowDialog() ?? false;
        if (!res)
        {
            Application.Current.Shutdown(0);
            return;
        }

        //MessageBox.Show($"Dialog: {res}  Players count: {Model.PlayersCount}");

        Debug.Assert(Model.PlayersCount >= 1 && Model.PlayersCount <= 4);

        HandViewModels = new HandViewModel[Model.PlayersCount];
        View.Player1HandUserControl.Visibility = Visibility.Visible;
        HandViewModels[0] = new HandViewModel(View, View.Player1HandUserControl, Model, 0);

        if (Model.PlayersCount > 1)
        {
            View.Player2HandUserControl.Visibility = Visibility.Visible;
            HandViewModels[1] = new HandViewModel(View, View.Player2HandUserControl, Model, 1);
        }
        else
            View.Player2HandUserControl.Visibility = Visibility.Collapsed;

        if (Model.PlayersCount > 2)
        {
            View.Player3HandUserControl.Visibility = Visibility.Visible;
            HandViewModels[2] = new HandViewModel(View, View.Player3HandUserControl, Model, 2);
        }
        else
            View.Player3HandUserControl.Visibility = Visibility.Collapsed;

        if (Model.PlayersCount > 3)
        {
            View.Player4HandUserControl.Visibility = Visibility.Visible;
            HandViewModels[3] = new HandViewModel(View, View.Player4HandUserControl, Model, 3);
        }
        else
            View.Player4HandUserControl.Visibility = Visibility.Collapsed;

        PerformNewGame();
    }

    // -------------------------------------------------
    // View helpers

    // ToDo: who's calling?
    //internal void InitializeBoard() => Model.InitializeBoard();

    // Draw board placed tiles with a dark background
    // ToDo: Only for dev I think
    internal void DrawBoard()
    {
        foreach (TileRowCol m in Model.Board)
            View.BoardDrawingCanvasAddUITile(m.Tile, new RowCol(m.Row, m.Col), false);
    }

    // For dev, draw test tiles with gray background
    internal void DrawCurrentMoves()
    {
        foreach (UITileRowCol m in CurrentMoves)
            CurrentMoves.Add(View.BoardDrawingCanvasAddUITile(m.Tile, m.RC, true));
    }

    internal void UpdateCurrentMoves(UITilesSelection selection)
    {
        foreach (UITileRowCol uitp in selection)
        {
            bool found = false;
            foreach (UITileRowCol m in CurrentMoves)
            {
                if (m.Tile == uitp.Tile)
                {
                    RemoveCurrentMove(m);
                    found = true;
                    break;
                }
            }
            Debug.Assert(found);
            CurrentMoves.Add(new UITileRowCol(uitp.UIT, uitp.RC));
        }
    }

    internal void RemoveCurrentMove(UITileRowCol m)
    {
        Debug.Assert(CurrentMoves.Contains(m));
        CurrentMoves.Remove(m);
    }

    internal CellState GetCellState(int row, int col)
        => Model.Board.GetCellState(row, col);

    // -------------------------------------------------
    // Commands

    // Since the command is in the view, MainWindow.xaml should directly reference command in the view, no need for this ViewModel intermediate...
    private void RescaleAndCenterExecute(object obj)
        => View.RescaleAndCenter(true, true);        // Use animations

    // -----------------------------------

    private bool ValidateCanExecute(object obj) => CurrentMovesStatus == MoveStatus.Valid;

    private void ValidateExecute(object obj) => PerformValidate();

    // -----------------------------------

    private bool UndoCanExecute(object obj) => HistoryActions.Count > 1 || CurrentMoves.Count > 0;    // Since initial bag is recorded as first action, don't consider it's enough to undo

    private void UndoExecute(object obj) => PerformUndo();

    // -----------------------------------

    private bool ReturnSelectedTilesCanExecute(object obj) => !View.BoardIM.Selection.IsEmpty;

    private void ReturnSelectedTilesExecute(object obj)
    {
        PerformDelete(false);
        EvaluateCurrentMoves();
    }

    // -----------------------------------

    private bool ReturnAllTilesCanExecute(object obj) => CurrentMoves.Count > 0;

    private void ReturnAllTilesExecute(object obj)
    {
        PerformDelete(true);
        EvaluateCurrentMoves();
    }

    // -----------------------------------

    private bool SuggestPlayCanExecute(object obj) => Model.Players.Length != 0 && CurrentPlayer.Hand.Count > 0;

    private void SuggestPlayExecute(object obj)
    {
        PlaySuggestion = Model.Board.Play(CurrentPlayer.Hand);      // Generate a new suggestion each time wi click on button or hit f1, ignoring PlaySuggestion already computed
        PerformSuggestPlay();
    }

    // -----------------------------------

    private bool ExchangeTilesCanExecute(object obj) => !Model.IsBagEmpty;

    private void ExchangeTilesExecute(object obj) => PerformExchangeTiles();

    // -----------------------------------

    private static bool IsShiftPressed
        => System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) ||
           System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);

    private void NewGameExecute(object obj)
    {
        if (IsShiftPressed)
            PerformNewPlayers();
        else
            PerformNewGame();
    }

    private void NewPlayersExecute(object obj) => PerformNewPlayers();

    private void AutoPlayExecute(object obj) => PerformAutoPlay(false);

    private void AutoPlayForeverExecute(object obj) => PerformAutoPlay(true);

    private void QuitExecute(object obj) => Environment.Exit(0);

    private void AboutExecute(object obj) => new AboutWindow().ShowDialog();
}
