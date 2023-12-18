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
using System.Diagnostics;
using static QwirkleUI.App;
using static QwirkleUI.ViewHelpers;
using LibQwirkle;

namespace QwirkleUI;

public partial class HandUserControl: UserControl
{
    private HandViewModel HandViewModel;
    internal readonly List<UITileRowCol> Hand = [];
    internal InteractionManager HandIM;

    public HandUserControl()
    {
        InitializeComponent();

        HandIM = new HandInteractionManager(Hand);

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

    internal void SetViewModel(HandViewModel handViewModel) => HandViewModel = handViewModel;

    internal void AddUITile(string shapeColor, int instance, RowCol p)
    {
        var t = new UITile(shapeColor, instance);
        t.GrayBackground = true;
        t.SetValue(Canvas.TopProperty, p.Row * UnitSize);
        t.SetValue(Canvas.LeftProperty, p.Col * UnitSize);
        t.Width = UnitSize;
        t.Height = UnitSize;
        HandDrawingCanvas.Children.Add(t);

        var h = new UITileRowCol(t, p);
        Hand.Add(h);
    }

    // --------------------------------------------------------------------
    // Mouse click and drag management

    private void HandCanvas_MouseMoveWhenUp(object sender, MouseEventArgs e)
        => HandIM.IM_MouseMoveWhenUp(sender, e);

    /*
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
            HandDrawingCanvas.Children.Remove(item.UIT);
            HandDrawingCanvas.Children.Add(item.UIT);
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
    */

    private void HandCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Debug.WriteLine("HandCanvas_MouseDown");

        HandCanvas.MouseMove -= HandCanvas_MouseMoveWhenUp;
        HandCanvas.MouseMove += HandCanvas_MouseMoveWhenDown;
        HandIM.IM_MouseDown(sender, e, HandCanvas, HandDrawingCanvas, TransformationMatrix.Matrix);
    }
    /*
    {
        EndAnimationsInProgress();

        HandCanvas.MouseMove -= HandCanvas_MouseMoveWhenUp;
        HandCanvas.MouseMove += HandCanvas_MouseMoveWhenDown;
        previousMouseRowCol = e.GetPosition(HandCanvas);
        bool tileHit = UpdateSelectionAfterClick(e);            // Ensures that selected tiles are on top of others in the visual tree

        if (tileHit)
            pmm = GetMouseDownMoveAction();
        else
            pmm = null;

        // Be sure to call GetRowCol before Capture, otherwise GetRowCol returns 0 after Capture
        // Capture to get MouseUp event raised by grid
        Mouse.Capture(HandCanvas);
    }
    */

    /*
    private Action<Point>? GetMouseDownMoveAction()
    {
        Debug.Assert(!Selection.IsEmpty);

        // Reverse-transform mouse Grid coordinates into DrawingCanvas coordinates
        Matrix m = TransformationMatrix.Matrix;
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
    */

    private void HandCanvas_MouseMoveWhenDown(object sender, MouseEventArgs e)
    {
        Debug.WriteLine("HandCanvas_MouseMoveWhenDown");

        HandIM.IM_MouseMoveWhenDown(sender, e, HandCanvas, TransformationMatrix.Matrix);
    }
    /*
    {
        if (pmm != null)
        {
            var newRowCol = e.GetPosition(HandCanvas);
            Matrix m = TransformationMatrix.Matrix;
            m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
            pmm(m.Transform(newRowCol));
        }

        // For hand, we don't move grid
    }
    */

    private void HandCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Debug.WriteLine("HandCanvas_MouseUp");

        HandCanvas.MouseMove -= HandCanvas_MouseMoveWhenDown;
        HandCanvas.MouseMove += HandCanvas_MouseMoveWhenUp;
        HandIM.IM_MouseUp(sender, e);
    }

    /*

        Mouse.Capture(null);
        if (pmm != null)
        {
            pmm = null;

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
                        if (!NewHand.Any(uitp => uitp.RC.Row==r && uitp.RC.Col==c))
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

        }
    }
    */

    private void HandCanvas_MouseWheel(object sender, MouseWheelEventArgs e) =>
        // This is not used for Hand
        Debug.WriteLine("HandCanvas_MouseWheel");

    private void HandCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        => HandIM.IM_MouseRightButtonDown(sender, e, HandCanvas, HandDrawingCanvas);

/*
{
HandIM.EndAnimationsInProgress();
bool tileHit = HandIM.UpdateSelectionAfterClick(e);

// ToDo, show context menu, maybe different whether there is tile selection or not
Debug.WriteLine($"HandCanvas_MouseRightButtonDown, tileHit: {tileHit}, Selection.IsEmpty: {Selection.IsEmpty}");
}
*/

}

internal class HandInteractionManager: InteractionManager
{
    private readonly List<UITileRowCol> Hand;

    public HandInteractionManager(List<UITileRowCol> hand)
        => Hand = hand;

    internal override void UpdateTargetPosition(UITilesSelection selection)
    {
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

            //Debug.WriteLine($"pos left={left} top={top}");

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

                        //Debug.WriteLine($"ld[{zz++}] tleft={targetLeft} ttop={targetTop}  dist²={dist}");

                        ld.Add((new RowCol(r, c), dist));
                    }
            var xxmin = ld.MinBy(tup => tup.Item2);
            //Debug.WriteLine($"min: {xxmin}");

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
    }
}
