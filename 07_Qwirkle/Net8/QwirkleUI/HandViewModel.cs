// QwirkleUI
// ViewModel of Hand UserControl
//
// 2023-12-17   PV      First version

using LibQwirkle;
using System.ComponentModel;

namespace QwirkleUI;

internal class HandViewModel: INotifyPropertyChanged
{
    // View and Model
    private readonly HandUserControl View;
    private readonly Model Model;
    private readonly int PlayerIndex;

    // Implementation of INotifyPropertyChanged, standard since View is only linked through DataBinding
    public event PropertyChangedEventHandler? PropertyChanged;

    // Constructor
    public HandViewModel(MainWindow mainWindow, HandUserControl view, Model model, int playerIndex)
    {
        Model = model;
        View = view;
        PlayerIndex = playerIndex;

        view.SetViewModelAndMainWindow(mainWindow, this);
    }

    private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    internal void DrawHand()
    {
        int c = 0;
        foreach (Tile t in Model.Players[PlayerIndex].Hand)
            View.HandAddUITile(t, new RowCol(0, c++));
    }
}