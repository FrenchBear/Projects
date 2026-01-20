// UniSearch Extension Methods
//
// 2020-12-29   PV
// 2026-01-20	PV		Net10 C#14

using System.Text.RegularExpressions;

namespace UniSearch;

internal static class ExtensionMethods
{
    // Convenient helper doing a Regex Match and returning success
    internal static bool IsMatchMatch(this Regex re, string s, out Match ma)
    {
        ma = re.Match(s);
        return ma.Success;
    }
}