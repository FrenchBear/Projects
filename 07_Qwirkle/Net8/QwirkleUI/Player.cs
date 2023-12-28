using LibQwirkle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwirkleUI;
internal class Player
{
    public string Name { get; set; } = "";
    public int Score { get; set; }
    public int Index { get; init; }
    public bool IsComputer { get; set; }
    public Hand Hand { get; init; } = new();
}
