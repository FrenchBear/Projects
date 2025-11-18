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
    | unknown_statement
    ;

number_statement: NUMBER;

atomic_statement: 
    inv=I22_invert? sta=(I38_sin|I39_cos|I30_tan)
    | sta=I25_clear
    | sta=I24_correct_entry
    ;

di_statement: 
    sta=(I42_store|I43_recall) ind=INDIRECT1? (DD1|INVALID1)
    | inv=I22_invert? sta=I44_sum ind=INDIRECT1? (DD1|INVALID1)
    ;

d_statement:
    sta=(I62_program_indirect|I63_exchange_indirect|I72_store_indirect|I73_recall_indirect|I82_hir) (DD2|INVALID2)
    | inv=I22_invert? sta=(I64_product_indirect|I67_x_equals_t_indextra|I74_sum_indirect|I77_x_greater_or_equal_than_t_indextra) (DD2|INVALID2)
    ;

lai_statement:
    sta=I61_goto (MNEMONIC3|ADD3b|ADD3a|DD3|(INDIRECT3 (DD3|INVALID3))|INVALID3)
    | inv=I22_invert? sta=I67_x_equals_t (MNEMONIC3|ADD3b|ADD3a|DD3|(INDIRECT3 (DD3|INVALID3))|INVALID3)
    ;

doi_lai_statement:
    inv=I22_invert? sta=(I87_if_flag|I97_dsz) (DD4a|(INDIRECT4a DD4a)) (MNEMONIC4|ADD4b|ADD4a|DD4b|(INDIRECT4b (DD4b|INVALID4b))|INVALID4b)
    | inv=I22_invert? sta=(I87_if_flag|I97_dsz) INDIRECT4a INVALID4a
    | inv=I22_invert? sta=(I87_if_flag|I97_dsz) INVALID4a
    ;

label_statement: sta=I76_label (DD5|MNEMONIC5|INVALID5);

inv_statement: sta=I22_invert;

unknown_statement: INVALID_TOKEN;
