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
        private TalonFaceDownStack TalonFD;
        private TalonFaceUpStack TalonFU;

        private IEnumerable<GameStack> AllStacks()
        {
            foreach (var gs in Bases)
                yield return gs;
            foreach (var gs in Columns)
                yield return gs;
            yield return TalonFU;
            yield return TalonFD;
        }

        MovingGroup movingGroup;    // Origin of card moved

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Bases = new BaseStack[4];
            Bases[0] = new BaseStack("Base0", PlayingCanvas, Base0);
            Bases[1] = new BaseStack("Base1", PlayingCanvas, Base1);
            Bases[2] = new BaseStack("Base2", PlayingCanvas, Base2);
            Bases[3] = new BaseStack("Base3", PlayingCanvas, Base3);

            Columns = new ColumnStack[7];
            Columns[0] = new ColumnStack("Column0", PlayingCanvas, Column0);
            Columns[1] = new ColumnStack("Column1", PlayingCanvas, Column1);
            Columns[2] = new ColumnStack("Column2", PlayingCanvas, Column2);
            Columns[3] = new ColumnStack("Column3", PlayingCanvas, Column3);
            Columns[4] = new ColumnStack("Column4", PlayingCanvas, Column4);
            Columns[5] = new ColumnStack("Column5", PlayingCanvas, Column5);
            Columns[6] = new ColumnStack("Column6", PlayingCanvas, Column6);

            TalonFD = new TalonFaceDownStack("TalonFD", PlayingCanvas, Talon0);
            TalonFU = new TalonFaceUpStack("TalonFU", PlayingCanvas, Talon1);

            var lc = new List<string>();
            foreach (char c in "HDSC")
                foreach (char v in "A23456789XJQK")
                    lc.Add($"{c}{v}");
            var rnd = new Random(22);
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
            for (int mt = 0; mt < lc.Count; mt++)     // 3 = Temp for testing talon rotation with a 3-card talon
            {
                TalonFD.AddCard(lc[mt], false);
                //Debug.WriteLine($"TalonFD Add {lc[mt]}");
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
                    Point P1 = movingGroup.GetTopLeft();
                    StartingPoint = P1;
                    Vector v = P1 - mouseT;
                    // A closure to be executed my MouseMoveWhenDown with mouse coordinates (in game space) as parameter P
                    pmm = P =>
                    {
                        movingGroup.SetTopLeft(P + v);
                        movingGroup.ToStack = null;                     // Reset in case mouse is moving away from a potential drop target
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


        // Do not really start moving unless the group is movable the mouse has moved 4 pixels in any direction, makes detection of clicks easier
        void MainGrid_MouseMoveWhenDown(object sender, MouseEventArgs e)
        {
            var newMousePosition = e.GetPosition(mainGrid);
            // We only onsider a real move beyond a system-defined threshold, configured at 4 pixels (both X and Y) on my machine
            if (!isMovingMode && movingGroup.IsMovable && (Math.Abs(newMousePosition.X - previousMousePosition.X) >= SystemParameters.MinimumHorizontalDragDistance || Math.Abs(newMousePosition.Y - previousMousePosition.Y) >= SystemParameters.MinimumVerticalDragDistance))
                isMovingMode = true;

            if (isMovingMode)
            {
                Matrix m = mainMatrixTransform.Matrix;
                m.Invert();     // By construction, all applied transformations are reversible, so m is invertible
                pmm(m.Transform(newMousePosition));
            }
        }

        // Just to avoid a reference to Windows.Forms...
        // Anyway, its current valus in 500ms which is waaaaaaaaay too long for me
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)] public static extern int GetDoubleClickTime();

        private void MainGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mainGrid.MouseMove -= new MouseEventHandler(MainGrid_MouseMoveWhenDown);
            mainGrid.MouseMove += new MouseEventHandler(MainGrid_MouseMoveWhenUp);

            // Clicked outside any active area
            if (movingGroup == null)
                return;

            // If move did not start (goup not movable or mouse didn't mouve enough), it's a click or a double click
            // Note that movingGroup.ToStack is null in this case
            if (!isMovingMode)
            {
                var mouseUpDateTime = System.DateTime.Now;
                if ((mouseUpDateTime - lastMouseUpDateTime).TotalMilliseconds <= 200 /* GetDoubleClickTime()*/ )
                {
                    Debug.WriteLine("Double-Click detected on MovingGroup");
                    DoubleClickOnGroup(movingGroup);
                    lastMouseUpDateTime = DateTime.MinValue;  // Don't need a triple-click or multiple double-cliks!
                    return;
                }

                Debug.WriteLine("Click detected on MovingGroup");
                ClickOnGroup(movingGroup);
                lastMouseUpDateTime = mouseUpDateTime;
                return;
            }

            // Move started
            if (movingGroup.ToStack != null)
            {
                // Valid target selected, Ok to move (without visual animation, or maybe from trop point to target point??)
                movingGroup.DoMove();
            }
            else
            {
                // We cancel the move
                // Here we could have a visual animation if drop point is far enough from starting point to make clear that the move was rejected
                movingGroup.SetTopLeft(StartingPoint);
            }
        }


        private void ClickOnGroup(MovingGroup movingGroup)
        {
            //Debug.WriteLine("ClickOnGroup");
            if (movingGroup.FromStack is TalonFaceDownStack)
            {
                if (movingGroup.MovingCards is null)
                {
                    // It's a talon reset
                    TalonFD.ResetTalon(TalonFU);
                }
                else
                {
                    // We move a single card from TalonFD to TalonFU
                    // Note: the card in movingGroup.MovingCards has already be turned face up and put on top of visual stack
                    movingGroup.ToStack = TalonFU;
                    movingGroup.DoMove();
                }
                return;
            }
        }


        private void DoubleClickOnGroup(MovingGroup movingGroup)
        {
            //Debug.WriteLine("DoubleClickOnGroup");
            // ToDo: Automatic move mechanisms
            // But I'm thinking it could be better on single click?  To test and decide.

            // First check if we can move to a base
            if (movingGroup.MovingCards.Count == 1)
            {
                var baseStack = GetCompatibleBaseStack(movingGroup.MovingCards[0]);
                if (baseStack != null)
                {
                    movingGroup.ToStack = baseStack;
                    movingGroup.DoMove();
                    return;
                }
            }

            // If we try to move more than 1 card, check that they go in deceasing value order and alternate color
            for (int i = 1; i < movingGroup.MovingCards.Count; i++)
                if (movingGroup.MovingCards[i].Value != 1 + movingGroup.MovingCards[i - 1].Value
                    || movingGroup.MovingCards[i].Value % 2 == movingGroup.MovingCards[i - 1].Value % 2)
                    return;

            // Look if we have another column that can accept it
            for (int i = 0; i < 7; i++)
            {
                if (movingGroup.MovingCards[movingGroup.MovingCards.Count - 1].Value == 13 && Columns[i].PlayingCards.Count == 0)
                {
                    movingGroup.ToStack = Columns[i];
                    movingGroup.DoMove();
                    return;
                }

                if (Columns[i].PlayingCards.Count > 0
                    && Columns[i].PlayingCards[0].Value == movingGroup.MovingCards[movingGroup.MovingCards.Count - 1].Value + 1
                    && Columns[i].PlayingCards[0].Color % 2 != movingGroup.MovingCards[movingGroup.MovingCards.Count - 1].Color % 2)
                {
                    movingGroup.ToStack = Columns[i];
                    movingGroup.DoMove();
                    return;
                }
            }

        }

        private BaseStack GetCompatibleBaseStack(PlayingCard playingCard)
        {
            if (playingCard.Value == 1)
            {
                for (int i = 0; i < 4; i++)
                    if (Bases[i].PlayingCards.Count == 0)
                        return Bases[i];
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    if (Bases[i].PlayingCards.Count > 0)
                        if (Bases[i].PlayingCards[0].Color == playingCard.Color && Bases[i].PlayingCards[0].Value + 1 == playingCard.Value)
                            return Bases[i];

            }
            return null;
        }



        // Just to test if mouse coordinates transformations are correct
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
