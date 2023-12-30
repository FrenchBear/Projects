// QwirkleUI
// ViewModel of Hand UserControl
//
// 2023-12-17   PV      First version

using LibQwirkle;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using static QwirkleUI.App;

namespace QwirkleUI;

internal class HandViewModel: INotifyPropertyChanged
{
    // View and Model
    private readonly HandUserControl View;
    private readonly Model Model;
    private readonly int PlayerIndex;
    internal readonly HashSet<UITileRowCol> UIHand = [];

    // Implementation of INotifyPropertyChanged, standard since View is only linked through DataBinding
    public event PropertyChangedEventHandler? PropertyChanged;

    // Constructor
    public HandViewModel(MainWindow mainWindow, HandUserControl view, Model model, int playerIndex)
    {
        Model = model;
        View = view;
        PlayerIndex = playerIndex;

        view.SetViewModelAndMainWindow(mainWindow, this);

        PlayerName = $"Joueur {PlayerIndex}";
    }

    private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // -------------------------------------------------
    // Bindings

    // ToDo: retrieve info from Model
    private string _PlayerName = "";
    public string PlayerName
    {
        get => _PlayerName;
        set
        {
            if (_PlayerName != value)
            {
                _PlayerName = value;
                NotifyPropertyChanged(nameof(PlayerName));
            }
        }
    }

    private string _Score = "0";
    public string Score
    {
        get => _Score;
        set
        {
            if (_Score != value)
            {
                _Score = value;
                NotifyPropertyChanged(nameof(Score));
            }
        }
    }

    private string _Rank = "-";
    public string Rank
    {
        get => _Rank;
        set
        {
            if (_Rank != value)
            {
                _Rank = value;
                NotifyPropertyChanged(nameof(Rank));
            }
        }
    }

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

    // -------------------------------------------------

    internal void DrawHand()
    {
        foreach (Tile t in Model.Players[PlayerIndex].Hand)
            AddAndDrawTile(t);
    }

    internal void AddAndDrawTile(Tile tile)
    {
        // Find a free cell in hand
        int row, col = 0;
        for (row = 0; row < HandRows; row++)
            for (col = 0; col < HandColumns; col++)
                if (!UIHand.Any(uitrc => uitrc.RC.Row == row && uitrc.RC.Col == col))
                    goto ExitDoubleLoop;

                // Guaranteed to find one
                ExitDoubleLoop:
        View.HandAddUITile(tile, new RowCol(row, col));
    }

    internal void RemoveUITile(UITile uit)
    {
        var todel = UIHand.FirstOrDefault(item => item.UIT == uit);
        Debug.Assert(todel != null);
        Debug.Assert(UIHand.Contains(todel));
        UIHand.Remove(todel);
        View.RemoveUITileFromHandView(uit);
    }

    internal void RemoveUITileFromTile(Tile t)
        => RemoveUITile(UIHand.First(item => item.UIT.Tile == t).UIT);
}