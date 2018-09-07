// UniView CS WPF
// Simple prototype of an app with a window based interface
// Not implemented using MVVM pattern on purpose to keep it simple
//
// 2018-08-29   PV


using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace UniViewNS
{
    public partial class MainWindow : Window
    {
        private DataBag b;

        //class DataBag : INotifyPropertyChanged
        //{
        //    public event PropertyChangedEventHandler PropertyChanged;

        //    private void NotifyPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));


        //    private string _sourceText;
        //    public string SourceText
        //    {
        //        get { return _sourceText; }
        //        set
        //        {
        //            if (_sourceText != value)
        //            {
        //                _sourceText = value;
        //                NotifyPropertyChanged(nameof(SourceText));
        //            }
        //        }
        //    }
        //}

        // No need to support INotifyPropertyChanged since we won't update SourceText from code except at the very beginning
        // so we call Transform manually in this case
        class DataBag
        {
            public string SourceText { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            b = new DataBag();
            DataContext = b;

            Case_None.IsChecked = true;
            Norm_None.IsChecked = true;
            Seq_CN.IsChecked = true;

            b.SourceText = "Aé♫山𝄞🐗\r\nœæĳøß≤≠Ⅷﬁﬆ\r\n🐱‍🏍 🐱‍👓 🐱‍🚀 🐱‍👤 🐱‍🐉 🐱‍💻\r\n🧝 🧝‍♂️ 🧝‍♀️ 🧝🏽 🧝🏽‍♂️ 🧝🏽‍♀️";
            Transform();

            Loaded += (sender, e) => { InputText.Focus(); InputText.SelectionStart = 9999; };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private void SourceUpdated_Handler(object sender, System.Windows.Data.DataTransferEventArgs e) => Transform();

        private void Option_Click(object sender, RoutedEventArgs e) => Transform();

        private void Transform()
        {
            string s = b.SourceText;

            Regex reCodepoint = new Regex(@"{([0-9a-fA-F]+)}");
            s = reCodepoint.Replace(s, ma =>
            {
                if (int.TryParse(ma.Groups[1].ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int cp))
                    if (UnicodeData.IsValidCodepoint(cp))
                        return UnicodeData.CPtoString(cp);
                return "U+" + ma.Groups[1].ToString();
            });
            Regex reName = new Regex(@"{([^}]+)}");
            s = reName.Replace(s, ma =>
            {
                int cp = UnicodeData.GetCPFromName(ma.Groups[1].ToString());
                if (cp < 0)
                    return "<" + ma.Groups[1] + ">";
                return UnicodeData.CPtoString(cp);
            });

            if (Seq_CN.IsChecked.Value)
            {
                if (Case_Lower.IsChecked.Value)
                    s = s.ToLower();
                else if (Case_Upper.IsChecked.Value)
                    s = s.ToUpper();
            }

            if (Norm_NFC.IsChecked.Value)
                s = s.Normalize(NormalizationForm.FormC);
            else if (Norm_NFD.IsChecked.Value)
                s = s.Normalize(NormalizationForm.FormD);
            else if (Norm_NFKC.IsChecked.Value)
                s = s.Normalize(NormalizationForm.FormKC);
            else if (Norm_NFKD.IsChecked.Value)
                s = s.Normalize(NormalizationForm.FormKD);

            if (Seq_NC.IsChecked.Value)
            {
                if (Case_Lower.IsChecked.Value)
                    s = s.ToLower();
                else if (Case_Upper.IsChecked.Value)
                    s = s.ToUpper();
            }

            ResultText.Text = s;

            // // Convert string into array of Unicode codepoints using 32-bit integers
            // byte[] tb = Encoding.UTF32.GetBytes(s);
            // int[] codepoints = new int[tb.Length / sizeof(int)];
            // Buffer.BlockCopy(tb, 0, codepoints, 0, tb.Length);

            CharDetailsList.Items.Clear();
            foreach (var cd in GetCharDetails(s))
                CharDetailsList.Items.Add(cd);

            // Resize columns
            foreach (var c in (CharDetailsList.View as GridView).Columns)
            {
                if (double.IsNaN(c.Width))
                    c.Width = c.ActualWidth;
                c.Width = double.NaN;
            }
        }


        private IList<CharDetail> GetCharDetails(string s)
        {
            var l = new List<CharDetail>();
            for (int i = 0; i < s.Length; i++)
            {
                char c1 = s[i];
                char c2;
                int cp = (int)c1;
                if (cp >= 0xD800 && cp <= 0xDBFF)
                {
                    c2 = s[++i];
                    cp = 0x10000 + ((c1 & 0x3ff) << 10) + (0x3ff & (int)c2);
                }

                var cd = new CharDetail(cp);
                l.Add(cd);
            }
            return l;
        }

/*
        System.Windows.Threading.DispatcherTimer dispatcherTimer;

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("dispatcherTimer_Tick");
            dispatcherTimer.Stop();
            dispatcherTimer.Tick -= dispatcherTimer_Tick;
            dispatcherTimer = null;
        }
        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            Debug.WriteLine("TextChanged");

            if (dispatcherTimer == null)
            {
                Debug.WriteLine("Create DispatchTimer");
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += dispatcherTimer_Tick;
                dispatcherTimer.Interval = new TimeSpan(0,0, 0, 0,500);
                Debug.WriteLine(dispatcherTimer.Interval);
            }
            else
            {
                Debug.WriteLine("Stop DispatchTimer");
                dispatcherTimer.Stop();
            }

            Debug.WriteLine("Start DispatchTimer");
            dispatcherTimer.Start();
        }
*/

        //// Not used anymore
        //public static void Send(Key key, RoutedEvent r)
        //{
        //    if (Keyboard.PrimaryDevice != null)
        //    {
        //        if (Keyboard.PrimaryDevice.ActiveSource != null)
        //        {
        //            var e = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key)
        //            {
        //                RoutedEvent = r
        //            };
        //            InputManager.Current.ProcessInput(e);

        //            // Note: Based on your requirements you may also need to fire events for:
        //            // RoutedEvent = Keyboard.PreviewKeyDownEvent
        //            // RoutedEvent = Keyboard.KeyUpEvent
        //            // RoutedEvent = Keyboard.PreviewKeyUpEvent
        //        }
        //    }
        //}


        //// Doesn't work with emoji combined with ZWJ
        //// From https://social.msdn.microsoft.com/Forums/vstudio/en-US/ddd9c850-25a6-4b99-8a43-5816a0d329a1/convert-string-to-opentype-unicode-valuesglyphs?forum=wpf
        //private IList<GlyphRun> GetGlyphRuns(string s)
        //{
        //    List<GlyphRun> glyphRuns = new List<GlyphRun>();
        //    DrawingGroup drawing = new DrawingGroup();
        //    DrawingContext drawingContext = drawing.Open();
        //    FormattedText formattedText = new FormattedText(s, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Segoe UI"), 14, Brushes.Black, Globals.m_dpiInfo.PixelsPerDip);
        //    drawingContext.DrawText(formattedText, new Point(0, 0));
        //    //drawingContext.DrawLine(new Pen(Brushes.Black, 1.0), new Point(0, 0), new Point(10, 10));
        //    drawingContext.Close();
        //    ObtainGlyphRuns(drawing, glyphRuns);
        //    return glyphRuns;


        //    void ObtainGlyphRuns(Drawing dr, List<GlyphRun> gr)
        //    {
        //        if (dr is DrawingGroup group)
        //        {
        //            // recursively go down the DrawingGroup
        //            foreach (Drawing child in group.Children)
        //                ObtainGlyphRuns(child, gr);
        //        }
        //        else
        //        {
        //            if (dr is GlyphRunDrawing glyphRunDrawing)
        //            {
        //                // add the glyph run to the collection
        //                GlyphRun glyphRun = glyphRunDrawing.GlyphRun;
        //                if (glyphRun != null)
        //                    gr.Add(glyphRun);
        //            }
        //        }
        //    }
        //}
    }


    // Helper class for ListView and binding
    internal class CharDetail
    {
        readonly int cp;

        public CharDetail(int cp)
        {
            this.cp = cp;
        }

        public string Codepoint => $"U+{cp:X4}";

        public string Name => UnicodeData.GetName(cp);

        public string Category => UnicodeData.GetCategory(cp);

        public string UTF16 => cp <= 0xD7FF || (cp >= 0xE000 && cp <= 0xFFFF) ? cp.ToString("X4") : (0xD800 + ((cp - 0x10000) >> 10)).ToString("X4") + " " + (0xDC00 + (cp & 0x3ff)).ToString("X4");

        public string UTF8
        {
            get
            {
                if (cp <= 0x7F)
                    return $"{cp:X2}";
                else if (cp <= 0x7FF)
                    return $"{0xC0 + cp / 0x40:X2} {0x80 + cp % 0x40:X2}";
                else if (cp <= 0xFFFF)
                    return $"{0xE0 + (cp / 0x40) / 0x40:X2} {0x80 + (cp / 0x40) % 0x40:X2} {0x80 + cp % 0x40:X2}";
                else if (cp <= 0x1FFFFF)
                    return $"{0xF0 + ((cp / 0x40) / 0x40) / 0x40:X2} {0x80 + ((cp / 0x40) / 0x40) % 0x40:X2} {0x80 + (cp / 0x40) % 0x40:X2} {0x80 + cp % 0x40:X2}";
                return "?{cp}?";
            }
        }
    }
}
