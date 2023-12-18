// QwirkleUI
// WPF interface for Qwirkle project
//
// 2023-12-10   PV      First version, convert SVG tiles from https://fr.wikipedia.org/wiki/Qwirkle in XAML using Inkscape

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
using static QwirkleUI.ViewHelpers;

namespace QwirkleUI;

internal struct MainGridSelection
{
    public UITile? uitile;
    public int startRow, startCol;
}

public partial class MainWindow: Window
{
    private readonly MainViewModel viewModel;
    private MainGridSelection Selection = new();
    private readonly List<UITile> m_UITilesList = [];
    private readonly HandViewModel[] HandViewModels = [];

    public MainWindow()
    {
        InitializeComponent();
        viewModel = new MainViewModel(this);
        DataContext = viewModel;

        // Just 1 player for now
        HandViewModels = new HandViewModel[1];
        HandViewModels[0] = new HandViewModel(Player1HandUserControl, viewModel.GetModel, 0);

        // Can only reference ActualWidth after Window is loaded
        Loaded += MainWindow_Loaded;
        KeyDown += MainWindow_KeyDown;
    }

    public void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        viewModel.InitializeBoard();
        DrawBoard();
        RescaleAndCenter(false);

        foreach (var hvm in HandViewModels)
            hvm.DrawHand();

        viewModel.StatusText = "Done.";
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            EndAnimationsInProgress();

            // Move in progress?
            if (pmm != null)
            {
                MainGrid.MouseMove -= MainGrid_MouseMoveWhenDown;
                MainGrid.MouseMove += MainGrid_MouseMoveWhenUp;
                pmm = null;
                Mouse.Capture(null);
                return;
            }
        }
        //else if (e.Key == Key.A && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
        //{
        //    // Select all
        //    m_Sel.Add(m_WordAndCanvasList);
        //}
    }

    // Clears the grid
    internal void ClearGrid()
    {
        // Clear previous elements layout
        DrawingCanvas.Children.Clear();
        ClearBackgroundGrid();
        viewModel.StatusText = "Clear.";
        m_UITilesList.Clear();
        Selection.uitile = null;
    }

    internal void DrawBoard()
    {
        // ToDo: Make sure that DrawingCanvas.Children is empty
        Debug.Assert(DrawingCanvas.Children.Count == 0);

        UpdateBackgroundGrid();
        viewModel.DrawAllTiles();
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
        double rescaleFactorX = ClippingCanvas.ActualWidth / (p2Screen.X - p1Screen.X);
        double rescaleFactorY = ClippingCanvas.ActualHeight / (p2Screen.Y - p1Screen.Y);
        double rescaleFactor = Math.Min(rescaleFactorX, rescaleFactorY);
        rescaleMatrix.Scale(rescaleFactor, rescaleFactor);

        // Then adjust location and center
        p1Screen = rescaleMatrix.Transform(p1Grid);
        p2Screen = rescaleMatrix.Transform(p2Grid);
        double offX1 = -p1Screen.X;
        double offX2 = ClippingCanvas.ActualWidth - p2Screen.X;
        double offY1 = -p1Screen.Y;
        double offY2 = ClippingCanvas.ActualHeight - p2Screen.Y;
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

    private bool IsMatrixAnimationInProgress;
    private Matrix rescaleMatrix;

    // Event handler when MatrixAnimation is completed, need to "free" animated properties otherwise
    // they're "hold" by animation
    private void MatrixAnimationCompleted(object? sender, EventArgs e) => EndMatrixAnimation();

    // Terminate transformation in a clean way, "freeing" animated properties
    private void EndMatrixAnimation()
    {
        IsMatrixAnimationInProgress = false;
        TransformationMatrix.BeginAnimation(MatrixTransform.MatrixProperty, null);

        // Final tasks
        TransformationMatrix.Matrix = rescaleMatrix;
    }

    // --------------------------------------------------------------------
    // Mouse click and drag management

    private Point previousMouseRowCol;

    // null indicates background grid move, or delegate must be executed by MouseMove to perform move
    // action, P is current mouse coordinates in non-transformed user space
    private Action<Point>? pmm;

    private void MainGrid_MouseMoveWhenUp(object sender, MouseEventArgs e)
    {
        // Maybe provide word hovering visual feed-back? Or a tooltip with debug info?
    }

    // Terminate immediately animations in progress, set final values
    internal void EndAnimationsInProgress()
    {
        if (IsMatrixAnimationInProgress)
            EndMatrixAnimation();
        if (IsMoveUITileAnimationInProgress)
            EndMoveUITileAnimation();
    }

    private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        EndAnimationsInProgress();

        MainGrid.MouseMove -= MainGrid_MouseMoveWhenUp;
        MainGrid.MouseMove += MainGrid_MouseMoveWhenDown;
        previousMouseRowCol = e.GetPosition(MainGrid);

        Selection.uitile = GetHitHile(e.GetPosition(DrawingCanvas), DrawingCanvas);

        if (Selection.uitile != null)
            pmm = GetMouseDownMoveAction();
        else
            pmm = null;

        // Be sure to call GetRowCol before Capture, otherwise GetRowCol returns 0 after Capture
        // Capture to get MouseUp event raised by grid
        Mouse.Capture(MainGrid);
    }

    // Separate from MainGrid_MouseDown to reduce complexity
    private Action<Point> GetMouseDownMoveAction()
    {
        Debug.Assert(Selection.uitile != null);
        // Reverse-transform mouse Grid coordinates into DrawingCanvas coordinates
        Matrix m = TransformationMatrix.Matrix;
        m.Invert();     // To convert from screen transformed coordinates into ideal grid
                        // coordinates starting at (0,0) with a square side of UnitSize
        Vector clickOffset = new();
        double startLeft = (double)Selection.uitile.GetValue(Canvas.LeftProperty);
        double startTop = (double)Selection.uitile.GetValue(Canvas.TopProperty);
        var p = new Point(startLeft, startTop);
        clickOffset = p - m.Transform(previousMouseRowCol);

        Selection.startRow = (int)Math.Floor(startTop / UnitSize + 0.5);
        Selection.startCol = (int)Math.Floor(startLeft / UnitSize + 0.5);

        // Move as last child of DrawingCanvas so it's drawn on top
        DrawingCanvas.Children.Remove(Selection.uitile);
        DrawingCanvas.Children.Add(Selection.uitile);

        // When moving, point is current mouse in ideal grid coordinates
        return point =>
        {
            // Just move selected tile
            double preciseTop = point.Y + clickOffset.Y;
            double preciseLeft = point.X + clickOffset.X;

            Selection.uitile.SetValue(Canvas.TopProperty, preciseTop);
            Selection.uitile.SetValue(Canvas.LeftProperty, preciseLeft);

            // Round position to closest square on the grid
            int row = (int)Math.Floor(preciseTop / UnitSize + 0.5);
            int col = (int)Math.Floor(preciseLeft / UnitSize + 0.5);

            Selection.uitile.Hatched = !(viewModel.GetCellState(row, col) != CellState.Tiled || (row == Selection.startRow && col == Selection.startCol));
        };
    }

    // Relay from Window_MouseDown handler when it's actually a right click
    private void MainGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Selection.uitile = GetHitHile(e.GetPosition(DrawingCanvas), DrawingCanvas);

        ContextMenu? cm;
        if (Selection.uitile != null)
            cm = FindResource("TileCanvasMenu") as ContextMenu;
        else
            cm = FindResource("BackgroundCanvasMenu") as ContextMenu;
        Debug.Assert(cm != null);
        cm.PlacementTarget = sender as UIElement;
        cm.IsOpen = true;
        e.Handled = true;
    }

    private void MainGrid_MouseMoveWhenDown(object sender, MouseEventArgs e)
    {
        var newRowCol = e.GetPosition(MainGrid);
        Matrix m = TransformationMatrix.Matrix;

        if (pmm == null)
        {
            // move drawing surface
            var delta = newRowCol - previousMouseRowCol;
            previousMouseRowCol = newRowCol;
            m.Translate(delta.X, delta.Y);
            TransformationMatrix.Matrix = m;
            UpdateBackgroundGrid();
        }
        else
        {
            // Move selected word using generated lambda and capture on click down
            m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
            pmm(m.Transform(newRowCol));
        }
    }

    private void MainGrid_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Mouse.Capture(null);
        MainGrid.MouseMove -= MainGrid_MouseMoveWhenDown;
        MainGrid.MouseMove += MainGrid_MouseMoveWhenUp;

        if (pmm != null)
        {
            pmm = null;
            Debug.Assert(Selection.uitile != null);

            // End of visual feed-back, align on grid
            // Round position to closest square on the grid
            int row = (int)Math.Floor((double)Selection.uitile.GetValue(Canvas.TopProperty) / UnitSize + 0.5);
            int col = (int)Math.Floor((double)Selection.uitile.GetValue(Canvas.LeftProperty) / UnitSize + 0.5);

            if (viewModel.GetCellState(row, col) == CellState.Tiled)
            {
                row = Selection.startRow;
                col = Selection.startCol;
            }

            // ToDo: Update tiles being placed in VM (no not update model yet)

            Selection.uitile.Hatched = false;

            // With animation
            MoveSelection(row * UnitSize, col * UnitSize);      
            
            // Without animation
            //Selection.uitile.SetValue(Canvas.TopProperty, row * UnitSize);
            //Selection.uitile.SetValue(Canvas.LeftProperty, col * UnitSize);

            //FinalRefreshAfterUpdate();
        }
    }

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

    private bool IsMoveUITileAnimationInProgress;
    private readonly List<(UITile, DependencyProperty, double)> finalMoveUITileAnimationData = [];

    // Stops animation and set the final value
    private void EndMoveUITileAnimation()
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
        var boardBounds = viewModel.Bounds;
        return new (boardBounds.Min.Row - 5,
                    boardBounds.Max.Row + 5,
                    boardBounds.Min.Col - 5,
                    boardBounds.Max.Col + 5);
    }

    private void UpdateBackgroundGrid()
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

    internal void AddUITile(RowCol position, string shapeColor, int instance)
    {
        var t = new UITile(shapeColor, instance);
        t.SetValue(Canvas.TopProperty, position.Row * UnitSize);
        t.SetValue(Canvas.LeftProperty, position.Col * UnitSize);
        t.Width = UnitSize;
        t.Height = UnitSize;
        DrawingCanvas.Children.Add(t);

        //Debug.Assert(t.Row == position.Row);
        //Debug.Assert(t.Col == position.Column);
    }

    internal void AddCircle(RowCol position)
    {
        var e = new Ellipse();
        e.Width = 2 * UnitSize;
        e.Height = 2*UnitSize;
        e.Stroke = Brushes.Black;
        e.StrokeThickness = 5.0;
        //e.Fill = Brushes.SkyBlue;
        e.SetValue(Canvas.TopProperty, (position.Row-0.5) * UnitSize);
        e.SetValue(Canvas.LeftProperty, (position.Col-0.5) * UnitSize);
        DrawingCanvas.Children.Add(e);
    }
}