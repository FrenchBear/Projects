// SettingsWindow
// Select genberal game options
//
// 2024-01-06   PV

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

public enum HintOptionsEnum
{
    ShowBestPlay,
    ShowExcellent,
    ShowNothing,
}

public partial class SettingsWindow : Window
{
    private readonly GameSettings GS;

    public SettingsWindow(GameSettings gs)
    {
        InitializeComponent();
        GS = gs;
        (gs.BestPlayHint switch { 
            HintOptionsEnum.ShowBestPlay => BestPlayHintShowBestPlayOption,
            HintOptionsEnum.ShowExcellent => BestPlayHintShowExcellentOption,
            _ => BestPlayHintShowNothingOption
        }).IsChecked = true;
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        GS.BestPlayHint = 
            BestPlayHintShowBestPlayOption.IsChecked ?? false ? HintOptionsEnum.ShowBestPlay :
            BestPlayHintShowExcellentOption.IsChecked ?? false ? HintOptionsEnum.ShowExcellent :
            HintOptionsEnum.ShowNothing;

        // Persist options for next launch
        Properties.Settings.Default["BestPlayHint"] = Enum.GetName(typeof(HintOptionsEnum), GS.BestPlayHint);
        Properties.Settings.Default.Save();

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;

    // Double click on option validates dialog
    private void Option_MouseDoubleClick(object sender, MouseButtonEventArgs e) 
        => OKButton_Click(sender, new RoutedEventArgs());
}

public class GameSettings
{
    public HintOptionsEnum BestPlayHint { get; set; }
}