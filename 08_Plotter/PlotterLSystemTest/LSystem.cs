// LSystem.cs
// Simple data class to store the parameters of a L-System
//
// 2021-12-09  PV

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
        Name = name;
        Comment = comment;
        Angle = angle;
        Axiom = axiom;
        Rules = rules;
    }

    public override string ToString() => Name;
}
