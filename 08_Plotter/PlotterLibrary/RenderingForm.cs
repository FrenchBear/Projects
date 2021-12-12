﻿// Plotter - Rendering form
// Provides onscreen and printed rendering using GDI if application doesn't provide a picOut
//
// 2021-12-09   PV

using System;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace PlotterLibrary;

internal partial class RenderingForm: Form
{
    private readonly Plotter p;

    public RenderingForm(Plotter p)
    {
        InitializeComponent();

        // Fill printers list
        foreach (string printer in PrinterSettings.InstalledPrinters)
            PrintersList.Items.Add(printer.ToString());
        if (PrintersList.Items.Count > 0)
            PrintersList.SelectedIndex = 0;

        this.p = p;
    }

    private void RenderingForm_Resize(object sender, EventArgs e) => p.Refresh();

    private void PrintButton_Click(object sender, EventArgs e)
    {
        if (PrintersList.SelectedItem == null)
        {
            MessageBox.Show("Select a printer first!");
            return;
        }

        p.Print((string)PrintersList.SelectedItem);
    }
}
