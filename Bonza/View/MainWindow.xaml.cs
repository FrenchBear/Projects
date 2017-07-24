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


namespace Bonza.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BonzaViewModel viewModel;

        const double UnitSize = 25;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new BonzaViewModel();
            DataContext = viewModel;
            UpdateTransformationsFeedBack();

            // Can only reference ActualWidth after Window is loaded
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);
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

                myCanvas.Children.Add(r);
            }
            */


            // Draw letters
            var arial = new FontFamily("Arial");
            foreach (WordPosition wp in viewModel.Layout)
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
                myCanvas.Children.Add(wordCanvas);
            }
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
            //previousMousePosition = e.GetPosition(mainGrid);

            //// Reverse-transform mouse Grid coordinates into Canvas coordinates
            //Matrix m = mainMatrixTransform.Matrix;

            //// ToDo
            //Point pointInUserSpace = new Point(0, 0), pointOnScreenSpace;
            //pointOnScreenSpace = m.Transform(pointInUserSpace);
        }


        // When moving a word
        Canvas hitCanvas;
        WordPosition hitWordPosition;

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainGrid.MouseMove -= new MouseEventHandler(MainGrid_MouseMoveWhenUp);
            mainGrid.MouseMove += new MouseEventHandler(MainGrid_MouseMoveWhenDown);
            previousMousePosition = e.GetPosition(mainGrid);

            // Reverse-transform mouse Grid coordinates into Canvas coordinates
            Matrix m = mainMatrixTransform.Matrix;

            pmm = null;
            // HitTest: Test if a word was clicked on, if true, hitTextBlock is a TextBloxk
            if (myCanvas.InputHitTest(e.GetPosition(myCanvas)) is TextBlock hitTextBlock)
            {
                // We want to move its parent Canvas, that contains all the text blocks for the word
                hitCanvas = (hitTextBlock.Parent) as Canvas;
                if (!CanvasToWordPosition.ContainsKey(hitCanvas))
                    Debugger.Break();
                hitWordPosition = CanvasToWordPosition[hitCanvas];

                if (hitCanvas != null)
                {
                    // Make sure that hitCanvas is drawn on top of others
                    myCanvas.Children.Remove(hitCanvas);
                    myCanvas.Children.Add(hitCanvas);
                    SetWordCanvasColor(hitCanvas, Brushes.White, Brushes.DarkBlue);

                    m.Invert();     // To convert from screen transformed coordinates into ideal grid
                                    // coordinates starting at (0,0) with a square side of UnitSize
                    Point canvasTopLeft = new Point((double)hitCanvas.GetValue(Canvas.LeftProperty), (double)hitCanvas.GetValue(Canvas.TopProperty));
                    // clickOffset memorizes the difference between (top,left) of word canvas and the clicked point
                    // since when we move, we need that information to adjust word canvas position
                    Vector clickOffset = canvasTopLeft - m.Transform(previousMousePosition);
                    pmm = P =>      // When moving, P is current mouse in ideal grid coordinates
                    {
                        // Just move canvas
                        hitCanvas.SetValue(Canvas.TopProperty, P.Y + clickOffset.Y);
                        hitCanvas.SetValue(Canvas.LeftProperty, P.X + clickOffset.X);

                        // Round position to closest square on the grid
                        int left = (int)Math.Floor(((double)hitCanvas.GetValue(Canvas.LeftProperty) / UnitSize) + 0.5);
                        int top = (int)Math.Floor(((double)hitCanvas.GetValue(Canvas.TopProperty) / UnitSize) + 0.5);

                        if (viewModel.OkPlaceWord(hitWordPosition, left, top))
                            SetWordCanvasColor(hitCanvas, Brushes.White, Brushes.DarkBlue);
                        else
                            SetWordCanvasColor(hitCanvas, Brushes.White, Brushes.DarkRed);
                    };
                }
            }

            // Hit nothing: move background canvas itself (ppm=null)

            // Be sure to call GetPosition before Capture, otherwise GetPosition returns 0 after Capture
            // Capture to get MouseUp event raised by grid
            Mouse.Capture(mainGrid);
        }


        void MainGrid_MouseMoveWhenDown(object sender, MouseEventArgs e)
        {
            var newPosition = e.GetPosition(mainGrid);
            Matrix m = mainMatrixTransform.Matrix;

            if (pmm == null)
            {
                // move drawing surface
                var delta = newPosition - previousMousePosition;
                previousMousePosition = newPosition;
                m.Translate(delta.X, delta.Y);
                mainMatrixTransform.Matrix = m;
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
            Matrix matrix = mainMatrixTransform.Matrix;
            var a = Math.Atan2(matrix.M12, matrix.M11) / Math.PI * 180;
            rotationTextBlock.Text = string.Format("{0:F1}°", a);
            var s = Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
            scaleTextBlock.Text = string.Format("{0:F2}", s);
            translationTextBlock.Text = string.Format("X:{0:F2} Y:{1:F2}", matrix.OffsetX, matrix.OffsetY);
        }



        private void MainGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mainGrid.MouseMove -= new MouseEventHandler(MainGrid_MouseMoveWhenDown);
            mainGrid.MouseMove += new MouseEventHandler(MainGrid_MouseMoveWhenUp);

            if (pmm != null)
            {
                // End of visual feed-back, align on grid, and update ViewModel
                SetWordCanvasColor(hitCanvas, Brushes.White, Brushes.Black);

                // Round position to closest square on the grid
                int left = (int)Math.Floor(((double)hitCanvas.GetValue(Canvas.LeftProperty) / UnitSize) + 0.5);
                int top = (int)Math.Floor(((double)hitCanvas.GetValue(Canvas.TopProperty) / UnitSize) + 0.5);

                // If position is not valid, look around until a valid position is found
                // Examine surrounding cells in a "snail pattern" 
                if (!viewModel.OkPlaceWord(hitWordPosition, left, top))
                {
                    int st = 1;
                    int sign = 1;

                    for (;;)
                    {
                        for (int i = 0; i < st; i++)
                        {
                            left += sign;
                            if (viewModel.OkPlaceWord(hitWordPosition, left, top)) goto FoundValidPosition;
                        }
                        for (int i = 0; i < st; i++)
                        {
                            top += sign;
                            if (viewModel.OkPlaceWord(hitWordPosition, left, top)) goto FoundValidPosition;
                        }
                        sign = -sign;
                        st++;
                    }
                }

            FoundValidPosition:
                hitCanvas.SetValue(Canvas.LeftProperty, left * UnitSize);
                hitCanvas.SetValue(Canvas.TopProperty, top * UnitSize);

                // Notify ViewModel of the final position
                viewModel.UpdateWordPositionLocation(hitWordPosition, left, top);
            }
        }

        private void MainGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var newPosition = e.GetPosition(mainGrid);
            var m = mainMatrixTransform.Matrix;

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
            mainMatrixTransform.Matrix = m;

            UpdateTransformationsFeedBack();
            UpdateBackgroundGrid();
        }



        private void UpdateBackgroundGrid()
        {
            var matrix = mainMatrixTransform.Matrix;

            var w = mainGrid.ActualWidth;
            var h = mainGrid.ActualHeight;
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
            backgroundImage.Source = di;


            // Update status bar
            var a = Math.Atan2(matrix.M12, matrix.M11) / Math.PI * 180;
            rotationTextBlock.Text = string.Format("{0:F1}°", a);
            var s = Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
            scaleTextBlock.Text = string.Format("{0:F2}", s);
        }

    }
}
