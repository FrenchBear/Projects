// NewGameWindow
// Selects options for a new game
//
// 2024-01-02   PV

using System.Windows;

namespace QwirkleUI;

public partial class NewGameWindow: Window
{
    private readonly NewGameViewModel ViewModel;

    internal NewGameWindow(Model model)
    {
        InitializeComponent();

        ViewModel = new(model);
        DataContext = ViewModel;
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ApplyValues();
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}
