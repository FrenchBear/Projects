using System;
using System.Diagnostics;
using System.Text;

namespace QwirkleLib
{
    // A playing constraint
    public struct Constraint
    {
        internal static readonly Constraint None = new Constraint(-1, 0);

        public int LineAttribute;   // -2=blocked, -1=not constrained, 0..5=constrained
        public int BlockedMask;

        public Constraint(int lineAttribute, int blockedMask)
        {
            LineAttribute = lineAttribute;
            BlockedMask = blockedMask;
        }


        public Constraint Inter(Constraint c2)
        {
            // Either one is blocked or different lined consraints -> blocked
            if (LineAttribute == -2 || c2.LineAttribute == -2 || (LineAttribute>=0 && c2.LineAttribute>=0 && LineAttribute != c2.LineAttribute))
                return new Constraint(-2, 63);

            return new Constraint(Math.Max(LineAttribute, c2.LineAttribute), BlockedMask | c2.BlockedMask);
        }


        public string ToShapeConstraint => ToStringConstraint(65, 49);
        public string ToColorConstraint => ToStringConstraint(49, 65);

        public string ToStringConstraint(int i1, int i2)
        {
            if (LineAttribute == -2)
                return "Blocked";
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
}
