// NewKey
// UserControl to represent a keyboard key in LearningKeyboard App
//
// 2017-09-19   PV


using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LearningKeyboard
{
    public partial class NewKey : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public NewKey()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double H = Height - 2.0;
            double W = Width - 2.0;

            KeyCanvas.SetValue(TextBlock.FontSizeProperty, H / 38.0 * 15.0);

            SimpleTextTB.Width = W;
            SimpleTextTB.Height = 20;
            SimpleTextTB.SetValue(Canvas.TopProperty, H / 2.0 - H / 4);
            SimpleTextTB.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            //SimpleTextTB.Background = Brushes.Yellow;

            NormalTextTB.SetValue(Canvas.TopProperty, H / 2.0);
            NormalTextTB.SetValue(Canvas.LeftProperty, 0.0);
            NormalTextTB.Width = W / 2;
            NormalTextTB.LineHeight = H / 2;
            NormalTextTB.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;

            ShiftTextTB.SetValue(Canvas.TopProperty, 0.0);
            ShiftTextTB.SetValue(Canvas.LeftProperty, 0.0);
            ShiftTextTB.Width = W / 2;
            ShiftTextTB.LineHeight = H / 2;
            ShiftTextTB.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;

            AltGrTextTB.SetValue(Canvas.TopProperty, H / 2.0);
            AltGrTextTB.SetValue(Canvas.LeftProperty, W / 2.0);
            AltGrTextTB.Width = W / 2;
            AltGrTextTB.LineHeight = H / 2;
            AltGrTextTB.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;

            ShiftAltGrTextTB.SetValue(Canvas.TopProperty, 0.0);
            ShiftAltGrTextTB.SetValue(Canvas.LeftProperty, W / 2.0);
            ShiftAltGrTextTB.Width = W / 2;
            ShiftAltGrTextTB.LineHeight = H / 2;
            ShiftAltGrTextTB.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
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



        public bool IsNormalDeadKey
        {
            get { return (bool)GetValue(NormalDeadKeyProperty); }
            set { SetValue(NormalDeadKeyProperty, value); }
        }

        public static readonly DependencyProperty NormalDeadKeyProperty =
            DependencyProperty.Register(nameof(IsNormalDeadKey), typeof(bool), typeof(NewKey), new UIPropertyMetadata(false));

        public bool IsShiftDeadKey
        {
            get { return (bool)GetValue(ShiftDeadKeyProperty); }
            set { SetValue(ShiftDeadKeyProperty, value); }
        }

        public static readonly DependencyProperty ShiftDeadKeyProperty =
            DependencyProperty.Register(nameof(IsShiftDeadKey), typeof(bool), typeof(NewKey), new UIPropertyMetadata(false));

        public bool IsAltGrDeadKey
        {
            get { return (bool)GetValue(AltGrDeadKeyProperty); }
            set { SetValue(AltGrDeadKeyProperty, value); }
        }

        public static readonly DependencyProperty AltGrDeadKeyProperty =
            DependencyProperty.Register(nameof(IsAltGrDeadKey), typeof(bool), typeof(NewKey), new UIPropertyMetadata(false));


        public bool IsShiftAltGrDeadKey
        {
            get { return (bool)GetValue(ShiftAltGrDeadKeyProperty); }
            set { SetValue(ShiftAltGrDeadKeyProperty, value); }
        }

        public static readonly DependencyProperty ShiftAltGrDeadKeyProperty =
            DependencyProperty.Register(nameof(IsShiftAltGrDeadKey), typeof(bool), typeof(NewKey), new UIPropertyMetadata(false));



        public enum NewKeyStyle
        {
            Normal = 1,         // All 4 texts are shown together
            Simple = 2,         // Only 1 among 4 text is shown at a time
            Static = 3          // Always show NormalText, nothing else
        }

        private NewKeyStyle DefaultKeyStyle = (NewKeyStyle)0;

        public NewKeyStyle KeyStyle
        {
            get { return (NewKeyStyle)GetValue(KeyStyleProperty); }
            set
            {
                if (DefaultKeyStyle == (NewKeyStyle)0)
                    DefaultKeyStyle = value;
                SetValue(KeyStyleProperty, value);
            }
        }

        public static readonly DependencyProperty KeyStyleProperty =
            DependencyProperty.Register("KeyStyle", typeof(NewKeyStyle), typeof(NewKey), new UIPropertyMetadata(NewKeyStyle.Normal, new PropertyChangedCallback(OnKeyStyleChanged)));

        private static void OnKeyStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NewKey key = d as NewKey;

            switch (key.KeyStyle)
            {
                case NewKeyStyle.Normal:
                    key.NormalTextTB.Visibility = Visibility.Visible;
                    key.ShiftTextTB.Visibility = Visibility.Visible;
                    key.AltGrTextTB.Visibility = Visibility.Visible;
                    key.ShiftAltGrTextTB.Visibility = Visibility.Visible;
                    key.SimpleTextTB.Visibility = Visibility.Hidden;
                    break;

                case NewKeyStyle.Simple:
                case NewKeyStyle.Static:
                    key.NormalTextTB.Visibility = Visibility.Hidden;
                    key.ShiftTextTB.Visibility = Visibility.Hidden;
                    key.AltGrTextTB.Visibility = Visibility.Hidden;
                    key.ShiftAltGrTextTB.Visibility = Visibility.Hidden;
                    key.SimpleTextTB.Visibility = Visibility.Visible;

                    if (key.KeyStyle == NewKeyStyle.Simple)
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
            DependencyProperty.Register("GhostedForeground", typeof(Brush), typeof(NewKey), new UIPropertyMetadata(Brushes.Gray, new PropertyChangedCallback(OnForegroundChanged)));



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




        private Brush _NormalTextBackground = Brushes.Transparent;

        public Brush NormalTextBackground
        {
            get { return _NormalTextBackground; }
            set
            {
                if (_NormalTextBackground != value)
                {
                    _NormalTextBackground = value;
                    NotifyPropertyChanged(nameof(NormalTextBackground));
                }
            }
        }

        private Brush _ShiftTextBackground = Brushes.Transparent;

        public Brush ShiftTextBackground
        {
            get { return _ShiftTextBackground; }
            set
            {
                if (_ShiftTextBackground != value)
                {
                    _ShiftTextBackground = value;
                    NotifyPropertyChanged(nameof(ShiftTextBackground));
                }
            }
        }

        private Brush _AltGrTextBackground = Brushes.Transparent;

        public Brush AltGrTextBackground
        {
            get { return _AltGrTextBackground; }
            set
            {
                if (_AltGrTextBackground != value)
                {
                    _AltGrTextBackground = value;
                    NotifyPropertyChanged(nameof(AltGrTextBackground));
                }
            }
        }

        private Brush _ShiftAltGrTextBackground = Brushes.Transparent;

        public Brush ShiftAltGrTextBackground
        {
            get { return _ShiftAltGrTextBackground; }
            set
            {
                if (_ShiftAltGrTextBackground != value)
                {
                    _ShiftAltGrTextBackground = value;
                    NotifyPropertyChanged(nameof(ShiftAltGrTextBackground));
                }
            }
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
            if (KeyStyle == NewKeyStyle.Normal)
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
            if (KeyStyle == NewKeyStyle.Normal)
            {
                ActiveAllText(false);
            }
            else
            {
                SimpleTextTB.Foreground = NormalForeground;
            }
            m_IsPressed = false;
        }

        public enum KeyState
        {
            Normal,
            Shift,
            AltGr,
            ShiftAltGr
        }

        private KeyState _NewKeyState = KeyState.Normal;

        public KeyState NewKeyState
        {
            get
            {
                return _NewKeyState;
            }
            set
            {
                // Shift states do not apply to static keys, they can just be pressed or not
                if (KeyStyle == NewKeyStyle.Static)
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
            switch (DefaultKeyStyle)
            {
                case NewKeyStyle.Static:
                    SimpleTextTB.Text = NormalText;
                    SimpleTextTB.Foreground = NormalForeground;
                    break;

                case NewKeyStyle.Simple:
                    switch (_NewKeyState)
                    {
                        case KeyState.Normal:
                            KeyStyle = NewKeyStyle.Simple;
                            SimpleTextTB.Text = NormalText;
                            break;

                        case KeyState.Shift:
                            KeyStyle = NewKeyStyle.Simple;
                            SimpleTextTB.Text = ShiftText;
                            break;

                        default:
                            KeyStyle = NewKeyStyle.Normal;
                            SimpleTextTB.Text = "";
                            ActiveAllText(false);
                            return;
                            //case KeyState.AltGr:
                            //    SimpleTextTB.Text = AltGrText;
                            //    break;
                            //case KeyState.ShiftAltGr:
                            //    SimpleTextTB.Text = ShiftAltGrText;
                            //    break;
                    }
                    SimpleTextTB.Foreground = NormalForeground;
                    break;

                case NewKeyStyle.Normal:
                    ActiveAllText(false);
                    break;
            }
        }

        private void ActiveAllText(bool isPressed)
        {
            ActiveText(NormalTextTB, _NewKeyState == KeyState.Normal, isPressed);
            ActiveText(ShiftTextTB, _NewKeyState == KeyState.Shift, isPressed);
            ActiveText(AltGrTextTB, _NewKeyState == KeyState.AltGr, isPressed);
            ActiveText(ShiftAltGrTextTB, _NewKeyState == KeyState.ShiftAltGr, isPressed);
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