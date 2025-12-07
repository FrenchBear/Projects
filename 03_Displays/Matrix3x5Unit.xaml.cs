// Matrix3x5Unit UserControl
// Implement a standard 3x5 display
//
// 2025-12-06   PV

// Path Markup Syntax:
// https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/path-markup-syntax

namespace Displays;

public partial class Matrix3x5Unit: UserControl
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
        },
        ['R'] = new bool[,]
        {
            { false, false, false },
            { false, false, false },
            { true, true, true },
            { true, false, false },
            { true, false, false }
        },
        ['O'] = new bool[,]
        {
            { false, false, false },
            { false, false, false },
            { true, true, true },
            { true, false, true },
            { true, true, true }
        }
    };

    private readonly Shape[,] dots = new Shape[5, 3];

    public Matrix3x5Unit()
    {
        InitializeComponent();
        InitializeVariant();
    }

    void InitializeVariant()
    {
        DotMatrixGrid.Children.Clear();

        // Variant 0 = all squares
        // Variant 1 = 4 rounded corners, all other square
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                Shape dot;
                if (Variant == 1 && row == 0 && col == 0)
                {
                    dot = new Path
                    {
                        Data = Geometry.Parse("M 0,10 A 10,10 90 0 1 10,0 L 10,10")
                    };
                }
                else if (Variant == 1 && row == 0 && col == 2)
                {
                    dot = new Path
                    {
                        Data = Geometry.Parse("M 0,0 A 10,10 90 0 1 10,10 L 0,10")
                    };
                }
                else if (Variant == 1 && row == 4 && col == 2)
                {
                    dot = new Path
                    {
                        Data = Geometry.Parse("M 10,0 A 10,10 90 0 1 0,10 L 0,0")
                    };
                }
                else if (Variant == 1 && row == 4 && col == 0)
                {
                    dot = new Path
                    {
                        Data = Geometry.Parse("M 10,10 A 10,10 90 0 1 0,0 L 10,0")
                    };
                }
                else
                {
                    dot = new Rectangle();
                }
                dot.Fill = Brushes.Blue;
                dot.Opacity = 0.1;
                dot.Margin = new Thickness(0.15);
                Grid.SetRow(dot, row);
                Grid.SetColumn(dot, col);
                DotMatrixGrid.Children.Add(dot);
                dots[row, col] = dot;
            }
        }
    }

    // ----
    // Displayed symbol

    public static readonly DependencyProperty SymbolProperty =
        DependencyProperty.Register("Symbol", typeof(char), typeof(Matrix3x5Unit),
        new PropertyMetadata(' ', new PropertyChangedCallback(OnSymbolChanged)));

    public char Symbol
    {
        get => (char)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((Matrix3x5Unit)d).UpdateDots();

    // ----
    // Style

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register("Variant", typeof(int), typeof(Matrix3x5Unit),
        new PropertyMetadata(0, new PropertyChangedCallback(OnVariantChanged)));

    public int Variant
    {
        get => (int)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Matrix3x5Unit)d).InitializeVariant();
        ((Matrix3x5Unit)d).UpdateDots();
    }

    // ----

    private void UpdateDots()
    {
        if (CharMap.TryGetValue(char.ToUpper(Symbol), out bool[,]? pattern))
        {
            for (int row = 0; row < 5; row++)
                for (int col = 0; col < 3; col++)
                    dots[row, col].Opacity = pattern[row, col] ? 1.0 : 0.1;
        }
        else
        {
            // Hide all if character not found
            for (int row = 0; row < 5; row++)
                for (int col = 0; col < 3; col++)
                    dots[row, col].Opacity = 0.1;
        }
    }
}
