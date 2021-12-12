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
        var Dragon = new LSystem { Name = "Dragon", Comment = "Original Dragon curve", Angle = 8, Axiom = "FX", Rules = "F=\r\nX=-FX++FY-\r\nY=+FX--FY+" };
        var Hilbert = new LSystem { Name = "Hilbert", Comment = "Basic Hilbert curve", Angle = 4, Axiom = "X", Rules = "X=-YF+XFX+FY-\r\nY=+XF-YFY-FX+" };
        LSystemsComboBox.Items.AddRange(new LSystem[] {
            Hilbert,
            Dragon,
        });
        LSystemsComboBox.SelectedIndex = 0;

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
        var s = LSystemsComboBox.SelectedItem as LSystem;
        NameTextBox.Text = s.Name;
        CommentTextBox.Text = s.Comment;
        AngleTextBox.Text = s.Angle.ToString();
        AxiomTextBox.Text = s.Axiom;
        RulesTextBox.Text = s.Rules;
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

        p.Print(PrintersList.SelectedItem.ToString());
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

        PlotSmoothed(testPoints);
    }

    private void PlotSmoothed(List<PointD> points)
    {
        p.Clear();
        p.PenWidth(1);

// ToDo: Add a checkbox ?        
        //p.PenColor(Color.Blue);
        //p.DrawPoints(points);
        //p.PenUp();

        var smooth = SmoothingMethodsComboBox.SelectedIndex switch
        {
            0 => Smoothing.GetSplineInterpolationCatmullRom(points, (int)IterationsUpDown.Value),
            1 => Smoothing.GetCurveSmoothingChaikin(points, (float)TensionUpDown.Value, (int)IterationsUpDown.Value),
            2 => Smoothing.GetCutCorners(points, (float)TensionUpDown.Value),
            _ => points,
        };
        p.PenColor(Color.Blue);
        p.DrawPoints(smooth);

        p.AutoScale();
        p.Refresh();
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

    private Action CurrentDrawingAction;

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

        var ls = LSystemsComboBox.SelectedItem as LSystem;

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
        var drawString = LSystemProcessor.LSystemIterator((int)DepthUpDown.Value, ls.Axiom, rules);
        StringBuilder sb = new();
        foreach (char c in drawString)
            sb.Append(c);
        string ss = sb.ToString();

        // Generation of points
        var points = new List<PointD>();

        AngleAndPosition ap = new()
        {
            SegmentLength = 1.0f
        };     // All fields start at 0.0

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
                        // Draw a fake unstroked segment to restore correctly current position for rendering subsystems
                        // that memorize current position
                        p.PenUp();
                        p.Plot(ap.Px, ap.Py);
                    }
                    break;

                case 'F':
                case 'G':
                    nx = (float)(ap.Px + ap.SegmentLength * Math.Cos(ap.Angle));
                    ny = (float)(ap.Py + ap.SegmentLength * Math.Sin(ap.Angle));
                    // ToDo: handle multiple strokes
                    //if (c == 'G')
                    //    p.PenUp();
                    //p.DrawLine(ap.Px, ap.Py, nx, ny);
                    points.Add(new PointD(nx, ny));
                    ap.Px = nx;
                    ap.Py = ny;
                    break;

                case 'D':
                case 'M':
                    nx = (float)(ap.Px + ap.SegmentLength * Math.Cos(ap.DirectAngle));
                    ny = (float)(ap.Py + ap.SegmentLength * Math.Sin(ap.DirectAngle));
                    // ToDo: handle multiple strokes
                    //if (c == 'M')
                    //    p.PenUp();
                    points.Add(new PointD(nx, ny));
                    ap.Px = nx;
                    ap.Py = ny;
                    break;
            }
        }

        // Rendring
        PlotSmoothed(points);
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
