// Interaction Manager
// Base component to handle mouse interactions

using LibQwirkle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static QwirkleUI.App;
using static QwirkleUI.Helpers;

namespace QwirkleUI;

// Combination of a UITile and a RowCol
[DebuggerDisplay("UITileRowCol: {UIT} {RC}")]
internal sealed class UITileRowCol(UITile UIT, RowCol RC): IEquatable<UITileRowCol>
{
    internal UITile UIT { get; private set; } = UIT;
    internal RowCol RC { get; set; } = RC;      // set accessor because RC is mutable

    // Helpers
    public int Row => RC.Row;
    public int Col => RC.Col;
    public Tile Tile => UIT.Tile;

    // Used when dragging (offset from mouse click down position) or animating (target position)
    public Vector Offset { get; set; }

    public override string ToString() => $"UITileRowCol: {UIT} {RC}";

    // Equality is only based on Tile information        
    public bool EqualsTile(UITileRowCol? other) => other is not null && Tile == other.Tile;

    public override bool Equals(object? other)
        => ReferenceEquals(this, other) || (other is not null && EqualsTile(other as UITileRowCol));

    public bool Equals(UITileRowCol? other)
        => EqualsTile(other);

    public override int GetHashCode()
        => Tile.GetHashCode();

    public static bool operator ==(UITileRowCol? left, UITileRowCol? right)
        => left is not null && right is not null && left.EqualsTile(right);

    public static bool operator !=(UITileRowCol? left, UITileRowCol? right)
        => !(left == right);
}

internal readonly struct UITilesSelection: IReadOnlyCollection<UITileRowCol>
{
    private readonly List<UITileRowCol> _items = [];

    // A compiler requirement...
    public UITilesSelection() { }

    public readonly void Clear()
    {
        foreach (var item in _items)
            item.UIT.SelectionBorder = false;
        _items.Clear();
    }

    public IEnumerator<UITileRowCol> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

    public readonly bool IsEmpty => _items.Count == 0;

    public readonly int Count => _items.Count;

    internal readonly void Add(UITileRowCol hit)
    {
        Debug.Assert(!_items.Contains(hit));
        Debug.Assert(!hit.UIT.SelectionBorder);     // Why?
        _items.Add(hit);
        hit.UIT.SelectionBorder = true;                     // And then we add a selection border??
    }

    internal readonly void RemoveUITile(UITile uit)
    {
        Debug.Assert(ContainsUITile(uit));
        Debug.Assert(uit.SelectionBorder);
        var todel = _items.First(it => it.UIT == uit);
        Debug.Assert(todel != null);
        _items.Remove(todel);
        uit.SelectionBorder = false;
    }

    internal bool ContainsUITile(UITile uit)
        => _items.Any(uitm => uitm.UIT == uit);
}

abstract internal class InteractionManager
{
    public readonly UITilesSelection Selection = [];
    protected Action<Point>? pmm;
    protected Point previousMousePosition;
    protected Point mouseDownStartPosition;

    public InteractionManager() { }

    public void IM_MouseMoveWhenUp(object sender, MouseEventArgs e) { } // Maybe some visual hinting?

    /// <summary>
    /// Returs false if no UITile has been hit
    /// </summary>
    private bool UpdateSelectionAfterClick(MouseEventArgs e, Canvas c, Canvas dc)
    {
        // If no Hand UITile is hit, just clear selection and return

        UITile? t = GetHitHile(e.GetPosition(c), c);
        if (t == null)
        {
            Selection.Clear();
            return false;
        }

        int row = (int)Math.Floor((double)t.GetValue(Canvas.TopProperty) / UnitSize + 0.5);
        int col = (int)Math.Floor((double)t.GetValue(Canvas.LeftProperty) / UnitSize + 0.5);
        var hit = new UITileRowCol(t, new RowCol(row, col));

        // If Ctrl key is NOT pressed, clear previous selection
        // But if we click again in something already selected, do not clear selection!
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            if (!Selection.ContainsUITile(hit.UIT))
                Selection.Clear();

        // Only gray background tiles can be selected
        if (!hit.UIT.GrayBackground)
            return false;

        // Add current hit to selection
        if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Selection.ContainsUITile(hit.UIT))
        {
            Selection.RemoveUITile(hit.UIT);
            if (Selection.IsEmpty)
                return false;
        }
        else
            if (!Selection.ContainsUITile(hit.UIT))
            Selection.Add(hit);

        // Remove and add again elements to move so they're displayed above non-moved elements
        foreach (UITileRowCol item in Selection)
        {
            dc.Children.Remove(item.UIT);
            dc.Children.Add(item.UIT);
            item.UIT.SetValue(Canvas.TopProperty, item.RC.Row * UnitSize);
            item.UIT.SetValue(Canvas.LeftProperty, item.RC.Col * UnitSize);
        }

        return true;
    }

    public virtual void EndAnimationsInProgress()
    {
    }

    internal void IMEndMoveInProgress()
    {
        // TileRowCol in progress?
        if (pmm != null)
        {
            pmm = null;
            Mouse.Capture(null);
            return;
        }
    }

    public void IM_MouseDown(object sender, MouseEventArgs e, Canvas c, Canvas dc, Matrix m)
    {
        EndAnimationsInProgress();

        previousMousePosition = e.GetPosition(c);
        mouseDownStartPosition = previousMousePosition;
        bool tileHit = UpdateSelectionAfterClick(e, c, dc);            // Ensures that selected tiles are on top of others in the visual tree

        pmm = tileHit ? GetMouseDownMoveAction(m, false) : null;

        // Be sure to call GetRowCol before Capture, otherwise GetRowCol returns 0 after Capture
        // Capture to get MouseUp event raised by grid
        Mouse.Capture(c);
    }

    public void IM_HandOver_MouseDown(Canvas c, Canvas dc, Matrix m)
    {
        EndAnimationsInProgress();
        previousMousePosition = Mouse.GetPosition(c);
        pmm = GetMouseDownMoveAction(m, true);
        Mouse.Capture(c);
    }

    private Action<Point>? GetMouseDownMoveAction(Matrix m, bool skipOffsetCalculation)
    {
        Debug.Assert(!Selection.IsEmpty);

        if (!skipOffsetCalculation)
        {
            // Reverse-transform mouse Grid coordinates into BoardDrawingCanvas coordinates
            m.Invert();     // To convert from screen transformed coordinates into ideal grid
                            // coordinates starting at (0,0) with a square side of UnitSize
            var mp = m.Transform(previousMousePosition);
            foreach (UITileRowCol item in Selection)
            {
                double startLeft = (double)item.UIT.GetValue(Canvas.LeftProperty);
                double startTop = (double)item.UIT.GetValue(Canvas.TopProperty);
                var p = new Point(startLeft, startTop);
                item.Offset = p - mp;
            }
        }

        // When moving, point is current mouse in ideal grid coordinates
        return point =>
        {
            // Just move selected tiles
            foreach (UITileRowCol item in Selection)
            {
                double preciseTop = point.Y + item.Offset.Y;
                double preciseLeft = point.X + item.Offset.X;

                // Round position to closest square on the grid
                // Originally to decide if UITile should be hatched or not, but logic is different between Board and Hand
                int row = (int)Math.Floor(preciseTop / UnitSize + 0.5);
                int col = (int)Math.Floor(preciseLeft / UnitSize + 0.5);

                item.UIT.SetValue(Canvas.TopProperty, preciseTop);
                item.UIT.SetValue(Canvas.LeftProperty, preciseLeft);

                // ToDo: Decide of hatched feed-back
                // Possible: an abstract function overriden in Board/Hand ImplementationManager classes
                //item.UIT.Hatched = !(viewModel.GetCellState(row, col) != CellState.Tiled || (row == Selection.startRow && col == Selection.startCol));
            }
        };
    }

    internal virtual Point IM_MouseMoveWhenDown(object sender, MouseEventArgs e, Canvas c, Matrix m)
    {
        var canvasPosition = e.GetPosition(c);
        bool smallMouseMove = (mouseDownStartPosition - canvasPosition).Length < SmallMouseMoveLengthThreshold;

        m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
        var drawingCanvasPosition = m.Transform(canvasPosition);

        // Ignore small mouse moves
        if (!smallMouseMove)
            pmm?.Invoke(drawingCanvasPosition);

        return drawingCanvasPosition;
    }

    internal void StartHandOverEndCaptureAndPmm()
    {
        Mouse.Capture(null);
        pmm = null;
    }

    internal void IM_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Mouse.Capture(null);

        if (pmm != null)
        {
            pmm = null;
            UpdateTargetPosition();
        }
    }

    internal abstract void UpdateTargetPosition();

    public void IM_MouseWheel(object sender, MouseWheelEventArgs e) { }
    public void IM_MouseRightButtonDown(object sender, MouseButtonEventArgs e, Canvas c, Canvas dc)
    {
        EndAnimationsInProgress();
        bool tileHit = UpdateSelectionAfterClick(e, c, dc);

        OnMouseRightButtonDown(sender, Selection, tileHit);
        e.Handled = true;
    }

    public virtual void OnMouseRightButtonDown(object sender, UITilesSelection selection, bool tileHit) { }
}
