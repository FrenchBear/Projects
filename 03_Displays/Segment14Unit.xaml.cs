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

    // Truth table for 14-segment display, mapping ASCII characters 32-126 to a 14-bit integer.
    // Bit 13=A, 12=B, 11=C, 10=D, 9=E, 8=F, 7=G1, 6=G2, 5=H, 4=I, 3=J, 2=K, 1=L, 0=M
    //       [2    1]    [8    4    2    1]   [8     4     2    1]   [8    4    2    1]
    private static Dictionary<char, int> CharToSegments = new()
    {
        { ' ', 0x0000 },  // 
        { '!', 0x0012 },  // I,L
        { '"', 0x1010 },  // B,F
        { '#', 0x1cd2 },  // B,C,D,G1,G2,I,L
        { '$', 0x2dd2 },  // A,C,D,F,G1,G2,I,L
        { '%', 0x090c },  // C,F,J,K
        { '&', 0x26e1 },  // A,D,E,G1,G2,H,M
        { '\'', 0x0008 },  // J
        { '(', 0x0009 },  // J,M
        { ')', 0x0024 },  // H,K
        { '*', 0x00ff },  // G1,G2,H,I,J,K,L,M
        { '+', 0x00d2 },  // G1,G2,I,L
        { ',', 0x0004 },  // K
        { '-', 0x00c0 },  // G1,G2
        { '.', 0x0200 },  // E
        { '/', 0x000c },  // J,K
        { '0', 0x3f0c },  // A,B,C,D,E,F,J,K
        { '1', 0x1808 },  // B,C,J
        { '2', 0x36c0 },  // A,B,D,E,G1,G2
        { '3', 0x3cc0 },  // A,B,C,D,G1,G2
        { '4', 0x19c0 },  // B,C,F,G1,G2
        { '5', 0x2dc0 },  // A,C,D,F,G1,G2
        { '6', 0x2fc0 },  // A,C,D,E,F,G1,G2
        { '7', 0x3800 },  // A,B,C
        { '8', 0x3fc0 },  // A,B,C,D,E,F,G1,G2
        { '9', 0x3dc0 },  // A,B,C,D,F,G1,G2
        { ':', 0x2400 },  // A,D
        { ';', 0x2004 },  // A,K
        { '<', 0x040c },  // D,J,K
        { '=', 0x04c0 },  // D,G1,G2
        { '>', 0x0421 },  // D,H,M
        { '?', 0x3042 },  // A,B,G2,L
        { '@', 0x3750 },  // A,B,D,E,F,G2,I
        { 'A', 0x3bc0 },  // A,B,C,E,F,G1,G2
        { 'B', 0x3c52 },  // A,B,C,D,G2,I,L
        { 'C', 0x2700 },  // A,D,E,F
        { 'D', 0x3c12 },  // A,B,C,D,I,L
        { 'E', 0x2780 },  // A,D,E,F,G1
        { 'F', 0x2380 },  // A,E,F,G1
        { 'G', 0x2f40 },  // A,C,D,E,F,G2
        { 'H', 0x1bc0 },  // B,C,E,F,G1,G2
        { 'I', 0x2412 },  // A,D,I,L
        { 'J', 0x1e00 },  // B,C,D,E
        { 'K', 0x0389 },  // E,F,G1,J,M
        { 'L', 0x0700 },  // D,E,F
        { 'M', 0x1b28 },  // B,C,E,F,H,J
        { 'N', 0x1b21 },  // B,C,E,F,H,M
        { 'O', 0x3f00 },  // A,B,C,D,E,F
        { 'P', 0x33c0 },  // A,B,E,F,G1,G2
        { 'Q', 0x3f01 },  // A,B,C,D,E,F,M
        { 'R', 0x33c1 },  // A,B,E,F,G1,G2,M
        { 'S', 0x2dc0 },  // A,C,D,F,G1,G2
        { 'T', 0x2012 },  // A,I,L
        { 'U', 0x1f00 },  // B,C,D,E,F
        { 'V', 0x030c },  // E,F,J,K
        { 'W', 0x1b05 },  // B,C,E,F,K,M
        { 'X', 0x002d },  // H,J,K,M
        { 'Y', 0x002a },  // H,J,L
        { 'Z', 0x240c },  // A,D,J,K
        { '[', 0x2700 },  // A,D,E,F
        { '\\', 0x0021 },  // H,M
        { ']', 0x3c00 },  // A,B,C,D
        { '^', 0x300c },  // A,B,J,K
        { '_', 0x0400 },  // D
        { '`', 0x0020 },  // H
        { 'a', 0x0682 },  // D,E,G1,L
        { 'b', 0x0781 },  // D,E,F,G1,M
        { 'c', 0x06c0 },  // D,E,G1,G2
        { 'd', 0x1c44 },  // B,C,D,G2,K
        { 'e', 0x0684 },  // D,E,G1,K
        { 'f', 0x00ca },  // G1,G2,J,L
        { 'g', 0x3c60 },  // A,B,C,D,G2,H
        { 'h', 0x0bc0 },  // C,E,F,G1,G2
        { 'i', 0x0002 },  // L
        { 'j', 0x1c00 },  // B,C,D
        { 'k', 0x0053 },  // G2,I,L,M
        { 'l', 0x0304 },  // E,F,K
        { 'm', 0x0ac2 },  // C,E,G1,G2,L
        { 'n', 0x0ac0 },  // C,E,G1,G2
        { 'o', 0x0ec0 },  // C,D,E,G1,G2
        { 'p', 0x2388 },  // A,E,F,G1,J
        { 'q', 0x3860 },  // A,B,C,G2,H
        { 'r', 0x02c0 },  // E,G1,G2
        { 's', 0x0441 },  // D,G2,M
        { 't', 0x0780 },  // D,E,F,G1
        { 'u', 0x0e00 },  // C,D,E
        { 'v', 0x0204 },  // E,K
        { 'w', 0x0a05 },  // C,E,K,M
        { 'x', 0x00c5 },  // G1,G2,K,M
        { 'y', 0x1c60 },  // B,C,D,G2,H
        { 'z', 0x0484 },  // D,G1,K
        { '{', 0x24a4 },  // A,D,G1,H,K
        { '|', 0x0012 },  // I,L
        { '}', 0x2449 },  // A,D,G2,J,M
        { '~', 0x0128 },  // F,H,J
    };

    private void UpdateSegments()
    {
        if (CharToSegments.TryGetValue(Symbol, out var segments))
        {
            SegmentAOpacity = (segments & (1 << 13)) != 0 ? 1.0 : 0.1;
            SegmentBOpacity = (segments & (1 << 12)) != 0 ? 1.0 : 0.1;
            SegmentCOpacity = (segments & (1 << 11)) != 0 ? 1.0 : 0.1;
            SegmentDOpacity = (segments & (1 << 10)) != 0 ? 1.0 : 0.1;
            SegmentEOpacity = (segments & (1 << 9)) != 0 ? 1.0 : 0.1;
            SegmentFOpacity = (segments & (1 << 8)) != 0 ? 1.0 : 0.1;
            SegmentG1Opacity = (segments & (1 << 7)) != 0 ? 1.0 : 0.1;
            SegmentG2Opacity = (segments & (1 << 6)) != 0 ? 1.0 : 0.1;
            SegmentHOpacity = (segments & (1 << 5)) != 0 ? 1.0 : 0.1;
            SegmentIOpacity = (segments & (1 << 4)) != 0 ? 1.0 : 0.1;
            SegmentJOpacity = (segments & (1 << 3)) != 0 ? 1.0 : 0.1;
            SegmentKOpacity = (segments & (1 << 2)) != 0 ? 1.0 : 0.1;
            SegmentLOpacity = (segments & (1 << 1)) != 0 ? 1.0 : 0.1;
            SegmentMOpacity = (segments & (1 << 0)) != 0 ? 1.0 : 0.1;
        }
        else
        {
            // All segments off
            SegmentAOpacity = 0.1;
            SegmentBOpacity = 0.1;
            SegmentCOpacity = 0.1;
            SegmentDOpacity = 0.1;
            SegmentEOpacity = 0.1;
            SegmentFOpacity = 0.1;
            SegmentG1Opacity = 0.1;
            SegmentG2Opacity = 0.1;
            SegmentHOpacity = 0.1;
            SegmentIOpacity = 0.1;
            SegmentJOpacity = 0.1;
            SegmentKOpacity = 0.1;
            SegmentLOpacity = 0.1;
            SegmentMOpacity = 0.1;
        }

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

    // -----------------------------
    // For dev/debug

    //public static void DumpDic()
    //{
    //    for (int i = 32; i < 127; i++)
    //        DumpSymbol((char)i);
    //}

    //private static void DumpSymbol(char c)
    //{
    //    string[] Segments = ["A", "B", "C", "D", "E", "F", "G1", "G2", "H", "I", "J", "K", "L", "M"];
    //    Debug.Write("{ '");
    //    if (c is '\'' or '\\')
    //        Debug.Write("\\");
    //    Debug.Write(c);
    //    Debug.Write($"', 0x{CharToSegments[c]:x4}");
    //    Debug.Write(" },  // ");
    //    var s = Enumerable.Range(0, 14)
    //                .Where(b => (CharToSegments[c] & (1 << (13 - b))) != 0)
    //                .Select(b => Segments[b]);
    //    Debug.WriteLine(string.Join(",", s));
    //}

    private void Segment_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        //if (sender is Polygon p)
        //{
        //    int b = p.Name switch
        //    {
        //        "SegmentA" => 13,
        //        "SegmentB" => 12,
        //        "SegmentC" => 11,
        //        "SegmentD" => 10,
        //        "SegmentE" => 9,
        //        "SegmentF" => 8,
        //        "SegmentG1" => 7,
        //        "SegmentG2" => 6,
        //        "SegmentH" => 5,
        //        "SegmentI" => 4,
        //        "SegmentJ" => 3,
        //        "SegmentK" => 2,
        //        "SegmentL" => 1,
        //        "SegmentM" => 0,
        //        _ => -1
        //    };
        //    if (b >= 0)
        //        Update(b);
        //}
    }

    //private void Update(int b)
    //{
    //    if (!CharToSegments.TryGetValue(Symbol, out int val))
    //        return;
    //    if ((val & (1 << b)) == 0)
    //        val |= 1 << b;
    //    else
    //        val &= ~(1 << b);
    //    CharToSegments[Symbol] = val;
    //    UpdateSegments();
    //    DumpSymbol(Symbol);
    //}
}
