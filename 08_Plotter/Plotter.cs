﻿// Plotter - Plotter.cs
// Public API of plotter (Plotter class)
//
// 2021-12-09   PV

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Plotter;

internal partial class Plotter
{
    private readonly List<PlotterCommand> Commands = new();
    private RenderingForm rf = null;

    /// <summary>
    /// All current pen attributes (position, color, width...)
    /// </summary>
    internal CurrentPenAttributes Pen = new();

    /// <summary>
    /// Reset plotter: clear all commands, reset pen attributes to default values
    /// </summary>
    internal void Clear()
    {
        Commands.Clear();
        Pen.Reset();
    }

    internal void ScaleP1P2(float p1x, float p1y, float p2x, float p2y)
    {
        var cmd = new PC_ScaleP1P2 { P1X = p1x, P1Y = p1y, P2X = p2x, P2Y = p2y };
        Commands.Add(cmd);
    }

    internal void PenColor(Color color)
        => Pen.Color = color;

    internal void PenWidth(float w)
        => Pen.Width = w;

    internal void DrawLine(float p1x, float p1y, float p2x, float p2y)
    {
        var cmd = new PC_DrawLine { Color = Pen.Color, Width = Pen.Width, P1X = p1x, P1Y = p1y, P2X = p2x, P2Y = p2y };
        Commands.Add(cmd);
    }

    internal void DrawBox(float p1x, float p1y, float p2x, float p2y)
    {
        var cmd = new PC_DrawBox { Color = Pen.Color, Width = Pen.Width, P1X = p1x, P1Y = p1y, P2X = p2x, P2Y = p2y };
        Commands.Add(cmd);
    }

    internal void DrawAxes(float ox, float oy, float stepx, float stepy)
    {
        var cmd = new PC_DrawAxes { Color = Pen.Color, Width = Pen.Width, OX = ox, OY = oy, StepX = stepx, StepY = stepy };
        Commands.Add(cmd);
        Pen.X = ox;
        Pen.Y = oy;
        Pen.PenDown = false;
    }

    internal void DrawGrid(float ox, float oy, float stepx, float stepy)
    {
        var cmd = new PC_DrawGrid { Color = Pen.Color, Width = Pen.Width, OX = ox, OY = oy, StepX = stepx, StepY = stepy };
        Commands.Add(cmd);
        Pen.X = ox;
        Pen.Y = oy;
        Pen.PenDown = false;
    }

    internal void DrawCircle(float cx, float cy, float r)
    {
        var cmd = new PC_DrawCircle { Color = Pen.Color, Width = Pen.Width, CX = cx, CY = cy, R = r };
        Commands.Add(cmd);
    }

    internal void PenUp()
        => Pen.PenDown = false;

    internal void PenDown()
        => Pen.PenDown = true;

    internal void Plot(float x, float y)
    {
        if (Pen.PenDown)
            DrawLine(Pen.X, Pen.Y, x, y);
        Pen.X = x;
        Pen.Y = y;
        Pen.PenDown = true;
    }

    internal void WindowTitle(string title)
    {
        var cmd = new PC_WindowTitle { WindowTitle = title };
        Commands.Add(cmd);
    }

    private PictureBox picOut = null;

    internal void Output(PictureBox picOut) => this.picOut = picOut;

    internal void Refresh()
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
    private static float CosDegree(float alpha)
        => (float)Math.Cos(alpha / 180 * Math.PI);

    private static float SinDegree(float alpha)
    => (float)Math.Sin(alpha / 180 * Math.PI);

    internal void Forward(float dist)
    {
        float newX = Pen.X + dist * CosDegree(Pen.Angle);
        float newY = Pen.Y + dist * SinDegree(Pen.Angle);
        Plot(newX, newY);
    }

    internal void Right(float delta)
        => Pen.Angle -= delta;

    internal void Left(float delta)
        => Pen.Angle += delta;

    internal void Angle(float angle)
        => Pen.Angle = angle;

    internal void TextFont(string fontFamily, float size, FontStyle style)
    {
        Pen.FontFamily = fontFamily;
        Pen.FontSize = size;
        Pen.FontStyle = style;
    }

    // HorizontalAlignment: 0=Left, 1=Right, 2=Center
    // VerticalAlignment:   0=Top, 1=Bottom, 2=Middle
    internal void Text(float px, float py, string text, int hz = 0, int vt = 0)
    {
        var cmd = new PC_Text { PX = px, PY = py, Text = text, Hz = hz, Vt = vt, Color = Pen.Color, FontFamily=Pen.FontFamily, FontSize = Pen.FontSize, FontStyle=Pen.FontStyle };
        Commands.Add(cmd);
    }
}

internal class CurrentPenAttributes
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
            if (value > 0)
                width = value;
            else
                throw new ArgumentException("Width must be >0");
        }
    }
    private float width;

    /// <summary>
    /// Current Angle for turtle, in degrees
    /// </summary>
    public float Angle { get; set; }

    public string FontFamily { get; internal set; }

    public float FontSize { get; internal set; }

    public FontStyle FontStyle { get; internal set; }

    // Constructor, make sure structure is initialized with known defaults
    internal CurrentPenAttributes()
        => Reset();

    /// <summary>
    /// Reset pen attributes to known defaults: X=Y=0, black, pen up, width 1, angle 0
    /// </summary>
    internal void Reset()
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
