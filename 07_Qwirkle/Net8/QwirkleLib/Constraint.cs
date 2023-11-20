// Constraint.cs - Represents a tile placement constraint
// Qwirkle simulation project
//
// 2019-01-12   PV
// 2023-11-20   PV      Net8 C#12

using System;
using System.Text;

namespace QwirkleLib;

// Playing constraint base class, either a shape or a color constraint
public abstract class Constraint(int lineAttribute, int blockedMask)
{
    public int LineAttribute = lineAttribute;   // -2=blocked, -1=not constrained, 0..5=constrained
    public int BlockedMask = blockedMask;

    protected string ToStringConstraint(int i1, int i2)
    {
        if (LineAttribute == -2)
            return "Impossi";
        if (LineAttribute == -1)
            return "Unconst";

        var sb = new StringBuilder();
        sb.Append((char)(i1 + LineAttribute));  // Can't construct a StringBuilder from a char (promoted to int and interpreted as initial size)
        for (int i = 0; i < 6; i++)
            if ((BlockedMask & (1 << i)) == 0)
                sb.Append((char)(i2 + i));
            else
                sb.Append('_');
        return sb.ToString();
    }
}

public class ShapeConstraint: Constraint
{
    internal static readonly ShapeConstraint None = new(-1, 0);

    public ShapeConstraint(int lineAttribute, int blockedMask) : base(lineAttribute, blockedMask) { }

    public ShapeConstraint(ShapeConstraint copy) : base(copy.LineAttribute, copy.BlockedMask) { }

    public ShapeConstraint Inter(ShapeConstraint c2)
    {
        // Either one is blocked or different lined consraints -> blocked
        if (LineAttribute == -2 || c2.LineAttribute == -2 || (LineAttribute >= 0 && c2.LineAttribute >= 0 && LineAttribute != c2.LineAttribute))
            return new ShapeConstraint(-2, 63);

        return new ShapeConstraint(Math.Max(LineAttribute, c2.LineAttribute), BlockedMask | c2.BlockedMask);
    }

    public override string ToString() => ToStringConstraint(65, 49);
}

public class ColorConstraint: Constraint
{
    internal static readonly ColorConstraint None = new(-1, 0);

    public ColorConstraint(int lineAttribute, int blockedMask) : base(lineAttribute, blockedMask) { }

    public ColorConstraint(ColorConstraint copy) : base(copy.LineAttribute, copy.BlockedMask) { }

    public ColorConstraint Inter(ColorConstraint c2)
    {
        // Either one is blocked or different lined consraints -> blocked
        if (LineAttribute == -2 || c2.LineAttribute == -2 || (LineAttribute >= 0 && c2.LineAttribute >= 0 && LineAttribute != c2.LineAttribute))
            return new ColorConstraint(-2, 63);

        return new ColorConstraint(Math.Max(LineAttribute, c2.LineAttribute), BlockedMask | c2.BlockedMask);
    }

    public override string ToString() => ToStringConstraint(49, 65);
}

