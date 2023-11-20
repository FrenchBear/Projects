// 2023-11-20   PV      Net8 C#12

using System;

namespace DetectClosedArea;

class Program
{
    static void Main()
    {
        var t = new TestBoard(7);
        t.Fill();
        t.Print();
        t.FindAreas(0);
    }
}
