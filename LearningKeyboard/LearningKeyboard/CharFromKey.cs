using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static LearningKeyboard.NativeMethods;


namespace LearningKeyboard
{
    internal static class CharFromKey
    {
        internal static Dictionary<int, KeyboardKey> AllKeys = new Dictionary<int, KeyboardKey>();


        /// <summary>
        /// Returns a list of tuples (string Key, string NewKey) giving new generated key
        /// when dicratical key passed in parameter is combined with Key
        /// </summary>
        /// <param name="vkDic">Virtual key of dicratical</param>
        /// <param name="shiftDic">Shift state to get dicratical</param>
        /// <param name="altGrDic">AltGr state to get dicratical</param>
        internal static List<Tuple<string, string>> GetDeadKeyCombinations(Keys vkDic, bool shiftDic, bool altGrDic)
        {
            var l = new List<Tuple<string, string>>();

            string s1, s1d;
            foreach (var k in AllKeys.Values)
            {
                foreach ((bool shift, bool altGr) in new(bool, bool)[] { (false, false), (true, false), (false, true), (true, true) })
                {
                    s1 = GetCharsFromKeys2Simplified(k.vk, shift, altGr);
                    s1d = GetCharsFromKeys2AfterDiacritical(vkDic, shiftDic, altGrDic, k.vk, shift, altGr);
                    if (s1.Length == 1 && s1d.Length == 1 && !l.Any(t => t.Item1==s1)) l.Add(new Tuple<string, string>(s1, s1d));
                }
            }
            return l;
        }

        private static string GetCharsFromKeys2AfterDiacritical(Keys dic, bool shiftDic, bool altGrDic, Keys vk, bool shift, bool altGr)
        {
            GetCharsFromKeys(dic, shiftDic, altGrDic, out bool b0);
            string s = GetCharsFromKeys(vk, shift, altGr, out bool b1);
            GetCharsFromKeys(Keys.Space, false, false, out bool b2);
            return s;
        }

        private static string GetCharsFromKeys2Simplified(Keys vk, bool shift, bool altGr)
        {
            string s = GetCharsFromKeys(vk, shift, altGr, out bool b1);
            GetCharsFromKeys(Keys.Space, false, false, out bool b2);
            return s;
        }

        // The apparently useless calls to GetCharsFromKeys
        // clean internal buffer state, perturbed by previous dicratical
        // characters such as ~ or ^
        private static (string, bool) GetCharsFromKeys2(Keys vk, bool shift, bool altGr)
        {
            string s = GetCharsFromKeys(vk, shift, altGr, out bool b1);
            GetCharsFromKeys(Keys.Space, false, false, out bool b2);
            return (s, b1);
        }

        private static string GetCharsFromKeys(Keys keys, bool shift, bool altGr, out bool isDeadKey)
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
            if (n < 0)
            {
                isDeadKey = true;
                n = 1;
            }
            else
                isDeadKey = false;
            return buf.ToString().Substring(0, n);
        }

    }
}
