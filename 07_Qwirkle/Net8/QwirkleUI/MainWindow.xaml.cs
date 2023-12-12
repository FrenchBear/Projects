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
using System.Windows.Shapes;
using static QwirkleUI.App;

namespace QwirkleUI;

struct Selection
{
    public UITile? uitile;
    public int startRow, startCol;
}

public partial class MainWindow: Window
{
    private readonly ViewModel viewModel;
    private Selection Selection = new();
    private readonly List<UITile> m_UITilesList = [];

    public MainWindow()
    {
        InitializeComponent();
        viewModel = new ViewModel(this);
        DataContext = viewModel;

        // Can only reference ActualWidth after Window is loaded
        Loaded += MainWindow_Loaded;
        KeyDown += MainWindow_KeyDown;
        // ContentRendered += (sender, e) => Environment.Exit(0);       // For performance testing

    }

    public void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        viewModel.InitializeBoard();
        FinalRefreshAfterUpdate();
        RescaleAndCenter(false);
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

    internal void FinalRefreshAfterUpdate()
    {
        UpdateBackgroundGrid();
        viewModel.DrawAllTiles();
        viewModel.StatusText = "Done.";
    }

    // Adjust scale and origin to see the whole puzzle
    internal void RescaleAndCenter(bool isWithAnimation)
    {
        BoundingRectangle r = viewModel.Bounds;
        // Add some extra margin and always represent a 10x10 grid at minimum
        r = new BoundingRectangle(Math.Min(45, r.Min.Row - 3), Math.Max(55, r.Max.Row + 4), Math.Min(45, r.Min.Column - 3), Math.Max(55, r.Max.Column + 4));

        // Reverse-transform corners into WordCanvas coordinates
        var p1Grid = new Point(r.Min.Column * UnitSize, r.Min.Row * UnitSize);
        var p2Grid = new Point(r.Max.Column * UnitSize, r.Max.Row * UnitSize);

        rescaleMatrix = MainMatrixTransform.Matrix;

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
                From = MainMatrixTransform.Matrix,
                To = rescaleMatrix,
                Duration = new Duration(TimeSpan.FromSeconds(0.35))
            };
            ma.Completed += MatrixAnimationCompleted;
            IsMatrixAnimationInProgress = true;
            MainMatrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, ma);
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
        MainMatrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, null);

        // Final tasks
        MainMatrixTransform.Matrix = rescaleMatrix;
    }

    // --------------------------------------------------------------------
    // Mouse click and drag management

    private Point previousMousePosition;

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
    }

    private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        EndAnimationsInProgress();

        MainGrid.MouseMove -= MainGrid_MouseMoveWhenUp;
        MainGrid.MouseMove += MainGrid_MouseMoveWhenDown;
        previousMousePosition = e.GetPosition(MainGrid);

        Selection.uitile = GetHitHile(e);

        if (Selection.uitile != null)
            pmm = GetMouseDownMoveAction();
        else
            pmm = null;

        // Be sure to call GetPosition before Capture, otherwise GetPosition returns 0 after Capture
        // Capture to get MouseUp event raised by grid
        Mouse.Capture(MainGrid);
    }

    // Separate from MainGrid_MouseDown to reduce complexity
    private Action<Point> GetMouseDownMoveAction()
    {
        Debug.Assert(Selection.uitile != null);
        // Reverse-transform mouse Grid coordinates into DrawingCanvas coordinates
        Matrix m = MainMatrixTransform.Matrix;
        m.Invert();     // To convert from screen transformed coordinates into ideal grid
                        // coordinates starting at (0,0) with a square side of UnitSize
        Vector clickOffset = new();
        var p = new Point((double)Selection.uitile.GetValue(Canvas.LeftProperty), (double)Selection.uitile.GetValue(Canvas.TopProperty));
        clickOffset = p - m.Transform(previousMousePosition);

        Selection.startRow = Selection.uitile.Row;
        Selection.startCol = Selection.uitile.Col;

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

            Selection.uitile.SelectionBorder = !(viewModel.GetCellState(row, col) != CellState.Tiled || (row == Selection.startRow && col == Selection.startCol));
        };
    }

    UITile? GetHitHile(MouseButtonEventArgs e)
    {
        if (DrawingCanvas.InputHitTest(e.GetPosition(DrawingCanvas)) is not DependencyObject h)
            return null;
        for (; ; )
        {
            h = VisualTreeHelper.GetParent(h);
            if (h == null || h is Canvas)
                return null;
            if (h is UITile ut)
                return ut;
        }
    }

    // Relay from Window_MouseDown handler when it's actually a right click
    private void MainGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Selection.uitile = GetHitHile(e);

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
        var newPosition = e.GetPosition(MainGrid);
        Matrix m = MainMatrixTransform.Matrix;

        if (pmm == null)
        {
            // move drawing surface
            var delta = newPosition - previousMousePosition;
            previousMousePosition = newPosition;
            m.Translate(delta.X, delta.Y);
            MainMatrixTransform.Matrix = m;
            UpdateBackgroundGrid();
        }
        else
        {
            // Move selected word using generated lambda and capture on click down
            m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
            pmm(m.Transform(newPosition));
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

            // End of visual feed-back, align on grid, and update ViewModel
            // Round position to closest square on the grid
            int row = (int)Math.Floor((double)Selection.uitile.GetValue(Canvas.TopProperty) / UnitSize + 0.5);
            int col = (int)Math.Floor((double)Selection.uitile.GetValue(Canvas.LeftProperty) / UnitSize + 0.5);

            if (viewModel.GetCellState(row, col) == CellState.Tiled)
            {
                row = Selection.startRow;
                col = Selection.startCol;
            }

            Selection.uitile.SetValue(Canvas.TopProperty, row * UnitSize);
            Selection.uitile.SetValue(Canvas.LeftProperty, col * UnitSize);
            Selection.uitile.SelectionBorder = false;

            //FinalRefreshAfterUpdate();
        }
    }

    private void MainGrid_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        var newPosition = e.GetPosition(MainGrid);
        var m = MainMatrixTransform.Matrix;

        // Ctrl+MouseWheel for rotation
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            double angle = e.Delta / 16.0;
            m.RotateAt(angle, newPosition.X, newPosition.Y);
        }
        else
        {
            var sign = -Math.Sign(e.Delta);
            var scale = 1 - sign / 10.0;
            m.ScaleAt(scale, scale, newPosition.X, newPosition.Y);
        }
        MainMatrixTransform.Matrix = m;

        UpdateBackgroundGrid();
    }

    // Grid currently drawn
    private BoundingRectangle? gridBounding;

    private void ClearBackgroundGrid()
    {
        BackgroundGrid.Children.Clear();
        gridBounding = new BoundingRectangle(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
    }

    private void UpdateBackgroundGrid()
    {
        var bounds = viewModel.Bounds;

        // Add some extra margin and always represent a 10x10 grid at minimum
        var r = new BoundingRectangle(
            Math.Min(45, bounds.Min.Row - 2),
            Math.Max(55, bounds.Max.Row + 3),
            Math.Min(45, bounds.Min.Column - 2),
            Math.Max(55, bounds.Max.Column + 3));

        if (!r.Equals(gridBounding))
        {
            ClearBackgroundGrid();
            gridBounding = r;

            for (int row = r.Min.Row; row <= r.Max.Row; row++)
            {
                var l = new Line
                {
                    X1 = r.Min.Column * UnitSize,
                    X2 = r.Max.Column * UnitSize,
                    Y1 = row * UnitSize,
                    Y2 = row * UnitSize,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = row == 0 ? 3 : 1
                };
                BackgroundGrid.Children.Add(l);
            }

            for (int column = r.Min.Column; column <= r.Max.Column; column++)
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

    internal void AddUITile(Position position, string shapeColor)
    {
        var t = new UITile();
        t.ShapeColor = shapeColor;
        t.SetValue(Canvas.TopProperty, position.Row * UnitSize);
        t.SetValue(Canvas.LeftProperty, position.Column * UnitSize);
        t.Width = UnitSize;
        t.Height = UnitSize;
        DrawingCanvas.Children.Add(t);

        Debug.Assert(t.Row == position.Row);
        Debug.Assert(t.Col == position.Column);
    }
}