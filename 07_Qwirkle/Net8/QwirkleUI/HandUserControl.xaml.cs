﻿// Hand UserControl
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
internal record UITilePosition(UITile UIT, Position P)
{
    internal UITile UIT { get; private set; } = UIT;
    internal Position P { get; private set; } = P;

    // Used when dragging (offset from mouse click down position) or animating (target position)
    public Vector Offset { get; set; }
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

    public readonly bool IsEmpty => _items.Count == 0;

    public readonly int Count => _items.Count;

    internal readonly bool ContainsUITile(UITilePosition uitp)
        => _items.Any(it => it.UIT== uitp.UIT);

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

public partial class HandUserControl: UserControl
{
    const int HandRows = 2;
    const int HandColumns = 8;

    private HandViewModel HandViewModel;
    private readonly HandSelection Selection = new();
    internal readonly List<UITilePosition> Hand = new();

    public HandUserControl()
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
    }

    internal void SetViewModel(HandViewModel handViewModel)
    {
        HandViewModel = handViewModel;
    }

    internal void AddUITile(string shapeColor, int instance, Position p)
    {
        var t = new UITile(shapeColor, instance);
        t.GrayBackground = true;
        t.SetValue(Canvas.TopProperty, p.Row * UnitSize);
        t.SetValue(Canvas.LeftProperty, p.Col * UnitSize);
        t.Width = UnitSize;
        t.Height = UnitSize;
        HandDrawingCanvas.Children.Add(t);

        var h = new UITilePosition(t, p);
        Hand.Add(h);
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
        var hit = new UITilePosition(t, new Position(row, col));

        // If Ctrl key is NOT pressed, clear previous selection
        // But if we click again in something already selected, do not clear selection!
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            if (!Selection.ContainsUITile(hit))
                Selection.Clear();

        // Add current hit to selection
        if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Selection.ContainsUITile(hit))
        {
            Selection.Remove(hit);
            if (Selection.IsEmpty)
                return false;
        }
        else
            if (!Selection.ContainsUITile(hit))
            Selection.Add(hit);

        // Remove and add again elements to move so they're displayed above non-moved elements
        foreach (UITilePosition item in Selection)
        {
            HandDrawingCanvas.Children.Remove(item.UIT);
            HandDrawingCanvas.Children.Add(item.UIT);
            item.UIT.SetValue(Canvas.TopProperty, item.P.Row * UnitSize);
            item.UIT.SetValue(Canvas.LeftProperty, item.P.Col * UnitSize);
        }

        return true;
    }

    internal void EndAnimationsInProgress()
    {
        //if (IsMoveWordAnimationInProgress)
        //    EndMoveWordAnimation();
        //if (IsMatrixAnimationInProgress)
        //    EndMatrixAnimation();
    }

    private void HandCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        EndAnimationsInProgress();

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
            double startLeft = (double)item.UIT.GetValue(Canvas.LeftProperty);
            double startTop = (double)item.UIT.GetValue(Canvas.TopProperty);
            var p = new Point(startLeft, startTop);
            item.Offset = p - mp;
        }

        // When moving, point is current mouse in ideal grid coordinates
        return point =>
        {
            // Just move selected tiles
            foreach (UITilePosition item in Selection)
            {
                double preciseTop = point.Y + item.Offset.Y;
                double preciseLeft = point.X + item.Offset.X;

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
        if (pmm != null)
        {
            var newPosition = e.GetPosition(HandCanvas);
            Matrix m = TransformationMatrix.Matrix;
            m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
            pmm(m.Transform(newPosition));
        }

        // For hand, we don't move grid
    }

    private void HandCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Mouse.Capture(null);
        HandCanvas.MouseMove -= HandCanvas_MouseMoveWhenDown;
        HandCanvas.MouseMove += HandCanvas_MouseMoveWhenUp;

        if (pmm != null)
        {
            pmm = null;

            // Find a free position
            // Build NewHand without tiles being moved
            /*
            var NewHand = new List<UITilePosition>();
            foreach (var h in Hand)
                if (!HandSelection.Contains(h))
                {

                }
            // $$$
            */
        }
    }

    private void HandCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        // ToDo: This is not used for Hand
        Debug.WriteLine("HandCanvas_MouseWheel");
    }

    private void HandCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        EndAnimationsInProgress();
        bool tileHit = UpdateSelectionAfterClick(e);

        // ToDo, show context menu, maybe different whether there is tile selection or not
        Debug.WriteLine($"HandCanvas_MouseRightButtonDown, tileHit: {tileHit}, Selection.IsEmpty: {Selection.IsEmpty}");
    }

}