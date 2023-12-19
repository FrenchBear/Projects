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
    private readonly int HandIndex;

    // Implementation of INotifyPropertyChanged, standard since View is only linked through DataBinding
    public event PropertyChangedEventHandler? PropertyChanged;

    // Constructor
    public HandViewModel(HandUserControl view, Model model, int handIndex)
    {
        Model = model;
        View = view;
        HandIndex = handIndex;

        view.SetViewModel(this);
    }

    private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    internal void DrawHand()
    {
        int c = 0;
        foreach (Tile t in Model.Hands[HandIndex])
            View.AddUITile(t.Shape.ToString() + t.Color.ToString(), t.Instance, new RowCol(0, c++));
    }
}