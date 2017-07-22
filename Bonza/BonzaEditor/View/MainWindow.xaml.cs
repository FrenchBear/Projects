// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM View
// 2017-07-22   PV  First version


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bonza
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BonzaViewModel myViewModel;

        const double UnitSize = 25;

        public MainWindow()
        {
            InitializeComponent();
            myViewModel = new BonzaViewModel();
            DataContext = myViewModel;
            UpdateTransformationsFeedBack();

            // Can only reference ActualWidth after Window is loaded
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var puzzle = new BonzaPuzzle();

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

            // Draw letters
            foreach (WordPosition wp in puzzle.Layout)
            {
                for (int i = 0; i < wp.Word.Length; i++)
                {
                    TextBlock tb = new TextBlock();
                    tb.Background = Brushes.Black;
                    tb.Foreground = Brushes.White;
                    tb.TextAlignment = TextAlignment.Center;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.Text = wp.Word.Substring(i, 1);
                    tb.FontFamily = new FontFamily("Arial");
                    tb.FontSize = 16;
                    tb.FontWeight = FontWeights.Bold;
                    tb.Padding = new Thickness(0, 3, 0, 0);

                    tb.SetValue(Canvas.LeftProperty, 1 + UnitSize * (wp.StartColumn + (wp.IsVertical ? 0 : i)));
                    tb.SetValue(Canvas.TopProperty, 1 + UnitSize * (wp.StartRow + (wp.IsVertical ? i : 0)));


                    if (i == wp.Word.Length - 1 || wp.IsVertical)
                        tb.Width = UnitSize - 2;
                    else
                        tb.Width = UnitSize;

                    if (i == wp.Word.Length - 1 || !wp.IsVertical)
                        tb.Height = UnitSize - 2;
                    else
                        tb.Height = UnitSize;

                    myCanvas.Children.Add(tb);
                }
            }
        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBackgroundGrid();
        }

        // Mouse click and drag management
        Point previousMousePosition;
        delegate void ProcessMouseMove(Point p);
        ProcessMouseMove pmm;       // null indicates canvas move


        private void MainGrid_MouseMoveWhenUp(object sender, MouseEventArgs e)
        {
            previousMousePosition = e.GetPosition(mainGrid);

            // Reverse-transform mouse Grid coordinates into Canvas coordinates
            Matrix m = mainMatrixTransform.Matrix;

            // ToDo
            Point pointInUserSpace = new Point(0, 0), pointOnScreenSpace;
            pointOnScreenSpace = m.Transform(pointInUserSpace);
        }

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainGrid.MouseMove -= new MouseEventHandler(MainGrid_MouseMoveWhenUp);
            mainGrid.MouseMove += new MouseEventHandler(MainGrid_MouseMoveWhenDown);
            previousMousePosition = e.GetPosition(mainGrid);

            // Reverse-transform mouse Grid coordinates into Canvas coordinates
            Matrix m = mainMatrixTransform.Matrix;

            pmm = null;
            // HitTest: ToDo

            // No hit test: move canvas itself (ppm=null)

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
                // Move a point defining a line end
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
