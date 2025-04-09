// ForeignNumbers
// Formatting numbers in Roman, Arabic and Chinese
//
// 2019-03-21   PV
// 2019-12-27   PV      WPF Core; Icon
// 2021-11-14   PV      Net6 C#10
// 2023-03-16   PV      Net7 C#11
// 2023-11-20   PV      Net8 C#12
// 2025-03-16   PV      Net9 C#13
// 2025-04-09   PV      Added Hebrew, Bengali and Thai

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
            HebrewTextBlock.Text = HebrewNumber(n);
            BengaliTextBlock.Text = BengaliNumber(n);
            ThaiTextBlock.Text = ThaiNumber(n);
        }
        else
        {
            ChineseTextBlock.Text = "";
            ArabicTextBlock.Text = "";
            RomanTextBlock.Text = "";
            HebrewTextBlock.Text = "";
            BengaliTextBlock.Text = "";
            ThaiTextBlock.Text = "";
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

    // https://stackoverflow.com/questions/30675226/convert-number-to-string-using-hebrew-letters
    static string HebrewNumber(int num)
    {
        if (num is <= 0 or >= 3000)
            return "---";

        var ret = new StringBuilder();  // new string('ת', num / 400));
        if (num >= 2000)
        {
            ret.Append("ב׳");
            num -= 2000;
        }
        else if (num >= 1000)
        {
            ret.Append("א'");
            num -= 1000;
        }

        if (num >= 100)
        {
            ret.Append("קרשתךםןףץ"[num / 100 - 1]);
            num %= 100;
        }
        switch (num)
        {
            // Avoid letter combinations from the Tetragrammaton
            case 16:
                ret.Append("טז");
                break;
            case 15:
                ret.Append("טו");
                break;
            default:
                if (num >= 10)
                {
                    //ret.Append("כלמנסעפצי"[num / 10 - 1]);
                    ret.Append("יכלמנסעפצ"[num / 10 - 1]);
                    num %= 10;
                }
                if (num > 0)
                    ret.Append("אבגדהוזחט"[num - 1]);
                break;
        }
        return ret.ToString();
    }

    static string BengaliNumber(int n)
    {
        string digits = "০১২৩৪৫৬৭৮৯";
        string s = n.ToString();
        for (int d = 0; d <= 9; d++)
        {
            s = s.Replace((char)('0' + d), digits[d]);
        }
        return s;
    }

    static string ThaiNumber(int n)
    {
        string digits = "๐๑๒๓๔๕๖๗๘๙";
        string s = n.ToString();
        for (int d = 0; d <= 9; d++)
        {
            s = s.Replace((char)('0' + d), digits[d]);
        }
        return s;
    }
}
