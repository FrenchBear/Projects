// UITile User Control
// Visual representation of a Qwirkle tile
//
// 2023-12-11   PV

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QwirkleUI;

public partial class UITile: UserControl
{
    private bool _GrayBackground = false;

    public bool GrayBackground
    {
        get => _GrayBackground;
        set
        {
            if (_GrayBackground != value)
            {
                _GrayBackground = value;
                TileLayer.Fill = Resources[value ? "GreyTile" : "BlackTile"] as Brush;
            }
        }
    }

    private bool _SelectionBorder = false;

    public bool SelectionBorder
    {
        get => _SelectionBorder;
        set
        {
            if (_SelectionBorder != value)
            {
                _SelectionBorder = value;
                SelectionLayer.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }

    private string _ShapeColor = "CircleRed";
    public string ShapeColor
    {
        get => _ShapeColor;
        set
        {
            if (_ShapeColor != value)
            {
                _ShapeColor = value;
                ShapeColorLayer.Fill = Resources[_ShapeColor] as Brush;
            }
        }
    }

    public UITile() => InitializeComponent();
}
