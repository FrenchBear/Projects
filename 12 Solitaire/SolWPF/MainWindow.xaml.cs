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
        private GameDataBag b;

        private IEnumerable<GameStack> AllStacks()
        {
            foreach (var gs in b.Bases)
                yield return gs;
            foreach (var gs in b.Columns)
                yield return gs;
            yield return b.TalonFU;
            yield return b.TalonFD;
        }


        public MainWindow()
        {
            InitializeComponent();
            b = new GameDataBag();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            b.Bases = new BaseStack[4];
            b.Bases[0] = new BaseStack(b, "Base0", PlayingCanvas, Base0);
            b.Bases[1] = new BaseStack(b, "Base1", PlayingCanvas, Base1);
            b.Bases[2] = new BaseStack(b, "Base2", PlayingCanvas, Base2);
            b.Bases[3] = new BaseStack(b, "Base3", PlayingCanvas, Base3);

            b.Columns = new ColumnStack[7];
            b.Columns[0] = new ColumnStack(b, "Column0", PlayingCanvas, Column0);
            b.Columns[1] = new ColumnStack(b, "Column1", PlayingCanvas, Column1);
            b.Columns[2] = new ColumnStack(b, "Column2", PlayingCanvas, Column2);
            b.Columns[3] = new ColumnStack(b, "Column3", PlayingCanvas, Column3);
            b.Columns[4] = new ColumnStack(b, "Column4", PlayingCanvas, Column4);
            b.Columns[5] = new ColumnStack(b, "Column5", PlayingCanvas, Column5);
            b.Columns[6] = new ColumnStack(b, "Column6", PlayingCanvas, Column6);

            b.TalonFD = new TalonFaceDownStack(b, "TalonFD", PlayingCanvas, Talon0);
            b.TalonFU = new TalonFaceUpStack(b, "TalonFU", PlayingCanvas, Talon1);

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
                    b.Columns[c].AddCard(s, i == c);
                }
            for (int mt = 0; mt < lc.Count; mt++)     // 3 = Temp for testing talon rotation with a 3-card talon
            {
                b.TalonFD.AddCard(lc[mt], false);
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
        private MovingGroup movingGroup;    // Current interactive moving group of cards


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
                    b.TalonFD.ResetTalon(b.TalonFU);
                }
                else
                {
                    // We move a single card from TalonFD to TalonFU
                    // Note: the card in movingGroup.MovingCards has already be turned face up and put on top of visual stack
                    movingGroup.ToStack = b.TalonFU;
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
                if (movingGroup.MovingCards[movingGroup.MovingCards.Count - 1].Value == 13 && b.Columns[i].PlayingCards.Count == 0)
                {
                    movingGroup.ToStack = b.Columns[i];
                    movingGroup.DoMove();
                    return;
                }

                if (b.Columns[i].PlayingCards.Count > 0
                    && b.Columns[i].PlayingCards[0].Value == movingGroup.MovingCards[movingGroup.MovingCards.Count - 1].Value + 1
                    && b.Columns[i].PlayingCards[0].Color % 2 != movingGroup.MovingCards[movingGroup.MovingCards.Count - 1].Color % 2)
                {
                    movingGroup.ToStack = b.Columns[i];
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
                    if (b.Bases[i].PlayingCards.Count == 0)
                        return b.Bases[i];
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    if (b.Bases[i].PlayingCards.Count > 0)
                        if (b.Bases[i].PlayingCards[0].Color == playingCard.Color && b.Bases[i].PlayingCards[0].Value + 1 == playingCard.Value)
                            return b.Bases[i];

            }
            return null;
        }

        private void UndoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = b.UndoStack.Count > 0;
        }

        private void UndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Undo!");
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
