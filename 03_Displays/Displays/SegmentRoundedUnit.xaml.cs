// SegmentRoundedUnit UserControl
// Implement a standard 7-segment display with a dot
//
// 2025-12-06   PV

namespace Displays;

public partial class SegmentRoundedUnit: UserControl, INotifyPropertyChanged
{
    public SegmentRoundedUnit()
    {
        InitializeComponent();
        //InitializeVariant();
    }

    //void InitializeVariant()
    //{
    //    // Variant 0: Separated a bit thinner segments
    //    // Variant 1: Jointive thick segments

    //    double m = (Variant & 1) == 1 ? 0 : 1;
    //    double t = (Variant & 1) == 1 ? 5 : 4.5;

    //    PointCollection HorizSegment(double x, double y, double w)
    //    {
    //        if (Variant < 2)
    //            return [new Point(x, y), new Point(x + t, y - t), new Point(x + w - t, y - t), new Point(x + w, y), new Point(x + w - t, y + t), new Point(x + t, y + t)];
    //        return [new Point(x + m, y - t), new Point(x + w - m, y - t), new Point(x + w - m, y + t), new Point(x + m, y + t)];
    //    }
    //    PointCollection VertSegment(double x, double y, double h)
    //    {
    //        if (Variant < 2)
    //            return [new Point(x, y), new Point(x + t, y + t), new Point(x + t, y + h - t), new Point(x, y + h), new Point(x - t, y + h - t), new Point(x - t, y + t)];
    //        return [new Point(x + t, y + m), new Point(x + t, y + h - m), new Point(x - t, y + h - m), new Point(x - t, y + m)];
    //    }

    //    SegmentA.Points = HorizSegment(10 + m, 10, 40 - 2 * m);
    //    SegmentB.Points = VertSegment(50, 10 + m, 40 - 2 * m);
    //    SegmentC.Points = VertSegment(50, 50 + m, 40 - 2 * m);
    //    SegmentD.Points = HorizSegment(10 + m, 90, 40 - 2 * m);
    //    SegmentE.Points = VertSegment(10, 50 + m, 40 - 2 * m);
    //    SegmentF.Points = VertSegment(10, 10 + m, 40 - 2 * m);
    //    SegmentG.Points = HorizSegment(10 + m, 50, 40 - 2 * m);

    //    UpdateSegments();
    //    UpdateDot();
    //}

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // ----

    public static readonly DependencyProperty SymbolProperty =
        DependencyProperty.Register("Symbol", typeof(char), typeof(SegmentRoundedUnit), new PropertyMetadata(' ', OnSymbolChanged));

    public char Symbol
    {
        get => (char)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    // ----

    public static readonly DependencyProperty DotProperty =
        DependencyProperty.Register("Dot", typeof(bool), typeof(SegmentRoundedUnit), new PropertyMetadata(false, OnDotChanged));

    public bool Dot
    {
        get => (bool)GetValue(DotProperty);
        set => SetValue(DotProperty, value);
    }

    // ----

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register("Variant", typeof(int), typeof(SegmentRoundedUnit),
        new PropertyMetadata(0, new PropertyChangedCallback(OnVariantChanged)));

    public int Variant
    {
        get => (int)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    // ----

    private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((SegmentRoundedUnit)d).UpdateSegments();

    private static void OnDotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((SegmentRoundedUnit)d).UpdateDot();

    private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //((SegmentRoundedUnit)d).InitializeVariant();
        ((SegmentRoundedUnit)d).UpdateSegments();
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
        { 'R', [F, F, F, F, T, F, T] },
        { 'r', [F, F, F, F, T, F, T] },
        { 'S', [T, F, T, T, F, T, T] },
        { 's', [T, F, T, T, F, T, T] },
        { 'T', [F, F, F, T, T, T, T] },
        { 't', [F, F, F, T, T, T, T] },
        { 'U', [F, T, T, T, T, T, F] },
        { 'u', [F, F, T, T, T, F, F] },
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
