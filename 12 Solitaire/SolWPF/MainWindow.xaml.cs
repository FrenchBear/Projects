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
            Columns[0] = new ColumnStack(PlayingCanvas, Column0);
            Columns[1] = new ColumnStack(PlayingCanvas, Column1);
            Columns[2] = new ColumnStack(PlayingCanvas, Column2);
            Columns[3] = new ColumnStack(PlayingCanvas, Column3);
            Columns[4] = new ColumnStack(PlayingCanvas, Column4);
            Columns[5] = new ColumnStack(PlayingCanvas, Column5);
            Columns[6] = new ColumnStack(PlayingCanvas, Column6);

            Talon = new TalonStack(PlayingCanvas, Talon0);

            var lc = new List<string>();
            foreach (char c in "HDSC")
                foreach (char v in "A23456789XJQK")
                    lc.Add($"{c}{v}");
            var rnd = new Random();
            for (int i = 0; i < lc.Count; i++)
            {
                var i1 = rnd.Next(lc.Count);
                var i2 = rnd.Next(lc.Count);
                var t = lc[i1];
                lc[i1] = lc[i2];
                lc[i2] = t;
            }

            for (int c = 0; c < 7; c++)
                for (int i = 0; i <= c; i++)
                {
                    string s = lc[0];
                    lc.RemoveAt(0);
                    Columns[c].AddCard(s, i == c);
                }
            foreach (string s in lc)
                Talon.AddCard(s, false);
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
                movingGroup = gsSource.FromHitTest(mouseT);
                if (movingGroup != null)
                {
                    movingGroup.FromStack = gsSource;
                    Point P1 = movingGroup.GetTopLeft();
                    StartingPoint = P1;
                    Vector v = P1 - mouseT;
                    // A closure to be executed my MouseMoveWhenDown with mouse coordinates (in game space) as parameter P
                    pmm = P =>
                    {
                        movingGroup.SetTopLeft(P + v);
                        movingGroup.ToStack = null;
                        foreach (var gsTarget in AllStacks())
                            if (gsTarget != movingGroup.FromStack)
                            {
                                if (gsTarget.ToHitTest(P))
                                    movingGroup.ToStack = gsTarget;
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
            m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
            pmm(m.Transform(newPosition));
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
