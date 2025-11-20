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
	| di_statement
    | d_statement
    | lai_statement
    | doi_lai_statement
	| label_statement
    | inv_statement         // Must be last
    ;

// These two rules xan't be defined in Vocab.g4
number_statement: INT|D1|D2|A3|A4;
mnemonic: I24_correct_entry|I25_clear|I42_store|I43_recall;         // See TI doc, 72 variants available

atomic_statement: 
    inv=I22_invert? sta=(I38_sin|I39_cos|I30_tan)
    | sta=I25_clear
    | sta=I24_correct_entry
    ;

di_statement: 
    sta=(I42_store|I43_recall) ind=I40_indirect? t=(D1|D2)
    | inv=I22_invert? sta=I44_sum ind=I40_indirect? t=(D1|D2)
    ;

d_statement:
    sta=(I62_program_indirect|I63_exchange_indirect|I72_store_indirect|I73_recall_indirect|I82_hir) t=(D1|D2)
    | inv=I22_invert? sta=(I64_product_indirect|I67_x_equals_t_indextra|I74_sum_indirect|I77_x_greater_or_equal_than_t_indextra) t=(D1|D2)
    ;

// lai_addr: mnemonic|t=(D2|A3|A4)|(I40_indirect t=(D1|D2));

// lai_statement:
//     sta=I61_goto lai_addr
//     | inv=I22_invert? sta=I67_x_equals_t lai_addr
//     ;

lai_statement:
    sta=I61_goto (mnemonic|t=(D2|A3|A4)|(I40_indirect t=(D1|D2)))
    | inv=I22_invert? sta=I67_x_equals_t (mnemonic|t=(D2|A3|A4)|(I40_indirect t=(D1|D2)))
    ;


doi_lai_statement:
    inv=I22_invert? sta=(I87_if_flag|I97_dsz) (D1|D2|(I40_indirect (D1|D2))) (mnemonic|D2|A3|A4|(I40_indirect (D1|D2)))
    ;

label_statement: sta=I76_label (D1|D2|mnemonic);

inv_statement: sta=I22_invert;
