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
        public readonly Keys vk;            // Windows Forms VK
        public readonly string digit;       // L5..L2 R2..R5


        public KeyboardKey(string dispoNF, int scanCode, string digit, NewKeyStyle style, string simpleTextOverride, int w, int h)
        {
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

            (ADNormalText, _) = GetCharsFromKeys2(vk, false, false);
            (ADShiftText, _) = GetCharsFromKeys2(vk, true, false);

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

    }
}