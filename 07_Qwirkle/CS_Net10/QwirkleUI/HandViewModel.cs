// QwirkleUI
// ViewModel of Hand UserControl
//
// 2023-12-17   PV      First version
// 2026-01-20	PV		Net10 C#14

using LibQwirkle;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using static QwirkleUI.App;

namespace QwirkleUI;

internal sealed class HandViewModel: INotifyPropertyChanged
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

        view.SetMainWindowAndViewModel(mainWindow, this);
        PlayerName = ThisPlayer.Name;
    }

    private Player ThisPlayer => Model.Players[PlayerIndex];

    private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // -------------------------------------------------
    // Bindings

    // ToDo: retrieve info from Model
    public string PlayerName
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                NotifyPropertyChanged(nameof(PlayerName));
            }
        }
    } = "";

    public string Score
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                NotifyPropertyChanged(nameof(Score));
            }
        }
    } = "0";

    public string Rank
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                NotifyPropertyChanged(nameof(Rank));
            }
        }
    } = "";

    public Brush TitleBrush => Model.PlayerIndex == PlayerIndex ? Brushes.LightCoral : Brushes.Transparent;
    internal void UpdateTitleBrush() => NotifyPropertyChanged(nameof(TitleBrush));

    // -------------------------------------------------

    //internal void InitPlayerNameAndScore()
    //{
    //    PlayerName = ThisPlayer.Name;
    //    Score = ThisPlayer.Score.ToString();
    //}

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
        View.AddUITile(tile, new RowCol(row, col));
    }

    internal void RemoveUITile(UITile uit)
    {
        var todel = UIHand.FirstOrDefault(item => item.UIT == uit);
        Debug.Assert(todel != null);
        Debug.Assert(UIHand.Contains(todel));
        UIHand.Remove(todel);
        View.RemoveUITile(uit);
    }

    internal void RemoveUITileFromTile(Tile t)
        => RemoveUITile(UIHand.First(item => item.UIT.Tile == t).UIT);

    internal void RemoveAllUITiles()
    {
        UIHand.Clear();
        View.RemoveAllUITiles();
    }

    internal void RefillAndDrawHand()
    {
        while (!Model.Bag.IsEmpty && UIHand.Count < 6)
        {
            var t = Model.Bag.GetTile();
            AddAndDrawTile(t);
            ThisPlayer.Hand.Add(t);
        }
    }
}