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
using LibQwirkle;
using System.Diagnostics;
using static QwirkleUI.App;
using static QwirkleUI.ViewHelpers;

namespace QwirkleUI;

// record: reference object like a class, but with comparing and hashing like a struct
internal record UITileAndSlot(UITile UIT, int Slot);

internal readonly struct DockSelection
{
    private readonly List<UITileAndSlot> _items = [];

    // A compiler requirement...
    public DockSelection() { }

    public readonly void Clear()
    {
        foreach (var item in _items)
            item.UIT.SelectionBorder = false;
        _items.Clear();
    }

    public readonly IReadOnlyList<UITileAndSlot> Items
        => _items;

    public readonly bool IsEmpty => _items.Count > 0;

    internal readonly bool Contains(UITileAndSlot hit)
        => _items.Contains(hit);

    internal readonly void Add(UITileAndSlot hit)
    {
        Debug.Assert(!_items.Contains(hit));
        Debug.Assert(hit.UIT.SelectionBorder == false);
        _items.Add(hit);
        hit.UIT.SelectionBorder = true;
    }

    internal readonly void Remove(UITileAndSlot hit)
    {
        Debug.Assert(_items.Contains(hit));
        Debug.Assert(hit.UIT.SelectionBorder == true);
        _items.Remove(hit);
        hit.UIT.SelectionBorder = false;
    }
}

public partial class Dock: UserControl
{
    private readonly DockSelection Selection = new();

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
                AddUITile(t.Shape.ToString() + t.Color.ToString(), i);
        }
    }

    internal void AddUITile(string shapeColor, int slot)
    {
        var t = new UITile();
        t.ShapeColor = shapeColor;
        t.GrayBackground = true;
        t.SetValue(Canvas.TopProperty, 10.0);
        t.SetValue(Canvas.LeftProperty, 10.0 + slot * UnitSize);
        t.Width = UnitSize;
        t.Height = UnitSize;
        DockDrawingCanvas.Children.Add(t);
    }

    // --------------------------------------------------------------------
    // Mouse click and drag management

    private Point previousMousePosition;

    // null indicates background grid move, or delegate must be executed by MouseMove to perform move
    // action, P is current mouse coordinates in non-transformed user space
    private Action<Point>? pmm;

    private void DockCanvas_MouseMoveWhenUp(object sender, MouseEventArgs e)
    {
        // %aybe some visual hinting
    }

    // Returs false if no UITile has been hit
    private bool UpdateSelectionAfterClick(MouseButtonEventArgs e)
    {
        // If no dock tile is hit, just clear selection and return
        UITile? t = GetHitHile(e, DockCanvas);
        if (t == null)
        {
            Selection.Clear();
            return false;
        }

        int slot = (int)Math.Floor(((double)t.GetValue(Canvas.LeftProperty) - 10.0) / UnitSize + 0.5);
        var hit = new UITileAndSlot(t, slot);

        // If Ctrl key is NOT pressed, clear previous selection
        // But if we click again in something already selected, do not clear selection!
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            if (!Selection.Contains(hit))
                Selection.Clear();

        // Add current hit to selection
        if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Selection.Contains(hit))
        {
            Selection.Remove(hit);
            if (Selection.IsEmpty)
                return false;
        }
        else
            if (!Selection.Contains(hit))
                Selection.Add(hit);

        // Remove and add again elements to move so they're displayed above non-moved elements
        foreach (UITileAndSlot item in Selection.Items)
        {
            DockDrawingCanvas.Children.Remove(item.UIT);
            DockDrawingCanvas.Children.Add(item.UIT);
            item.UIT.SetValue(Canvas.TopProperty, 10.0);
            item.UIT.SetValue(Canvas.LeftProperty, 10.0 + item.Slot * UnitSize);
        }

        return true;
    }

    private void DockCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        //EndAnimationsInProgress();

        DockCanvas.MouseMove -= DockCanvas_MouseMoveWhenUp;
        DockCanvas.MouseMove += DockCanvas_MouseMoveWhenDown;
        previousMousePosition = e.GetPosition(DockCanvas);
        bool tileHit = UpdateSelectionAfterClick(e);

        if (tileHit)
            pmm = GetMouseDownMoveAction();
        else
            pmm = null;

        // Be sure to call GetPosition before Capture, otherwise GetPosition returns 0 after Capture
        // Capture to get MouseUp event raised by grid
        Mouse.Capture(DockCanvas);
    }
    private Action<Point>? GetMouseDownMoveAction()
    {
        return null;
    }

    private void DockCanvas_MouseMoveWhenDown(object sender, MouseEventArgs e)
    {
        //
    }

    private void DockCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Mouse.Capture(null);
        DockCanvas.MouseMove -= DockCanvas_MouseMoveWhenDown;
        DockCanvas.MouseMove += DockCanvas_MouseMoveWhenUp;
    }

    private void DockCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        //
    }

    private void DockCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        //
    }
}
