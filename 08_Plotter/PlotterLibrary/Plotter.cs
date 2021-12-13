﻿// Plotter - Plotter.cs
// Public API of plotter (Plotter class)
//
// 2021-12-09   PV

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PlotterLibrary;

public partial class Plotter
{
    private readonly List<PlotterCommand> Commands = new();
    private RenderingForm? rf = null;
    public DrawingExtent Extent = new();
    private PictureBox? picOut = null;

    /// <summary>
    /// All current pen attributes (position, color, width...)
    /// </summary>
    public CurrentPenAttributes Pen = new();

    /// <summary>
    /// Reset plotter: clear all commands, reset pen attributes to default values
    /// </summary>
    public void Clear()
    {
        Commands.Clear();
        Pen.Clear();
        Extent.Clear();
    }

    public void ScaleP1P2(float p1x, float p1y, float p2x, float p2y)
    {
        var cmd = new PC_ScaleP1P2 { P1X = p1x, P1Y = p1y, P2X = p2x, P2Y = p2y };
        Commands.Add(cmd);
    }

    public void AutoScale()
    {
        // Simple attempt to determine scale automatically based on lines and circles extent (only text origin is guaranteed to be in extent)
        // Override manual scale if it exists
        const float km = 0.02f;
        var newScale = new PC_ScaleP1P2
        {
            P1X = Extent.XMin - (Extent.XMax - Extent.XMin) * km,
            P1Y = Extent.YMin - (Extent.YMax - Extent.YMin) * km,
            P2X = Extent.XMax + (Extent.XMax - Extent.XMin) * km,
            P2Y = Extent.YMax + (Extent.YMax - Extent.YMin) * km
        };

        bool found = false;
        for (int i = 0; i < Commands.Count; i++)
            if (Commands[i] is PC_ScaleP1P2)
            {
                Commands[i] = newScale;
                found = true;
                break;
            }
        if (!found)
            Commands.Insert(0, newScale);
    }

    public void AdjustExtent(float x, float y)
    {
        if (x > Extent.XMax)
            Extent.XMax = x;
        if (x < Extent.XMin)
            Extent.XMin = x;
        if (y > Extent.YMax)
            Extent.YMax = y;
        if (y < Extent.YMin)
            Extent.YMin = y;
    }

    public void PenColor(Color color)
        => Pen.Color = color;

    private readonly Color[] ColorsTable = new Color[]
    {
        Color.Black,
        Color.Red,
        Color.Blue,
        Color.Green,
        Color.Magenta,
        Color.Gray,
        Color.Orange,
        Color.LightPink,
        Color.SkyBlue,
        Color.Gold,
        Color.Purple,
        Color.Brown,
        Color.Chartreuse,
        Color.DarkSalmon,
        Color.CornflowerBlue,
        Color.Goldenrod,
    };

    public void PenColor(int colorIndex)
        => Pen.Color = ColorsTable[colorIndex % ColorsTable.Length];

    public void PenWidth(float w)
        => Pen.Width = w;

    public void DrawLine(float p1x, float p1y, float p2x, float p2y)
    {
        var cmd = new PC_DrawLine { Color = Pen.Color, Width = Pen.Width, P1X = p1x, P1Y = p1y, P2X = p2x, P2Y = p2y };
        Commands.Add(cmd);
        AdjustExtent(p1x, p1y);
        AdjustExtent(p2x, p2y);
    }

    public void DrawBox(float p1x, float p1y, float p2x, float p2y)
    {
        var cmd = new PC_DrawBox { Color = Pen.Color, Width = Pen.Width, P1X = p1x, P1Y = p1y, P2X = p2x, P2Y = p2y };
        Commands.Add(cmd);
        AdjustExtent(p1x, p1y);
        AdjustExtent(p2x, p2y);
    }

    public void DrawAxes(float ox, float oy, float stepx, float stepy)
    {
        var cmd = new PC_DrawAxes { Color = Pen.Color, Width = Pen.Width, OX = ox, OY = oy, StepX = stepx, StepY = stepy };
        Commands.Add(cmd);
        Pen.X = ox;
        Pen.Y = oy;
        Pen.PenDown = false;
    }

    public void DrawGrid(float ox, float oy, float stepx, float stepy)
    {
        var cmd = new PC_DrawGrid { Color = Pen.Color, Width = Pen.Width, OX = ox, OY = oy, StepX = stepx, StepY = stepy };
        Commands.Add(cmd);
        Pen.X = ox;
        Pen.Y = oy;
        Pen.PenDown = false;
    }

    public void DrawCircle(float cx, float cy, float r)
    {
        var cmd = new PC_DrawCircle { Color = Pen.Color, Width = Pen.Width, CX = cx, CY = cy, R = r };
        Commands.Add(cmd);
        AdjustExtent(cx + r, cy + r);
        AdjustExtent(cx - r, cy - r);
    }

    public void PenUp()
        => Pen.PenDown = false;

    public void PenDown()
        => Pen.PenDown = true;

    public void Plot(float x, float y)
    {
        if (Pen.PenDown)
            DrawLine(Pen.X, Pen.Y, x, y);
        AdjustExtent(x, y);
        Pen.X = x;
        Pen.Y = y;
        Pen.PenDown = true;
    }

    public void Move(float x, float y)
    {
        AdjustExtent(x, y);
        Pen.X = x;
        Pen.Y = y;
        Pen.PenDown = true;
    }

    public void WindowTitle(string title)
    {
        var cmd = new PC_WindowTitle { WindowTitle = title };
        Commands.Add(cmd);
    }

    public void Output(PictureBox picOut) => this.picOut = picOut;

    public void Refresh()
    {
        if (picOut == null)
        {
            rf = new RenderingForm(this);
            rf.Show();
            picOut = rf.picOut;
        }
        RefreshPlot();
    }

    // Turtle commands
    public static float CosDegree(float alpha)
        => (float)Math.Cos(alpha / 180 * Math.PI);

    public static float SinDegree(float alpha)
    => (float)Math.Sin(alpha / 180 * Math.PI);

    public void Forward(float dist)
    {
        float newX = Pen.X + dist * CosDegree(Pen.Angle);
        float newY = Pen.Y + dist * SinDegree(Pen.Angle);
        Plot(newX, newY);
    }

    public void Right(float delta)
        => Pen.Angle -= delta;

    public void Left(float delta)
        => Pen.Angle += delta;

    public void Angle(float angle)
        => Pen.Angle = angle;

    public void TextFont(string fontFamily, float size, FontStyle style)
    {
        Pen.FontFamily = fontFamily;
        Pen.FontSize = size;
        Pen.FontStyle = style;
    }

    // HorizontalAlignment: 0=Left, 1=Right, 2=Center
    // VerticalAlignment:   0=Top, 1=Bottom, 2=Middle
    public void Text(float px, float py, string text, int hz = 0, int vt = 0)
    {
        var cmd = new PC_Text { PX = px, PY = py, Text = text, Hz = hz, Vt = vt, Color = Pen.Color, FontFamily = Pen.FontFamily, FontSize = Pen.FontSize, FontStyle = Pen.FontStyle };
        Commands.Add(cmd);
        AdjustExtent(px, py);       // We don't know the extent in user coordinates, so we just ajust extent using text origin, that may not be enough...
    }
}

public class CurrentPenAttributes
{
    /// <summary>
    /// Current Pen X coordinate
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Current Pen Y coordinate
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// Current Pen writing status, true=write, false=does nothing
    /// </summary>
    public bool PenDown;

    /// <summary>
    /// Current Pen drawing color
    /// </summary>
    public System.Drawing.Color Color;

    /// <summary>
    /// Current Pen drawing width (unit is pixel for screen rendering, but for printer rendering, it seems thicker...
    /// </summary>
    public float Width
    {
        get => width;
        set
        {
            if (value >= 0)
                width = value;
            else
                throw new ArgumentException("Width must be >=0");
        }
    }
    private float width;

    /// <summary>
    /// Current Angle for turtle, in degrees
    /// </summary>
    public float Angle { get; set; }

    public string FontFamily { get; internal set; } = "Arial";

    public float FontSize { get; internal set; }

    public FontStyle FontStyle { get; internal set; }

    // Constructor, make sure structure is initialized with known defaults
    internal CurrentPenAttributes()
        => Clear();

    /// <summary>
    /// Reset pen attributes to known defaults: X=Y=0, black, pen up, width 1, angle 0
    /// </summary>
    internal void Clear()
    {
        X = 0;
        Y = 0;
        PenDown = false;
        Color = System.Drawing.Color.Black;
        Width = 1.0f;
        Angle = 0;

        FontFamily = "Arial";
        FontSize = 12;
        FontStyle = FontStyle.Regular;
    }

}

public struct DrawingExtent
{
    public float XMin { get; internal set; } = float.MaxValue;
    public float XMax { get; internal set; } = float.MinValue;
    public float YMin { get; internal set; } = float.MaxValue;
    public float YMax { get; internal set; } = float.MinValue;

    public void Clear()
    {
        XMin = float.MaxValue;
        YMin = float.MaxValue;
        XMax = float.MinValue;
        YMax = float.MinValue;
    }
}
