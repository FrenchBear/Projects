// Solitaire WPF
// NewGameOptionsWindow
// A quick-and-dirty WPF window to enter options for a new game
//
// 2019-04-21   PV
// 2020-12-19   PV      .Net 5, C#9, nullable enable
// 2021-11-13   PV      Net6 C#10
// 2023-11-20   PV      Net8 C#12

using System.Windows;

namespace Solitaire;

public partial class NewGameOptionsWindow: Window
{
    public NewGameOptionsWindow(NewGameOptionsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        Loaded += (s, e) =>
        {
            GameSerialTextBox.Focus();
            GameSerialTextBox.SelectAll();
        };
    }

    //private void OKButton_Click(object sender, RoutedEventArgs e)
    //{
    //    DialogResult = true;
    //}

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

}
