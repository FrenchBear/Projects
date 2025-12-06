// Segment14Unit UserControl
// Implement a standard 14-segment display with a dot
//
// 2025-12-06   PV

namespace Displays;

public partial class Segment14Unit: UserControl, INotifyPropertyChanged
{
    public Segment14Unit()
    {
        InitializeComponent();

        //const double m = 1;
        //const double t = 4.5;

        //static PointCollection HorizSegment(double x, double y, double w)
        //    => [new Point(x, y), new Point(x + t, y - t), new Point(x + w - t, y - t), new Point(x + w, y), new Point(x + w - t, y + t), new Point(x + t, y + t)];
        //static PointCollection VertSegment(double x, double y, double h)
        //    => [new Point(x, y), new Point(x + t, y + t), new Point(x + t, y + h - t), new Point(x, y + h), new Point(x - t, y + h - t), new Point(x - t, y + t)];

        //SegmentA.Points = HorizSegment(10 + m, 10, 40 - 2 * m);
        //SegmentB.Points = VertSegment(50, 10 + m, 40 - 2 * m);
        //SegmentC.Points = VertSegment(50, 50 + m, 40 - 2 * m);
        //SegmentD.Points = HorizSegment(10 + m, 90, 40 - 2 * m);
        //SegmentE.Points = VertSegment(10, 50 + m, 40 - 2 * m);
        //SegmentF.Points = VertSegment(10, 10 + m, 40 - 2 * m);
        //SegmentG.Points = HorizSegment(10 + m, 50, 40 - 2 * m);

        UpdateSegments();
        UpdateDot();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // ----

    public static readonly DependencyProperty SymbolProperty =
        DependencyProperty.Register("Symbol", typeof(char), typeof(Segment14Unit), new PropertyMetadata(' ', OnSymbolChanged));

    public char Symbol
    {
        get => (char)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    // ----

    public static readonly DependencyProperty DotProperty =
        DependencyProperty.Register("Dot", typeof(bool), typeof(Segment14Unit), new PropertyMetadata(false, OnDotChanged));

    public bool Dot
    {
        get => (bool)GetValue(DotProperty);
        set => SetValue(DotProperty, value);
    }

    // ----

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register("Variant", typeof(int), typeof(Segment14Unit),
        new PropertyMetadata(0, new PropertyChangedCallback(OnVariantChanged)));

    public int Variant
    {
        get => (int)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    // ----

    private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((Segment14Unit)d).UpdateSegments();

    private static void OnDotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((Segment14Unit)d).UpdateDot();

    private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((Segment14Unit)d).UpdateSegments();

    public double SegmentAOpacity { get; private set; }
    public double SegmentBOpacity { get; private set; }
    public double SegmentCOpacity { get; private set; }
    public double SegmentDOpacity { get; private set; }
    public double SegmentEOpacity { get; private set; }
    public double SegmentFOpacity { get; private set; }
    public double SegmentG1Opacity { get; private set; }
    public double SegmentG2Opacity { get; private set; }
    public double SegmentHOpacity { get; private set; }
    public double SegmentIOpacity { get; private set; }
    public double SegmentJOpacity { get; private set; }
    public double SegmentKOpacity { get; private set; }
    public double SegmentLOpacity { get; private set; }
    public double SegmentMOpacity { get; private set; }

    public double DotOpacity { get; private set; }

    private void UpdateSegments()
    {
        bool a, b, c, d, e, f, g1, g2, h, i, j, k, l, m;
        a = b = c = d = e = f = g1 = g2 = h = i = j = k = l = m = false;

        switch (Symbol)
        {
            case '0':
                a = b = c = d = e = f = true;
                break;
            case '1':
                b = c = true;
                break;
            case '2':
                a = b = g1 = g2 = e = d = true;
                break;
            case '3':
                a = b = g1 = g2 = c = d = true;
                break;
            case '4':
                f = g1 = g2 = b = c = true;
                break;
            case '5':
                a = f = g1 = g2 = c = d = true;
                break;
            case '6':
                a = f = g1 = g2 = e = c = d = true;
                break;
            case '7':
                a = b = c = true;
                break;
            case '8':
                a = b = c = d = e = f = g1 = g2 = true;
                break;
            case '9':
                a = b = c = d = f = g1 = g2 = true;
                break;
            case 'A':
                a = b = c = e = f = g1 = g2 = true;
                break;
            case 'B':
                c = d = e = f = g1 = g2 = true;
                break;
            case 'C':
                a = d = e = f = true;
                break;
            case 'D':
                b = c = d = e = g1 = g2 = true;
                break;
            case 'E':
                a = d = e = f = g1 = g2 = true;
                break;
            case 'F':
                a = e = f = g1 = g2 = true;
                break;
            case '-':
                g1 = true;
                break;
            case 'r':
                e = g1 = true;
                break;
            case 'o':
                c = d = e = g1 = g2 = true;
                break;
        }

        SegmentAOpacity = a ? 1.0 : 0.1;
        SegmentBOpacity = b ? 1.0 : 0.1;
        SegmentCOpacity = c ? 1.0 : 0.1;
        SegmentDOpacity = d ? 1.0 : 0.1;
        SegmentEOpacity = e ? 1.0 : 0.1;
        SegmentFOpacity = f ? 1.0 : 0.1;
        SegmentG1Opacity = g1 ? 1.0 : 0.1;
        SegmentG2Opacity = g2 ? 1.0 : 0.1;
        SegmentHOpacity = h ? 1.0 : 0.1;
        SegmentIOpacity = i ? 1.0 : 0.1;
        SegmentJOpacity = j ? 1.0 : 0.1;
        SegmentKOpacity = k ? 1.0 : 0.1;
        SegmentLOpacity = l ? 1.0 : 0.1;
        SegmentMOpacity = m ? 1.0 : 0.1;

        NotifyPropertyChanged(nameof(SegmentAOpacity));
        NotifyPropertyChanged(nameof(SegmentBOpacity));
        NotifyPropertyChanged(nameof(SegmentCOpacity));
        NotifyPropertyChanged(nameof(SegmentDOpacity));
        NotifyPropertyChanged(nameof(SegmentEOpacity));
        NotifyPropertyChanged(nameof(SegmentFOpacity));
        NotifyPropertyChanged(nameof(SegmentG1Opacity));
        NotifyPropertyChanged(nameof(SegmentG2Opacity));
        NotifyPropertyChanged(nameof(SegmentHOpacity));
        NotifyPropertyChanged(nameof(SegmentIOpacity));
        NotifyPropertyChanged(nameof(SegmentJOpacity));
        NotifyPropertyChanged(nameof(SegmentKOpacity));
        NotifyPropertyChanged(nameof(SegmentLOpacity));
        NotifyPropertyChanged(nameof(SegmentMOpacity));
    }

    private void UpdateDot()
    {
        DotOpacity = Dot ? 1.0 : 0.1;
        NotifyPropertyChanged(nameof(DotOpacity));
    }
}
