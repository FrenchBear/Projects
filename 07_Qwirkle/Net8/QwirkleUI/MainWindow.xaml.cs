// QwirkleUI
// WPF interface for Qwirkle project
//
// 2023-12-10   PV      First version, convert SVG tiles from https://fr.wikipedia.org/wiki/Qwirkle in XAML using Inkscape
// 2023-12-19   PV      Refactoring using InteractionManager

using LibQwirkle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using static QwirkleUI.App;

namespace QwirkleUI;

internal struct MainGridSelection
{
    public UITile? uitile;
    public int startRow, startCol;
}

public partial class MainWindow: Window
{
    private readonly MainViewModel ViewModel;
    private MainGridSelection Selection = new();
    private readonly List<UITile> m_UITilesList = [];
    private readonly HandViewModel[] HandViewModels = [];
    internal readonly HashSet<UITileRowCol> CurrentMoves = [];
    internal InteractionManager BoardIM;

    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainViewModel(this);
        DataContext = ViewModel;

        // Just 1 player for now
        HandViewModels = new HandViewModel[1];
        HandViewModels[0] = new HandViewModel(this, Player1HandUserControl, ViewModel.GetModel, 0);

        BoardIM = new BoardInteractionManager(CurrentMoves, this, ViewModel);

        // Can only reference ActualWidth after Window is loaded
        Loaded += MainWindow_Loaded;
        KeyDown += MainWindow_KeyDown;
    }

    public void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.InitializeBoard();
        DrawBoard();
        RescaleAndCenter(false);

        foreach (var hvm in HandViewModels)
            hvm.DrawHand();

        ViewModel.StatusText = "Done.";
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            BoardIM.EndAnimationsInProgress();
            BoardIM.EndMoveInProgress();
            MainGrid.MouseMove -= MainGrid_MouseMoveWhenDown;
            MainGrid.MouseMove += MainGrid_MouseMoveWhenUp;
        }
    }

    // Clears the grid
    internal void ClearGrid()
    {
        // Clear previous elements layout
        BoardDrawingCanvas.Children.Clear();
        ClearBackgroundGrid();
        ViewModel.StatusText = "Clear.";
        m_UITilesList.Clear();
        Selection.uitile = null;
    }

    internal void DrawBoard()
    {
        // ToDo: Make sure that BoardDrawingCanvas.Children is empty
        Debug.Assert(BoardDrawingCanvas.Children.Count == 0);

        UpdateBackgroundGrid();
        ViewModel.DrawBoard();
        ViewModel.DrawCurrentMoves();
    }

    // Adjust scale and origin to see the whole puzzle
    internal void RescaleAndCenter(bool isWithAnimation)
    {
        // Add some extra margin and always represent a 10x10 grid at minimum
        var r = BoundingRectangleWithMargins();

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
    private void MainGrid_MouseMoveWhenUp(object sender, MouseEventArgs e)
        => BoardIM.IM_MouseMoveWhenUp(sender, e);

    private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        MainGrid.MouseMove -= MainGrid_MouseMoveWhenUp;
        MainGrid.MouseMove += MainGrid_MouseMoveWhenDown;
        BoardIM.IM_MouseDown(sender, e, BoardCanvas, BoardDrawingCanvas, TransformationMatrix.Matrix);
    }

    // Relay from Window_MouseDown handler when it's actually a right click
    private void MainGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        => BoardIM.IM_MouseRightButtonDown(sender, e, BoardCanvas, BoardDrawingCanvas);

    private void MainGrid_MouseMoveWhenDown(object sender, MouseEventArgs e)
        => BoardIM.IM_MouseMoveWhenDown(sender, e, BoardCanvas, TransformationMatrix.Matrix);

    private void MainGrid_MouseUp(object sender, MouseButtonEventArgs e)
    {
        MainGrid.MouseMove -= MainGrid_MouseMoveWhenDown;
        MainGrid.MouseMove += MainGrid_MouseMoveWhenUp;
        BoardIM.IM_MouseUp(sender, e);
    }

    // ANimated move, not used right now...
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

    private void Sb_Completed(object? sender, EventArgs e) => EndMoveUITileAnimation();

    internal bool IsMoveUITileAnimationInProgress;
    private readonly List<(UITile, DependencyProperty, double)> finalMoveUITileAnimationData = [];

    // Stops animation and set the final value
    internal void EndMoveUITileAnimation()
    {
        IsMoveUITileAnimationInProgress = false;
        foreach (var item in finalMoveUITileAnimationData)
        {
            item.Item1.BeginAnimation(item.Item2, null);
            item.Item1.SetValue(item.Item2, item.Item3);
        }
    }

    private void MainGrid_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        var newRowCol = e.GetPosition(MainGrid);
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
    private BoundingRectangle CurrentGridBoundingWithMargins = new(45, 55, 45, 55);

    private void ClearBackgroundGrid()
    {
        BackgroundGrid.Children.Clear();
        CurrentGridBoundingWithMargins = new(45, 55, 45, 55);
    }

    // Add some extra margin and always represent a 10x10 grid at minimum
    private BoundingRectangle BoundingRectangleWithMargins()
    {
        var boardBounds = ViewModel.Bounds;
        return new(boardBounds.Min.Row - 5,
                    boardBounds.Max.Row + 5,
                    boardBounds.Min.Col - 5,
                    boardBounds.Max.Col + 5);
    }

    internal void UpdateBackgroundGrid()
    {
        var r = BoundingRectangleWithMargins();
        if (!r.Equals(CurrentGridBoundingWithMargins))
        {
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
                BackgroundGrid.Children.Add(l);
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
                BackgroundGrid.Children.Add(l);
            }
        }
    }

    internal UITileRowCol AddUITile(RowCol position, string shapeColor, int instance, bool gray)
    {
        var t = new UITile(shapeColor, instance);
        t.GrayBackground = gray;
        t.SetValue(Canvas.TopProperty, position.Row * UnitSize);
        t.SetValue(Canvas.LeftProperty, position.Col * UnitSize);
        t.Width = UnitSize;
        t.Height = UnitSize;
        BoardDrawingCanvas.Children.Add(t);

        return new UITileRowCol(t, position);
    }

    internal void AddCircle(RowCol position)
    {
        var e = new Ellipse();
        e.Width = 2 * UnitSize;
        e.Height = 2 * UnitSize;
        e.Stroke = Brushes.Black;
        e.StrokeThickness = 5.0;
        e.SetValue(Canvas.TopProperty, (position.Row - 0.5) * UnitSize);
        e.SetValue(Canvas.LeftProperty, (position.Col - 0.5) * UnitSize);
        BoardDrawingCanvas.Children.Add(e);
    }

    internal void AcceptHandOver(InteractionManager im)
    {
        Debug.WriteLine("MainWindow.AcceptHandOver");
        Debug.Assert(im != null && !im.Selection.IsEmpty);
        Debug.WriteLine($"MainWindow: Accepting HandOver of {im.Selection.Count} tile(s)");
        // ToDo
    }
}

internal class BoardInteractionManager: InteractionManager
{
    private readonly HashSet<UITileRowCol> CurrentMoves;
    private readonly MainWindow View;
    private readonly MainViewModel ViewModel;

    public BoardInteractionManager(HashSet<UITileRowCol> currentMoves, MainWindow view, MainViewModel viewModel)
    {
        CurrentMoves = currentMoves;
        View = view;
        ViewModel = viewModel;
    }

    // Terminate immediately animations in progress, set final values
    public override void EndAnimationsInProgress()
    {
        if (View.IsMatrixAnimationInProgress)
            View.EndMatrixAnimation();
        if (View.IsMoveUITileAnimationInProgress)
            View.EndMoveUITileAnimation();
    }

    internal override void UpdateTargetPosition(UITilesSelection selection)
    {
        Debug.Assert(!Selection.IsEmpty);

        // Find a free position
        // Build NewCurrentMoves without tiles being moved
        var NewCurrentMoves = new HashSet<UITileRowCol>();
        foreach (UITileRowCol uitp in CurrentMoves)
            if (!Selection.ContainsUITile(uitp.UIT))
                NewCurrentMoves.Add(uitp);

        // Find out if all prospect target position are Ok
        bool rollback = false;
        foreach (UITileRowCol uitp in Selection)
        {
            double left = (double)uitp.UIT.GetValue(Canvas.LeftProperty);
            double top = (double)uitp.UIT.GetValue(Canvas.TopProperty);

            // Candidate position
            int row = (int)Math.Floor(top / UnitSize + 0.5);
            int col = (int)Math.Floor(left / UnitSize + 0.5);

            if (ViewModel.GetCellState(row, col) == CellState.Tiled ||
                NewCurrentMoves.Any(uitp => uitp.RC.Row == row && uitp.RC.Col == col))
            {
                rollback = true;
                break;
            }

            uitp.RC = new RowCol(row, col);
            NewCurrentMoves.Add(new UITileRowCol(uitp.UIT, new RowCol(row, col)));
        }

        if (rollback)
        {
            foreach (UITileRowCol uitp in Selection)
            {
                uitp.Offset = new Vector(uitp.StartRC.Col * UnitSize, uitp.StartRC.Row * UnitSize);
                uitp.UIT.Hatched = false;
            }
        }
        else
        {
            foreach (UITileRowCol uitp in Selection)
            {
                uitp.Offset = new Vector(uitp.RC.Col * UnitSize, uitp.RC.Row * UnitSize);
            }
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
            var h = View.CurrentMoves.First(u => u.UIT == uitp.UIT);
            Debug.Assert(h != null);
            h.RC = uitp.RC;
        }
    }

    public override void OnMouseRightButtonDown(object sender, UITilesSelection selection, bool tileHit)
    {
        ContextMenu? cm;
        if (tileHit)
            cm = View.FindResource("TileCanvasMenu") as ContextMenu;
        else
            cm = View.FindResource("BackgroundCanvasMenu") as ContextMenu;
        Debug.Assert(cm != null);
        cm.PlacementTarget = sender as UIElement;
        cm.IsOpen = true;
    }

    internal override void IM_MouseMoveWhenDown(object sender, MouseEventArgs e, Canvas c, Matrix m)
    {
        var newRowCol = e.GetPosition(c);
        base.IM_MouseMoveWhenDown(sender, e, c, m);

        if (pmm == null)
        {
            // move drawing surface
            var delta = newRowCol - previousMouseRowCol;
            previousMouseRowCol = newRowCol;
            m.Translate(delta.X, delta.Y);
            View.TransformationMatrix.Matrix = m;
            View.UpdateBackgroundGrid();
        }
    }
}
