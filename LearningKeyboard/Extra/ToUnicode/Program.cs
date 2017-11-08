// ToUnicode
// Exercices to understand/test keyboard related stuff
// 2017-09  PV


using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static System.Console;
using static LearningKeyboard.NativeMethods;
using System.Collections.Generic;
using System.Diagnostics;

namespace ToUnicodeApp
{

    class Program
    {
        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint virtualKeyCode,
            uint scanCode,
            byte[] keyboardState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder receivingBuffer,
            int bufferSize,
            uint flags);


        static void Main()
        {
            Console.OutputEncoding = new UnicodeEncoding();        // UTF8Encoding does not work with Windows 7, needs Unicode encoding (UTF-16)

            PrepareStandardLayout();
            PrepareDicraticalCombinations();
            WriteLine();

            Keys vk = Keys.R;
            (string NormalText, bool IsNormalDeadKey) = GetCharsFromKeys2(vk, false, false);
            (string ShiftText, bool IsShiftDeadKey) = GetCharsFromKeys2(vk, true, false);
            (string AltGrText, bool IsAltGrDeadKey) = GetCharsFromKeys2(vk, false, true);
            (string ShiftAltGrText, bool IsShiftAltGrDeadKey) = GetCharsFromKeys2(vk, true, true);


            WriteLine("Key: " + vk.ToString());
            WriteLine($"Normal:     {NormalText}\t{IsNormalDeadKey}");
            WriteLine($"Shift:      {ShiftText}\t{IsShiftDeadKey}");
            WriteLine($"AltGr:      {AltGrText}\t{IsAltGrDeadKey}");
            WriteLine($"ShiftAltGr: {ShiftAltGrText}\t{IsShiftAltGrDeadKey}");

            // Retrieve dead keys combinations
            WriteLine();

            // µ
            var combinations = GetDeadKeyCombinations(Keys.Oem5, true, false);
            //Write("µ: ");
            WriteLine("var dki = new Tuple(Keys.Oem5, true, false);");
            WriteLine("var lc = new List<Tuple<string, string>>();");
            foreach (var item in combinations)
                WriteLine($"lc.Add(new Tuple<string,string>(\"{item.Item1}\", \"{item.Item2}\"));");
            WriteLine("dicCombi.Add(dki, lc);");
            WriteLine();

            /*
            foreach (Keys vkl in new Keys[] { Keys.A, Keys.B, Keys.C, Keys.D, Keys.D2 })
                foreach (bool shift in new bool[] { false, true })
                {
                    string s1 = GetCharsFromKeys2Simplified(vkl, shift, false);
                    string s1d = GetCharsFromKeys2AfterDiacritical(Keys.Oem5, true, false, vkl, shift, false);
                    WriteLine($"µ+{s1}: " + s1d);
                }

            WriteLine();
            foreach (Keys vkl in new Keys[] { Keys.L, Keys.E, Keys.R })
                foreach ((bool shift, bool altGr) in new(bool, bool)[] { (false, false), (true, false), (false, true) })
                {
                    string s1 = GetCharsFromKeys2Simplified(vkl, shift, altGr);
                    string s1d = GetCharsFromKeys2AfterDiacritical(Keys.Oem1, false, true, vkl, shift, altGr);
                    WriteLine($"¤+{s1}: " + s1d);
                }
            */

            Console.WriteLine();
            Console.Write("(Pause)");
            Console.ReadLine();
        }

        private static void PrepareDicraticalCombinations()
        {
            foreach (var key in AllKeys.Values)
            {
                if (key.IsNormalDeadKey) PrepareDicraticalCombinationsOneKey(key.vk, false, false);
                if (key.IsShiftDeadKey) PrepareDicraticalCombinationsOneKey(key.vk, true, false);
                if (key.IsAltGrDeadKey) PrepareDicraticalCombinationsOneKey(key.vk, false, true);
                if (key.IsShiftAltGrDeadKey) PrepareDicraticalCombinationsOneKey(key.vk, true, true);
            }
        }

        static Dictionary<Tuple<Keys, bool, bool>, List<Tuple<string, string>>> dicCombi = new Dictionary<Tuple<Keys, bool, bool>, List<Tuple<string, string>>>();
        private static void PrepareDicraticalCombinationsOneKey(Keys vk, bool shift, bool altGr)
        {
            var combinations = GetDeadKeyCombinations(vk, shift, altGr);
            var dki = new Tuple<Keys, bool, bool>(vk, shift, altGr);
            dicCombi.Add(dki, combinations);
        }

        /// <summary>
        /// Returns a list of tuples (string Key, string NewKey) giving new generated key
        /// when dicratical key passed in parameter is combined with Key
        /// </summary>
        /// <param name="dic">Virtual key of dicratical</param>
        /// <param name="shiftDic">Shift state to get dicratical</param>
        /// <param name="altGrDic">AltGr state to get dicratical</param>
        private static List<Tuple<string, string>> GetDeadKeyCombinations(Keys dic, bool shiftDic, bool altGrDic)
        {
            var l = new List<Tuple<string, string>>();

            string s1, s1d;
            foreach (var k in AllKeys.Values)
            {
                foreach ((bool shift, bool altGr) in new(bool, bool)[] { (false, false), (true, false), (false, true), (true, true) })
                {
                    s1 = GetCharsFromKeys2Simplified(k.vk, shift, altGr);
                    s1d = GetCharsFromKeys2AfterDiacritical(dic, shiftDic, altGrDic, k.vk, shift, altGr);
                    if (s1.Length == 1 && s1d.Length == 1) l.Add(new Tuple<string, string>(s1, s1d));
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


        private static void PrepareStandardLayout()
        {
            AddKey("E00", 41, "L5", "²");
            AddKey("E01", 2, "L5", "& 1");
            AddKey("E02", 3, "L4", "é 2 ~");
            AddKey("E03", 4, "L3", "\" 3 #");
            AddKey("E04", 5, "L2", "4 ' {");
            AddKey("E05", 6, "L2", "5 ( [");
            AddKey("E06", 7, "R2", "6 - |");
            AddKey("E07", 8, "R2", "7 è `");
            AddKey("E08", 9, "R3", "8 _ \\");
            AddKey("E09", 10, "R4", "9 ç ^");
            AddKey("E10", 11, "R5", "0 à @");
            AddKey("E11", 12, "R5", "° ) ]");
            AddKey("E12", 13, "R5", "+ = }");
            AddKey("E13", 14, "R5*", "Backspace");

            AddKey("D00", 15, "L5*", "Tab");
            AddKey("D01", 16, "L5", "a");
            AddKey("D02", 17, "L4", "z");
            AddKey("D03", 18, "L3", "e €");
            AddKey("D04", 19, "L2", "r");
            AddKey("D05", 20, "L2", "t");
            AddKey("D06", 21, "R2", "y");
            AddKey("D07", 22, "R2", "u");
            AddKey("D08", 23, "R3", "i");
            AddKey("D09", 24, "R4", "o");
            AddKey("D10", 25, "R5", "p");
            AddKey("D11", 26, "R5", "^ ¨");
            AddKey("D12", 27, "R5", "$ £ ¤");

            AddKey("C00", 58, "L5*", "CapsLock");
            AddKey("C01", 30, "L5", "q");
            AddKey("C02", 31, "L4", "s");
            AddKey("C03", 32, "L3", "d");
            AddKey("C04", 33, "L2", "f");
            AddKey("C05", 34, "L2", "g");
            AddKey("C06", 35, "R2", "h");
            AddKey("C07", 36, "R2", "j");
            AddKey("C08", 37, "R3", "k");
            AddKey("C09", 38, "R4", "l");
            AddKey("C10", 39, "R5", "m");
            AddKey("C11", 40, "R5", "ù %");
            AddKey("C12", 43, "R5", "* µ");
            AddKey("C13", 28, "R5*", "Enter");

            AddKey("BL", 42, "", "Shift");
            AddKey("B00", 86, "L5", "< >");
            AddKey("B01", 44, "L5", "w");
            AddKey("B02", 45, "L4", "x");
            AddKey("B03", 46, "L3", "c");
            AddKey("B04", 47, "L2", "v");
            AddKey("B05", 48, "L2", "b");
            AddKey("B06", 49, "R2", "n");
            AddKey("B07", 50, "R2", ", ?");
            AddKey("B08", 51, "R3", "; .");
            AddKey("B09", 52, "R4", ": /");
            AddKey("B10", 53, "R5", "! §");
            AddKey("B11", 54, "", "Shift");

            AddKey("A00", 29, "", "Ctrl");
            AddKey("A01", 91, "", "W");
            AddKey("A02", 56, "", "Alt");
            AddKey("A03", 57, "", "Space");
            AddKey("A04", 541, "", "Alt Gr");
            AddKey("A05", 92, "", "W");
            AddKey("A06", 93, "", "Menu");
            AddKey("A07", 329, "", "Ctrl");
        }

        static Dictionary<int, AKey> AllKeys = new Dictionary<int, AKey>();


        private static void AddKey(string dispoNF, int scanCode, string finger, string label)
        {
            if (string.IsNullOrEmpty(finger) || finger.IndexOf('*') >= 0)
                return;

            Keys vk = (System.Windows.Forms.Keys)MapVirtualKey((uint)scanCode, MAPVK_VSC_TO_VK);
            AKey k = new AKey(dispoNF, scanCode, finger, label, vk);

            (k.NormalText, k.IsNormalDeadKey) = GetCharsFromKeys2(vk, false, false);
            (k.ShiftText, k.IsShiftDeadKey) = GetCharsFromKeys2(vk, true, false);
            (k.AltGrText, k.IsAltGrDeadKey) = GetCharsFromKeys2(vk, false, true);
            (k.ShiftAltGrText, k.IsShiftAltGrDeadKey) = GetCharsFromKeys2(vk, true, true);

            AllKeys.Add(scanCode, k);

            //if (!string.IsNullOrEmpty(k.NormalText) || !string.IsNullOrEmpty(k.ShiftText) || !string.IsNullOrEmpty(k.AltGrText) || !string.IsNullOrEmpty(k.ShiftAltGrText))
            //    WriteLine($"{vk.ToString(),-16} {k.NormalText,-2} {k.ShiftText,-2}  {k.AltGrText,-2}  {k.ShiftAltGrText,-2}");
        }
    }

    class AKey
    {
        public readonly string dispoNF;
        public readonly int scanCode;
        public readonly string finger;
        public readonly string label;
        public readonly Keys vk;
        public string NormalText, ShiftText, AltGrText, ShiftAltGrText;
        public bool IsNormalDeadKey, IsShiftDeadKey, IsAltGrDeadKey, IsShiftAltGrDeadKey;

        public AKey(string dispoNF, int scanCode, string finger, string label, Keys vk)
        {
            this.dispoNF = dispoNF;
            this.scanCode = scanCode;
            this.finger = finger;
            this.label = label;
            this.vk = vk;
        }
    }
}
