// PlotterCommonViewModel
// ViewModel of PlotterCommonUserControl
// Stores data abour pen color, pen width or printer
//
// 2021-12-17   PV
// 2026-01-20	PV		Net10 C#14

using PlotterLibrary;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;

namespace PlotterWPFTest.PlotterCommon;

public class PlotterCommonViewModel(Action plotChart): BaseViewModel
{
    private readonly Action DoPlotChart = plotChart;
    public Plotter? Plotter;

    public int PenWidth { get; set => SetProperty(ref field, value); } = 1;

    public List<string> PenColors => _PenColors;
    internal List<string> _PenColors = [];

    public int PenColorIndex { get; set => SetProperty(ref field, value); } = -1;

    public List<string> PrintersList => _PrintersList;
    internal List<string> _PrintersList = [];

    internal void InitPlotter(Plotter p)
    {
        Plotter = p;

        foreach (var item in p.ColorsTable)
            _PenColors.Add(item.ToString());
        PenColorIndex = 0;

        // Fill printers list
        foreach (string printer in PrinterSettings.InstalledPrinters)
            _PrintersList.Add(printer.ToString());
        SelectedPrinter = PrintersList[0];
    }

    public string SelectedPrinter { get; set => SetProperty(ref field, value); } = "";

    public override void PlotChart()
        => DoPlotChart();
}
