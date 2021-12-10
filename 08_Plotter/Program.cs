// Plotter - Program.cs
// Application startup
//
// 2021-12-09   PV

using System;
using System.Windows.Forms;

namespace Plotter;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
