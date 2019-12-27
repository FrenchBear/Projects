// IntlNumbers
// Formatting numbers in Roman, Arabic and Chinese
//
// 2019-03-21   PV
// 2019-12-27   PV      WPF Core; Icon

#nullable enable
#pragma warning disable IDE0060 // Remove unused parameter

using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace IntlNumbersWPFCore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WesternTextBlock.Focus();
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateRandom();
        }

        void GenerateRandom()
        {
            var rnd = new Random();
            int n = rnd.Next(1, 10000);
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
            if (n <= 0) return "≤0";
            if (n >= 10_000) return "≥10000";

            string digits = "0一二三四五六七八九";
            string ten = "十";
            string hundred = "百";
            string thousand = "千";

            var sb = new StringBuilder();
            if (n >= 1000)
            {
                int t = n / 1000;
                n %= 1000;
                if (t > 1) sb.Append(digits.Substring(t, 1));
                sb.Append(thousand);
            }
            if (n >= 100)
            {
                int h = n / 100;
                n %= 100;
                if (h > 1) sb.Append(digits.Substring(h, 1));
                sb.Append(hundred);
            }
            if (n >= 10)
            {
                int t = n / 10;
                n %= 10;
                if (t > 1) sb.Append(digits.Substring(t, 1));
                sb.Append(ten);
            }
            if (n >= 1)
                sb.Append(digits.Substring(n, 1));

            return sb.ToString();
        }

        static string RomanNumber(int n)
        {
            if (n <= 0) return "≤0";
            if (n >= 1_000_000) return "≥1 000 000";

            // "IVXLCDMĪV̄X̄L̄C̄D̄M̄";
            StringBuilder sb = new StringBuilder();

            void RT(int min, int max, string r1, string r5, string r10)
            {
                if (n >= min)
                {
                    int i = n / min;
                    n %= min;

                    switch (i)
                    {
                        case 1: sb.Append(r1); break;
                        case 2: sb.Append(r1); sb.Append(r1); break;
                        case 3: sb.Append(r1); sb.Append(r1); sb.Append(r1); break;
                        case 4: sb.Append(r1); sb.Append(r5); break;
                        case 5: sb.Append(r5); break;
                        case 6: sb.Append(r5); sb.Append(r1); break;
                        case 7: sb.Append(r5); sb.Append(r1); sb.Append(r1); break;
                        case 8: sb.Append(r5); sb.Append(r1); sb.Append(r1); sb.Append(r1); break;
                        case 9: sb.Append(r1); sb.Append(r10); break;
                    }
                }
            }

            RT(100000, 900000, "C̄", "D̄", "M̄");
            RT(10000, 90000, "X̄", "L̄", "C̄");
            RT(1000, 9000, "M", "V̄", "X̄");
            RT(100, 900, "C", "D", "M");
            RT(10, 90, "X", "L", "C");
            RT(1, 9, "I", "V", "X");

            return sb.ToString();
        }

    }
}
