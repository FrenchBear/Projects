using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace SolWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double cardWidth = 100, cardHeight = 140;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyCard.Width = cardWidth;
            MyCard.Height = cardHeight;

            Rectangle1.Width = cardWidth;
            Rectangle1.Height = cardHeight;

            Rectangle2.Width = cardWidth;
            Rectangle2.Height = cardHeight;
        }


        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Mouse click and drag management
        Point previousMousePosition;
        delegate void ProcessMouseMove(Point p);
        ProcessMouseMove pmm;       // null indicates canvas move

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            previousMousePosition = e.GetPosition(mainGrid);

            // Reverse-transform mouse Grid coordinates into Canvas coordinates
            Matrix m = mainMatrixTransform.Matrix;

            pmm = null;
            Point P1 = new Point((double)Rectangle1.GetValue(Canvas.LeftProperty), (double)Rectangle1.GetValue(Canvas.TopProperty));
            Point P2 = new Point((double)Rectangle1.GetValue(Canvas.LeftProperty) + cardWidth, (double)Rectangle1.GetValue(Canvas.TopProperty) + cardHeight);
            Point P1T = m.Transform(P1);
            Point P2T = m.Transform(P2);

            Debug.WriteLine($"P1:({P1.X}; {P1.Y}), P1T:({P1T.X}; {P1T.Y}), Mouse:({previousMousePosition.X}; {previousMousePosition.Y})");

            if (previousMousePosition.X >= P1T.X && previousMousePosition.X <= P2T.X && previousMousePosition.Y >= P1T.Y && previousMousePosition.Y <= P2T.Y)
            {
                // Move card
                m.Invert();
                Vector v = P1 - m.Transform(previousMousePosition);
                Point Q1 = new Point((double)Rectangle2.GetValue(Canvas.LeftProperty), (double)Rectangle2.GetValue(Canvas.TopProperty));
                Point Q2 = new Point((double)Rectangle2.GetValue(Canvas.LeftProperty) + cardWidth, (double)Rectangle2.GetValue(Canvas.TopProperty) + cardHeight);
                pmm = P =>
                {
                    MyCard.SetValue(Canvas.LeftProperty, P.X + v.X);
                    MyCard.SetValue(Canvas.TopProperty, P.Y + v.Y);

                    if (P.X >= Q1.X && P.X <= Q2.X && P.Y >= Q1.Y && P.Y <= Q2.Y)
                    {
                        Rectangle2.Stroke = Brushes.Red;
                        Rectangle2.StrokeThickness = 5.0;
                    }
                    else
                    {
                        Rectangle2.Stroke = Brushes.Black;
                        Rectangle2.StrokeThickness = 3.0;
                    }
                    //Debug.WriteLine($"pmm: P=({P.X}, {P.Y}), Q1=({Q1.X}, {Q1.Y}), Q1T=({Q1T.X}, {Q1T.Y})");
                };
                // Be sure to call GetPosition before Capture, otherwise GetPosition returns 0 after Capture
                // Capture to get MouseUp event raised by grid
                Mouse.Capture(mainGrid);
                mainGrid.MouseMove -= new MouseEventHandler(MainGrid_MouseMoveWhenUp);
                mainGrid.MouseMove += new MouseEventHandler(MainGrid_MouseMoveWhenDown);
            }
        }

        void MainGrid_MouseMoveWhenUp(object sender, MouseEventArgs e)
        {
            // Nothing special to do
        }

        void MainGrid_MouseMoveWhenDown(object sender, MouseEventArgs e)
        {
            var newPosition = e.GetPosition(mainGrid);
            Matrix m = mainMatrixTransform.Matrix;

            if (pmm == null)
            {
                // nop
                // shouldn't consider this case here
            }
            else
            {
                // Move a point defining a line end
                m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
                pmm(m.Transform(newPosition));
            }
        }

        private void MainGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mainGrid.MouseMove -= new MouseEventHandler(MainGrid_MouseMoveWhenDown);
            mainGrid.MouseMove += new MouseEventHandler(MainGrid_MouseMoveWhenUp);

            if (Rectangle2.Stroke == Brushes.Red)
            {
                MyCard.SetValue(Canvas.LeftProperty, Rectangle2.GetValue(Canvas.LeftProperty));
                MyCard.SetValue(Canvas.TopProperty, Rectangle2.GetValue(Canvas.TopProperty));
                Rectangle2.Stroke = Brushes.Black;
                Rectangle2.StrokeThickness = 3.0;
            }
            else
            {
                MyCard.SetValue(Canvas.LeftProperty, Rectangle1.GetValue(Canvas.LeftProperty));
                MyCard.SetValue(Canvas.TopProperty, Rectangle1.GetValue(Canvas.TopProperty));
            }
        }

        private void MainGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var newPosition = e.GetPosition(mainGrid);
            var m = mainMatrixTransform.Matrix;

            // Ctrl+MouseWheel for rotation
            if (System.Windows.Input.Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                //double angle = e.Delta / 16.0;
                //m.RotateAt(angle, newPosition.X, newPosition.Y);
            }
            else
            {
                var sign = Math.Sign(e.Delta);
                var scale = 1 - sign / 10.0;
                m.ScaleAt(scale, scale, newPosition.X, newPosition.Y);
            }
            mainMatrixTransform.Matrix = m;

            //UpdateTransformationsFeedBack();
            //UpdateBackgroundGrid();
        }
    }
}
