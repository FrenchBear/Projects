// Interaction Manager
// Base component to handle mouse interactions

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static QwirkleUI.App;
using static QwirkleUI.ViewHelpers;
using LibQwirkle;
using System.Reflection.Metadata;

namespace QwirkleUI;

// Combination of a UITile and a RowCol
[DebuggerDisplay("UITileRowCol: {UIT} {RC}")]
internal record UITileRowCol(UITile UIT, RowCol P)
{
    internal UITile UIT { get; private set; } = UIT;
    internal RowCol RC { get; set; } = P;      // set accessor because RC is mutable

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
    protected readonly UITilesSelection Selection = [];
    private Point previousMouseRowCol;
    private Action<Point>? pmm;

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

        // Reverse-transform mouse Grid coordinates into DrawingCanvas coordinates
        //Matrix m =  TransformationMatrix.Matrix;
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

                Debug.WriteLine($"MouseMoveDown Action: left={preciseLeft:F0} top={preciseTop:F0}");

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

    internal void IM_MouseMoveWhenDown(object sender, MouseEventArgs e, Canvas c, Matrix m)
    {
        if (pmm != null)
        {
            var newRowCol = e.GetPosition(c);
            //Matrix m = TransformationMatrix.Matrix;
            m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
            pmm(m.Transform(newRowCol));
        }

        // For hand, we don't move grid
    }

    abstract internal void UpdateTargetPosition(UITilesSelection selection);

    internal void IM_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Mouse.Capture(null);

        if (pmm != null)
        {
            pmm = null;

            UpdateTargetPosition(Selection);

            /*
            // Find a free position
            // Build NewHand without tiles being moved
            var NewHand = new List<UITileRowCol>();
            foreach (UITileRowCol uitp in Hand)
                if (!Selection.ContainsUITile(uitp.UIT))
                    NewHand.Add(uitp);

            foreach (UITileRowCol uitp in Selection)
            {
                double left = (double)uitp.UIT.GetValue(Canvas.LeftProperty);
                double top = (double)uitp.UIT.GetValue(Canvas.TopProperty);

                Debug.WriteLine($"pos left={left} top={top}");

                // Build list of distances to empty positions on hand
                var ld = new List<(RowCol, double)>();
                int zz = 0;
                for (int r = 0; r < HandRows; r++)
                    for (int c = 0; c < HandColumns; c++)
                        if (!NewHand.Any(uitp => uitp.RC.Row == r && uitp.RC.Col == c))
                        {
                            double targetLeft = c * UnitSize;
                            double targetTop = r * UnitSize;
                            // Actually dist squared, but that's enough to find the minimum
                            double dist = (targetLeft - left) * (targetLeft - left) + (targetTop - top) * (targetTop - top);

                            Debug.WriteLine($"ld[{zz++}] tleft={targetLeft} ttop={targetTop}  dist²={dist}");

                            ld.Add((new RowCol(r, c), dist));
                        }
                var xxmin = ld.MinBy(tup => tup.Item2);
                Debug.WriteLine($"min: {xxmin}");

                var closestRowCol = ld.MinBy(tup => tup.Item2).Item1;
                uitp.RC = closestRowCol;
                uitp.Offset = new Vector(closestRowCol.Col * UnitSize, closestRowCol.Row * UnitSize);

                // Now the position is taken, not free for the rest of selection
                NewHand.Add(uitp);
            }

            // For now, direct move for testing
            // ToDo: Replace by animation using storyboard    Actually not sure it's needed for Hand, looks Ok without animation
            foreach (UITileRowCol uitp in Selection)
            {
                uitp.UIT.SetValue(Canvas.TopProperty, uitp.Offset.Y);
                uitp.UIT.SetValue(Canvas.LeftProperty, uitp.Offset.X);
                var h = Hand.Find(u => u.UIT == uitp.UIT);
                Debug.Assert(h != null);
                h.RC = uitp.RC;
            }
            */
        }
    }

    public void IM_MouseWheel(object sender, MouseWheelEventArgs e) =>
        // This is not used for Hand
        Debug.WriteLine("HandCanvas_MouseWheel");

    public void IM_MouseRightButtonDown(object sender, MouseButtonEventArgs e, Canvas c, Canvas dc)
    {
        EndAnimationsInProgress();
        bool tileHit = UpdateSelectionAfterClick(e, c, dc);

        // ToDo, show context menu, maybe different whether there is tile selection or not
        // Call a virtual method
        Debug.WriteLine($"IM_MouseRightButtonDown, tileHit: {tileHit}, Selection.IsEmpty: {Selection.IsEmpty}");
    }

}
