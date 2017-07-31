// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM View
// 2017-07-22   PV  First version

// ToDo: Esc cancel a move operation, clean properly
// ToDo: Let user change orientation of a word
// ToDo: Delete selection command
// ToDo: Add a word or a group of words

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Bonza.Generator;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


namespace Bonza.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class BonzaView : Window
    {
        private readonly BonzaModel model;
        private readonly BonzaViewModel viewModel;

        // Size of the side of a square
        // Background grid is drawn using lines of width 1 (or 3 for origin axes) [hardcoded]
        // Font size is 16 and padding are also hardcoded
        private const double UnitSize = 25;

        internal readonly Selection sel;    // Manages current selection
        internal readonly Map map;     // Mapping WordPosition <--> WordCanvas


        // --------------------------------------------------------------------
        // Constructor and Window events

        public BonzaView()
        {
            InitializeComponent();
            model = new BonzaModel();
            viewModel = new BonzaViewModel(model, this);
            model.SetViewModel(viewModel);
            DataContext = viewModel;

            sel = new Selection(this, viewModel);
            map = new Map();

            UpdateTransformationsFeedBack();

            // Can only reference ActualWidth after Window is loaded
            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;
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

        // --------------------------------------------------------------------
        // Entry points for ViewModel

        // Clears the grid, before loading a new list of words or reorganizing layout
        internal void ClearLayout()
        {
            // Clear previous elements layout
            DrawingCanvas.Children.Clear();
            ClearBackgroundGrid();
        }


        // Initial drawing of for current model layout
        internal void InitialLayoutDisplay()
        {
            // Draw letters
            foreach (WordPosition wp in viewModel.WordPositionList)
            {
                // Group letters in a WordCanvas to be able later to move them at once
                WordCanvas wordCanvas = new WordCanvas(wp);
                map.Add(wp, wordCanvas);

                DrawingCanvas.Children.Add(wordCanvas);
            }

            sel.Clear();

            // After initial drawing, rescale and center without animations
            // Also draw initial background grid
            RescaleAndCenter(false);

            // Just as a safeguard
            moveWordAnimationInProgressCount = 0;
        }


        // Adjust scale and origin to see the whole puzzle
        internal void RescaleAndCenter(bool isWithAnimation)
        {
            if (model.Layout == null)
                return;

            (int minRow, int maxRow, int minColumn, int maxColumn) = model.Layout.GetBounds();
            // Add some extra margin
            minRow -= 2; minColumn -= 2;
            maxRow += 3; maxColumn += 3;

            // Reverse-transform corners into WordCanvas coordinates
            Point P1Grid = new Point(minColumn * UnitSize, minRow * UnitSize);
            Point P2Grid = new Point(maxColumn * UnitSize, maxRow * UnitSize);


            RescaleMatrix = MainMatrixTransform.Matrix;

            // Set rotation to zero
            // get angle from transformation matrix:
            // | M11 M12 0 |   | s.cos θ -s.sin θ   0 |
            // | M21 M22 0 | = | s.sin θ  s.cos θ   0 |  (s = scale)
            // | dx  dy  1 |   | dx       dy        1 |
            double θ = Math.Atan2(RescaleMatrix.M21, RescaleMatrix.M11);    // Just to use a variable named θ
            RescaleMatrix.Rotate(θ / Math.PI * 180);            // It would certainly kill Microsoft to indicate on Rotate page or Intellisense tooltip that angle is in degrees...

            // First adjust scale
            Point P1Screen = RescaleMatrix.Transform(P1Grid);
            Point P2Screen = RescaleMatrix.Transform(P2Grid);
            double rescaleFactorX = ClippingCanvas.ActualWidth / (P2Screen.X - P1Screen.X);
            double rescaleFactorY = ClippingCanvas.ActualHeight / (P2Screen.Y - P1Screen.Y);
            double rescaleFactor = Math.Min(rescaleFactorX, rescaleFactorY);
            RescaleMatrix.Scale(rescaleFactor, rescaleFactor);

            // Then adjust location and center
            P1Screen = RescaleMatrix.Transform(P1Grid);
            P2Screen = RescaleMatrix.Transform(P2Grid);
            double offX1 = -P1Screen.X;
            double offX2 = ClippingCanvas.ActualWidth - P2Screen.X;
            double offY1 = -P1Screen.Y;
            double offY2 = ClippingCanvas.ActualHeight - P2Screen.Y;
            RescaleMatrix.Translate((offX1 + offX2) / 2, (offY1 + offY2) / 2);

            if (isWithAnimation)
            {
                // Use an animation for a smooth transformation
                MatrixAnimation ma = new MatrixAnimation()
                {
                    From = MainMatrixTransform.Matrix,
                    To = RescaleMatrix,
                    Duration = new Duration(TimeSpan.FromSeconds(0.35))
                };
                ma.Completed += MatrixAnimationCompleted;
                isMatrixAnimationInProgress = true;
                MainMatrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, ma);
            }
            else
                MatrixAnimationEnd();
        }

        bool isMatrixAnimationInProgress;
        Matrix RescaleMatrix;

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
            MainMatrixTransform.Matrix = RescaleMatrix;
            UpdateTransformationsFeedBack();
            UpdateBackgroundGrid();
        }



        // --------------------------------------------------------------------
        // Color management

        // Some colors
        internal static readonly Brush NormalBackgroundBrush = Brushes.Black;
        internal static readonly Brush NormalForegroundBrush = Brushes.White;

        internal static readonly Brush SelectedBackgroundBrush = Brushes.DarkBlue;
        internal static readonly Brush SelectedForegroundBrush = Brushes.White;

        internal static readonly Brush ProblemBackgroundBrush = Brushes.DarkRed;
        internal static readonly Brush ProblemForegroundBrush = Brushes.White;



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
        private void UpdateSelectionAfterClick(object sender, MouseButtonEventArgs e)
        {
            if (DrawingCanvas.InputHitTest(e.GetPosition(DrawingCanvas)) is TextBlock hitTextBlock)
            {
                WordCanvas hitC = (hitTextBlock.Parent) as WordCanvas;
                WordPosition hitWP = map.GetWordPositionFromWordCanvas(hitC);
                Debug.Assert(hitWP != null);

                // If Ctrl key is NOT pressed, clear previous selection
                // But if we click again in something already selected, do not clear selection!
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                    if (sel.WordPositionList != null && !sel.WordPositionList.Select(wp => map.GetWordCanvasFromWordPosition(wp)).Contains(hitC))
                        sel.Clear();


                // Add current word to selection
                sel.Add(hitWP);

                // If Shift key is pressed, selection is extended to connected words
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    //model.Layout.GetConnectedWordPositions(hitWP).ForEach(connected => AddWordPositionToSelection(connected));
                    foreach (WordPosition connected in model.Layout.GetConnectedWordPositions(hitWP))
                        sel.Add(connected);

                // Remove and add again elements to move so they're displayed above non-moved elements
                foreach (WordCanvas wc in sel.WordPositionList.Select(wp => map.GetWordCanvasFromWordPosition(wp)))
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

            UpdateSelectionAfterClick(sender, e);

            // HitTest: Test if a word was clicked on, if true, hitTextBlock is a TextBloxk
            if (DrawingCanvas.InputHitTest(e.GetPosition(DrawingCanvas)) is TextBlock hitTextBlock)
                // We'reinterested in its parent WordCanvas, that contains all the text blocks for the word
                pmm = GetMouseDownMoveAction();
            else
            {
                pmm = null;
                sel.Clear();
            }

            // Be sure to call GetPosition before Capture, otherwise GetPosition returns 0 after Capture
            // Capture to get MouseUp event raised by grid
            Mouse.Capture(MainGrid);
        }

        // Separate from MainGrid_MouseDown to reduce complexity
        private Action<Point> GetMouseDownMoveAction()
        {
            // Need a layout without moved word to validate placement
            model.BuildMoveTestLayout(sel.WordPositionList);

            // Reverse-transform mouse Grid coordinates into DrawingCanvas coordinates
            Matrix m = MainMatrixTransform.Matrix;
            m.Invert();     // To convert from screen transformed coordinates into ideal grid
                            // coordinates starting at (0,0) with a square side of UnitSize
            List<Vector> clickOffsetList = new List<Vector>(sel.WordPositionList.Count);
            foreach (WordCanvas wc in sel.WordPositionList.Select(wp => map.GetWordCanvasFromWordPosition(wp)))
            {
                Point canvasTopLeft = new Point((double)wc.GetValue(Canvas.LeftProperty), (double)wc.GetValue(Canvas.TopProperty));
                // clickOffset memorizes the difference between (top,left) of WordCanvas and the clicked point
                // since when we move, we need that information to adjust WordCanvas position
                clickOffsetList.Add(canvasTopLeft - m.Transform(previousMousePosition));
            }

            // When moving, P is current mouse in ideal grid coordinates
            return P =>
            {
                // Just move selected WordCanvas
                for (int i = 0; i < sel.WordPositionList.Count; i++)
                {
                    double preciseTop = P.Y + clickOffsetList[i].Y;
                    double preciseLeft = P.X + clickOffsetList[i].X;

                    WordCanvas wc = map.GetWordCanvasFromWordPosition(sel.WordPositionList[i]);
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
                    if (model.CanPlaceWordInMoveTestLayout(sel.WordPositionList[i], new PositionOrientation { StartRow = top, StartColumn = left, IsVertical = sel.WordPositionList[i].IsVertical }))
                        wc.SetColor(SelectedForegroundBrush, SelectedBackgroundBrush);
                    else
                        wc.SetColor(ProblemForegroundBrush, ProblemBackgroundBrush);
                }
            };
        }


        // Relay from Window_MouseDown handler when it's actually a right click
        private void MainGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            UpdateSelectionAfterClick(sender, e);

            ContextMenu cm = null;
            if (DrawingCanvas.InputHitTest(e.GetPosition(DrawingCanvas)) is TextBlock hitTextBlock)
                cm = this.FindResource("WordCanvasMenu") as ContextMenu;
            else
                cm = this.FindResource("BackgroundCanvasMenu") as ContextMenu;

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
            RotationTextBlock.Text = string.Format("{0:F1}°", a);
            var s = Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
            ScaleTextBlock.Text = string.Format("{0:F2}", s);
            TranslationTextBlock.Text = string.Format("X:{0:F2} Y:{1:F2}", matrix.OffsetX, matrix.OffsetY);
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
                for (int il = 0; il < sel.WordPositionList.Count; il++)
                {
                    WordCanvas wc = map.GetWordCanvasFromWordPosition(sel.WordPositionList[il]);
                    wc.SetColor(SelectedForegroundBrush, SelectedBackgroundBrush);

                    // Round position to closest square on the grid
                    int top = (int)Math.Floor(((double)wc.GetValue(Canvas.TopProperty) / UnitSize) + 0.5);
                    int left = (int)Math.Floor(((double)wc.GetValue(Canvas.LeftProperty) / UnitSize) + 0.5);
                    topLeftList.Add(new PositionOrientation { StartRow = top, StartColumn = left, IsVertical = sel.WordPositionList[il].IsVertical });
                }

                // If position is not valid, look around until a valid position is found
                // Examine surrounding cells in a "snail pattern" 
                bool CanPlaceAllWords()
                {
                    for (int il = 0; il < sel.WordPositionList.Count; il++)
                        if (!model.CanPlaceWordInMoveTestLayout(sel.WordPositionList[il], topLeftList[il]))
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
                            for (int il = 0; il < sel.WordPositionList.Count; il++)
                                topLeftList[il] = new PositionOrientation { StartRow = topLeftList[il].StartRow, StartColumn = topLeftList[il].StartColumn + sign, IsVertical = sel.WordPositionList[il].IsVertical };
                            if (CanPlaceAllWords()) goto FoundValidPosition;
                        }
                        for (int i = 0; i < st; i++)
                        {
                            for (int il = 0; il < sel.WordPositionList.Count; il++)
                                topLeftList[il] = new PositionOrientation { StartRow = topLeftList[il].StartRow + sign, StartColumn = topLeftList[il].StartColumn, IsVertical = sel.WordPositionList[il].IsVertical };
                            if (CanPlaceAllWords()) goto FoundValidPosition;
                        }
                        sign = -sign;
                        st++;
                    }
                }

                FoundValidPosition:
                // Move to final, rounded position
                viewModel.UpdateWordPositionLocation(sel.WordPositionList, topLeftList, true);     // Update WordPosition with new location
                MoveWordPositionList(sel.WordPositionList);
            }
        }

        int moveWordAnimationInProgressCount;

        public void MoveWordPositionList(IEnumerable<WordPosition> wordPositionList)
        {
            if (wordPositionList == null) throw new ArgumentNullException(nameof(wordPositionList));

            // If bounding rectangle is updated, need to redraw background grid
            (int minRow, int maxRow, int minColumn, int maxColumn) = model.Layout.GetBounds();
            if (minRow != minRowGrid || minColumn != minColumnGrid || maxRow != maxRowGrid || maxColumn != maxColumnGrid)
                UpdateBackgroundGrid();

            // Compute distance moved on 1st element to choose speed
            WordPosition wp1 = wordPositionList.FirstOrDefault();
            Debug.Assert(wp1 != null);
            WordCanvas wc1 = map.GetWordCanvasFromWordPosition(wp1);
            double deltaX = (double)wc1.GetValue(Canvas.LeftProperty) - (wp1.StartColumn * UnitSize);
            double deltaY = (double)wc1.GetValue(Canvas.TopProperty) - (wp1.StartRow * UnitSize);
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            // Could optimize if there is no actual displacement, for instance, after a click down
            // and up without moving the mouse

            foreach (WordPosition wp in wordPositionList)
            {
                WordCanvas wc = map.GetWordCanvasFromWordPosition(wp);

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
            if (System.Windows.Input.Keyboard.IsKeyDown(Key.LeftCtrl))
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
            RotationTextBlock.Text = string.Format("{0:F1}°", a);
            var s = Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
            ScaleTextBlock.Text = string.Format("{0:F2}", s);

            ClearBackgroundGrid();
            if (model.Layout != null)
            {
                (minRowGrid, maxRowGrid, minColumnGrid, maxColumnGrid) = model.Layout.GetBounds();
                // Add some extra margin
                int minRow = minRowGrid - 2;
                int minColumn = minColumnGrid - 2;
                int maxRow = maxRowGrid + 3;
                int maxColumn = maxColumnGrid + 3;

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
