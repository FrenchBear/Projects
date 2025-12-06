namespace Displays;

public partial class DotMatrix3x5Display : UserControl
{
    private static readonly Dictionary<char, bool[,]> CharMap = new()
    {
        ['0'] = new bool[,]
        {
            { true, true, true },
            { true, false, true },
            { true, false, true },
            { true, false, true },
            { true, true, true }
        },
        ['1'] = new bool[,]
        {
            { false, true, false },
            { false, true, false },
            { false, true, false },
            { false, true, false },
            { false, true, false }
        },
        ['2'] = new bool[,]
        {
            { true, true, true },
            { false, false, true },
            { true, true, true },
            { true, false, false },
            { true, true, true }
        },
        ['3'] = new bool[,]
        {
            { true, true, true },
            { false, false, true },
            { true, true, true },
            { false, false, true },
            { true, true, true }
        },
        ['4'] = new bool[,]
        {
            { true, false, true },
            { true, false, true },
            { true, true, true },
            { false, false, true },
            { false, false, true }
        },
        ['5'] = new bool[,]
        {
            { true, true, true },
            { true, false, false },
            { true, true, true },
            { false, false, true },
            { true, true, true }
        },
        ['6'] = new bool[,]
        {
            { true, true, true },
            { true, false, false },
            { true, true, true },
            { true, false, true },
            { true, true, true }
        },
        ['7'] = new bool[,]
        {
            { true, true, true },
            { false, false, true },
            { false, false, true },
            { false, false, true },
            { false, false, true }
        },
        ['8'] = new bool[,]
        {
            { true, true, true },
            { true, false, true },
            { true, true, true },
            { true, false, true },
            { true, true, true }
        },
        ['9'] = new bool[,]
        {
            { true, true, true },
            { true, false, true },
            { true, true, true },
            { false, false, true },
            { true, true, true }
        },
        ['A'] = new bool[,]
        {
            { true, true, true },
            { true, false, true },
            { true, true, true },
            { true, false, true },
            { true, false, true }
        },
        ['B'] = new bool[,]
        {
            { true, true, false },
            { true, false, true },
            { true, true, false },
            { true, false, true },
            { true, true, false }
        },
        ['C'] = new bool[,]
        {
            { true, true, true },
            { true, false, false },
            { true, false, false },
            { true, false, false },
            { true, true, true }
        },
        ['D'] = new bool[,]
        {
            { true, true, false },
            { true, false, true },
            { true, false, true },
            { true, false, true },
            { true, true, false }
        },
        ['E'] = new bool[,]
        {
            { true, true, true },
            { true, false, false },
            { true, true, true },
            { true, false, false },
            { true, true, true }
        },
        ['F'] = new bool[,]
        {
            { true, true, true },
            { true, false, false },
            { true, true, true },
            { true, false, false },
            { true, false, false }
        },
        ['-'] = new bool[,]
        {
            { false, false, false },
            { false, false, false },
            { true, true, true },
            { false, false, false },
            { false, false, false }
        },
        ['.'] = new bool[,]
        {
            { false, false, false },
            { false, false, false },
            { false, false, false },
            { false, false, false },
            { false, true, false }
        },
        [' '] = new bool[,]
        {
            { false, false, false },
            { false, false, false },
            { false, false, false },
            { false, false, false },
            { false, false, false }
        }
    };

    private readonly Rectangle[,] dots = new Rectangle[5, 3];

    public DotMatrix3x5Display()
    {
        InitializeComponent();
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                var dot = new Rectangle
                {
                    Fill = Brushes.Blue,
                    Visibility = Visibility.Hidden,
                    Margin = new Thickness(0.15),
                };
                Grid.SetRow(dot, row);
                Grid.SetColumn(dot, col);
                DotMatrixGrid.Children.Add(dot);
                dots[row, col] = dot;
            }
        }
    }

    public static readonly DependencyProperty DigitProperty =
        DependencyProperty.Register("Digit", typeof(char), typeof(DotMatrix3x5Display),
        new PropertyMetadata(' ', new PropertyChangedCallback(OnDigitChanged)));

    public char Digit
    {
        get { return (char)GetValue(DigitProperty); }
        set { SetValue(DigitProperty, value); }
    }

    private static void OnDigitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DotMatrix3x5Display)d).UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (CharMap.TryGetValue(char.ToUpper(Digit), out bool[,]? pattern))
        {
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    dots[row, col].Visibility = pattern[row, col] ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }
        else
        {
            // Hide all if character not found
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    dots[row, col].Visibility = Visibility.Hidden;
                }
            }
        }
    }
}
