// Interaction Manager
// Base component to handle mouse interactions

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
using static QwirkleUI.ViewHelpers;

namespace QwirkleUI;

// Combination of a UITile and a RowCol
[DebuggerDisplay("UITileRowCol: {UIT} {RC}")]
internal record UITileRowCol(UITile UIT, RowCol P)
{
    internal UITile UIT { get; private set; } = UIT;
    internal RowCol RC { get; set; } = P;      // set accessor because RC is mutable
    internal RowCol StartRC { get; set; }

    // Used when dragging (offset from mouse click down position) or animating (target position)
    public Vector Offset { get; set; }

    public override string ToString() => $"UITileRowCol: {UIT} {RC}";
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

    internal readonly bool ContainsUITile(UITileRowCol uitp)
        => _items.Any(it => it.UIT == uitp.UIT);

    internal readonly void Add(UITileRowCol hit)
    {
        Debug.Assert(!_items.Contains(hit));
        Debug.Assert(hit.UIT.SelectionBorder == false);
        _items.Add(hit);
        hit.UIT.SelectionBorder = true;
    }

    internal readonly void Remove(UITileRowCol hit)
    {
        Debug.Assert(_items.Contains(hit));
        Debug.Assert(hit.UIT.SelectionBorder == true);
        _items.Remove(hit);
        hit.UIT.SelectionBorder = false;
    }

    internal bool ContainsUITile(UITile uit)
        => _items.Any(uitm => uitm.UIT == uit);
}

abstract internal class InteractionManager
{
    public readonly UITilesSelection Selection = [];
    protected Action<Point>? pmm;
    protected Point previousMouseRowCol;

    public InteractionManager() { }

    public void IM_MouseMoveWhenUp(object sender, MouseEventArgs e)
    {
        // %aybe some visual hinting
    }

    // Returs false if no UITile has been hit
    private bool UpdateSelectionAfterClick(MouseEventArgs e, Canvas c, Canvas dc)
    {
        // If no Hand tile is hit, just clear selection and return
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
            if (!Selection.ContainsUITile(hit))
                Selection.Clear();

        // Only gray background tiles can be selected
        if (!hit.UIT.GrayBackground)
            return false;

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
        foreach (UITileRowCol item in Selection)
        {
            dc.Children.Remove(item.UIT);
            dc.Children.Add(item.UIT);
            item.UIT.SetValue(Canvas.TopProperty, item.RC.Row * UnitSize);
            item.UIT.SetValue(Canvas.LeftProperty, item.RC.Col * UnitSize);
            item.StartRC = item.RC;
        }

        return true;
    }

    public virtual void EndAnimationsInProgress()
    {
    }

    internal void EndMoveInProgress()
    {
        // Move in progress?
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

        previousMouseRowCol = e.GetPosition(c);
        bool tileHit = UpdateSelectionAfterClick(e, c, dc);            // Ensures that selected tiles are on top of others in the visual tree

        if (tileHit)
            pmm = GetMouseDownMoveAction(m);
        else
            pmm = null;

        // Be sure to call GetRowCol before Capture, otherwise GetRowCol returns 0 after Capture
        // Capture to get MouseUp event raised by grid
        Mouse.Capture(c);
    }

    private Action<Point>? GetMouseDownMoveAction(Matrix m)
    {
        Debug.Assert(!Selection.IsEmpty);

        // Reverse-transform mouse Grid coordinates into BoardDrawingCanvas coordinates
        m.Invert();     // To convert from screen transformed coordinates into ideal grid
                        // coordinates starting at (0,0) with a square side of UnitSize
        var mp = m.Transform(previousMouseRowCol);
        //var clickOffsetList = new List<Vector>(Selection.Count);
        foreach (UITileRowCol item in Selection)
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
            foreach (UITileRowCol item in Selection)
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

    internal virtual void IM_MouseMoveWhenDown(object sender, MouseEventArgs e, Canvas c, Matrix m)
    {
        if (pmm != null)
        {
            var newRowCol = e.GetPosition(c);
            m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
            var localRowCol = m.Transform(newRowCol);
            pmm(localRowCol);
            CheckStartHandOver(localRowCol);
        }
    }

    internal virtual void CheckStartHandOver(Point localRowCol) { }

    internal virtual void StartHandOver(UITilesSelection selection) 
    {
        //Debug.WriteLine("InteractionManager.StartHandOver");
        Mouse.Capture(null);
        pmm = null;
    }

    abstract internal void UpdateTargetPosition(UITilesSelection selection);

    internal void IM_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Mouse.Capture(null);

        if (pmm != null)
        {
            pmm = null;
            UpdateTargetPosition(Selection);
        }
    }

    public void IM_MouseWheel(object sender, MouseWheelEventArgs e) =>
        // This is not used for Hand
        Debug.WriteLine("IM_MouseWheel ToDo");

    public void IM_MouseRightButtonDown(object sender, MouseButtonEventArgs e, Canvas c, Canvas dc)
    {
        EndAnimationsInProgress();
        bool tileHit = UpdateSelectionAfterClick(e, c, dc);

        OnMouseRightButtonDown(sender, Selection, tileHit);
        e.Handled = true;
    }

    public virtual void OnMouseRightButtonDown(object sender, UITilesSelection selection, bool tileHit)
    { }
}
