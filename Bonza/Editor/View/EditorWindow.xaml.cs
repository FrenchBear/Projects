// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM View
// 2017-07-22   PV  First version

// ToDo: Esc cancel a move operation, clean properly
// ToDo: let user change orientation of a word

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
    public partial class EditorWindow : Window
    {
        private BonzaModel model;
        private BonzaViewModel viewModel;

        // Size of the side of a square
        // Background grid is drawn using lines of width 1 (or 3 for origin axes) [hardcoded]
        // Font size is 16 and padding are also hardcoded
        private const double UnitSize = 25;

        public EditorWindow()
        {
            InitializeComponent();
            model = new BonzaModel();
            viewModel = new BonzaViewModel(model, this);
            model.SetViewModel(viewModel);
            DataContext = viewModel;
            UpdateTransformationsFeedBack();

            // Can only reference ActualWidth after Window is loaded
            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;
            // ContentRendered += (sender, e) => Environment.Exit(0);       // For performance testing
        }

        // HitTest selects a canvas, this dictionary maps it to associated WordPosition
        Dictionary<Canvas, WordPosition> CanvasToWordPosition = new Dictionary<Canvas, WordPosition>();

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


        // Clears the grid, before loading a new list of words or reorganizing layout
        internal void ClearLayout()
        {
            // Clear previous elements layout
            DrawingCanvas.Children.Clear();
            ClearBackgroundGrid();
        }


        // Initial drawing of canvas for current model layout
        internal void InitialLayoutDisplay()
        {
            // Draw letters
            var arial = new FontFamily("Arial");
            foreach (WordPosition wp in viewModel.WordPositionList)
            {
                // Group letters in a canvas to be able later to move them at once
                var wordCanvas = new Canvas();
                CanvasToWordPosition.Add(wordCanvas, wp);

                for (int i = 0; i < wp.Word.Length; i++)
                {
                    TextBlock tb = new TextBlock()
                    {
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = wp.Word.Substring(i, 1),
                        FontFamily = arial,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Padding = new Thickness(0, 3, 0, 0)
                    };
                    double top, left, width, height;
                    top = wp.IsVertical ? UnitSize * i : 0;
                    left = wp.IsVertical ? 0 : UnitSize * i;
                    width = UnitSize;
                    height = UnitSize;

                    tb.SetValue(Canvas.LeftProperty, left);
                    tb.SetValue(Canvas.TopProperty, top);
                    tb.Width = width;
                    tb.Height = height;

                    wordCanvas.Children.Add(tb);
                }

                wordCanvas.SetValue(Canvas.LeftProperty, UnitSize * wp.StartColumn);
                wordCanvas.SetValue(Canvas.TopProperty, UnitSize * wp.StartRow);
                SetWordCanvasColor(wordCanvas, Brushes.White, Brushes.Black);
                DrawingCanvas.Children.Add(wordCanvas);
            }

            ClearSelection();

            // After initial drawing, rescale and center without animations
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

            // Reverse-transform corners into Canvas coordinates
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
                // ToDo: Stop animation if user interacts with canvas
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


        // Some colors
        private static readonly Brush NormalBackgroundBrush = Brushes.Black;
        private static readonly Brush NormalForegroundBrush = Brushes.White;

        private static readonly Brush SelectedBackgroundBrush = Brushes.DarkBlue;
        private static readonly Brush SelectedForegroundBrush = Brushes.White;

        private static readonly Brush ProblemBackgroundBrush = Brushes.DarkRed;
        private static readonly Brush ProblemForegroundBrush = Brushes.White;


        // Current selection
        List<Canvas> selectedCanvasList;
        List<WordPosition> selectedWordPositionList;

        private void ClearSelection()
        {
            if (selectedCanvasList == null)
                return;

            if (selectedCanvasList != null && selectedCanvasList.Count > 0)
                foreach (var c in selectedCanvasList)
                    SetWordCanvasColor(c, NormalForegroundBrush, NormalBackgroundBrush);

            // Optimization, by convention null means no selection
            selectedCanvasList = null;
            selectedWordPositionList = null;
        }

        // Helper
        private void AddWordPositionToSelection(WordPosition wp)
        {
            if (selectedCanvasList == null)
            {
                selectedCanvasList = new List<Canvas>();
                selectedWordPositionList = new List<WordPosition>();
            }
            if (!selectedWordPositionList.Contains(wp))
            {
                selectedWordPositionList.Add(wp);
                Canvas c = GetCanvasFromWordPosition(wp);
                selectedCanvasList.Add(c);
                SetWordCanvasColor(c, SelectedForegroundBrush, SelectedBackgroundBrush);
            }
        }

        // Helper to set foreground/background on all TextBlock of a wordCanvas 
        static void SetWordCanvasColor(Canvas wordCanvas, Brush foreground, Brush background)
        {
            foreach (TextBlock tb in wordCanvas.Children)
            {
                tb.Foreground = foreground;
                tb.Background = background;
            }
        }


        // Helper, mapping Canvas -> WordPosition or null
        private WordPosition GetWordPositionFromCanvas(Canvas c)
        {
            return CanvasToWordPosition.TryGetValue(c, out WordPosition wp) ? wp : null;
        }

        // Helper, mapping WordPosition -> Canvas or null
        private Canvas GetCanvasFromWordPosition(WordPosition wp)
        {
            foreach (var kv in CanvasToWordPosition)
                if (kv.Value == wp)
                    return kv.Key;
            return null;
        }



        // Mouse click and drag management
        Point previousMousePosition;
        // null indicates canvas move, or delegate must be executed by MouseMove to perform move 
        // action, P is current mouse coordinates in non-transformed user space
        Action<Point> pmm;

        private void MainGrid_MouseMoveWhenUp(object sender, MouseEventArgs e)
        {
            // ToDo, for example, provide word hovering visual feed-back
        }


        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (moveWordAnimationInProgressCount > 0)
                return;
            if (isMatrixAnimationInProgress)
                MatrixAnimationEnd();

            MainGrid.MouseMove -= MainGrid_MouseMoveWhenUp;
            MainGrid.MouseMove += MainGrid_MouseMoveWhenDown;
            previousMousePosition = e.GetPosition(MainGrid);

            // HitTest: Test if a word was clicked on, if true, hitTextBlock is a TextBloxk
            if (DrawingCanvas.InputHitTest(e.GetPosition(DrawingCanvas)) is TextBlock hitTextBlock)
                // We want to move its parent Canvas, that contains all the text blocks for the word
                pmm = GetMouseDownMoveAction((hitTextBlock.Parent) as Canvas);
            else
            {
                pmm = null;
                ClearSelection();
            }

            // Be sure to call GetPosition before Capture, otherwise GetPosition returns 0 after Capture
            // Capture to get MouseUp event raised by grid
            Mouse.Capture(MainGrid);
        }

        // Separate from MainGrid_MouseDown to reduce complexity
        private Action<Point> GetMouseDownMoveAction(Canvas hitC)
        {
            WordPosition hitWP = GetWordPositionFromCanvas(hitC);
            Debug.Assert(hitWP != null);

            // If Ctrl key is NOT pressed, clear previous selection
            // But if we click again in something already selected, do not clear selection!
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                if (selectedCanvasList != null && !selectedCanvasList.Contains(hitC))
                    ClearSelection();


            // Add current word to selection
            AddWordPositionToSelection(hitWP);

            // If Shift key is pressed, selection is extended to connected words
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                //model.Layout.GetConnectedWordPositions(hitWP).ForEach(connected => AddWordPositionToSelection(connected));
                foreach (WordPosition connected in model.Layout.GetConnectedWordPositions(hitWP))
                    AddWordPositionToSelection(connected);

            // Remove and add again elements to move so they're displayed above non-moved elements
            foreach (var c in selectedCanvasList)
            {
                DrawingCanvas.Children.Remove(c);
                DrawingCanvas.Children.Add(c);
                SetWordCanvasColor(c, Brushes.White, Brushes.DarkBlue);
            }

            // Need a layout without moved word to validate placement
            model.BuildMoveTestLayout(selectedWordPositionList);

            // Reverse-transform mouse Grid coordinates into Canvas coordinates
            Matrix m = MainMatrixTransform.Matrix;
            m.Invert();     // To convert from screen transformed coordinates into ideal grid
                            // coordinates starting at (0,0) with a square side of UnitSize
            List<Vector> clickOffsetList = new List<Vector>(selectedWordPositionList.Count);
            for (int i = 0; i < selectedCanvasList.Count; i++)
            {
                // Point constructor is (x, y), that is (left, top), beware it's opposite to conventions of this app
                Point canvasTopLeft = new Point((double)selectedCanvasList[i].GetValue(Canvas.LeftProperty), (double)selectedCanvasList[i].GetValue(Canvas.TopProperty));
                // clickOffset memorizes the difference between (top,left) of word canvas and the clicked point
                // since when we move, we need that information to adjust word canvas position
                clickOffsetList.Add(canvasTopLeft - m.Transform(previousMousePosition));
            }

            // When moving, P is current mouse in ideal grid coordinates
            return P =>
            {
                // Just move canvas
                for (int i = 0; i < selectedCanvasList.Count; i++)
                {
                    selectedCanvasList[i].SetValue(Canvas.TopProperty, P.Y + clickOffsetList[i].Y);
                    selectedCanvasList[i].SetValue(Canvas.LeftProperty, P.X + clickOffsetList[i].X);

                    // Round position to closest square on the grid
                    int top = (int)Math.Floor(((double)selectedCanvasList[i].GetValue(Canvas.TopProperty) / UnitSize) + 0.5);
                    int left = (int)Math.Floor(((double)selectedCanvasList[i].GetValue(Canvas.LeftProperty) / UnitSize) + 0.5);

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
                    if (model.CanPlaceWordInMoveTestLayout(selectedWordPositionList[i], (top, left)))
                        SetWordCanvasColor(selectedCanvasList[i], SelectedForegroundBrush, SelectedBackgroundBrush);
                    else
                        SetWordCanvasColor(selectedCanvasList[i], ProblemForegroundBrush, ProblemBackgroundBrush);
                }
            };
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
                List<(int Top, int Left)> topLeftList = new List<(int, int)>();
                foreach (Canvas hitCanvas in selectedCanvasList)
                {
                    SetWordCanvasColor(hitCanvas, SelectedForegroundBrush, SelectedBackgroundBrush);

                    // Round position to closest square on the grid
                    int top = (int)Math.Floor(((double)hitCanvas.GetValue(Canvas.TopProperty) / UnitSize) + 0.5);
                    int left = (int)Math.Floor(((double)hitCanvas.GetValue(Canvas.LeftProperty) / UnitSize) + 0.5);
                    topLeftList.Add((top, left));
                }

                // If position is not valid, look around until a valid position is found
                // Examine surrounding cells in a "snail pattern" 
                bool CanPlaceAllWords()
                {
                    for (int il = 0; il < selectedWordPositionList.Count; il++)
                        if (!model.CanPlaceWordInMoveTestLayout(selectedWordPositionList[il], topLeftList[il]))
                            return false;
                    return true;
                }

                if (!CanPlaceAllWords())
                {
                    int st = 1;
                    int sign = 1;

                    for (; ; )
                    {
                        for (int i = 0; i < st; i++)
                        {
                            for (int il = 0; il < selectedWordPositionList.Count; il++)
                                topLeftList[il] = (topLeftList[il].Top, topLeftList[il].Left + sign);
                            if (CanPlaceAllWords()) goto FoundValidPosition;
                        }
                        for (int i = 0; i < st; i++)
                        {
                            for (int il = 0; il < selectedWordPositionList.Count; il++)
                                topLeftList[il] = (topLeftList[il].Top + sign, topLeftList[il].Left);
                            if (CanPlaceAllWords()) goto FoundValidPosition;
                        }
                        sign = -sign;
                        st++;
                    }
                }

                FoundValidPosition:
                // Move to final, rounded position
                viewModel.UpdateWordPositionLocation(selectedWordPositionList, topLeftList, true);     // Update WordPosition with new location
                MoveWordPositionList(selectedWordPositionList);
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
            Canvas c1 = GetCanvasFromWordPosition(wp1);
            double deltaX = (double)c1.GetValue(Canvas.LeftProperty) - (wp1.StartColumn * UnitSize);
            double deltaY = (double)c1.GetValue(Canvas.TopProperty) - (wp1.StartRow * UnitSize);
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            // Could optimize if there is no actual displacement, for instance, after a click down
            // and up without moving the mouse

            foreach (WordPosition wp in wordPositionList)
            {
                Canvas c = GetCanvasFromWordPosition(wp);

                DoubleAnimation daLeft = new DoubleAnimation();
                double finalLeftValue = wp.StartColumn * UnitSize;
                daLeft.From = (double)c.GetValue(Canvas.LeftProperty);
                daLeft.To = finalLeftValue;
                daLeft.Duration = new Duration(TimeSpan.FromSeconds(distance >= UnitSize ? 0.35 : 0.1));
                daLeft.Completed += (sender, e) => { MoveWordAnimationEnd(c, Canvas.LeftProperty, finalLeftValue); };
                System.Threading.Interlocked.Increment(ref moveWordAnimationInProgressCount);
                c.BeginAnimation(Canvas.LeftProperty, daLeft);

                DoubleAnimation daTop = new DoubleAnimation();
                double finalTopValue = wp.StartRow * UnitSize;
                daTop.From = (double)c.GetValue(Canvas.TopProperty);
                daTop.To = finalTopValue;
                daTop.Duration = new Duration(TimeSpan.FromSeconds(distance >= UnitSize ? 0.35 : 0.1));
                daTop.Completed += (sender, e) => { MoveWordAnimationEnd(c, Canvas.TopProperty, finalTopValue); };
                System.Threading.Interlocked.Increment(ref moveWordAnimationInProgressCount);
                c.BeginAnimation(Canvas.TopProperty, daTop);
            }
        }

        private void MoveWordAnimationEnd(Canvas c, DependencyProperty dp, double finalValue)
        {
            // Need to wait all animations end!
            System.Threading.Interlocked.Decrement(ref moveWordAnimationInProgressCount);
            c.BeginAnimation(dp, null);

            // Set final value
            c.SetValue(dp, finalValue);
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
