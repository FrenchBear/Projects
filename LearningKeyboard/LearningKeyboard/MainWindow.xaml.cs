// Learning Keyboard
// Visual Keyboard to learn typing
// 2017-09-18   PV
// 2017-10-20   PV      TextRendering and TextFormatting
// 2017-12-22   PV      1.1.1 AlwaysOnTop option

using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using static LearningKeyboard.CharFromKey;


namespace LearningKeyboard
{
    public partial class MainWindow : Window
    {
        private IKeyboardMouseEvents m_GlobalHook;

        private bool shift;
        private bool control;
        private bool alt;

        public MainWindow()
        {
            InitializeComponent();

            TextFormatting = (string)Properties.Settings.Default["TextFormatting"];
            TextRendering = (string)Properties.Settings.Default["TextRendering"];
            ApplyWPFTextOptions();

            DrawKeyboard();
            PrepareDicraticalCombinations();
            ApplyColorScheme();
            Subscribe();

            AlwaysOnTop = (bool)Properties.Settings.Default["AlwaysOnTop"];
            Activated += (s, e) => { if (AlwaysOnTop) Topmost = true; };
            Deactivated += (s, e) => { if (AlwaysOnTop) { Topmost = true; Activate(); } };
        }

        private void ApplyWPFTextOptions()
        {
            switch (TextFormatting)
            {
                case "Ideal": MyMainWindow.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Ideal); break;
                case "Display": MyMainWindow.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display); break;
            }
            switch (TextRendering)
            {
                case "Auto": MyMainWindow.SetValue(TextOptions.TextRenderingModeProperty, TextRenderingMode.Auto); break;
                case "Aliased": MyMainWindow.SetValue(TextOptions.TextRenderingModeProperty, TextRenderingMode.Aliased); break;
                case "Grayscale": MyMainWindow.SetValue(TextOptions.TextRenderingModeProperty, TextRenderingMode.Grayscale); break;
                case "ClearType": MyMainWindow.SetValue(TextOptions.TextRenderingModeProperty, TextRenderingMode.ClearType); break;
            }
        }

        private Dictionary<Tuple<Keys, bool, bool>, List<Tuple<string, string>>> dicCombi = new Dictionary<Tuple<Keys, bool, bool>, List<Tuple<string, string>>>();

        private void PrepareDicraticalCombinations()
        {
            foreach (var key in AllKeys.Values)
            {
                if (key.IsNormalDeadKey) PrepareDicraticalCombinationsOneKey(key.vk, false, false);
                if (key.IsShiftDeadKey) PrepareDicraticalCombinationsOneKey(key.vk, true, false);
                if (key.IsAltGrDeadKey) PrepareDicraticalCombinationsOneKey(key.vk, false, true);
                if (key.IsShiftAltGrDeadKey) PrepareDicraticalCombinationsOneKey(key.vk, true, true);
            }
        }

        private void PrepareDicraticalCombinationsOneKey(Keys vkDic, bool shiftDic, bool altGrDic)
        {
            var combinations = GetDeadKeyCombinations(vkDic, shiftDic, altGrDic);
            var dki = new Tuple<Keys, bool, bool>(vkDic, shiftDic, altGrDic);
            dicCombi.Add(dki, combinations);
        }


        public void Subscribe()
        {
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.KeyDown += M_GlobalHook_KeyDown;
            m_GlobalHook.KeyUp += M_GlobalHook_KeyUp;
        }

        private void M_GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e is KeyEventArgsExt e2)
            {
                int scanCode = e2.ScanCode;
                if (e.KeyCode == Keys.RControlKey) scanCode += 300;
                else if (e.KeyCode == Keys.RMenu) scanCode = 0;
                UpdateModifiers(e.KeyCode, true);

                if (AllKeys.ContainsKey(scanCode))
                    AllKeys[scanCode].IsPressed = true;

                var dki = new Tuple<Keys, bool, bool>(e.KeyCode, e.Shift, e.Alt && e.Control);
                if (dicCombi.ContainsKey(dki))
                {
                    CombinationsWrapPanel.Children.Clear();
                    foreach (var item in dicCombi[dki])
                    {
                        var t1 = new TextBlock
                        {
                            Width = 14,
                            TextAlignment = TextAlignment.Center,
                            Text = item.Item1
                        };
                        var t2 = new TextBlock
                        {
                            Width = 16,
                            TextAlignment = TextAlignment.Center,
                            Text = item.Item2,
                            FontWeight = FontWeights.Bold
                        };
                        var sp = new StackPanel
                        {
                            Orientation = System.Windows.Controls.Orientation.Horizontal,
                            Margin = new Thickness(0, 0, 4, 0)
                        };
                        sp.Children.Add(t1);
                        sp.Children.Add(t2);
                        CombinationsWrapPanel.Children.Add(sp);
                    }
                }
                else
                {
                    CombinationsWrapPanel.Children.Clear();
                }
            }
        }

        private void M_GlobalHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e is KeyEventArgsExt e2)
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

                NewKey.KeyState ks = NewKey.KeyState.Normal;
                if (shift && !(control && alt))
                    ks = NewKey.KeyState.Shift;
                else if ((!shift) && control && alt)
                    ks = NewKey.KeyState.AltGr;
                else if (shift && control && alt)
                    ks = NewKey.KeyState.ShiftAltGr;

                foreach (var k in AllKeys.Values)
                {
                    k.NewKeyState = ks;
                }
            }
        }

        private string ColorScheme;
        private string TextRendering;
        private string TextFormatting;
        private bool AlwaysOnTop;

        private void DrawKeyboard()
        {
            ColorScheme = (string)Properties.Settings.Default["ColorScheme"];

            const int bo = 5;       // border margin
            const int m = 40;       // width/height of a square key
            const int sp = 1;       // horizontal/vertical space between keys
            const int ms = 4;       // middle separator

            int rm = bo + 15 * m + 14 * sp + ms;     // Right margin

            int xc = 0, yc = 0;
            Func<int, int> nextX = (int w) => { int xs = xc; xc += w + sp; return xs; };

            xc = bo;
            yc = bo;
            AddKey("E00", 41, nextX(m), yc, m, m, "L5", NewKey.NewKeyStyle.Normal);                    // ²
            AddKey("E01", 2, nextX(m), yc, m, m, "L5", NewKey.NewKeyStyle.Normal);                     // & 1
            AddKey("E02", 3, nextX(m), yc, m, m, "L4", NewKey.NewKeyStyle.Normal);                     // é 2 ~
            AddKey("E03", 4, nextX(m), yc, m, m, "L3", NewKey.NewKeyStyle.Normal);                     // " 3 #
            AddKey("E04", 5, nextX(m), yc, m, m, "L2", NewKey.NewKeyStyle.Normal);                     // 4 ' {
            AddKey("E05", 6, nextX(m), yc, m, m, "L2", NewKey.NewKeyStyle.Normal);                     // 5 ( [
            xc += ms;
            AddKey("E06", 7, nextX(m), yc, m, m, "R2", NewKey.NewKeyStyle.Normal);                     // 6 - |
            AddKey("E07", 8, nextX(m), yc, m, m, "R2", NewKey.NewKeyStyle.Normal);                     // 7 è `
            AddKey("E08", 9, nextX(m), yc, m, m, "R3", NewKey.NewKeyStyle.Normal);                     // 8 _ \
            AddKey("E09", 10, nextX(m), yc, m, m, "R4", NewKey.NewKeyStyle.Normal);                    // 9 ç ^
            AddKey("E10", 11, nextX(m), yc, m, m, "R5", NewKey.NewKeyStyle.Normal);                    // 0 à @
            AddKey("E11", 12, nextX(m), yc, m, m, "R5", NewKey.NewKeyStyle.Normal);                    // ° ) ]
            AddKey("E12", 13, nextX(m), yc, m, m, "R5", NewKey.NewKeyStyle.Normal);                    // + = }
            AddKey("E13", 14, xc, yc, rm - xc, m, "R5", NewKey.NewKeyStyle.Static, "\u232B");          // Backspace

            xc = bo;
            yc += m + sp;
            AddKey("D00", 15, nextX((int)(1.5 * m)), yc, (int)(1.5 * m), m, "L5", NewKey.NewKeyStyle.Static, "Tab");
            AddKey("D01", 16, nextX(m), yc, m, m, "L5", NewKey.NewKeyStyle.Simple);                    // a
            AddKey("D02", 17, nextX(m), yc, m, m, "L4", NewKey.NewKeyStyle.Simple);                    // z
            AddKey("D03", 18, nextX(m), yc, m, m, "L3", NewKey.NewKeyStyle.Simple);                    // e €
            AddKey("D04", 19, nextX(m), yc, m, m, "L2", NewKey.NewKeyStyle.Simple);                    // r
            AddKey("D05", 20, nextX(m), yc, m, m, "L2", NewKey.NewKeyStyle.Simple);                    // t
            xc += ms;
            AddKey("D06", 21, nextX(m), yc, m, m, "R2", NewKey.NewKeyStyle.Simple);                    // y
            AddKey("D07", 22, nextX(m), yc, m, m, "R2", NewKey.NewKeyStyle.Simple);                    // u
            AddKey("D08", 23, nextX(m), yc, m, m, "R3", NewKey.NewKeyStyle.Simple);                    // i
            AddKey("D09", 24, nextX(m), yc, m, m, "R4", NewKey.NewKeyStyle.Simple);                    // o
            AddKey("D10", 25, nextX(m), yc, m, m, "R5", NewKey.NewKeyStyle.Simple);                    // p
            AddKey("D11", 26, nextX(m), yc, m, m, "R5", NewKey.NewKeyStyle.Normal);                    // ^ ¨
            AddKey("D12", 27, nextX(m), yc, m, m, "R5", NewKey.NewKeyStyle.Normal);                    // $ £ ¤

            xc = bo;
            yc += m + sp;
            AddKey("C00", 58, nextX((int)(1.75 * m)), yc, (int)(1.75 * m), m, "L5", NewKey.NewKeyStyle.Static, "CapsLock");
            AddKey("C01", 30, nextX(m), yc, m, m, "L5", NewKey.NewKeyStyle.Simple);                    // q
            AddKey("C02", 31, nextX(m), yc, m, m, "L4", NewKey.NewKeyStyle.Simple);                    // s
            AddKey("C03", 32, nextX(m), yc, m, m, "L3", NewKey.NewKeyStyle.Simple);                    // d
            AddKey("C04", 33, nextX(m), yc, m, m, "L2", NewKey.NewKeyStyle.Simple);                    // f
            AddKey("C05", 34, nextX(m), yc, m, m, "L2", NewKey.NewKeyStyle.Simple);                    // g
            xc += ms;
            AddKey("C06", 35, nextX(m), yc, m, m, "R2", NewKey.NewKeyStyle.Simple);                    // h
            AddKey("C07", 36, nextX(m), yc, m, m, "R2", NewKey.NewKeyStyle.Simple);                    // j
            AddKey("C08", 37, nextX(m), yc, m, m, "R3", NewKey.NewKeyStyle.Simple);                    // k
            AddKey("C09", 38, nextX(m), yc, m, m, "R4", NewKey.NewKeyStyle.Simple);                    // l
            AddKey("C10", 39, nextX(m), yc, m, m, "R5", NewKey.NewKeyStyle.Simple);                    // m
            AddKey("C11", 40, nextX(m), yc, m, m, "R5", NewKey.NewKeyStyle.Normal);                    // ù %
            AddKey("C12", 43, nextX(m), yc, m, m, "R5", NewKey.NewKeyStyle.Normal);                    // * µ
            AddKey("C13", 28, xc, yc - m - sp, rm - xc, 2 * m + sp, "R5", NewKey.NewKeyStyle.Static, "Enter");

            xc = bo;
            yc += m + sp;
            AddKey("BL", 42, nextX((int)(1.125 * m)), yc, (int)(1.125 * m), m, "", NewKey.NewKeyStyle.Static, "Shift");
            AddKey("B00", 86, nextX(m), yc, m, m, "L5", NewKey.NewKeyStyle.Normal);                    // < >
            AddKey("B01", 44, nextX(m), yc, m, m, "L5", NewKey.NewKeyStyle.Simple);                    // w
            AddKey("B02", 45, nextX(m), yc, m, m, "L4", NewKey.NewKeyStyle.Simple);                    // x
            AddKey("B03", 46, nextX(m), yc, m, m, "L3", NewKey.NewKeyStyle.Simple);                    // c
            AddKey("B04", 47, nextX(m), yc, m, m, "L2", NewKey.NewKeyStyle.Simple);                    // v
            AddKey("B05", 48, nextX(m), yc, m, m, "L2", NewKey.NewKeyStyle.Simple);                    // b
            xc += ms;
            AddKey("B06", 49, nextX(m), yc, m, m, "R2", NewKey.NewKeyStyle.Simple);                    // n
            AddKey("B07", 50, nextX(m), yc, m, m, "R2", NewKey.NewKeyStyle.Normal);                    // , ?
            AddKey("B08", 51, nextX(m), yc, m, m, "R3", NewKey.NewKeyStyle.Normal);                    // ; .
            AddKey("B09", 52, nextX(m), yc, m, m, "R4", NewKey.NewKeyStyle.Normal);                    // : /
            AddKey("B10", 53, nextX(m), yc, m, m, "R5", NewKey.NewKeyStyle.Normal);                    // ! §
            AddKey("B11", 54, xc, yc, rm - xc, m, "", NewKey.NewKeyStyle.Static, "Shift");

            xc = bo;
            yc += m + sp;
            AddKey("A00", 29, nextX((int)(1.35 * m)), yc, (int)(1.35 * m), m, "", NewKey.NewKeyStyle.Static, "Ctrl");
            AddKey("A01", 91, nextX((int)(1.35 * m)), yc, (int)(1.35 * m), m, "", NewKey.NewKeyStyle.Static, "W");
            AddKey("A02", 56, nextX((int)(1.35 * m)), yc, (int)(1.35 * m), m, "", NewKey.NewKeyStyle.Static, "Alt");
            AddKey("A03", 57, nextX((int)(5.8 * m)), yc, (int)(5.8 * m), m, "", NewKey.NewKeyStyle.Static, "Space");
            AddKey("A04", 541, nextX((int)(1.35 * m)), yc, (int)(1.35 * m), m, "", NewKey.NewKeyStyle.Static, "Alt Gr");
            AddKey("A05", 92, nextX((int)(1.35 * m)), yc, (int)(1.35 * m), m, "", NewKey.NewKeyStyle.Static, "W");
            AddKey("A06", 93, nextX((int)(1.35 * m)), yc, (int)(1.35 * m), m, "", NewKey.NewKeyStyle.Static, "Menu");
            AddKey("A07", 329, xc, yc, rm - xc, m, "", NewKey.NewKeyStyle.Static, "Ctrl");

            MyCanvas.Width = rm + 2 * bo + 17;
            MyCanvas.Height = 2 * bo + 5 * m + 4 * sp;
            CombinationsWrapPanel.Height = MyCanvas.Height;

            CloseButton.SetValue(Canvas.TopProperty, (double)bo);
            CloseButton.SetValue(Canvas.LeftProperty, (double)MyCanvas.Width - bo - CloseButton.Width);
            AboutButton.SetValue(Canvas.TopProperty, (double)bo + 4 + CloseButton.Height);
            AboutButton.SetValue(Canvas.LeftProperty, (double)MyCanvas.Width - bo - CloseButton.Width);
            SettingsButton.SetValue(Canvas.TopProperty, (double)bo + 2 * (4 + AboutButton.Height));
            SettingsButton.SetValue(Canvas.LeftProperty, (double)MyCanvas.Width - bo - SettingsButton.Width);
        }


        private static Brush DarkerBrush = new SolidColorBrush(Color.FromArgb(64, 40, 40, 40));

        private void ApplyColorScheme()
        {
            ResourceDictionary rd = (Resources?[ColorScheme] ?? Resources["Brown"]) as ResourceDictionary;

            foreach (KeyboardKey k in AllKeys.Values)
            {
                if (rd.Contains(k.digit))
                {
                    k.NormalBackground = rd[k.digit] as Brush;
                }
                else
                {
                    k.NormalBackground = Brushes.Black;
                    k.NormalForeground = Brushes.White;
                }

                if (k.IsNormalDeadKey) k.NormalTextBackground = DarkerBrush;
                if (k.IsShiftDeadKey) k.ShiftTextBackground = DarkerBrush;
                if (k.IsAltGrDeadKey) k.AltGrTextBackground = DarkerBrush;
                if (k.IsShiftAltGrDeadKey) k.ShiftAltGrTextBackground = DarkerBrush;
            }
        }


        private void AddKey(string dispoNF, int keyCode, int p1X, int p1Y, int w, int h, string digit, NewKey.NewKeyStyle style, string simpleTextOverride = "")
        {
            KeyboardKey k = new KeyboardKey(dispoNF, keyCode, digit, style, simpleTextOverride, w, h);
            //{
            //    Width = w,
            //    Height = h
            //};
            // Special color for F and J keys to replace physical bump
            if (keyCode == 33 || keyCode == 36)
                k.NormalForeground = Brushes.Red;
            // Special font for Windows keys
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
            var st = new SettingsWindow(ColorScheme, TextFormatting, TextRendering, AlwaysOnTop);
            if (st.ShowDialog().Value)
            {
                if (st.ColorScheme != ColorScheme)
                {
                    ColorScheme = st.ColorScheme;
                    ApplyColorScheme();
                }
                if (st.TextFormatting != TextFormatting || st.TextRendering != TextRendering)
                {
                    TextFormatting = st.TextFormatting;
                    TextRendering = st.TextRendering;
                    ApplyWPFTextOptions();
                }
                AlwaysOnTop = st.AlwaysOnTop;
                Topmost = AlwaysOnTop;
            }
        }
    }
}