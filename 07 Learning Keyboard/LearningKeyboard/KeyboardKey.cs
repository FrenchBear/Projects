// KeyboardKey
// Specialization of NewKey UserControl to represent a virtual key
//
// 2017-09-19   PV
// 2020-11-17   PV      C#8, nullable enable
// 2020-11-20   PV      Separate InitializeLabels from constructor to update labels after a keyboard layout change

using System.Diagnostics;
using System.Windows.Forms;
using static LearningKeyboard.NativeMethods;

#nullable enable


namespace LearningKeyboard
{
    internal class KeyboardKey : NewKey
    {
        public Keys vk;            // Windows Forms VK
        public readonly string digit;       // L5..L2 R2..R5 = Left hand fichers 5..2 and Right hand fingers 2..5, thumb is not used

        private readonly int scanCode;
        private KeyStyles style;
        private readonly string simpleTextOverride;


        public KeyboardKey(string dispoNF, int scanCode, string digit, KeyStyles style, string simpleTextOverride, int w, int h)
        {
            this.digit = digit;

            Width = w;
            Height = h;

            this.scanCode = scanCode;
            this.style = style;
            this.simpleTextOverride = simpleTextOverride;

            // Initial display
            IsPressed = false;
        }

        public void InitializeLabels()
        {
            vk = (Keys)MapVirtualKey((uint)scanCode, MAPVK_VSC_TO_VK);

            // Style Simple/Detail actually depends on keyboard layout.
            // On French layout, m/M key style is simple while in US layout, the same key is ;/: and style is Detail
            // In fact, only keys a..z have a simple style by convention.  May not be true in all cases (arabic keyboard, ö/ä keys on German keyboards...) but no big deal.
            if (style != KeyStyles.Static)
                style = (vk >= Keys.A && vk <= Keys.Z) ? KeyStyles.Simple : KeyStyles.Detail;

            DefaultKeyStyle = style;
            KeyStyle = style;

            if (style == KeyStyles.Static)
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