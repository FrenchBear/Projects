// Plotter - Program.cs
// Application startup
//
// 2021-12-09   PV
// 2023-11-20   PV      Net8 C#12

using System;
using System.Windows.Forms;

namespace TestPlotter;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new TestForm());
    }
}
