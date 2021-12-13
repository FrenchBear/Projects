// LSystem.cs
// Simple data class to store the parameters of a L-System
//
// 2021-12-09  PV
// 2021-12-13  PV   Source

using System.Collections.Generic;
using System.Diagnostics;

namespace LSystemTest;

internal class LSystem
{
    public string Name { get; }
    public string Comment { get; }
    public int Angle { get; }
    public string Axiom { get;  }
    public string Rules { get; }

    public LSystem(string name, string comment, int angle, string axiom, string rules)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(name));
        Debug.Assert(angle >= 3 && angle <= 360);
        Debug.Assert(!string.IsNullOrWhiteSpace(axiom));
        Debug.Assert(!string.IsNullOrWhiteSpace(rules));

        Name = name;
        Comment = comment;
        Angle = angle;
        Axiom = axiom;
        Rules = rules;
    }

    public override string ToString() => Name;
}

internal class Source
{
    public string Name { get; }
    public List<LSystem> LSystems {get;}

    public Source(string name, List<LSystem> list)
    {
        Name = name;
        LSystems = list;
    }

    public override string ToString() => Name;
}
