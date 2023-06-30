﻿// Plotter - MainForm.cs
// Plotter test application using fractals generated by a L-System and optionally smoothed
//
// 2021-12-09   PV
// 2021-12-13   PV      Math using double here to avoid cumulative rounding errors, visible in Triangle fractal (PlotLib remains float)
//                      Read .l definitions
using PlotterLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace LSystemTest;

public partial class LSystemForm: Form
{
    private readonly Plotter p;

    public LSystemForm()
    {
        InitializeComponent();

        // Fill printers list
        foreach (string printer in PrinterSettings.InstalledPrinters)
            PrintersList.Items.Add(printer.ToString());
        if (PrintersList.Items.Count > 0)
            PrintersList.SelectedIndex = 0;

        // Fill smooting methods
        SmoothingMethodsComboBox.Items.AddRange(new string[] {
            "Spline interpolation",
            "Chaikin",
            "Cut corners",
            "No interpolation",
        });
        SmoothingMethodsComboBox.SelectedIndex = 3;

        // Fill L-Systems
        var Internal = new List<LSystem> {
            new LSystem("HilbertDonut", "Hilbert curve on a circle", 4, "XFXFXFXF", "X=-YF+XFX+FY-\r\nY=+XF-YFY-FX+"),
            new LSystem("Dragon", "Original Dragon curve", 8, "FX", "F=\r\nX=-FX++FY-\r\nY=+FX--FY+"),
            new LSystem("Hilbert", "Basic Hilbert curve", 4, "X", "X=-YF+XFX+FY-\r\nY=+XF-YFY-FX+"),
            new LSystem("Flocon", "Von Koch curve", 6, "F", "F=F-F++F-F"),
            new LSystem("F5", "Carré 5x5", 4, "α315F", "F=@2F-F+@I2>1F+F-F+F+<1@2F-F@I2>1-F-F+F-F+F<1"),
            new LSystem("F3", "Carré 3x3", 4, "α45F", "F=F-F+F+F+F-F-F-F+F"),
            new LSystem("Alternatif", "Courant alternatif carré", 4, "F", "F=F-F+F+FF-F-F+F"),
            new LSystem("Triangle", "Triangle évidé", 6, "--X", "X=++FXF++FXF++FXF>1\r\nF=FF"),
            new LSystem("Penrose", "Color Penrose", 10, "+WC02F--XC04F---YC04F--ZC02F", "W=YC04F++ZC02F----XC04F[-YC04F----WC02F]++\r\nX=+YC04F--ZC02F[---WC02F--XC04F]+\r\nY=-WC02F++XC04F[+++YC04F++ZC02F]-\r\nZ=--YC04F++++WC02F[+ZC02F++++XC04F]--XC04F\n\rF="),
            new LSystem("Hexa", "Hexagonal fractal (ma propre version)", 6, "α210X", "X=XF-YF--YF+XF++XFXF+YF->1\r\nY=+XF-YFYF--YF-XF++XF+YF<1\r\nF="),
        };

        List<Source> Sources = new()
        {
            new Source("(Internal)", Internal)
        };

        foreach (string file in Directory.GetFiles("Systems", "*.l"))
            Sources.Add(new Source(Path.GetFileName(file), GetLSystemsFromFile(file)));

        p = new();
        p.Output(picOut);

        foreach (var color in p.ColorsTable)
            ColorsComboBox.Items.Add(color.ToString());
        ColorsComboBox.SelectedIndex = 0;

        foreach (var source in Sources)
            SourcesComboBox.Items.Add(source);
        SourcesComboBox.SelectedIndex = 0;
    }

    private static List<LSystem> GetLSystemsFromFile(string file)
    {
        string Comments = "";
        string Name = "";
        int Angle = 0;
        string Axiom = "";
        string Rules = "";

        var list = new List<LSystem>();

        using StreamReader sr = new(file);
        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            string lineComment;
            int startComment = line.IndexOf(';');
            if (startComment >= 0)
            {
                lineComment = line[startComment..];
                line = startComment == 0 ? "" : line[..startComment];
                if (string.IsNullOrEmpty(Comments))
                    Comments = lineComment;
                else
                    Comments += "\r\n" + lineComment;
            }

            line = line.Trim();
            if (line.Length == 0)
                continue;

            int p = line.IndexOf('{');
            if (p >= 0)
            {
                line = line[..(p - 1)].Trim();
                Name = line;
                continue;
            }

            if (line.StartsWith("Angle", StringComparison.InvariantCultureIgnoreCase))
            {
                int p1 = 5;
                while (char.IsWhiteSpace(line[p1]) || line[p1] == '=')
                    p1++;
                if (int.TryParse(line[p1..], out int a))
                    Angle = a;
                continue;
            }
            else if (line.StartsWith("Axiom", StringComparison.InvariantCultureIgnoreCase))
            {
                Axiom = line[5..].Trim().ToUpperInvariant();
                continue;
            }
            else if (line.Contains('='))
            {
                if (string.IsNullOrEmpty(Rules))
                    Rules = line.ToUpperInvariant();
                else
                    Rules += "\r\n" + line.ToUpperInvariant();
                continue;
            }
            else if (line == "}")
            {
                LSystem ls = new(Name, Comments, Angle, Axiom, Rules);
                list.Add(ls);
                Comments = "";
                Name = "";
                Angle = 0;
                Axiom = "";
                Rules = "";
            }
        }

        return list;
    }

    private void SourcesComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (SourcesComboBox.SelectedItem is Source source)
        {
            LSystemsComboBox.Items.Clear();
            foreach (var ls in source.LSystems)
                LSystemsComboBox.Items.Add(ls);
            LSystemsComboBox.SelectedIndex = 0;
        }
    }

    private void LSystemsComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (LSystemsComboBox.SelectedItem is LSystem s)
        {
            IgnoreEvents = true;
            CommentTextBox.Text = s.Comment;
            AngleTextBox.Text = s.Angle.ToString();
            AxiomTextBox.Text = s.Axiom;
            RulesTextBox.Text = s.Rules;
            LSystemDrawing();
            IgnoreEvents = false;
        }
        else
        {
            p.Clear();
            p.Refresh();
        }
    }

    private void MainForm_Resize(object sender, EventArgs e)
        => p?.Refresh();

    private void PrintButton_Click(object sender, EventArgs e)
    {
        if (PrintersList.SelectedItem == null)
        {
            MessageBox.Show("Select a printer first!");
            return;
        }

        p.Print((string)PrintersList.SelectedItem);
    }

    private void SmoothingMethodsComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (SmoothingMethodsComboBox.SelectedIndex)
        {
            case 0:
                IterationsUpDown.Enabled = true;
                IterationsTrackBar.Enabled = true;
                TensionUpDown.Enabled = false;
                TensionTrackBar.Enabled = false;
                break;

            case 1:
                IterationsUpDown.Enabled = true;
                IterationsTrackBar.Enabled = true;
                TensionUpDown.Enabled = true;
                TensionTrackBar.Enabled = true;
                break;

            case 2:
                IterationsUpDown.Enabled = false;
                IterationsTrackBar.Enabled = false;
                TensionUpDown.Enabled = true;
                TensionTrackBar.Enabled = true;
                break;

            case 3:
                IterationsUpDown.Enabled = false;
                IterationsTrackBar.Enabled = false;
                TensionUpDown.Enabled = false;
                TensionTrackBar.Enabled = false;
                break;
        }
        LSystemDrawing();
    }

    private bool IgnoreEvents;

    private void IterationsTrackBar_Scroll(object sender, EventArgs e)
    {
        if (!IgnoreEvents)
            IterationsUpDown.Value = IterationsTrackBar.Value;
    }

    private void IterationsUpDown_ValueChanged(object sender, EventArgs e)
    {
        IgnoreEvents = true;
        IterationsTrackBar.Value = (int)IterationsUpDown.Value;
        IgnoreEvents = false;
        LSystemDrawing();
    }

    private void TensionTrackBar_Scroll(object sender, EventArgs e)
    {
        if (!IgnoreEvents)
            TensionUpDown.Value = (decimal)(TensionTrackBar.Value / 100.0);
    }

    private void TensionUpDown_ValueChanged(object sender, EventArgs e)
    {
        IgnoreEvents = true;
        TensionTrackBar.Value = (int)(100 * TensionUpDown.Value);
        IgnoreEvents = false;
        LSystemDrawing();
    }

    private void DepthUpDown_ValueChanged(object sender, EventArgs e)
        => LSystemDrawing();

    private void ColorsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        => LSystemDrawing();

    private void ForceMonochromeCheckBox_CheckedChanged(object sender, EventArgs e)
        => LSystemDrawing();

    private void LSystemDrawing()
    {
        if (p == null)
            return;
        if (LSystemsComboBox.SelectedItem is not LSystem ls0)
            return;
        var ls = new LSystem(ls0.Name, CommentTextBox.Text, int.Parse(AngleTextBox.Text), AxiomTextBox.Text, RulesTextBox.Text);

        Dictionary<char, string> rules = new();
        foreach (string s in ls.Rules.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            char c = s.ToUpperInvariant().Split('=')[0][0];
            string r = s.ToUpperInvariant().Split('=')[1];
            if (rules.ContainsKey(c))
                rules[c] += r;
            else
                rules.Add(c, r);
        }
        int depth = (int)DepthUpDown.Value;
        bool isMonochrome = ForceMonochromeCheckBox.Checked;

        // Apply the rules recursively to produce final drawString
        var drawString = LSystemProcessor.LSystemIterator(depth, ls.Axiom, rules);

        // Generation of points
        AngleAndPosition ap = new()
        {
            color = ColorsComboBox.SelectedIndex,
        };

        var pointsLists = new List<StrokeAndColor>();
        List<PointD> points;

        void StartNewStroke(double pX, double pY)
        {
            var sac = new StrokeAndColor();
            sac.color = ap.color;
            points = sac.points;
            points.Add(new PointD(pX, pY));
            pointsLists.Add(sac);
        }
        StartNewStroke(0, 0);

        Stack<AngleAndPosition> apStack = new();

        double nx;                          // New X position
        double ny;                          // New Y position
        double angleIncrement = 2 * Math.PI / ls.Angle;
        int generalOrientation = 1;         // General orientation, changed to -1 by !
        char escapeChar = '\0';             // Different than \0 when processing @ or \ or / escape sequence
        string escapeOptions = "";          // Accumulated I and or Q for @ sequence
        string argumentNum = "";            // Buffer to accumulate @, \ and / numeric argument
        double xmin = 0, xmax = 0, ymin = 0, ymax = 0;

        var sw = Stopwatch.StartNew();

        foreach (char c in drawString)
        {
            // Max 2 seconds of rendering to avoir issues with too complex drawings
            if (sw.ElapsedMilliseconds > 2000)
                break;

            if (escapeChar != '\0')
            {
                if (c is >= '0' and <= '9' || c == '.')
                {
                    argumentNum += c;
                    continue;
                }
                switch (escapeChar)
                {
                    case 'α':
                        ap.Angle = -generalOrientation * double.Parse(argumentNum, CultureInfo.InvariantCulture) * Math.PI / 180;
                        escapeChar = '\0';
                        break;

                    case '/':
                        ap.DirectAngle -= generalOrientation * double.Parse(argumentNum, CultureInfo.InvariantCulture) * Math.PI / 180;
                        escapeChar = '\0';
                        break;

                    case '\\':
                        ap.DirectAngle += generalOrientation * double.Parse(argumentNum, CultureInfo.InvariantCulture) * Math.PI / 180;
                        escapeChar = '\0';
                        break;

                    case '@':
                        if (c == 'I' || c == 'Q')
                        {
                            escapeOptions += c;
                            continue;
                        }
                        double f = double.Parse(argumentNum, CultureInfo.InvariantCulture);
                        if (escapeOptions == "IQ")
                            f = 1.0 / Math.Sqrt(f);
                        else if (escapeOptions == "QI")
                            f = Math.Sqrt(1.0 / f);
                        else if (escapeOptions == "I")
                            f = 1.0 / f;
                        else if (escapeOptions == "Q")
                            f = Math.Sqrt(f);
                        ap.SegmentLength *= f;
                        escapeChar = '\0';
                        break;

                    case 'C':
                        if (!isMonochrome)
                        {
                            ap.color = int.Parse(argumentNum, CultureInfo.InvariantCulture);
                            StartNewStroke(ap.Px, ap.Py);
                        }
                        escapeChar = '\0';
                        break;

                    case '>':
                        if (!isMonochrome)
                        {
                            var deltaColor = int.Parse(argumentNum, CultureInfo.InvariantCulture);
                            if (deltaColor == 0)
                                deltaColor = 1;
                            ap.color += deltaColor;
                            StartNewStroke(ap.Px, ap.Py);
                        }
                        escapeChar = '\0';
                        break;

                    case '<':
                        if (!isMonochrome)
                        {
                            var deltaColor = int.Parse(argumentNum, CultureInfo.InvariantCulture);
                            if (deltaColor == 0)
                                deltaColor = -1;
                            ap.color -= deltaColor;
                            StartNewStroke(ap.Px, ap.Py);
                        }
                        escapeChar = '\0';
                        break;
                }
            }

            switch (c)
            {
                case '+':
                    ap.Angle += generalOrientation * angleIncrement;
                    break;

                case '-':
                    ap.Angle -= generalOrientation * angleIncrement;
                    break;

                case '|':
                    if (ls.Angle % 2 == 0)
                        ap.Angle += Math.PI;
                    else
                        ap.Angle += generalOrientation * (int)(ls.Angle / 2) * angleIncrement;
                    break;

                case '!':
                    generalOrientation = -generalOrientation;
                    break;

                case 'α':       // Set Angle absolute (not direct angle)
                case '\\':      // Add to direct angle
                case '/':       // Subtract to direct angle
                case 'C':       // Set color to nn
                case '>':       // Increment color of nn
                case '<':       // Decrement color of nn
                    escapeChar = c;
                    argumentNum = "";
                    break;

                case '@':       // Scale factor with options, I=Invert, Q=Square root
                    escapeChar = c;
                    argumentNum = "";
                    escapeOptions = "";
                    break;

                case '[':
                    apStack.Push(ap);
                    break;

                case ']':
                    if (apStack.Count > 0)
                    {
                        ap = apStack.Pop();
                        StartNewStroke(ap.Px, ap.Py);
                    }
                    break;

                case 'F':
                case 'G':
                    nx = ap.Px + ap.SegmentLength * Math.Cos(-ap.Angle);
                    ny = ap.Py + ap.SegmentLength * Math.Sin(-ap.Angle);
                    if (c == 'G')
                        StartNewStroke(nx, ny);
                    else
                        points.Add(new PointD(nx, ny));
                    ap.Px = nx;
                    ap.Py = ny;
                    if (nx > xmax)
                        xmax = nx;
                    if (nx < xmin)
                        xmin = nx;
                    if (ny > ymax)
                        ymax = ny;
                    if (ny < ymin)
                        ymin = ny;
                    break;

                case 'D':
                case 'M':
                    nx = ap.Px + ap.SegmentLength * Math.Cos(-ap.DirectAngle);
                    ny = ap.Py + ap.SegmentLength * Math.Sin(-ap.DirectAngle);
                    if (c == 'M')
                        StartNewStroke(nx, ny);
                    else
                        points.Add(new PointD(nx, ny));
                    ap.Px = nx;
                    ap.Py = ny;
                    if (nx > xmax)
                        xmax = nx;
                    if (nx < xmin)
                        xmin = nx;
                    if (ny > ymax)
                        ymax = ny;
                    if (ny < ymin)
                        ymin = ny;
                    break;
            }
        }

        // Rendring
        p.Clear();
        bool first = false;
        foreach (var sac in pointsLists)
            if (sac.points.Count > 0)
            {
                if (!first)
                {
                    p.DrawCircle((float)sac.points[0].X, (float)sac.points[0].Y, 0.25f);
                    first = true;
                }
                PlotSmoothed(sac.points, sac.color, xmin, ymin, xmax, ymax);
            }

        // Legend in black
        p.PenColor(Color.Black);

        var smoothingMethod = (string)SmoothingMethodsComboBox.SelectedItem;
        var iterations = (int)IterationsUpDown.Value;
        var tension = (double)TensionUpDown.Value;

        p.Text(p.Extent.XMin, p.Extent.YMin - 0.5f, $"{ls.Name}\nAngle={ls.Angle}\nAxiom={ls.Axiom}\nRules:\n{ls.Rules}\nDepth={depth}\n\nSmoothing={smoothingMethod}\nIterations={iterations}\nTension={tension:F2}", 0, 1);
        p.Move(p.Extent.XMin, p.Extent.YMin - 0.5f - (p.Extent.YMax - p.Extent.YMin) / 2);

        p.AutoScale();
        p.Refresh();
    }

    private void PlotSmoothed(List<PointD> points, int colorIndex, double xmin, double ymin, double xmax, double ymax)
    {
        var smooth = SmoothingMethodsComboBox.SelectedIndex switch
        {
            0 => Smoothing.GetSplineInterpolationCatmullRom(points, (int)IterationsUpDown.Value),
            1 => Smoothing.GetCurveSmoothingChaikin(points, (double)TensionUpDown.Value, (int)IterationsUpDown.Value),
            2 => Smoothing.GetCutCorners(points, (double)TensionUpDown.Value),
            _ => points,
        };
        p.PenColor(colorIndex);
        p.PenUp();
        p.PenWidth(0.1f);

        if (LSystemsComboBox.SelectedItem is LSystem ls && ls.Name == "HilbertDonut")
        {
            // Special transformation
            var newList = new List<PointD>();
            foreach (var point in smooth)
            {
                double a = (point.X - xmin) / (xmax - xmin) * 2 * Math.PI;
                double r = 2 + 2 * (point.Y - ymin) / (ymax - ymin);
                var tp = new PointD(2 + r * Math.Cos(a), 2 + r * Math.Sin(a));
                newList.Add(tp);
            }
            p.DrawPoints(newList);
        }
        else
            p.DrawPoints(smooth);
    }

    private void AngleTextBox_TextChanged(object sender, EventArgs e)
    {
        if (!IgnoreEvents)
            LSystemDrawing();
    }

    private void AxiomTextBox_TextChanged(object sender, EventArgs e)
    {
        if (!IgnoreEvents)
            LSystemDrawing();
    }

    private void RulesTextBox_TextChanged(object sender, EventArgs e)
    {
        if (!IgnoreEvents)
            LSystemDrawing();
    }
}

internal struct AngleAndPosition
{
    public double Px;                       // Current X
    public double Py;                       // Current Y
    public double Angle;                    // Angle in radians controlled by + and -
    public double DirectAngle;              // Angle in radians controlled by / and \
    public double SegmentLength = 1;        // Stroke length
    public int color;                       // 0=Black

    public AngleAndPosition()
    { Px = 0; Py = 0; Angle = 0; DirectAngle = 0; color = 0; }
}

internal class StrokeAndColor
{
    public int color;
    public List<PointD> points = new();
}

internal static class ExtensionMethods
{
    internal static void DrawPoints(this Plotter p, IEnumerable<PointD> points)
    {
        foreach (var point in points)
            p.Plot((float)point.X, (float)point.Y);
    }
}