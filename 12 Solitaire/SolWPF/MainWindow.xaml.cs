// SolWPF
// Early tests in WPF of Solitaire cards drag-n-drop
// 2019-04-09   PV

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;


namespace SolWPF
{

    public partial class MainWindow : Window
    {
        public static double cardWidth = 100, cardHeight = 140;
        private GameStack[] Bases;
        private GameStack[] Columns;
        private GameStack Talon;

        private IEnumerable<GameStack> AllStacks()
        {
            foreach (var gs in Bases)
                yield return gs;
            foreach (var gs in Columns)
                yield return gs;
            yield return Talon;
        }

        MovingGroup movingGroup;    // Origin of card moved
        //PlayingCard MyCard;     // Card being moved
        //int CardSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Bases = new GameStack[4];
            Bases[0] = new GameStack(PlayingCanvas, Base0);
            Bases[1] = new GameStack(PlayingCanvas, Base1);
            Bases[2] = new GameStack(PlayingCanvas, Base2);
            Bases[3] = new GameStack(PlayingCanvas, Base3);

            Columns = new GameStack[7];
            Columns[0] = new GameStack(PlayingCanvas, Column0);
            Columns[1] = new GameStack(PlayingCanvas, Column1);
            Columns[2] = new GameStack(PlayingCanvas, Column2);
            Columns[3] = new GameStack(PlayingCanvas, Column3);
            Columns[4] = new GameStack(PlayingCanvas, Column4);
            Columns[5] = new GameStack(PlayingCanvas, Column5);
            Columns[6] = new GameStack(PlayingCanvas, Column6);

            Talon = new TalonStack(PlayingCanvas, Talon0);

            Bases[0].AddCard("HK");
            Bases[0].AddCard("DQ");
            Bases[0].AddCard("C3");
            Bases[3].AddCard("@@");
        }


        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Mouse click and drag management
        Point previousMousePosition;
        delegate void ProcessMouseMove(Point p);
        ProcessMouseMove pmm;       // null indicates canvas move
        Rectangle startingRect;
        Point StartingPoint;

        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            previousMousePosition = e.GetPosition(mainGrid);

            // Reverse-transform mouse grid coordinates (screen) into Canvas coordinates
            Matrix m = mainMatrixTransform.Matrix;
            m.Invert();
            var mouseT = m.Transform(previousMousePosition);

            pmm = null;
            //movingGroup= null;

            foreach (var gsSource in AllStacks())
            {
                movingGroup = gsSource.startingHit(mouseT);
                if (movingGroup != null)
                {
                    movingGroup.FromStack = gsSource;
                    Point P1 = movingGroup.GetTopLeft();

                    //Point P1 = new Point((double)br.GetValue(Canvas.LeftProperty), (double)br.GetValue(Canvas.TopProperty));
                    //Point P2 = new Point((double)br.GetValue(Canvas.LeftProperty) + cardWidth, (double)br.GetValue(Canvas.TopProperty) + cardHeight);

                    StartingPoint = P1;

                    // Move card
                    Vector v = P1 - mouseT;

                    pmm = P =>
                    {
                        movingGroup.SetTopLeft(P + v);
                        movingGroup.ToStack = null;
                        foreach (var gsTarget in AllStacks())
                            if (gsTarget != movingGroup.FromStack)
                            {
                                if (gsTarget.isTargetHit(P))
                                    movingGroup.ToStack = gsTarget;
                                /*
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
                                    */
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
                // shouldn't consider this case for solitaire (don't translate background)
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

            if (movingGroup == null)
                return;

            if (movingGroup.ToStack != null)
            {
                movingGroup.ToStack.ClearTargetHighlight();
                movingGroup.DoMove();
            }
            else
                movingGroup.SetTopLeft(StartingPoint);
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
