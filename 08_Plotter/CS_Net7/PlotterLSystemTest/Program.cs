// Plotter - Program.cs
// Application startup
//
// 2021-12-09   PV

using System;
using System.Windows.Forms;

namespace LSystemTest;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new LSystemForm());
    }
}