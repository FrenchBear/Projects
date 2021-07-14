// NewKeyTestApp
// Simple code to valudate NewKey User Control
// 2017-09-19   PV


using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NewKeyControl;
using System.Collections.Generic;


namespace NewKeyTestApp
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Dictionary<Key, NewKey> keysDictionary = new Dictionary<Key, NewKey>();

        public MainWindow()
        {
            InitializeComponent();

            keysDictionary.Add(Key.D0, KeyD0);
            keysDictionary.Add(Key.R, KeyR);
            keysDictionary.Add(Key.Enter, KeyEnter);
            keysDictionary.Add(Key.Space, KeySpace);

            foreach (var k in keysDictionary.Values)
                k.NewKeyState= NewKey.NewKeyStates.Normal;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            OnKeyDownOrUp();
            if (keysDictionary.ContainsKey(e.Key))
                keysDictionary[e.Key].Pressed();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            OnKeyDownOrUp();
            if (keysDictionary.ContainsKey(e.Key))
                keysDictionary[e.Key].Released();
        }

        bool shift, ctrl, alt;

        private void OnKeyDownOrUp()
        {
            var m_shift = System.Windows.Input.Keyboard.IsKeyDown(Key.LeftShift) || System.Windows.Input.Keyboard.IsKeyDown(Key.RightShift) || Keyboard.GetKeyStates(Key.CapsLock) == KeyStates.Toggled;
            var m_ctrl = System.Windows.Input.Keyboard.IsKeyDown(Key.LeftCtrl) || System.Windows.Input.Keyboard.IsKeyDown(Key.RightCtrl);
            var m_alt = System.Windows.Input.Keyboard.IsKeyDown(Key.LeftAlt) || System.Windows.Input.Keyboard.IsKeyDown(Key.RightAlt);

            if (m_shift != shift || m_ctrl != ctrl || m_alt != alt)
            {
                shift = m_shift;
                ctrl = m_ctrl;
                alt = m_alt;

                NewKey.NewKeyStates ks = NewKey.NewKeyStates.Normal;
                if (shift && !(ctrl && alt))
                    ks = NewKey.NewKeyStates.Shift;
                else if ((!shift) && ctrl && alt)
                    ks = NewKey.NewKeyStates.AltGr;
                else if (shift && ctrl && alt)
                    ks = NewKey.NewKeyStates.ShiftAltGr;

                foreach (var k in keysDictionary.Values)
                    k.NewKeyState = ks;
            }
        }
    }
}
