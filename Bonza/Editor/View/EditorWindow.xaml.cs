// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM View
// 2017-07-22   PV  First version


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Bonza.Generator;
using System.Windows.Media.Animation;

namespace Bonza.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        private BonzaModel model;
        private BonzaViewModel viewModel;

        private static readonly double UnitSize = 25;

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
        }

        // HitTest selects a canvas, this dictionary maps it to associated WordPosition
        Dictionary<Canvas, WordPosition> CanvasToWordPosition = new Dictionary<Canvas, WordPosition>();

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            // Draw bounding Rectangle
            foreach (WordPosition wp in puzzle.Layout)
            {
                Rectangle r = new Rectangle();
                if (wp.IsVertical)
                {
                    r.Width = UnitSize;
                    r.Height = wp.Word.Length * UnitSize;
                }
                else
                {
                    r.Width = wp.Word.Length * UnitSize;
                    r.Height = UnitSize;
                }

                r.SetValue(Canvas.LeftProperty, UnitSize * wp.StartColumn);
                r.SetValue(Canvas.TopProperty, UnitSize * wp.StartRow);
                r.Stroke = Brushes.Blue;
                r.StrokeThickness = 1;

                DrawingCanvas.Children.Add(r);
            }
            */

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

            RescaleAndCenter();
        }

        // Adjust scale and origin to see the whole puzzle
        internal void RescaleAndCenter()
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
            // | M11 M12 0 |   | cos θ  -sin θ   0 |
            // | M21 M22 0 | = | sin θ   cos θ   0 |
            // | dx  dy  1 |   | dx      dy      1 |
            double θ = Math.Atan2(RescaleMatrix.M21, RescaleMatrix.M11);    // Just to use a variable named θ
            RescaleMatrix.Rotate(θ / Math.PI * 180);            // It would certainly kill Microsoft to indicate on Rotate page or tooltip that angle is in degrees...

            // First adjust scale
            Point P1Screen = RescaleMatrix.Transform(P1Grid);
            Point P2Screen = RescaleMatrix.Transform(P2Grid);
            double scaleX = ClippingCanvas.ActualWidth / (P2Screen.X - P1Screen.X);
            double scaleY = ClippingCanvas.ActualHeight / (P2Screen.Y - P1Screen.Y);
            double scale = Math.Min(scaleX, scaleY);
            if (scale > 5) scale = 5;
            RescaleMatrix.Scale(scale, scale);

            // Then adjust location and center
            P1Screen = RescaleMatrix.Transform(P1Grid);
            P2Screen = RescaleMatrix.Transform(P2Grid);
            double offX1 = -P1Screen.X;
            double offX2 = ClippingCanvas.ActualWidth - P2Screen.X;
            double offY1 = -P1Screen.Y;
            double offY2 = ClippingCanvas.ActualHeight - P2Screen.Y;
            RescaleMatrix.Translate((offX1 + offX2) / 2, (offY1 + offY2) / 2);


            // Use an animation for a smooth transformation
            // ToDo: Stop animation if user interacts with canvas
            MatrixAnimation ma = new MatrixAnimation(RescaleMatrix, new Duration(TimeSpan.FromSeconds(0.35)));
            ma.From = MainMatrixTransform.Matrix;
            ma.Completed += sb_Completed;
            MainMatrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, ma);
        }

        Matrix RescaleMatrix;

        // Terminate transformation in a clean way, "freeing" property
        private void sb_Completed(object sender, EventArgs e)
        {
            MainMatrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, null);

            // Final tasks
            MainMatrixTransform.Matrix = RescaleMatrix;
            UpdateTransformationsFeedBack();
            UpdateBackgroundGrid();
        }

        // Helper to set foreground/background on all TextBlock of a wordCanvas 
        private void SetWordCanvasColor(Canvas wordCanvas, Brush foreground, Brush background)
        {
            foreach (TextBlock tb in wordCanvas.Children)
            {
                tb.Foreground = foreground;
                tb.Background = background;
            }
        }

        // On window resize, background grid need update
        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBackgroundGrid();
        }

        // Mouse click and drag management
        Point previousMousePosition;
        delegate void ProcessMouseMove(Point p);
        // null indicates canvas move, or delegate must be executed by MouseMove to perform move 
        // action, P is current mouse coordinates in non-transformed user space
        ProcessMouseMove pmm;

        private void MainGrid_MouseMoveWhenUp(object sender, MouseEventArgs e)
        {
            //previousMousePosition = e.GetPosition(MainGrid);

            //// Reverse-transform mouse Grid coordinates into Canvas coordinates
            //Matrix m = MainMatrixTransform.Matrix;

            //// ToDo
            //Point pointInUserSpace = new Point(0, 0), pointOnScreenSpace;
            //pointOnScreenSpace = m.Transform(pointInUserSpace);
        }


        // When moving a word
        List<Canvas> hitCanvasList;
        List<WordPosition> hitWordPositionList;

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainGrid.MouseMove -= MainGrid_MouseMoveWhenUp;
            MainGrid.MouseMove += MainGrid_MouseMoveWhenDown;
            previousMousePosition = e.GetPosition(MainGrid);

            // Reverse-transform mouse Grid coordinates into Canvas coordinates
            Matrix m = MainMatrixTransform.Matrix;

            pmm = null;
            // HitTest: Test if a word was clicked on, if true, hitTextBlock is a TextBloxk
            if (DrawingCanvas.InputHitTest(e.GetPosition(DrawingCanvas)) is TextBlock hitTextBlock)
            {
                // We want to move its parent Canvas, that contains all the text blocks for the word
                Canvas hitC = (hitTextBlock.Parent) as Canvas;
                if (!CanvasToWordPosition.ContainsKey(hitC))
                    Debugger.Break();
                WordPosition hitWP = CanvasToWordPosition[hitC];
                hitCanvasList = new List<Canvas> { hitC };
                hitWordPositionList = new List<WordPosition> { hitWP };

                // If Ctrl key is pressed, selection to move is extended to connected words
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    foreach (WordPosition connected in model.Layout.GetConnectedWordPositions(hitWP))
                    {
                        hitWordPositionList.Add(connected);
                        foreach (var k in CanvasToWordPosition)
                            if (k.Value == connected)
                            {
                                hitCanvasList.Add(k.Key);
                                break;
                            }
                    }

                foreach (var hitCanvas in hitCanvasList)
                {
                    DrawingCanvas.Children.Remove(hitCanvas);
                    DrawingCanvas.Children.Add(hitCanvas);
                    SetWordCanvasColor(hitCanvas, Brushes.White, Brushes.DarkBlue);
                }

                // Need a layout without moved word to validate placement
                model.BuildMoveTestLayout(hitWordPositionList);

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
                pmm = P =>      // When moving, P is current mouse in ideal grid coordinates
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
                        if (model.CanPlaceWordInMoveTestLayout(hitWordPositionList[i], left, top))
                            SetWordCanvasColor(hitCanvasList[i], Brushes.White, Brushes.DarkBlue);
                        else
                            SetWordCanvasColor(hitCanvasList[i], Brushes.White, Brushes.DarkRed);
                    }
                };
            }

            // Hit nothing: move background canvas itself (ppm=null)

            // Be sure to call GetPosition before Capture, otherwise GetPosition returns 0 after Capture
            // Capture to get MouseUp event raised by grid
            Mouse.Capture(MainGrid);
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

        private void UpdateTransformationsFeedBack()
        {
            // Update status bar
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
                List<int> leftList = new List<int>();
                List<int> topList = new List<int>();
                foreach (Canvas hitCanvas in hitCanvasList)
                {
                    SetWordCanvasColor(hitCanvas, Brushes.White, Brushes.Black);

                    // Round position to closest square on the grid
                    leftList.Add((int)Math.Floor(((double)hitCanvas.GetValue(Canvas.LeftProperty) / UnitSize) + 0.5));
                    topList.Add((int)Math.Floor(((double)hitCanvas.GetValue(Canvas.TopProperty) / UnitSize) + 0.5));
                }

                // If position is not valid, look around until a valid position is found
                // Examine surrounding cells in a "snail pattern" 
                bool CanPlaceAllWords()
                {
                    for (int il = 0; il < hitWordPositionList.Count; il++)
                        if (!model.CanPlaceWordInMoveTestLayout(hitWordPositionList[il], leftList[il], topList[il]))
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
                            for (int il = 0; il < hitWordPositionList.Count; il++)
                                leftList[il] += sign;
                            if (CanPlaceAllWords()) goto FoundValidPosition;
                        }
                        for (int i = 0; i < st; i++)
                        {
                            for (int il = 0; il < hitWordPositionList.Count; il++)
                                topList[il] += sign;
                            if (CanPlaceAllWords()) goto FoundValidPosition;
                        }
                        sign = -sign;
                        st++;
                    }
                }

                FoundValidPosition:
                // Move wp to final, rounded position
                for (int il = 0; il < hitWordPositionList.Count; il++)
                {
                    hitCanvasList[il].SetValue(Canvas.LeftProperty, leftList[il] * UnitSize);
                    hitCanvasList[il].SetValue(Canvas.TopProperty, topList[il] * UnitSize);
                    // Notify ViewModel of the final position
                    viewModel.UpdateWordPositionLocation(hitWordPositionList[il], leftList[il], topList[il]);
                }

            }
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



        private void UpdateBackgroundGrid()
        {
            var matrix = MainMatrixTransform.Matrix;

            var w = MainGrid.ActualWidth;
            var h = MainGrid.ActualHeight;
            Rect rectBackground = new Rect(0, 0, w, h);

            PathGeometry pgGrid = new PathGeometry();
            PathGeometry pgAxis = new PathGeometry();
            PathFigure pf;


            // Origin and vectors i and j transformed to get new referential
            Point o = matrix.Transform(new Point(0, 0));
            Vector vi, vj;
            vi = matrix.Transform(new Vector(UnitSize, 0));
            vj = matrix.Transform(new Vector(0, UnitSize));


            // Draw grid using transformed referential from -gmax to gmax on both axes
            // A better algorithm should be implemented, supporting infinite scroll
            // typically find a point of the grid on screen, then explore/expand to
            // fill the screen from this point
            const int gmax = 145;
            for (int i = -gmax; i < gmax; i++)
                for (int j = -gmax; j < gmax; j++)
                {
                    Point p1 = o + i * vi + j * vj;
                    Point p2 = p1 + vi;
                    if (rectBackground.Contains(p1) || rectBackground.Contains(p2))
                    {
                        pf = new PathFigure(p1, new List<PathSegment> { new LineSegment(p2, true) }, false);
                        if (j == 0)
                            pgAxis.Figures.Add(pf);
                        else
                            pgGrid.Figures.Add(pf);
                    }
                    p2 = p1 + vj;
                    if (rectBackground.Contains(p1) || rectBackground.Contains(p2))
                    {
                        pf = new PathFigure()
                        {
                            StartPoint = p1
                        };
                        pf.Segments.Add(new LineSegment(p2, true));
                        if (i == 0)
                            pgAxis.Figures.Add(pf);
                        else
                            pgGrid.Figures.Add(pf);
                    }

                }


            // Grid in gray
            GeometryDrawing gdGrid = new GeometryDrawing(null, new Pen(Brushes.LightGray, 1.0), pgGrid);
            GeometryDrawing gdAxis = new GeometryDrawing(null, new Pen(Brushes.LightGray, 3.0), pgAxis);

            DrawingGroup dg = new DrawingGroup();
            dg.Children.Add(gdGrid);
            dg.Children.Add(gdAxis);

            // Add a transparent border rectangle to force scale to be correct just in case
            // there is only 1 line drawn in one direction
            var eg = new RectangleGeometry(new Rect(0, 0, w, h));
            dg.Children.Add(new GeometryDrawing(null, new Pen(Brushes.Transparent, 1.0), eg));

            // And also clip on this rectangle
            dg.ClipGeometry = eg;

            // Make a DrawingImage and freeze it for performance
            DrawingImage di = new DrawingImage(dg);
            di.Freeze();

            // Finally draw it!
            BackgroundImage.Source = di;


            // Update status bar
            var a = Math.Atan2(matrix.M12, matrix.M11) / Math.PI * 180;
            RotationTextBlock.Text = string.Format("{0:F1}°", a);
            var s = Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
            ScaleTextBlock.Text = string.Format("{0:F2}", s);
        }

    }
}
