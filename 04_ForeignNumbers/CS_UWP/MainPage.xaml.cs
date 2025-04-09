// ForeignNumbers UWP
// Formatting numbers in Arabic and Chinese
//
// 2019-03-21   PV
// 2019-04-07   PV      UWP version
// 2023-05-21   PV      Refresh; Hebrew; AccessKeys

using System;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//#pragma warning disable IDE0060 // Remove unused parameter

namespace ForeignNumbersUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage: Page
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
                WesternTextBlock.Focus(FocusState.Programmatic);
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
            => GenerateRandom();

        private void WesternTextBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(WesternTextBlock.Text, out int n))
            {
                ChineseTextBlock.Text = ChineseNumber(n);
                ArabicTextBlock.Text = ArabicNumber(n);
                RomanTextBlock.Text = RomanNumber(n);
                HebrewTextBlock.Text = HebrewNumber(n);
            }
            else
            {
                ChineseTextBlock.Text = "";
                ArabicTextBlock.Text = "";
                RomanTextBlock.Text = "";
                HebrewTextBlock.Text = "";
            }
        }

        void GenerateRandom()
        {

            var rnd = new Random();
            int n = rnd.Next(1, 10000);
            WesternTextBlock.Text = $"{n}";
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
            if (n <= 0 || n >= 10_000)
                return "---";

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
                    sb.Append(digits.Substring(t, 1));
                sb.Append(thousand);
            }
            if (n >= 100)
            {
                int h = n / 100;
                n %= 100;
                if (h > 1)
                    sb.Append(digits.Substring(h, 1));
                sb.Append(hundred);
            }
            if (n >= 10)
            {
                int t = n / 10;
                n %= 10;
                if (t > 1)
                    sb.Append(digits.Substring(t, 1));
                sb.Append(ten);
            }
            if (n >= 1)
                sb.Append(digits.Substring(n, 1));

            return sb.ToString();
        }

        static string RomanNumber(int n)
        {
            if (n <= 0 || n >= 4_000_000)
                return "---";

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

            string barre = "\u0305";

            RT(1000000, 9000000, "M" + barre, "?", "?");
            RT(100000, 900000, "C" + barre, "D" + barre, "M" + barre);
            RT(10000, 90000, "X" + barre, "L" + barre, "C" + barre);
            RT(1000, 9000, "M", "V" + barre, "X" + barre);
            RT(100, 900, "C", "D", "M");
            RT(10, 90, "X", "L", "C");
            RT(1, 9, "I", "V", "X");

            return sb.ToString();
        }

        // https://stackoverflow.com/questions/30675226/convert-number-to-string-using-hebrew-letters
        static string HebrewNumber(int num)
        {
            if (num <= 0 || num >= 3000)
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

    }
}
