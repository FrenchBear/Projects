// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM View
// 2017-07-22   PV  First version

// ToDo: Let user change orientation of a word
// ToDo: Add a word or a group of words


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
using Bonza.Editor.ViewModel;
using Bonza.Editor.Support;
using Bonza.Generator;
using static Bonza.Editor.App;

namespace Bonza.Editor.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class BonzaView : Window
    {
        private readonly BonzaViewModel viewModel;
        private readonly Selection m_Sel;                     // Manages current selection, internal since it's accessed from ViewModel
        private List<WordAndCanvas> m_WordAndCanvasList;    // Current list of WordAndCanvas managed by view


        // --------------------------------------------------------------------
        // Constructor and Window events

        public BonzaView()
        {
            InitializeComponent();
            viewModel = new BonzaViewModel(this);
            DataContext = viewModel;

            m_WordAndCanvasList = new List<WordAndCanvas>();
            m_Sel = new Selection(viewModel);

            UpdateTransformationsFeedBack();

            // Can only reference ActualWidth after Window is loaded
            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;
            KeyDown += MainWindow_KeyDown;
            // ContentRendered += (sender, e) => Environment.Exit(0);       // For performance testing
        }

        public void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // For development tests
            viewModel.LoadWordsList(@"..\Lists\Fruits.txt");
            //viewModel.LoadWordsList(@"..\Lists\Countries.txt");       // For performance testing
        }

        // On window resize, background grid need update
        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBackgroundGrid();
        }


        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Actually we should terminate a move in progress, but since it's short, for now we can ignore it
                if (IsAnimationInProgress()) return;

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
        }



        // --------------------------------------------------------------------
        // Entry points for ViewModel

        // Clears the grid, before loading a new list of words or reorganizing layout
        internal void ClearLayout()
        {
            // Clear previous elements layout
            DrawingCanvas.Children.Clear();
            ClearBackgroundGrid();

            m_WordAndCanvasList = new List<WordAndCanvas>();
        }


        // Initial drawing of for current model layout
        internal void InitialLayoutDisplay()
        {
            // Draw letters
            foreach (WordPosition wp in viewModel.WordPositionList)
            {
                WordCanvas wc = new WordCanvas(wp);
                WordAndCanvas wac = new WordAndCanvas(wp, wc);
                DrawingCanvas.Children.Add(wc);
                m_WordAndCanvasList.Add(wac);
            }

            m_Sel.Clear();

            // After initial drawing, rescale and center without animations
            // Also draw initial background grid
            RescaleAndCenter(false);

            // Just as a safeguard
            moveWordAnimationInProgressCount = 0;
        }


        // Remove all the words from selection
        internal void DeleteSelection()
        {
            Debug.Assert(m_Sel.WordAndCanvasList != null && m_Sel.WordAndCanvasList.Count > 0);

            DeleteWordAndCanvasList(new List<WordAndCanvas>(m_Sel.WordAndCanvasList), true);
        }

        // More general Delete function, also used by Undo support
        internal void DeleteWordAndCanvasList(IList<WordAndCanvas> wordAndCanvasList, bool memorizeForUndo)
        {
            Debug.Assert(m_Sel.WordAndCanvasList != null && m_Sel.WordAndCanvasList.Count > 0);

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

            // Finally redraw grid if needed
            UpdateBackgroundGrid();
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

            // Finally redraw grid if needed
            UpdateBackgroundGrid();
        }

        public void SwapOrientation()
        {
            m_Sel.SwapOrientation();
        }



        // Adjust scale and origin to see the whole puzzle
        internal void RescaleAndCenter(bool isWithAnimation)
        {
            if (viewModel.Layout == null)
                return;

            (int minRow, int maxRow, int minColumn, int maxColumn) = viewModel.Layout.GetBounds();
            // Add some extra margin and always represent a 20x20 grid at minimum
            minRow = Math.Min(-11, minRow - 3);
            minColumn = Math.Min(-11, minColumn - 3);
            maxRow = Math.Max(11, maxRow + 4);
            maxColumn = Math.Max(11, maxColumn + 4);


            // Reverse-transform corners into WordCanvas coordinates
            Point p1Grid = new Point(minColumn * UnitSize, minRow * UnitSize);
            Point p2Grid = new Point(maxColumn * UnitSize, maxRow * UnitSize);


            rescaleMatrix = MainMatrixTransform.Matrix;

            // Set rotation to zero
            // get angle from transformation matrix:
            // | M11 M12 0 |   | s.cos θ -s.sin θ   0 |
            // | M21 M22 0 | = | s.sin θ  s.cos θ   0 |  (s = scale)
            // | dx  dy  1 |   | dx       dy        1 |
            double θ = Math.Atan2(rescaleMatrix.M21, rescaleMatrix.M11);    // Just to use a variable named θ
            rescaleMatrix.Rotate(θ / Math.PI * 180);            // It would certainly kill Microsoft to indicate on Rotate page or Intellisense tooltip that angle is in degrees...

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
                MatrixAnimation ma = new MatrixAnimation()
                {
                    From = MainMatrixTransform.Matrix,
                    To = rescaleMatrix,
                    Duration = new Duration(TimeSpan.FromSeconds(0.35))
                };
                ma.Completed += MatrixAnimationCompleted;
                isMatrixAnimationInProgress = true;
                MainMatrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, ma);
            }
            else
                MatrixAnimationEnd();
        }

        private bool isMatrixAnimationInProgress;
        private Matrix rescaleMatrix;

        // Event handler when MatrixAnimation is completed, need to "free" animated properties otherwise
        // they're "hold" by animation
        private void MatrixAnimationCompleted(object sender, EventArgs e)
        {
            MatrixAnimationEnd();
        }

        // Terminate transformation in a clean way, "freeing" animated properties
        private void MatrixAnimationEnd()
        {
            isMatrixAnimationInProgress = false;
            MainMatrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, null);

            // Final tasks
            MainMatrixTransform.Matrix = rescaleMatrix;
            UpdateTransformationsFeedBack();
            UpdateBackgroundGrid();
        }


        // --------------------------------------------------------------------
        // Mouse click and drag management

        Point previousMousePosition;

        // null indicates background grid move, or delegate must be executed by MouseMove to perform move 
        // action, P is current mouse coordinates in non-transformed user space
        Action<Point> pmm;

        private void MainGrid_MouseMoveWhenUp(object sender, MouseEventArgs e)
        {
            // ToDo, for example, provide word hovering visual feed-back
        }


        // Helper
        private bool IsAnimationInProgress()
        {
            // ToDo Maybe: terminate WordAnimation
            if (moveWordAnimationInProgressCount > 0) return true;
            if (isMatrixAnimationInProgress) MatrixAnimationEnd();
            return false;
        }



        // Helper, this was originally in MainGrid_MouseDown handler, but when a right-click occurs,
        // it is assumed than it will select automatically a non-selected word, so code got promoted to its own function...
        private void UpdateSelectionAfterClick(MouseButtonEventArgs e)
        {
            if (DrawingCanvas.InputHitTest(e.GetPosition(DrawingCanvas)) is TextBlock hitTextBlock)
            {
                WordCanvas hitC = (hitTextBlock.Parent) as WordCanvas;
                WordAndCanvas hit = m_WordAndCanvasList.FirstOrDefault(wac => ReferenceEquals(wac.WordCanvas, hitC));
                Debug.Assert(hit != null);

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
            // Ignore event if there are animations in progress, could actually force-terminate them, but they're quick anyway
            if (IsAnimationInProgress()) return;

            MainGrid.MouseMove -= MainGrid_MouseMoveWhenUp;
            MainGrid.MouseMove += MainGrid_MouseMoveWhenDown;
            previousMousePosition = e.GetPosition(MainGrid);
            UpdateSelectionAfterClick(e);

            // HitTest: Test if a word was clicked on, if true, hitTextBlock is a TextBloxk
            if (DrawingCanvas.InputHitTest(e.GetPosition(DrawingCanvas)) is TextBlock)
                // We'reinterested in its parent WordCanvas, that contains all the text blocks for the word
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

        // Separate from MainGrid_MouseDown to reduce complexity
        private Action<Point> GetMouseDownMoveAction()
        {
            // Need a layout without moved word to validate placement
            viewModel.BuildMoveTestLayout(m_Sel.WordAndCanvasList.Select(wac => wac.WordPosition));

            // Reverse-transform mouse Grid coordinates into DrawingCanvas coordinates
            Matrix m = MainMatrixTransform.Matrix;
            m.Invert();     // To convert from screen transformed coordinates into ideal grid
                            // coordinates starting at (0,0) with a square side of UnitSize
            List<Vector> clickOffsetList = new List<Vector>(m_Sel.WordAndCanvasList.Count);
            foreach (WordCanvas wc in m_Sel.WordAndCanvasList.Select(wac => wac.WordCanvas))
            {
                Point canvasTopLeft = new Point((double)wc.GetValue(Canvas.LeftProperty), (double)wc.GetValue(Canvas.TopProperty));
                // clickOffset memorizes the difference between (top,left) of WordCanvas and the clicked point
                // since when we move, we need that information to adjust WordCanvas position
                clickOffsetList.Add(canvasTopLeft - m.Transform(previousMousePosition));
            }

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

                    // ToDo: MoveTestLayout is not Ok if we remove all words from selection from TestLayout,
                    // some words of the selection will be considered incorrectly placed
                    // Need to find a solution to that!
                    // If we remove just selected word from TestLayout, word will collide with ghosts of
                    // other words being moved => Not Ok
                    // Maybe the test of validity can only be done if there is a real move: as long
                    // we remain at original position (after rounding), by definition we know it's Ok
                    // or the test may not be done on separate words, but just on a list of squares...
                    // Need more thinking...
                    // Other option, temp placement rules are more flexible and allow for temporary
                    // invalid placements, validity is checked later
                    // Note that during generation, current stringent rules must prevail

                    // Find out if it's possible to place the word here, provide color feed-back
                    if (viewModel.CanPlaceWordInMoveTestLayout(m_Sel.WordAndCanvasList[i].WordPosition, new PositionOrientation { StartRow = top, StartColumn = left, IsVertical = m_Sel.WordAndCanvasList[i].WordPosition.IsVertical }))
                        wc.SetColor(SelectedForegroundBrush, SelectedBackgroundBrush);
                    else
                        wc.SetColor(ProblemForegroundBrush, ProblemBackgroundBrush);
                }
            };
        }


        // Relay from Window_MouseDown handler when it's actually a right click
        private void MainGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            UpdateSelectionAfterClick(e);

            ContextMenu cm;
            if (DrawingCanvas.InputHitTest(e.GetPosition(DrawingCanvas)) is TextBlock)
                cm = FindResource("WordCanvasMenu") as ContextMenu;
            else
                cm = FindResource("BackgroundCanvasMenu") as ContextMenu;
            Debug.Assert(cm != null);
            cm.PlacementTarget = sender as UIElement;
            cm.IsOpen = true;
            e.Handled = true;
        }



        void MainGrid_MouseMoveWhenDown(object sender, MouseEventArgs e)
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
                UpdateTransformationsFeedBack();
                UpdateBackgroundGrid();
            }
            else
            {
                // Move selected word using generated lambda and capture on click down
                m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
                pmm(m.Transform(newPosition));
            }
        }

        // Update status bar
        private void UpdateTransformationsFeedBack()
        {
            Matrix matrix = MainMatrixTransform.Matrix;
            var a = Math.Atan2(matrix.M12, matrix.M11) / Math.PI * 180;
            RotationTextBlock.Text = $"{a:F1}°";
            var s = Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
            ScaleTextBlock.Text = $"{s:F2}";
            TranslationTextBlock.Text = $"X:{matrix.OffsetX:F2} Y:{matrix.OffsetY:F2}";
        }



        private void MainGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            MainGrid.MouseMove -= MainGrid_MouseMoveWhenDown;
            MainGrid.MouseMove += MainGrid_MouseMoveWhenUp;

            if (pmm != null)
            {
                // End of visual feed-back, align on grid, and update ViewModel
                // Not efficient to manage a single list of (top, left) tuple since in the snail pattern
                // placement code, top is updated independently from left, and a tuple makes it heavy
                List<PositionOrientation> topLeftList = new List<PositionOrientation>();
                foreach (WordAndCanvas wac in m_Sel.WordAndCanvasList)
                {
                    WordCanvas wc = wac.WordCanvas;
                    wc.SetColor(SelectedForegroundBrush, SelectedBackgroundBrush);

                    // Round position to closest square on the grid
                    int top = (int)Math.Floor(((double)wc.GetValue(Canvas.TopProperty) / UnitSize) + 0.5);
                    int left = (int)Math.Floor(((double)wc.GetValue(Canvas.LeftProperty) / UnitSize) + 0.5);
                    topLeftList.Add(new PositionOrientation { StartRow = top, StartColumn = left, IsVertical = wac.WordPosition.IsVertical });
                }

                // If position is not valid, look around until a valid position is found
                // Examine surrounding cells in a "snail pattern" 
                bool CanPlaceAllWords()
                {
                    for (int il = 0; il < m_Sel.WordAndCanvasList.Count; il++)
                        if (!viewModel.CanPlaceWordInMoveTestLayout(m_Sel.WordAndCanvasList[il].WordPosition, topLeftList[il]))
                            return false;
                    return true;
                }

                if (!CanPlaceAllWords())
                {
                    int st = 1;
                    int sign = 1;

                    for (;;)
                    {
                        for (int i = 0; i < st; i++)
                        {
                            for (int il = 0; il < m_Sel.WordAndCanvasList.Count; il++)
                                topLeftList[il] = new PositionOrientation { StartRow = topLeftList[il].StartRow, StartColumn = topLeftList[il].StartColumn + sign, IsVertical = m_Sel.WordAndCanvasList[il].WordPosition.IsVertical };
                            if (CanPlaceAllWords()) goto FoundValidPosition;
                        }
                        for (int i = 0; i < st; i++)
                        {
                            for (int il = 0; il < m_Sel.WordAndCanvasList.Count; il++)
                                topLeftList[il] = new PositionOrientation { StartRow = topLeftList[il].StartRow + sign, StartColumn = topLeftList[il].StartColumn, IsVertical = m_Sel.WordAndCanvasList[il].WordPosition.IsVertical };
                            if (CanPlaceAllWords()) goto FoundValidPosition;
                        }
                        sign = -sign;
                        st++;
                    }
                }

            FoundValidPosition:
                // Move to final, rounded position
                viewModel.UpdateWordPositionLocation(m_Sel.WordAndCanvasList, topLeftList, true);     // Update WordPosition with new location
                MoveWordAndCanvasList(m_Sel.WordAndCanvasList);
            }
        }


        int moveWordAnimationInProgressCount;

        internal void MoveWordAndCanvasList(IList<WordAndCanvas> wordAndCanvasList)
        {
            if (wordAndCanvasList == null) throw new ArgumentNullException(nameof(wordAndCanvasList));
            if (wordAndCanvasList.Count == 0) throw new ArgumentException(nameof(wordAndCanvasList));

            // If bounding rectangle is updated, need to redraw background grid
            (int minRow, int maxRow, int minColumn, int maxColumn) = viewModel.Layout.GetBounds();
            if (minRow != minRowGrid || minColumn != minColumnGrid || maxRow != maxRowGrid || maxColumn != maxColumnGrid)
                UpdateBackgroundGrid();

            // Compute distance moved on 1st element to choose speed
            WordPosition wp1 = wordAndCanvasList.First().WordPosition;
            WordCanvas wc1 = wordAndCanvasList.First().WordCanvas;
            double deltaX = (double)wc1.GetValue(Canvas.LeftProperty) - (wp1.StartColumn * UnitSize);
            double deltaY = (double)wc1.GetValue(Canvas.TopProperty) - (wp1.StartRow * UnitSize);
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            // Could optimize if there is no actual displacement, for instance, after a click down
            // and up without moving the mouse

            foreach (WordAndCanvas wac in wordAndCanvasList)
            {
                WordCanvas wc = wac.WordCanvas;
                WordPosition wp = wac.WordPosition;

                DoubleAnimation daLeft = new DoubleAnimation();
                double finalLeftValue = wp.StartColumn * UnitSize;
                daLeft.From = (double)wc.GetValue(Canvas.LeftProperty);
                daLeft.To = finalLeftValue;
                daLeft.Duration = new Duration(TimeSpan.FromSeconds(distance >= UnitSize ? 0.35 : 0.1));
                daLeft.Completed += (sender, e) => { MoveWordAnimationEnd(wc, Canvas.LeftProperty, finalLeftValue); };
                System.Threading.Interlocked.Increment(ref moveWordAnimationInProgressCount);
                wc.BeginAnimation(Canvas.LeftProperty, daLeft);

                DoubleAnimation daTop = new DoubleAnimation();
                double finalTopValue = wp.StartRow * UnitSize;
                daTop.From = (double)wc.GetValue(Canvas.TopProperty);
                daTop.To = finalTopValue;
                daTop.Duration = new Duration(TimeSpan.FromSeconds(distance >= UnitSize ? 0.35 : 0.1));
                daTop.Completed += (sender, e) => { MoveWordAnimationEnd(wc, Canvas.TopProperty, finalTopValue); };
                System.Threading.Interlocked.Increment(ref moveWordAnimationInProgressCount);
                wc.BeginAnimation(Canvas.TopProperty, daTop);
            }
        }

        private void MoveWordAnimationEnd(WordCanvas wc, DependencyProperty dp, double finalValue)
        {
            // Need to wait all animations end!
            System.Threading.Interlocked.Decrement(ref moveWordAnimationInProgressCount);
            wc.BeginAnimation(dp, null);

            // Set final value
            wc.SetValue(dp, finalValue);
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

            UpdateTransformationsFeedBack();
            UpdateBackgroundGrid();
        }


        // Grid currently drawn
        int minRowGrid, maxRowGrid, minColumnGrid, maxColumnGrid;

        private void ClearBackgroundGrid()
        {
            BackgroundGrid.Children.Clear();
            minRowGrid = int.MinValue;          // Force redraw after it's been cleared
        }

        private void UpdateBackgroundGrid()
        {
            var matrix = MainMatrixTransform.Matrix;

            // Update status bar
            var a = Math.Atan2(matrix.M12, matrix.M11) / Math.PI * 180;
            RotationTextBlock.Text = $"{a:F1}°";
            var s = Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
            ScaleTextBlock.Text = $"{s:F2}";

            ClearBackgroundGrid();
            if (viewModel.Layout != null)
            {
                (minRowGrid, maxRowGrid, minColumnGrid, maxColumnGrid) = viewModel.Layout.GetBounds();

                // Add some extra margin and always represent a 20x20 grid at minimum
                int minRow = Math.Min(-10, minRowGrid - 2);
                int minColumn = Math.Min(-10, minColumnGrid - 2);
                int maxRow = Math.Max(10, maxRowGrid + 3);
                int maxColumn = Math.Max(10, maxColumnGrid + 3);

                for (int row = minRow; row <= maxRow; row++)
                {
                    Line l = new Line()
                    {
                        X1 = minColumn * UnitSize,
                        X2 = maxColumn * UnitSize,
                        Y1 = row * UnitSize,
                        Y2 = row * UnitSize,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = row == 0 ? 3 : 1
                    };
                    BackgroundGrid.Children.Add(l);
                }

                for (int column = minColumn; column <= maxColumn; column++)
                {
                    Line l = new Line()
                    {
                        X1 = column * UnitSize,
                        X2 = column * UnitSize,
                        Y1 = minRow * UnitSize,
                        Y2 = maxRow * UnitSize,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = column == 0 ? 3 : 1
                    };
                    BackgroundGrid.Children.Add(l);
                }
            }
        }

    }
}
