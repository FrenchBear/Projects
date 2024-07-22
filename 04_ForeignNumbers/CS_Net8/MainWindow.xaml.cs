// ForeignNumbers
// Formatting numbers in Roman, Arabic and Chinese
//
// 2019-03-21   PV
// 2019-12-27   PV      WPF Core; Icon
// 2021-11-14   PV      Net6 C#10
// 2023-03-16   PV      Net7 C#11
// 2023-11-20   PV      Net8 C#12

//#pragma warning disable IDE0060 // Remove unused parameter

using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace IntlNumbersWPFCore;

public partial class MainWindow: Window
{
    public MainWindow()
    {
        // Check conversion algorithms
        //Debug.WriteLine(RomanNumber(1_000_000));
        //Debug.WriteLine(RomanToInt("C̄D̄"));
        //for (int i = 0; i < 4_000_000; i++)
        //{
        //    if (RomanToInt(RomanNumber(i))!=i)
        //        Debugger.Break();
        //}

        // Longest roman number
        int lm = 0;
        int im = 0;
        for (int i = 0; i < 500000; i++)
        {
            int l = RomanNumber(i).Length;
            if (l > lm)
            {
                lm = l;
                im = i;
            }
        }
        Debug.WriteLine(lm);
        Debug.WriteLine(im);
        Debug.WriteLine(RomanNumber(im));

        InitializeComponent();
        WesternTextBlock.Focus();
    }

    private void RandomButton_Click(object sender, RoutedEventArgs e) => GenerateRandom();

    void GenerateRandom()
    {
        var rnd = new Random();
        int n = rnd.Next(1, 500_000);
        WesternTextBlock.Text = $"{n}";
    }

    private void WesternTextBlock_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(WesternTextBlock.Text, out int n))
        {
            ChineseTextBlock.Text = ChineseNumber(n);
            ArabicTextBlock.Text = ArabicNumber(n);
            RomanTextBlock.Text = RomanNumber(n);
        }
        else
        {
            ChineseTextBlock.Text = "";
            ArabicTextBlock.Text = "";
            RomanTextBlock.Text = "";
        }
    }

    static string ArabicNumber(int n)
    {
        string digits = "٠١٢٣٤٥٦٧٨٩";
        string s = n.ToString();
        for (int d = 0; d <= 9; d++)
        {
            s = s.Replace((char)('0' + d), digits[d]);
        }
        return s;
    }

    static string ChineseNumber(int n)
    {
        if (n <= 0)
            return "≤0";
        if (n >= 10_000)
            return "≥10000";

        string digits = "0一二三四五六七八九";
        string ten = "十";
        string hundred = "百";
        string thousand = "千";

        var sb = new StringBuilder();
        if (n >= 1000)
        {
            int t = n / 1000;
            n %= 1000;
            if (t > 1)
                sb.Append(digits.AsSpan(t, 1));
            sb.Append(thousand);
        }
        if (n >= 100)
        {
            int h = n / 100;
            n %= 100;
            if (h > 1)
                sb.Append(digits.AsSpan(h, 1));
            sb.Append(hundred);
        }
        if (n >= 10)
        {
            int t = n / 10;
            n %= 10;
            if (t > 1)
                sb.Append(digits.AsSpan(t, 1));
            sb.Append(ten);
        }
        if (n >= 1)
            sb.Append(digits.AsSpan(n, 1));

        return sb.ToString();
    }

    static string RomanNumber(int n)
    {
        if (n < 0)
            return "<0";
        if (n > 4_000_000)
            return "≥4 000 000";
        if (n == 0)
            return string.Empty;

        // "IVXLCDMĪV̄X̄L̄C̄D̄M̄";
        var sb = new StringBuilder();

        void RT(int min, int max, string r1, string r5, string r10)
        {
            if (n >= min)
            {
                int i = n / min;
                n %= min;

                switch (i)
                {
                    case 1:
                        sb.Append(r1);
                        break;
                    case 2:
                        sb.Append(r1);
                        sb.Append(r1);
                        break;
                    case 3:
                        sb.Append(r1);
                        sb.Append(r1);
                        sb.Append(r1);
                        break;
                    case 4:
                        sb.Append(r1);
                        sb.Append(r5);
                        break;
                    case 5:
                        sb.Append(r5);
                        break;
                    case 6:
                        sb.Append(r5);
                        sb.Append(r1);
                        break;
                    case 7:
                        sb.Append(r5);
                        sb.Append(r1);
                        sb.Append(r1);
                        break;
                    case 8:
                        sb.Append(r5);
                        sb.Append(r1);
                        sb.Append(r1);
                        sb.Append(r1);
                        break;
                    case 9:
                        sb.Append(r1);
                        sb.Append(r10);
                        break;
                }
            }
        }

        RT(1000000, 3999999, "M̄", "?", "?");
        RT(100000, 900000, "C̄", "D̄", "M̄");
        RT(10000, 90000, "X̄", "L̄", "C̄");
        RT(1000, 9000, "M", "V̄", "X̄");
        RT(100, 900, "C", "D", "M");
        RT(10, 90, "X", "L", "C");
        RT(1, 9, "I", "V", "X");

        return sb.ToString();
    }

    /*
    static readonly Dictionary<string, int> RomanDigits = new() {
        { "I", 1 },
        { "V", 5 },
        { "X", 10 },
        { "L", 50 },
        { "C", 100 },
        { "D", 500 },
        { "M", 1000 },
        { "V̄", 5000 },
        { "X̄", 10000 },
        { "L̄", 50000 },
        { "C̄", 100000 },
        { "D̄", 500000 },
        { "M̄", 1000000 },
    };

    static int RomanToInt(string romanNumber)
    {
        static IEnumerable<int> BreakRomanString(string s)
        {
            while (s.Length > 0)
            {
                if (s.Length >= 2 && RomanDigits.TryGetValue(s[0..2], out int value2))
                {
                    yield return value2;
                    s = s[2..];
                }
                else if (RomanDigits.TryGetValue(s[0..1], out int value1))
                {
                    yield return value1;
                    s = s[1..];
                }
                else
                    throw new Exception("Invalid Roman digit: " + s);
            }
            yield break;
        }

        int lastDigit = 0;
        int value = 0;
        foreach (var digit in BreakRomanString(romanNumber))
        {
            if (lastDigit < digit)
                value -= lastDigit;
            else
                value += lastDigit;
            lastDigit = digit;
        }
        value += lastDigit;
        return value;
    }
    */
}
