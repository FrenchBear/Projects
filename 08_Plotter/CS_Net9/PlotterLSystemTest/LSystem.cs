// LSystem.cs
// Simple data class to store the parameters of a L-System
//
// 2021-12-09   PV
// 2021-12-13   PV       Source
// 2023-11-20   PV      Net8 C#12

using System.Collections.Generic;

namespace LSystemTest;

internal sealed class LSystem(string name, string comment, int angle, string axiom, string rules)
{
    public string Name { get; } = name;
    public string Comment { get; } = comment;
    public int Angle { get; } = angle;
    public string Axiom { get; } = axiom;
    public string Rules { get; } = rules;

    public override string ToString() => Name;
}

internal sealed class Source(string name, List<LSystem> list)
{
    public string Name { get; } = name;
    public List<LSystem> LSystems { get; } = list;

    public override string ToString() => Name;
}