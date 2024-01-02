// NewGameViewModel
// ViewModel managing options for a new game
//
// 2024-01-02   PV

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwirkleUI;
internal class NewGameViewModel: INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    private readonly Model Model;

    public NewGameViewModel(Model model)
    {
        Model = model;
        _PlayersCount = Model.PlayersCount;
        _Player1Name = Model.Players.Length > 0 ? Model.Players[0].Name : "Joueur #1";
        _Player2Name = Model.Players.Length > 1 ? Model.Players[1].Name : "Joueur #2";
        _Player3Name = Model.Players.Length > 2 ? Model.Players[2].Name : "Joueur #3";
        _Player4Name = Model.Players.Length > 3 ? Model.Players[3].Name : "Joueur #4";
        _Player1IsComputer = Model.Players.Length > 0 && Model.Players[0].IsComputer;
        _Player2IsComputer = Model.Players.Length > 1 && Model.Players[1].IsComputer;
        _Player3IsComputer = Model.Players.Length > 2 && Model.Players[2].IsComputer;
        _Player4IsComputer = Model.Players.Length > 3 && Model.Players[3].IsComputer;
    }

    internal void ApplyValues()
    {
        Model.PlayersCount = _PlayersCount;
        Model.NewPlayers();
        Model.Players[0].Name = _Player1Name;
        Model.Players[0].IsComputer = _Player1IsComputer;
        if (_PlayersCount >= 2)
        {
            Model.Players[1].Name = _Player2Name;
            Model.Players[1].IsComputer = _Player2IsComputer;
        }
        if (_PlayersCount >= 3)
        {
            Model.Players[2].Name = _Player3Name;
            Model.Players[2].IsComputer = _Player3IsComputer;
        }
        if (_PlayersCount >= 4)
        {
            Model.Players[3].Name = _Player4Name;
            Model.Players[3].IsComputer = _Player4IsComputer;
        }
    }

    private int _PlayersCount;
    public int PlayersCount
    {
        get => _PlayersCount;
        set
        {
            if (_PlayersCount != value)
            {
                _PlayersCount = value;
                NotifyPropertyChanged(nameof(PlayersCount));
            }
        }
    }

    private string _Player1Name;
    public string Player1Name
    {
        get => _Player1Name;
        set
        {
            if (_Player1Name != value)
            {
                _Player1Name = value;
                NotifyPropertyChanged(nameof(Player1Name));
            }
        }
    }

    private string _Player2Name;
    public string Player2Name
    {
        get => _Player2Name;
        set
        {
            if (_Player2Name != value)
            {
                _Player2Name = value;
                NotifyPropertyChanged(nameof(Player2Name));
            }
        }
    }

    private string _Player3Name;
    public string Player3Name
    {
        get => _Player3Name;
        set
        {
            if (_Player3Name != value)
            {
                _Player3Name = value;
                NotifyPropertyChanged(nameof(Player3Name));
            }
        }
    }

    private string _Player4Name;
    public string Player4Name
    {
        get => _Player4Name;
        set
        {
            if (_Player4Name != value)
            {
                _Player4Name = value;
                NotifyPropertyChanged(nameof(Player4Name));
            }
        }
    }

    private bool _Player1IsComputer;
    public bool Player1IsComputer
    {
        get => _Player1IsComputer;
        set
        {
            if (_Player1IsComputer != value)
            {
                _Player1IsComputer = value;
                NotifyPropertyChanged(nameof(Player1IsComputer));
            }
        }
    }

    private bool _Player2IsComputer;
    public bool Player2IsComputer
    {
        get => _Player2IsComputer;
        set
        {
            if (_Player2IsComputer != value)
            {
                _Player2IsComputer = value;
                NotifyPropertyChanged(nameof(Player2IsComputer));
            }
        }
    }

    private bool _Player3IsComputer;
    public bool Player3IsComputer
    {
        get => _Player3IsComputer;
        set
        {
            if (_Player3IsComputer != value)
            {
                _Player3IsComputer = value;
                NotifyPropertyChanged(nameof(Player3IsComputer));
            }
        }
    }

    private bool _Player4IsComputer;
    public bool Player4IsComputer
    {
        get => _Player4IsComputer;
        set
        {
            if (_Player4IsComputer != value)
            {
                _Player4IsComputer = value;
                NotifyPropertyChanged(nameof(Player4IsComputer));
            }
        }
    }
}
