// Matrix3x5Display8 UserControl
// Implement a display of eight standard 3x5 units
//
// 2025-12-06   PV

namespace Displays;

public partial class Matrix3x5Screen8: UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(string), typeof(Matrix3x5Screen8), new PropertyMetadata("", OnValueChanged));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((Matrix3x5Screen8)d).UpdateScreen();

    public Matrix3x5Screen8()
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

    private readonly Matrix3x5Unit[] Digits = new Matrix3x5Unit[8];

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
                    Digits[id++].Digit = ' ';
                return;
            }

            Digits[id++].Digit = v[iv++];
        }
    }
}