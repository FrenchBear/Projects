// Solitaire WPF
// class PlayingCard
// A visual class (inherits from ButtonBase) to represent a Solitaire card and its representation.
// Use Face dependency property to select correct representation in PlayingCards.xaml when visually rendered.
//
// 2019-04-12   PV
// 2020-12-19   PV      .Net 5, C#9, nullable enable
// 2021-11-13   PV      Net6 C#10

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;

#nullable enable

namespace SolWPF;

[DebuggerDisplay("PlayingCard {Signature()}")]
public class PlayingCard: ButtonBase
{
    // Declare and register dependency properties
    public static readonly DependencyProperty FaceProperty = DependencyProperty.Register("Face", typeof(string), typeof(PlayingCard));

    public static readonly DependencyProperty IsFaceUpProperty = DependencyProperty.Register("IsFaceUp", typeof(bool), typeof(PlayingCard));

    public const string Colors = "HSDC";
    public const string Values = "A23456789XJQK";

    private const string SignatureColors = "♥♠♦♣";
    private const string SignatureValues = "A23456789XJQK";
    private const string SignatureFaceUp = "˄";
    private const string SignatureFaceDown = "˅";

    static PlayingCard() =>        // Override style
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PlayingCard), new FrameworkPropertyMetadata(typeof(PlayingCard)));

    public PlayingCard(string face, bool isFaceUp)
    {
        Face = face;
        IsFaceUp = isFaceUp;
    }

    public string Face
    {
        get => (string)GetValue(FaceProperty);
        set => SetValue(FaceProperty, value);
    }

    public bool IsFaceUp
    {
        get => (bool)GetValue(IsFaceUpProperty);
        set => SetValue(IsFaceUpProperty, value);
    }

    // More user-friendly representation than ToString
    internal string Signature() => string.Concat(SignatureValues.AsSpan(Value - 1, 1), SignatureColors.AsSpan(Color, 1), IsFaceUp ? SignatureFaceUp : SignatureFaceDown);

    public override string ToString() => $"PlayingCard {Face}, IsFaceUp={IsFaceUp}";

    //
    public int Color => Colors.IndexOf(Face[0], StringComparison.Ordinal);        // 0..3; %2==0 => Red, %2==1 => Black

    public int Value => Values.IndexOf(Face[1], StringComparison.Ordinal) + 1;    // 1..13
}
