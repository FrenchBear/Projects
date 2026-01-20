// Solitaire Solver Library
// class SolverCard
// Implements a game card, Face combines Color and Value such as D3 for a Three of Diamonds, and a boolean indicating if face is up (visible)
//
// 2019-04-07   PV
// 2021-11-13   PV      Net6 C#10
// 2023-11-20   PV      Net8 C#12
// 2025-03-16   PV      Net9 C#13
// 2026-01-20	PV		Net10 C#14

using System;
using System.Collections.Generic;

namespace SolLib;

public class SolverCard(int v, int c, bool isFaceUp)
{
    public const string Colors = "HSDC";
    public const string Values = "A23456789XJQK";

    private const string SignatureColors = "♥♠♦♣";
    private const string SignatureValues = "A23456789XJQK";
    private const string SignatureFaceUp = "˄";
    private const string SignatureFaceDown = "˅";

    public string Face { get; set; } = string.Concat(Colors.AsSpan(c, 1), Values.AsSpan(v - 1, 1));
    public bool IsFaceUp { get; set; } = isFaceUp;

    public int Color => Colors.IndexOf(Face[0], StringComparison.Ordinal);            // 0..3; %2==0 => Red, %2==1 => Black
    public int Value => Values.IndexOf(Face[1], StringComparison.Ordinal) + 1;        // 1..13

    public override string ToString() => $"PlayingCard {Face}, IsFaceUp={IsFaceUp}";

    internal string Signature(bool overrideFaceUp = false) => string.Concat(SignatureValues.AsSpan(Value - 1, 1), SignatureColors.AsSpan(Color, 1), (IsFaceUp || overrideFaceUp) ? SignatureFaceUp : SignatureFaceDown);

    public static List<SolverCard> Set52()
    {
        var Cards = new List<SolverCard>();
        for (int v = 1; v <= 13; v++)
            for (int ci = 0; ci <= 3; ci++)
                Cards.Add(new SolverCard(v, ci, true));
        return Cards;
    }
}   // class SolverCard
