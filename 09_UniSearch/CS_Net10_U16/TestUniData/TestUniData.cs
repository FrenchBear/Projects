// TestUniData.cs
// Simple tests for UniData library
//
// 2024-05-15   PV      Created to test new Julia tab completions file
// 2026-01-20	PV		Net10 C#14

using System;
using System.Text;
using UniDataNS;

namespace TestUniData;

internal sealed class TestUniData
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Get Unicode data
        var CharactersRecords = UniData.CharacterRecords;
        //var CategoryRecords  = UniData.CategoryRecords;
        //var BlockRecords  = UniData.BlockRecords;

        var c = CharactersRecords[0x1F417];     // boar
        Console.WriteLine($"{c.CodepointHex}\t{c.Name}\t{c.Julia}\t{c.Character}");
        c = CharactersRecords[0x2200];     // forall
        Console.WriteLine($"{c.CodepointHex}\t{c.Name}\t{c.Julia}\t{c.Character}");
    }
}
