// T59v6WPF - Visual rendering of colorized/reformatted T59 source code
// It's a WPF app, but rendering is done in HTML using WebView2 component
//
// 2025-11-29   PV

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using T59v6Core;

namespace T59v6WPF;

public partial class MainWindow: Window
{
    private bool IsDirty;
    private string? FileName;

    const string AppName = "T59 v6 WPF";

    public static readonly RoutedUICommand TestColors = new("_Test Colors", "TestColors", typeof(MainWindow));
    public static readonly RoutedUICommand About = new("À propos de...", "About", typeof(MainWindow), [new KeyGesture(Key.F1)]);

    public MainWindow()
    {
        InitializeComponent();

        CommandBindings.Add(new CommandBinding(TestColors, TestColorsExecuted));
        CommandBindings.Add(new CommandBinding(About, AboutExecuted));
    }

    private void AboutExecuted(object sender, ExecutedRoutedEventArgs e)
        => MessageBox.Show("Simple ANTRL4 lexer + my own grammar parser to validate, encode and colorize a TI-58C/59 program", AppName, MessageBoxButton.OK, MessageBoxImage.Information);

    private async void TestColorsExecuted(object sender, ExecutedRoutedEventArgs e)
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
        var out2 = HtmlRender(test);
        await webView2.EnsureCoreWebView2Async();
        webView2.NavigateToString(out2);
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

    private async void SourceTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        IsDirty = true;
        UpdateTitle();

        var programs = T59Processor.GetPrograms(SourceTextBox.Text);
        if (programs.Count == 0)
        {
            StatusMessage.Text = "";
            DisplayHTML("", "");
            return;
        }
        StatusMessage.Text = programs.Count > 1 ? "More than 1 program, only the first one is reformatted" : "";

        // First panel, source colorized, preserving original layout
        var out1 = HtmlRender(programs[0].OriginalColorizedTagged());

        // Second panel, reformatted source code with labels and errors lists
        var t2 = programs[0].L3ReformattedTagged().Replace("<", "&lt;");
        var x = programs[0].LabelsTagged().Replace("<", "&lt;");
        if (x.Length > 0)
            t2 += "<H2>Labels</H2>" + x;
        x = programs[0].ErrorsTagged().Replace("<", "&lt;");
        if (x.Length > 0)
            t2 += "<H2>Errors</H2>" + x;
        var out2 = HtmlRender(t2);

        DisplayHTML(out1, out2);
    }

    private void UpdateTitle()
    {
        string msg = IsDirty ? "*" : "";
        if (FileName != null)
            msg += FileName;
        if (msg != "")
            msg += " – ";
        msg += AppName;
        Title = msg;
    }

    private async void DisplayHTML(string out1, string out2)
    {
        await webView1.EnsureCoreWebView2Async();
        webView1.NavigateToString(out1);
        await webView2.EnsureCoreWebView2Async();
        webView2.NavigateToString(out2);
    }

    static string HtmlRender(string s)
    {
        var processedText = reTag.Replace(s, Evaluator).Replace("\r\n", "<P>").Replace("\n", "<P>");

        var htmlContent = $@"
<html>
<head>
	<style>
		body {{
			background-color: #1e1e1e;
			color: #D0D0D0;
			font-family: Consolas;
			font-size: 11pt;
			white-space: pre-wrap;
		}}
        p {{ margin-top: 0; margin-bottom: 0; line-height: 1.2em; }}
        p:empty::before {{ content: ""\00a0""; }}
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

    protected override void OnClosing(CancelEventArgs e)
    {
        if (IsDirty)
            e.Cancel = !CanContinue();
        else
            base.OnClosing(e);
    }

    private void NewExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (!CanContinue())
            return;
        SourceTextBox.Text = "";
        FileName = null;
        IsDirty = false;
        UpdateTitle();
    }

    private bool CanContinue()
    {
        if (IsDirty)
        {
            var r = MessageBox.Show("Le programme a été modifié mais pas enregistré.\r\nVoulez-vous conserver ces changements?", "T59v6WPF", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.None);
            return MessageBoxResult.No == r;
        }
        else
            return true;
    }

    private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (!CanContinue())
            return;

        // Configure open file dialog box
        Microsoft.Win32.OpenFileDialog dlg = new()
        {
            FileName = "", // Default file name
            DefaultExt = ".t59", // Default file extension
            Filter = "Ti 58C/59 programs|*.t59|Tous les fichiers (*.*)|*.*"
        };

        // Show open file dialog box
        bool? result = dlg.ShowDialog();

        // Process open file dialog box results
        if (result == true)
        {
            try
            {
                using StreamReader sr = new(dlg.FileName, Encoding.UTF8);
                SourceTextBox.Text = sr.ReadToEnd();
                IsDirty = false;
                FileName = dlg.FileName;
                UpdateTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur est survenue lors de l'ouverture du fichier " + dlg.FileName + ": " + ex.Message, "T59v6WPF", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }

    private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        => Close();

    private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (FileName == null)
            SaveAsExecuted(sender, e);
        else
        {
            try
            {
                using StreamWriter sw = new(FileName, false, Encoding.UTF8);
                sw.Write(SourceTextBox.Text);
                IsDirty = false;
                UpdateTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur est survenue lors de l'écriture du fichier " + FileName + ": " + ex.Message, "T59v6WPF", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

    }

    private void SaveAsExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        // Configure save file dialog box
        Microsoft.Win32.SaveFileDialog dlg = new()
        {
            FileName = "", // Default file name
            DefaultExt = ".t59", // Default file extension
            Filter = "Ti 58C/59 programs|*.t59|Tous les fichiers (*.*)|*.*"  // Filter files by extension
        };

        // Show save file dialog box
        bool? result = dlg.ShowDialog();

        // Process save file dialog box results
        if (result == true)
        {
            // Save program
            FileName = dlg.FileName;
            SaveExecuted(sender, e);
        }
    }
}
