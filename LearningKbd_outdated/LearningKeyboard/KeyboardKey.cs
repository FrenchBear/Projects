// KeyboardKey
// Specialization of NewKey UserControl to represent a virtual key
//
// 2017-09-19   PV


using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;


namespace LearningKeyboard
{

    class KeyboardKey : NewKey
    {
        private int scanCode;
        private int vkInt;             // Windows Forms VK
        private Keys vk;

        [DllImport("user32.dll")]
        private static extern int MapVirtualKey(uint uCode, uint uMapType);
        private const uint MAPVK_VK_TO_VSC = 0x00;
        private const uint MAPVK_VSC_TO_VK = 0x01;

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint virtualKeyCode,
            uint scanCode,
            byte[] keyboardState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder receivingBuffer,
            int bufferSize,
            uint flags);


        public KeyboardKey(int scanCode, int w, int h, KeyStyles style, string simpleTextOverride)
        {
            this.scanCode = scanCode;

            // Resize key
            Width = w;
            Height = h;

            vkInt = MapVirtualKey((uint)scanCode, MAPVK_VSC_TO_VK);
            vk = (System.Windows.Forms.Keys)vkInt;

            this.KeyStyle = style;

            if (style == KeyStyles.Static)
            {
                if (string.IsNullOrEmpty(simpleTextOverride))
                    NormalText = vk.ToString();
                else
                    NormalText = simpleTextOverride;
            }
            else
            {
                // The apparently useless calls to GetCharsFromKeys
                // clean internal buffer state, perturbed by dicratical
                // characters such as ~ or ^
                NormalText = GetCharsFromKeys(vk, false, false);
                GetCharsFromKeys(Keys.Space, false, false);

                ShiftText = GetCharsFromKeys(vk, true, false);
                GetCharsFromKeys(Keys.Space, false, false);

                AltGrText = GetCharsFromKeys(vk, false, true);
                GetCharsFromKeys(Keys.Space, false, false);

                ShiftAltGrText = GetCharsFromKeys(vk, true, true);
                GetCharsFromKeys(Keys.Space, false, false);
            }

            // Initial display
            IsPressed = false;
        }


        private static string GetCharsFromKeys(Keys keys, bool shift, bool altGr)
        {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            if (shift)
                keyboardState[(int)Keys.ShiftKey] = 0xff;
            if (altGr)
            {
                keyboardState[(int)Keys.ControlKey] = 0xff;
                keyboardState[(int)Keys.Menu] = 0xff;
            }
            int n = ToUnicode((uint)keys, 0, keyboardState, buf, 256, 0);
            if (n < 0) n = 1;
            return buf.ToString().Substring(0, n);
        }
    }
}
