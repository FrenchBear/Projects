// SolWPF
// Early tests in WPF of Solitaire cards drag-n-drop
// 2019-04-09   PV

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SolWPF
{

    public partial class MainWindow : Window
    {
        private double cardWidth = 100, cardHeight = 140;
        private Rectangle[] BasesRect;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyCard.Width = cardWidth;
            MyCard.Height = cardHeight;
            MyCard.SetValue(Canvas.LeftProperty, (double)Rectangle1.GetValue(Canvas.LeftProperty));
            MyCard.SetValue(Canvas.TopProperty, (double)Rectangle1.GetValue(Canvas.TopProperty));

            BasesRect = new Rectangle[] { Rectangle1, Rectangle2, Rectangle3, Rectangle4 };
            foreach (var br in BasesRect)
            {
                br.Width = cardWidth;
                br.Height = cardHeight;
            }
        }


        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Mouse click and drag management
        Point previousMousePosition;
        delegate void ProcessMouseMove(Point p);
        ProcessMouseMove pmm;       // null indicates canvas move
        double startingLeft, startingTop;
        Rectangle startingRect;

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            previousMousePosition = e.GetPosition(mainGrid);

            // Reverse-transform mouse Grid coordinates into Canvas coordinates
            Matrix m = mainMatrixTransform.Matrix;

            pmm = null;

            foreach (var br in BasesRect)
            {
                Point P1 = new Point((double)br.GetValue(Canvas.LeftProperty), (double)br.GetValue(Canvas.TopProperty));
                Point P2 = new Point((double)br.GetValue(Canvas.LeftProperty) + cardWidth, (double)br.GetValue(Canvas.TopProperty) + cardHeight);
                Point P1T = m.Transform(P1);
                Point P2T = m.Transform(P2);

                if (previousMousePosition.X >= P1T.X && previousMousePosition.X <= P2T.X && previousMousePosition.Y >= P1T.Y && previousMousePosition.Y <= P2T.Y)
                {
                    startingLeft = P1.X;
                    startingTop = P1.Y;
                    startingRect = br;

                    // Move card
                    m.Invert();
                    Vector v = P1 - m.Transform(previousMousePosition);
                    pmm = P =>
                    {
                        MyCard.SetValue(Canvas.LeftProperty, P.X + v.X);
                        MyCard.SetValue(Canvas.TopProperty, P.Y + v.Y);

                        foreach (var br in BasesRect)
                            if (br != startingRect)
                            {
                                Point Q = new Point((double)br.GetValue(Canvas.LeftProperty), (double)br.GetValue(Canvas.TopProperty));

                                if (P.X >= Q.X && P.X <= Q.X + cardWidth && P.Y >= Q.Y && P.Y <= Q.Y + cardHeight)
                                {
                                    br.Stroke = Brushes.Red;
                                    br.StrokeThickness = 5.0;
                                }
                                else
                                {
                                    br.Stroke = Brushes.Black;
                                    br.StrokeThickness = 3.0;
                                }
                            }
                    };
                    // Be sure to call GetPosition before Capture, otherwise GetPosition returns 0 after Capture
                    // Capture to get MouseUp event raised by grid
                    Mouse.Capture(mainGrid);
                    mainGrid.MouseMove -= new MouseEventHandler(MainGrid_MouseMoveWhenUp);
                    mainGrid.MouseMove += new MouseEventHandler(MainGrid_MouseMoveWhenDown);
                    return;
                }
            }
        }

        void MainGrid_MouseMoveWhenUp(object sender, MouseEventArgs e)
        {
            // Nothing special to do
            // Cards will react to hovering themselves
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
                m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
                pmm(m.Transform(newPosition));
            }
        }

        private void MainGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mainGrid.MouseMove -= new MouseEventHandler(MainGrid_MouseMoveWhenDown);
            mainGrid.MouseMove += new MouseEventHandler(MainGrid_MouseMoveWhenUp);

            bool found = false;
            foreach (var br in BasesRect)
                if (br.Stroke == Brushes.Red)
                {
                    MyCard.SetValue(Canvas.LeftProperty, br.GetValue(Canvas.LeftProperty));
                    MyCard.SetValue(Canvas.TopProperty, br.GetValue(Canvas.TopProperty));
                    br.Stroke = Brushes.Black;
                    br.StrokeThickness = 3.0;
                    found = true;
                    break;
                }

            if (!found)
            {
                MyCard.SetValue(Canvas.LeftProperty, startingLeft);
                MyCard.SetValue(Canvas.TopProperty, startingTop);
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
        }
    }
}
