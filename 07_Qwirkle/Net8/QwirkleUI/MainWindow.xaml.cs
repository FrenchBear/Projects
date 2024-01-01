// QwirkleUI
// WPF interface for Qwirkle project
//
// 2023-12-10   PV      First version, convert SVG tiles from https://fr.wikipedia.org/wiki/Qwirkle in XAML using Inkscape
// 2023-12-19   PV      Refactoring using InteractionManager

using LibQwirkle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static QwirkleUI.App;
using static QwirkleUI.Helpers;

namespace QwirkleUI;

public partial class MainWindow: Window
{
    private readonly MainViewModel ViewModel;
    internal BoardInteractionManager BoardIM;

    public MainWindow()
    {
        TraceCall();

        InitializeComponent();
        ViewModel = new MainViewModel(this);
        DataContext = ViewModel;

        BoardIM = new BoardInteractionManager(ViewModel.CurrentMoves, this, ViewModel);

        Loaded += MainWindow_Loaded;
        KeyDown += MainWindow_KeyDown;
    }

    public void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        TraceCall();

        ViewModel.NewBoard(true);
        DrawBoard();
        RescaleAndCenter(false, true);
        ViewModel.DrawHands();

        //ViewModel.StatusMessage = "Done.";
        ViewModel.EvaluateCurrentMoves();
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        TraceCall();

        if (e.Key == Key.Escape)
        {
            BoardIM.IMEndMoveInProgress();
            BoardIM.EndAnimationsInProgress();
            BoardIM.IMEndMoveInProgress();
            BoardCanvas.MouseMove -= BoardCanvas_MouseMoveWhenDown;
            BoardCanvas.MouseMove += BoardCanvas_MouseMoveWhenUp;
        }
    }

    // Clears the grid
    internal void ClearGrid()
    {
        TraceCall();

        // Clear previous elements layout
        BoardDrawingCanvas.Children.Clear();
        ClearBackgroundGrid();
        ViewModel.StatusMessage = "Clear.";
        BoardIM.Selection.Clear();
    }

    internal void DrawBoard()
    {
        TraceCall();

        Debug.Assert(BoardDrawingCanvas.Children.Count == 0);
        UpdateBackgroundGrid();
        ViewModel.DrawBoard();
        ViewModel.DrawCurrentMoves();       // For dev, draw test tiles with gray background
    }

    // Adjust scale and origin to see the whole puzzle
    internal void RescaleAndCenter(bool isWithAnimation, bool forceRescale = false)
    {
        TraceCall();

        // As long as we've 2 row/col of emty cells abound board, no need to rescale unless it's a foced request (Ctrl+R)
        var bb = ViewModel.Bounds();
        if ((!forceRescale)
            && bb.Min.Row - 2 >= CurrentGridBoundingWithMargins.Min.Row
            && bb.Min.Col - 2 >= CurrentGridBoundingWithMargins.Min.Col
            && bb.Max.Row + 2 < CurrentGridBoundingWithMargins.Max.Row
            && bb.Max.Col + 2 < CurrentGridBoundingWithMargins.Max.Col)
            return;

        UpdateBackgroundGrid();
        var r = BoundingRectangleWithMargins();
        CurrentGridBoundingWithMargins = r;

        // Reverse-transform corners into WordCanvas coordinates
        var p1Grid = new Point(r.Min.Col * UnitSize, r.Min.Row * UnitSize);
        var p2Grid = new Point(r.Max.Col * UnitSize, r.Max.Row * UnitSize);

        rescaleMatrix = TransformationMatrix.Matrix;

        // Set rotation to zero
        // Get angle from transformation matrix
        double θ = Math.Atan2(rescaleMatrix.M21, rescaleMatrix.M11);    // Just to use a variable named θ
        rescaleMatrix.Rotate(θ / Math.PI * 180);        // It would certainly kill Microsoft to indicate on Rotate page or Intellisense tooltip that angle is in degrees...

        // First adjust scale
        Point p1Screen = rescaleMatrix.Transform(p1Grid);
        Point p2Screen = rescaleMatrix.Transform(p2Grid);
        double rescaleFactorX = BoardCanvas.ActualWidth / (p2Screen.X - p1Screen.X);
        double rescaleFactorY = BoardCanvas.ActualHeight / (p2Screen.Y - p1Screen.Y);
        double rescaleFactor = Math.Min(rescaleFactorX, rescaleFactorY);
        rescaleMatrix.Scale(rescaleFactor, rescaleFactor);

        // Then adjust location and center
        p1Screen = rescaleMatrix.Transform(p1Grid);
        p2Screen = rescaleMatrix.Transform(p2Grid);
        double offX1 = -p1Screen.X;
        double offX2 = BoardCanvas.ActualWidth - p2Screen.X;
        double offY1 = -p1Screen.Y;
        double offY2 = BoardCanvas.ActualHeight - p2Screen.Y;
        rescaleMatrix.Translate((offX1 + offX2) / 2, (offY1 + offY2) / 2);

        UpdateBackgroundGrid();

        if (isWithAnimation)
        {
            // Use an animation for a smooth transformation
            var ma = new MatrixAnimation
            {
                From = TransformationMatrix.Matrix,
                To = rescaleMatrix,
                Duration = new Duration(TimeSpan.FromSeconds(0.35))
            };
            ma.Completed += MatrixAnimationCompleted;
            IsMatrixAnimationInProgress = true;
            TransformationMatrix.BeginAnimation(MatrixTransform.MatrixProperty, ma);
        }
        else
            EndMatrixAnimation();
    }

    internal bool IsMatrixAnimationInProgress;
    private Matrix rescaleMatrix;

    // Event handler when MatrixAnimation is completed, need to "free" animated properties otherwise
    // they're "hold" by animation
    private void MatrixAnimationCompleted(object? sender, EventArgs e) => EndMatrixAnimation();

    // Terminate transformation in a clean way, "freeing" animated properties
    internal void EndMatrixAnimation()
    {
        IsMatrixAnimationInProgress = false;
        TransformationMatrix.BeginAnimation(MatrixTransform.MatrixProperty, null);

        // Final tasks
        TransformationMatrix.Matrix = rescaleMatrix;
    }

    // --------------------------------------------------------------------
    // Mouse click and drag management

    // Maybe provide hovering visual feed-back? Or a tooltip with debug info?
    private void BoardCanvas_MouseMoveWhenUp(object sender, MouseEventArgs e)
    {
        if (HandOverState == HandOverStateEnum.InTransition)
            return;
        //TraceCall();

        BoardIM.IM_MouseMoveWhenUp(sender, e);
    }

    private void BoardCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        TraceCall();

        BoardCanvas.MouseMove -= BoardCanvas_MouseMoveWhenUp;
        BoardCanvas.MouseMove += BoardCanvas_MouseMoveWhenDown;
        BoardIM.IM_MouseDown(sender, e, BoardCanvas, BoardDrawingCanvas, TransformationMatrix.Matrix);
    }

    // Relay from Window_MouseDown handler when it's actually a right click
    private void BoardCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        TraceCall();

        BoardIM.IM_MouseRightButtonDown(sender, e, BoardCanvas, BoardDrawingCanvas);
    }

    private void BoardCanvas_MouseMoveWhenDown(object sender, MouseEventArgs e)
    {
        if (HandOverState == HandOverStateEnum.InTransition)
            return;
        TraceCall();

        BoardIM.IM_MouseMoveWhenDown(sender, e, BoardCanvas, TransformationMatrix.Matrix);
    }

    private void BoardCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        // An event MouseUp is raised during handover, don't know why
        if (HandOverState == HandOverStateEnum.InTransition)
            return;
        TraceCall();

        BoardCanvas.MouseMove -= BoardCanvas_MouseMoveWhenDown;
        BoardCanvas.MouseMove += BoardCanvas_MouseMoveWhenUp;
        BoardIM.IM_MouseUp(sender, e);

        ViewModel.EvaluateCurrentMoves();
    }

    // Animated move, not used right now...
    /*
    private void MoveSelection(double toTop, double toLeft)
    {
        Debug.Assert(Selection.uitile != null);

        // If bounding rectangle is updated, need to redraw background grid
        if (!BoundingRectangleWithMargins().Equals(CurrentGridBoundingWithMargins))
            UpdateBackgroundGrid();

        double fromTop = (double)Selection.uitile.GetValue(Canvas.TopProperty);
        double fromLeft = (double)Selection.uitile.GetValue(Canvas.LeftProperty);

        // Compute distance moved on 1st element to choose animation speed (duration)
        double deltaX = fromLeft - toLeft;
        double deltaY = fromTop - toTop;
        double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        // If distance is null, for instance after a selection click, we're done
        if (distance <= 0.0001)
            return;

        // Group animations in a storyboard to simplify premature ending
        var sb = new Storyboard();
        var duration = new Duration(TimeSpan.FromSeconds(distance >= UnitSize ? 0.35 : 0.1));
        finalMoveUITileAnimationData.Clear();

        var daLeft = new DoubleAnimation
        {
            Duration = duration,
            From = fromLeft,
            To = toLeft
        };
        Storyboard.SetTarget(daLeft, Selection.uitile);
        Storyboard.SetTargetProperty(daLeft, new PropertyPath("(Canvas.Left)"));
        sb.Children.Add(daLeft);
        finalMoveUITileAnimationData.Add(new(Selection.uitile, Canvas.LeftProperty, toLeft));

        var daTop = new DoubleAnimation
        {
            Duration = duration,
            From = fromTop,
            To = toTop
        };
        Storyboard.SetTarget(daTop, Selection.uitile);
        Storyboard.SetTargetProperty(daTop, new PropertyPath("(Canvas.Top)"));
        sb.Children.Add(daTop);
        finalMoveUITileAnimationData.Add(new(Selection.uitile, Canvas.TopProperty, toTop));

        IsMoveUITileAnimationInProgress = true;
        sb.Completed += Sb_Completed;
        sb.Begin();
    }
    */

    private void Sb_Completed(object? sender, EventArgs e) => EndMoveUITileAnimation();

    internal bool IsMoveUITileAnimationInProgress;
    private readonly List<(UITile, DependencyProperty, double)> finalMoveUITileAnimationData = [];

    // Stops animation and set the final value
    internal void EndMoveUITileAnimation()
    {
        TraceCall();

        IsMoveUITileAnimationInProgress = false;
        foreach (var item in finalMoveUITileAnimationData)
        {
            item.Item1.BeginAnimation(item.Item2, null);
            item.Item1.SetValue(item.Item2, item.Item3);
        }
    }

    private void BoardCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        TraceCall();

        var newRowCol = e.GetPosition(BoardCanvas);
        var m = TransformationMatrix.Matrix;

        // Ctrl+MouseWheel for rotation
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            double angle = e.Delta / 16.0;
            m.RotateAt(angle, newRowCol.X, newRowCol.Y);
        }
        else
        {
            var sign = -Math.Sign(e.Delta);
            var scale = 1 - sign / 10.0;
            m.ScaleAt(scale, scale, newRowCol.X, newRowCol.Y);
        }
        TransformationMatrix.Matrix = m;

        UpdateBackgroundGrid();
    }

    // Grid currently drawn, include margins added to modev/V% bounding that represents board bounding
    // Expressed in row/col int values despite being part of view...
    private BoundingRectangle CurrentGridBoundingWithMargins = new(46, 54, 46, 54);

    private void ClearBackgroundGrid()
    {
        TraceCall();

        BoardBackgroundGrid.Children.Clear();
        CurrentGridBoundingWithMargins = new(45, 55, 45, 55);
    }

    // Add some extra margin and always represent a 8x8 grid at minimum
    private BoundingRectangle BoundingRectangleWithMargins()
    {
        TraceCall();

        var boardBounds = ViewModel.Bounds();
        return new(boardBounds.Min.Row - 3,
                    boardBounds.Max.Row + 4,
                    boardBounds.Min.Col - 3,
                    boardBounds.Max.Col + 4);
    }

    internal void UpdateBackgroundGrid()
    {
        TraceCall();

        var r = BoundingRectangleWithMargins();
        //Debug.WriteLine($"UpdateBackgroundGrid: r={r}");
        if (!r.Equals(CurrentGridBoundingWithMargins))
        {
            //Debug.WriteLine("Redraw grid");
            ClearBackgroundGrid();
            CurrentGridBoundingWithMargins = r;

            for (int row = r.Min.Row; row <= r.Max.Row; row++)
            {
                var l = new Line
                {
                    X1 = r.Min.Col * UnitSize,
                    X2 = r.Max.Col * UnitSize,
                    Y1 = row * UnitSize,
                    Y2 = row * UnitSize,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = row == 0 ? 3 : 1
                };
                BoardBackgroundGrid.Children.Add(l);
            }

            for (int column = r.Min.Col; column <= r.Max.Col; column++)
            {
                var l = new Line
                {
                    X1 = column * UnitSize,
                    X2 = column * UnitSize,
                    Y1 = r.Min.Row * UnitSize,
                    Y2 = r.Max.Row * UnitSize,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = column == 0 ? 3 : 1
                };
                BoardBackgroundGrid.Children.Add(l);
            }

            // Add circle
            var e = new Ellipse();
            e.Width = 1.8 * UnitSize;
            e.Height = 1.8 * UnitSize;
            e.Stroke = Brushes.LightGray;
            e.StrokeThickness = 5.0;
            e.SetValue(Canvas.TopProperty, (50 - 0.4) * UnitSize);
            e.SetValue(Canvas.LeftProperty, (50 - 0.4) * UnitSize);

            BoardBackgroundGrid.Children.Add(e);
        }
        //else
        //    //Debug.WriteLine("Grid unchanged");
    }

    internal UITileRowCol BoardDrawingCanvasAddUITile(Tile ti, RowCol position, bool gray)
    {
        TraceCall();

        var t = new UITile(ti);
        t.GrayBackground = gray;
        t.SetValue(Canvas.TopProperty, position.Row * UnitSize);
        t.SetValue(Canvas.LeftProperty, position.Col * UnitSize);
        t.Width = UnitSize;
        t.Height = UnitSize;
        BoardDrawingCanvas.Children.Add(t);

        return new UITileRowCol(t, position);
    }

    internal void BoardDrawingCanvasRemoveUITile(UITile uit)
    {
        Debug.Assert(BoardDrawingCanvas.Children.Contains(uit));
        BoardDrawingCanvas.Children.Remove(uit);
    }

    internal void BoardDrawingCanvasRemoveAllUITiles() 
        => BoardDrawingCanvas.Children.Clear();

    internal void MainWindowAcceptHandOver(HandInteractionManager playerIM)
    {
        TraceCall();

        Debug.Assert(playerIM != null && !playerIM.Selection.IsEmpty);
        //Debug.WriteLine($"MainWindowAcceptHandOver: Accepting HandOver of {playerIM.Selection.Count} tile(s)");

        // Get mouse position in BoardCanvas
        Point canvasPosition = Mouse.GetPosition(BoardCanvas);
        var m = TransformationMatrix.Matrix;
        m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
        var drawingCanvasPosition = m.Transform(canvasPosition);
        //Debug.WriteLine($"MainWindowAcceptHandOver: CanvasPosition Y={canvasPosition.Y:F0} X={canvasPosition.X:F0}  DrawingCanvasPosition Y={drawingCanvasPosition.Y:F0} X={drawingCanvasPosition.X:F0}");

        BoardIM.Selection.Clear();
        foreach (var pt in playerIM.Selection)
        {
            int row = (int)Math.Floor(drawingCanvasPosition.Y / UnitSize + 0.5);
            int col = (int)Math.Floor(drawingCanvasPosition.X / UnitSize + 0.5);
            //Debug.WriteLine($"MainWindowAcceptHandOver: row={row} col={col}   Offset Y={pt.Offset.Y:F0} X={pt.Offset.X:F0}");

            var dupTile = BoardDrawingCanvasAddUITile(pt.Tile, new RowCol(row, col), true);
            dupTile.Offset = pt.Offset;
            BoardIM.Selection.Add(dupTile);
            ViewModel.RemoveUITileFromHand(pt.UIT);
        }

        playerIM.Selection.Clear();

        BoardIM.IM_HandOver_MouseDown(BoardCanvas, BoardDrawingCanvas, TransformationMatrix.Matrix);
        BoardCanvas.MouseMove -= BoardCanvas_MouseMoveWhenUp;
        BoardCanvas.MouseMove += BoardCanvas_MouseMoveWhenDown;

        // Restart MouseMove events processing
        HandOverState = HandOverStateEnum.Active;
    }

    internal void Refresh() 
        => Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
}

internal class BoardInteractionManager(HashSet<UITileRowCol> currentMoves, MainWindow view, MainViewModel viewModel): InteractionManager
{
    private readonly HashSet<UITileRowCol> CurrentMoves = currentMoves;
    private readonly MainWindow View = view;
    private readonly MainViewModel ViewModel = viewModel;

    // Terminate immediately animations in progress, set final values
    public override void EndAnimationsInProgress()
    {
        TraceCall("over Board.");

        if (View.IsMatrixAnimationInProgress)
            View.EndMatrixAnimation();
        if (View.IsMoveUITileAnimationInProgress)
            View.EndMoveUITileAnimation();
    }

    internal override void UpdateTargetPosition()
    {
        TraceCall("over Board.");

        Debug.Assert(!Selection.IsEmpty);

        // Find a free position
        // Build NewCurrentMoves without tiles being moved
        var NewCurrentMoves = new HashSet<UITileRowCol>();
        foreach (UITileRowCol uitp in CurrentMoves)
            if (!Selection.ContainsUITile(uitp.UIT))
                NewCurrentMoves.Add(uitp);

        // Search first if drop point is Ok, using a shift offset of (0,0)
        int deltaRow = 0;
        int deltaCol = 0;
        if (!OkPosition(0, 0))
        {
            // Then explore in concentric squares around drop position
            for (int delta = 1; ; delta++)
            {
                // First we search for N, S, W and E squares
                if (OkPosition(-delta, 0))
                    goto ValidDropPointFound;
                if (OkPosition(+delta, 0))
                    goto ValidDropPointFound;
                if (OkPosition(0, -delta))
                    goto ValidDropPointFound;
                if (OkPosition(0, +delta))
                    goto ValidDropPointFound;

                // Then expand to square corners
                for (int offset = 1; offset <= delta; offset++)
                {
                    if (OkPosition(-delta, -offset))
                        goto ValidDropPointFound;
                    if (OkPosition(-delta, +offset))
                        goto ValidDropPointFound;
                    if (OkPosition(+delta, -offset))
                        goto ValidDropPointFound;
                    if (OkPosition(+delta, +offset))
                        goto ValidDropPointFound;
                    // Avoid doing corners twice
                    if (offset < delta)
                    {
                        if (OkPosition(-offset, -delta))
                            goto ValidDropPointFound;
                        if (OkPosition(+offset, -delta))
                            goto ValidDropPointFound;
                        if (OkPosition(-offset, +delta))
                            goto ValidDropPointFound;
                        if (OkPosition(+offset, +delta))
                            goto ValidDropPointFound;
                    }
                }
            }
        }

    ValidDropPointFound:
        // Helper, find out if all prospect target position are Ok once shifted of (dr, dc)
        bool OkPosition(int dr, int dc)
        {
            var NewCurrentMoves2 = new HashSet<UITileRowCol>(NewCurrentMoves);

            foreach (UITileRowCol uitp in Selection)
            {
                double left = (double)uitp.UIT.GetValue(Canvas.LeftProperty);
                double top = (double)uitp.UIT.GetValue(Canvas.TopProperty);

                // Candidate position shifted of (dr, dc)
                int row = (int)Math.Floor(top / UnitSize + 0.5) + dr;
                int col = (int)Math.Floor(left / UnitSize + 0.5) + dc;

                if (ViewModel.GetCellState(row, col) == CellState.Tiled ||
                    NewCurrentMoves2.Any(uitp => uitp.RC.Row == row && uitp.RC.Col == col))
                    return false;

                //uitp.RC = new RowCol(row, col);
                NewCurrentMoves2.Add(new UITileRowCol(uitp.UIT, new RowCol(row, col)));
            }
            deltaRow = dr;
            deltaCol = dc;
            return true;
        }

        // Appliying shifd (deltaRow, deltaCol)
        // Offset vector contains target position on DrawingCanvas
        foreach (UITileRowCol uitp in Selection)
        {
            double left = (double)uitp.UIT.GetValue(Canvas.LeftProperty);
            double top = (double)uitp.UIT.GetValue(Canvas.TopProperty);

            // Shifted position
            int row = (int)Math.Floor(top / UnitSize + 0.5) + deltaRow;
            int col = (int)Math.Floor(left / UnitSize + 0.5) + deltaCol;

            uitp.RC = new RowCol(row, col);
            uitp.Offset = new Vector(col * UnitSize, row * UnitSize);
        }

        // In case of handover ending, update CurrentMoves
        if (HandOverState == HandOverStateEnum.Active)
        {
            foreach (UITileRowCol uitp in Selection)
                ViewModel.CurrentMoves.Add(uitp);
            HandOverState = HandOverStateEnum.Inactive;
        }
        else
        {
            ViewModel.UpdateCurrentMoves(Selection);
        }

        // With animation
        //MoveSelection(row * UnitSize, col * UnitSize);
        //FinalRefreshAfterUpdate();

        // For now, direct move for testing
        // ToDo: Replace by animation using storyboard
        foreach (UITileRowCol uitp in Selection)
        {
            uitp.UIT.SetValue(Canvas.TopProperty, uitp.Offset.Y);
            uitp.UIT.SetValue(Canvas.LeftProperty, uitp.Offset.X);
            var h = ViewModel.CurrentMoves.First(u => u.UIT == uitp.UIT);
            Debug.Assert(h != null);
            h.RC = uitp.RC;
        }

        View.RescaleAndCenter(true);
    }

    public override void OnMouseRightButtonDown(object sender, UITilesSelection selection, bool tileHit)
    {
        TraceCall("over Board.");

        ContextMenu? cm;
        if (tileHit)
            cm = View.FindResource("MoveTileMenu") as ContextMenu;
        else
            cm = View.FindResource("BoardTileAndBackgroundMenu") as ContextMenu;
        Debug.Assert(cm != null);
        cm.PlacementTarget = sender as UIElement;
        cm.IsOpen = true;
    }

    internal override Point IM_MouseMoveWhenDown(object sender, MouseEventArgs e, Canvas c, Matrix m)
    {
        TraceCall("over Board.");

        var canvasMousePosition = e.GetPosition(c);
        var drawingCanvasMousePosition = base.IM_MouseMoveWhenDown(sender, e, c, m);

        if (pmm == null)
        {
            //Debug.WriteLine($"PreviousMousePosition: {previousMousePosition}");
            // move drawing surface
            if ((canvasMousePosition - mouseDownStartPosition).Length >= SmallMouseMoveLengthThreshold)
            {
                var delta = canvasMousePosition - previousMousePosition;
                previousMousePosition = canvasMousePosition;
                m.Translate(delta.X, delta.Y);
                View.TransformationMatrix.Matrix = m;
                View.UpdateBackgroundGrid();
            }
        }

        return drawingCanvasMousePosition;
    }
}
