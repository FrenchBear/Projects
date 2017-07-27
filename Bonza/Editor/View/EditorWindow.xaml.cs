// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM View
// 2017-07-22   PV  First version


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
        }



        // Initial drawing of canvas for current model layout
        internal void NewLayout()
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


        // Helper to set foreground/background on all TextBlock of a wordCanvas 
        static void SetWordCanvasColor(Canvas wordCanvas, Brush foreground, Brush background)
        {
            foreach (TextBlock tb in wordCanvas.Children)
            {
                tb.Foreground = foreground;
                tb.Background = background;
            }
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


        // Word or a group of connected words being moved
        List<Canvas> hitCanvasList;
        List<WordPosition> hitWordPositionList;

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

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (moveWordAnimationInProgressCount > 0)
                return;
            if (isMatrixAnimationInProgress)
                MatrixAnimationEnd();

            MainGrid.MouseMove -= MainGrid_MouseMoveWhenUp;
            MainGrid.MouseMove += MainGrid_MouseMoveWhenDown;
            previousMousePosition = e.GetPosition(MainGrid);

            pmm = null;
            // HitTest: Test if a word was clicked on, if true, hitTextBlock is a TextBloxk
            if (DrawingCanvas.InputHitTest(e.GetPosition(DrawingCanvas)) is TextBlock hitTextBlock)
            {
                // We want to move its parent Canvas, that contains all the text blocks for the word
                Canvas hitC = (hitTextBlock.Parent) as Canvas;
                pmm = GetMouseDownMoveAction(hitC);
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
            hitCanvasList = new List<Canvas> { hitC };
            hitWordPositionList = new List<WordPosition> { hitWP };

            // If Control key is pressed, selection to move is extended to connected words
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                foreach (WordPosition connected in model.Layout.GetConnectedWordPositions(hitWP))
                {
                    hitWordPositionList.Add(connected);
                    hitCanvasList.Add(GetCanvasFromWordPosition(connected));
                }

            // Remove and add again elements to move so they're displayed above non-moved elements
            foreach (var hitCanvas in hitCanvasList)
            {
                DrawingCanvas.Children.Remove(hitCanvas);
                DrawingCanvas.Children.Add(hitCanvas);
                SetWordCanvasColor(hitCanvas, Brushes.White, Brushes.DarkBlue);
            }

            // Need a layout without moved word to validate placement
            model.BuildMoveTestLayout(hitWordPositionList);

            // Reverse-transform mouse Grid coordinates into Canvas coordinates
            Matrix m = MainMatrixTransform.Matrix;
            m.Invert();     // To convert from screen transformed coordinates into ideal grid
                            // coordinates starting at (0,0) with a square side of UnitSize
            List<Vector> clickOffsetList = new List<Vector>(hitWordPositionList.Count);
            for (int i = 0; i < hitCanvasList.Count; i++)
            {
                Point canvasTopLeft = new Point((double)hitCanvasList[i].GetValue(Canvas.LeftProperty), (double)hitCanvasList[i].GetValue(Canvas.TopProperty));
                // clickOffset memorizes the difference between (top,left) of word canvas and the clicked point
                // since when we move, we need that information to adjust word canvas position
                clickOffsetList.Add(canvasTopLeft - m.Transform(previousMousePosition));
            }

            // When moving, P is current mouse in ideal grid coordinates
            return P =>
            {
                // Just move canvas
                for (int i = 0; i < hitCanvasList.Count; i++)
                {
                    hitCanvasList[i].SetValue(Canvas.TopProperty, P.Y + clickOffsetList[i].Y);
                    hitCanvasList[i].SetValue(Canvas.LeftProperty, P.X + clickOffsetList[i].X);

                    // Round position to closest square on the grid
                    int left = (int)Math.Floor(((double)hitCanvasList[i].GetValue(Canvas.LeftProperty) / UnitSize) + 0.5);
                    int top = (int)Math.Floor(((double)hitCanvasList[i].GetValue(Canvas.TopProperty) / UnitSize) + 0.5);

                    // Find out if it's possible to place the word here, provide color feed-back
                    if (model.CanPlaceWordInMoveTestLayout(hitWordPositionList[i], (left, top)))
                        SetWordCanvasColor(hitCanvasList[i], Brushes.White, Brushes.DarkBlue);
                    else
                        SetWordCanvasColor(hitCanvasList[i], Brushes.White, Brushes.DarkRed);
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
                List<(int Left, int Top)> leftTopList = new List<(int, int)>();
                foreach (Canvas hitCanvas in hitCanvasList)
                {
                    SetWordCanvasColor(hitCanvas, Brushes.White, Brushes.Black);

                    // Round position to closest square on the grid
                    int left = (int)Math.Floor(((double)hitCanvas.GetValue(Canvas.LeftProperty) / UnitSize) + 0.5);
                    int top = (int)Math.Floor(((double)hitCanvas.GetValue(Canvas.TopProperty) / UnitSize) + 0.5);
                    leftTopList.Add((left, top));
                }

                // If position is not valid, look around until a valid position is found
                // Examine surrounding cells in a "snail pattern" 
                bool CanPlaceAllWords()
                {
                    for (int il = 0; il < hitWordPositionList.Count; il++)
                        if (!model.CanPlaceWordInMoveTestLayout(hitWordPositionList[il], leftTopList[il]))
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
                            for (int il = 0; il < hitWordPositionList.Count; il++)
                                leftTopList[il] = (leftTopList[il].Left + sign, leftTopList[il].Top);
                            if (CanPlaceAllWords()) goto FoundValidPosition;
                        }
                        for (int i = 0; i < st; i++)
                        {
                            for (int il = 0; il < hitWordPositionList.Count; il++)
                                leftTopList[il] = (leftTopList[il].Left, leftTopList[il].Top + sign);
                            if (CanPlaceAllWords()) goto FoundValidPosition;
                        }
                        sign = -sign;
                        st++;
                    }
                }

                FoundValidPosition:
                // Move to final, rounded position
                viewModel.UpdateWordPositionLocation(hitWordPositionList, leftTopList, true);     // Update WordPosition with new location
                MoveWordPositionList(hitWordPositionList);
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

        private void UpdateBackgroundGrid()
        {
            var matrix = MainMatrixTransform.Matrix;

            // Update status bar
            var a = Math.Atan2(matrix.M12, matrix.M11) / Math.PI * 180;
            RotationTextBlock.Text = string.Format("{0:F1}°", a);
            var s = Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
            ScaleTextBlock.Text = string.Format("{0:F2}", s);

            BackgroundGrid.Children.Clear();
            minRowGrid = int.MinValue;          // Force redraw after it's been cleared
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
                    Line l = new Line();
                    l.X1 = minColumn * UnitSize;
                    l.X2 = maxColumn * UnitSize;
                    l.Y1 = row * UnitSize;
                    l.Y2 = row * UnitSize;
                    l.Stroke = Brushes.LightGray;
                    l.StrokeThickness = row == 0 ? 3 : 1;
                    BackgroundGrid.Children.Add(l);
                }

                for (int column = minColumn; column <= maxColumn; column++)
                {
                    Line l = new Line();
                    l.X1 = column * UnitSize;
                    l.X2 = column * UnitSize;
                    l.Y1 = minRow * UnitSize;
                    l.Y2 = maxRow * UnitSize;
                    l.Stroke = Brushes.LightGray;
                    l.StrokeThickness = column == 0 ? 3 : 1;
                    BackgroundGrid.Children.Add(l);
                }
            }
        }

    }
}
