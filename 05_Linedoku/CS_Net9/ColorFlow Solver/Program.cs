// ColorFlow Solver from Android app Linedoku
//
// 2017-10-27   PV
// 2023-11-20   PV      Net8 C#12

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using static System.Console;

namespace ColorFlowSolver;

static partial class Program
{
    static StreamWriter? sw;
    static Stopwatch? stw;

    [STAThread]
    static void Main()
    {
        OutputEncoding = Encoding.UTF8;

        var n = DateTime.Now;
        using (var swu = new StreamWriter(string.Format(@"C:\temp\ColorFlow-{0}-{1:D2}-{2:D2}-{3:D2}.{4:D2}.{5:D2}.txt", n.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second)))
        {
            swu.AutoFlush = true;
            sw = swu;

            stw = Stopwatch.StartNew();

            //Board(5, new Line[] { new Line(0, 0, 4, 4), new Line(0, 1, 1, 3), new Line(2, 1, 4, 0), new Line(3, 1, 4, 3) });
            //Board(7, new Line[] { new Line(0, 0, 4, 5), new Line(1, 0, 3, 3), new Line(2, 4, 6, 3), new Line(3, 2, 4, 2), new Line(5, 5, 6, 4) });
            Board(9, [new Line(0, 0, 4, 5), new Line(1, 3, 3, 8), new Line(1, 7, 5, 6), new Line(2, 0, 2, 5), new Line(3, 0, 8, 4), new Line(4, 8, 8, 5), new Line(6, 2, 7, 7)]);
            //Board(11, new Line[] { new Line(0, 0, 5, 5), new Line(1, 0, 2, 5), new Line(1, 3, 7, 9), new Line(1, 7, 3, 10), new Line(2, 10, 6, 9), new Line(5, 4, 10, 10), new Line(6, 1, 8, 3), new Line(7, 5, 9, 5), new Line(9, 1, 10, 9) });

            Solve(false);
            //TestsClosedAreas();

            stw.Stop();

            LogWriteLine();
            LogWriteLine($"Total duration:            {stw.Elapsed}");
            LogWriteLine($"Solutions:                 {Solutions}");
            LogWriteLine($"Calls to SolveSE:          {SolveSECalls:N0}");
            LogWriteLine($"Calls to CheckClosedAreas: {CheckClosedAreasCalls:N0}");
        }

        WriteLine();
        Write("(Pause)");
        ReadLine();
    }

    internal static void LogWrite(string s)
    {
        // Not multi-threaded app
        sw?.Write(s);
        Write(s);
    }

    internal static void LogWriteLine(string s = "")
    {
        sw?.WriteLine(s);
        WriteLine(s);
    }

    /// <summary>
    /// Just a simple test building a solution manually to test Print()
    /// </summary>
    internal static void TestWalls()
    {
        for (byte row = 0; row < Side; row++)
            for (byte column = 0; column < Sidem1; column++)
                SetHzWall(row, column);
        for (byte row = 0; row < Sidem1; row++)
            for (byte column = 0; column < Side; column++)
                SetVtWall(row, column);
        for (byte row = 0; row < Side; row++)
            for (byte column = 0; column < Sidem1; column++)
                ResetHzWall(row, column);
        for (byte row = 0; row < Sidem1; row++)
            for (byte column = 0; column < Side; column++)
                ResetVtWall(row, column);
    }

    internal static void Test1()
    {
        Board(5, []);
        SetHzWall(0, 1);
        SetHzWall(0, 2);
        SetHzWall(0, 3);
        SetHzWall(1, 0);
        SetHzWall(1, 1);
        SetHzWall(1, 3);
        SetHzWall(2, 0);
        SetHzWall(2, 2);
        SetHzWall(2, 3);
        SetHzWall(3, 2);
        SetHzWall(4, 1);

        SetVtWall(0, 0);
        SetVtWall(2, 0);
        SetVtWall(3, 0);
        SetVtWall(3, 1);
        SetVtWall(1, 2);
        SetVtWall(3, 2);
        SetVtWall(3, 3);
        SetVtWall(0, 4);
        SetVtWall(2, 4);
        SetVtWall(3, 4);

        PrintBoard("Test1");
    }

    internal static void Test2()
    {
        Board(11, []);
        SetHzWall(0, 0);
        SetHzWall(0, 1);
        SetHzWall(0, 2);
        SetHzWall(0, 3);
        SetHzWall(0, 4);
        SetHzWall(0, 5);
        SetHzWall(0, 6);
        SetHzWall(0, 7);
        SetHzWall(0, 8);
        SetHzWall(0, 9);
        SetHzWall(1, 0);
        SetHzWall(1, 1);
        SetHzWall(1, 2);
        SetHzWall(1, 3);
        SetHzWall(1, 4);
        SetHzWall(1, 5);
        SetHzWall(1, 7);
        SetHzWall(1, 8);
        SetHzWall(1, 9);
        SetHzWall(2, 6);
        SetHzWall(2, 7);
        SetHzWall(2, 8);
        SetHzWall(3, 0);
        SetHzWall(3, 1);
        SetHzWall(3, 2);
        SetHzWall(3, 3);
        SetHzWall(3, 4);
        SetHzWall(3, 5);
        SetHzWall(3, 6);
        SetHzWall(3, 7);
        SetHzWall(3, 8);
        SetHzWall(4, 0);
        SetHzWall(4, 1);
        SetHzWall(4, 2);
        SetHzWall(4, 3);
        SetHzWall(4, 4);
        SetHzWall(4, 5);
        SetHzWall(4, 6);
        SetHzWall(4, 7);
        SetHzWall(4, 8);
        SetHzWall(4, 9);
        SetHzWall(5, 0);
        SetHzWall(5, 1);
        SetHzWall(5, 2);
        SetHzWall(5, 3);
        SetHzWall(5, 4);
        SetHzWall(5, 5);
        SetHzWall(5, 6);
        SetHzWall(5, 7);
        SetHzWall(5, 8);
        SetHzWall(5, 9);
        SetHzWall(6, 0);
        SetHzWall(6, 1);
        SetHzWall(6, 2);
        SetHzWall(6, 3);
        SetHzWall(6, 4);
        SetHzWall(6, 5);
        SetHzWall(6, 6);
        SetHzWall(6, 7);
        SetHzWall(7, 1);
        SetHzWall(7, 2);
        SetHzWall(7, 3);
        SetHzWall(7, 5);
        SetHzWall(7, 8);
        SetHzWall(7, 9);
        SetHzWall(8, 1);
        SetHzWall(8, 4);
        SetHzWall(8, 6);
        SetHzWall(8, 7);
        SetHzWall(8, 8);
        SetHzWall(8, 9);
        SetHzWall(9, 2);
        SetHzWall(9, 4);
        SetHzWall(9, 5);
        SetHzWall(9, 6);
        SetHzWall(9, 7);
        SetHzWall(10, 3);
        SetHzWall(10, 4);
        SetHzWall(10, 5);
        SetHzWall(10, 6);
        SetHzWall(10, 7);
        SetVtWall(0, 0);
        SetVtWall(0, 10);
        SetVtWall(1, 6);
        SetVtWall(2, 9);
        SetVtWall(3, 0);
        SetVtWall(4, 10);
        SetVtWall(5, 0);
        SetVtWall(6, 8);
        SetVtWall(7, 1);
        SetVtWall(7, 4);
        SetVtWall(7, 5);
        SetVtWall(7, 6);
        SetVtWall(7, 10);
        SetVtWall(8, 2);
        SetVtWall(9, 3);
        SetVtWall(9, 8);
        PrintBoard("Test2");
    }

    internal static void TestsClosedAreas()
    {
        TestClosedArea1();
        TestClosedArea2();
        TestClosedArea3a();
        TestClosedArea3b();
        TestClosedArea4();
    }

    internal static void TestClosedArea1()
    {
        Board(5, [new Line(0, 0, 2, 2), new Line(3, 0, 1, 3), new Line(4, 0, 4, 4)]);

        SetHzWall(0, 0);
        SetHzWall(0, 1);
        SetPointLine(0, 1, 0);
        SetVtWall(0, 2);
        SetPointLine(0, 2, 0);
        SetVtWall(1, 2);
        SetPointLine(1, 2, 0);

        SetHzWall(3, 0);
        SetPointLine(3, 1, 1);
        SetHzWall(3, 1);
        SetPointLine(3, 2, 1);
        SetHzWall(3, 2);
        SetPointLine(3, 3, 1);
        SetVtWall(2, 3);
        SetPointLine(2, 3, 1);
        SetVtWall(1, 3);
        SetPointLine(1, 3, 1);

        //PrintBoard("TestClosedArea1");
        Debug.Assert(CheckClosedAreas(0));
        Debug.Assert(!CheckClosedAreas(1));
    }

    internal static void TestClosedArea2()
    {
        Board(5, [new Line(0, 0, 2, 2), new Line(3, 0, 1, 3), new Line(2, 0, 4, 4)]);

        SetHzWall(0, 0);
        SetHzWall(0, 1);
        SetPointLine(0, 1, 0);
        SetVtWall(0, 2);
        SetPointLine(0, 2, 0);
        SetVtWall(1, 2);
        SetPointLine(1, 2, 0);

        SetHzWall(3, 0);
        SetPointLine(3, 1, 1);
        SetHzWall(3, 1);
        SetPointLine(3, 2, 1);
        SetHzWall(3, 2);
        SetPointLine(3, 3, 1);
        SetVtWall(2, 3);
        SetPointLine(2, 3, 1);
        SetVtWall(1, 3);
        SetPointLine(1, 3, 1);

        //PrintBoard("TestClosedArea2");
        Debug.Assert(CheckClosedAreas(0));
        Debug.Assert(!CheckClosedAreas(1));
    }

    internal static void TestClosedArea3a()
    {
        Board(3, [new Line(1, 0, 0, 0), new Line(2, 0, 2, 1)]);

        SetHzWall(1, 0);
        SetPointLine(1, 1, 0);
        SetHzWall(1, 1);
        SetPointLine(1, 2, 0);
        SetVtWall(0, 2);
        SetPointLine(0, 2, 0);

        //PrintBoard("TestClosedArea3a");
        Debug.Assert(CheckClosedAreas(0));
    }

    internal static void TestClosedArea3b()
    {
        Board(3, [new Line(1, 0, 0, 2), new Line(2, 0, 2, 1)]);

        SetHzWall(1, 0);
        SetPointLine(1, 1, 0);
        SetHzWall(1, 1);
        SetPointLine(1, 2, 0);
        SetVtWall(0, 2);
        SetPointLine(0, 2, 0);

        //PrintBoard("TestClosedArea3b");
        Debug.Assert(!CheckClosedAreas(0));
    }

    internal static void TestClosedArea4()
    {
        Board(3, [new Line(1, 0, 0, 2), new Line(2, 0, 2, 1)]);

        SetVtWall(0, 0);
        SetPointLine(0, 0, 0);
        SetHzWall(0, 0);
        SetPointLine(0, 1, 0);
        SetVtWall(0, 1);
        SetPointLine(1, 1, 0);
        SetHzWall(1, 1);
        SetPointLine(1, 2, 0);

        //PrintBoard("TestClosedArea4");
        Debug.Assert(CheckClosedAreas(0));
    }

}
