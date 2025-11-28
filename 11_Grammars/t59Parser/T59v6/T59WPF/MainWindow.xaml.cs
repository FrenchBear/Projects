using System.Windows;
using System.Windows.Controls;

namespace T59Visual
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                await webView.EnsureCoreWebView2Async();
                webView.NavigateToString("");
            };
        }

        private async void HtmlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await webView.EnsureCoreWebView2Async();

            // ToDo: Use regex

            // Replace custom tags with spans that have CSS classes
            var processedText = htmlTextBox.Text
                .Replace("<", "&lt;")
                .Replace("\r\n", "</BR>")
                .Replace("\n", "</BR>")
                .Replace("[comment]", "<span class=\"comment\">")
                .Replace("[/comment]", "</span>")
                .Replace("[invalid]", "<span class=\"invalid\">")
                .Replace("[/invalid]", "</span>")
                .Replace("[unknown]", "<span class=\"unknown\">")
                .Replace("[/unknown]", "</span>")
                .Replace("[eof]", "<span class=\"eof\">")
                .Replace("[/eof]", "</span>")
                .Replace("[instruction]", "<span class=\"instruction\">")
                .Replace("[/instruction]", "</span>")
                .Replace("[number]", "<span class=\"number\">")
                .Replace("[/number]", "</span>")
                .Replace("[direct]", "<span class=\"direct\">")
                .Replace("[/direct]", "</span>")
                .Replace("[indirect]", "<span class=\"indirect\">")
                .Replace("[/indirect]", "</span>")
                .Replace("[tag]", "<span class=\"tag\">")
                .Replace("[/tag]", "</span>")
                .Replace("[label]", "<span class=\"label\">")
                .Replace("[/label]", "</span>")
                .Replace("[address]", "<span class=\"address\">")
                .Replace("[/address]", "</span>")
                .Replace("[linenumber]", "<span class=\"linenumber\">")
                .Replace("[/linenumber]", "</span>");

            // HTML content with an embedded CSS stylesheet for styling
            var htmlContent = $@"
                <html>
                <head>
                    <style>
                        body {{
                            color: #D0D0D0; /* Light gray text color */
                            font-family: Consolas;
                            background-color: #1e1e1e; /* A dark background similar to code editors */
                            white-space: pre-wrap; /* Preserve whitespace and wrap lines */
                        }}
                        .comment {{ color: #40c040; }}
                        .invalid {{ color: #ff4040; }}
                        .unknown {{ color: #ff0000; }}
                        .eof {{ color: #ffffff; }}
                        .instruction {{ color: #80c0ff; }}
                        .number {{ color: #d2d2d2; }}
                        .direct {{ color: #ffc0ff; }}
                        .indirect {{ color: #ff80ff; }}
                        .tag {{ color: #ffc080; }}
                        .label {{ color: #ffc000; }}
                        .address {{ color: #ff9000; }}
                        .linenumber {{ color: #a0a0a0; }}
                    </style>
                </head>
                <body>{processedText}</body>
                </html>";

            webView.NavigateToString(htmlContent);
        }
    }
}