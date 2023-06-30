// LSystemProcessor class
// Two implementations of a LSystem processor
//
// 2012-02-05   PV  First version
// 2021-09-23   PV  VS2022; Net6

using System.Collections.Generic;
using System.Text;

namespace LSystemTest;

public static class LSystemProcessor
{
    // Basic fast string-implementation of a LSystemProcessor
    // Uses all the memory needed to represent the full string
    // Memory use is exp(depth)
    public static string LSystemString(int depth, string axiom, IDictionary<char, string> rules)
    {
        string s = axiom;
        for (int i = 0; i < depth; i++)
        {
            StringBuilder s2 = new();
            foreach (char c in s)
                _ = rules.TryGetValue(c, out string? value) ? s2.Append(value) : s2.Append(c);
            s = s2.ToString();
        }
        return s;
    }

    // Implementation of LSystemProcessor processing using iterators
    // A bit slower than string, but uses almost no memory for large output
    // Memory use is linear(depth)
    public static IEnumerable<char> LSystemIterator(int depth, IEnumerable<char> axiom, IDictionary<char, string> rules)
    {
        // Special case, depth==0, simply returns axiom
        if (depth == 0)
        {
            foreach (char c in axiom)
                yield return c;
            yield break;
        }

        // General case, apply transformation rules on depth-1 version
        foreach (char c in LSystemIterator(depth - 1, axiom, rules))
        {
            if (rules.TryGetValue(c, out string? value))
            {
                foreach (char c2 in value)
                    yield return c2;
            }
            else
                yield return c;
        }
    }
}