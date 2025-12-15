// Segment7Unit UserControl
// Implement a standard 7-segment display with a dot
//
// 2025-12-06   PV

using static System.FormattableString;

namespace Displays;

public partial class Segment7Unit: UserControl, INotifyPropertyChanged
{
    public Segment7Unit()
    {
        InitializeComponent();
        InitializeVariant();
    }

    void InitializeVariant()
    {
        if (Variant == 4)
        {
            SegmentA.Data = Geometry.Parse("F1 M 10.380 16.015 v -3.83381 c 0 -0.994833 0.806979 -1.80181 1.80181 -1.80181 h 11.1151 c 0.42598 0 0.81756 0.148166 1.12712 0.396875 l 4.55084 -9.82662 c -1.35732 -0.608542 -2.8575 -0.949854 -4.43707 -0.949854 H 10.938 c -6.01663 0 -10.9379 4.92125 -10.9379 10.9379 v 1.03452 z");
            SegmentB.Data = Geometry.Parse("F1 M 25.093 12.028 c 0.005 0.05027 0.008 0.100542 0.008 0.153458 v 12.4222 c 0.42333 0.10848 0.84402 0.23548 1.26206 0.38364 c 0.97102 0.34396 1.90764 0.7964 2.77812 1.33615 c 0.87577 0.5371 1.67746 1.15623 2.43152 1.82298 c 0.4154 0.37041 0.81492 0.762 1.19857 1.17475 c 0.57943 -0.74877 1.08479 -1.55311 1.49225 -2.41036 c 0.81756 -1.7145 1.21973 -3.61685 1.21708 -5.51391 V 10.938 c 0 -4.03754 -2.21721 -7.58031 -5.4954 -9.47473 z");
            SegmentC.Data = Geometry.Parse("F1 M 32.768 31.065 c -0.381 0.41275 -0.78052 0.80698 -1.19856 1.17475 c -0.75406 0.66675 -1.55575 1.28588 -2.43152 1.82298 c -0.87048 0.5371 -1.80711 0.99219 -2.77813 1.33615 c -0.41539 0.14816 -0.83873 0.27516 -1.26206 0.38364 v 15.6316 c 0 0.0185 0 0.0344 -0.003 0.0529 l 4.87098 10.6733 c 3.28613 -1.89177 5.51127 -5.43983 5.51127 -9.48531 v -13.6657 c 0 -1.89706 -0.39952 -3.79941 -1.21708 -5.51391 c -0.40746 -0.85725 -0.91281 -1.66159 -1.49225 -2.41036 z");
            SegmentD.Data = Geometry.Parse("F1 M 24.458 52.795 c -0.31221 0.26458 -0.71702 0.42333 -1.15888 0.42333 H 12.184 c -0.994833 0 -1.80181 -0.80698 -1.80181 -1.80181 V 47.583 l -10.3796 4.04283 v 1.03452 c 0 6.01663 4.92125 10.9379 10.9379 10.9379 H 24.540 c 1.57162 0 3.06387 -0.33867 4.41854 -0.93927 l -4.50056 -9.86102 z");
            SegmentE.Data = Geometry.Parse("F1 M 6.339 34.065 c -0.8731 -0.5371 -1.6775 -1.1562 -2.4315 -1.8230 c -0.4154 -0.3704 -0.8176 -0.7620 -1.1986 -1.1748 c -0.5794 0.7488 -1.0848 1.5531 -1.4923 2.4104 c -0.8176 1.7145 -1.2197 3.6169 -1.2171 5.5139 v 0 v 11.4168 l 10.3796 -4.0428 v -10.5807 c -0.4233 -0.1085 -0.8440 -0.2355 -1.2621 -0.3837 c -0.9710 -0.3440 -1.9077 -0.7964 -2.7781 -1.3361 z");
            SegmentF.Data = Geometry.Parse("F1 M 0.000 21.397 v 0 c 0 1.89706 0.399521 3.79941 1.21708 5.51391 c 0.407458 0.85725 0.912813 1.66159 1.49225 2.41036 c 0.381 -0.41275 0.780521 -0.80698 1.19856 -1.17475 c 0.754063 -0.66675 1.55575 -1.28588 2.43152 -1.82298 c 0.870479 -0.53711 1.8071 -0.99219 2.77813 -1.33615 c 0.415396 -0.14816 0.838729 -0.27516 1.26206 -0.38364 V 17.230 L 0.000 13.187 Z");
            SegmentG.Data = Geometry.Parse("F1 M 28.543 33.102 c 0.80963 -0.49741 1.56369 -1.07685 2.27278 -1.70656 c 0.42597 -0.37835 0.83079 -0.78052 1.21179 -1.20121 c -0.38365 -0.42069 -0.78582 -0.8255 -1.21179 -1.20121 c -0.70909 -0.6297 -1.4658 -1.20914 -2.27278 -1.70656 c -0.80697 -0.49742 -1.66687 -0.91281 -2.56116 -1.23031 c -1.79123 -0.64294 -3.70681 -0.8599 -5.59859 -0.8599 h -5.28902 c -1.89442 0 -3.80735 0.21961 -5.59858 0.8599 c -0.894291 0.3175 -1.75154 0.73289 -2.56117 1.23031 c -0.809625 0.49742 -1.56369 1.07686 -2.27277 1.70656 c -0.425979 0.37836 -0.830792 0.78052 -1.21179 1.20121 c 0.383646 0.42069 0.785813 0.8255 1.21179 1.20121 c 0.709083 0.62971 1.46579 1.20915 2.27277 1.70656 c 0.806979 0.49742 1.66688 0.91282 2.56117 1.23032 c 1.79123 0.64293 3.70681 0.85989 5.59858 0.85989 h 5.28902 c 1.89442 0 3.80736 -0.2196 5.59859 -0.85989 c 0.89429 -0.3175 1.75154 -0.7329 2.56116 -1.23032 z");

            return;
        }

        // Variant 0: Separated a bit thinner segments
        // Variant 1: Jointive thick segments

        double m = (Variant & 1) == 1 ? 0 : 1;
        double t = (Variant & 1) == 1 ? 5 : 4.5;

        // Version with segments as Polygons
        //
        //PointCollection HorizSegment(double x, double y, double w)
        //{
        //    if (Variant < 2)
        //        return [new Point(x, y), new Point(x + t, y - t), new Point(x + w - t, y - t), new Point(x + w, y), new Point(x + w - t, y + t), new Point(x + t, y + t)];
        //    return [new Point(x + m, y - t), new Point(x + w - m, y - t), new Point(x + w - m, y + t), new Point(x + m, y + t)];
        //}
        //PointCollection VertSegment(double x, double y, double h)
        //{
        //    if (Variant < 2)
        //        return [new Point(x, y), new Point(x + t, y + t), new Point(x + t, y + h - t), new Point(x, y + h), new Point(x - t, y + h - t), new Point(x - t, y + t)];
        //    return [new Point(x + t, y + m), new Point(x + t, y + h - m), new Point(x - t, y + h - m), new Point(x - t, y + m)];
        //}
        //
        //SegmentA.Points = HorizSegment(10 + m, 10, 40 - 2 * m);
        //SegmentB.Points = VertSegment(50, 10 + m, 40 - 2 * m);
        //SegmentC.Points = VertSegment(50, 50 + m, 40 - 2 * m);
        //SegmentD.Points = HorizSegment(10 + m, 90, 40 - 2 * m);
        //SegmentE.Points = VertSegment(10, 50 + m, 40 - 2 * m);
        //SegmentF.Points = VertSegment(10, 10 + m, 40 - 2 * m);
        //SegmentG.Points = HorizSegment(10 + m, 50, 40 - 2 * m);

        string HorizSegment(double x, double y, double w)
            => Variant < 2
                ? "F1 " + Invariant($"M {x} {y} ") + Invariant($"l {t} {-t} ") + Invariant($"h {w - 2 * t} ") + Invariant($"l {t} {t} {-t} {t} ") + Invariant($"h {2 * t - w} ") + "Z"
                : "F1 " + Invariant($"M {x + m} {y - t} ") + Invariant($"h {w - 2 * m} ") + Invariant($"v {2 * t} ") + Invariant($"h {2 * m - w} ") + "Z";

        string VertSegment(double x, double y, double h)
            => (Variant < 2)
                ? "F1 " + Invariant($"M {x} {y} ") + Invariant($"l {t} {t} ") + Invariant($"v {h - 2 * t} ") + Invariant($"l {-t} {t} {-t} {-t} ") + Invariant($"v {2 * t - h} ") + "Z"
                : "F1 " + Invariant($"M {x - t} {y + m} ") + Invariant($"v {h - 2 * m} ") + Invariant($"h {2 * t} ") + Invariant($"v {2 * m - h} ") + "Z";

        //var sA = HorizSegment(10 + m, 10, 40 - 2 * m);

        SegmentA.Data = Geometry.Parse(HorizSegment(10 + m, 10, 40 - 2 * m));
        SegmentB.Data = Geometry.Parse(VertSegment(50, 10 + m, 40 - 2 * m));
        SegmentC.Data = Geometry.Parse(VertSegment(50, 50 + m, 40 - 2 * m));
        SegmentD.Data = Geometry.Parse(HorizSegment(10 + m, 90, 40 - 2 * m));
        SegmentE.Data = Geometry.Parse(VertSegment(10, 50 + m, 40 - 2 * m));
        SegmentF.Data = Geometry.Parse(VertSegment(10, 10 + m, 40 - 2 * m));
        SegmentG.Data = Geometry.Parse(HorizSegment(10 + m, 50, 40 - 2 * m));

        UpdateSegments();
        UpdateDot();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // ----

    public static readonly DependencyProperty SymbolProperty =
        DependencyProperty.Register("Symbol", typeof(char), typeof(Segment7Unit), new PropertyMetadata(' ', OnSymbolChanged));

    public char Symbol
    {
        get => (char)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    // ----

    public static readonly DependencyProperty DotProperty =
        DependencyProperty.Register("Dot", typeof(bool), typeof(Segment7Unit), new PropertyMetadata(false, OnDotChanged));

    public bool Dot
    {
        get => (bool)GetValue(DotProperty);
        set => SetValue(DotProperty, value);
    }

    // ----

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register("Variant", typeof(int), typeof(Segment7Unit),
        new PropertyMetadata(0, new PropertyChangedCallback(OnVariantChanged)));

    public int Variant
    {
        get => (int)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    // ----

    private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((Segment7Unit)d).UpdateSegments();

    private static void OnDotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((Segment7Unit)d).UpdateDot();

    private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Segment7Unit)d).InitializeVariant();
        ((Segment7Unit)d).UpdateSegments();
    }

    public double SegmentAOpacity { get; private set; }
    public double SegmentBOpacity { get; private set; }
    public double SegmentCOpacity { get; private set; }
    public double SegmentDOpacity { get; private set; }
    public double SegmentEOpacity { get; private set; }
    public double SegmentFOpacity { get; private set; }
    public double SegmentGOpacity { get; private set; }
    public double DotOpacity { get; private set; }

    const bool F = false;
    const bool T = true;

    private static readonly Dictionary<char, bool[]> CharToSegments = new()
    {
        { ' ', [F, F, F, F, F, F, F] },
        { '0', [T, T, T, T, T, T, F] },
        { '1', [F, T, T, F, F, F, F] },
        { '2', [T, T, F, T, T, F, T] },
        { '3', [T, T, T, T, F, F, T] },
        { '4', [F, T, T, F, F, T, T] },
        { '5', [T, F, T, T, F, T, T] },
        { '6', [T, F, T, T, T, T, T] },
        { '7', [T, T, T, F, F, F, F] },
        { '8', [T, T, T, T, T, T, T] },
        { '9', [T, T, T, T, F, T, T] },
        { '-', [F, F, F, F, F, F, T] },
        { '=', [F, F, F, T, F, F, T] },
        { '(', [T, F, F, T, T, T, F] },
        { ')', [T, T, T, T, F, F, F] },
        { 'A', [T, T, T, F, T, T, T] },
        { 'a', [T, T, T, F, T, T, T] },
        { 'B', [F, F, T, T, T, T, T] },
        { 'b', [F, F, T, T, T, T, T] },
        { 'C', [T, F, F, T, T, T, F] },
        { 'c', [F, F, F, T, T, F, T] },
        { 'D', [F, T, T, T, T, F, T] },
        { 'd', [F, T, T, T, T, F, T] },
        { 'E', [T, F, F, T, T, T, T] },
        { 'e', [T, F, F, T, T, T, T] },
        { 'F', [T, F, F, F, T, T, T] },
        { 'f', [T, F, F, F, T, T, T] },
        { 'G', [T, F, T, T, T, T, F] },
        { 'g', [T, F, T, T, T, T, F] },
        { 'H', [F, T, T, F, T, T, T] },
        { 'h', [F, F, T, F, T, T, T] },
        { 'I', [F, T, T, F, F, F, F] },
        { 'i', [F, F, T, F, F, F, F] },
        { 'J', [F, T, T, T, F, F, F] },
        { 'j', [F, F, T, T, F, F, F] },
        { 'L', [F, F, F, T, T, T, F] },
        { 'l', [F, F, F, T, T, F, F] },
        { 'N', [T, T, T, F, T, T, F] },
        { 'n', [F, F, T, F, T, F, T] },
        { 'O', [T, T, T, T, T, T, F] },
        { 'o', [F, F, T, T, T, F, T] },
        { 'P', [T, T, F, F, T, T, T] },
        { 'p', [T, T, F, F, T, T, T] },
        { 'Q', [T, T, T, F, F, T, T] },
        { 'q', [T, T, T, F, F, T, T] },
        { 'R', [F, F, F, F, T, F, T] },
        { 'r', [F, F, F, F, T, F, T] },
        { 'S', [T, F, T, T, F, T, T] },
        { 's', [T, F, T, T, F, T, T] },
        { 'T', [F, F, F, T, T, T, T] },
        { 't', [F, F, F, T, T, T, T] },
        { 'U', [F, T, T, T, T, T, F] },
        { 'u', [F, F, T, T, T, F, F] },
        { 'Y', [F, T, T, T, F, T, T] },
        { 'y', [F, T, T, T, F, T, T] },
        { '[', [T, F, F, T, T, T, F] },
        { '|', [F, T, T, F, F, F, F] },
        { ']', [T, T, T, T, F, F, F] },
        { '~', [F, T, F, F, T, F, T] },
    };

    private void UpdateSegments()
    {
        if (!CharToSegments.TryGetValue(Symbol, out var segments))
            segments = CharToSegments[' '];

        SegmentAOpacity = segments[0] ? 1.0 : 0.1;
        SegmentBOpacity = segments[1] ? 1.0 : 0.1;
        SegmentCOpacity = segments[2] ? 1.0 : 0.1;
        SegmentDOpacity = segments[3] ? 1.0 : 0.1;
        SegmentEOpacity = segments[4] ? 1.0 : 0.1;
        SegmentFOpacity = segments[5] ? 1.0 : 0.1;
        SegmentGOpacity = segments[6] ? 1.0 : 0.1;

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
