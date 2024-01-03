using LibQwirkle;

namespace QwirkleUI;
internal class Player
{
    public string Name { get; set; } = "";
    public int Score { get; set; }
    public string Rank { get; set; } = "";
    public int Index { get; init; }
    public bool IsComputer { get; set; }
    public Hand Hand { get; init; } = [];
}
