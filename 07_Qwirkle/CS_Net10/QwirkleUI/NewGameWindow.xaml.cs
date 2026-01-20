// NewGameWindow
// Selects options for a new game
//
// 2024-01-02   PV
// 2026-01-20	PV		Net10 C#14

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

        Loaded += (s, e) =>
        {
            Player1Name.Focus();
            Player1Name.SelectAll();
            Player2Name.SelectAll();
            Player3Name.SelectAll();
            Player4Name.SelectAll();
        };
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ApplyValues();
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}
