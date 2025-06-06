﻿// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// Editor View, main surface to interact and edit layout
//
// 2017-07-22   PV      First version
// 2021-11-13   PV      Net6 C#10
// 2023-11-20   PV      Net8 C#12
// 2024-11-15	PV		Net9 C#13

using Bonza.Editor.Support;
using Bonza.Editor.ViewModel;
using Bonza.Generator;
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
using static Bonza.Editor.App;

namespace Bonza.Editor.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class EditorView: Window
{
    private readonly EditorViewModel viewModel;
    private readonly Selection m_Sel;                   // Manages current selection, internal since it's accessed from ViewModel
    private List<WordAndCanvas> m_WordAndCanvasList;    // Current list of WordAndCanvas managed by view

    // --------------------------------------------------------------------
    // Constructor and Window events

    public EditorView()
    {
        InitializeComponent();
        viewModel = new EditorViewModel(this);
        DataContext = viewModel;

        m_WordAndCanvasList = [];
        m_Sel = new Selection(viewModel);

        // Can only reference ActualWidth after Window is loaded
        Loaded += MainWindow_Loaded;
        KeyDown += MainWindow_KeyDown;
        // ContentRendered += (sender, e) => Environment.Exit(0);       // For performance testing
    }

    public void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // For development tests
        viewModel.AddWordsFromFile(@"..\..\Lists\Fruits.txt");
        // viewModel.LoadWordsList(@"..\..\Lists\Animals.txt");
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

                // Restore position of selected WordCanvas
                foreach (var wac in m_Sel.WordAndCanvasList)
                {
                    double top = wac.WordPosition.StartRow * UnitSize;
                    double left = wac.WordPosition.StartColumn * UnitSize;
                    wac.WordCanvas.SetValue(Canvas.TopProperty, top);
                    wac.WordCanvas.SetValue(Canvas.LeftProperty, left);
                }
                return;
            }

            // CLear selection if any
            if (m_Sel.WordAndCanvasList?.Count > 0)
                m_Sel.Clear();
        }
        else if (e.Key == Key.A && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
        {
            // Select all
            m_Sel.Add(m_WordAndCanvasList);
        }
    }

    // --------------------------------------------------------------------
    // Entry points for ViewModel

    // Clears the grid, before loading a new list of words or reorganizing layout
    internal void ClearWordAndCanvas()
    {
        // Clear previous elements layout
        DrawingCanvas.Children.Clear();
        ClearBackgroundGrid();
        viewModel.UpdateStatus(PlaceWordStatus.Valid);
        m_WordAndCanvasList = [];
        m_Sel.Clear();
    }

    // Initial drawing of for current model layout
    internal IList<WordAndCanvas> AddCanvasForWordPositionList(IEnumerable<WordPosition> wordPositionList)
    {
        var wordAndCanvasList = new List<WordAndCanvas>();

        // Draw letters
        foreach (WordPosition wp in wordPositionList)
        {
            var wc = new WordCanvas(wp);
            var wac = new WordAndCanvas(wp, wc);
            DrawingCanvas.Children.Add(wc);
            m_WordAndCanvasList.Add(wac);
            wordAndCanvasList.Add(wac);
        }

        return wordAndCanvasList;
    }

    // Remove all the words from selection
    internal void DeleteSelection()
    {
        Debug.Assert(m_Sel.WordAndCanvasList != null && m_Sel.WordAndCanvasList.Count > 0);

        DeleteWordAndCanvasList([.. m_Sel.WordAndCanvasList], true);
    }

    // More general Delete function, also used by Undo support
    internal void DeleteWordAndCanvasList(IList<WordAndCanvas> wordAndCanvasList, bool memorizeForUndo)
    {
        Debug.Assert(wordAndCanvasList != null && wordAndCanvasList.Count > 0);

        // Undo support
        if (memorizeForUndo)
            viewModel.UndoStack.MemorizeDelete(wordAndCanvasList);

        // Delete in View and ViewModel
        foreach (WordAndCanvas wac in wordAndCanvasList)
        {
            // Delete in selection
            if (m_Sel.WordAndCanvasList.Contains(wac))
                m_Sel.Delete(wac);

            DrawingCanvas.Children.Remove(wac.WordCanvas);
            m_WordAndCanvasList.Remove(wac);
            viewModel.RemoveWordPosition(wac.WordPosition);
        }

        FinalRefreshAfterUpdate();

        // If everything has been deleter, recenter automatically
        if (m_WordAndCanvasList.Count == 0)
            RescaleAndCenter(true);
    }

    internal void AddWordAndCanvasList(IList<WordAndCanvas> wordAndCanvasList, bool memorizeForUndo)
    {
        // Clear selection first, if it exists
        m_Sel.Clear();

        if (memorizeForUndo)
            viewModel.UndoStack.MemorizeAdd(m_WordAndCanvasList);

        // Add to View and ViewModel
        foreach (WordAndCanvas wac in wordAndCanvasList)
        {
            viewModel.AddWordPosition(wac.WordPosition);
            m_WordAndCanvasList.Add(wac);
            DrawingCanvas.Children.Add(wac.WordCanvas);
        }

        // Select all we've just restored
        m_Sel.Add(wordAndCanvasList);

        FinalRefreshAfterUpdate();
    }

    // Swaps a single word between horizontal and vertical orientation
    internal void SwapOrientation()
    {
        Debug.Assert(m_Sel.WordAndCanvasList != null && m_Sel.WordAndCanvasList.Count == 1);
        SwapOrientation(m_Sel.WordAndCanvasList, true);
    }

    internal void SwapOrientation(IList<WordAndCanvas> wordAndCanvasList, bool memorizeForUndo)
    {
        Debug.Assert(wordAndCanvasList != null && wordAndCanvasList.Count == 1);
        WordAndCanvas wac = wordAndCanvasList.First();

        if (memorizeForUndo)
        {
            viewModel.UndoStack.MemorizeSwapOrientation(wordAndCanvasList);
            viewModel.RemoveWordPosition(wac.WordPosition);

            wac.WordPosition.SetNewPositionOrientation(new PositionOrientation(wac.WordPosition.StartRow, wac.WordPosition.StartColumn, !wac.WordPosition.IsVertical));

            // Do not accept Illegal placements, adjust to only valid placements
            WordPositionLayout layout = viewModel.GetLayoutExcludingWordPosition(wac.WordPosition);
            var topLeftList = new List<PositionOrientation>
                {
                    new(wac.WordPosition.PositionOrientation)
                };
            AdjustToSuitableLocationInLayout(layout, wordAndCanvasList, topLeftList, true);
            wac.WordPosition.SetNewPositionOrientation(topLeftList[0]);
            viewModel.AddWordPosition(wac.WordPosition);
        }
        wac.RebuildCanvasAfterOrientationSwap();    // Only relocate visually letters of the word
        MoveWordAndCanvasList(wordAndCanvasList);   // Visual animation

        FinalRefreshAfterUpdate();
    }

    internal void AutoPlace()
    {
        if (m_Sel.WordAndCanvasList.Count != 1)
        {
            MessageBox.Show("Sorry, AutoPlace currently only works for a single word.", AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return;
        }
        Debug.Assert(m_Sel.WordAndCanvasList != null && m_Sel.WordAndCanvasList.Count == 1);

        // ToDo: Nice compact code, but this doesn't work for Undo
        // ToDo: Also support Undo when adding words
        // ToDo: Tweak PlaceWord optimization to avoid always replacing a word in the same place after moving it
        // Do: Keep the same WordAndCanvas and at the end, just process it as a simple move (which it is)
        // This will provide animation and undo support automatically

        //WordAndCanvas before = m_Sel.WordAndCanvasList[0];
        DeleteSelection();
        //WordAndCanvas after = viewModel.AddWordsList(new List<string> { before.WordPosition.OriginalWord })?.First();
    }

    internal void FinalRefreshAfterUpdate()
    {
        UpdateBackgroundGrid();

        PlaceWordStatus status = RecolorizeAllWords();
        viewModel.UpdateStatus(status);
    }

    // Adjust scale and origin to see the whole puzzle
    internal void RescaleAndCenter(bool isWithAnimation)
    {
        if (viewModel.Layout == null)
            return;

        BoundingRectangle r = viewModel.Layout.Bounds;
        // Add some extra margin and always represent a 20x20 grid at minimum
        r = new BoundingRectangle(Math.Min(-11, r.Min.Row - 3), Math.Max(11, r.Max.Row + 4), Math.Min(-11, r.Min.Column - 3), Math.Max(11, r.Max.Column + 4));

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
    private void MatrixAnimationCompleted(object sender, EventArgs e) => EndMatrixAnimation();

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
    private Action<Point> pmm;

    private void MainGrid_MouseMoveWhenUp(object sender, MouseEventArgs e)
    {
        // Maybe provide word hovering visual feed-back? Or a tooltip with debug info?
    }

    // Terminate immediately animations in progress, set final values
    internal void EndAnimationsInProgress()
    {
        if (IsMoveWordAnimationInProgress)
            EndMoveWordAnimation();
        if (IsMatrixAnimationInProgress)
            EndMatrixAnimation();
    }

    // Helper, this was originally in MainGrid_MouseDown handler, but when a right-click occurs,
    // it is assumed than it will select automatically a non-selected word, so code got promoted to its own function...
    private void UpdateSelectionAfterClick(MouseButtonEventArgs e)
    {
        if (DrawingCanvas.InputHitTest(e.GetPosition(DrawingCanvas)) is TextBlock hitTextBlock)
        {
            var hitC = hitTextBlock.Parent as WordCanvas;
            WordAndCanvas hit = m_WordAndCanvasList.FirstOrDefault(wac => ReferenceEquals(wac.WordCanvas, hitC));
            Debug.Assert(hit != null);

            //Debug.WriteLine("Hit " + hit.WordPosition.ToString());

            // If Ctrl key is NOT pressed, clear previous selection
            // But if we click again in something already selected, do not clear selection!
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                if (m_Sel.WordAndCanvasList != null && !m_Sel.WordAndCanvasList.Contains(hit))
                    m_Sel.Clear();

            // Add current word to selection
            m_Sel.Add(hit);

            // If Shift key is pressed, selection is extended to connected words
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                //viewModel.Layout.GetConnectedWordPositions(hitWP).ForEach(connected => AddWordPositionToSelection(connected));
                foreach (WordPosition connected in viewModel.Layout.GetConnectedWordPositions(hit.WordPosition))
                    m_Sel.Add(m_WordAndCanvasList.FirstOrDefault(wac => wac.WordPosition == connected));

            // Remove and add again elements to move so they're displayed above non-moved elements
            //foreach (WordCanvas wc in m_Sel.WordPositionList.Select(wp => Map.GetWordCanvasFromWordPosition(wp)))
            foreach (WordCanvas wc in m_Sel.WordAndCanvasList.Select(wac => wac.WordCanvas))
            {
                DrawingCanvas.Children.Remove(wc);
                DrawingCanvas.Children.Add(wc);
                wc.SetColor(Brushes.White, Brushes.DarkBlue);
            }
        }
    }

    private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        EndAnimationsInProgress();

        MainGrid.MouseMove -= MainGrid_MouseMoveWhenUp;
        MainGrid.MouseMove += MainGrid_MouseMoveWhenDown;
        previousMousePosition = e.GetPosition(MainGrid);
        UpdateSelectionAfterClick(e);

        // HitTest: Test if a word was clicked on, if true, hitTextBlock is a TextBloxk
        if (DrawingCanvas.InputHitTest(e.GetPosition(DrawingCanvas)) is TextBlock)
            // We're interested in its parent WordCanvas, that contains all the text blocks for the word
            pmm = GetMouseDownMoveAction();
        else
        {
            pmm = null;
            m_Sel.Clear();
        }

        // Be sure to call GetPosition before Capture, otherwise GetPosition returns 0 after Capture
        // Capture to get MouseUp event raised by grid
        Mouse.Capture(MainGrid);
    }

    // Layout of WordPosition that do not move
    private WordPositionLayout m_FixedLayout;

    // Separate from MainGrid_MouseDown to reduce complexity
    private Action<Point> GetMouseDownMoveAction()
    {
        // Need a layout without moved word to validate placement
        m_FixedLayout = viewModel.GetLayoutExcludingWordPositionList(m_Sel.WordAndCanvasList.Select(wac => wac.WordPosition));

        // Reverse-transform mouse Grid coordinates into DrawingCanvas coordinates
        Matrix m = MainMatrixTransform.Matrix;
        m.Invert();     // To convert from screen transformed coordinates into ideal grid
                        // coordinates starting at (0,0) with a square side of UnitSize
        var clickOffsetList = new List<Vector>(m_Sel.WordAndCanvasList.Count);
        clickOffsetList.AddRange(m_Sel.WordAndCanvasList
            .Select(wac => new Point((double)wac.WordCanvas.GetValue(Canvas.LeftProperty), (double)wac.WordCanvas.GetValue(Canvas.TopProperty)))
            .Select(canvasTopLeft => canvasTopLeft - m.Transform(previousMousePosition)));

        // When moving, point is current mouse in ideal grid coordinates
        return point =>
        {
            // Just move selected WordCanvas
            for (int i = 0; i < m_Sel.WordAndCanvasList.Count; i++)
            {
                double preciseTop = point.Y + clickOffsetList[i].Y;
                double preciseLeft = point.X + clickOffsetList[i].X;

                WordCanvas wc = m_Sel.WordAndCanvasList[i].WordCanvas;
                wc.SetValue(Canvas.TopProperty, preciseTop);
                wc.SetValue(Canvas.LeftProperty, preciseLeft);

                // Round position to closest square on the grid
                int top = (int)Math.Floor(preciseTop / UnitSize + 0.5);
                int left = (int)Math.Floor(preciseLeft / UnitSize + 0.5);

                PlaceWordStatus status = EditorViewModel.CanPlaceWordAtPositionInLayout(m_FixedLayout, m_Sel.WordAndCanvasList[i].WordPosition, new PositionOrientation(top, left, m_Sel.WordAndCanvasList[i].WordPosition.IsVertical));
                RecolorizeWord(m_Sel.WordAndCanvasList[i], status);
            }
        };
    }

    // Relay from Window_MouseDown handler when it's actually a right click
    private void MainGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        UpdateSelectionAfterClick(e);

        ContextMenu cm = DrawingCanvas.InputHitTest(e.GetPosition(DrawingCanvas)) is TextBlock
            ? FindResource("WordCanvasMenu") as ContextMenu
            : FindResource("BackgroundCanvasMenu") as ContextMenu;
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

            // End of visual feed-back, align on grid, and update ViewModel
            // Round position to closest square on the grid
            var topLeftList = new List<PositionOrientation>();
            foreach (WordAndCanvas wac in m_Sel.WordAndCanvasList)
            {
                WordCanvas wc = wac.WordCanvas;
                int top = (int)Math.Floor((double)wc.GetValue(Canvas.TopProperty) / UnitSize + 0.5);
                int left = (int)Math.Floor((double)wc.GetValue(Canvas.LeftProperty) / UnitSize + 0.5);
                topLeftList.Add(new PositionOrientation(top, left, wac.WordPosition.IsVertical));
            }

            // Do not accept Illegal placements, adjust to only valid placements
            AdjustToSuitableLocationInLayout(m_FixedLayout, m_Sel.WordAndCanvasList, topLeftList, true);

            // Move to final, rounded position
            viewModel.UpdateWordPositionLocation(m_Sel.WordAndCanvasList, topLeftList, true);     // Update WordPosition with new location
            MoveWordAndCanvasList(m_Sel.WordAndCanvasList);     // Visual animation

            FinalRefreshAfterUpdate();
        }
    }

    internal PlaceWordStatus RecolorizeAllWords() => RecolorizeWordAndCanvasList(m_WordAndCanvasList);

    internal PlaceWordStatus RecolorizeWordAndCanvasList(List<WordAndCanvas> wordAndCanvasList)
    {
        PlaceWordStatus result = PlaceWordStatus.Valid;
        foreach (WordAndCanvas wac in wordAndCanvasList)
        {
            var layout = viewModel.GetLayoutExcludingWordPosition(wac.WordPosition);
            PlaceWordStatus status = EditorViewModel.CanPlaceWordInLayout(layout, wac);
            RecolorizeWord(wac, status);
            if (status == PlaceWordStatus.TooClose && result == PlaceWordStatus.Valid)
                result = PlaceWordStatus.TooClose;
            else if (status == PlaceWordStatus.Invalid && result != PlaceWordStatus.Invalid)
                result = PlaceWordStatus.Invalid;
        }
        return result;
    }

    private void RecolorizeWord(WordAndCanvas wac, PlaceWordStatus status)
    {
        bool isInSelection = m_Sel.WordAndCanvasList?.Contains(wac) ?? false;
        switch (status)
        {
            case PlaceWordStatus.Valid:
                if (isInSelection)
                    wac.WordCanvas.SetColor(SelectedValidForeground, SelectedValidBackground);
                else
                    wac.WordCanvas.SetColor(NormalValidForeground, NormalValidBackground);
                break;

            case PlaceWordStatus.TooClose:
                if (isInSelection)
                    wac.WordCanvas.SetColor(SelectedTooCloseForeground, SelectedTooCloseBackground);
                else
                    wac.WordCanvas.SetColor(NormalTooCloseForeground, NormalTooCloseBackground);
                break;

            case PlaceWordStatus.Invalid:
                if (isInSelection)
                    wac.WordCanvas.SetColor(SelectedInvalidForeground, SelectedInvalidBackground);
                else
                    wac.WordCanvas.SetColor(NormalInvalidForeground, NormalInvalidBackground);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    // If position is not valid, look around until a valid position is found
    // Examine surrounding cells in a "snail pattern"
    private void AdjustToSuitableLocationInLayout(WordPositionLayout layout, IList<WordAndCanvas> wordAndCanvasList, List<PositionOrientation> topLeftList, bool onlyValidPlacement)
    {
        // Internal helper to check all words
        bool CanPlaceAllWords(bool isOnlyValidPlacement)
        {
            for (int il = 0; il < wordAndCanvasList.Count; il++)
            {
                var placmentStatus = EditorViewModel.CanPlaceWordAtPositionInLayout(layout, wordAndCanvasList[il].WordPosition, topLeftList[il]);
                if (placmentStatus == PlaceWordStatus.Invalid || (placmentStatus == PlaceWordStatus.TooClose && !isOnlyValidPlacement))
                    return false;
            }
            return true;
        }

        if (!CanPlaceAllWords(onlyValidPlacement))
        {
            int st = 1;
            int sign = 1;

            for (; ; )
            {
                for (int i = 0; i < st; i++)
                {
                    for (int il = 0; il < wordAndCanvasList.Count; il++)
                        topLeftList[il] = new PositionOrientation(topLeftList[il].StartRow, topLeftList[il].StartColumn + sign, m_Sel.WordAndCanvasList[il].WordPosition.IsVertical);
                    if (CanPlaceAllWords(true))
                        return;
                }
                for (int i = 0; i < st; i++)
                {
                    for (int il = 0; il < wordAndCanvasList.Count; il++)
                        topLeftList[il] = new PositionOrientation(topLeftList[il].StartRow + sign, topLeftList[il].StartColumn, m_Sel.WordAndCanvasList[il].WordPosition.IsVertical);
                    if (CanPlaceAllWords(true))
                        return;
                }
                sign = -sign;
                st++;
            }
        }
    }

    internal void MoveWordAndCanvasList(IList<WordAndCanvas> wordAndCanvasList)
    {
        ArgumentNullException.ThrowIfNull(wordAndCanvasList);
        if (wordAndCanvasList.Count == 0)
            throw new ArgumentException("Zero words in list", nameof(wordAndCanvasList));

        // If bounding rectangle is updated, need to redraw background grid
        BoundingRectangle r = viewModel.Layout.Bounds;
        if (!r.Equals(gridBounding))
            UpdateBackgroundGrid();

        // Compute distance moved on 1st element to choose animation speed (duration)
        WordPosition wp1 = wordAndCanvasList.First().WordPosition;
        WordCanvas wc1 = wordAndCanvasList.First().WordCanvas;
        double deltaX = (double)wc1.GetValue(Canvas.LeftProperty) - wp1.StartColumn * UnitSize;
        double deltaY = (double)wc1.GetValue(Canvas.TopProperty) - wp1.StartRow * UnitSize;
        double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        // If distance is null, for instance after a selection click, we're done
        if (distance <= 0.0001)
            return;

        // Group animations in a storyboard to simplify premature ending
        var sb = new Storyboard();
        var duration = new Duration(TimeSpan.FromSeconds(distance >= UnitSize ? 0.35 : 0.1));
        finalMoveWordAnimationData = [];
        foreach (WordAndCanvas wac in wordAndCanvasList)
        {
            WordCanvas wc = wac.WordCanvas;

            double finalLeftValue = wac.WordPosition.StartColumn * UnitSize;
            var daLeft = new DoubleAnimation
            {
                Duration = duration,
                From = (double)wc.GetValue(Canvas.LeftProperty),
                To = finalLeftValue
            };
            Storyboard.SetTarget(daLeft, wc);
            Storyboard.SetTargetProperty(daLeft, new PropertyPath("Left"));
            sb.Children.Add(daLeft);
            finalMoveWordAnimationData.Add((wc, Canvas.LeftProperty, finalLeftValue));

            double finalTopValue = wac.WordPosition.StartRow * UnitSize;
            var daTop = new DoubleAnimation
            {
                Duration = duration,
                From = (double)wc.GetValue(Canvas.TopProperty),
                To = finalTopValue
            };
            Storyboard.SetTarget(daTop, wc);
            Storyboard.SetTargetProperty(daTop, new PropertyPath("Top"));
            sb.Children.Add(daTop);
            finalMoveWordAnimationData.Add((wc, Canvas.TopProperty, finalTopValue));
        }
        IsMoveWordAnimationInProgress = true;
        sb.Completed += Sb_Completed;
        sb.Begin();
    }

    private void Sb_Completed(object sender, EventArgs e) => EndMoveWordAnimation();

    private bool IsMoveWordAnimationInProgress;
    private List<(Canvas, DependencyProperty, double)> finalMoveWordAnimationData;

    private void EndMoveWordAnimation()
    {
        IsMoveWordAnimationInProgress = false;
        foreach (var item in finalMoveWordAnimationData)
        {
            item.Item1.BeginAnimation(item.Item2, null);
            item.Item1.SetValue(item.Item2, item.Item3);
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
    private BoundingRectangle gridBounding;

    private void ClearBackgroundGrid()
    {
        BackgroundGrid.Children.Clear();
        gridBounding = new BoundingRectangle(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
    }

    private void UpdateBackgroundGrid()
    {
        //var matrix = MainMatrixTransform.Matrix;

        var bounds = viewModel.Layout.Bounds;
        // Add some extra margin and always represent a 20x20 grid at minimum
        var r = new BoundingRectangle(
            Math.Min(-10, bounds.Min.Row - 2),
            Math.Max(10, bounds.Max.Row + 3),
            Math.Min(-10, bounds.Min.Column - 2),
            Math.Max(10, bounds.Max.Column + 3));

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
}
