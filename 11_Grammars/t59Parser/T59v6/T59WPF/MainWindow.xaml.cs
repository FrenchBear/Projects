using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using T59v6Core;

namespace T59v6WPF;

public partial class MainWindow: Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            //                string test = @"comment:     [comment]Text 123[/comment]
            //invalid:     [invalid]Text 123[/invalid]
            //unknown:     [unknown]Text 123[/unknown]
            //instruction: [instruction]Text 123[/instruction]
            //number:      [number]Text 123[/number]
            //direct:      [direct]Text 123[/direct]
            //indirect:    [indirect]Text 123[/indirect]
            //tag:         [tag]Text 123[/tag]
            //label:       [label]Text 123[/label]
            //address:     [address]Text 123[/address]

            //[comment]// Example[/comment]
            //[tag]@Loop:[/tag] [number]-1.6E-19[/number] [instruction]PD*[/instruction] [indirect]04[/indirect] [invalid]ZYP[/invalid] [instruction]Dsz[/instruction] [direct]12[/direct] [label]CLR[/label]";
            //                await webView1.EnsureCoreWebView2Async();
            //                HtmlRender(test);

            htmlTextBox.Text = "// Initial comment\r\nLbl CLR STO 12 @Loop3: STO IND 12 Lbl Σ+ GTO CLR GTO 25 GTO 123 Lbl 25 GTO 00 10 ZYP 123456 GTO @Tag Sto Ind Ind 12 Nop Sto Sin e^x INV SBR";

            //await webView1.EnsureCoreWebView2Async();
            //webView1.NavigateToString("");
            //await webView2.EnsureCoreWebView2Async();
            //webView2.NavigateToString("");
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
        var programs = T59Processor.GetPrograms(htmlTextBox.Text);
        var out1 = HtmlRender(programs[0].OriginalColorizedTagged());
        var out2 = HtmlRender(programs[0].L3ReformattedTagged());

        await webView1.EnsureCoreWebView2Async();
        webView1.NavigateToString(out1);
        await webView2.EnsureCoreWebView2Async();
        webView2.NavigateToString(out2);
    }

    static string HtmlRender(string s)
    {
        var processedText = reTag.Replace(s, Evaluator);

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

        return htmlContent;
    }
}