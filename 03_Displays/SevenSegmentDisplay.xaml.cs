namespace Displays;

public partial class SevenSegmentDisplay: UserControl, INotifyPropertyChanged
{
    public SevenSegmentDisplay()
    {
        InitializeComponent();
        UpdateSegments();
        UpdateDot();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public static readonly DependencyProperty DigitProperty =
        DependencyProperty.Register("Digit", typeof(char), typeof(SevenSegmentDisplay), new PropertyMetadata(' ', OnDigitChanged));

    public char Digit
    {
        get => (char)GetValue(DigitProperty);
        set => SetValue(DigitProperty, value);
    }

    public static readonly DependencyProperty DotProperty =
        DependencyProperty.Register("Dot", typeof(bool), typeof(SevenSegmentDisplay), new PropertyMetadata(false, OnDotChanged));

    public bool Dot
    {
        get => (bool)GetValue(DotProperty); set => SetValue(DotProperty, value);
    }

    private static void OnDigitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((SevenSegmentDisplay)d).UpdateSegments();

    private static void OnDotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((SevenSegmentDisplay)d).UpdateDot();

    public double SegmentAOpacity { get; private set; }
    public double SegmentBOpacity { get; private set; }
    public double SegmentCOpacity { get; private set; }
    public double SegmentDOpacity { get; private set; }
    public double SegmentEOpacity { get; private set; }
    public double SegmentFOpacity { get; private set; }
    public double SegmentGOpacity { get; private set; }
    public double DotOpacity { get; private set; }

    private void UpdateSegments()
    {
        bool a, b, c, d, e, f, g;
        a = b = c = d = e = f = g = false;

        switch (char.ToUpper(Digit))
        {
            case '0':
                a = b = c = d = e = f = true;
                break;
            case '1':
                b = c = true;
                break;
            case '2':
                a = b = g = e = d = true;
                break;
            case '3':
                a = b = g = c = d = true;
                break;
            case '4':
                f = g = b = c = true;
                break;
            case '5':
                a = f = g = c = d = true;
                break;
            case '6':
                a = f = g = e = c = d = true;
                break;
            case '7':
                a = b = c = true;
                break;
            case '8':
                a = b = c = d = e = f = g = true;
                break;
            case '9':
                a = b = c = d = f = g = true;
                break;
            case 'A':
                a = b = c = e = f = g = true;
                break;
            case 'B':
                c = d = e = f = g = true;
                break;
            case 'C':
                a = d = e = f = true;
                break;
            case 'D':
                b = c = d = e = g = true;
                break;
            case 'E':
                a = d = e = f = g = true;
                break;
            case 'F':
                a = e = f = g = true;
                break;
            case '-':
                g = true;
                break;
        }

        SegmentAOpacity = a ? 1.0 : 0.1;
        SegmentBOpacity = b ? 1.0 : 0.1;
        SegmentCOpacity = c ? 1.0 : 0.1;
        SegmentDOpacity = d ? 1.0 : 0.1;
        SegmentEOpacity = e ? 1.0 : 0.1;
        SegmentFOpacity = f ? 1.0 : 0.1;
        SegmentGOpacity = g ? 1.0 : 0.1;

        NotifyPropertyChanged(nameof(SegmentAOpacity));
        NotifyPropertyChanged(nameof(SegmentBOpacity));
        NotifyPropertyChanged(nameof(SegmentCOpacity));
        NotifyPropertyChanged(nameof(SegmentDOpacity));
        NotifyPropertyChanged(nameof(SegmentEOpacity));
        NotifyPropertyChanged(nameof(SegmentFOpacity));
        NotifyPropertyChanged(nameof(SegmentGOpacity));
    }

    private void UpdateDot()
    {
        DotOpacity = Dot ? 1.0 : 0.1;
        NotifyPropertyChanged(nameof(DotOpacity));
    }
}
