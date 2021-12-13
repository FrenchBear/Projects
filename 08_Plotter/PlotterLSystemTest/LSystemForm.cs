// Plotter - MainForm.cs
// Test application
//
// 2021-12-09   PV

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Printing;
using PlotterLibrary;
using System.Text;
using System.Globalization;

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

        // Fill L-Systems
        var Dragon = new LSystem("Dragon", "Original Dragon curve", 8, "FX", "F=\r\nX=-FX++FY-\r\nY=+FX--FY+");
        var Hilbert = new LSystem("Hilbert", "Basic Hilbert curve", 4, "X", "X=-YF+XFX+FY-\r\nY=+XF-YFY-FX+");
        var Flocon = new LSystem("Flocon", "Von Koch curve", 6, "FX", "F=\r\nX=FX-FX++FX-FX");
        var F5 = new LSystem("F5", "Carré 5x5", 4, "α45FX", "F=\r\nX=@2FX-FX+@I2FX+FX-FX+FX+@2FX-FX@I2-FX-FX+FX-FX+FX");
        LSystemsComboBox.Items.AddRange(new LSystem[] {
            Hilbert,
            Dragon,
            Flocon,
            F5,
        });
        LSystemsComboBox.SelectedIndex = 3;

        // Fill smooting methods
        SmoothingMethodsComboBox.Items.AddRange(new string[] {
            "Spline interpolation",
            "Chaikin",
            "Cut corners",
            "No interpolation",
        });
        SmoothingMethodsComboBox.SelectedIndex = 3;

        p = new();
        p.Output(picOut);
    }

    private void LSystemsComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (LSystemsComboBox.SelectedItem is LSystem s)
        {
            NameTextBox.Text = s.Name;
            CommentTextBox.Text = s.Comment;
            AngleTextBox.Text = s.Angle.ToString();
            AxiomTextBox.Text = s.Axiom;
            RulesTextBox.Text = s.Rules;
        }
    }

    private void MainForm_Resize(object sender, EventArgs e)
    {
        if (p != null)
            p.Refresh();
    }

    private void PrintButton_Click(object sender, EventArgs e)
    {
        if (PrintersList.SelectedItem == null)
        {
            MessageBox.Show("Select a printer first!");
            return;
        }

        p.Print((string)PrintersList.SelectedItem);
    }

    private void SmoothingTest()
    {
        if (p == null)
            return;

        var testPoints = new List<PointD>
            {
                new PointD(3, 0.5f),
                new PointD(2, 1),
                new PointD(3, 2),
                new PointD(3, 3),
                new PointD(4, 2.5f),
                new PointD(4.8f, 3.7f),
                new PointD(3, 5.5f),
                new PointD(6, 8f),
                new PointD(7, 9.5f),
                new PointD(8.3f, 5.1f),
                new PointD(6.5f, 4.2f),
                new PointD(7, 3),
                new PointD(8, 2),
                new PointD(9, 2),
                new PointD(9, 3),
                new PointD(6, 0.5f)
            };

        p.Clear();
        PlotSmoothed(testPoints);
        p.AutoScale();
        p.Refresh();
    }

    private void PlotSmoothed(List<PointD> points)
    {
        var smooth = SmoothingMethodsComboBox.SelectedIndex switch
        {
            0 => Smoothing.GetSplineInterpolationCatmullRom(points, (int)IterationsUpDown.Value),
            1 => Smoothing.GetCurveSmoothingChaikin(points, (float)TensionUpDown.Value, (int)IterationsUpDown.Value),
            2 => Smoothing.GetCutCorners(points, (float)TensionUpDown.Value),
            _ => points,
        };
        p.PenColor(Color.Black);
        p.PenWidth(0.1f);
        p.DrawPoints(smooth);
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
        RefreshDrawing();
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
        RefreshDrawing();
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
        RefreshDrawing();
    }

    private Action? CurrentDrawingAction;

    private void RefreshDrawing()
        => CurrentDrawingAction?.Invoke();

    private void DrawTestButton_Click(object sender, EventArgs e)
    {
        CurrentDrawingAction = SmoothingTest;
        RefreshDrawing();
    }

    private void DrawLSystemButton_Click(object sender, EventArgs e)
    {
        CurrentDrawingAction = LSystemDrawing;
        RefreshDrawing();
    }

    private void LSystemDrawing()
    {
        if (p == null)
            return;
        if (LSystemsComboBox.SelectedItem is not LSystem ls)
            return;

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
        var drawString = LSystemProcessor.LSystemIterator(depth, ls.Axiom, rules);
        StringBuilder sb = new();
        foreach (char c in drawString)
            sb.Append(c);
        string ss = sb.ToString();

        // Generation of points
        var pointsLists = new List<List<PointD>>();
        List<PointD> points;

        void StartNewStroke()
        {
            points = new List<PointD>();
            pointsLists.Add(points);
        }
        StartNewStroke();

        AngleAndPosition ap = new()
        {
            SegmentLength = 1.0f            // Scale factor 1
        };

        Stack<AngleAndPosition> apStack = new();

        float nx;                          // New X position
        float ny;                          // New Y position

        float angleIncrement = (float)(2 * Math.PI / ls.Angle);

        int generalOrientation = 1;         // General orientation, changed to -1 by !
        char escapeChar = '\0';             // Different than \0 when processing @ or \ or / escape sequence
        string escapeOptions = "";          // Accumulated I and or Q for @ sequence
        string argumentNum = "";            // Buffer to accumulate @, \ and / numeric argument

        points.Add(new PointD(ap.Px, ap.Py));
        for (int i = 0; i < ss.Length; i++)
        {
            char c = ss[i];

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
                        ap.Angle = (float)(generalOrientation * double.Parse(argumentNum, CultureInfo.InvariantCulture) * Math.PI / 180);
                        break;

                    case '/':
                        ap.DirectAngle -= (float)(generalOrientation * double.Parse(argumentNum, CultureInfo.InvariantCulture) * Math.PI / 180);
                        escapeChar = '\0';
                        break;

                    case '\\':
                        ap.DirectAngle += (float)(generalOrientation * double.Parse(argumentNum, CultureInfo.InvariantCulture) * Math.PI / 180);
                        escapeChar = '\0';
                        break;

                    case '@':
                        if (c == 'I' || c == 'Q')
                        {
                            escapeOptions += c;
                            continue;
                        }
                        float f = float.Parse(argumentNum, CultureInfo.InvariantCulture);
                        if (escapeOptions == "IQ")
                            f = (float)(1.0 / Math.Sqrt(f));
                        else if (escapeOptions == "QI")
                            f = (float)Math.Sqrt(1.0 / f);
                        else if (escapeOptions == "I")
                            f = 1.0f / f;
                        else if (escapeOptions == "Q")
                            f = (float)Math.Sqrt(f);
                        ap.SegmentLength *= f;
                        escapeChar = '\0';
                        break;

                    case 'C':
                        ap.color = int.Parse(argumentNum, CultureInfo.InvariantCulture);
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
                        ap.Angle += (float)Math.PI;
                    else
                        ap.Angle += generalOrientation * (int)(ls.Angle / 2) * angleIncrement;
                    break;

                case '!':
                    generalOrientation = -generalOrientation;
                    break;

                case 'α':
                    escapeChar = c;
                    argumentNum = "";
                    break;

                case '\\':
                    escapeChar = c;
                    argumentNum = "";
                    break;

                case '/':
                    escapeChar = c;
                    argumentNum = "";
                    break;

                case 'C':
                    escapeChar = c;
                    argumentNum = "";
                    break;

                case '@':
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
                        StartNewStroke();
                    }
                    break;

                case 'F':
                case 'G':
                    nx = (float)(ap.Px + ap.SegmentLength * Math.Cos(-ap.Angle));
                    ny = (float)(ap.Py + ap.SegmentLength * Math.Sin(-ap.Angle));
                    if (c == 'G')
                        StartNewStroke();
                    else
                        points.Add(new PointD(nx, ny));
                    ap.Px = nx;
                    ap.Py = ny;
                    break;

                case 'D':
                case 'M':
                    nx = (float)(ap.Px + ap.SegmentLength * Math.Cos(-ap.DirectAngle));
                    ny = (float)(ap.Py + ap.SegmentLength * Math.Sin(-ap.DirectAngle));
                    if (c == 'M')
                        StartNewStroke();
                    else
                        points.Add(new PointD(nx, ny));
                    ap.Px = nx;
                    ap.Py = ny;
                    break;
            }
        }

        // Rendring
        p.Clear();
        foreach (var list in pointsLists)
            PlotSmoothed(list);

        p.PenColor(Color.Black);

        var smoothingMethod = (string)SmoothingMethodsComboBox.SelectedItem;
        var iterations = (int)IterationsUpDown.Value;
        var tension = (float)TensionUpDown.Value;

        p.Text(p.Extent.XMin, p.Extent.YMin - 0.5f, $"{ls.Name}\nAngle={ls.Angle}\nAxiom={ls.Axiom}\nRules:\n{ls.Rules}\nDepth={depth}\n\nSmoothing={smoothingMethod}\nIterations={iterations}\nTension={tension:F2}", 0, 1);
        p.Move(p.Extent.XMin, p.Extent.YMin - 0.5f - (p.Extent.YMax-p.Extent.YMin) / 2);

        p.AutoScale();
        p.Refresh();
    }
}

struct AngleAndPosition
{
    public float Px;                       // Current X
    public float Py;                       // Current Y
    public float Angle;                    // Angle in radians controlled by + and -
    public float DirectAngle;              // Angle in radians controlled by / and \
    public float SegmentLength;            // Stroke length
    public int color;                       // 0=black, the rest in undefined
}

static class ExtensionMethods
{
    internal static void DrawPoints(this Plotter p, IEnumerable<PointD> points)
    {
        foreach (var point in points)
            p.Plot(point.X, point.Y);
    }
}
