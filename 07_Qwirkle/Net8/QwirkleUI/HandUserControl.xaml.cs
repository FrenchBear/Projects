// Hand UserControl
// Reprsent a player Hand
//
// 2023-12-12   PV

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static QwirkleUI.App;

namespace QwirkleUI;

public partial class HandUserControl: UserControl
{
    private HandViewModel HandViewModel;
    internal readonly HashSet<UITileRowCol> Hand = [];
    internal InteractionManager HandIM;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public HandUserControl()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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

        KeyDown += UserControl_KeyDown;

    }

    private void UserControl_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            HandIM.EndAnimationsInProgress();
            HandIM.EndMoveInProgress();
            HandCanvas.MouseMove -= HandCanvas_MouseMoveWhenDown;
            HandCanvas.MouseMove += HandCanvas_MouseMoveWhenUp;
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

    private void HandCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        HandCanvas.MouseMove -= HandCanvas_MouseMoveWhenUp;
        HandCanvas.MouseMove += HandCanvas_MouseMoveWhenDown;
        HandIM.IM_MouseDown(sender, e, HandCanvas, HandDrawingCanvas, TransformationMatrix.Matrix);
    }

    private void HandCanvas_MouseMoveWhenDown(object sender, MouseEventArgs e)
        => HandIM.IM_MouseMoveWhenDown(sender, e, HandCanvas, TransformationMatrix.Matrix);

    private void HandCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        HandCanvas.MouseMove -= HandCanvas_MouseMoveWhenDown;
        HandCanvas.MouseMove += HandCanvas_MouseMoveWhenUp;
        HandIM.IM_MouseUp(sender, e);
    }

    // Actually not used for Hand
    private void HandCanvas_MouseWheel(object sender, MouseWheelEventArgs e) =>
        HandIM.IM_MouseWheel(sender, e);

    private void HandCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        => HandIM.IM_MouseRightButtonDown(sender, e, HandCanvas, HandDrawingCanvas);
}

internal class HandInteractionManager: InteractionManager
{
    private readonly HashSet<UITileRowCol> Hand;

    public HandInteractionManager(HashSet<UITileRowCol> hand)
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

            // Build list of distances to empty positions on hand
            var ld = new List<(RowCol, double)>();
            for (int r = 0; r < HandRows; r++)
                for (int c = 0; c < HandColumns; c++)
                    if (!NewHand.Any(uitp => uitp.RC.Row == r && uitp.RC.Col == c))
                    {
                        double targetLeft = c * UnitSize;
                        double targetTop = r * UnitSize;
                        // Actually dist squared, but that's enough to find the minimum
                        double dist = (targetLeft - left) * (targetLeft - left) + (targetTop - top) * (targetTop - top);

                        ld.Add((new RowCol(r, c), dist));
                    }
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
            var h = Hand.First(u => u.UIT == uitp.UIT);
            Debug.Assert(h != null);
            h.RC = uitp.RC;
        }
    }
}
