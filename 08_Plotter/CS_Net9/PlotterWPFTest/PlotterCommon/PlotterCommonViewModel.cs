﻿// PlotterCommonViewModel
// ViewModel of PlotterCommonUserControl
// Stores data abour pen color, pen width or printer
//
// 2021-12-17   PV

using PlotterLibrary;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;

namespace PlotterWPFTest.PlotterCommon;

public class PlotterCommonViewModel(Action plotChart): BaseViewModel
{
    private readonly Action DoPlotChart = plotChart;
    public Plotter? Plotter;

    public int PenWidth { get => _PenWidth; set => SetProperty(ref _PenWidth, value); }
    private int _PenWidth = 1;

    public List<string> PenColors => _PenColors;
    internal List<string> _PenColors = [];

    public int PenColorIndex { get => _PenColorIndex; set => SetProperty(ref _PenColorIndex, value); }
    private int _PenColorIndex = -1;    // So that when PenColorIndex is set to 0, it actually selects the 1st event bypassing SetProperty optimization

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

    public string SelectedPrinter { get => _SelectedPrinter; set => SetProperty(ref _SelectedPrinter, value); }
    private string _SelectedPrinter = "";

    public override void PlotChart()
        => DoPlotChart();
}
