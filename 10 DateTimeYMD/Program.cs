// DateTymeYMD
// Simple tool to print current date and time in a sortable order without forbidden characters for a filename
// 2019-08-16   PV

using System;

class Program
{
    static void Main() => Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss"));
}
