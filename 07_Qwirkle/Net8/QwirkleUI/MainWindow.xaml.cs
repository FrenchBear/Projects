// QwirkleUI
// WPF interface for Qwirkle project
//
// 2023-12-10   PV      First version, convert SVG tiles from https://fr.wikipedia.org/wiki/Qwirkle in XAML using Inkscape

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace QwirkleUI;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow: Window
{
    public enum Shape
    {
        Circle,
        Cross,
        Lozange,
        Square,
        Star,
        Clover,
    }

    public enum Color
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple,
    }

    static bool EventsActive = false;

    public MainWindow()
    {
        InitializeComponent();
        foreach (var shape in Enum.GetValues(typeof(Shape)))
            ShapesCombo.Items.Add(shape);
        foreach (var color in Enum.GetValues(typeof(Color)))
            ColorsCombo.Items.Add(color);
        ShapesCombo.SelectedIndex = 0;
        ColorsCombo.SelectedIndex = 0;
        EventsActive = true;
    }

    private void GrayBackgroundCheckBox_Click(object sender, RoutedEventArgs e) 
        => TestTile.GrayBackground = GrayBackgroundCheckBox.IsChecked ?? false;

    private void SelectionBorderCheckBox_Click(object sender, RoutedEventArgs e) 
        => TestTile.SelectionBorder = SelectionBorderCheckBox.IsChecked ?? false;

    private void ShapesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EventsActive)
            TestTile.ShapeColor = ShapesCombo.SelectedItem.ToString() + ColorsCombo.SelectedItem.ToString();
    }

    private void ColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EventsActive)
            TestTile.ShapeColor = ShapesCombo.SelectedItem.ToString() + ColorsCombo.SelectedItem.ToString();
    }

}