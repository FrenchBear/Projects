// NewGameWindow
// Selects options for a new game
//
// 2024-01-02   PV

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
