using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NewKeyTestApp
{
    /// <summary>
    /// Interaction logic for NewKey.xaml
    /// </summary>
    public partial class NewKey : UserControl
    {
        public NewKey()
        {
            InitializeComponent();
            DataContext = this;
        }


        //public string SimpleText
        //{
        //    // Relay to registered property
        //    get { return (string)GetValue(SimpleTextProperty); }
        //    set { SetValue(SimpleTextProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for SimpleText.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty SimpleTextProperty =
        //    DependencyProperty.Register("SimpleText", typeof(string), typeof(NewKey), new UIPropertyMetadata(""));


        public string NormalText
        {
            // Relay to registered property
            get { return (string)GetValue(NormalTextProperty); }
            set { SetValue(NormalTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NormalText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NormalTextProperty =
            DependencyProperty.Register("NormalText", typeof(string), typeof(NewKey), new UIPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));



        public string ShiftText
        {
            // Relay to registered property
            get { return (string)GetValue(ShiftTextProperty); }
            set { SetValue(ShiftTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShiftText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShiftTextProperty =
            DependencyProperty.Register("ShiftText", typeof(string), typeof(NewKey), new UIPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));



        public string AltGrText
        {
            // Relay to registered property
            get { return (string)GetValue(AltGrTextProperty); }
            set { SetValue(AltGrTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AltGrText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AltGrTextProperty =
            DependencyProperty.Register("AltGrText", typeof(string), typeof(NewKey), new UIPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));

        public string ShiftAltGrText
        {
            // Relay to registered property
            get { return (string)GetValue(ShiftAltGrTextProperty); }
            set { SetValue(ShiftAltGrTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShiftAltGrText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShiftAltGrTextProperty =
            DependencyProperty.Register("ShiftAltGrText", typeof(string), typeof(NewKey), new UIPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NewKey key = d as NewKey;
            key.SetNewKeyState();
        }

        public bool SimpleLayout
        {
            // Relay to registered property
            get { return (bool)GetValue(SimpleLayoutProperty); }
            set { SetValue(SimpleLayoutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SimpleLayout.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SimpleLayoutProperty =
            DependencyProperty.Register("SimpleLayout", typeof(bool), typeof(NewKey), new UIPropertyMetadata(false, new PropertyChangedCallback(OnSimpleLayoutChanged)));

        private static void OnSimpleLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NewKey key = d as NewKey;

            key.NormalTextTB.Visibility = key.SimpleLayout ? Visibility.Hidden : Visibility.Visible;
            key.ShiftTextTB.Visibility = key.SimpleLayout ? Visibility.Hidden : Visibility.Visible;
            key.AltGrTextTB.Visibility = key.SimpleLayout ? Visibility.Hidden : Visibility.Visible;
            key.ShiftAltGrTextTB.Visibility = key.SimpleLayout ? Visibility.Hidden : Visibility.Visible;

            key.SimpleTextTB.Visibility = key.SimpleLayout ? Visibility.Visible : Visibility.Hidden;
            key.SetNewKeyState();
        }


        internal void Released()
        {
            KeyBorder.BorderBrush = Brushes.Black;
            KeyBorder.BorderThickness = new Thickness(1);
        }

        internal void Pressed()
        {
            KeyBorder.BorderBrush = Brushes.Red;
            KeyBorder.BorderThickness = new Thickness(2);
        }

        internal enum NewKeyStates
        {
            Normal,
            Shift,
            AltGr,
            ShiftAltGr
        }

        private NewKeyStates _NewKeyState = NewKeyStates.Normal;
        internal NewKeyStates NewKeyState
        {
            get
            {
                return _NewKeyState;
            }
            set
            { 
                if (value!= _NewKeyState)
                {
                    _NewKeyState = value;
                    SetNewKeyState();
                }
            }
        }

        private void SetNewKeyState()
        {
            if (SimpleLayout)
            {
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
            }
            else
            {
                ActiveText(NormalTextTB, _NewKeyState == NewKeyStates.Normal);
                ActiveText(ShiftTextTB, _NewKeyState == NewKeyStates.Shift);
                ActiveText(AltGrTextTB, _NewKeyState == NewKeyStates.AltGr);
                ActiveText(ShiftAltGrTextTB, _NewKeyState == NewKeyStates.ShiftAltGr);
            }
        }

        private void ActiveText(TextBlock tb, bool isActive)
        {
            if (isActive)
            {
                tb.Foreground = Brushes.Black;
                tb.FontWeight = FontWeights.Bold;
            }
            else
            {
                tb.Foreground = Brushes.Silver;
                tb.FontWeight = FontWeights.Regular;
            }
        }
    }
}
