// 2023-11-20   PV      Net8 C#12
// 2026-01-20	PV		Net10 C#14

namespace DetectClosedArea;

sealed class Program
{
    static void Main()
    {
        var t = new TestBoard(7);
        t.Fill();
        t.Print();
        t.FindAreas(0);
    }
}
