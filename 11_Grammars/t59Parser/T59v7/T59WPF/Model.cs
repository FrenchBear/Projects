// T59WPF Model
//
// 2025-12-04   PV

using System.Diagnostics;
using System.Text.RegularExpressions;
using T59v7Core;

namespace T59v7WPF;

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

internal sealed class Model
{
    private ViewModel VM = null!;

    internal void SetViewModel(ViewModel vm) => VM = vm;

    internal void ProcessSourceCode(string source)
    {
        VM.IsDirty = true;

        var sw = Stopwatch.StartNew();
        var programs = T59Processor.GetPrograms(source);
        var t = sw.ElapsedMilliseconds;
        VM.Timing1 = $"Parse: {t}ms";

        if (programs.Count == 0)
        {
            VM.StatusMessage = "";
            VM.ColorizedHtml = "";
            VM.ReformattedHtml = "";
            return;
        }

        VM.StatusMessage = programs.Count > 1 ? "More than 1 program, only the first one is reformatted" : "";

        // First panel, source colorized, preserving original layout
        sw.Restart();
        var out1 = HtmlRender(programs[0].OriginalColorizedTagged());
        t = sw.ElapsedMilliseconds;
        VM.Timing2 = $"Color: {t}ms";
        VM.ColorizedHtml = out1;

        // Second panel, reformatted source code with labels and errors lists
        sw.Restart();
        var t2 = programs[0].L3ReformattedTagged().Replace("<", "&lt;");
        var x = programs[0].LabelsTagged().Replace("<", "&lt;");
        if (x.Length > 0)
            t2 += "<H2>Labels</H2>" + x;
        x = programs[0].ErrorsTagged().Replace("<", "&lt;");
        if (x.Length > 0)
            t2 += "<H2>Errors</H2>" + x;
        var out2 = HtmlRender(t2);
        t = sw.ElapsedMilliseconds;
        VM.Timing3 = $"Refmt: {t}ms";
        VM.ReformattedHtml = out2;
    }

    static readonly Regex reTag = new(@"\[([^]]+)\]");

    private static string Evaluator(Match match)
    {
        string tag = match.Groups[1].Value.ToLower();
        return tag.StartsWith('/') ? "</span>" : "<span class=\"" + tag + "\">";
    }

    public static string HtmlRender(string s)
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
}
