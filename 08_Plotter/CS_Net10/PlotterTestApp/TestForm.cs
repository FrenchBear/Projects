// Plotter - MainForm.cs
// Test application
//
// 2021-12-09   PV
// 2021-12-17   PV      Spirograph
// 2023-11-20   PV      Net8 C#12
// 2026-01-20	PV		Net10 C#14

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Printing;
using PlotterLibrary;

namespace TestPlotter;

public partial class TestForm: Form
{
    private readonly Plotter p;

    public TestForm()
    {
        InitializeComponent();

        // Fill printers list
        foreach (string printer in PrinterSettings.InstalledPrinters)
            PrintersList.Items.Add(printer.ToString());
        if (PrintersList.Items.Count > 0)
            PrintersList.SelectedIndex = 0;

        p = new();
        p.Output(picOut);

        TestComboBox.Items.AddRange([
            "First test",
            "Bosse des maths",
            "x * Sin(x⁻¹)",
            "Continuous but not derivable",
            "Turtle 1",
            "Cardioïde",
            "AutoScale",
            "Predefined colors",
            "Color Squares",
            "Spirograph",
        ]);

        TestComboBox.SelectedIndex = 9;
    }

    private void TestComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (TestComboBox.SelectedIndex)
        {
            case 0:
                Test1();
                break;
            case 1:
                BosseDesMaths();
                break;
            case 2:
                XTimesSinInvX();
                break;
            case 3:
                ContinuousButNotDerivable();
                break;
            case 4:
                Turtle1();
                break;
            case 5:
                Cardioide();
                break;
            case 6:
                AutoScaleTest();
                break;
            case 7:
                PredefinedColors();
                break;
            case 8:
                ColorSquares();
                break;
            case 9:
                Spirograph();
                break;
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

    private void Test1()
    {
        p.Clear();

        p.ScaleP1P2(-10.5f, -10.5f, 10.5f, 10.5f);

        p.PenWidth(1);
        p.PenColor(Color.LightGray);
        p.DrawGrid(0, 0, 1, 1);

        p.PenWidth(3);
        p.PenColor(Color.Blue);
        p.DrawAxes(0, 0, 1, 1);

        p.PenWidth(2);
        p.PenColor(Color.Black);
        p.DrawLine(-10, -10, 10, 10);

        p.PenWidth(5);
        p.PenColor(Color.Red);
        p.DrawCircle(0, 0, 10);

        p.PenColor(Color.Purple);
        p.PenWidth(3);
        for (float f = -10; f < 10; f += 0.1f)
            p.Plot(f, (float)(0.02 * (f - 2.0) * (f + 1.0) * (f + 2.0)));

        p.Text(0, 0, "Origin");
        p.Text(0, 9.5f, "Y Axis", 0, 1);
        p.Text(9.5f, 0, "X Axis", 1);

        p.Refresh();
    }

    private void XTimesSinInvX()
    {
        p.Clear();

        p.ScaleP1P2(-0.4f, -0.4f, 0.4f, 0.4f);

        p.PenWidth(1);
        p.PenColor(Color.LightGray);
        p.DrawGrid(0, 0, 0.1f, 0.1f);

        p.PenWidth(3);
        p.PenColor(Color.Blue);
        p.DrawAxes(0, 0, 1, 1);

        p.PenColor(Color.Purple);
        p.PenWidth(1);

        // Variable step
        static float Inc(float f) => Math.Abs(f) switch
        {
            > 0.1f => 0.001f,
            > 0.01f => 0.0001f,
            _ => 0.00001f
        };

        for (float f = -1; f <= 1; f += Inc(f))
            p.Plot(f, (float)(f * Math.Sin(1.0 / f)));
        p.WindowTitle("x * Sin(x⁻¹)");
        p.Refresh();
    }

    private void BosseDesMaths()
    {
        p.Clear();
        p.ScaleP1P2(0, 0, 10.8f, 9);
        p.DrawBox(0, 0, 10.8f, 9);
        p.PenWidth(2);
        p.PenColor(Color.BlueViolet);

        const float gridStep = (float)(Math.PI / 15.0);
        const float drawStep = (float)(Math.PI / 60.0);
        const float zscale = 1.5f;
        float ky = 1.8f;

        static float f(float x, float y)
            => (float)(zscale * ((1 - Math.Cos(x)) * (1 - Math.Cos(y))));

        for (float x = 0; x <= 2 * Math.PI + 0.00001; x += gridStep)
        {
            p.PenUp();
            for (float y = 0; y <= 2 * Math.PI + 0.00001; y += drawStep)
            {
                var z = f(x, y);
                p.Plot(x + y / ky, y / ky + z);
            }
        }
        for (float y = 0; y <= 2 * Math.PI + 0.00001; y += gridStep)
        {
            p.PenUp();
            for (float x = 0; x <= 2 * Math.PI + 0.00001; x += drawStep)
            {
                var z = f(x, y);
                p.Plot(x + y / ky, y / ky + z);
            }
        }
        p.WindowTitle("Bosse des maths");
        p.Refresh();
    }

    private void ContinuousButNotDerivable()
    {
        p.Clear();
        p.ScaleP1P2(0, 0, 1, 1);

        var l = new List<(float, float)>
        {
            (0, 0),
            (1, 1),
        };

        var ColorsTable = new Color[]
        {
            Color.Black,
            Color.Red,
            Color.Blue,
            Color.Yellow,
            Color.Gray,
            Color.Green,
            Color.Orange,
            Color.LightPink,
            Color.SkyBlue,
            Color.Purple,
        };

        int div = 64;
        int level = 200;
        float pw = 0.5f;
        while (div > 2)
        {
            p.PenColor(Color.FromArgb(level, level, level));
            p.PenWidth(pw);
            p.DrawGrid(0, 0, (float)(1.0 / div), (float)(1.0 / div));
            div /= 2;
            level -= 16;
            pw += 0.5f;
        }

        p.PenWidth(3);
        p.PenColor(Color.Black);
        p.DrawBox(0, 0, 1, 1);

        p.PenWidth(3);

        void PlotList(List<(float, float)> list, Color color)
        {
            p.PenColor(color);
            p.PenUp();
            foreach (var item in l)
                p.Plot(item.Item1, item.Item2);
        }

        PlotList(l, Color.Black);
        for (int i = 0; i < 9; i++)
        {
            var newList = new List<(float, float)>();
            (float, float) lastPoint = l[0];
            newList.Add(lastPoint);
            for (int j = 1; j < l.Count; j++)
            {
                (float, float) point = l[j];
                (float, float) newPoint = ((lastPoint.Item1 + point.Item1) / 2, lastPoint.Item2 + (point.Item2 - lastPoint.Item2) / 4);
                newList.Add(newPoint);
                newList.Add(point);
                lastPoint = point;
            }

            l = newList;
            PlotList(l, ColorsTable[i]);
        }

        p.WindowTitle("Courbe de Weierstrass, continue et non dérivable");
        p.Refresh();
    }

    private void Turtle1()
    {
        p.Clear();
        p.ScaleP1P2(-10, -10, 10, 10);

        void DrawSquare()
        {
            for (int i = 0; i < 4; i++)
            {
                p.Forward(2);
                p.Left(90);
            }
        }

        p.Plot(5, 0);
        for (int i = 0; i < 36; i++)
        {
            p.DrawCircle(p.Pen.X, p.Pen.Y, 0.1f);
            DrawSquare();
            p.PenUp();
            p.Left(90 + 10);
            p.Forward(1);
            p.Right(90);
        }

        p.WindowTitle("Turtle 1");
        p.Refresh();
    }

    private void Cardioide()
    {
        p.Clear();
        p.ScaleP1P2(-1.1f, -1.1f, 1.1f, 1.2f);

        p.PenColor(Color.Black);
        for (float theta = 0; theta <= Math.PI * 2; theta += (float)Math.PI / 60)
            p.DrawLine((float)Math.Cos(theta), (float)Math.Sin(theta), (float)Math.Cos(2 * theta), (float)Math.Sin(2 * theta));

        p.PenColor(Color.Red);
        p.TextFont("Verdana", 18.0f, FontStyle.Bold | FontStyle.Underline);
        p.Text(0, 1.12f, "Cardioïde", 2, 2);

        p.WindowTitle("Cardioïde");
        p.Refresh();
    }

    private void AutoScaleTest()
    {
        p.Clear();

        p.DrawLine(0, 0, 5, 5);
        p.DrawLine(0, 5, 5, 0);
        p.DrawCircle(4, 4, 3);
        p.AutoScale();

        p.Refresh();
    }

    private void PredefinedColors()
    {
        p.Clear();

        p.ScaleP1P2(-1, -1, 10, 16);
        p.PenWidth(6);
        for (int i = 0; i < 16; i++)
        {
            p.PenColor(i);
            p.Text(0, i - 0.3f, $"Color {i}");
            p.DrawLine(3, i, 9, i);
        }

        p.Refresh();
    }

    private void ColorSquares()
    {
        p.Clear();

        p.ScaleP1P2(-11, -11, 11, 11);
        p.PenWidth(3);
        double a = Math.PI / 4;
        double r = 2;   // 9 * Math.Sqrt(2);
        for (; ; )
        {
            for (int c = 0; c < 4; c++)
            {
                p.PenColor(c);
                p.DrawLine((float)(r * Math.Cos(a + c * Math.PI / 2)), (float)(r * Math.Sin(a + c * Math.PI / 2)), (float)(r * Math.Cos(a + (c + 1) * Math.PI / 2)), (float)(r * Math.Sin(a + (c + 1) * Math.PI / 2)));
            }
            a += 0.085;
            r *= 1.02;
            if (r >= 10.7)
                break;
        }
        p.Refresh();
    }

    private void Spirograph()
    {
        p.Clear();

        const int z1 = 40;
        const int z2 = 45;
        const double k2 = 1.75;      // Position of pen on wheel 2 as a percentage of wheel 2 radius, 0=center, 1=edge, but can be byond [0..1] range
        double r1 = z1 / (2 * Math.PI);
        double r2 = z2 / (2 * Math.PI);
        int zper = PPMC(z1, z2);

        p.ScaleP1P2((float)(-r1 - 2 * r2 * k2 - 1), (float)(-r1 - 2 * r2 * k2 - 1), (float)(r1 + 2 * r2 * k2 + 1), (float)(r1 + 2 * r2 * k2 + 1));
        p.PenColor(1);
        p.PenWidth(1);
        for (int z = 0; z <= 2 * zper; z++)
        {
            // a1 = angle of rotation of contact point wheel 1/wheel 2
            double a1 = z / (double)z1 * Math.PI;

            // a2 = weel 2 proper angle of rotation (real angle of rotation of wheel 2 is a1+a2)
            double a2 = a1 * z1 / z2;

            // Center of wheel 2
            double xc2 = (r1 + r2) * Math.Cos(a1);
            double yc2 = (r1 + r2) * Math.Sin(a1);

            // Pen position
            double xp = xc2 + r2 * k2 * Math.Cos(a1 + a2);
            double yp = yc2 + r2 * k2 * Math.Sin(a1 + a2);

            p.Plot((float)xp, (float)yp);
        }

        p.Refresh();
    }

    /// <summary>
    /// Return Greatest Common Divisor using Euclidean Algorithm (Plus Grand Diviseur Commun)
    /// </summary>
    private static int PGDC(int a, int b)
    {
        if (a <= 0 || b <= 0)
            throw new ArgumentException("Negative or zero argument not supported");
        while (b != 0)
            (a, b) = (b, a % b);
        return a;
    }

    /// <summary>
    /// Smallest Common Multiple (Plus Petit Multiple Commun)
    /// </summary>
    private static int PPMC(int a, int b)
        => a * b / PGDC(a, b);
}