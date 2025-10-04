﻿// UniSearch Extension Methods
//
// 2020-12-29   PV
// 2024-09-27   PV      WinUI3 version

using System.Text.RegularExpressions;

namespace UniSearchWinUI3;

internal static class ExtensionMethods
{
    // Convenient helper doing a Regex Match and returning success
    internal static bool IsMatchMatch(this Regex re, string s, out Match ma)
    {
        ma = re.Match(s);
        return ma.Success;
    }
}
