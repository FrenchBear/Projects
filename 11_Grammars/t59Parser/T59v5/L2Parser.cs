// Class L2Parser, my own version of T59 language grammar analyzer
// Transforms a stream of L2Token into a stream of L2Statement
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

    public void AddL1Token(L1Token l1t)
        => L1Tokens.Add(l1t);

    internal string AsString()
    {
        var sb = new StringBuilder();

        var invalid = this is L2InvalidStatement;
        if (invalid)
            sb.Append(Couleurs.GetCategoryColor(SyntaxCategory.Invalid));
        sb.Append($"{GetType().Name,-18}: ");
        foreach (var (ix, l1t) in Enumerable.Index(L1Tokens))
        {
            if (ix != 0)
            {
                sb.Append("                    ");
            }
            sb.Append(l1t.AsString(invalid));
            sb.Append('\n');
        }
        if (invalid)
            sb.Append(Couleurs.GetDefaultColor());
        return sb.ToString();
    }

    internal string AsStringWithOpcodes()
    {
        var s = AsString();

        var op1 = this is L2Instruction l2i ? l2i.OpCodes
                  : this is L2Number l2n ? l2n.OpCodes
                  : null;
        if (op1 != null && op1.Count > 0)
            s += "OpCodes: " + string.Join(" ", op1.Select(o => o.ToString("D2"))) + "\n";
        return s;
    }
}

sealed record L2Eof: L2StatementBase
{
    public L2Eof(L1Eof l1i) => AddL1Token(l1i);
}

sealed record L2PgmSeparator: L2StatementBase
{
    public L2PgmSeparator(L1PgmSeparator l1i) => AddL1Token(l1i);
}

sealed record L2LineComment: L2StatementBase
{
    public L2LineComment(L1LineComment l1i) => AddL1Token(l1i);
}

sealed record L2InvalidStatement: L2StatementBase { }

sealed record L2Tag: L2StatementBase { }

sealed record L2Instruction: L2StatementBase
{
    public List<byte> OpCodes { get; set; } = [];
}

sealed record L2Number: L2StatementBase
{
    public List<byte> OpCodes { get; set; } = [];
}

// -----

internal sealed class L2Parser(L1Tokenizer L1T)
{
    enum L2ParserState
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

    public IEnumerable<L2StatementBase> EnumerateL2Statements()
    {
        List<L1Token> Context = [];

        //L1Token? nextToken = null;
        var e = L1T.EnumerateL1Tokens().GetEnumerator();

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

        L2ParserState state = L2ParserState.zero;
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
                case L2ParserState.zero:
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
                            yield return new L2Eof(l1eof);
                            break;

                        case L1PgmSeparator l1ps:
                            yield return new L2PgmSeparator(l1ps);
                            break;

                        // For now, we consider a L1LineComment as a standalone statement, that can't be used
                        // in the middle of an instruction (contrary to most languages).
                        // ToDo: Allow this at some point: STO  // guess where?\r\n  12
                        case L1LineComment l1lc:
                            yield return new L2LineComment(l1lc);
                            break;

                        case L1Tag:
                            Context.Add(token);
                            state = L2ParserState.expect_colon;
                            continue;

                        case L1Num:
                        case L1D2:
                        case L1A3:
                            var l2n = new L2Number();
                            l2n.AddL1Token(token with { Cat = SyntaxCategory.Number });
                            yield return l2n;
                            break;

                        case L1D1 l1d1:
                            var st = new L2Instruction();
                            var l1inst = new L1Instruction() { Tokens = l1d1.Tokens, Cat = SyntaxCategory.Instruction, Inst = new TIKey { Op = [byte.Parse(l1d1.Tokens[0].Text)], M = l1d1.Tokens[0].Text, S = StatementSyntax.a } };
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
                                    state = L2ParserState.expect_d;
                                    break;

                                case StatementSyntax.i:
                                    Context.Add(l1i);
                                    state = L2ParserState.expect_i;
                                    break;

                                case StatementSyntax.di:
                                    Context.Add(l1i);
                                    state = L2ParserState.expect_di;
                                    break;

                                case StatementSyntax.b:
                                    Context.Add(l1i);
                                    state = L2ParserState.expect_b;
                                    break;

                                case StatementSyntax.dib:
                                    Context.Add(l1i);
                                    state = L2ParserState.expect_dib;
                                    break;

                                case StatementSyntax.m:
                                    Context.Add(l1i);
                                    state = L2ParserState.expect_m;
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

                case L2ParserState.expect_d:
                    if (token is L1D1 or L1D2)
                    {
                        Context.Add(token with { Cat = SyntaxCategory.DirectMemoryOrNumber });
                        var l2s = new L2Instruction();
                        l2s.L1Tokens.AddRange(Context);
                        Context.Clear();
                        yield return l2s;
                        state = L2ParserState.zero;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2ParserState.zero;
                        goto RestartAnalysis;
                    }
                    break;

                case L2ParserState.expect_i:
                    if (token is L1D1 or L1D2)
                    {
                        Context.Add(token with { Cat = SyntaxCategory.IndirectMemory });
                        var l2s = new L2Instruction();
                        l2s.L1Tokens.AddRange(Context);     // AddRange because of INV SM* for instance
                        Context.Clear();
                        yield return l2s;
                        state = L2ParserState.zero;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2ParserState.zero;
                        goto RestartAnalysis;
                    }
                    break;

                case L2ParserState.expect_di:
                    if (token is L1Instruction { Inst.Op: [40] })
                    {
                        Context.Add(token);
                        state = L2ParserState.expect_i;
                        continue;
                    }
                    state = L2ParserState.expect_d;
                    goto RestartAnalysis;

                case L2ParserState.expect_b:
                    if (IsLabelMnemonic(token))
                    {
                        Context.Add(token with { Cat = SyntaxCategory.Label });     // Need to recategorise token
                        var l2s = new L2Instruction();
                        l2s.L1Tokens.AddRange(Context);     // AddRange because of INV x=t for instance
                        Context.Clear();
                        yield return l2s;
                        state = L2ParserState.zero;
                        break;
                    }
                    if (token is L1A3 or L1Tag)
                    {
                        Context.Add(token);                 // Already categorized
                        var l2s = new L2Instruction();
                        l2s.L1Tokens.AddRange(Context);     // AddRange because of INV x=t for instance
                        Context.Clear();
                        yield return l2s;
                        state = L2ParserState.zero;
                        break;
                    }
                    if (token is L1D2 d2 && d2.Tokens[0].Text != "40")
                    {
                        // Extension, numeric label 10..99 (except 40)
                        if (!d2.Tokens[0].Text.StartsWith('0'))
                        {
                            Context.Add(token with { Cat = SyntaxCategory.Label });     // Need to categorise token
                            var l2s = new L2Instruction();
                            l2s.L1Tokens.AddRange(Context);     // AddRange because of INV x=t for instance
                            Context.Clear();
                            yield return l2s;
                            state = L2ParserState.zero;
                        }
                        else    // We are in A4 = D2 D2 with 1st D2 starting with 0, for compatibility with .t59 modules dumps
                        {
                            Context.Add(token with { Cat = SyntaxCategory.DirectAddress });     // Need to categorise token
                            state = L2ParserState.expect_a4_part_2;
                        }
                        break;
                    }
                    else if (token is L1Instruction { Inst.Op: [40] })
                    {
                        Context.Add(token);
                        state = L2ParserState.expect_i;
                        continue;
                    }

                    // We've exhausted valid possibilities, token it not accepted
                    // Flush incomplete context as an error
                    foreach (var s in FlushContext(false))
                        yield return s;
                    state = L2ParserState.zero;
                    goto RestartAnalysis;

                case L2ParserState.expect_a4_part_2:
                    if (token is L1D2)
                    {
                        Context.Add(token with { Cat = SyntaxCategory.DirectAddress });     // Need to categorise token
                        var l2s = new L2Instruction();
                        l2s.L1Tokens.AddRange(Context);     // AddRange because of INV x=t for instance
                        Context.Clear();
                        yield return l2s;
                        state = L2ParserState.zero;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2ParserState.zero;
                        goto RestartAnalysis;
                    }
                    break;

                case L2ParserState.expect_dib:
                    if (token is L1Instruction { Inst.Op: [40] })
                    {
                        Context.Add(token);
                        state = L2ParserState.expect_dib_i;
                        break;
                    }
                    state = L2ParserState.expect_dib_d;
                    goto RestartAnalysis;

                case L2ParserState.expect_dib_d:
                    if (token is L1D1 or L1D2)
                    {
                        Context.Add(token with { Cat = SyntaxCategory.DirectMemoryOrNumber });
                        state = L2ParserState.expect_b;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2ParserState.zero;
                        goto RestartAnalysis;
                    }
                    break;

                case L2ParserState.expect_dib_i:
                    if (token is L1D1 or L1D2)
                    {
                        Context.Add(token with { Cat = SyntaxCategory.IndirectMemory });
                        state = L2ParserState.expect_b;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2ParserState.zero;
                        goto RestartAnalysis;
                    }
                    break;

                case L2ParserState.expect_m:
                    if (IsLabelMnemonic(token) || (token is L1D2 ld2 && ld2.Tokens[0].Text != "40" && ld2.Tokens[0].Text[0] != '0'))
                    {
                        Context.Add(token with { Cat = SyntaxCategory.Label });     // Need to recategorise token
                        var l2s = new L2Instruction();
                        l2s.L1Tokens.AddRange(Context);     // AddRange because of INV x=t for instance
                        Context.Clear();
                        yield return l2s;
                        state = L2ParserState.zero;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2ParserState.zero;
                        goto RestartAnalysis;
                    }
                    break;

                case L2ParserState.expect_colon:
                    if (token is L1Colon)
                    {
                        Context.Add(token);
                        var l2s = new L2Tag();
                        l2s.L1Tokens.AddRange(Context);
                        Context.Clear();
                        yield return l2s;
                        state = L2ParserState.zero;
                    }
                    else
                    {
                        // Flush incomplete context as an error
                        foreach (var s in FlushContext(false))
                            yield return s;
                        state = L2ParserState.zero;
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
