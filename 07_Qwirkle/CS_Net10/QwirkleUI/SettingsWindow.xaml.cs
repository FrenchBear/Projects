// SettingsWindow
// Select genberal game options
//
// 2024-01-06   PV
// 2026-01-20	PV		Net10 C#14

using System;
using System.Windows;
using System.Windows.Input;

namespace QwirkleUI;

public enum HintOptionsEnum
{
    ShowBestPlay,
    ShowExcellent,
    ShowNothing,
}

public partial class SettingsWindow: Window
{
    private readonly GameSettings GS;

    public SettingsWindow(GameSettings gs)
    {
        InitializeComponent();
        GS = gs;
        (gs.BestPlayHint switch
        {
            HintOptionsEnum.ShowBestPlay => BestPlayHintShowBestPlayOption,
            HintOptionsEnum.ShowExcellent => BestPlayHintShowExcellentOption,
            _ or HintOptionsEnum.ShowNothing => BestPlayHintShowNothingOption,
        }).IsChecked = true;
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        GS.BestPlayHint =
            BestPlayHintShowBestPlayOption.IsChecked ?? false ? HintOptionsEnum.ShowBestPlay :
            BestPlayHintShowExcellentOption.IsChecked ?? false ? HintOptionsEnum.ShowExcellent :
            HintOptionsEnum.ShowNothing;

        // Persist options for next launch
        Properties.Settings.Default["BestPlayHint"] = Enum.GetName(GS.BestPlayHint);
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