// NewKey
// UserControl to represent a keyboard key in LearningKeyboard App
//
// 2017-09-19   PV


using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LearningKeyboard
{
    public partial class NewKey : UserControl
    {
        public NewKey()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SimpleTextTB.Width = Width;
            SimpleTextTB.SetValue(Canvas.TopProperty, Height / 2.0 - 12);

            NormalTextTB.SetValue(Canvas.TopProperty, Height / 2.0 - 3.0);
            NormalTextTB.SetValue(Canvas.LeftProperty, 0.0);
            NormalTextTB.Width = Width / 2;

            ShiftTextTB.SetValue(Canvas.TopProperty, 0.0);
            ShiftTextTB.SetValue(Canvas.LeftProperty, 0.0);
            ShiftTextTB.Width = Width / 2;

            AltGrTextTB.SetValue(Canvas.TopProperty, Height / 2.0 - 3.0);
            AltGrTextTB.SetValue(Canvas.LeftProperty, Width / 2.0 - 2.0);
            AltGrTextTB.Width = Width / 2;

            ShiftAltGrTextTB.SetValue(Canvas.TopProperty, 0.0);
            ShiftAltGrTextTB.SetValue(Canvas.LeftProperty, Width / 2.0 - 2.0);
            ShiftAltGrTextTB.Width = Width / 2;

        }



        public string NormalText
        {
            get { return (string)GetValue(NormalTextProperty); }
            set { SetValue(NormalTextProperty, value); }
        }

        public static readonly DependencyProperty NormalTextProperty =
            DependencyProperty.Register("NormalText", typeof(string), typeof(NewKey), new UIPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));



        public string ShiftText
        {
            get { return (string)GetValue(ShiftTextProperty); }
            set { SetValue(ShiftTextProperty, value); }
        }

        public static readonly DependencyProperty ShiftTextProperty =
            DependencyProperty.Register("ShiftText", typeof(string), typeof(NewKey), new UIPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));



        public string AltGrText
        {
            get { return (string)GetValue(AltGrTextProperty); }
            set { SetValue(AltGrTextProperty, value); }
        }

        public static readonly DependencyProperty AltGrTextProperty =
            DependencyProperty.Register("AltGrText", typeof(string), typeof(NewKey), new UIPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));



        public string ShiftAltGrText
        {
            get { return (string)GetValue(ShiftAltGrTextProperty); }
            set { SetValue(ShiftAltGrTextProperty, value); }
        }

        public static readonly DependencyProperty ShiftAltGrTextProperty =
            DependencyProperty.Register("ShiftAltGrText", typeof(string), typeof(NewKey), new UIPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NewKey key = d as NewKey;
            key.SetNewKeyState();
        }



        public enum KeyStyles
        {
            Normal,         // All 4 texts are shown together
            Simple,         // Only 1 among 4 text is shown at a time
            Static          // Always show NormalText, nothing else
        }

        public KeyStyles KeyStyle
        {
            get { return (KeyStyles)GetValue(KeyStyleProperty); }
            set { SetValue(KeyStyleProperty, value); }
        }

        public static readonly DependencyProperty KeyStyleProperty =
            DependencyProperty.Register("KeyStyle", typeof(KeyStyles), typeof(NewKey), new UIPropertyMetadata(KeyStyles.Normal, new PropertyChangedCallback(OnKeyStyleChanged)));

        private static void OnKeyStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NewKey key = d as NewKey;

            switch (key.KeyStyle)
            {
                case KeyStyles.Normal:
                    key.NormalTextTB.Visibility = Visibility.Visible;
                    key.ShiftTextTB.Visibility = Visibility.Visible;
                    key.AltGrTextTB.Visibility = Visibility.Visible;
                    key.ShiftAltGrTextTB.Visibility = Visibility.Visible;
                    key.SimpleTextTB.Visibility = Visibility.Hidden;
                    break;

                case KeyStyles.Simple:
                case KeyStyles.Static:
                    key.NormalTextTB.Visibility = Visibility.Hidden;
                    key.ShiftTextTB.Visibility = Visibility.Hidden;
                    key.AltGrTextTB.Visibility = Visibility.Hidden;
                    key.ShiftAltGrTextTB.Visibility = Visibility.Hidden;
                    key.SimpleTextTB.Visibility = Visibility.Visible;

                    if (key.KeyStyle == KeyStyles.Simple)
                    {
                        key.SimpleTextTB.FontWeight = FontWeights.Bold;
                        key.SimpleTextTB.FontSize = 16;
                    }
                    else
                    {
                        key.SimpleTextTB.FontSize = 14;
                        key.SimpleTextTB.FontWeight = FontWeights.Light;
                    }

                    break;
            }
            key.SetNewKeyState();
        }


        public Brush NormalForeground
        {
            get { return (Brush)GetValue(NormalForegroundProperty); }
            set { SetValue(NormalForegroundProperty, value); }
        }

        public static readonly DependencyProperty NormalForegroundProperty =
            DependencyProperty.Register("NormalForeground", typeof(Brush), typeof(NewKey), new UIPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnForegroundChanged)));

        private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NewKey key = d as NewKey;
            key.SetNewKeyState();
        }


        public Brush GhostedForeground
        {
            get { return (Brush)GetValue(GhostedForegroundProperty); }
            set { SetValue(GhostedForegroundProperty, value); }
        }

        public static readonly DependencyProperty GhostedForegroundProperty =
            DependencyProperty.Register("GhostedForeground", typeof(Brush), typeof(NewKey), new UIPropertyMetadata(Brushes.Silver, new PropertyChangedCallback(OnForegroundChanged)));



        public Brush NormalBackground
        {
            get { return (Brush)GetValue(NormalBackgroundProperty); }
            set { SetValue(NormalBackgroundProperty, value); }
        }

        public static readonly DependencyProperty NormalBackgroundProperty =
            DependencyProperty.Register("NormalBackground", typeof(Brush), typeof(NewKey), new UIPropertyMetadata(Brushes.AliceBlue, new PropertyChangedCallback(OnBackgroundChanged)));

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NewKey key = d as NewKey;
            if (key.m_IsPressed)
                key.Pressed();
            else
                key.Released();
        }



        private bool m_IsPressed;

        public bool IsPressed
        {
            get => m_IsPressed;
            set
            {
                if (m_IsPressed != value)
                {
                    m_IsPressed = value;
                    if (m_IsPressed)
                        Pressed();
                    else
                        Released();
                }
            }
        }

        private void Pressed()
        {
            KeyBorder.BorderBrush = Brushes.Red;
            KeyBorder.Background = NormalForeground;
            if (KeyStyle == KeyStyles.Normal)
            {
                ActiveAllText(true);
            }
            else
            {
                SimpleTextTB.Foreground = NormalBackground;
            }
            m_IsPressed = true;
        }

        private void Released()
        {
            KeyBorder.BorderBrush = Brushes.Black;
            KeyBorder.Background = NormalBackground;
            if (KeyStyle == KeyStyles.Normal)
            {
                ActiveAllText(false);
            }
            else
            {
                SimpleTextTB.Foreground = NormalForeground;
            }
            m_IsPressed = false;
        }

        public enum NewKeyStates
        {
            Normal,
            Shift,
            AltGr,
            ShiftAltGr
        }

        private NewKeyStates _NewKeyState = NewKeyStates.Normal;
        public NewKeyStates NewKeyState
        {
            get
            {
                return _NewKeyState;
            }
            set
            {
                // Shift states do not apply to static keys, they can just be pressed or not
                if (KeyStyle == KeyStyles.Static)
                    return;

                if (value != _NewKeyState)
                {
                    _NewKeyState = value;
                    SetNewKeyState();
                }
            }
        }

        private void SetNewKeyState()
        {
            switch (KeyStyle)
            {
                case KeyStyles.Static:
                    SimpleTextTB.Text = NormalText;
                    SimpleTextTB.Foreground = NormalForeground;
                    break;

                case KeyStyles.Simple:
                    switch (_NewKeyState)
                    {
                        case NewKeyStates.Normal:
                            SimpleTextTB.Text = NormalText;
                            break;
                        case NewKeyStates.Shift:
                            SimpleTextTB.Text = ShiftText;
                            break;
                        case NewKeyStates.AltGr:
                            SimpleTextTB.Text = AltGrText;
                            break;
                        case NewKeyStates.ShiftAltGr:
                            SimpleTextTB.Text = ShiftAltGrText;
                            break;
                    }
                    SimpleTextTB.Foreground = NormalForeground;
                    break;

                case KeyStyles.Normal:
                    ActiveAllText(false);
                    break;
            }
        }

        private void ActiveAllText(bool isPressed)
        {
            ActiveText(NormalTextTB, _NewKeyState == NewKeyStates.Normal, isPressed);
            ActiveText(ShiftTextTB, _NewKeyState == NewKeyStates.Shift, isPressed);
            ActiveText(AltGrTextTB, _NewKeyState == NewKeyStates.AltGr, isPressed);
            ActiveText(ShiftAltGrTextTB, _NewKeyState == NewKeyStates.ShiftAltGr, isPressed);

        }

        private void ActiveText(TextBlock tb, bool isActive, bool isPressed)
        {
            if (isActive)
            {
                tb.Foreground = isPressed ? NormalBackground : NormalForeground;
                tb.FontWeight = FontWeights.Bold;
            }
            else
            {
                tb.Foreground = GhostedForeground;
                tb.FontWeight = FontWeights.Regular;
            }
        }

    }
}
