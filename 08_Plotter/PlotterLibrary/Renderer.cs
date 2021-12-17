// Plotter - Renderer.cs
// Rendering code using GDI
//
// 2021-12-09   PV

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

namespace PlotterLibrary;

public partial class Plotter
{
    private void RefreshPlot()
    {
        if (picOut==null) return;
        if (picOut.Size.Width <= 1 || picOut.Size.Height <= 1)
            return;  // Too small pic area
        Bitmap bmpOut = new(picOut.Size.Width, picOut.Size.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        picOut.Image = bmpOut;
        var graOut = Graphics.FromImage(bmpOut);
        GraphicsDraw(graOut, picOut.Size.Width, picOut.Size.Height);
    }

    // Low-level rendering function
    // penWidthFactor is used to compensate too thick lines when printing
    private void GraphicsDraw(Graphics outGraphics, float renderingWidth, float renderingHeight, float penWidthFactor = 1.0f)
    {
        float r;
        float x0;
        float y0;

        void SetUserScale(float p1x, float p1y, float p2x, float p2y)
        {
            float rx, ry;

            // Pass 0 is continuing, caclulate scale factor
            if (Math.Abs(p2x - p1x) < 1e-8)
                rx = 100000;
            else
                rx = (float)(renderingWidth / (1.01 * (p2x - p1x)));
            if (Math.Abs(p2y - p1y) < 1e-8)
                ry = 100000;
            else
                ry = (float)(renderingHeight / (1.01 * (p2y - p1y)));
            r = Math.Min(Math.Abs(rx), Math.Abs(ry));

            x0 = p1x + (p2x - p1x - renderingWidth / r) / 2;
            y0 = p1y + (p2y - p1y - renderingHeight / r) / 2;
        }

        (float rend_x, float rend_y) UserToRend(float x, float y)
        {
            float rend_x = (x - x0) * r;
            float rend_y = renderingHeight - (y - y0) * r;
            return (rend_x, rend_y);
        }

        outGraphics.Clear(Color.White);
        outGraphics.SmoothingMode = SmoothingMode.HighQuality;
        SetUserScale(-15, -15, 15, 15);            // Default user scale 
        for (int i = 0; i < Commands.Count; i++)
        {
            var pc = Commands[i];

            if (pc is PC_ScaleP1P2 scale)
            {
                SetUserScale(scale.P1X, scale.P1Y, scale.P2X, scale.P2Y);
            }
            else if (pc is PC_DrawLine line)
            {
                var LastX = line.P2X;
                var LastY = line.P2Y;
                var LastWidth = line.Width;
                var LastColor = line.Color;

                var (rx1, ry1) = UserToRend(line.P1X, line.P1Y);
                var (rx2, ry2) = UserToRend(LastX, LastY);

                var tp = new List<PointF>
                {
                    new PointF(rx1, ry1),
                    new PointF(rx2, ry2)
                };

                // Optimization, as long as following segments join with current one, then we merge them in a list to do a single DrawLines call
                // Limited to 1000 points, PDF rendering doesn't like huge lists
                while (i < Commands.Count - 1 && tp.Count<1000 && Commands[i + 1] is PC_DrawLine pc2 && pc2.P1X == LastX && pc2.P1Y == LastY && pc2.Width == LastWidth && pc2.Color == LastColor)
                {
                    LastX = pc2.P2X;
                    LastY = pc2.P2Y;
                    var (rxn, ryn) = UserToRend(LastX, LastY);
                    tp.Add(new PointF(rxn, ryn));
                    i++;
                }

                var p = new Pen(LastColor, LastWidth*penWidthFactor);
                outGraphics.DrawLines(p, tp.ToArray());
            }
            else if (pc is PC_DrawBox box)
            {
                var (rx1, ry1) = UserToRend(box.P1X, box.P1Y);
                var (rx2, ry2) = UserToRend(box.P2X, box.P2Y);
                var p = new Pen(box.Color, box.Width * penWidthFactor);
                outGraphics.DrawRectangle(p, Math.Min(rx1, rx2), Math.Min(ry1, ry2), Math.Abs(rx2 - rx1), Math.Abs(ry2 - ry1));
            }
            else if (pc is PC_DrawCircle circle)
            {
                var (rcx, rcy) = UserToRend(circle.CX, circle.CY);
                var rr = circle.R * r;
                float rx1 = rcx - rr;
                float ry1 = rcy - rr;
                var p = new Pen(circle.Color, circle.Width * penWidthFactor);
                outGraphics.DrawEllipse(p, rx1, ry1, 2 * rr, 2 * rr);
            }
            else if (pc is PC_DrawAxes axes)
            {
                var (rox, roy) = UserToRend(axes.OX, axes.OY);
                var rstepx = axes.StepX * r;
                var rstepy = axes.StepY * r;

                var p = new Pen(axes.Color, axes.Width * penWidthFactor);

                // Plot tick marks first
                if (rstepx > 0)
                {
                    var x = rox + rstepx;
                    while (x < renderingWidth)
                    {
                        outGraphics.DrawLine(p, x, roy - 5, x, roy + 5);
                        x += rstepx;
                    }
                    x = rox - rstepx;
                    while (x >= 0)
                    {
                        outGraphics.DrawLine(p, x, roy - 5, x, roy + 5);
                        x -= rstepx;
                    }
                }
                if (rstepy > 0)
                {
                    var y = roy + rstepy;
                    while (y < renderingHeight)
                    {
                        outGraphics.DrawLine(p, rox - 5, y, rox + 5, y);
                        y += rstepy;
                    }
                    y = roy - rstepy;
                    while (y >= 0)
                    {
                        outGraphics.DrawLine(p, rox - 5, y, rox + 5, y);
                        y -= rstepy;
                    }
                }
                outGraphics.DrawLine(p, 0.0f, roy, (float)renderingWidth, roy);
                outGraphics.DrawLine(p, rox, 0.0f, rox, (float)renderingHeight);
            }
            else if (pc is PC_DrawGrid grid)
            {
                var (rox, roy) = UserToRend(grid.OX, grid.OY);
                var rstepx = grid.StepX * r;
                var rstepy = grid.StepY * r;

                var p = new Pen(grid.Color, grid.Width * penWidthFactor);

                if (rstepx > 0)
                {
                    var x = rox;
                    while (x < renderingWidth)
                    {
                        outGraphics.DrawLine(p, x, 0, x, renderingHeight);
                        x += rstepx;
                    }
                    x = rox - rstepx;
                    while (x >= 0)
                    {
                        outGraphics.DrawLine(p, x, 0, x, renderingHeight);
                        x -= rstepx;
                    }
                }
                if (rstepy > 0)
                {
                    var y = roy;
                    while (y < renderingHeight)
                    {
                        outGraphics.DrawLine(p, 0, y, renderingWidth, y);
                        y += rstepy;
                    }
                    y = roy - rstepy;
                    while (y >= 0)
                    {
                        outGraphics.DrawLine(p, 0, y, renderingWidth, y);
                        y -= rstepy;
                    }
                }
            }
            else if (pc is PC_WindowTitle cmd)
            {
                // Just when rendering is done in a separate window
                //Text = cmd.WindowTitle;
            }
            else if (pc is PC_Text text)
            {
                var (px, py) = UserToRend(text.PX, text.PY);

                var b = new SolidBrush(text.Color);
                var f = new Font(text.FontFamily, text.FontSize, text.FontStyle);
                var mes = outGraphics.MeasureString(text.Text, f);
                if (text.Hz == 1)
                    px -= mes.Width;
                else if (text.Hz == 2)
                    px -= mes.Width / 2;
                py -= mes.Height;
                if (text.Vt == 1)
                    py += mes.Height;
                else if (text.Vt == 2)
                    py += mes.Height / 2;
                outGraphics.DrawString(text.Text, f, b, px, py);
            }
        }
    }

    // Printing support

    /// <summary>
    /// Print drawing to specified printer
    /// </summary>
    public void Print(string printerName)
    {
        var pd = new PrintDocument();
        pd.PrinterSettings.PrinterName = printerName;

        // Select A4 paper
        for (int i = 0; i < pd.PrinterSettings.PaperSizes.Count; i++)
        {
            var pkSize = pd.PrinterSettings.PaperSizes[i];
            if (pkSize.PaperName.Contains("A4"))
                pd.DefaultPageSettings.PaperSize = pkSize;
        }

        foreach (PrinterResolution pr in pd.PrinterSettings.PrinterResolutions)
        {
            if (pr.Kind == PrinterResolutionKind.High)
            {
                pd.DefaultPageSettings.PrinterResolution = pr;
                break;
            }
        }

        pd.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);
        pd.Print();
    }

    private void PrintDocument_PrintPage(object sender, PrintPageEventArgs ev)
    {
        if (ev.Graphics is Graphics g)      // Also avoids null value
            GraphicsDraw(g, ev.PageBounds.Width, ev.PageBounds.Height, 0.6f);
    }

}
