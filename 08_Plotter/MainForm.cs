// Plotter - MainForm.cs
// Test application
//
// 2021-12-09   PV

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable IDE0051 // Remove unused private members

namespace Plotter;
public partial class MainForm: Form
{
    public MainForm()
    {
        InitializeComponent();
        Test1();
        //BosseDesMaths();
        //XTimesSinInvX();
        //ContinuousButNotDerivable();
        //Turtle1();
        //Cardioide();
        WindowState = FormWindowState.Minimized;
    }

    private static void Test1()
    {
        var p = new Plotter();

        p.ScaleP1P2(-10, -10, 10, 10);

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
        p.Text(0, 9.5f, "Y Axis",0,1);
        p.Text(9.5f, 0, "X Axis", 1);

        p.Refresh();
    }

    private static void XTimesSinInvX()
    {
        var p = new Plotter();

        p.ScaleP1P2(-1, -0.3f, 1, 0.9f);

        p.PenWidth(1);
        p.PenColor(Color.LightGray);
        p.DrawGrid(0, 0, 0.1f, 0.1f);

        p.PenWidth(3);
        p.PenColor(Color.Blue);
        p.DrawAxes(0, 0, 1, 1);

        p.PenColor(Color.Purple);
        p.PenWidth(3);
        for (float f = -1; f <= 1; f += 0.001f)
            p.Plot(f, (float)(f * Math.Sin(1.0 / f)));
        p.WindowTitle("x * Sin(x⁻¹)");
        p.Refresh();
    }

    private static void BosseDesMaths()
    {
        var p = new Plotter();
        p.ScaleP1P2(0, 0, 10.8f, 9);
        p.DrawBox(0, 0, 10.8f, 9);
        p.PenWidth(3);
        p.PenColor(Color.BlueViolet);

        const float gridStep = (float)(Math.PI / 15.0);
        const float drawStep = (float)(Math.PI / 60.0);
        const float zscale = 1.5f;
        float sqr2 = (float)Math.Sqrt(2);

        static float f(float x, float y)
            => (float)(zscale * ((1 - Math.Cos(x)) * (1 - Math.Cos(y))));

        for (float x = 0; x <= 2 * Math.PI + 0.00001; x += gridStep)
        {
            p.PenUp();
            for (float y = 0; y <= 2 * Math.PI + 0.00001; y += drawStep)
            {
                var z = f(x, y);
                p.Plot(x + y / sqr2, y / sqr2 + z);
            }
        }
        for (float y = 0; y <= 2 * Math.PI + 0.00001; y += gridStep)
        {
            p.PenUp();
            for (float x = 0; x <= 2 * Math.PI + 0.00001; x += drawStep)
            {
                var z = f(x, y);
                p.Plot(x + y / sqr2, y / sqr2 + z);
            }
        }
        p.WindowTitle("Bosse des maths");
        p.Refresh();
    }

    private static void ContinuousButNotDerivable()
    {
        var p = new Plotter();
        p.ScaleP1P2(0, 0, 1, 1);

        var l = new List<(float, float)>
        {
            (0, 0),
            (1, 1),
        };

        var ColorsTable = new Color[]
        {
            Color.Red,
            Color.Blue,
            Color.Yellow,
            Color.Gray,
            Color.Green,
            Color.Orange,
            Color.LightPink,
            Color.SkyBlue,
            Color.Purple
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

    private static void Turtle1()
    {
        var p = new Plotter();
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

    private static void Cardioide()
    {
        var p = new Plotter();
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
}
