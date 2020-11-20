// NewKey
// UserControl to represent a keyboard key in LearningKeyboard App
//
// 2017-09-19   PV
// 2020-11-17   PV      C#8, nullable enable

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#nullable enable


namespace LearningKeyboard
{
    /// <summary>
    /// User control to represent a key on screen, and its own DataContext for binding at the same time.
    /// </summary>
    public partial class NewKey : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

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
            SimpleTextTB.Height = 21;
            SimpleTextTB.SetValue(Canvas.TopProperty, H / 2.0 - H / 4);
            SimpleTextTB.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;

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


            ADNormalTextTB.SetValue(Canvas.TopProperty, H / 2.0);
            ADNormalTextTB.SetValue(Canvas.LeftProperty, 0.0);
            ADNormalTextTB.Width = W / 2;
            ADNormalTextTB.LineHeight = H / 2;
            ADNormalTextTB.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;

            ADShiftTextTB.SetValue(Canvas.TopProperty, 0.0);
            ADShiftTextTB.SetValue(Canvas.LeftProperty, 0.0);
            ADShiftTextTB.Width = W / 2;
            ADShiftTextTB.LineHeight = H / 2;
            ADShiftTextTB.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;

            ADResultTextTB.SetValue(Canvas.TopProperty, H / 2.0);
            ADResultTextTB.SetValue(Canvas.LeftProperty, W / 2.0);
            ADResultTextTB.Width = W / 2;
            ADResultTextTB.LineHeight = H / 2;
            ADResultTextTB.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;

            ADShiftResultTextTB.SetValue(Canvas.TopProperty, 0.0);
            ADShiftResultTextTB.SetValue(Canvas.LeftProperty, W / 2.0);
            ADShiftResultTextTB.Width = W / 2;
            ADShiftResultTextTB.LineHeight = H / 2;
            ADShiftResultTextTB.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
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




        public string ADNormalText
        {
            get { return (string)GetValue(ADNormalTextProperty); }
            set { SetValue(ADNormalTextProperty, value); }
        }

        public static readonly DependencyProperty ADNormalTextProperty =
            DependencyProperty.Register("ADNormalText", typeof(string), typeof(NewKey), new UIPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));



        public string ADShiftText
        {
            get { return (string)GetValue(ADShiftTextProperty); }
            set { SetValue(ADShiftTextProperty, value); }
        }

        public static readonly DependencyProperty ADShiftTextProperty =
            DependencyProperty.Register("ADShiftText", typeof(string), typeof(NewKey), new UIPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));



        public string ADResultText
        {
            get { return (string)GetValue(ADResultTextProperty); }
            set { SetValue(ADResultTextProperty, value); }
        }

        public static readonly DependencyProperty ADResultTextProperty =
            DependencyProperty.Register("ADResultText", typeof(string), typeof(NewKey), new UIPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));



        public string ADShiftResultText
        {
            get { return (string)GetValue(ADShiftResultTextProperty); }
            set { SetValue(ADShiftResultTextProperty, value); }
        }

        public static readonly DependencyProperty ADShiftResultTextProperty =
            DependencyProperty.Register("ADShiftResultText", typeof(string), typeof(NewKey), new UIPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));





        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NewKey key)
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


        /// <summary>
        /// All possible styles to display a key: Normal for 4 texts, Simple for 1 letter, Static only shows NormalText, AfterDead for display after Dead Key.
        /// </summary>
        public enum KeyStyles
        {

            /// <summary>
            /// 4 texts are shown together, for &amp;/1, é/2/~, ...
            /// </summary>
            Detail = 1,

            /// <summary>
            /// Only 1 letter is shown at a time (KeyState Normal and Shift only), for a, b...
            /// </summary>
            Simple = 2,

            /// <summary>
            /// Always show NormalText, nothing else.  Used for Shift, Tab, ...
            /// </summary>
            Static = 3,        

            /// <summary>
            /// Style used after a dead key is pressed.  Shows all 4 AD text (key letter/shifted key letter, and dead key+letter key/dead key+shift letter key)
            /// </summary>
            AfterDead = 4,
        }

        /// <summary>
        /// Used to restore KeyStyle when AfterDeak style is cleared.
        /// </summary>
        protected KeyStyles DefaultKeyStyle = (KeyStyles)0;       // Special initial value to force initialization


        /// <summary>
        /// Defines how this key should be shown, among Detail (4 text), Simple (1 text), Static (never changes), AfterDead (key and composed chars).<para />
        /// Detail and simple will change to AfterDead once a dead key is entered to show combined results, and then back to DefaultKeyStyle.
        /// </summary>
        public KeyStyles KeyStyle
        {
            get { return (KeyStyles)GetValue(KeyStyleProperty); }
            set
            {
                if (DefaultKeyStyle == (KeyStyles)0)
                    DefaultKeyStyle = value;
                SetValue(KeyStyleProperty, value);
            }
        }

        // For binding
        public static readonly DependencyProperty KeyStyleProperty =
            DependencyProperty.Register("KeyStyle", typeof(KeyStyles), typeof(NewKey), new UIPropertyMetadata(KeyStyles.Detail, new PropertyChangedCallback(OnKeyStyleChanged)));

        /// <summary>
        /// When KeyStyleProperty changes, adjust visible/hidden labels accordingly.<para />
        /// ToDo: Current code does not set KeyStyles.Detail on startup, so NewKey XAML has to predefine labels visibility according to this case.
        /// </summary>
        private static void OnKeyStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is NewKey key))
                return;

            switch (key.KeyStyle)
            {
                case KeyStyles.Detail:
                    key.NormalTextTB.Visibility = Visibility.Visible;
                    key.ShiftTextTB.Visibility = Visibility.Visible;
                    key.AltGrTextTB.Visibility = Visibility.Visible;
                    key.ShiftAltGrTextTB.Visibility = Visibility.Visible;

                    key.SimpleTextTB.Visibility = Visibility.Hidden;

                    key.ADNormalTextTB.Visibility = Visibility.Hidden;
                    key.ADShiftTextTB.Visibility = Visibility.Hidden;
                    key.ADResultTextTB.Visibility = Visibility.Hidden;
                    key.ADShiftResultTextTB.Visibility = Visibility.Hidden;
                    break;

                case KeyStyles.AfterDead:
                    key.NormalTextTB.Visibility = Visibility.Hidden;
                    key.ShiftTextTB.Visibility = Visibility.Hidden;
                    key.AltGrTextTB.Visibility = Visibility.Hidden;
                    key.ShiftAltGrTextTB.Visibility = Visibility.Hidden;

                    key.SimpleTextTB.Visibility = Visibility.Hidden;

                    key.ADNormalTextTB.Visibility = Visibility.Visible;
                    key.ADShiftTextTB.Visibility = Visibility.Visible;
                    key.ADResultTextTB.Visibility = Visibility.Visible;
                    key.ADShiftResultTextTB.Visibility = Visibility.Visible;
                    break;

                case KeyStyles.Simple:
                case KeyStyles.Static:
                    key.NormalTextTB.Visibility = Visibility.Hidden;
                    key.ShiftTextTB.Visibility = Visibility.Hidden;
                    key.AltGrTextTB.Visibility = Visibility.Hidden;
                    key.ShiftAltGrTextTB.Visibility = Visibility.Hidden;

                    key.SimpleTextTB.Visibility = Visibility.Visible;

                    key.ADNormalTextTB.Visibility = Visibility.Hidden;
                    key.ADShiftTextTB.Visibility = Visibility.Hidden;
                    key.ADResultTextTB.Visibility = Visibility.Hidden;
                    key.ADShiftResultTextTB.Visibility = Visibility.Hidden;

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
            if (d is NewKey key)
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
            if (d is NewKey key)
            {
                if (key.m_IsPressed)
                    key.Pressed();
                else
                    key.Released();
            }
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
            if (KeyStyle == KeyStyles.Detail)
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
            if (KeyStyle == KeyStyles.Detail)
            {
                ActiveAllText(false);
            }
            else
            {
                SimpleTextTB.Foreground = NormalForeground;
            }
            m_IsPressed = false;
        }

        /// <summary>
        /// Current key state, that can be normal, after Shift, after AltGr, after Shift+AltGr, after a Dead Key, after Shift+Dead Key
        /// </summary>
        public enum KeyState
        {
            /// <summary>
            /// Normal default state of the key
            /// </summary>
            Normal,

            /// <summary>
            /// State of the key after Shift pressed
            /// </summary>
            Shift,

            /// <summary>
            /// State of the key after AltGr pressed
            /// </summary>
            AltGr,

            /// <summary>
            /// State of the key after Shift+AltGr pressed
            /// </summary>
            ShiftAltGr,

            /// <summary>
            /// State of the key after a Dead Key is pressed
            /// </summary>
            ADNormal,

            /// <summary>
            /// State of the key after Shift+Dead Key is pressed
            /// </summary>
            ADShift,
        }

        private KeyState _NewKeyState = KeyState.Normal;        // Default is normal state

        public KeyState NewKeyState
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
            switch (DefaultKeyStyle)
            {
                case KeyStyles.Static:
                    SimpleTextTB.Text = NormalText;
                    SimpleTextTB.Foreground = NormalForeground;
                    break;

                case KeyStyles.Simple:
                    switch (_NewKeyState)
                    {
                        case KeyState.Normal:
                            KeyStyle = KeyStyles.Simple;
                            SimpleTextTB.Text = NormalText;
                            break;

                        case KeyState.Shift:
                            KeyStyle = KeyStyles.Simple;
                            SimpleTextTB.Text = ShiftText;
                            break;

                        case KeyState.ADNormal:
                        case KeyState.ADShift:
                            KeyStyle = KeyStyles.AfterDead;
                            SimpleTextTB.Text = "";
                            ActiveAllText(false);
                            break;

                        default:
                            KeyStyle = KeyStyles.Detail;
                            SimpleTextTB.Text = "";
                            ActiveAllText(false);
                            return;
                    }
                    SimpleTextTB.Foreground = NormalForeground;
                    break;

                case KeyStyles.Detail:
                    if (_NewKeyState == KeyState.ADNormal || _NewKeyState == KeyState.ADShift)
                        KeyStyle = KeyStyles.AfterDead;
                    else
                        KeyStyle = KeyStyles.Detail;
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

            ActiveADText(ADNormalTextTB, ADResultTextTB, _NewKeyState == KeyState.ADNormal, isPressed);
            ActiveADText(ADShiftTextTB, ADShiftResultTextTB, _NewKeyState == KeyState.ADShift, isPressed);
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

        private void ActiveADText(TextBlock tbRef, TextBlock tbAD, bool isActive, bool isPressed)
        {
            if (isActive)
            {
                tbRef.Foreground = GhostedForeground;
                tbRef.FontWeight = FontWeights.Bold;
                tbAD.Foreground = isPressed ? NormalBackground : NormalForeground;
                tbAD.FontWeight = FontWeights.Bold;
            }
            else
            {
                tbRef.Foreground = GhostedForeground;
                tbRef.FontWeight = FontWeights.Regular;
                tbAD.Foreground = GhostedForeground;
                tbAD.FontWeight = FontWeights.Bold;
            }
        }
    }
}