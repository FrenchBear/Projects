// Hand UserControl
// Reprsent a player Hand
//
// 2023-12-12   PV

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using LibQwirkle;
using System.Diagnostics;
using static QwirkleUI.App;
using static QwirkleUI.ViewHelpers;
using System.Collections;

namespace QwirkleUI;

// record: reference object like a class, but with comparing and hashing like a struct
internal record UITilePosition(UITile UIT, int Row, int Col)
{
    public double StartTop;
    public double StartLeft;
    public Vector ClickOffset;
}

internal readonly struct HandSelection: IReadOnlyCollection<UITilePosition>
{
    private readonly List<UITilePosition> _items = [];

    // A compiler requirement...
    public HandSelection() { }

    public readonly void Clear()
    {
        foreach (var item in _items)
            item.UIT.SelectionBorder = false;
        _items.Clear();
    }

    public IEnumerator<UITilePosition> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

    public readonly bool IsEmpty => _items.Count > 0;

    public readonly int Count => _items.Count;

    internal readonly bool Contains(UITilePosition hit)
        => _items.Contains(hit);

    internal readonly void Add(UITilePosition hit)
    {
        Debug.Assert(!_items.Contains(hit));
        Debug.Assert(hit.UIT.SelectionBorder == false);
        _items.Add(hit);
        hit.UIT.SelectionBorder = true;
    }

    internal readonly void Remove(UITilePosition hit)
    {
        Debug.Assert(_items.Contains(hit));
        Debug.Assert(hit.UIT.SelectionBorder == true);
        _items.Remove(hit);
        hit.UIT.SelectionBorder = false;
    }
}

public partial class Hand: UserControl
{
    private readonly HandSelection Selection = new();

    const int HandRows = 2;
    const int HandColumns = 8;

    readonly Tile?[,] HandArray = new Tile?[HandRows, HandColumns];

    public Hand()
    {
        InitializeComponent();

        var tm = TransformationMatrix.Matrix;
        tm.Translate(10.0, 10.0);
        TransformationMatrix.Matrix = tm;

        for (int r = 0; r < HandRows; r++)
            for (int c = 0; c < HandColumns; c++)
            {
                var rect = new Rectangle();
                rect.Width = UnitSize;
                rect.Height = UnitSize;
                rect.SetValue(Canvas.TopProperty, r * UnitSize);
                rect.SetValue(Canvas.LeftProperty, c * UnitSize);
                rect.StrokeThickness = 1.0;
                rect.Stroke = Brushes.LightGray;
                HandBackgroundGrid.Children.Add(rect);
            }

        var h1 = new Tile(LibQwirkle.Shape.Lozange, LibQwirkle.Color.Blue, 1);
        var h2 = new Tile(LibQwirkle.Shape.Square, LibQwirkle.Color.Blue, 1);
        var h3 = new Tile(LibQwirkle.Shape.Star, LibQwirkle.Color.Blue, 1);
        var h4 = new Tile(LibQwirkle.Shape.Star, LibQwirkle.Color.Yellow, 1);
        var h5 = new Tile(LibQwirkle.Shape.Square, LibQwirkle.Color.Purple, 1);
        var h6 = new Tile(LibQwirkle.Shape.Square, LibQwirkle.Color.Green, 1);

        //Hand = new Tile?[HandRows, HandColumns];
        HandArray[0, 0] = h1;
        HandArray[0, 1] = h2;
        HandArray[0, 2] = h3;
        HandArray[0, 3] = h4;
        HandArray[0, 4] = h5;
        HandArray[0, 5] = h6;

        for (int c = 0; c < 6; c++)
        {
            Tile? t = HandArray[0, c];
            if (t != null)
                AddUITile(t.Shape.ToString() + t.Color.ToString(), 0, c);
        }
    }

    internal void AddUITile(string shapeColor, int row, int col)
    {
        var t = new UITile();
        t.ShapeColor = shapeColor;
        t.GrayBackground = true;
        t.SetValue(Canvas.TopProperty, row * UnitSize);
        t.SetValue(Canvas.LeftProperty, col * UnitSize);
        t.Width = UnitSize;
        t.Height = UnitSize;
        HandDrawingCanvas.Children.Add(t);
    }

    // --------------------------------------------------------------------
    // Mouse click and drag management

    private Point previousMousePosition;

    // null indicates background grid move, or delegate must be executed by MouseMove to perform move
    // action, P is current mouse coordinates in non-transformed user space
    private Action<Point>? pmm;

    private void HandCanvas_MouseMoveWhenUp(object sender, MouseEventArgs e)
    {
        // %aybe some visual hinting
    }

    // Returs false if no UITile has been hit
    private bool UpdateSelectionAfterClick(MouseButtonEventArgs e)
    {
        // If no Hand tile is hit, just clear selection and return
        UITile? t = GetHitHile(e, HandCanvas);
        if (t == null)
        {
            Selection.Clear();
            return false;
        }

        int row = (int)Math.Floor((double)t.GetValue(Canvas.TopProperty) / UnitSize + 0.5);
        int col = (int)Math.Floor((double)t.GetValue(Canvas.LeftProperty) / UnitSize + 0.5);
        var hit = new UITilePosition(t, row, col);

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
        foreach (UITilePosition item in Selection)
        {
            HandDrawingCanvas.Children.Remove(item.UIT);
            HandDrawingCanvas.Children.Add(item.UIT);
            item.UIT.SetValue(Canvas.TopProperty, item.Row * UnitSize);
            item.UIT.SetValue(Canvas.LeftProperty, item.Col * UnitSize);
        }

        return true;
    }

    private void HandCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        //EndAnimationsInProgress();

        HandCanvas.MouseMove -= HandCanvas_MouseMoveWhenUp;
        HandCanvas.MouseMove += HandCanvas_MouseMoveWhenDown;
        previousMousePosition = e.GetPosition(HandCanvas);
        bool tileHit = UpdateSelectionAfterClick(e);            // Ensures that selected tiles are on top of others in the visual tree

        if (tileHit)
            pmm = GetMouseDownMoveAction();
        else
            pmm = null;

        // Be sure to call GetPosition before Capture, otherwise GetPosition returns 0 after Capture
        // Capture to get MouseUp event raised by grid
        Mouse.Capture(HandCanvas);
    }

    private Action<Point>? GetMouseDownMoveAction()
    {
        Debug.Assert(!Selection.IsEmpty);

        // Reverse-transform mouse Grid coordinates into DrawingCanvas coordinates
        Matrix m = TransformationMatrix.Matrix;
        m.Invert();     // To convert from screen transformed coordinates into ideal grid
                        // coordinates starting at (0,0) with a square side of UnitSize
        var mp = m.Transform(previousMousePosition);
        //var clickOffsetList = new List<Vector>(Selection.Count);
        foreach (UITilePosition item in Selection)
        {
            item.StartLeft = (double)item.UIT.GetValue(Canvas.LeftProperty);
            item.StartTop = (double)item.UIT.GetValue(Canvas.TopProperty);
            var p = new Point(item.StartLeft, item.StartTop);
            item.ClickOffset = p - mp;
        }

        // When moving, point is current mouse in ideal grid coordinates
        return point =>
        {
            // Just move selected tiles
            foreach (UITilePosition item in Selection)
            {
                double preciseTop = point.Y + item.ClickOffset.Y;
                double preciseLeft = point.X + item.ClickOffset.X;

                item.UIT.SetValue(Canvas.TopProperty, preciseTop);
                item.UIT.SetValue(Canvas.LeftProperty, preciseLeft);

                // Round position to closest square on the grid
                int row = (int)Math.Floor(preciseTop / UnitSize + 0.5);
                int col = (int)Math.Floor(preciseLeft / UnitSize + 0.5);

                // ToDo: Decide of hatched feed-back
                //item.UIT.Hatched = !(viewModel.GetCellState(row, col) != CellState.Tiled || (row == Selection.startRow && col == Selection.startCol));
            }
        };
    }

    private void HandCanvas_MouseMoveWhenDown(object sender, MouseEventArgs e)
    {
        //
    }

    private void HandCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Mouse.Capture(null);
        HandCanvas.MouseMove -= HandCanvas_MouseMoveWhenDown;
        HandCanvas.MouseMove += HandCanvas_MouseMoveWhenUp;
    }

    private void HandCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        //
    }

    private void HandCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        //
    }
}
