// NativeMethods class
// Regroup P/Invoke declarations, to follow code analysis recommendations
//
// 2017-09-21   PV

using System.Runtime.InteropServices;
using System.Text;


namespace LearningKeyboard
{
    public static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern int MapVirtualKey(uint uCode, uint uMapType);

        internal const uint MAPVK_VK_TO_VSC = 0x00;
        internal const uint MAPVK_VSC_TO_VK = 0x01;

        [DllImport("user32.dll")]
        internal static extern int ToUnicode(
            uint virtualKeyCode,
            uint scanCode,
            byte[] keyboardState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder receivingBuffer,
            int bufferSize,
            uint flags);
    }
}