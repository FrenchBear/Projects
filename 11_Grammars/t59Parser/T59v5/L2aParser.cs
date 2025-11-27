// Class L2aParser, my own version of T59 language grammar analyzer
// Transforms a stream of L1Token into a stream of L2Statement
//
// 2025-11-21   PV      First version

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace T59v5;

// -----

abstract record L2StatementBase
{
    public List<L1Token> L1Tokens { get; set; } = [];
    public T59Message? Message { get; set; }

    public void AddL1Token(L1Token l1t)
        => L1Tokens.Add(l1t);

    internal string AsDebugString()
    {
        var sb = new StringBuilder();
        var invalid = this is L2InvalidStatement;
        if (invalid)
            sb.Append(Couleurs.GetCategoryColor(SyntaxCategory.Invalid));
        sb.Append($"{GetType().Name,-18}: ");
        foreach (var (ix, l1t) in Enumerable.Index(L1Tokens))
        {
            if (ix != 0)
                sb.Append("                    ");
            sb.Append(l1t.AsDebugString(invalid));
            sb.Append('\n');
        }
        if (invalid)
            sb.Append(Couleurs.GetDefaultColor());
        return sb.ToString();
    }

    internal string AsDebugStringWithOpcodes()
    {
        var s = AsDebugString();
        var op1 = this is L2Instruction l2i ? l2i.OpCodes
                  : this is L2Number l2n ? l2n.OpCodes
                  : null;
        if (op1 != null && op1.Count > 0)
            s += "OpCodes: " + string.Join(" ", op1.Select(o => o.ToString("D2"))) + "\n";
        return s;
    }

    internal virtual string AsFormattedString()
    {
        var sb = new StringBuilder();
        var invalid = this is L2InvalidStatement;
        if (invalid)
            sb.Append(Couleurs.GetCategoryColor(SyntaxCategory.Invalid));
        sb.Append(string.Join(" ", L1Tokens.Select(l1t => l1t.AsFormattedString(invalid))));
        sb.Append(Couleurs.GetDefaultColor());
        return sb.ToString();
    }
}

sealed record L2Eof: L2StatementBase { }

sealed record L2LineComment: L2StatementBase { }

sealed record L2InvalidStatement: L2StatementBase { }

abstract record L2AddressableInstructionBase: L2StatementBase
{
    public int Address { get; set; }
    public List<byte> OpCodes { get; set; } = [];
}

sealed record L2Tag: L2AddressableInstructionBase
{
    public required string Tag { get; set; }

    internal override string AsFormattedString()
        => string.Join("", L1Tokens.Select(l1t => l1t.AsFormattedString()));
}

sealed record L2Instruction: L2AddressableInstructionBase { }

sealed record L2Number: L2AddressableInstructionBase
{
    internal override string AsFormattedString()
    {
        StringBuilder mantissa = new();
        StringBuilder exponent = new();
        bool inExponent = false;

        foreach (var op in OpCodes)
        {
            switch (op)
            {
                case >= 0 and <= 9:
                    if (inExponent)
                        exponent.Append((char)(48 + op));
                    else
                        mantissa.Append((char)(48 + op));
                    break;
                case 93:
                    if (inExponent || mantissa.ToString().Contains('.'))
                        Debugger.Break();
                    mantissa.Append('.');
                    break;
                case 94:
                    if (inExponent)
                    {
                        if (exponent.Length > 0 && exponent[0] == '-')
                            Debugger.Break();
                        else
                            exponent.Append('.');
                    }
                    else
                    {
                        if (mantissa.Length > 0 && mantissa[0] == '-')
                            Debugger.Break();
                        else
                            mantissa.Append('.');
                    }
                    break;
                case 52:
                    if (inExponent)
                        Debugger.Break();
                    inExponent = true;
                    break;
                default:
                    Debugger.Break();
                    break;
            }
        }

        var res = mantissa.ToString();
        if (exponent.Length > 0)
            res += "E" + mantissa.ToString();
        return Couleurs.GetCategoryColor(SyntaxCategory.Number) + res + Couleurs.GetDefaultColor();
    }
}

// -----

internal sealed class L2aParser(T59Program Prog)
{
    enum L2aParserState
    {
        zero,
        expect_d,           // Expects D1 or D2, to be retagged as SyntaxCategory.DirectMemoryOrNumber
        expect_i,           // Expects D1 or D2, to be retagged as SyntaxCategory.IndirectMemory
        expect_di,          // If 'Ind' -> expect_i else -> expect_d
        expect_b,           // Expects mnemonic (L1Instruction excepted Ind), A3, A2 (10-99), A2 (00-09) -> expect_a4_part_2, Ind -> expect_i, L1TAG
        expect_dib,
        expect_dib_d,
        expect_dib_i,
        expect_m,           // Expects mnemonic (L1Instruction excepted Ind)
        expect_a4_part_2,   // When expect_b finds a D2 starting with '0', valid follow-up is D2
        expect_colon,       // After a tag at state 0, expects :
    }

    public void L2Process() => Prog.L2Statements.AddRange(EnumerateL2Statements());

    private IEnumerable<L2StatementBase> EnumerateL2Statements()
    {
        List<L1Token> Context = [];

        var e = Prog.L1TokensWithoutWhiteSpace().GetEnumerator();

        bool IsContextInv()
            => Context.Count > 0 && Context[0] is L1Instruction { Inst.Op: [22] };

        // If we have a non-empty context, then we must flush it as a L2InvalidStatement
        IEnumerable<L2StatementBase> FlushContext(bool keepOnlyIndStatement)
        {
            // Special case, if the context starts by Inv, then we emit a legitimate L2Statement for this prefix,
            // it can't be considered as an error
            if (IsContextInv())
            {
                if (keepOnlyIndStatement && Context.Count == 1)
                    yield break;

                var s = new L2Instruction();
                s.L1Tokens.Add(Context[0]);
                yield return s;
                Context.RemoveAt(0);
            }

            if (Context.Count > 0)
            {
                var err = new L2InvalidStatement();
                err.L1Tokens.AddRange(Context);
                yield return err;
                Context.Clear();
            }

            yield break;
        }

        // Helper to create a new L2Instruction from current context after adding l1t
        L2Instruction BuildL2Instruction(L1Token l1t)
        {
            Context.Add(l1t);
            var l2s = new L2Instruction();
            l2s.L1Tokens.AddRange(Context);
            Context.Clear();
            return l2s;
        }

        L2aParserState state = L2aParserState.zero;
        for (; ; )
        {
            // Don't need peeking support, so a simple foreach should be enough

            //L1Token token;
            //if (nextToken != null)
            //{
            //    token = nextToken;
            //    nextToken = null;
            //}
            //else
            //{
            //    if (!e.MoveNext())
            //        break;
            //    token = e.Current;
            //}

            if (!e.MoveNext())
                break;
            L1Token token = e.Current;

        // If at some point we get an unexpected token, we emit context as error and flush it, revert to state zero
        // and we restart analysis at unexpected token, not at next token
        // We control precisely the lexer restart context at unexpected input, what I haven't found how to do reliabily
        // with ANTLR4 generated parser
        RestartAnalysis:
            switch (state)
            {
                case L2aParserState.zero:
                    // We marge (L1InvalidToken or illegal token in state 0) with current state and we remain at state 0,
                    // will be flushed at next L1Token that is accepted at state 0.
                    // In theory we should always get L1Eof or L1ProgramSeparator before the end of the flow, and these
                    // L1Token are valid in state 0
                    if (token is L1InvalidToken)
                    {
                        Context.Add(token);
                        continue;
                    }

                    // At this point in state 0, if we have a context, that's a problem.
                    // Just flush existing context as InvalidStatement
                    // If current token is an invertible instruction and current context is just {Inv}, we keep it
                    foreach (var s in FlushContext(token is L1Instruction { Inst.I: true }))
                        yield return s;

                    switch (token)
                    {
                        case L1Eof l1eof:
                            var l2eof = new L2Eof();
                            l2eof.L1Tokens.Add(l1eof);
                            yield return l2eof;
                            break;

                        // For now, we consider a L1LineComment as a standalone statement, that can't be used
                        // in the middle of an instruction (contrary to most languages).
                        // Allow this at some point: STO  // guess where?\r\n  12
                        case L1LineComment l1lc:
                            var l2lc = new L2LineComment();
                            yield return l2lc;
                            l2lc.L1Tokens.Add(l1lc);
                            break;

                        case L1Tag:
                            Context.Add(token);
                            state = L2aParserState.expect_colon;
                            continue;

                        case L1Num or L1D2 or L1A3 or L1A4:
                            var l2n = new L2Number();
                            l2n.AddL1Token(token with { Cat = SyntaxCategory.Number });
                            yield return l2n;
                            break;

                        case L1D1 l1d1:
                            var st = new L2Instruction();
                            var l1inst = new L1Instruction() { L0Tokens = l1d1.L0Tokens, Cat = SyntaxCategory.Instruction, Inst = new TIKey { Op = [byte.Parse(l1d1.L0Tokens[0].Text)], M = l1d1.L0Tokens[0].Text, S = StatementSyntax.a } };
                            st.L1Tokens.Add(l1inst);
                            yield return st;
                            break;

                        case L1Instruction l1i:
                            // Special case: Fix or SBR following INV is atomic
                            if (IsContextInv() && l1i.Inst.Op is [58] or [71])
                            {
                                Context.Add(l1i);
                                var l2f = new L2Instruction();
                                l2f.L1Tokens.AddRange(Context);
                                Context.Clear();
                                yield return l2f;
                                break;
                            }

                            switch (l1i.Inst.S)
                            {
                                case StatementSyntax.a:
                                    // Special case, INV, we just add it to the context and continue at state 0, since it's potentially
                                    // part of next instruction
                                    if (l1i.Inst.Op is [22])
                                    {
                                        Context.Add(l1i);
                                        continue;
                                    }
                                    // For all other alomic stateements, we're done: emit L2Instruction, clear context, continue
                                    Context.Add(l1i);
                                    var l2s = new L2Instruction();
                                    l2s.L1Tokens.AddRange(Context);
                                    Context.Clear();
                                    yield return l2s;
                                    break;

                                case StatementSyntax.d:
                                    Context.Add(l1i);
                                    state = L2aParserState.expect_d;
                                    break;

                                case StatementSyntax.i:
                                    Context.Add(l1i);
                                    state = L2aParserState.expect_i;
                                    break;

                                case StatementSyntax.di:
                                    Context.Add(l1i);
                                    state = L2aParserState.expect_di;
                                    break;

                                case StatementSyntax.b:
                                    Context.Add(l1i);
                                    state = L2aParserState.expect_b;
                                    break;

                                case StatementSyntax.dib:
                                    Context.Add(l1i);
                                    state = L2aParserState.expect_dib;
                                    break;

                                case StatementSyntax.m:
                                    Context.Add(l1i);
                                    state = L2aParserState.expect_m;
                                    break;

                                case StatementSyntax.p:
                                    // Ind is not valid as a statement
                                    Context.Add(token);
                                    foreach (var s in FlushContext(false))
                                        yield return s;
                                    break;

                                default:
                                    // Unreachable
                                    Debugger.Break();
                                    break;
                            }
                            break;

                        default:
                            // Unreachable
                            Debugger.Break();
                            break;
                    }
                    break;

                case L2aParserState.expect_d:
                    if (token is L1D1 or L1D2)
                    {
                        yield return BuildL2Instruction(token with { Cat = SyntaxCategory.DirectMemoryOrNumber });
                        state = L2aParserState.zero;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2aParserState.zero;
                        goto RestartAnalysis;
                    }
                    break;

                case L2aParserState.expect_i:
                    if (token is L1D1 or L1D2)
                    {
                        yield return BuildL2Instruction(token with { Cat = SyntaxCategory.IndirectMemory });
                        state = L2aParserState.zero;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2aParserState.zero;
                        goto RestartAnalysis;
                    }
                    break;

                case L2aParserState.expect_di:
                    if (token is L1Instruction { Inst.Op: [40] })
                    {
                        Context.Add(token);
                        state = L2aParserState.expect_i;
                        continue;
                    }
                    state = L2aParserState.expect_d;
                    goto RestartAnalysis;

                case L2aParserState.expect_b:
                    if (IsLabelMnemonic(token))
                    {
                        yield return BuildL2Instruction(token with { Cat = SyntaxCategory.Label });
                        state = L2aParserState.zero;
                        break;
                    }
                    if (token is L1A3 or L1A4 or L1Tag)
                    {
                        yield return BuildL2Instruction(token);
                        state = L2aParserState.zero;
                        break;
                    }
                    if (token is L1D2 d2 && d2.L0Tokens[0].Text != "40")
                    {
                        // Extension, numeric label 10..99 (except 40)
                        if (!d2.L0Tokens[0].Text.StartsWith('0'))
                        {
                            yield return BuildL2Instruction(token with { Cat = SyntaxCategory.Label });
                            state = L2aParserState.zero;
                        }
                        else    // We are in A4 = D2 D2 with 1st D2 starting with 0, for compatibility with .t59 modules dumps
                        {
                            Context.Add(token with { Cat = SyntaxCategory.DirectAddress });     // Need to categorise token
                            state = L2aParserState.expect_a4_part_2;
                        }
                        break;
                    }
                    else if (token is L1Instruction { Inst.Op: [40] })
                    {
                        Context.Add(token);
                        state = L2aParserState.expect_i;
                        continue;
                    }

                    // We've exhausted valid possibilities, token it not accepted
                    // Flush incomplete context as an error
                    foreach (var s in FlushContext(false))
                        yield return s;
                    state = L2aParserState.zero;
                    goto RestartAnalysis;

                case L2aParserState.expect_a4_part_2:
                    if (token is L1D2 second)
                    {
                        var first = Context[^1];
                        var t1a4 = new L1A4 { L0Tokens = [.. first.L0Tokens, .. second.L0Tokens] };
                        Context.RemoveAt(Context.Count - 1);
                        // Upgrade L1D2 L1D2 to a new L1A4, will make later addresses checking much easier by only checking L1A3, L1A4 and L1TAG
                        yield return BuildL2Instruction(t1a4 with { Cat = SyntaxCategory.DirectAddress });
                        state = L2aParserState.zero;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2aParserState.zero;
                        goto RestartAnalysis;
                    }
                    break;

                case L2aParserState.expect_dib:
                    if (token is L1Instruction { Inst.Op: [40] })
                    {
                        Context.Add(token);
                        state = L2aParserState.expect_dib_i;
                        break;
                    }
                    state = L2aParserState.expect_dib_d;
                    goto RestartAnalysis;

                case L2aParserState.expect_dib_d:
                    if (token is L1D1 or L1D2)
                    {
                        Context.Add(token with { Cat = SyntaxCategory.DirectMemoryOrNumber });
                        state = L2aParserState.expect_b;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2aParserState.zero;
                        goto RestartAnalysis;
                    }
                    break;

                case L2aParserState.expect_dib_i:
                    if (token is L1D1 or L1D2)
                    {
                        Context.Add(token with { Cat = SyntaxCategory.IndirectMemory });
                        state = L2aParserState.expect_b;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2aParserState.zero;
                        goto RestartAnalysis;
                    }
                    break;

                case L2aParserState.expect_m:
                    if (IsLabelMnemonic(token) || (token is L1D2 ld2 && ld2.L0Tokens[0].Text != "40" && ld2.L0Tokens[0].Text[0] != '0'))
                    {
                        yield return BuildL2Instruction(token with { Cat = SyntaxCategory.Label });
                        state = L2aParserState.zero;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2aParserState.zero;
                        goto RestartAnalysis;
                    }
                    break;

                case L2aParserState.expect_colon:
                    if (token is L1Colon)
                    {
                        // Build a L2Tag here, not a L2Instruction, can't reuse BuildL2Instruction
                        Context.Add(token);
                        var l2t = new L2Tag { Tag = Context[0].L0Tokens[0].Text };
                        l2t.L1Tokens.AddRange(Context);
                        Context.Clear();
                        yield return l2t;
                        state = L2aParserState.zero;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2aParserState.zero;
                        goto RestartAnalysis;
                    }
                    break;

                default:
                    // Unreachable
                    Debugger.Break();
                    break;
            }
        }

        yield break;
    }

    // Any instruction in TIKeys can be a mnemonic except Ind
    // Manual (V-56) tells that we should avoid R/S but it's still legitimate
    // I've tested mergeg codes, they are also Ok
    private static bool IsLabelMnemonic(L1Token token) => token is L1Instruction l1inst && l1inst.Inst.Op is not [40];
}
