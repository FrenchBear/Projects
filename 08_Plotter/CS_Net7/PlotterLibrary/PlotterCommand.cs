// Plotter - PlotterCommands.cs
// Hierarchy of classes to perist plotter commands
//
// 2021-12-09   PV

using System.Drawing;

namespace PlotterLibrary;

/// <summary>
/// General base class for plotter commands persistence
/// </summary>
internal abstract class PlotterCommand
{
}

/// <summary>
/// Helper abse class for plotter commands using pen attributes Width and Color
/// </summary>
internal abstract class PlotterCommandWidthAndColor: PlotterCommand
{
    public float Width { get; init; }
    public Color Color { get; init; }
}

internal class PC_ScaleP1P2: PlotterCommand
{
    public float P1X { get; init; }
    public float P1Y { get; init; }
    public float P2X { get; init; }
    public float P2Y { get; init; }
}

internal class PC_DrawLine: PlotterCommandWidthAndColor
{
    public float P1X { get; init; }
    public float P1Y { get; init; }
    public float P2X { get; init; }
    public float P2Y { get; init; }
}

internal class PC_DrawBox: PlotterCommandWidthAndColor
{
    public float P1X { get; init; }
    public float P1Y { get; init; }
    public float P2X { get; init; }
    public float P2Y { get; init; }
}

internal class PC_DrawCircle: PlotterCommandWidthAndColor
{
    public float CX { get; init; }
    public float CY { get; init; }
    public float R { get; init; }
}

internal class PC_DrawAxes: PlotterCommandWidthAndColor
{
    public float OX { get; init; }
    public float OY { get; init; }
    public float StepX { get; init; }
    public float StepY { get; init; }
}

internal class PC_DrawGrid: PlotterCommandWidthAndColor
{
    public float OX { get; init; }
    public float OY { get; init; }
    public float StepX { get; init; }
    public float StepY { get; init; }
}

internal class PC_WindowTitle: PlotterCommand
{
    public string WindowTitle { get; init; } = "";
}

internal class PC_Text: PlotterCommand
{
    public float PX { get; init; }
    public float PY { get; init; }
    public string Text { get; init; } = "";

    /// <summary>
    /// HorizontalAlignment: 0=Left, 1=Right, 2=Center
    /// </summary>
    public int Hz { get; init; }

    /// <summary>
    /// VerticalAlignment:   0=Top, 1=Bottom, 2=Middle
    /// </summary>
    public int Vt { get; init; }

    public Color Color { get; init; }
    public string FontFamily { get; init; } = "Arial";
    public float FontSize { get; init; }
    public FontStyle FontStyle { get; init; }
}