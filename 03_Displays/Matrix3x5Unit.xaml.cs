// Matrix3x5Unit UserControl
// Implement a standard 3x5 display
//
// 2025-12-06   PV

// Path Markup Syntax:
// https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/path-markup-syntax

namespace Displays;

public partial class Matrix3x5Unit: UserControl
{
    const bool F = false;
    const bool T = true;

    private static readonly Dictionary<char, bool[,]> CharMap = new()
    {
        [' '] = new bool[,] { { F, F, F }, { F, F, F }, { F, F, F }, { F, F, F }, { F, F, F } },
        ['!'] = new bool[,] { { F, T, F }, { F, T, F }, { F, T, F }, { F, F, F }, { F, T, F } },
        ['"'] = new bool[,] { { T, F, T }, { T, F, T }, { F, F, F }, { F, F, F }, { F, F, F } },
        ['#'] = new bool[,] { { T, F, T }, { T, T, T }, { T, F, T }, { T, T, T }, { T, F, T } },
        ['$'] = new bool[,] { { F, T, T }, { T, T, F }, { F, T, F }, { F, T, T }, { T, T, F } },
        ['%'] = new bool[,] { { T, F, F }, { F, F, T }, { F, T, F }, { T, F, F }, { F, F, T } },
        ['&'] = new bool[,] { { F, T, F }, { T, F, T }, { F, T, F }, { T, F, F }, { F, T, T } },
        ['\''] = new bool[,] { { F, T, F }, { T, F, F }, { F, F, F }, { F, F, F }, { F, F, F } },
        ['('] = new bool[,] { { F, F, T }, { F, T, F }, { F, T, F }, { F, T, F }, { F, F, T } },
        [')'] = new bool[,] { { T, F, F }, { F, T, F }, { F, T, F }, { F, T, F }, { T, F, F } },
        ['*'] = new bool[,] { { F, F, F }, { T, F, T }, { F, T, F }, { T, F, T }, { F, F, F } },
        ['+'] = new bool[,] { { F, F, F }, { F, T, F }, { T, T, T }, { F, T, F }, { F, F, F } },
        [','] = new bool[,] { { F, F, F }, { F, F, F }, { F, F, F }, { F, T, F }, { T, F, F } },
        ['-'] = new bool[,] { { F, F, F }, { F, F, F }, { T, T, T }, { F, F, F }, { F, F, F } },
        ['.'] = new bool[,] { { F, F, F }, { F, F, F }, { F, F, F }, { F, F, F }, { F, T, F } },
        ['/'] = new bool[,] { { F, F, F }, { F, F, T }, { F, T, F }, { T, F, F }, { F, F, F } },
        ['0'] = new bool[,] { { T, T, T }, { T, F, T }, { T, F, T }, { T, F, T }, { T, T, T } },
        ['1'] = new bool[,] { { F, T, F }, { F, T, F }, { F, T, F }, { F, T, F }, { F, T, F } },
        ['2'] = new bool[,] { { T, T, T }, { F, F, T }, { T, T, T }, { T, F, F }, { T, T, T } },
        ['3'] = new bool[,] { { T, T, T }, { F, F, T }, { T, T, T }, { F, F, T }, { T, T, T } },
        ['4'] = new bool[,] { { T, F, T }, { T, F, T }, { T, T, T }, { F, F, T }, { F, F, T } },
        ['5'] = new bool[,] { { T, T, T }, { T, F, F }, { T, T, T }, { F, F, T }, { T, T, T } },
        ['6'] = new bool[,] { { T, T, T }, { T, F, F }, { T, T, T }, { T, F, T }, { T, T, T } },
        ['7'] = new bool[,] { { T, T, T }, { F, F, T }, { F, F, T }, { F, F, T }, { F, F, T } },
        ['8'] = new bool[,] { { T, T, T }, { T, F, T }, { T, T, T }, { T, F, T }, { T, T, T } },
        ['9'] = new bool[,] { { T, T, T }, { T, F, T }, { T, T, T }, { F, F, T }, { T, T, T } },
        [':'] = new bool[,] { { F, F, F }, { F, T, F }, { F, F, F }, { F, T, F }, { F, F, F } },
        [';'] = new bool[,] { { F, F, F }, { F, T, F }, { F, F, F }, { F, T, F }, { T, F, F } },
        ['<'] = new bool[,] { { F, F, T }, { F, T, F }, { T, F, F }, { F, T, F }, { F, F, T } },
        ['='] = new bool[,] { { F, F, F }, { T, T, T }, { F, F, F }, { T, T, T }, { F, F, F } },
        ['>'] = new bool[,] { { T, F, F }, { F, T, F }, { F, F, T }, { F, T, F }, { T, F, F } },
        ['?'] = new bool[,] { { T, T, F }, { F, F, T }, { F, T, F }, { F, F, F }, { F, T, F } },
        ['@'] = new bool[,] { { F, T, F }, { T, F, T }, { T, T, F }, { T, F, F }, { F, T, T } },
        ['A'] = new bool[,] { { F, T, F }, { T, F, T }, { T, T, T }, { T, F, T }, { T, F, T } },
        ['a'] = new bool[,] { { T, T, F }, { F, F, T }, { F, T, T }, { T, F, T }, { F, T, T } },
        ['B'] = new bool[,] { { T, T, F }, { T, F, T }, { T, T, F }, { T, F, T }, { T, T, F } },
        ['b'] = new bool[,] { { T, F, F }, { T, F, F }, { T, T, F }, { T, F, T }, { T, T, F } },
        ['C'] = new bool[,] { { F, T, T }, { T, F, F }, { T, F, F }, { T, F, F }, { F, T, T } },
        ['c'] = new bool[,] { { F, F, F }, { F, F, F }, { F, T, T }, { T, F, F }, { F, T, T } },
        ['D'] = new bool[,] { { T, T, F }, { T, F, T }, { T, F, T }, { T, F, T }, { T, T, F } },
        ['d'] = new bool[,] { { F, F, T }, { F, F, T }, { F, T, T }, { T, F, T }, { F, T, T } },
        ['E'] = new bool[,] { { T, T, T }, { T, F, F }, { T, T, F }, { T, F, F }, { T, T, T } },
        ['e'] = new bool[,] { { F, T, F }, { T, F, T }, { T, T, F }, { T, F, F }, { F, T, T } },
        ['F'] = new bool[,] { { T, T, T }, { T, F, F }, { T, T, F }, { T, F, F }, { T, F, F } },
        ['f'] = new bool[,] { { F, T, F }, { T, F, F }, { T, T, F }, { T, F, F }, { T, F, F } },
        ['G'] = new bool[,] { { F, T, T }, { T, F, F }, { T, F, T }, { T, F, T }, { F, T, T } },
        ['g'] = new bool[,] { { F, T, F }, { T, F, T }, { F, T, T }, { F, F, T }, { T, T, F } },
        ['H'] = new bool[,] { { T, F, T }, { T, F, T }, { T, T, T }, { T, F, T }, { T, F, T } },
        ['h'] = new bool[,] { { T, F, F }, { T, F, F }, { T, T, F }, { T, F, T }, { T, F, T } },
        ['I'] = new bool[,] { { T, T, T }, { F, T, F }, { F, T, F }, { F, T, F }, { T, T, T } },
        ['i'] = new bool[,] { { F, T, F }, { F, F, F }, { T, T, F }, { F, T, F }, { T, T, T } },
        ['J'] = new bool[,] { { F, F, T }, { F, F, T }, { F, F, T }, { T, F, T }, { F, T, F } },
        ['j'] = new bool[,] { { F, T, F }, { F, F, F }, { F, T, T }, { F, F, T }, { T, T, F } },
        ['K'] = new bool[,] { { T, F, F }, { T, F, T }, { T, F, T }, { T, T, F }, { T, F, T } },
        ['k'] = new bool[,] { { T, F, F }, { T, F, F }, { T, F, T }, { T, T, F }, { T, F, T } },
        ['L'] = new bool[,] { { T, F, F }, { T, F, F }, { T, F, F }, { T, F, F }, { T, T, T } },
        ['l'] = new bool[,] { { T, T, F }, { F, T, F }, { F, T, F }, { F, T, F }, { T, T, T } },
        ['M'] = new bool[,] { { T, F, T }, { T, T, T }, { T, F, T }, { T, F, T }, { T, F, T } },
        ['m'] = new bool[,] { { F, F, F }, { F, F, F }, { T, F, T }, { T, T, T }, { T, F, T } },
        ['N'] = new bool[,] { { T, T, F }, { T, F, T }, { T, F, T }, { T, F, T }, { T, F, T } },
        ['n'] = new bool[,] { { F, F, F }, { F, F, F }, { T, T, F }, { T, F, T }, { T, F, T } },
        ['O'] = new bool[,] { { F, T, F }, { T, F, T }, { T, F, T }, { T, F, T }, { F, T, F } },
        ['o'] = new bool[,] { { F, F, F }, { F, F, F }, { F, T, F }, { T, F, T }, { F, T, F } },
        ['P'] = new bool[,] { { T, T, F }, { T, F, T }, { T, T, F }, { T, F, F }, { T, F, F } },
        ['p'] = new bool[,] { { F, F, F }, { T, T, F }, { T, F, T }, { T, T, F }, { T, F, F } },
        ['Q'] = new bool[,] { { F, T, T }, { T, F, T }, { F, T, T }, { F, F, T }, { F, F, T } },
        ['q'] = new bool[,] { { F, F, F }, { F, T, T }, { T, F, T }, { F, T, T }, { F, F, T } },
        ['R'] = new bool[,] { { T, T, F }, { T, F, T }, { T, T, T }, { T, T, F }, { T, F, T } },
        ['r'] = new bool[,] { { F, F, F }, { F, F, F }, { T, F, T }, { T, T, F }, { T, F, F } },
        ['S'] = new bool[,] { { F, T, T }, { T, F, F }, { F, T, F }, { F, F, T }, { T, T, F } },
        ['s'] = new bool[,] { { F, F, F }, { F, T, T }, { T, F, F }, { F, F, T }, { T, T, F } },
        ['T'] = new bool[,] { { T, T, T }, { F, T, F }, { F, T, F }, { F, T, F }, { F, T, F } },
        ['t'] = new bool[,] { { T, F, F }, { T, F, F }, { T, T, F }, { T, F, F }, { F, T, F } },
        ['U'] = new bool[,] { { T, F, T }, { T, F, T }, { T, F, T }, { T, F, T }, { F, T, F } },
        ['u'] = new bool[,] { { F, F, F }, { F, F, F }, { T, F, T }, { T, F, T }, { F, T, T } },
        ['V'] = new bool[,] { { T, F, T }, { T, F, T }, { T, F, T }, { F, T, F }, { F, T, F } },
        ['v'] = new bool[,] { { F, F, F }, { F, F, F }, { T, F, T }, { T, F, T }, { F, T, F } },
        ['W'] = new bool[,] { { T, F, T }, { T, F, T }, { T, F, T }, { T, T, T }, { T, F, T } },
        ['w'] = new bool[,] { { F, F, F }, { F, F, F }, { T, F, T }, { T, T, T }, { T, F, T } },
        ['X'] = new bool[,] { { T, F, T }, { T, F, T }, { F, T, F }, { T, F, T }, { T, F, T } },
        ['x'] = new bool[,] { { F, F, F }, { F, F, F }, { T, F, T }, { F, T, F }, { T, F, T } },
        ['Y'] = new bool[,] { { T, F, T }, { T, F, T }, { F, T, F }, { F, T, F }, { F, T, F } },
        ['y'] = new bool[,] { { F, F, F }, { T, F, T }, { F, T, T }, { F, F, T }, { T, T, F } },
        ['Z'] = new bool[,] { { T, T, T }, { F, F, T }, { F, T, F }, { T, F, F }, { T, T, T } },
        ['z'] = new bool[,] { { F, F, F }, { F, F, F }, { T, T, T }, { F, T, F }, { T, T, T } },
        ['['] = new bool[,] { { F, T, T }, { F, T, F }, { F, T, F }, { F, T, F }, { F, T, T } },
        ['\\'] = new bool[,] {{ F, F, F }, { T, F, F }, { F, T, F }, { F, F, T }, { F, F, F } },
        [']'] = new bool[,] { { T, T, F }, { F, T, F }, { F, T, F }, { F, T, F }, { T, T, F } },
        ['^'] = new bool[,] { { F, T, F }, { T, F, T }, { F, F, F }, { F, F, F }, { F, F, F } },
        ['_'] = new bool[,] { { F, F, F }, { F, F, F }, { F, F, F }, { F, F, F }, { T, T, T } },
        ['`'] = new bool[,] { { F, T, F }, { F, F, T }, { F, F, F }, { F, F, F }, { F, F, F } },
        ['{'] = new bool[,] { { F, F, T }, { F, T, F }, { T, T, F }, { F, T, F }, { F, F, T } },
        ['|'] = new bool[,] { { F, T, F }, { F, T, F }, { F, T, F }, { F, T, F }, { F, T, F } },
        ['}'] = new bool[,] { { T, F, F }, { F, T, F }, { F, T, T }, { F, T, F }, { T, F, F } },
        ['~'] = new bool[,] { { F, F, F }, { T, F, F }, { T, T, T }, { F, F, T }, { F, F, F } },
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
#pragma warning disable IDE0045 // Convert to conditional expression
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
#pragma warning restore IDE0045 // Convert to conditional expression
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
        if (!CharMap.TryGetValue(Symbol, out bool[,]? pattern))
            pattern = CharMap[' '];

        for (int row = 0; row < 5; row++)
            for (int col = 0; col < 3; col++)
                dots[row, col].Opacity = pattern[row, col] ? 1.0 : 0.1;
    }
}
