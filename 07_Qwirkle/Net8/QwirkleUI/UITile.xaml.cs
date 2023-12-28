// UITile User Control
// Visual representation of a Qwirkle tile
//
// 2023-12-11   PV
// 2023-12-28   PV      Integrate Tile

using LibQwirkle;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QwirkleUI;

[DebuggerDisplay("UITile: SC={ShapeColor} I={Instance} GB={GrayBackground} SB={SelectionBorder} Hatched={Hatched}")]
public partial class UITile: UserControl
{
    public UITile(Tile t)
    {
        InitializeComponent();
        Tile = t;
        ShapeColorLayer.Fill = Resources[t.ShapeColor] as Brush;
    }

    public Tile Tile { get; init; }

    private bool _GrayBackground = false;

    public bool GrayBackground
    {
        get => _GrayBackground;
        set
        {
            if (_GrayBackground != value)
            {
                _GrayBackground = value;
                TileLayer.Fill = Resources[value ? "GrayTile" : "BlackTile"] as Brush;
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

    private bool _Hatched = false;

    public bool Hatched
    {
        get => _Hatched;
        set
        {
            if (_Hatched != value)
            {
                _Hatched = value;
                HatchLayer.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }

    public override string ToString() => $"UITile: SC={Tile.ShapeColor} I={Tile.Instance} GB={GrayBackground} SB={SelectionBorder} Hatched={Hatched}";
}
