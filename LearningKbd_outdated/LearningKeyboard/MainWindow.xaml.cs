// Learning Keyboard
// Visual Keyboard to learn typing 
// 2017-09-18   PV


using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

namespace LearningKeyboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IKeyboardMouseEvents m_GlobalHook;

        private bool shift;
        private bool control;
        private bool alt;

        public MainWindow()
        {
            InitializeComponent();
            DrawKeyboard();
            Subscribe();

            Activated += (s, e) => { Topmost = true; };
            Deactivated += (s, e) => { Topmost = true; Activate(); };
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Subscribe()
        {
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.KeyDown += M_GlobalHook_KeyDown;
            m_GlobalHook.KeyUp += M_GlobalHook_KeyUp;
        }

        private void M_GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            KeyEventArgsExt e2 = e as KeyEventArgsExt;
            if (e2 != null)
            {
                int scanCode = e2.ScanCode;
                if (e.KeyCode == Keys.RControlKey) scanCode += 300;
                else if (e.KeyCode == Keys.RMenu) scanCode = 0;
                UpdateModifiers(e.KeyCode, true);

                if (AllKeys.ContainsKey(scanCode))
                    AllKeys[scanCode].IsPressed = true;
            }
        }

        private void M_GlobalHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            KeyEventArgsExt e2 = e as KeyEventArgsExt;
            if (e2 != null)
            {
                int scanCode = e2.ScanCode;
                if (e.KeyCode == Keys.RControlKey) scanCode += 300;
                else if (e.KeyCode == Keys.RMenu) scanCode = 0;
                UpdateModifiers(e.KeyCode, false);

                if (e.KeyCode == Keys.CapsLock || e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
                    // 58 = ScanCode of CapsLock
                    AllKeys[58].IsPressed = System.Windows.Forms.Control.IsKeyLocked(Keys.CapsLock);

                if (e.KeyCode != Keys.CapsLock && AllKeys.ContainsKey(scanCode))
                    AllKeys[scanCode].IsPressed = false;
            }
        }

        private void UpdateModifiers(Keys KeyCode, bool isPressed)
        {
            var m_shift = shift;
            var m_control = control;
            var m_alt = alt;

            if (KeyCode == Keys.CapsLock) m_shift = System.Windows.Forms.Control.IsKeyLocked(Keys.CapsLock);
            if (KeyCode == Keys.LShiftKey || KeyCode == Keys.RShiftKey) m_shift = isPressed;
            if (KeyCode == Keys.LControlKey || KeyCode == Keys.RControlKey) m_control = isPressed;
            if (KeyCode == Keys.LMenu || KeyCode == Keys.RMenu) m_alt = isPressed;

            if (m_shift != shift || m_control != control || m_alt != alt)
            {
                shift = m_shift;
                control = m_control;
                alt = m_alt;

                NewKey.NewKeyStates ks = NewKey.NewKeyStates.Normal;
                if (shift && !(control && alt))
                    ks = NewKey.NewKeyStates.Shift;
                else if ((!shift) && control && alt)
                    ks = NewKey.NewKeyStates.AltGr;
                else if (shift && control && alt)
                    ks = NewKey.NewKeyStates.ShiftAltGr;

                foreach (var k in AllKeys.Values)
                    k.NewKeyState = ks;
            }

        }

        private void DrawKeyboard()
        {
            const int bo = 5;       // border margin
            const int m = 40;       // width/height of a square key
            const int sp = 1;       // horizontal/vertical space between keys
            const int ms = 4;       // middle separator

            int rm = bo + 15 * m + 14 * sp + ms;     // Right margin

            int xc = 0, yc = 0;
            Func<int, int> nextX = (int w) => { int xs = xc; xc += w + sp; return xs; };

            xc = bo;
            yc = bo;
            AddKey(41, nextX(m), yc, m, m, "L5", NewKey.KeyStyles.Normal);
            AddKey(2, nextX(m), yc, m, m, "L5", NewKey.KeyStyles.Normal);
            AddKey(3, nextX(m), yc, m, m, "L4", NewKey.KeyStyles.Normal);
            AddKey(4, nextX(m), yc, m, m, "L3", NewKey.KeyStyles.Normal);
            AddKey(5, nextX(m), yc, m, m, "L2", NewKey.KeyStyles.Normal);
            AddKey(6, nextX(m), yc, m, m, "L2", NewKey.KeyStyles.Normal);
            xc += ms;
            AddKey(7, nextX(m), yc, m, m, "R2", NewKey.KeyStyles.Normal);
            AddKey(8, nextX(m), yc, m, m, "R2", NewKey.KeyStyles.Normal);
            AddKey(9, nextX(m), yc, m, m, "R3", NewKey.KeyStyles.Normal);
            AddKey(10, nextX(m), yc, m, m, "R4", NewKey.KeyStyles.Normal);
            AddKey(11, nextX(m), yc, m, m, "R5", NewKey.KeyStyles.Normal);
            AddKey(12, nextX(m), yc, m, m, "R5", NewKey.KeyStyles.Normal);
            AddKey(13, nextX(m), yc, m, m, "R5", NewKey.KeyStyles.Normal);
            AddKey(14, xc, yc, rm - xc, m, "R5", NewKey.KeyStyles.Static, "\u232B");

            xc = bo;
            yc += m + sp;
            AddKey(15, nextX((int)(1.5 * m)), yc, (int)(1.5 * m), m, "L5", NewKey.KeyStyles.Static, "Tab");
            AddKey(16, nextX(m), yc, m, m, "L5", NewKey.KeyStyles.Simple);
            AddKey(17, nextX(m), yc, m, m, "L4", NewKey.KeyStyles.Simple);
            AddKey(18, nextX(m), yc, m, m, "L3", NewKey.KeyStyles.Simple);
            AddKey(19, nextX(m), yc, m, m, "L2", NewKey.KeyStyles.Simple);
            AddKey(20, nextX(m), yc, m, m, "L2", NewKey.KeyStyles.Simple);
            xc += ms;
            AddKey(21, nextX(m), yc, m, m, "R2", NewKey.KeyStyles.Simple);
            AddKey(22, nextX(m), yc, m, m, "R2", NewKey.KeyStyles.Simple);
            AddKey(23, nextX(m), yc, m, m, "R3", NewKey.KeyStyles.Simple);
            AddKey(24, nextX(m), yc, m, m, "R4", NewKey.KeyStyles.Simple);
            AddKey(25, nextX(m), yc, m, m, "R5", NewKey.KeyStyles.Simple);
            AddKey(26, nextX(m), yc, m, m, "R5", NewKey.KeyStyles.Normal);
            AddKey(27, nextX(m), yc, m, m, "R5", NewKey.KeyStyles.Normal);

            xc = bo;
            yc += m + sp;
            AddKey(58, nextX((int)(1.75 * m)), yc, (int)(1.75 * m), m, "L5", NewKey.KeyStyles.Static, "CapsLock");
            AddKey(30, nextX(m), yc, m, m, "L5", NewKey.KeyStyles.Simple);
            AddKey(31, nextX(m), yc, m, m, "L4", NewKey.KeyStyles.Simple);
            AddKey(32, nextX(m), yc, m, m, "L3", NewKey.KeyStyles.Simple);
            AddKey(33, nextX(m), yc, m, m, "L2", NewKey.KeyStyles.Simple);
            AddKey(34, nextX(m), yc, m, m, "L2", NewKey.KeyStyles.Simple);
            xc += ms;
            AddKey(35, nextX(m), yc, m, m, "R2", NewKey.KeyStyles.Simple);
            AddKey(36, nextX(m), yc, m, m, "R2", NewKey.KeyStyles.Simple);
            AddKey(37, nextX(m), yc, m, m, "R3", NewKey.KeyStyles.Simple);
            AddKey(38, nextX(m), yc, m, m, "R4", NewKey.KeyStyles.Simple);
            AddKey(39, nextX(m), yc, m, m, "R5", NewKey.KeyStyles.Simple);
            AddKey(40, nextX(m), yc, m, m, "R5", NewKey.KeyStyles.Normal);
            AddKey(43, nextX(m), yc, m, m, "R5", NewKey.KeyStyles.Normal);
            AddKey(28, xc, yc - m - sp, rm - xc, 2 * m + sp, "R5", NewKey.KeyStyles.Static, "Enter");

            xc = bo;
            yc += m + sp;
            AddKey(42, nextX((int)(1.125 * m)), yc, (int)(1.125 * m), m, "", NewKey.KeyStyles.Static, "Shift");
            AddKey(86, nextX(m), yc, m, m, "L5", NewKey.KeyStyles.Normal);
            AddKey(44, nextX(m), yc, m, m, "L5", NewKey.KeyStyles.Simple);
            AddKey(45, nextX(m), yc, m, m, "L4", NewKey.KeyStyles.Simple);
            AddKey(46, nextX(m), yc, m, m, "L3", NewKey.KeyStyles.Simple);
            AddKey(47, nextX(m), yc, m, m, "L2", NewKey.KeyStyles.Simple);
            AddKey(48, nextX(m), yc, m, m, "L2", NewKey.KeyStyles.Simple);
            xc += ms;
            AddKey(49, nextX(m), yc, m, m, "R2", NewKey.KeyStyles.Simple);
            AddKey(50, nextX(m), yc, m, m, "R2", NewKey.KeyStyles.Normal);
            AddKey(51, nextX(m), yc, m, m, "R3", NewKey.KeyStyles.Normal);
            AddKey(52, nextX(m), yc, m, m, "R4", NewKey.KeyStyles.Normal);
            AddKey(53, nextX(m), yc, m, m, "R5", NewKey.KeyStyles.Normal);
            AddKey(54, xc, yc, rm - xc, m, "", NewKey.KeyStyles.Static, "Shift");

            xc = bo;
            yc += m + sp;
            AddKey(29, nextX((int)(1.35 * m)), yc, (int)(1.35 * m), m, "", NewKey.KeyStyles.Static, "Ctrl");
            AddKey(91, nextX((int)(1.35 * m)), yc, (int)(1.35 * m), m, "", NewKey.KeyStyles.Static, "W");
            AddKey(56, nextX((int)(1.35 * m)), yc, (int)(1.35 * m), m, "", NewKey.KeyStyles.Static, "Alt");
            AddKey(57, nextX((int)(5.8 * m)), yc, (int)(5.8 * m), m, "", NewKey.KeyStyles.Static, "Space");
            AddKey(541, nextX((int)(1.35 * m)), yc, (int)(1.35 * m), m, "", NewKey.KeyStyles.Static, "Alt Gr");
            AddKey(92, nextX((int)(1.35 * m)), yc, (int)(1.35 * m), m, "", NewKey.KeyStyles.Static, "W");
            AddKey(93, nextX((int)(1.35 * m)), yc, (int)(1.35 * m), m, "", NewKey.KeyStyles.Static, "Menu");
            AddKey(329, xc, yc, rm - xc, m, "", NewKey.KeyStyles.Static, "Ctrl");

            MyCanvas.Width = rm + 2 * bo + 17;
            MyCanvas.Height = 2 * bo + 5 * m + 4 * sp;

            CloseButton.SetValue(Canvas.TopProperty, (double)bo);
            CloseButton.SetValue(Canvas.LeftProperty, (double)MyCanvas.Width - bo - CloseButton.Width);
            AboutButton.SetValue(Canvas.TopProperty, (double)bo + 4 + CloseButton.Height);
            AboutButton.SetValue(Canvas.LeftProperty, (double)MyCanvas.Width - bo - CloseButton.Width);
            SettingsButton.SetValue(Canvas.TopProperty, (double)bo + 2 * (4 + AboutButton.Height));
            SettingsButton.SetValue(Canvas.LeftProperty, (double)MyCanvas.Width - bo - SettingsButton.Width);

        }

        Dictionary<int, KeyboardKey> AllKeys = new Dictionary<int, KeyboardKey>();

        //Dictionary<string, Brush> DigitBrushes = new Dictionary<string, Brush>
        //{
        //    {"L5", Brushes.DarkSalmon },
        //    {"L4", Brushes.Tan },
        //    {"L3", Brushes.Wheat },
        //    {"L2", Brushes.LightGoldenrodYellow },
        //    {"R2", Brushes.Khaki },
        //    {"R3", Brushes.Wheat },
        //    {"R4", Brushes.Tan },
        //    {"R5", Brushes.DarkSalmon }
        //};

        private void AddKey(int keyCode, int p1X, int p1Y, int w, int h, string digit, NewKey.KeyStyles style, string simpleTextOverride = "")
        {
            string colorScheme = (string)Properties.Settings.Default["ColorScheme"];
            ResourceDictionary rd = (Resources?[colorScheme] ?? Resources["Brown"])  as ResourceDictionary;


            KeyboardKey k = new KeyboardKey(keyCode, w, h, style, simpleTextOverride);
            if (rd.Contains(digit))
            {
                k.NormalBackground = rd[digit] as Brush;
            }
            else
            {
                k.NormalBackground = Brushes.Black;
                k.NormalForeground = Brushes.White;
            }
            if (keyCode == 33 || keyCode == 36)
                k.NormalForeground = Brushes.Red;
            if (keyCode == 91 || keyCode == 92)
                k.SimpleTextTB.FontFamily = new FontFamily("Marlett");
            k.SetValue(Canvas.TopProperty, (double)p1Y);
            k.SetValue(Canvas.LeftProperty, (double)p1X);
            MyCanvas.Children.Add(k);
            AllKeys.Add(keyCode, k);
        }

        // The window can be moved from gray border
        private void MyCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var ab = new AboutWindow();
            ab.ShowDialog();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var st = new SettingsWindow();
            st.ShowDialog();
        }
    }
}