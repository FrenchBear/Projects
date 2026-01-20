// UITile User Control
// Visual representation of a Qwirkle tile
//
// 2023-12-11   PV
// 2023-12-28   PV      Integrate Tile
// 2026-01-20	PV		Net10 C#14

using LibQwirkle;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QwirkleUI;

[DebuggerDisplay("UITile: Tile={Tile} GB={GrayBackground} SB={SelectionBorder} Hatched={Hatched}")]
public partial class UITile: UserControl
{
    public UITile(Tile t)
    {
        InitializeComponent();
        Tile = t;
        ShapeColorLayer.Fill = Resources[t.ShapeColor] as Brush;
    }

    public Tile Tile { get; init; }

    public bool GrayBackground
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                TileLayer.Fill = Resources[value ? "GrayTile" : "BlackTile"] as Brush;
            }
        }
    }

    public bool SelectionBorder
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SelectionLayer.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }

    public bool Hatched
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                HatchLayer.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }

    public override string ToString() => $"UITile: SC={Tile.ShapeColor} I={Tile.Instance} GB={GrayBackground} SB={SelectionBorder} Hatched={Hatched}";
}
