// KeyboardKey
// Specialization of NewKey UserControl to represent a virtual key
//
// 2017-09-19   PV


using System.Text;
using System.Windows.Forms;
using static LearningKeyboard.NativeMethods;

#pragma warning disable IDE0060 // Remove unused parameter

namespace LearningKeyboard
{
    internal class KeyboardKey : NewKey
    {
        //private readonly string dispoNF;    // Key ref in NF-Z-71-300
        //private readonly int scanCode;
        public readonly Keys vk;            // Windows Forms VK
        public readonly string digit;       // L5..L2 R2..R5


        public KeyboardKey(string dispoNF, int scanCode, string digit, NewKeyStyle style, string simpleTextOverride, int w, int h)
        {
            //this.dispoNF = dispoNF;
            //this.scanCode = scanCode;
            this.digit = digit;

            Width = w;
            Height = h;

            vk = (Keys)MapVirtualKey((uint)scanCode, MAPVK_VSC_TO_VK);

            this.KeyStyle = style;

            if (style == NewKeyStyle.Static)
            {
                if (string.IsNullOrEmpty(simpleTextOverride))
                    NormalText = vk.ToString();
                else
                    NormalText = simpleTextOverride;
            }
            else
            {
                (NormalText, IsNormalDeadKey) = GetCharsFromKeys2(vk, false, false);
                (ShiftText, IsShiftDeadKey) = GetCharsFromKeys2(vk, true, false);
                (AltGrText, IsAltGrDeadKey) = GetCharsFromKeys2(vk, false, true);
                (ShiftAltGrText, IsShiftAltGrDeadKey) = GetCharsFromKeys2(vk, true, true);
            }

            // Initial display
            IsPressed = false;
        }


        // The apparently useless calls to GetCharsFromKeys
        // clean internal buffer state, perturbed by previous diacritical
        // characters such as ~ or ^
        private static (string, bool) GetCharsFromKeys2(Keys vk, bool shift, bool altGr)
        {
            string s = CharFromKey.GetCharsFromKeys(vk, shift, altGr, out bool b1);
            CharFromKey.GetCharsFromKeys(Keys.Space, false, false, out bool _);
            return (s, b1);
        }


    //    private static string GetCharsFromKeys(Keys keys, bool shift, bool altGr, out bool isDeadKey)
    //    {
    //        var buf = new StringBuilder(256);
    //        var keyboardState = new byte[256];
    //        if (shift)
    //            keyboardState[(int)Keys.ShiftKey] = 0xff;
    //        if (altGr)
    //        {
    //            keyboardState[(int)Keys.ControlKey] = 0xff;
    //            keyboardState[(int)Keys.Menu] = 0xff;
    //        }

    //        int n = ToUnicode((uint)keys, 0, keyboardState, buf, 256, 0);
    //        if (n < 0)
    //        {
    //            isDeadKey = true;
    //            n = 1;
    //        }
    //        else
    //            isDeadKey = false;
    //        return buf.Length==0 ? "" : buf.ToString().Substring(0, n);
    //    }
    }
}