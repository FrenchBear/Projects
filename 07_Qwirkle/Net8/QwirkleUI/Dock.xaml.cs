// Dock UserControl
// Reprsent a player dock
//
// 2023-12-12   PV

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static QwirkleUI.App;
using LibQwirkle;
using System.Diagnostics;

namespace QwirkleUI;
/// <summary>
/// Interaction logic for PlayerDock.xaml
/// </summary>
public partial class Dock: UserControl
{
    public Dock()
    {
        InitializeComponent();

        var h1 = new Tile(LibQwirkle.Shape.Lozange, LibQwirkle.Color.Blue, 1);
        var h2 = new Tile(LibQwirkle.Shape.Square, LibQwirkle.Color.Blue, 1);
        var h3 = new Tile(LibQwirkle.Shape.Star, LibQwirkle.Color.Blue, 1);
        var h4 = new Tile(LibQwirkle.Shape.Star, LibQwirkle.Color.Yellow, 1);
        var h5 = new Tile(LibQwirkle.Shape.Square, LibQwirkle.Color.Purple, 1);
        var h6 = new Tile(LibQwirkle.Shape.Square, LibQwirkle.Color.Green, 1);

        var Hand = new Tile?[8];
        Hand[0] = h1;
        Hand[1] = h2;
        Hand[2] = h3;
        Hand[3] = h4;
        Hand[4] = h5;
        Hand[5] = h6;
        Hand[6] = null;
        Hand[7] = null;

        for (int i = 0; i < 8; i++)
        {
            Tile? t = Hand[i];
            if (t != null)
                AddUITile(i, t.Shape.ToString() + t.Color.ToString());
        }
    }

    internal void AddUITile(int col, string shapeColor)
    {
        var t = new UITile();
        t.ShapeColor = shapeColor;
        t.SetValue(Canvas.TopProperty, 10.0);
        t.SetValue(Canvas.LeftProperty, 10.0 + col * UnitSize);
        t.Width = UnitSize;
        t.Height = UnitSize;
        DockDrawingCanvas.Children.Add(t);
    }

    private void Dock_MouseMoveWhenUp(object sender, MouseEventArgs e)
    {
        //
    }

    private void Dock_MouseDown(object sender, MouseButtonEventArgs e)
    {
        //
    }

    private void Dock_MouseUp(object sender, MouseButtonEventArgs e)
    {
        //
    }

    private void Dock_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        //
    }

    private void Dock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        //
    }
}
