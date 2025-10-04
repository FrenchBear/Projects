// UniSearch Extension Methods
// 2020-12-29   PV

using System.Text.RegularExpressions;

namespace UniSearchNS
{
    internal static class ExtensionMethods
    {
        // Convenient helper doing a Regex Match and returning success
        internal static bool IsMatchMatch(this Regex re, string s, out Match ma)
        {
            ma = re.Match(s);
            return ma.Success;
        }
    }
}
