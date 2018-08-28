// RunANSI
// 2018-08-29   PV

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace RunANSI
{
    public class Program
    {

        // ReSharper disable InconsistentNaming

        private const int STD_INPUT_HANDLE = -10;
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;
        private const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

        // ReSharper restore InconsistentNaming

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
            //    Console.WriteLine("failed to get input console mode");
            //    Console.ReadKey();
            //    return;
            //}
            if (!GetConsoleMode(iStdOut, out uint outConsoleMode))
            {
                Console.WriteLine("failed to get output console mode");
                Console.ReadKey();
                return;
            }

            //inConsoleMode |= ENABLE_VIRTUAL_TERMINAL_INPUT;
            outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING; //| DISABLE_NEWLINE_AUTO_RETURN;

            //if (!SetConsoleMode(iStdIn, inConsoleMode))
            //{
            //    Console.WriteLine($"failed to set input console mode, error code: {GetLastError()}");
            //    Console.ReadKey();
            //    return;
            //}
            if (!SetConsoleMode(iStdOut, outConsoleMode))
            {
                Console.WriteLine($"failed to set output console mode, error code: {GetLastError()}");
                Console.ReadKey();
                return;
            }

            //inConsoleMode &= ~ENABLE_VIRTUAL_TERMINAL_INPUT;

            //if (!SetConsoleMode(iStdIn, inConsoleMode))
            //{
            //    Console.WriteLine($"failed to set input console mode, error code: {GetLastError()}");
            //    Console.ReadKey();
            //    return;
            //}
        }

        public static int Main(string[] args)
        {
            if (args.Length == 0)
                throw new Exception("RunANSI: Need at least one argument, the name of the application to run");
            SetVTMode();

            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < args.Length; i++)
            {
                if (sb.Length > 0) sb.Append(" ");
                if (args[i].IndexOf(' ') >= 0)
                    sb.Append('"').Append(args[i]).Append('"');
                else
                    sb.Append(args[i]);
            }

            ProcessStartInfo start = new ProcessStartInfo();
            int exitCode;
            start.Arguments = sb.ToString();
            start.FileName = args[0];
            start.UseShellExecute = false;
            start.WindowStyle = ProcessWindowStyle.Normal;
            //start.CreateNoWindow = true;
            //start.WorkingDirectory = @"C:\Temp";
            using (Process proc = Process.Start(start))
            {
                proc.WaitForExit();
                exitCode = proc.ExitCode;
            }

            return exitCode;
        }


        private static void RunCmd()
        {
        }

    }
}
