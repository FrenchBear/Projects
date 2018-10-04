using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main()
        {
            var key = Key.D0;       // WPF Key
            var virtualKeyFromKey = KeyInterop.VirtualKeyFromKey(key);      // Win32 VirtualKey
            var displayString = GetLocalizedKeyStringUnsafe(virtualKeyFromKey);

            Console.WriteLine($"{key}: {displayString}");


            int sc = 11;
            int vk = MapVirtualKey((uint)sc, MAPVK_VSC_TO_VK);
            var k = (System.Windows.Forms.Keys)vk;
            var k2 = KeyInterop.KeyFromVirtualKey(vk);
            Console.WriteLine("VK: {0}  Key WinForms (System.Windows.Forms.Keys): {1}  Key WPF (System.Windows.Input.Key): {2}", vk, k, k2);

            var lw = System.Windows.Forms.Keys.LControlKey;
            sc = MapVirtualKey((uint)lw, MAPVK_VK_TO_VSC);
            Console.WriteLine($"VK {lw} -> ScanCode {sc}");
            var rw = System.Windows.Forms.Keys.RControlKey;
            sc = MapVirtualKey((uint)rw, MAPVK_VK_TO_VSC);
            Console.WriteLine($"VK {rw} -> ScanCode {sc}");

            const int VK_LSHIFT = 0xA0;
            const int VK_RSHIFT = 0xA1;
            const int VK_LCONTROL = 0xA2;
            const int VK_RCONTROL = 0xA3;
            const int VK_LMENU = 0xA4;
            const int VK_RMENU = 0xA5;
            foreach (int i in new int[] { VK_LSHIFT, VK_RSHIFT, VK_LCONTROL, VK_RCONTROL, VK_LMENU , VK_RMENU })
            {
                sc = MapVirtualKey((uint)i, MAPVK_VK_TO_VSC);
                Console.WriteLine($"VK {i:x2} -> ScanCode {sc}");
            }

            Console.WriteLine();
            Console.Write("(Pause)");
            Console.ReadLine();
        }

        private static string GetLocalizedKeyStringUnsafe(int key)
        {
            // strip any modifier keys
            long keyCode = key & 0xffff;

            var sb = new StringBuilder(256);

            long scanCode = MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC);

            // shift the scancode to the high word
            scanCode = (scanCode << 16); // | (1 << 24);
            if (keyCode == 45 ||
                keyCode == 46 ||
                keyCode == 144 ||
                (33 <= keyCode && keyCode <= 40))
            {
                // add the extended key flag
                scanCode |= 0x1000000;
            }

            GetKeyNameText((int)scanCode, sb, 256);
            return sb.ToString();
        }

        private const uint MAPVK_VK_TO_VSC = 0x00;
        private const uint MAPVK_VSC_TO_VK = 0x01;

        [DllImport("user32.dll")]
        private static extern int MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", EntryPoint = "GetKeyNameTextW", CharSet = CharSet.Unicode)]
        private static extern int GetKeyNameText(int lParam, [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder str, int size);
    }
}