using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace T59Visual
{
    public partial class MainWindow: Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                string test = @"comment:     [comment]Text 123[/comment]
invalid:     [invalid]Text 123[/invalid]
unknown:     [unknown]Text 123[/unknown]
instruction: [instruction]Text 123[/instruction]
number:      [number]Text 123[/number]
direct:      [direct]Text 123[/direct]
indirect:    [indirect]Text 123[/indirect]
tag:         [tag]Text 123[/tag]
label:       [label]Text 123[/label]
address:     [address]Text 123[/address]

[comment]// Example[/comment]
[tag]@Loop:[/tag] [number]-1.6E-19[/number] [instruction]PD*[/instruction] [indirect]04[/indirect] [invalid]ZYP[/invalid] [instruction]Dsz[/instruction] [direct]12[/direct] [label]CLR[/label]";
                await webView.EnsureCoreWebView2Async();
                HtmlRender(test);
            };
        }

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        static readonly Regex reTag = new(@"\[([^]]+)\]");

        private static string Evaluator(Match match)
        {
            string tag = match.Groups[1].Value.ToLower();
#pragma warning disable IDE0046 // Convert to conditional expression
            if (tag.StartsWith('/'))
                return "</span>";
            else
                return "<span class=\"" + tag + "\">";
#pragma warning restore IDE0046 // Convert to conditional expression
        }

        private async void HtmlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await webView.EnsureCoreWebView2Async();
            HtmlRender(htmlTextBox.Text);
        }

        void HtmlRender(string s)
        {
            var processedText = reTag.Replace(s, Evaluator);

            // Replace custom tags with spans that have CSS classes
            //var processedText = htmlTextBox.Text
            //    .Replace("<", "&lt;")
            //    .Replace("\r\n", "</BR>")
            //    .Replace("\n", "</BR>")
            //    .Replace("[comment]", "<span class=\"comment\">")
            //    .Replace("[/comment]", "</span>")
            //    .Replace("[invalid]", "<span class=\"invalid\">")
            //    .Replace("[/invalid]", "</span>")
            //    .Replace("[unknown]", "<span class=\"unknown\">")
            //    .Replace("[/unknown]", "</span>")
            //    .Replace("[eof]", "<span class=\"eof\">")
            //    .Replace("[/eof]", "</span>")
            //    .Replace("[instruction]", "<span class=\"instruction\">")
            //    .Replace("[/instruction]", "</span>")
            //    .Replace("[number]", "<span class=\"number\">")
            //    .Replace("[/number]", "</span>")
            //    .Replace("[direct]", "<span class=\"direct\">")
            //    .Replace("[/direct]", "</span>")
            //    .Replace("[indirect]", "<span class=\"indirect\">")
            //    .Replace("[/indirect]", "</span>")
            //    .Replace("[tag]", "<span class=\"tag\">")
            //    .Replace("[/tag]", "</span>")
            //    .Replace("[label]", "<span class=\"label\">")
            //    .Replace("[/label]", "</span>")
            //    .Replace("[address]", "<span class=\"address\">")
            //    .Replace("[/address]", "</span>")
            //    .Replace("[linenumber]", "<span class=\"linenumber\">")
            //    .Replace("[/linenumber]", "</span>");

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
                        .opcode {{ color: #c0a080; }}
                    </style>
                </head>
                <body>{processedText}</body>
                </html>";

            webView.NavigateToString(htmlContent);
        }
    }
}