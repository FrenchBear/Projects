// Level 1 tokenizer
//
// Transform a stream of lexer tokens into L1Tokens:
// - Ignore WS
// - Group successive InvalidChar into InvalidToken
// - Transform all I_xx into L1Instruction with attributes
// - Add a property SyntaxCategory, initialized at a reasonable default from lexer perspective but maigh change later
//
// 2025-11-22   PV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace T59v5;

record L1Token
{
    public List<IToken> Tokens { get; set; }
    public SyntaxCategory Cat { get; set; }

    public string AsString()
    {
        var s = string.Join(", ", Tokens.Select(t => $"{t.Line}:{t.Column} {t.Text}"));
        var c = Cat.ToString();
        return $"{this.GetType().Name,-15}: {s,-15} {c}";
    }
}

record L1Eof: L1Token { }

record L1LineComment: L1Token { }
record L1ProgramSeparator: L1Token { }
record L1InvalidToken: L1Token { }
record L1Instruction: L1Token { }
record L1D1: L1Token { }
record L1D2: L1Token { }
record L1A3: L1Token { }
record L1Num: L1Token { }
record L1Tag: L1Token { }
record L1Colon: L1Token { }


internal class L1Tokenizer
{
    private Vocab lexer;

    public L1Tokenizer(Vocab lexer)
    {
        this.lexer = lexer;
    }

    public IEnumerable<L1Token> EnumerateAllTokens()
    {
        IToken? nextToken = null;
        for (; ; )
        {
            IToken token;
            if (nextToken != null)
            {
                token = nextToken;
                nextToken = null;
            }
            else
                token = lexer.NextToken();

            string symbolicName = lexer.Vocabulary.GetSymbolicName(token.Type);
            string typeName = symbolicName ?? lexer.Vocabulary.GetDisplayName(token.Type);
            //Console.WriteLine($"Type: {typeName,-15} Text: '{token.Text}'");

            switch (token.Type)
            {
                case TokenConstants.EOF:
                    var teof = new L1Eof { Tokens = new List<IToken> { token }, Cat = SyntaxCategory.Eof };
                    yield return teof;
                    yield break;

                case Vocab.WS:
                    // Ignore WS, they are useful to split ambiguous sequences, but we don't need them for analysis
                    break;

                case Vocab.LINE_COMMENT:
                    var tlc = new L1LineComment { Tokens = new List<IToken> { token }, Cat = SyntaxCategory.LineComment };
                    yield return tlc;
                    break;

                case Vocab.PROGRAM_SEPARATOR:
                    var tps = new L1ProgramSeparator { Tokens = new List<IToken> { token }, Cat = SyntaxCategory.ProgramSeparator };
                    yield return tps;
                    break;

                case Vocab.D1:      // SyntaxCategory will be determined later
                    var td1 = new L1D1 { Tokens = new List<IToken> { token }, Cat = SyntaxCategory.Unknown };
                    yield return td1;
                    break;

                case Vocab.D2:      // SyntaxCategory will be determined later
                    var td2 = new L1D2 { Tokens = new List<IToken> { token }, Cat = SyntaxCategory.Unknown };
                    yield return td2;
                    break;

                case Vocab.A3:
                    var ta3 = new L1A3 { Tokens = new List<IToken> { token }, Cat = SyntaxCategory.DirectAddress };
                    yield return ta3;
                    break;

                case Vocab.NUM:
                    var tnum = new L1Num { Tokens = new List<IToken> { token }, Cat = SyntaxCategory.Number };
                    yield return tnum;
                    break;

                case Vocab.TAG:
                    var ttag = new L1Tag { Tokens = new List<IToken> { token }, Cat = SyntaxCategory.Tag };
                    yield return ttag;
                    break;

                case Vocab.COLON:       // Category is not really clear, let's assume Tag for now
                    var tco = new L1Colon() { Tokens = new List<IToken> { token }, Cat = SyntaxCategory.Tag };
                    yield return tco;
                    break;

                case Vocab.INVALID_CHAR:
                    var tic = new L1InvalidToken() { Tokens = new List<IToken> { token }, Cat = SyntaxCategory.Invalid };
                    for (; ; )
                    {
                        nextToken = lexer.NextToken();
                        if (nextToken.Type != Vocab.INVALID_CHAR)
                            break;
                        tic.Tokens.Add(nextToken);
                    }
                    yield return tic;
                    break;

                default:
                    Debug.Assert(symbolicName.StartsWith('I'));
                    var ti = new L1Instruction() { Tokens = new List<IToken> { token }, Cat = SyntaxCategory.Instruction };
                    yield return ti;
                    break;
            }
        }
    }
}
