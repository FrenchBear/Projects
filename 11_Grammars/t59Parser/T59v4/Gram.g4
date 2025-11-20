// Fourth variant of T59 grammar - parser
// Don't use INVALID_TOKEN anymore
//
// 2025-11-20   PV

grammar Gram;

options {
	tokenVocab = Vocab;
	caseInsensitive = true;
}

program: statement+ EOF;

statement:
    number_statement
	| atomic_statement
	| d_statement           // Direct address
    | i_statement           // Indirect address
    | bd_statement          // Branch direct or mnemonic
    | bi_statement          // Branch indirect
    | t_statement           // 4 variants of test and branch
	| label_statement
    | inv_statement         // Must be last
    ;

// These two rules xan't be defined in Vocab.g4
number_statement: NUM|D1|D2|A3|A4;
mnemonic: I24_correct_entry|I25_clear|I42_store|I43_recall;         // See TI doc, 72 variants available

atomic_statement: 
    inv=I22_invert? sta=(I38_sin|I39_cos|I30_tan|I71_subroutine)
    | sta=(I25_clear|I24_correct_entry|I66_pause|I68_nop|I65_multiply|I92_return)
    ;

d_statement: 
    sta=(I42_store|I43_recall|I82_hir) t=(D1|D2)            // direct, non-invertible
    | inv=I22_invert? sta=I44_sum t=(D1|D2)                 // direct, invertible
    ;

i_statement:
    sta=(I62_program_indirect|I63_exchange_indirect|I72_store_indirect|I73_recall_indirect) t=(D1|D2)   // Merged non-invertible indirect statements that do not use Ind
    | inv=I22_invert? sta=(I64_product_indirect|I67_x_equals_t_indextra|I74_sum_indirect|I77_x_greater_or_equal_than_t_indextra) t=(D1|D2)  // Merged invertible indirect statements that do not use Ind
    | sta=(I42_store|I43_recall|I44_sum) ind=I40_indirect t=(D1|D2)   // Non-invertible statements that use Ind
    | inv=I22_invert? sta=I44_sum ind=I40_indirect t=(D1|D2)   // Invertible statements that use Ind
    ;

ad_statement: mnemonic|t=(D2|A3|A4);
ai_statement: I40_indirect t=(D1|D2);
bd_statement:
    sta=(I61_goto|I71_subroutine) ad_statement
    | inv=I22_invert? sta=(I67_x_equals_t|I77_x_greater_or_equal_than_t) ad_statement
    ;

bi_statement:
    sta=(I61_goto|I71_subroutine) ai_statement
    | inv=I22_invert? sta=(I67_x_equals_t|I77_x_greater_or_equal_than_t) ai_statement
    ;

doi_lai_statement:
    inv=I22_invert? sta=(I87_if_flag|I97_dsz) (D1|D2|(I40_indirect (D1|D2))) (mnemonic|D2|A3|A4|(I40_indirect (D1|D2)))
    ;

t_statement: tdd_statement | tdi_statement | tid_statement | tii_statement;
td_statement: D1|D2;
ti_statement: I40_indirect (D1|D2);
tdd_statement: inv=I22_invert? sta=(I87_if_flag|I97_dsz) td_statement ad_statement;
tdi_statement: inv=I22_invert? sta=(I87_if_flag|I97_dsz) td_statement ai_statement;
tid_statement: inv=I22_invert? sta=(I87_if_flag|I97_dsz) ti_statement ad_statement;
tii_statement: inv=I22_invert? sta=(I87_if_flag|I97_dsz) ti_statement ai_statement;

label_statement: sta=I76_label (D1|D2|mnemonic);

inv_statement: sta=I22_invert;
