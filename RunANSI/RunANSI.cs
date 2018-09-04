﻿// RunANSI
// Launches a console program after enabling virtual terminal mode, to process ANSI/VT escape sequences
//
// 2018-08-29   PV
// 2018-09-04   PV      1.1 Check for missing argument; process its own options; silent by default, option -v to show messages

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static System.Console;


namespace RunANSI
{
    public class Program
    {
        private static bool SilentMode = true;

        private const int STD_INPUT_HANDLE = -10;
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;
        private const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();


        private static void SetVTMode()
        {
            //var iStdIn = GetStdHandle(STD_INPUT_HANDLE);
            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);

            //if (!GetConsoleMode(iStdIn, out uint inConsoleMode))
            //{
            //    WriteLine("failed to get input console mode");
            //    ReadKey();
            //    return;
            //}
            if (!GetConsoleMode(iStdOut, out uint outConsoleMode))
            {
                if (!SilentMode)
                    WriteLine("failed to get output console mode");
                return;
            }

            //inConsoleMode |= ENABLE_VIRTUAL_TERMINAL_INPUT;
            outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING; //| DISABLE_NEWLINE_AUTO_RETURN;

            //if (!SetConsoleMode(iStdIn, inConsoleMode))
            //{
            //    WriteLine($"failed to set input console mode, error code: {GetLastError()}");
            //    ReadKey();
            //    return;
            //}
            if (!SetConsoleMode(iStdOut, outConsoleMode))
            {
                if (!SilentMode)
                    WriteLine($"failed to set output console mode, error code: {GetLastError()}");
                return;
            }

            //inConsoleMode &= ~ENABLE_VIRTUAL_TERMINAL_INPUT;

            //if (!SetConsoleMode(iStdIn, inConsoleMode))
            //{
            //    WriteLine($"failed to set input console mode, error code: {GetLastError()}");
            //    ReadKey();
            //    return;
            //}
        }

        public static int Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            int iCmdArg = -1;
            for (int i = 0; i < args.Length; i++)
            {
                if (iCmdArg < 0 && args[i][0] == '-')
                {
                    switch (args[i][1])
                    {
                        case 'h':
                        case '?':
                            Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                            AssemblyTitleAttribute aTitleAttr = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyTitleAttribute));
                            string sAssemblyVersion = myAssembly.GetName().Version.Major.ToString() + "." + myAssembly.GetName().Version.Minor.ToString() + "." + myAssembly.GetName().Version.Build.ToString();
                            AssemblyDescriptionAttribute aDescAttr = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyDescriptionAttribute));
                            AssemblyCopyrightAttribute aCopyrightAttr = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(myAssembly, typeof(AssemblyCopyrightAttribute));

                            WriteLine(aTitleAttr.Title + " " + sAssemblyVersion);
                            WriteLine(aDescAttr.Description);
                            WriteLine(aCopyrightAttr.Copyright);
                            return 0;

                        case 'v':
                            SilentMode = false;
                            break;

                        default:
                            WriteLine($"RunANSI: Option {args[i]} ignored, use -h for help");
                            break;
                    }

                }
                else
                {
                    if (iCmdArg < 0)
                        iCmdArg = i;
                    else
                    {
                        if (sb.Length > 0)
                            sb.Append(" ");
                        if (args[i].IndexOf(' ') >= 0)
                            sb.Append('"').Append(args[i]).Append('"');
                        else
                            sb.Append(args[i]);
                    }
                }
            }
            if (iCmdArg < 0)
            {
                WriteLine("RunANSI: Need at least one argument, the name of the application to run");
                return 1;
            }

            // Main goal of this application
            SetVTMode();

            ProcessStartInfo start = new ProcessStartInfo
            {
                Arguments = sb.ToString(),
                FileName = args[iCmdArg],
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Normal
            };
            int exitCode;

            try
            {
                using (Process proc = Process.Start(start))
                {
                    proc.WaitForExit();
                    exitCode = proc.ExitCode;
                }
            }
            catch (Exception ex)
            {
                WriteLine("RunANSI: Error launching command " + ProtectSpace(args[0]) + " " + sb.ToString() + "\n" + ex.Message);
                return 1;
            }

            return exitCode;
        }
        private static string ProtectSpace(string s) => s.IndexOf(' ') >= 0 ? "\"" + s + "\"" : s;
    }
}
