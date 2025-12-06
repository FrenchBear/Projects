// SevenSegmentDisplay8 UserControl
// Implement a display of eight standard 7-segment units
//
// 2025-12-06   PV

namespace Displays;

public partial class SevenSegmentScreen8: UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(string), typeof(SevenSegmentScreen8), new PropertyMetadata("", OnValueChanged));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((SevenSegmentScreen8)d).UpdateScreen();

    public SevenSegmentScreen8()
    {
        InitializeComponent();
        Digits[0] = D0;
        Digits[1] = D1;
        Digits[2] = D2;
        Digits[3] = D3;
        Digits[4] = D4;
        Digits[5] = D5;
        Digits[6] = D6;
        Digits[7] = D7;
    }

    private readonly SevenSegmentUnit[] Digits = new SevenSegmentUnit[8];

    private void UpdateScreen()
    {
        string v = Value ?? "";
        int iv = 0;     // Index of Value
        int id = 0;     // Index of Digits

        for (; ; )
        {
            if (id == 8)
                return;
            if (iv == v.Length)
            {
                while (id < 8)
                {
                    Digits[id].Digit = ' ';
                    Digits[id].Dot = false;
                    id++;
                }
                return;
            }

            char c = v[iv];
            if (c == '.')
            {
                if (iv == 0 || v[iv - 1] != '.')
                {
                    Digits[id - 1].Dot = true;
                    iv++;
                    continue;
                }
                Digits[id].Digit = ' ';
                Digits[id].Dot = true;
            }
            else
            {
                Digits[id].Digit = c;
                Digits[id].Dot = false;
            }

            iv++;
            id++;
        }
    }
}
