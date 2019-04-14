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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SolWPF
{

    public partial class MainWindow : Window
    {
        public static double cardWidth = 100, cardHeight = 140;
        private BaseStack[] Bases;
        private ColumnStack[] Columns;
        private TalonStack Talon;

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
            Bases = new BaseStack[4];
            Bases[0] = new BaseStack(PlayingCanvas, Base0);
            Bases[1] = new BaseStack(PlayingCanvas, Base1);
            Bases[2] = new BaseStack(PlayingCanvas, Base2);
            Bases[3] = new BaseStack(PlayingCanvas, Base3);

            Columns = new ColumnStack[7];
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
            for (int mt = 0; mt < 3 /*lc.Count*/; mt++)     // 3 = Temp for testing talon rotation with a 3-card talon
            {
                Talon.AddCard(lc[mt], true);
                Debug.WriteLine($"Talon Add {lc[mt]}");
            }
            Talon.AddCard("@@", false);
            Talon.PrintTalon("Initial           ");
        }


        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Mouse click and drag management
        Point previousMousePosition;
        delegate void ProcessMouseMove(Point p);
        ProcessMouseMove pmm;       // null indicates canvas move
        Point StartingPoint;
        bool isMovingMode;
        DateTime lastMouseUpDateTime = DateTime.MinValue;


        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            previousMousePosition = e.GetPosition(mainGrid);

            // Reverse-transform mouse grid coordinates (screen) into Canvas coordinates
            Matrix m = mainMatrixTransform.Matrix;
            m.Invert();
            var mouseT = m.Transform(previousMousePosition);

            pmm = null;
            isMovingMode = false;

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
                                if (gsTarget.ToHitTest(P, movingGroup))
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

            // Clicked outside active and valid area
            movingGroup = null;
        }


        void MainGrid_MouseMoveWhenUp(object sender, MouseEventArgs e)
        {
            // Nothing special to do
            // Cards will react to hovering themselves
        }

        // Do not really start moving unless the move has moved 5 pixels in any direction, makes detection of clicks easier
        void MainGrid_MouseMoveWhenDown(object sender, MouseEventArgs e)
        {
            var newMousePosition = e.GetPosition(mainGrid);
            // We only onsider a real move beyond a system-defined threshold, configured at 4 pixels (both X and Y) on my machine
            if (!isMovingMode && movingGroup.IsMouvable && (Math.Abs(newMousePosition.X - previousMousePosition.X) >= SystemParameters.MinimumHorizontalDragDistance || Math.Abs(newMousePosition.Y - previousMousePosition.Y) >= SystemParameters.MinimumVerticalDragDistance))
                isMovingMode = true;

            if (isMovingMode)
            {
                Matrix m = mainMatrixTransform.Matrix;
                m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
                pmm(m.Transform(newMousePosition));
            }
        }

        // Just to avoid a reference to Windows.Forms...
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)] public static extern int GetDoubleClickTime();

        private void MainGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mainGrid.MouseMove -= new MouseEventHandler(MainGrid_MouseMoveWhenDown);
            mainGrid.MouseMove += new MouseEventHandler(MainGrid_MouseMoveWhenUp);

            // Clicked outside any active area
            if (movingGroup == null)
                return;

            if (!isMovingMode)
            {
                var mouseUpDateTime = System.DateTime.Now;
                //if ((clickDateTime - lastClickDateTime).TotalMilliseconds <= 100 /* GetDoubleClickTime()*/ )
                //{
                //    //Debug.WriteLine("Double-Click detected on MovingGroup");
                //    DoubleClickOnGroup(movingGroup);
                //    lastClickDateTime = DateTime.MinValue;  // Don't need a triple-click or multiple double-cliks!
                //    return;
                //}

                ClickOnGroup(movingGroup);
                lastMouseUpDateTime = mouseUpDateTime;
                return;
            }

            if (movingGroup.ToStack != null)
            {
                movingGroup.ToStack.ClearTargetHighlight();
                movingGroup.DoMove();
            }
            else
                movingGroup.SetTopLeft(StartingPoint);
        }


        private void ClickOnGroup(MovingGroup movingGroup)
        {
            Debug.WriteLine("ClickOnGroup");
            if (movingGroup.FromStack is TalonStack)
            {
                // Talon rotation
                Talon.RotateOne();
                return;
            }
        }


        private void DoubleClickOnGroup(MovingGroup movingGroup)
        {
            Debug.WriteLine("DoubleClickOnGroup");
            // ToDo: Automatic move mechanisms
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
