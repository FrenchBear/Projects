using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSystemTest;

internal class LSystem
{
    public string Name { get; set; }
    public string Comment { get; set; }
    public int Angle { get; set; }
    public string Axiom { get; set; }
    public string Rules { get; set; }

    public override string ToString() => Name;
}

