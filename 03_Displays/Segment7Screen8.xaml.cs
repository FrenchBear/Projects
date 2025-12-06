// Segment7Display8 UserControl
// Implement a display of eight standard 7-segment units
//
// 2025-12-06   PV

namespace Displays;

public partial class Segment7Screen8: UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // ----

    protected void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(string), typeof(Segment7Screen8), new PropertyMetadata("", OnValueChanged));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((Segment7Screen8)d).UpdateScreen();

    // ----

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register("Variant", typeof(int), typeof(Segment7Screen8),
        new PropertyMetadata(0, new PropertyChangedCallback(OnVariantChanged)));

    public int Variant
    {
        get => (int)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((Segment7Screen8)d).UpdateScreen();

    // ----

    public Segment7Screen8()
    {
        InitializeComponent();
        Symbols[0] = S0;
        Symbols[1] = S1;
        Symbols[2] = S2;
        Symbols[3] = S3;
        Symbols[4] = S4;
        Symbols[5] = S5;
        Symbols[6] = S6;
        Symbols[7] = S7;
    }

    private readonly Segment7Unit[] Symbols = new Segment7Unit[8];

    private void UpdateScreen()
    {

        string v = Value ?? "";
        int iv = 0;     // Index of Value
        int id = 0;     // Index of Symbols

        for (; ; )
        {
            if (id == 8)
                return;
            if (iv == v.Length)
            {
                while (id < 8)
                {
                    Symbols[id].Symbol = ' ';
                    Symbols[id].Dot = false;
                    Symbols[id].Variant = Variant;
                    id++;
                }
                return;
            }

            char c = v[iv];
            if (c == '.')
            {
                if (iv == 0 || v[iv - 1] != '.')
                {
                    Symbols[id - 1].Dot = true;
                    iv++;
                    continue;
                }
                Symbols[id].Symbol = ' ';
                Symbols[id].Dot = true;
                Symbols[id].Variant = Variant;
            }
            else
            {
                Symbols[id].Symbol = c;
                Symbols[id].Dot = false;
                Symbols[id].Variant = Variant;
            }

            iv++;
            id++;
        }
    }
}
