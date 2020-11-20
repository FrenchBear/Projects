// LearningKeyboard Settings Window
//
// 2017-09-21   PV
// 2017-10-20   PV      TextRendering and TextFormatting
// 2017-12-22   PV      AlwaysOnTop
// 2020-11-17   PV      C#8, nullable enable
// 2020-11-20   PV      Scale option

using System;
using System.Windows;
using System.Windows.Input;

#nullable enable


namespace LearningKeyboard
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    internal partial class SettingsWindow : Window
    {
        internal string ColorScheme;
        internal string TextFormatting;
        internal string TextRendering;
        internal bool AlwaysOnTop;
        internal double Scale;


        internal SettingsWindow(string colorScheme, string textFormatting, string textRendering, bool alwaysOnTop, double scale)
        {
            InitializeComponent();
            ColorScheme = colorScheme;
            TextRendering = textRendering;
            TextFormatting = textFormatting;
            AlwaysOnTop = alwaysOnTop;
            Scale = scale;

            Loaded += (s, e) =>
            {
                switch (ColorScheme)
                {
                    case "Brown": BrownOption.IsChecked = true; break;
                    case "Rainbow": RainbowOption.IsChecked = true; break;
                    case "Pastel": PastelOption.IsChecked = true; break;
                }
                switch (TextFormatting)
                {
                    case "Ideal": TextFormattingIdealOption.IsChecked = true; break;
                    case "Display": TextFormattingDisplayOption.IsChecked = true; break;
                }
                switch (TextRendering)
                {
                    case "Auto": TextRenderingAutoOption.IsChecked = true; break;
                    case "Aliased": TextRenderingAliasedOption.IsChecked = true; break;
                    case "Grayscale": TextRenderingGrayscaleOption.IsChecked = true; break;
                    case "ClearType": TextRenderingClearTypeOption.IsChecked = true; break;
                }
                AlwaysOnTopCheckBox.IsChecked = AlwaysOnTop;
                ScaleTextBox.Text = $"{(int)(100 * Scale + 0.5)}";
            };
        }

        private void CancelButton_Click(object sender, RoutedEventArgs? e)
        {
            DialogResult = false;
        }

        // Apply settings
        private void OKButton_Click(object sender, RoutedEventArgs? e)
        {
            var s = ScaleTextBox.Text;
            if (!int.TryParse(s, out int sint))
            {
                MessageBox.Show("Invalid scale, not numeric", "Learning Keyboard Options", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (sint < 25 || sint > 400)
            {
                MessageBox.Show("Invalid scale, not in 25..400 range", "Learning Keyboard Options", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Scale = sint / 100.0;

            if (BrownOption.IsChecked!.Value) ColorScheme = "Brown";
            else if (RainbowOption.IsChecked!.Value) ColorScheme = "Rainbow";
            else if (PastelOption.IsChecked!.Value) ColorScheme = "Pastel";

            if (TextFormattingIdealOption.IsChecked!.Value) TextFormatting = "Ideal";
            else if (TextFormattingDisplayOption.IsChecked!.Value) TextFormatting = "Display";

            if (TextRenderingAutoOption.IsChecked!.Value) TextRendering = "Auto";
            else if (TextRenderingAliasedOption.IsChecked!.Value) TextRendering = "Aliased";
            else if (TextRenderingGrayscaleOption.IsChecked!.Value) TextRendering = "Grayscale";
            else if (TextRenderingClearTypeOption.IsChecked!.Value) TextRendering = "ClearType";

            AlwaysOnTop = AlwaysOnTopCheckBox.IsChecked ?? false;

            Properties.Settings.Default["ColorScheme"] = ColorScheme;
            Properties.Settings.Default["TextFormatting"] = TextFormatting;
            Properties.Settings.Default["TextRendering"] = TextRendering;
            Properties.Settings.Default["AlwaysOnTop"] = AlwaysOnTop;
            Properties.Settings.Default["Scale"] = Scale;
            Properties.Settings.Default.Save();


            DialogResult = true;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Topmost = true;
        }

        private void Option_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OKButton_Click(sender, null);
        }
    }
}